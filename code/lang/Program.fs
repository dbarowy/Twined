﻿module code.App

// Import namespaces
open System
open System.IO
open System.Net.Http
open System.Text
open System.Diagnostics
open Newtonsoft.Json
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Http
open Giraffe
open AST
open Par
open Eval
//open Library

// ---------------------------------
// Models
// ---------------------------------

// Define the data model for messages
type Message =
    {
        Text : string
    }

// Define the data model for chat messages
type ChatMessage =
    {
        role : string
        content : string
    }

// Define the request model for the OpenAI API
type ChatRequest =
    {
        model : string
        messages : ChatMessage array
        temperature : float
        max_tokens : int
        top_p : float
        frequency_penalty : float
        presence_penalty : float
    }

// Define the response choice model for the OpenAI API
type ChatResponseChoice =
    {
        message : ChatMessage
    }

// Define the response model for the OpenAI API
type ChatResponse =
    {
        choices : ChatResponseChoice array
    }

// Define the model for the user's prompt
type UserPrompt = {
    userPrompt : string
}
type AppState = { mutable LastInput: int }

// ---------------------------------
// State and Environment
// ---------------------------------

let state = {LastInput = 0}
let mutable envi : Env = Map.empty

let exe_location = "outputs/exe.sh"
let gv_location = "inputs/text_folder/gv.txt"
let preparse_location = "inputs/preparse.txt"
let text_location = "tText_Content/cont.txt"
let svg_location = "tSVG_Op/graph.svg"
//let pdf_location = "input/pdf_folder/pdf_output.txt" // Future Feature!

// ---------------------------------
// Utility Functions
// ---------------------------------


let zsh_check =
    try
        let psi = new System.Diagnostics.ProcessStartInfo("zsh", "--version")
        psi.RedirectStandardOutput <- true
        psi.UseShellExecute <- false
        let pro = System.Diagnostics.Process.Start(psi)
        pro.WaitForExit()
        pro.ExitCode = 0
    with
    | _ -> false

(*executes a given script as a new process using azsh or powershell as avaible*)
let executeDotCommand (dotPath: string) (inputFile: string) (outputFile: string) =
    let logOutput (proc: Process) =
        let output = proc.StandardOutput.ReadToEnd()
        let error = proc.StandardError.ReadToEnd()
        proc.WaitForExit()
        printfn "Script Output:\n%s" output
        printfn "Script Error:\n%s" error
        printfn "Exit Code: %d" proc.ExitCode

    let startInfo = ProcessStartInfo()
    startInfo.FileName <- dotPath
    startInfo.Arguments <- sprintf "-Tsvg %s -o %s" inputFile outputFile
    startInfo.RedirectStandardOutput <- true
    startInfo.RedirectStandardError <- true
    startInfo.UseShellExecute <- false

    use proc = new Process()
    proc.StartInfo <- startInfo
    proc.Start() |> ignore
    logOutput proc

(*executes a given script as a new process using azsh or powershell as avaible*)
let executeScript (filename: string) =
        let scriptProcess = ProcessStartInfo("zsh", filename) 
        let pro = Process.Start(scriptProcess)
        pro.WaitForExit() |> ignore
   

// ---------------------------------
// Views
// ---------------------------------

module Views =
    open Giraffe.ViewEngine

    // Define a layout for the HTML view
    let layout (content: XmlNode list) =
        html [] [
            head [] [
                title []  [ encodedText "GiraffeApp" ]
                link [ _rel  "stylesheet"
                       _type "text/css"
                       _href "/main.css" ]
            ]
            body [] content
        ]

    // Define a partial view for the title
    let partial () =
        h1 [] [ encodedText "GiraffeApp" ]

    // Define the index view using the layout and a model
    let index (model : Message) =
        [
            partial()
            p [] [ encodedText model.Text ]
        ] |> layout

// ---------------------------------
// OpenAI API Integration
// ---------------------------------

