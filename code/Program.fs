open Par
open Eval 
open AST
open Combinator

[<EntryPoint>]
let main argv = 
    if argv.Length <> 1 then
        printfn "usage dotnet run \"string to parse\""
        exit 1
    else
        let input = argv[0]
        let ast = parse input
        0

