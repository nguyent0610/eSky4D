﻿//// Declare //////////////////////////////////////////////////////////

var keys = ['ScreenNumber'];
var fieldsCheckRequire = ["ScreenNumber"];
var fieldsLangCheckRequire = ["ScreenNumber"];
var _Source = 0;
var _maxSource = 5;
var _isLoadMaster = false;
/////////////////////Store////////////////////
var checkLoad = function (sto) {
    _Source = 1;
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"), App.frmMain);
    App.cboRecType.getStore().addListener('load', cboStore_load);
    App.cboUsr_GrByType.getStore().addListener('load', cboStore_load);
    App.cboCpny.getStore().addListener('load', cboStore_load);
    App.cboModule.getStore().addListener('load', cboStore_load);
    App.cboScreenNbr.getStore().addListener('load', cboStore_load);

    App.cboRecType.getStore().reload();
    App.cboCpny.getStore().reload();
    App.cboModule.getStore().reload();
    App.cboScreenNbr.getStore().reload();

};
var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboRecType.getStore().load(function () {
        App.cboUsr_GrByType.getStore().load(function (sto) {
            //if (sto.length > 0) {
            App.cboUsr_GrByTypeCopy.store.data.addAll(sto);
            //}
            App.cboCpny.getStore().load(function () {
                App.cboModule.getStore().load(function () {
                    HQ.common.showBusy(false);
                })
            })
        })
    })
};

var loadCheck = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.txtInitRights.getStore().load(function () {
        HQ.common.showBusy(false, HQ.common.getLang("loadingData"));
    })
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_AccessDetRights);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_AccessDetRights);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_AccessDetRights);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_AccessDetRights);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                refresh('yes');
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_AccessDetRights, keys);
            }
            break;
        case "delete":
            if (App.slmSYS_AccessDetRights.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    if (App.slmSYS_AccessDetRights.selected.items[0] != undefined) {
                        if (App.slmSYS_AccessDetRights.selected.items[0].data.ScreenNumber != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSYS_AccessDetRights)], 'deleteData', true);
                        }
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSYS_AccessDetRights, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            break;
    }

};
var beforeSelectcombo = function () {
    loadSourceCombo();
};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.util.checkAccessRight(); // kiểm tra các quyền update,insert,del
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    checkLoad(); // Mới
};

function cboStore_load(sto) {
    if (_Source < _maxSource) {
        _Source++;
    } else {
        HQ.common.showBusy(false);
    }

    if (sto.storeId == App.cboUsr_GrByType.store.storeId) {
        App.cboUsr_GrByTypeCopy.store.removeAll();
        App.cboUsr_GrByTypeCopy.clearValue();
        App.cboUsr_GrByTypeCopy.store.add(sto.getRange());
    }

    if (sto.getCount() > 0) {
        var str = sto.storeId;
        var n = str.indexOf("SA00700");
        var cboID = str.substring(0, n);
        if (cboID == App.cboRecType.id || cboID == App.cboCpny.id || cboID == App.cboModule.id) {
            var cbo = App[cboID];
            cbo.setValue(sto.getAt(0));            
        }
    }
}

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoSYS_AccessDetRights);
    HQ.common.changeData(HQ.isChange, 'SA00700');//co thay doi du lieu gan * tren tab title header
};
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSYS_AccessDetRights.reload();
    }
};

var beforeLoad = function () {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
    App.InitRightsCheckAll.setValue(false);
    App.InsertRightsCheckAll.setValue(false);
    App.UpdateRightsCheckAll.setValue(false);
    App.DeleteRightsCheckAll.setValue(false);
    App.ViewRightsCheckAll.setValue(false);
    App.ReleaseRightsCheckAll.setValue(false);
}
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {

    HQ.common.showBusy(false);
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false; //sto load cuoi se su dung
    }
    frmChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
    //App.frmMain.suspendLayouts();
    //store.reload();
    //App.frmMain.resumeLayouts(true);
    // App.frmMain.doLayout();
};
//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
var grdSYS_AccessDetRights_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdSYS_AccessDetRights_Edit = function (item, e) {
    if (e.field == 'InitRights') {
        // e.record.set("InitRights", e.value);
        e.record.set("InsertRights", e.value);
        e.record.set("UpdateRights", e.value);
        e.record.set("DeleteRights", e.value);
        e.record.set("ViewRights", e.value);
        e.record.set("ReleaseRights", e.value);
    } else if (e.field == 'InsertRights' || e.field == 'UpdateRights' || e.field == 'DeleteRights' || e.field == 'ViewRights' || e.field == 'ReleaseRights') {
        var initRight = true;
        if (e.record.data.InsertRights == false || e.record.data.UpdateRights == false || e.record.data.DeleteRights == false || e.record.data.ViewRights == false || e.record.data.ReleaseRights == false) {
            initRight = false;
        }
        e.record.set("InitRights", initRight);
    }
    HQ.grid.checkInsertKey(App.grdSYS_AccessDetRights, e, keys);
    frmChange();
};
var grdSYS_AccessDetRights_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_AccessDetRights, e, keys);
};
var grdSYS_AccessDetRights_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_AccessDetRights);
    frmChange();
};

