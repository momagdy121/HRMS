namespace HRSystem.Business.Exceptions;

public class SoftDeletedNameConflictException : Exception
{
    public SoftDeletedNameConflictException(string resourceType, int resourceId, string name)
        : base($"A deleted {resourceType.ToLowerInvariant()} named \"{name}\" already exists.")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
        Name = name;
    }

    public string ResourceType { get; }

    public int ResourceId { get; }

    public string Name { get; }
}
