namespace H2W

type Error = {
    Error: string
    Description: string
}

module Message =
    type MessageLevel =
        | Error
        | Info
        | Result

    type T = {
        Level: MessageLevel
        Payload: string
    }

    let New level k v = [{Level=level; Payload=(sprintf "%-*s%s" 12 (sprintf "%s:" k) v)}]
    let Error = New Error "ERROR"
    let Info = New Info
    let Result = New Result

    let Dump (msg:T) =
        printfn "%s" msg.Payload

    let DumpAll msgs =
        msgs
        |> List.iter Dump

    let AppendTo text newLine = List.append text newLine
    let Append newLine text =  AppendTo text newLine
