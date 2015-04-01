
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
var parentRecordIDAll = "";
var parentRecordID = ""

//bien tam Grid
var keys = ['CpnyID'];
var fieldsCheckRequire = ["CpnyID"];
var fieldsLangCheckRequire = ["CpnyID"];



var menuClick = function (command) {
    switch (command) {
        case "first":
            //if (_focusrecord == 1) {

            //    var combobox = App.cboInvtID;
            //    var v = combobox.getValue();
            //    var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
            //    var index = combobox.store.indexOf(record);
            //    App.cboInvtID.setValue(combobox.store.getAt(0).data.InvtID);

            //} else if (_focusrecord == 2) {
            //    App.slmGrid.select(0);
            //}
            if (HQ.focus == 'header') {
                HQ.combo.first(App.cboInvtID, HQ.isChange);
            } else if (HQ.focus == 'grd') {
                HQ.grid.first(App.grd);
            }

            break;
        case "prev":
            //if (_focusrecord == 1) {

            //    var combobox = App.cboInvtID;
            //    var v = combobox.getValue();
            //    var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
            //    var index = combobox.store.indexOf(record);
            //    App.cboInvtID.setValue(combobox.store.getAt(index - 1).data.InvtID);

            //} else if (_focusrecord == 2) {
            //    App.slmGrid.selectPrevious();
            //}
            if (HQ.focus == 'header') {
                HQ.combo.prev(App.cboInvtID, HQ.isChange);
            } else if (HQ.focus == 'grd') {
                HQ.grid.prev(App.grd);
            }

            break;
        case "next":
            //if (_focusrecord == 1) {

            //    var combobox = App.cboInvtID;
            //    var v = combobox.getValue();
            //    var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
            //    var index = combobox.store.indexOf(record);
            //    App.cboInvtID.setValue(combobox.store.getAt(index + 1).data.InvtID);

            //} else if (_focusrecord == 2) {
            //    App.slmGrid.selectNext();
            //}
            if (HQ.focus == 'header') {
                HQ.combo.next(App.cboInvtID, HQ.isChange);
            } else if (HQ.focus == 'grd') {
                HQ.grid.next(App.grd);
            }
            break;
        case "last":
            //if (_focusrecord == 1) {

            //    var combobox = App.cboInvtID;
            //    var v = combobox.getValue();
            //    var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
            //    var index = combobox.store.indexOf(record);
            //    App.cboInvtID.setValue(App.cboInvtID.store.getAt(App.cboInvtID.store.getTotalCount() - 1).data.InvtID);

            //} else if (_focusrecord == 2) {
            //    App.slmGrid.select(App.storeGrid.getCount() - 1);
            //}
            if (HQ.focus == 'header') {
                HQ.combo.last(App.cboInvtID, HQ.isChange);
            } else if (HQ.focus == 'grd') {
                HQ.grid.last(App.grd);
            }



            break;
        case "refresh":
            //App.storeFormBig.reload();
            //App.storeGrid.reload();
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            } else {
                HQ.isChange = false;
                //App.cboMailID.getStore().load(function () { App.stoMailHeader.reload(); });

            }
            break;
        case "new":
            if (HQ.isInsert) {

                if (HQ.focus == 'header') {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', '');
                    }
                    else {
                        App.cboInvtID.setValue('');
                        App.storeFormBig.reload();
                    }

                } else if (HQ.focus == 'grd') {
                    HQ.grid.insert(App.grd, keys);
                }

            }
            break;
        case "delete":
            var curRecord = App.frmMain.getRecord();
            if (HQ.isDelete) {
             
                if (HQ.focus == 'header') {
                    if (App.cboInvtID.value != "" && App.cboStatus.value != "A" && App.cboStatus.value != "O") {

                        HQ.message.show(11, '', 'deleteRecordForm');
                    } else if (HQ.focus == 'grdL') {
                        var rowindex = HQ.grid.indexSelect(App.grd);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grd), ''], 'deleteRecordGrid', true)
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {

                if (!App.slmTree.selected.items.length>0) {
                    HQ.message.show(213, '', '', '');
                    return;
                }
                if (HQ.form.checkRequirePass(App.frmMain) &&
                    HQ.store.checkRequirePass(App.storeGrid, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    Save();
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
        if (App.cboInvtID.valueModels == null) App.cboInvtID.setValue('');
        App.cboInvtID.getStore().reload();
        App.storeFormBig.reload();
        App.storeGrid.reload();

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

    if (tmpCopyForm == "1") {
        setValueToGrid();
    }
    var curRecord = App.frmMain.getRecord();
    curRecord.data.Picture = App.imgPPCStorePicReq.imageUrl;

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
            waitMsg: 'Submiting...',
            url: 'IN20500/Save',
            params: {
                lstheader: Ext.encode(App.storeFormBig.getChangedData({ skipIdForPhantomRecords: false })),//,
                lstheader2: Ext.encode(App.storeFormSmall.getChangedData({ skipIdForPhantomRecords: false })),
                lstgrd: Ext.encode(App.storeGrid.getChangedData({ skipIdForPhantomRecords: false })),
                InvtID: App.cboInvtID.getValue(),
                Handle: App.cboHandle.getValue(),
                NodeID: App.slmTree.selected.items[0].data.text,
                NodeLevel: App.slmTree.selected.items[0].data.depth,
                ParentRecordID: parentRecordID,
                hadchild: App.slmTree.selected.items[0].childNodes.length,
                approveStatus: App.cboApproveStatus.getValue(),
                Public: App.chkPublic.getValue(),
                StkItem: App.chkStkItem.getValue(),
                imageChange: App.frmMain.getRecord().data.Picture,
                tmpImageDelete: tmpImageDelete,
                tmpMediaDelete: tmpMediaDelete,
                tmpImageForUpload: App.imgPPCStorePicReq.imageUrl,
                tmpSelectedNode: tmpSelectedNode,
                tmpCopyFormSave: tmpCopyFormSave,
                tmpCopyForm: tmpCopyForm,
                tmpCopyFormImageUrl: curRecord.data.Picture,
                tmpCopyFormMedia: curRecord.data.Media,
                tmpOldFileName: tmpOldFileName,
                mediaExist: App.frmMain.getRecord().data.Media,
            },
            success: function (result, data) {
                tmpCopyForm = "0";
                App.cboInvtID.getStore().reload();
                //App.cboHandle.setValue("");
                HQ.message.show(201405071, '', null);
                //App.cboInvtID.setValue('');
                //menuClick("refresh");
                //var dt = Ext.decode(data.response.responseText);
                //App.cboInvtID.setValue("");

                //setTimeout(function () { refreshTree(dt); }, 2000);
                //App.cboInvtID.setValue(dt);

                refresh('yes');

                //setTimeout(function () { refreshTree(result,data); }, 1000);
                if (data.result.addNewOrUpdate == "addNew") {
                    if (data.result.tmpChangeTreeDic == "0") {
                        var newNode = App.slmTree.selected.items[0].data.id;
                        var record = App.IDTree.getStore().getNodeById(newNode);
                        var node = data.result.invtID + "-" + data.result.Descr;

                        record.appendChild({ text: node, leaf: true, id: node });

                        var findNode = App.IDTree.items.items[0].store.data.items;
                        for (var i = 0; i < findNode.length; i++) {
                            if (findNode[i].data.id == node) {
                                //click = 1;
                                App.slmTree.select(i);
                                //setTimeout(function () { refreshTree(i); }, 7000);
                                break;
                            }
                        }
                    } else if (data.result.tmpChangeTreeDic == "1") {
                        var tmpSelectedNode = data.result.tmpSelectedNode;
                        var recordtmpSelectedNode = App.IDTree.getStore().getNodeById(tmpSelectedNode);
                        recordtmpSelectedNode.remove(true);

                        var newNode = App.slmTree.selected.items[0].data.id;
                        var record = App.IDTree.getStore().getNodeById(newNode);

                        var node = data.result.invtID + "-" + data.result.Descr;
                        //var findNode = App.IDTree.items.items[0].store.data.items;
                        //for (var i = 0; i < findNode.length; i++) {
                        //    if (findNode[i].data.id == node) {
                        //        //click = 1;
                        //        App.slmTree.select(i);
                        //        //setTimeout(function () { refreshTree(i); }, 7000);
                        //        break;
                        //    }
                        //}
                        //var newNode2 = App.slmTree.selected.items[0].data.id;
                        //var recordDeleteNode = App.IDTree.getStore().getNodeById(newNode2);
                        //recordDeleteNode.remove(true);
                        record.appendChild({ text: node, leaf: true, id: node });
                        App.cboInvtID.setValue("");
                        //for (var i = 0; i < findNode.length; i++) {
                        //    if (findNode[i].data.id == node) {
                        //        //click = 1;
                        //        App.slmTree.select(i);
                        //        //setTimeout(function () { refreshTree(i); }, 7000);

                        //        break;
                        //    }
                        //}



                    }
                }
                if (data.result.addNewOrUpdate == "update") {
                    var newNode = App.slmTree.selected.items[0].data.id;
                    var record = App.IDTree.getStore().getNodeById(newNode);
                    var node = data.result.invtID + "-" + data.result.Descr;
                    if (newNode != node) {
                        record.parentNode.appendChild({ text: node, leaf: true, id: node });
                        var findNode = App.IDTree.items.items[0].store.data.items;
                        for (var i = 0; i < findNode.length; i++) {
                            if (findNode[i].data.id == node) {
                                //click = 1;
                                App.slmTree.select(i);
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


                }

            }
            , failure: function (errorMsg, data) {

                //var dt = Ext.decode(data.response.responseText);
                //HQ.message.show(dt.code, dt.colName + ',' + dt.value, null);
                if (data.result.code) {
                    HQ.message.show(data.result.code, '', '');
                }
                else {
                    HQ.message.process(errorMsg, data, true);
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


// Xem lai
function Close() {
    if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
    if (App.storeGrid.getChangedData().Updated == undefined &&
        App.storeGrid.getChangedData().Deleted == undefined &&
        App.frmMain.getRecord() == undefined)
        parent.App.tabIN20500.close();
    else if (App.storeGrid.getChangedData().Updated != undefined ||
        App.storeGrid.getChangedData().Created != undefined ||
        App.storeGrid.getChangedData().Deleted != undefined ||
        storeIsChange(App.storeFormSmall, false)) {
        HQ.message.show(5, '', 'closeScreen');
    } else {
        parent.App.tabIN20500.close();
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
        if (parent.App.tabIN20500 != null)
            parent.App.tabIN20500.close();
    }
};
// Xac nhan xoa record tren grid
var deleteRecordForm = function (item) {
    if (item == "yes") {

        //try {
        //    App.direct.IN20500Delete(App.cboInvtID.getValue(), {
        //        success: function (data) {
        //            menuClick('refresh');
        //            var invtIDDescr = App.cboInvtID.value + "-" + App.txtDescr.value;
        //            var record = App.IDTree.getStore().getNodeById(invtIDDescr);
        //            App.cboInvtID.getStore().reload();
        //            App.cboInvtID.setValue('');
        //            App.storeFormBig.reload();
        //            //App.Tree.selected().remove(true);
        //            record.remove(true)


        //        },
        //        failure: function () {
        //            //
        //        },
        //        eventMask: { msg: '@Util.GetLang("DeletingData")', showMask: true }
        //    });
        //} catch (ex) {
        //    alert(ex.message);
        //}

        App.frmMain.submit({
            clientValidation: false,
            timeout: 1800000,
            waitMsg: HQ.common.getLang('DeletingData'),
            url: 'IN20500/IN20500Delete',
            params: {
                invtID: App.cboInvtID.getValue(),
                cpnyID: App.cboCpnyID.getValue(),
                status: App.cboStatus.value
            },
            //App.direct.AR20400Delete(App.cboCustId.getValue(),App.cboCpnyID.value,App.cboStatus.value, {
            success: function (data) {

                menuClick('refresh');
                var invtIDDescr = App.cboInvtID.value + "-" + App.txtDescr.value;
                var record = App.IDTree.getStore().getNodeById(invtIDDescr);
                App.cboInvtID.getStore().reload();
                App.cboInvtID.setValue('');
                App.storeFormBig.reload();
                //App.Tree.selected().remove(true);
                record.remove(true)



            },
            failure: function () {
                //
                if (data.result.msgCode) {
                    if (data.result.msgCode == 2000)//loi trung key ko the add
                        HQ.message.show(data.result.msgCode, [App.cboInvtID.fieldLabel, App.cboInvtID.getValue()], '', true);
                    else HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                }
                else {
                    HQ.message.process(msg, data, true);
                }
            },
            eventMask: { msg: '@Util.GetLang("DeletingData")', showMask: true }
        });
    }
};



//Grid/////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////
var loadDataGrid = function (store) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            if (App.cboInvtID.getValue() != "") {
                HQ.store.insertBlank(store, keys);
            }
        }
        HQ.isFirstLoad = false;
    }
    frmChange();
};


var deleteRecordGrid = function (item) {
    if (item == "yes") {
        App.grd.deleteSelected();
        frmChange();
    }
};


var grd_BeforeEdit = function (editor, e) {

    return HQ.grid.checkBeforeEdit(e, keys);
};
var grd_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grd, e, keys);
};
var grd_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grd, e, keys);
};
var grd_Reject = function (record) {
    HQ.grid.checkReject(record, App.grd);
    stoChangedTop(App.storeGrid);
};












//Phan trang tren grid
var onComboBoxSelect = function (combo) {
    var store = combo.up("gridpanel").getStore();
    store.pageSize = parseInt(combo.getValue(), 10);
    store.reload();

};

////////////Kiem tra combo chinh cboInvtID
//khi co su thay doi du lieu cua cac conttol tren form
//load lần đầu khi mở
var firstLoad = function () {
    ReloadTreeIN20500();
    App.cboInvtID.getStore().reload();
    App.storeFormBig.reload();
};
var frmChange = function () {
    //đề phòng trường hợp nếu store chưa có gì cả giao diện chưa load xong mà App.frmMain.getForm().updateRecord(); được gọi sẽ gây lỗi
    if (App.storeFormBig.data.length > 0) {
        App.frmMain.getForm().updateRecord();

        HQ.isChange = HQ.store.isChange(App.storeFormBig);
        HQ.common.changeData(HQ.isChange, 'IN20500');//co thay doi du lieu gan * tren tab title header
        //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
        if (App.cboInvtID.valueModels == null || HQ.isNew == true)//App.cboInvtID.valueModels == null khi ko co select item nao
            App.cboInvtID.setReadOnly(false);
        else App.cboInvtID.setReadOnly(HQ.isChange);
    }

};

var storeBeforeLoad = function (store) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'), App.frmMain);
};



