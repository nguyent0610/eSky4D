
var selectedIndex = 0;
var _hold = "H";
var _curSlsperid = "";
var _focusrecord = 0;
var beforeedit = '';
var prefixvalue = "";
var lastfixvalue = "";
var shownextlotserial = "";
var chooseGrid = "";
var click = 0;
var tmpImageDelete = 0;
var tmpMediaDelete = 0;
var tmpSelectedNode = "";
var tmpCopyForm = "0";
var tmpCopyFormSave = "0";
var tmpOldFileName = "";
var cpnyIDTree = "";

var clickcboCustId = "0";
var tmpAddNew = "0";
var tampSaveButton = "0";
var parentRecordIDAll = "";
var parentRecordID = ""
var selectNodeAfterInsert = "0";
var tmpCountryloadStoreOrForm = "0";
var menuClick = function (command) {
    switch (command) {
        case "first":
            

            var combobox = App.cboCustId;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboCustId.setValue(combobox.store.getAt(0).data.CustId);

         

            break;
        case "prev":
          

            var combobox = App.cboCustId;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboCustId.setValue(combobox.store.getAt(index - 1).data.CustId);

      

            break;
        case "next":
          

            var combobox = App.cboCustId;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboCustId.setValue(combobox.store.getAt(index + 1).data.CustId);

   
            break;
        case "last":

            var combobox = App.cboCustId;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboCustId.setValue(App.cboCustId.store.getAt(App.cboCustId.store.getTotalCount() - 1).data.CustId);




            break;
        case "refresh":
            App.storeFormBig.reload();
            

            break;
        case "new":
            if (isInsert) {
                //App.cboCustId.setValue('');
                //tmpAddNew = "1";
                App.cboCustId.setValue('');
                var time = new Date();
                var record = Ext.create("App.AR_CustomerModel", {
                    //CustId: "",
                    Status: "H",
                    ClassId: "CN",
                    CustType: "N",
                    PriceClassID: "NPPCH",
                    CrRule: "N",
                    Territory: "R001",
                    Country: "VN",
                    State: "0043",
                    City: "HCM",
                    District: "0043001",
                    ExpiryDate: time,
                    EstablishDate: time,
                    Birthdate: time,
                    TaxDflt: "C",
                    TaxID00: "VAT10",
                    TaxID01: "VAT10",
                    TaxID02: "VAT10",
                    TaxID03: "VAT10"



                });
                App.storeFormBig.insert(0, record);
                App.dataForm.getForm().loadRecord(App.storeFormBig.getAt(0));
                //tmpAddNew = "1";
                    //App.cboCustId.setValue('');
                    //App.storeFormBig.reload();
                //var index = App.ApproveStatusAll.indexOf(App.ApproveStatusAll.findRecord("Code", "H"));
                    //var time = new Date();
                    //App.calExpiryDate.setValue(time);
            }
            break;
        case "delete":
            var curRecord = App.dataForm.getRecord();
            if (isDelete) {
                if (App.cboCustId.value != "" && App.cboStatus.value != "A" && App.cboStatus.value != "O") {

                    
                        callMessage(11, '', 'deleteRecordForm');
                
                }
            }
            break;
        case "save":
            if (isUpdate || isInsert || isDelete) {

                
                   // if (App.Tree.selected.items[0].childNodes.length != 0) {
                        Save();
                   // } else {
                    //    callMessage(8081, '', null);
                    //}
                


            }
            break;
        case "print":
            alert(command);
            break;
        case "close":
            Close();
            break;
    }

};
var refreshTree = function (i) {
    App.Tree.select(i);
}
var refreshTree2 = function (dt) {
    App.cboInvtID.setValue(dt);
    var findNode = App.IDTree.items.items[0].store.data.items;
    for (var i = 0; i < findNode.length; i++) {
        if (findNode[i].data.id == node) {
            //click = 1;
            App.Tree.select(i);
            //setTimeout(function () { refreshTree(i); }, 7000);
            break;

        }

    }
}
function Save() {

    
    var curRecord = App.dataForm.getRecord();
   

    //var tmpforChangeFucn = App.dataForm.getRecord().data.LossRate00;
    //App.dataForm.getRecord().data.LossRate00 = 0;
    //App.dataForm.getRecord().data.LossRate00 = tmpforChangeFucn;
    //App.chooseGrid.setValue("2")
    //setValueToGrid();
    //App.chooseGrid.setValue("1")

    App.dataForm.getForm().updateRecord();
   //App.dataForm.updateRecord(curRecord);
    if (App.dataForm.isValid()) {
        App.dataForm.submit({
            waitMsg: 'Submiting...',
            url: 'AR20400/Save',
            params: {
                lstheader: Ext.encode(App.storeFormBig.getChangedData({ skipIdForPhantomRecords: false })),//,
                CustID: App.cboCustId.getValue(),
                Handle: App.cboHandle.getValue(),
                NodeID: App.Tree.selected.items[0].data.text,
                NodeLevel: App.Tree.selected.items[0].data.depth,
                ParentRecordID: parentRecordID,
                hadchild: App.Tree.selected.items[0].childNodes.length,
                Status: App.cboStatus.getValue(),
                tmpSelectedNode: tmpSelectedNode,
                branchID: App.cboCpnyID.value,
                custName: App.txtCustName.value,
            },
            success: function (result, data) {
                //if (data.result.statusAfterAll == "H") {
                //    enableForm();
                //}
                tmpCopyForm = "0";
                App.cboCustId.getStore().reload();
                
                //App.cboHandle.setValue("");
                callMessage(201405071, '', null);
                //App.cboInvtID.setValue('');
                menuClick("refresh");
                //tampSaveButton = "1";
                if (data.result.value3 == "addNew") {
                    if (data.result.value4 == "0") {
                        //ReloadTree();
                        //setTimeout(function () { waitReloadTreeDone(data.result.value); }, 4000);
                        //App.cboCustId.setValue(data.result.value);
                        var newNode = App.Tree.selected.items[0].data.id;
                        var record = App.IDTree.getStore().getNodeById(newNode);
                        var node = data.result.value + "-" + data.result.value2;
                        record.appendChild({ text: node, leaf: true, id: node });

                        //cach lam tim node tren cay de select vao`
                        //var findNode = App.IDTree.items.items[0].store.data.items;
                        //for (var i = 0; i < findNode.length; i++) {
                        //    if (findNode[i].data.id == node) {
                        //        //click = 1;
                        //        //selectNodeAfterInsert = "1";
                        //        App.Tree.select(i);
                        //        break;
                        //        //setTimeout(function () { refreshTree(i); }, 7000);
                                
                        //    }
                        //}

                        //cach lam set value lai cho ID
                        App.cboCustId.setValue("");
                        setTimeout(function () { waitReloadTreeDone(data.result.value); }, 3000);
                        //App.cboCustId.setValue(data.result.value);
                    } else if (data.result.value4 == "1") {
                        //ReloadTree();
                        //setTimeout(function () { waitReloadTreeDone(data.result.value); }, 4000);
                        var tmpSelectedNode = data.result.value5;
                        var recordtmpSelectedNode = App.IDTree.getStore().getNodeById(tmpSelectedNode);
                        recordtmpSelectedNode.remove(true);

                        var newNode = App.Tree.selected.items[0].data.id;
                        var record = App.IDTree.getStore().getNodeById(newNode);

                        var node = data.result.value + "-" + data.result.value2;
                 
                        record.appendChild({ text: node, leaf: true, id: node });

                        App.cboCustId.setValue("");
                        setTimeout(function () { waitReloadTreeDone(data.result.value); }, 3000);
                        //App.cboCustId.setValue(data.result.value);



                    }
                }
                if (data.result.value3 == "update") {
                    //ReloadTree();
                    //setTimeout(function () { waitReloadTreeDone(data.result.value); }, 4000);
                    var newNode = App.Tree.selected.items[0].data.id;
                    var record = App.IDTree.getStore().getNodeById(newNode);
                    var node = data.result.value + "-" + data.result.value2;
                    if (newNode != node) {
                        record.parentNode.appendChild({ text: node, leaf: true, id: node });
                        var findNode = App.IDTree.items.items[0].store.data.items;
                        for (var i = 0; i < findNode.length; i++) {
                            if (findNode[i].data.id == node) {
                                //click = 1;
                                App.Tree.select(i);
                                continue;
                                //setTimeout(function () { refreshTree(i); }, 7000);

                            }
                        }
                        var newNode2 = App.Tree.selected.items[0].data.id;
                        var record2 = App.IDTree.getStore().getNodeById(newNode2);
                        record2.remove(true);
                        record.parentNode.replaceChild(record2, record);
                        var findNode = App.IDTree.items.items[0].store.data.items;
                        for (var i = 0; i < findNode.length; i++) {
                            if (findNode[i].data.id == node) {
                                //click = 1;
                                App.Tree.select(i);
                                //setTimeout(function () { refreshTree(i); }, 7000);
                                break;
                            }
                        }
                    }
                    
                    //App.cboCustId.setValue(data.result.value);


                }



                
            }
            , failure: function (errorMsg, data) {

                //var dt = Ext.decode(data.response.responseText);
                //callMessage(dt.code, dt.colName + ',' + dt.value, null);
                if (data.result.code) {
                    callMessage(data.result.code, '', '');
                }
                else {
                    processMessage(errorMsg, data);
                }
            }
        });
    }
    else {
        var fields = App.dataForm.getForm().getFields().each(
                function (item) {
                    if (!item.isValid()) {
                        alert(item);
                    }
                }
            );
    }
}

