namespace H2W

module Main =
    [<EntryPoint>]
    let main argv =
        match argv |> Array.toList with
        | "test" :: x -> Test.Run x
        | x -> App.Run x
        0 // return an integer exit code
