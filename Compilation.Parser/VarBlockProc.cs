using System.Collections.Generic;
using Compilation.Parser.AST;

namespace Compilation.Parser {
    partial class Processor {


        TreeNode[] AssignNonConstVariables(ref Dictionary<string, Variable> vars) {
            List <TreeNode> result = new();
            foreach (var item in vars) {
                if (!item.Value.@const && item.Value.initValue != null)
                    result.Add(new Assign(new AST.Variable(item.Key), item.Value.initValue));
            }
            return result.ToArray();
        }


        SimpleOut ProcessVarDefBlock_VarAura(StringBox phrase, ref Dictionary<string, Variable> lvariables) {
            StringBox[] statements = phrase.RemoveSub(0, 1).Split('g');
            byte mode = 0; //1 - initialised only, 2 - just type
            for (ushort i = 0; i != statements.Length; ++i) {
                var statmnt = statements[i];

                if (statmnt.Contains('l')) {
                    if (mode != 2) {
                        mode = 1;
                        StringBox[] parts = statmnt.Split('l');
                        if (parts.Length > 2) {
                            var inp = input.tokens[parts[2].StartNumber];
                            return SimpleOut.MakeFail("ты посадил семена оператора присваивания и они проросли?\nУдали лишние операторы - не смеши меня.", inp.line, inp.column);
                        }
                        if (parts[0].ExtactString() != "n") {
                            var inp = input.tokens[parts[0].StartNumber];
                            return SimpleOut.MakeFail("ты знаешь что такое переменная?\nУкажи правильное имя переменной - не смеши коней.", inp.line, inp.column);
                        }
                        if (parts[1].Length == 0) {
                            var inp = input.tokens[parts[1].StartNumber];
                            return SimpleOut.MakeFail("зачем ты поставил сюда оператор присваивания? Чтобы он мучался?\nУбери оператор присваивания или дай ему значение.", inp.line, inp.column);
                        }
                        var outp = JustArithm(DecodeLabelToCode(parts[1]), out TreeNode value, null);
                        if (outp.fail) return outp;

                        lvariables.Add(input.tokens[parts[0].StartNumber].content, new Variable(input.tokens[parts[0].StartNumber].content, DataType.Integer, value, false));
                    } else {
                        var inp = input.tokens[statmnt.StartNumber];
                        return SimpleOut.MakeFail("ну нельзя так беспардонно объявлять переменные...\nНельзя в одном блоке объявлять переменные со значением и без.", inp.line, inp.column);
                    }
                } else {
                    if (mode != 1) {
                        mode = 2;
                        if (i + 1 == statements.Length) {
                            StringBox[] parts = statmnt.Split('e');
                            if (parts.Length == 1) {
                                var inp = input.tokens[statmnt.StartNumber];
                                return SimpleOut.MakeFail("между прочим, при таком объявлении переменных, должен быть указан их тип.\nУкажи тип 'Integer'.", inp.line, inp.column);
                            }
                            if (parts.Length > 2) {
                                var inp = input.tokens[parts[2].StartNumber];
                                return SimpleOut.MakeFail("а тебе не кажется, что указано слишком много типов на одно выражение объявления?\nУдали лишние указания типа.", inp.line, inp.column);
                            }
                            if (parts[0].ExtactString() != "n") {
                                var inp = input.tokens[parts[0].StartNumber];
                                return SimpleOut.MakeFail("ты знаешь что такое переменная?\nУкажи правильное имя переменной - не смеши коней.", inp.line, inp.column);
                            }
                            if (parts[1].ExtactString() != "j") {
                                var inp = input.tokens[parts[1].StartNumber];
                                return SimpleOut.MakeFail("а больше ты ничего не хочешь?\nРаскатал тут губу...\nДавай, закатывай и объявляй целочисленный тип (Integer) - я только с ним работаю.", inp.line, inp.column);
                            }

                            lvariables.Add(input.tokens[parts[0].StartNumber].content, new Variable(input.tokens[parts[0].StartNumber].content, DataType.Integer, null, false));
                        } else {
                            if (statmnt.ExtactString() != "n") {
                                var inp = input.tokens[statmnt.StartNumber];
                                return SimpleOut.MakeFail("ты знаешь что такое переменная?\nУкажи правильное имя переменной - не смеши коней.", inp.line, inp.column);
                            }

                            lvariables.Add(input.tokens[statmnt.StartNumber].content, new Variable(input.tokens[statmnt.StartNumber].content, DataType.Integer, null, false));
                        }
                    } else {
                        var inp = input.tokens[statmnt.StartNumber];
                        return SimpleOut.MakeFail("ну нельзя так беспардонно объявлять переменные...\nНельзя в одном блоке объявлять переменные со значением и без.", inp.line, inp.column);
                    }
                }
            }
            return SimpleOut.MakeSucess();
        }

