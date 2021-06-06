namespace ABD

module CommandLineOptions =
    open Command
    open ABD.Core

    type OutputOption = OutputPath of string

    type CommandLineOptions =
        { command: Command option
          output: OutputOption }

    type ParseMode =
        | InputLevel
        | FlagLevel
        | OutputFlagLevel
        | UrlFlagLevel
        | ConcurrencyFlagLevel

    type FoldState =
        { options: CommandLineOptions
          parseMode: ParseMode }

    let private parseFlagLevel (arg: string) optionsSoFar =
        match arg with
        | "-h"
        | "--help" ->
            let newOptionsSoFar =
                { optionsSoFar with
                      command = Some HelpCommand }

            { options = newOptionsSoFar
              parseMode = FlagLevel }
        | "-o"
        | "--output" ->
            { options = optionsSoFar
              parseMode = OutputFlagLevel }
        | "-u"
        | "--url" ->
            { options = optionsSoFar
              parseMode = UrlFlagLevel }
        | "-c"
        | "--concurrency" ->
            { options = optionsSoFar
              parseMode = ConcurrencyFlagLevel }
        | x ->
            printfn "Option '%s' is unrecognized" x

            { options = optionsSoFar
              parseMode = FlagLevel }

    let private parseInputLevel (arg: string) optionsSoFar =
        match arg with
        | url when url.StartsWith("-") -> parseFlagLevel arg optionsSoFar
        | url ->
            let downloadCommand = DownloadCommand { url = url; concurrency = None }

            let newOptionsSoFar =
                { optionsSoFar with
                      command = Some downloadCommand }

            { options = newOptionsSoFar
              parseMode = FlagLevel }

    let private parseOutputFlagLevel (arg: string) optionsSoFar =
        match arg with
        | path ->
            let newOptionsSoFar =
                { optionsSoFar with
                      output = OutputPath path }

            { options = newOptionsSoFar
              parseMode = FlagLevel }

    let private parseUrlFlagLevel (arg: string) optionsSoFar =
        let command =
            match optionsSoFar.command with
            | Some (DownloadCommand dargs) ->
                DownloadCommand { dargs with url = arg }
            | Some (HelpCommand) ->
                HelpCommand
            | None ->
                DownloadCommand { url = arg; concurrency = None }

        let newOptionsSoFar =
            { optionsSoFar with
                  command = Some command }

        { options = newOptionsSoFar
          parseMode = FlagLevel }

    module Int =
        let tryParse s =
            try
                s
                |> int
                |> Some
            with _ ->
                None

    let private parseConcurrencyFlagLevel (arg: string) optionsSoFar =
        match Int.tryParse arg with
        | Some concurrency ->
            let command =
                match optionsSoFar.command with
                | Some (DownloadCommand dargs) ->
                    DownloadCommand { dargs with concurrency = Some concurrency }
                | Some (HelpCommand) ->
                    HelpCommand
                | None ->
                    DownloadCommand { concurrency = Some concurrency; url = "" }

            let newOptionsSoFar =
                { optionsSoFar with
                      command = Some command }

            { options = newOptionsSoFar
              parseMode = FlagLevel }
        | None ->
            failwithf "Concurrency requires an integer, but `%s` was provided" arg

    let private foldFunction state element =
        match state with
        | { options = optionsSoFar
            parseMode = InputLevel } -> parseInputLevel element optionsSoFar
        | { options = optionsSoFar
            parseMode = FlagLevel } -> parseFlagLevel element optionsSoFar
        | { options = optionsSoFar
            parseMode = OutputFlagLevel } -> parseOutputFlagLevel element optionsSoFar
        | { options = optionsSoFar
            parseMode = UrlFlagLevel } -> parseUrlFlagLevel element optionsSoFar
        | { options = optionsSoFar
            parseMode = ConcurrencyFlagLevel } -> parseConcurrencyFlagLevel element optionsSoFar

    let private defaultOptions =
        { command = None
          output = OutputPath "./" }

    let parse args =
        let initialState =
            { options = defaultOptions
              parseMode = InputLevel }

        let { options = options } =
            args |> List.fold foldFunction initialState

        options

    let handle (options: CommandLineOptions) =
        match options with
        | { command = Some HelpCommand } -> HelpCommand.handle
        | { command = Some (DownloadCommand args)
            output = OutputPath path } -> DownloadCommand.handle args path
        | { command = None } ->
            Log.error "No command was given"
            HelpCommand.handle