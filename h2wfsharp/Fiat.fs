namespace H2W

open Client
    
module Fiat =
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

    let DefaultFiat = {
        Endpoint="";
        Email="";
        Password="";
        Token="";
        File=".h2wfsharptoken";
        Cred=TokenFile(".h2wfsharptoken");
        Quickstats=false;
        Handler=(fun s -> ())
        }

    let withUserCred fiat = {fiat with Cred=UserCred({Email=fiat.Email; Password=fiat.Password})}
    let withTokenFile fiat = {fiat with Cred=TokenFile(fiat.File)}
    let withUrl url fiat = {fiat with Endpoint=(h2wUrl url)}
    let withHandler handler fiat = {fiat with Handler=handler}
    let withTokenHandler fiat = {fiat with Handler=(tokenHandler fiat.File)}
    let withDashboardHandler fiat =
        match fiat.Quickstats with
        | true -> withHandler quickstatsHandler fiat
        | false -> withHandler dashboardHandler fiat
