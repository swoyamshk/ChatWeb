﻿using AuthSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace AuthSystem.Areas.Identity.Data;

public class AuthDbContext : IdentityDbContext<ApplicationUser>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<Message> Messages { get; set; }
    public DbSet<ChatRoom> ChatRooms { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<ChatParticipant> ChatParticipants { get; set; }
    public DbSet<UserActivity> UserActivities { get; set; }
    public DbSet<FAQ> FAQs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure relationships
        builder.Entity<ChatParticipant>()
            .HasKey(cp => new { cp.ChatRoomId, cp.UserId });

        builder.Entity<ChatParticipant>()
            .HasOne(cp => cp.ChatRoom)
            .WithMany(cr => cr.Participants)
            .HasForeignKey(cp => cp.ChatRoomId);

        builder.Entity<ChatParticipant>()
            .HasOne(cp => cp.User)
            .WithMany(u => u.ChatRooms)
            .HasForeignKey(cp => cp.UserId);

        builder.Entity<ChatRoom>()
           .HasOne(cr => cr.Creator)
           .WithMany()
           .HasForeignKey(cr => cr.CreatorId)
           .OnDelete(DeleteBehavior.Restrict);

        // Seed FAQs
        builder.Entity<FAQ>().HasData(
            new FAQ { Id = 1, Question = "How can I create a new chat room?", Answer = "You can navigate through Create Chat Room." },
            new FAQ { Id = 2, Question = "Thank you", Answer = "You are welcome." },
            new FAQ { Id = 3, Question = "How do I reset my password?", Answer = "To reset your password, go to profile section." },
            new FAQ { Id = 4, Question = "How can I contact support?", Answer = "You can contact support by clicking on their socials." }
        );
    }
}
