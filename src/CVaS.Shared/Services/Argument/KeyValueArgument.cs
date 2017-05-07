using System.Text;

namespace CVaS.Shared.Services.Argument
{
    public class KeyValueArgument : Argument
    {
        public Argument Content { get; set; }

        public string Key { get; set; }
        public KeyValueArgument(string key, Argument content) : base(ArgumentType.KeyValue)
        {
            Content = content;
            Key = key;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append('-', Key.Length == 1 ? 1 : 2);
            builder.Append(Key);
            builder.Append(Key.Length == 1 ? ' ' : '=');
            builder.Append(Content.ToString());

            return builder.ToString();
        }
    }
}