using CVaS.Shared.Services.Argument;
using Xunit;

namespace CVaS.UnitTests
{
    public class ArgumentTests
    {
        [Fact]
        public void KeyValueArgument_LongKeyAndNumber_KeyEqualValueExpected()
        {
            var num = 10;
            var key = "key";

            var argument = new KeyValueArgument(key, new GenericArgument<int>(num));

            Assert.Equal("--key=10", argument.ToString());
        }

        [Fact]
        public void KeyValueArgument_ShortKeyAndText_KeySpaceValueExpected()
        {
            var text = "dog and cat";
            var key = "k";

            var argument = new KeyValueArgument(key, new StringArgument(text));

            Assert.Equal("-k \"dog and cat\"", argument.ToString());
        }
    }
}
