// module Library

// open OpenAI
// open Par
// open Eval
// open AST
// open Combinator
// open System.IO
// open System.Diagnostics
// open System.Net.Http
// open System.Text
// open Newtonsoft.Json
// open Tesseract
// open Giraffe
// open Microsoft.AspNetCore.Http


// // Define the model for the user's prompt
// type UserPrompt = {
//     userPrompt : string
// }

// // ---------------------------------
// // CHECKS OS FOR CMD TYPE
// // ---------------------------------

// (*the location of the exe.sh file*)
// let exe_location = "outputs/exe.sh"

// let gv_location = "inputs/text_folder/gv.txt"

// let preparse_location = "inputs/preparse.txt"

// let text_location = "tText_Content/cont.txt"

// let svg_location = "tSVG_Op/graph.svg"

// let pdf_location = "input/pdf_folder/pdf_output.txt"

// type AppState = { mutable LastInput: string option }

// let state = {LastInput = None}

// let zsh_check =
//     try
//         let psi = new System.Diagnostics.ProcessStartInfo("zsh", "--version")
//         psi.RedirectStandardOutput <- true
//         psi.UseShellExecute <- false
//         let pro = System.Diagnostics.Process.Start(psi)
//         pro.WaitForExit()
//         pro.ExitCode = 0
//     with
//     | _ -> false

// (*executes a given script as a new process using azsh or powershell as avaible*)
// let executeDotCommand (dotPath: string) (inputFile: string) (outputFile: string) =
//     let logOutput (proc: Process) =
//         let output = proc.StandardOutput.ReadToEnd()
//         let error = proc.StandardError.ReadToEnd()
//         proc.WaitForExit()
//         printfn "Script Output:\n%s" output
//         printfn "Script Error:\n%s" error
//         printfn "Exit Code: %d" proc.ExitCode

//     let startInfo = ProcessStartInfo()
//     startInfo.FileName <- dotPath
//     startInfo.Arguments <- sprintf "-Tsvg %s -o %s" inputFile outputFile
//     startInfo.RedirectStandardOutput <- true
//     startInfo.RedirectStandardError <- true
//     startInfo.UseShellExecute <- false

//     use proc = new Process()
//     proc.StartInfo <- startInfo
//     proc.Start() |> ignore
//     logOutput proc

// // ---------------------------------
// // OpenAI
// // ---------------------------------

// let apiKey = "XXXXXXXX"

// (*
//     https://learn.microsoft.com/en-us/dotnet/api/system.net.http.headers.authenticationheadervalue.-ctor?view=net-8.0
//     https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-8.0 
    
// *)

// let httpClient = new HttpClient()
// httpClient.DefaultRequestHeaders.Authorization <-
//     new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey)

// httpClient.DefaultRequestHeaders.Accept.Add(
//     new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"))


// let makeChatCompletionRequest (userPrompt: string) =
//     async {
//         let payload = 
//             {|
//                 model = "gpt-3.5-turbo"
//                 messages = [| { role = "user"; content = userPrompt } |]
//                 temperature = 1.0
//                 max_tokens = 256
//                 top_p = 1.0
//                 frequency_penalty = 0.0
//                 presence_penalty = 0.0
//             |} |> JsonConvert.SerializeObject

//         let content = new StringContent(payload, Encoding.UTF8, "application/json")
        
//         let! response = httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content) |> Async.AwaitTask
//         let! responseBody = response.Content.ReadAsStringAsync() |> Async.AwaitTask

//         //return responseBody

//         let parsedResponse = JsonConvert.DeserializeObject<CompletionResponse>(responseBody)
      
//         match parsedResponse.choices |> Array.tryHead with
//         | Some choice -> return choice.message.content
//         | None -> return "No response content available."
//     }

// // ---------------------------------
// // OCR 
// // ---------------------------------

// (*OCR TEST BUT NOT FULLY WORKING*)

// let ocrImage (imagePath: string) (outputPath: string) =
//     let engine = new TesseractEngine(@"./inputs/tessdata", "eng", EngineMode.Default)
//     let img = Pix.LoadFromFile(imagePath)
//     let page = engine.Process(img)
//     let text = page.GetText()
//     File.WriteAllText(outputPath, text)
//     printfn "OCR completed. Output saved to %s." outputPath

// (*OCR NOT FULLY WORKING*)
// let openImage (imagePath: string): unit =
//     if zsh_check then
//         let scriptCommand = sprintf "open '%s'" imagePath
//         File.WriteAllText("openImage.sh", scriptCommand)
//         executeScript "openImage.sh"
//     else
//         let psi = new ProcessStartInfo("powershell", sprintf "Start '%s'" imagePath)
//         psi.UseShellExecute <- false
//         Process.Start(psi) |> ignore


