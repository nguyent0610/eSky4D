//// Declare //////////////////////////////////////////////////////////

var keys = ['ReasonCD'];
var fieldsCheckRequire = ["ReasonCD"];
var fieldsLangCheckRequire = ["ReasonCD"];
var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;
/////////////////////Store////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoSYS_AccessDetRights.reload();
        HQ.common.showBusy(false);
    }
};
var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboRecType.getStore().load(function () {
        App.cboUsr_GrByType.getStore().load(function () {
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
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.grdSYS_AccessDetRights.show();
                App.stoSYS_AccessDetRights.reload();
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

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoSYS_AccessDetRights);
    HQ.common.changeData(HQ.isChange, 'IN23300');//co thay doi du lieu gan * tren tab title header
};
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.grdSYS_AccessDetRights.show();
        App.stoSYS_AccessDetRights.reload();
    }
};


//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
   
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
    if (HQ.isFirstLoad) {
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
    HQ.grid.checkInsertKey(App.grdSYS_AccessDetRights, e, keys);
};
var grdSYS_AccessDetRights_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_AccessDetRights, e, keys);
};
var grdSYS_AccessDetRights_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_AccessDetRights);
    frmChange();
};

var chkApplyForHeaderAll_Change = function (value) {
    if (value) {
        var allData = App.stoSYS_AccessDetRights.snapshot || App.stoSYS_AccessDetRights.allData || App.stoSYS_AccessDetRights.data;
        App.stoSYS_AccessDetRights.suspendEvents();
        allData.each(function (item) {
            item.set("CheckApplyFor", value.checked);
        });
        App.stoSYS_AccessDetRights.resumeEvents();
        App.grdSYS_AccessDetRights.view.refresh();
    }
};

var LoadGrid = function (sender, e) {
    App.grdSYS_AccessDetRights.show();
    App.stoSYS_AccessDetRights.reload();
    //if (App.InitRightsCheckAll.value == true || App.InsertRightsCheckAll.value == true ||
    //    App.UpdateRightsCheckAll.value == true || App.DeleteRightsCheckAll.value == true ||
    //    App.ViewRightsCheckAll.value == true || App.ReleaseRightsCheckAll.value == true) {
    //    App.InitRightsCheckAll.setValue(false);
    //    App.InsertRightsCheckAll.setValue(false);
    //    App.UpdateRightsCheckAll.setValue(false);
    //    App.DeleteRightsCheckAll.setValue(false);
    //    App.ViewRightsCheckAll.setValue(false);
    //    App.ReleaseRightsCheckAll.setValue(false);
    //}
};

var cboRecType_Change = function (sender, e) {
    App.cboUsr_GrByType.store.load();
    App.grdSYS_AccessDetRights.show();
    App.stoSYS_AccessDetRights.reload();
};

var cboUsr_GrByType_Change = function (sender, e) {
    App.grdSYS_AccessDetRights.show();
    App.stoSYS_AccessDetRights.reload();
};


var cboScreenNumber_Change = function (value) {
    var k = value.displayTplData[0].Descr;
    App.slmSYS_AccessDetRights.selected.items[0].set('Descr', k);
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
            url: 'IN23300/Save',
            params: {
                lstSYS_AccessDetRights: HQ.store.getData(App.stoSYS_AccessDetRights),
                RecType: App.cboRecType.getValue(),
                UserID: App.cboUsr_GrByType.getValue()
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                //menuClick("refresh");
                refresh("yes");
                App.stoSYS_AccessDetRights.reload();
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
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.grdSYS_AccessDetRights.show();
        App.stoSYS_AccessDetRights.reload();
    }
};
///////////////////////////////////