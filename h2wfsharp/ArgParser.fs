namespace H2W

module ArgParser =
    open Fiat

    let Parse args =
        let rec parseFlags (fiat:Fiat) =
            function
            | [] -> fiat
            | ("-e" | "--email") :: email :: x -> 
                parseFlags {fiat with Email=email} x
            | ("-p" | "--password") :: password :: x ->
                parseFlags {fiat with Password=password} x
            | ("-t" | "--token") :: token :: x ->
                parseFlags {fiat with Token=token} x
            | ("-f" | "--file") :: file :: x ->
                parseFlags {fiat with File=file} x
            | ("-q" | "--quickstats") :: x ->
                parseFlags {fiat with Quickstats=true} x
            | x ->
                fiat |> asInvalid "Invalid Arg" x.[0]

        match args with
        | "auth" :: "verify" :: x ->
            parseFlags DefaultFiat x
            |> withUrl "auth/token/verify"
            |> withTokenFile
        | "auth" :: x ->
            parseFlags DefaultFiat x
            |> withUrl "auth/token"
            |> withUserCred
            |> withTokenHandler
        | "dashboard" :: x ->
            parseFlags DefaultFiat x
            |> withUrl "dashboard"
            |> withTokenFile
            |> withDashboardHandler
        | [] ->
            DefaultFiat |> asInvalid "No Endpoint" "Must specify an endpoint"
        | x ->
            DefaultFiat |> asInvalid "Invalid Endpoint" x.[0]
