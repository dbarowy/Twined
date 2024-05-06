open Par
open Eval
open AST
open Combinator
open System.IO
open System.Diagnostics

(*let apiKey = ""  
  let httpClient = new HttpClient()*)

(*executes a given script as a new process using zsh*)
let executeScript (filename: string) =
    let scriptProcess = ProcessStartInfo("powershell", filename) // had to change to powershell instead of zsh to run on windows.
    scriptProcess.CreateNoWindow <- true
    scriptProcess.RedirectStandardOutput <- true
    scriptProcess.UseShellExecute <- false

    let pro = Process.Start(scriptProcess)
    pro.WaitForExit() |> ignore

let update_svg (ast: Expr) (fullPath: string): unit =
    let gvText = evalExpr ast  // Convert AST to a string or graph format.
    File.WriteAllText(fullPath, gvText)  // Write the output to the specified path.

    let executionName = "exe.sh"
    File.WriteAllText(executionName, sprintf "dot -Tsvg %s -o %s" fullPath (fullPath.Replace(".txt", ".svg")))
    executeScript executionName
    printfn "Graph generated, located at %s." (fullPath.Replace(".txt", ".svg"))

(*let callOpenAiApi topic =
    let requestBody = sprintf """
    {
        "model": "gpt-3.5-turbo",
        "prompt": "Expand on the topic of %s in the format of '{Sunlight, (Plant Growth,)} {Water, (Plant Growth,)} {Soil Nutrients, (Plant Growth,)}', give me the output",
        "temperature": 1,
        "max_tokens": 256,
        "top_p": 1,
        "frequency_penalty": 0,
        "presence_penalty": 0
    }
    """ topic *)

(*let update_svg (ast): unit =
    let file_name = "text_folder/gv.txt"
    let gvText = evalExpr ast
    File.WriteAllText(file_name, gvText)

    (*generates a .sh file to exicute the graphviz code generating an svg file in
    the svg folder TODO add ability of user to name the output*)
    
    let execution_name = "exe.sh"
    // File.WriteAllText(execution_name, ("dot -Tpng " + file_name + " -o ../docs/images/plant_test_0.1.png"))

    File.WriteAllText(execution_name, ("dot -Tsvg " + file_name + " -o svg_folder/graph.svg"))
    executeScript execution_name
    printfn "Graph generated, located in svg_folder." *)

(*where we switch to taking user input to answer questions*)
(*let rec maintwo (debug: bool): unit =
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
        maintwo(debug)*)

(*let rec maintwo (debug: bool): unit =
    printfn "Would you like to further explore one of these topics, if so which one? Type 'No' to exit."
    let input = System.Console.ReadLine()

    match input.Trim().ToLower() with
    | "No" -> 
        printfn "Exiting program. Thank you for using the application."
        System.Environment.Exit(0)
    | _ ->
        let final_input = 
            sprintf "Using the information provided before can you ...%s in the form {name, (node1, node2, node3,)}" input
        printfn "%A" final_input
        let ast = parse final_input debug

        match ast with
        | Some ast ->
            printfn "Please enter a name for the output file (e.g., graph1.svg):"
            let fileName = System.Console.ReadLine()
            let fullPath = sprintf "svg_folder/%s.svg" fileName  // Directly use .svg to avoid confusion
            update_svg ast fullPath  // Ensure update_svg function is defined to accept (ast: Expr) (fullPath: string)

            printfn "File generated: %s. Would you like to open it? (yes/no)" fullPath

            match System.Console.ReadLine().ToLower() with
            | "yes" ->
                let psi = new System.Diagnostics.ProcessStartInfo("powershell", sprintf "Start %s" fullPath)
                psi.UseShellExecute <- false
                System.Diagnostics.Process.Start(psi) |> ignore
            | _ -> ()
            printfn "Would you like to run another operation? (yes/no)"
            match System.Console.ReadLine().ToLower() with
            | "yes" -> maintwo debug
            | _ -> printfn "Thank you for using the application."
        | None ->
            printfn "Failed to parse input, retry or type \"exit\" to exit."
            maintwo debug *)

