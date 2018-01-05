//Declare
var keys = ['InvtID'];
var fieldsCheckRequirePO_Price = ["InvtID", "UOM"];
var fieldsLangCheckRequirePO_Price = ["InvtID", "UOM"];

var keys1 = ['CpnyID'];
var fieldsCheckRequirePO_PriceCpny = ["CpnyID"];
var fieldsLangCheckRequirePO_PriceCpny = ["CpnyID"];

var _focusNo = 0;

//////////////////////////////////////////////////////////////////////

var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboPriceID.getStore().load(function () {
        App.stoPOPriceHeader.reload();
        //HQ.common.showBusy(false, HQ.common.getLang("loadingData"));
    });
};
var loadComboGrid = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboInvtID.getStore().load(function () {
        App.cboCpnyID.getStore().load(function () {
            App.stoPO_Price.reload();
            App.stoPO_PriceCpny.reload();
            HQ.common.showBusy(false, HQ.common.getLang("loadingData"));
        })
    });
};

var pnl_render = function (cmd) {
    cmd.getEl().on('mousedown', function () {
        if (cmd.id == 'pnlPO_Price') {
            _focusNo = 1;
        }
        else if (cmd.id == 'pnlPO_PriceCpny') {
            _focusNo = 2;
        }
        else {//pnlHeader
            _focusNo = 0;
        }
    });
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (_focusNo == 0) {
                HQ.combo.first(App.cboPriceID, HQ.isChange);
            }
            else if (_focusNo == 1) {
                HQ.grid.first(App.grdPO_Price);
            }
            if (_focusNo == 2) {
                HQ.grid.first(App.grdPO_PriceCpny);
            }
            //if (_focusNo == 0) {
            //    HQ.grid.first(App.grdPO_PriceCpny);
            //}
            break;
        case "prev":
            if (_focusNo == 0) {
                HQ.combo.prev(App.cboPriceID, HQ.isChange);
            }
            else if (_focusNo == 1) {
                HQ.grid.prev(App.grdPO_Price);
            }
            else if (_focusNo == 2) {
                HQ.grid.prev(App.grdPO_PriceCpny);
            }
            break;
        case "next":
            if (_focusNo == 0) {
                HQ.combo.next(App.cboPriceID, HQ.isChange);
            }
            else if (_focusNo == 1) {
                HQ.grid.next(App.grdPO_Price);
            }
            else if (_focusNo == 2) {
                HQ.grid.next(App.grdPO_PriceCpny);
            }
            break;
        case "last":
            if (_focusNo == 0) {
                HQ.combo.last(App.cboPriceID, HQ.isChange);
            }
            else if (_focusNo == 1) {
                HQ.grid.last(App.grdPO_Price);
            }
            else if (_focusNo == 2) {
                HQ.grid.last(App.grdPO_PriceCpny);
            }
            break;
        case "refresh":
           
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoPOPriceHeader.reload();
                
            }
            break;
        case "new":
            if (_focusNo == 0) {
                if (HQ.isChange) {
                    HQ.message.show(150, '', 'refresh');
                } else {
                    PriceID = '';
                    App.cboPriceID.setValue('');
                    HQ.isFirstLoad = true;
                }
                //App.cboPriceID.setValue("");
                //App.EffDate.setValue(new Date());
                //App.stoPO_Price.reload();
                //App.stoPO_PriceCpny.reload();
            }
            else if (_focusNo == 1) {
                if (HQ.isInsert) {
                    HQ.grid.insert(App.grdPO_Price);
                }
            }
            else if (_focusNo == 2) {
                if (HQ.isInsert) {
                    HQ.grid.insert(App.grdPO_PriceCpny);
                }
            }

            break;
        case "delete":
            if (_focusNo == 0) {
                if (App.cboPriceID.value) {
                    if (HQ.isDelete) {
                        HQ.message.show(11, '', 'deleteData');
                    }
                } else {
                    menuClick('new');
                }
            }
            else if (_focusNo == 1) {
                if (App.slmPO_Price.selected.items[0] != undefined) {
                    if (HQ.isDelete) {
                        if (App.slmPO_Price.selected.items[0] != undefined) {
                            if (App.slmPO_Price.selected.items[0].data.InvtID != "") {
                                HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdPO_Price)], 'deleteData', true);
                            }
                        }
                    }
                }
            }
            else if (_focusNo == 2) {
                //if (App.slmPO_PriceCpny.selected.items[0] != undefined) {
                //    if (HQ.isDelete) {
                //        HQ.message.show(11, '', 'deleteData');
                //    }
                //}
                if (App.slmPO_PriceCpny.selected.items[0] != undefined) {
                    if (HQ.isDelete) {
                        if (App.slmPO_PriceCpny.selected.items[0] != undefined) {
                            if (App.slmPO_PriceCpny.selected.items[0].data.CpnyID != "") {
                                HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdPO_PriceCpny)], 'deleteData', true);
                            }
                        }
                    }
                }
            }


            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain) && HQ.store.checkRequirePass(App.stoPO_Price, keys, fieldsCheckRequirePO_Price, fieldsLangCheckRequirePO_Price)
                        && HQ.store.checkRequirePass(App.stoPO_PriceCpny, keys1, fieldsCheckRequirePO_PriceCpny, fieldsLangCheckRequirePO_PriceCpny)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            HQ.common.close(this);
            break;
    }

};

