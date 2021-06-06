namespace ABD

module DownloadCommand =
    open System
    open Command
    open ABD.Core

    let handle { url = url; concurrency = concurrency } (path: string) =
        async {
            let uri = Uri(url)
            let! downloaded, failed = Download.AsyncGetTracks uri path concurrency

            Log.info (sprintf "%i files downloaded, %i failed. Press a key" downloaded.Length failed.Length)
        }