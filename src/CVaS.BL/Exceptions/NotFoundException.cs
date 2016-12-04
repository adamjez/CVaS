﻿namespace CVaS.BL.Exceptions
{
    public class NotFoundException : ApiException
    {
        public NotFoundException(string message = "Object Not Found") : base(message, 404)
        {
        }
    }
}