var cboInvtID_Change = function (item, newValue, oldValue) {
    App.cboUOM.store.reload();
};

function btnFill_Click() {
    App.stoPO_Price.snapshot.each(function (item, index, totalItems) {
        item.set('Disc', App.txtFill.getValue());
    });
};

var chkPublic_Change = function (checkbox, checked) {
    if (checked) {
        App.tabBot.closeTab(App.pnlPO_PriceCpny);
       
    }
    else {
        App.tabBot.addTab(App.pnlPO_PriceCpny);
    }
};

var cboPriceID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null) {
        App.stoPOPriceHeader.reload();
    }
};

//khi nhan combo xo ra, neu da thay doi thi ko xo ra
var cboPriceID_Expand = function (sender, value) {
    if (HQ.isChange) {
        App.cboPriceID.collapse();
    }
};

//khi nhan X xoa tren combo, neu du lieu thay doi thi ko cho xoa, du lieu chua thay doi thi add new
var cboPriceID_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {
        menuClick('new');
    }
};

var firstLoad = function () {
    HQ.isFirstLoad = true;
    HQ.util.checkAccessRight(); // kiểm tra các quyền update,insert,del
    //HQ.isFirstLoad = true;
   // App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    loadSourceCombo();
};

var frmChange = function () {
    if (App.stoPOPriceHeader.getCount() > 0) {
        App.frmMain.getForm().updateRecord();
        HQ.isChange = (HQ.store.isChange(App.stoPOPriceHeader) == false ? HQ.store.isChange(App.stoPO_Price) : true) || (HQ.store.isChange(App.stoPOPriceHeader) == false ? HQ.store.isChange(App.stoPO_PriceCpny) : true);
        HQ.common.changeData(HQ.isChange, 'PO20100');
        if (App.cboPriceID.valueModels == null || HQ.isNew == true)
            App.cboPriceID.setReadOnly(false);
        else App.cboPriceID.setReadOnly(HQ.isChange);
    }
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

var stoLoad = function (sto) {
    HQ.isFirstLoad = true;
    HQ.isNew = false;
    HQ.common.showBusy(false);
    App.cboPriceID.forceSelection = true;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "PriceID");
        record = sto.getAt(0);

        HQ.isNew = true;//record la new    
        App.cboPriceID.forceSelection = false;
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboPriceID.focus(true);//focus ma khi tao moi
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);
    if (!HQ.isNew) {
        if (!HQ.isUpdate) {
            HQ.common.lockItem(App.frmMain, true);
        }
        else{
            //HQ.isFirstLoad = false;
            HQ.common.lockItem(App.frmMain, false);
        }
        }
    
    else {
        if (!HQ.isInsert) {
            HQ.common.lockItem(App.frmMain, true);
        }
        else {
            HQ.common.lockItem(App.frmMain, false);

        }
    }
   loadComboGrid();
};

// =====================Grd PO_Price =======================//
var stoPO_Price_Load = function (sto) {
    //if (HQ.isFirstLoad) {
   
        if (HQ.isInsert) {
            var record = HQ.store.findRecord(sto, keys, ['']);
            if (!record) {
                // HQ.common.lockItem(App.frmMain, true);
                HQ.store.insertBlank(sto, keys);
            }
        }
        //HQ.isFirstLoad = false;
    //}
    frmChange();
    HQ.common.showBusy(false);
};

