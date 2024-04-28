module AST

type Expr =
| Node of Expr * Expr
| Edge_list of Expr list
| Str of string
| Num of int

