# Компилятор для подмножества языка Паскаль
[![License](https://img.shields.io/badge/LICENSE-LGPL%20v2.1-green?style=flat-square)](/LICENSE)  [![Version](https://img.shields.io/badge/VERSION-RELEASE%20--%201.0-green?style=flat-square)](https://github.com/averov90/Pascal-Compiler/releases)
### :small_orange_diamond: [English version](/README-eng.md)

В данном репозитории представлен лексер и парсер для подмножества языка программирования Pascal. 
И лексер, и парсер сообщают о синтаксических и лексических соответственно, указывая место в исходном коде (строку и столбец).

Полноценный компилятор состоит из трёх частей (если выполнять деление по уже показанному принципу): *лексер*, *парсер*, *генератор кода*. 
В данном репозитории представлены только первые 2 компонента. Это оставляет открытым вопрос конечной формы кода, то есть во что будет преобразован код на языке Паскаль в конечном счёте.

После работы, парсер создаёт Абстрактной Синтаксическое Дерево (АСД), которое является представлением логики работы исходной программы.
Далее АСД можно использовать как угодно: начиная от генерации кода на любом языке (ассемблере, машинном коде, python) и заканчивая исполнением на месте, то есть, интерпретацией.
В последнем случае получается не компилятор, а интерпретатор.

Все части компилятора (лексер и парсер в данном случае) имеют протокол "общения", который для удобства был сведён к формату JSON. Это позволит при необходимости работать с взаимодействием через сетевые протоколы, что является прямой дорогой к микросервисам.

## Поддерживаемые возможности Pascal

* распознавание символа program (задание имени программы)
* объявление глобальных переменных целочисленного типа (integer) в самом верху программы (после точки входа или процедуры глобальные var запрещены)
* объявление локальныех переменных целочисленного типа (integer) в процедуре
* поддержка любой вложенности блоков кода begin/end.
* поддержка циклов for (с поддержкой «to» и «downto»)
* поддержка циклов while
* числовые операции бинарные: +, -, \*, / (он же div т.к. нету real), mod, унарный минус
* логические операции: and, or
* операторы сравнения: =, >, <, <>, <= и >=
* неизменяемые (const) var-значения (и локальные и глобальные)
* процедуры (без возврата значения через return, но с поддержкой возврата значения через аргумент)
* ключевое слово exit
* pre-defined процедура writeln

## Лексер

Входные данные для работы лексера должны иметь следующий формат:
```json
{
  "session": "id",
  "source": [ "text data" ]
}
```

* **session** - идентификатор сессии, который выдаётся менеджером (может быть использован для идентификации пайплайна обработки кода).
* **source** - текст (код), который ввёл пользователь в поле ввода кода. В лексер текст передаётся без каких-либо изменений.

Выходные данные лексера имеют следующий формат:
```json
{
  "success": true,
  "errorDesc": "text",
  "errorLine": 10,
  "errorColumn": 4,
  "tokens": []
}
```

* **success** - true или false - успешность выполнения операции извлечения токенов. Если false, значит произошла ошибка.
* **errorDesc** - нужно только если success=false - это описание ошибки, например "Встречен неожиданный символ. Ожидался символ ;"
* **errorLine** - может быть (может и не быть) только если success=false - это номер строки, в которой произошла ошибка. Номер строки должен быть из оригинального текста, а не отфильтрованного текста из лексера.
* **errorColumn** - может быть (может и не быть) только если success=false - это номер символа в строке, который является ошибкой. Номер символа должен быть из оригинального текста, а не отфильтрованного текста из лексера.
* **tokens** - должно быть только при success=true - список извлечённых из программы токенов.

Лексер выполняет разделение кода на следующие токены (группы):
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

## Парсер

Входные данные для работы лексера должны иметь следующий формат:
```json
{
  "session": "id",
  "tokens": []
}
```

* **session** - идентификатор сессии, который выдаётся менеджером (может быть использован для идентификации пайплайна обработки кода).
* **tokens** - список токенов.

Выходные данные лексера имеют следующий формат:
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

* **success** - true или false - успешность выполнения операции построения синтаксического дерева. Если false, значит произошла ошибка.
* **errorDesc** - нужно только если success=false - это описание ошибки, например "Попытка изменить значение константы."
* **errorLine** - может быть (может и не быть) только если success=false - это номер строки, в которой произошла ошибка.
* **errorColumn** - может быть (может и не быть) только если success=false - это номер символа в строке, который является ошибкой.
* **variables** - требуется только если success=true - список глобальных переменных, которые используются в программе (с указанием типа). Например: "variables": [ { "name": "someData", "type": "integer", "constValue": null } ]
* **syntaxTree** - требуется только если success=true - абстрактное синтаксическое дерево (НЕ бинарное), которое имеет рекурсивную структуру.

### Вложенные узлы дерева

Категория 1:
```json
{
  "type": "operator"
}
```

Категория 2:
```json
{
  "type": "number"
}
```

Категория 3:
```json
{
  "type": "boolean"
}
```

Категория 4:
```json
{
  "type": "call"
}
```

Категория 5:
```json
{
  "type": "variable"
}
```

Категория 6:
```json
{
  "type": "for"
}
```

Категория 7:
```json
{
  "type": "while"
}
```

#### Категория 1 - подробно
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

#### Категория 2 - подробно
```json
{
  "type": "number",
  "number": 1234
}
```

#### Категория 3 - подробно
```json
{
  "type": "boolean",
  "value": true
}
```

#### Категория 4 - подробно
```json
{
  "type": "call",
  "function": "",
  "arguments": [
    {
      <!--Это комментарий. В этом объекте могут попасться только вычислимые значения: арифметический оператор, число или переменная. Даже boolean попасться не может.-->
    }
  ]
}
```

#### Категория 5 - подробно
```json
{
  "type": "variable",
  "name": ""
}
```

#### Категория 6 - подробно
```json
{
  "type": "for",
  "from": {},   <!--Это комментарий. В этом объекте могут попасться только вычислимые значения: арифметический оператор, число или переменная. Даже boolean попасться не может.-->
  "to": {},  <!--Это комментарий. В этом объекте могут попасться только вычислимые значения: арифметический оператор, число или переменная. Даже boolean попасться не может.-->
  "ascending": true,
  "body": [
    {}  <!--Это комментарий. В этом объекте может попасться всё что угодно.-->
  ]
}
```

#### Категория 7 - подробно
```json
{
  "type": "while",
  "condition": {}, <!--Это комментарий. В этом объекте могут попасться только логические значения: boolean, логический оператор или оператор сравнения.-->
  "body": [
    {}  <!--Это комментарий. В этом объекте может попасться всё что угодно.-->
  ]
}
```

### Не-вложенные узлы дерева
```json
{
  "main": [
    {} <!--Это комментарий. В этом объекте может попасться всё что угодно.-->
  ],
  "procedures": [
    {
      "name": "",
      "arguments": [
        {
          "type": "integer",
          "name": "", <!--Это имя - одновременно и объявление-->
          "pointer": true, <!--pointer==true и const==true не могут быть одновременно-->
		  "const": false
        }
      ],
      "variables": [],  <!--Здесь может быть такое { "name": "", "type": "integer", "constValue": null } -->
      "body": [
        {} <!--Это комментарий. В этом объекте может попасться всё что угодно.-->
      ]
    }
  ]
}
```

## Примеры

### Пример 1 работы составляющих компилятора

Исходный код:
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

1. Исходный код нужно поместить в файл с именем `code.pas`
2. Запустить лексер командой `Compilation.Lexer.exe /pretty`
На выходе получится файл с именем `code.lex.out` и следующим содержимым:

<details>
  <summary>Содержимое файла code.lex.out</summary>
  
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

3. Убедиться, что токенизация прошла успешно (в файле поле `success` имеет значение `true`), иначе следует посмотреть сообщение об ошибке и место в коде, вызвавшее ошибку.
4. Переделать файл содержимое файла `code.lex.out` и поместить в файл `code.lex.in`. Содержимое `code.lex.in` должно быть следующим:

<details>
  <summary>Содержимое файла code.lex.in</summary>
  
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

5. Запустить парсер командой `Compilation.Parser.exe /pretty`
На выходе получится файл с именем `code.parsed.out` и следующим содержимым:
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

6. Убедиться, что парсинг прошёл успешно (в файле поле `success` имеет значение `true`), иначе следует посмотреть сообщение об ошибке и место в коде, вызвавшее ошибку.
7. Готово!

### Пример 2 работы составляющих компилятора

Исходный код:
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

1. Исходный код нужно поместить в файл с именем `code.pas`
2. Запустить лексер командой `Compilation.Lexer.exe /pretty`
На выходе получится файл с именем `code.lex.out` и следующим содержимым:

<details>
  <summary>Содержимое файла code.lex.out</summary>

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

3. Убедиться, что токенизация прошла успешно (в файле поле `success` имеет значение `true`), иначе следует посмотреть сообщение об ошибке и место в коде, вызвавшее ошибку.
4. Переделать файл содержимое файла `code.lex.out` и поместить в файл `code.lex.in`. Содержимое `code.lex.in` должно быть следующим:

<details>
  <summary>Содержимое файла code.lex.in</summary>

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

5. Запустить парсер командой `Compilation.Parser.exe /pretty`
На выходе получится файл с именем `code.parsed.out` и следующим содержимым:
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

6. Убедиться, что парсинг прошёл успешно (в файле поле `success` имеет значение `true`), иначе следует посмотреть сообщение об ошибке и место в коде, вызвавшее ошибку.
7. Готово!