var grdPO_Price_BeforeEdit = function (editor, e) {

    if (!Ext.isEmpty(App.stoPOPriceHeader.data.items[0].data.PriceID))
    {
        if (!HQ.isUpdate)
        {
            return false;
        } else if (e.field == 'UOM' && e.record.data.tstamp != '') {
            return false;
        }
    }

    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdPO_Price_Edit = function (item, e) {
    //Kiem tra cac key da duoc nhap se insert them dong moi
    HQ.grid.checkInsertKey(App.grdPO_Price, e, keys);
    if (e.field == 'InvtID') {
        if (!Ext.isEmpty(e.value)) {
            e.record.set('QtyBreak', '1')
        }
    }
   

    if (e.field == 'InvtID') {
        var selectedRecord = App.cboInvtID.store.findRecord(e.field, e.value);
        if (selectedRecord) {
            e.record.set("Descr", selectedRecord.data.Descr);
        }
        else {
            e.record.set("Descr", "");
        }
    }
    frmChange();
};

var grdPO_Price_ValidateEdit = function (item, e) {
    //ko cho nhap key co ki tu dac biet, va kiem tra trung du lieu



    return HQ.grid.checkValidateEdit(App.grdPO_Price, e, keys);
};

var grdPO_Price_Reject = function (record) {
    //reject dong thay doi du lieu ve ban dau
    HQ.grid.checkReject(record, App.grdPO_Price);
    frmChange();
};

// =====================Grd PO_PriceCpny =======================//
var stoPO_PriceCpny_Load = function (sto) {
   // if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            var record = HQ.store.findRecord(sto, keys1, ['']);
            if (!record) {
                // HQ.common.lockItem(App.frmMain, true);
                HQ.store.insertBlank(sto, keys1);
            }
        }
      //  HQ.isFirstLoad = false;
   // }
    frmChange();
    HQ.common.showBusy(false);
};

var grdPO_PriceCpny_BeforeEdit = function (editor, e) {
    //Kiem tra cac key da duoc nhap se insert them dong moi
    //HQ.grid.checkInsertKey(App.grdPO_PriceCpny, e, keys1);
    return HQ.grid.checkBeforeEdit(e, keys1);
};

var grdPO_PriceCpny_Edit = function (item, e) {
    //Kiem tra cac key da duoc nhap se insert them dong moi
    HQ.grid.checkInsertKey(App.grdPO_PriceCpny, e, keys1);

    if (e.field == 'CpnyID') {
        var selectedRecord = App.cboCpnyID.store.findRecord("BranchID", e.value);
        if (selectedRecord) {
            e.record.set("CpnyName", selectedRecord.data.BranchName);
        }
        else {
            e.record.set("CpnyName", "");
        }
    }
    frmChange();

};

var grdPO_PriceCpny_ValidateEdit = function (item, e) {
    //ko cho nhap key co ki tu dac biet, va kiem tra trung du lieu
    return HQ.grid.checkValidateEdit(App.grdPO_PriceCpny, e, keys1);
};

var grdPO_PriceCpny_Reject = function (record) {
    //reject dong thay doi du lieu ve ban dau
    HQ.grid.checkReject(record, App.grdPO_PriceCpny);
    frmChange();
};

var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.updateRecord();
        var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/;
        var value = App.cboPriceID.getValue();
        if (!HQ.util.passNull(value.toString()).match(regex)) {
            HQ.message.show(20140811, App.cboPriceID.fieldLabel, '');
            return;
        }
        if (App.Public.getValue()) {
            if (App.stoPO_Price.getCount() == 1) {
                HQ.message.show(1000, App.InvtID.text, '');
                App.tabBot.setActiveTab(App.pnlPO_Price);
                return;
            }
        }
        if (!App.Public.getValue()) {
            if (App.stoPO_PriceCpny.getCount() == 1) {
                HQ.message.show(1000, App.txtCpny.text, '');
                App.tabBot.setActiveTab(App.pnlPO_PriceCpny);
                return;
            }
            if (App.stoPO_Price.getCount() == 1) {
                HQ.message.show(1000, App.InvtID.text, '');
                App.tabBot.setActiveTab(App.pnlPO_Price);
                return;
            }
        }
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'PO20100/Save',
            timeout: 1800000,
            params: {
                lstPOPriceHeader: Ext.encode([App.frmMain.getRecord().data]),
                //lstPOPriceHeader: HQ.store.getData(App.frmMain.getRecord().store),
                lstPO_Price: HQ.store.getData(App.stoPO_Price),
                lstPO_PriceCpny: HQ.store.getData(App.stoPO_PriceCpny)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                App.cboPriceID.getStore().reload();
                 refresh("yes");
                //App.stoPO_Price.reload();
                //App.stoPO_PriceCpny.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        if (_focusNo == 0) {
            if (App.frmMain.isValid()) {
                App.frmMain.updateRecord();
                App.frmMain.submit({
                    waitMsg: HQ.common.getLang("DeletingData"),
                    url: 'PO20100/DeleteAll',
                    timeout: 1800000,
                    success: function (msg, data) {
                        App.cboPriceID.getStore().load();
                        refresh("yes");
                    },
                    failure: function (msg, data) {
                        HQ.message.process(msg, data, true);
                    }
                });
            }

        }
        else if (_focusNo == 1) {
            App.grdPO_Price.deleteSelected();
            frmChange();
        }
        else if (_focusNo == 2) {
            App.grdPO_PriceCpny.deleteSelected();
            frmChange();
        }
    }
};

