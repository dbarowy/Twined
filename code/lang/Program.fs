module code.App

// Import namespaces
open System
open System.IO
open System.Net.Http
open System.Text
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
open Library


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


let state = {LastInput = 0}
let mutable envi : Env = Map.empty
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

let apiKey = "***REMOVED***" // Replace with your actual API key

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
                "Welcome to Twined: Would you like to              1: Open an exiting graph in a text file?            2: Open a pdf, image, or other text file with raw data?"

            return! json response next ctx
        }


(* this needs to talk between web and library after the start of main*)
let path_finder : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task{
            let! userPrompt = ctx.BindJsonAsync<UserPrompt>()
            let userInput = userPrompt.userPrompt.Trim()
            match userInput with 
            | "1" | "2" ->
                let state = {LastInput = 3}
                let response = "Please enter " + string (state.LastInput) + " followed by the path of your graph"
                return! json response next ctx
            | _ -> 
                let response = "Please enter one or two"
                return! json response next ctx
        }

let path_conversion : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
            task{
                let! userPrompt = ctx.BindJsonAsync<UserPrompt>()
                (*trims the first character*)
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

                        // write the envi to a file
                        File.WriteAllText(text_location, response)
                        // write the nodes to a file
                        File.WriteAllText(file_name, gvText)

                        (*generates a .sh file to exicute the graphviz code generating an svg file in
                        the svg folder TODO add ability of user to name the output*)
                        
                        let execution_name = exe_location

                        File.WriteAllText(execution_name, ("dot -Tsvg " + file_name + " -o " + svg_location))
                        executeScript execution_name


                        let responseContent = Async.RunSynchronously (handleTwinedOpSVG "graph")
                        return! json responseContent next ctx
                        
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

// Update the API handler
let apiHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            // Bind the incoming JSON request to the UserPrompt model
            let! userPrompt = ctx.BindJsonAsync<UserPrompt>()
            let userInput = userPrompt.userPrompt.Trim()

            // Log the received prompt
            ctx.GetLogger().LogInformation("Received prompt: {Prompt}", userInput)

            // Check if the user prompt starts with "TwinedChat:"
            if userInput.StartsWith("TwinedChat:") then
                // Make the chat completion request and get the response content
                let responseContent = Async.RunSynchronously (makeChatCompletionRequest userInput)

                // Return the response content as JSON
                return! json responseContent next ctx

            // Check if the user prompt starts with "TwinedGraph:"
            elif userInput.StartsWith("Expand") || userInput.StartsWith("expand") then
                // Extract the topic from the user prompt
                let topic = userInput.Substring("expand".Length).Trim()

                // Handle the "TwinedGraph:" command and get the response content
                let responseContent = Async.RunSynchronously (handleTwinedGraph topic)
                File.WriteAllText(preparse_location, responseContent)

                return! json preparse_location next ctx




            // Check if the user prompt starts with "TwinedOpSVG:"
            elif userInput.StartsWith("TwinedOpSVG:") then
                // Extract the file name from the user prompt
                let fileName = userInput.Substring("TwinedOpSVG:".Length).Trim()

                // Handle the "TwinedOpSVG:" command and get the response content
                let responseContent = Async.RunSynchronously (handleTwinedOpSVG fileName)

                // Return the response content as JSON
                return! json responseContent next ctx

            // Check if the user prompt starts with "TwinedText:"
            elif userInput.StartsWith("TwinedText:") then
                // Extract the file name from the user prompt
                let fileName = userInput.Substring("TwinedText:".Length).Trim()

                // Handle the "TwinedText:" command and get the response content
                let responseContent = Async.RunSynchronously (handleTwinedText fileName)

                // Return the response content as JSON
                return! json responseContent next ctx

            // Check if the user prompt starts with "TwinedOpText:"
            elif userInput.StartsWith("TwinedOpText:") then
                // Extract the file name from the user prompt
                let fileName = userInput.Substring("TwinedOpText:".Length).Trim()

                // Handle the "TwinedOpText:" command and get the response content
                let responseContent = Async.RunSynchronously (handleTwinedOpText fileName)

                // Return the response content as JSON
                return! json responseContent next ctx

            else
                // If the prompt does not start with a recognized command, return a default response
                return! json "Invalid input. Please start your prompt with 'TwinedChat:', 'TwinedGraph:', 'TwinedOpSVG:', 'TwinedText:', or 'TwinedOpText:'" next ctx
        }


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

