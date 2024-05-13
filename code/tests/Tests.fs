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

(*
    [<TestMethod>]
    member this.TestMethodPassing () =
        Assert.IsTrue(true); 
*)

    [<TestMethod>]
    member this.TestValidNodeParsing () =
        // let input = File.ReadAllText "test_text/political_systems.txt"

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
            Assert.AreEqual(expected, ws)

        | None ->
            Assert.IsTrue false

    [<TestMethod>]
    member this.TestValidEval () =
        let input = 
            "{European Fascism, (Germany, Italy, Spain,)}
            {Germany, (Adolf Hitler,)}
            {Italy, (Benito Mussolini,)}
            {Spain,(Francisco Franco,)}"

        let expected = File.ReadAllText "test_text/answers/valid_eval.txt"
          
            // "digraph G {
            //     \"European Fascism\" -> \"Germany\";
            //     \"European Fascism\" -> \"Italy\";
            //     \"European Fascism\" -> \"Spain\";
            //     \"Germany\" -> \"Adolf Hitler\";
            //     \"Italy\" -> \"Benito Mussolini\";
            //     \"Spain\" -> \"Francisco Franco\";
            //     }"
    
        let result = parse input false
        
        match result with
        | Some ast -> 

            let evaluation, _ = eval ast Map.empty  

            Assert.AreEqual(expected, evaluation)

        | None ->
            Assert.IsTrue false