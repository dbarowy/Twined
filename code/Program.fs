open Par
open Eval
open AST
open Combinator
open System.IO

[<EntryPoint>]
let main argv =
    if argv.Length <> 1 && argv.Length <> 2 then
        printfn "Usage: dotnet run <file> [debug]"
        exit 1

    (* read in the input file *)
    let file = argv.[0]
    let input = File.ReadAllText file

    (* does the user want parser debugging turned on? *)
    let do_debug = if argv.Length = 2 then true else false

    let ast = parse input do_debug

    match ast with
    | Some ast ->
        let gvText = evalExpr ast
        File.WriteAllText("gv.txt", gvText)
        printfn "Graph representation written to gv.txt"
        
        (* To generate the SVG file, we can use the following command:
         dot gv.txt -Tsvg > graph.svg *)
    | None ->
        printfn "Failed to parse input"

    0