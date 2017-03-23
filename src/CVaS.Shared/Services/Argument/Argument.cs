namespace CVaS.Shared.Services.Argument
{
    public abstract class Argument
    {
        public ArgumentType Type { get; set; }

        protected Argument(ArgumentType type)
        {
            Type = type;
        }
    }
}