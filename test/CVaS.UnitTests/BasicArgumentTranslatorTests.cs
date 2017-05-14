using CVaS.BL.Services.ArgumentTranslator;
using CVaS.Shared.Exceptions;
using CVaS.Shared.Services.Argument;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;

namespace CVaS.UnitTests
{
    public class BasicArgumentTranslatorTests
    {
        private readonly BasicArgumentTranslator _translator;

        public BasicArgumentTranslatorTests()
        {
            _translator = new BasicArgumentTranslator();
        }

        class ArgumentCompare : IEqualityComparer<Argument>
        {
            public bool Equals(Argument x, Argument y)
            {
                if (x.GetType() == y.GetType())
                {
                    return x.Type == y.Type;
                }

                return false;
            }
            public int GetHashCode(Argument arg)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void ArgumentTranslator_SimpleIntParse_GenericIntExpected()
        {
            var num = 10;

            var expected = new GenericArgument<int>(num);

            var result = _translator.Process(new object[] { num });

            Assert.Equal(expected.Content, ((GenericArgument<int>)result.First()).Content);
        }

        [Fact]
        public void ArgumentTranslator_SimpleFloatParse_GenericFloatExpected()
        {
            var num = 10.0f;

            var expected = new GenericArgument<float>(num);

            var result = _translator.Process(new object[] { num });

            Assert.Equal(expected.Content, ((GenericArgument<float>)result.First()).Content);
        }

        [Fact]
        public void ArgumentTranslator_SimpleDoubleParse_GenericDoubleExpected()
        {
            var num = 10.0;

            var expected = new GenericArgument<double>(num);

            var result = _translator.Process(new object[] { num });

            Assert.Equal(expected.Content, ((GenericArgument<double>)result.First()).Content);
        }

        [Fact]
        public void ArgumentTranslator_StringParse_GenericStringExpected()
        {
            var text = "text";

            var expected = new StringArgument(text);

            var result = _translator.Process(new object[] { text });

            Assert.Equal(expected.Content, ((StringArgument)result.First()).Content);
        }

        [Fact]
        public void ArgumentTranslator_ComplexArrayParse_ComplexGenericExpected()
        {
            var text = "text";
            var num1 = 10.0;
            var num2 = 10;

            var expected = new List<Argument>()
            {
                new StringArgument(text),
                new GenericArgument<double>(num1),
                new GenericArgument<int>(num2)
            };

            var result = _translator.Process(new object[] { text, num1, num2 });
            Assert.Equal(expected, result, new ArgumentCompare());
        }

        [Fact]
        public void ArgumentTranslator_DictionaryParse_KeyValueArgumentExpected()
        {
            var id = Guid.NewGuid();
            var filePath = "local://" + id;

            var text = "file";

            var input = new Dictionary<string, object>()
            {
                { text, filePath }
            };

            var expected = new List<Argument>()
            {
                new KeyValueArgument(text, new FileArgument(id))                
            };

            var result = _translator.Process(new object[] { input });
            Assert.Equal(expected, result, new ArgumentCompare());
        }

        [Fact]
        public void ArgumentTranslator_UnknownTypeParse_ExceptionExpected()
        {
            Assert.Throws<ArgumentException>(() => _translator.Process(new object[] { new object() }));
        }

        [Fact]
        public void ArgumentTranslator_LocalFileParse_FileArgumentExpected()
        {
            var id = Guid.NewGuid();
            var filePath = "local://" + id;

            var expected = new FileArgument(id);

            var result = _translator.Process(new object[] { filePath });
            Assert.Equal(expected.FileId, ((FileArgument)result.First()).FileId);
        }

        [Fact]
        public void ArgumentTranslator_MalformedLocalFileParse_ExceptionExpected()
        {
            var filePath = "local://fdsgfdgh456hfd";

            Assert.Throws<ArgumentMalformedException>(() => _translator.Process(new object[] { filePath }));
        }

        [Fact]
        public void ArgumentTranslator_IntArgumentToString_InvariantStringExpected()
        {
            var num = 10.256;
            var result = new GenericArgument<double>(num);

            var cultureInfo = new CultureInfo("cs-CZ");

            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            Assert.Equal("10.256", result.ToString());
        }

        [Fact]
        public void ArgumentTranslator_StringArgumentToString_QuotedStringExpected()
        {
            var text = "Lorem Ipsum Lorem";
            var result = new StringArgument(text);

            Assert.Equal($"\"{text}\"", result.ToString());
        }

        [Fact]
        public void ArgumentTranslator_ObjectWithComplexKeyParse_NormalizedKeyValueStringExpected()
        {
            var text = "key test ";
            var num = 5;

            var argument = new KeyValueArgument(text, new GenericArgument<int>(num));

            Assert.Equal("--key-test=5", argument.ToString());
        }
    }
}
