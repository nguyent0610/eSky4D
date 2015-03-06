
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

var tmpCity = "";
var tmpDistrict = "";

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.combo.first(App.cboCustId, HQ.isChange);

            //var combobox = App.cboCustId;
            //    var v = combobox.getValue();
            //    var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
            //    var index = combobox.store.indexOf(record);
            //    App.cboCustId.setValue(combobox.store.getAt(0).data.CustId);



            break;
        case "prev":
            HQ.combo.next(App.cboCustId, HQ.isChange);

            //var combobox = App.cboCustId;
            //    var v = combobox.getValue();
            //    var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
            //    var index = combobox.store.indexOf(record);
            //    App.cboCustId.setValue(combobox.store.getAt(index - 1).data.CustId);



            break;
        case "next":
            HQ.combo.prev(App.cboCustId, HQ.isChange);

            //var combobox = App.cboCustId;
            //    var v = combobox.getValue();
            //    var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
            //    var index = combobox.store.indexOf(record);
            //    App.cboCustId.setValue(combobox.store.getAt(index + 1).data.CustId);


            break;
        case "last":
            HQ.combo.last(App.cboCustId, HQ.isChange);
            //var combobox = App.cboCustId;
            //    var v = combobox.getValue();
            //    var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
            //    var index = combobox.store.indexOf(record);
            //    App.cboCustId.setValue(App.cboCustId.store.getAt(App.cboCustId.store.getTotalCount() - 1).data.CustId);




            break;
        case "refresh":
            //App.storeFormBig.reload();
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }

            break;
        case "new":
            if (HQ.isInsert) {
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
                    ExpiryDate: HQ.businessDate,
                    EstablishDate: HQ.businessDate,
                    Birthdate: HQ.businessDate,
                    TaxDflt: "C",
                    TaxID00: "VAT10",
                    TaxID01: "VAT10",
                    TaxID02: "VAT10",
                    TaxID03: "VAT10"



                });
                App.storeFormBig.insert(0, record);
                App.frmMain.getForm().loadRecord(App.storeFormBig.getAt(0));
                //tmpAddNew = "1";
                //App.cboCustId.setValue('');
                //App.storeFormBig.reload();
                //var index = App.ApproveStatusAll.indexOf(App.ApproveStatusAll.findRecord("Code", "H"));
                //var time = new Date();
                //App.calExpiryDate.setValue(time);
            }
            break;
        case "delete":
            var curRecord = App.frmMain.getRecord();
            if (HQ.isDelete) {
                if (App.cboCustId.value != "" && App.cboStatus.value != "A" && App.cboStatus.value != "O") {


                    HQ.message.show(11, '', 'deleteRecordForm');

                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {

                if (HQ.form.checkRequirePass(App.frmMain))
                {
                    // if (App.slmTree.selected.items[0].childNodes.length != 0) {
                    Save();
                // } else {
                //    HQ.message.show(8081, '', null);
                //}

                }

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
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        if (App.cboCustId.valueModels == null) App.cboCustId.setValue('');
        App.cboCustId.getStore().reload();
        App.storeFormBig.reload();
    }
};

////////////Kiem tra combo chinh CustID
//khi co su thay doi du lieu cua cac conttol tren form
//load lần đầu khi mở
var firstLoad = function () {
    App.cboCustId.getStore().reload();
    App.storeFormBig.reload();
};
var frmChange = function () {
    //đề phòng trường hợp nếu store chưa có gì cả giao diện chưa load xong mà App.frmMain.getForm().updateRecord(); được gọi sẽ gây lỗi
    if (App.storeFormBig.data.length > 0) {
        App.frmMain.getForm().updateRecord();

        HQ.isChange = HQ.store.isChange(App.storeFormBig);
        HQ.common.changeData(HQ.isChange, 'IN20500');//co thay doi du lieu gan * tren tab title header
        HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
        if (App.cboCustId.valueModels == null || HQ.isNew == true)//App.cboCustId.valueModels == null khi ko co select item nao
            App.cboCustId.setReadOnly(false);
        else App.cboCustId.setReadOnly(HQ.isChange);
    }

};

var refreshTree = function (i) {
    App.slmTree.select(i);
}
var refreshTree2 = function (dt) {
    App.cboInvtID.setValue(dt);
    var findNode = App.IDTree.items.items[0].store.data.items;
    for (var i = 0; i < findNode.length; i++) {
        if (findNode[i].data.id == node) {
            //click = 1;
            App.slmTree.select(i);
            //setTimeout(function () { refreshTree(i); }, 7000);
            break;

        }

    }
}
function Save() {


    var curRecord = App.frmMain.getRecord();


    //var tmpforChangeFucn = App.frmMain.getRecord().data.LossRate00;
    //App.frmMain.getRecord().data.LossRate00 = 0;
    //App.frmMain.getRecord().data.LossRate00 = tmpforChangeFucn;
    //App.chooseGrid.setValue("2")
    //setValueToGrid();
    //App.chooseGrid.setValue("1")

    App.frmMain.getForm().updateRecord();
    //App.frmMain.updateRecord(curRecord);
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang('Submiting...'),
            url: 'AR20400/Save',
            params: {
                lstheader: Ext.encode(App.storeFormBig.getChangedData({ skipIdForPhantomRecords: false })),//,
                CustID: App.cboCustId.getValue(),
                Handle: App.cboHandle.getValue(),
                NodeID: App.slmTree.selected.items[0].data.text,
                NodeLevel: App.slmTree.selected.items[0].data.depth,
                ParentRecordID: parentRecordID,
                hadchild: App.slmTree.selected.items[0].childNodes.length,
                Status: App.cboStatus.getValue(),
                tmpSelectedNode: tmpSelectedNode,
                branchID: App.cboCpnyID.value,
                custName: App.txtCustName.value,
                isNew: HQ.isNew,
            },
            success: function (result, data) {
                //if (data.result.statusAfterAll == "H") {
                //    enableForm();
                //}
                tmpCopyForm = "0";
                App.cboCustId.getStore().reload();

                //App.cboHandle.setValue("");
                HQ.message.show(201405071, '', null);
                //App.cboInvtID.setValue('');
                App.storeFormBig.reload();
                //tampSaveButton = "1";
                if (data.result.addNewOrUpdate == "addNew") {
                    if (data.result.changeTreeBranch == "0") {//nếu ko chuyển nhánh khác cho node
                        //ReloadTree();
                        //setTimeout(function () { waitReloadTreeDone(data.result.custID); }, 4000);
                        //App.cboCustId.setValue(data.result.custID);
                        var newNode = App.slmTree.selected.items[0].data.id;
                        var record = App.IDTree.getStore().getNodeById(newNode);
                        var node = data.result.custID + "-" + data.result.custName;
                        record.appendChild({ text: node, leaf: true, id: node });

                        //cach lam tim node tren cay de select vao`
                        //var findNode = App.IDTree.items.items[0].store.data.items;
                        //for (var i = 0; i < findNode.length; i++) {
                        //    if (findNode[i].data.id == node) {
                        //        //click = 1;
                        //        //selectNodeAfterInsert = "1";
                        //        App.slmTree.select(i);
                        //        break;
                        //        //setTimeout(function () { refreshTree(i); }, 7000);

                        //    }
                        //}

                        //cach lam set value lai cho ID
                        App.cboCustId.setValue("");
                        setTimeout(function () { waitReloadTreeDone(data.result.custID); }, 3000);
                        //App.cboCustId.setValue(data.result.custID);
                    } else if (data.result.changeTreeBranch == "1") {//nếu chuyển nhánh khác cho node
                        //ReloadTree();
                        //setTimeout(function () { waitReloadTreeDone(data.result.custID); }, 4000);
                        var tmpSelectedNode = data.result.value5;
                        var recordtmpSelectedNode = App.IDTree.getStore().getNodeById(tmpSelectedNode);
                        recordtmpSelectedNode.remove(true);

                        var newNode = App.slmTree.selected.items[0].data.id;
                        var record = App.IDTree.getStore().getNodeById(newNode);

                        var node = data.result.custID + "-" + data.result.custName;

                        record.appendChild({ text: node, leaf: true, id: node });

                        App.cboCustId.setValue("");
                        setTimeout(function () { waitReloadTreeDone(data.result.custID); }, 3000);
                        //App.cboCustId.setValue(data.result.custID);



                    }
                }
                if (data.result.addNewOrUpdate == "update") {
                    //ReloadTree();
                    //setTimeout(function () { waitReloadTreeDone(data.result.custID); }, 4000);
                    var newNode = App.slmTree.selected.items[0].data.id;
                    var record = App.IDTree.getStore().getNodeById(newNode);
                    var node = data.result.custID + "-" + data.result.custName;
                    if (newNode != node) {
                        record.parentNode.appendChild({ text: node, leaf: true, id: node });
                        var findNode = App.IDTree.items.items[0].store.data.items;
                        for (var i = 0; i < findNode.length; i++) {
                            if (findNode[i].data.id == node) {
                                //click = 1;
                                App.slmTree.select(i);
                                continue;
                                //setTimeout(function () { refreshTree(i); }, 7000);

                            }
                        }
                        var newNode2 = App.slmTree.selected.items[0].data.id;
                        var record2 = App.IDTree.getStore().getNodeById(newNode2);
                        record2.remove(true);
                        record.parentNode.replaceChild(record2, record);
                        var findNode = App.IDTree.items.items[0].store.data.items;
                        for (var i = 0; i < findNode.length; i++) {
                            if (findNode[i].data.id == node) {
                                //click = 1;
                                App.slmTree.select(i);
                                //setTimeout(function () { refreshTree(i); }, 7000);
                                break;
                            }
                        }
                    }

                    //App.cboCustId.setValue(data.result.custID);


                }
                App.storeFormBig.reload();




            }
            , failure: function (msg, data) {

                if (data.result.msgCode) {
                    if (data.result.msgCode == 2000)//loi trung key ko the add
                        HQ.message.show(data.result.msgCode, [App.cboCustId.fieldLabel, App.cboCustId.getValue()], '', true);
                    else HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                }
                else {
                    HQ.message.process(msg, data, true);
                }
            }
        });
    }
    else {
        var fields = App.frmMain.getForm().getFields().each(
                function (item) {
                    if (!item.isValid()) {
                        alert(item);
                    }
                }
            );
    }
}

