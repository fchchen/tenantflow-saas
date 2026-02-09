namespace TenantFlow.Data.Abstractions;

public interface ITenantScoped
{
    Guid TenantId { get; set; }
}
