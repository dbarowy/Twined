module Par
//open AST
open Combinator

type Expr =
| Node of string * Expr list
| Variable of char 
| Abstraction of char * Expr 
| Application of Expr * Expr

(*a parser that takes either a variable, abstraction or application parser*)
let expr, exprImpl = recparser()

(*parses a node*)
let node: Parser<Expr> = pmany1 (pletter <|> pdigit) |>> stringify |>> (fun s -> Node(s,[]))

let node_list: Parser<Expr> = failwith "1"

(*a parser that parses abstractions taking the form (L<var>.<expr>)*)
let graph_list : Parser<Expr> = pbetween
                                        (pstr "(")
                                        (pseq
                                            (pleft node  (pchar ':'))
                                            (expr)
                                            (fun c -> Abstraction(c))
                                        )
                                        (pchar ')')

(*a parser that pares applications given in the form (<expr><expr>)*)
let application : Parser<Expr> =  pbetween
                                        (pstr "(")
                                        (pseq
                                            expr
                                            expr
                                            (fun c -> Application(c))
                                        )
                                        (pchar ')')

(*the implimentation of the expression parser*)
exprImpl := variable <|> abstraction <|> application


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


