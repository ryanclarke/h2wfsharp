﻿namespace H2W

open Client
open Fiat

module ArgParser =
    let rec parseFlags args (fiat:Fiat) =
        match args with
        | ("-e" | "--email") :: email :: x -> 
            parseFlags x {fiat with Email=email}
        | ("-p" | "--password") :: password :: x ->
            parseFlags x {fiat with Password=password}
        | ("-t" | "--token") :: token :: x ->
            parseFlags x {fiat with Token=token}
        | ("-f" | "--file") :: file :: x ->
            parseFlags x {fiat with File=file}
        | ("-q" | "--quickstats") :: x -> parseFlags x {fiat with Quickstats=true}
        | _ -> fiat

    let Parse args =
        match args with
        | "auth" :: "verify" :: x ->
            DefaultFiat
            |> withUrl "auth/token/verify"
            |> parseFlags x
            |> withTokenFile
        | "auth" :: x ->
            DefaultFiat
            |> withUrl "auth/token"
            |> parseFlags x
            |> withUserCred
            |> withTokenHandler
        | "dashboard" :: x ->
            DefaultFiat
            |> withUrl "dashboard"
            |> parseFlags x
            |> withTokenFile
            |> withDashboardHandler
        | _ -> DefaultFiat

