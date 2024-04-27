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


(*a parser allows later definition of expr*)
let expr, exprImpl = recparser()


let node_name = pmany1 (pletter <|> pdigit) |>> stringify |>> Str

(*a parser that parses abstractions taking the form (L<var>.<expr>)*)
let node_list : Parser<Expr> = pbetween
                                        (pstr "(")
                                        (pseq
                                            (pleft node_name  (pchar ','))
                                            expr
                                            (fun c -> Node(c))
                                        )
                                        (pchar ')')

(*parses a single node*)
let node: Parser<Expr> = 
    pbetween
        (pstr "{")
        (pseq
            (pleft node_name  (pchar ','))
            node_list
            (fun (c, cs) -> Node(c, cs))
        )
        (pchar '}')


exprImpl := node <|> node_list


(* pad p
 *   Parses p, surrounded by optional whitespace.
 *)
let pad p = pbetween pws0 p pws0

let graph = pad (node)

let grammar = pleft graph peof

let parse (s:string) : Expr option =
    match grammar (prepare s) with 
    | Success(ex,_) -> Some ex
    | Failure(_,_) -> None