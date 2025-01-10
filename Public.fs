module Datasto.Pages

open IcedTasks

open Microsoft.AspNetCore.Antiforgery

open Falco
open Hox
open System.Security.Claims
open Microsoft.AspNetCore.Authentication.Cookies
open System

[<Struct>]
type LoginFormExtraField =
  | Email
  | Password
  | EmailAndPassword

[<return: Struct>]
let (|MissingCredentials|_|) fields =
  match fields with
  | [ "email"; "password" ]
  | [ "password"; "email" ] -> ValueSome EmailAndPassword
  | [ "email" ] -> ValueSome Email
  | [ "password" ] -> ValueSome Password
  | _ -> ValueNone

module Fragments =

  let inputsWithMissingFields
    (missingFields: string list, email: string option)
    =
    let email =
      let email = defaultArg email ""
      h($"input[name=email][type=email][required][value={email}]")

    let password = h("input[name=password][type=password][required]")

    fragment(
      [
        match missingFields with
        | [] ->
          email
          password
        | MissingCredentials Email ->
          email
          h("p", "Please enter your email")
          password
        | MissingCredentials Password ->
          email
          password
          h("p", "Please enter your password")
        | _ ->
          email
          password
          h("p", "Please enter both email and password")
      ]
    )

  let loginForm
    (
      tokenSet: AntiforgeryTokenSet,
      missingFields: string list,
      message: string option,
      email: string option
    ) =
    let msg =
      match message with
      | Some msg -> h("p", msg)
      | None -> empty

    h(
      "form[action=/login][method=post]",
      h(
        $"input[type=hidden][name={tokenSet.FormFieldName}][value={tokenSet.RequestToken}]"
      ),
      inputsWithMissingFields(missingFields, email),
      msg,
      h("button[type=submit]", "Login")
    )

let login: HttpHandler =
  fun ctx -> taskUnit {
    let af = ctx.Plug<IAntiforgery>()
    let tokenSet = af.GetAndStoreTokens ctx

    let reqData = Request.getQuery ctx
    let missingFields = reqData.GetStringNonEmptyList "missingField"
    let message = reqData.TryGetString "message"
    let email = reqData.TryGetString "email"

    let loginForm = Fragments.loginForm(tokenSet, missingFields, message, email)

    let content = Layout.Page(h("article", h("h1", "Login"), loginForm))

    return! Response.ofHox content ctx
  }

let loginPost: HttpHandler =
  fun ctx -> taskUnit {
    let! postedData = Request.getFormSecure ctx

    match postedData with
    | Some formData ->
      let email = formData.GetStringNonEmpty("email", "")
      let password = formData.GetStringNonEmpty("password", "")

      if email = "admin@admin" && password = "password" then
        let claims =
          ClaimsPrincipal(
            ClaimsIdentity(
              [
                Claim(ClaimTypes.Email, email)
                Claim(ClaimTypes.Role, "admin")
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
        let missing =
          match email, password with
          | "", "" -> $"missingField=email&missingField=password&email={email}"
          | "", _ -> $"missingField=email"
          | _, "" -> "missingField=password&email={email}"
          | _ -> ""

        let query =
          if missing = "" then
            $"message=Wrong Credentials&email={email}"
          else
            $"message=Wrong Credentials&{missing}"

        return! Response.redirectTemporarily $"/login?{query}" ctx
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
