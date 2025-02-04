﻿//// Declare //////////////////////////////////////////////////////////
var keys = ['SlsperId'];
var fieldsCheckRequire = ["SlsperId"];
var fieldsLangCheckRequire = ["SlsperId"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboZone.getStore().load(function () {
        App.cboTerritory.getStore().load(function () {
            App.cboDist.getStore().load(function () {
                HQ.common.showBusy(false, HQ.common.getLang("loadingData"));
            })
        })
    })
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdOM_FCS);
            break;
        case "prev":
            HQ.grid.prev(App.grdOM_FCS);
            break;
        case "next":
            HQ.grid.next(App.grdOM_FCS);
            break;
        case "last":
            HQ.grid.last(App.grdOM_FCS);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoOM_FCS.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                //HQ.grid.insert(App.grdOM_FCS, keys);
            }
            break;
        case "delete":
            if (App.slmOM_FCS.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoOM_FCS, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
var renderPosition = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboPositionOM23100_pcPosition.findRecord("Code", rec.data.Position);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};
var beforeSelectcombo = function () {
    loadSourceCombo();
};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoOM_FCS.reload();
};
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM23100');
    App.cboDist.setReadOnly(HQ.isChange);
    App.cboTerritory.setReadOnly(HQ.isChange);
    App.cboZone.setReadOnly(HQ.isChange);
    App.dateFcs.setReadOnly(HQ.isChange);
    App.btnSearch.setDisabled(HQ.isChange);
    //App.dateFcs.setDisabled(HQ.isChange);
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM23100');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            //HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
    HQ.common.showBusy(false);
    stoChanged(App.stoOM_FCS);
};
//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
var grdOM_FCS_BeforeEdit = function (editor, e) {
    // thang nho hon thi khong cho sua
    var d = new Date(App.dateFcs.getValue());

    if (d.getYear() > _dateServer.getYear()) {
        return HQ.grid.checkBeforeEdit(e, keys);
    }
    else if (d.getYear() == _dateServer.getYear()) {
        if (d.getMonth() < _dateServer.getMonth()) {
            return false;
        }
        else if (d.getMonth() >= _dateServer.getMonth()) {
            return HQ.grid.checkBeforeEdit(e, keys);
        }
    }
    else if (d.getYear() > _dateServer.getYear()) {
        return false;
    }
};
var grdOM_FCS_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdOM_FCS, e, keys);
    if (e.field == "SlsperId") {
        var selectedRecord = App.cboSlsperId.store.findRecord(e.field, e.value);
        if (selectedRecord) {
            e.record.set("Name", selectedRecord.data.Name);
        }
        else {
            e.record.set("Name", "");
        }
    }
};
var grdOM_FCS_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdOM_FCS, e, keys);
};
var grdOM_FCS_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_FCS);
    stoChanged(App.stoOM_FCS);
};
var btnSearch_Click = function (sender, e) {
    if (HQ.form.checkRequirePass(App.frmMain)) {
        HQ.isFirstLoad = true;
        App.grdOM_FCS.show();
        App.stoOM_FCS.reload();
    }
};
var dateFcs_expand = function (dte, eOpts) {
    dte.picker.setWidth(300);
    dte.picker.monthEl.setWidth(200);
};
var cboZone_Change = function (sender, e) {
    if (HQ.isChange) {
        HQ.message.show(20150303, '', 'refresh');
    }
    else {
        App.grdOM_FCS.store.removeAll();
        App.grdOM_FCS.hide();
        App.cboTerritory.store.load();
    }
};
var cboTerritory_Change = function (sender, e) {
    if (HQ.isChange) {
        HQ.message.show(20150303, '', 'refresh');
    }
    else {
        App.grdOM_FCS.store.removeAll();
        App.grdOM_FCS.hide();
        App.cboDist.store.load();
    }
};
var cboDist_Change = function (sender, e) {
    if (HQ.isChange) {
        HQ.message.show(20150303, '', 'refresh');
    }
    else {
        App.grdOM_FCS.store.removeAll();
        App.grdOM_FCS.hide();
        App.cboSlsperId.store.reload();
    }
};
var dateFcs_Change = function (sender, e) {
    if (HQ.isChange) {
        HQ.message.show(20150303, '', 'refresh');
    }
    else {
        App.grdOM_FCS.store.removeAll();
        App.grdOM_FCS.hide();
    }
};
var btnImport_Click = function (c, e) {
    var fileName = c.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (ext == "xls" || ext == "xlsx") {
        App.frmMain.submit({
            waitMsg: HQ.waitMsg,
            clientValidation: false,
            method: 'POST',
            url: 'OM23100/Import',
            timeout: 1000000,           
            success: function (msg, data) {
                if (!Ext.isEmpty(this.result.message)) {
                    HQ.message.show('2013103001', [this.result.message], '', true);
                }
                else {
                    HQ.message.process(msg, data, true);                    
                }                           
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
                App.btnImport.reset();
            }
        });
    } else {
        HQ.message.show('2014070701', [ext], '', true);
        App.btnImport.reset();
    }
};
var btnExport_Click = function () {  
    App.frmMain.submit({
        //waitMsg: HQ.common.getLang("Exporting"),
        url: 'OM23100/Export',
        type: 'POST',
        timeout: 1000000,
        clientValidation: false,
        params: {
            type: 'O',
        },
        success: function (msg, data) {          
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    //HQ.isFirstLoad = true;
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'OM23100/Save',
            params: {
                //lstOM_FCS: Ext.HQ.store.getData(App.stoOM_FCS)
                lstOM_FCS: Ext.encode(App.stoOM_FCS.getChangedData({ skipIdForPhantomRecords: false })),
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoOM_FCS.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};
var deleteData = function (item) {
    if (item == "yes") {
        App.grdOM_FCS.deleteSelected();
        stoChanged(App.stoOM_FCS);
    }
};
/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoOM_FCS.reload();
    }
};
////////////////////////////////////////////////////////////////////////