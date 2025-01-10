namespace Datasto

open Hox
open Hox.Core

type Layout =

  static member Page(content: Node) =

    h(
      "html",
      h("head", h("link[rel=stylesheet][href=/styles/app.css]")),
      h(
        "body",
        content,
        h("script[type=module]")
          .attr(
            "src",
            "https://cdn.jsdelivr.net/gh/starfederation/datastar@v1.0.0-beta.1/bundles/datastar.js"
          )
      )
    )
