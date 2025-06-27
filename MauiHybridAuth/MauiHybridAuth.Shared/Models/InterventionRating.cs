using System.ComponentModel.DataAnnotations.Schema;

namespace MauiHybridAuth.Shared.Models;

public class InterventionRating
{
    public Guid Id { get; set; }

    public Guid InterventionId { get; set; }

    public Intervention Intervention { get; set; } = default!;

    public string ApplicationUserId { get; set; } = default!;

    public int Rating { get; set; }
}