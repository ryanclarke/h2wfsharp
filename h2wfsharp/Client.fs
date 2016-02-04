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

    let bodyPreview (body:string) =
        match body.Length with
        | length when length < 80 -> sprintf "%s" (body.Substring (0, length))
        | _ -> sprintf "%s ..." (body.Substring (0, 76))

    let tokenHandler body =
        TokenProvider.Parse(body).Token
        |> storeToken
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
        decimal dash.TodaySteps / decimal onTrackToday

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

    let rec flagParser userCred flags =
        match flags with
        | "-e" :: email :: x ->
            flagParser {userCred with email=email} x
        | "-p" :: password :: x ->
            flagParser {userCred with password=password} x
        | _ -> userCred

    let userCredParser flags = flagParser {email=""; password=""} flags

    let authReq args =
        match args with
        | "verify" :: x ->
            Req(h2wUrl "auth/token/verify", getToken())
        | x ->
            Req(h2wUrl "auth/token", UserCred(userCredParser x))

    let dashboardResp args =
        match args with
        | "quickstats" :: x -> handleResponse quickstatsHandler
        | _ -> handleResponse dashboardHandler

    let Start args =
        match args with
        | "auth" :: x ->
            authReq x
            |> hitEndpoint
            |> handleResponse tokenHandler
        | "dashboard" :: x -> 
            Req(h2wUrl "dashboard", getToken())
            |> hitEndpoint
            |> dashboardResp x
        | _ -> Nothing() |> handleResponse (fun s -> ())
