// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

namespace H2W

open FSharp.Core
open FSharp.Data
open FSharp.Data.JsonExtensions
open System
open System.IO

module Main =
    [<EntryPoint>]
    let main argv =
        let args = argv |> Array.toList
        match args with
        | "test"::xs -> Test.Run xs
        | _ -> Client.Start args
        0 // return an integer exit code