let apiKey = "XXXXXXXX"

// Create an HttpClient for making HTTP requests
let httpClient = new HttpClient()

// Set the authorization header for the HttpClient
httpClient.DefaultRequestHeaders.Authorization <- 
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey)

// Set the accept header for the HttpClient
httpClient.DefaultRequestHeaders.Accept.Add(
    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"))

// Function to make a chat completion request to the OpenAI API
let makeChatCompletionRequest (userPrompt: string) =
    async {
        // Create the request payload
        let payload = 
            {
                model = "gpt-4-turbo" // "gpt-3.5-turbo"
                messages = [| { role = "user"; content = userPrompt } |]
                temperature = 1.0
                max_tokens = 4000
                top_p = 1.0
                frequency_penalty = 0.0
                presence_penalty = 0.0
            } |> JsonConvert.SerializeObject

        // Create the request content
        let content = new StringContent(payload, Encoding.UTF8, "application/json")
        
        // Send the request and get the response
        let! response = httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content) |> Async.AwaitTask
        let! responseBody = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        // Deserialize the response
        let parsedResponse = JsonConvert.DeserializeObject<ChatResponse>(responseBody)
      
        // Return the first choice's message content or a default message if no choices are available
        match parsedResponse.choices |> Array.tryHead with
        | Some choice -> return choice.message.content
        | None -> return "No response content available."
    }

// ---------------------------------
// Command Handlers
// ---------------------------------

// Function to handle the "TwinedGraph:" command
let handleTwinedGraph (userInput: string) =

    // Define the predefined graph prompt
    let prompt_template = 
                "input:
                {Fuel, (Engine Operation,)}
                {Air, (Engine Operation,)}
                {Spark, (Engine Operation,)}
                ^^Fuel := provides energy^^
                ^^Air := helps burn fuel^^
                ^^Spark := ignites fuel-air mix^^

                Expand on the types of fuel and what they normally power. Include additional nodes and descriptions related to different fuels such as gasoline, diesel, electricity, and hydrogen, and specify what each fuel powers. Also, include nodes and descriptions for engine operation and vehicle movement.

                The output should be:

                {Fuel, (Engine Operation,)}
                {Air, (Engine Operation,)}
                {Spark, (Engine Operation,)}
                {Engine Operation, (Car Movement,)}
                {Car Movement, (Travel,)}
                {Gasoline, (Engine Operation,)}
                {Diesel, (Engine Operation,)}
                {Electricity, (Electric Motor Operation,)}
                {Hydrogen, (Fuel Cell Engine Operation,)}
                {Electric Motor Operation, (Electric Vehicle Movement,)}
                {Fuel Cell Engine Operation, (Hydrogen Vehicle Movement,)}
                {Electric Vehicle Movement, (Travel,)}
                {Hydrogen Vehicle Movement, (Travel,)}
                ^^Fuel := provides energy^^
                ^^Air := helps burn fuel^^
                ^^Spark := ignites fuel-air mix^^
                ^^Engine Operation := converts fuel energy to mechanical power^^
                ^^Car Movement := enables travel^^
                ^^Travel := allows movement from one location to another^^
                ^^Gasoline := powers internal combustion engines^^
                ^^Diesel := powers internal combustion engines, often in trucks and buses^^
                ^^Electricity := powers electric motors, used in electric vehicles^^
                ^^Hydrogen := used in fuel cells to power electric motors in hydrogen vehicles^^
                ^^Electric Motor Operation := converts electricity into mechanical power^^
                ^^Fuel Cell Engine Operation := converts hydrogen into electrical power^^
                ^^Electric Vehicle Movement := enables travel of electric vehicles^^
                ^^Hydrogen Vehicle Movement := enables travel of hydrogen vehicles^^"

    let graph_text = (File.ReadAllText(preparse_location))
    let predefinedPrompt = Printf.StringFormat<string -> string>( 
        """given the following example as a template """ + prompt_template + 
        
        """given the following graph 
  
        """ + graph_text + """
        
        explain "%s" in the same way making sure to include the origional graph, all origional definitions that make sense, and any additional nodes or definitions needed to answer the question in the output, in addition ensure the response use no extra words outside of the graph format:
        """
    ) 

    // Format the prompt with the user's input
    let prompt = sprintf predefinedPrompt userInput

    // Create the chat completion request and return the response content
    makeChatCompletionRequest prompt

