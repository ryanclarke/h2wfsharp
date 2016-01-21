// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

let auth = "auth"
let dashboard = "dashboard"

let start args =
    match args with
    | [] -> printfn "Help: do it right."
    | "auth"::xs -> printfn "Login"
    | "dashboard"::xs -> printfn "5,000 steps"
    | _ -> printfn "%A" args

let testCase args =
    start args
    |> printf "%A => %A" args 

let test = 
    testCase []
    testCase ["garbage"]
    testCase ["garbage"; "auth"]
    testCase ["auth"]
    testCase ["dashboard"]

[<EntryPoint>]
let main argv = 
    let args = argv |> Array.toList
    match args with
    | "test"::xs -> test
    | _ -> start args
    0 // return an integer exit code




