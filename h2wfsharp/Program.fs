// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open FSharp.Data
open System

let auth = "auth"
let dashboard = "dashboard"

type RunResponse = 
    | Response of FSharp.Data.HttpResponse
    | Nothing of unit

let basic email password = 
    let cred = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sprintf "%s:%s" email password))
    sprintf "Basic %s" cred

let fetchUrl url email password =  
    Http.Request(sprintf "https://h2w.cc/api/v1/%s" url, 
        headers = [ "X-H2W-Client-ID", "0"; "Authorization", basic email password ], 
        silentHttpErrors = true )
    |> Response

let login email password =
    fetchUrl "auth/token" email password

let helpText () = printfn "You're gonna need help to do this right."

let handleResponse run = 
    match run with
    | Response resp -> 
        printfn "URL:    %A" resp.ResponseUrl
        printfn "STATUS: %A" resp.StatusCode
        printfn "BODY:   %A" resp.Body
    | _ -> helpText()

let call args =
    match args with
    | "auth"::email::password::xss -> login email password
    | "dashboard"::xs -> Nothing()
    | _ -> Nothing()

let start args =
    call args |> handleResponse

let testCase args =
    printfn "ARGS:   %A" args
    start args
    printfn "" 
    
let testBadCases() =
    testCase []
    testCase ["garbage"]
    testCase ["garbage"; "auth"]

let test email password =
    testBadCases()
    testCase ["auth"]
    testCase ["auth"; email]
    testCase ["auth"; email; "badpass"]
    testCase ["auth"; "bademail"; password]
    testCase ["auth"; email; password]
    testCase ["dashboard"]

[<EntryPoint>]
let main argv = 
    let args = argv |> Array.toList
    match args with
    | "test"::xs ->
        match xs with
        | email::password::xss -> test email password
        | _ -> testBadCases()
    | _ -> start args
    0 // return an integer exit code




