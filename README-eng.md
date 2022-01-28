# Compiler for a subset of the Pascal language
[![License](https://img.shields.io/badge/LICENSE-LGPL%20v2.1-green?style=flat-square)](/LICENSE)  [![Version](https://img.shields.io/badge/VERSION-RELEASE%20--%201.0-green?style=flat-square)](https://github.com/averov90/Pascal-Compiler/releases)
### :small_orange_diamond: [Russian version](/README.md)

This repository provides a lexer and parser for a subset of the Pascal programming language.
Both the lexer and the parser report syntactic and lexical, respectively, by indicating the location in the source code (row and column).

A full-fledged compiler consists of three parts (if you perform division according to the principle already shown): *lexer*, *parser*, *code generator*.
This repository contains only the first 2 components. This leaves open the question of the final form of the code, that is, what the Pascal code will ultimately be converted to.

After work, the parser creates an Abstract Syntax Tree (AST), which is a representation of the logic of the source program.
Further, AST can be used in any way: from code generation in any language (assembler, machine code, python) to execution in place, that is, interpretation.
In the latter case, it turns out not a compiler, but an interpreter.

All parts of the compiler (lexer and parser in this case) have a "communication" protocol, which has been reduced to JSON format for convenience. This will allow, if necessary, to work with interaction through network protocols, which is a direct path to microservices.

## Supported Pascal features

* program character recognition (setting the program name)
* declaration of global variables of integer type (integer) at the very top of the program (after an entry point or procedure, global vars are prohibited)
* declaration of local variables of integer type (integer) in the procedure
* support for any nesting of begin/end code blocks.
* support for for loops (with "to" and "downto" support)
* support for while loops
* binary numeric operations: +, -, \*, / (aka div because there is no real), mod, unary minus
* logical operations: and, or
* comparison operators: =, >, <, <>, <= and >=
* immutable (const) var-values ​​(both local and global)
* procedures (without returning a value via return, but with support for returning a value via an argument)
* exit keyword
* pre-defined writeln procedure

## Lexer

The input data for the lexer to work must have the following format:
```json
{
  "session": "id",
  "source": [ "text data" ]
}
```

* **session** - session identifier given by the manager (can be used to identify the code processing pipeline).
* **source** - text (code) entered by the user in the code entry field. The text is passed to the lexer without any changes.

The lexer output has the following format:
```json
{
  "success": true,
  "errorDesc": "text",
  "errorLine": 10,
  "errorColumn": 4,
  "tokens": []
}
```

* **success** - true or false - success of the token extraction operation. If false, then an error occurred.
* **errorDesc** - only needed if success=false is a description of the error, such as "Unexpected character encountered. Expected character ;"
* **errorLine** - may or may not be present only if success=false is the line number where the error occurred. The line number must be from the original text, not the filtered text from the lexer.
* **errorColumn** - may be (may not be) only if success=false is the number of the character in the string that is an error. The character number must be from the original text, not filtered text from the lexer.
* **tokens** - should be only if success=true - a list of tokens retrieved from the program.

The lexer splits the code into the following tokens (groups):
* program
* procedure
* begin
* end
* colon
* dot
* comma
* semicolon
* var
* integer-word
* const
* assign
* exit
* internal-name
* number
* boolean
* left-parenthesis
* right-parenthesis
* arithmetic-operator
* logic-operator
* comparsion-operator
* for
* to
* downto
* while
* do

## Parser

The input data for the lexer to work must have the following format:
```json
{
  "session": "id",
  "tokens": []
}
```

* **session** - session identifier given by the manager (can be used to identify the code processing pipeline).
* **tokens** - list of tokens.

The lexer output has the following format:
```json
{
  "success": true,
  "errorDesc": "text",
  "errorLine": 10,
  "errorColumn": 4,
  "variables": [],
  "syntaxTree": {}
}
```

* **success** - true or false - success of the syntax tree building operation. If false, then an error occurred.
* **errorDesc** - only needed if success=false is a description of the error, such as "Attempt to change the value of a constant."
* **errorLine** - may or may not be present only if success=false is the line number where the error occurred.
* **errorColumn** - may be (may not be) only if success=false is the number of the character in the string that is an error.
* **variables** - required only if success=true - a list of global variables that are used in the program (with type indication). For example: "variables": [ { "name": "someData", "type": "integer", "constValue": null } ]
* **syntaxTree** - only required if success=true is an abstract syntax tree (NOT binary) that has a recursive structure.

### Nested tree nodes

Category 1:
```json
{
  "type": "operator"
}
```

Category 2:
```json
{
  "type": "number"
}
```

Category 3:
```json
{
  "type": "boolean"
}
```

Category 4:
```json
{
  "type": "call"
}
```

Category 5:
```json
{
  "type": "variable"
}
```

Category 6:
```json
{
  "type": "for"
}
```

Category 7:
```json
{
  "type": "while"
}
```

#### Category 1 - detail
```json
{
  "type": "operator",
  "operator": "u_minus",
  "operandRight": {}
}
```

```json
{
  "type": "operator",
  "operator": "plus",
  "operandLeft": {},
  "operandRight": {}
}
```

```json
{
  "type": "operator",
  "operator": "minus",
  "operandLeft": {},
  "operandRight": {}
}
```

```json
{
  "type": "operator",
  "operator": "multiply",
  "operandLeft": {},
  "operandRight": {}
}
```

```json
{
  "type": "operator",
  "operator": "divide",
  "operandLeft": {},
  "operandRight": {}
}
```

```json
{
  "type": "operator",
  "operator": "div",
  "operandLeft": {},
  "operandRight": {}
}
```

```json
{
  "type": "operator",
  "operator": "mod",
  "operandLeft": {},
  "operandRight": {}
}
```

```json
{
  "type": "operator",
  "operator": "less",
  "operandLeft": {},
  "operandRight": {}
}
```

```json
{
  "type": "operator",
  "operator": "more",
  "operandLeft": {},
  "operandRight": {}
}
```

```json
{
  "type": "operator",
  "operator": "equal",
  "operandLeft": {},
  "operandRight": {}
}
```

```json
{
  "type": "operator",
  "operator": "non-equal",
  "operandLeft": {},
  "operandRight": {}
}
```

```json
{
  "type": "operator",
  "operator": "less-equal",
  "operandLeft": {},
  "operandRight": {}
}
```

```json
{
  "type": "operator",
  "operator": "more-equal",
  "operandLeft": {},
  "operandRight": {}
}
```

```json
{
  "type": "operator",
  "operator": "and",
  "operandLeft": {},
  "operandRight": {}
}
```

```json
{
  "type": "operator",
  "operator": "or",
  "operandLeft": {},
  "operandRight": {}
}
```

#### Category 2 - detail
```json
{
  "type": "number",
  "number": 1234
}
```

#### Category 3 - detail
```json
{
  "type": "boolean",
  "value": true
}
```

#### Category 4 - detail
```json
{
  "type": "call",
  "function": "",
  "arguments": [
    {
      <!--This is a comment. Only computable values can be found in this object: an arithmetic operator, a number, or a variable. Even boolean can't get caught.-->
    }
  ]
}
```

#### Category 5 - detail
```json
{
  "type": "variable",
  "name": ""
}
```

#### Category 6 - detail
```json
{
  "type": "for",
  "from": {},   <!--This is a comment. Only computable values can be found in this object: an arithmetic operator, a number, or a variable. Even boolean can't get caught.-->
  "to": {},  <!--This is a comment. Only computable values can be found in this object: an arithmetic operator, a number, or a variable. Even boolean can't get caught.-->
  "ascending": true,
  "body": [
    {}  <!--This is a comment. Anything can be found in this object.-->
  ]
}
```

#### Category 7 - detail
```json
{
  "type": "while",
  "condition": {}, <!--This is a comment. Only logical values can be found in this object: boolean, logical operator, or comparison operator.-->
  "body": [
    {}  <!--This is a comment. Anything can be found in this object.-->
  ]
}
```

### Не-вложенные узлы дерева
```json
{
  "main": [
    {} <!--This is a comment. Anything can be found in this object.-->
  ],
  "procedures": [
    {
      "name": "",
      "arguments": [
        {
          "type": "integer",
          "name": "", <!--This name is also an announcement-->
          "pointer": true, <!--pointer==true and const==true cannot be at the same time-->
		  "const": false
        }
      ],
      "variables": [],  <!--Here it could be { "name": "", "type": "integer", "constValue": null } -->
      "body": [
        {} <!--This is a comment. Anything can be found in this object.-->
      ]
    }
  ]
}
```

## Examples

### Example 1

Source:
```pascal
PROGRAM dfg;

var r0eg := 5  + 8 * 2 mod ( 4 + 2 ) / (7-5);

procedure name4(const a, b : Integer; var c : Integer);
begin
	c := 10;
	begin
		while a > b or false or 4-5<>2 do
			c := a + b;
	end;
end;

BEGIN
	name4(10, 20, r0eg);
end.
```

1. The source code must be placed in a file named `code.pas`
2. Run the lexer with the command `Compilation.Lexer.exe /pretty`
The output will be a file named `code.lex.out` with the following content:

<details>
  <summary>The contents of the code.lex.out file</summary>
  
```json
{
  "success": true,
  "errorDesc": null,
  "errorLine": 0,
  "errorColumn": 0,
  "tokens": [
    {
      "type": "program",
      "content": "program",
      "line": 0,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "dfg",
      "line": 0,
      "column": 8
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 0,
      "column": 11
    },
    {
      "type": "var",
      "content": "var",
      "line": 2,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "r0eg",
      "line": 2,
      "column": 4
    },
    {
      "type": "assign",
      "content": ":=",
      "line": 2,
      "column": 9
    },
    {
      "type": "number",
      "content": "5",
      "line": 2,
      "column": 12
    },
    {
      "type": "arithmetic-operator",
      "content": "\u002B",
      "line": 2,
      "column": 15
    },
    {
      "type": "number",
      "content": "8",
      "line": 2,
      "column": 17
    },
    {
      "type": "arithmetic-operator",
      "content": "*",
      "line": 2,
      "column": 19
    },
    {
      "type": "number",
      "content": "2",
      "line": 2,
      "column": 21
    },
    {
      "type": "arithmetic-operator",
      "content": "mod",
      "line": 2,
      "column": 23
    },
    {
      "type": "left-parenthesis",
      "content": "(",
      "line": 2,
      "column": 27
    },
    {
      "type": "number",
      "content": "4",
      "line": 2,
      "column": 29
    },
    {
      "type": "arithmetic-operator",
      "content": "\u002B",
      "line": 2,
      "column": 31
    },
    {
      "type": "number",
      "content": "2",
      "line": 2,
      "column": 33
    },
    {
      "type": "right-parenthesis",
      "content": ")",
      "line": 2,
      "column": 35
    },
    {
      "type": "arithmetic-operator",
      "content": "/",
      "line": 2,
      "column": 37
    },
    {
      "type": "left-parenthesis",
      "content": "(",
      "line": 2,
      "column": 39
    },
    {
      "type": "number",
      "content": "7",
      "line": 2,
      "column": 40
    },
    {
      "type": "arithmetic-operator",
      "content": "-",
      "line": 2,
      "column": 41
    },
    {
      "type": "number",
      "content": "5",
      "line": 2,
      "column": 42
    },
    {
      "type": "right-parenthesis",
      "content": ")",
      "line": 2,
      "column": 43
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 2,
      "column": 44
    },
    {
      "type": "procedure",
      "content": "procedure",
      "line": 4,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "name4",
      "line": 4,
      "column": 10
    },
    {
      "type": "left-parenthesis",
      "content": "(",
      "line": 4,
      "column": 15
    },
    {
      "type": "const",
      "content": "const",
      "line": 4,
      "column": 16
    },
    {
      "type": "internal-name",
      "content": "a",
      "line": 4,
      "column": 22
    },
    {
      "type": "comma",
      "content": ",",
      "line": 4,
      "column": 23
    },
    {
      "type": "internal-name",
      "content": "b",
      "line": 4,
      "column": 25
    },
    {
      "type": "colon",
      "content": ":",
      "line": 4,
      "column": 27
    },
    {
      "type": "integer-word",
      "content": "integer",
      "line": 4,
      "column": 29
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 4,
      "column": 36
    },
    {
      "type": "var",
      "content": "var",
      "line": 4,
      "column": 38
    },
    {
      "type": "internal-name",
      "content": "c",
      "line": 4,
      "column": 42
    },
    {
      "type": "colon",
      "content": ":",
      "line": 4,
      "column": 44
    },
    {
      "type": "integer-word",
      "content": "integer",
      "line": 4,
      "column": 46
    },
    {
      "type": "right-parenthesis",
      "content": ")",
      "line": 4,
      "column": 53
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 4,
      "column": 54
    },
    {
      "type": "begin",
      "content": "begin",
      "line": 5,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "c",
      "line": 6,
      "column": 0
    },
    {
      "type": "assign",
      "content": ":=",
      "line": 6,
      "column": 2
    },
    {
      "type": "number",
      "content": "10",
      "line": 6,
      "column": 5
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 6,
      "column": 7
    },
    {
      "type": "begin",
      "content": "begin",
      "line": 7,
      "column": 0
    },
    {
      "type": "while",
      "content": "while",
      "line": 8,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "a",
      "line": 8,
      "column": 6
    },
    {
      "type": "comparsion-operator",
      "content": "\u003E",
      "line": 8,
      "column": 8
    },
    {
      "type": "internal-name",
      "content": "b",
      "line": 8,
      "column": 10
    },
    {
      "type": "logic-operator",
      "content": "or",
      "line": 8,
      "column": 12
    },
    {
      "type": "boolean",
      "content": "false",
      "line": 8,
      "column": 15
    },
    {
      "type": "logic-operator",
      "content": "or",
      "line": 8,
      "column": 21
    },
    {
      "type": "number",
      "content": "4",
      "line": 8,
      "column": 24
    },
    {
      "type": "arithmetic-operator",
      "content": "-",
      "line": 8,
      "column": 25
    },
    {
      "type": "number",
      "content": "5",
      "line": 8,
      "column": 26
    },
    {
      "type": "comparsion-operator",
      "content": "\u003C\u003E",
      "line": 8,
      "column": 27
    },
    {
      "type": "number",
      "content": "2",
      "line": 8,
      "column": 29
    },
    {
      "type": "do",
      "content": "do",
      "line": 8,
      "column": 31
    },
    {
      "type": "internal-name",
      "content": "c",
      "line": 9,
      "column": 0
    },
    {
      "type": "assign",
      "content": ":=",
      "line": 9,
      "column": 2
    },
    {
      "type": "internal-name",
      "content": "a",
      "line": 9,
      "column": 5
    },
    {
      "type": "arithmetic-operator",
      "content": "\u002B",
      "line": 9,
      "column": 7
    },
    {
      "type": "internal-name",
      "content": "b",
      "line": 9,
      "column": 9
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 9,
      "column": 10
    },
    {
      "type": "end",
      "content": "end",
      "line": 10,
      "column": 0
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 10,
      "column": 3
    },
    {
      "type": "end",
      "content": "end",
      "line": 11,
      "column": 0
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 11,
      "column": 3
    },
    {
      "type": "begin",
      "content": "begin",
      "line": 13,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "name4",
      "line": 14,
      "column": 0
    },
    {
      "type": "left-parenthesis",
      "content": "(",
      "line": 14,
      "column": 5
    },
    {
      "type": "number",
      "content": "10",
      "line": 14,
      "column": 6
    },
    {
      "type": "comma",
      "content": ",",
      "line": 14,
      "column": 8
    },
    {
      "type": "number",
      "content": "20",
      "line": 14,
      "column": 10
    },
    {
      "type": "comma",
      "content": ",",
      "line": 14,
      "column": 12
    },
    {
      "type": "internal-name",
      "content": "r0eg",
      "line": 14,
      "column": 14
    },
    {
      "type": "right-parenthesis",
      "content": ")",
      "line": 14,
      "column": 18
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 14,
      "column": 19
    },
    {
      "type": "end",
      "content": "end",
      "line": 15,
      "column": 0
    },
    {
      "type": "dot",
      "content": ".",
      "line": 15,
      "column": 3
    }
  ]
}
```

</details>

3. Make sure that the tokenization was successful (the `success` field in the file has the value `true`), otherwise you should look at the error message and the place in the code that caused the error.
4. Convert the contents of the `code.lex.out` file and place it in the `code.lex.in` file. The content of `code.lex.in` should be:

<details>
  <summary>The contents of the code.lex.in file</summary>
  
```json
{
  "session": "",
  "tokens": [
    {
      "type": "program",
      "content": "program",
      "line": 0,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "dfg",
      "line": 0,
      "column": 8
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 0,
      "column": 11
    },
    {
      "type": "var",
      "content": "var",
      "line": 2,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "r0eg",
      "line": 2,
      "column": 4
    },
    {
      "type": "assign",
      "content": ":=",
      "line": 2,
      "column": 9
    },
    {
      "type": "number",
      "content": "5",
      "line": 2,
      "column": 12
    },
    {
      "type": "arithmetic-operator",
      "content": "\u002B",
      "line": 2,
      "column": 15
    },
    {
      "type": "number",
      "content": "8",
      "line": 2,
      "column": 17
    },
    {
      "type": "arithmetic-operator",
      "content": "*",
      "line": 2,
      "column": 19
    },
    {
      "type": "number",
      "content": "2",
      "line": 2,
      "column": 21
    },
    {
      "type": "arithmetic-operator",
      "content": "mod",
      "line": 2,
      "column": 23
    },
    {
      "type": "left-parenthesis",
      "content": "(",
      "line": 2,
      "column": 27
    },
    {
      "type": "number",
      "content": "4",
      "line": 2,
      "column": 29
    },
    {
      "type": "arithmetic-operator",
      "content": "\u002B",
      "line": 2,
      "column": 31
    },
    {
      "type": "number",
      "content": "2",
      "line": 2,
      "column": 33
    },
    {
      "type": "right-parenthesis",
      "content": ")",
      "line": 2,
      "column": 35
    },
    {
      "type": "arithmetic-operator",
      "content": "/",
      "line": 2,
      "column": 37
    },
    {
      "type": "left-parenthesis",
      "content": "(",
      "line": 2,
      "column": 39
    },
    {
      "type": "number",
      "content": "7",
      "line": 2,
      "column": 40
    },
    {
      "type": "arithmetic-operator",
      "content": "-",
      "line": 2,
      "column": 41
    },
    {
      "type": "number",
      "content": "5",
      "line": 2,
      "column": 42
    },
    {
      "type": "right-parenthesis",
      "content": ")",
      "line": 2,
      "column": 43
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 2,
      "column": 44
    },
    {
      "type": "procedure",
      "content": "procedure",
      "line": 4,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "name4",
      "line": 4,
      "column": 10
    },
    {
      "type": "left-parenthesis",
      "content": "(",
      "line": 4,
      "column": 15
    },
    {
      "type": "const",
      "content": "const",
      "line": 4,
      "column": 16
    },
    {
      "type": "internal-name",
      "content": "a",
      "line": 4,
      "column": 22
    },
    {
      "type": "comma",
      "content": ",",
      "line": 4,
      "column": 23
    },
    {
      "type": "internal-name",
      "content": "b",
      "line": 4,
      "column": 25
    },
    {
      "type": "colon",
      "content": ":",
      "line": 4,
      "column": 27
    },
    {
      "type": "integer-word",
      "content": "integer",
      "line": 4,
      "column": 29
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 4,
      "column": 36
    },
    {
      "type": "var",
      "content": "var",
      "line": 4,
      "column": 38
    },
    {
      "type": "internal-name",
      "content": "c",
      "line": 4,
      "column": 42
    },
    {
      "type": "colon",
      "content": ":",
      "line": 4,
      "column": 44
    },
    {
      "type": "integer-word",
      "content": "integer",
      "line": 4,
      "column": 46
    },
    {
      "type": "right-parenthesis",
      "content": ")",
      "line": 4,
      "column": 53
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 4,
      "column": 54
    },
    {
      "type": "begin",
      "content": "begin",
      "line": 5,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "c",
      "line": 6,
      "column": 0
    },
    {
      "type": "assign",
      "content": ":=",
      "line": 6,
      "column": 2
    },
    {
      "type": "number",
      "content": "10",
      "line": 6,
      "column": 5
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 6,
      "column": 7
    },
    {
      "type": "begin",
      "content": "begin",
      "line": 7,
      "column": 0
    },
    {
      "type": "while",
      "content": "while",
      "line": 8,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "a",
      "line": 8,
      "column": 6
    },
    {
      "type": "comparsion-operator",
      "content": "\u003E",
      "line": 8,
      "column": 8
    },
    {
      "type": "internal-name",
      "content": "b",
      "line": 8,
      "column": 10
    },
    {
      "type": "logic-operator",
      "content": "or",
      "line": 8,
      "column": 12
    },
    {
      "type": "boolean",
      "content": "false",
      "line": 8,
      "column": 15
    },
    {
      "type": "logic-operator",
      "content": "or",
      "line": 8,
      "column": 21
    },
    {
      "type": "number",
      "content": "4",
      "line": 8,
      "column": 24
    },
    {
      "type": "arithmetic-operator",
      "content": "-",
      "line": 8,
      "column": 25
    },
    {
      "type": "number",
      "content": "5",
      "line": 8,
      "column": 26
    },
    {
      "type": "comparsion-operator",
      "content": "\u003C\u003E",
      "line": 8,
      "column": 27
    },
    {
      "type": "number",
      "content": "2",
      "line": 8,
      "column": 29
    },
    {
      "type": "do",
      "content": "do",
      "line": 8,
      "column": 31
    },
    {
      "type": "internal-name",
      "content": "c",
      "line": 9,
      "column": 0
    },
    {
      "type": "assign",
      "content": ":=",
      "line": 9,
      "column": 2
    },
    {
      "type": "internal-name",
      "content": "a",
      "line": 9,
      "column": 5
    },
    {
      "type": "arithmetic-operator",
      "content": "\u002B",
      "line": 9,
      "column": 7
    },
    {
      "type": "internal-name",
      "content": "b",
      "line": 9,
      "column": 9
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 9,
      "column": 10
    },
    {
      "type": "end",
      "content": "end",
      "line": 10,
      "column": 0
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 10,
      "column": 3
    },
    {
      "type": "end",
      "content": "end",
      "line": 11,
      "column": 0
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 11,
      "column": 3
    },
    {
      "type": "begin",
      "content": "begin",
      "line": 13,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "name4",
      "line": 14,
      "column": 0
    },
    {
      "type": "left-parenthesis",
      "content": "(",
      "line": 14,
      "column": 5
    },
    {
      "type": "number",
      "content": "10",
      "line": 14,
      "column": 6
    },
    {
      "type": "comma",
      "content": ",",
      "line": 14,
      "column": 8
    },
    {
      "type": "number",
      "content": "20",
      "line": 14,
      "column": 10
    },
    {
      "type": "comma",
      "content": ",",
      "line": 14,
      "column": 12
    },
    {
      "type": "internal-name",
      "content": "r0eg",
      "line": 14,
      "column": 14
    },
    {
      "type": "right-parenthesis",
      "content": ")",
      "line": 14,
      "column": 18
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 14,
      "column": 19
    },
    {
      "type": "end",
      "content": "end",
      "line": 15,
      "column": 0
    },
    {
      "type": "dot",
      "content": ".",
      "line": 15,
      "column": 3
    }
  ]
}
```

</details>

5. Run the parser with the command `Compilation.Parser.exe /pretty`
The output will be a file named `code.parsed.out` with the following content:
```json
{
  "success": true,
  "errorDesc": null,
  "errorLine": 0,
  "errorColumn": 0,
  "variables": [
    {
      "name": "r0eg",
      "type": "number",
      "constValue": null
    },
    {
      "name": "-PROGNAME-",
      "type": "number",
      "constValue": {
        "name": "dfg",
        "type": "variable"
      }
    }
  ],
  "syntaxTree": {
    "main": [
      {
        "operandLeft": {
          "name": "r0eg",
          "type": "variable"
        },
        "operandRight": {
          "operandLeft": {
            "number": 5,
            "type": "number"
          },
          "operandRight": {
            "operandLeft": {
              "number": 8,
              "type": "number"
            },
            "operandRight": {
              "operandLeft": {
                "operandLeft": {
                  "number": 2,
                  "type": "number"
                },
                "operandRight": {
                  "operandLeft": {
                    "number": 4,
                    "type": "number"
                  },
                  "operandRight": {
                    "number": 2,
                    "type": "number"
                  },
                  "operator": "plus",
                  "type": "operator"
                },
                "operator": "mod",
                "type": "operator"
              },
              "operandRight": {
                "operandLeft": {
                  "number": 7,
                  "type": "number"
                },
                "operandRight": {
                  "number": 5,
                  "type": "number"
                },
                "operator": "minus",
                "type": "operator"
              },
              "operator": "divide",
              "type": "operator"
            },
            "operator": "multiply",
            "type": "operator"
          },
          "operator": "plus",
          "type": "operator"
        },
        "operator": "assign",
        "type": "operator"
      },
      {
        "function": "name4",
        "arguments": [
          {
            "number": 10,
            "type": "number"
          },
          {
            "number": 20,
            "type": "number"
          },
          {
            "name": "r0eg",
            "type": "variable"
          }
        ],
        "type": "call"
      }
    ],
    "functions": [
      {
        "name": "name4",
        "arguments": [
          {
            "name": "a",
            "type": "number",
            "pointer": false,
            "const": true
          },
          {
            "name": "b",
            "type": "number",
            "pointer": false,
            "const": true
          },
          {
            "name": "c",
            "type": "number",
            "pointer": true,
            "const": false
          }
        ],
        "variables": [],
        "body": [
          {
            "operandLeft": {
              "name": "c",
              "type": "variable"
            },
            "operandRight": {
              "number": 10,
              "type": "number"
            },
            "operator": "assign",
            "type": "operator"
          },
          {
            "condition": {
              "operandLeft": {
                "operandLeft": {
                  "name": "a",
                  "type": "variable"
                },
                "operandRight": {
                  "name": "b",
                  "type": "variable"
                },
                "operator": "more",
                "type": "operator"
              },
              "operandRight": {
                "operandLeft": {
                  "value": false,
                  "type": "boolean"
                },
                "operandRight": {
                  "operandLeft": {
                    "operandLeft": {
                      "number": 4,
                      "type": "number"
                    },
                    "operandRight": {
                      "number": 5,
                      "type": "number"
                    },
                    "operator": "minus",
                    "type": "operator"
                  },
                  "operandRight": {
                    "number": 2,
                    "type": "number"
                  },
                  "operator": "non-equal",
                  "type": "operator"
                },
                "operator": "or",
                "type": "operator"
              },
              "operator": "or",
              "type": "operator"
            },
            "body": [
              {
                "operandLeft": {
                  "name": "c",
                  "type": "variable"
                },
                "operandRight": {
                  "operandLeft": {
                    "name": "a",
                    "type": "variable"
                  },
                  "operandRight": {
                    "name": "b",
                    "type": "variable"
                  },
                  "operator": "plus",
                  "type": "operator"
                },
                "operator": "assign",
                "type": "operator"
              }
            ],
            "type": "while"
          }
        ]
      }
    ]
  }
}
```

6. Make sure that the parsing was successful (the `success` field in the file has the value `true`), otherwise you should look at the error message and the place in the code that caused the error.
7. Done!

### Example 2

Source:
```pascal
PROGRAM dfg;

const r0eg = 5  + 8 * 2 mod ( 4 + 2 ) / (7-5);

var result : integer;

procedure name4(const a, b : Integer; var c : Integer);
var int : INTEGER;
begin
	for c := a to 10 do
	begin
		int := a + b;
	end;		
end;

BEGIN
	name4(r0eg, 8-4, result);
end.
```

1. The source code must be placed in a file named `code.pas`
2. Run the lexer with the command `Compilation.Lexer.exe /pretty`
The output will be a file named `code.lex.out` with the following content:

<details>
  <summary>The contents of the code.lex.out file</summary>

```json
{
  "success": true,
  "errorDesc": null,
  "errorLine": 0,
  "errorColumn": 0,
  "tokens": [
    {
      "type": "program",
      "content": "program",
      "line": 0,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "dfg",
      "line": 0,
      "column": 8
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 0,
      "column": 11
    },
    {
      "type": "const",
      "content": "const",
      "line": 2,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "r0eg",
      "line": 2,
      "column": 6
    },
    {
      "type": "comparsion-operator",
      "content": "=",
      "line": 2,
      "column": 11
    },
    {
      "type": "number",
      "content": "5",
      "line": 2,
      "column": 13
    },
    {
      "type": "arithmetic-operator",
      "content": "\u002B",
      "line": 2,
      "column": 16
    },
    {
      "type": "number",
      "content": "8",
      "line": 2,
      "column": 18
    },
    {
      "type": "arithmetic-operator",
      "content": "*",
      "line": 2,
      "column": 20
    },
    {
      "type": "number",
      "content": "2",
      "line": 2,
      "column": 22
    },
    {
      "type": "arithmetic-operator",
      "content": "mod",
      "line": 2,
      "column": 24
    },
    {
      "type": "left-parenthesis",
      "content": "(",
      "line": 2,
      "column": 28
    },
    {
      "type": "number",
      "content": "4",
      "line": 2,
      "column": 30
    },
    {
      "type": "arithmetic-operator",
      "content": "\u002B",
      "line": 2,
      "column": 32
    },
    {
      "type": "number",
      "content": "2",
      "line": 2,
      "column": 34
    },
    {
      "type": "right-parenthesis",
      "content": ")",
      "line": 2,
      "column": 36
    },
    {
      "type": "arithmetic-operator",
      "content": "/",
      "line": 2,
      "column": 38
    },
    {
      "type": "left-parenthesis",
      "content": "(",
      "line": 2,
      "column": 40
    },
    {
      "type": "number",
      "content": "7",
      "line": 2,
      "column": 41
    },
    {
      "type": "arithmetic-operator",
      "content": "-",
      "line": 2,
      "column": 42
    },
    {
      "type": "number",
      "content": "5",
      "line": 2,
      "column": 43
    },
    {
      "type": "right-parenthesis",
      "content": ")",
      "line": 2,
      "column": 44
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 2,
      "column": 45
    },
    {
      "type": "var",
      "content": "var",
      "line": 4,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "result",
      "line": 4,
      "column": 4
    },
    {
      "type": "colon",
      "content": ":",
      "line": 4,
      "column": 11
    },
    {
      "type": "integer-word",
      "content": "integer",
      "line": 4,
      "column": 13
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 4,
      "column": 20
    },
    {
      "type": "procedure",
      "content": "procedure",
      "line": 6,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "name4",
      "line": 6,
      "column": 10
    },
    {
      "type": "left-parenthesis",
      "content": "(",
      "line": 6,
      "column": 15
    },
    {
      "type": "const",
      "content": "const",
      "line": 6,
      "column": 16
    },
    {
      "type": "internal-name",
      "content": "a",
      "line": 6,
      "column": 22
    },
    {
      "type": "comma",
      "content": ",",
      "line": 6,
      "column": 23
    },
    {
      "type": "internal-name",
      "content": "b",
      "line": 6,
      "column": 25
    },
    {
      "type": "colon",
      "content": ":",
      "line": 6,
      "column": 27
    },
    {
      "type": "integer-word",
      "content": "integer",
      "line": 6,
      "column": 29
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 6,
      "column": 36
    },
    {
      "type": "var",
      "content": "var",
      "line": 6,
      "column": 38
    },
    {
      "type": "internal-name",
      "content": "c",
      "line": 6,
      "column": 42
    },
    {
      "type": "colon",
      "content": ":",
      "line": 6,
      "column": 44
    },
    {
      "type": "integer-word",
      "content": "integer",
      "line": 6,
      "column": 46
    },
    {
      "type": "right-parenthesis",
      "content": ")",
      "line": 6,
      "column": 53
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 6,
      "column": 54
    },
    {
      "type": "var",
      "content": "var",
      "line": 7,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "int",
      "line": 7,
      "column": 4
    },
    {
      "type": "colon",
      "content": ":",
      "line": 7,
      "column": 8
    },
    {
      "type": "integer-word",
      "content": "integer",
      "line": 7,
      "column": 10
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 7,
      "column": 17
    },
    {
      "type": "begin",
      "content": "begin",
      "line": 8,
      "column": 0
    },
    {
      "type": "for",
      "content": "for",
      "line": 9,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "c",
      "line": 9,
      "column": 4
    },
    {
      "type": "assign",
      "content": ":=",
      "line": 9,
      "column": 6
    },
    {
      "type": "internal-name",
      "content": "a",
      "line": 9,
      "column": 9
    },
    {
      "type": "to",
      "content": "to",
      "line": 9,
      "column": 11
    },
    {
      "type": "number",
      "content": "10",
      "line": 9,
      "column": 14
    },
    {
      "type": "do",
      "content": "do",
      "line": 9,
      "column": 17
    },
    {
      "type": "begin",
      "content": "begin",
      "line": 10,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "int",
      "line": 11,
      "column": 0
    },
    {
      "type": "assign",
      "content": ":=",
      "line": 11,
      "column": 4
    },
    {
      "type": "internal-name",
      "content": "a",
      "line": 11,
      "column": 7
    },
    {
      "type": "arithmetic-operator",
      "content": "\u002B",
      "line": 11,
      "column": 9
    },
    {
      "type": "internal-name",
      "content": "b",
      "line": 11,
      "column": 11
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 11,
      "column": 12
    },
    {
      "type": "end",
      "content": "end",
      "line": 12,
      "column": 0
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 12,
      "column": 3
    },
    {
      "type": "end",
      "content": "end",
      "line": 13,
      "column": 0
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 13,
      "column": 3
    },
    {
      "type": "begin",
      "content": "begin",
      "line": 15,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "name4",
      "line": 16,
      "column": 0
    },
    {
      "type": "left-parenthesis",
      "content": "(",
      "line": 16,
      "column": 5
    },
    {
      "type": "internal-name",
      "content": "r0eg",
      "line": 16,
      "column": 6
    },
    {
      "type": "comma",
      "content": ",",
      "line": 16,
      "column": 10
    },
    {
      "type": "number",
      "content": "8",
      "line": 16,
      "column": 12
    },
    {
      "type": "arithmetic-operator",
      "content": "-",
      "line": 16,
      "column": 13
    },
    {
      "type": "number",
      "content": "4",
      "line": 16,
      "column": 14
    },
    {
      "type": "comma",
      "content": ",",
      "line": 16,
      "column": 15
    },
    {
      "type": "internal-name",
      "content": "result",
      "line": 16,
      "column": 17
    },
    {
      "type": "right-parenthesis",
      "content": ")",
      "line": 16,
      "column": 23
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 16,
      "column": 24
    },
    {
      "type": "end",
      "content": "end",
      "line": 17,
      "column": 0
    },
    {
      "type": "dot",
      "content": ".",
      "line": 17,
      "column": 3
    }
  ]
}
```

</details>

3. Make sure that the tokenization was successful (the `success` field in the file has the value `true`), otherwise you should look at the error message and the place in the code that caused the error.
4. Convert the contents of the `code.lex.out` file and place it in the `code.lex.in` file. The content of `code.lex.in` should be:

<details>
  <summary>The contents of the code.lex.in file</summary>

```json
{
  "session": "",
  "tokens": [
    {
      "type": "program",
      "content": "program",
      "line": 0,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "dfg",
      "line": 0,
      "column": 8
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 0,
      "column": 11
    },
    {
      "type": "const",
      "content": "const",
      "line": 2,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "r0eg",
      "line": 2,
      "column": 6
    },
    {
      "type": "comparsion-operator",
      "content": "=",
      "line": 2,
      "column": 11
    },
    {
      "type": "number",
      "content": "5",
      "line": 2,
      "column": 13
    },
    {
      "type": "arithmetic-operator",
      "content": "\u002B",
      "line": 2,
      "column": 16
    },
    {
      "type": "number",
      "content": "8",
      "line": 2,
      "column": 18
    },
    {
      "type": "arithmetic-operator",
      "content": "*",
      "line": 2,
      "column": 20
    },
    {
      "type": "number",
      "content": "2",
      "line": 2,
      "column": 22
    },
    {
      "type": "arithmetic-operator",
      "content": "mod",
      "line": 2,
      "column": 24
    },
    {
      "type": "left-parenthesis",
      "content": "(",
      "line": 2,
      "column": 28
    },
    {
      "type": "number",
      "content": "4",
      "line": 2,
      "column": 30
    },
    {
      "type": "arithmetic-operator",
      "content": "\u002B",
      "line": 2,
      "column": 32
    },
    {
      "type": "number",
      "content": "2",
      "line": 2,
      "column": 34
    },
    {
      "type": "right-parenthesis",
      "content": ")",
      "line": 2,
      "column": 36
    },
    {
      "type": "arithmetic-operator",
      "content": "/",
      "line": 2,
      "column": 38
    },
    {
      "type": "left-parenthesis",
      "content": "(",
      "line": 2,
      "column": 40
    },
    {
      "type": "number",
      "content": "7",
      "line": 2,
      "column": 41
    },
    {
      "type": "arithmetic-operator",
      "content": "-",
      "line": 2,
      "column": 42
    },
    {
      "type": "number",
      "content": "5",
      "line": 2,
      "column": 43
    },
    {
      "type": "right-parenthesis",
      "content": ")",
      "line": 2,
      "column": 44
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 2,
      "column": 45
    },
    {
      "type": "var",
      "content": "var",
      "line": 4,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "result",
      "line": 4,
      "column": 4
    },
    {
      "type": "colon",
      "content": ":",
      "line": 4,
      "column": 11
    },
    {
      "type": "integer-word",
      "content": "integer",
      "line": 4,
      "column": 13
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 4,
      "column": 20
    },
    {
      "type": "procedure",
      "content": "procedure",
      "line": 6,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "name4",
      "line": 6,
      "column": 10
    },
    {
      "type": "left-parenthesis",
      "content": "(",
      "line": 6,
      "column": 15
    },
    {
      "type": "const",
      "content": "const",
      "line": 6,
      "column": 16
    },
    {
      "type": "internal-name",
      "content": "a",
      "line": 6,
      "column": 22
    },
    {
      "type": "comma",
      "content": ",",
      "line": 6,
      "column": 23
    },
    {
      "type": "internal-name",
      "content": "b",
      "line": 6,
      "column": 25
    },
    {
      "type": "colon",
      "content": ":",
      "line": 6,
      "column": 27
    },
    {
      "type": "integer-word",
      "content": "integer",
      "line": 6,
      "column": 29
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 6,
      "column": 36
    },
    {
      "type": "var",
      "content": "var",
      "line": 6,
      "column": 38
    },
    {
      "type": "internal-name",
      "content": "c",
      "line": 6,
      "column": 42
    },
    {
      "type": "colon",
      "content": ":",
      "line": 6,
      "column": 44
    },
    {
      "type": "integer-word",
      "content": "integer",
      "line": 6,
      "column": 46
    },
    {
      "type": "right-parenthesis",
      "content": ")",
      "line": 6,
      "column": 53
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 6,
      "column": 54
    },
    {
      "type": "var",
      "content": "var",
      "line": 7,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "int",
      "line": 7,
      "column": 4
    },
    {
      "type": "colon",
      "content": ":",
      "line": 7,
      "column": 8
    },
    {
      "type": "integer-word",
      "content": "integer",
      "line": 7,
      "column": 10
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 7,
      "column": 17
    },
    {
      "type": "begin",
      "content": "begin",
      "line": 8,
      "column": 0
    },
    {
      "type": "for",
      "content": "for",
      "line": 9,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "c",
      "line": 9,
      "column": 4
    },
    {
      "type": "assign",
      "content": ":=",
      "line": 9,
      "column": 6
    },
    {
      "type": "internal-name",
      "content": "a",
      "line": 9,
      "column": 9
    },
    {
      "type": "to",
      "content": "to",
      "line": 9,
      "column": 11
    },
    {
      "type": "number",
      "content": "10",
      "line": 9,
      "column": 14
    },
    {
      "type": "do",
      "content": "do",
      "line": 9,
      "column": 17
    },
    {
      "type": "begin",
      "content": "begin",
      "line": 10,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "int",
      "line": 11,
      "column": 0
    },
    {
      "type": "assign",
      "content": ":=",
      "line": 11,
      "column": 4
    },
    {
      "type": "internal-name",
      "content": "a",
      "line": 11,
      "column": 7
    },
    {
      "type": "arithmetic-operator",
      "content": "\u002B",
      "line": 11,
      "column": 9
    },
    {
      "type": "internal-name",
      "content": "b",
      "line": 11,
      "column": 11
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 11,
      "column": 12
    },
    {
      "type": "end",
      "content": "end",
      "line": 12,
      "column": 0
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 12,
      "column": 3
    },
    {
      "type": "end",
      "content": "end",
      "line": 13,
      "column": 0
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 13,
      "column": 3
    },
    {
      "type": "begin",
      "content": "begin",
      "line": 15,
      "column": 0
    },
    {
      "type": "internal-name",
      "content": "name4",
      "line": 16,
      "column": 0
    },
    {
      "type": "left-parenthesis",
      "content": "(",
      "line": 16,
      "column": 5
    },
    {
      "type": "internal-name",
      "content": "r0eg",
      "line": 16,
      "column": 6
    },
    {
      "type": "comma",
      "content": ",",
      "line": 16,
      "column": 10
    },
    {
      "type": "number",
      "content": "8",
      "line": 16,
      "column": 12
    },
    {
      "type": "arithmetic-operator",
      "content": "-",
      "line": 16,
      "column": 13
    },
    {
      "type": "number",
      "content": "4",
      "line": 16,
      "column": 14
    },
    {
      "type": "comma",
      "content": ",",
      "line": 16,
      "column": 15
    },
    {
      "type": "internal-name",
      "content": "result",
      "line": 16,
      "column": 17
    },
    {
      "type": "right-parenthesis",
      "content": ")",
      "line": 16,
      "column": 23
    },
    {
      "type": "semicolon",
      "content": ";",
      "line": 16,
      "column": 24
    },
    {
      "type": "end",
      "content": "end",
      "line": 17,
      "column": 0
    },
    {
      "type": "dot",
      "content": ".",
      "line": 17,
      "column": 3
    }
  ]
}
```

</details>

5. Run the parser with the command `Compilation.Parser.exe /pretty`
The output will be a file named `code.parsed.out` with the following content:
```json
{
  "success": true,
  "errorDesc": null,
  "errorLine": 0,
  "errorColumn": 0,
  "variables": [
    {
      "name": "r0eg",
      "type": "number",
      "constValue": {
        "operandLeft": {
          "number": 5,
          "type": "number"
        },
        "operandRight": {
          "operandLeft": {
            "number": 8,
            "type": "number"
          },
          "operandRight": {
            "operandLeft": {
              "operandLeft": {
                "number": 2,
                "type": "number"
              },
              "operandRight": {
                "operandLeft": {
                  "number": 4,
                  "type": "number"
                },
                "operandRight": {
                  "number": 2,
                  "type": "number"
                },
                "operator": "plus",
                "type": "operator"
              },
              "operator": "mod",
              "type": "operator"
            },
            "operandRight": {
              "operandLeft": {
                "number": 7,
                "type": "number"
              },
              "operandRight": {
                "number": 5,
                "type": "number"
              },
              "operator": "minus",
              "type": "operator"
            },
            "operator": "divide",
            "type": "operator"
          },
          "operator": "multiply",
          "type": "operator"
        },
        "operator": "plus",
        "type": "operator"
      }
    },
    {
      "name": "result",
      "type": "number",
      "constValue": null
    },
    {
      "name": "-PROGNAME-",
      "type": "number",
      "constValue": {
        "name": "dfg",
        "type": "variable"
      }
    }
  ],
  "syntaxTree": {
    "main": [
      {
        "function": "name4",
        "arguments": [
          {
            "name": "r0eg",
            "type": "variable"
          },
          {
            "operandLeft": {
              "number": 8,
              "type": "number"
            },
            "operandRight": {
              "number": 4,
              "type": "number"
            },
            "operator": "minus",
            "type": "operator"
          },
          {
            "name": "result",
            "type": "variable"
          }
        ],
        "type": "call"
      }
    ],
    "functions": [
      {
        "name": "name4",
        "arguments": [
          {
            "name": "a",
            "type": "number",
            "pointer": false,
            "const": true
          },
          {
            "name": "b",
            "type": "number",
            "pointer": false,
            "const": true
          },
          {
            "name": "c",
            "type": "number",
            "pointer": true,
            "const": false
          }
        ],
        "variables": [
          {
            "name": "int",
            "type": "number",
            "constValue": null
          }
        ],
        "body": [
          {
            "from": {
              "name": "a",
              "type": "variable"
            },
            "to": {
              "number": 10,
              "type": "number"
            },
            "ascending": true,
            "body": [
              {
                "operandLeft": {
                  "name": "int",
                  "type": "variable"
                },
                "operandRight": {
                  "operandLeft": {
                    "name": "a",
                    "type": "variable"
                  },
                  "operandRight": {
                    "name": "b",
                    "type": "variable"
                  },
                  "operator": "plus",
                  "type": "operator"
                },
                "operator": "assign",
                "type": "operator"
              }
            ],
            "type": "for"
          }
        ]
      }
    ]
  }
}
```

6. Make sure that the parsing was successful (the `success` field in the file has the value `true`), otherwise you should look at the error message and the place in the code that caused the error.
7. Done!