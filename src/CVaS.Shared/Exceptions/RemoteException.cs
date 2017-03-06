namespace CVaS.Shared.Exceptions
{
    public class RemoteException : ApiException
    {
        public RemoteException(ApiException innerExceptionType) 
            : base("Remote Exception: " + innerExceptionType.Message, innerExceptionType.StatusCode)
        {
        }
    }
}