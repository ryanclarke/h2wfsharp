// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open FSharp.Data
open System

let auth = "auth"
let dashboard = "dashboard"

let basic email password = 
    let cred = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sprintf "%s:%s" email password))
    sprintf "Basic %s" cred

let fetchUrl url email password =  
    let uri = sprintf "https://h2w.cc/api/v1/%s" url
    let resp = Http.Request( uri, headers = [ "X-H2W-Client-ID", "0"; "Authorization", basic email password ] )
    printfn "%A" resp

let login email password =
    fetchUrl "auth/token" email password

let start args =
    match args with
        | [] -> printfn "Help: do it right."
        | "auth"::xs -> login xs.[0] xs.[1]
        | "dashboard"::xs -> printfn "5,000 steps"
        | _ -> printfn "%A" args

//let testCase args =
//    start args
//    //|> printf "%A => %A" args 
//
//let test = 
//    testCase []
//    testCase ["garbage"]
//    testCase ["garbage"; "auth"]
//    testCase ["auth"]
//    testCase ["dashboard"]

[<EntryPoint>]
let main argv = 
    let args = argv |> Array.toList
    match args with
        //| "test"::xs -> test
        | _ -> start args
    0 // return an integer exit code