var InitRightsCheckAll_Change = function (value) {
    if (value) {
        var allData = App.stoSYS_AccessDetRights.snapshot || App.stoSYS_AccessDetRights.allData || App.stoSYS_AccessDetRights.data;

        App.stoSYS_AccessDetRights.suspendEvents();

        allData.each(function (item) {
            item.set("InitRights", value.checked);
            item.set("InsertRights", value.checked);
            item.set("UpdateRights", value.checked);
            item.set("DeleteRights", value.checked);
            item.set("ViewRights", value.checked);
            item.set("ReleaseRights", value.checked);
        });
        App.stoSYS_AccessDetRights.resumeEvents();
        App.grdSYS_AccessDetRights.view.refresh();
    }
    frmChange();
};

var InsertRightsCheckAll_Change = function (value) {
    if (value) {
        var allData = App.stoSYS_AccessDetRights.snapshot || App.stoSYS_AccessDetRights.allData || App.stoSYS_AccessDetRights.data;
        App.stoSYS_AccessDetRights.suspendEvents();
        allData.each(function (item) {
            item.set("InsertRights", value.checked);
            var initRight = true;
            if (item.data.InsertRights == false || item.data.UpdateRights == false || item.data.DeleteRights == false || item.data.ViewRights == false || item.data.ReleaseRights == false) {
                initRight = false;
            }
            item.set("InitRights", initRight);
        });
        App.stoSYS_AccessDetRights.resumeEvents();
        App.grdSYS_AccessDetRights.view.refresh();
    }

    frmChange();
};

var UpdateRightsCheckAll_Change = function (value) {
    if (value) {
        var allData = App.stoSYS_AccessDetRights.snapshot || App.stoSYS_AccessDetRights.allData || App.stoSYS_AccessDetRights.data;
        App.stoSYS_AccessDetRights.suspendEvents();
        allData.each(function (item) {
            item.set("UpdateRights", value.checked);
            var initRight = true;
            if (item.data.InsertRights == false || item.data.UpdateRights == false || item.data.DeleteRights == false || item.data.ViewRights == false || item.data.ReleaseRights == false) {
                initRight = false;
            }
            item.set("InitRights", initRight);
        });
        App.stoSYS_AccessDetRights.resumeEvents();
        App.grdSYS_AccessDetRights.view.refresh();
    }
    frmChange();
};

var DeleteRightsCheckAll_Change = function (value) {
    if (value) {
        var allData = App.stoSYS_AccessDetRights.snapshot || App.stoSYS_AccessDetRights.allData || App.stoSYS_AccessDetRights.data;
        App.stoSYS_AccessDetRights.suspendEvents();
        allData.each(function (item) {
            item.set("DeleteRights", value.checked);
            var initRight = true;
            if (item.data.InsertRights == false || item.data.UpdateRights == false || item.data.DeleteRights == false || item.data.ViewRights == false || item.data.ReleaseRights == false) {
                initRight = false;
            }
            item.set("InitRights", initRight);
        });
        App.stoSYS_AccessDetRights.resumeEvents();
        App.grdSYS_AccessDetRights.view.refresh();
    }
    frmChange();
};

var ViewRightsCheckAll_Change = function (value) {
    if (value) {
        var allData = App.stoSYS_AccessDetRights.snapshot || App.stoSYS_AccessDetRights.allData || App.stoSYS_AccessDetRights.data;
        App.stoSYS_AccessDetRights.suspendEvents();
        allData.each(function (item) {
            item.set("ViewRights", value.checked);
            var initRight = true;
            if (item.data.InsertRights == false || item.data.UpdateRights == false || item.data.DeleteRights == false || item.data.ViewRights == false || item.data.ReleaseRights == false) {
                initRight = false;
            }
            item.set("InitRights", initRight);
        });
        App.stoSYS_AccessDetRights.resumeEvents();
        App.grdSYS_AccessDetRights.view.refresh();
    }
    frmChange();
};

