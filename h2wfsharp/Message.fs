namespace H2W

module Message =
    type Level =
        | Result
        | Error
        | Info

    type T = {
        Level: Level
        Title: string
        Payload: string
    }

    let NewMsg level k v = [{Level=level; Title=k; Payload=v}]
    let ErrorMsg = NewMsg Level.Error "ERROR"
    let InfoMsg = NewMsg Level.Info
    let ResultMsg = NewMsg Level.Result

    let PrettyString (msg:T) =
        sprintf "%-*s%s" 12 (sprintf "%s:" msg.Title) msg.Payload

    let PlainString (msg:T) =
        msg.Payload

    let DumpMsg b (msg:T) =
        Util.print (msg |>
            match b with
            | true -> PrettyString
            | false -> PlainString)

    let DumpMsgs msgs =
        snd msgs
        |> List.iter (fst msgs |> DumpMsg)

    let Filter (msgs:bool * T list) =
        let newmsgs =
            match fst msgs with
            | true -> snd msgs
            | false -> snd msgs |> List.filter (fun x -> x.Level <= Level.Error)
        (fst msgs, newmsgs)

    let AppendMsgTo text newLine = List.append text newLine
    let AppendMsg newLine text =  AppendMsgTo text newLine


