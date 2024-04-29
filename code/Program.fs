open Par
open Eval
open AST
open Combinator
open System.IO
open System.Diagnostics

(*executes a given script as a new process using zsh*)
let executeScript (filename: string) =
    let scriptProcess = ProcessStartInfo("zsh", filename)
    scriptProcess.CreateNoWindow <- true
    scriptProcess.RedirectStandardOutput <- true
    scriptProcess.UseShellExecute <- false

    let pro = Process.Start(scriptProcess)
    pro.WaitForExit() |> ignore

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

        let file_name = "text_folder/gv.txt"
        let gvText = evalExpr ast
        File.WriteAllText(file_name, gvText)

        (*generates a .sh file to exicute the graphviz code generating an svg file in
        the svg folder TODO add ability of user to name the output*)
        
        let execution_name = "exe.sh"
        //File.WriteAllText(execution_name, ("dot -Tpng " + file_name + " -o ../docs/images/Fascism.png"))

        File.WriteAllText(execution_name, ("dot -Tsvg " + file_name + " -o svg_folder/graph.svg"))
        executeScript execution_name
        printfn "Graph generated, located in svg_folder."
        
    | None ->
        printfn "Failed to parse input"

    0