var waitReloadTreeDone = function (value){
    //App.cboCustId.setValue(data.result.value);
    App.cboCustId.setValue(value);
}



// Xem lai
function Close() {
    if (App.dataForm.getRecord() != undefined) App.dataForm.updateRecord();
    if (storeIsChange(App.storeFormBig, false)) {
        callMessage(5, '', 'closeScreen');

    } else {
        parent.App.tabAR20400.close();
    }
}
// Xem lai
function storeIsChange(store, isCreate) {
    if (isCreate == undefined) isCreate = true;
    if ((isCreate == true ? store.getChangedData().Created.length > 1 : false)
        || store.getChangedData().Updated != undefined
        || store.getChangedData().Deleted != undefined) {
        return true;
    }
    return false;
};
var closeScreen = function (item) {
    if (item == "yes") {

        Save();
    }
    else {
        if (parent.App.tabAR20400 != null)
            parent.App.tabAR20400.close();
    }
};
// Xac nhan xoa record tren grid
var deleteRecordForm = function (item) {
    if (item == "yes") {

        try {
            App.direct.AR20400Delete(App.cboCustId.getValue(),App.cboCpnyID.value,App.cboStatus.value, {
                success: function (data) {
                    
                    menuClick('refresh');
                    var custIDcustName = App.cboCustId.value + "-" + App.txtCustName.value;
                    var record = App.IDTree.getStore().getNodeById(custIDcustName);
                    App.cboCustId.getStore().reload();
                    App.cboCustId.setValue('');
                    App.storeFormBig.reload();
                    //App.Tree.selected().remove(true);
                    record.remove(true)

                    
                },
                failure: function () {
                    //
                    alert("fail");
                },
                eventMask: { msg: '@Util.GetLang("DeletingData")', showMask: true }
            });
        } catch (ex) {
            alert(ex.message);
        }
    }
};



