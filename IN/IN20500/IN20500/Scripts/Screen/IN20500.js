
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
var _nodeID ;
var _nodeLevel;
var _parentRecordID;
//bien tam Grid
var keys = ['CpnyID'];
var fieldsCheckRequire = ["CpnyID"];
var fieldsLangCheckRequire = ["CpnyID"];
var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == 'header') {
                HQ.combo.first(App.cboInvtID, HQ.isChange);
            } else if (HQ.focus == 'grd') {
                HQ.grid.first(App.grd);
            }

            break;
        case "prev":
           
            if (HQ.focus == 'header') {
                HQ.combo.prev(App.cboInvtID, HQ.isChange);
            } else if (HQ.focus == 'grd') {
                HQ.grid.prev(App.grd);
            }

            break;
        case "next":
            
            if (HQ.focus == 'header') {
                HQ.combo.next(App.cboInvtID, HQ.isChange);
            } else if (HQ.focus == 'grd') {
                HQ.grid.next(App.grd);
            }
            break;
        case "last":            
            if (HQ.focus == 'header') {
                HQ.combo.last(App.cboInvtID, HQ.isChange);
            } else if (HQ.focus == 'grd') {
                HQ.grid.last(App.grd);
            }
            break;
        case "refresh":
          
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            } else {
                HQ.isChange = false;             
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
                        App.stoIN_Inventory.reload();
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
                    } 
                }
                else if (HQ.focus == 'grd') {
                    var rowindex = HQ.grid.indexSelect(App.grd);
                    if (rowindex != '')
                        HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grd), ''], 'deleteRecordGrid', true)
                }
            }
            break;
        case "save":
            if (!App.slmTree.selected.items.length > 0) {
                HQ.message.show(213, '', '', '');
                return;
            }
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain) &&
                    HQ.store.checkRequirePass(App.stoCompany, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
        App.stoIN_Inventory.reload();
        App.stoCompany.reload();
        
    }
};
//var refreshTree = function (i) {
//    App.Tree.select(i);
//}
//var refreshTree2 = function (dt) {
//    App.cboInvtID.setValue(dt);
//    var findNode = App.IDTree.items.items[0].store.data.items;
//    for (var i = 0; i < findNode.length; i++) {
//        if (findNode[i].data.id == node) {
//            App.Tree.select(i);
//            break;

//        }

