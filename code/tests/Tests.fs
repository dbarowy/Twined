namespace tests
open Par
open AST
open System
open Microsoft.VisualStudio.TestTools.UnitTesting

[<TestClass>]
type TestClass () =
    [<TestMethod>]
    member this.TestMethodPassing () =
        Assert.IsTrue(true);


