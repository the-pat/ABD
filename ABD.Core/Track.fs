namespace ABD.Core

module Track =
    open System
    open System.IO
    open System.Text.Json
    open FSharp.Data
    open FSharp.Data.HttpRequestHeaders

    type RawTrack =
        { track: int
          name: string
          chapter_link_dropbox: string
          duration: string
          chapter_id: string
          post_id: string
          url: string }

    type Track =
        { filename: string
          url: Uri }

    type AutoplaylistMp3Link =
        { link_mp3: string
          duration: float }

    let private getTrackId {RawTrack.track = track} =
        sprintf "%02i" (track - 1)

    let private getFilename (rawTrack: RawTrack): string =
        sprintf "%s - %s%s" (getTrackId rawTrack) rawTrack.name (Path.GetExtension(rawTrack.chapter_link_dropbox))

    let private getUri = function
    | {RawTrack.url = "NA"; chapter_id = chapterId} ->
        async {
            let body = JsonSerializer.Serialize {| chapterId = chapterId; serverType = 1 |}
            let! resp = Http.AsyncRequestString
                            ("https://autoplaylist.top/api-us/getMp3Link",
                             body = TextRequest body,
                             headers = [ContentType HttpContentTypes.Json])

            let link = JsonSerializer.Deserialize<AutoplaylistMp3Link> resp
            return Uri(link.link_mp3)
        }
    | {RawTrack.url = url} ->
        async {
            return Uri(url)
        }

    let fromRawTrack (rawTrack: RawTrack) =
        async {
            let filename = getFilename rawTrack
            let! uri = getUri rawTrack

            return { filename = filename; url = uri }
        }