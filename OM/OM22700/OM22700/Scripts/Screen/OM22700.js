//// Declare //////////////////////////////////////////////////////////
var msgError = '';
var keys = ['SlsFreqID', 'WeekofVisit'];
var fieldsCheckRequire = ["SlsFreqID", "WeekofVisit"];
var fieldsLangCheckRequire = ["SlsFreqID", "WeekofVisit"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdOM_WeekOfVisit);
            break;
        case "prev":
            HQ.grid.prev(App.grdOM_WeekOfVisit);
            break;
        case "next":
            HQ.grid.next(App.grdOM_WeekOfVisit);
            break;
        case "last":
            HQ.grid.last(App.grdOM_WeekOfVisit);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoOM_WeekOfVisit.reload();
            }          
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdOM_WeekOfVisit, keys);
            }
            break;
        case "delete":
            if (App.slmOM_WeekOfVisit.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoOM_WeekOfVisit, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    App.stoOM_WeekOfVisit.reload();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM22700');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM22700');
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
var grdOM_WeekOfVisit_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdOM_WeekOfVisit_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdOM_WeekOfVisit, e, keys);
};
var grdOM_WeekOfVisit_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdOM_WeekOfVisit, e, keys);
};
var grdOM_WeekOfVisit_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_WeekOfVisit);
    stoChanged(App.stoOM_WeekOfVisit);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (CheckData()) {
        HQ.message.show(201302071,[msgError.slice(0,-2)],'',true);
    }
    else {
        if (App.frmMain.isValid()) {
            App.frmMain.submit({
                timeout: 1800000,
                waitMsg: HQ.common.getLang("SavingData"),
                url: 'OM22700/Save',
                params: {
                    lstOM_WeekOfVisit: HQ.store.getData(App.stoOM_WeekOfVisit)
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
    }
};

var CheckData = function () {
    msgError = '';
    App.grdOM_WeekOfVisit.getStore().each(function (item) {
        var total = item.data.SlsFreqID =='F8'?2:3;
        var count = 0;
        if (item.data.SlsFreqID != '' && item.data.WeekofVisit != '') {
            if (item.data.Mon)
                count++;
            if (item.data.Tue)
                count++;
            if (item.data.Wed)
                count++;
            if (item.data.Thu)
                count++;
            if (item.data.Fri)
                count++;
            if (item.data.Sat)
                count++;
            if (item.data.Sun)
                count++;

            if (count != total)
                msgError += item.raw.$id + ', ';
        }
    });
    if (msgError)
        return true;
    else
        return false;
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdOM_WeekOfVisit.deleteSelected();
        stoChanged(App.stoOM_WeekOfVisit);
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoOM_WeekOfVisit.reload();
    }
};
///////////////////////////////////








