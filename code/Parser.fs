module Par
open AST
open Combinator


let expr = pmany0 (pstr "aa")

(* pad p
 *   Parses p, surrounded by optional whitespace.
 *)
let pad p = pbetween pws0 p pws0

let graph = pad (pstr "aa")

let g_expr = pright graph expr

let grammar = pleft g_expr peof

let parse (input:string) : Expr option =
    match grammar (prepare input) with
    |Success(ast,_) -> Some ast
    |Failure(_,_) -> None