var ReleaseRightsCheckAll_Change = function (value) {
    if (value) {
        var allData = App.stoSYS_AccessDetRights.snapshot || App.stoSYS_AccessDetRights.allData || App.stoSYS_AccessDetRights.data;
        App.stoSYS_AccessDetRights.suspendEvents();
        allData.each(function (item) {
            item.set("ReleaseRights", value.checked);
            var initRight = true;
            if (item.data.InsertRights == false || item.data.UpdateRights == false || item.data.DeleteRights == false || item.data.ViewRights == false || item.data.ReleaseRights == false) {
                initRight = false;
            }
            item.set("InitRights", initRight);
        });
        App.stoSYS_AccessDetRights.resumeEvents();
        App.grdSYS_AccessDetRights.view.refresh();
    }
    frmChange();
};

var LoadGrid = function (sender, e) {
    if (!App.frmMain.isValid()) {
        return;
    }
    App.grdSYS_AccessDetRights.show();
    App.stoSYS_AccessDetRights.reload();
    if (App.InitRightsCheckAll.value == true || App.InsertRightsCheckAll.value == true ||
        App.UpdateRightsCheckAll.value == true || App.DeleteRightsCheckAll.value == true ||
        App.ViewRightsCheckAll.value == true || App.ReleaseRightsCheckAll.value == true) {
        App.InitRightsCheckAll.setValue(false);
        App.InsertRightsCheckAll.setValue(false);
        App.UpdateRightsCheckAll.setValue(false);
        App.DeleteRightsCheckAll.setValue(false);
        App.ViewRightsCheckAll.setValue(false);
        App.ReleaseRightsCheckAll.setValue(false);
    }
};

var cboRecType_Change = function (sender, e) {
    App.cboUsr_GrByType.store.load();
};

var cboScreenNumber_Change = function (value) {
    var k = value.displayTplData[0].Descr;
    var l = value.displayTplData[0].ScreenNumberCmt;
    App.slmSYS_AccessDetRights.selected.items[0].set('Descr', k);
    App.slmSYS_AccessDetRights.selected.items[0].set('ScreenNumberCmt', l);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.cboRecType.getValue() != '') {
        if (!App.cboUsr_GrByType.getValue()) {
            HQ.message.show(15, App.cboUsr_GrByType.fieldLabel, '');
            return;
        }
    }
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA00700/Save',
            params: {
                lstSYS_AccessDetRights: HQ.store.getData(App.stoSYS_AccessDetRights),
                RecType: App.cboRecType.getValue(),
                UserID: App.cboUsr_GrByType.getValue(),
                CpnyID: App.cboCpny.getValue()
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                refresh("yes");
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};


var copy = function () {
    if (App.cboRecType.getValue() != '') {
        if (Ext.isEmpty(App.cboUsr_GrByType.getValue())) {
            HQ.message.show(15, App.cboUsr_GrByType.fieldLabel, '');
            return;
        }
        if (Ext.isEmpty(App.cboUsr_GrByTypeCopy.getValue())) {
            HQ.message.show(15, App.cboUsr_GrByTypeCopy.fieldLabel, '');
            return;
        }
    } else {
        HQ.message.show(15, App.cboRecType.fieldLabel, '');
        return;
    }

    if (App.cboUsr_GrByTypeCopy.getValue() == App.cboUsr_GrByType.getValue()) {
        HQ.message.show(2019022803, [App.cboUsr_GrByTypeCopy.fieldLabel, App.cboUsr_GrByType.fieldLabel], '', true);
        return;
    }
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SA00700CoppingData"),
            url: 'SA00700/copy',
            params: {
                RecType: App.cboRecType.getValue(),
                UserID: App.cboUsr_GrByType.getValue(),
                userIDCopy: App.cboUsr_GrByTypeCopy.getValue(),
            },
            success: function (msg, data) {
                HQ.message.show(2019022802);
                refresh("yes");
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdSYS_AccessDetRights.deleteSelected();
        frmChange();
    }
};

var isAllValidKey = function (items) {
    if (items != undefined) {
        for (var i = 0; i < items.length; i++) {
            for (var j = 0; j < keys.length; j++) {
                if (items[i][keys[j]] == '' || items[i][keys[j]] == undefined)
                    return false;
            }
        }
        return true;
    } else {
        return true;
    }
};
/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
///////////////////////////////////