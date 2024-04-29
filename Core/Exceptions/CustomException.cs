
namespace Core.Exceptions;

public class CustomException : Exception
{
    // Constructors
    public CustomException() : base() { }

    public CustomException(string message) : base(message) { }

    public CustomException(string message, Exception innerException) : base(message, innerException) { }
}
