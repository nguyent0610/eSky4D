//// Declare //////////////////////////////////////////////////////////
var keys = ['AddrID'];
var fieldsCheckRequire = [""];
var fieldsLangCheckRequire = [""];

var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;
var _listState = '';
var _listDistrict = '';

///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoSYS_Cpny.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
                HQ.grid.first(App.grdSYS_Cpny);
            break;
        case "prev":
                HQ.grid.prev(App.grdSYS_Cpny);
            break;
        case "next":
                HQ.grid.next(App.grdSYS_Cpny);
            break;
        case "last":
                HQ.grid.last(App.grdSYS_Cpny);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoSYS_Cpny.reload();
            }          
            break;
        case "new":
            var newRecord = Ext.create('App.mdlSA00001_pgLoadGridCompany');
            App.frmDetail.loadRecord(newRecord);
            App.winLocation.setTitle("New")
            App.winLocation.show();
            HQ.common.setRequire(App.frmDetail);
            App.txtCpnyID.setReadOnly(false);
            App.txtCpnyID.setValue('');
            App.txtCpnyName.setValue('');
            App.Address.setValue('');
            //App.Address1.setValue('');
            //App.Address2.setValue('');           
            App.Tel.setValue('');
            App.Fax.setValue('');
            App.TaxRegNbr.setValue('');
            App.cboChannel.setValue('');
            App.cboTerritory.setValue('');
            App.cboCountry.setValue('');
            App.cboState.setValue('');
            App.cboCity.setValue('');
            App.cboDistrict.setValue('');
            App.cboCpnyType.setValue('');
            App.cboStatus.setValue('');
            App.Email.setValue('');
            App.Owner.setValue('');
            App.Plant.setValue('');
            App.Deposit.setValue('');
            App.CreditLimit.setValue('');
            App.MaxValue.setValue('');
            App.txtSalesDistrict.setValue('');
            App.txtSalesDistrictDescr.setValue('');
            App.txtSalesState.setValue('');
            App.txtSalesStateDescr.setValue('');

            App.stoSys_CompanyAddr.reload();
            App.txtSalesStateDescr.setVisible(HQ.allowSalesState);
            App.btnSalesState.setVisible(HQ.allowSalesState);
            App.txtSalesDistrictDescr.setVisible(HQ.allowSalesDistrict);
            App.btnSalesDistrict.setVisible(HQ.allowSalesDistrict);
            App.txtSalesDistrictDescr.allowBlank = !HQ.allowSalesDistrict;
            App.txtSalesStateDescr.allowBlank = !HQ.allowSalesState;
            App.frmDetail.validate();
            // App.BrandQSR.setValue('');
            //App.tableHD.setValue('');
            //App.tableTA.setValue('');
            //App.IP.setValue('');
            //App.Port.setValue('');
            //App.lat.setValue('');
            //App.lng.setValue('');
            //App.Station.setValue('');
            HQ.isNew = true;
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmSYS_Cpny.selected.items[0] != undefined) {
                    if (App.slmSYS_Cpny.selected.items[0].data.RoleID != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSYS_Cpny)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoSYS_Cpny, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":       
            break;
    }
};

//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.util.checkAccessRight();
    //App.txtSalesDistrictDescr.hide(!HQ.allowSalesState);
    //App.txtSalesStateDescr.hide(!HQ.allowSalesDistrict);
    
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    checkLoad();
    
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoSYS_Cpny);
    HQ.common.changeData(HQ.isChange, 'SA00001');//co thay doi du lieu gan * tren tab title header
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoSYS_Cpny_Load = function (sto) {
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

var grdSYS_Cpny_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdSYS_Cpny_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_Cpny, e, keys);
    frmChange();
};

var grdSYS_Cpny_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_Cpny, e, keys);
};

var grdSYS_Cpny_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_Cpny);
    frmChange();
};

