namespace ABD

module HelpCommand =
    let handle =
        async {
            printfn """
                    Usage
                        $ abd <url> [...OPTIONS]

                    Options
	                    -h, --help	This text
	                    -v, --verbose	Will print more logging messages
	                    -o, --output	The output directory. Defaults to './'
	                    -u, --url	The download url. Can use in-place of the first input
                    """
        }