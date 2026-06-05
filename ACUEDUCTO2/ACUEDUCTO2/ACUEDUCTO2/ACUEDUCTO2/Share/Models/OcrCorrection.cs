using System;

namespace ACUEDUCTO2.Share.Models
{
    public sealed class OcrCorrection
    {
        public int Id { get; set; }
        public string ImageSha256 { get; set; } = string.Empty;
        public string? NumeroMedidor { get; set; }
        public double? LecturaM3 { get; set; }
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }
}
