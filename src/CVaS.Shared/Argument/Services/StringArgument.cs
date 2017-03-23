namespace CVaS.Shared.Services.Argument
{
    public class StringArgument : Argument
    {
        public string Content { get; set; }

        public StringArgument(string content) : base(ArgumentType.String)
        {
            Content = content;
        }

        public override string ToString()
        {
            return "\"" + Content + "\"";
        }
    }
}