var loadDataAutoHeaderSmall = function () {

    if (App.storeFormSmall.getCount() == 0) {
        App.storeFormSmall.insert(0, Ext.data.Record());
    }
    var record = App.storeFormSmall.getAt(0);
    if (record) {
        App.dataForm.getForm().loadRecord(record);
        App.cboCpnyID.getStore().reload();
    }
};

var loadGrid = function () {

    
}
var setValueApproveStatus = function () {
    App.IDTree.store.tree.root.collapse();
}
var chooseNodeFromTree = function () {
    //tu dong bat node tree khi chon 1 cai tu cboCustId
    if (App.cboStatus.value == "O" || App.cboStatus.value == "A") {
        disableForm();
    } else if (App.cboStatus.value == "H") {
        enableForm();
    }
    var custIDcustName = App.cboCustId.value + "-" + App.txtCustName.value;
    
    var record = App.IDTree.getStore().getNodeById(custIDcustName);
    var depth = App.IDTree.getStore().getNodeById(custIDcustName).data.depth;
    if (depth == 3) {
        //setTimeout(function () { waitCollacpse(); }, 1500);
        //App.IDTree.store.tree.root.collapse();
        App.IDTree.getStore().getNodeById(custIDcustName).parentNode.parentNode.parentNode.expand();
        App.IDTree.getStore().getNodeById(custIDcustName).parentNode.parentNode.expand();
        App.IDTree.getStore().getNodeById(custIDcustName).parentNode.expand();
    } else if (depth == 2) {
        //setTimeout(function () { waitCollacpse(); }, 1500);
        //App.IDTree.store.tree.root.collapse();
        App.IDTree.getStore().getNodeById(custIDcustName).parentNode.parentNode.expand();
        App.IDTree.getStore().getNodeById(custIDcustName).parentNode.expand();
    } else if (depth == 1) {
        //setTimeout(function () { waitCollacpse(); }, 1500);
        //App.IDTree.store.tree.root.collapse();
        App.IDTree.getStore().getNodeById(custIDcustName).parentNode.expand();
    }
    var findNode = App.IDTree.items.items[0].store.data.items;
    for (var i = 0; i < findNode.length; i++) {
        if (findNode[i].data.id == custIDcustName) {
            App.Tree.select(i);
            tmpSelectedNode = App.Tree.selected.items[0].data.id;
            break;
        }

    }
   
    //App.IDTree.getStore().getNodeById(custIDcustName).select();
}
var waitapproveStatus = function () {
    //load len cboapprovestatus
    if (App.cboApproveStatus.value == "" || App.cboApproveStatus.value == null) {
        App.cboApproveStatus.setValue("H");
        setTimeout(function () { waitToLoadHandle(); }, 1000);
    }
    //load hinh len khi chon invtID
    var curRecord = App.dataForm.getRecord();
    if (curRecord) {
        if (curRecord.data.Picture) {
            App.direct.GetImages(curRecord.data.Picture);
        } else {
            App.imgPPCStorePicReq.setImageUrl("");
        }
        if (curRecord.data.Media) {
            setMediaImage();
        }
    }else{
        App.imgPPCStorePicReq.setImageUrl("");

    }



}

