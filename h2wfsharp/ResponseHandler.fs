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
    let appendS newLine text =  appendTo text newLine
    let appendToL text newLine = List.append text newLine
    let append newLine text =  appendToL text newLine

    let helpText () = Message.Result "HELP" "You're gonna need help to do this right."
    let ErrorHandler (err:Error) =
        Message.Error err.Error err.Description
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
        |> Message.Result "TOKEN"

    let sformat format (x:'a) = String.Format(format, x)
    let niceInt (i:int) = sformat "{0:N0}" i
    let nicePct (d:decimal) = sformat "{0:N2}%" d

    let dashboardHandler lines body =
        DashboardProvider.Parse(body).JsonValue.ToString().Split('\n')
        |> Seq.take lines
        |> Seq.reduce appendTo
        |> appendS "..."
        |> Message.Info "CONTENT"

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
        |> Message.Result "STATS"

    let parseBody onSuccess resp =
        match resp.Body with
        | Text(body) ->
            let bodyString = appendToL (Message.Info "BODY" (bodyPreview body))
            match resp.StatusCode with
            | 200 ->
                onSuccess body
                |> bodyString
            | _ ->
                ErrorProvider.Parse(body).Error
                |> Message.Error "ERROR"
                |> bodyString
        | Binary(body) ->
            match resp.StatusCode with
            | 204 -> Message.Info "BODY" "<NO CONTENT>"
            | _ -> Message.Info "BODY" (sprintf "%A" body)

    let HandleResponse onSuccess resp =
        Message.Info "URL" resp.ResponseUrl
        |> append (Message.Info "STATUS" (sprintf "%A" resp.StatusCode))
        |> append (parseBody onSuccess resp)