var waitReloadTreeDone = function (value) {
    //App.cboCustId.setValue(data.result.custID);
    App.cboCustId.setValue(value);
}



// Xem lai
function Close() {
    if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
    if (storeIsChange(App.storeFormBig, false)) {
        HQ.message.show(5, '', 'closeScreen');

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

        App.frmMain.submit({
            waitMsg: HQ.common.getLang('DeletingData'),
            url: 'AR20400/AR20400Delete',
            params: {
                custID: App.cboCustId.getValue(),
                cpnyID: App.cboCpnyID.getValue(),
                status: App.cboStatus.value
            },
            //App.direct.AR20400Delete(App.cboCustId.getValue(),App.cboCpnyID.value,App.cboStatus.value, {
            success: function (data) {

                menuClick('refresh');
                var custIDcustName = App.cboCustId.value + "-" + App.txtCustName.value;
                var record = App.IDTree.getStore().getNodeById(custIDcustName);
                App.cboCustId.getStore().reload();
                App.cboCustId.setValue('');
                App.storeFormBig.reload();
                //App.slmTree.selected().remove(true);
                record.remove(true)


            },
            failure: function () {
                //
                alert("fail");
            },
            eventMask: { msg: '@Util.GetLang("DeletingData")', showMask: true }
        });

    }
};