// Function to handle the "TwinedOpSVG:" command
let handleTwinedOpSVG (userInput: string) =
    let directoryPath = "tSVG_Op"
    if String.IsNullOrWhiteSpace(userInput) then
        // Return the list of SVG files
        let files = Directory.GetFiles(directoryPath, "*.svg") |> Array.map Path.GetFileName
        let result = String.Join("\n", files)
        async { return result }
    else
        let filePath = Path.Combine(directoryPath, userInput.Trim() + ".svg")
            
        if File.Exists(filePath) then
            async { return File.ReadAllText(filePath) }
        else
            async { return "File not found." }

let handleTwinedText (userInput: string) =
    let filePath = Path.Combine("tGraph_Text", userInput.Trim() + ".txt")
    if File.Exists(filePath) then
        let text = File.ReadAllText(filePath)
        let mainTopic = text.Split('\n') |> Array.head
        let nodes = text.Split('\n') |> Array.filter (fun line -> line.StartsWith("{"))
        let nodeDefinitions = String.Join("\n", nodes)

        let prompt = 
            let graph_text = (File.ReadAllText(preparse_location))
            sprintf """
            Given the following graph:
            %s
            

            For each node, starting with "%s," provide a 3-sentence concise definition, then add the following nodes with 3-sentence definitions for each. Conclude with a brief summary of how all the information is interconnected:
            %s
            """ graph_text mainTopic nodeDefinitions

        async {
            let responseContent = Async.RunSynchronously (makeChatCompletionRequest prompt)
            let outputFilePath = Path.Combine("tText_Content", userInput.Trim() + "_content.txt")
            Directory.CreateDirectory("tText_Content") |> ignore
            File.WriteAllText(outputFilePath, responseContent)
            return responseContent
        }
    else
        async { return "File not found." }

// Function to handle the "TwinedOpText:" command
let handleTwinedOpText (userInput: string) =
    let directoryPath = "tText_Content"
    if String.IsNullOrWhiteSpace(userInput) then

        // Return the list of text files
        let files = Directory.GetFiles(directoryPath, "*.txt") |> Array.map Path.GetFileName
        let result = String.Join("\n", files)
        async { return result }
    else

        // Return the content of the specific text file
        let filePath = Path.Combine(directoryPath, userInput.Trim() + ".txt")
        if File.Exists(filePath) then
            async { return File.ReadAllText(filePath) }
        else
            async { return "File not found." }

// ---------------------------------
// Web app
// ---------------------------------

// Define a handler for the index route
let indexHandler (name : string) =
    let greetings = sprintf "Hello %s, from Giraffe!" name
    let model     = { Text = greetings }
    let view      = Views.index model
    htmlView view

(*starts the main method in main*)
let mainHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task{
            let! userPrompt = ctx.BindJsonAsync<UserPrompt>()
            let userInput = userPrompt.userPrompt.Trim()
            let response = 
                "Welcome to Twined!!<br>" +
                "<br> -> Twined is a revolutionary knowledge programming language designed to transform how you manage and interact with textual information.<br>" +
                "<br> -> It converts unstructured text data into structured, navigable knowledge graphs, enhancing your ability to comprehend and analyze information.<br>" +
                "<br> -> With Twined, you can explore data interactively and create insights through intuitive graph-based reasoning visualizations.<br>" +
                "<br>Would you like to:<br>" +
                "1: Open an existing graph in a text file?<br>" +
                "2: Open a PDF, image, or other text file with raw data? (Soon!)"
 
            return! json response next ctx
        }


