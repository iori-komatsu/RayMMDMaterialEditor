using RayMMDMaterialEditor.Models.Materials;
using Xunit;

namespace UnitTests.Models.Materials {
    public class StatementTest {
        [Theory]
        [InlineData("")]
        [InlineData("// comment")]
        public void RenderOpaqueText(string content) {
            var sut = new OpaqueText(content);
            Assert.Equal(content, sut.Render());
        }

        [Theory]
        [InlineData(
            "ALBEDO_MAP_FILE", "Maps/Body2b_BaseColor.png",
            "#define ALBEDO_MAP_FILE \"Maps/Body2b_BaseColor.png\"")]
        [InlineData(
            "EMPTY_STRING", "",
            "#define EMPTY_STRING \"\"")]
        [InlineData(
            "STRING_WITH_ESCAPED_CHARS", "\\\"\n",
            "#define STRING_WITH_ESCAPED_CHARS \"\\\\\\\"\\n\"")]
        public void RenderStringDefineStatement(string name, string value, string expected) {
            var sut = new StringDefineStatement(name, value);
            Assert.Equal(expected, sut.Render());
        }

        [Theory]
        [InlineData("ALBEDO_MAP_FROM", 1L, "#define ALBEDO_MAP_FROM 1")]
        [InlineData("X", 12345L, "#define X 12345")]
        [InlineData("NEGATIVE", -1L, "#define NEGATIVE -1")]
        public void RenderIntegerDefineStatement(string name, long value, string expected) {
            var sut = new IntegerDefineStatement(name, value);
            Assert.Equal(expected, sut.Render());
        }
    }
}