var loadDataAutoHeaderBig = function (store) {

    setFocusAllCombo();
    HQ.isFirstLoad = true;
    HQ.common.showBusy(false);
    HQ.isNew = false;
    App.cboInvtID.forceSelection = true;
    if (store.data.length == 0 && HQ.focus != 'ClassID') {
        HQ.store.insertBlank(store, "InvtID");
        record = store.getAt(0);
        //App.storeFormBig.insert(0, record);
        record.data.ApproveStatus = 'H';
        //App.cboCountry.setValue("VN");
        //setTimeout(function () { waitStateLoad(); }, 1500);
        //App.frmMain.getForm().loadRecord(App.storeFormBig.getAt(0));

        store.commitChanges();//commit cho record thanh updated muc dich de dung ham HQ.store.isChange
        HQ.isNew = true;//record la new
        App.cboInvtID.forceSelection = false;
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboInvtID.focus(true);//focus ma khi tao moi
    }
    var record = store.getAt(0);

    App.frmMain.getForm().loadRecord(record);

    frmChange();
};

var loadDataAutoHeaderSmall = function () {

    //if (App.storeFormSmall.getCount() == 0) {
    //    App.storeFormSmall.insert(0, Ext.data.Record());
    //}
    var record = App.storeFormSmall.getAt(0);
    if (record) {
        App.frmMain.getForm().loadRecord(record);

        frmChange();
        App.cboCpnyID.getStore().reload();
    }
};


