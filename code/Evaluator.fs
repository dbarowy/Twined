module Eval
open AST
open Combinator
open System

(* Represents a variable environment *)
type Env = Map<string,Expr>