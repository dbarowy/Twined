module AST

type Expr =
|Node_list of Expr list
|Node of string * Expr

