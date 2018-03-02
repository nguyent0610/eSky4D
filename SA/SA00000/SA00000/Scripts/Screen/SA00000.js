var keys = ['AddrID'];
var fieldsCheckRequire = ["AddrID"];
var fieldsLangCheckRequire = ["AddrID"];

var keys1 = ['SubCpnyID'];
var fieldsCheckRequire1 = ["SubCpnyID"];
var fieldsLangCheckRequire1 = ["SubCpnyID"];

var CpnyID = '';
var _Source = 0;
var _maxSource = 12;
var _isLoadMaster = false;
var _country = '';
var _countryOld = '';
var _state = '';
var _stateOld = '';
var _listState = '';
var _listDistrict = '';
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoSYS_Company.reload();
        HQ.common.showBusy(false);
        App.cboBranchOld.store.loadData(App.cboCpnyID.store.data.items);
    }
};

var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    if (HQ.showSalesStateSalesDistrict) {
        //App.cbo
    }
    App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));

    App.cboCpnyID.getStore().addListener('load', checkLoad);
    App.cboChannel.getStore().addListener('load', checkLoad);
    App.cboTerritory.getStore().addListener('load', checkLoad);
    App.cboCountry.getStore().addListener('load', checkLoad);
    App.cboCpnyType.getStore().addListener('load', checkLoad);
    App.cboType.getStore().addListener('load', checkLoad);
    App.cboCountry_grd.getStore().addListener('load', checkLoad);
    App.cboTaxId00.getStore().addListener('load', checkLoad);
    App.cboTaxId01.getStore().addListener('load', checkLoad);
    App.cboTaxId02.getStore().addListener('load', checkLoad);
    App.cboTaxId03.getStore().addListener('load', checkLoad);
    App.cboSubCpnyID.getStore().addListener('load', checkLoad);
    App.cboState_grd.getStore().addListener('load', checkLoad);
    //App.cboState_grd.store.reload();
    App.txtSalesDistrictDescr.setVisible(HQ.showSalesState);
    App.btnSalesState.setVisible(HQ.showSalesState);
    App.btnSalesDistrict.setVisible(HQ.showSalesState);
    App.txtSalesStateDescr.setVisible(HQ.showSalesState);
    App.txtSalesDistrictDescr.allowBlank = !HQ.showSalesState;
    App.txtSalesStateDescr.allowBlank = !HQ.showSalesState;
    App.cboOwner.allowBlank = !HQ.allowOwer;
    App.Address2.allowBlank = !HQ.allowAddress2;
    App.frmMain.isValid();
    if (HQ.SA00000PP != '1') {
        App.tabDetail.child('#pnlHandle').tab.hide();
    }
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == 'header') {
                HQ.isFirstLoad = true;
                HQ.combo.first(App.cboCpnyID, HQ.isChange);
            }
            else if (HQ.focus == 'grdSys_CompanyAddr') {
                HQ.grid.first(App.grdSys_CompanyAddr);
            }
            else if (HQ.focus == 'grdSYS_SubCompany') {
                HQ.grid.first(App.grdSYS_SubCompany);
            }
            break;
        case "prev":
            if (HQ.focus == 'header') {
                HQ.isFirstLoad = true;
                HQ.combo.prev(App.cboCpnyID, HQ.isChange);
            }
            else if (HQ.focus == 'grdSys_CompanyAddr') {
                HQ.grid.prev(App.grdSys_CompanyAddr);
            }
            else if (HQ.focus == 'grdSYS_SubCompany') {
                HQ.grid.prev(App.grdSYS_SubCompany);
            }
            break;
        case "next":
            if (HQ.focus == 'header') {
                HQ.isFirstLoad = true;
                HQ.combo.next(App.cboCpnyID, HQ.isChange);
            }
            else if (HQ.focus == 'grdSys_CompanyAddr') {
                HQ.grid.next(App.grdSys_CompanyAddr);
            }
            else if (HQ.focus == 'grdSYS_SubCompany') {
                HQ.grid.next(App.grdSYS_SubCompany);
            }
            break;
        case "last":
            if (HQ.focus == 'header') {
                HQ.isFirstLoad = true;
                HQ.combo.last(App.cboCpnyID, HQ.isChange);
            }
            else if (HQ.focus == 'grdSys_CompanyAddr') {
                HQ.grid.last(App.grdSys_CompanyAddr);
            }
            else if (HQ.focus == 'grdSYS_SubCompany') {
                HQ.grid.last(App.grdSYS_SubCompany);
            }
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                refresh("yes");
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.focus == 'header') {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', 'refresh');
                    } else {
                        App.cboCpnyID.setValue('');
                        App.stoSYS_Company.reload();
                    }
                }
                else if (HQ.focus == 'grdSys_CompanyAddr') {
                    HQ.grid.insert(App.grdSys_CompanyAddr, keys);
                }
                else if (HQ.focus == 'grdSYS_SubCompany') {
                    HQ.grid.insert(App.grdSYS_SubCompany, keys1);
                }
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (HQ.focus == 'header') {
                    if (App.cboCpnyID.getValue()) {
                        HQ.message.show(11, '', 'deleteData');
                    } else {
                        menuClick('new');
                    }
                }
                else if (HQ.focus == 'grdSys_CompanyAddr') {
                    if (App.slmSys_CompanyAddr.selected.items[0] != undefined) {
                        if (App.slmSys_CompanyAddr.selected.items[0].data.AddrID != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSys_CompanyAddr)], 'deleteData', true);
                        }
                    }
                }
                else if (HQ.focus == 'grdSYS_SubCompany') {
                    if (App.slmSYS_SubCompany.selected.items[0] != undefined) {
                        if (App.slmSYS_SubCompany.selected.items[0].data.SubCpnyID != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSYS_SubCompany)], 'deleteData', true);
                        }
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain)
                    && HQ.util.checkEmail(App.Email.value)
                    && HQ.store.checkRequirePass(App.stoSys_CompanyAddr, keys, fieldsCheckRequire, fieldsLangCheckRequire)
                    && HQ.store.checkRequirePass(App.stoSYS_SubCompany, keys1, fieldsCheckRequire1, fieldsLangCheckRequire1)) {
                    if (HQ.util.checkSpecialChar(App.cboCpnyID.getValue()) == true) {
                        save();
                    }
                    else {
                        HQ.message.show(20140811, App.cboCpnyID.fieldLabel);
                        App.cboCpnyID.focus();
                        App.cboCpnyID.selectText();
                    }
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

// Event when cboCpnyID is changed or selected item 
var cboCpnyID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if ((!HQ.isNew || sender.valueModels != null) && !App.stoSYS_Company.loading) {
        App.stoSYS_Company.reload();
    }
};

