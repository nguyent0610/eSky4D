//// Declare //////////////////////////////////////////////////////////

var keys = ['EntryID'];
///////////////////////////////////////////////////////////////////////

//// Store /////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////


//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdEntryType);
            break;
        case "prev":
            HQ.grid.prev(App.grdEntryType);
            break;
        case "next":
            HQ.grid.next(App.grdEntryType);
            break;
        case "last":
            HQ.grid.last(App.grdEntryType);
            break;
        case "refresh":
            App.stoEntryType.reload();
            HQ.grid.first(App.grdEntryType);
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdEntryType);
            }
            break;
        case "delete":
            if (App.slmEntryType.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (checkRequire(App.stoEntryType.getChangedData().Created) && checkRequire(App.stoEntryType.getChangedData().Updated)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.store.isChange(App.stoEntryType)) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};
var grdEntryType_BeforeEdit = function (editor, e) {
    if (!HQ.isUpdate) return false;
    //keys = e.record.idProperty.split(',');

    if (keys.indexOf(e.field) != -1) {
        if (e.record.data.tstamp != "")
            return false;
    }
    return HQ.grid.checkInput(e, keys);
};
var grdEntryType_Edit = function (item, e) {

    if (keys.indexOf(e.field) != -1) {
        if (e.value != '' && isAllValidKey(App.stoEntryType.getChangedData().Created) && isAllValidKey(App.stoEntryType.getChangedData().Updated))
            HQ.store.insertBlank(App.stoEntryType);
    }
};
var grdEntryType_ValidateEdit = function (item, e) {
    if (keys.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(App.grdEntryType, e, keys)) {
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

var grdEntryType_CancelEdit = function (editor, e) {
    if (e.record.phantom) {
        e.store.remove(e.record);
    }
};

var grd_Reject = function (record) {
    if (record.data.tstamp == '') {
        App.stoEntryType.remove(record);
        App.grdEntryType.getView().focusRow(App.stoEntryType.getCount() - 1);
        App.grdEntryType.getSelectionModel().select(App.stoEntryType.getCount() - 1);
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
            url: 'CA20100/Save',
            params: {
                lstEntryType: HQ.store.getData(App.stoEntryType)
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
        App.grdEntryType.deleteSelected();
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
            if (items[i]["EntryID"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("EntryID"));
                return false;
            }
          
            if (items[i]["descr"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("descr"));
                return false;
            }
            if (items[i]["RcptDisbFlg"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("RcptDisbFlg"));
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

var ActiveCheckAll_Change = function (value) {
    //var colIdx = 3;
    if (value) {
        App.grdEntryType.getStore().each(function (item) {

            item.set("active", value.checked);
        });
    }
};

var change = function (value) {
    var record = App.stoMasterType.findRecord("Code", value);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
    //var descr = "";
    //for (var i = 0; i < App.stoMasterType.getCount() ; i++) {
    //    if (App.stoMasterType.data.items[i].data.Code == value) {
    //        descr = App.stoMasterType.data.items[i].data.Descr;
    //        break;
    //    }
    //}
    //if (descr) {
    //    return descr;
    //}
    //else {
    //    return value;
    //}
}
/////////////////////////////////////////////////////////////////////////








