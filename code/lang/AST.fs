module AST

type Expr =
| Nodes_and_Assignments of Expr * Expr
| Node of Expr * Expr
| Edge_list of Expr list
| Assignment_list of Expr list
| Node_name of string
| Assignment of Expr * Expr 
| Node_info of string
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