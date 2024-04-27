module Par
//open AST
open Combinator

type Expr =
| Node of Expr * Expr
| Node_list of Expr list
| Str of string
| Num of int
| EString of string
| Variable of string
| Assignment of Expr * Expr
| Plus of Expr * Expr
| Print of Expr


(* pad p
 *   Parses p, surrounded by optional whitespace.
 *)
let pad p = pbetween pws0 p pws0

(*a parser allows later definition of expr*)
let expr, exprImpl = recparser()


let node_name = pmany1 (pletter <|> pdigit) |>> stringify |>> Str

let pad_node_name = pad node_name

let node_in_list : Parser<Expr> = (pleft pad_node_name  (pmany0 (pchar ','))) <!> "node in list"

(*a parser that parses abstractions taking the form (L<var>.<expr>)*)
let node_list : Parser<Expr> = pbetween
                                        (pstr "(")
                                        (pseq
                                            (pleft pad_node_name  (pchar ','))
                                            (pmany0 node_in_list)
                                            (fun (c,cs) -> Node_list(c::cs))
                                        )
                                        (pchar ')') <!> "node list"

let pad_node_list = pad node_list

(*parses a single node*)
let node: Parser<Expr> = 
    pbetween
        (pstr "{")
        (pseq
            (pleft pad_node_name  (pchar ','))
            pad_node_list
            (fun (c, cs) -> Node(c, cs))
        )
        (pchar '}') <!> "node"

let pad_node = pad node <!> "pad_node"

let pad_list_of_nodes: Parser<Expr> = pad (pmany1 pad_node) |>> Node_list <!> "list of nodes"


exprImpl := pad_list_of_nodes <|> pad_node_name <|> pad_node <|> pad_node_list 

let grammar = pleft expr peof


let parse (input: string)(do_debug: bool) : Expr option =
    let i = (if do_debug then debug else prepare) input
    match grammar i with
    | Success(ast,_) -> Some ast
    | Failure(_,_)   -> None

// let parse (s:string) : Expr option =
//     match grammar (prepare s) with 
//     | Success(ex,_) -> Some ex
//     | Failure(_,_) -> None