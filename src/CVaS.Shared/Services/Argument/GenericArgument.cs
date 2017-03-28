using System;
using System.Globalization;

namespace CVaS.Shared.Services.Argument
{
    public class GenericArgument<T> : Argument
    {
        public T Content { get; set; }

        public GenericArgument(T content) : base(ArgumentType.Raw)
        {
            Content = content;
        }

        public override string ToString()
        {
            if (Content is IFormattable)
            {
                return ((IFormattable)Content).ToString(null, CultureInfo.InvariantCulture);
            }

            return Content.ToString();
        }
    }
}