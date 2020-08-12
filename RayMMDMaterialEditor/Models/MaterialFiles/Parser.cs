using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RayMMDMaterialEditor.Models.MaterialFiles {
    static class Parser {
        public static List<Statement> Parse(string content) {
            var statements = new List<Statement>();
            var reader = new StringReader(content);

            while (true) {
                var line = reader.ReadLine();
                if (line == null) { break; }

                var (statement, rest) = ParseLine(line);
                if (statement == null || rest != "") {
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
                //return ParseFloatN(s);
                return (null, s);
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
            s.Substring(1);

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
            int digitEnd = -1;
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

            if (digitEnd != -1) {
                char c = s[digitEnd];
                if (c == 'u' || c == 'U' || c == 'l' || c == 'L') {
                    digitEnd++;
                }
            }

            return (sign * n, s.Substring(digitEnd));
        }
    }
}
