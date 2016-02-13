namespace H2W

module App =
    let Run args =
        let fiat = ArgParser.Parse args
        match fiat.InvalidFiat with
        | Some(err) ->
            Out.ErrorText err
            ResponseHandler.helpText()
        | None ->
            Client.Req(fiat.Endpoint, fiat.Cred)
            |> Client.HitEndpoint
            |> ResponseHandler.HandleResponse fiat.Handler
