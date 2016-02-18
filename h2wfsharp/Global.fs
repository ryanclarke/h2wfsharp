﻿namespace H2W

module Util =
    let str (x:'x) = sprintf "%A" x
    let print x = printfn "%s" x

module Error =
    type T = {
        Error: string
        Description: string
    }

    let str (err:T) = sprintf "%s: %s" err.Error err.Description

module Message =
    type MessageLevel =
        | Error
        | Info
        | Result

    type T = {
        Level: MessageLevel
        Payload: string
    }

    let NewMsg level k v = [{Level=level; Payload=(sprintf "%-*s%s" 12 (sprintf "%s:" k) v)}]
    let ErrorMsg = NewMsg Error "ERROR"
    let InfoMsg = NewMsg Info
    let ResultMsg = NewMsg Result

    let DumpMsg (msg:T) =
        Util.print msg.Payload

    let DumpMsgs msgs =
        msgs
        |> List.iter DumpMsg

    let AppendMsgTo text newLine = List.append text newLine
    let AppendMsg newLine text =  AppendMsgTo text newLine
