using CVaS.Web.Providers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CVaS.UnitTests
{
    public class JsonArgumentParserTests
    {
        private readonly JsonArgumentParser _parser;

        public JsonArgumentParserTests()
        {
            _parser = new JsonArgumentParser();
        }
            

        [Fact]
        public void JsonParser_JValueCanParse_TrueExpected()
        {
            var jvalue = new JValue(10);

            Assert.True(_parser.CanParse(jvalue));
        }

        [Fact]
        public void JsonParser_JArrayCanParse_TrueExpected()
        {
            var jarray = new JArray(new[] { 1, 2, 3 });

            Assert.True(_parser.CanParse(jarray));
        }

        [Fact]
        public void JsonParser_JObjectCanParse_TrueExpected()
        {
            dynamic jsonObject = new JObject();
            jsonObject.Value = 10;

            Assert.True(_parser.CanParse(jsonObject));
        }

        [Fact]
        public void JsonParser_ObjectCanParse_FalseExpected()
        {
            var @object = new Object();

            Assert.False(_parser.CanParse(@object));
        }

        [Fact]
        public void JsonParser_IntegerParse_SuccessExpected()
        {
            var value = new JValue(20);

            Assert.Equal(20, _parser.Parse(value).First());
        }

        [Fact]
        public void JsonParser_FloatParse_SuccessExpected()
        {
            var value = new JValue(1.2f);

            Assert.Equal(1.2f, _parser.Parse(value).First());
        }

        [Fact]
        public void JsonParser_DoubleParse_SuccessExpected()
        {
            var value = new JValue(1.2);

            Assert.Equal(1.2f, _parser.Parse(value).First());
        }

        [Fact]
        public void JsonParser_BoolParse_SuccessExpected()
        {
            var value = new JValue(true);

            Assert.Equal(true, _parser.Parse(value).First());
        }

        [Fact]
        public void JsonParser_StringParse_SuccessExpected()
        {
            var value = new JValue("Text");

            Assert.Equal("Text", _parser.Parse(value).First());
        }

        [Fact]
        public void JsonParser_ArrayParse_SuccessExpected()
        {
            var array = new[] { 1, 2, 3 };
            var jArray = new JArray(array);

            Assert.Equal(array, _parser.Parse(jArray).Cast<int>().ToArray());
        }

        [Fact]
        public void JsonParser_ObjectParse_SuccessExpected()
        {
            var dict = new Dictionary<string, object>()
            {
                { "Value", 10 },
                { "Text", "Text" }
            };

            var jObject = new JObject
            {
                ["Value"] = 10,
                ["Text"] = "Text"
            };

            var result = (Dictionary<string, object>)_parser.Parse(jObject).First();
            Assert.Equal(dict, result);
        }

        [Fact]
        public void JsonParser_ComplexObjectParse_SuccessExpected()
        {
            var dict = new Dictionary<string, object>()
            {
                { "Value", 10 },
                { "Text", "Text" }
            };

            var array = new[] { 1, 2, 3 };
            var jArray = new JArray(array);

            dynamic jObject = new JObject();
            jObject.Value = 10;
            jObject.Text = "Text";
            jArray.Add(jObject);

            var expected = array.Select(x => (object)x).ToList();
            expected.Add(dict);

            List<object> result = _parser.Parse(jArray);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void JsonParser_TooComplexObjectParse_ExceptionExpected()
        {
            dynamic jObject = new JObject();
            jObject.Value = 10;
            jObject.Array = new JArray(new[] { 1, 2, 3 });

            Assert.Throws<ArgumentException>(() => _parser.Parse(jObject));
        }
    }
}
