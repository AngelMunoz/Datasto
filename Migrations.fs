module Datasto.Migrations

open Migrondi.Core
open IcedTasks
open Microsoft.Extensions.Logging

let migrate(mi: IMigrondi, logger: ILogger) =
  mi.Initialize()

  for result in mi.RunUp() do
    logger.LogInformation("Applied migration {MigrationName}", result.name)
