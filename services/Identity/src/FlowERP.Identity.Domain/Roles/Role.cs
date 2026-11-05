namespace FlowERP.Identity.Domain.Roles;

public sealed class Role
{
    private Role()
    {
        // Required by EF Core when loading a role from the database.
    }

    public Role(
        Guid id,
        string name)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(
                "Role ID is required.",
                nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Role name is required.",
                nameof(name));

        var normalizedName = name.Trim();

        if (normalizedName.Length > 100)
            throw new ArgumentException(
                "Role name cannot exceed 100 characters.",
                nameof(name));

        Id = id;
        Name = normalizedName;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;
}