        SimpleOut ProcessVarDefBlock_ConstAura(StringBox phrase, ref Dictionary<string, Variable> lvariables) {
            StringBox[] statements = phrase.RemoveSub(0, 1).Split('g');
            for (ushort i = 0; i != statements.Length; ++i) {
                var statmnt = statements[i];

                if (statmnt.Contains('u')) {
                    StringBox[] parts = statmnt.Split("u", a => input.tokens[a].content == "=");
                    if (parts.Length > 2) {
                        var inp = input.tokens[parts[2].StartNumber];
                        return SimpleOut.MakeFail("ты посадил семена оператора '=' и они проросли?\nУдали лишние операторы - не смеши меня.", inp.line, inp.column);
                    }
                    if (parts[0].ExtactString() != "n") {
                        var inp = input.tokens[parts[0].StartNumber];
                        return SimpleOut.MakeFail("ты знаешь что такое rjycnfynf?\nЭто слово \"константа\", набранное на английской раскладке.\nУкажи правильное имя константе - не смеши коней.", inp.line, inp.column);
                    }
                    if (parts[1].Length == 0) {
                        var inp = input.tokens[parts[1].StartNumber];
                        return SimpleOut.MakeFail("тут надо указать какое-то значение, если тебе не понятно.\nХоть 0 поставь - мне всё равно.", inp.line, inp.column);
                    }
                    var outp = JustArithm(DecodeLabelToCode(parts[1]), out TreeNode value, null);
                    if (outp.fail) return outp;

                    lvariables.Add(input.tokens[parts[0].StartNumber].content, new Variable(input.tokens[parts[0].StartNumber].content, DataType.Integer, value, true));

                } else {
                    var inp = input.tokens[statmnt.StartNumber];
                    return SimpleOut.MakeFail("интересный ты человек...\nИ как же ты будешь использовать константу без значения?", inp.line, inp.column);
                }
            }
            return SimpleOut.MakeSucess();
        }


        SimpleOut ProcessVarDefBlock_Global(string rawreg) {
            StringBox[] phrases = new StringBox(rawreg).Split('h');

            bool varaura = false, constaura = false, prognameset = true;
            foreach (var phrase in phrases) {
                if (phrase.Length == 0) {
                    //var inp = input.tokens[phrase.StartNumber];
                    //return SimpleOut.MakeFail("ты посадил семена semicolon и они проросли?\nНе трать semicolon зря, иначе semicolon потратит твоё время.", inp.line, inp.column);
                    continue;
                }
                if (phrase[0] == 'i') {
                    constaura = false;
                    varaura = true;
                    var oval = ProcessVarDefBlock_VarAura(phrase, ref variables);
                    if (oval.fail) return oval;
                } else if (phrase[0] == 'k') {
                    varaura = false;
                    constaura = true;
                    var oval = ProcessVarDefBlock_ConstAura(phrase, ref variables);
                    if (oval.fail) return oval;
                } else if (phrase[0] == 'a') {
                    if (prognameset) 
                        programName = DecodeLabelToCode(phrase.RemoveSub(0, 1)).ExtactString();
                    else {
                        var inp = input.tokens[phrase.StartNumber];
                        return SimpleOut.MakeFail("менять решение по 7 раз на дню - неэффективно, а ты ещё и меня пытаешься заставить это делать.\nИмя программе можно дать только ОДИН раз.", inp.line, inp.column);
                    }
                    prognameset = false;
                } else if (varaura) {
                    var oval = ProcessVarDefBlock_VarAura(phrase, ref variables);
                    if (oval.fail) return oval;
                } else if (constaura) {
                    var oval = ProcessVarDefBlock_ConstAura(phrase, ref variables);
                    if (oval.fail) return oval;
                } else {
                    var inp = input.tokens[phrase.StartNumber];
                    return SimpleOut.MakeFail("ты сам-то понял, что написал?\nПочитай про синтаксис паскаля ещё...", inp.line, inp.column);
                }
            }
            return SimpleOut.MakeSucess();
        }

        SimpleOut ProcessVarDefBlock_Local(StringBox rawreg, ref Dictionary<string, Variable> lvariables) {
            StringBox[] phrases = rawreg.Split('h');

            bool varaura = false, constaura = false;
            foreach (var phrase in phrases) {
                if (phrase.Length == 0) {
                    //var inp = input.tokens[phrase.StartNumber];
                    //return SimpleOut.MakeFail("ты посадил семена semicolon и они проросли?\nНе трать semicolon зря, иначе semicolon потратит твоё время.", inp.line, inp.column);
                    continue;
                }
                if (phrase[0] == 'i') {
                    constaura = false;
                    varaura = true;
                    var oval = ProcessVarDefBlock_VarAura(phrase, ref lvariables);
                    if (oval.fail) return oval;
                } else if (phrase[0] == 'k') {
                    varaura = false;
                    constaura = true;
                    var oval = ProcessVarDefBlock_ConstAura(phrase, ref lvariables);
                    if (oval.fail) return oval;
                } else if (varaura) {
                    var oval = ProcessVarDefBlock_VarAura(phrase, ref lvariables);
                    if (oval.fail) return oval;
                } else if (constaura) {
                    var oval = ProcessVarDefBlock_ConstAura(phrase, ref lvariables);
                    if (oval.fail) return oval;
                } else {
                    var inp = input.tokens[phrase.StartNumber];
                    return SimpleOut.MakeFail("ты сам-то понял, что написал?\nПочитай про синтаксис паскаля ещё...", inp.line, inp.column);
                }
            }
            return SimpleOut.MakeSucess();
        }

    }
}
