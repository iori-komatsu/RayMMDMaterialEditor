using RayMMDMaterialEditor.Models.Materials;
using Xunit;

namespace UnitTests.Models.Materials {
    public class ParserTest {
        [Theory]
        [InlineData("#define ALBEDO_MAP_FROM 1", "ALBEDO_MAP_FROM", 1L)]
        [InlineData("#define AZaz09_ 1234567890", "AZaz09_", 1234567890L)]
        [InlineData("#define  X   0", "X", 0L)]
        [InlineData("#define HEX 0xff", "HEX", 255)]
        [InlineData("#define OCT 010", "OCT", 8)]
        [InlineData("#define WITH_SUFFIX 123L", "WITH_SUFFIX", 123L)]
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
    }
}