var setValueApproveStatus = function () {
    App.IDTree.store.tree.root.collapse();
}
//var waitCollapse3 = function () {
//    App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.parentNode.parentNode.expand();
//    App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.parentNode.expand();
//    App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.expand();
//}
//var waitCollapse2 = function () {
//    App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.parentNode.expand();
//    App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.expand();
//}
//var waitCollapse1 = function () {
//    App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.expand();
//}
var choosefromcboInvtID = function () {
    //tu dong bat node tree khi chon 1 cai tu cboInvtID
    var invtIDDescr = App.cboInvtID.value + "-" + App.txtDescr.value;
    //if (App.IDTree.getStore().getNodeById(invtIDDescr)) {
    var record = App.IDTree.getStore().getNodeById(invtIDDescr);
    //}
    if (App.IDTree.getStore().getNodeById(invtIDDescr)) {
        var depth = App.IDTree.getStore().getNodeById(invtIDDescr).data.depth;

        if (depth == 3) {
            //setTimeout(function () { waitCollacpse3(); }, 1500);
            //App.IDTree.store.tree.root.collapse();
            App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.parentNode.parentNode.expand();
            App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.parentNode.expand();
            App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.expand();
        } else if (depth == 2) {
            //setTimeout(function () { waitCollacpse2(); }, 1500);
            //App.IDTree.store.tree.root.collapse();
            App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.parentNode.expand();
            App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.expand();
        } else if (depth == 1) {
            //setTimeout(function () { waitCollacpse1(); }, 1500);
            //App.IDTree.store.tree.root.collapse();
            App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.expand();
        }
        var findNode = App.IDTree.items.items[0].store.data.items;
        for (var i = 0; i < findNode.length; i++) {
            if (findNode[i].data.id == invtIDDescr) {
                App.slmTree.select(i);
                tmpSelectedNode = App.slmTree.selected.items[0].data.id;
                break;
            }

        }
    }
    //App.IDTree.getStore().getNodeById(invtIDDescr).select();
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
    if (sender.valueModels != null) {
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
    }
};

