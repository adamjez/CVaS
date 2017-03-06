namespace CVaS.Shared.Exceptions
{
    public class NotFoundException : ApiException
    {
        public NotFoundException(string message) : base(message, 404)
        {
        }

        public NotFoundException() : base("Object Not Found", 404)
        {
        }
    }
}