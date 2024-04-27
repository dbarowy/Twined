module AST

// type Expr =
// |Node_list of Expr list
// |Node of string * Expr
type Expr =
| Node of string * Expr
| Node_list of string list
| Str of string
| Num of int
| EString of string
| Variable of string
| Assignment of Expr * Expr
| Plus of Expr * Expr
| Print of Expr
| Sequence of Expr list