// // ---------------------------------
// // Opens Graph 
// // ---------------------------------

// let open_graph (fullPath: string): (unit) =

//     if zsh_check then
//         let execution_name = exe_location
//         File.WriteAllText(execution_name, ("open -a 'Google Chrome' " + fullPath + " -o " + svg_location))
//         executeScript execution_name
    
//     else
//         let psi = new System.Diagnostics.ProcessStartInfo("powershell", sprintf "Start '%s'" fullPath)
//         psi.UseShellExecute <- false
//         System.Diagnostics.Process.Start(psi) |> ignore

// // ---------------------------------
// // Generating New Graph 
// // ---------------------------------

// let update_svg (ast: Expr) (envi: Env) (fullPath: string): unit =
//     let gvText, _ = eval ast envi  // Convert AST to a string or graph format.
//     File.WriteAllText(fullPath, gvText)  // Write the output to the specified path.

//     let executionName = exe_location
//     File.WriteAllText(executionName, sprintf "dot -Tsvg %s -o %s" fullPath (fullPath.Replace(".txt", ".svg")))
//     executeScript executionName
//     printfn "Graph generated, located at %s." (fullPath.Replace(".txt", ".svg"))


// let prompt_helper (initial: string) : string =
//     let final = 
//         "given the following example graph
//         {Sunlight, (Plant Growth, Photolysis, Electron Excitation, ATP Formation, NADPH Formation,)}
//         {Water, (Plant Growth, Photolysis, Electron Supply, Proton Gradient,)}
//         {Photolysis, (Oxygen, Electron Supply, Proton Gradient,)}
//         ^^Sunlight := Sunlight provides the energy that excites electrons in chlorophyll, driving the light-dependent reactions of photosynthesis. This energy is crucial for splitting water molecules (photolysis), forming ATP through chemiosmosis, and generating NADPH for the Calvin cycle.^^
//         ^^Water := Water is crucial in photosynthesis as it provides the electrons and protons needed for the light-dependent reactions. The splitting of water molecules (photolysis) supplies the electrons that drive the electron transport chain and the protons that contribute to the formation of a proton gradient for ATP synthesis.^^
//         ^^Oxygen := Oxygen is released as a byproduct of photosynthesis when water molecules are split during photolysis. This oxygen is essential for the survival of aerobic organisms, including humans.^^
//         ^^Soil Nutrients := Plants absorb essential nutrients from the soil, which are necessary for their growth and overall health. These nutrients support various biochemical processes, including photosynthesis.^^
//         ^^Photolysis := Photolysis is the process of using light energy absorbed by chlorophyll and other pigments to split water molecules into oxygen, protons, and electrons. This process occurs in the thylakoid membranes of the chloroplasts and is essential for providing the necessary components for the light-dependent reactions of photosynthesis.^^
//         can you give explain " + initial + " in the same way with no extra words"
//     final

// // ---------------------------------------------------
// // Recursive Main Method that Handles the User's Input 
// // ---------------------------------------------------

// let rec maintwo (debug: bool) (inputFilePath: string): unit =
//     printfn "\n(Twined) -> Graph generated with success! It is now located in the svg_folder.\n"
//     printfn "(Twined) -> What would you like to do next? (type 'help' for options or 'exit' to quit)\n"

//     let displayHelpMenu () =
//         printfn "\n(Twined) -> Help Menu:"
//         printfn "    1 - Expand current graph (Feature coming soon)"
//         printfn "    2 - View graph (This will open the generated SVG file located in the svg_folder)"
//         printfn "    3 - Create a new graph (Feature coming soon)"
//         printfn "    4 - Search for another graph (Feature coming soon)"
//         printfn "    5 - Print content of the original text file"
//         printfn "    6 - Print content of the generated GV file"
//         printfn "    7 - Exit to quit the program\n"
//         printfn "    8 - View and OCR an image"
//         printfn "    9 - View OCR text results"

//     let handleUserSelection input =
//         match input with

//         | "1" -> 
//             printfn "\n(Twined) -> Please enter your prompt:"
//             let userPrompt = System.Console.ReadLine().Trim()
//             let responseContent = Async.RunSynchronously (makeChatCompletionRequest (prompt_helper userPrompt))
            
//             let ast = parse responseContent false
//             match ast with
//             |Some expr -> 

//                 let file_name = gv_location
//                 let gvText, envi = eval expr Map.empty