var setValueToGrid = function () {
    App.tabAtIN20500.setActiveTab(App.tabCompany);
    if (App.chooseGrid.value == "2" && App.tabAtIN20500.activeTab.id == "tabCompany") {
        for (var i = 0; i <= App.storeGrid.getCount() - 1; i++) {
            App.slmGrid.select(0);
            var cpnyid = App.slmGrid.selected.items[0].data.CpnyID;
            var cpnyname = App.slmGrid.selected.items[0].data.CpnyName;
            App.grd.deleteSelected();
            App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());
            App.slmGrid.select(App.storeGrid.getCount() - 1);
            App.slmGrid.selected.items[0].set('CpnyID', cpnyid);
            App.slmGrid.selected.items[0].set('CpnyName', cpnyname);
        }
    }
    if (tmpCopyForm == "1" && App.tabAtIN20500.activeTab.id == "tabCompany") {
        for (var i = 0; i <= App.storeGrid.getCount() - 1; i++) {
            App.slmGrid.select(0);
            var cpnyid = App.slmGrid.selected.items[0].data.CpnyID;
            var cpnyname = App.slmGrid.selected.items[0].data.CpnyName;
            App.grd.deleteSelected();
            App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());
            App.slmGrid.select(App.storeGrid.getCount() - 1);
            App.slmGrid.selected.items[0].set('CpnyID', cpnyid);
            App.slmGrid.selected.items[0].set('CpnyName', cpnyname);
        }
    }
    App.tabAtIN20500.setActiveTab(App.tabInfo)
}

