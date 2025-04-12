using Microsoft.EntityFrameworkCore;
using dotamix.Models;

namespace dotamix.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<TournamentParticipant> TournamentParticipants { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Match> Matches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Nickname)
                .IsUnique();

            modelBuilder.Entity<TournamentParticipant>()
                .HasIndex(tp => new { tp.UserId, tp.TournamentId })
                .IsUnique();

            modelBuilder.Entity<Match>()
                .HasOne(m => m.HomeTeam)
                .WithMany(t => t.HomeMatches)
                .HasForeignKey(m => m.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.AwayTeam)
                .WithMany(t => t.AwayMatches)
                .HasForeignKey(m => m.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Конфигурация связей для следующих матчей победителей
            modelBuilder.Entity<Match>()
                .HasOne(m => m.WinnerNextMatch)
                .WithMany(m => m.WinnerPreviousMatches)
                .HasForeignKey(m => m.WinnerNextMatchId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Конфигурация связей для следующих матчей проигравших
            modelBuilder.Entity<Match>()
                .HasOne(m => m.LoserNextMatch)
                .WithMany(m => m.LoserPreviousMatches)
                .HasForeignKey(m => m.LoserNextMatchId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Конфигурация полей для сидов команд
            modelBuilder.Entity<Match>()
                .Property(m => m.HomeTeamSeed)
                .IsRequired(false);

            modelBuilder.Entity<Match>()
                .Property(m => m.AwayTeamSeed)
                .IsRequired(false);
        }
    }
} 