namespace HRSystem.Business.Exceptions;

public class NotFoundException : Exception
{
    public string? Area { get; }

    public string Controller { get; }

    public string Action { get; }

    public NotFoundException(string message)
        : this(message, controller: "Home", action: "Index")
    {
    }

    public NotFoundException(string message, string controller, string action = "Index", string? area = null)
        : base(message)
    {
        Controller = controller;
        Action = action;
        Area = area;
    }
}
