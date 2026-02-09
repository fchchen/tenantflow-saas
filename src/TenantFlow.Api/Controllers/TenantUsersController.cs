using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TenantFlow.Api.Models;
using TenantFlow.Api.Services;
using TenantFlow.Data;
using TenantFlow.Data.Abstractions;
using TenantFlow.Data.Entities;

namespace TenantFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/tenant/users")]
public class TenantUsersController : ControllerBase
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "TenantAdmin",
        "TenantUser"
    };

    private readonly TenantFlowDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditService _audit;

    public TenantUsersController(TenantFlowDbContext db, ITenantContext tenantContext, IAuditService audit)
    {
        _db = db;
        _tenantContext = tenantContext;
        _audit = audit;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TenantUserResponse>>> Get(CancellationToken cancellationToken)
    {
        if (!CanManageTenantUsers())
        {
            return Forbid();
        }

        var users = await _db.TenantUsers
            .AsNoTracking()
            .Where(tu => tu.TenantId == _tenantContext.TenantId)
            .Join(_db.Users, tu => tu.UserId, u => u.Id,
                (tu, u) => new TenantUserResponse(u.Id, u.Email, u.DisplayName, tu.Role, u.IsActive))
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);

        return Ok(users);
    }

    [HttpPost]
    public async Task<ActionResult<TenantUserResponse>> Create([FromBody] CreateTenantUserRequest request, CancellationToken cancellationToken)
    {
        if (!CanManageTenantUsers())
        {
            return Forbid();
        }

        if (!AllowedRoles.Contains(request.Role))
        {
            return BadRequest(new { message = "Role must be TenantAdmin or TenantUser." });
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.IgnoreQueryFilters().SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null)
        {
            user = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                DisplayName = request.DisplayName.Trim(),
                PasswordHash = string.Empty,
                IsActive = true,
                CreatedUtc = DateTime.UtcNow
            };
            _db.Users.Add(user);
        }

        var membershipExists = await _db.TenantUsers.AnyAsync(m => m.TenantId == _tenantContext.TenantId && m.UserId == user.Id, cancellationToken);
        if (membershipExists)
        {
            return Conflict(new { message = "User already belongs to this tenant." });
        }

        _db.TenantUsers.Add(new TenantUser
        {
            TenantId = _tenantContext.TenantId,
            UserId = user.Id,
            Role = request.Role,
            JoinedUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(_tenantContext.TenantId, _tenantContext.UserId, "tenant-user.create", nameof(AppUser), user.Id.ToString(), null, cancellationToken);

        return CreatedAtAction(nameof(Get), new TenantUserResponse(user.Id, user.Email, user.DisplayName, request.Role, user.IsActive));
    }

    [HttpPatch("{userId:guid}/roles")]
    public async Task<ActionResult> UpdateRole([FromRoute] Guid userId, [FromBody] UpdateTenantUserRoleRequest request, CancellationToken cancellationToken)
    {
        if (!CanManageTenantUsers())
        {
            return Forbid();
        }

        if (!AllowedRoles.Contains(request.Role))
        {
            return BadRequest(new { message = "Role must be TenantAdmin or TenantUser." });
        }

        var membership = await _db.TenantUsers.SingleOrDefaultAsync(m => m.TenantId == _tenantContext.TenantId && m.UserId == userId, cancellationToken);
        if (membership is null)
        {
            return NotFound();
        }

        membership.Role = request.Role;
        await _db.SaveChangesAsync(cancellationToken);

        var metadata = JsonSerializer.Serialize(new { role = request.Role });
        await _audit.LogAsync(_tenantContext.TenantId, _tenantContext.UserId, "tenant-user.role-update", nameof(TenantUser), userId.ToString(), metadata, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{userId:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        if (!CanManageTenantUsers())
        {
            return Forbid();
        }

        var membership = await _db.TenantUsers.SingleOrDefaultAsync(m => m.TenantId == _tenantContext.TenantId && m.UserId == userId, cancellationToken);
        if (membership is null)
        {
            return NotFound();
        }

        var hasOtherMemberships = await _db.TenantUsers.AnyAsync(
            m => m.UserId == userId && m.TenantId != _tenantContext.TenantId,
            cancellationToken);

        _db.TenantUsers.Remove(membership);

        if (!hasOtherMemberships)
        {
            var user = await _db.Users.IgnoreQueryFilters().SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user is not null)
            {
                user.IsActive = false;
                user.DeletedUtc = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(_tenantContext.TenantId, _tenantContext.UserId, "tenant-user.delete", nameof(AppUser), userId.ToString(), null, cancellationToken);

        return NoContent();
    }

    private bool CanManageTenantUsers()
    {
        return _tenantContext.Roles.Contains("PlatformAdmin", StringComparer.OrdinalIgnoreCase)
            || _tenantContext.Roles.Contains("TenantAdmin", StringComparer.OrdinalIgnoreCase);
    }
}
