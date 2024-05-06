module Eval
open AST

// (*Holds the data from each node in a
// * dictionary, each node name will be a key that map to
// * its data *)
type Env = Map<string,Expr>


(*  Recursive function evalExpr that takes an expression of type Expr and returns a string. *)
let rec evalExpr (expr: Expr): string =
    match expr with
    (* Handle the case where the expression is a Node containing a string and a list of expressions. *)
    | Node (Node_name n, Edge_list ns) ->
        ns (* 'ns' is a list of expressions that are the children or connected nodes of 'n'. *)
        |> List.collect (function  (* Map each element to a new list and concatenate all lists. *)
            | Edge_list ns' -> ns' (* If the element is a Node_list, use its inner list. *)
            | Num n -> [Node_name (string n)] (* If it's a number, convert to string and wrap in Str. *)
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
    

    (*need to add ability to add the string to the environment*)
    | Node_name s -> s  
    | Num n -> string n  
    (*these just dont break things for now*)


    | Exit -> 
        printfn "Thanks for using Twined!"
        exit(0)
    | _ -> failwith "Invalid expression"  



// type Expr =
// | Node of Expr * Expr
// | Edge_list of Expr list
// | Node_name of string
// | Num of int
// | Assignment of Expr * Expr 
// | Node_info of string
// | Exit

(*adding ability to tract the environment, doesnt work yet*)
// let rec eval (e: Expr)(env: Env) : string * Env =
//     match e with

//     | Node (Node_name n, Edge_list ns) ->
//         ns (* 'ns' is a list of expressions that are the children or connected nodes of 'n'. *)
//         |> List.collect (function  (* Map each element to a new list and concatenate all lists. *)
//             | Edge_list ns' -> ns' (* If the element is a Node_list, use its inner list. *)
//             | Num n -> [Node_name (string n)] (* If it's a number, convert to string and wrap in Str. *)
//             | Node_name s -> [Node_name s] (* If it's a string, wrap it in Str to maintain type consistency. *)
//             | _ -> failwith "Invalid expression" 
//         )
       
//         |> List.map (fun (node, env) -> sprintf "\"%s\" -> \"%s\";" n (eval node, env)) (* Format each node connection as a DOT graph edge. *)
//         |> String.concat "\n" |> (,env)

//     | Num _ -> e, env
    
//     | Node_info v ->
//         if Map.containsKey v env then
//             let value = env[v]
//             value, env
//         else
//             printfn "Undefined variable."
//             exit 1
//     | Assignment (lhs, rhs) ->
//         match lhs with
//         | Variable v ->
//             let rhsr, env1 = eval rhs env
//             let env2 = env1.Add (v, rhsr)
//             rhsr, env2
//         | _ ->
//             printfn "Left hand side of an assignment must be a variable."
//             exit 1
//     | Plus (lhs, rhs) ->
//         let lhsr, env1 = eval lhs env
//         let rhsr, env2 = eval rhs env1
//         match lhsr, rhsr with
//         | Num n1, Num n2 -> Num (n1 + n2), env2
//         | _ ->
//             printfn "Invalid operation. Plus requires numeric operands."
//             exit 1
//     | Print e ->
//         let er, env1 = eval e env
//         printfn "%s" (prettyprint er)
//         er, env1
//     | Sequence es ->
//         match es with
//         | [] ->
//             printfn "Empty sequence not allowed."
//             exit 1
//         | [e] -> eval e env
//         | e::es2 ->
//             let _, env1 = eval e env
//             let s = Sequence es2
//             eval s env1
//     | Exit -> 
//         printfn "Thanks for using Twined!"
//         exit(0)
//     | _ -> failwith "Invalid expression" 