function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        menuClick("refresh");
    }
};

//var tabPO_Setup_AfterRender = function (obj) {
//    if (this.parentAutoLoadControl != null)
//        obj.setHeight(this.parentAutoLoadControl.getHeight() - 100);
//    else {
//        obj.setHeight(Ext.getBody().getViewSize().height - 100);
//    }
//};

//var cboPriceID_Change = function (item, newValue, oldValue) {
//    App.stoPOPriceHeader.load();
//    App.grdPO_Price.store.reload();
//    App.grdPO_PriceCpny.store.reload();
//};
//var loadData = function () {
//    if (App.stoPOPriceHeader.getCount() == 0) {
//        var record = Ext.create("App.mdlPOPriceHeader", {
//            PriceID: App.cboPriceID.getValue()
//        });
//        App.stoPOPriceHeader.insert(0, record);
//    }
//    App.frmMain.getForm().loadRecord(App.stoPOPriceHeader.getAt(0));
//};
//function btnFill_Click() {
//    App.stoPO_Price.snapshot.each(function (item, index, totalItems) {
//        item.set('Disc', App.txtFill.getValue());
//    });
//};

//var chkPublic_Change = function (checkbox, checked) {
//    if (checked) {
//        App.tabBot.closeTab(App.pnlPO_PriceCpny);
//    }
//    else {
//        App.tabBot.addTab(App.pnlPO_PriceCpny);
//    }
//};

//var cboInvtID_Change = function (item, newValue, oldValue) {
//    //var r = App.cboInvtID.valueModels[0];

//    //if (r == null) {
//    //    App.slmPO_Price.getSelection()[0].set('Descr', "");
//    //}
//    //else {
//    //    App.slmPO_Price.getSelection()[0].set('Descr', r.data.Descr);
//    //}

//};

//var grdPO_Price_BeforeEdit = function (editor, e) {
//    if (!HQ.isUpdate) return false;
//    if (keys.indexOf(e.field) != -1) {
//        if (e.record.data.tstamp != "")
//            return false;
//    }
//    paramInvtID = e.record.data.InvtID;
//    App.cboUOM.getStore().load();
//    return HQ.grid.checkInput(e, keys);
//};

//var grdPO_Price_Edit = function (item, e) {

//    if (keys.indexOf(e.field) != -1) {
//        if (e.value != '' && isAllValidKey(App.stoPO_Price.getChangedData().Created) && isAllValidKey(App.stoPO_Price.getChangedData().Updated))
//            HQ.store.insertBlank(App.stoPO_Price);
//    }

//if (e.field == 'InvtID') {
//    var selectedRecord = App.cboInvtID.store.findRecord(e.field, e.value);
//    if (selectedRecord) {
//        e.record.set("Descr", selectedRecord.data.Descr);
//    }
//    else {
//        e.record.set("Descr", "");
//    }
//}
//};

//var grdPO_Price_ValidateEdit = function (item, e) {
//    if (keys.indexOf(e.field) != -1) {
//        if (HQ.grid.checkDuplicate(App.grdPO_Price, e, keys)) {
//            HQ.message.show(1112, e.value);
//            return false;
//        }
//        var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/
//        if (!e.value.match(regex)) {
//            HQ.message.show(20140811, e.column.text);
//            return false;
//        }
//    }
//};

//var grdPO_Price_Reject = function (record) {
//    if (record.data.tstamp == '') {
//        App.stoPO_Price.remove(record);
//        App.grdPO_Price.getView().focusRow(App.stoPO_Price.getCount() - 1);
//        App.grdPO_Price.getSelectionModel().select(App.stoPO_Price.getCount() - 1);
//    } else {
//        record.reject();
//    }
//};

