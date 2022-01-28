using System;
using System.Collections.Generic;
using System.Linq;
using PCRE;
using Compilation.Parser.AST.ArithmeticOperators;

namespace Compilation.Parser {
    partial class Processor {
        SimpleOut JustArithm(StringBox code, out AST.TreeNode value, Dictionary<string, Variable> lvariables) {
            List<PcreMatch> pars = PcreRegex.Matches(code.ExtactString(), @"\((?:[^()]|(?R))*\)").ToList();
            List<StringBox> replacement = new();

            for (int i = pars.Count - 1; i >= 0; --i) {
                var par = pars[i];
                replacement.Add(code.SubstringRange(par.Index + 1, par.EndIndex - 2));
                code = code.ReplaceRange(par.Index, par.EndIndex, new StringBox("?", replacement.Count - 1, false));
            }

            for (int i = 0; i != replacement.Count; ++i) {
                StringBox temp = replacement[i];
                pars = PcreRegex.Matches(temp.ExtactString(), @"\((?:[^()]|(?R))*\)").ToList();
                for (int j = pars.Count - 1; j >= 0; --j) {
                    var par = pars[j];
                    replacement.Add(temp.SubstringRange(par.Index + 1, par.EndIndex - 2));
                    replacement[i] = temp.ReplaceRange(par.Index, par.EndIndex, new StringBox("?", replacement.Count - 1, false));
                }
            }

            return MakeArithmOperations(code, out value, typeof(Plus), ref replacement, lvariables);
        }

        struct SimpleOut {
            public static SimpleOut MakeFail(string errorDesc, uint errorLine, uint errorColumn) {
                var outp = new SimpleOut();
                outp.fail = true;
                outp.errorDesc = errorDesc;
                outp.errorLine = errorLine;
                outp.errorColumn = errorColumn;
                return outp;
            }

            public static SimpleOut MakeSucess() {
                var outp = new SimpleOut();
                outp.fail = false;
                return outp;
            }

            public bool fail;
            public string errorDesc;
            public uint errorLine,
                errorColumn;
        }

        struct TypeSign {
            public TypeSign(Type next_type, string my_sign) {
                this.next_type = next_type;
                this.my_sign = my_sign;
            }
            public Type next_type;
            public string my_sign;
        }
        readonly Dictionary<Type, TypeSign> typePriority = new Dictionary<Type, TypeSign>(new KeyValuePair<Type, TypeSign>[] {
            new(typeof(Plus), new(typeof(Minus), "+")),
            new(typeof(Minus), new(typeof(Multiply), "-")),
            new(typeof(Multiply), new(typeof(Divide), "*")),
            new(typeof(Divide), new(typeof(Div), "/")),
            new(typeof(Div), new(typeof(Mod), "div")),
            new(typeof(Mod), new(null, "mod"))
        });

        bool IsVariableExists(string name, Dictionary<string, Variable> locals, out Variable found) {
            if (locals != null && locals.ContainsKey(name)) {
                found = locals[name];
                return true;
            } else if (variables.ContainsKey(name)) {
                found = variables[name];
                return true;
            }
            found = new Variable();
            return false;
        }

        SimpleOut TryProcessUnit(StringBox expr, out AST.TreeNode tree, ref List<StringBox> replacements, Dictionary<string, Variable> lvariables) {
            tree = null;
            if (expr.Length == 0) {
                return SimpleOut.MakeFail("здесь что-то должно быть, но, почему-то, этого здесь нет...\nСделай одолжение, исправь...", input.tokens[expr.StartNumber].line, input.tokens[expr.StartNumber].column);
            }

            if (expr.Length == 1 && expr[0] == '?') {
                return MakeArithmOperations(replacements[expr.StartNumber], out tree, typeof(Plus), ref replacements, lvariables);
            } else if (ushort.TryParse(expr.ExtactString(), out ushort temp)) {
                tree = new AST.DataTypes.Integer(temp);
            } else if (input.tokens[expr.StartNumber].content == expr.ExtactString()) { //Variable
                string varn = expr.ExtactString();
                if (IsVariableExists(varn, lvariables, out Variable found)) {
                    if (found.type != DataType.Integer)
                        return SimpleOut.MakeFail("хочу открыть тебе тайну. Знаешь, а переменная \"" + varn + "\"... она... не числовая... :'(", input.tokens[expr.StartNumber].line, input.tokens[expr.StartNumber].column);

                    /*if (found.initValue == null) { //I changed my mind about doing static analysis
                        return SimpleOut.MakeFail("просто замечательно: ты собрался использовать значение переменной \"" + varn + "\", которой не присвоено значение...\nДелаешь успехи...", input.tokens[expr.StartNumber].line, input.tokens[expr.StartNumber].column);
                    }*/
                    tree = new AST.Variable(varn);
                } else {
                    return SimpleOut.MakeFail("отлично, просто замечательно, но я не знаю переменную по имени \"" + varn + "\"\nПеременную надо СНАЧАЛА объявить, а уж потом использовать. Пожалуйста, не перепутай.", input.tokens[expr.StartNumber].line, input.tokens[expr.StartNumber].column);
                }
            } /*else if (input.tokens[expr.StartNumber].content == expr.ExtactString() + '?') { //Function

            }*/
            return SimpleOut.MakeSucess();
        }

