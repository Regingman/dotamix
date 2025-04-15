using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace dotamix.Models
{
    public class Team
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int TournamentId { get; set; }
        public virtual Tournament Tournament { get; set; }

        public int CaptainId { get; set; }
        public virtual User Captain { get; set; }

        public bool IsWinner { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int Order { get; set; }

        public virtual ICollection<TournamentParticipant> Players { get; set; }
        public virtual ICollection<Match> HomeMatches { get; set; }
        public virtual ICollection<Match> AwayMatches { get; set; }
    }
}