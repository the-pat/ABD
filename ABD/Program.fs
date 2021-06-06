open ABD
open ABD.Core
open System

[<EntryPoint>]
let main argv =
    try
        argv
        |> List.ofArray
        |> CommandLineOptions.parse
        |> CommandLineOptions.handle
        |> Async.RunSynchronously

        Console.ReadKey() |> ignore
        0
    with e ->
        Log.error (sprintf "%A" e)
        1