using System.Collections.Generic;

namespace Compilation.Parser {

    static class TokenMapping {

        public enum Token { //"_" - means "flexible" category (need more accurate recognition)
            Program, //a program name definition keyword
            Procedure, //b Procedure definition start
            Begin, //c code section
            End, //d code section
            Colon, //e in this case, colon is keyword
            Dot, //f End of program
            Comma, //g Argument list or var def
            Semicolon, //h End of phrase

            Var, //i Varables definition or pointer
            IntegerDef, //j DataType name: Integer
            Const, //k Const var

            Assign, //l Assing value to variable
            Exit, //m Exit from the program

            //DataTypes
            InternalName, //n Variable-valid sequence (variable, function-name, etc.)
            Integer, //o Integer value (like 89)
            Boolean, //p True or False values

            ParenthesisLeft, //q (
            ParenthesisRight, //r )

            ArithmeticOperator, //s +, -, *, /, etc.
            LogicOperator, //t and, or
            ComparsionOperator, //u >, <, =, <>, <=, >=

            LoopFor, //v for
            LoopFor_to, //w 1 to 9
            LoopFor_downto, //x 9 downto 1
            LoopWhile, //y while
            Loop_do //z while <condition> do
        }

        //\((?:[a-z]\w*|[\d+*\-\/]|(?R))+\)
        public static readonly Dictionary<string, Token> TokenCvt = new(new KeyValuePair<string, Token>[] {
            new("program", Token.Program),
            new("procedure", Token.Procedure),
            new("begin", Token.Begin),
            new("end", Token.End),
            new("colon", Token.Colon),
            new("dot", Token.Dot),
            new("comma", Token.Comma),
            new("semicolon", Token.Semicolon),

            new("var", Token.Var),
            new("integer-word", Token.IntegerDef),
            new("const", Token.Const),

            new("assign", Token.Assign),
            new("exit", Token.Exit),

            new("internal-name", Token.InternalName),
            new("number", Token.Integer),
            new("boolean", Token.Boolean),

            new("left-parenthesis", Token.ParenthesisLeft),
            new("right-parenthesis", Token.ParenthesisRight),

            new("arithmetic-operator", Token.ArithmeticOperator),
            new("logic-operator", Token.LogicOperator), 
            new("comparsion-operator", Token.ComparsionOperator),

            new("for", Token.LoopFor),
            new("to", Token.LoopFor_to),
            new("downto", Token.LoopFor_downto),
            new("while", Token.LoopWhile),
            new("do", Token.Loop_do)
        });
    }
}
