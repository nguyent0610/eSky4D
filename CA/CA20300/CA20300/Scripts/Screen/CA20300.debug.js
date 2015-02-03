//// Declare //////////////////////////////////////////////////////////

var keys = ['TypeID'];
///////////////////////////////////////////////////////////////////////

//// Store /////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////


//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdCostType);
            break;
        case "prev":
            HQ.grid.prev(App.grdCostType);
            break;
        case "next":
            HQ.grid.next(App.grdCostType);
            break;
        case "last":
            HQ.grid.last(App.grdCostType);
            break;
        case "refresh":
            App.stoCostType.reload();
            HQ.grid.first(App.grdCostType);
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdCostType);
            }
            break;
        case "delete":
            if (App.slmCostType.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (checkRequire(App.stoCostType.getChangedData().Created) && checkRequire(App.stoCostType.getChangedData().Updated)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.store.isChange(App.stoCostType)) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};
var grdCostType_BeforeEdit = function (editor, e) {
    if (!HQ.isUpdate) return false;
    //keys = e.record.idProperty.split(',');

    if (keys.indexOf(e.field) != -1) {
        if (e.record.data.tstamp != "")
            return false;
    }

};
var grdCostType_Edit = function (item, e) {

    if (keys.indexOf(e.field) != -1) {
        if (e.value != '' && isAllValidKey(App.stoCostType.getChangedData().Created) && isAllValidKey(App.stoCostType.getChangedData().Updated))
            HQ.store.insertBlank(App.stoCostType);
    }
};
var grdCostType_ValidateEdit = function (item, e) {
    if (keys.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(App.grdCostType, e, keys)) {
            HQ.message.show(1112, e.value);
            return false;
        }
        var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/
        if (e.value.match(regex)) {
            return true;

        } else {
            HQ.message.show(20140811, e.column.text);
            return false;
        }
    }
};

var grdCostType_CancelEdit = function (editor, e) {
    if (e.record.phantom) {
        e.store.remove(e.record);
    }
};

var grd_Reject = function (record) {
    if (record.data.tstamp == '') {
        App.stoCostType.remove(record);
        App.grdCostType.getView().focusRow(App.stoCostType.getCount() - 1);
        App.grdCostType.getSelectionModel().select(App.stoCostType.getCount() - 1);
    } else {
        record.reject();
    }
};

/////////////////////////////////////////////////////////////////////////



//// Process Data ///////////////////////////////////////////////////////

var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'CA20300/Save',
            params: {
                lstCostType: HQ.store.getData(App.stoCostType)
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
        App.grdCostType.deleteSelected();
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
            if (items[i]["TypeID"] == undefined) continue;
            if (items[i]["TypeID"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("TypeID"));
                return false;
            }
            if (items[i]["Descr"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("Descr"));
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

var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.close(this);
    }
};

/////////////////////////////////////////////////////////////////////////








