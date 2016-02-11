namespace H2W

module App =
    let Run args =
        let fiat = ArgParser.Parse args
        match fiat.Endpoint with
        | "" -> ResponseHandler.helpText()
        | _ ->
            Client.Req(fiat.Endpoint, fiat.Cred)
            |> Client.HitEndpoint
            |> ResponseHandler.HandleResponse fiat.Handler
