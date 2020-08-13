using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace RayMMDMaterialEditor.Models.Materials {
    public static class Parser {
        public static List<Statement> Parse(string content) {
            var statements = new List<Statement>();
            var reader = new StringReader(content);

            while (true) {
                var line = reader.ReadLine();
                if (line == null) { break; }

                var (statement, rest) = ParseLine(line);
                if (statement == null || rest.Trim() != "") {
                    statement = new OpaqueText(line);
                }

                statements.Add(statement);
            }

            return statements;
        }

        private static (Statement, string) ParseLine(string s) {
            s = s.TrimStart();
            if (s.StartsWith("#")) {
                return ParsePreprocessor(s.Substring(1));
            } else {
                return ParseFloatN(s);
            }
        }

        private static (Statement, string) ParsePreprocessor(string s) {
            var (identifier, rest1) = ParseIdentifier(s);
            if (identifier == "define") {
                var (name, rest2) = ParseIdentifier(rest1);
                rest2 = rest2.TrimStart();
                if (rest2.StartsWith("\"")) {
                    var (str, rest3) = ParseStringLiteral(rest2);
                    return (new StringDefineStatement(name, str), rest3);
                } else {
                    var (n, rest3) = ParseIntegerLiteral(rest2);
                    return (new IntegerDefineStatement(name, n), rest3);
                }
            } else if (identifier == "include") {
                return (new IncludeStatement(rest1.TrimStart()), "");
            } else {
                return (null, s);
            }
        }

        private static (string, string) ParseIdentifier(string s) {
            // FIXME: 数値で始まるものも識別子として認識してしまう

            s = s.TrimStart();
            for (int i = 0; i < s.Length; ++i) {
                int cc = s[i];
                bool isLowerAlpha = 'a' <= cc && cc <= 'z';
                bool isUpperAlpha = 'A' <= cc && cc <= 'Z';
                bool isDigit = '0' <= cc && cc <= '9';
                bool isUnderscore = cc == '_';
                if (!isLowerAlpha && !isUpperAlpha && !isDigit && !isUnderscore) {
                    return (s.Substring(0, i), s.Substring(i));
                }
            }
            return (s, "");
        }

        private static (string, string) ParseStringLiteral(string s) {
            s = s.TrimStart();

            if (!s.StartsWith("\"")) { return (null, s); }
            s = s.Substring(1);

            var builder = new StringBuilder();
            int literalEnd = -1;
            bool escaped = false;
            for (int i = 0; i < s.Length; ++i) {
                char c = s[i];
                if (escaped) {
                    // TODO: support '\###' syntax and '\x#' syntax

                    if (c == 'a') builder.Append('\a');
                    else if (c == 'b') builder.Append('\b');
                    else if (c == 'f') builder.Append('\f');
                    else if (c == 'n') builder.Append('\n');
                    else if (c == 'r') builder.Append('\r');
                    else if (c == 't') builder.Append('\t');
                    else if (c == 'v') builder.Append('\v');
                    else builder.Append(c);

                    escaped = false;
                } else if (c == '\\') {
                    escaped = true;
                } else if (c == '"') {
                    literalEnd = i;
                    break;
                } else {
                    builder.Append(c);
                }
            }

            if (literalEnd == -1) {
                return (null, s);
            }

            return (builder.ToString(), s.Substring(literalEnd + 1));
        }

        private static (long, string) ParseIntegerLiteral(string s) {
            s = s.TrimStart();

            int sign = 1;
            if (s.StartsWith("-")) {
                sign = -1;
                s = s.Substring(1);
            }

            s = s.TrimStart();

            int radix = 10;
            if (s.StartsWith("0x")) {
                radix = 16;
                s = s.Substring(2);
            } else if (s.StartsWith("0")) {
                radix = 8;
                s = s.Substring(1);
            }

            long n = 0;
            int digitEnd = s.Length;
            for (int i = 0; i < s.Length; ++i) {
                int cc = s[i];
                if (radix == 10) {
                    if ('0' <= cc && cc <= '9') {
                        n = 10 * n + cc - '0';
                    } else {
                        digitEnd = i;
                        break;
                    }
                } else if (radix == 8) {
                    if ('0' <= cc && cc <= '7') {
                        n = 8 * n + cc - '0';
                    } else {
                        digitEnd = i;
                        break;
                    }
                } else {
                    if ('0' <= cc && cc <= '9') {
                        n = 16 * n + cc - '0';
                    } else if ('a' <= cc && cc <= 'f') {
                        n = 16 * n + cc - 'a' + 10;
                    } else if ('A' <= cc && cc <= 'F') {
                        n = 16 * n + cc - 'A' + 10;
                    } else {
                        digitEnd = i;
                        break;
                    }
                }
            }

            if (digitEnd != s.Length) {
                char c = s[digitEnd];
                if (c == 'u' || c == 'U' || c == 'l' || c == 'L') {
                    digitEnd++;
                }
            }

            return (sign * n, s.Substring(digitEnd));
        }

        private static (Statement, string) ParseFloatN(string s) {
            var (keyword1, rest1) = ParseIdentifier(s);
            if (keyword1 != "const") return (null, s);

            var (keyword2, rest2) = ParseIdentifier(rest1);
            int dim;
            if (keyword2 == "float") dim = 1;
            else if (keyword2 == "float2") dim = 2;
            else if (keyword2 == "float3") dim = 3;
            else if (keyword2 == "float4") dim = 4;
            else return (null, s);

            var (identifier, rest3) = ParseIdentifier(rest2);

            rest3 = rest3.TrimStart();
            if (!rest3.StartsWith("=")) return (null, s);
            rest3 = rest3.Substring(1);

            var (f, rest4) = ParseFloat(rest3);
            if (float.IsNaN(f)) return (null, s);

            var floatN = new float[dim];
            for (int i = 0; i < dim; ++i) {
                floatN[i] = f;
            }

            rest4 = rest4.TrimStart();
            if (!rest4.StartsWith(";")) return (null, s);
            rest4 = rest4.Substring(1);

            return (new FloatNStatement(identifier, floatN), rest4);
        }

        private static (float, string) ParseFloat(string s) {
            s = s.TrimStart();
            var t = s + '\0';

            // sign
            int i = 0;
            if (t[i] == '+' || t[i] == '-') {
                ++i;
            }

            // digit sequence before period
            bool digitExists = false;
            for (; i < t.Length; ++i) {
                if (t[i] < '0' || t[i] > '9') break;
                digitExists = true;
            }

            if (t[i] == '.') {
                ++i;
                // digit sequence after period
                for (; i < t.Length; ++i) {
                    if (t[i] < '0' || t[i] > '9') break;
                    digitExists = true;
                }
            }

            // この時点で少なくとも１つは数字が出現していなければならない
            if (!digitExists) {
                return (float.NaN, s);
            }

            if (t[i] == 'e' || t[i] == 'E') {
                ++i;
                // sign
                if (t[i] == '+' || t[i] == '-') {
                    ++i;
                }
                // digit sequence after e
                digitExists = false;
                for (; i < t.Length; ++i) {
                    if (t[i] < '0' || t[i] > '9') break;
                    digitExists = true;
                }
                if (!digitExists) {
                    return (float.NaN, s);
                }
            }

            float f = float.Parse(t.Substring(0, i), CultureInfo.InvariantCulture);

            // floating stuff
            if (t[i] == 'f' || t[i] == 'F') {
                ++i;
            }

            return (f, s.Substring(i));
        }
    }
}
