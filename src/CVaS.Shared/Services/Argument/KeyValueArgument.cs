using System.Text;
using System.Text.RegularExpressions;

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

            var normalizedKey = Regex.Replace(Key.Trim(), @"\s+", "-");

            builder.Append('-', normalizedKey.Length == 1 ? 1 : 2);
            builder.Append(normalizedKey);
            builder.Append(normalizedKey.Length == 1 ? ' ' : '=');
            builder.Append(Content);

            return builder.ToString();
        }
    }
}