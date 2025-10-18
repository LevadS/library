namespace LevadS.Examples.Exceptions;

public class IntException(int fallbackValue) : Exception
{
    public int FallbackValue { get; } = fallbackValue;
}