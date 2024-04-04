import { equal, deepEqual, notEqual } from 'assert';
import { JsonController, OntologyAnnotation } from "./ARCtrl/index.js"

describe('JsonController', function () {
    it("OntologyAnnotation", function () {
        let oa = new OntologyAnnotation("instrument model", "MS", "MS:08239081")
        let json = JsonController.OntologyAnnotation.toJsonString(oa)
        let oa2 = JsonController.OntologyAnnotation.fromJsonString(json)
        deepEqual(oa, oa2, "OntologyAnnotation before and after write-read should be same.")
    });
});