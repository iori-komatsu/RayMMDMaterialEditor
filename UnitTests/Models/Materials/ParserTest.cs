using RayMMDMaterialEditor.Models.Materials;
using Xunit;

namespace UnitTests.Models.Materials {
    public class ParserTest {
        [Theory]
        [InlineData("#define ALBEDO_MAP_FROM 1", "ALBEDO_MAP_FROM", 1L)]
        [InlineData("#define AZaz09_ 1234567890", "AZaz09_", 1234567890L)]
        [InlineData("#define X 0", "X", 0L)]
        [InlineData("#define HEX 0xff", "HEX", 255)]
        [InlineData("#define OCT 010", "OCT", 8)]
        [InlineData("#define WITH_SUFFIX 123L", "WITH_SUFFIX", 123L)]
        [InlineData("#define NEGATIVE -1", "NEGATIVE", -1L)]
        [InlineData("  #  define   WHITE_SPACES    100   ", "WHITE_SPACES", 100)]
        public void ParseIntegerDefineStatement(string source, string expectedName, long expectedValue) {
            var statements = Parser.Parse(source);
            Assert.Single(statements);

            var statement = statements[0];
            Assert.IsType<IntegerDefineStatement>(statement);

            var defineStatement = (IntegerDefineStatement)statement;
            Assert.Equal(expectedName, defineStatement.Name);
            Assert.Equal(expectedValue, defineStatement.Value);
        }

        [Theory]
        [InlineData("#define ALBEDO_MAP_FILE \"Maps/Body2b_BaseColor.png\"", "ALBEDO_MAP_FILE", "Maps/Body2b_BaseColor.png")]
        [InlineData("#define EMPTY_STRING \"\"", "EMPTY_STRING", "")]
        [InlineData("#define STRING_WITH_ESCAPED_CHARS \"\\\\\\\"\\n\"", "STRING_WITH_ESCAPED_CHARS", "\\\"\n")]
        public void ParseStringDefineStatement(string source, string expectedName, string expectedValue) {
            var statements = Parser.Parse(source);
            Assert.Single(statements);

            var statement = statements[0];
            Assert.IsType<StringDefineStatement>(statement);

            var defineStatement = (StringDefineStatement)statement;
            Assert.Equal(expectedName, defineStatement.Name);
            Assert.Equal(expectedValue, defineStatement.Value);
        }

        [Theory]
        [InlineData("const float x = 1.5;", "x", 1.5f)]
        [InlineData("const float x = 0;", "x", 0.0f)]
        [InlineData("const float x = 123.456;", "x", 123.456f)]
        [InlineData("const float x = .5;", "x", 0.5f)]
        [InlineData("const float x = 2e2;", "x", 200f)]
        [InlineData("const float x = 2e-2;", "x", 0.02f)]
        [InlineData("const float x = 2E-2;", "x", 0.02f)]
        [InlineData("const float x = 0.5f;", "x", 0.5f)]
        [InlineData("const float x = 0.5F;", "x", 0.5f)]
        [InlineData("const float x = -3.14;", "x", -3.14f)]
        public void ParseFloatStatement(string source, string expectedName, float expectedValue) {
            var statements = Parser.Parse(source);
            Assert.Single(statements);

            var statement = statements[0];
            Assert.IsType<FloatNStatement>(statement);

            var floatNStatement = (FloatNStatement)statement;
            Assert.Equal(expectedName, floatNStatement.Name);
            Assert.Single(floatNStatement.Values);
            Assert.Equal(expectedValue, floatNStatement.Values[0]);
        }

        [Theory]
        [InlineData("const float x = .;")]
        [InlineData("const float x = +;")]
        [InlineData("const float x = e5;")]
        [InlineData("const float x = 1e;")]
        public void ParseFloatStatement_ParseError(string source) {
            var statements = Parser.Parse(source);
            Assert.Single(statements);

            var statement = statements[0];
            Assert.IsType<OpaqueText>(statement);
        }
    }
}
