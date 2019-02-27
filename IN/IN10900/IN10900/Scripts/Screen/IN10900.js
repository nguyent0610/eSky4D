//// Declare //////////////////////////////////////////////////////////
var keys = ['PosmID', 'BranchID', 'ClassID', 'InvtID', 'SiteID', 'Date'];
//var keys = ['PosmID', 'BranchID', 'ClassID', 'InvtID', 'SiteID'];
var fieldsCheckRequire = ["PosmID", "BranchID", "ClassID", "InvtID", "SiteID", "Date", "FCS"];
var fieldsLangCheckRequire = ["ProcID", "BranchID", "ClassID", "InvtID", "SiteId", "ExpDate", "FCS"];
var _posmID = '';
var _branchID = '';
var _classID = '';
var _siteId = '';
var _invtID = '';

/////////////Store/////////////////
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoData_changed = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN10900');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoData_load = function (sto) {  
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'IN10900');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
         //   HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
  //  checkReadOnlyProgType();
};
checkHasData= function () {
    var hasData = true;
    if (App.cboHandle.getValue() == 'R') {
        var countselect = 0;
        App.stoData.each(function (record) {
            if (record.data.Selected == 1) {
                countselect++;
                return hasData;
            }
        });
        if (countselect == 0) {
            hasData = false;
        }
    }
    else {
        hasData = false;
    }

    return hasData;
}

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
                HQ.grid.insert(App.grdDet, keys);
            }
            break;
        case "delete":
            if (App.slmData.selected.items[0] != undefined) {
                var rowindex = HQ.grid.indexSelect(App.grdDet);
                if (rowindex != '')
                    HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdDet), ''], 'deleteData', true)
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain) &&
                    HQ.store.checkRequirePass(App.stoData, keys, fieldsCheckRequire, fieldsLangCheckRequire)
                    && checkHasData()
                    ) {
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
////load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    //HQ.util.checkAccessRight();
    App.frmMain.isValid();
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
    App.cboHandle.store.reload();
    App.cboHandle.setValue("N");

    App.stoData.reload();
};
var grdDet_BeforeEdit = function (editor, e) {
    if (HQ.isUpdate == false && HQ.isInsert == false) {
        return false;
    }

};
var grdDet_Edit = function (item, e) {
    if (e.field == 'PosmID') {
        if (Ext.isEmpty(e.record.data.PosmID)) {
            e.record.set('BranchID', '');
            e.record.set('CpnyName', '');
            e.record.set('SiteID', '');
        }
    }
    if (e.field == 'BranchID') {
        if (Ext.isEmpty(e.record.data.BranchID)) {
            e.record.set('CpnyName', '');
            e.record.set('SiteID', '');
        }
    }
    if (e.field == 'ClassID') {
        if (Ext.isEmpty(e.record.data.ClassID)) {
            e.record.set('InvtID', '');
            e.record.set('Descr', '');
        }
    }
    if (e.field == 'BranchID') {
        //Ten combo + ten proceduce --> lay duoc data cua combo do
        var obj = App.cboBranchIDIN10900_pcBranchID.findRecord('BranchID', e.value);
        if (obj) {
            e.record.set('CpnyName', obj.data.CpnyName);            
        }
    }
    if (e.field == 'InvtID') {
        //Ten combo + ten proceduce --> lay duoc data cua combo do
        var obj = App.cboInvtIDIN10900_pcInvtID.findRecord('InvtID', e.value);
        if (obj) {
            e.record.set('Descr', obj.data.Descr);
        }
    }
   
    HQ.grid.checkInsertKey(App.grdDet, e, keys);
    stoData_changed(App.stoData);
    //checkReadOnlyProgType();
};
var grdDet_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdDet, e, keys, false);
 
};
var grdDet_Reject = function (record) {
    var progType = App.cboProgID.getValue();
    if (record.data.BranchID != '' && record.data.ClassID != '' && record.data.SiteID != ''
        && (progType == 'D4' || progType == 'D3' && record.data.InvtID != '' && record.data.Date))
    {
        HQ.grid.checkReject(record, App.grdDet);
    }
    else
    {
        record.set('PosmID', '');
        record.set('BranchID', '');
        record.set('CpnyName', '');
        record.set('ClassID', '');       
        record.set('InvtID', '');
        record.set('Descr', '');
        record.set('SiteID', '');
        record.set('Date', '');
       // record.set('Qty', '');
    }
    //checkReadOnlyProgType();
};
// 
var cboCpnyID_Change = function (sender, value) {
    if (sender.valueModels && sender.valueModels[0]) {
        _posmID = value;
        //App.dteFromDate.setValue(sender.valueModels[0].data.FromDate);
        //App.dteToDate.setValue(sender.valueModels[0].data.ToDate);
        App.stoData.reload();
    } else {
        _posmID = '';
        App.cboSlsPerID.setValue('');
        //App.dteFromDate.setValue('');
        //App.dteToDate.setValue('');
        App.stoData.clearData();
        App.grdDet.view.refresh();
    //    checkReadOnlyProgType();
    //    HQ.store.insertBlank(App.stoData, keys);
    }
    //App.cboBranchID.getStore().reload();
    //if (sender.valueModels && sender.valueModels[0]) {
    //    _posmID = value;
    //    App.cboBranchID.getStore().reload();
    //} else {
    //    _posmID = '';
    //    App.cboBranchID.getStore().reload();
    //}
};
// cboPosmID_Select
var cboCpnyID_Select = function (sender, value) {
    HQ.isChange = false;
    HQ.isFirstLoad = true;
    App.cboSlsPerID.store.reload();
    App.stoData.reload();
    //if (!App.cboBranchID.getStore().loading) {
    //    if (sender.valueModels && sender.valueModels[0]) {
    //        _posmID = sender.valueModels[0].data.CpnyID;
    //     //   App.cboProgID.setValue(sender.valueModels[0].data.ProgTypeFCS);
    //        App.dteFromDate.setValue(sender.valueModels[0].data.FromDate);
    //        App.dteToDate.setValue(sender.valueModels[0].data.ToDate);
    //        App.stoData.reload();
    //    } else {
    //        _posmID = '';
    //        App.cboSlsPerID.setValue('');
    //        App.dteFromDate.setValue('');
    //        App.dteToDate.setValue('');
    //        App.stoData.clearData();
    //        App.grdDet.view.refresh();
    //        //();
    //        HQ.store.insertBlank(App.stoData, keys);
    //    }
    //    //  App.cboBranchID.getStore().reload();
    //}
    //if (!App.cboBranchID.getStore().loading) {
    //    if (sender.valueModels && sender.valueModels[0]) {
    //        _posmID = sender.valueModels[0].data.PosmID;
    //    } else {
    //        _posmID = '';
    //    }
    //    App.cboBranchID.getStore().reload();
    //}    
};

