//// Declare //////////////////////////////////////////////////////////
var keys = ['Country', 'State'];
var fieldsCheckRequire = ["Country", "State", "Descr"];
var fieldsLangCheckRequire = ["Country", "State", "Descr"];
var _Source = 0;
var _maxSource = 2;
var _isLoadMaster = false;


///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoData.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

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
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoData.reload();
            }
            //var record = HQ.store.findInStore(App.grdDet.store, ['Country', 'State'], ['', ''])
            //    if (!record) {
            //        HQ.store.insertBlank(App.grdDet.store, keys);
            //    }
            var record = HQ.store.findRecord(App.stoData, keys, ['Country', 'State']);
                if (!record) {
                    HQ.store.insertBlank(App.stoData, ['Country', 'State']);
                }
            break;
        case "new":
            
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdDet, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmData.selected.items[0] != undefined) {
                    if (App.slmData.selected.items[0].data.RoleID != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdDet)], 'deleteData', true);
                    }
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
            //HQ.common.close(this);
            break;
    }
};

//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboCountry.getStore().addListener('load', checkLoad);
    App.cboTerritory.getStore().addListener('load', checkLoad);

    if (HQ.provinceView === false) {
        
        App.Province.hide();
   }
   if (HQ.country == false)
   {
       
       keys = ["State"];
       fieldsCheckRequire = [ "State", "Descr"];
       fieldsLangCheckRequire = [ "State", "Descr"];
       App.Country.hide();
   }
   else
   {
       keys = ['Country', 'Territory', "State"];
       fieldsCheckRequire = ["Country" ];
       fieldsLangCheckRequire = ["Country" ];
       App.Country.show();
   }
    checkLoad();
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoData);
    HQ.common.changeData(HQ.isChange, 'SI20400');//co thay doi du lieu gan * tren tab title header
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoData_Load = function (sto) {
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false; //sto load cuoi se su dung
    }
    //Sto tiep theo
    frmChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

var grdDet_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
    if (e.field === 'Province') {
        return false;
    }
};

var grdDet_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdDet, e, keys);
   // if()
    frmChange();
};

var grdDet_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdDet, e, keys);
};

var grdDet_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdDet);
    frmChange();
};


/////////////////////////////////////
////////////////////////////////////
////Process
////Function menuClick
var save = function () {
    
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SI20700/Save',
            params: {
                lstData: HQ.store.getData(App.stoData)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isFirstLoad = true;
                App.stoData.reload();
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
        frmChange();
    }
};

///////////////////////////////////////////////
//////////////////////////////////////////////
//Other function

function refresh(item) {
    
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoData.reload();
    }
};


////
var grdTerritoryClassDetail_Edit = function (item, e) {
    
    if (e.field == "Territory") {
        var a = HQ.store.findRecord(App.cboTerritory.store, ['Territory'], [e.value]);
        if (e.value != "" && e.value != null) {
            e.record.set('DescrTerritory', a.data.DescrTerritory);
            //  e.record.set('Addr1', a.data.Addr1);
        }
        else {
            //   e.record.set('CustName', '');
            e.record.set('DescrTerritory', '');
        }
    }
    //if (e.field == "Territory") {
    //    if (_territory != e.value) {
    //        e.record.set("Area", '');
    //        e.record.set("State", '')
    //    }
    //}
    //if (e.field == "Territory" || e.field == "State" || e.field == "Area") {
    //    if (_territory != e.record.data.Territory || _area != e.record.data.Area || _state != e.record.data.State) {
    //        e.record.set("BranchID", '');
    //    }
    //}
    //if (e.field == "BranchID") {
    //    if (_branchID != e.value) {
    //        e.record.set("CustId", '')
    //    }
    //}
    //App.cbo_SubTerritory.store.clearFilter();
    //App.cboState.store.clearFilter();
    //App.cboBranchID.store.clearFilter();
    //App.cboCustID.store.clearFilter();
   // App.cboTerritory.store.clearFilter();
    HQ.grid.checkInsertKey(App.grdDet, e, keys);
    frmChange();
};


/////////////////////////////////////
var btnImport_Click = function (sender, e) {
    
    var fileName = sender.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (ext == "xls" || ext == "xlsx") {
        App.frmMain.submit({
            waitMsg: "Importing....",
            url: 'SI20700/Import',
            timeout: 18000000,
            clientValidation: false,
            method: 'POST',
            params: {
            },
            success: function (msg, data) {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                if (!Ext.isEmpty(this.result.data.message)) {
                    HQ.message.show('2013103001', [this.result.data.message], '', true);
                }
                else {
                    HQ.message.process(msg, data, true);
                }
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
    else {
        HQ.message.show('2014070701', '', '');
        sender.reset();
    }
};
///////////////////////////////////
//Export
var btnExport_Click = function () {
    App.frmMain.submit({
        url: 'SI20700/Export',
        type: 'POST',
        timeout: 1000000,
        clientValidation: false,
        params: {

        },
        success: function (msg, data) {
            alert('sus');
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });
};

