// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

namespace H2W

open FSharp.Core
open FSharp.Data
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
        Email: string
        Password: string
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
            | UserCred({Email=e; Password=p}) -> sprintf "%s:%s" e p
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
        Endpoint: string
        Email: string
        Password: string
        Token: string
        File: string
        Cred: Credentials
        Handler: string -> unit
        }

    let rec parseFlags (command:Command) args =
        match args with
        | "-e" :: email :: x -> 
            parseFlags {command with Email=email} x
        | "-p" :: password :: x ->
            parseFlags {command with Password=password} x
        | "-t" :: token :: x ->
            parseFlags {command with Token=token} x
        | "-f" :: file :: x ->
            parseFlags {command with File=file} x
        | _ -> command

    let setUserCred command = {command with Cred=UserCred({Email=command.Email; Password=command.Password})}
    let setTokenFile command = {command with Cred=TokenFile(command.File)}

    let parse (command:Command) args =
        match args with
        | "auth" :: "verify" :: x -> parseFlags {command with Endpoint="auth/token/verify"} x |> setTokenFile
        | "auth" :: x -> parseFlags {command with Endpoint="auth/token"; Handler=(tokenHandler command.File)} x |> setUserCred
        | "dashboard" :: "quickstats" :: x -> parseFlags {command with Endpoint="dashboard"; Handler=quickstatsHandler} x |> setTokenFile
        | "dashboard" :: x -> parseFlags {command with Endpoint="dashboard"; Handler=dashboardHandler} x |> setTokenFile
        | _ -> command

    let Start args =
        let command = parse {Endpoint=""; Email=""; Password=""; Token=""; File=".h2wfsharptoken"; Cred=TokenFile(".h2wfsharptoken"); Handler=(fun s -> ())} args
        match command.Endpoint with
        | "" -> Nothing() |> handleResponse command.Handler
        | _ ->
            let req = Req(h2wUrl command.Endpoint, command.Cred)
            hitEndpoint req
            |> handleResponse command.Handler

