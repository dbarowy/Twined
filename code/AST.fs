module AST

type Expr =
| Node of Expr * Expr
| Edge_list of Expr list
| Node_name of string
| Num of int
| Node_info of string
| Exit

