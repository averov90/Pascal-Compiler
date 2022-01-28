using System;
using System.Collections.Generic;
using System.Linq;
using PCRE;

namespace Compilation.Parser {
    partial class Processor {

        SimpleOut JustArithmLogics(StringBox code, out AST.TreeNode value, Dictionary<string, Variable> lvariables) {
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

            return MakeArithmLogicOperations(code, out value, typeof(AST.LogicOperators.Or), ref replacement, lvariables);
        }

        SimpleOut TryProcessLogicUnit(StringBox expr, out AST.TreeNode tree, ref List<StringBox> replacements, Dictionary<string, Variable> lvariables) {
            tree = null;
            if (expr.Length == 0) {
                return SimpleOut.MakeFail("здесь что-то должно быть, но, почему-то, этого здесь нет...\nСделай одолжение, исправь...", input.tokens[expr.StartNumber].line, input.tokens[expr.StartNumber].column);
            }

            if (expr.Length == 1 && expr[0] == '?') {
                return MakeArithmLogicOperations(replacements[expr.StartNumber], out tree, typeof(AST.LogicOperators.Or), ref replacements, lvariables);
            } else if (bool.TryParse(expr.ExtactString(), out bool temp)) {
                tree = new AST.DataTypes.Boolean(temp);
            } /*else if (input.tokens[expr.StartNumber].content == expr.ExtactString() + '?') { //Function

            }*/
            return SimpleOut.MakeSucess();
        }

