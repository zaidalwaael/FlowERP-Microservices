
using FlowERP.Identity.Api.Authorization;
using FlowERP.Identity.Api.Contracts.Users;
using FlowERP.Identity.Application.Authorization;
using FlowERP.Identity.Application.Roles.DTOs;
using FlowERP.Identity.Application.UserRoles;
using FlowERP.Identity.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace FlowERP.Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController(
    CreateUserService createUserService,
    AssignRoleService assignRoleService,
    GetUserRolesService getUserRolesService,
    RemoveRoleService removeRoleService,
    GetUserPermissionsService getUserPermissionsService,
    ManageUsersService manageUsersService) : ControllerBase
{
    // Get all users.
    [HttpGet]
    [HasPermission(PermissionNames.Users.Read)]
    [ProducesResponseType(
        typeof(IReadOnlyList<UserResponseDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserResponseDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var users = await manageUsersService.GetAllAsync(
            cancellationToken);

        return Ok(users);
    }

    // Get user by ID.
    [HttpGet("{userId:guid}")]
    [HasPermission(PermissionNames.Users.Read)]
    [ProducesResponseType(
        typeof(UserResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> GetById(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await manageUsersService.GetAsync(
            userId,
            cancellationToken);

        return Ok(user);
    }

    // Update user email.
    [HttpPut("{userId:guid}/email")]
    [HasPermission(PermissionNames.Users.Update)]
    [ProducesResponseType(
        typeof(UserResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponseDto>> UpdateEmail(
        Guid userId,
        UpdateUserEmailRequest request,
        CancellationToken cancellationToken)
    {
        var user = await manageUsersService.UpdateEmailAsync(
            userId,
            request.Email,
            cancellationToken);

        return Ok(user);
    }

    // Activate or deactivate user.
    [HttpPatch("{userId:guid}/status")]
    [HasPermission(PermissionNames.Users.Deactivate)]
    [ProducesResponseType(
        typeof(UserResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> SetStatus(
        Guid userId,
        SetUserActiveRequest request,
        CancellationToken cancellationToken)
    {
        var user = await manageUsersService.SetActiveAsync(
            userId,
            request.IsActive,
            cancellationToken);

        return Ok(user);
    }

    // Create a new user.
    [HttpPost]
    [HasPermission(PermissionNames.Users.Create)]
    [ProducesResponseType(
        typeof(Guid),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Guid>> Create(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var userId = await createUserService.CreateAsync(
            request.Email,
            request.Password,
            cancellationToken);

        return Created($"/api/users/{userId}", userId);
    }

    // Get currently authenticated user.
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetCurrentUser()
    {
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

    // Get effective permissions for a user.
    [HttpGet("{userId:guid}/permissions")]
    [HasPermission(PermissionNames.Permissions.Read)]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<string>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<string>>> GetPermissions(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var permissions = await getUserPermissionsService.GetAsync(
            userId,
            cancellationToken);

        return Ok(permissions);
    }

    // Assign role to user.
    [HttpPost("{userId:guid}/roles/{roleId:guid}")]
    [HasPermission(PermissionNames.Roles.Assign)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignRole(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        await assignRoleService.AssignAsync(
            userId,
            roleId,
            cancellationToken);

        return NoContent();
    }

    // Remove role from user.
    [HttpDelete("{userId:guid}/roles/{roleId:guid}")]
    [HasPermission(PermissionNames.Roles.Assign)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRole(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        await removeRoleService.RemoveAsync(
            userId,
            roleId,
            cancellationToken);

        return NoContent();
    }

    // Get all roles assigned to a user.
    [HttpGet("{userId:guid}/roles")]
    [HasPermission(PermissionNames.Roles.Read)]
    [ProducesResponseType(
        typeof(IReadOnlyList<RoleResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<RoleResponseDto>>> GetUserRoles(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var roles = await getUserRolesService.GetByUserIdAsync(
            userId,
            cancellationToken);

        return Ok(roles);
    }
}
