using Microsoft.EntityFrameworkCore;
using NewsDigestBot.Core.Entities;

namespace NewsDigestBot.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Username).HasMaxLength(255);
            e.Property(u => u.FirstName).HasMaxLength(255);
        });

        // Topic
        modelBuilder.Entity<Topic>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasIndex(t => t.Slug).IsUnique();
            e.Property(t => t.Slug).HasMaxLength(50);
            e.Property(t => t.Name).HasMaxLength(100);
        });

        // Article
        modelBuilder.Entity<Article>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.Url).IsUnique(); 
            e.Property(a => a.Url).HasMaxLength(2048);
            e.Property(a => a.Title).HasMaxLength(512);
            e.HasOne(a => a.Topic)
             .WithMany(t => t.Articles)
             .HasForeignKey(a => a.TopicId);
        });

        // Subscription 
        modelBuilder.Entity<Subscription>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasIndex(s => new { s.UserId, s.TopicId }).IsUnique();
            e.HasOne(s => s.User)
             .WithMany(u => u.Subscriptions)
             .HasForeignKey(s => s.UserId);
            e.HasOne(s => s.Topic)
             .WithMany(t => t.Subscriptions)
             .HasForeignKey(s => s.TopicId);
        });

        // Seed 
        modelBuilder.Entity<Topic>().HasData(
            new Topic { Id = 1, Name = "Технологии", Slug = "tech", Description = "IT, AI, гаджеты" },
            new Topic { Id = 2, Name = "Наука", Slug = "science", Description = "Научные открытия" },
            new Topic { Id = 3, Name = "Бизнес", Slug = "business", Description = "Экономика и финансы" },
            new Topic { Id = 4, Name = "Спорт", Slug = "sport", Description = "Спортивные новости" },
            new Topic { Id = 5, Name = "Мир", Slug = "world", Description = "Мировые новости" }
        );
    }
}