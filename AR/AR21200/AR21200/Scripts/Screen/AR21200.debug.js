//// Declare //////////////////////////////////////////////////////////
var fieldsCheckRequire = ["Location", "Descr"];
var keys = ['Location'];
///////////////////////////////////////////////////////////////////////

//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoLocation.reload();
        HQ.common.showBusy(false);
    }
};

////////////////////////////////////////////////////////////////////////


//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdLocation);
            break;
        case "prev":
            HQ.grid.prev(App.grdLocation);
            break;
        case "next":
            HQ.grid.next(App.grdLocation);
            break;
        case "last":
            HQ.grid.last(App.grdLocation);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoChannel.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdLocation);
            }
            break;
        case "delete":
            //if (App.slmLocation.selected.items[0] != undefined) {
            //    if (HQ.isDelete) {
            //        HQ.message.show(11, '', 'deleteData');
            //    }
            //}
            //break;
            if (HQ.isDelete) {
                if (App.slmLocation.selected.items[0] != undefined) {
                    if (App.slmLocation.selected.items[0].data.Location != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.slmLocation)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            //if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
            //    if (checkRequire(App.stoLocation.getChangedData().Created) && checkRequire(App.stoLocation.getChangedData().Updated)) {
            //        save();
            //    }
            //}
            //break;
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoLocation, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.store.isChange(App.stoLocation)) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};
var firstLoad = function () {
    HQ.util.checkAccessRight(); // kiểm tra các quyền update,insert,del
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    checkLoad(); // Mới
};
var grdLocation_BeforeEdit = function (editor, e) {
    if (!HQ.isUpdate) return false; 
    if (keys.indexOf(e.field) != -1) {
        if (e.record.data.tstamp != "")
            return false;
    }
    return HQ.grid.checkInput(e, keys);
};
var grdLocation_Edit = function (item, e) {

    if (keys.indexOf(e.field) != -1) {
        if (e.value != '' && isAllValidKey(App.stoLocation.getChangedData().Created) && isAllValidKey(App.stoLocation.getChangedData().Updated))
            HQ.store.insertBlank(App.stoLocation);
    }
};
var grdLocation_ValidateEdit = function (item, e) {
    if (keys.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(App.grdLocation, e, keys)) {
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

var grdLocation_CancelEdit = function (editor, e) {
    if (e.record.phantom) {
        e.store.remove(e.record);
    }
};

var grd_Reject = function (record) {
    if (record.data.tstamp == '') {
        App.stoLocation.remove(record);
        App.grdLocation.getView().focusRow(App.stoLocation.getCount() - 1);
        App.grdLocation.getSelectionModel().select(App.stoLocation.getCount() - 1);
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
            url: 'AR21200/Save',
            params: {
                lstLocation: HQ.store.getData(App.stoLocation)
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
        App.grdLocation.deleteSelected();
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
            if (items[i]["Location"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("Location"));
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








