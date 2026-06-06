using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.services;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Authorization;
namespace ChatAndEvents.Web.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly IConversationListService _conversationListService;
        private readonly IReadReceiptService _readReceiptService;
        private readonly CurrentUserContext _currentUserContext;

        public ChatController(
            IMessageService messageService,
            IConversationListService conversationListService,
            IReadReceiptService readReceiptService,
            CurrentUserContext currentUserContext)
        {
            _messageService = messageService;
            _conversationListService = conversationListService;
            _readReceiptService = readReceiptService;
            _currentUserContext = currentUserContext;
        }
        
        [HttpGet]
        public async Task<IActionResult> Index(Guid conversationId)
        {
            var currentUserId = _currentUserContext.UserId;
            var conversation = await _conversationListService.GetByIdAsync(conversationId);
            if (conversation == null) return NotFound();

            var messages = await _messageService.GetMessagesAsync(conversationId, currentUserId, 0, 100);
            
            var cannotSendReason = await _messageService.GetCannotSendReasonAsync(conversationId, currentUserId);

            var viewModel = new ChatViewModel
            {
                ConversationId = conversationId,
                CurrentUserId = currentUserId,
                ConversationTitle = conversation.Title ?? "Direct Message",
                Messages = messages,
                IsInputDisabled = !string.IsNullOrWhiteSpace(cannotSendReason),
                InputDisabledReason = cannotSendReason
            };
            
            await _readReceiptService.MarkLatestAsReadAsync(conversationId, currentUserId);

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Messages(Guid conversationId)
        {
            var currentUserId = _currentUserContext.UserId;
            var messages = await _messageService.GetMessagesAsync(conversationId, currentUserId, 0, 100);

            await _readReceiptService.MarkLatestAsReadAsync(conversationId, currentUserId);

            return Json(new
            {
                messages = messages.Select(message => new
                {
                    id = message.Id,
                    senderUsername = message.SenderUsername ?? "Unknown",
                    content = message.Content ?? string.Empty,
                    createdAt = message.CreatedAt.ToLocalTime().ToString("HH:mm"),
                    isMine = message.UserId == currentUserId
                })
            });
        }
        
        [HttpPost]
        public async Task<IActionResult> SendMessage(Guid conversationId, string messageInput)
        {
            var currentUserId = _currentUserContext.UserId;
            if (!string.IsNullOrWhiteSpace(messageInput))
            {
                await _messageService.SendMessageAsync(conversationId, currentUserId, messageInput, null);
            }
            
            return RedirectToAction("Index", new { conversationId = conversationId });
        }

        [HttpPost]
        public async Task<IActionResult> SendMessageJson(Guid conversationId, string messageInput)
        {
            var currentUserId = _currentUserContext.UserId;
            if (string.IsNullOrWhiteSpace(messageInput))
            {
                return BadRequest(new { error = "Message cannot be empty." });
            }

            try
            {
                await _messageService.SendMessageAsync(conversationId, currentUserId, messageInput, null);
                return Ok(new { ok = true });
            }
            catch (Exception exception)
            {
                return BadRequest(new { error = exception.Message });
            }
        }
    }
}
