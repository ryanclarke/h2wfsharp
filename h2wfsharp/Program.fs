namespace H2W

module Main =
    [<EntryPoint>]
    let main argv =
        let args = argv |> Array.toList
        match args with
        | "test" :: xs -> Test.Run xs
        | _ -> App.Run args
        0 // return an integer exit code
