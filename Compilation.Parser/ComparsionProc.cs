using System;
using System.Collections.Generic;
using Compilation.Parser.AST.ArithmeticOperators;

namespace Compilation.Parser {
    partial class Processor {

        readonly Dictionary<Type, TypeSign> typeComparsionPriority = new Dictionary<Type, TypeSign>(new KeyValuePair<Type, TypeSign>[] {
            new(typeof(AST.LogicOperators.NotEqual), new(typeof(AST.LogicOperators.Equal), "<>")),
            new(typeof(AST.LogicOperators.Equal), new(typeof(AST.LogicOperators.MoreEqual), "=")),
            new(typeof(AST.LogicOperators.MoreEqual), new(typeof(AST.LogicOperators.More), ">=")),
            new(typeof(AST.LogicOperators.More), new(typeof(AST.LogicOperators.LessEqual), ">")),
            new(typeof(AST.LogicOperators.LessEqual), new(typeof(AST.LogicOperators.Less), "<=")),
            new(typeof(AST.LogicOperators.Less), new(null, "<"))  
        });

        SimpleOut TryProcessComparsionUnit(StringBox expr, out AST.TreeNode tree, ref List<StringBox> replacements, Dictionary<string, Variable> lvariables) {
            tree = null;
            if (expr.Length == 0) {
                return SimpleOut.MakeFail("здесь что-то должно быть, но, почему-то, этого здесь нет...\nСделай одолжение, исправь...", input.tokens[expr.StartNumber].line, input.tokens[expr.StartNumber].column);
            }

            if (expr.Length == 1 && expr[0] == '?') {
                return MakeComparsionOperations(replacements[expr.StartNumber], out tree, typeof(AST.LogicOperators.NotEqual), ref replacements, lvariables);
            }
            return SimpleOut.MakeSucess();
        }

        SimpleOut MakeComparsionOperations(StringBox expr, out AST.TreeNode result, Type opType, ref List<StringBox> replacements, Dictionary<string, Variable> lvariables) {
            result = null;
            AST.TreeNode oout;
            StringBox[] exparts;
            {
                string sign = typeComparsionPriority[opType].my_sign;
                exparts = expr.Split(sign, p => input.tokens[p].content == sign);
            }
            if (exparts.Length > 1) {
                int last = (int)(exparts.Length - 1);
                SimpleOut oval;
                AST.Binary root = (AST.Binary)Activator.CreateInstance(opType, new object[] { null, null });

                //root.operandRight
                oval = TryProcessComparsionUnit(exparts[last], out oout, ref replacements, lvariables);
                root.operandRight = oout;

                if (oval.fail) return oval;
                if (root.operandRight == null) {
                    var nexT = typeComparsionPriority[opType].next_type;
                    if (nexT != null) {
                        oval = MakeComparsionOperations(exparts[last], out oout, nexT, ref replacements, lvariables);
                        root.operandRight = oout;
                        if (oval.fail) return oval;
                    } else {
                        oval = MakeArithmOperations(exparts[last], out oout, typeof(Plus), ref replacements, lvariables);
                        root.operandRight = oout;
                        if (oval.fail) return oval;
                    }
                }
                --last;
                //root.operandLeft
                oval = TryProcessComparsionUnit(exparts[last], out oout, ref replacements, lvariables);
                root.operandLeft = oout;
                if (oval.fail) return oval;
                if (root.operandLeft == null) {
                    var nexT = typeComparsionPriority[opType].next_type;
                    if (nexT != null) {
                        oval = MakeComparsionOperations(exparts[last], out oout, nexT, ref replacements, lvariables);
                        root.operandLeft = oout;
                        if (oval.fail) return oval;
                    } else {
                        oval = MakeArithmOperations(exparts[last], out oout, typeof(Plus), ref replacements, lvariables);
                        root.operandLeft = oout;
                        if (oval.fail) return oval;
                    }
                }
                --last;
                //tree
                for (; last >= 0; --last) {
                    root = (AST.Binary)Activator.CreateInstance(opType, new object[] { null, root });

                    oval = TryProcessComparsionUnit(exparts[last], out oout, ref replacements, lvariables);
                    root.operandLeft = oout;
                    if (oval.fail) return oval;
                    if (root.operandLeft == null) {
                        var nexT = typeComparsionPriority[opType].next_type;
                        if (nexT != null) {
                            oval = MakeComparsionOperations(exparts[last], out oout, nexT, ref replacements, lvariables);
                            root.operandLeft = oout;
                            if (oval.fail) return oval;
                        } else {
                            oval = MakeArithmOperations(exparts[last], out oout, typeof(Plus), ref replacements, lvariables);
                            root.operandLeft = oout;
                            if (oval.fail) return oval;
                        }
                    }
                }
                result = root;
            } else {
                var oval = TryProcessComparsionUnit(expr, out result, ref replacements, lvariables);
                if (oval.fail) return oval;
                if (result == null) {
                    var nexT = typeComparsionPriority[opType].next_type;
                    if (nexT != null) {
                        return MakeComparsionOperations(expr, out result, nexT, ref replacements, lvariables);
                    } else {
                        oval = MakeArithmOperations(expr, out result, typeof(Plus), ref replacements, lvariables);
                        if (oval.fail) return oval;
                    }
                }
            }
            return SimpleOut.MakeSucess();
        }
    }
}
