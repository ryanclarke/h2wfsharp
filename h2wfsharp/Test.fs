namespace H2W

module Test =
    let RunCase args =
        let vargs = List.append args ["-v"]
        let msgs = App.Test vargs
        (fst msgs,
            snd msgs
            |> (vargs
                |> Util.str 
                |> Message.InfoMsg "ARGS"
                |> Message.AppendMsgTo))
        |> Message.DumpMsgs
        Util.print " "

    let RunBadCases () =
        RunCase ["garbage"]
        RunCase ["dashboard"; "--invalid-arg"]
        RunCase ["dashboard"; "--max-lines"]
        RunCase ["dashboard"; "--max-lines"; "string"]
        RunCase ["auth"; "--email"; "bademail"; "--password"; "badpassword"]

    let RunAuthCases email password =
        RunCase ["auth"; "--email"; email; "--password"; password]
        RunCase ["auth"; "verify"]

    let RunDashboardCases () =
        RunCase ["dashboard"; "-m"; "5"]
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
