namespace H2W

module Main =
    [<EntryPoint>]
    let main argv =
        match argv |> Array.toList with
        | "test" :: xs -> Test.Run xs
        | xs -> App.Run xs
        0 // return an integer exit code
