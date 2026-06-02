using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.services;
using ChatModule.ViewModels;

namespace ChatModule.src.view_models
{
    public class ChatViewModel : BaseViewModel
    {
        private const int PageLimit = 100;
        private const string ImagePrefix = "[Image] ";

        private readonly IMessageService _messageService;
        private readonly IMessageInteractionService _interactionService;
        private readonly IReadReceiptService _readReceiptService;
        private readonly IMentionService _mentionService;
        private readonly IDirectMessageService _directMessageService;
        private readonly IConversationListService _conversationListService;
        private readonly Guid _currentUserId;

        private string _conversationTitle = string.Empty;
        private Message? _pinnedMessage;
        private bool _isInputDisabled;
        private string? _inputDisabledReason;
        private bool _isLoading;
        private string _messageInput = string.Empty;
        private string? _selectedAttachmentPath;
        private Message? _replyingTo;
        private Message? _editingMessage;
        private int _messageSkip = 0;
        private bool _hasMoreMessages = true;
        private bool _isUnreadInitialized;
        private bool _isConversationGroup;
        private bool _isSearchVisible;
        private string? _errorMessage;
        private Message? _firstUnreadMessage;
        private int _unreadSeparatorCount;

        public ChatViewModel(
            IMessageService messageService,
            IMessageInteractionService interactionService,
            IReadReceiptService readReceiptService,
            IMentionService mentionService,
            IDirectMessageService directMessageService,
            IConversationListService conversationListService,
            ISearchService searchService,
            Guid currentUserId)
        {
            this._messageService = messageService;
            this._interactionService = interactionService;
            this._readReceiptService = readReceiptService;
            this._mentionService = mentionService;
            this._directMessageService = directMessageService;
            this._conversationListService = conversationListService;
            this._currentUserId = currentUserId;
            this.MessageSearch = new MessageSearchViewModel(searchService, currentUserId);

            this.MentionSuggestions.CollectionChanged += this.HandleMentionSuggestionsChanged;
            this.MessageSearch.CloseRequested += () => this.IsSearchVisible = false;
            this.MessageSearch.JumpToMessageRequested += messageId => _ = this.ScrollToMessageAsync(messageId);

            this.ReactCommand = new RelayCommand<Guid>(this.OpenEmojiPickerAsync);
            this.ScrollToMessageCommand = new RelayCommand<Guid>(this.ScrollToMessageAsync);
            this.SendCommand = new RelayCommand(this.SendAsync);
            this.CancelReplyCommand = new RelayCommand(this.CancelReplyAsync);
            this.LoadMoreCommand = new RelayCommand(this.LoadMoreAsync);
            this.EditMessageCommand = new RelayCommand<Guid>(this.StartEditAsync);
            this.DeleteMessageCommand = new RelayCommand<Guid>(this.DeleteAsync);
            this.CancelEditCommand = new RelayCommand(this.CancelEditAsync);
            this.ReplyToCommand = new RelayCommand<Guid>(this.ReplyToAsync);
            this.InsertMentionCommand = new RelayCommand<User>(this.InsertMentionAsync);
            this.ReactWithSpecificEmojiCommand = new RelayCommand<Tuple<Guid, string>>(this.ReactWithSpecificEmojiAsync);
            this.ToggleReactionCounterCommand = new RelayCommand<Guid>(this.ToggleReactionCounterAsync);
            this.OpenSearchCommand = new RelayCommand(this.OpenSearchAsync);
            this.CloseSearchCommand = new RelayCommand(this.CloseSearchAsync);
            this.JumpToSearchResultCommand = new RelayCommand<Guid>(this.JumpToSearchResultAsync);
            this.ShowReadReceiptDetailsCommand = new RelayCommand<Guid>(this.ShowReadReceiptDetailsAsync);
            this.PinMessageCommand = new RelayCommand<Guid>(this.PinAsync);
            this.UnpinMessageCommand = new RelayCommand(this.UnpinAsync);
        }

        public event Action<Guid, List<Message>>? ReactionsChanged;
        public event Action<Guid>? ScrollToMessageRequested;
        public event Action<string>? ReadReceiptDetailsRequested;
        public event Action<Guid>? ReplyPreviewTapped;
        public event Action? LeaveGroupRequested;
        public event Action? SetNicknameRequested;
        public event Action? ClearNicknameRequested;