var waitToLoadHandle = function () {
    if (App.cboHandle.value == null || App.cboHandle.value == "") {
        App.cboHandle.setValue("N");
    }
}
var cboInvtID_Change = function (sender, e) {
    if (tmpCopyForm == "1") {
        tmpCopyFormSave = "1";
        App.cboApproveStatus.setValue("H");
        setTimeout(function () { waitToLoadHandle(); }, 1000);
    } else {
        App.imgPPCStoreMediaReq.setImageUrl("");
        App.chooseGrid.setValue("1");
        App.storeFormBig.load();
        App.storeGrid.load();
        //if (App.cboInvtID.value != "" && App.Tree.selected.items[0].childNodes[0] == undefined) {
        setTimeout(function () { choosefromcboInvtID(); }, 2000);
        //}
        setTimeout(function () { waitapproveStatus(); }, 2000);
    }
    //tmpCopyForm = "0";
};






//var chkIsAttachFile_Change = function (sender, e) {
//    if (e) {
//        App.chkIsDeleteFile.setValue(true);
//        App.chkIsDeleteFile.disable();
//    }
//    else {
//        App.chkIsDeleteFile.enable();
//    }


//}

//var cboReportID_Change = function (sender, e) {
//    //App.SelectionModelMailAutoDetail.selected.items[0].set('ReportViewID', '');

//    App.cboReportViewID.getStore().load();

//}




