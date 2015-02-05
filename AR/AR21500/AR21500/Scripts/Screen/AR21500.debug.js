var keys = ['DispMethod'];
///////////////////////////////////////////////////////////////////////

//// Store /////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////


//// Event /////////////////////////////////////////////////////////////
   
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdDisplayMethod);
            break;
        case "prev":
            HQ.grid.prev(App.grdDisplayMethod);
            break;
        case "next":
            HQ.grid.next(App.grdDisplayMethod);
            break;
        case "last":
            HQ.grid.last(App.grdDisplayMethod);
            break;
        case "refresh":
            App.stoDisplayMethod.reload();
            HQ.grid.first(App.grdDisplayMethod);
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdDisplayMethod);
            }
            break;
        case "delete":
            if (App.slmDisplayMethod.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (checkRequire(App.stoDisplayMethod.getChangedData().Created)
                    && checkRequire(App.stoDisplayMethod.getChangedData().Updated))
                {
                    save();
                }
            }
            break;
        case "print":
            alert(command);
            break;
        case "close":
            if (HQ.store.isChange(App.stoDisplayMethod)) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }
};
// Danh cho grid ///////////////////////////////////////////////////////
var grdDisplayMethod_BeforeEdit = function (editor, e) {
    if (!HQ.isUpdate) return false;
    //keys = e.record.idProperty.split(',');

    if (keys.indexOf(e.field) != -1) {
        if (e.record.data.tstamp != "")
            return false;
    }
    return HQ.grid.checkInput(e, keys);
};
var grdDisplayMethod_Edit = function (item, e) {

    if (keys.indexOf(e.field) != -1) {
        if (e.value != '' && isAllValidKey(App.stoDisplayMethod.getChangedData().Created) && isAllValidKey(App.stoDisplayMethod.getChangedData().Updated))
            HQ.store.insertBlank(App.stoDisplayMethod);
    }
};
var grdDisplayMethod_ValidateEdit = function (item, e) {
    if (keys.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(App.grdDisplayMethod, e, keys)) {
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
var grdDisplayMethod_CancelEdit = function (editor, e) {
    if (e.record.phantom) {
        e.store.remove(e.record);
    }
};
var grdDisplayMethod_Reject = function (record) {
    if (record.data.tstamp == '') {
        App.stoDisplayMethod.remove(record);
        App.grdDisplayMethod.getView().focusRow(App.stoDisplayMethod.getCount() - 1);
        App.grdDisplayMethod.getSelectionModel().select(App.stoDisplayMethod.getCount() - 1);
    } else record.reject();
};
/////////////////////////////////////////////////////////////////////////

// Process Data ////////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'AR21500/Save',
            params: {
                lstgrd: HQ.store.getData(App.stoDisplayMethod)
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
        App.grdDisplayMethod.deleteSelected();
    }
};
/////////////////////////////////////////////////////////////////////////

// Kiem tra nhung field yeu cau bat buoc nhap //////////////////////////
var checkRequire = function (items) {
    if (items != undefined) {
        for (var i = 0; i < items.length; i++) {
            if (HQ.grid.checkRequirePass(items[i], keys)) continue;
            if (items[i]["DispMethod"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("EntryID"));
                return false;
            }

            if (items[i]["Descr"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("descr"));
                return false;
            }
        }
        return true;
    } else {
        return true;
    }
};
///////////////////////////////////////////////////////////////////////

//kiem tra key da nhap du chua /////////////////////////////////////////
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
///////////////////////////////////////////////////////////////////////

// Other Functions ////////////////////////////////////////////////////
var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.close(this);
    }
};
///////////////////////////////////////////////////////////////////////

var AutoLoadGrid = function () {
    menuClick("refresh");
};

var stoDisplayMethod_load = function () {
    App.stoMasterType.load();
    App.stoMasterStyle.load();
    App.stoMasterShelf.load();
};

var rdType = function (value) {
    var record = App.stoMasterType.findRecord("Code", value);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};
var rdStyle = function (value) {
    var record = App.stoMasterStyle.findRecord("Code", value);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};
var rdShelf = function (value) {
    var record = App.stoMasterShelf.findRecord("Code", value);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};

var ActiveCheckAll_Change = function (value) {
    if (value) {
        App.grdDisplayMethod.getStore().each(function (item) {

            item.set("Active", value.checked);
        });
    }
};
