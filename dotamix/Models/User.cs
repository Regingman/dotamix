using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace dotamix.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [StringLength(100)]
        public string? Name { get; set; }
        public string? SteamId { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        [StringLength(50)]
        public string Nickname { get; set; }

        public virtual ICollection<TournamentParticipant> Participations { get; set; }
    }
} 