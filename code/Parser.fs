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
let node_name = pmany1 (pletter <|> pdigit <|> pchar ' ') |>> stringify |>> Node_name

(*allows whitespace infront of or behind a nodename*)
let pad_node_name = pad node_name

// (*reads in a series of strings separated by spaces or "," and put them in a list of connections*)
// let node_in_list : Parser<Expr> = (pleft pad_node_name  (pmany0 (pchar ','))) <!> "node in list"
let node_in_list : Parser<Expr> = (pleft pad_node_name  (pchar ',')) <!> "node in list"

(*parses a list of nodes that share an edge with the node associated with the list*)
let edge_list : Parser<Expr> = pbetween
                                         (pstr "(")
                                        (pseq
                                            (pleft pad_node_name  (pchar ','))
                                            // (pleft pad_node_name  (pmany0 (pchar ',')))
                                            (pmany0 node_in_list)
                                            (fun (c,cs) -> Edge_list(c::cs))
                                         ) //<|> (pchar '0') |>> int |>> Num) trying to make lists empty
                                         (pchar ')') <!> "node list"

(*pads a node list to allow for whitespace*)
let pad_edge_list = pad edge_list

(*parses a single node to see the name of the node and the names of the nodes it is connected to*)
let node: Parser<Expr> = 
    pbetween
        (pstr "{")
        (pseq
            (pleft pad_node_name  (pchar ','))
            pad_edge_list
            (fun (c, cs) -> Node(c, cs))
        )
        (pchar '}') <!> "node"



(*allows whitespace before and after a node*)
let pad_node = pad node <!> "pad_node"

(*contains all characters allowed for use in Node_info*)
let allowed_chars = 
    pletter <|> pdigit <|> pchar ' ' <|> pchar '?' <|> pchar '!' <|> pchar '@' <|> pchar '#'
    <|> pchar '$' <|> pchar '%' <|> pchar '&' <|> pchar '*' <|> pchar '(' <|> pchar ')'<|> pchar '_'
    <|> pchar '-' <|> pchar '+' <|> pchar '=' <|> pchar '{' <|> pchar '}' <|> pchar '[' <|> pchar ']'
    <|> pchar '|'  <|> pchar ':' <|> pchar ':' <|> pchar '"' <|> pchar ''' <|> pchar '<'  <|> pchar '>'
    <|> pchar '>' <|> pchar ',' <|> pchar '.'  <|> pchar '/'  <|> pchar '~'  <|> pchar '\n'  <|> pchar '`'
    <|> pchar '~'

(*Any number of allowed characters*)
let str = pmany1 allowed_chars

(*converts the string to the Node_info type*)
let node_info : Parser<Expr> = str |>> stringify |>> (fun s -> Node_info s)

(*Pads node info*)
let pad_node_info = pad node_info

(*assigns some info in the environment to the nodename*)
let assignment : Parser<Expr> = 
    pbetween
        (pstr"^^")
        (pseq
            (pleft pad_node_name (pad (pstr ":=")))
            pad_node_info
            (fun (v, st) -> Assignment(v, st))
        )
        (pstr"^^") <!> "passign"
    
(*Pads assignment*)
let pad_assignment = pad assignment

(*Allows user to exit*)
let exit = pstr "exit()" |>> (fun _ -> Exit) <!> "exit"

(*parses a list of one or more nodes in a graph*)
let pad_list_of_nodes: Parser<Expr> = pad (pmany1 pad_node) |>> Edge_list <!> "list of nodes"

exprImpl := exit <|> pad_list_of_nodes <|> pad_node_name <|> pad_node <|> pad_edge_list <|> pad_assignment

(*defines how language can be interpreted*)
let grammar = pleft expr peof

(*parses a string to determine if the grammar is followed, if yes returns an AST if not returns none*)
let parse (input: string)(do_debug: bool) : Expr option =
    let i = (if do_debug then debug else prepare) input
    match grammar i with
    | Success(ast,_) -> Some ast
    | Failure(_,_)   -> None