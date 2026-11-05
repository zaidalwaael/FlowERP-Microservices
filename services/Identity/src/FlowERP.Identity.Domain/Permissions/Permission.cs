namespace FlowERP.Identity.Domain.Permissions;

public sealed class Permission
{
    private Permission()
    {
        // Required by EF Core when loading a permission from the database.
    }

    public Permission(
        Guid id,
        string name)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(
                "Permission ID is required.",
                nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Permission name is required.",
                nameof(name));

        var normalizedName = name.Trim().ToLowerInvariant();

        if (normalizedName.Length > 150)
            throw new ArgumentException(
                "Permission name cannot exceed 150 characters.",
                nameof(name));

        Id = id;
        Name = normalizedName;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;
}