//// Declare //////////////////////////////////////////////////////////

var keys = ['BranchID','BankAcct'];
///////////////////////////////////////////////////////////////////////

//// Store /////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////


//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdAccount);
            break;
        case "prev":
            HQ.grid.prev(App.grdAccount);
            break;
        case "next":
            HQ.grid.next(App.grdAccount);
            break;
        case "last":
            HQ.grid.last(App.grdAccount);
            break;
        case "refresh":
            App.stoAccount.reload();
            HQ.grid.first(App.grdAccount);
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdAccount);
            }
            break;
        case "delete":
            if (App.slmAccount.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (checkRequire(App.stoAccount.getChangedData().Created) && checkRequire(App.stoAccount.getChangedData().Updated)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.store.isChange(App.stoAccount)) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};
var grdAccount_BeforeEdit = function (editor, e) {
    if (!HQ.isUpdate) return false;
    if (keys.indexOf(e.field) != -1) {
        if (e.record.data.tstamp != "")
            return false;
    }
    return HQ.grid.checkInput(e, keys);
};
var grdAccount_Edit = function (item, e) {

    if (keys.indexOf(e.field) != -1) {
        if (e.value != '' && isAllValidKey(App.stoAccount.getChangedData().Created) && isAllValidKey(App.stoAccount.getChangedData().Updated))
            HQ.store.insertBlank(App.stoAccount);
    }
};
var grdAccount_ValidateEdit = function (item, e) {
    if (keys.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(App.grdAccount, e, keys)) {
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

var grdAccount_CancelEdit = function (editor, e) {
    if (e.record.phantom) {
        e.store.remove(e.record);
    }
};

var grd_Reject = function (record) {
    if (record.data.tstamp == '') {
        App.stoAccount.remove(record);
        App.grdAccount.getView().focusRow(App.stoAccount.getCount() - 1);
        App.grdAccount.getSelectionModel().select(App.stoAccount.getCount() - 1);
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
            url: 'CA20200/Save',
            params: {
                lstAccount: HQ.store.getData(App.stoAccount)
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
        App.grdAccount.deleteSelected();
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
            if (HQ.grid.checkRequirePass(items[i])) continue;
            if (items[i]["BranchID"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("BranchID"));
                return false;
            }
         
            if (items[i]["BankAcct"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("BankAcct"));
                return false;
            }
            if (items[i]["AcctName"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("AcctName"));
                return false;
            }
            if (items[i]["AcctNbr"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("AcctNbr"));
                return false;
            }
            if (items[i]["AddrID"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("AddrID"));
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
        App.grdAccount.getStore().each(function (item) {

            item.set("Active", value.checked);
        });
    }
};
/////////////////////////////////////////////////////////////////////////