        public Guid ConversationId { get; private set; }

        public string ConversationTitle
        {
            get => this._conversationTitle;
            private set => this.Set(ref this._conversationTitle, value);
        }

        public ObservableCollection<Message> Messages { get; } = new ();
        public ObservableCollection<User> MentionSuggestions { get; } = new ();
        public bool HasMentionSuggestions => this.MentionSuggestions.Count > 0;

        public Message? PinnedMessage
        {
            get => this._pinnedMessage;
            private set
            {
                if (this.Set(ref this._pinnedMessage, value))
                {
                    this.OnPropertyChanged(nameof(this.PinnedMessageExpiryLabel));
                }
            }
        }

        public string? PinnedMessageExpiryLabel =>
            this._pinnedMessage?.PinExpiresAt.HasValue == true
                ? $"Expires {this._pinnedMessage.PinExpiresAt.Value.ToLocalTime():g}"
                : null;

        public bool IsConversationGroup
        {
            get => this._isConversationGroup;
            private set => this.Set(ref this._isConversationGroup, value);
        }

        public bool IsInputDisabled
        {
            get => this._isInputDisabled;
            private set => this.Set(ref this._isInputDisabled, value);
        }

        public string? InputDisabledReason
        {
            get => this._inputDisabledReason;
            private set => this.Set(ref this._inputDisabledReason, value);
        }

        public bool IsLoading
        {
            get => this._isLoading;
            private set => this.Set(ref this._isLoading, value);
        }

        public string MessageInput
        {
            get => this._messageInput;
            set
            {
                if (this.Set(ref this._messageInput, value))
                {
                    _ = this.UpdateMentionSuggestionsAsync();
                }
            }
        }

        public string? SelectedAttachmentPath
        {
            get => this._selectedAttachmentPath;
            private set => this.Set(ref this._selectedAttachmentPath, value);
        }

        public string? SelectedAttachmentName => string.IsNullOrWhiteSpace(this.SelectedAttachmentPath)
            ? null
            : Path.GetFileName(this.SelectedAttachmentPath);

        public Message? ReplyingTo
        {
            get => this._replyingTo;
            private set => this.Set(ref this._replyingTo, value);
        }

        public Message? EditingMessage
        {
            get => this._editingMessage;
            private set => this.Set(ref this._editingMessage, value);
        }

        public Func<Task<string?>>? RequestEmojiAsync { get; set; }
        public Func<Task<DateTime?>>? RequestPinExpiryAsync { get; set; }

        public RelayCommand<Guid> ReactCommand { get; }
        public RelayCommand<Guid> ScrollToMessageCommand { get; }
        public RelayCommand SendCommand { get; }
        public RelayCommand CancelReplyCommand { get; }
        public RelayCommand LoadMoreCommand { get; }
        public RelayCommand<Guid> EditMessageCommand { get; }
        public RelayCommand<Guid> DeleteMessageCommand { get; }
        public RelayCommand CancelEditCommand { get; }
        public RelayCommand<Guid> ReplyToCommand { get; }
        public RelayCommand<User> InsertMentionCommand { get; }
        public RelayCommand<Tuple<Guid, string>> ReactWithSpecificEmojiCommand { get; }
        public RelayCommand<Guid> ToggleReactionCounterCommand { get; }
        public RelayCommand OpenSearchCommand { get; }
        public ICommand CloseSearchCommand { get; }
        public RelayCommand<Guid> JumpToSearchResultCommand { get; }
        public RelayCommand<Guid> ShowReadReceiptDetailsCommand { get; }
        public RelayCommand<Guid> PinMessageCommand { get; }
        public RelayCommand UnpinMessageCommand { get; }
        public MessageSearchViewModel MessageSearch { get; }

        public bool IsSearchVisible
        {
            get => this._isSearchVisible;
            private set => this.Set(ref this._isSearchVisible, value);
        }

        public string? ErrorMessage
        {
            get => this._errorMessage;
            private set => this.Set(ref this._errorMessage, value);
        }

