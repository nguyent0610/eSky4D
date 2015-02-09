var keys = ['Code'];

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_Configurations);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_Configurations);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_Configurations);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_Configurations);
            break;
        case "refresh":
            App.stoData.reload();
            HQ.grid.first(App.grdSYS_Configurations);
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSYS_Configurations);
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
                if (checkRequire(App.stoData.getChangedData().Created) && checkRequire(App.stoData.getChangedData().Updated)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.store.isChange(App.stoData)) {
                HQ.message.show(7, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};
var grdSYS_Configurations_BeforeEdit = function (editor, e) {
    if (!HQ.isUpdate) return false;
    //keys = e.record.idProperty.split(',');

    if (keys.indexOf(e.field) != -1) {
        if (e.record.data.tstamp != "")
            return false;
    }
    return HQ.grid.checkInput(e, keys);
};
var grdSYS_Configurations_Edit = function (item, e) {

    if (keys.indexOf(e.field) != -1) {
        if (e.value != '' && isAllValidKey(App.stoData.getChangedData().Created) && isAllValidKey(App.stoData.getChangedData().Updated))
            HQ.store.insertBlank(App.stoData);
    }
};
var grdSYS_Configurations_ValidateEdit = function (item, e) {
    if (keys.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(App.grdSYS_Configurations, e, keys)) {
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
var grdSYS_Configurations_Reject = function (record) {
    if (record.data.tstamp == '') {
        App.stoData.remove(record);
        App.grdSYS_Configurations.getView().focusRow(App.stoData.getCount() - 1);
        App.grdSYS_Configurations.getSelectionModel().select(App.stoData.getCount() - 1);
    } else {
        record.reject();
    }
};

var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SA01300/Save',
            params: {
                lstData: HQ.store.getData(App.stoData)
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
        App.grdSYS_Configurations.deleteSelected();
    }
};
//kiem tra key da nhap du chua
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
//kiem tra nhung field yeu cau bat buoc nhap
var checkRequire = function (items) {
    if (items != undefined) {
        for (var i = 0; i < items.length; i++) {
            if (HQ.grid.checkRequirePass(items[i], keys)) continue;
            if (items[i]["Code"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("Code"));
                return false;
            }

            if (items[i]["TextVal"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("TextVal"));
                return false;
            }
        }
        return true;
    } else {
        return true;
    }
};

//// Other Functions ////////////////////////////////////////////////////

var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.close(this);
    }
};