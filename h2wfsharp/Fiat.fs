namespace H2W

module Fiat =
    open Client
    open System.IO

    type Fiat = {
        Endpoint: string
        Email: string
        Password: string
        Token: string
        File: string
        Cred: Credentials
        Quickstats: bool
        Handler: string -> unit
        }

    let getToken tokenFile = File.ReadAllLines(tokenFile).[0]
    let getTokenCredFromFile tokenFile = TokenCred(getToken tokenFile)

    let DefaultFiat = {
        Endpoint="";
        Email="";
        Password="";
        Token="";
        File=".h2wfsharptoken";
        Cred=(getTokenCredFromFile ".h2wfsharptoken");
        Quickstats=false;
        Handler=(fun s -> ())
        }

    let withUserCred fiat = {fiat with Cred=UserCred({Email=fiat.Email; Password=fiat.Password})}
    let withTokenFile fiat = {fiat with Cred=(getTokenCredFromFile fiat.File)}
    let withUrl url fiat = {fiat with Endpoint=(h2wUrl url)}
    let withHandler handler fiat = {fiat with Handler=handler}
    let withTokenHandler fiat = {fiat with Handler=(ResponseHandler.tokenHandler fiat.File)}
    let withDashboardHandler fiat =
        match fiat.Quickstats with
        | true -> withHandler ResponseHandler.quickstatsHandler fiat
        | false -> withHandler ResponseHandler.dashboardHandler fiat
