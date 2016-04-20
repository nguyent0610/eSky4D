//// Declare //////////////////////////////////////////////////////////
var keys = ['PosmID', 'BranchID', 'CustId', 'ClassID', 'SiteID', 'InvtID', 'Date'];
var fieldsCheckRequire = ["PosmID", "BranchID", "CustId", "ClassID", "SiteID", "InvtID", "Date", "Qty"];
var fieldsLangCheckRequire = ["ProcID", "BranchID", "CustId", "ClassID", "SiteId", "InvtID", "ExpDate", "Qty"];
var _posmID = '';
var _branchID = '';
var _classID = '';
var _siteId = '';
var _invtID = '';

/////////////Store/////////////////
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoData_changed = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM23600');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoData_load = function (sto) {  
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM23600');
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
////load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    //HQ.util.checkAccessRight();
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
    App.stoData.reload();
};
var grdDet_BeforeEdit = function (editor, e) {
    //if (_posmID == e.record.data.PosmID)
    //{
    //    //e.record.set('BranchID', '');
    //    App.cboBranchID.store.reload();
      
    //    //App.cboBranchID.setvalue('');
    //}
    _posmID = e.record.data.PosmID;
    App.cboBranchID.store.reload();
    _classID = e.record.data.ClassID;
    App.cboClassID.store.reload();
    _siteId = e.record.data.SiteID;
    App.cboSiteId.store.reload();
    //_classID = e.record.data.ClassID;
    //App.cboInvtID.store.reload();
    if (e.field == 'SiteID') {
        if (Ext.isEmpty(e.record.data.BranchID)) {
            App.cboSiteId.getStore().removeAll();

        }
        else {
            App.cboSiteId.store.reload();
        }
    }
    if (HQ.isUpdate == false && HQ.isInsert == false) {
        return false;
    }
    else {
        return HQ.grid.checkBeforeEdit(e, keys);
    }
    //return HQ.grid.checkBeforeEdit(e, keys);
};
var grdDet_Edit = function (item, e) {
    if (e.field == 'PosmID')
    {
        if (Ext.isEmpty(e.record.data.PosmID) )
        {
            e.record.set('BranchID', '');
            e.record.set('CustId', '');
            e.record.set('CustName', '');
            e.record.set('SiteID', '');
        }
    }
    if (e.field == 'BranchID') {
        if (Ext.isEmpty(e.record.data.BranchID)) {
            e.record.set('CustId', '');
            e.record.set('CustName', '');
            e.record.set('SiteID', '');
           
        }
    }
    if (e.field == 'ClassID') {
        if (Ext.isEmpty(e.record.data.BranchID)) {
            e.record.set('InvtID', '');
            e.record.set('Descr', '');
  
        }
    }
    if (e.field == 'SiteID' || e.field == 'InvtID') {
        if (Ext.isEmpty(e.record.data.BranchID)) {
            e.record.set('Date', '');
           
        }
    }
    if (e.field == 'CustId') {
        //Ten combo + ten proceduce --> lay duoc data cua combo do
        var obj = App.cboCustIdOM23600_pcCustId.findRecord('CustId', e.value);
        if (obj) {
            e.record.set('CustName', obj.data.CustName);
            
        }
    }
    if (e.field == 'InvtID') {
        //Ten combo + ten proceduce --> lay duoc data cua combo do
        var obj = App.cboInvtIDOM23600_pcInvtID.findRecord('InvtID', e.value);
        if (obj) {
            e.record.set('Descr', obj.data.Descr);

        }
    }
    //if (e.field == 'PosmID') {
    //    if (e.record.data.CpnyID == "") {
    //        //e.record.set('BranchID', '');
    //        return false;
    //    }

    //}
    HQ.grid.checkInsertKey(App.grdDet, e, keys);
    stoData_changed(App.stoData);
};
var grdDet_ValidateEdit = function (item, e) {
    if (e.field == 'Date'
        && e.value
        && e.record) {
        e.record.set('Date', Ext.Date.parse(e.value, HQ.formatDate));
        //Ze.record.data.Date = Ext.Date.parse(e.value, HQ.formatDate);
    }     
     return HQ.grid.checkValidateEdit(App.grdDet, e, keys, false);
 
};
var grdDet_Reject = function (record) {
    if (record.data.PosmID != '' && record.data.BranchID != ''
        && record.data.CustId != '' && record.data.ClassID != ''
        && record.data.SiteID != '' && record.data.InvtID != ''
        && record.data.Date != '')
    {
        HQ.grid.checkReject(record, App.grdDet);
    }
    else
    {
        record.set('PosmID', '');
        record.set('BranchID', '');
        record.set('CustId', '');
        record.set('CustName', '');
        record.set('ClassID', '');
        record.set('SiteID', '');
        record.set('InvtID', '');
        record.set('Descr', '');
        record.set('Date', '');
    }
};
// cboPosmID_Change 
var cboPosmID_Change = function (sender, value) {
    if (sender.valueModels && sender.valueModels[0]) {
        _posmID = value;
        App.cboBranchID.getStore().reload();
    } else {
        _posmID = '';
        App.cboBranchID.getStore().reload();
    }
};
// cboPosmID_Select
var cboPosmID_Select = function (sender, value) {
    if (!App.cboBranchID.getStore().loading) {
        if (sender.valueModels && sender.valueModels[0]) {
            _posmID = sender.valueModels[0].data.PosmID;
        } else {
            _posmID = '';
        }
        App.cboBranchID.getStore().reload();
    }    
};
// cboBranchID Change
var cboBranchID_Change = function (sender, value) {
    if (sender.valueModels && sender.valueModels[0] ) {
        _branchID = value;
        App.cboCustId.getStore().reload();
        App.cboSiteId.getStore().reload();
    } else {
        _branchID = '';
        App.cboCustId.getStore().reload();
        App.cboSiteId.getStore().reload();
    }
};
// cboBranchID Select
var cboBranchID_Select = function (sender, value) {
    if (!App.cboCustId.getStore().loading) {
        if (sender.valueModels && sender.valueModels[0]) {
            _branchID = sender.valueModels[0].data.BranchID;
        } else {
            _branchID = '';            
        }
        App.cboCustId.getStore().reload();
    }
};
//cboClassID_Change
var cboClassID_Change = function (sender, value) {
    if (sender.valueModels && sender.valueModels[0]) {
        _classID = value;
        App.cboInvtID.getStore().reload();
    } else {
        _classID = '';
        App.cboInvtID.getStore().reload();
    }
};
// cboClassID_Select
var cboClassID_Select = function (sender, value) {
    if (!App.cboInvtID.getStore().loading) {
        if (sender.valueModels && sender.valueModels[0]) {
            _classID = sender.valueModels[0].data.ClassID;
        } else {
            _classID = '';
        }
        App.cboInvtID.getStore().reload();
    }
};
//cboSiteId_Change
var cboSiteId_Change = function (sender, value) {
    if (sender.valueModels && sender.valueModels[0]) {
        _siteId = value;
        App.cboExpDate.getStore().reload();
    } else {
        _siteId = '';
        App.cboExpDate.getStore().reload();
    }
};
// cboSiteId_Select
var cboSiteId_Select = function (sender, value) {
    if (!App.cboExpDate.getStore().loading) {
        if (sender.valueModels && sender.valueModels[0]) {
            _siteId = sender.valueModels[0].data.SiteID;
        } else {
            _siteId = '';
        }
        App.cboExpDate.getStore().reload();
    }
};
//cboInvtID_Change
var cboInvtID_Change = function (sender, value) {
    if (sender.valueModels && sender.valueModels[0]) {
        _invtID = value;
        App.cboExpDate.getStore().reload();
    } else {
        _invtID = '';
        App.cboExpDate.getStore().reload();
    }
};
// cboInvtID_Select
var cboInvtID_Select = function (sender, value) {
    if (!App.cboExpDate.getStore().loading) {
        if (sender.valueModels && sender.valueModels[0]) {
            _invtID = sender.valueModels[0].data.InvtID;
        } else {
            _invtID = '';
        }
        App.cboExpDate.getStore().reload();
    }
};


///////////////////////////////////////////////////////////////////////////
////// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'OM23600/Save',
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
        App.grdDet.deleteSelected();
        stoData_changed(App.stoData);
      
    }
};
var refresh = function (item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoData.reload();       
    }
}
var dateTime_Renderer = function (value) {   
    return Ext.Date.format(value, "m/d/Y");
};