var cboCpnyID_Select = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoSYS_Company.loading) {
        App.stoSYS_Company.reload();
    }
};

//khi nhan combo xo ra, neu da thay doi thi ko xo ra
var cboCpnyID_Expand = function (sender, value) {
    if (HQ.isChange) {
        App.cboCpnyID.collapse();
    }
};

//khi nhan X xoa tren combo, neu du lieu thay doi thi ko cho xoa, du lieu chua thay doi thi add new
var cboCpnyID_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {
        menuClick('new');
    }
};
var cboType_Change = function (sender, e) {
    App.cboTerritory.store.clearFilter();
    if (App.cboType.getValue())
        App.cboTerritory.store.filter('Zone', App.cboType.getValue());
}


var cboTerritory_Change = function (sender, e) {
    App.cboState.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord != undefined)
            if (curRecord.data.State) {
                App.cboState.setValue(curRecord.data.State);
            }
        var dt = HQ.store.findInStore(App.cboState.getStore(), ["State"], [App.cboState.getValue()]);
        if (!dt) {
            if (sender.hasfocus) {
                curRecord.data.State = '';
                App.cboState.setValue("");
            }
        }
        if (App.cboState.value == curRecord.data.State) {
            cboState_Change(App.cboState, curRecord.data.State);
        }
    });

    App.cboOwner.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord != undefined)
            if (curRecord.data.Owner) {
                App.cboOwner.setValue(curRecord.data.Owner);
            }
        var dt = HQ.store.findInStore(App.cboOwner.getStore(), ["Owner"], [App.cboOwner.getValue()]);
        if (!dt) {
            if (sender.hasfocus) {
                curRecord.data.Owner = '';
                App.cboOwner.setValue("");
            }
        }
        
    });

};

