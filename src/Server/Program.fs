open System.IO

open System.Net

open Suave
open Suave.Operators

open Shared

let path = Path.Combine("..","Client") |> Path.GetFullPath 
let port = 8085us

let config =
  { defaultConfig with 
      homeFolder = Some path
      bindings = [ HttpBinding.create HTTP (IPAddress.Parse "0.0.0.0") port ] }

let getCounter () : Async<Counter> = async { return 42 }

let init : WebPart = 
  Filters.path "/api/init" >=>
  fun ctx ->
    async {
      let! counter = getCounter()
      return! Successful.OK (string counter) ctx
    }

let webPart =
  choose [
    init
    Files.browseHome
  ]

startWebServer config webPart