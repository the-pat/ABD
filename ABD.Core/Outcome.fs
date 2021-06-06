namespace ABD.Core

module Outcome =

    type Outcome =
        | OK of filename: string
        | Failed of filename: string

    let isOK =
        function
        | OK _ -> true
        | Failed _ -> false

    let filename =
        function
        | OK fn
        | Failed fn -> fn