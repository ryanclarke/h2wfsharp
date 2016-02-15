namespace H2W

module ResponseHandler =
    open FSharp.Data
    open System
    open System.IO
    open System.Linq

    type DashboardProvider = JsonProvider<"Dashboard.json">
    type TokenProvider = JsonProvider<""" {"token":"guid string"} """>
    type ErrorProvider = JsonProvider<""" {"error":"http error"} """>

    let appendTo text newLine = sprintf "%s\n%s" text newLine
    let append newLine text =  appendTo text newLine

    let helpText () = sprintf "You're gonna need help to do this right."
    let ErrorHandler (err:Error) =
        sprintf "%s: '%s'" err.Error err.Description
        |> append (helpText())

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
        |> sprintf "TOKEN:  %A"

    let niceNumber format (n:'a) = String.Format(format, n)
    let niceInt (i:int) = niceNumber "{0:N0}" i
    let nicePct (d:decimal) = niceNumber "{0:N2}%" d

    let dashboardHandler body =
        let prettyPrint = DashboardProvider.Parse(body).JsonValue.ToString()
        let lines = prettyPrint.Split('\n')
        let trimmedLines = lines.Take(5).Concat(["..."])
        trimmedLines.Aggregate(fun sum line-> (appendTo sum line))

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
        |> sprintf "STATS:  %s"

    let parseBody onSuccess resp =
        match resp.Body with
        | Text(body) ->
            let n = appendTo (sprintf "BODY:   %A" (bodyPreview body))
            match resp.StatusCode with
            | 200 ->
                onSuccess body
                |> n
            | _ ->
                ErrorProvider.Parse(body).Error
                |> sprintf "ERROR:  %A"
                |> n
        | Binary(body) ->
            match resp.StatusCode with
            | 204 -> sprintf "BODY:   <NO CONTENT>"
            | _ -> sprintf "BODY:   %A" body

    let HandleResponse onSuccess resp =
        sprintf "URL:    %A" resp.ResponseUrl
        |> append (sprintf "STATUS: %A" resp.StatusCode)
        |> append (parseBody onSuccess resp)

