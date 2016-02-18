namespace H2W

module App =
    let private doIt args =
        let fiat = ArgParser.Parse args
        match fiat.Invalid with
        | Some(err) ->
            ResponseHandler.ErrorHandler err
        | None ->
            Client.Req(fiat.Endpoint, fiat.Cred)
            |> Client.HitEndpoint
            |> ResponseHandler.HandleResponse fiat.Handler

    let Run args =
        doIt args |> Message.DumpAll

    let Test = doIt
