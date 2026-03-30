using System;

namespace Top5.Models
{
    public class DMSEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; }
        public string Utilisateur { get; set; } = string.Empty;
        public string Machine { get; set; } = string.Empty;
        public string Piece { get; set; } = string.Empty;
        public string Moule { get; set; } = string.Empty;
    }
}