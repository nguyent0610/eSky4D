//// Declare //////////////////////////////////////////////////////////

var keys = ['InvtID'];
var fieldsCheckRequire = ["InvtID", "Date"];
var fieldsLangCheckRequire = ["InvtID", "Date"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdData);
            break;
        case "prev":
            HQ.grid.prev(App.grdData);
            break;
        case "next":
            HQ.grid.next(App.grdData);
            break;
        case "last":
            HQ.grid.last(App.grdData);
            break;
        case "refresh":
	
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoData.reload();
            }          
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdData, keys);
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
                if (HQ.store.checkRequirePass(App.stoData, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            HQ.common.close(this);            
            break;
    }

};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoData.reload();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN21500');
};
var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoData);
    HQ.common.changeData(HQ.isChange, 'IN21500');
}
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN21500');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
};
//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
var grdData_BeforeEdit = function (editor, e) {
    if (e.field == 'Date' && e.record.data.Date) return false;
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdData_Edit = function (item, e) {
    if (e.field == 'InvtID') {
        var objInvtID = HQ.store.findRecord(App.cboInvtID.getStore(), ["InvtID"], [e.record.data.InvtID]);
        if (objInvtID) {
            e.record.set('Descr', objInvtID.data.Descr);
        }        
    }
    HQ.grid.checkInsertKey(App.grdData, e, ["InvtID"]);
    frmChange();
};
var grdData_ValidateEdit = function (item, e) {
    if (e.field == 'Date') {

        var yyyy =  e.value.getFullYear().toString();
        var mm = ( e.value.getMonth() + 1).toString(); // getMonth() is zero-based
        var dd =  e.value.getDate().toString();
        var date = (mm[1] ? mm : "0" + mm[0]) + "/" + yyyy;

        if (HQ.grid.checkDuplicate(App.grdData, e, ["InvtID","Date"])) {
            HQ.message.show(1112, datess);
            return false;
        }
    }       
};
var grdData_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdData);
    frmChange();
}
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'IN21500/Save',
            params: {
                lstData: HQ.store.getData(App.stoData)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isChange = false;
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
        App.grdData.deleteSelected();
        stoChanged(App.stoData);
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoData.reload();
    }
};
///////////////////////////////////








