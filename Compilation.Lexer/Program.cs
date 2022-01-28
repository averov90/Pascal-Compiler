using System.Text.Json;
using System.Linq;

namespace Compilation.Lexer {
    class Program {
        static readonly JsonSerializerOptions JSON_SERIZLIZER_OPTIONS = new JsonSerializerOptions(JsonSerializerDefaults.Web) { IncludeFields = true };
        static readonly JsonSerializerOptions JSON_SERIZLIZER_OPTIONS_PRETTY = new JsonSerializerOptions(JsonSerializerDefaults.Web) { IncludeFields = true, WriteIndented = true };


        static void Main(string[] args) {
            string[] input_text = System.IO.File.ReadAllLines("code.pas");

            CodeProc processor = new(input_text);
            Output output = processor.Run();

            string output_text = JsonSerializer.Serialize<Output>(output, (args.Any(a => a.ToLower().TrimStart(new char[] { '-', '/' }) == "pretty") ? JSON_SERIZLIZER_OPTIONS_PRETTY : JSON_SERIZLIZER_OPTIONS));

            System.IO.File.WriteAllText("code.lex.out", output_text);
        }

        struct Input {
            public string session_id;
            public string[] source;
        }

        public struct Token {
            public Token(string type, string content, int line, int column) {
                this.type = type;
                this.content = content;
                this.line = line;
                this.column = column;
            }

            public string type,
                content;
            public int line,
                column;
        }

        public struct Output {
            public static Output MakeFail(string errorDesc, int errorLine, int errorColumn) {
                var outp = new Output();
                outp.success = false;
                outp.tokens = null;
                outp.errorDesc = errorDesc;
                outp.errorLine = errorLine;
                outp.errorColumn = errorColumn;
                return outp;
            }

            public static Output MakeSucess(Token[] tokens) {
                var outp = new Output();
                outp.success = true;
                outp.tokens = tokens;
                outp.errorDesc = null;
                outp.errorLine = 0;
                outp.errorColumn = 0;
                return outp;
            }

            public bool success;
            public string errorDesc;
            public int errorLine,
                errorColumn;

            public Token[] tokens;
        }
    }
}
