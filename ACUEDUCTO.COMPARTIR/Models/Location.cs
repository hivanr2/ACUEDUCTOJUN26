using System.Text.Json.Serialization;

namespace ACUEDUCTO.Models
{
    public class Location
    {
        [JsonPropertyName("latitud")]
        public double Latitud { get; set; }

        [JsonPropertyName("longitud")]
        public double Longitud { get; set; }
    }

}