//                 File.WriteAllText(file_name, gvText)

//                 (*generates a .sh file to exicute the graphviz code generating an svg file in
//                 the svg folder TODO add ability of user to name the output*)
//                 let execution_name = exe_location
//                 File.WriteAllText(execution_name, ("dot -Tsvg " + file_name + " -o " + svg_location))
//                 executeScript execution_name   
                
//             | None -> 
//                 printfn "\n(Twined) -> %s" responseContent

//         | "2" ->
//             let fullPath = svg_location

//             if System.IO.File.Exists(fullPath) then
//                 printfn "\n(Twined) -> Viewing graph... (This will open the generated SVG file located at %s)" fullPath
//                 open_graph(fullPath)

//                 printfn "\n(Twined) -> Graph is open!"
//             else
//                 printfn "\n(Twined) -> Error: The specified SVG file does not exist at %s." fullPath

//         | "3" -> printfn "\n(Twined) -> The feature to create a new graph is not yet implemented!"

//         | "4" -> printfn "\n(Twined) -> The feature to search for another graph is not yet implemented!"

//         | "5" ->
//             let originalText = File.ReadAllText inputFilePath
//             printfn "\n(Twined) -> Contents of the original text file:\n" 
//             printfn "%s" originalText

//         | "6" ->
//             let gvText = File.ReadAllText gv_location
//             printfn "\n(Twined) -> Contents of the generated GV file:"
//             printfn "%s\n" gvText

//         | "7" ->
//             printfn "\n(Twined) -> Exiting the program, see you soon!"
//             System.Environment.Exit(0)

//         | "8" -> 
//             let imagePath = "image_samples/Hello-Text.png"
//             let outputPath = "text_samples/ocr.txt"
//             if File.Exists(imagePath) then
//                 openImage imagePath
//                 printfn "\n(Twined) -> Image opened: %s" imagePath
//                 printfn "\n(Twined) -> Performing OCR..."
//                 ocrImage imagePath outputPath
//                 printfn "\n(Twined) -> OCR text available at: %s" outputPath
//             else
//                 printfn "\n(Twined) -> Error: The specified image file does not exist at %s." imagePath

//         | "9" ->
//             let txtPath = "text_samples/ocr.txt"
//             if File.Exists(txtPath) then
//                 let ocrText = File.ReadAllText(txtPath)
//                 printfn "\n(Twined) -> Contents of the OCR text file:\n"
//                 printfn "%s" ocrText
//             else
//                 printfn "\n(Twined) -> Error: The OCR text file does not exist at %s." txtPath

//         | _ -> printfn "\n(Twined) -> Invalid selection. Please try again or type 'help' for options."

//     let rec processInput () =
//         printf "(User) -> "
//         let input = System.Console.ReadLine().Trim().ToLower()
//         match input with

//         | "exit" -> 
//             printfn "\n(Twined) -> Exiting the program, see you soon!\n"
//             System.Environment.Exit(0)

//         | "help" ->
//             displayHelpMenu ()
//             processInput ()  

//         | _ ->
//             handleUserSelection input
//             printfn "\n(Twined) -> What would you like to do next? Type 'help' for options or 'exit' to quit.\n"
//             processInput ()  

//     processInput () 

// // ---------------------------------------------------
// // PDF TO TEXT Converter (Not Working Properly Yet) 
// // ---------------------------------------------------

// let pdf_convert (file_name: string) =

//     (*convert pdf to text using a python script because it is open source/free*)    
//     let executionName = exe_location
//     File.WriteAllText(executionName, sprintf "python3.9 pdf_extract.py %s" file_name )
//     executeScript executionName
//     let target_name = pdf_location
    
//     target_name

// // ---------------------------------------------------
// // Checking Path (CHOSING PATHS BY TYPE INPUT) 
// // ---------------------------------------------------    

// let text_path (fullPath: string) (do_debug: bool) : unit =
//     let input = File.ReadAllText fullPath

//         (* does the user want parser debugging turned on? *)

//     let ast = parse input do_debug
    
//     match ast with
//     | Some ast ->
//         let file_name = gv_location
//         let gvText, envi = eval ast Map.empty

//         File.WriteAllText(file_name, gvText)

//         (*generates a .sh file to exicute the graphviz code generating an svg file in
//         the svg folder TODO add ability of user to name the output*)
        
//         let execution_name = exe_location
//         // File.WriteAllText(execution_name, ("dot -Tpng " + file_name + " -o ../docs/images/plant_test_0.1.png"))

