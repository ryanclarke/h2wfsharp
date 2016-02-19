namespace H2W

module Error =
    type T = {
        Error: string
        Description: string
    }

    let str (err:T) = sprintf "%s: %s" err.Error err.Description
