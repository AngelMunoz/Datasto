[<AutoOpen>]
module Extensions

open System.Runtime.CompilerServices

open System.Threading.Tasks
open IcedTasks
open Hox.Core
open Hox
open Hox.Rendering

open Microsoft.AspNetCore.Http
open StarFederation.Datastar.DependencyInjection
open Falco
open Microsoft.AspNetCore.Antiforgery
open System.Text.Json


module Response =

  let ofHox(view: Node) : HttpHandler =
    fun (ctx: HttpContext) -> taskUnit {
      ctx.Response.ContentType <- "text/html; charset=utf-8"

      do! ctx.Response.StartAsync(ctx.RequestAborted)

      do! ctx.Response.WriteAsync("<!DOCTYPE html>")

      do!
        Render.toStream(
          view,
          ctx.Response.Body,
          cancellationToken = ctx.RequestAborted
        )

      do! ctx.Response.CompleteAsync()
    }


[<Extension>]
type DataStarHoxExtensions =

  [<Extension>]
  static member inline MergeHoxFragments
    (sse: IServerSentEventService, node: Node)
    =
    taskUnit {
      let! content = Render.asString node
      return! sse.MergeFragments content
    }

  [<Extension>]
  static member inline MergeHoxFragments
    (
      sse: IServerSentEventService,
      node: Node,
      options: ServerSentEventMergeFragmentsOptions
    ) =
    taskUnit {
      let! content = Render.asString node
      return! sse.MergeFragments(content, options)
    }


  [<Extension>]
  static member inline dataOn(node: Node, event: string, value: string) =
    node.attr($"data-on-{event}", value)

  [<Extension>]
  static member inline requestCsrf
    (
      node: Node,
      verb: string,
      event: string,
      url: string,
      csrf: AntiforgeryTokenSet,
      ?options: Map<string, obj>
    ) =

    let options = defaultArg options Map.empty

    let options =
      options
      |> Map.add "headers" (Map.ofList [ csrf.HeaderName, csrf.RequestToken ])

    let options = JsonSerializer.Serialize(options)

    node.dataOn(event, $"@{verb}('{url}', {options})")

  [<Extension>]
  static member inline postCsrf
    (
      node: Node,
      event: string,
      url: string,
      csrf: AntiforgeryTokenSet,
      ?options: Map<string, obj>
    ) =
    node.requestCsrf("post", event, url, csrf, ?options = options)

  [<Extension>]
  static member inline putCsrf
    (
      node: Node,
      event: string,
      url: string,
      csrf: AntiforgeryTokenSet,
      ?options: Map<string, obj>
    ) =
    node.requestCsrf("get", event, url, csrf, ?options = options)

  [<Extension>]
  static member inline patchCsrf
    (
      node: Node,
      event: string,
      url: string,
      csrf: AntiforgeryTokenSet,
      ?options: Map<string, obj>
    ) =
    node.requestCsrf("patch", event, url, csrf, ?options = options)

  [<Extension>]
  static member inline deleteCsrf
    (
      node: Node,
      event: string,
      url: string,
      csrf: AntiforgeryTokenSet,
      ?options: Map<string, obj>
    ) =
    node.requestCsrf("delete", event, url, csrf, ?options = options)
