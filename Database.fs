module Datasto.Database

open IcedTasks
open Donald
open System.Data

module private Queries =

  module Roles =

    [<Literal>]
    let findRoleByName = "SELECT id, name FROM roles WHERE name = @name;"

    [<Literal>]
    let createRole = "INSERT INTO roles (name) VALUES (@name);"

    [<Literal>]
    let findAllRoles = "SELECT * FROM roles;"

    [<Literal>]
    let deleteRole = "DELETE FROM roles WHERE id = @id;"

  module Users =
    // The following queries are sqlite queries and should be syntax compatible
    [<Literal>]
    let createUser =
      "INSERT INTO users (name, email, password) VALUES (@name, @email, @password);"

    [<Literal>]
    let findUserByEmail =
      // finds a user with the given email and brings also the roles of the user
      "SELECT id, name, email, created_at FROM users WHERE email = @email;"

    [<Literal>]
    let findRolesForUser =
      "SELECT r.name FROM roles r JOIN user_roles ur ON r.id = ur.role_id WHERE ur.user_id = @user_id;"

    [<Literal>]
    let setNameAndEmail =
      "UPDATE users SET name = @name, email = @email WHERE id = @id;"

module private Readers =

  module User =

    let ofSelectMostFields(rd: IDataReader) =
      let id = rd.ReadInt32 "id"
      let name = rd.ReadString "name"
      let email = rd.ReadString "email"
      let createdAt = rd.ReadDateTime "created_at"
      (id, name, email, createdAt)

  module Roles =

    let ofSelectName(rd: IDataReader) = rd.ReadString "name"

module Users =

  let create(con: IDbConnection) =
    fun (name: string, email: string, password: string) -> cancellableTask {
      let! cancellationToken = CancellableTask.getCancellationToken()

      let op =
        con
        |> Db.newCommand Queries.Users.createUser
        |> Db.setParams [
          "name", SqlType.sqlString name
          "email", SqlType.sqlString email
          "password", SqlType.sqlString password
        ]
        |> Db.setCancellationToken cancellationToken

      return! Db.Async.exec op
    }

  let findUserByEmail(con: IDbConnection) =
    fun (email: string) -> cancellableTask {
      let! cancellationToken = CancellableTask.getCancellationToken()

      let! user =
        con
        |> Db.newCommand Queries.Users.findUserByEmail
        |> Db.setParams [ "email", SqlType.sqlString email ]
        |> Db.setCancellationToken cancellationToken
        |> Db.Async.querySingle Readers.User.ofSelectMostFields

      match user with
      | Some(id, name, email, createdAt) ->

        let! roles =
          con
          |> Db.newCommand Queries.Users.findRolesForUser
          |> Db.setParams [ "user_id", SqlType.sqlInt32 id ]
          |> Db.setCancellationToken cancellationToken
          |> Db.Async.query Readers.Roles.ofSelectName

        return
          Some {
            id = id
            name = name
            email = email
            createdAt = createdAt
            roles = roles
          }

      | None -> return None
    }

  let setNameAndEmail(con: IDbConnection) =
    fun (id: int, name: string, email: string) -> cancellableTask {
      let! cancellationToken = CancellableTask.getCancellationToken()

      let op =
        con
        |> Db.newCommand Queries.Users.setNameAndEmail
        |> Db.setParams [
          "id", SqlType.sqlInt32 id
          "name", SqlType.sqlString name
          "email", SqlType.sqlString email
        ]
        |> Db.setCancellationToken cancellationToken

      return! Db.Async.exec op
    }

module Roles =

  let create(con: IDbConnection) =
    fun (name: string) -> cancellableTask {
      let! cancellationToken = CancellableTask.getCancellationToken()

      let op =
        con
        |> Db.newCommand Queries.Roles.createRole
        |> Db.setParams [ "name", SqlType.sqlString name ]
        |> Db.setCancellationToken cancellationToken

      return! Db.Async.exec op
    }

  let findRoleByName(con: IDbConnection) =
    fun (name: string) -> cancellableTask {
      let! cancellationToken = CancellableTask.getCancellationToken()

      let! role =
        con
        |> Db.newCommand Queries.Roles.findRoleByName
        |> Db.setParams [ "name", SqlType.sqlString name ]
        |> Db.setCancellationToken cancellationToken
        |> Db.Async.querySingle Readers.Roles.ofSelectName

      return role
    }

  let findAllRoles(con: IDbConnection) = cancellableTask {
    let! cancellationToken = CancellableTask.getCancellationToken()

    let! roles =
      con
      |> Db.newCommand Queries.Roles.findAllRoles
      |> Db.setCancellationToken cancellationToken
      |> Db.Async.query Readers.Roles.ofSelectName

    return roles
  }
