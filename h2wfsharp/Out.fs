namespace H2W

module Out =
    let ErrorText (err:Fiat.Error) =
        printfn "%s: '%s'" err.Error err.Description