        public Message? FirstUnreadMessage
        {
            get => this._firstUnreadMessage;
            private set => this.Set(ref this._firstUnreadMessage, value);
        }

        public int UnreadSeparatorCount
        {
            get => this._unreadSeparatorCount;
            private set => this.Set(ref this._unreadSeparatorCount, value);
        }

        public bool HasUnreadSeparator => this.FirstUnreadMessage != null && this.UnreadSeparatorCount > 0;

        public async Task LoadAsync(Guid conversationId)
        {
            this.IsLoading = true;
            this.ErrorMessage = null;
            try
            {
                this.ConversationId = conversationId;
                this.OnPropertyChanged(nameof(this.ConversationId));

                var messages = await this._messageService.GetMessagesAsync(conversationId, this._currentUserId, 0, PageLimit);
                this.Messages.Clear();
                foreach (var message in messages)
                {
                    PrepareMessageForDisplay(message);
                    message.IsMine = message.UserId == this._currentUserId;
                    this.Messages.Add(message);
                }

                this._messageSkip = 0;
                this._hasMoreMessages = messages.Count >= PageLimit;

                await this.PopulateReactionCountersAsync();
                await this.PopulateReplyPreviewsAsync();

                var conversation = await this._conversationListService.GetByIdAsync(conversationId);
                this.IsConversationGroup = conversation?.Type == ConversationType.Group;

                if (conversation?.Type == ConversationType.Dm)
                {
                    var otherUser = await this._directMessageService.GetOtherUserAsync(conversationId, this._currentUserId);
                    this.ConversationTitle = otherUser != null ? otherUser.Username : "Direct Message";
                }
                else
                {
                    this.ConversationTitle = conversation?.Title ?? "Conversation";
                }

                if (conversation?.PinnedMessageId != null)
                {
                    var pinned = messages.FirstOrDefault(message => message.Id == conversation.PinnedMessageId.Value);
                    if (pinned != null && pinned.PinExpiresAt.HasValue && pinned.PinExpiresAt.Value <= DateTime.UtcNow)
                    {
                        await this._directMessageService.ClearExpiredPinAsync(conversationId, pinned.Id);
                        this.PinnedMessage = null;
                    }
                    else
                    {
                        this.PinnedMessage = pinned;
                    }
                }
                else
                {
                    this.PinnedMessage = null;
                }

                var isBlocked = await this._directMessageService.IsBlockedAsync(conversationId, this._currentUserId);
                if (isBlocked)
                {
                    this.IsInputDisabled = true;
                    this.InputDisabledReason = "Messaging is disabled because one of the users is blocked.";
                }
                else
                {
                    var cannotSendReason = await this._messageService.GetCannotSendReasonAsync(conversationId, this._currentUserId);
                    this.IsInputDisabled = !string.IsNullOrWhiteSpace(cannotSendReason);
                    this.InputDisabledReason = cannotSendReason;
                }

                await this.PopulateReadReceiptMetadataAsync();
                await this.UpdateUnreadSeparatorAsync();
                this._isUnreadInitialized = true;
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        public Task OpenReplyTargetAsync(Guid replyToId)
        {
            if (replyToId != Guid.Empty)
            {
                this.ReplyPreviewTapped?.Invoke(replyToId);
            }

            return Task.CompletedTask;
        }

        public async Task MarkVisibleMessagesAsReadAsync(Guid lastVisibleMessageId)
        {
            if (this.ConversationId == Guid.Empty || !this._isUnreadInitialized)
            {
                return;
            }

            await this._readReceiptService.MarkAsReadAsync(this.ConversationId, this._currentUserId, lastVisibleMessageId);
            await this.PopulateReadReceiptMetadataAsync();
            await this.UpdateUnreadSeparatorAsync();
        }

        public async Task MarkConversationAsReadAsync()
        {
            if (this.ConversationId == Guid.Empty)
            {
                return;
            }

            await this._readReceiptService.MarkLatestAsReadAsync(this.ConversationId, this._currentUserId);
            await this.PopulateReadReceiptMetadataAsync();
            await this.UpdateUnreadSeparatorAsync();
        }

        public async Task ShowReadReceiptDetailsAsync(Guid messageId)
        {
            if (this.ConversationId == Guid.Empty || messageId == Guid.Empty)
            {
                return;
            }

            var readers = await this._readReceiptService.GetReaderUsernamesAsync(this.ConversationId, messageId, this._currentUserId);
            if (readers.Count == 0)
            {
                this.ReadReceiptDetailsRequested?.Invoke("No one else has seen this message yet.");
                return;
            }

            var body = string.Join(Environment.NewLine, readers);
            this.ReadReceiptDetailsRequested?.Invoke(body);
        }

        public Task LeaveGroupAsync()
        {
            if (this.IsConversationGroup)
            {
                this.LeaveGroupRequested?.Invoke();
            }

            return Task.CompletedTask;
        }

        public Task SetNicknameAsync()
        {
            if (this.IsConversationGroup)
            {
                this.SetNicknameRequested?.Invoke();
            }

            return Task.CompletedTask;
        }

        public Task ClearNicknameAsync()
        {
            if (this.IsConversationGroup)
            {
                this.ClearNicknameRequested?.Invoke();
            }

            return Task.CompletedTask;
        }

        public Task SetAttachmentAsync(string path)
        {
            this.SelectedAttachmentPath = string.IsNullOrWhiteSpace(path) ? null : path;
            this.OnPropertyChanged(nameof(this.SelectedAttachmentName));
            return Task.CompletedTask;
        }

        public Task ClearAttachmentAsync()
        {
            this.SelectedAttachmentPath = null;
            this.OnPropertyChanged(nameof(this.SelectedAttachmentName));
            return Task.CompletedTask;
        }

        private static void PrepareMessageForDisplay(Message message)
        {
            if (string.IsNullOrWhiteSpace(message.Content))
            {
                message.AttachmentImagePath = null;
                return;
            }

            var content = message.Content!;
            if (!content.StartsWith(ImagePrefix, StringComparison.Ordinal))
            {
                message.AttachmentImagePath = null;
                return;
            }

            var body = content.Substring(ImagePrefix.Length);
            var split = body.Split(new[] { "\r\n", "\n" }, 2, StringSplitOptions.None);
            var imagePath = split[0].Trim();
            if (!File.Exists(imagePath))
            {
                var fileName = Path.GetFileName(imagePath);
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    var candidate = Path.Combine(AppContext.BaseDirectory, "attachments", fileName);
                    if (File.Exists(candidate))
                    {
                        imagePath = candidate;
                    }
                }
            }

            message.AttachmentImagePath = imagePath;
            message.Content = split.Length > 1 ? split[1] : string.Empty;
        }

        private void HandleMentionSuggestionsChanged(object? sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            this.OnPropertyChanged(nameof(this.HasMentionSuggestions));
        }

        private async Task SendAsync()
        {
            this.ErrorMessage = null;
            try
            {
                if (this.ConversationId == Guid.Empty)
                {
                    return;
                }

                if (this.IsInputDisabled)
                {
                    var cannotSendReason = await this._messageService.GetCannotSendReasonAsync(this.ConversationId, this._currentUserId);
                    if (!string.IsNullOrWhiteSpace(cannotSendReason))
                    {
                        this.InputDisabledReason = cannotSendReason;
                        this.ErrorMessage = cannotSendReason;
                    }

                    return;
                }

                var liveCannotSendReason = await this._messageService.GetCannotSendReasonAsync(this.ConversationId, this._currentUserId);
                if (!string.IsNullOrWhiteSpace(liveCannotSendReason))
                {
                    this.IsInputDisabled = true;
                    this.InputDisabledReason = liveCannotSendReason;
                    this.ErrorMessage = liveCannotSendReason;
                    return;
                }

                if (string.IsNullOrWhiteSpace(this.MessageInput) && string.IsNullOrWhiteSpace(this.SelectedAttachmentPath))
                {
                    this.ErrorMessage = "Empty messages cannot be sent.";
                    return;
                }

                if (this.EditingMessage != null)
                {
                    await this.ConfirmEditAsync();
                    return;
                }

                var content = this.MessageInput;
                if (!string.IsNullOrWhiteSpace(this.SelectedAttachmentPath))
                {
                    var storedAttachmentPath = await this._messageService.PersistImageAttachmentAsync(this.SelectedAttachmentPath);
                    content = string.IsNullOrWhiteSpace(content)
                        ? $"{ImagePrefix}{storedAttachmentPath}"
                        : $"{ImagePrefix}{storedAttachmentPath}{Environment.NewLine}{content}";
                }

                var replyToId = this.ReplyingTo?.Id;
                var message = await this._messageService.SendMessageAsync(this.ConversationId, this._currentUserId, content, replyToId);
                PrepareMessageForDisplay(message);
                message.IsMine = true;
                this.ApplyMessageActions(message);

                if (replyToId.HasValue)
                {
                    var replyParts = await this._interactionService.BuildReplyPreviewPartsAsync(replyToId.Value);
                    if (replyParts.HasValue)
                    {
                        message.ReplyPreviewSender = replyParts.Value.Sender;
                        message.ReplyPreviewContent = replyParts.Value.Content;
                        message.ReplyPreviewText = $"{replyParts.Value.Sender}: {replyParts.Value.Content}";
                    }
                }

                this.Messages.Add(message);
                await this.PopulateReadReceiptMetadataAsync();
                await this.UpdateUnreadSeparatorAsync();

                this.MessageInput = string.Empty;
                this.SelectedAttachmentPath = null;
                this.OnPropertyChanged(nameof(this.SelectedAttachmentName));
                this.ReplyingTo = null;

                await this._readReceiptService.MarkLatestAsReadAsync(this.ConversationId, this._currentUserId);
                await this.PopulateReadReceiptMetadataAsync();
                await this.UpdateUnreadSeparatorAsync();

                var postSendCannotSendReason = await this._messageService.GetCannotSendReasonAsync(this.ConversationId, this._currentUserId);
                if (!string.IsNullOrWhiteSpace(postSendCannotSendReason))
                {
                    this.IsInputDisabled = true;
                    this.InputDisabledReason = postSendCannotSendReason;
                }
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
        }

        private async Task LoadMoreAsync()
        {
            this.ErrorMessage = null;
            try
            {
                if (this.ConversationId == Guid.Empty || !this._hasMoreMessages)
                {
                    return;
                }

                this._messageSkip += PageLimit;
                var older = await this._messageService.GetMessagesAsync(this.ConversationId, this._currentUserId, this._messageSkip, PageLimit);
                foreach (var message in older)
                {
                    PrepareMessageForDisplay(message);
                    message.IsMine = message.UserId == this._currentUserId;
                    this.Messages.Add(message);
                }

                this._hasMoreMessages = older.Count >= PageLimit;

                await this.PopulateReactionCountersAsync();
                await this.PopulateReplyPreviewsAsync();
                await this.PopulateReadReceiptMetadataAsync();
                await this.UpdateUnreadSeparatorAsync();
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
        }

        private Task StartEditAsync(Guid messageId)
        {
            var targetMessage = this.Messages.FirstOrDefault(message => message.Id == messageId);
            if (targetMessage == null)
            {
                return Task.CompletedTask;
            }

            this.EditingMessage = targetMessage;
            this.MessageInput = targetMessage.Content ?? string.Empty;
            this.ReplyingTo = null;
            return Task.CompletedTask;
        }

        private async Task ConfirmEditAsync()
        {
            this.ErrorMessage = null;
            if (this.EditingMessage == null)
            {
                return;
            }

            var messageId = this.EditingMessage.Id;
            var newContent = this.MessageInput;

            await this._messageService.EditMessageAsync(messageId, this._currentUserId, newContent);

            var index = this.Messages.IndexOf(this.EditingMessage);
            if (index >= 0)
            {
                var updated = this.Messages[index];
                updated.Content = newContent;
                updated.IsEdited = true;
                this.Messages[index] = updated;
            }

            this.MessageInput = string.Empty;
            this.EditingMessage = null;
            await this.PopulateReadReceiptMetadataAsync();
        }

        private Task CancelEditAsync()
        {
            this.EditingMessage = null;
            this.MessageInput = string.Empty;
            return Task.CompletedTask;
        }

        private async Task DeleteAsync(Guid messageId)
        {
            this.ErrorMessage = null;
            try
            {
                await this._messageService.DeleteMessageAsync(messageId, this._currentUserId);

                var targetMessage = this.Messages.FirstOrDefault(message => message.Id == messageId);
                if (targetMessage != null)
                {
                    targetMessage.IsDeleted = true;
                    var index = this.Messages.IndexOf(targetMessage);
                    if (index >= 0)
                    {
                        this.Messages[index] = targetMessage;
                    }
                }

                await this.PopulateReadReceiptMetadataAsync();
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
        }

        private Task CancelReplyAsync()
        {
            this.ReplyingTo = null;
            return Task.CompletedTask;
        }

        private Task ReplyToAsync(Guid messageId)
        {
            this.ReplyingTo = this.Messages.FirstOrDefault(message => message.Id == messageId);
            return Task.CompletedTask;
        }

        private async Task OpenEmojiPickerAsync(Guid messageId)
        {
            this.ErrorMessage = null;
            try
            {
                if (this.RequestEmojiAsync == null)
                {
                    return;
                }

                var emoji = await this.RequestEmojiAsync();
                if (emoji == null)
                {
                    return;
                }

                await this._interactionService.ReactToMessageAsync(messageId, this._currentUserId, emoji);

                var reactions = await this._interactionService.GetReactionsAsync(messageId);
                this.ReactionsChanged?.Invoke(messageId, reactions);

                await this.PopulateReactionCountersAsync();
                await this.PopulateReplyPreviewsAsync();
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
        }

        private Task ScrollToMessageAsync(Guid messageId)
        {
            this.ScrollToMessageRequested?.Invoke(messageId);
            return Task.CompletedTask;
        }

        private async Task UpdateMentionSuggestionsAsync()
        {
            this.MentionSuggestions.Clear();

            if (this.ConversationId == Guid.Empty)
            {
                return;
            }

            var atIndex = this._messageInput.LastIndexOf('@');
            if (atIndex < 0)
            {
                return;
            }

            var token = this._messageInput.Substring(atIndex + 1);
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            var candidates = await this._mentionService.GetCandidatesAsync(this.ConversationId, token);
            foreach (var user in candidates)
            {
                this.MentionSuggestions.Add(user);
            }
        }

        private async Task PopulateReactionCountersAsync()
        {
            for (var index = 0; index < this.Messages.Count; index++)
            {
                var message = this.Messages[index];
                if (message.MessageType == MessageType.Reaction)
                {
                    message.ReactionCounts.Clear();
                    this.Messages[index] = message;
                    continue;
                }

                var reactions = await this._interactionService.GetReactionsAsync(message.Id);
                message.ReactionCounts = reactions
                    .Where(reaction => !reaction.IsDeleted && !string.IsNullOrWhiteSpace(reaction.Content))
                    .GroupBy(reaction => reaction.Content!)
                    .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
                this.Messages[index] = message;
            }
        }

        private async Task PopulateReplyPreviewsAsync()
        {
            for (var index = 0; index < this.Messages.Count; index++)
            {
                var message = this.Messages[index];
                this.ApplyMessageActions(message);

                if (message.ReplyToId.HasValue)
                {
                    var replyParts = await this._interactionService.BuildReplyPreviewPartsAsync(message.ReplyToId.Value);
                    if (replyParts.HasValue)
                    {
                        message.ReplyPreviewSender = replyParts.Value.Sender;
                        message.ReplyPreviewContent = replyParts.Value.Content;
                        message.ReplyPreviewText = $"{replyParts.Value.Sender}: {replyParts.Value.Content}";
                    }
                    else
                    {
                        message.ReplyPreviewSender = null;
                        message.ReplyPreviewContent = null;
                        message.ReplyPreviewText = null;
                    }
                }
                else
                {
                    message.ReplyPreviewSender = null;
                    message.ReplyPreviewContent = null;
                    message.ReplyPreviewText = null;
                }

                this.Messages[index] = message;
            }
        }

        private async Task ReactWithSpecificEmojiAsync(Tuple<Guid, string> payload)
        {
            this.ErrorMessage = null;
            var messageId = payload.Item1;
            var emoji = payload.Item2;
            if (messageId == Guid.Empty || string.IsNullOrWhiteSpace(emoji))
            {
                return;
            }

            await this._interactionService.ReactToMessageAsync(messageId, this._currentUserId, emoji);

            var reactions = await this._interactionService.GetReactionsAsync(messageId);
            this.ReactionsChanged?.Invoke(messageId, reactions);

            await this.PopulateReactionCountersAsync();
            await this.PopulateReplyPreviewsAsync();
        }

        private async Task ToggleReactionCounterAsync(Guid messageId)
        {
            this.ErrorMessage = null;
            try
            {
                var targetMessage = this.Messages.FirstOrDefault(message => message.Id == messageId);
                if (targetMessage == null || targetMessage.ReactionCounts.Count == 0)
                {
                    return;
                }

                var topReaction = targetMessage.ReactionCounts
                    .OrderByDescending(entry => entry.Value)
                    .ThenBy(entry => entry.Key, StringComparer.Ordinal)
                    .First().Key;

                var reactions = await this._interactionService.GetReactionsAsync(messageId);
                var mine = reactions.FirstOrDefault(reaction => reaction.UserId == this._currentUserId && !reaction.IsDeleted);

                if (mine != null && string.Equals(mine.Content, topReaction, StringComparison.Ordinal))
                {
                    await this._interactionService.RemoveReactionAsync(messageId, this._currentUserId);
                }
                else
                {
                    await this._interactionService.ReactToMessageAsync(messageId, this._currentUserId, topReaction);
                }

                var updated = await this._interactionService.GetReactionsAsync(messageId);
                this.ReactionsChanged?.Invoke(messageId, updated);
                await this.PopulateReactionCountersAsync();
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
        }

        private Task OpenSearchAsync()
        {
            if (this.ConversationId != Guid.Empty)
            {
                this.MessageSearch.Initialise(this.ConversationId);
            }

            this.IsSearchVisible = true;
            return Task.CompletedTask;
        }

        private Task CloseSearchAsync()
        {
            this.IsSearchVisible = false;
            return Task.CompletedTask;
        }

        private Task JumpToSearchResultAsync(Guid messageId)
        {
            this.IsSearchVisible = false;
            return this.ScrollToMessageAsync(messageId);
        }

        private Task InsertMentionAsync(User user)
        {
            var atIndex = this._messageInput.LastIndexOf('@');
            if (atIndex >= 0)
            {
                this.MessageInput = this._messageInput.Substring(0, atIndex) + $"@{user.Username} ";
            }

            this.MentionSuggestions.Clear();
            return Task.CompletedTask;
        }

        private async Task PopulateReadReceiptMetadataAsync()
        {
            if (this.ConversationId == Guid.Empty)
            {
                return;
            }

            var participants = await this._readReceiptService.GetParticipantsAsync(this.ConversationId);
            var participantCount = participants.Count;

            for (var index = 0; index < this.Messages.Count; index++)
            {
                var message = this.Messages[index];
                message.IsMine = message.UserId == this._currentUserId;

                if (message.MessageType != MessageType.Reaction && !message.IsDeleted)
                {
                    var readByCount = await this._readReceiptService.GetReadByCountAsync(this.ConversationId, message.Id);
                    message.ReadByCount = readByCount;

                    if (message.IsMine && message.MessageType != MessageType.System)
                    {
                        var otherReaders = await this._readReceiptService.GetReadByOthersCountAsync(this.ConversationId, message.Id, this._currentUserId);
                        if (otherReaders <= 0)
                        {
                            message.ReadReceiptLabel = null;
                        }
                        else if (!this.IsConversationGroup)
                        {
                            message.ReadReceiptLabel = "Seen";
                        }
                        else
                        {
                            message.ReadReceiptLabel = $"Seen by {otherReaders}/{Math.Max(1, participantCount - 1)}";
                        }
                    }
                    else
                    {
                        message.ReadReceiptLabel = null;
                    }
                }
                else
                {
                    message.ReadByCount = 0;
                    message.ReadReceiptLabel = null;
                }

                this.Messages[index] = message;
            }
        }

        private async Task UpdateUnreadSeparatorAsync()
        {
            if (this.Messages.Count == 0)
            {
                this.FirstUnreadMessage = null;
                this.UnreadSeparatorCount = 0;
                this.ApplyUnreadSeparatorFlag();
                return;
            }

            var lastReadMessageId = await this._readReceiptService.GetLastReadMessageAsync(this.ConversationId, this._currentUserId);
            var lastReadTimestamp = await this._readReceiptService.GetLastReadTimestampAsync(this.ConversationId, this._currentUserId);
            var firstUnread = default(Message);
            var unreadCount = 0;

            if (!lastReadMessageId.HasValue)
            {
                foreach (var message in this.Messages)
                {
                    if (message.UserId == this._currentUserId || message.MessageType == MessageType.System)
                    {
                        continue;
                    }

                    firstUnread ??= message;
                    unreadCount++;
                }

                this.FirstUnreadMessage = firstUnread;
                this.UnreadSeparatorCount = unreadCount;
                this.ApplyUnreadSeparatorFlag();
                return;
            }

            var crossedLastRead = false;
            foreach (var message in this.Messages)
            {
                if (!crossedLastRead)
                {
                    if (message.Id == lastReadMessageId.Value)
                    {
                        crossedLastRead = true;
                    }

                    continue;
                }

                if (message.UserId == this._currentUserId || message.MessageType == MessageType.System)
                {
                    continue;
                }

                if (lastReadTimestamp.HasValue && message.CreatedAt <= lastReadTimestamp.Value)
                {
                    continue;
                }

                firstUnread ??= message;
                unreadCount++;
            }

            this.FirstUnreadMessage = firstUnread;
            this.UnreadSeparatorCount = unreadCount;
            this.ApplyUnreadSeparatorFlag();
        }

        private void ApplyUnreadSeparatorFlag()
        {
            foreach (var message in this.Messages)
            {
                message.ShowUnreadSeparator = false;
            }

            if (this.FirstUnreadMessage != null)
            {
                this.FirstUnreadMessage.ShowUnreadSeparator = true;
            }

            this.OnPropertyChanged(nameof(this.HasUnreadSeparator));
        }

        private async Task PinAsync(Guid messageId)
        {
            this.ErrorMessage = null;
            try
            {
                if (this.RequestPinExpiryAsync == null)
                {
                    return;
                }

                var expiresAt = await this.RequestPinExpiryAsync();
                if (!expiresAt.HasValue)
                {
                    return;
                }

                var (_, notice) = await this._directMessageService.PinMessageAsync(this.ConversationId, this._currentUserId, messageId, expiresAt.Value);

                var messageToPin = this.Messages.FirstOrDefault(message => message.Id == messageId);
                if (messageToPin != null)
                {
                    messageToPin.PinExpiresAt = expiresAt.Value;
                    var index = this.Messages.IndexOf(messageToPin);
                    if (index >= 0)
                    {
                        this.Messages[index] = messageToPin;
                    }
                }

                this.PinnedMessage = messageToPin;

                PrepareMessageForDisplay(notice);
                this.ApplyMessageActions(notice);
                this.Messages.Add(notice);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
        }

        private async Task UnpinAsync()
        {
            this.ErrorMessage = null;
            try
            {
                var notice = await this._directMessageService.UnpinMessageAsync(this.ConversationId, this._currentUserId);
                this.PinnedMessage = null;

                PrepareMessageForDisplay(notice);
                this.ApplyMessageActions(notice);
                this.Messages.Add(notice);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
        }

        private void ApplyMessageActions(Message message)
        {
            var mine = message.UserId.HasValue && message.UserId.Value == this._currentUserId;
            var editableType = message.MessageType == MessageType.Text;
            var notDeleted = !message.IsDeleted;
            var pinnable = !this.IsConversationGroup
                           && message.MessageType != MessageType.System
                           && message.MessageType != MessageType.Reaction
                           && notDeleted;

            message.CanDelete = mine && notDeleted;
            message.CanEdit = mine && notDeleted && editableType;
            message.CanPin = pinnable;
        }
    }
}
