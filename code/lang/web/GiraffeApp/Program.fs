module GiraffeApp.App

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
                model = "gpt-3.5-turbo"
                messages = [| { role = "user"; content = userPrompt } |]
                temperature = 1.0
                max_tokens = 256
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
// Web app
// ---------------------------------

// Define a handler for the index route
let indexHandler (name : string) =
    let greetings = sprintf "Hello %s, from Giraffe!" name
    let model     = { Text = greetings }
    let view      = Views.index model
    htmlView view

// Define a handler for the API route
let apiHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            
            // Bind the incoming JSON request to the UserPrompt model
            let! userPrompt = ctx.BindJsonAsync<UserPrompt>()

            // Check if the user prompt starts with "TwinedChat:"
            if userPrompt.userPrompt.StartsWith("TwinedChat:") then

                // Make the chat completion request and get the response content
                let responseContent = Async.RunSynchronously (makeChatCompletionRequest userPrompt.userPrompt)

                // Return the response content as JSON
                return! json responseContent next ctx

            else

                // If the prompt does not start with "TwinedChat:", return a default response
                return! json "Invalid input. Please start your prompt with 'TwinedChat:'" next ctx
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

