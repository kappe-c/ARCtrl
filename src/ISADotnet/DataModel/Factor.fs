namespace ISADotNet

open System.Text.Json.Serialization

type Factor = 
    {
        [<JsonPropertyName(@"@id")>]
        ID : URI option
        [<JsonPropertyName(@"factorName")>]
        Name : string option
        [<JsonPropertyName(@"factorType")>]
        FactorType : OntologyAnnotation option
        [<JsonPropertyName(@"comments")>]
        Comments : Comment list option
    }

    static member create id name factorType comments =
        {
            ID      = id
            Name    = name
            FactorType = factorType
            Comments = comments         
        }

    static member empty =
        Factor.create None None None None

    static member Create(?Id,?Name,?FactorType,?Comments) =
        Factor.create Id Name FactorType Comments

    /// Create a ISAJson Factor from ISATab string entries
    static member fromString (name : string) (term:string) (accession:string) (source:string) =
        let oa =
            OntologyAnnotation.fromString term accession source
            |> Option.fromValueWithDefault OntologyAnnotation.empty
        Factor.create None (Option.fromValueWithDefault "" name) oa None

    /// Get ISATab string entries from an ISAJson Factor object
    static member toString (factor : Factor) =
        factor.FactorType |> Option.map OntologyAnnotation.toString |> Option.defaultValue ("","","")  

    /// Returns the name of the factor as string
    member this.NameAsString =
        this.Name
        |> Option.defaultValue ""

    /// Returns the name of the factor with the number as string (e.g. "temperature #2")
    member this.NameAsStringWithNumber =       
        this.FactorType
        |> Option.map (fun oa -> oa.NameAsStringWithNumber)
        |> Option.defaultValue ""

    interface IISAPrintable with
        member this.Print() =
            this.ToString()
        member this.PrintCompact() =
            "OA " + this.NameAsStringWithNumber

[<AnyOf>]
type Value =
    | [<SerializationOrder(0)>] Ontology of OntologyAnnotation
    | [<SerializationOrder(1)>] Int of int
    | [<SerializationOrder(2)>] Float of float
    | [<SerializationOrder(3)>] Name of string

    static member fromOptions (value : string Option) (termAccesssion: string Option) (termSource: string Option) =
        match value, termSource, termAccesssion with
        | Some value, None, None ->
            try Value.Int (int value)
            with
            | _ -> 
                try Value.Float (float value)
                with
                | _ -> Value.Name value
            |> Some
        | None, None, None -> 
            None
        | _ -> 
            OntologyAnnotation.fromString (Option.defaultValue "" value) (Option.defaultValue "" termAccesssion) (Option.defaultValue "" termSource)
            |> Value.Ontology
            |> Some

    static member toOptions (value : Value) =
        match value with
        | Ontology oa -> oa.Name |> Option.map AnnotationValue.toString,oa.TermAccessionNumber,oa.TermSourceREF
        | Int i -> string i |> Some, None, None
        | Float f -> string f |> Some, None, None
        | Name s -> s |> Some, None, None

    interface IISAPrintable with
        member this.Print() =
            this.ToString()
        member this.PrintCompact() =
            match this with
            | Ontology oa   -> oa.NameAsString
            | Int i         -> sprintf "%i" i
            | Float f       -> sprintf "%f" f        
            | Name n        -> n

type FactorValue =
    {
        [<JsonPropertyName(@"@id")>]
        ID : URI option
        [<JsonPropertyName(@"category")>]
        Category : Factor option
        [<JsonPropertyName(@"value")>]
        Value : Value option
        [<JsonPropertyName(@"unit")>]
        Unit : OntologyAnnotation option
    
    }

    static member create id category value unit =
        {
            ID      = id
            Category = category
            Value = value
            Unit = unit         
        }

    static member Create(?Id,?Category,?Value,?Unit) =
        FactorValue.create Id Category Value Unit

    static member empty =
        FactorValue.create None None None None

    interface IISAPrintable with
        member this.Print() =
            this.ToString()
        member this.PrintCompact() =
            let category = this.Category |> Option.map (fun f -> f.NameAsString)
            let unit = this.Unit |> Option.map (fun oa -> oa.NameAsString)
            let value = 
                this.Value
                |> Option.map (fun v ->
                    let s = (v :> IISAPrintable).PrintCompact()
                    match unit with
                    | Some u -> s + " " + u
                    | None -> s
                )
            match category,value with
            | Some category, Some value -> category + ":" + value
            | Some category, None -> category + ":" + "No Value"
            | None, Some value -> value
            | None, None -> ""
