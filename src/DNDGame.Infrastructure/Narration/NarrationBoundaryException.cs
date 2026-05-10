namespace DNDGame.Infrastructure.Narration;

public abstract class NarrationBoundaryException : Exception
{
    protected NarrationBoundaryException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public sealed class NarrationTransportException : NarrationBoundaryException
{
    public NarrationTransportException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public sealed class NarrationGuardrailException : NarrationBoundaryException
{
    public NarrationGuardrailException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}