////Function menuClick
var save = function () {
    var CompID = App.txtCpnyID.getValue();
    if (!HQ.util.checkSpecialChar(CompID)) {
        HQ.message.show(20140811, App.txtCpnyID.fieldLabe);
        return;

    }
    if (Ext.isEmpty(App.txtCpnyID.getValue())) {
        HQ.message.show(15, App.txtCpnyID.fieldLabel);
        return;
    }
    if (Ext.isEmpty(App.cboStatus.getValue())) {
        HQ.message.show(15, App.cboStatus.fieldLabel);
        return;
    }
    if (App.frmDetail.isValid()) {
        App.frmDetail.updateRecord();
        if (HQ.isNew) {
            App.stoSYS_Cpny.insert(App.stoSYS_Cpny.getCount(), App.frmDetail.updateRecord()._record);
        }
        App.frmDetail.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SA00001/Save',
            params: {
                lstSYS_Cpny: HQ.store.getData(App.stoSYS_Cpny),
                lstSys_CompanyAddr: HQ.store.getData(App.stoSys_CompanyAddr),
                compID: App.txtCpnyID.getValue()
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isFirstLoad = true;
                App.stoSYS_Cpny.reload();
                App.winLocation.hide();
                HQ.isNew = false;
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdSYS_Cpny.deleteSelected();
        frmChange();
    }
};

function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSYS_Cpny.reload();
        //App.stoSys_CompanyAddr.reload();
    }
};

var btnEdit_Click = function (record) {
    App.frmDetail.loadRecord(record);

    //App.txtCpnyID.setValue(record.data.CpnyID.split(','));
    //App.txtUserID.setValue(record.data.UserName);
    App.txtCpnyID.setReadOnly(true);
    App.winLocation.setTitle("Edit");
    HQ.isNew = false;
    App.winLocation.show();
    App.stoSys_CompanyAddr.reload();
    
    App.txtSalesStateDescr.setVisible(HQ.allowSalesState);
    App.btnSalesState.setVisible(HQ.allowSalesState);
    App.txtSalesDistrictDescr.setVisible(HQ.allowSalesDistrict);
    App.btnSalesDistrict.setVisible(HQ.allowSalesDistrict);
    App.txtSalesDistrictDescr.allowBlank = !HQ.allowSalesDistrict;
    App.txtSalesStateDescr.allowBlank = !HQ.allowSalesState;
    App.frmDetail.validate();
    //App.stoForm.reload();

};

var btnLocationCancel_Click = function () {
    App.winLocation.hide();
};

var btnLocationOK_Click = function () {
    //if (HQ.isUpdate == false && HQ.isInsert) {
    //    return;
    //}
    if (App.txtCpnyID.getValue() == null) {
        HQ.message.show(15, App.txtCpnyID.fieldLabel);
        return;
    }
    save();
};

var cboCountry_Change = function (sender, e) {
    var cboCountry_Change = function (sender, e, oldValue) {
        if (!HQ.isFirstLoad && e != oldValue) {
            App.txtSalesState.setValue("");
            App.txtSalesStateDescr.setValue("");
            App.txtSalesDistrict.setValue("");
            App.txtSalesDistrictDescr.setValue("");
        }
    };
    App.cboState.store.reload();
};

var cboZone_Change = function (sender, e) {
    if (sender.hasFocus) {
        App.cboTerritory.getStore().load(function () {
            var curRecord = App.frmDetail.getRecord();
            if (curRecord != undefined) {
                if (curRecord.data.Territory) {
                    App.cboTerritory.setValue(curRecord.data.Territory);
                }
            }
            var dt = HQ.store.findInStore(App.cboTerritory.getStore(), ["Territory", "Zone"], [App.cboTerritory.getValue(), App.cboZone.getValue()]);
            if (!dt) {
                curRecord.data.Territory = '';                
                App.cboTerritory.setValue("");

                curRecord.data.State = '';
                //App.cboState.setValue("");
                App.cboCity.setValue("");
                curRecord.data.City = '';
                App.cboDistrict.setValue("");
                curRecord.data.District = '';
            }
            if (App.cboTerritory.value == curRecord.data.Territory) {
                cboTerritory_Change(App.cboTerritory, curRecord.data.Territory);
            }
        });
    }
};

