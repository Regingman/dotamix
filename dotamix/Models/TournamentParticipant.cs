using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Collections.Generic;

namespace dotamix.Models
{
    public class TournamentParticipant
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int TournamentId { get; set; }
        public virtual Tournament Tournament { get; set; }

        public bool IsPaid { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public int? TeamId { get; set; }
        public virtual Team Team { get; set; }

        public bool IsCaptain { get; set; }
        public bool HasPaid { get; set; }
        public int MMR { get; set; }
        public string Positions { get; set; } // Хранится как строка с разделителями, например "1,2,4"
        public string? PhoneNumber { get; set; } 
        public DateTime CreatedAt { get; set; }

        public List<int> GetPositions()
        {
            if (string.IsNullOrEmpty(Positions))
                return new List<int>();
            return Positions.Split(',').Select(int.Parse).ToList();
        }

        public void SetPositions(List<int> positions)
        {
            Positions = string.Join(",", positions.OrderBy(p => p).Distinct());
        }

        public bool HasPosition(int position)
        {
            return GetPositions().Contains(position);
        }
    }
} 