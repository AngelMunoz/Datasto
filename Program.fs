open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open StarFederation.Datastar.DependencyInjection
open Falco
open Falco.Routing
open Datasto
open System.Data
open Microsoft.Data.Sqlite

let builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs())

builder.Services.AddAuthentication().AddCookie()
|> ignore<Microsoft.AspNetCore.Authentication.AuthenticationBuilder>

builder.Services
  .AddAntiforgery()
  .AddDatastar()
  .AddScoped<IDbConnection>(fun _ ->
    let connection =
      builder.Configuration.GetConnectionString("DefaultConnection")

    match connection with
    | null -> failwith "Connection string not found"
    | connection ->
      let connection = new SqliteConnection(connection)
      connection.Open()
      connection)
|> ignore<IServiceCollection>

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
      get "/" Pages.index
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
