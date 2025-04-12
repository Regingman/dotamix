using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotamix.Models
{
    public class Match
    {
        public int Id { get; set; }

        public int TournamentId { get; set; }
        public virtual Tournament Tournament { get; set; }

        public int? HomeTeamId { get; set; }
        public virtual Team HomeTeam { get; set; }
        public int? HomeTeamSeed { get; set; }

        public int? AwayTeamId { get; set; }
        public virtual Team AwayTeam { get; set; }
        public int? AwayTeamSeed { get; set; }

        public DateTime? ScheduledTime { get; set; }

        public int? HomeTeamScore { get; set; }
        public int? AwayTeamScore { get; set; }

        public bool IsCompleted { get; set; }

        public BracketType BracketType { get; set; }
        public int Round { get; set; }
        public int MatchNumber { get; set; }

        public int? WinnerNextMatchId { get; set; }
        public virtual Match WinnerNextMatch { get; set; }
        public virtual ICollection<Match> WinnerPreviousMatches { get; set; }

        public int? LoserNextMatchId { get; set; }
        public virtual Match LoserNextMatch { get; set; }
        public virtual ICollection<Match> LoserPreviousMatches { get; set; }

        public int? WinnerNextMatchPosition { get; set; }
        public int? LoserNextMatchPosition { get; set; }

        public bool IsGrandFinal { get; set; }
        public bool IsSecondGrandFinal { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Match()
        {
            WinnerPreviousMatches = new HashSet<Match>();
            LoserPreviousMatches = new HashSet<Match>();
        }
    }
} 