//var loadDataAutoHeaderSmall = function () {

//    if (App.storeFormSmall.getCount() == 0) {
//        App.storeFormSmall.insert(0, Ext.data.Record());
//    }
//    var record = App.storeFormSmall.getAt(0);
//    if (record) {
//        App.frmMain.getForm().loadRecord(record);
//        App.cboCpnyID.getStore().reload();
//    }
//};

//var loadGrid = function () {


//}
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
    if (App.IDTree.getStore().getNodeById(custIDcustName)) {
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
                App.slmTree.select(i);
                tmpSelectedNode = App.slmTree.selected.items[0].data.id;
                break;
            }

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
    var curRecord = App.frmMain.getRecord();
    if (curRecord) {
        if (curRecord.data.Picture) {
            App.direct.GetImages(curRecord.data.Picture);
        } else {
            App.imgPPCStorePicReq.setImageUrl("");
        }
        if (curRecord.data.Media) {
            setMediaImage();
        }
    } else {
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
        //if (App.cboInvtID.value != "" && App.slmTree.selected.items[0].childNodes[0] == undefined) {
        setTimeout(function () { choosefromcboInvtID(); }, 2000);
        //}
        setTimeout(function () { waitapproveStatus(); }, 2000);
    }
    //tmpCopyForm = "0";
};






//var chkIsAttachFile_Change = function (sender, e) {
//    if (e) {
//        App.chkIsDeleteFile.setValue(true);
//        App.chkIsDeleteFile.readOnly = true;
//    }
//    else {
//        App.chkIsDeleteFile.readOnly = false;
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

