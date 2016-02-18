namespace H2W

module ResponseHandler =
    open FSharp.Data
    open Message
    open System
    open System.IO
    open System.Linq

    type DashboardProvider = JsonProvider<"Dashboard.json">
    type TokenProvider = JsonProvider<""" {"token":"guid string"} """>
    type ErrorProvider = JsonProvider<""" {"error":"http error"} """>

    let appendTo text newLine = sprintf "%s\n%s" text newLine
    let append newLine text =  appendTo text newLine

    let helpText () = ResultMsg "HELP" "You're gonna need help to do this right."
    let ErrorHandler (err:Error) =
        let sprinterr (err:Error) = sprintf "%s: %s" err.Error err.Description
        ErrorMsg (sprinterr err)
        |> AppendMsg (helpText())

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
        |> ResultMsg "TOKEN"

    let sformat format (x:'a) = String.Format(format, x)
    let niceInt (i:int) = sformat "{0:N0}" i
    let nicePct (d:decimal) = sformat "{0:N2}%" d

    let dashboardHandler lines body =
        DashboardProvider.Parse(body).JsonValue.ToString().Split('\n')
        |> Seq.take lines
        |> Seq.reduce appendTo
        |> appendTo ""
        |> append "..."
        |> InfoMsg "JSON"

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
        |> ResultMsg "STATS"

    let parseBody onSuccess resp =
        match resp.Body with
        | Text(body) ->
            let bodyString = AppendMsgTo (InfoMsg "BODY" (bodyPreview body))
            match resp.StatusCode with
            | 200 ->
                onSuccess body
                |> bodyString
            | _ ->
                ErrorProvider.Parse(body).Error
                |> ErrorMsg
                |> bodyString
        | Binary(body) ->
            match resp.StatusCode with
            | 204 -> InfoMsg "BODY" "<NO CONTENT>"
            | _ -> InfoMsg "BODY" (sprintf "%A" body)

    let HandleResponse onSuccess resp =
        InfoMsg "URL" resp.ResponseUrl
        |> AppendMsg (InfoMsg "STATUS" (sprintf "%A" resp.StatusCode))
        |> AppendMsg (parseBody onSuccess resp)

