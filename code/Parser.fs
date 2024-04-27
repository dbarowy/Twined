module Par
//open AST
open Combinator

type Expr =
| Node of string * string list
| Node_list of string list
| Abstraction of char * Expr 


(*a parser that takes either a variable, abstraction or application parser*)
let expr, exprImpl = recparser()

let node_name = pmany1 (pletter <|> pdigit) |>> stringify //

(*parses a single node*)
let node: Parser<Expr> = 
    pbetween
        (pstr "{")
        (pseq
            (pleft node_name  (pchar ','))
            expr
            (fun c -> Node(c))
        )
        (pchar '}')


// (*trying to parse a list of nodes are connected to the starting node*)
// let node_in_list: Parser<Expr> = 
//     (pseq
//         (pleft node  (pchar ','))
//         (expr)
//         (fun c -> n)
//     )
                                    
                                    // pmany1 (pletter <|> pdigit) |>> stringify |>> (fun s -> Node(s,[]))

(*a parser that parses abstractions taking the form (L<var>.<expr>)*)
let node_list : Parser<Expr> = pbetween
                                        (pstr "(")
                                        (pseq
                                            (pleft node_name  (pchar ','))
                                            expr
                                            (fun (c,cs) -> Node_list(c::cs))
                                        )
                                        (pchar ')')

(*here as a template*)
let application : Parser<Expr> =  pbetween
                                        (pstr "(")
                                        (pseq
                                            expr
                                            expr
                                            (fun c -> Application(c))
                                        )
                                        (pchar ')')

(*here as a template*)
exprImpl := graph_list <|> abstraction <|> application


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


