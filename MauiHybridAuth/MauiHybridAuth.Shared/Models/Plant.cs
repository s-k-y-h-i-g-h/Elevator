namespace MauiHybridAuth.Shared.Models
{
    public class Plant : Substance
    {
        public List<Compound> Constituents { get; set; } = new List<Compound>();
    }
} 