//// Declare //////////////////////////////////////////////////////////
var keys;
var fieldsCheckRequire ;
var fieldsLangCheckRequire ;
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
            var record = HQ.store.findRecord(App.stoData, keys, ['Country', 'State']);
                if (!record) {
                    HQ.store.insertBlank(App.stoData, ['Country', 'State']);
                }
            break;
        case "new":
            
            if (HQ.isInsert) {
             //  insert(App.grdDet, keys);
                HQ.grid.insert(App.grdDet, ['State']);

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

    
   if (HQ.country == false)
   {
       keys = ['State', "Territory"];
       fieldsCheckRequire = ["State", "Descr", "Territory"];
       fieldsLangCheckRequire = [ "State", "Descr","Territory"];
       App.Country.hide();
   }
   else
   {
       keys = ['Country', 'State', "Territory"];
       fieldsCheckRequire = ["Country", "State", "Descr","Territory"];
       fieldsLangCheckRequire = ["Country", "State", "Descr","Territory"];
       App.Country.show();
   }
    checkLoad();
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoData);
    HQ.common.changeData(HQ.isChange, 'SI20700');//co thay doi du lieu gan * tren tab title header
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
    if (e.field == "State") {
        if(e.value != "")
        {
            return false;
        }
    }
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
   
};

//var grdDet_Edit = function (item, e) {
    
  
//};

var grdDet_ValidateEdit = function (item, e) {
    if (HQ.isDublicateState) {
        if (checkValidateEdit(App.grdDet, e, keys)) {
            return false;
        } else {
            var keys1 = ['State'];
            return checkValidateEdit(App.grdDet, e, keys1);

        }
    }
    else return checkValidateEdit(App.grdDet, e, keys);
   
};

var grdDet_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdDet);
    frmChange();
};
//var State_edit = function ()
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
var checkDeleteData = function (indexColum, check, checkState) {

    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SI20700/CheckDelete',
            params: {
                lstIndexColum: indexColum,
                lstCheck: check,
                lstCheckState: checkState
            },
            success: function (msg, data) {
                //  App.grdIN_ProductClass.deleteSelected();
                App.grdDet.deleteSelected();
                frmChange();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};
var deleteData = function (item) {
    if (item == "yes") {
        //App.grdDet.deleteSelected();
        //frmChange();
        var indexcolum = '';
        var check = '';
        var checkState = '';
        var lstSelete = App.grdDet.selModel.selected;
        for (var i = 0; i < lstSelete.length; i++) {
            indexcolum = indexcolum + (lstSelete.items[i].index + 1) + ",";
            check = check + lstSelete.items[i].data.Country + ",";
            checkState = checkState + lstSelete.items[i].data.State + ",";
        }
        checkDeleteData(indexcolum, check, checkState);
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
        }
        else {
            e.record.set('DescrTerritory', '');
        }
    }
    HQ.grid.checkInsertKey(App.grdDet, e, ['State', 'Territory']);
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
                App.grdDet.store.reload();    // reload lại store
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
        //waitMsg: HQ.common.getLang("Exporting"),
        url: 'SI20700/Export',
        type: 'POST',
        timeout: 1000000,
        clientValidation: false,
        params: {
            //lstOM_CabinetChecking: Ext.encode(App.stoOM_CabinetChecking.getRecordsValues())
        },
        success: function (msg, data) {
            window.location = 'SI20700/DownloadAndDelete?file=' + data.result.fileName;

        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });
};

var checkValidateEdit = function (grd, e, keys, isCheckSpecialChar) {
    if (keys.indexOf(e.field) != -1) {
        var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/
        if (isCheckSpecialChar == undefined) isCheckSpecialChar = true;
        if (isCheckSpecialChar) {
            if (e.value)
                if (!HQ.util.passNull(e.value) == '' && !HQ.util.passNull(e.value.toString()).match(regex)) {
                    HQ.message.show(20140811, e.column.text);
                    return false;
                }
        }
        if (checkDuplicate(grd, e, keys)) {
            
            if (e.column.xtype == "datecolumn")
                HQ.message.show(1112, Ext.Date.format(e.value, e.column.format));
            else HQ.message.show(1112, e.value);
            return false;
        }

    }
};
var checkDuplicate = function (grd, row, keys) {
    
    var found = false;
    var store = grd.getStore();
    if (keys == undefined) keys = row.record.idProperty.split(',');
    var allData = store.snapshot || store.allData || store.data;
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
                    rowdata += (row.record.data[keys[jkey]] ? row.record.data[keys[jkey]].toString().toLowerCase() : '') + ',';
            }
        }
        if (found = (data == rowdata && record.id != row.record.id) ? true : false) {
            break;
        };
    }
    return found;
};

var insert = function (grd, keys)
{
        var store = grd.getStore();
        var createdItems = store.getChangedData().Created;
        if (createdItems != undefined)
        {
            if (store.currentPage != Math.ceil(store.totalCount / store.pageSize) && store.totalCount != 0)
            {
                store.loadPage(Math.ceil(store.totalCount / store.pageSize),
                    {callback: function ()
                        {HQ.grid.last(grd);
                        setTimeout(function ()
                        {
                            //grd.editingPlugin.startEditByPosition({ row: store.getCount() - 1, column: 1 });
                            if (grd.editingPlugin)
                            {
                                grd.editingPlugin.startEditByPosition({row: store.getCount() - 1,column: 1});
                            }
                            else
                            {
                                grd.lockedGrid.editingPlugin.startEditByPosition({row: store.getCount() - 1,column: 1});
                            }
                        }, 300);
                    }
                });
            }
            else
            {
                HQ.grid.last(grd);
                if (grd.editingPlugin)
                {
                    grd.editingPlugin.startEditByPosition({row: store.getCount() - 1,column: 1});
                }
                else
                {
                    grd.lockedGrid.editingPlugin.startEditByPosition({row: store.getCount() - 1,column: 1});
                }
            }
            return;
        }
        if (store.currentPage != Math.ceil(store.totalCount / store.pageSize))
        {
            store.loadPage(Math.ceil(store.totalCount / store.pageSize),
            {callback: function ()
                {
                    if (HQ.grid.checkRequirePass(store.getChangedData().Updated, keys))
                    {
                        HQ.store.insertBlank(store, keys);
                    }
                    HQ.grid.last(grd);
                    setTimeout(function ()
                    {
                        // grd.editingPlugin.startEditByPosition({ row: store.getCount() - 1, column: 1 });
                        if (grd.editingPlugin)
                        {
                            grd.editingPlugin.startEditByPosition({row: store.getCount() - 1,column: 1});
                        }
                        else
                        {
                            grd.lockedGrid.editingPlugin.startEditByPosition({row: store.getCount() - 1,column: 1});
                        }
                    }, 300);
                }
            });
        }
        else
        {
            if (HQ.grid.checkRequirePass(store.getChangedData().Updated, keys))
            {
                HQ.store.insertBlank(store, keys);
            }
            HQ.grid.last(grd);
            //grd.editingPlugin.startEditByPosition({ row: store.getCount() - 1, column: 1 });
            if (grd.editingPlugin)
            {
                grd.editingPlugin.startEditByPosition({row: store.getCount() - 1,column: 1});
            }
            else
            {
                grd.lockedGrid.editingPlugin.startEditByPosition({row: store.getCount() - 1,column: 1});
            }
        }
};