var cboTerritory_Change = function (sender, e) {
    if (sender.hasFocus) {
        //App.cboState.getStore().load(function () {
            var curRecord = App.frmDetail.getRecord();
            if (curRecord != undefined)
                if (curRecord.data.State) {
                    App.cboState.setValue(curRecord.data.State);
                }
            var dt = HQ.store.findInStore(App.cboState.getStore(), ["State", "Territory"], [App.cboState.getValue(), App.cboTerritory.getValue()]);
            if (!dt) {
                curRecord.data.State = '';
                App.cboState.setValue("");
                App.cboCity.setValue("");
                curRecord.data.City = '';
                App.cboDistrict.setValue("");
                curRecord.data.District = '';
            }
            if (App.cboState.value == curRecord.data.State) {
                cboState_Change(App.cboState, curRecord.data.State);
            }
      //  });
    }
};

// Expand Territory
var cboTerritory_Expand = function (combo) {
    App.cboTerritory.store.clearFilter();
    var zone = App.cboZone.getValue();
    var store = App.cboTerritory.store;        
    // Filter data -- 
    store.filterBy(function (record) {
        if (record) {
            if (record.data['Zone'].toString() == zone) {
                return record;
            }
        }
    });
};
var cboTerritory_Collapse = function (cbombo) {   
    App.cboTerritory.store.clearFilter();    
};

var cboState_Change = function (sender, e) {
    if (sender.hasFocus) {

       // App.cboDistrict.getStore().load(function () {
            var curRecord = App.frmDetail.getRecord();
            if (curRecord && curRecord.data.District) {
                App.cboDistrict.setValue(curRecord.data.District);
                HQ.combo.expand(App.cboDistrict, ',');
            }
            var dt = HQ.store.findInStore(App.cboDistrict.getStore(), ["District", "State", "Country"], [App.cboDistrict.getValue(), App.cboState.getValue(), App.cboCountry.getValue()]);
            if (!dt) {
                curRecord.data.District = '';
                App.cboDistrict.setValue("");
            }
            else {
                HQ.combo.expand(App.cboDistrict, ',');
            }
       // });

        App.cboCity.getStore().load(function () {
            var curRecord = App.frmDetail.getRecord();
            if (curRecord && curRecord.data.City) {
                App.cboCity.setValue(curRecord.data.City);
            }
            var dt = HQ.store.findInStore(App.cboCity.getStore(), ["City"], [App.cboCity.getValue()]);
            if (!dt) {
                curRecord.data.City = '';
                App.cboCity.setValue("");
            }            
        });
    }
};

 //Expand Territory
var cboState_Expand = function (combo) {
    App.cboState.store.clearFilter();
    var country = App.cboCountry.getValue();
    var territory = App.cboTerritory.getValue();

    HQ.combo.expand(this, ',');

    var store = App.cboState.store;
    // Filter data -- 
    store.filterBy(function (record) {
        if (record) {
            if (record.data['Territory'].toString() == territory && record.data['Country'].toString() == country) {
                return record;
            }
        }
    });
};
var cboState_Collapse = function (cbombo) {
    App.cboState.store.clearFilter();
};

var cboDistrict_Change = function (sender, value) {
    //var curRecord = App.frmDetail.getRecord();
    //if (curRecord && curRecord.data.District) {
    //    App.cboDistrict.setValue(curRecord.data.District);
    //    HQ.combo.expand(App.cboDistrict, ',');
    //}
    //var dt = HQ.store.findInStore(App.cboDistrict.getStore(), ["District"], App.cboDistrict.getValue());
    //if (!dt) {
    //    curRecord.data.District = '';
    //    App.cboDistrict.setValue("");
    //}
    //else {
    //    HQ.combo.expand(App.cboDistrict, ',');
    //}
};

// Expand District
var cboDistrict_Expand = function (combo) {
    App.cboDistrict.store.clearFilter();
    var country = App.cboCountry.getValue();
    var state = App.cboState.getValue();    

    var store = App.cboDistrict.store;
    // Filter data -- 
    store.filterBy(function (record) {
        if (record) {
            if (record.data['State'].toString() == state && record.data['Country'].toString() == country) {
                return record;
            }
        }
    });
    
    HQ.combo.expand(App.cboDistrict, ',');
};
var cboDistrict_Collapse = function (cbombo) {
    App.cboDistrict.store.clearFilter();
};

