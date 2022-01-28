using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PCRE;
using Compilation.Parser.AST;

namespace Compilation.Parser {
    partial class Processor {

        SimpleOut ParseSequentalElementCode_Seq(StringBox seq, Dictionary<string, Variable> locals, out List<TreeNode> code, List<StringBox> nested) {
            code = new();
            if (seq.Length == 0) {
                return SimpleOut.MakeSucess();
            }
            if (seq[seq.Length - 1] != 'h') {
                code = null;
                var inp = input.tokens[seq.NumberAT(seq.Length - 1)];
                return SimpleOut.MakeFail("а точку с запятой феи должны поставить?\nПоставь точку с запятой.", inp.line, inp.column);
            }

            StringBox[] statements = seq.Remove(seq.Length - 1).Split('h');

            for (int sti = 0; sti != statements.Length; ++sti) {
                StringBox statement = statements[sti];

                var estring = statement.ExtactString();

                if (estring.StartsWith("nl")) { //<var>:=
                    if (IsVariableExists(input.tokens[statement.StartNumber].content, locals, out Variable found)) {
                        if (!found.@const) {
                            var outp = JustArithm(DecodeLabelToCode(statement.RemoveSub(0, 2)), out TreeNode value, locals);
                            if (outp.fail) return outp;
                            code.Add(new Assign(new AST.Variable(found.name), value));
                        } else {
                            var inp = input.tokens[statement.NumberAT(1)];
                            return SimpleOut.MakeFail("ты пытаешься присвоить значение константе...\nЕсли что, константа имеет имя \"" + found.name + "\".", inp.line, inp.column);
                        }
                    } else {
                        var inp = input.tokens[statement.NumberAT(1)];
                        return SimpleOut.MakeFail("ты пытаешься присвоить значение переменной, которая не сущетвует...\nЕсли что, ты указал вот это имя: \"" + input.tokens[statement.StartNumber].content + "\".", inp.line, inp.column);
                    }
                } else if (estring.StartsWith("nq")) { //<var>(
                    if (estring[estring.Length - 1] != 'r') {
                        var inp = input.tokens[statement.NumberAT(statement.Length - 1)];
                        return SimpleOut.MakeFail("скобку закрой - аргументы не трать.\nОператор вызова процедуры состоит из имени функции, открытой скобки и закрытой скобки...", inp.line, inp.column);
                    }
                    var funcname = input.tokens[statement.StartNumber].content;
                    if (procedures.ContainsKey(funcname)) {
                        StringBox[] arglist = statement.SubstringRange(2, statement.Length - 2).Split('g');

                        if (procedures[funcname].arguments.Length != arglist.Length) {
                            var inp = input.tokens[statement.NumberAT(1)];
                            return SimpleOut.MakeFail("процедура, которую ты хочешь вызвать, имеет не то количество аргументов, которое ты передал!\nИмя процедуры: " + funcname, inp.line, inp.column);
                        }

                        List<TreeNode> args = new();
                        for (int i = 0; i != arglist.Length; ++i) {
                            if (arglist[i].ExtactString() == "n") {
                                if (IsVariableExists(input.tokens[arglist[i].StartNumber].content, locals, out Variable found)) {
                                    if (found.@const && procedures[funcname].arguments[i].pointer) {
                                        var inp = input.tokens[arglist[i].StartNumber];
                                        return SimpleOut.MakeFail("нельзя передать константу \"" + found.name + "\" в ссылочный аргумент!\nИспользуй обычную переменную, либо же измени тип аргумента.", inp.line, inp.column);
                                    }
                                    args.Add(new AST.Variable(found.name));
                                } else {
                                    var inp = input.tokens[arglist[i].StartNumber];
                                    return SimpleOut.MakeFail("ты пытаешься использовать значение переменной, которая не сущетвует...\nЕсли что, ты указал вот это имя: \"" + input.tokens[statement.StartNumber].content + "\".", inp.line, inp.column);
                                }
                            } else {
                                if (procedures[funcname].arguments[i].pointer) {
                                    var inp = input.tokens[arglist[i].StartNumber];
                                    return SimpleOut.MakeFail("нельзя передать выражение \""+ DecodeLabelToCode(arglist[i]).ExtactString() + "\" в ссылочный аргумент!\nИспользуй просто переменную, либо же измени тип аргумента.", inp.line, inp.column);
                                }
                                var outp = JustArithm(DecodeLabelToCode(arglist[i]), out TreeNode value, locals);
                                if (outp.fail) return outp;
                                args.Add(value);
                            }
                        }

                        code.Add(new Call(funcname, args.ToArray()));
                    } else {
                        var inp = input.tokens[statement.NumberAT(1)];
                        return SimpleOut.MakeFail("процедуры с именем \""+ funcname + "\" не существует!" + input.tokens[statement.StartNumber].content + "\".", inp.line, inp.column);
                    }
                } else if (estring.StartsWith('y')) { //while
                    int conde = estring.IndexOf('z');
                    if (conde == -1) {
                        var inp = input.tokens[statement.StartNumber];
                        return SimpleOut.MakeFail("описанная сигнатура не соответствует сигнатуре цикла while!\nЦикл while должен выглядеть следующим образом: while <logical-condition> do" + input.tokens[statement.StartNumber].content + "\".", inp.line, inp.column);
                    }

                    var outp = JustArithmLogics(DecodeLabelToCode(statement.SubstringRange(1, conde - 1)), out TreeNode equation, locals);
                    if (outp.fail) return outp;

                    var oval = ParseSequentalElementCode_Seq(statement.Substring(conde + 1).Plus(new StringBox("h", 0)), locals, out List<TreeNode> ocode, nested);
                    if (oval.fail) return oval;

                    code.Add(new AST.While(ocode.ToArray(), equation));
                } else if (estring.StartsWith("v")) { //for
                    Match reg = Regex.Match(estring, @"^vnl([nosqr]+)([wx])([nosqr]+)z");
                    if (reg.Success && reg.Groups.Count == 4) {
                        var varn = input.tokens[statement.NumberAT(1)].content;
                        if (IsVariableExists(varn, locals, out Variable found)) {
                            if (found.@const) {
                                var inp = input.tokens[statement.NumberAT(1)];
                                return SimpleOut.MakeFail("нельзя использовать константу \"" + found.name + "\" в роли переменной-счётчика for!\nИспользуй обычную переменную, либо же измени тип аргумента.", inp.line, inp.column);
                            }
                            var outp = JustArithm(DecodeLabelToCode(statement.Substring(reg.Groups[1].Index, reg.Groups[1].Length)), out TreeNode from, locals);
                            if (outp.fail) return outp;
                            outp = JustArithm(DecodeLabelToCode(statement.Substring(reg.Groups[3].Index, reg.Groups[3].Length)), out TreeNode to, locals);
                            if (outp.fail) return outp;

                            var oval = ParseSequentalElementCode_Seq(statement.Substring(reg.Length).Plus(new StringBox("h", 0)), locals, out List<TreeNode> ocode, nested);
                            if (oval.fail) return oval;

                            code.Add(new For(ocode.ToArray(), from, to, (reg.Groups[2].Value == "w" ? true : false)));
                        } else {
                            var inp = input.tokens[statement.NumberAT(1)];
                            return SimpleOut.MakeFail("ты пытаешься использовать значение переменной, которая не сущетвует...\nЕсли что, ты указал вот это имя: \"" + input.tokens[statement.StartNumber].content + "\".", inp.line, inp.column);
                        }
                    } else {
                        var inp = input.tokens[statement.StartNumber];
                        return SimpleOut.MakeFail("описанная сигнатура не соответствует сигнатуре цикла for!\nЦикл for должен выглядеть следующим образом: for <variable-name> := <initial_value> to [downto] <final_value> do" + input.tokens[statement.StartNumber].content + "\".", inp.line, inp.column);
                    }
                } else if (estring == "m") { //exit
                    code.Add(new Exit());
                } else if (estring == "?") { //nested
                    var oval = ParseSequentalElementCode_Seq(nested[statement.StartNumber], locals, out List<TreeNode> ocode, nested);
                    if (oval.fail) return oval;
                    code = code.Concat(ocode).ToList();
                } else {
                    var inp = input.tokens[statement.StartNumber];
                    return SimpleOut.MakeFail("выражение не может быть распознано!\nТы написал что-то совсем не внятное: " + DecodeLabelToCode(statement).ExtactString() + "\nИсправь.", inp.line, inp.column);
                }
            }

            return SimpleOut.MakeSucess();
        }

