module Eval
open AST

// (*Holds the data from each node in a
// * dictionary, each node name will be a key that map to
// * its data *)
type Env = Map<string,string>


(*  Recursive function evalExpr that takes an expression of type Expr and returns a string. *)
let rec evalExpr (expr: Expr): string =
    match expr with
    (* Handle the case where the expression is a Node containing a string and a list of expressions. *)
    | Node (Node_name n, Edge_list ns) ->
        ns (* 'ns' is a list of expressions that are the children or connected nodes of 'n'. *)
        |> List.collect (function  (* Map each element to a new list and concatenate all lists. *)
            // | Edge_list ns' -> ns' (* If the element is a Node_list, use its inner list. *)
            | Node_name s -> [Node_name s] (* If it's a string, wrap it in Str to maintain type consistency. *)
            | _ -> failwith "Invalid expression" 
        )
       
        |> List.map (fun node -> sprintf "\"%s\" -> \"%s\";" n (evalExpr node)) (* Format each node connection as a DOT graph edge. *)
        |> String.concat "\n" (* Concatenate all formatted strings with new lines between them. *)
    
    (* Handle the case where the expression is a list of nodes. *)
    | Edge_list ns ->
        (* Begin a DOT graph declaration. *)
        "digraph G {\n" + 
        (* Recursively evaluate each expression in the list and concatenate them, separated by new lines. *)
        (String.concat "\n" (List.map evalExpr ns)) +
        (* Close the DOT graph declaration. *)
        "\n}"
    

    // (*need to add ability to add the string to the environment*)
    | Node_name s -> s   
    // (*these just dont break things for now*)


    | Exit -> 
        printfn "Thanks for using Twined!"
        exit(0)
    | _ -> failwith "Invalid expression"  


let rec convert (x:string) (xs: Expr list): string =
    xs (* 'ns' is a list of expressions that are the children or connected nodes of 'n'. *)
        |> List.collect (function  (* Map each element to a new list and concatenate all lists. *)
            // | Edge_list ns' -> ns' (* If the element is a Node_list, use its inner list. *)
            | Node_name s -> [Node_name s] (* If it's a string, wrap it in Str to maintain type consistency. *)
            | _ -> failwith "Invalid expression" 
        )
        // |> List.map (fun node -> sprintf "\"%s\" -> \"%s\";" x (convert x node)) (* Format each node connection as a DOT graph edge. *)
        |> List.map (fun node -> sprintf "\"%s\" -> \"%s\";" x (evalExpr node)) (* Format each node connection as a DOT graph edge. *)
        |> String.concat "\n"
    
let rec separate_node_list (xs) =
    (String.concat "\n" (List.map evalExpr xs))
    

let rec eval (expr: Expr) (envi: Env): string * Env =
    match expr with
    | Nodes_and_Assignments (nodes, assignments) ->
        let str, _ =eval nodes envi
        let _, envi1 = eval assignments envi
        (str,envi1)

    | Assignment_list assignments ->
        match assignments with
        | x::xs -> 
            let _, envi1 =eval x envi
            eval (Assignment_list(xs)) envi1

        | [] -> " ", envi

    (* converts a node into a useful graph*)
    | Node (Node_name n, Edge_list ns) ->
        let edges = convert n ns
        (edges, envi)

    | Edge_list edges ->
        let nodes = (separate_node_list edges)
        (* a DOT graph declaration. *)
        let ret = "digraph G {\n" + nodes + "\n}"
        (ret, envi)
    
    (* a node name should never be encountered outside of the 
       other types that use it*)
    | Node_name name ->
        failwith "1"
    
    (* adding a node_name and its info to the environment, returns the variable*)
    | Assignment (lhs, rhs) ->
        match lhs, rhs with
        | Node_name v, Node_info s ->
            let env2 = envi.Add (v, s)
            (v, env2) 
        | _,_ ->
            printfn "Left hand side of an assignment must be a variable."
            exit 1

    | Node_info info ->
        (info, envi)
    | Exit -> 
        printfn "Thanks for using Twined!"
        exit(0)
    | _ -> 
        failwith "Invalid Expression"
 