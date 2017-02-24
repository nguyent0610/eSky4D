//// Declare //////////////////////////////////////////////////////////
var keys = ['KPI'];
var fieldsCheckRequire = ["KPI", "Name", "ApplyFor", "ApplyTo", "Type"];
var fieldsLangCheckRequire = ["OM25000KPI", "OM25000Name", "OM25000ApplyFor", "OM25000ApplyTo", "Type"];

var _Source = 0;
var _maxSource = 3;

var checkLoad = function () {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true
        _Source = 0;
        App.stoData.reload();
    }
};

///////////Store/////////////////
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoData_changed = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM25000');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoData_load = function (sto) {  
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM25000');
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
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {

    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
    App.cboApplyForDescr.getStore().addListener('load', checkLoad);
    App.cboApplyToDescr.getStore().addListener('load', checkLoad);
    App.cboTypeDescr.getStore().addListener('load', checkLoad);

    HQ.isFirstLoad = true;
    HQ.util.checkAccessRight();
};
var grdDet_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdDet_Edit = function (item, e) {
    //if (e.field == 'KPI') {
    //    //Ten combo + ten proceduce --> lay duoc data cua combo do-->hien Descr khi chon combo
    //    var obj = App.cboKPIOM25000_pcLoadKPI.findRecord('KPI', e.value);
    //    if (obj) {
    //        e.record.set('Name', obj.data.Name);
    //    }
    //}
    HQ.grid.checkInsertKey(App.grdDet, e, keys);
    //App.grdDet.view.refresh();
    //stoData_changed(App.stoData);
};
var grdDet_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdDet, e, keys);
};

var cboApplyForDescr_select = function (cbo, newValue, oldValue, eOpts) {
    // App.cboType.setValue(newValue[0].data.Code);
    //if (newValue[0].data.Code != null && newValue[0].data.Code != '')
    //{
    App.grdDet.selModel.selected.items[0].data.ApplyFor = newValue[0].data.Code;
    App.grdDet.selModel.selected.items[0].data.ApplyForDescr = newValue[0].data.Descr;
    //}
    
};

var cboApplyToDescr_select = function (cbo, newValue, oldValue, eOpts) {
    // App.cboType.setValue(newValue[0].data.Code);
    App.grdDet.selModel.selected.items[0].data.ApplyTo = newValue[0].data.Code;
    App.grdDet.selModel.selected.items[0].data.ApplyToDescr = newValue[0].data.Descr;
    
};

var cboTypeDescr_select= function (cbo, newValue, oldValue, eOpts) {
    // App.cboType.setValue(newValue[0].data.Code);
    App.grdDet.selModel.selected.items[0].data.Type = newValue[0].data.Code;
};

var grdDet_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdDet);
    //stoData_changed(App.stoData);
};

//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'OM25000/Save',
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
        //   stoData_changed(App.stoData);
        HQ.isChange = true;
        HQ.common.changeData(HQ.isChange, 'OM25000');
      
    }
};
var refresh = function (item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoData.reload();
    }
}
var renderApplyForName = function (value, metaData, record, row, col, store, gridView) {
    var r = HQ.store.findRecord(App.cboApplyForDescr.store, ['Code'], [record.data.ApplyFor])
    if (Ext.isEmpty(r)) {
        return value;
    }
    return r.data.Descr;
};

var renderApplyToName = function (value, metaData, record, row, col, store, gridView) {
    var r = HQ.store.findRecord(App.cboApplyToDescr.store, ['Code'], [record.data.ApplyTo])
    if (Ext.isEmpty(r)) {
        return value;
    }
    return r.data.Descr;
};
//var renderType = function (value) {
//    var r = HQ.store.findRecord(App.cboType.store, ['Code'], [value])
//    if (Ext.isEmpty(r)) {
//        return value;
//    }
//    return r.data.Descr;
//};


var renderType = function (value, metaData, record, row, col, store, gridView)
{
    var r = HQ.store.findRecord(App.cboTypeDescr.store, ['Code'], [record.data.Type])
    if (Ext.isEmpty(r))
    {
        return value;
    }
    return r.data.Descr;

}



var stringFilter = function (record) {
    if (this.dataIndex == 'ApplyFor') {
        App.cboApplyForDescr.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboApplyForDescr.store, "Code", "Descr");
    }
    else if (this.dataIndex == 'ApplyTo') {
        App.cboApplyToDescr.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboApplyToDescr.store, "Code", "Descr");
    }
    else if (this.dataIndex == 'Type') {
        App.cboTypeDescr.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboTypeDescr.store, "Code", "Descr");
    }
    else return HQ.grid.filterString(record, this);      
}