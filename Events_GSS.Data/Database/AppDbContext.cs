using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.CommunityHub.Models;
using ChatAndEvents.Data.Database.Configurations;
using ChatAndEvents.Data.EventsData.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatAndEvents.Data.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Chat
    public DbSet<ChatData.domain.User> Users { get; set; }
    public DbSet<Friend> Friends { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Participant> Participants { get; set; }

    // Events identity/reputation mirror. This table intentionally shares GUIDs with Users.
    public DbSet<ChatAndEvents.Data.EventsData.Models.User> EventUsers { get; set; }

    // Community hub
    public DbSet<HubCommunity> HubCommunities { get; set; }
    public DbSet<CommunityMembership> CommunityMemberships { get; set; }
    public DbSet<CommunityPost> CommunityPosts { get; set; }
    public DbSet<DatingProfile> DatingProfiles { get; set; }
    public DbSet<DatingInterest> DatingInterests { get; set; }
    public DbSet<DatingPhoto> DatingPhotos { get; set; }
    public DbSet<DatingSwipe> DatingSwipes { get; set; }

    // Events
    public DbSet<Event> Events { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<AttendedEvent> AttendedEvents { get; set; }
    public DbSet<Memory> Memories { get; set; }
    public DbSet<MemoryLike> MemoryLikes { get; set; }
    public DbSet<UserReputationScore> UserReputationScores { get; set; }
    public DbSet<AnnouncementReadReceipt> AnnouncementReadReceipts { get; set; }
    public DbSet<QuestMemory> QuestMemories { get; set; }
    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<AnnouncementReaction> AnnouncementReactions { get; set; }
    
    public DbSet<Discussion> Discussions { get; set; }
    public DbSet<DiscussionMessage> DiscussionMessages { get; set; }
    public DbSet<DiscussionReaction> DiscussionReactions { get; set; }
    public DbSet<DiscussionMute> DiscussionMutes { get; set; }
    public DbSet<Quest> Quests { get; set; }
    
    public DbSet<Achievement> Achievements { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new FriendConfiguration());
        modelBuilder.ApplyConfiguration(new ConversationConfiguration());
        modelBuilder.ApplyConfiguration(new MessageConfiguration());
        modelBuilder.ApplyConfiguration(new ParticipantConfiguration());

        modelBuilder.Entity<ChatAndEvents.Data.EventsData.Models.User>(entity =>
        {
            entity.ToTable("User");
            entity.HasKey(user => user.UserId);
            entity.Property(user => user.Name).IsRequired();
        });

        modelBuilder.Entity<HubCommunity>(entity =>
        {
            entity.ToTable("HubCommunities");
            entity.HasKey(community => community.Id);
            entity.Property(community => community.Name).HasMaxLength(120).IsRequired();
            entity.Property(community => community.Description).HasMaxLength(1000).IsRequired();
            entity.Property(community => community.Category).HasMaxLength(80).IsRequired();
            entity.HasIndex(community => community.Name).IsUnique();
            entity.HasOne(community => community.Owner)
                .WithMany()
                .HasForeignKey(community => community.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CommunityMembership>(entity =>
        {
            entity.ToTable("CommunityMemberships");
            entity.HasKey(member => new { member.CommunityId, member.UserId });
            entity.HasOne(member => member.Community)
                .WithMany(community => community.Members)
                .HasForeignKey(member => member.CommunityId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(member => member.User)
                .WithMany()
                .HasForeignKey(member => member.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CommunityPost>(entity =>
        {
            entity.ToTable("CommunityPosts");
            entity.HasKey(post => post.Id);
            entity.Property(post => post.Title).HasMaxLength(160).IsRequired();
            entity.Property(post => post.Body).HasMaxLength(4000).IsRequired();
            entity.HasOne(post => post.Community)
                .WithMany(community => community.Posts)
                .HasForeignKey(post => post.CommunityId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(post => post.Author)
                .WithMany()
                .HasForeignKey(post => post.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DatingProfile>(entity =>
        {
            entity.ToTable("DatingProfiles");
            entity.HasKey(profile => profile.UserId);
            entity.Property(profile => profile.DisplayName).HasMaxLength(120).IsRequired();
            entity.Property(profile => profile.Gender).HasMaxLength(60);
            entity.Property(profile => profile.PreferredGenders).HasMaxLength(160);
            entity.Property(profile => profile.Location).HasMaxLength(160);
            entity.Property(profile => profile.DatingBio).HasMaxLength(1000);
            entity.Property(profile => profile.LoverType).HasMaxLength(80);
            entity.HasOne(profile => profile.User)
                .WithOne()
                .HasForeignKey<DatingProfile>(profile => profile.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DatingInterest>(entity =>
        {
            entity.ToTable("DatingInterests");
            entity.HasKey(interest => interest.Id);
            entity.Property(interest => interest.Name).HasMaxLength(80).IsRequired();
            entity.HasIndex(interest => new { interest.UserId, interest.Name }).IsUnique();
            entity.HasOne(interest => interest.DatingProfile)
                .WithMany(profile => profile.Interests)
                .HasForeignKey(interest => interest.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DatingPhoto>(entity =>
        {
            entity.ToTable("DatingPhotos");
            entity.HasKey(photo => photo.Id);
            entity.Property(photo => photo.Url).HasMaxLength(1000).IsRequired();
            entity.HasOne(photo => photo.DatingProfile)
                .WithMany(profile => profile.Photos)
                .HasForeignKey(photo => photo.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DatingSwipe>(entity =>
        {
            entity.ToTable("DatingSwipes");
            entity.HasKey(swipe => swipe.Id);
            entity.Property(swipe => swipe.Action).HasMaxLength(20).IsRequired();
            entity.HasIndex(swipe => new { swipe.FromUserId, swipe.ToUserId }).IsUnique();
            entity.HasOne<ChatAndEvents.Data.ChatData.domain.User>()
                .WithMany()
                .HasForeignKey(swipe => swipe.FromUserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ChatAndEvents.Data.ChatData.domain.User>()
                .WithMany()
                .HasForeignKey(swipe => swipe.ToUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.ApplyConfiguration(new EventConfiguration());
        modelBuilder.ApplyConfiguration(new AttendedEventConfiguration());
        modelBuilder.ApplyConfiguration(new MemoryConfiguration());
        modelBuilder.ApplyConfiguration(new MemoryLikeConfiguration());
        modelBuilder.ApplyConfiguration(new UserReputationScoreConfiguration());
        modelBuilder.ApplyConfiguration(new AnnouncementReadReceiptConfiguration());
        modelBuilder.ApplyConfiguration(new QuestMemoryConfiguration());
        
        modelBuilder.ApplyConfiguration(new DiscussionConfiguration());
        modelBuilder.ApplyConfiguration(new DiscussionReactionConfiguration());
        modelBuilder.ApplyConfiguration(new DiscussionMuteConfiguration());
        modelBuilder.ApplyConfiguration(new QuestConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
        modelBuilder.ApplyConfiguration(new AnnouncementConfiguration());
        modelBuilder.ApplyConfiguration(new AnnouncementReactionConfiguration());

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");
            entity.HasKey(category => category.CategoryId);
            entity.Property(category => category.Title).IsRequired();
        });

        modelBuilder.Entity<Category>().HasData(
        new Category { CategoryId = 1, Title = "NATURE" },
        new Category { CategoryId = 2, Title = "FITNESS" },
        new Category { CategoryId = 3, Title = "MUSIC" },
        new Category { CategoryId = 4, Title = "SOCIAL" },
        new Category { CategoryId = 5, Title = "ART" },
        new Category { CategoryId = 6, Title = "PETS" },
        new Category { CategoryId = 7, Title = "TECH" },
        new Category { CategoryId = 8, Title = "FUN" }
        );

        modelBuilder.Entity<Achievement>().HasData(
            new Achievement { AchievementId = 1, Name = "First Steps", Description = "Attend your first event.", IsUnlocked = false },
            new Achievement { AchievementId = 2, Name = "Proper Host", Description = "Create 3 events.", IsUnlocked = false },
            new Achievement { AchievementId = 3, Name = "Quest Solver", Description = "Approve 25 quest submissions.", IsUnlocked = false },
            new Achievement { AchievementId = 4, Name = "Memory Keeper", Description = "Add 50 memories with photos.", IsUnlocked = false },
            new Achievement { AchievementId = 5, Name = "Social Butterfly", Description = "Send 100 discussion messages.", IsUnlocked = false },
            new Achievement { AchievementId = 6, Name = "Event Veteran", Description = "Attend 10 events.", IsUnlocked = false },
            new Achievement { AchievementId = 7, Name = "Perfectionist", Description = "Complete every quest in an event.", IsUnlocked = false }
        );

    }
}
