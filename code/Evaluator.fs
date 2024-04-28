module Eval
open AST
// open Combinator
// open System

// (*Holds the data from each node in a
// * dictionary, each node name will be a key that map to
// * its data *)
// type Env = Map<string,Expr>

(*  Recursive function evalExpr that takes an expression of type Expr and returns a string. *)
let rec evalExpr (expr: Expr) : string =
    match expr with
    (* Handle the case where the expression is a Node containing a string and a list of expressions. *)
    | Node (Str n, Edge_list ns) ->
        ns (* 'ns' is a list of expressions that are the children or connected nodes of 'n'. *)
        |> List.collect (function  (* Map each element to a new list and concatenate all lists. *)
            | Edge_list ns' -> ns' (* If the element is a Node_list, use its inner list. *)
            | Num n -> [Str (string n)] (* If it's a number, convert to string and wrap in Str. *)
            | Str s -> [Str s] (* If it's a string, wrap it in Str to maintain type consistency. *)
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
    | Str s -> s  
    | Num n -> string n  
    | _ -> failwith "Invalid expression"  


(* Commands to be Used
    Example:
        dotnet run plant.txt [debug]
        dot -Tsvg gv.txt -o plant_simple_graph.svg
*)
