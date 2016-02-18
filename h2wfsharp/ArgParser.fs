namespace H2W

module ArgParser =
    open Fiat
    open System

    let Parse args =
        let rec parseFlags (fiat:Fiat) =
            let badArg arg = asInvalid "Invalid Arg" arg fiat
            let badArgType arg t = badArg (sprintf "%s must be a %s" arg t)
            function
            | [] -> fiat
            | ("-e" | "--email") :: email :: x -> 
                parseFlags {fiat with Email=email} x
            | ("-f" | "--file") :: file :: x ->
                parseFlags {fiat with File=file} x
            | ("-m" | "--max-lines") :: head :: x -> 
                let h = Int32.TryParse(head)
                match (fst h) with
                | true ->
                    parseFlags {fiat with MaxLines=Some(snd h)} x
                | false ->
                    badArgType head "number"
            | ("-p" | "--password") :: password :: x ->
                parseFlags {fiat with Password=password} x
            | ("-q" | "--quickstats") :: x ->
                parseFlags {fiat with Quickstats=true} x
            | ("-t" | "--token") :: token :: x ->
                parseFlags {fiat with Token=token} x
            | ("-v" | "--verbose") :: x ->
                parseFlags {fiat with Verbose=true} x
            | x ->
                badArg (List.head x)

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
            DefaultFiat
            |> asInvalid "No Endpoint" "Must specify an endpoint"
        | x ->
            DefaultFiat
            |> asInvalid "Invalid Endpoint" x.[0]
