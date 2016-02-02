// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

namespace H2W

open FSharp.Core
open FSharp.Data
open FSharp.Data.JsonExtensions
open System
open System.IO

module Test =
    let RunCase args =
        printfn "ARGS:   %A" args
        Client.Start args
        printfn "" 
    
    let RunBadCases () =
        RunCase ["garbage"]
        RunCase ["garbage"; "auth"]

    let RunAuthCases email password =
        RunCase ["auth"]
        RunCase ["auth"; email]
        RunCase ["auth"; email; "badpass"]
        RunCase ["auth"; "bademail"; password]
        RunCase ["auth"; email; password]
        RunCase ["auth"; "-e"; email; "-p"; password]
        RunCase ["auth"; "verify"]

    let RunDashboardCases () =
        RunCase ["dashboard"]

    let RunAll email password =
        RunCase []
        RunBadCases()
        RunAuthCases email password
        RunDashboardCases()

    let Run args =
        match args with
        | "all" :: email :: password :: xs -> RunAll email password
        | "auth" :: email :: password :: xs -> RunAuthCases email password
        | "bad" :: xs -> RunBadCases()
        | "dashboard" :: xs -> RunDashboardCases()
        | email :: password :: xs -> RunAll email password
        | _ -> ()
