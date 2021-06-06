open ABD.Core
open System

open System.Text.Json
open FSharp.Data
open FSharp.Data.HttpRequestHeaders

[<EntryPoint>]
let main argv =
    try
        let uri = Uri("https://tokybook.com/empire-of-the-summer-moon/")
        let concurrency = Some 5
        let downloaded, failed =
            Download.AsyncGetTracks uri "/Users/pat/Downloads/Moon" concurrency
            |> Async.RunSynchronously

        failed
        |> Array.iter ((sprintf "Failed: %s") >> Log.error)

        Log.info (sprintf
            "%i files downloaded, %i failed. Press a key"
            downloaded.Length
            failed.Length
        )

        Console.ReadKey() |> ignore
        0
    with e ->
        Log.error (sprintf "%A" e)
        1