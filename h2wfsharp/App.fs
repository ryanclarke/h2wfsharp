namespace H2W

module App =
    let main args =
        let fiat = ArgParser.Parse args
        match fiat.Invalid with
        | Some(err) ->
            ResponseHandler.ErrorHandler err
        | None ->
            Client.Req(fiat.Endpoint, fiat.Cred)
            |> Client.HitEndpoint
            |> ResponseHandler.HandleResponse fiat.Handler

    let Run args =
        main args
        |> Message.DumpMsgs

    let Test = main
