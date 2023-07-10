﻿module CompositeColumnTests

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open ISA
open ISA.Spreadsheet

open TestObjects.ArcTable

let private parameterTests = 
    testList "Parameter" [
        testCase "SingleWithUnit" (fun () ->
            let cols = 
                initTableCols
                    [Parameter.appendTemperatureColumn]
            let col = CompositeColumn.fromFsColumns cols
                
            Expect.equal col.Header Parameter.temperatureHeader "Header did not match"
            Expect.equal col.Cells.Length 1 "Wrong number of cells"
            Expect.equal col.Cells.[0] Parameter.temperatureValue "Value did not match"
        )
        testCase "SingleWithTerm" (fun () ->
            let cols = 
                initTableCols
                    [Parameter.appendInstrumentColumn]
            let col = CompositeColumn.fromFsColumns cols
                
            Expect.equal col.Header Parameter.instrumentHeader "Header did not match"
            Expect.equal col.Cells.Length 1 "Wrong number of cells"
            Expect.equal col.Cells.[0] Parameter.instrumentValue "Value did not match"
        )
    ]
    
let characteristicTests = 
    testList "Characteristic" [
        testCase "SingleWithTerm" (fun () ->
            ///Same test as above, but with characteristic and organism header and value
            let cols = 
                initTableCols
                    [Characteristic.appendOrganismColumn]
            let col = CompositeColumn.fromFsColumns cols

            Expect.equal col.Header Characteristic.organismHeader "Header did not match"
            Expect.equal col.Cells.Length 1 "Wrong number of cells"
            Expect.equal col.Cells.[0] Characteristic.organismValue "Value did not match"
        )      
    ]

/// Factor tests, one test (unitized) with time header and value
let factorTests = 
    testList "Factor" [
        testCase "SingleWithUnit" (fun () ->
            let cols = 
                initTableCols
                    [Factor.appendTimeColumn]
            let col = CompositeColumn.fromFsColumns cols
            Expect.equal col.Header Factor.timeHeader "Header did not match"
            Expect.equal col.Cells.Length 1 "Wrong number of cells"
            Expect.equal col.Cells.[0] Factor.timeValue "Value did not match"
        )
    ]

let protocolTests = 
    testList "Protocol" [
        testCase "REFSingle" (fun () ->
            let cols = 
                initTableCols
                    [Protocol.REF.appendLolColumn]
            let col = CompositeColumn.fromFsColumns cols
            Expect.equal col.Header Protocol.REF.lolHeader "Header did not match"
            Expect.equal col.Cells.Length 1 "Wrong number of cells"
            Expect.equal col.Cells.[0] Protocol.REF.lolValue "Value did not match"
        )
        testCase "TypeSingle" (fun () ->
            let cols =
                initTableCols
                    [Protocol.Type.appendCollectionColumn]
            let col = CompositeColumn.fromFsColumns cols
            Expect.equal col.Header Protocol.Type.collectionHeader "Header did not match"
            Expect.equal col.Cells.Length 1 "Wrong number of cells"
            Expect.equal col.Cells.[0] Protocol.Type.collectionValue "Value did not match"
        )
    ]





let main = 
    testList "CompositeColumn" [
        parameterTests
        characteristicTests
        factorTests
        protocolTests
    ]