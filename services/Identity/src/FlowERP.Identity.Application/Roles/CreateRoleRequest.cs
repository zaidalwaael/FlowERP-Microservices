
using System.ComponentModel.DataAnnotations;

namespace FlowERP.Identity.Api.Contracts.Roles;

public sealed class CreateRoleRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }
}