var cboClassID_Change = function (sender, e) {
    if (HQ.focus == 'ClassID') {
        App.chooseGrid.setValue("2");
        App.storeFormSmall.load();
        App.storeGrid.load();
        setTimeout(function () { setValueToGrid(); }, 1500);
    }






};

var txtDescr_Change = function (sender, e) {
    //var combobox = App.cboInvtID;
    //var v = combobox.getValue();
    //var d = App.cboInvtID.value + "-" + App.txtDescr.value;
    //var record = combobox.findRecord(d, v);
    //var index = combobox.store.indexOf(record);
    //App.Tree.select(index);
}

var chkIsAttachFile_Change = function (sender, e) {
    if (e) {
        App.chkIsDeleteFile.setValue(true);
        App.chkIsDeleteFile.disable();
    }
    else {
        App.chkIsDeleteFile.enable();
    }


}

var tabSA_Setup_AfterRender = function (obj, padding) {
    if (this.parentAutoLoadControl != null) {
        obj.setHeight(this.parentAutoLoadControl.getHeight() - padding);
    }
    else {
        obj.setHeight(Ext.getBody().getViewSize().height - padding);
    }
};
var frmloadAfterRender = function (obj) {
    App.storePOPriceHeader.load();
    App.storePOPrice.load();
    App.storePOPriceCpny.load();
    lockcontrol();
};




