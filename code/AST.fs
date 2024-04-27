module AST

type Expr =
|Node of string * Expr list
|Graph of string