/*var save = function () {
    var CompID = App.txtCpnyID.getValue();
    if (!HQ.util.checkSpecialChar(CompID))
    {
        HQ.message.show(20140811, App.txtCpnyID.fieldLabe);
        return;

    }
    if (Ext.isEmpty(App.txtCpnyID.getValue())) {
        HQ.message.show(15, App.txtCpnyID.fieldLabel);
        return;
    }
    //if (!HQ.util.checkEmail(App.txtEmail.getValue()))
    //{
    //    return;
    //}
    App.frmDetail.submit({
        timeout: 1800000,
        waitMsg: HQ.common.getLang("SavingData"),
        url: 'SA03001/Save',
        params: {
            //lstUser: Ext.encode(App.stoForm.getRecordsValues()),
            //ckbBloked: App.ckbBloked.getValue(),
            //lstCpnyID : App.txtCpnyID.getValue().join(','),
            //isNewUser: HQ.isNew,
            //expireDay: App.txtExpireDay.getValue(),
        },
        success: function (msg, data) {
            HQ.message.show(201405071);
            App.stoSYS_Cpny.reload();
            App.winLocation.hide();
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });
};*/

var stoSys_CompanyAddr_Load = function (sto) {
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
    //if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        //HQ.isFirstLoad = false; //sto load cuoi se su dung
    //}
    //App.stoSys_CompanyAddr.reload();
};

var grdSys_CompanyAddr_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdSys_CompanyAddr_Edit = function (item, e) {
    //Kiem tra cac key da duoc nhap se insert them dong moi
    HQ.grid.checkInsertKey(App.grdSys_CompanyAddr, e, keys);
    frmChange();
};

var grdSys_CompanyAddr_ValidateEdit = function (item, e) {
    //ko cho nhap key co ki tu dac biet, va kiem tra trung du lieu

    if (e.column.dataIndex == "Country") {
        if (e.value != e.record.data.Country) {
            e.record.set("State", "");
            e.record.set("City", "");
        }
    }
    if (e.column.dataIndex == "State") {
        if (e.value != e.record.data.State) {
            e.record.set("City", "");
        }
    }
    return HQ.grid.checkValidateEdit(App.grdSys_CompanyAddr, e, keys);
};

function isNumeric(n) {
    return !isNaN(parseFloat(n)) && isFinite(n);
}

var txtLat_Change = function (sender, value) {
    if (value != "-") {
        if (isNumeric(value) == false) {
            HQ.message.show("201711251");
            App.txtLat.setValue(App.stoSYS_Cpny.findRecord("CpnyID", App.txtCpnyID.getValue()).data.Lat);
        }
    }

}

var txtLat_Blur = function (sender, value) {
    if (App.txtLat.getValue() == "-") {
        HQ.message.show("201711251");
        App.txtLat.setValue(App.stoSYS_Cpny.findRecord("CpnyID", App.txtCpnyID.getValue()).data.Lat);
    }

}


var txtLng_Change = function (sender, value) {
    if (value != "-") {
        if (isNumeric(value) == false) {
            HQ.message.show("201711252");
            App.txtLng.setValue(App.stoSYS_Cpny.findRecord("CpnyID", App.txtCpnyID.getValue()).data.Lng);
        }
    }
}

var txtLng_Blur = function (sender, value) {
    if (App.txtLng.getValue() == "-") {
        HQ.message.show("201711252");
        App.txtLng.setValue(App.stoSYS_Cpny.findRecord("CpnyID", App.txtCpnyID.getValue()).data.Lng);
    }

}


/////////////////////////////////////////////////////////////////////

var btnState_TriggerClick = function () {
    _listState = App.txtSalesState.getValue();// joinParams(App.cboMailTo);
    //App.chkActive_All.setValue(false);
    App.winLocation.mask();
    App.stoState.removeAll();
    App.stoState.reload();
    App.winState.show();
};
var winState_beforeShow = function () {
    App.winLocation.mask();
    App.winState.mask();
    App.winState.setHeight(App.frmMain.getHeight() - 20);
    App.winState.setWidth(App.frmMain.getWidth() - 10);
};


var btnOKState_Click = function () {
    var res = "";
    var stateName = "";
    var store = App.stoState;
    var allRecords = store.snapshot || store.allData || store.data;
    store.suspendEvents();
    allRecords.each(function (record) {
        if (record.data.Selected == true) {
            res += record.data.State + ',';
            stateName += record.data.Descr + ',';
        }
    });
    store.resumeEvents();
    var selState = '';
    var selstateName = '';
    if (res.length > 1) {
        selState = res.substring(0, res.length - 1);
    }
    if (stateName.length > 1) {
        selstateName = stateName.substring(0, stateName.length - 1);
    }
    App.txtSalesState.setValue(selState);
    App.txtSalesStateDescr.setValue(selstateName);
    App.winState.hide();
    App.winLocation.unmask();
};