(*let rec maintwo (debug: bool): unit =
    printfn "You can type 'exit' at any point to stop. What would you like to do next? (type 'help' for options)"
    let input = System.Console.ReadLine().Trim().ToLower()

    match input with
    | "exit" -> 
        printfn "Exiting program. Thank you for using the application."
        System.Environment.Exit(0)
    | "help" ->
        printfn "Help: Enter graph specifications in the form {name, (node1, node2, node3,)}, or 'exit' to quit."
        maintwo debug
    | _ ->
        let final_input = sprintf "Using the information provided before can you ...%s" input
        printfn "Processing: %s" final_input
        let ast = parse final_input debug

        match ast with
        | Some ast ->
            printfn "Please enter a name for the output file (default 'graph1.svg'):"
            let defaultFileName = "graph1.svg"
            let fileName = System.Console.ReadLine()
            let effectiveFileName = if fileName = "" then defaultFileName else fileName
            let fullPath = sprintf "svg_folder/%s" effectiveFileName

            printfn "Generating graph... %s" fullPath
            update_svg ast fullPath

            printfn "File generated: %s. Would you like to view this graph now? (yes/no)" fullPath
            match System.Console.ReadLine().ToLower() with
            | "yes" ->
                let psi = new System.Diagnostics.ProcessStartInfo("powershell", sprintf "Start %s" fullPath)
                psi.UseShellExecute <- false
                System.Diagnostics.Process.Start(psi) |> ignore
                printfn "Graph is open, close the viewer to continue."
            | _ -> printfn "Not viewing the graph."

            printfn "Would you like to run another operation? (yes/no)"
            match System.Console.ReadLine().ToLower() with
            | "yes" -> maintwo debug
            | _ -> printfn "Thank you for using the application."
        | None ->
            printfn "Failed to parse input, please try again or type 'help' for more information."
            maintwo debug*)

let rec maintwo (debug: bool) (inputFilePath: string): unit =
    printfn "\n(Twined) -> Graph generated with success! It is now located in the svg_folder.\n"
    printfn "(Twined) -> What would you like to do next? (type 'help' for options or 'exit' to quit)\n"

    let displayHelpMenu () =
        printfn "\n(Twined) -> Help Menu:"
        printfn "    1 - Expand current graph (Feature coming soon)"
        printfn "    2 - View graph (This will open the generated SVG file located in the svg_folder)"
        printfn "    3 - Create a new graph (Feature coming soon)"
        printfn "    4 - Search for another graph (Feature coming soon)"
        printfn "    5 - Print content of the original text file"
        printfn "    6 - Print content of the generated GV file"
        printfn "    7 - Exit to quit the program\n"

    let handleUserSelection input =
        match input with

        | "1" -> printfn "\n(Twined) -> The feature to expand is not yet implemented!"

        | "2" ->
            let fullPath = "svg_folder/graph.svg"

            if System.IO.File.Exists(fullPath) then
                printfn "\n(Twined) -> Viewing graph... (This will open the generated SVG file located at %s)" fullPath
                let psi = new System.Diagnostics.ProcessStartInfo("powershell", sprintf "Start '%s'" fullPath)
                psi.UseShellExecute <- false
                System.Diagnostics.Process.Start(psi) |> ignore

                printfn "\n(Twined) -> Graph is open!"
            else
                printfn "\n(Twined) -> Error: The specified SVG file does not exist at %s." fullPath

        | "3" -> printfn "\n(Twined) -> The feature to create a new graph is not yet implemented!"

        | "4" -> printfn "\n(Twined) -> The feature to search for another graph is not yet implemented!"

        | "5" ->
            let originalText = File.ReadAllText inputFilePath
            printfn "\n(Twined) -> Contents of the original text file:\n" 
            printfn "%s" originalText

        | "6" ->
            let gvText = File.ReadAllText "text_folder/gv.txt"
            printfn "\n(Twined) -> Contents of the generated GV file:\n"
            printfn "%s\n" gvText

        | "7" ->
            printfn "\n(Twined) -> Exiting the program, see you soon!"
            System.Environment.Exit(0)

        | _ -> printfn "\n(Twined) -> Invalid selection. Please try again or type 'help' for options."

    let rec processInput () =
        printf "(User) -> "
        let input = System.Console.ReadLine().Trim().ToLower()
        match input with

        | "exit" -> 
            printfn "\n(Twined) -> Exiting the program, see you soon!\n"
            System.Environment.Exit(0)

        | "help" ->
            displayHelpMenu ()
            processInput ()  

        | _ ->
            handleUserSelection input
            printfn "\n(Twined) -> What would you like to do next? Type 'help' for options or 'exit' to quit.\n"
            processInput ()  

    processInput () 


[<EntryPoint>]
let main argv =
    if argv.Length <> 1 && argv.Length <> 2 then
        printfn "Usage: dotnet run <file> [debug]"
        1
    else 

    (* read in the input file *)
    let file = argv.[0]
    let fullPath = Path.GetFullPath(file)
    let input = File.ReadAllText fullPath

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
        //printfn "Graph generated, located in svg_folder."
        
    | None ->
        printfn "Failed to parse input"

    maintwo(do_debug) fullPath

    0  
