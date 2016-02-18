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

    type Message = {
        Level: MessageLevel
        Payload: string
    }

    let b level payload = [{Level=level; Payload=payload}]
    let New level k v = b level (sprintf "%s:   %s" k v) 
    let Error = New Error
    let Info = New Info
    let Result = New Result

    let Dump (msg:Message) =
        printfn "%s" msg.Payload

    let DumpAll msgs =
        msgs
        |> List.iter Dump
