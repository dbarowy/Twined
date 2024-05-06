module AST

type Expr =
| Node of Expr * Expr
| Edge_list of Expr list
| Node_name of string
| Num of int
| Exit

type Message = {
    role: string
    content: string
}

type Choice = {
    index: int
    message: Message
}

type CompletionResponse = {
    choices: Choice[]
}

type Canvas = Line list

let CANVAS_SZ = 400