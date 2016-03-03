namespace H2W

module Client =
    open FSharp.Core
    open FSharp.Data
    open System

    type EmailAndPassword = {
        Email: string
        Password: string
        }

    type Token = string
    type TokenFile = TokenFile of string

    type Credentials =
    | UserCred of EmailAndPassword
    | TokenCred of Token

    type Req(url: string, credentials: Credentials) =
        let credString cred =
            match cred with
            | UserCred({Email=e; Password=p}) -> sprintf "%s:%s" e p
            | TokenCred(t) -> sprintf "%s:" t

        let basic =
            credentials
            |> credString
            |> System.Text.Encoding.UTF8.GetBytes
            |> Convert.ToBase64String
            |> sprintf "Basic %s"

        member this.Url = url
        member this.Cred = credentials
        member this.BasicAuthorization = basic

    let h2wUrl endpoint = sprintf "https://www.h2w.cc/api/v1/%s" endpoint

    let HitEndpoint (req:Req) =
        Http.Request(req.Url,
            headers = [ "X-H2W-Client-ID", "0"; "Authorization", req.BasicAuthorization ],
            silentHttpErrors = true)
