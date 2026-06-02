using ChatAndEvents.Data;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;
using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.services;
using ChatAndEvents.Data.Database;
using ChatAndEvents.Data.EventsData.Repositories;
using ChatAndEvents.Data.EventsData.Repositories.achievementRepository;
using ChatAndEvents.Data.EventsData.Repositories.announcementRepository;
using ChatAndEvents.Data.EventsData.Repositories.categoriesRepository;
using ChatAndEvents.Data.EventsData.Repositories.discussionRepository;
using ChatAndEvents.Data.EventsData.Repositories.eventRepository;
using ChatAndEvents.Data.EventsData.Repositories.eventStatisticsRepository;
using ChatAndEvents.Data.EventsData.Repositories.notificationRepository;
using ChatAndEvents.Data.EventsData.Repositories.reputationRepository;
using ChatAndEvents.Data.EventsData.Services;
using ChatAndEvents.Data.EventsData.Services.achievementServices;
using ChatAndEvents.Data.EventsData.Services.announcementServices;
using ChatAndEvents.Data.EventsData.Services.attendedEventServices;
using ChatAndEvents.Data.EventsData.Services.categoryServices;
using ChatAndEvents.Data.EventsData.Services.discussionService;
using ChatAndEvents.Data.EventsData.Services.eventServices;
using ChatAndEvents.Data.EventsData.Services.eventStatisticsServices;
using ChatAndEvents.Data.EventsData.Services.Interfaces;
using ChatAndEvents.Data.EventsData.Services.memoryServices;
using ChatAndEvents.Data.EventsData.Services.notificationServices;
using ChatAndEvents.Data.EventsData.Services.reputationService;
using ChatAndEvents.Data.EventsData.Services.userServices;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ChatAndEventsDB") ?? throw new InvalidOperationException("Connection string 'ChatAndEventsDB' not found.")));
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // This tells Swagger to use the full path (e.g. ChatData.domain.User) 
    // to prevent naming collisions!
    options.CustomSchemaIds(type => type.FullName);
});

// chat module repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFriendRepository, FriendRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IParticipantRepository, ParticipantRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();

// chat module services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IConversationListService, ConversationListService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IDirectMessageService, DirectMessageService>();
builder.Services.AddScoped<IBlockService, BlockService>();
builder.Services.AddScoped<IFriendListService, FriendListService>();
builder.Services.AddScoped<IFriendRequestService, FriendRequestService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IMemberPanelService, MemberPanelService>();
builder.Services.AddScoped<IMentionService, MentionService>();
builder.Services.AddScoped<IMessageInteractionService, MessageInteractionService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IModerationService, ModerationService>();
builder.Services.AddScoped<IReadReceiptService, ReadReceiptService>();

// events module repositories
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IQuestRepository, QuestRepository>();
builder.Services.AddScoped<IQuestMemoryRepository, QuestMemoryRepository>();
builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
builder.Services.AddScoped<IDiscussionRepository, DiscussionRepository>();
builder.Services.AddScoped<IMemoryRepository, MemoryRepository>();
builder.Services.AddScoped<IAttendedEventRepository, AttendedEventRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IReputationRepository, ReputationRepository>();
builder.Services.AddScoped<IAchievementRepository, AchievementRepository>();
builder.Services.AddScoped<IEventStatisticsRepository, EventStatisticsRepository>();

// events module services
builder.Services.AddScoped<IUserService, ChatUserService>();
builder.Services.AddScoped<IMemoryService, MemoryService>();
builder.Services.AddScoped<IQuestApprovalService, QuestApprovalService>();
builder.Services.AddScoped<IQuestService, QuestService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEventStatisticsService, EventStatisticsService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IDiscussionService, DiscussionService>();
builder.Services.AddScoped<ICategoryServices, CategoryServices>();
builder.Services.AddScoped<IAttendedEventService, AttendedEventService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<IReputationService, ReputationService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var db = await contextFactory.CreateDbContextAsync();
    await CommunityHubDatabaseInitializer.InitializeAsync(db);
}



app.UseSwagger();
app.UseSwaggerUI();


if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();
app.MapControllers();
app.MapDefaultEndpoints();
app.Run();
