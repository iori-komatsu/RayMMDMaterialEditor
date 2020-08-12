using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RayMMDMaterialEditor.Models.MaterialFiles {
    public class File {
        public string FileName { get; }
        public List<Statement> Statements { get; }

        private File(string fileName, List<Statement> statements) {
            FileName = fileName;
            Statements = statements;
        }

        public static File Load(string fileName) {
            using (var reader = new StreamReader(fileName)) {
                var content = reader.ReadToEnd();
                var statements = Parser.Parse(content);
                return new File(fileName, statements);
            }
        }

    }
}
