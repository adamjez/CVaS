using CVaS.Web.Providers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CVaS.UnitTests
{
    public class PrimitiveArgumentParserTests
    {
        private readonly PrimitiveArgumentParser _parser;

        public PrimitiveArgumentParserTests()
        {
            _parser = new PrimitiveArgumentParser();
        }

        [Fact]
        public void PrimitiveParser_StringCanParse_TrueExpected()
        {
            var value = "test";

            Assert.True(_parser.CanParse(value));
        }

        [Fact]
        public void PrimitiveParser_IntegerCanParse_TrueExpected()
        {
            var value = 25;

            Assert.True(_parser.CanParse(value));
        }

        [Fact]
        public void PrimitiveParser_BoolCanParse_TrueExpected()
        {
            Assert.True(_parser.CanParse(true));
        }

        [Fact]
        public void PrimitiveParser_NullCanParse_TrueExpected()
        {
            Assert.True(_parser.CanParse(null));
        }

        [Fact]
        public void PrimitiveParser_IntegerParse_SuccessExpected()
        {
            var value = 20;

            Assert.Equal(20, _parser.Parse(value).First());
        }

        [Fact]
        public void PrimitiveParser_StringParse_SuccessExpected()
        {
            var value = "test";

            Assert.Equal(value, _parser.Parse(value).First());
        }

        [Fact]
        public void PrimitiveParser_BoolParse_SuccessExpected()
        {
            Assert.Equal(true, _parser.Parse(true).First());
        }

        [Fact]
        public void PrimitiveParser_NullParse_SuccessExpected()
        {
            Assert.Equal(0, _parser.Parse(null).Count());
        }
    }
}
