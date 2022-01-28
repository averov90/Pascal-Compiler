using Compilation.Parser.IOstructs;
using System.Text.Json;
using System.Linq;

namespace Compilation.Parser {
    class Program {
        static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new JsonSerializerOptions(JsonSerializerDefaults.Web) { IncludeFields = true };
        static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS_PRETTY = new JsonSerializerOptions(JsonSerializerDefaults.Web) { IncludeFields = true, WriteIndented = true };


        // Verification fingerprint: 9009a36b73f98d59b90b14b244c703195f5b2bc42ee6341cebb88820a24d397a

        static void Main(string[] args) {
            string input_text = System.IO.File.ReadAllText("code.lex.in");

            Input session = JsonSerializer.Deserialize<Input>(input_text, JSON_SERIALIZER_OPTIONS);
           
            Processor processor = new(session);
            Output output = processor.Run();

            string output_text = JsonSerializer.Serialize<Output>(output, (args.Any(a => a.ToLower().TrimStart(new char[] { '-', '/' }) == "pretty") ? JSON_SERIALIZER_OPTIONS_PRETTY : JSON_SERIALIZER_OPTIONS));

            System.IO.File.WriteAllText("code.parsed.out", output_text);
        }
    }
}
