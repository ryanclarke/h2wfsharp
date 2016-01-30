// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open FSharp.Core
open FSharp.Data
open FSharp.Data.JsonExtensions
open System
open System.IO

module Client =
    type TokenProvider = JsonProvider<""" {"token":"guid string"} """>
    type ErrorProvider = JsonProvider<""" {"error":"http error"} """>

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

    type Req(url: string, credentials: Credentials) =
        let credString cred =
            match cred with
            | UserCred({email=e; password=p}) -> sprintf "%s:%s" e p
            | TokenCred(t) -> sprintf "%s:" t

        let basic =
            credentials
            |> credString
            |> System.Text.Encoding.UTF8.GetBytes
            |> Convert.ToBase64String
            |> sprintf "Basic %s"

        member this.Url = url
        member this.Cred = credentials
        member this.BasicAuthorization = basic

    let h2wUrl endpoint = sprintf "https://h2w.cc/api/v1/%s" endpoint

    let hitEndpoint (req:Req) =
        Http.Request(req.Url,
            headers = [ "X-H2W-Client-ID", "0"; "Authorization", req.BasicAuthorization ],
            silentHttpErrors = true )
        |> Response

    let helpText () = printfn "You're gonna need help to do this right."

    let storeToken token =
        File.WriteAllLines(".h2wfsharptoken", [token])
        token

    let getToken () = File.ReadAllLines(".h2wfsharptoken").[0] |> TokenCred

    let handleResponse run =
        match run with
        | Response resp -> 
            printfn "URL:    %A" resp.ResponseUrl
            printfn "STATUS: %A" resp.StatusCode
            match resp.Body with
            | Text(body) -> 
                printfn "BODY:   %A" body
                match resp.StatusCode with
                | 200 ->
                    match TokenProvider.Parse(body).Token with
                    | "" -> ()
                    | t -> storeToken t |> printfn "TOKEN:  %A"
                | _ ->
                    ErrorProvider.Parse(body).Error
                    |> printfn "ERROR:  %A"
            | Binary(body) ->
                match resp.StatusCode with
                | 204 -> printfn "BODY:   <NO CONTENT>"
                | _ -> printfn "BODY:   %A" body
        | _ -> helpText()

    let authCall args =
        match args with
        | email :: password :: x ->
            Req(h2wUrl "auth/token", UserCred({ email=email; password=password }))
            |> hitEndpoint
        | "verify" :: x ->
            Req(h2wUrl "auth/token/verify", getToken())
            |> hitEndpoint
        | _ -> Nothing()

    let call args =
        match args with
        | "auth" :: x -> authCall x
        | "dashboard" :: x ->
            Req(h2wUrl "dashboard", getToken())
            |> hitEndpoint
        | _ -> Nothing()

    let Start args = args |> call |> handleResponse

module Test =
    let RunCase args =
        printfn "ARGS:   %A" args
        Client.Start args
        printfn "" 
    
    let RunBadCases () =
        RunCase ["garbage"]
        RunCase ["garbage"; "auth"]

    let RunAuthCases email password =
        RunCase ["auth"]
        RunCase ["auth"; email]
        RunCase ["auth"; email; "badpass"]
        RunCase ["auth"; "bademail"; password]
        RunCase ["auth"; email; password]
        RunCase ["auth"; "verify"]

    let RunAll email password =
        RunCase []
        RunBadCases()
        RunAuthCases email password
        RunCase ["dashboard"]

[<EntryPoint>]
let main argv =
    let args = argv |> Array.toList
    match args with
    | "test"::xs ->
        match xs with
        | email::password::xss -> Test.RunAll email password
        | _ -> Test.RunBadCases()
    | _ -> Client.Start args
    0 // return an integer exit code
