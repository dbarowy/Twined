module AST

type Expr =
| Node of Expr * Expr
| Node_list of Expr list
| Str of string

