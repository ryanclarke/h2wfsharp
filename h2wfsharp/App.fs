namespace H2W

module App =
    open ArgParser
    open Client
    open Fiat

    let Run args =
        let fiat = Parse args
        match fiat.Endpoint with
        | "" -> Nothing() |> handleResponse fiat.Handler
        | _ ->
            Req(fiat.Endpoint, fiat.Cred)
            |> hitEndpoint
            |> handleResponse fiat.Handler

