using Elevator.Shared.Models.Interventions;

namespace Elevator.Shared.Models.Protocols;

/// <summary>
/// Junction table for many-to-many relationship between Protocols and Interventions
/// </summary>
public class ProtocolIntervention
{
    /// <summary>
    /// Unique identifier for this protocol-intervention association
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID of the protocol
    /// </summary>
    public int ProtocolId { get; set; }

    /// <summary>
    /// ID of the intervention
    /// </summary>
    public int InterventionId { get; set; }

    /// <summary>
    /// Dosage information for this intervention in the protocol
    /// </summary>
    public string? Dosage { get; set; }

    /// <summary>
    /// Schedule information for this intervention in the protocol
    /// </summary>
    public string? Schedule { get; set; }

    /// <summary>
    /// Additional notes about this intervention in the protocol
    /// </summary>
    public string? Notes { get; set; }

    // Navigation properties
    /// <summary>
    /// The protocol this association belongs to
    /// </summary>
    public virtual Protocol Protocol { get; set; } = null!;

    /// <summary>
    /// The intervention in this association
    /// </summary>
    public virtual Intervention Intervention { get; set; } = null!;
}