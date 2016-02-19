namespace H2W

module Message =
    type Level =
        | Error
        | Result
        | Info

    type T = {
        Level: Level
        Payload: string
    }

    let NewMsg level k v = [{Level=level; Payload=(sprintf "%-*s%s" 12 (sprintf "%s:" k) v)}]
    let ErrorMsg = NewMsg Level.Error "ERROR"
    let InfoMsg = NewMsg Level.Info
    let ResultMsg = NewMsg Level.Result

    let DumpMsg (msg:T) =
        Util.print msg.Payload

    let DumpMsgs msgs =
        msgs
        |> List.iter DumpMsg

    let AppendMsgTo text newLine = List.append text newLine
    let AppendMsg newLine text =  AppendMsgTo text newLine


