//// Declare //////////////////////////////////////////////////////////

var keys = ['Code'];
///////////////////////////////////////////////////////////////////////

//// Store /////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////


//// Event /////////////////////////////////////////////////////////////
   
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSellingProducts);
            break;
        case "prev":
            HQ.grid.prev(App.grdSellingProducts);
            break;
        case "next":
            HQ.grid.next(App.grdSellingProducts);
            break;
        case "last":
            HQ.grid.last(App.grdSellingProducts);
            break;
        case "refresh":
            App.stoSellingProducts.reload();
            HQ.grid.first(App.grdSellingProducts);
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdSellingProducts);
            }
            break;
        case "delete":
            if (App.slmSellingProducts.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (checkRequire(App.stoSellingProducts.getChangedData().Created) && checkRequire(App.stoSellingProducts.getChangedData().Updated)) {
                    save();
                }
            }
            break;
        case "print":
            alert(command);
            break;
        case "close":
            if (HQ.store.isChange(App.stoSellingProducts)) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }
};
// Danh cho grid ///////////////////////////////////////////////////////
var grdSellingProducts_BeforeEdit = function (editor, e) {
    if (!HQ.isUpdate) return false;  
    if (keys.indexOf(e.field) != -1) {
        if (e.record.data.tstamp != "")
            return false;
    }
    return HQ.grid.checkInput(e, keys);
};
var grdSellingProducts_Edit = function (item, e) {

    if (keys.indexOf(e.field) != -1) {
        if (e.value != '' && isAllValidKey(App.stoSellingProducts.getChangedData().Created) && isAllValidKey(App.stoSellingProducts.getChangedData().Updated))
            HQ.store.insertBlank(App.stoSellingProducts);
    }
};
var grdSellingProducts_ValidateEdit = function (item, e) {
    if (keys.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(App.grdSellingProducts, e, keys)) {
            HQ.message.show(1112, e.value);
            return false;
        }
        else
            return true;
    }
};
var grdSellingProducts_CancelEdit = function (editor, e) {
    if (e.record.phantom) {
        e.store.remove(e.record);
    }
};
var grdSellingProducts_Reject = function (record) {
    if (record.data.tstamp == '') {
        App.stoSellingProducts.remove(record);
        App.grdSellingProducts.getView().focusRow(App.stoSellingProducts.getCount() - 1);
        App.grdSellingProducts.getSelectionModel().select(App.stoSellingProducts.getCount() - 1);
    } else record.reject();
};
/////////////////////////////////////////////////////////////////////////

// Process Data ////////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'AR21400/Save',
            params: {
                lstgrd: HQ.store.getData(App.stoSellingProducts)
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
        App.grdSellingProducts.deleteSelected();
    }
};
/////////////////////////////////////////////////////////////////////////

// Kiem tra nhung field yeu cau bat buoc nhap //////////////////////////
var checkRequire = function (items) {
    if (items != undefined) {
        for (var i = 0; i < items.length; i++) {
            if (HQ.grid.checkRequirePass(items[i], keys)) continue;
            if (items[i]["Code"].trim() == "") {
                callMessage(15, HQ.common.getLang("Code"));
                return false;
            }
            if (items[i]["Descr"].trim() == "") {
                callMessage(15, HQ.common.getLang("Descr"));
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



