var keys = ['GroupID'];
var fieldsCheckRequire = ["GroupID", "Descr"];
var fieldsLangCheckRequire = ["GroupID", "Descr"];
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_Group);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_Group);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_Group);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_Group);
            break;
        case "refresh":
            App.stoSYS_Group.reload();
            HQ.grid.first(App.grdSYS_Group);
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_Group);
            }
            break;
        case "delete":
            if (App.slmData.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSYS_Group, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.store.isChange(App.stoSYS_Group)) {
                HQ.message.show(7, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};
var grdSYS_Group_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdSYS_Group_Edit = function (item, e) {

    HQ.grid.checkInsertKey(App.stoSYS_Group, e, keys);
};
var grdSYS_Group_ValidateEdit = function (item, e) {
    if (keys.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(App.grdSYS_Group, e, keys)) {
            HQ.message.show(1112, e.value);
            return false;
        }
        var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/
        if (!e.value.match(regex)) {
            HQ.message.show(20140811, e.column.text);
            return false;
        }
    }
};
var grdSYS_Group_Reject = function (record) {
    if (record.data.tstamp == '') {
        App.stoSYS_Group.remove(record);
        App.grdSYS_Group.getView().focusRow(App.stoSYS_Group.getCount() - 1);
        App.grdSYS_Group.getSelectionModel().select(App.stoSYS_Group.getCount() - 1);
    } else {
        record.reject();
    }
};

var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SA00500/Save',
            params: {
                lstData: HQ.store.getData(App.stoSYS_Group)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                menuClick("refresh");
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdSYS_Group.deleteSelected();
    }
};

//// Other Functions ////////////////////////////////////////////////////

var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.close(this);
    }
};