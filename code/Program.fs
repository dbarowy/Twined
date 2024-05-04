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


let update_svg (ast): unit =
    let file_name = "text_folder/gv.txt"
    let gvText = evalExpr ast
    File.WriteAllText(file_name, gvText)

    (*generates a .sh file to exicute the graphviz code generating an svg file in
    the svg folder TODO add ability of user to name the output*)
    
    let execution_name = "exe.sh"
    // File.WriteAllText(execution_name, ("dot -Tpng " + file_name + " -o ../docs/images/plant_test_0.1.png"))

    File.WriteAllText(execution_name, ("dot -Tsvg " + file_name + " -o svg_folder/graph.svg"))
    executeScript execution_name
    printfn "Graph generated, located in svg_folder."

(*where we switch to taking user input to answer questions*)
let rec maintwo (debug: bool): unit =
    printfn "Would you like to further explore one of these topics, if so which one?"
    let input = System.Console.ReadLine()
    // need to see if input is exit() so we know to stop
    let final_input = 
        "Using the information provided before can you ..." +
        input + " in the form {name, (node1, node2, node3,)}"

    //send final_input to LLM of choice to get a new graph.
    printfn "%A" final_input
    let ast = parse final_input debug
    match ast with
    |Some ast ->
        update_svg(ast) |> ignore
        maintwo(debug)
    |None ->
        printfn "Failed to parse input, retry or type \"exit()\" to exit."
        maintwo(debug)

[<EntryPoint>]
let main argv =
    if argv.Length <> 1 && argv.Length <> 2 then
        printfn "Usage: dotnet run <file> [debug]"
        1
    else 

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
        // File.WriteAllText(execution_name, ("dot -Tpng " + file_name + " -o ../docs/images/plant_test_0.1.png"))

        File.WriteAllText(execution_name, ("dot -Tsvg " + file_name + " -o svg_folder/graph.svg"))
        executeScript execution_name
        printfn "Graph generated, located in svg_folder."
        
    | None ->
        printfn "Failed to parse input"

    maintwo(do_debug)

    0  
