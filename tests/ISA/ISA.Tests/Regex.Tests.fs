﻿module Regex.Tests

open ISA

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

let private tests_AutoGeneratedTableName = 
    testList "AutoGeneratedTableName" [
        testCase "match" (fun () ->
            let testString = @"New Table 10"
            let number = 
                match testString with
                | Regex.ActivePatterns.AutoGeneratedTableName num -> Some num
                | _ -> None
            Expect.isSome number "isSome"
            Expect.equal number.Value 10 ""
        )
        testCase "no match, no number" (fun () ->
            let testString = @"New Table Testing"
            let number = 
                match testString with
                | Regex.ActivePatterns.AutoGeneratedTableName num -> Some num
                | _ -> None
            Expect.isNone number ""
        )
        testCase "no match, wrong start" (fun () ->
            let testString = @"My New Table"
            let number = 
                match testString with
                | Regex.ActivePatterns.AutoGeneratedTableName num -> Some num
                | _ -> None
            Expect.isNone number ""
        )
        testCase "no match, wrong end mixed" (fun () ->
            let testString = @"New Table 10 x20"
            let number = 
                match testString with
                | Regex.ActivePatterns.AutoGeneratedTableName num -> Some num
                | _ -> None
            Expect.isNone number ""
        )
        testCase "no match, wrong end numbers" (fun () ->
            let testString = @"New Table 10 20"
            let number = 
                match testString with
                | Regex.ActivePatterns.AutoGeneratedTableName num -> Some num
                | _ -> None
            Expect.isNone number ""
        )
    ]

let private tests_AnnotationTableColums =
    testList "AnnotationTableColumns" [
        testCase "Term Source REF" (fun () ->
            let localID = "12345"
            let space = "UO"
            let testString = $"Term Source REF ({space}:{localID})"
            let r = 
                match testString with
                | Regex.ActivePatterns.TSRColumnHeader result -> Some result
                | _ -> None
            Expect.isSome r "Could not match TSRColumnHeader"
            let rv = r.Value
            Expect.equal rv.LocalTAN localID "LocalId did not match"
            Expect.equal rv.TermSourceREF space "TermSourceREF did not match"
            Expect.equal rv.TermAccessionNumber $"{space}:{localID}" "TermAccessionNumber did not match"
        )
        testCase "Term Source REF Empty" (fun () ->
            let testString = $"Term Source REF ()"
            let r = 
                match testString with
                | Regex.ActivePatterns.TSRColumnHeader result -> Some result
                | _ -> None
            Expect.isSome r "Could not match TSRColumnHeader"
            let rv = r.Value
            Expect.equal rv.LocalTAN "" "LocalID should be empty"
            Expect.equal rv.TermSourceREF "" "TermSourceREF should be empty"
            Expect.equal rv.TermAccessionNumber "" "TermAccessionNumber should be empty"
        )
        testCase "Term Accession Number" (fun () ->
            let localID = "12345"
            let space = "UO"
            let testString = $"Term Accession Number ({space}:{localID})"
            let r = 
                match testString with
                | Regex.ActivePatterns.TANColumnHeader result -> Some result
                | _ -> None
            Expect.isSome r "Could not match TANColumnHeader"
            let rv = r.Value
            Expect.equal rv.LocalTAN localID "LocalId did not match"
            Expect.equal rv.TermSourceREF space "TermSourceREF did not match"
            Expect.equal rv.TermAccessionNumber $"{space}:{localID}" "TermAccessionNumber did not match"      
        )
        testCase "Term Accession Number Empty" (fun () ->
            let testString = $"Term Accession Number ()"
            let r = 
                match testString with
                | Regex.ActivePatterns.TANColumnHeader result -> Some result
                | _ -> None
            Expect.isSome r "Could not match TANColumnHeader"
            let rv = r.Value
            Expect.equal rv.LocalTAN "" "LocalID should be empty"
            Expect.equal rv.TermSourceREF "" "TermSourceREF should be empty"
            Expect.equal rv.TermAccessionNumber "" "TermAccessionNumber should be empty"
        )
        testCase "Reference Column Header Empty" (fun () ->

            let testString = $"Term Accession Number ()"
            let r = 
                match testString with
                | Regex.ActivePatterns.ReferenceColumnHeader result -> Some result
                | _ -> None
            Expect.isSome r "Could not match TANColumnHeader"
            let rv = r.Value
            Expect.equal rv.Annotation "" "Annotation should be empty"

            let testString = $"Term Source REF ()"
            let r = 
                match testString with
                | Regex.ActivePatterns.ReferenceColumnHeader result -> Some result
                | _ -> None
            Expect.isSome r "Could not match TANColumnHeader"
            let rv = r.Value
            Expect.equal rv.Annotation "" "Annotation should be empty"

            let testString = $"Any Other String ()"
            let r = 
                match testString with
                | Regex.ActivePatterns.ReferenceColumnHeader result -> Some result
                | _ -> None
            Expect.isNone r "Should not match other String"
        )
    ]
    


let main = 
    testList "Regex" [
        tests_AnnotationTableColums
        tests_AutoGeneratedTableName
    ]

