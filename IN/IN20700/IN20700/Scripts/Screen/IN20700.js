//////////Declare/////////////////
var keys = ['ReasonCD'];
var fieldsCheckRequire = ["ReasonCD"];
var fieldsLangCheckRequire = ["ReasonCD"];
///////////Store/////////////////
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoData_changed = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN20700');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoData_load = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN20700');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
};
//trước khi load trang busy la dang load data
var stoData_beforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
/////////////////////////////////
////////////Event/////////////////
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdDet);
            break;
        case "prev":
            HQ.grid.prev(App.grdDet);
            break;
        case "next":
            HQ.grid.next(App.grdDet);
            break;
        case "last":
            HQ.grid.last(App.grdDet);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                refresh('yes');
            }
            break;           
        case "new":
            if (HQ.isInsert) {
                App.grdDet.filters.clearFilters();
                if (!checkDuplicateRow) {
                    HQ.grid.insert(App.grdDet);
                } else {
                    return false
                }
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmData.selected.items[0] != undefined) {
                    var rowindex = HQ.grid.indexSelect(App.grdDet);
                    if (rowindex != '')
                        HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdDet), ''], 'deleteData', true)
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
    }

};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    if (HQ.isInsert == false) {
        App.menuClickbtnNew.disable();
    }
    if (HQ.isDelete == false) {
        App.menuClickbtnDelete.disable();
    }
    if (HQ.isUpdate == false && HQ.isInsert == false && HQ.isDelete == false) {
        App.menuClickbtnSave.disable();
    }
    if (HQ.isShowSiteID) {    
            HQ.grid.show(App.grdDet,['SiteID'])
         
    } else {
        HQ.grid.hide(App.grdDet, ['SiteID'])
    }
    if (HQ.isShowSlsperID) {

        HQ.grid.show(App.grdDet, ['SlsperID'])

    } else {
        HQ.grid.hide(App.grdDet, ['SlsperID'])
    }
    App.stoData.reload();
}
var grdDet_BeforeEdit = function (editor, e) {
    if (HQ.isUpdate == false && HQ.isInsert == false)
    {
        return false;
    }
    else {
        return HQ.grid.checkBeforeEdit(e, keys);
    }
    //return HQ.grid.checkBeforeEdit(e, keys);
};
var grdDet_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdDet, e, keys);
    //if (e.field == 'ReasonCD') {
    //    if (e.record.data.ReasonCD != "") {
    //        e.record.set();
    //    }

    //}
};
var grdDet_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdDet, e, keys);
};
var grdDet_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdDet);
    stoData_changed(App.stoData);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'IN20700/Save',
            params: {
                lstData: HQ.store.getData(App.stoData)
            },
            success: function (msg, data) {
                HQ.message.process(msg, data, true);
                refresh('yes');
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};
var deleteData = function (item) {
    if (item == "yes") {
        //alert(App.stoData);
        var reasonCD = '';
        var selectedRow = App.grdDet.selModel.selected.items.length;
        for (var idx = selectedRow -1; idx >=0; idx --)
        {
            reasonCD += App.grdDet.selModel.selected.items[idx].data.ReasonCD + ",";
        }
        if (App.frmMain.isValid() && reasonCD != '') {
            //App.frmMain.updateRecord();
            App.frmMain.submit({
                timeout: 1800000,
                waitMsg: HQ.common.getLang("DeleteData"),
                url: 'IN20700/Delete',
                params: {
                    reasonCD: reasonCD
                },
                success: function (msg, data) {
                    App.grdDet.deleteSelected();
                    stoData_changed(App.stoData);
                    
                },
                failure: function (msg, data) {
                    HQ.message.show(18, true);
                    
                }
            });
        }
        
    }
};
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoData.reload();
    }
};

var checkDuplicateRow = function (grd, row, keys) {
    var found = false;
    var store = grd.getStore();
    if (keys == undefined) keys = row.record.idProperty.split(',');
    var allData = store.data;
    for (var i = 0; i < allData.items.length; i++) {
        var record = allData.items[i];
        var data = '';
        var rowdata = '';
        for (var jkey = 0; jkey < keys.length; jkey++) {
            if (record.data[keys[jkey]] != undefined) {
                data += record.data[keys[jkey]].toString().toLowerCase() + ',';
                if (row.field == keys[jkey])
                    rowdata += (row.value == null ? "" : row.value.toString().toLowerCase()) + ',';
                else
                    rowdata += row.record.data[keys[jkey]].toString().toLowerCase() + ',';
            }
        }
        if (found = (data == rowdata && record.id != row.record.id) ? true : false) {
            break;
        };
    }

    return found;
};