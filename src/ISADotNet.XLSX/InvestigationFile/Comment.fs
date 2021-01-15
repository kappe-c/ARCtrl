namespace ISADotNet.XSLX

open ISADotNet
open System.Text.RegularExpressions

module Comment = 
    let commentRegex = Regex(@"(?<=Comment\[<).*(?=>\])")

    let commentRegexNoAngleBrackets = Regex(@"(?<=Comment\[).*(?=\])")

    let (|Comment|_|) (key : Option<string>) =
        key
        |> Option.bind (fun k ->
            let r = commentRegex.Match(k)
            if r.Success then Some r.Value
            else 
                let r = commentRegexNoAngleBrackets.Match(k)
                if r.Success then Some r.Value
                else None
        )

module Remark = 

    let remarkRegex = Regex(@"(?<=#).*")


    let (|Remark|_|) (key : Option<string>) =
        key
        |> Option.bind (fun k ->
            let r = remarkRegex.Match(k)
            if r.Success then Some r.Value
            else None
        )

    let wrapCommentKey k = 
        sprintf "Comment[<%s>]" k

    let wrapRemark r = 
        sprintf "#%s" r