//var save = function () {
//    if (App.frmMain.isValid()) {
//        App.frmMain.updateRecord();
//        App.frmMain.submit({
//            waitMsg: HQ.common.getLang("WaitMsg"),
//            url: 'PO20100/Save',
//            params: {
//                lstPOPriceHeader: HQ.store.getData(App.frmMain.getRecord().store),
//                lstPO_Price: HQ.store.getData(App.stoPO_Price),
//                lstPO_PriceCpny: HQ.store.getData(App.stoPO_PriceCpny)
//            },
//            success: function (msg, data) {
//                HQ.message.show(201405071);
//                App.cboPriceID.getStore().reload();
//                menuClick("refresh");
//            },
//            failure: function (msg, data) {
//                HQ.message.process(msg, data, true);
//            }
//        });
//    }
//};

//var deleteData = function (item) {
//    if (item == "yes") {
//        if (_focusNo == 0) {
//            App.direct.DeleteAll(App.cboPriceID.getValue(), {
//                success: function () {
//                    App.cboPriceID.getStore().load();
//                    menuClick("new");
//                },
//                eventMask: { msg: 'HQ.common.getLang("DeletingData")', showMask: true }
//            });
//        }
//        else if (_focusNo == 1) {
//            App.grdPO_Price.deleteSelected();
//        }
//        else if (_focusNo == 2) {
//            App.grdPO_PriceCpny.deleteSelected();
//        }
//    }
//};

////kiem tra key da nhap du chua
//var isAllValidKey = function (items) {
//    if (items != undefined) {
//        if (_focusNo == 1) {
//            for (var i = 0; i < items.length; i++) {
//                for (var j = 0; j < keys.length; j++) {
//                    if (items[i][keys[j]] == '' || items[i][keys[j]] == undefined)
//                        return false;
//                }
//            }
//        }
//        if (_focusNo == 2) {
//            for (var i = 0; i < items.length; i++) {
//                for (var j = 0; j < keysCpny.length; j++) {
//                    if (items[i][keysCpny[j]] == '' || items[i][keysCpny[j]] == undefined)
//                        return false;
//                }
//            }
//        }
//        return true;
//    } else {
//        return true;
//    }
//};

////kiem tra nhung field yeu cau bat buoc nhap
//var checkRequire = function (items) {
//    if (items != undefined) {
//        for (var i = 0; i < items.length; i++) {
//            if (HQ.grid.checkRequirePass(items[i], keys)) continue;
//            if (items[i]["InvtID"].trim() == "") {
//                HQ.message.show(15, HQ.common.getLang("InvtID"));
//                return false;
//            }
//            if (items[i]["UOM"].trim() == "") {
//                HQ.message.show(15, HQ.common.getLang("UOM"));
//                return false;
//            }

//        }
//        return true;
//    } else {
//        return true;
//    }
//};

///////grdPOPriceCnpy
//var grdPO_PriceCpny_BeforeEdit = function (editor, e) {
//    if (!HQ.isUpdate) return false;
//    //keys = e.record.idProperty.split(',');
//    return HQ.grid.checkInput(e, keysCpny);
//};
//var grdPO_PriceCpny_Edit = function (item, e) {

//    if (keysCpny.indexOf(e.field) != -1) {
//        if (e.value != '' && isAllValidKey(App.stoPO_PriceCpny.getChangedData().Created) && isAllValidKey(App.stoPO_PriceCpny.getChangedData().Updated))
//            HQ.store.insertBlank(App.stoPO_PriceCpny);
//    }

//if (e.field == 'CpnyID') {
//    var selectedRecord = App.cboCpnyID.store.findRecord("BranchID", e.value);
//    if (selectedRecord) {
//        e.record.set("CpnyName", selectedRecord.data.BranchName);
//    }
//    else {
//        e.record.set("CpnyName", "");
//    }
//}
//};
//var grdPO_PriceCpny_ValidateEdit = function (item, e) {
//    if (keysCpny.indexOf(e.field) != -1) {
//        if (HQ.grid.checkDuplicate(App.grdPO_PriceCpny, e, keysCpny)) {
//            HQ.message.show(1112, e.value);
//            return false;
//        }
//        var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/
//        if (e.value && !e.value.match(regex)) {
//            HQ.message.show(20140811, e.column.text);
//            return false;
//        }
//    }
//};
//var grdPO_PriceCpny_Reject = function (record) {
//    record.reject();
//};

////// Other Functions ////////////////////////////////////////////////////

//var askClose = function (item) {
//    if (item == "no" || item == "ok") {
//        HQ.common.close(this);
//    }
//};
