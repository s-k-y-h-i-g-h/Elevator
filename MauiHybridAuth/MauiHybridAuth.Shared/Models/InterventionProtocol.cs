using System.ComponentModel.DataAnnotations;

namespace MauiHybridAuth.Shared.Models;

public class InterventionProtocol
{
    public Guid Id { get; set; }
    
    public Guid InterventionId { get; set; }
    public Intervention Intervention { get; set; } = default!;
    
    public Guid ProtocolId { get; set; }
    public Protocol Protocol { get; set; } = default!;
    
    [StringLength(500)]
    public string? Dosage { get; set; }
    
    [StringLength(500)]
    public string? Schedule { get; set; }
    
    [StringLength(1000)]
    public string? Notes { get; set; }
}