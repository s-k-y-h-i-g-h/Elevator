namespace MauiHybridAuth.Shared.Models
{
    public class Formulation : Substance
    {
        public List<Compound> Constituents { get; set; } = new List<Compound>();
        public bool Liposomal { get; set; } = false;
        public bool Micronised { get; set; } = false;
        public bool ExtendedRelease { get; set; } = false;
    }
} 