
using System.Security.Claims;
using FlowERP.Identity.Api.Authorization;
using FlowERP.Identity.Api.Contracts.Users;
using FlowERP.Identity.Application.Authorization;
using FlowERP.Identity.Application.Roles.DTOs;
using FlowERP.Identity.Application.UserRoles;
using FlowERP.Identity.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowERP.Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController(
    CreateUserService createUserService,
    AssignRoleService assignRoleService,
    GetUserRolesService getUserRolesService,
    RemoveRoleService removeRoleService,
    GetUserPermissionsService getUserPermissionsService) : ControllerBase
{
    [HasPermission(PermissionNames.Users.Create)]
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Guid>> Create(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        // Execute the user creation use case.
        var userId = await createUserService.CreateAsync(
            request.Email,
            request.Password,
            cancellationToken);

        // Return the identifier of the created user.
        return Created($"/api/users/{userId}", userId);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        // Retrieve the authenticated user's claims.
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        var email = User.FindFirstValue(
            ClaimTypes.Email);

        return Ok(new
        {
            UserId = userId,
            Email = email
        });
    }

    [HttpGet("{userId:guid}/permissions")]
    [HasPermission(PermissionNames.Permissions.Read)]
    [ProducesResponseType(typeof(IReadOnlyCollection<string>),StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<string>>> GetPermissions(Guid userId,CancellationToken ct) =>
        Ok(await getUserPermissionsService.GetAsync(userId,ct));

    [HttpPost("{userId:guid}/roles/{roleId:guid}")]
    [HasPermission(PermissionNames.Roles.Assign)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignRole(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        // Execute the role assignment use case.
        await assignRoleService.AssignAsync(
            userId,
            roleId,
            cancellationToken);

        // Return 204 when the role is assigned successfully.
        return NoContent();
    }

    [HttpDelete("{userId:guid}/roles/{roleId:guid}")]
    [HasPermission(PermissionNames.Roles.Assign)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRole(Guid userId, Guid roleId, CancellationToken cancellationToken)
    {
        await removeRoleService.RemoveAsync(userId, roleId, cancellationToken);
        return NoContent();
    }

    [HttpGet("{userId:guid}/roles")]
    [HasPermission(PermissionNames.Roles.Read)]
    [ProducesResponseType(typeof(IReadOnlyList<RoleResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<RoleResponseDto>>> GetUserRoles(
        Guid userId,
        CancellationToken cancellationToken)
    {
        // Retrieve all roles assigned to the specified user.
        var roles = await getUserRolesService.GetByUserIdAsync(
            userId,
            cancellationToken);

        // Return the user's assigned roles.
        return Ok(roles);
    }
}
