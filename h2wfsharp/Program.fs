// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open FSharp.Core
open FSharp.Data
open FSharp.Data.JsonExtensions
open System

type TokenProvider = JsonProvider<""" {"token":"abc" } """>

let auth = "auth"
let dashboard = "dashboard"

type RunResponse =
    | Response of FSharp.Data.HttpResponse
    | Nothing of unit

type EmailAndPassword = {
    email: string
    password: string
}

type Token = string

type Credentials =
    | UserCred of EmailAndPassword
    | TokenCred of Token

type Req = {
    url: string
    cred: Credentials
}

let credString cred =
    match cred with
    | UserCred({email=e; password=p}) -> sprintf "%s:%s" e p
    | TokenCred(t) -> sprintf "%s:" t

let basic cred =
    cred
    |> credString
    |> System.Text.Encoding.UTF8.GetBytes
    |> Convert.ToBase64String
    |> sprintf "Basic %s"

let h2wUrl endpoint = sprintf "https://h2w.cc/api/v1/%s" endpoint

let hitEndpoint req =
    Http.Request(req.url,
        headers = [ "X-H2W-Client-ID", "0"; "Authorization", basic req.cred ],
        silentHttpErrors = true )
    |> Response

let helpText () = printfn "You're gonna need help to do this right."

let handleResponse run =
    match run with
    | Response resp -> 
        printfn "URL:    %A" resp.ResponseUrl
        printfn "STATUS: %A" resp.StatusCode
        match resp.Body with
        | Text(body) -> 
            printfn "BODY:   %A" body
            if resp.StatusCode = 200
            then
                let jv = TokenProvider.Parse(body)
                printfn "TOKEN:  %A" jv.Token
        | Binary(body) -> printfn "BODY:   %A" body
    | _ -> helpText()

let call args =
    match args with
    | "auth"::e::p::xss ->
        { url=h2wUrl "auth/token"; cred=UserCred({ email=e; password=p }) }
        |> hitEndpoint
    | "dashboard"::xs -> Nothing()
    | _ -> Nothing()

let start args = args |> call |> handleResponse

let testCase args =
    printfn "ARGS:   %A" args
    start args
    printfn "" 
    
let testBadCases () =
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