//    }
//}
function Save() {  
    var curRecord = App.frmMain.getRecord();
    curRecord.data.Picture = App.imgPPCStorePicReq.imageUrl;
    App.frmMain.getForm().updateRecord();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang('SavingData'),         
            url: 'IN20500/Save',
            timeout: 1800000,

            params: {
                lstheader: Ext.encode(App.stoIN_Inventory.getRecordsValues()),//,        
                lstgrd: Ext.encode(App.stoCompany.getChangedData({ skipIdForPhantomRecords: false })),
                InvtID: App.cboInvtID.getValue(),
                Handle: App.cboHandle.getValue(),
                isNew:HQ.isNew,
                NodeID: _nodeID,
                NodeLevel: _nodeLevel,
                ParentRecordID: _parentRecordID,
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
                var InvtID = App.cboInvtID.getValue();
                App.cboInvtID.getStore().reload();
                App.cboInvtID.getStore().load(function () {
                    App.cboInvtID.setValue(InvtID);
                    App.stoIN_Inventory.reload();
                });

                HQ.message.show(201405071, '', null);              
                         
                //if (data.result.addNewOrUpdate == "addNew") {
                //    if (data.result.tmpChangeTreeDic == "0") {
                //        var newNode = App.slmTree.selected.items[0].data.id;
                //        var record = App.IDTree.getStore().getNodeById(newNode);
                //        var node = data.result.invtID + "-" + data.result.Descr;

                //        record.appendChild({ text: node, leaf: true, id: node });

                //        var findNode = App.IDTree.items.items[0].store.data.items;
                //        for (var i = 0; i < findNode.length; i++) {
                //            if (findNode[i].data.id == node) {                           
                //                App.slmTree.select(i);                              
                //                break;
                //            }
                //        }
                //    } else if (data.result.tmpChangeTreeDic == "1") {
                //        var tmpSelectedNode = data.result.tmpSelectedNode;
                //        var recordtmpSelectedNode = App.IDTree.getStore().getNodeById(tmpSelectedNode);
                //        recordtmpSelectedNode.remove(true);

                //        var newNode = App.slmTree.selected.items[0].data.id;
                //        var record = App.IDTree.getStore().getNodeById(newNode);

                //        var node = data.result.invtID + "-" + data.result.Descr;
                //        //var findNode = App.IDTree.items.items[0].store.data.items;
                //        //for (var i = 0; i < findNode.length; i++) {
                //        //    if (findNode[i].data.id == node) {
                //        //        //click = 1;
                //        //        App.slmTree.select(i);
                //        //        //setTimeout(function () { refreshTree(i); }, 7000);
                //        //        break;
                //        //    }
                //        //}
                //        //var newNode2 = App.slmTree.selected.items[0].data.id;
                //        //var recordDeleteNode = App.IDTree.getStore().getNodeById(newNode2);
                //        //recordDeleteNode.remove(true);
                //        record.appendChild({ text: node, leaf: true, id: node });
                //        App.cboInvtID.setValue("");
                //        //for (var i = 0; i < findNode.length; i++) {
                //        //    if (findNode[i].data.id == node) {
                //        //        //click = 1;
                //        //        App.slmTree.select(i);
                //        //        //setTimeout(function () { refreshTree(i); }, 7000);

                //        //        break;
                //        //    }
                //        //}



                //    }
                //}
                //if (data.result.addNewOrUpdate == "update") {
                //    var newNode = App.slmTree.selected.items[0].data.id;
                //    var record = App.IDTree.getStore().getNodeById(newNode);
                //    var node = data.result.invtID + "-" + data.result.Descr;
                //    if (newNode != node) {
                //        record.parentNode.appendChild({ text: node, leaf: true, id: node });
                //        var findNode = App.IDTree.items.items[0].store.data.items;
                //        for (var i = 0; i < findNode.length; i++) {
                //            if (findNode[i].data.id == node) {
                //                //click = 1;
                //                App.slmTree.select(i);
                //                //setTimeout(function () { refreshTree(i); }, 7000);

                //            }
                //        }
                //        var newNode2 = App.slmTree.selected.items[0].data.id;
                //        var record2 = App.IDTree.getStore().getNodeById(newNode2);
                //        record2.remove(true);
                //        record.parentNode.replaceChild(record2, record);
                //        var findNode = App.IDTree.items.items[0].store.data.items;
                //        for (var i = 0; i < findNode.length; i++) {
                //            if (findNode[i].data.id == node) {
                //                //click = 1;
                //                App.slmTree.select(i);
                //                //setTimeout(function () { refreshTree(i); }, 7000);
                //                break;
                //            }
                //        }
                //    }


                //}
                ReloadTreeIN20500();

            }
            , failure: function (errorMsg, data) {
                HQ.message.process(errorMsg, data, true);
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

// Xac nhan xoa record tren grid
var deleteRecordForm = function (item) {
    if (item == "yes") {
        App.frmMain.submit({
            clientValidation: false,
            timeout: 1800000,
            waitMsg: HQ.common.getLang('DeletingData'),
            url: 'IN20500/IN20500Delete',
            params: {
                invtID: App.cboInvtID.getValue(),
                cpnyID: App.cboCpnyID.getValue(),
                status: App.cboStatus.getValue()
            },
            success: function (msg, data) {
                App.cboInvtID.getStore().reload();
                App.cboInvtID.setValue('');                             
                App.stoIN_Inventory.reload();                        
            },
            failure: function (msg, data) {               
                    HQ.message.process(msg, data, true);                
            }
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
    stoChangedTop(App.stoCompany);
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
    App.stoIN_Inventory.reload();
};
var frmChange = function () {
    //đề phòng trường hợp nếu store chưa có gì cả giao diện chưa load xong mà App.frmMain.getForm().updateRecord(); được gọi sẽ gây lỗi
    if (App.stoIN_Inventory.data.length > 0) {
        App.frmMain.getForm().updateRecord();

        HQ.isChange = HQ.store.isChange(App.stoIN_Inventory);
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
var stoIN_Inventory_Loaded = function (store) {

    setFocusAllCombo();
    HQ.isFirstLoad = true;
    HQ.common.showBusy(false);
    HQ.isNew = false;
    App.cboInvtID.forceSelection = true;
    if (store.data.length == 0 && HQ.focus != 'ClassID') {
        HQ.store.insertBlank(store, "InvtID");
        record = store.getAt(0);     
        record.data.ApproveStatus = 'H';    
        store.commitChanges();//commit cho record thanh updated muc dich de dung ham HQ.store.isChange
        HQ.isNew = true;//record la new
        App.cboInvtID.forceSelection = false;
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboInvtID.focus(true);//focus ma khi tao moi
    }
    var record = store.getAt(0);
    App.frmMain.getForm().loadRecord(record);
    frmChange();
    App.stoCompany.reload();
    if (!App.cboInvtID.getValue()) {
        App.cboInvtID.forceSelection = false;
        HQ.isNew = true;
    }
    if (record.data.ApproveStatus != 'H') {
        HQ.common.lockItem(App.frmMain, true);
    }
    else HQ.common.lockItem(App.frmMain, false);
    // display image
    App.fupPPCStorePicReq.reset();
    if (record.data.Picture) {
        displayImage(App.imgPPCStorePicReq, record.data.Picture);
    }
    else {
        App.imgPPCStorePicReq.setImageUrl("");
    }
    // display media
    App.fupPPCStoreMediaReq.reset();
    if (record.data.Media) {
        displayImage(App.imgPPCStoreMediaReq, record.data.Media);
    }
    else {
        App.imgPPCStoreMediaReq.setImageUrl("");
    }

};
var stoIN_ProductClass_Loaded = function () {

    if (App.stoIN_ProductClass.getCount() == 0) {
        App.stoIN_ProductClass.insert(0, Ext.data.Record());
    }
    var record = App.stoIN_ProductClass.getAt(0);
    if (record) {
        App.frmMain.getForm().loadRecord(record);
       
        frmChange();
        App.cboCpnyID.getStore().reload();
    }
};
var setValueApproveStatus = function () {
    App.IDTree.store.tree.root.collapse();
}
var searchNode = function () {
    HQ.common.showBusy(true, HQ.common.getLang('searching node....'), App.frmMain);
    //tu dong bat node tree khi chon 1 cai tu cboInvtID
    var invtIDDescr ="InvtID-"+ App.cboInvtID.value;
    var record = App.IDTree.getStore().getNodeById(invtIDDescr);
    if (App.IDTree.getStore().getNodeById(invtIDDescr)) {
        var depth = App.IDTree.getStore().getNodeById(invtIDDescr).data.depth;

        if (depth == 3) {
            App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.parentNode.parentNode.expand();
            App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.parentNode.expand();
            App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.expand();
        } else if (depth == 2) {      
            App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.parentNode.expand();
            App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.expand();
        } else if (depth == 1) {          
            App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.expand();
        }
        var findNode = App.IDTree.items.items[0].store.data.items;
        for (var i = 0; i < findNode.length; i++) {
            if (findNode[i].data.id == invtIDDescr) {
                App.slmTree.select(i);
                tmpSelectedNode = App.slmTree.selected.items[0].data.id;
                
                if (App.slmTree.selected.items[0].parentNode.data.id.split('-').length == 3) {
                    _nodeID = App.slmTree.selected.items[0].parentNode.data.id.split('-')[0];
                    _nodeLevel = App.slmTree.selected.items[0].parentNode.data.id.split('-')[1];
                    _parentRecordID = App.slmTree.selected.items[0].parentNode.data.id.split('-')[2];
                }

                break;
            }

        }
    }
    HQ.common.showBusy(false);
    //App.IDTree.getStore().getNodeById(invtIDDescr).select();
}


var cboInvtID_Change = function (sender, e) {
    if ((!HQ.isNew || sender.valueModels != null) && !App.stoIN_Inventory.loading) {  
        App.stoIN_Inventory.reload();
        searchNode();
    } 
};
var cboInvtID_Select = function (sender, value) {
    if (sender.valueModels != null && !App.stoIN_Inventory.loading) {     
        App.stoIN_Inventory.reload();
        searchNode();
    }

};
var setValueToGrid = function () {
    App.tabAtIN20500.setActiveTab(App.tabCompany);
    if (App.chooseGrid.value == "2" && App.tabAtIN20500.activeTab.id == "tabCompany") {
        for (var i = 0; i <= App.stoCompany.getCount() - 1; i++) {
            App.slmGrid.select(0);
            var cpnyid = App.slmGrid.selected.items[0].data.CpnyID;
            var cpnyname = App.slmGrid.selected.items[0].data.CpnyName;
            App.grd.deleteSelected();
            App.stoCompany.insert(App.stoCompany.getCount(), Ext.data.Record());
            App.slmGrid.select(App.stoCompany.getCount() - 1);
            App.slmGrid.selected.items[0].set('CpnyID', cpnyid);
            App.slmGrid.selected.items[0].set('CpnyName', cpnyname);
        }
    }
    if (tmpCopyForm == "1" && App.tabAtIN20500.activeTab.id == "tabCompany") {
        for (var i = 0; i <= App.stoCompany.getCount() - 1; i++) {
            App.slmGrid.select(0);
            var cpnyid = App.slmGrid.selected.items[0].data.CpnyID;
            var cpnyname = App.slmGrid.selected.items[0].data.CpnyName;
            App.grd.deleteSelected();
            App.stoCompany.insert(App.stoCompany.getCount(), Ext.data.Record());
            App.slmGrid.select(App.stoCompany.getCount() - 1);
            App.slmGrid.selected.items[0].set('CpnyID', cpnyid);
            App.slmGrid.selected.items[0].set('CpnyName', cpnyname);
        }
    }
    App.tabAtIN20500.setActiveTab(App.tabInfo)
}
var cboClassID_Change = function (sender, e) {
    if (HQ.focus == 'ClassID') {
        App.chooseGrid.setValue("2");
        App.stoIN_ProductClass.load();
        App.stoCompany.load();
        setTimeout(function () { setValueToGrid(); }, 1500);
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
    //    App.stoIN_Inventory.reload();
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
    if(App.cboLotSerTrack.value == null)
    {
        App.tabLotSerial.tab.disable(true);
        App.tabLotSerial.disable(true);
        App.stoCompany.reload();
    }
    else if (App.cboLotSerTrack.displayTplData[0].Code == "L" || App.cboLotSerTrack.displayTplData[0].Code == "S") {
        App.tabLotSerial.tab.enable(true);
        App.tabLotSerial.enable(true);
        prefixvalue = App.txtLotSerFxdVal.getValue();
        lastfixvalue = App.txtLotSerNumVal.getValue();
        shownextlotserial = prefixvalue + lastfixvalue;
        App.lblShowNextLotSerial.setText(shownextlotserial);
        App.lblShowNextLotSerial.show();
    } else if(App.cboLotSerTrack.displayTplData[0].Code == "N"){
        App.tabLotSerial.tab.disable(true);
        App.tabLotSerial.disable(true);
        App.stoCompany.reload();

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
    parentRecordIDAll = operation.data.id.split("-");   
    if (operation.childNodes.length == 0) {
        var invtall = operation.data.id.split("-")
        var invt1 = invtall[1];
        App.cboInvtID.setValue(invt1);
        parentRecordIDAll = operation.parentNode.data.id.split("-");
        _nodeID = parentRecordIDAll[0];
        _nodeLevel = parentRecordIDAll[1];
        _parentRecordID = parentRecordIDAll[2];

    }
    else if (parentRecordIDAll.length == 3) {
        _nodeID = parentRecordIDAll[0];
        _nodeLevel = parentRecordIDAll[1];
        _parentRecordID = parentRecordIDAll[2];
    }
     

}


var cboApproveStatus_Change = function () {   
    App.cboHandle.getStore().load(function () {
        App.cboHandle.setValue("N");
    });

}

var Tab_Change = function (sender, e)
{

}




// Event when uplPPCStorePicReq is change a file
var fupPPCStorePicReq_Change = function (fup, newValue, oldValue, eOpts) { 
        if (fup.value) {
            var ext = fup.value.split(".").pop().toLowerCase();
            if (ext == "jpg" || ext == "png" || ext == "gif") {
                HQ.common.showBusy(true, HQ.common.getLang('uploading'), App.frmMain);
                App.hdnPPCStorePicReq.setValue(fup.value);               
                readImage(fup, App.imgPPCStorePicReq);
            }
            else {
                HQ.message.show(148, '', '');
            }
        }    
};

// Event when uplPPCStorePicReq is change a file
var fupPPCStoreMediaReq_Change = function (fup, newValue, oldValue, eOpts) {
    if (fup.value) {
        var ext = fup.value.split(".").pop().toLowerCase();
        if (ext == "mp4") {
            HQ.common.showBusy(true, HQ.common.getLang('uploading'), App.frmMain);
            App.hdnPPCStoreMediaReq.setValue(fup.value);
            readImage(fup, App.imgPPCStoreMediaReq);
        }
        else {
            HQ.message.show(148, '', '');
        }
    }
};

// Click to clear image of sales person
var btnClearImage_Click = function (sender, e) {
    App.fupPPCStorePicReq.reset();
    App.imgPPCStorePicReq.setImageUrl("");
    App.hdnPPCStorePicReq.setValue("");
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

var btnDeleteMedia_Click = function (sender, e) {
    App.fupPPCStoreMediaReq.reset();
    App.imgPPCStoreMediaReq.setImageUrl("");
    App.hdnPPCStoreMediaReq.setValue("");
};


var PlayVideo = function (sender, e) {
    var curRecord = App.frmMain.getRecord();
    if (curRecord) {
        if (curRecord.data.Media) {
            App.direct.PlayMedia(curRecord.data.Media);
        }
    }
};

var CopyForm_Click = function (sender, e) {
    if (App.txtBarCode.value != "") {
        //cboInvtID_Change
        tmpOldFileName = App.txtBarCode.value + ".mp4";
        var barcode = App.txtBarCode.value;
        App.cboInvtID.setValue(barcode);        
        App.cboInvtID.setValue('');
        HQ.isNew = true;
    }
};

function ReloadTreeIN20500() {
    try {
        App.direct.ReloadTreeIN20500({
            success: function (data) {
                searchNode();
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

//other
var displayImage= function (imgControl, fileName) {
    Ext.Ajax.request({
        url: 'IN20500/ImageToBin',
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        params: JSON.stringify({
            fileName: fileName
        }),
        success: function (result) {
            var jsonData = Ext.decode(result.responseText);
            if (jsonData.imgSrc) {
                imgControl.setImageUrl(jsonData.imgSrc);
            }
            else {
                imgControl.setImageUrl("");
            }
        },
        failure: function (errorMsg, data) {
            HQ.message.process(errorMsg, data, true);
        }
    });
};
var readImage= function (fup, imgControl) {
    var files = fup.fileInputEl.dom.files;
    if (files && files[0]) {
        var FR = new FileReader();
        FR.onload = function (e) {
            imgControl.setImageUrl(e.target.result);
            HQ.common.showBusy(fasle, HQ.common.getLang(''), App.frmMain);
        };
        FR.readAsDataURL(files[0]);
    }
}