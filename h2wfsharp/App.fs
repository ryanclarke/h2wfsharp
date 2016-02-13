namespace H2W

module App =
    let Run args =
        let fiat = ArgParser.Parse args
        match fiat.InvalidFiat with
        | Some(err) ->
            printfn "%s: '%s'" err.Error err.Description
            ResponseHandler.helpText()
        | None ->
            match fiat.Endpoint with
            | "" -> ResponseHandler.helpText()
            | _ ->
                Client.Req(fiat.Endpoint, fiat.Cred)
                |> Client.HitEndpoint
                |> ResponseHandler.HandleResponse fiat.Handler
