using Compilation.Parser.AST;

namespace Compilation.Parser.IOstructs {
    struct Token {
        public string type, 
            content;
        public uint line, 
            column;
    }

    struct Input {
        public string session_id;
        public Token[] tokens;
    }

    class TreeRoot {
        public TreeRoot(TreeNode[] main, Procedure[] functions) {
            this.main = main;
            this.functions = functions;
        }

        public object[] main;
        public Procedure[] functions;
    }

    struct Output {
        public static Output MakeFail(string errorDesc, uint errorLine, uint errorColumn) {
            var outp = new Output();
            outp.success = false;
            outp.errorDesc = errorDesc;
            outp.errorLine = errorLine;
            outp.errorColumn = errorColumn;
            outp.syntaxTree = null;
            outp.variables = null;
            return outp;
        }

        public static Output MakeSucess(TreeRoot syntaxTree, VarDef_Constable[] variables) {
            var outp = new Output();
            outp.success = true;
            outp.errorDesc = null;
            outp.errorLine = 0;
            outp.errorColumn = 0;
            outp.syntaxTree = syntaxTree;
            outp.variables = variables;
            return outp;
        }

        public bool success;
        public string errorDesc;
        public uint errorLine, 
            errorColumn;

        public VarDef_Constable[] variables;
        public TreeRoot syntaxTree;
    }
}
