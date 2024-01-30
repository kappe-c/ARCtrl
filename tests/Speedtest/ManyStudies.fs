﻿module ManyStudies

open ARCtrl.ISA.Spreadsheet
open ARCtrl.ISA


let write () =

    let inv = ArcInvestigation.init("MyInvestigation")
    for i = 0 to 1500 do 
        let s = ArcStudy.init($"Study{i}")
        inv.AddRegisteredStudy(s)
    ArcInvestigation.toFsWorkbook inv