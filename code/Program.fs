open Par
open Eval 
open AST
open Combinator

[<EntryPoint>]
let main argv = 

    if argv.Length <> 1 && argv.Length <> 2 then
        printfn "Usage: dotnet run <file> [debug]"
        exit 1

    (* read in the input file *)
    let file = argv.[0]
    // let input = File.ReadAllText file

    (* does the user want parser debugging turned on? *)
    let do_debug = if argv.Length = 2 then true else false
    printfn "%A" do_debug

    (* try to parse what they gave us *)
    // let ast_maybe = parse file do_debug
    // match ast_maybe with
    //                     | Some ast ->
    //                         printfn "%A" ast
    //                         eval ast Map.empty |> ignore
    //                         0
    //                     | None     ->
    //                         printfn "Invalid program."
    
    // 0
    // if argv.Length <> 1 then
    //     printfn "usage dotnet run \"string to parse\""
    //     exit 1
    // else




    let ast = parse file do_debug
    printfn "%A" ast

    0