var cboCountry_Change = function (sender, e, oldValue) {
    App.cboState.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord != undefined)
            if (curRecord.data.State) {
                App.cboState.setValue(curRecord.data.State);
            }
        var dt = HQ.store.findInStore(App.cboState.getStore(), ["State"], [App.cboState.getValue()]);
        if (!dt) {
            curRecord.data.State = '';
            App.cboState.setValue("");
        }
        if (App.cboState.value == curRecord.data.State) {
            cboState_Change(App.cboState, curRecord.data.State);
        }
    });

    if ((oldValue != undefined) || sender.hasFocus) {
        if (e != oldValue) {            
            App.txtSalesDistrict.setValue("");
            App.txtSalesDistrictDescr.setValue("");
            App.txtSalesState.setValue("");
            App.txtSalesStateDescr.setValue("");
        }
    }

    //App.cboSalesState.getStore().load(function () {
    //    var curRecord = App.frmMain.getRecord();
    //    if (curRecord && curRecord.data.SalesState) {
    //        App.cboSalesState.setValue(curRecord.data.SalesState);
    //        HQ.combo.expand(App.cboSalesState, ',');
    //    }
    //    var dt = HQ.store.findInStore(App.cboSalesState.getStore(), ["State"], App.cboSalesState.getValue());
    //    if (!dt) {
    //        curRecord.data.SalesState = '';
    //        App.cboSalesState.setValue("");
    //    }
    //    else {
    //        HQ.combo.expand(App.cboSalesState, ',');
    //    }
    //    if (App.cboSalesState.value == curRecord.data.SalesState) {
    //        //cboSalesState_Change(App.cboSalesState, curRecord.data.SalesState);
    //    }
    //});
};

var cboSalesState_Change = function (sender, e) {
    App.cboSalesDistrict.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord && curRecord.data.SalesDistrict) {
            App.cboSalesDistrict.setValue(curRecord.data.SalesDistrict);
            HQ.combo.expand(App.cboSalesDistrict, ',');
        }
        var dt = HQ.store.findInStore(App.cboSalesDistrict.getStore(), ["District"], App.cboSalesDistrict.getValue());
        if (!dt) {
            curRecord.data.SalesDistrict = '';
            App.cboSalesDistrict.setValue("");
        }
        else {
            HQ.combo.expand(App.cboSalesDistrict, ',');
        }
    });
};



var cboState_Change = function (sender, e) {
    App.cboCity.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord && curRecord.data.City) {
            App.cboCity.setValue(curRecord.data.City);
        }
        var dt = HQ.store.findInStore(App.cboCity.getStore(), ["City"], [App.cboCity.getValue()]);
        if (!dt) {
            curRecord.data.City = '';
            App.cboCity.setValue("");
        }

        App.cboDistrict.getStore().load(function () {
            var curRecord = App.frmMain.getRecord();
            if (curRecord && curRecord.data.District) {
                App.cboDistrict.setValue(curRecord.data.District);
                HQ.combo.expand(App.cboDistrict, ',');
            }
            var dt = HQ.store.findInStore(App.cboDistrict.getStore(), ["District"], App.cboDistrict.getValue());
            if (!dt) {
                curRecord.data.District = '';
                App.cboDistrict.setValue("");
            }
            else {
                HQ.combo.expand(App.cboDistrict, ',');
            }
        });

    });
};

var cboCountry_grd_Change = function (sender, e) {
    App.cboState_grd.getStore().load(function () {

        var curRecord = App.frmMain.getRecord();
        if (curRecord != undefined)
            if (curRecord.data.State) {
                App.cboState_grd.setValue(curRecord.data.State);
            }
        var dt = HQ.store.findInStore(App.cboState_grd.getStore(), ["State"], [App.cboState_grd.getValue()]);
        if (!dt) {
            curRecord.data.State = '';
            App.cboState_grd.setValue("");
        }
    });
};

var cboState_grd_Change = function (sender, e) {
    App.cboCity_grd.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord && curRecord.data.City) {
            App.cboCity_grd.setValue(curRecord.data.City);
        }
        var dt = HQ.store.findInStore(App.cboCity_grd.getStore(), ["City"], [App.cboCity_grd.getValue()]);
        if (!dt) {
            //App.cboCity_grd.clearValue();
            curRecord.data.City = '';
            App.cboCity_grd.setValue("");
        }
    });
};

