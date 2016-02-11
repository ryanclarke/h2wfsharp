namespace H2W

module ResponseHandler =
    open FSharp.Data
    open System
    open System.IO
    open System.Linq

    type DashboardProvider = JsonProvider<"Dashboard.json">
    type TokenProvider = JsonProvider<""" {"token":"guid string"} """>
    type ErrorProvider = JsonProvider<""" {"error":"http error"} """>

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

    let HandleResponse onSuccess resp =
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
