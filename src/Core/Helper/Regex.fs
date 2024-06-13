﻿/// <summary>
/// This module contains unified regex patterns and matching functions to parse isa tab column headers to BuildingBlock information.
/// </summary>
module ARCtrl.Helper.Regex

open System

module Pattern =

    let handleGroupPatterns (pattern : string) =
        let pyify (pattern : string) =
            pattern.Replace(@"(?<", @"(?P<")
        #if FABLE_COMPILER_PYTHON
        pyify pattern
        #else
        pattern
        #endif

    module MatchGroups =
        
        [<Literal>]
        let numberFormat = "numberFormat"

        [<Literal>]
        let localID = "localid"

        [<Literal>]
        let idspace = "idspace"

        [<Literal>]
        let iotype = "iotype"

        [<Literal>]
        let commentKey = "commentKey"

    /// This pattern is only used to remove any leftover #id attributes from previous Swate version. 
    /// `"Parameter [biological replicate#2]"` This #id is deprecated but the pattern can still be used to remove any from files. 
    /// Was deprecated before 2023.
    [<LiteralAttribute>]
    let IdPattern = "#\d+" //  @"(?<=#)\d+(?=[\)\]])" <- Cannot be used in IE11

    /// <summary> This pattern captures characters between squared brackets, without id: Parameter [biological replicate#2] -> [biological replicate]
    /// 
    /// TODO: Could be redesigned to use capture groups, then it could return inner without brackets
    /// </summary>
    [<LiteralAttribute>]
    let SquaredBracketsTermNamePattern = "\[.*\]" //  @"(?<= \[)[^#\]]*(?=[\]#])" <- Cannot be used in IE11

    /// Used to get unit name from Excel numberFormat: 0.00 "degree Celsius" --> degree Celsius
    let ExcelNumberFormat = $"\"(?<{MatchGroups.numberFormat}>(.*?))\""

    /// Hits Unit column header
    [<LiteralAttribute>]
    let UnitPattern = @"Unit"

    /// Hits Term Source REF and Term Accession Number column headers
    ///
    /// Example 1: "Term Source REF (MS:1003022)"
    ///
    /// Example 2: "Term Accession Number (MS:1003022)"
    ///
    /// the id part "MS:1003022" is captured as `id` group.
    [<LiteralAttribute>]
    let ReferenceColumnPattern = @"(Term Source REF|Term Accession Number)\s\((?<id>.*)\)"   

    /// Hits Term Accession Number column header
    ///
    /// Example 1: "Term Source REF (MS:1003022)"
    ///
    /// the id part "MS:1003022" is captured as `id` group.
    [<LiteralAttribute>]
    let TermSourceREFColumnPattern = @"Term Source REF\s\((?<id>.*)\)" 

    /// Hits Term Source REF column header
    ///
    /// Example 1: "Term Accession Number (MS:1003022)"
    ///
    /// the id part "MS:1003022" is captured as `id` group.
    [<LiteralAttribute>]
    let TermAccessionNumberColumnPattern = @"Term Accession Number\s\((?<id>.*)\)" 

    /// Hits term accession, without id: ENVO:01001831
    let TermAnnotationShortPattern = $@"(?<{MatchGroups.idspace}>\w+?):(?<{MatchGroups.localID}>\w+)" //prev: @"[\w]+?:[\d]+"

    // https://obofoundry.org/id-policy.html#mapping-of-owl-ids-to-obo-format-ids
    /// <summary>Regex pattern is designed to hit only Foundry-compliant URIs.</summary>
    let TermAnnotationURIPattern = $@"http://purl.obolibrary.org/obo/(?<{MatchGroups.idspace}>\w+?)_(?<{MatchGroups.localID}>\w+)"

    /// Watch this closely, this could hit some edge cases we do not want to cover.
    let TermAnnotationURIPattern_lessRestrictive = $@".*\/(?<{MatchGroups.idspace}>\w+?)[:_](?<{MatchGroups.localID}>\w+)"

    /// Watch this closely, this could hit some edge cases we do not want to cover.
    let TermAnnotationURIPattern_MS_RO_PO = $@".*252F(?<{MatchGroups.idspace}>\w+?)_(?<{MatchGroups.localID}>\w+)"



    /// This pattern is used to match both Input and Output columns and capture the IOType as `iotype` group.
    let IOTypePattern = $@"(Input|Output)\s\[(?<{MatchGroups.iotype}>.+)\]"

    /// This pattern is used to match Input column and capture the IOType as `iotype` group.
    let InputPattern = $@"Input\s\[(?<{MatchGroups.iotype}>.+)\]"

    /// This pattern is used to match Output column and capture the IOType as `iotype` group.
    let OutputPattern = $@"Output\s\[(?<{MatchGroups.iotype}>.+)\]"

    /// This pattern is used to match Comment column and capture the comment key as `commentKey` group.
    let CommentPattern = $@"Comment\s\[(?<{MatchGroups.commentKey}>.+)\]"

    /// This pattern matches any column header starting with some text, followed by one whitespace and a term name inside squared brackets.
    ///
    /// Captures column type as named group: "termcolumntype" (e.g. Component, Characteristic .. ).
    ///
    /// Captures term name as named group: "termname" (e.g. instrument model).
    ///
    /// Exmp. 1: Parameter [instrument model] --> termcolumntype: Parameter; termname: instrument model
    ///
    /// Exmp. 2: Characteristic [species] --> termcolumntype: Characteristic; termname: species
    [<LiteralAttribute>]
    let TermColumnPattern = @"(?<termcolumntype>.+?)\s\[(?<termname>.+)\]"

    /// This pattern is used to match auto-generated table names. Mainly used in ArcAssay.addTable(). Default tables will get such a name.
    ///
    /// Will match "New Table 10" and will capture the number as `number` group.
    [<LiteralAttribute>]
    let AutoGeneratedTableName = @"^New\sTable\s(?<number>\d+)$"

