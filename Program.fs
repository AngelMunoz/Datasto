open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open StarFederation.Datastar.DependencyInjection
open Falco
open Falco.Routing
open Datasto

let builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs())

builder.Services.AddAuthentication().AddCookie() |> ignore
builder.Services.AddAntiforgery().AddDatastar() |> ignore<IServiceCollection>

let app = builder.Build()

let migrondi =
  Migrondi.Core.Migrondi.MigrondiFactory(
    Migrondi.Core.MigrondiConfig.Default,
    "."
  )

app
  .UseStaticFiles()
  .UseIf(
    app.Environment.IsDevelopment(),
    fun app -> app.UseDeveloperExceptionPage()
  )
  .UseAuthentication()
  .UseAntiforgery()
  .UseRouting()
  .UseFalco(
    [
      get "/login" Pages.login
      post "/login" Pages.loginPost
      post "/logout" Pages.logoutPost
    ]
  )
|> ignore

try
  Migrations.migrate(migrondi, app.Logger)
  app.Run()
with ex ->
  eprintfn $"%O{ex}"