//var waitCityAndDistrictLoad = function () {
//    App.cboCity.setValue("HCM");
//    App.cboDistrict.setValue("0043001");

//}
//var waitStateLoad = function () {
//    App.cboState.setValue("0043");
//    setTimeout(function () { waitCityAndDistrictLoad(); }, 1500);
//}
//dang DirectMethod co the load duoc node root slmTree
function ReloadTree() {
    try {
        App.direct.ReloadTree(App.cboCpnyID.getValue(), {
            success: function (data) {
                //if (tampSaveButton == "1") {
                //    tampSaveButton = "0";
                //} else {
                //App.cboCustId.getStore().reload();
                //App.cboCountry.setValue("VN");
                //setTimeout(function () { waitStateLoad(); }, 1500);
                App.cboHandle.getStore().reload();
                
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


//trước khi load trang busy la dang load data
var storeBeforeLoad = function (store) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'),App.frmMain);
};
var storeLoad = function (store) {
    HQ.common.showBusy(false);
    HQ.isNew = false;
    App.cboCustId.forceSelection = true;
    if (store.data.length == 0) {
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
            ExpiryDate: HQ.businessDate,
            EstablishDate: HQ.businessDate,
            Birthdate: HQ.businessDate,
            TaxDflt: "C",
            TaxID00: "VAT10",
            TaxID01: "VAT10",
            TaxID02: "VAT10",
            TaxID03: "VAT10"



        });
        App.storeFormBig.insert(0, record);
        //App.cboCountry.setValue("VN");
        //setTimeout(function () { waitStateLoad(); }, 1500);
        App.frmMain.getForm().loadRecord(App.storeFormBig.getAt(0));
        //HQ.store.insertBlank(store, "CustId");
        //record = store.getAt(0);
        store.commitChanges();//commit cho record thanh updated muc dich de dung ham HQ.store.isChange
        HQ.isNew = true;//record la new
        App.cboCustId.forceSelection = false;
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboCustId.focus(true);//focus ma khi tao moi
    }
    var record = store.getAt(0);
    tmpCity = store.getAt(0).data.City;
    tmpDistrict = store.getAt(0).data.District;
    App.frmMain.getForm().loadRecord(record);
    
    frmChange();
};

var cboCustId_Change = function (sender, newValue, oldValue) {
    if (sender.valueModels != null) {
        tmpCountryloadStoreOrForm = "1";
        App.storeFormBig.reload();
        App.cboDfltShipToId.getStore().reload();
        App.cboLTTContract.getStore().reload();
        setTimeout(function () { chooseNodeFromTree(); }, 1000);
        if (App.cboStatus.value == "O" || App.cboStatus.value == "A") {
            disableForm();
        } else if (App.cboStatus.value == "H") {
            enableForm();
        }
    }

}

//khi nhan combo xo ra, neu da thay doi thi ko xo ra
var cboCustId_Expand = function (sender, value) {
    if (App.cboCustId.getStore().data.length == 0) {
        App.cboCustId.getStore().reload();
    }
    if (HQ.isChange && App.cboCustId.getValue()) {
        App.cboCustId.collapse();
    }
};

var cboCountry_Change = function (sender, newValue, oldValue) {
    //bien tam tmpCountryloadStoreOrForm de bat neu chon CustID thi load form len du sau do set ve trang thai cu
    if (tmpCountryloadStoreOrForm == "1") {
        App.cboState.getStore().reload();
        App.frmMain.getForm().loadRecord(App.frmMain.getRecord());

    } else {
        App.cboState.getStore().reload();
    }
    //App.cboState.getStore().load(function () {
    //    var curRecord = App.frmMain.getRecord();
    //    if (curRecord != undefined)
    //        if (curRecord.data.State) {
    //            App.cboState.setValue(curRecord.data.State);
    //        }
    //    var dt = HQ.store.findInStore(App.cboState.getStore(), ["State"], [App.cboState.getValue()]);
    //    if (!dt) {
    //        curRecord.data.State = '';
    //        App.cboState.setValue("");
    //    }

    //});
}


var cboState_Change = function (sender, newValue, oldValue) {
    if (tmpCountryloadStoreOrForm == "1") {
        App.cboCity.getStore().reload();
        App.cboDistrict.getStore().reload();
        App.frmMain.getForm().loadRecord(App.frmMain.getRecord());
        App.cboCity.setValue(tmpCity);
        App.cboDistrict.setValue(tmpDistrict);
        tmpCountryloadStoreOrForm = "0";
    } else {
        App.cboCity.getStore().reload();
        App.cboDistrict.getStore().reload();
    }
    //App.cboCity.getStore().load(function () {
    //    var curRecord = App.frmMain.getRecord();
    //    if (curRecord != undefined)
    //        if (curRecord.data.City) {
    //            App.cboCity.setValue(curRecord.data.City);
    //        }
    //    var dt = HQ.store.findInStore(App.cboCity.getStore(), ["City"], [App.cboCity.getValue()]);
    //    if (!dt) {
    //        curRecord.data.City = '';
    //        App.cboCity.setValue("");
    //    }

    //});

    //App.cboDistrict.getStore().load(function () {
    //    var curRecord = App.frmMain.getRecord();
    //    if (curRecord != undefined)
    //        if (curRecord.data.District) {
    //            App.cboDistrict.setValue(curRecord.data.District);
    //        }
    //    var dt = HQ.store.findInStore(App.cboCity.getStore(), ["District"], [App.cboDistrict.getValue()]);
    //    if (!dt) {
    //        curRecord.data.City = '';
    //        App.cboDistrict.setValue("");
    //    }

    //});
}

var cboBillCountry_Change = function (sender, newValue, oldValue) {
    if (tmpCountryloadStoreOrForm == "1") {
        App.cboBillState.getStore().reload();
        App.frmMain.getForm().loadRecord(App.frmMain.getRecord());
    } else {
        App.cboBillState.getStore().reload();
    }

}


var cboBillState_Change = function (sender, newValue, oldValue) {
    if (tmpCountryloadStoreOrForm == "1") {
        App.cboBillCity.getStore().reload();
        App.frmMain.getForm().loadRecord(App.frmMain.getRecord());
        tmpCountryloadStoreOrForm = "0";
    } else {
        App.cboBillCity.getStore().reload();
    }
}
var waitBillCountryLoad = function () {
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
    //App.cboCustId.readOnly = true;
    App.cboCpnyID.readOnly = true;
    App.cboClassId.readOnly = true;
    App.cboCustType.readOnly = true;
    App.txtCustName.readOnly = true;
    App.cboPriceClassID.readOnly = true;
    App.cboTerms.readOnly = true;
    App.txtTradeDisc.readOnly = true;
    App.cboCrRule.readOnly = true;
    App.txtCrLmt.readOnly = true;
    App.txtGracePer.readOnly = true;
    App.cboTerritory.readOnly = true;
    App.cboArea.readOnly = true;
    App.cboLocation.readOnly = true;
    App.cboChannel.readOnly = true;
    App.cboShopType.readOnly = true;
    App.chkGiftExch.readOnly = true;
    App.chkHasPG.readOnly = true;
    App.cboSlsperId.readOnly = true;
    App.cboDeliveryID.readOnly = true;
    App.cboSupID.readOnly = true;
    App.cboSiteId.readOnly = true;
    App.cboDfltShipToId.readOnly = true;
    App.txtCustFillPriority.readOnly = true;
    App.cboLTTContract.readOnly = true;
    App.cboDfltSalesRouteID.readOnly = true;
    App.txtEmpNum.readOnly = true;
    App.calExpiryDate.readOnly = true;
    App.cboEstablishDate.readOnly = true;
    App.cboBirthday.readOnly = true;
    App.txtCustNam2.readOnly = true;
    App.txtAttn.readOnly = true;
    App.txtSalut.readOnly = true;
    App.txtAddr1.readOnly = true;
    App.txtAddr2.readOnly = true;
    App.cboCountry.readOnly = true;
    App.cboState.readOnly = true;
    App.cboCity.readOnly = true;
    App.cboDistrict.readOnly = true;
    App.txtZip.readOnly = true;
    App.txtPhone.readOnly = true;
    App.txtFax.readOnly = true;
    App.txtEMailAddr.readOnly = true;
    App.btnCopy.readOnly = true;
    App.txtBillName.readOnly = true;
    App.txtBillAttn.readOnly = true;
    App.txtBillSalut.readOnly = true;
    App.txtBillAddr1.readOnly = true;
    App.txtBillAddr2.readOnly = true;
    App.cboBillCountry.readOnly = true;
    App.cboBillState.readOnly = true;
    App.cboBillCity.readOnly = true;
    App.txtBillZip.readOnly = true;
    App.txtBillPhone.readOnly = true;
    App.txtBillFax.readOnly = true;
    App.cboTaxDflt.readOnly = true;
    App.txtTaxRegNbr.readOnly = true;
    App.txtTaxLocId.readOnly = true;
    App.cboTaxID00.readOnly = true;
    App.cboTaxID01.readOnly = true;
    App.cboTaxID02.readOnly = true;
    App.cboTaxID03.readOnly = true;
    //App.frmTopLeftTab1.readOnly = true;
    //App.frmTopRightTab1.readOnly = true;
    //App.frmBotLeftTab1.readOnly = true;
    //App.frmBotRightTab1.readOnly = true;
    //App.frmLeftTab2.readOnly = true;
    //App.frmRightTab2.readOnly = true;
    //App.frmMidTab3.readOnly = true;

}

var enableForm = function () {
    //App.cboCustId.readOnly = false;
    App.cboCpnyID.readOnly = false;
    App.cboClassId.readOnly = false;
    App.cboCustType.readOnly = false;
    App.txtCustName.readOnly = false;
    App.cboPriceClassID.readOnly = false;
    App.cboTerms.readOnly = false;
    App.txtTradeDisc.readOnly = false;
    App.cboCrRule.readOnly = false;
    App.txtCrLmt.readOnly = false;
    App.txtGracePer.readOnly = false;
    App.cboTerritory.readOnly = false;
    App.cboArea.readOnly = false;
    App.cboLocation.readOnly = false;
    App.cboChannel.readOnly = false;
    App.cboShopType.readOnly = false;
    App.chkGiftExch.readOnly = false;
    App.chkHasPG.readOnly = false;
    App.cboSlsperId.readOnly = false;
    App.cboDeliveryID.readOnly = false;
    App.cboSupID.readOnly = false;
    App.cboSiteId.readOnly = false;
    App.cboDfltShipToId.readOnly = false;
    App.txtCustFillPriority.readOnly = false;
    App.cboLTTContract.readOnly = false;
    App.cboDfltSalesRouteID.readOnly = false;
    App.txtEmpNum.readOnly = false;
    App.calExpiryDate.readOnly = false;
    App.cboEstablishDate.readOnly = false;
    App.cboBirthday.readOnly = false;
    App.txtCustNam2.readOnly = false;
    App.txtAttn.readOnly = false;
    App.txtSalut.readOnly = false;
    App.txtAddr1.readOnly = false;
    App.txtAddr2.readOnly = false;
    App.cboCountry.readOnly = false;
    App.cboState.readOnly = false;
    App.cboCity.readOnly = false;
    App.cboDistrict.readOnly = false;
    App.txtZip.readOnly = false;
    App.txtPhone.readOnly = false;
    App.txtFax.readOnly = false;
    App.txtEMailAddr.readOnly = false;
    App.btnCopy.readOnly = false;
    App.txtBillName.readOnly = false;
    App.txtBillAttn.readOnly = false;
    App.txtBillSalut.readOnly = false;
    App.txtBillAddr1.readOnly = false;
    App.txtBillAddr2.readOnly = false;
    App.cboBillCountry.readOnly = false;
    App.cboBillState.readOnly = false;
    App.cboBillCity.readOnly = false;
    App.txtBillZip.readOnly = false;
    App.txtBillPhone.readOnly = false;
    App.txtBillFax.readOnly = false;
    App.cboTaxDflt.readOnly = false;
    App.txtTaxRegNbr.readOnly = false;
    App.txtTaxLocId.readOnly = false;
    App.cboTaxID00.readOnly = false;
    App.cboTaxID01.readOnly = false;
    App.cboTaxID02.readOnly = false;
    App.cboTaxID03.readOnly = false;
    //App.frmTopLeftTab1.readOnly = false;
    //App.frmTopRightTab1.readOnly = false;
    //App.frmBotLeftTab1.readOnly = false;
    //App.frmBotRightTab1.readOnly = false;
    //App.frmLeftTab2.readOnly = false;
    //App.frmRightTab2.readOnly = false;
    //App.frmMidTab3.readOnly = false;

}

var tabSA_Setup_AfterRender = function (obj, padding) {
    if (this.parentAutoLoadControl != null)
        obj.setHeight(this.parentAutoLoadControl.getHeight() - padding);
    else {
        obj.setHeight(Ext.getBody().getViewSize().height - padding);
    }
};