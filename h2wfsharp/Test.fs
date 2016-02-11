﻿namespace H2W

module Test =
    let RunCase args =
        printfn "ARGS:   %A" args
        App.Run args
        printfn "" 
    
    let RunBadCases () =
        RunCase ["garbage"]
        RunCase ["auth"; "--email"; "bademail"; "--password"; "badpassword"]

    let RunAuthCases email password =
        RunCase ["auth"; "--email"; email; "--password"; password]
        RunCase ["auth"; "verify"]

    let RunDashboardCases () =
        RunCase ["dashboard"]
        RunCase ["dashboard"; "--quickstats"]

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
