namespace LuShop.Core;

public class DomainException : Exception
{
    /// <summary>
    /// Exception personalizado
    /// </summary>
    /// <param name="message"></param>
    public DomainException(string message) : base(message)
    {
        
    }
/// <summary>
/// Contrutor para encadear erros
/// </summary>
/// <param name="message"></param>
/// <param name="innerException"></param>
    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
        
    }
}