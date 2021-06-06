namespace ABD

module Command =
    type Command =
    | DownloadCommand of DownloadArgs
    | HelpCommand

    and DownloadArgs = { url: string; concurrency: int option }