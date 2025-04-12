using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace dotamix.Models
{
    public class Tournament
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public TournamentStatus Status { get; set; }

        public int NumberOfTeams { get; set; }

        public int CurrentRound { get; set; }

        public BracketType CurrentBracket { get; set; }

        public bool IsGrandFinalNeeded { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<TournamentParticipant> Participants { get; set; }
        public virtual ICollection<Team> Teams { get; set; }
        public virtual ICollection<Match> Matches { get; set; }
    }
} 