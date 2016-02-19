namespace H2W

module App =
    let main args =
        let fiat = ArgParser.Parse args
        let msgs = match fiat.Invalid with
            | Some(err) ->
                ResponseHandler.ErrorHandler err
            | None ->
                Client.Req(fiat.Endpoint, fiat.Cred)
                |> Client.HitEndpoint
                |> ResponseHandler.HandleResponse fiat.Handler
        match fiat.Verbose with
        | true ->
            msgs
        | false ->
            msgs
            |> List.filter (fun x -> x.Level <= Message.Level.Result)

    let Run args =
        main args
        |> Message.DumpMsgs

    let Test = main