var NodeSelected_Change = function (store, operation, options) {
    //if (click == 0) {
    parentRecordIDAll = options.textContent.split("-");
    parentRecordID = parentRecordIDAll[0];
    //if (selectNodeAfterInsert == "1") {
    //    selectNodeAfterInsert = "0";
    //} else {
    //enableForm();
        if (operation.childNodes.length == 0) {
            var custIDall = options.textContent.split("-")
            var custID1 = custIDall[0];
            App.cboCustId.setValue(custID1);
        }
    //}
   // }
    //click = 0;

}


var cboApproveStatus_Change = function () {
    //App.cboApproveStatus.setValue("H");
    App.cboHandle.store.reload();
    


}

//var waitTreeLoad = function () {
//    App.cboCustId.getStore().reload();
//}


var cboCpnyID_Change = function (sender, newValue, oldValue) {
    //cpnyIDTree = App.cboCpnyID.value;
    App.cboCustId.getStore().reload();
    ReloadTree();
    //setTimeout(function () { waitTreeLoad(); }, 3000);
    
}

var waitCityAndDistrictLoad = function () {
    App.cboCity.setValue("HCM");
    App.cboDistrict.setValue("0043001");
    
}
var waitStateLoad = function () {
    App.cboState.setValue("0043");
    setTimeout(function () { waitCityAndDistrictLoad(); }, 1500);
}
//dang DirectMethod co the load duoc node root Tree
function ReloadTree() {
        try {
            App.direct.ReloadTree(App.cboCpnyID.getValue(), {
                success: function (data) {
                    //if (tampSaveButton == "1") {
                    //    tampSaveButton = "0";
                    //} else {
                        //App.cboCustId.getStore().reload();
                        App.cboHandle.getStore().reload();
                        App.cboCountry.setValue("VN");
                        setTimeout(function () { waitStateLoad(); }, 1500);
                   // }
                },
                failure: function () {
                    //
                    alert("fail");
                },
                eventMask: { msg: 'Loading Tree', showMask: true }
            });
        } catch (ex) {
            alert(ex.message);
        }
    
};


var cboStatus_Change = function (sender, newValue, oldValue) {
    App.cboHandle.getStore().reload();
    //if (App.cboStatus.value == "O" || App.cboStatus.value == "A") {
    //    disableForm();
    //} else if (App.cboStatus.value == "H") {
    //    enableForm();
    //}
}



var loadDataAutoHeaderBig = function () {
    //if (tmpAddNew == "1") {


    //    tmpAddNew = "0";

    //} else
    //{
        //if(App.storeFormBig.getCount() == 0) {
        //    App.storeFormBig.insert(0, Ext.data.Record());
        //}
        //var record = App.storeFormBig.getAt(0);
        //if (record) {
        //    App.dataForm.getForm().loadRecord(record);
        //}
    //}
    var record = App.storeFormBig.getAt(0);
    if (record) {
        App.dataForm.getForm().loadRecord(record);
    } else {
        var time = new Date();
        var record = Ext.create("App.AR_CustomerModel", {
            //CustId: "",
            Status: "H",
            ClassId: "CN",
            CustType: "N",
            PriceClassID: "NPPCH",
            CrRule: "N",
            Territory: "R001",
            Country: "VN",
            State: "0043",
            City: "HCM",
            District: "0043001",
            ExpiryDate: time,
            EstablishDate: time,
            Birthdate: time,
            TaxDflt: "C",
            TaxID00: "VAT10",
            TaxID01: "VAT10",
            TaxID02: "VAT10",
            TaxID03: "VAT10"



        });
        App.storeFormBig.insert(0, record);
        App.dataForm.getForm().loadRecord(App.storeFormBig.getAt(0));
    }

};

var cboCustId_Change = function (sender, newValue, oldValue) {
    tmpCountryloadStoreOrForm = "1";
     App.storeFormBig.reload();
     setTimeout(function () { chooseNodeFromTree(); }, 1000);
     if (App.cboStatus.value == "O" || App.cboStatus.value == "A") {
         disableForm();
     } else if (App.cboStatus.value == "H") {
         enableForm();
     }
    
}