module ActivePatterns =
    
    open System.Text.RegularExpressions
    
    /// Matches, if the input string matches the given regex pattern.
    let (|Regex|_|) pattern (input : string) =
        let pattern = Pattern.handleGroupPatterns pattern
        let m = Regex.Match(input.Trim(), pattern)
        if m.Success then Some(m)
        else None

    /// Matches any column header starting with some text, followed by one whitespace and a term name inside squared brackets.
    let (|ReferenceColumnHeader|_|) input = 
        match input with
        | Regex Pattern.ReferenceColumnPattern r ->
            {|Annotation = r.Groups.["id"].Value|}
            |> Some
        | _ -> None

    /// Matches any column header starting with some text, followed by one whitespace and a term name inside squared brackets.
    let (|TermColumn|_|) input = 
        match input with
        | Regex Pattern.TermColumnPattern r ->
            {|TermColumnType = r.Groups.["termcolumntype"].Value; TermName = r.Groups.["termname"].Value|}
            |> Some
        | _ -> None

    /// Matches a "Unit" column header.
    let (|UnitColumnHeader|_|) input = 
        match input with
        | Regex Pattern.UnitPattern o -> Some (o.Value)
        | _ -> None

    /// Matches a "Parameter [Term]" or "Parameter Value [Term]" column header and returns the Term string.
    let (|ParameterColumnHeader|_|) input = 
        match input with
        | TermColumn r ->
            match r.TermColumnType with
            | "Parameter" 
            | "Parameter Value"             -> Some r.TermName
            | _ -> None
        | _ -> None

    /// Matches a "Factor [Term]" or "Factor Value [Term]" column header and returns the Term string.
    let (|FactorColumnHeader|_|) input = 
        match input with
        | TermColumn r ->
            match r.TermColumnType with
            | "Factor" 
            | "Factor Value"             -> Some r.TermName
            | _ -> None
        | _ -> None

    /// Matches a "Characteristic [Term]" or "Characteristics [Term]" or "Characteristics Value [Term]" column header and returns the Term string.
    let (|CharacteristicColumnHeader|_|) input = 
        match input with
        | TermColumn r ->
            match r.TermColumnType with
            | "Characteristic" 
            | "Characteristics"
            | "Characteristics Value" -> Some r.TermName
            | _ -> None
        | _ -> None

    /// Matches a "Component [Term]" or "Component Value [Term]" column header and returns the Term string.
    let (|ComponentColumnHeader|_|) input = 
        match input with
        | TermColumn r ->
            match r.TermColumnType with
            | "Component" 
            | "Component Value" -> Some r.TermName
            | _ -> None
        | _ -> None

    /// Matches a short term string and returns the term source ref and the annotation number strings.
    /// 
    /// Example: "MS:1003022" --> term source ref: "MS"; annotation number: "1003022"
    let (|TermAnnotationShort|_|) input =
        match input with
        | Regex Pattern.TermAnnotationShortPattern value ->
            let idspace = value.Groups.[Pattern.MatchGroups.idspace].Value
            let localID = value.Groups.[Pattern.MatchGroups.localID].Value
            {|IDSpace = idspace; LocalID = localID|}
            |> Some
        | _ ->
            None

    /// Matches a term string (either short or URI) and returns the term source ref and the annotation number strings.
    /// 
    /// Example 1: "MS:1003022" --> term source ref: "MS"; annotation number: "1003022"
    ///
    /// Example 2: "http://purl.obolibrary.org/obo/MS_1003022" --> term source ref: "MS"; annotation number: "1003022"
    let (|TermAnnotation|_|) input =
        match input with
        | Regex Pattern.TermAnnotationShortPattern value 
        | Regex Pattern.TermAnnotationURIPattern value 
        | Regex Pattern.TermAnnotationURIPattern_lessRestrictive value 
        | Regex Pattern.TermAnnotationURIPattern_MS_RO_PO value ->
            let idspace = value.Groups.[Pattern.MatchGroups.idspace].Value
            let localID = value.Groups.[Pattern.MatchGroups.localID].Value
            {|IDSpace = idspace; LocalID = localID|}
            |> Some
        | _ ->
            None

    /// Matches a "Term Source REF (ShortTerm)" column header and returns the ShortTerm as Term Source Ref and Annotation Number.
    ///
    /// Example: "Term Source REF (MS:1003022)" --> term source ref: "MS"; annotation number: "1003022"
    let (|TSRColumnHeader|_|) input = 
        match input with
        | Regex Pattern.TermSourceREFColumnPattern r ->
            match r.Groups.["id"].Value with
            | TermAnnotation r -> Some r 
            | _ -> Some {|IDSpace = ""; LocalID = ""|}
         | _ -> None

    /// Matches a "Term Accession Number (ShortTerm)" column header and returns the ShortTerm as Term Source Ref and Annotation Number.
    ///
    /// Example: "Term Accession Number (MS:1003022)" --> term source ref: "MS"; annotation number: "1003022"
    let (|TANColumnHeader|_|) input = 
        match input with
        | Regex Pattern.TermAccessionNumberColumnPattern r ->
            match r.Groups.["id"].Value with
            | TermAnnotation r -> Some r 
            | _ -> Some {|IDSpace = ""; LocalID = ""|}
         | _ -> None

    /// Matches a "Input [InputType]" column header and returns the InputType as string.
    let (|InputColumnHeader|_|) input = 
        match input with
        | Regex Pattern.InputPattern r ->
            Some r.Groups.[Pattern.MatchGroups.iotype].Value          
         | _ -> None

    /// Matches a "Output [OutputType]" column header and returns the OutputType as string.
    let (|OutputColumnHeader|_|) input =
        match input with
        | Regex Pattern.OutputPattern r ->
            Some r.Groups.[Pattern.MatchGroups.iotype].Value
        | _ -> None


    /// Matches auto-generated readable table names. Mainly used in ArcAssay.addTable(). Default tables will get such a name.
    ///
    /// Will match "New Table 10" and return the number `10`.
    let (|AutoGeneratedTableName|_|) input =
        match input with
        | Regex Pattern.AutoGeneratedTableName r ->
            r.Groups.["number"].Value |> int |> Some
        | _ -> None

    let (|Comment|_|) input =
        match input with
        | Regex Pattern.CommentPattern r ->
            Some r.Groups.[Pattern.MatchGroups.commentKey].Value
        | _ -> None

