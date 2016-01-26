// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open FSharp.Data
open System

let auth = "auth"
let dashboard = "dashboard"

type RunResponse = 
    | Response of FSharp.Data.HttpResponse
    | Nothing of unit

type EmailAndPassword = string * string
type Token = string

type Credentials = 
    | EmailAndPassword of string * string
    | Token of string

let credString (cred:Credentials) = 
    match cred with
    | Credentials.EmailAndPassword(e, p) -> sprintf "%s:%s" e p
    | Credentials.Token(t) -> sprintf "%s:" t

let basic cred = 
    cred
    |> credString
    |> System.Text.Encoding.UTF8.GetBytes
    |> Convert.ToBase64String
    |> sprintf "Basic %s"

let fetchUrl url cred =  
    Http.Request(sprintf "https://h2w.cc/api/v1/%s" url, 
        headers = [ "X-H2W-Client-ID", "0"; "Authorization", basic cred ], 
        silentHttpErrors = true )
    |> Response

let login cred = fetchUrl "auth/token" cred

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
    | "auth"::email::password::xss ->
        Credentials.EmailAndPassword(email, password)
        |> login
    | "dashboard"::xs -> Nothing()
    | _ -> Nothing()

let start args = args |> call |> handleResponse

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




