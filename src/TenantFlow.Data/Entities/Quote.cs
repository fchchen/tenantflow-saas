using System.ComponentModel.DataAnnotations;
using TenantFlow.Data.Abstractions;

namespace TenantFlow.Data.Entities;

public class Quote : ITenantScoped, ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    [MaxLength(30)]
    public string QuoteNumber { get; set; } = string.Empty;

    [MaxLength(160)]
    public string CustomerName { get; set; } = string.Empty;

    public decimal Premium { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedUtc { get; set; }
}
