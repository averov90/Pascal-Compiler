using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Compilation.Lexer {
    class CodeProc {
        string[] code;

        public CodeProc(string[] code) {
            this.code = code;
        }

        List<Program.Token> tokens = new();

        struct TapePos {
            public TapePos(byte method, string pattern, byte prohibitFollowLetters, string token, bool toLowerCase) {
                this.method = method;
                this.pattern = pattern;
                this.prohibitFollowLetters = prohibitFollowLetters;
                this.token = token;
                this.toLowerCase = toLowerCase;
            }
            public byte method; //0 - StartsWith, 1 - regex
            public string pattern;
            public byte prohibitFollowLetters; //0 - no, 1 - just letters and underscore, 2 - letters, underscore and digits
            public string token;
            public bool toLowerCase;
        }

        TapePos[] tape = new TapePos[] {
            new(0, "program", 2, "program", true),
            new(0, "procedure", 2, "procedure", true),
            new(0, "begin", 2, "begin", true),
            new(0, "end", 2, "end", true),
            new(0, ":=", 0, "assign", false),

            new(0, "var", 2, "var", true),
            new(0, "integer", 2, "integer-word", true),
            new(0, "const", 2, "const", true),

            new(0, "exit", 2, "exit", true),
            
            new(0, "(", 0, "left-parenthesis", false),
            new(0, ")", 0,"right-parenthesis", false),

            new(1, "^[+*\\-\\/]", 0, "arithmetic-operator", false),
            new(0, "div", 1, "arithmetic-operator", true),
            new(0, "mod", 1, "arithmetic-operator", true),

            new(0, "and", 2, "logic-operator", true),
            new(0, "or", 2, "logic-operator", true),

            new(0, "<>", 0, "comparsion-operator", false),
            new(0, "<=", 0, "comparsion-operator", false),
            new(0, ">=", 0, "comparsion-operator", false),
            new(0, "=", 0, "comparsion-operator", false),
            new(0, "<", 0, "comparsion-operator", false),
            new(0, ">", 0, "comparsion-operator", false),
           
            new(0, "for", 2, "for", true),
            new(0, "to", 2, "to", true),
            new(0, "downto", 2, "downto", true),
            new(0, "while", 2, "while", true),
            new(0, "do", 2, "do", true),

            new(0, ":", 0, "colon", false),
            new(0, ".", 0, "dot", false),
            new(0, ",", 0, "comma", false),
            new(0, ";", 0, "semicolon", false),

            new(0, "true", 2, "boolean", true),
            new(0, "false", 2, "boolean", true),

            new(1, "^[0-9]+", 0, "number", false),
            new(1, "^[a-zA-Z_][a-zA-Z0-9_]*", 0, "internal-name", false)
        };

        readonly char[] Letters = "abcdefghijklmnopqrstuvwxyz_".ToCharArray();
        readonly char[] LettersDigits = "abcdefghijklmnopqrstuvwxyz_0123456789".ToCharArray();

        bool IsNotProhibited(char lwchar, byte mode) {
            if (mode == 2) {
                if (LettersDigits.All(l => l != lwchar)) {
                    return true;
                }
            } else { //mode == 1
                if (Letters.All(l => l != lwchar)) {
                    return true;
                }
            }
            return false;
        }

        public Program.Output Run() {
            for (int linei = 0; linei != code.Length; ++linei) {
                string line = code[linei];
                StringBox[] lineparts = new StringBox(Regex.Replace(code[linei], @"[^\S ]+", "")).Split(' ');
                foreach (var part in lineparts) {
                    if (part.Length == 0) continue;

                    string token = null, strpart = part.ExtactString();
                    foreach (var pos in tape) {
                        if (pos.method == 0) {
                            if (pos.toLowerCase) {
                                var lw = strpart.ToLower();
                                if (pos.pattern == lw) {
                                    strpart = lw;
                                    token = pos.token;
                                    break;
                                }
                            } else {
                                if (pos.pattern == strpart) {
                                    token = pos.token;
                                    break;
                                }
                            }
                        } else {
                            if (pos.toLowerCase) {
                                var lw = strpart.ToLower();
                                if (Regex.IsMatch(lw, pos.pattern + '$')) {
                                    strpart = lw;
                                    token = pos.token;
                                    break;
                                }
                            } else {
                                if (Regex.IsMatch(strpart, pos.pattern + '$')) {
                                    token = pos.token;
                                    break;
                                }
                            }
                        }
                    }

                    if (token == null) {
                        do {
                            int initLen = strpart.Length;
                            foreach (var pos in tape) {
                                if (pos.method == 0) {
                                    if (pos.toLowerCase) {
                                        var lw = strpart.ToLower();
                                        if (lw.StartsWith(pos.pattern)) {
                                            if (pos.prohibitFollowLetters != 0) {
                                                if (pos.pattern.Length == lw.Length || IsNotProhibited(lw[pos.pattern.Length], pos.prohibitFollowLetters)) {
                                                    tokens.Add(new Program.Token(pos.token, pos.pattern, linei, part.StartNumber));
                                                    strpart = part.RemoveSub(0, pos.pattern.Length).ExtactString();
                                                    break;
                                                }
                                            } else {
                                                tokens.Add(new Program.Token(pos.token, pos.pattern, linei, part.StartNumber));
                                                strpart = part.RemoveSub(0, pos.pattern.Length).ExtactString();
                                                break;
                                            }
                                        }
                                    } else {
                                        if (strpart.StartsWith(pos.pattern)) {
                                            if (pos.prohibitFollowLetters != 0) {
                                                if (pos.pattern.Length == strpart.Length || IsNotProhibited(char.ToLower(strpart[pos.pattern.Length]), pos.prohibitFollowLetters)) {
                                                    tokens.Add(new Program.Token(pos.token, pos.pattern, linei, part.StartNumber));
                                                    strpart = part.RemoveSub(0, pos.pattern.Length).ExtactString();
                                                    break;
                                                }
                                            } else {
                                                tokens.Add(new Program.Token(pos.token, pos.pattern, linei, part.StartNumber));
                                                strpart = part.RemoveSub(0, pos.pattern.Length).ExtactString();
                                                break;
                                            }
                                        }
                                    }
                                } else {
                                    if (pos.toLowerCase) {
                                        var lw = strpart.ToLower();
                                        var match = Regex.Match(lw, pos.pattern);
                                        if (match.Success) {
                                            if (pos.prohibitFollowLetters != 0) {
                                                if (pos.pattern.Length == lw.Length || IsNotProhibited(lw[match.Value.Length], pos.prohibitFollowLetters)) {
                                                    tokens.Add(new Program.Token(pos.token, match.Value, linei, part.StartNumber));
                                                    strpart = part.RemoveSub(0, match.Value.Length).ExtactString();
                                                    break;
                                                }
                                            } else {
                                                tokens.Add(new Program.Token(pos.token, match.Value, linei, part.StartNumber));
                                                strpart = part.RemoveSub(0, match.Value.Length).ExtactString();
                                                break;
                                            }
                                        }
                                    } else {
                                        var match = Regex.Match(strpart, pos.pattern);
                                        if (match.Success) {
                                            if (pos.prohibitFollowLetters != 0) {
                                                if (pos.pattern.Length == strpart.Length || IsNotProhibited(char.ToLower(strpart[match.Value.Length]), pos.prohibitFollowLetters)) {
                                                    tokens.Add(new Program.Token(pos.token, match.Value, linei, part.StartNumber));
                                                    strpart = part.RemoveSub(0, match.Value.Length).ExtactString();
                                                    break;
                                                }
                                            } else {
                                                tokens.Add(new Program.Token(pos.token, match.Value, linei, part.StartNumber));
                                                strpart = part.RemoveSub(0, match.Value.Length).ExtactString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            if (strpart.Length == initLen) {
                                return Program.Output.MakeFail("синтаксическая конструкция не может быть распознана: " + strpart, linei, part.StartNumber);
                            }
                        } while (strpart.Length != 0);
                    } else {
                        tokens.Add(new Program.Token(token, strpart, linei, part.StartNumber));
                    }
                }
            }

            return Program.Output.MakeSucess(tokens.ToArray());
        }
    }
}
