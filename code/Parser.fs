module Par
open AST
open Combinator

(* pad p
 *   Parses p, surrounded by optional whitespace.
 *)
let pad p = pbetween pws0 p pws0

(*a parser allows later definition of expr*)
let expr, exprImpl = recparser()

(*takes a series of characters or strings as the name for a node*)
let node_name = pmany1 (pletter <|> pdigit) |>> stringify |>> Str

(*allows whitespace infront of or behind a nodename*)
let pad_node_name = pad node_name

(*reads in a series of strings separated by spaces or "," and put them in a list of connections*)
let node_in_list : Parser<Expr> = (pleft pad_node_name  (pmany0 (pchar ','))) <!> "node in list"

(*parses a list of nodes that share an edge with the node associated with the list*)
let node_list : Parser<Expr> = pbetween
                                        (pstr "(")
                                        (pseq
                                            (pleft pad_node_name  (pmany0 (pchar ',')))
                                            (pmany0 node_in_list)
                                            (fun (c,cs) -> Node_list(c::cs))
                                        ) //<|> (pchar '0') |>> int |>> Num) trying to make lists empty
                                        (pchar ')') <!> "node list"

(*pads a node list to allow for whitespace*)
let pad_node_list = pad node_list

(*parses a single node to see the name of the node and the names of the nodes it is connected to*)
let node: Parser<Expr> = 
    pbetween
        (pstr "{")
        (pseq
            (pleft pad_node_name  (pchar ','))
            pad_node_list
            (fun (c, cs) -> Node(c, cs))
        )
        (pchar '}') <!> "node"

(*allows whitespace before and after a node*)
let pad_node = pad node <!> "pad_node"

(*parses a list of one or more nodes in a graph*)
let pad_list_of_nodes: Parser<Expr> = pad (pmany1 pad_node) |>> Node_list <!> "list of nodes"

exprImpl := pad_list_of_nodes <|> pad_node_name <|> pad_node <|> pad_node_list 

(*defines how language can be interpreted*)
let grammar = pleft expr peof

(*parses a string to determine if the grammar is followed, if yes returns an AST if not returns none*)
let parse (input: string)(do_debug: bool) : Expr option =
    let i = (if do_debug then debug else prepare) input
    match grammar i with
    | Success(ast,_) -> Some ast
    | Failure(_,_)   -> None