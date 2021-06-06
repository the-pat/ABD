namespace ABD.Core

module Log =
    open System
    open System.Threading

    let report =
        let lockObj = obj ()

        fun (color: ConsoleColor) (message: string) ->
            lock
                lockObj
                (fun _ ->
                    Console.ForegroundColor <- color
                    printfn "%s (thread ID: %i)" message Thread.CurrentThread.ManagedThreadId
                    Console.ResetColor ())

    let error = report ConsoleColor.Red
    let warn = report ConsoleColor.Yellow
    let debug = report ConsoleColor.Green
    let info = report ConsoleColor.Cyan