module Eval
open AST
open Combinator
open System

(*Holds the data from each node in a
* dictionary, each node name will be a key that map to
* its data *)
type Env = Map<string,Expr>