using System.ComponentModel.DataAnnotations;

namespace TenantFlow.Api.Models;

public sealed record CreateTenantUserRequest(
    [param: Required, EmailAddress, MaxLength(200)] string Email,
    [param: Required, MaxLength(150)] string DisplayName,
    [param: Required, RegularExpression("^(TenantAdmin|TenantUser)$")] string Role);

public sealed record UpdateTenantUserRoleRequest(
    [param: Required, RegularExpression("^(TenantAdmin|TenantUser)$")] string Role);

public sealed record TenantUserResponse(Guid UserId, string Email, string DisplayName, string Role, bool IsActive);
