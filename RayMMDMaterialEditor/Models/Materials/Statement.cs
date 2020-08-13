using System;
using System.Linq;
using System.Text;

namespace RayMMDMaterialEditor.Models.Materials {
    public abstract class Statement {
        public abstract string Render();
    }

    public class OpaqueText : Statement {
        public string Content { get; }

        public OpaqueText(string content) {
            Content = content;
        }

        public override string Render() {
            return Content;
        }
    }

    public class StringDefineStatement : Statement {
        public string Name { get; }
        public string Value { get; }

        public StringDefineStatement(string name, string value) {
            Name = name;
            Value = value;
        }

        public override string Render() {
            return $"#define {Name} \"{Escape(Value)}\"";
        }

        private static string Escape(string s) {
            // TODO: support '\###' syntax and '\x#' syntax

            var builder = new StringBuilder();
            foreach (char c in s) {
                if (c == '\a') builder.Append("\\a");
                else if (c == '\b') builder.Append("\\b");
                else if (c == '\f') builder.Append("\\f");
                else if (c == '\n') builder.Append("\\n");
                else if (c == '\r') builder.Append("\\r");
                else if (c == '\t') builder.Append("\\t");
                else if (c == '\v') builder.Append("\\v");
                else if (c == '"') builder.Append("\\\"");
                else if (c == '\\') builder.Append("\\\\");
                else builder.Append(c);
            }
            return builder.ToString();
        }
    }

    public class IntegerDefineStatement : Statement {
        public string Name { get; }
        public long Value { get; }

        public IntegerDefineStatement(string name, long value) {
            Name = name;
            Value = value;
        }

        public override string Render() {
            return $"#define {Name} {Value}";
        }
    }

    public class FloatNStatement : Statement {
        public string Name { get; }
        public float[] Values { get; }

        public FloatNStatement(string name, float[] values) {
            if (values.Length < 1 || values.Length > 4) {
                throw new ArgumentException("values.Length must be between 1 and 4");
            }

            Name = name;
            Values = (float[])values.Clone();
        }

        public override string Render() {
            var builder = new StringBuilder();
            builder.Append("const ");
            builder.Append(TypeName(Values));
            builder.Append(" = ");
            if (Values.All(x => x == Values[0])) {
                builder.Append(RenderFloat(Values[0]));
            } else {
                builder.Append(TypeName(Values));
                builder.Append("(");
                builder.Append(string.Join(", ", Values.Select(RenderFloat)));
                builder.Append(");");
            }
            builder.Append(";");
            return builder.ToString();
        }

        private static string TypeName(float[] values) {
            if (values.Length == 1) {
                return "float";
            } else {
                return $"float{values.Length}";
            }
        }

        private static string RenderFloat(float x) {
            return string.Format("{0:0.0#####}", x);
        }
    }

    public class IncludeStatement : Statement {
        public string Argument { get; }

        public IncludeStatement(string argument) {
            Argument = argument;
        }

        public override string Render() {
            return $"#include {Argument}";
        }
    }
}