var cboDistrict_Change = function (sender, value) {
    //var curRecord = App.frmMain.getRecord();
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
var btnAddSAManager_Click = function () {
    App.winUsers.show();
    App.stoUsers.reload();
};
var btnAdd_Click = function () {
    var listUSers = '';
    var allData = App.stoUsers.snapshot || App.stoUsers.allData || App.stoUsers.data;
    for (var i = 0; i < allData.length; i++) {
        if (allData.items[i].data.CheckUser == true) {
            listUSers += allData.items[i].data.UserName + ', ';
        }
    }
    App.txtManager.setValue(listUSers);
    App.winUsers.hide();
};

var btnCancel_Click = function () {
    App.winUsers.hide();
};
var CheckUser_Change = function () {
    var allData = App.stoUsers.snapshot || App.stoUsers.allData || App.stoUsers.data;
    App.stoUsers.suspendEvents();
    for (var i = 0; i < allData.items.length; i++) {
        item = allData.items[i];
        if (App.CheckUser_All.getValue() == true) {
            item.set('CheckUser', true);
        }
        else
            item.set('CheckUser', false);
    }
    App.stoUsers.resumeEvents();
    App.grdUsers.view.refresh();
};
////////////Kiem tra combo chinh CpnyID
//khi co su thay doi du lieu cua cac conttol tren form
var frmChange = function () {
    if (App.stoSYS_Company.getCount() > 0) {
        App.frmMain.getForm().updateRecord();
    }
    HQ.isChange = HQ.store.isChange(App.stoSYS_Company) == false ? (HQ.store.isChange(App.stoSys_CompanyAddr)
                                                            == false ? (HQ.store.isChange(App.stoSYS_SubCompany)) : true) : true;
    HQ.common.changeData(HQ.isChange, 'SA00000');
    if (App.cboCpnyID.valueModels == null || HQ.isNew == true) {
        App.cboCpnyID.setReadOnly(false);
    }
    else {
        App.cboCpnyID.setReadOnly(HQ.isChange);
    }
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

//load store khi co su thay doi CpnyID
var stoLoad = function (sto) {
    HQ.isNew = false;
    HQ.common.lockItem(App.frmMain, false);

    App.cboCpnyID.forceSelection = true;
    App.cboCountry.forceSelection = false;
    App.cboState.forceSelection = false;
    App.cboCity.forceSelection = false;
    App.cboDistrict.forceSelection = false;
    //App.cboSalesState.forceSelection = false;
    //App.cboSalesDistrict.forceSelection = false;
    App.cboCountry.store.clearFilter();
    App.cboState.store.clearFilter();
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "CpnyID");
        record = sto.getAt(0);
        record.data.DatabaseName = HQ.DatabaseName;
        HQ.isNew = true;//record la new    
        App.cboCpnyID.forceSelection = false;
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboCpnyID.focus(true);//focus ma khi tao moi
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);
    App.stoSys_CompanyAddr.reload();

    if (App.cboCountry.value == record.data.Country) {
        cboCountry_Change(App.cboCountry, record.data.Country);
    }
    else if (App.cboState.value == record.data.State) {
        cboState_Change(App.cboState, record.data.State);
    }

    if (!HQ.isInsert && HQ.isNew) {
        App.cboCpnyID.forceSelection = true;
        HQ.common.lockItem(App.frmMain, true);
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }

    App.cboBranchOld.setValue('');
    App.cboSlsperID.setValue('');
    App.txtManager.setValue('');
};

/////////////////////////////// GIRD Sys_CompanyAddr /////////////////////////////////
var stoSys_CompanyAddr_Load = function (sto) {
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));

    if (HQ.isInsert) {
        var record = HQ.store.findRecord(App.stoSys_CompanyAddr, keys, ['']);
        if (!record) {
            HQ.store.insertBlank(sto, keys);
        }
    }

    //HQ.isFirstLoad = false; //sto load cuoi se su dung

    App.stoSYS_SubCompany.reload();
    frmChange();
    //sto load cuoi se su dung
    //if (_isLoadMaster) {
    //    HQ.common.showBusy(false);
    //}
};

