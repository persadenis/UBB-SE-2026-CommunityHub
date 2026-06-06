//https://localhost:7283-> port for https -> look in properties  for the current web app

using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.services;
using ChatAndEvents.Data.CommunityHub.Services;
using ChatAndEvents.Data.Database;
using ChatAndEvents.Data.EventsData.Services;
using ChatAndEvents.Data.EventsData.Services.achievementServices;
using ChatAndEvents.Data.EventsData.Services.announcementServices;
using ChatAndEvents.Data.EventsData.Services.attendedEventServices;
using ChatAndEvents.Data.EventsData.Services.discussionService;
using ChatAndEvents.Data.EventsData.Services.eventServices;
using ChatAndEvents.Data.EventsData.Services.eventStatisticsServices;
using ChatAndEvents.Data.EventsData.Services.Interfaces;
using ChatAndEvents.Data.EventsData.Services.memoryServices;
using ChatAndEvents.Data.EventsData.Services.notificationServices;
using ChatAndEvents.Data.EventsData.Services.reputationService;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatModule.src.HttpService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Security.Claims;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseConfiguredDatabase(
        builder.Configuration.GetConnectionString("ChatAndEventsDB")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=ChatAndEventsDB;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;",
        builder.Configuration["DatabaseProvider"],
        builder.Configuration["DATABASE_URL"]));

builder.Services.AddScoped<ICommunityHubService, CommunityHubService>();
builder.Services.AddScoped<IMatchmakingService, MatchmakingService>();
builder.Services.AddSingleton<ChatAndEvents.Web.Services.IUploadStorageService, ChatAndEvents.Web.Services.UploadStorageService>();

builder.Services.AddScoped<IMessageService, MessageHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new MessageHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IConversationListService, ConversationListHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new ConversationListHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IReadReceiptService, ReadReceiptHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new ReadReceiptHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IMemberPanelService, MemberPanelHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new MemberPanelHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IModerationService, ModerationHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new ModerationHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<ISearchService, SearchHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new SearchHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IFriendListService, FriendListHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new FriendListHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IFriendRequestService, FriendRequestHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new FriendRequestHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IProfileService, ProfileHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new ProfileHttpService(factory.CreateClient("API"));
});



builder.Services.AddScoped<IBlockService, BlockHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new BlockHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IDirectMessageService, DirectMessageHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new DirectMessageHttpService(factory.CreateClient("API"));
});

// 1. Tell ASP.NET we want to be able to access the current HTTP Request inside our services
builder.Services.AddHttpContextAccessor(); 

// 2. Dynamically build the CurrentUserContext per request!
builder.Services.AddScoped<CurrentUserContext>(sp =>
{
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var userPrincipal = httpContextAccessor.HttpContext?.User;

    // Look inside the Cookie for the user's ID
    var userIdString = userPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier);

    if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out Guid realUserId))
    {
        // We found a logged-in user!
        return new CurrentUserContext(realUserId);
    }

    // Fallback for when the user is sitting on the Login page and has no cookie yet
    return new CurrentUserContext(Guid.Empty);
});
builder.Services.AddScoped<IAnnouncementService, AnnouncementHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new AnnouncementHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IReputationService, ReputationHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new ReputationHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<INotificationService, NotificationHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new NotificationHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IAchievementService, AchievementHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new AchievementHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IUserService, UserHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var currentUserContext = sp.GetRequiredService<CurrentUserContext>();
    return new UserHttpService(factory.CreateClient("API"), currentUserContext);
});

builder.Services.AddScoped<IAuthenticationService, AuthenticationHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new AuthenticationHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IMemoryService, MemoryHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new MemoryHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IDiscussionService, DiscussionHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new DiscussionHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IEventService, EventHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new EventHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IEventStatisticsService, EventStatisticsHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new EventStatisticsHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IAttendedEventService, ChatAndEvents.Data.EventsData.Services.attendedEventServices.AttendedEventHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new ChatAndEvents.Data.EventsData.Services.attendedEventServices.AttendedEventHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IQuestService, QuestHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new QuestHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IQuestApprovalService, QuestApprovalHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new QuestApprovalHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<IGroupService, ChatAndEvents.Web.Services.GroupHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new ChatAndEvents.Web.Services.GroupHttpService(factory.CreateClient("API"));
});

builder.Services.AddScoped<ISearchService, SearchHttpService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new SearchHttpService(factory.CreateClient("API"));
});

// Add services to the container.
builder.Services.AddControllersWithViews();


//cookie authentification
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
    });
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient("API", client =>
{
    var apiHostPort = builder.Configuration["Api:HostPort"]
        ?? Environment.GetEnvironmentVariable("API_HOSTPORT");

    var apiBaseAddress = !string.IsNullOrWhiteSpace(apiHostPort)
        ? $"http://{apiHostPort.TrimEnd('/')}/"
        : builder.Configuration["Api:BaseAddress"] ?? "http://localhost:5572/";
    client.BaseAddress = new Uri(apiBaseAddress);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }

    return handler;
});


try
{
    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await contextFactory.CreateDbContextAsync();
        await CommunityHubDatabaseInitializer.InitializeAsync(db);
    }

    var isRender = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RENDER"));
    if (!app.Environment.IsDevelopment() && !isRender)
    {
        app.UseHttpsRedirection();
    }

    var uploadsRoot = builder.Configuration["Uploads:Root"]
        ?? Environment.GetEnvironmentVariable("UPLOADS_ROOT")
        ?? Path.Combine(app.Environment.WebRootPath, "uploads");
    Directory.CreateDirectory(uploadsRoot);

    app.UseStaticFiles();
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsRoot),
        RequestPath = "/uploads",
    });
    app.UseRouting();
    app.UseSession();
    app.UseAuthentication();
    app.UseAuthorization();



    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=MainWindow}/{action=Index}/{id?}");

    app.MapGet("/health", () => Results.Ok("OK")).AllowAnonymous();

    app.Run();
}catch(Exception ex)
{
    Console.WriteLine("STARTUP ERROR: " + ex.Message);
    Console.WriteLine(ex.StackTrace);
    Console.ReadLine();
}

