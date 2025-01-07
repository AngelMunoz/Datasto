module Datasto.Pages

open IcedTasks

open Microsoft.AspNetCore.Antiforgery

open StarFederation.Datastar.DependencyInjection
open Falco
open Hox
open System.Security.Claims
open Microsoft.AspNetCore.Authentication.Cookies
open System

[<Struct>]
type LoginFields =
  | Email
  | Password
  | Both

[<return: Struct>]
let (|MissingCredentials|_|) fields =
  match fields with
  | [ "email"; "password" ]
  | [ "password"; "email" ] -> ValueSome Both
  | [ "email" ] -> ValueSome Email
  | [ "password" ] -> ValueSome Password
  | _ -> ValueNone

let login: HttpHandler =
  fun ctx -> taskUnit {
    let af = ctx.Plug<IAntiforgery>()
    let tokenSet = af.GetAndStoreTokens ctx

    let reqData = Request.getQuery ctx
    let missingFields = reqData.GetStringNonEmptyList "missingField"
    let message = reqData.TryGetString "message"

    let content =
      Layout.Page(
        h(
          "article",
          h("h1", "Login"),
          h(
            "form"
            , h(
              $"input[type=hidden][name={tokenSet.FormFieldName}][value={tokenSet.RequestToken}]"
            )
            , match missingFields with
              | MissingCredentials Both ->
                h("p", "Please enter both email and password")
              | MissingCredentials Email -> h("p", "Please enter your email")
              | MissingCredentials Password ->
                h("p", "Please enter your password")
              | _ -> empty
            , h("input[name=username][type=text][required]")
            , h("input[name=password][type=password][required]")
            , match message with
              | Some msg -> h("p", msg)
              | None -> empty
            , h("button", "Login")
          )
        )
      )

    return! Response.ofHox content ctx
  }

let loginPost: HttpHandler =
  fun ctx -> taskUnit {
    let! postedData = Request.getFormSecure ctx

    match postedData with
    | Some formData ->
      let username = formData.GetStringNonEmpty("username", "")
      let password = formData.GetStringNonEmpty("password", "")

      if username = "admin" && password = "password" then
        let claims =
          ClaimsPrincipal(
            ClaimsIdentity(
              [
                Claim(ClaimTypes.Name, "admin")
                Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
              ],
              CookieAuthenticationDefaults.AuthenticationScheme
            )
          )

        return!
          Response.signInAndRedirect
            CookieAuthenticationDefaults.AuthenticationScheme
            claims
            "/admin"
            ctx
      else
        return!
          Response.redirectTemporarily "/login?message=Wrong Credentials" ctx
    | None ->
      return!
        Response.redirectTemporarily
          "/login?missingField=email&missingField=password"
          ctx

  }

let logoutPost: HttpHandler =
  Response.signOutAndRedirect
    CookieAuthenticationDefaults.AuthenticationScheme
    "/login"