var cboStkUnit_Change = function (sender, e) {

    App.cboDfltPOUnit.getStore().reload();
    App.cboDfltSOUnit.getStore().reload();
    //if (_focusrecord != 3) {
    //    App.storeFormBig.reload();
    //}
}

var PrefixValue_Change = function (sender, e) {
    if (App.txtLotSerFxdVal.value.length != App.txtLotSerFxdLen.value) {
        HQ.message.show(22, e.value, '');
    } else {
        prefixvalue = App.txtLotSerFxdVal.getValue();
        shownextlotserial = prefixvalue + lastfixvalue;
        App.lblShowNextLotSerial.setText(shownextlotserial);
        App.lblShowNextLotSerial.show();
    }

}

var LastFixValue_Change = function (sender, e) {
    if (App.txtLotSerNumVal.value.length != App.txtLotSerNumLen.value) {
        HQ.message.show(22, e.value, '');
    } else {
        lastfixvalue = App.txtLotSerNumVal.getValue();
        shownextlotserial = prefixvalue + lastfixvalue;
        App.lblShowNextLotSerial.setText(shownextlotserial);
        App.lblShowNextLotSerial.show();

    }
}

var cboLotSerTrack_Change = function () {
    if (App.cboLotSerTrack.value == null) {
        App.tabLotSerial.tab.disable(true);
        App.tabLotSerial.disable(true);
        App.storeGrid.reload();
    }
    else if (App.cboLotSerTrack.displayTplData[0].Code == "L" || App.cboLotSerTrack.displayTplData[0].Code == "S") {
        App.tabLotSerial.tab.enable(true);
        App.tabLotSerial.enable(true);
        prefixvalue = App.txtLotSerFxdVal.getValue();
        lastfixvalue = App.txtLotSerNumVal.getValue();
        shownextlotserial = prefixvalue + lastfixvalue;
        App.lblShowNextLotSerial.setText(shownextlotserial);
        App.lblShowNextLotSerial.show();
    } else if (App.cboLotSerTrack.displayTplData[0].Code == "N") {
        App.tabLotSerial.tab.disable(true);
        App.tabLotSerial.disable(true);
        App.storeGrid.reload();

    }

}

var chkPublic_Change = function (item) {

    if (App.chkPublic.getValue() == true) {
        App.tabCompany.tab.disable(true);
        App.tabCompany.disable(true);
        if (App.tabAtIN20500.activeTab.id == "tabCompany") {
            App.tabAtIN20500.setActiveTab(App.tabInfo);
        }
    } else {

        App.tabCompany.tab.enable(true);
        App.tabCompany.enable(true);

    }


}

