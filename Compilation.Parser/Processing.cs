using System;
using System.Collections.Generic;
using System.Linq;
using PCRE;
using Compilation.Parser.IOstructs;
using Compilation.Parser.AST;

namespace Compilation.Parser {
    partial class Processor {
        Input input;
        readonly char[] Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        readonly string[] whatJScanDo = { "запускать всё подряд", "гладиолус", "всё пропало", "удалить твой код", "выполнить весь твой код", "грибы росли", "жить долго и несчастливо", "дорозофилы улетели на юг", "код спел оперу" };

        Dictionary<string, Variable> variables = new Dictionary<string, Variable>();
        Dictionary<string, Procedure> procedures = new Dictionary<string, Procedure>();
        string programName = "";

        enum DataType {
            Integer,
            Boolean
        }
        struct Variable {
            public Variable(string name, DataType type, TreeNode value, bool @const) {
                this.name = name;
                this.type = type;
                initValue = value;
                this.@const = @const;
            }
            public string name;
            public DataType type;
            public TreeNode initValue;
            public bool @const;
        }

        public Processor(Input input) {
            this.input = input;
        }

        StringBox DecodeLabelToCode(StringBox str) {
            if (str.Length == 0) return new("");

            int[] nums = str.ExtactNumbers();
            StringBox strb = new(input.tokens[nums[0]].content, nums[0], false);

            for (int i = 1; i != nums.Length; ++i) {
                strb.Concatenate(new(input.tokens[nums[i]].content, nums[i], false));
            }
            return strb;
        }

