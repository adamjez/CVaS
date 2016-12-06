namespace CVaS.BL.Exceptions
{
    public class TimeOutException : ApiException
    {
        public TimeOutException(string message = "TimeOut! Your algorithm run exceeded given time.") : base(message, 404)
        {
        }
    }
}