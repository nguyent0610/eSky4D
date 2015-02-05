//// Declare //////////////////////////////////////////////////////////

var keys = ['Territory'];
///////////////////////////////////////////////////////////////////////

//// Store /////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////


//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdTerritory);
            break;
        case "prev":
            HQ.grid.prev(App.grdTerritory);
            break;
        case "next":
            HQ.grid.next(App.grdTerritory);
            break;
        case "last":
            HQ.grid.last(App.grdTerritory);
            break;
        case "refresh":
            App.stoTerritory.reload();
            HQ.grid.first(App.grdTerritory);
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdTerritory);
            }
            break;
        case "delete":
            if (App.slmTerritory.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (checkRequire(App.stoTerritory.getChangedData().Created) && checkRequire(App.stoTerritory.getChangedData().Updated)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.store.isChange(App.stoTerritory)) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};
var grdTerritory_BeforeEdit = function (editor, e) {
    if (!HQ.isUpdate) return false;   
    if (keys.indexOf(e.field) != -1) {
        if (e.record.data.tstamp != "")
            return false;
    }

    return HQ.grid.checkInput(e, keys);
};
var grdTerritory_Edit = function (item, e) {

    if (keys.indexOf(e.field) != -1) {
        if (e.value != '' && isAllValidKey(App.stoTerritory.getChangedData().Created) && isAllValidKey(App.stoTerritory.getChangedData().Updated))
            HQ.store.insertBlank(App.stoTerritory);
    }
};
var grdTerritory_ValidateEdit = function (item, e) {
    if (keys.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(App.grdTerritory, e, keys)) {
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

var grdTerritory_CancelEdit = function (editor, e) {
    if (e.record.phantom) {
        e.store.remove(e.record);
    }
};

var grd_Reject = function (record) {
    if (record.data.tstamp == '') {
        App.stoTerritory.remove(record);
        App.grdTerritory.getView().focusRow(App.stoTerritory.getCount() - 1);
        App.grdTerritory.getSelectionModel().select(App.stoTerritory.getCount() - 1);
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
            url: 'AR20900/Save',
            params: {
                lstTerritory: HQ.store.getData(App.stoTerritory)
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
        App.grdTerritory.deleteSelected();
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
            if (items[i]["Territory"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("Territory"));
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








