namespace ABD.Core

module Download =
    open System
    open System.IO
    open System.Net
    open System.Text.Json
    open System.Text.RegularExpressions
    open FSharp.Data
    open FSharpx.Control
    open Track

    let private asyncGetTracks (pageUri: Uri) =
        async {
            Log.info "Getting tracks..."

            let headers = ["User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Brave Chrome/83.0.4103.116 Safari/537.36"]
            let! html = Http.AsyncRequestString(pageUri.ToString(), headers = headers)
            let unparsedWithTrailingComma =
                Regex.Match(html, @"tracks = \[[.\s\S]*?\],").Value.Replace("tracks = ", "")
            let unserializedTracks = Regex.Replace(unparsedWithTrailingComma, @",(?!\s*?[{[""'\w])", "")

            return JsonSerializer.Deserialize<RawTrack array> unserializedTracks
        }

    let private asyncTryDownload (downloadDir: string) (rawTrack: RawTrack) =
        async {
            let! track = fromRawTrack rawTrack

            Log.info (sprintf "%s - starting download" track.filename)
            let filePath = Path.Combine(downloadDir, track.filename)

            use client = new WebClient()
            try
                do!
                    client.DownloadFileTaskAsync(track.url, filePath)
                    |> Async.AwaitTask

                Log.info (sprintf "%s - download complete" track.filename)
                return (Outcome.OK track.filename)
            with e ->
                Log.error (sprintf "%s - error: %s" track.filename e.Message)
                return (Outcome.Failed track.filename)
        }

    let private createDir path =
        if Directory.Exists path |> not then
            Log.debug (sprintf "Creating directory '%s'" path)
            Directory.CreateDirectory(path) |> ignore

        ()

    let AsyncGetTracks (pageUri: Uri) (downloadDir: string) (maxConcurrency: int option) =
        async {
            let! tracks = asyncGetTracks pageUri
            let concurrency =
                match maxConcurrency with
                | Some concurrency -> concurrency
                | None -> tracks.Length
            let downloadPath = Path.GetFullPath(downloadDir)

            Log.debug (sprintf "Downloading to... %s" downloadPath)

            do createDir downloadPath

            Log.info "Starting download..."

            let! downloadResults =
                tracks
                |> Seq.filter (fun track ->
                    String.IsNullOrWhiteSpace track.url |> not
                    && String.IsNullOrWhiteSpace track.chapter_link_dropbox |> not)
                |> Seq.map (asyncTryDownload downloadPath)
                |> Async.ParallelWithThrottle concurrency

            Log.info "Completed download..."

            let downloaded, failed =
                downloadResults |> Array.partition Outcome.isOK

            return downloaded |> Array.map Outcome.filename, failed |> Array.map Outcome.filename
        }