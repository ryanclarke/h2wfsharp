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

    let RunAll email password =
        RunCase []
        RunBadCases()
        RunAuthCases email password
        RunCase ["dashboard"]