(* this needs to talk between web and library after the start of main*)
let path_finder : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let! userPrompt = ctx.BindJsonAsync<UserPrompt>()
            let userInput = userPrompt.userPrompt.Trim()
            match userInput with
            | "1" ->
                let files = Directory.GetFiles("inputs/test_text")
                let fileList = files |> Array.map Path.GetFileName |> String.concat "<br>"
                let response = 
                    "Option One<br><br>" +
                    "Here is the list of current text files:<br><br>" +
                    fileList +
                    "<br><br> -> Copy the following path followed by the name of your chosen file (e.g., name.txt):<br><br>" +
                    "I.e Type the Following: 3 inputs/test_text/car.txt" +
                    "<br><br> -> You can also enter your own program by entered the path to the text file.<br>"
                    
                return! json response next ctx

            | "2" ->
                let response = "Future Implementation"
                return! json response next ctx

            | "Exit" | "exit" ->
                let response = "Exiting Twined. Goodbye!"
                return! json response next ctx

            | _ -> 
                let response = 
                    "Please enter a valid option:<br>" +
                    "1: Open an existing graph in a text file?<br>" +
                    "Exit: Close the application"

                return! json response next ctx
        }
let path_conversion : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let! userPrompt = ctx.BindJsonAsync<UserPrompt>()
            let user_input =
                if userPrompt.userPrompt.Trim().StartsWith("3") then
                    userPrompt.userPrompt.Trim().[1..].Trim()
                else
                    preparse_location

            let fullPath = Path.GetFullPath(user_input)
            File.WriteAllText(preparse_location, File.ReadAllText fullPath)

            match user_input.EndsWith(".txt") with 
            | true ->
                let input = File.ReadAllText preparse_location
                ctx.GetLogger().LogInformation("Input file content: {Content}", input)
                let ast = parse input false

                match ast with
                | Some ast ->
                    let file_name = gv_location
                    let gvText, env = eval ast envi
                    
                    let resp_list = 
                        [ for key in env.Keys do
                            let value = env.[key]
                            yield string key + ": \n" + string value + "\n\n" ]

                    let response = List.fold (fun acc elem -> acc + elem ) "" resp_list

                    File.WriteAllText(text_location, response)
                    File.WriteAllText(file_name, gvText)

                    ctx.GetLogger().LogInformation("Graphviz file content: {Content}", gvText)

                    let dotPath = "C:\\Program Files\\Graphviz\\bin\\dot.exe" // After installing Dot, We Need to Adjust this path to where dot.exe is located

                    ctx.GetLogger().LogInformation("Executing dot command: {Command}", sprintf "%s -Tsvg %s -o %s" dotPath file_name svg_location)

                    try
                        if zsh_check then
                            let execution_name = exe_location
                            File.WriteAllText(execution_name, ("dot -Tsvg " + file_name + " -o " + svg_location))
                            executeScript execution_name
                        else 
                            executeDotCommand dotPath file_name svg_location    
                    with
                    | ex ->
                        ctx.GetLogger().LogError(ex, "Error executing dot command")

                    // Check if the SVG file was created
                    if File.Exists(svg_location) then
                        let responseContent = Async.RunSynchronously (handleTwinedOpSVG "graph")
                        return! json responseContent next ctx
                    else
                        let errorMessage = "SVG file not generated."
                        ctx.GetLogger().LogError(errorMessage)
                        return! json errorMessage next ctx
                        
                | None ->       
                    let responseContent = "Error displaying graph."
                    return! json responseContent next ctx

            | false ->
                let state = {LastInput = 3}
                let response = "Please enter " + string (state.LastInput) + " followed by the path of your graph ending in .txt"
                return! json response next ctx
        }


let text_display : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                    let response = File.ReadAllText(text_location)
                    return! json response next ctx
            }

