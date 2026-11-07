using FlowERP.Identity.Api.Authorization;
using FlowERP.Identity.Api.Contracts.Roles;
using FlowERP.Identity.Application.Authorization;
using FlowERP.Identity.Application.Roles;
using FlowERP.Identity.Application.RolePermissions;
using FlowERP.Identity.Application.Roles.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FlowERP.Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RolesController(
    CreateRoleService createRoleService,
    GetRoleByIdService getRoleByIdService,
    GetAllRolesService getAllRolesService,
    ManageRolePermissionsService rolePermissions) : ControllerBase
{
    [HttpGet("{roleId:guid}/permissions")]
    [HasPermission(PermissionNames.Roles.Read)]
    public async Task<ActionResult<IReadOnlyList<PermissionResponseDto>>> GetPermissions(Guid roleId,CancellationToken ct) =>
        Ok(await rolePermissions.GetAsync(roleId,ct));

    [HttpPost("{roleId:guid}/permissions/{permissionId:guid}")]
    [HasPermission(PermissionNames.Roles.ManagePermissions)]
    public async Task<IActionResult> AssignPermission(Guid roleId,Guid permissionId,CancellationToken ct) {
        await rolePermissions.AssignAsync(roleId,permissionId,ct);
        return NoContent();
    }

    [HttpDelete("{roleId:guid}/permissions/{permissionId:guid}")]
    [HasPermission(PermissionNames.Roles.ManagePermissions)]
    public async Task<IActionResult> RemovePermission(Guid roleId,Guid permissionId,CancellationToken ct) {
        await rolePermissions.RemoveAsync(roleId,permissionId,ct);
        return NoContent();
    }

    [HttpPost]
    [HasPermission(PermissionNames.Roles.Create)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Guid>> Create(
        CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        // Execute the role creation use case.
        var roleId = await createRoleService.CreateAsync(
            request.Name,
            cancellationToken);

        // Return the identifier of the created role.
        return Created($"/api/roles/{roleId}", roleId);
    }

    [HttpGet]
    [HasPermission(PermissionNames.Roles.Read)]
    [ProducesResponseType(
        typeof(IReadOnlyList<RoleResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<RoleResponseDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        // Execute the get all roles use case.
        var roles = await getAllRolesService.GetAllAsync(
            cancellationToken);

        // Return all roles, or an empty list if none exist.
        return Ok(roles);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionNames.Roles.Read)]
    [ProducesResponseType(typeof(RoleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleResponseDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        // Execute the get role by ID use case.
        var role = await getRoleByIdService.GetByIdAsync(
            id,
            cancellationToken);

        // Return 404 if the role does not exist.
        if (role is null)
            return NotFound();

        // Return the role details.
        return Ok(role);
    }
}