var btnCancelState_Click = function () {
    App.winState.hide();
    App.winLocation.unmask();
};

var stoState_Load = function (sto) {
    HQ.common.showBusy(false);
    App.winState.unmask();
};

var chkActiveAll_Change = function (sender, value, oldValue) {
    if (sender.hasFocus) {
        var store = App.stoState;
        var allRecords = store.snapshot || store.allData || store.data;
        store.suspendEvents();
        allRecords.each(function (record) {
            record.set('Selected', value);
        });
        store.resumeEvents();
        App.grdState.view.refresh();
    }
};

var winDistrict_beforeShow = function () {
    App.winLocation.mask();
    App.winDistrict.mask();
    App.winDistrict.setHeight(App.frmMain.getHeight() - 20);
    App.winDistrict.setWidth(App.frmMain.getWidth() - 10);
};


var btnOKDistrict_Click = function () {
    var res = "";
    var resDistrictName = "";
    var store = App.stoDistrict;
    var allRecords = store.snapshot || store.allData || store.data;
    store.suspendEvents();
    allRecords.each(function (record) {
        if (record.data.Selected == true) {
            res += record.data.District + ',';
            resDistrictName += record.data.DistrictName + ',';
        }
    });
    store.resumeEvents();
    var selDistrict = '';
    var selDistrictName = '';
    if (res.length > 1) {
        selDistrict = res.substring(0, res.length - 1);
    }
    if (resDistrictName.length > 1) {
        selDistrictName = resDistrictName.substring(0, resDistrictName.length - 1);
    }
    App.txtSalesDistrict.setValue(selDistrict);
    App.txtSalesDistrictDescr.setValue(selDistrictName);
    App.winDistrict.hide();
    App.winLocation.unmask();
};

var btnCancelDistrict_Click = function () {
    App.winDistrict.hide();
    App.winLocation.unmask();
};

var btnDistrict_TriggerClick = function () {
    _listDistrict = App.txtSalesDistrict.getValue();// joinParams(App.cboMailTo);
    App.winLocation.mask();
    App.stoDistrict.removeAll();
    App.stoDistrict.reload();
    App.winDistrict.show();
};

var stoDistrict_Load = function (sto) {
    HQ.common.showBusy(false);
    App.winDistrict.unmask();
};

var chkActiveDistrictAll_Change = function (sender, value, oldValue) {
    if (sender.hasFocus) {
        var store = App.stoDistrict;
        var allRecords = store.snapshot || store.allData || store.data;
        store.suspendEvents();
        allRecords.each(function (record) {
            record.set('Selected', value);
        });
        store.resumeEvents();
        App.grdDistrict.view.refresh();
    }
};

var txtSalesState_Change = function (sender, e, oldValue) {
    if (!HQ.isFirstLoad && e != oldValue) {
        App.txtSalesDistrictDescr.setValue("");
        App.txtSalesDistrict.setValue("");
    }
};

var renderCountry = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboCountry_grd.findRecord("CountryID", rec.data.Country);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};

var stringFilterCountry = function (record) {

    if (this.dataIndex == 'Country') {
        App.cboCountry_grd.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboCountry_grd.store, "CountryID", "Descr");
    }

    return HQ.grid.filterString(record, this);
};
var renderState = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboState_grd.findRecord("State", rec.data.State);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};

var stringFilterState = function (record) {

    if (this.dataIndex == 'City') {
        App.cboState_grd.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboState_grd.store, "State", "Descr");
    }

    return HQ.grid.filterString(record, this);
};

var renderCity = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboCity_grd.findRecord("City", rec.data.City);
    if (record) {
        return record.data.Name;
    }
    else {
        return value;
    }
};

var stringFilterCity = function (record) {

    if (this.dataIndex == 'City') {
        App.cboCity_grd.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboCity_grd.store, "City", "Name");
    }

    return HQ.grid.filterString(record, this);
};

