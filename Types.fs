namespace Datasto

open System

type User = {
  id: int
  name: string
  email: string
  createdAt: DateTime
  roles: string list
}

type Product = {
  id: int
  name: string
  price: decimal option
  description: string option
  createdAt: DateTime
}

type Stock = {
  id: int
  name: string
  products: (Product * int) list
  createdAt: DateTime
  updatedAt: DateTime
}