        SimpleOut ParseSequentalElementCode(StringBox sequence, string name) {
            var myobj = procedures[name];
            Dictionary<string, Variable> locals = new();
            foreach (var item in myobj.arguments) {
                locals.TryAdd(item.name, new Variable(item.name, DataType.Integer, null, item.@const));
            }
            foreach (var item in myobj.variables) {
                locals.TryAdd(item.name, new Variable(item.name, DataType.Integer, null, item.constValue != null));
            }

            List<TreeNode> body = new((TreeNode[])myobj.body);
            List<StringBox> nested = new();

            var sections = PcreRegex.Matches(sequence.ExtactString(), @"c(?:[^cd]|(?R))*d").ToList();
            for (int i = sections.Count - 1; i >= 0; --i) {
                var par = sections[i];
                nested.Add(sequence.SubstringRange(par.Index + 1, par.EndIndex - 2));
                sequence = sequence.ReplaceRange(par.Index, par.EndIndex, new StringBox("?", nested.Count - 1, false));
            }

            for (int i = 0; i != nested.Count; ++i) {
                StringBox temp = nested[i];
                sections = PcreRegex.Matches(temp.ExtactString(), @"\((?:[^()]|(?R))*\)").ToList();
                for (int j = sections.Count - 1; j >= 0; --j) {
                    var par = sections[j];
                    nested.Add(temp.SubstringRange(par.Index + 1, par.EndIndex - 2));
                    nested[i] = temp.ReplaceRange(par.Index, par.EndIndex, new StringBox("?", nested.Count - 1, false));
                }
            }

            var oval = ParseSequentalElementCode_Seq(sequence, locals, out List<TreeNode> ocode, nested);
            if (oval.fail) return oval;

            myobj.body = body.Concat(ocode).ToArray();

            return SimpleOut.MakeSucess();
        }

