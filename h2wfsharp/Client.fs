// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

namespace H2W

open FSharp.Core
open FSharp.Data
open FSharp.Data.JsonExtensions
open System
open System.IO
open System.Linq

module Client =
    type DashboardProvider = JsonProvider<"Dashboard.json">
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
    type TokenFile = string

    type Credentials =
    | UserCred of EmailAndPassword
    | TokenCred of Token
    | TokenFile of TokenFile

    let getToken tokenFile = File.ReadAllLines(tokenFile).[0]

    type Req(url: string, credentials: Credentials) =
        let credString cred =
            match cred with
            | UserCred({email=e; password=p}) -> sprintf "%s:%s" e p
            | TokenCred(t) -> sprintf "%s:" t
            | TokenFile(f) -> getToken f |> sprintf "%s:"

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

    let storeToken tokenFile token =
        File.WriteAllLines(tokenFile, [token])
        token


    let bodyPreview (body:string) =
        match body.Length with
        | length when length < 80 -> sprintf "%s" (body.Substring (0, length))
        | _ -> sprintf "%s ..." (body.Substring (0, 76))

    let tokenHandler tokenFile body =
        TokenProvider.Parse(body).Token
        |> storeToken tokenFile
        |> printfn "TOKEN:  %A"

    let niceNumber format (n:'a) = String.Format(format, n)
    let niceInt (i:int) = niceNumber "{0:N0}" i
    let nicePct (d:decimal) = niceNumber "{0:N2}%" d

    let dashboardHandler body =
        DashboardProvider.Parse(body).Dashboard.CurrentSteps
        |> niceInt
        |> printfn "STEPS:  %s"

    let todayPercent (dash:DashboardProvider.Dashboard) =
        let onTrackToday = dash.TodayStepGoals.Where(fun x -> x.OnTrack.IsSome).Single().Steps
        decimal dash.TodaySteps / decimal onTrackToday * 100m

    let quickstatsHandler body =
        let dash = DashboardProvider.Parse(body).Dashboard

        let todaySteps = dash.TodaySteps |> niceInt
        let todayPct = todayPercent dash |> nicePct
        let weekSteps = dash.CurrentSteps |> niceInt
        let weekPct = dash.WeekFullPct |> nicePct

        sprintf "today: %s (%s); week: %s (%s)" todaySteps todayPct weekSteps weekPct
        |> printfn "STATS:  %s"

    let handleResponse onSuccess run =
        match run with
        | Response resp ->
            printfn "URL:    %A" resp.ResponseUrl
            printfn "STATUS: %A" resp.StatusCode
            match resp.Body with
            | Text(body) ->
                printfn "BODY:   %A" (bodyPreview body)
                match resp.StatusCode with
                | 200 -> onSuccess body
                | _ ->
                    ErrorProvider.Parse(body).Error
                    |> printfn "ERROR:  %A"
            | Binary(body) ->
                match resp.StatusCode with
                | 204 -> printfn "BODY:   <NO CONTENT>"
                | _ -> printfn "BODY:   %A" body
        | Nothing _ -> helpText()

    type Command = {
        endpoint: string
        email: string
        password: string
        token: string
        file: string
        cred: Credentials
        handler: string -> unit
        }

    let rec parseFlags (command:Command) args =
        match args with
        | "-e" :: email :: x -> 
            parseFlags {command with email=email} x
        | "-p" :: password :: x ->
            parseFlags {command with password=password} x
        | "-t" :: token :: x ->
            parseFlags {command with token=token} x
        | "-f" :: file :: x ->
            parseFlags {command with file=file} x
        | _ -> command

    let setUserCred command = {command with cred=UserCred({email=command.email; password=command.password})}
    let setTokenFile command = {command with cred=TokenFile(command.file)}

    let parse (command:Command) args =
        match args with
        | "auth" :: "verify" :: x -> parseFlags {command with endpoint="auth/token/verify"} x |> setTokenFile
        | "auth" :: x -> parseFlags {command with endpoint="auth/token"; handler=(tokenHandler command.file)} x |> setUserCred
        | "dashboard" :: "quickstats" :: x -> parseFlags {command with endpoint="dashboard"; handler=quickstatsHandler} x |> setTokenFile
        | "dashboard" :: x -> parseFlags {command with endpoint="dashboard"; handler=dashboardHandler} x |> setTokenFile
        | _ -> command

    let Start args =
        let command = parse {endpoint=""; email=""; password=""; token=""; file=".h2wfsharptoken"; cred=TokenFile(".h2wfsharptoken"); handler=(fun s -> ())} args
        match command.endpoint with
        | "" -> Nothing() |> handleResponse command.handler
        | _ ->
            let req = Req(h2wUrl command.endpoint, command.cred)
            hitEndpoint req
            |> handleResponse command.handler

