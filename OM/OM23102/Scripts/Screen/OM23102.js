//// Declare //////////////////////////////////////////////////////////
var keys = ['SlsperId', 'CustID'];
var fieldsCheckRequire = ["SlsperId", 'CustID'];
var fieldsLangCheckRequire = ["SlsperId", 'CustID'];
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
            HQ.grid.first(App.grdOM_PG_FCS);
            break;
        case "prev":
            HQ.grid.prev(App.grdOM_PG_FCS);
            break;
        case "next":
            HQ.grid.next(App.grdOM_PG_FCS);
            break;
        case "last":
            HQ.grid.last(App.grdOM_PG_FCS);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoOM_PG_FCS.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdOM_PG_FCS, keys);
            }
            break;
        case "delete":
            if (App.slmOM_PG_FCS.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoOM_PG_FCS, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    var record = App.cboPositionOM23102_pcPosition.findRecord("Code", rec.data.Position);
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
    App.stoOM_PG_FCS.reload();
};
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM23102');
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
    HQ.common.changeData(HQ.isChange, 'OM23102');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
    HQ.common.showBusy(false);
    stoChanged(App.stoOM_PG_FCS);
};
//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
var grdOM_PG_FCS_BeforeEdit = function (editor, e) {
    // thang nho hon thi khong cho sua
    if (e.field =='Position')
        return false;
    var d = new Date(App.dateFcs.getValue());

    if (d.getYear() > _dateServer.getYear()) {
        if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
        //return true;
    }
    else if (d.getYear() == _dateServer.getYear()) {
        if (d.getMonth() < _dateServer.getMonth()) {
            return false;
        }
        else if (d.getMonth() >= _dateServer.getMonth()) {
            if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
            //return true;
        }
    }
    else if (d.getYear() > _dateServer.getYear()) {
        return false;
    }
};
var grdOM_PG_FCS_Edit = function (item, e) {
    if (e.field == 'SlsperId') {
        if (e.value) {
            var recordT = App.cboSlsperIdOM23102_pcSalesPerson.findRecord("SlsperId", e.value);
            if (recordT) {
                e.record.set('Name', recordT.data.Name);
                e.record.set('Position', recordT.data.Position);
            }
            else {
                e.record.set('Name', '');
                e.record.set('Position', '');
            }
        }
    }
    else if (e.field == 'CustID') {
        if (e.value) {
            var recordT = App.cboCustIDOM23102_pcCustID.findRecord("CustId", e.value);
            if (recordT) {
                e.record.set('CustName', recordT.data.CustName);
            }
            else {
                e.record.set('CustName', '');
            }
        }
    }
    HQ.grid.checkInsertKey(App.grdOM_PG_FCS, e, keys);
    //if (e.field == "SlsperId") {
    //    var selectedRecord = App.cboSlsperId.store.findRecord(e.field, e.value);
    //    if (selectedRecord) {
    //        e.record.set("Name", selectedRecord.data.Name);
    //    }
    //    else {
    //        e.record.set("Name", "");
    //    }
    //}
};
var grdOM_PG_FCS_ValidateEdit = function (item, e) {
    return checkValidateEdit(App.grdOM_PG_FCS, e, keys);
};

var checkValidateEdit = function (grd, e, keys) {
    if (keys.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(grd, e, keys)) {
            HQ.message.show(1112, e.value);
            return false;
        }
    }
};

var grdOM_PG_FCS_Reject = function (record) {
    //HQ.grid.checkReject(record, App.grdOM_PG_FCS);
    //stoChanged(App.stoOM_PG_FCS);
};
var btnSearch_Click = function (sender, e) {
    if (HQ.form.checkRequirePass(App.frmMain)) {
        HQ.isFirstLoad = true;
        App.grdOM_PG_FCS.show();
        App.stoOM_PG_FCS.reload();
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
        App.grdOM_PG_FCS.store.removeAll();
        App.grdOM_PG_FCS.hide();
        App.cboTerritory.store.load();
    }
};
var cboTerritory_Change = function (sender, e) {
    if (HQ.isChange) {
        HQ.message.show(20150303, '', 'refresh');
    }
    else {
        App.grdOM_PG_FCS.store.removeAll();
        App.grdOM_PG_FCS.hide();
        App.cboDist.store.load();
    }
};
var cboDist_Change = function (sender, e) {
    if (e) {
        App.cboSlsperId.store.reload();
        App.cboCustID.store.reload();
    }
    if (HQ.isChange) {
        HQ.message.show(20150303, '', 'refresh');
    }
    else {
        //App.grdOM_PG_FCS.store.removeAll();
        App.grdOM_PG_FCS.hide();
        //App.cboSlsperId.store.reload();
    }
};
var cboDist_Select = function (sender, e) {
    if (e) {
        App.cboSlsperId.store.reload();
        App.cboCustID.store.reload();
    }
    if (HQ.isChange) {
        HQ.message.show(20150303, '', 'refresh');
    }
    else {
        //App.grdOM_PG_FCS.store.removeAll();
        App.grdOM_PG_FCS.hide();
        //App.cboSlsperId.store.reload();
    }
};

var dateFcs_Change = function (sender, e) {
    if (HQ.isChange) {
        HQ.message.show(20150303, '', 'refresh');
    }
    else {
        App.grdOM_PG_FCS.store.removeAll();
        App.grdOM_PG_FCS.hide();
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
            url: 'OM23102/Import',
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
        url: 'OM23102/Export',
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
            url: 'OM23102/Save',
            params: {
                lstOM_PG_FCS: HQ.store.getData(App.stoOM_PG_FCS)
                //lstOM_PG_FCS: Ext.encode(App.stoOM_PG_FCS.getChangedData({ skipIdForPhantomRecords: false })),
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoOM_PG_FCS.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};
var deleteData = function (item) {
    if (item == "yes") {
        App.grdOM_PG_FCS.deleteSelected();
        stoChanged(App.stoOM_PG_FCS);
    }
};
/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoOM_PG_FCS.reload();
    }
};
////////////////////////////////////////////////////////////////////////