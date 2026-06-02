using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.services;
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

        public ChatController(
            IMessageService messageService,
            IConversationListService conversationListService,
            IReadReceiptService readReceiptService)
        {
            _messageService = messageService;
            _conversationListService = conversationListService;
            _readReceiptService = readReceiptService;
        }
        
        [HttpGet]
        public async Task<IActionResult> Index(Guid conversationId, Guid currentUserId)
        {
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
        
        [HttpPost]
        public async Task<IActionResult> SendMessage(Guid conversationId, Guid currentUserId, string messageInput)
        {
            if (!string.IsNullOrWhiteSpace(messageInput))
            {
                await _messageService.SendMessageAsync(conversationId, currentUserId, messageInput, null);
            }
            
            return RedirectToAction("Index", new { conversationId = conversationId, currentUserId = currentUserId });
        }
    }
}