var cboCpnyID_Change = function (value) {
    var k = value.displayTplData[0].CpnyName;
    App.slmGrid.selected.items[0].set('CpnyName', k);


}

var NextShowNextLotSerial_AfterRender = function (value) {
    prefixvalue = App.txtLotSerFxdVal.getValue();
    lastfixvalue = App.txtLotSerNumVal.getValue();
    shownextlotserial = prefixvalue + lastfixvalue;
    App.lblShowNextLotSerial.setText(shownextlotserial);
    App.lblShowNextLotSerial.show();

}

var NodeSelected_Change = function (store, operation, options) {
    //if (click == 0) {
    parentRecordIDAll = options.textContent.split("-");
    parentRecordID = parentRecordIDAll[0];
    if (operation.childNodes.length == 0) {
        var invtall = options.textContent.split("-")
        var invt1 = invtall[0];
        App.cboInvtID.setValue(invt1);
    }

    // }
    //click = 0;

}


var cboApproveStatus_Change = function () {
    //App.cboApproveStatus.setValue("H");

    App.cboHandle.getStore().load(function (records, operation, success) {
        if (records.length) {
            if (App.cboApproveStatus.value == "H") {
                //setReadOnly();
                App.cboHandle.setValue("N");
            } else {
                //setReadOnly();
                App.cboHandle.setValue("N");
            }
        }
    });

}


//var cboStatus_Change = function () {
//    App.cboHandle.getStore().reload();
//    
//}


var Tab_Change = function (sender, e) {

}


function UploadImage() {
    App.frmMain.submit({
        waitMsg: 'Uploading your file...',
        url: 'IN20500/Upload',

        success: function (result) {
            //
            var curRecord = App.frmMain.getRecord();
            curRecord.data.Picture = App.imgPPCStorePicReq.imageUrl;
            //App.txtImageChange.setValue("1");
            curRecord.setDirty();
        },
        failure: function (error) {
            //
        }
    });
};


var upload = function () {

}

// Event when uplPPCStorePicReq is change a file
var NamePPCStorePicReq_Change = function (sender, e) {
    tmpImageDelete = 0;
    var fileName = sender.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (ext == "jpg" || ext == "png" || ext == "gif") {
        UploadImage();
        //setTimeout(function () { upload(); }, 5000);
        //var curRecord = App.frmMain.getRecord();
        //curRecord.data.Picture = App.imgPPCStorePicReq.imageUrl;
        //curRecord.setDirty();
    } else {
        alert("Please choose a picture! (.jpg, .png, .gif)");
        sender.reset();
    }
};

// Click to clear image of sales person
var btnClearImage_Click = function (sender, e) {
    if (tmpCopyForm == "1") {

    } else {
        App.imgPPCStorePicReq.setImageUrl("");
        var curRecord = App.frmMain.getRecord();
        curRecord.data.Picture = "";
        curRecord.setDirty();
        tmpImageDelete = 1;
    }
};

function UploadMedia() {
    App.frmMain.submit({
        clientValidation: false,
        waitMsg: 'Uploading your file...',
        url: 'IN20500/IN20500UploadMedia',
        timeout: 1800000,
        params: {

            invtID: App.cboInvtID.getValue(),
            //Descr: App.txtDescr.getValue()

        },
        success: function (result, data) {
            //

            //var curRecord = App.frmMain.getRecord();
            //curRecord.data.Picture = App.imgPPCStorePicReq.imageUrl;
            //curRecord.setDirty();

            //App.imgPPCStoreMediaReq.setimageUrl(data.result.invtID);
            alert("da upload xong");
            App.imgPPCStoreMediaReq.setImageUrl(data.result.imageStream);
            var curRecord = App.frmMain.getRecord();
            curRecord.data.Media = data.result.fullFileName;
            curRecord.setDirty();
            //App.Window.show();
        },
        failure: function (error, data) {
            alert("Please Choose InvtID");
        }
    });
};