//         File.WriteAllText(execution_name, ("dot -Tsvg " + file_name + " -o " + svg_location))
//         executeScript execution_name
        
        
//     | None ->

//         printfn "failed"


// let pdf_path (filename: string) (do_debug: bool) : unit =

//     let example = "{Sunlight, (Plant Growth, Photolysis, Electron Excitation, ATP Formation, NADPH Formation,)}\n{Water, (Plant Growth, Photolysis, Electron Supply, Proton Gradient,)}\n{Photolysis, (Oxygen, Electron Supply, Proton Gradient,)}\n^^Sunlight := Sunlight provides the energy that excites electrons in chlorophyll, driving the light-dependent reactions of photosynthesis. This energy is crucial for splitting water molecules (photolysis), forming ATP through chemiosmosis, and generating NADPH for the Calvin cycle.^^\n^^Water := Water is crucial in photosynthesis as it provides the electrons and protons needed for the light-dependent reactions. The splitting of water molecules (photolysis) supplies the electrons that drive the electron transport chain and the protons that contribute to the formation of a proton gradient for ATP synthesis.^^\n^^Oxygen := Oxygen is released as a byproduct of photosynthesis when water molecules are split during photolysis. This oxygen is essential for the survival of aerobic organisms, including humans.^^\n^^Soil Nutrients := Plants absorb essential nutrients from the soil, which are necessary for their growth and overall health. These nutrients support various biochemical processes, including photosynthesis.^^\n^^Photolysis := Photolysis is the process of using light energy absorbed by chlorophyll and other pigments to split water molecules into oxygen, protons, and electrons. This process occurs in the thylakoid membranes of the chloroplasts and is essential for providing the necessary components for the light-dependent reactions of photosynthesis.^^\n"

//     printfn "\n(Twined) -> are there any topics you would like to focus on? (for a broad overview type \"none\")"
//     let input = System.Console.ReadLine().Trim().ToLower()
//     match input with

//     (*want a broad overview of the topic*)
//     |"none" ->
//         let prompt = "Given the following example graph " + example + " in as many nodes as is needed can you discribe"
//         let responseContent = Async.RunSynchronously (makeChatCompletionRequest (prompt_helper prompt))

//         text_path responseContent do_debug

//     (*incase they want to exit*)
//     | "exit" -> 
//         printfn "Exiting the program, see you soon!"

//     (*asking for something specific*)
//     | _ ->
//         printfn "yay!"



// // // -----------------------------------------------------------
// // // MAIN METHOD (VERIFY USER'S INPUT - LAUNCHES TO PATH OUTPUT)
// // // -----------------------------------------------------------

// // (*sender should send messages via post, reciever accepts messages via get*)
// // let rec start_up (argv: string list) (ctx: HttpContext): int =
// //     if argv.Length <> 0 && argv.Length <> 1 then
// //             sender "Usage: dotnet run [debug]" ctx |> ignore
// //             1

// //     else
// //         let next = 
// //             sender "Welcome to Twined: \nWould you like to\n1: Open an exiting graph in a text file?\n2: Open a pdf, image, or other text file with raw data\n" ctx 
// //         receiver next ctx |> ignore
// //         let input = System.Console.ReadLine().Trim().ToLower()
// //         match input with
// //         |"1" ->
// //             let input = sender "Please enter the path of your graph" ctx 
            
            
            
// //             let fullPath = Path.GetFullPath(input)
// //             if File.Exists(fullPath) then
// //                 let do_debug = if argv.Length = 1 then true else false
// //                 text_path fullPath do_debug
// //                 0
                
// //             else
// //                 sender "invalid file " ctx |> ignore
// //                 start_up argv ctx
        
// //             0

// //         |"2" ->

// //             sender "Please enter the path of your file you wish to access" ctx |> ignore
// //             let input = System.Console.ReadLine().Trim().ToLower()
// //             let fullPath = Path.GetFullPath(input)

// //             if File.Exists(fullPath) then
// //                 let do_debug = if argv.Length = 1 then true else false

// //                 if fullPath.EndsWith(".pdf") then

// //                     let input = pdf_convert(fullPath)
// //                     pdf_path input do_debug
// //                     0

// //                 else
// //                     text_path fullPath do_debug
// //                     0
                    
// //             else
// //                 sender "invalid file" ctx |> ignore
// //                 start_up argv ctx
              
// //         | "exit" ->
// //             sender "Exiting the program, see you soon!" ctx |> ignore
// //             0
// //         | _ ->
// //             sender "Please choose 1 or 2" ctx |> ignore
// //             start_up argv ctx