var cboCountry_Change = function (sender, newValue, oldValue) {
    //bien tam tmpCountryloadStoreOrForm de bat neu chon CustID thi load form len du sau do set ve trang thai cu
    if (tmpCountryloadStoreOrForm == "1") {
        App.cboState.getStore().reload();
        App.dataForm.getForm().loadRecord(App.dataForm.getRecord());
        
    } else {
        App.cboState.getStore().reload();
    }

}


var cboState_Change = function (sender, newValue, oldValue) {
    if (tmpCountryloadStoreOrForm == "1") {
        App.cboCity.getStore().reload();
        App.cboDistrict.getStore().reload();
        App.dataForm.getForm().loadRecord(App.dataForm.getRecord());
        tmpCountryloadStoreOrForm = "0";
    } else {
        App.cboCity.getStore().reload();
        App.cboDistrict.getStore().reload();
    }
}

var cboBillCountry_Change = function (sender, newValue, oldValue) {
    if (tmpCountryloadStoreOrForm == "1") {
        App.cboBillState.getStore().reload();
        App.dataForm.getForm().loadRecord(App.dataForm.getRecord());
    } else {
        App.cboBillState.getStore().reload();
    }

}


var cboBillState_Change = function (sender, newValue, oldValue) {
    if (tmpCountryloadStoreOrForm == "1") {
        App.cboBillCity.getStore().reload();
        App.dataForm.getForm().loadRecord(App.dataForm.getRecord());
        tmpCountryloadStoreOrForm = "0";
    } else {
        App.cboBillCity.getStore().reload();
    }
}
var waitBillCountryLoad = function(){
    App.cboBillState.setValue(App.cboState.value);
}
var waitBillStateLoad = function () {
    App.cboBillCity.setValue(App.cboCity.value);
}
var CopyFormToBill_Click = function () {
    tmpCountryloadStoreOrForm = "0"
    App.txtBillName.setValue(App.txtCustNam2.value);
    App.txtBillAttn.setValue(App.txtAttn.value);
    App.txtBillSalut.setValue(App.txtSalut.value);
    App.txtBillAddr1.setValue(App.txtAddr1.value);
    App.txtBillAddr2.setValue(App.txtAddr2.value);
    App.cboBillCountry.setValue(App.cboCountry.value);
    setTimeout(function () { waitBillCountryLoad(); }, 500);
    setTimeout(function () { waitBillStateLoad(); }, 500);
    App.txtBillZip.setValue(App.txtZip.value);
    App.txtBillPhone.setValue(App.txtPhone.value);
    App.txtBillFax.setValue(App.txtFax.value);
    
}

var disableForm = function () {
    //App.cboCustId.disable();
    App.cboCpnyID.disable();
    App.cboClassId.disable();
    App.cboCustType.disable();
    App.txtCustName.disable();
    App.cboPriceClassID.disable();
    App.cboTerms.disable();
    App.txtTradeDisc.disable();
    App.cboCrRule.disable();
    App.txtCrLmt.disable();
    App.txtGracePer.disable();
    App.cboTerritory.disable();
    App.cboArea.disable();
    App.cboLocation.disable();
    App.cboChannel.disable();
    App.cboShopType.disable();
    App.chkGiftExch.disable();
    App.chkHasPG.disable();
    App.cboSlsperId.disable();
    App.cboDeliveryID.disable();
    App.cboSupID.disable();
    App.cboSiteId.disable();
    App.cboDfltShipToId.disable();
    App.txtCustFillPriority.disable();
    App.cboLTTContract.disable();
    App.cboDfltSalesRouteID.disable();
    App.txtEmpNum.disable();
    App.calExpiryDate.disable();
    App.cboEstablishDate.disable();
    App.cboBirthday.disable();
    App.txtCustNam2.disable();
    App.txtAttn.disable();
    App.txtSalut.disable();
    App.txtAddr1.disable();
    App.txtAddr2.disable();
    App.cboCountry.disable();
    App.cboState.disable();
    App.cboCity.disable();
    App.cboDistrict.disable();
    App.txtZip.disable();
    App.txtPhone.disable();
    App.txtFax.disable();
    App.txtEMailAddr.disable();
    App.btnCopy.disable();
    App.txtBillName.disable();
    App.txtBillAttn.disable();
    App.txtBillSalut.disable();
    App.txtBillAddr1.disable();
    App.txtBillAddr2.disable();
    App.cboBillCountry.disable();
    App.cboBillState.disable();
    App.cboBillCity.disable();
    App.txtBillZip.disable();
    App.txtBillPhone.disable();
    App.txtBillFax.disable();
    App.cboTaxDflt.disable();
    App.txtTaxRegNbr.disable();
    App.txtTaxLocId.disable();
    App.cboTaxID00.disable();
    App.cboTaxID01.disable();
    App.cboTaxID02.disable();
    App.cboTaxID03.disable();
    //App.dataFormTopLeftTab1.disable();
    //App.dataFormTopRightTab1.disable();
    //App.dataFormBotLeftTab1.disable();
    //App.dataFormBotRightTab1.disable();
    //App.dataFormLeftTab2.disable();
    //App.dataFormRightTab2.disable();
    //App.dataFormMidTab3.disable();

}