var NamePPCStoreMediaReq_Change = function (sender, e) {
    //App.Window.show();
    //runProgress4(App.Progress4, App.ButtonProgress);
    tmpMediaDelete = 0;
    var fileName = sender.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (ext == "mp4" || ext == "wmv") {
        UploadMedia();
        //setTimeout(function () { upload(); }, 5000);
        //var curRecord = App.frmMain.getRecord();
        //curRecord.data.Picture = App.imgPPCStorePicReq.imageUrl;
        //curRecord.setDirty();
    } else {
        alert("Please choose a Media! (.mp4, .wmv)");
        sender.reset();
    }
};

var btnDeleteMedia_Click = function (sender, e) {
    if (tmpCopyForm == "1") {

    } else {
        App.imgPPCStoreMediaReq.setImageUrl("");
        var curRecord = App.frmMain.getRecord();
        curRecord.data.Media = "";
        curRecord.setDirty();
        tmpMediaDelete = 1;
    }
};

function setMediaImage() {
    App.frmMain.submit({

        url: 'IN20500/IN20500SetMediaImage',

        success: function (result, data) {
            //
            App.imgPPCStoreMediaReq.setImageUrl(data.result.imageStream);
        },
        failure: function (error) {
            //
        }
    });
};

var PlayVideo = function (sender, e) {
    var curRecord = App.frmMain.getRecord();
    if (curRecord) {
        if (curRecord.data.Media) {
            App.direct.PlayMedia(curRecord.data.Media);
        }
    }
};
//ham timeout cua CopyFormClick
var waitCopyFormLoad = function () {
    tmpCopyForm = "1";
    App.cboInvtID.setValue("");

}
var CopyForm_Click = function (sender, e) {
    if (App.txtBarCode.value != "") {
        //cboInvtID_Change
        tmpOldFileName = App.txtBarCode.value + ".mp4";
        var barcode = App.txtBarCode.value;
        App.cboInvtID.setValue(barcode);
        setTimeout(function () { waitCopyFormLoad(); }, 4500);


    }
};

var LoadTree_AfterRender = function () {
    try {
        App.direct.LoadTree({
            success: function (data) {
                //if (tampSaveButton == "1") {
                //    tampSaveButton = "0";
                //} else {
                //App.cboCustId.getStore().reload();

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
}

var txtLoadTree_Change = function (sender, newValue, oldValue) {
    //cpnyIDTree = App.cboCpnyID.value;

    ReloadTreeIN20500();
    //setTimeout(function () { ReloadTree(); }, 1000);

}

function ReloadTreeIN20500() {
    try {
        App.direct.ReloadTreeIN20500({
            success: function (data) {
                //if (tampSaveButton == "1") {
                //    tampSaveButton = "0";
                //} else {
                //App.cboCustId.getStore().reload();

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

var setFocusAllCombo = function () {

    App.cboApproveStatus.forceSelection = false;
    App.cboHandle.forceSelection = false;
    App.cboStatus.forceSelection = false;
    App.cboClassID.forceSelection = false;
    App.cboPriceClassID.forceSelection = false;
    App.cboInvtType.forceSelection = false;
    App.cboSource.forceSelection = false;
    App.cboValMthd.forceSelection = false;
    App.cboLotSerTrack.forceSelection = false;
    App.cboBuyer.forceSelection = false;
    App.cboStkUnit.forceSelection = false;
    App.cboDfltPOUnit.forceSelection = false;
    App.cboDfltSOUnit.forceSelection = false;
    App.cboMaterialType.forceSelection = false;
    App.cboDfltSlsTaxCat.forceSelection = false;
    App.cboStyle.forceSelection = false;
    App.cboVendor1.forceSelection = false;
    App.cboVendor2.forceSelection = false;
    App.cboStkWtUnit.forceSelection = false;
    App.cboSerAssign.forceSelection = false;
    App.cboLotSerIssMthd.forceSelection = false;
    App.cboLotSerFxdTyp.forceSelection = false;


};