// cboProgID_Change change
var cboSlsPerID_Change = function (sender, value) {
    //App.stoData.reload();
    //if (value == "D3") {
    //    HQ.grid.show(App.grdDet, ['InvtID', 'Descr', 'Date']);
    //    keys = ['BranchID', 'ClassID', 'SiteID', 'InvtID', 'Date'];
    //    fieldsCheckRequire = ["BranchID", "ClassID", "SiteID", "InvtID", "Date"];
    //    fieldsLangCheckRequire = ["BranchID", "ClassID", "SiteId", "InvtID", "ExpDate"];
    //} else if (value == "D4") {
    //    HQ.grid.hide(App.grdDet, ['InvtID', 'Descr', 'Date']);
    //    keys = ['BranchID', 'ClassID', 'SiteID'];
    //    fieldsCheckRequire = ["BranchID", "ClassID", "SiteID"];
    //    fieldsLangCheckRequire = ["BranchID", "ClassID", "SiteId"];
    //}
};

var txtFromDate_Change = function () {
    if (App.dteFromDate.isValid() &&  App.dteToDate.isValid())
    App.stoData.reload();
}

var AdjustedCheckAll_Change = function (value) {
    //var colIdx = 3;
    if (value) {
        App.grdDet.getStore().each(function (item) {
            //vong if nay de check neu 1 so o da duoc check hay bi bo ko dong bo voi cac o con lai
            if ((value.checked == true && this.data.Selected == false) || (value.checked == false && this.data.Selected == true)) {
                item.set("Selected", value.checked);
             //   AdjustedCheckEveryRow_Change(0, 0, item);
            }

        });

    }
};


///////////////////////////////////////////////////////////////////////////
////// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'IN10900/Save',
            params: {
                lstData: HQ.store.getData(App.stoData)
            },
            success: function (msg, data) {
                HQ.message.process(msg, data, true);
               // App.cboPosmID.store.reload();
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
        App.grdDet.deleteSelected();
        stoData_changed(App.stoData);
        //checkReadOnlyProgType();
    }
};
var refresh = function (item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoData.reload();       
    }
}
// 


//// Import data
var ImportData = function () {
    try {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("Importing"),
            url: 'IN10900/Import',
            timeout: 1800000,
            clientValidation: false,
            method: 'POST',
            params: {
                //lstDetails: HQ.store.getAllData(App.stoKPISalespersonCheck)
            },
            success: function (msg, data) {
                if (!Ext.isEmpty(this.result.msgCode)) {
                    HQ.message.show(this.result.msgCode, '', '', true);
                }
                else {
                    HQ.message.process(msg, data, true);
                }
                refresh('yes');
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
                App.btnImport.reset();
            }
        });
    } catch (ex) {
        alert(ex.message);
    }
};


function btnLoad_click() {
    if (HQ.form.checkRequirePass(App.frmMain)) {
        HQ.common.showBusy(true);
        refresh('yes');
    }
}