var grdSys_CompanyAddr_BeforeEdit = function (editor, e) {
    if (e.field == 'State') {
        _country = e.record.data.Country;
        App.cboState_grd.store.reload();
        _stateOld = e.record.data.State;
    }
    if (e.field == 'Country') {
        _countryOld = e.record.data.Country;
    }
    if (e.field == 'City') {
        _state = e.record.data.State;
        App.cboCity_grd.store.reload();
    }
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdSys_CompanyAddr_Edit = function (item, e,old) {
    //Kiem tra cac key da duoc nhap se insert them dong moi

    if (e.field == 'Country') {
        if (_countryOld != e.record.data.Country) {
            e.record.set('State', "");
            e.record.set('City', "");
        }
    }
    if (e.field == 'State') {
        if (_stateOld != e.record.data.State) {
            e.record.set('City', "");
        }
    }

    HQ.grid.checkInsertKey(App.grdSys_CompanyAddr, e, keys);
    frmChange();
};

var grdSys_CompanyAddr_ValidateEdit = function (item, e) {
    //ko cho nhap key co ki tu dac biet, va kiem tra trung du lieu
    return HQ.grid.checkValidateEdit(App.grdSys_CompanyAddr, e, keys);
};

var grdSys_CompanyAddr_Reject = function (record) {
    //reject dong thay doi du lieu ve ban dau
    HQ.grid.checkReject(record, App.grdSys_CompanyAddr);
    frmChange();
};

/////////////////////////////// GIRD SYS_SubCompany /////////////////////////////////
var stoSYS_SubCompany_Load = function (sto) {
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys1);
        }
        HQ.isFirstLoad = false; //sto load cuoi se su dung
    }
    frmChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

var grdSYS_SubCompany_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys1)) return false;
};

var grdSYS_SubCompany_Edit = function (item, e) {
    if (e.field == "SubCpnyID") {
        if (e.value) {
            var objCpny = App.cboSubCpnyIDSA00000_pcCompanyAll.findRecord(['CpnyID'], [e.value]);
            if (objCpny) {
                e.record.set('SubCpnyName', objCpny.data.CpnyName);
            }
        }
    }
    HQ.grid.checkInsertKey(App.grdSYS_SubCompany, e, keys1);
    frmChange();
};

var grdSYS_SubCompany_ValidateEdit = function (item, e) {
    //ko cho nhap key co ki tu dac biet, va kiem tra trung du lieu
    return HQ.grid.checkValidateEdit(App.grdSYS_SubCompany, e, keys1);
};

var grdSYS_SubCompany_Reject = function (record) {
    //reject dong thay doi du lieu ve ban dau
    HQ.grid.checkReject(record, App.grdSYS_SubCompany);
    frmChange();
};

/////////////////////////////// PROCESS ///////////////////////////////////////////////

var save = function () {
    if (HQ.util.checkEmail(App.Email.getValue())) {
        if (App.frmMain.isValid()) {
            App.frmMain.updateRecord();

            App.frmMain.submit({
                waitMsg: HQ.common.getLang("WaitMsg"),
                url: 'SA00000/Save',
                params: {
                    lstSYS_Company: Ext.encode(App.stoSYS_Company.getRecordsValues()),
                    lstSys_CompanyAddr: HQ.store.getData(App.stoSys_CompanyAddr),
                    lstSYS_SubCompany: HQ.store.getData(App.stoSYS_SubCompany),
                    status: App.cboStatus.getValue(),
                },
                success: function (msg, data) {
                    HQ.message.show(201405071);
                    CpnyID = data.result.CpnyID;
                    HQ.isChange = false;
                    HQ.isFirstLoad = true;
                    App.cboCpnyID.getStore().load({
                        callback: function () {
                            if (Ext.isEmpty(App.cboCpnyID.getValue())) {
                                App.cboCpnyID.setValue(CpnyID);
                                App.stoSYS_Company.reload();
                            }
                            else {
                                App.cboCpnyID.setValue(CpnyID);
                                App.stoSYS_Company.reload();
                            }
                        }
                    });
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });

        }
    }
};


// Submit the deleted data into server side
var deleteData = function (item) {
    if (item == "yes") {
        if (HQ.focus == 'header') {
            if (App.frmMain.isValid()) {
                App.frmMain.updateRecord();
                App.frmMain.submit({
                    waitMsg: HQ.common.getLang("DeletingData"),
                    url: 'SA00000/DeleteAll',
                    timeout: 7200,
                    success: function (msg, data) {
                        App.cboCpnyID.getStore().load();
                        menuClick("new");
                    },
                    failure: function (msg, data) {
                        HQ.message.process(msg, data, true);
                    }
                });
            }

        }
        else if (HQ.focus == 'grdSys_CompanyAddr') {
            App.grdSys_CompanyAddr.deleteSelected();
            frmChange();
        }
        else if (HQ.focus == 'grdSYS_SubCompany') {
            App.grdSYS_SubCompany.deleteSelected();
            frmChange();
        }
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        if (HQ.isNew)
            App.cboCpnyID.setValue('');
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSYS_Company.reload();
    }
};

var renderBranchName = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboSubCpnyIDSA00000_pcCompanyAll.findRecord("CpnyID", rec.data.SubCpnyID);
    if (record) {
        return record.data.CpnyName;
    }
    else {
        return value;
    }
};

