//// Declare //////////////////////////////////////////////////////////

var keys = ['ScreenNumber'];
var fieldsCheckRequire = ["ScreenNumber"];
var fieldsLangCheckRequire = ["ScreenNumber"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboRecType.getStore().load(function () {
        App.cboUsr_GrByType.getStore().load(function () {
            App.cboCpny.getStore().load(function () {
                App.cboModule.getStore().load(function () {
                    HQ.common.showBusy(false, HQ.common.getLang("loadingData"));
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
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSYS_AccessDetRights.reload();
            }
            break;
        case "new":
        case "delete":
            if (App.slmSYS_AccessDetRights.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
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
            HQ.common.close(this);
            break;
    }

};
var beforeSelectcombo = function () {
    loadSourceCombo();
};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoSYS_AccessDetRights.reload();
};
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA00700');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA00700');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
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
    stoChanged(App.stoSYS_AccessDetRights);
};

var InitRightsCheckAll_Change = function (value) {
    if (value) {
        App.stoSYS_AccessDetRights.suspendEvents();
        App.grdSYS_AccessDetRights.getStore().each(function (item) {
            item.set("InitRights", value.checked);
        });
        App.stoSYS_AccessDetRights.resumeEvents();
        App.grdSYS_AccessDetRights.view.refresh();
    }

};

var InsertRightsCheckAll_Change = function (value) {
    if (value) {
        App.stoSYS_AccessDetRights.suspendEvents();
        App.grdSYS_AccessDetRights.getStore().each(function (item) {
            item.set("InsertRights", value.checked);
        });
        App.stoSYS_AccessDetRights.resumeEvents();
        App.grdSYS_AccessDetRights.view.refresh();
    }
};

var UpdateRightsCheckAll_Change = function (value) {
    if (value) {
        App.stoSYS_AccessDetRights.suspendEvents();
        App.grdSYS_AccessDetRights.getStore().each(function (item) {
            item.set("UpdateRights", value.checked);
        });
        App.stoSYS_AccessDetRights.resumeEvents();
        App.grdSYS_AccessDetRights.view.refresh();
    }
};

var DeleteRightsCheckAll_Change = function (value) {
    if (value) {
        App.stoSYS_AccessDetRights.suspendEvents();
        App.grdSYS_AccessDetRights.getStore().each(function (item) {
            item.set("DeleteRights", value.checked);
        });
        App.stoSYS_AccessDetRights.resumeEvents();
        App.grdSYS_AccessDetRights.view.refresh();
    }
};

var ViewRightsCheckAll_Change = function (value) {
    if (value) {
        App.stoSYS_AccessDetRights.suspendEvents();
        App.grdSYS_AccessDetRights.getStore().each(function (item) {
            item.set("ViewRights", value.checked);
        });
        App.stoSYS_AccessDetRights.resumeEvents();
        App.grdSYS_AccessDetRights.view.refresh();
    }
};

var ReleaseRightsCheckAll_Change = function (value) {
    if (value) {
        App.stoSYS_AccessDetRights.suspendEvents();
        App.grdSYS_AccessDetRights.getStore().each(function (item) {
            item.set("ReleaseRights", value.checked);
        });
        App.stoSYS_AccessDetRights.resumeEvents();
        App.grdSYS_AccessDetRights.view.refresh();
    }
};

var LoadGrid = function (sender, e) {
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
    App.slmSYS_AccessDetRights.selected.items[0].set('Descr', k);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
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
                //menuClick("refresh");
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
        stoChanged(App.stoSYS_AccessDetRights);
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSYS_AccessDetRights.reload();
    }
};
///////////////////////////////////