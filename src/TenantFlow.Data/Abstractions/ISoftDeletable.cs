namespace TenantFlow.Data.Abstractions;

public interface ISoftDeletable
{
    DateTime? DeletedUtc { get; set; }
}
