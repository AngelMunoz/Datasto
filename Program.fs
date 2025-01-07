open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open StarFederation.Datastar.DependencyInjection
open Falco
open Falco.Routing
open Datasto

let builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs())

builder.Services.AddAntiforgery().AddDatastar() |> ignore<IServiceCollection>

let app = builder.Build()

app
  .UseAntiforgery()
  .UseRouting()
  .UseFalco([ get "/login" Pages.login; post "/logout" Pages.logoutPost ])
|> ignore

app.Run()