        readonly Dictionary<Type, TypeSign> typeLogicPriority = new Dictionary<Type, TypeSign>(new KeyValuePair<Type, TypeSign>[] {
            new(typeof(AST.LogicOperators.Or), new(typeof(AST.LogicOperators.And), "or")),
            new(typeof(AST.LogicOperators.And), new(null, "and"))

        });
        SimpleOut MakeArithmLogicOperations(StringBox expr, out AST.TreeNode result, Type opType, ref List<StringBox> replacements, Dictionary<string, Variable> lvariables) {
            result = null;
            AST.TreeNode oout;
            StringBox[] exparts;
            {
                string sign = typeLogicPriority[opType].my_sign;
                exparts = expr.Split(sign, p => input.tokens[p].content == sign);
            }
            if (exparts.Length > 1) {
                int last = (int)(exparts.Length - 1);
                SimpleOut oval;
                AST.Binary root = (AST.Binary)Activator.CreateInstance(opType, new object[] { null, null });

                //root.operandRight
                oval = TryProcessLogicUnit(exparts[last], out oout, ref replacements, lvariables);
                root.operandRight = oout;
                if (oval.fail) return oval;
                if (root.operandRight == null) {
                    var nexT = typeLogicPriority[opType].next_type;
                    if (nexT != null) {
                        oval = MakeArithmLogicOperations(exparts[last], out oout, nexT, ref replacements, lvariables);
                        root.operandRight = oout;
                        if (oval.fail) return oval;
                    } else {
                        if (IsLogicalOperatorContains(exparts[last])) {
                            if (CountLogicalOperators(exparts[last]) == 1) {
                                oval = MakeComparsionOperations(exparts[last], out oout, typeof(AST.LogicOperators.NotEqual), ref replacements, lvariables);
                                root.operandRight = oout;
                                if (oval.fail) return oval;
                            } else {
                                return SimpleOut.MakeFail("слишком много операторов сравнения!\nВ одном выражении может быть использован лишь один оператор сравнения!\nПереданное выражение: " + exparts[last].ExtactString(), input.tokens[expr.StartNumber].line, input.tokens[expr.StartNumber].column);
                            }
                        } else {
                            return SimpleOut.MakeFail("невозможно преобразовать выражение к логическому без логического оператора сравнения: " + exparts[last].ExtactString() + "\nПоставьте оператор сравнения.", input.tokens[expr.StartNumber].line, input.tokens[expr.StartNumber].column);
                        }
                    }
                }
                --last;
                //root.operandLeft
                oval = TryProcessLogicUnit(exparts[last], out oout, ref replacements, lvariables);
                root.operandLeft = oout;
                if (oval.fail) return oval;
                if (root.operandLeft == null) {
                    var nexT = typeLogicPriority[opType].next_type;
                    if (nexT != null) {
                        oval = MakeArithmLogicOperations(exparts[last], out oout, nexT, ref replacements, lvariables);
                        root.operandLeft = oout;
                        if (oval.fail) return oval;
                    } else {
                        if (IsLogicalOperatorContains(exparts[last])) {
                            if (CountLogicalOperators(exparts[last]) == 1) {
                                oval = MakeComparsionOperations(exparts[last], out oout, typeof(AST.LogicOperators.NotEqual), ref replacements, lvariables);
                                root.operandLeft = oout;
                                if (oval.fail) return oval;
                            } else {
                                return SimpleOut.MakeFail("слишком много операторов сравнения!\nВ одном выражении может быть использован лишь один оператор сравнения!\nПереданное выражение: " + exparts[last].ExtactString(), input.tokens[expr.StartNumber].line, input.tokens[expr.StartNumber].column);
                            }
                        } else {
                            return SimpleOut.MakeFail("невозможно преобразовать выражение к логическому без логического оператора сравнения: " + exparts[last].ExtactString() + "\nПоставьте оператор сравнения.", input.tokens[expr.StartNumber].line, input.tokens[expr.StartNumber].column);
                        }
                    }
                }
                --last;
                //tree
                for (; last >= 0; --last) {
                    root = (AST.Binary)Activator.CreateInstance(opType, new object[] { null, root });

                    oval = TryProcessLogicUnit(exparts[last], out oout, ref replacements, lvariables);
                    root.operandLeft = oout;
                    if (oval.fail) return oval;
                    if (root.operandLeft == null) {
                        var nexT = typeLogicPriority[opType].next_type;
                        if (nexT != null) {
                            oval = MakeArithmLogicOperations(exparts[last], out oout, nexT, ref replacements, lvariables);
                            root.operandLeft = oout;
                            if (oval.fail) return oval;
                        } else {
                            if (IsLogicalOperatorContains(exparts[last])) {
                                if (CountLogicalOperators(exparts[last]) == 1) {
                                    oval = MakeComparsionOperations(exparts[last], out oout, typeof(AST.LogicOperators.NotEqual), ref replacements, lvariables);
                                    root.operandLeft = oout;
                                    if (oval.fail) return oval;
                                } else {
                                    return SimpleOut.MakeFail("слишком много операторов сравнения!\nВ одном выражении может быть использован лишь один оператор сравнения!\nПереданное выражение: " + exparts[last].ExtactString(), input.tokens[expr.StartNumber].line, input.tokens[expr.StartNumber].column);
                                }
                            } else {
                                return SimpleOut.MakeFail("невозможно преобразовать выражение к логическому без логического оператора сравнения: " + exparts[last].ExtactString() + "\nПоставьте оператор сравнения.", input.tokens[expr.StartNumber].line, input.tokens[expr.StartNumber].column);
                            }
                        }
                    }
                }
                result = root;
            } else {
                var oval = TryProcessLogicUnit(expr, out result, ref replacements, lvariables);
                if (oval.fail) return oval;
                if (result == null) {
                    var nexT = typeLogicPriority[opType].next_type;
                    if (nexT != null) {
                        return MakeArithmLogicOperations(expr, out result, nexT, ref replacements, lvariables);
                    } else {
                        if (IsLogicalOperatorContains(expr)) {
                            if (CountLogicalOperators(expr) == 1) {
                                oval = MakeComparsionOperations(expr, out result, typeof(AST.LogicOperators.NotEqual), ref replacements, lvariables);
                                if (oval.fail) return oval;
                            } else {
                                return SimpleOut.MakeFail("слишком много операторов сравнения!\nВ одном выражении может быть использован лишь один оператор сравнения!\nПереданное выражение: " + expr.ExtactString(), input.tokens[expr.StartNumber].line, input.tokens[expr.StartNumber].column);
                            }
                        } else {
                            return SimpleOut.MakeFail("невозможно преобразовать выражение к логическому без логического оператора сравнения: " + expr.ExtactString() + "\nПоставьте оператор сравнения.", input.tokens[expr.StartNumber].line, input.tokens[expr.StartNumber].column);
                        }
                    }
                }
            }
            return SimpleOut.MakeSucess();
        }

        bool IsLogicalOperatorContains(StringBox strb) {
            foreach (var item in strb.ExtactNumbers()) {
                if (input.tokens[item].type == "comparsion-operator")
                    return true;
            }
            return false;
        }

        ushort CountLogicalOperators(StringBox strb) {
            ushort count = 0;
            foreach (var item in strb.ExtactNumbers().Distinct()) {
                if (input.tokens[item].type == "comparsion-operator")
                    ++count;
            }
            return count;
        }

    }
}
