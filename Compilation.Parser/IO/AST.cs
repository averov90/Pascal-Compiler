
namespace Compilation.Parser.AST {
    struct VarDesc_ConstablePointeble {
        public VarDesc_ConstablePointeble(string name, string type, bool pointer, bool @const) {
            this.name = name;
            this.type = type;
            this.pointer = pointer;
            this.@const = @const;
        }
        public string name,
            type;
        public bool pointer, @const;
    }
    struct VarDef_Constable {
        public VarDef_Constable(string name, string type, TreeNode constValue) {
            this.name = name;
            this.type = type;
            this.constValue = constValue;
        }
        public string name,
            type;
        public object constValue;
    }

    abstract class TreeNode {
        public TreeNode(string type) {
            this.type = type;
        }
        public string type;
    }

    abstract class Operator : TreeNode {
        public Operator(string @operator) : base("operator") {
            this.@operator = @operator;
        }
        public string @operator;
    }

    abstract class Binary : Operator {
        public Binary(string @operator, TreeNode operandLeft, TreeNode operandRight) : base(@operator) {
            this.operandLeft = operandLeft;
            this.operandRight = operandRight;
        }
        public object operandLeft,
            operandRight;
    }
    abstract class Unary : Operator {
        public Unary(string @operator, TreeNode operandRight) : base(@operator) {
            this.operandRight = operandRight;
        }
        public object operandRight;
    }

    namespace ArithmeticOperators {
        class UMinus : Unary {
            public UMinus(TreeNode b) : base("u_minus", b) { }
        }
        class Plus : Binary {
            public Plus(TreeNode a, TreeNode b) : base("plus", a, b) { }
        }
        class Minus : Binary {
            public Minus(TreeNode a, TreeNode b) : base("minus", a, b) { }
        }
        class Multiply : Binary {
            public Multiply(TreeNode a, TreeNode b) : base("multiply", a, b) { }
        }
        class Divide : Binary { //Like Div in this case
            public Divide(TreeNode a, TreeNode b) : base("divide", a, b) { }
        }
        class Div : Binary {
            public Div(TreeNode a, TreeNode b) : base("div", a, b) { }
        }
        class Mod : Binary {
            public Mod(TreeNode a, TreeNode b) : base("mod", a, b) { }
        }
    }

    namespace LogicOperators {
        /*class Not : Unary {
            public Not(TreeNode b) : base("not", b) { }
        }*/
        class And : Binary {
            public And(TreeNode a, TreeNode b) : base("and", a, b) { }
        }
        class Or : Binary {
            public Or(TreeNode a, TreeNode b) : base("or", a, b) { }
        }
        class Less : Binary {
            public Less(TreeNode a, TreeNode b) : base("less", a, b) { }
        }
        class More : Binary {
            public More(TreeNode a, TreeNode b) : base("more", a, b) { }
        }
        class Equal : Binary {
            public Equal(TreeNode a, TreeNode b) : base("equal", a, b) { }
        }
        class LessEqual : Binary {
            public LessEqual(TreeNode a, TreeNode b) : base("less-equal", a, b) { }
        }
        class MoreEqual : Binary {
            public MoreEqual(TreeNode a, TreeNode b) : base("more-equal", a, b) { }
        }
        class NotEqual : Binary {
            public NotEqual(TreeNode a, TreeNode b) : base("non-equal", a, b) { }
        }
    }

    namespace DataTypes {
        class Integer : TreeNode {
            public Integer(int number) : base("number") {
                this.number = number;
            }
            public int number;
        }

        class Boolean : TreeNode {
            public Boolean(bool value) : base("boolean") {
                this.value = value;
            }
            public bool value;
        }

        /*class String : TreeNode {
            public String(string @string) : base("string") {
                this.@string = @string;
            }
            public string @string;
        }*/
    }

    class Call : TreeNode {
        public Call(string function, TreeNode[] arguments) : base("call") {
            this.function = function;
            this.arguments = arguments;
        }
        public string function;
        public object[] arguments;
    }

    class Assign : Binary {
        public Assign(TreeNode a, TreeNode b) : base("assign", a, b) { }
    }

    class Exit : Call {
        public Exit() : base("exit", System.Array.Empty<TreeNode>()) { }
    }

    class Variable : TreeNode {
        public Variable(string name) : base("variable") {
            this.name = name;
        }
        public string name;
    }

    class While : TreeNode {
        public While(TreeNode[] body, TreeNode condition) : base("while") {
            this.body = body;
            this.condition = condition;
        }
        public object condition;
        public object[] body;
    }

    class For : TreeNode {
        public For(TreeNode[] body, TreeNode from, TreeNode to, bool ascending) : base("for") {
            this.body = body;
            this.from = from;
            this.to = to;
            this.ascending = ascending;
        }
        public object from, to;
        public bool ascending;
        public object[] body;
    }


    class Procedure {
        public Procedure(string name, VarDesc_ConstablePointeble[] arguments, VarDef_Constable[] variables, TreeNode[] body) {
            this.name = name;
            this.arguments = arguments;
            this.variables = variables;
            this.body = body;
        }
        public string name;
        public VarDesc_ConstablePointeble[] arguments;
        public VarDef_Constable[] variables;
        public object[] body;
    }
}