let update : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let! userPrompt = ctx.BindJsonAsync<UserPrompt>()
                let input = userPrompt.userPrompt.Trim()
                ctx.GetLogger().LogInformation("update received prompt: {Prompt}", input)
                let responseContent = Async.RunSynchronously (handleTwinedGraph input)
                File.WriteAllText(preparse_location, responseContent)

                return! json preparse_location next ctx
            }

let apiHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {

            let! userPrompt = ctx.BindJsonAsync<UserPrompt>()
            let userInput = userPrompt.userPrompt.Trim()

            ctx.GetLogger().LogInformation("Received prompt: {Prompt}", userInput)

            if userInput.StartsWith("TwinedChat:") then

                let responseContent = Async.RunSynchronously (makeChatCompletionRequest userInput)

                return! json responseContent next ctx

            // Check if the user prompt starts with "TwinedGraph:"
            elif userInput.StartsWith("Expand") || userInput.StartsWith("expand") then

                let topic = userInput.Substring("expand".Length).Trim()

                let responseContent = Async.RunSynchronously (handleTwinedGraph topic)
                File.WriteAllText(preparse_location, responseContent)

                return! json preparse_location next ctx

            // Check if the user prompt starts with "TwinedOpSVG:"
            elif userInput.StartsWith("TwinedOpSVG:") then

                let fileName = userInput.Substring("TwinedOpSVG:".Length).Trim()

                let responseContent = Async.RunSynchronously (handleTwinedOpSVG fileName)

                return! json responseContent next ctx

            elif userInput.StartsWith("TwinedText:") then

                let fileName = userInput.Substring("TwinedText:".Length).Trim()

                // Handle the "TwinedText:" command and get the response content
                let responseContent = Async.RunSynchronously (handleTwinedText fileName)

                // Return the response content as JSON
                return! json responseContent next ctx

            elif userInput.StartsWith("TwinedOpText:") then

                let fileName = userInput.Substring("TwinedOpText:".Length).Trim()

                let responseContent = Async.RunSynchronously (handleTwinedOpText fileName)

                return! json responseContent next ctx

            else
                return! json "Invalid input. Please start your prompt with 'TwinedChat:', 'TwinedGraph:', 'TwinedOpSVG:', 'TwinedText:', or 'TwinedOpText:'" next ctx
        }


// ---------------------------------
// Web Application Routes
// ---------------------------------

// Define the web application routes
let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> htmlFile "Webroot/index.html"
                routef "/hello/%s" indexHandler
            ]
        POST >=> 
            choose [
                route "/api/chat" >=> apiHandler
                route "/api/callMain" >=> mainHandler
                route "/api/find" >=>  path_finder
                route "/api/path" >=> path_conversion
                route "/api/disp_text" >=> text_display
                route "/api/update" >=> update
            ]
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

// Define an error handler
let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

// Configure CORS policy
let configureCors (builder : CorsPolicyBuilder) =
    builder
        .WithOrigins(
            "http://localhost:5000",
            "https://localhost:5001")
       .AllowAnyMethod()
       .AllowAnyHeader()
       |> ignore

// Configure the application
let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()
    (match env.IsDevelopment() with
    | true  ->
        app.UseDeveloperExceptionPage()
    | false ->
        app .UseGiraffeErrorHandler(errorHandler)
            .UseHttpsRedirection())
        .UseCors(configureCors)
        .UseStaticFiles()
        .UseGiraffe(webApp)

// Configure the services
let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore

// Configure logging
let configureLogging (builder : ILoggingBuilder) =
    builder.AddConsole()
           .AddDebug() |> ignore

// Define the main entry point
[<EntryPoint>]
let main args =

    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .UseContentRoot(contentRoot)
                    .UseWebRoot(webRoot)
                    .Configure(Action<IApplicationBuilder> configureApp)
                    .ConfigureServices(configureServices)
                    .ConfigureLogging(configureLogging)
                    |> ignore)
        .Build()
        .Run()

    0

