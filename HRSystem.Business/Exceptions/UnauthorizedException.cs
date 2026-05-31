namespace HRSystem.Business.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException()
        : base("You must be signed in to access this resource.")
    {
    }

    public UnauthorizedException(string message)
        : base(message)
    {
    }
}
