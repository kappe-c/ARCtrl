﻿namespace ARCtrl.Json

open Thoth.Json.Core

open ARCtrl
open ARCtrl.Process

module Sample =

    module ROCrate = 

        let genID (s:Sample) : string = 
            match s.ID with
            | Some id -> id
            | None -> match s.Name with
                      | Some n -> "#Sample_" + n.Replace(" ","_")
                      | None -> "#EmptySample"
    
        let encoder (oa : Sample) = 

            let additionalProperties = 
                oa.Characteristics |> Option.defaultValue [] |> List.map MaterialAttributeValue.ROCrate.encoder
                |> List.append (oa.FactorValues |> Option.defaultValue [] |> List.map FactorValue.ROCrate.encoder)
            [
                "@id", Encode.string (oa |> genID)
                "@type", (Encode.list [ Encode.string "Sample"])
                Encode.tryInclude "name" Encode.string (oa.Name)
                Encode.tryIncludeList "additionalProperties" id additionalProperties
                "@context", ROCrateContext.Sample.context_jsonvalue
            ]
            |> Encode.choose
            |> Encode.object

        let additionalPropertyDecoder = 
            {new Decoder<MaterialAttributeValue option * FactorValue option> with
                member this.Decode(s,json) = 
                    let additionalType =
                        if s.hasProperty "additionalType" json then
                            match Decode.string.Decode(s,s.getProperty("additionalType",json)) with
                            | Ok s -> s
                            | _ -> ""
                        else ""
                    if additionalType = "FactorValue" then
                        match FactorValue.ROCrate.decoder.Decode(s,json) with
                        | Ok fv -> Ok (None, Some fv)
                        | Error e -> Error e
                    else 
                        match MaterialAttributeValue.ROCrate.decoder.Decode(s,json) with
                        | Ok ma -> Ok (Some ma, None)
                        | Error e -> Error e                   
            }

        let decoder : Decoder<Sample> =         
            
            Decode.object (fun get ->
                let additionalProperties = get.Optional.Field "additionalProperties" (Decode.list additionalPropertyDecoder)
                let characteristics,factors = 
                    match additionalProperties with
                    | None -> None, None
                    | Some additionalProperties -> 
                        additionalProperties
                        |> List.choose fst
                        |> Helper.Option.fromValueWithDefault [],
                        additionalProperties 
                        |> List.choose snd
                        |> Helper.Option.fromValueWithDefault []
                {
                    ID = get.Optional.Field "@id" Decode.uri
                    Name = get.Optional.Field "name" Decode.string
                    Characteristics = characteristics
                    FactorValues = factors
                    DerivesFrom = get.Optional.Field "derivesFrom" (Decode.list Source.ROCrate.decoder)
                }
            )

    module ISAJson =
    
        let encoder (oa : Sample) = 
            [
                Encode.tryInclude "@id" Encode.string (oa.ID)
                Encode.tryInclude "name" Encode.string (oa.Name)
                Encode.tryIncludeListOpt "characteristics" MaterialAttributeValue.ISAJson.encoder oa.Characteristics
                Encode.tryIncludeListOpt "factorValues" FactorValue.ISAJson.encoder oa.FactorValues
                Encode.tryIncludeListOpt "derivesFrom" Source.ISAJson.encoder oa.DerivesFrom
            ]
            |> Encode.choose
            |> Encode.object

        let allowedFields = ["@id";"name";"characteristics";"factorValues";"derivesFrom";"@type"; "@context"]

        let decoder: Decoder<Sample> =       
            Decode.objectNoAdditionalProperties allowedFields (fun get ->
                {
                    ID = get.Optional.Field "@id" Decode.uri
                    Name = get.Optional.Field "name" Decode.string
                    Characteristics = get.Optional.Field "characteristics" (Decode.list MaterialAttributeValue.ISAJson.decoder)
                    FactorValues = get.Optional.Field "factorValues" (Decode.list FactorValue.ISAJson.decoder)
                    DerivesFrom = get.Optional.Field "derivesFrom" (Decode.list Source.ISAJson.decoder)
                }
            )

[<AutoOpen>]
module SampleExtensions =
    
    type Sample with

        static member fromISAJsonString (s:string) = 
            Decode.fromJsonString Sample.ISAJson.decoder s   

        static member toISAJsonString(?spaces) =
            fun (f:Sample) ->
                Sample.ISAJson.encoder f
                |> Encode.toJsonString (Encode.defaultSpaces spaces)

        member this.ToISAJsonString(?spaces) =
            Sample.toISAJsonString(?spaces=spaces) this

        static member fromROCrateString (s:string) =
            Decode.fromJsonString Sample.ROCrate.decoder s

        static member toROCrateString(?spaces) =
            fun (f:Sample) ->
                Sample.ROCrate.encoder f
                |> Encode.toJsonString (Encode.defaultSpaces spaces)

        member this.ToROCrateString(?spaces) =
            Sample.toROCrateString(?spaces=spaces) this
