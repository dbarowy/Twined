namespace tests
open Par
open AST
open Eval
open System
open System.IO
open System.Text
open Microsoft.VisualStudio.TestTools.UnitTesting

[<TestClass>]
type TestClass () =

    let stripWhitespace (str: string) =
        str.Replace("\r", "").Replace("\n", "").Replace(" ", "").Replace("\t", "")

    [<TestMethod>]
    member this.TestValidNodeParsing () =
    
        let input = 
            "{European Fascism, (Germany, Italy, Spain,)}
            {Germany, (Adolf Hitler,)}
            {Italy, (Benito Mussolini,)}
            {Spain,(Francisco Franco,)}"

        let expected = 

            Nodes_and_Assignments

                (Edge_list
                    [Node
                        (Node_name "European Fascism",
                        Edge_list [Node_name "Germany"; Node_name "Italy"; Node_name "Spain"]);
                    Node (Node_name "Germany", Edge_list [Node_name "Adolf Hitler"]);
                    Node (Node_name "Italy", Edge_list [Node_name "Benito Mussolini"]);
                    Node (Node_name "Spain", Edge_list [Node_name "Francisco Franco"])],
                Assignment_list []) 


        let result = parse input false  

        match result with
        | Some ws ->

            printfn "\nParsed result: %A" ws

            Assert.AreEqual(expected, ws)

        | None ->
            printfn "\nParsing failed"

            Assert.IsTrue false

    [<TestMethod>]
    member this.TestValidEval () =
        let input = 
            "{European Fascism, (Germany, Italy, Spain,)}
            {Germany, (Adolf Hitler,)}
            {Italy, (Benito Mussolini,)}
            {Spain,(Francisco Franco,)}"

        let fullPath = Path.GetFullPath("../../../txt_files/answers/valid_eval.txt")

        let expected = File.ReadAllText fullPath |> stripWhitespace

        let result = parse input false
        
        match result with
        | Some ast -> 
            printfn "\nParsed AST: %A" ast

            let evaluation, _ = eval ast Map.empty  

            printfn "\nEvaluation result: %s" evaluation

            Assert.AreEqual(expected, stripWhitespace evaluation)
        | None ->

            printfn "\nParsing failed"

            Assert.IsTrue false
