﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using dotamix.Data;

#nullable disable

namespace dotamix.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250419123624_steam")]
    partial class steam
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.15")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("dotamix.Models.Match", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("AwayTeamId")
                        .HasColumnType("integer");

                    b.Property<int?>("AwayTeamScore")
                        .HasColumnType("integer");

                    b.Property<int?>("AwayTeamSeed")
                        .HasColumnType("integer");

                    b.Property<int>("BracketType")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int?>("HomeTeamId")
                        .HasColumnType("integer");

                    b.Property<int?>("HomeTeamScore")
                        .HasColumnType("integer");

                    b.Property<int?>("HomeTeamSeed")
                        .HasColumnType("integer");

                    b.Property<bool>("IsCompleted")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsGrandFinal")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsSecondGrandFinal")
                        .HasColumnType("boolean");

                    b.Property<int?>("LoserNextMatchId")
                        .HasColumnType("integer");

                    b.Property<int?>("LoserNextMatchPosition")
                        .HasColumnType("integer");

                    b.Property<int>("MatchNumber")
                        .HasColumnType("integer");

                    b.Property<int>("Round")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("ScheduledTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("TournamentId")
                        .HasColumnType("integer");

                    b.Property<int?>("WinnerNextMatchId")
                        .HasColumnType("integer");

                    b.Property<int?>("WinnerNextMatchPosition")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("AwayTeamId");

                    b.HasIndex("HomeTeamId");

                    b.HasIndex("LoserNextMatchId");

                    b.HasIndex("TournamentId");

                    b.HasIndex("WinnerNextMatchId");

                    b.ToTable("Matches");
                });

            modelBuilder.Entity("dotamix.Models.Team", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("CaptainId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("IsWinner")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Order")
                        .HasColumnType("integer");

                    b.Property<int>("TournamentId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CaptainId");

                    b.HasIndex("TournamentId");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("dotamix.Models.Tournament", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("CurrentBracket")
                        .HasColumnType("integer");

                    b.Property<int>("CurrentRound")
                        .HasColumnType("integer");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("IsGrandFinalNeeded")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("NumberOfTeams")
                        .HasColumnType("integer");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Tournaments");
                });

            modelBuilder.Entity("dotamix.Models.TournamentParticipant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("HasPaid")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsCaptain")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsPaid")
                        .HasColumnType("boolean");

                    b.Property<int>("MMR")
                        .HasColumnType("integer");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<string>("Positions")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("RegistrationDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int?>("TeamId")
                        .HasColumnType("integer");

                    b.Property<int>("TournamentId")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("TeamId");

                    b.HasIndex("TournamentId");

                    b.HasIndex("UserId", "TournamentId")
                        .IsUnique();

                    b.ToTable("TournamentParticipants");
                });

            modelBuilder.Entity("dotamix.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Nickname")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("SteamId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Nickname")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("dotamix.Models.Match", b =>
                {
                    b.HasOne("dotamix.Models.Team", "AwayTeam")
                        .WithMany("AwayMatches")
                        .HasForeignKey("AwayTeamId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("dotamix.Models.Team", "HomeTeam")
                        .WithMany("HomeMatches")
                        .HasForeignKey("HomeTeamId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("dotamix.Models.Match", "LoserNextMatch")
                        .WithMany("LoserPreviousMatches")
                        .HasForeignKey("LoserNextMatchId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("dotamix.Models.Tournament", "Tournament")
                        .WithMany("Matches")
                        .HasForeignKey("TournamentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("dotamix.Models.Match", "WinnerNextMatch")
                        .WithMany("WinnerPreviousMatches")
                        .HasForeignKey("WinnerNextMatchId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("AwayTeam");

                    b.Navigation("HomeTeam");

                    b.Navigation("LoserNextMatch");

                    b.Navigation("Tournament");

                    b.Navigation("WinnerNextMatch");
                });

            modelBuilder.Entity("dotamix.Models.Team", b =>
                {
                    b.HasOne("dotamix.Models.User", "Captain")
                        .WithMany()
                        .HasForeignKey("CaptainId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("dotamix.Models.Tournament", "Tournament")
                        .WithMany("Teams")
                        .HasForeignKey("TournamentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Captain");

                    b.Navigation("Tournament");
                });

            modelBuilder.Entity("dotamix.Models.TournamentParticipant", b =>
                {
                    b.HasOne("dotamix.Models.Team", "Team")
                        .WithMany("Players")
                        .HasForeignKey("TeamId");

                    b.HasOne("dotamix.Models.Tournament", "Tournament")
                        .WithMany("Participants")
                        .HasForeignKey("TournamentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("dotamix.Models.User", "User")
                        .WithMany("Participations")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Team");

                    b.Navigation("Tournament");

                    b.Navigation("User");
                });

            modelBuilder.Entity("dotamix.Models.Match", b =>
                {
                    b.Navigation("LoserPreviousMatches");

                    b.Navigation("WinnerPreviousMatches");
                });

            modelBuilder.Entity("dotamix.Models.Team", b =>
                {
                    b.Navigation("AwayMatches");

                    b.Navigation("HomeMatches");

                    b.Navigation("Players");
                });

            modelBuilder.Entity("dotamix.Models.Tournament", b =>
                {
                    b.Navigation("Matches");

                    b.Navigation("Participants");

                    b.Navigation("Teams");
                });

            modelBuilder.Entity("dotamix.Models.User", b =>
                {
                    b.Navigation("Participations");
                });
#pragma warning restore 612, 618
        }
    }
}