open Pattern
open ActivePatterns
open System
open System.Text.RegularExpressions
    

let tryParseReferenceColumnHeader (str : string) =
    match str.Trim() with
    | ReferenceColumnHeader v -> 
        Some v
    | _ -> None

let tryParseTermAnnotationShort (str:string) =
    match str.Trim() with
    | Regex TermAnnotationShortPattern value ->
        let idspace = value.Groups.[Pattern.MatchGroups.idspace].Value
        let localid = value.Groups.[Pattern.MatchGroups.localID].Value
        {|IDSpace = idspace; LocalID = localid|} 
        |> Some
    | _ -> None

/// <summary>
/// This function can be used to extract `IDSPACE:LOCALID` (or: `Term Accession`) from Swate header strings or obofoundry conform URI strings.
/// 
/// **Example 1:** "http://purl.obolibrary.org/obo/GO_000001" --> "GO:000001"
/// 
/// **Example 2:** "Term Source REF (NFDI4PSO:0000064)" --> "NFDI4PSO:0000064"
/// </summary>
let tryParseTermAnnotation (str:string) =
    match str.Trim() with
    | Regex TermAnnotationShortPattern value 
    | Regex TermAnnotationURIPattern value 
    | Regex TermAnnotationURIPattern_lessRestrictive value 
    | Regex TermAnnotationURIPattern_MS_RO_PO value ->
        let idspace = value.Groups.[Pattern.MatchGroups.idspace].Value
        let localid = value.Groups.[Pattern.MatchGroups.localID].Value
        {|IDSpace = idspace; LocalID = localid|}
        |> Some
    | _ ->
        None