        public Output Run() {
            string rawreg = string.Join("", input.tokens.Select(e => Alphabet[(ushort)TokenMapping.TokenCvt[e.type]]));
            rawreg = rawreg.Remove(rawreg.IndexOf("df") + 1);

            if (rawreg.Length == 0)
                return Output.MakeFail("программа не обнаружена!\nНормальная программа всегда содержит 'end.'", 0, 0);

            List<PcreMatch> sections = PcreRegex.Matches(rawreg, @"c(?:[^cd]|(?R))*d").ToList();

            MainSequence _main;
            List<ProcedureSequence> _procedures = new();

            {
                var temp = sections[sections.Count - 1];
                _main = new MainSequence(temp.Value.Substring(1, temp.Value.Length - 2), (uint)temp.Index + 1);
                rawreg = rawreg.Remove(temp.Index);
            }

            for (int sec = sections.Count - 2; sec >= 0; --sec) {
                var temp = sections[sec];
                if (rawreg[temp.EndIndex] != 'h') {
                    var inp = input.tokens[temp.EndIndex];
                    return Output.MakeFail("а точку с запятой за тебя кто поставит?\nЯ тебе не JavaScript, чтобы " + whatJScanDo[new Random((int)DateTime.Now.Ticks).Next(whatJScanDo.Length)] + ".", inp.line, inp.column);
                }
                if (temp.EndIndex != rawreg.Length - 1) {
                    var inp = input.tokens[temp.EndIndex + 1];
                    return Output.MakeFail("у тебя ужасный codestyle! Я не могу принять такое.\nНаучись ничего не писать между объявлениями процедур, иначе " + whatJScanDo[new Random((int)DateTime.Now.Ticks).Next(whatJScanDo.Length)] + "...", inp.line, inp.column);
                }

                ProcedureSequence proc = new(temp.Value.Substring(1, temp.Value.Length - 2), (uint)temp.Index + 1);
                rawreg = rawreg.Remove(temp.Index);

                if (sec == 0) {
                    if (rawreg.LastIndexOf('b') == -1) {
                        var inp = input.tokens[temp.EndIndex];
                        return Output.MakeFail("ты забыл поставить точку!\nЯ сначала не понял твоей задумки, но теперь мне всё ясно.\nПоставь точку там, где показываю, и эта проблема исчезнет.", inp.line, inp.column);
                    }
                } else {
                    if (rawreg.LastIndexOf('b') < sections[sec - 1].EndIndex - 1) {
                        var inp = input.tokens[temp.EndIndex];
                        return Output.MakeFail("ты забыл поставить точку!\nЯ сначала не понял твоей задумки, но теперь мне всё ясно.\nПоставь точку там, где показываю, и эта проблема исчезнет.", inp.line, inp.column);
                    }
                }

                proc.headIndex = (uint)rawreg.LastIndexOf('b') + 1;
                proc.head = rawreg.Substring((int)proc.headIndex, rawreg.Length - (int)proc.headIndex);
                rawreg = rawreg.Remove((int)proc.headIndex - 1);

                _procedures.Add(proc);
            }

            if (rawreg.Length != 0) {
                if (rawreg[rawreg.Length - 1] != 'h') {
                    var inp = input.tokens[rawreg.Length - 1];
                    return Output.MakeFail("а закрыть выражение?\nКто за тебя точку с запятой поставит?", inp.line, inp.column);
                }

                var outp = ProcessVarDefBlock_Global(rawreg.Remove(rawreg.Length - 1));
                if (outp.fail) return Output.MakeFail(outp.errorDesc, outp.errorLine, outp.errorColumn);
            }

            procedures.Add("writeln",
                            new Procedure("writeln",
                            new VarDesc_ConstablePointeble[] { new("data", "number", false, false) },
                            new VarDef_Constable[] { },
                            new TreeNode[] { }));


            { //Parsing procedures headers
                foreach (var procedure in _procedures) {
                    StringBox head = new(procedure.head, (int)procedure.headIndex);
                    if (head.Length > 3) {
                        if (head[0] != 'n') {
                            var inp = input.tokens[head.StartNumber];
                            return Output.MakeFail("вообще-то у процедуры должно быть имя...\nДай процедуре адекватное имя...", inp.line, inp.column);
                        }
                        if (head[1] != 'q') {
                            var inp = input.tokens[head.NumberAT(1)];
                            return Output.MakeFail("а скобку кто откроет?\nОткрой скобку!", inp.line, inp.column);
                        }

                        StringBox statements;
                        {
                            int parpos = procedure.head.IndexOf('r');
                            if (parpos == -1) {
                                var inp = input.tokens[head.NumberAT(1)];
                                return Output.MakeFail("когда ты из дома уходишь, дверь также не закрываешь?\nЗакрой скобку!", inp.line, inp.column);
                            } else {
                                if (procedure.head[parpos + 1] != 'h') {
                                    var inp = input.tokens[head.NumberAT(parpos + 1)];
                                    return Output.MakeFail("а точку с запятой за тебя кто поставит?\nЯ тебе не JavaScript, чтобы " + whatJScanDo[new Random((int)DateTime.Now.Ticks).Next(whatJScanDo.Length)] + ".", inp.line, inp.column);
                                }

                                statements = head.Substring(parpos + 2);
                                head = head.Remove(parpos + 2);
                            }
                        }

                        procedure._procedure_name = input.tokens[head.StartNumber].content;
                        if (procedures.ContainsKey(procedure._procedure_name)) {
                            return Output.MakeFail("функция с именем \""+ procedure._procedure_name + "\" уже была объявлена.\nДай этой функции другое имя.", input.tokens[head.StartNumber].line, input.tokens[head.StartNumber].column);
                        }
                        if (head[head.Length - 1] != 'h') {
                            var inp = input.tokens[head.NumberAT(head.Length - 1)];
                            return Output.MakeFail("а точку с запятой за тебя кто поставит?\nЯ тебе не JavaScript, чтобы " + whatJScanDo[new Random((int)DateTime.Now.Ticks).Next(whatJScanDo.Length)] + ".", inp.line, inp.column);
                        }

                        StringBox[] args = head.SubstringRange(2, head.Length - 3).Split('h');
                        List<VarDesc_ConstablePointeble> arguments = new();
                        foreach (var arg in args) {
                            StringBox[] groups = arg.Split('g');

                            bool pointers = false, @const = false;
                            for (ushort i = 0; i != groups.Length; ++i) {
                                StringBox item = groups[i];
                                string itemstr = item.ExtactString();
                                if (i == 0) {
                                    if (itemstr == "in") {
                                        pointers = true;
                                        arguments.Add(new(input.tokens[item.NumberAT(1)].content, "number", true, false));
                                    } else if (itemstr == "kn") {
                                        @const = true;
                                        arguments.Add(new(input.tokens[item.NumberAT(1)].content, "number", false, true));
                                    } else if (item.Length == 1 && item[0] == 'n') {
                                        arguments.Add(new(input.tokens[item.StartNumber].content, "number", false, false));
                                    } else if (groups.Length == 1) {
                                        if (itemstr == "inej") {
                                            arguments.Add(new(input.tokens[item.NumberAT(1)].content, "number", true, false));
                                        } else if (itemstr == "knej") {
                                            arguments.Add(new(input.tokens[item.NumberAT(1)].content, "number", false, true));
                                        } else if (itemstr == "nej") {
                                            arguments.Add(new(input.tokens[item.StartNumber].content, "number", false, false));
                                        } else {
                                            return Output.MakeFail("ну нельза просто так взять и вставить ключевое слово куда попало...\nВоостанови адекватность.", input.tokens[item.StartNumber].line, input.tokens[item.StartNumber].column);
                                        }
                                    } else {
                                        return Output.MakeFail("если ты хотел объявить аргумент-указатель, надо было писать так: var <name>", input.tokens[item.StartNumber].line, input.tokens[item.StartNumber].column);
                                    }
                                } else if (i + 1 == groups.Length) {
                                    if (itemstr == "nej") {
                                        arguments.Add(new(input.tokens[item.StartNumber].content, "number", pointers, @const));
                                    } else {
                                        return Output.MakeFail("а как, по твоему, узнаю тип аргументов?\nПропиши везде Integer, я работаю только с Integer.", input.tokens[item.StartNumber].line, input.tokens[item.StartNumber].column);
                                    }
                                } else {
                                    if (item.Length == 1 && item[0] == 'n') {
                                        arguments.Add(new(input.tokens[item.StartNumber].content, "number", pointers, @const));
                                    } else {
                                        return Output.MakeFail("мне почему-то кажется, что здесь хорошо бы смотрелся ещё один аргумент.\nДопиши ты этот агрумент или убери запятую, чтобы не мешала мне.", input.tokens[item.StartNumber].line, input.tokens[item.StartNumber].column);
                                    }
                                }
                            }
                        }

                        Dictionary<string, Variable> lvars = new();

                        if (statements.Length != 0) {
                            if (statements[statements.Length - 1] != 'h') {
                                var inp = input.tokens[statements.NumberAT(statements.Length - 1)];
                                return Output.MakeFail("а закрыть выражение?\nКто за тебя точку с запятой поставит?", inp.line, inp.column);
                            }

                            var outp = ProcessVarDefBlock_Local(statements, ref lvars);
                            if (outp.fail) return Output.MakeFail(outp.errorDesc, outp.errorLine, outp.errorColumn);
                        }
                        
                        procedures.Add(procedure._procedure_name, 
                            new Procedure(procedure._procedure_name,
                            arguments.ToArray(), 
                            lvars.Select(a => new VarDef_Constable(a.Value.name, "number", a.Value.@const ? a.Value.initValue : null)).ToArray(),
                            AssignNonConstVariables(ref lvars)));
                    } else {
                        var inp = input.tokens[head.StartNumber];
                        return Output.MakeFail("ну такое объявление фукнции совсем не годится...\nДолжны быть как минимум 5 элементов: procedure <name>();\nУ тебя явно чего-то не хватает.", inp.line, inp.column);
                    }
                }
            }

            foreach (var item in procedures) {
                if (item.Key != "writeln") {
                    var seq = _procedures.First(a => a._procedure_name == item.Key);
                    var outp = ParseSequentalElementCode(new StringBox(seq.body, (int)seq.bodyIndex), item.Key);
                    if (outp.fail) return Output.MakeFail(outp.errorDesc, outp.errorLine, outp.errorColumn);
                }
            }

            procedures.Remove("writeln");

            TreeRoot troot;
            {
                var outp = ParseSequentalMainCode(new StringBox(_main.body, (int)_main.index), out TreeNode[] sequence);
                if (outp.fail) return Output.MakeFail(outp.errorDesc, outp.errorLine, outp.errorColumn);

                troot = new TreeRoot(AssignNonConstVariables(ref variables).Concat(sequence).ToArray(), procedures.Select(a => a.Value).ToArray());
            }

            variables.Add("-PROGNAME-", new Variable("-PROGNAME-", DataType.Integer, new AST.Variable(programName), true));

            return Output.MakeSucess(troot, variables.Select(a => new VarDef_Constable(a.Value.name, "number", a.Value.@const ? a.Value.initValue : null)).ToArray());
        }

        struct MainSequence {
            public MainSequence(string body, uint index) {
                this.body = body;
                this.index = index;
            }
            public string body;
            public uint index;
        }

        class ProcedureSequence {
            public ProcedureSequence(string body, uint bodyIndex) {
                this.body = body;
                this.bodyIndex = bodyIndex;
            }
            public string body;
            public uint bodyIndex;

            public string head;
            public uint headIndex;

            public string _procedure_name;
        }

    }
}