        SimpleOut ParseSequentalMainCode(StringBox sequence, out TreeNode[] outcode) {
            outcode = null;
            List<StringBox> nested = new();

            var sections = PcreRegex.Matches(sequence.ExtactString(), @"c(?:[^cd]|(?R))*d").ToList();
            for (int i = sections.Count - 1; i >= 0; --i) {
                var par = sections[i];
                nested.Add(sequence.SubstringRange(par.Index + 1, par.EndIndex - 2));
                sequence = sequence.ReplaceRange(par.Index, par.EndIndex, new StringBox("?", nested.Count - 1, false));
            }

            for (int i = 0; i != nested.Count; ++i) {
                StringBox temp = nested[i];
                sections = PcreRegex.Matches(temp.ExtactString(), @"\((?:[^()]|(?R))*\)").ToList();
                for (int j = sections.Count - 1; j >= 0; --j) {
                    var par = sections[j];
                    nested.Add(temp.SubstringRange(par.Index + 1, par.EndIndex - 2));
                    nested[i] = temp.ReplaceRange(par.Index, par.EndIndex, new StringBox("?", nested.Count - 1, false));
                }
            }

            var oval = ParseSequentalElementCode_Seq(sequence, null, out List<TreeNode> ocode, nested);
            if (oval.fail) return oval;

            outcode = ocode.ToArray();

            return SimpleOut.MakeSucess();
        }
    }
}