/// Tries to parse 'str' to term accession and returns it in the format `Some "termsourceref:localtan"`. Exmp.: `Some "MS:000001"`
let tryGetTermAnnotationShortString (str:string) = 
    tryParseTermAnnotation str
    |> Option.map (fun r -> r.IDSpace + ":" + r.LocalID)

/// Parses 'str' to term accession and returns it in the format "termsourceref:localtan". Exmp.: "MS:000001"
let getTermAnnotationShortString (str:string) =
    match tryGetTermAnnotationShortString str with
    | Some s -> s
    | None -> failwith $"Unable to parse '{str}' to term accession."

/// <summary>
/// This function is used to parse Excel numberFormat string to term name.
/// 
/// **Example 1:** "0.00 "degree Celsius"" --> "degree Celsius"
/// </summary>
let tryParseExcelNumberFormat (headerStr:string) =
    match headerStr.Trim() with
    | Regex ExcelNumberFormat value ->
        // remove quotes at beginning and end of matched string
        let numberFormat = value.Groups.["numberFormat"].Value
        Some numberFormat
    | _ ->
        None

/// <summary>
/// This function is used to match both Input and Output columns and capture the IOType as `iotype` group.
/// 
/// **Example 1:** "Input [Sample]" --> "Sample"
/// </summary>
let tryParseIOTypeHeader (headerStr: string) =
    match headerStr.Trim() with
    | Regex IOTypePattern value ->
        // remove quotes at beginning and end of matched string
        let numberFormat = value.Groups.[Pattern.MatchGroups.iotype].Value
        Some numberFormat
    | _ ->
        None
        
/// Matches any column header starting with some text, followed by one whitespace and a term name inside squared brackets.
let tryParseTermColumn input = 
    match input with
    | TermColumn r -> Some r
    | _ -> None


/// Matches a "Unit" column header.
let tryParseUnitColumnHeader input = 
    match input with
    | UnitColumnHeader r -> Some r
    | _ -> None

/// Matches a "Parameter [Term]" or "Parameter Value [Term]" column header and returns the Term string.
let tryParseParameterColumnHeader input = 
    match input with
    | ParameterColumnHeader r -> Some r
    | _ -> None

/// Matches a "Factor [Term]" or "Factor Value [Term]" column header and returns the Term string.
let tryParseFactorColumnHeader input = 
    match input with
    | FactorColumnHeader r -> Some r
    | _ -> None

/// Matches a "Characteristic [Term]" or "Characteristics [Term]" or "Characteristics Value [Term]" column header and returns the Term string.
let tryParseCharacteristicColumnHeader input = 
    match input with
    | CharacteristicColumnHeader r -> Some r
    | _ -> None

/// Matches a "Component [Term]" or "Characteristics [Term]" or "Component Value [Term]" column header and returns the Term string.
let tryParseComponentColumnHeader input = 
    match input with
    | ComponentColumnHeader r -> Some r
    | _ -> None

/// Matches a "Term Source REF (ShortTerm)" column header and returns the ShortTerm as Term Source Ref and Annotation Number.
///
/// Example: "Term Source REF (MS:1003022)" --> term source ref: "MS"; annotation number: "1003022"
let tryParseTSRColumnHeader input = 
    match input with
    | TSRColumnHeader r -> Some r
    | _ -> None

/// Matches a "Term Accession Number (ShortTerm)" column header and returns the ShortTerm as Term Source Ref and Annotation Number.
///
/// Example: "Term Accession Number (MS:1003022)" --> term source ref: "MS"; annotation number: "1003022"
let tryParseTANColumnHeader input = 
    match input with
    | TANColumnHeader r -> Some r
    | _ -> None

/// Matches a "Input [InputType]" column header and returns the InputType as string.
let tryParseInputColumnHeader input =
    match input with
    | InputColumnHeader r -> Some r
    | _ -> None


/// Matches a "Output [OutputType]" column header and returns the OutputType as string.
let tryParseOutputColumnHeader input =
    match input with
    | OutputColumnHeader r -> Some r
    | _ -> None

