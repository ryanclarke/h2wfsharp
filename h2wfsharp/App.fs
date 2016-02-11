namespace H2W

module App =
    open ArgParser
    open Client
    open Fiat

    let Run args =
        let fiat = Parse args
        match fiat.Endpoint with
        | "" -> Nothing() |> HandleResponse fiat.Handler
        | _ ->
            Req(fiat.Endpoint, fiat.Cred)
            |> HitEndpoint
            |> HandleResponse fiat.Handler