var renderCountry = function (value, metaData, record, row, col, store, gridView) {
    var r = HQ.store.findRecord(App.cboCountry_grd.store, ['CountryID'], [record.data.Country]);
    if (Ext.isEmpty(r))
        return value;
    else
        return r.data.Descr;
};

var renderState = function (value, metaData, record, row, col, store, gridView) {
    var r = HQ.store.findRecord(App.cboState_grd.store, ['State'], [record.data.State]);
    if (Ext.isEmpty(r))
        return value;
    else
        return r.data.Descr;
};
var joinParams = function (multiCombo) {
    var returnValue = "";
    if (multiCombo.value && multiCombo.value.length) {
        returnValue = multiCombo.value.join();
    }
    else {
        if (multiCombo.getValue()) {
            returnValue = multiCombo.rawValue;
        }
    }
    return returnValue;
}

var stringCountry = function (record) {

    if (this.dataIndex == 'Country') {
        App.cboCountry_grd.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboCountry_grd.store, "CountryID", "Descr");
    }

    return HQ.grid.filterString(record, this);
}


//////////////////////////
var btnState_TriggerClick = function () {
    _listState = App.txtSalesState.getValue();// joinParams(App.cboMailTo);
    //App.chkActive_All.setValue(false);
    App.frmMain.mask();
    //App.stoState.removeAll();
    App.stoState.reload();
    App.winState.show();
}

//var winState_beforeShow = function () {
//    App.frmMain.mask();
//    App.winState.mask();
//    App.winState.setHeight(App.frmMain.getHeight() - 20);
//    App.winState.setWidth(App.frmMain.getWidth() - 10);
//};
var winState_beforeShow = function () {
    App.frmMain.mask();
    App.winState.mask();
}


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
    App.frmMain.unmask();
};

var btnCancelState_Click = function () {
    App.winState.hide();
    App.frmMain.unmask();
};

var stoState_Load = function (sto) {
    HQ.common.showBusy(false);
    App.winState.unmask();
};

var chkActiveAll_Change = function (sender, value, oldValue) {
    if (sender.hasFocus) {
        var store = App.stoState;
        var allRecords = store.allData ;
        store.suspendEvents();
        allRecords.each(function (record) {
            record.set('Selected', value);
        });
        store.resumeEvents();
        App.grdState.view.refresh();
    }
};

var btnDistrict_TriggerClick = function () {
    _listDistrict = App.txtSalesDistrict.getValue();// joinParams(App.cboMailTo);
    App.frmMain.mask();
    //App.stoDistrict.removeAll();
    App.stoDistrict.reload();
    App.winDistrict.show();
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
    App.frmMain.unmask();
};

var btnCancelDistrict_Click = function () {
    App.winDistrict.hide();
    App.frmMain.unmask();
};

var chkActiveDistrictAll_Change = function (sender, value, oldValue) {
    if (sender.hasFocus) {
        var store = App.stoDistrict;
        var allRecords = store.allData;
        store.suspendEvents();
        allRecords.each(function (record) {
            record.set('Selected', value);
        });
        store.resumeEvents();
        App.grdDistrict.view.refresh();
    }
};
var stoDistrict_Load = function (sto) {
    HQ.common.showBusy(false);
    App.winDistrict.unmask();
};

var winDistrict_beforeShow = function () {
    App.frmMain.mask();
    App.winDistrict.mask();
    App.winDistrict.setHeight(App.frmMain.getHeight() - 20);
    App.winDistrict.setWidth(App.frmMain.getWidth() - 10);
};

var txtSalesState_Change = function (sender, e, oldValue) {
    if (!HQ.isFirstLoad && e != oldValue) {
        App.txtSalesDistrictDescr.setValue("");
        App.txtSalesDistrict.setValue("");
    }
};


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