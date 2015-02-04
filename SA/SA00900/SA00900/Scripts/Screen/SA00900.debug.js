//// Declare //////////////////////////////////////////////////////////

var keys = ['Code'];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdLanguage);
            break;
        case "prev":
            HQ.grid.prev(App.grdLanguage);
            break;
        case "next":
            HQ.grid.next(App.grdLanguage);
            break;
        case "last":
            HQ.grid.last(App.grdLanguage);
            break;
        case "refresh":
            App.stoLanguage.reload();
            HQ.grid.first(App.grdLanguage);
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdLanguage);
            }
            break;
        case "delete":
            if (App.slmLanguage.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (checkRequire(App.stoLanguage.getChangedData().Created) && checkRequire(App.stoLanguage.getChangedData().Updated)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.store.isChange(App.stoLanguage)) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};
var grdLanguage_BeforeEdit = function (editor, e) {
    if (!HQ.isUpdate) return false;
      
    if (keys.indexOf(e.field) != -1) {
        if (e.record.data.tstamp != "")
            return false;
    }
    return HQ.grid.checkInput(e,keys)
};
var grdLanguage_Edit = function (item, e) {
    if (keys.indexOf(e.field) != -1) {
        if (e.value != '' && isAllValidKey(App.stoLanguage.getChangedData().Created) && isAllValidKey(App.stoLanguage.getChangedData().Updated))
            HQ.store.insertBlank(App.stoLanguage);
    }
 
    
};
var grdLanguage_ValidateEdit = function (item, e) {
    if (keys.indexOf(e.field) != -1) {
        var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/
        if (!e.value.match(regex)) 
        {
            HQ.message.show(20140811, e.column.text);
            return false;
        }
        if (HQ.grid.checkDuplicateAll(App.grdLanguage, e, keys)) {
            HQ.message.show(1112, e.value);
            return false;
        }
       
    }
};
var grdLanguage_Reject = function (record) {
    if (record.data.tstamp == '') {
        App.stoLanguage.remove(record);
        App.grdLanguage.getView().focusRow(App.stoLanguage.getCount() - 1);
        App.grdLanguage.getSelectionModel().select(App.stoLanguage.getCount() - 1);
    } else {
        record.reject();
    }
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA00900/Save',
            params: {
                lstLanguage: HQ.store.getData(App.stoLanguage)
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
        App.grdLanguage.deleteSelected();
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
            if (HQ.grid.checkRequirePass(items[i],keys)) continue;
            if (items[i]["Code"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("Code"));
                return false;
            }
            if (items[i]["Lang00"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("Lang00"));
                return false;
            }
            if (items[i]["Lang01"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("Lang01"));
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








