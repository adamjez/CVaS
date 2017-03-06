namespace CVaS.Shared.Exceptions
{
    public class ArgumentMalformedException : BadRequestException
    {
        public ArgumentMalformedException(string message = "Given argument are malformed") : base(message)
        {
        }
    }
}