var enableForm = function () {
    //App.cboCustId.enable();
    App.cboCpnyID.enable();
    App.cboClassId.enable();
    App.cboCustType.enable();
    App.txtCustName.enable();
    App.cboPriceClassID.enable();
    App.cboTerms.enable();
    App.txtTradeDisc.enable();
    App.cboCrRule.enable();
    App.txtCrLmt.enable();
    App.txtGracePer.enable();
    App.cboTerritory.enable();
    App.cboArea.enable();
    App.cboLocation.enable();
    App.cboChannel.enable();
    App.cboShopType.enable();
    App.chkGiftExch.enable();
    App.chkHasPG.enable();
    App.cboSlsperId.enable();
    App.cboDeliveryID.enable();
    App.cboSupID.enable();
    App.cboSiteId.enable();
    App.cboDfltShipToId.enable();
    App.txtCustFillPriority.enable();
    App.cboLTTContract.enable();
    App.cboDfltSalesRouteID.enable();
    App.txtEmpNum.enable();
    App.calExpiryDate.enable();
    App.cboEstablishDate.enable();
    App.cboBirthday.enable();
    App.txtCustNam2.enable();
    App.txtAttn.enable();
    App.txtSalut.enable();
    App.txtAddr1.enable();
    App.txtAddr2.enable();
    App.cboCountry.enable();
    App.cboState.enable();
    App.cboCity.enable();
    App.cboDistrict.enable();
    App.txtZip.enable();
    App.txtPhone.enable();
    App.txtFax.enable();
    App.txtEMailAddr.enable();
    App.btnCopy.enable();
    App.txtBillName.enable();
    App.txtBillAttn.enable();
    App.txtBillSalut.enable();
    App.txtBillAddr1.enable();
    App.txtBillAddr2.enable();
    App.cboBillCountry.enable();
    App.cboBillState.enable();
    App.cboBillCity.enable();
    App.txtBillZip.enable();
    App.txtBillPhone.enable();
    App.txtBillFax.enable();
    App.cboTaxDflt.enable();
    App.txtTaxRegNbr.enable();
    App.txtTaxLocId.enable();
    App.cboTaxID00.enable();
    App.cboTaxID01.enable();
    App.cboTaxID02.enable();
    App.cboTaxID03.enable();
    //App.dataFormTopLeftTab1.enable();
    //App.dataFormTopRightTab1.enable();
    //App.dataFormBotLeftTab1.enable();
    //App.dataFormBotRightTab1.enable();
    //App.dataFormLeftTab2.enable();
    //App.dataFormRightTab2.enable();
    //App.dataFormMidTab3.enable();

}

var tabSA_Setup_AfterRender = function (obj, padding) {
    if (this.parentAutoLoadControl != null)
        obj.setHeight(this.parentAutoLoadControl.getHeight() - padding);
    else {
        obj.setHeight(Ext.getBody().getViewSize().height - padding);
    }
};