        SimpleOut MakeArithmOperations(StringBox expr, out AST.TreeNode result, Type opType, ref List<StringBox> replacements, Dictionary<string, Variable> lvariables) {
            result = null;
            AST.TreeNode oout;
            StringBox[] exparts;
            {
                string sign = typePriority[opType].my_sign;
                exparts = expr.Split(sign, p => input.tokens[p].content == sign);
            }
            if (exparts.Length > 1) {
                int last = (int)(exparts.Length - 1);
                SimpleOut oval;
                AST.Binary root = (AST.Binary)Activator.CreateInstance(opType, new object[] { null, null });

                //root.operandRight
                oval = TryProcessUnit(exparts[last], out oout, ref replacements, lvariables);
                root.operandRight = oout;
                if (oval.fail) return oval;
                if (root.operandRight == null) {
                    var nexT = typePriority[opType].next_type;
                    if (nexT != null) {
                        oval = MakeArithmOperations(exparts[last], out oout, nexT, ref replacements, lvariables);
                        root.operandRight = oout;
                        if (oval.fail) return oval;
                    } else {
                        return SimpleOut.MakeFail("ты сам-то понимаешь, что пишешь? Если нет, зачем заставлять понимать меня?\nСимвол: \"" + exparts[last].ExtactString() + "\" не является числом! А я работаю только с числами.", input.tokens[expr.StartNumber].line, input.tokens[expr.StartNumber].column);
                    }
                }
                --last;
                //root.operandLeft
                if (opType == typeof(Minus) && exparts.Length == 2 && exparts[last].Length == 0) {
                    result = new UMinus((AST.TreeNode)root.operandRight); //Unary minus, no other doings needed
                    return SimpleOut.MakeSucess();
                }
                oval = TryProcessUnit(exparts[last], out oout, ref replacements, lvariables);
                root.operandLeft = oout;
                if (oval.fail) return oval;
                if (root.operandLeft == null) {
                    var nexT = typePriority[opType].next_type;
                    if (nexT != null) {
                        oval = MakeArithmOperations(exparts[last], out oout, nexT, ref replacements, lvariables);
                        root.operandLeft = oout;
                        if (oval.fail) return oval;
                    } else {
                        return SimpleOut.MakeFail("ты сам-то понимаешь, что пишешь? Если нет, зачем заставлять понимать меня?\nСимвол: \"" + exparts[last].ExtactString() + "\" не является числом! А я работаю только с числами.", input.tokens[expr.StartNumber].line, input.tokens[expr.StartNumber].column);
                    }
                }
                --last;
                //tree
                for (; last >= 0; --last) {
                    root = (AST.Binary)Activator.CreateInstance(opType, new object[] { null, root });

                    oval = TryProcessUnit(exparts[last], out oout, ref replacements, lvariables);
                    root.operandLeft = oout;
                    if (oval.fail) return oval;
                    if (root.operandLeft == null) {
                        var nexT = typePriority[opType].next_type;
                        if (nexT != null) {
                            oval = MakeArithmOperations(exparts[last], out oout, nexT, ref replacements, lvariables);
                            root.operandLeft = oout;
                            if (oval.fail) return oval;
                        } else {
                            return SimpleOut.MakeFail("ты сам-то понимаешь, что пишешь? Если нет, зачем заставлять понимать меня?\nСимвол: \"" + exparts[last].ExtactString() + "\" не является числом! А я работаю только с числами.", input.tokens[expr.StartNumber].line, input.tokens[expr.StartNumber].column);
                        }
                    }
                }
                result = root;
            } else {
                var oval = TryProcessUnit(expr, out result, ref replacements, lvariables);
                if (oval.fail) return oval;
                if (result == null) {
                    var nexT = typePriority[opType].next_type;
                    if (nexT != null) {
                        return MakeArithmOperations(expr, out result, nexT, ref replacements, lvariables);
                    } else {
                        return SimpleOut.MakeFail("ты сам-то понимаешь, что пишешь? Если нет, зачем заставлять понимать меня?\nСимвол: \"" + expr.ExtactString() + "\" не является числом! А я работаю только с числами.", input.tokens[expr.StartNumber].line, input.tokens[expr.StartNumber].column);
                    }
                }
            }
            return SimpleOut.MakeSucess();
        }
    }
}
