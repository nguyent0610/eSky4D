//// Declare //////////////////////////////////////////////////////////
var keys = ['AddrID'];
var fieldsCheckRequire = [""];
var fieldsLangCheckRequire = [""];

var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;
var _listState = '';
var _listDistrict = '';
var key = false;
var keycheckChange;

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
            keycheckChange = newRecord;
            App.winLocation.setTitle("New")
            App.winLocation.show();
            HQ.common.setRequire(App.frmDetail);
            App.txtCpnyID.setReadOnly(false);
            App.txtCpnyID.setValue('');
            App.txtCpnyName.setValue('');
            App.Address.setValue('');
            //App.Address1.setValue('');
            App.Address2.setValue('');           
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
            App.cboOwner.setValue('');
            App.Plant.setValue('');
            App.Deposit.setValue('');
            App.CreditLimit.setValue('');
            App.MaxValue.setValue('');
            App.txtSalesDistrict.setValue('');
            App.txtSalesDistrictDescr.setValue('');
            App.txtSalesState.setValue('');
            App.txtSalesStateDescr.setValue('');
            App.CountSiteID.setValue('');

            App.txtSAManager.setValue('');
            App.txtManager.setValue('');
            App.cboBranchOld.setValue('');
            App.cboSlsperID.setValue('');
            App.cboTDisplayID.setValue('');
            App.cboAccumulatedID.setValue('');

            App.stoSys_CompanyAddr.reload();
            App.cboBranchOld.store.reload();
            App.txtSalesStateDescr.setVisible(HQ.allowSalesState);
            App.btnSalesState.setVisible(HQ.allowSalesState);
            App.txtSalesDistrictDescr.setVisible(HQ.allowSalesDistrict);
            App.btnSalesDistrict.setVisible(HQ.allowSalesDistrict);
            App.txtSalesDistrictDescr.allowBlank = !HQ.allowSalesDistrict;
            App.txtSalesStateDescr.allowBlank = !HQ.allowSalesState;
            App.CountSiteID.setVisible(HQ.CountSiteID);
            App.CountSiteID.allowBlank = !HQ.CountSiteID;
            App.cboOwner.allowBlank = !HQ.allowOwer;
            App.cboOwner.isValid();
            App.Address2.allowBlank = !HQ.allowAddress2;
            App.Address2.isValid();
            App.frmDetail.validate();

            if (HQ.allowProccess !== 1) {
                App.tabDetail.child('#pnlProcess').tab.hide();
            }
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
    HQ.isFirstLoad = true;
    
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    checkLoad();
    if (HQ.showCreditChkRule) {
        App.CreditChkRuleDescr.show();
        //App.cboCheckCreditRule.allowBlank = false;
    }
    else {
        App.CreditChkRuleDescr.hide();
        //App.cboCheckCreditRule.allowBlank = true;
    }
    App.frmMain.isValid();
};

var frmChange = function () {
    HQ.isChange = frmDetail_CheckChange()|| HQ.store.isChange(App.stoSys_CompanyAddr);    
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

var grdState_Edit = function (item, e,value) {
    //if (e.field == "Selected") {
    //    if (e.value==true) {
    //        App.chkActive_All.events['change'].suspend();
    //        App.chkActive_All.setValue(true);
    //        App.chkActive_All.events['change'].resume();
    //    }
    //    else {
    //        var store = App.stoState;
    //        var allRecords = store.snapshot;
    //        var check1 = false;
    //        store.suspendEvents();
    //        for (var i = 0; i < allRecords.items.length; i++) {
    //            if (allRecords.items[i].data.Selected) {
    //                check1 = true;
    //                break;
    //            }
    //        }
    //        App.chkActive_All.events['change'].suspend();
    //        App.chkActive_All.setValue(check1);
    //        App.chkActive_All.events['change'].resume();
    //    }
    //    App.grdState.view.refresh();
    //}
    
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
        HQ.message.show(2018013112, App.txtCpnyID.fieldLabe);
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
    if (Ext.isEmpty(App.txtSalesDistrictDescr.getValue()) && HQ.allowSalesDistrict) {
        HQ.message.show(15, App.txtSalesDistrictDescr.fieldLabel);
        return;
    }
    if (Ext.isEmpty(App.txtSalesStateDescr.getValue()) && HQ.allowSalesState) {
        HQ.message.show(15, App.txtSalesStateDescr.fieldLabel);
        return;
    }
    if (Ext.isEmpty(App.CountSiteID.getValue()) && HQ.CountSiteID) {
        HQ.message.show(15, App.CountSiteID.fieldLabel);
        return;
    }
    if (HQ.util.checkEmail(App.Email.getValue())) {
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
                    compID: App.txtCpnyID.getValue(),
                    txtManager: App.txtManager.getValue(),
                    cboBranchOld: App.cboBranchOld.getValue(),
                    SlsperID: App.cboSlsperID.getValue(),
                    DisplayID: App.cboTDisplayID.getValue(),
                    AccumulateID: App.cboAccumulatedID.getValue(),
   

                   
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

function closewin(item) {
    if (item == 'no') {
        App.stoSYS_Cpny.rejectChanges();
        App.winLocation.hide();
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        
        //App.stoSys_CompanyAddr.reload();
    }
};
var btnEdit_Click = function (record) {
 
    App.frmDetail.loadRecord(record);
    keycheckChange = record;
    //App.txtCpnyID.setValue(record.data.CpnyID.split(','));
    //App.txtUserID.setValue(record.data.UserName);

   

    App.txtCpnyID.setReadOnly(true);
    if (App.cboStatus.getValue() == "IN") {
        App.dtpStopUsing.setReadOnly(false);
        App.txtReasonStopUsing.setReadOnly(false);
        App.dtpStopUsing.setValue(HQ.bussinessDate);
        App.dtpStopUsing.allowBlank = false;
        App.txtReasonStopUsing.allowBlank = false;
        App.txtReasonStopUsing.isValid();
    }
    else {
        App.dtpStopUsing.setReadOnly(true);
        App.txtReasonStopUsing.setReadOnly(true);
        App.dtpStopUsing.allowBlank = true;
        App.txtReasonStopUsing.allowBlank = true;
        App.txtReasonStopUsing.isValid();
        App.dtpStopUsing.setValue('');
    }
    App.winLocation.setTitle("Edit");
    HQ.isNew = false;
    App.winLocation.show();
    App.stoSys_CompanyAddr.reload();
    App.cboOwner.allowBlank = !HQ.allowOwer;
    App.cboOwner.isValid();
    App.Address2.allowBlank = !HQ.allowAddress2;
    App.Address2.isValid();
    App.txtSalesStateDescr.setVisible(HQ.allowSalesState);
    App.btnSalesState.setVisible(HQ.allowSalesState);
    App.txtSalesDistrictDescr.setVisible(HQ.allowSalesDistrict);
    App.btnSalesDistrict.setVisible(HQ.allowSalesDistrict);
    App.txtSalesDistrictDescr.allowBlank = !HQ.allowSalesDistrict;
    App.txtSalesStateDescr.allowBlank = !HQ.allowSalesState;
    App.CountSiteID.setVisible(HQ.CountSiteID);
    App.CountSiteID.allowBlank = !HQ.CountSiteID;
    App.frmDetail.validate();

    App.txtSAManager.setValue(record.data.SAManager);
    App.txtManager.setValue('');
    App.cboBranchOld.setValue('');
    App.cboSlsperID.setValue('');
    App.cboTDisplayID.setValue('');
    App.cboAccumulatedID.setValue('');
    App.cboBranchOld.store.reload();

    HQ.isChange = false;

    if (HQ.allowProccess !== 1) {
        App.tabDetail.child('#pnlProcess').tab.hide();
    }

    if (HQ.showCreditChkRule) {
        App.cboCheckCreditRule.show();
        App.cboCheckCreditRule.allowBlank = false;
    }
    else {
        App.cboCheckCreditRule.hide();
        App.cboCheckCreditRule.allowBlank = true;
    }
    App.cboCheckCreditRule.isValid();
};

var btnLocationCancel_Click = function () {
    frmDetail_Change();
    if (!HQ.isChange) {
        App.winLocation.hide();
    }
    else {
        HQ.message.show(5, '', 'closewin');
        //HQ.message.show(20140811, App.txtCpnyID.fieldLabe);
    }
};

var btnLocationOK_Click = function () {
    //if (HQ.isUpdate == false && HQ.isInsert) {
    //    return;
    //}
    if (App.CountSiteID.allowBlank == false)
    {
        if (App.CountSiteID.getValue() != '1' && App.CountSiteID.getValue() != '2' && App.CountSiteID.getValue() != '3' && App.CountSiteID.getValue() != '4') {
            if (App.CountSiteID.getValue() != null) {
                HQ.message.show(2018051160);
                return;
            }
            else {
                if (HQ.form.checkRequirePass(App.frmDetail)) {
                    if (App.txtCpnyID.getValue() == "") {
                        HQ.message.show(15, App.txtCpnyID.fieldLabel);
                        return;
                    }
                }
            }
        }
        else {
            if (HQ.form.checkRequirePass(App.frmDetail)) {
                if (App.txtCpnyID.getValue() == "") {
                    HQ.message.show(15, App.txtCpnyID.fieldLabel);
                    return;
                }
                save();
            }
        }
    }
    else
    {
        if (HQ.form.checkRequirePass(App.frmDetail)) {
            if (App.txtCpnyID.getValue() == "") {
                HQ.message.show(15, App.txtCpnyID.fieldLabel);
                return;
            }
            save();
        }
    }
    
   

    
};

var cboCountry_Change = function (sender, e, oldValue) {
    if ((oldValue!=undefined) || sender.hasFocus) {
        if (e != oldValue) {
            App.cboState.setValue("");
            App.cboState.store.reload();
            App.cboCity.setValue("");
            App.cboCity.store.reload();
            App.cboDistrict.setValue("");
            App.cboDistrict.store.reload();
            App.txtSalesDistrict.setValue("");
            App.txtSalesDistrictDescr.setValue("");
            App.txtSalesState.setValue("");
            App.txtSalesStateDescr.setValue("");
            App.CountSiteID.setValue("");
        }
    }


    //App.cboState.getStore().load(function () {
    //    var curRecord = App.frmDetail.getRecord();
    //    if (curRecord != undefined)
    //        if (curRecord.data.State) {
    //            App.cboState.setValue(curRecord.data.State);
    //        }
    //    var dt = HQ.store.findInStore(App.cboState.getStore(), ["State"], [App.cboState.getValue()]);
    //    if (!dt) {
    //        curRecord.data.State = '';
    //        App.cboState.setValue("");
    //    }
    //    //if (App.cboState.value == curRecord.data.State) {
    //    //    cboState_Change(App.cboState, curRecord.data.State);
    //    //}
    //});

    //App.cboState.store.reload();
};

var cboZone_Change = function (sender, e, oldValue) {
    if (sender.hasFocus || (oldValue != undefined)) {
        if (e != oldValue) {
          //  App.cboTerritory.setValue("");            
        }
    }
};
var cboStatus_Change = function (sender, e, oldValue) {
    if(App.cboStatus.getValue()=="IN")
    {
        App.dtpStopUsing.setReadOnly(false);
        App.txtReasonStopUsing.setReadOnly(false);
        App.dtpStopUsing.setValue(HQ.bussinessDate);
        App.dtpStopUsing.allowBlank = false;
        App.txtReasonStopUsing.allowBlank = false;
        App.txtReasonStopUsing.isValid();
    }
    else
    {
        App.dtpStopUsing.setReadOnly(true);
        App.txtReasonStopUsing.setReadOnly(true);
        App.dtpStopUsing.allowBlank = true;
        App.txtReasonStopUsing.allowBlank = true;
        App.txtReasonStopUsing.isValid();
        App.dtpStopUsing.setValue('');
    }
}
//var cboTerritory_Change = function (sender, e) {
//    if (sender.hasFocus) {
//        //App.cboState.getStore().load(function () {
//            var curRecord = App.frmDetail.getRecord();
//            if (curRecord != undefined)
//                if (curRecord.data.State) {
//                    App.cboState.setValue(curRecord.data.State);
//                }
//            var dt = HQ.store.findInStore(App.cboState.getStore(), ["State", "Territory"], [App.cboState.getValue(), App.cboTerritory.getValue()]);
//            if (!dt) {
//                curRecord.data.State = '';
//                App.cboState.setValue("");
//                App.cboCity.setValue("");
//                curRecord.data.City = '';
//                //App.cboDistrict.setValue("");
//                curRecord.data.District = '';
//            }
//            if (App.cboState.value == curRecord.data.State) {
//               // cboState_Change(App.cboState, curRecord.data.State);
//            }
//      //  });
//    }
//};

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

//var cboState_Change = function (sender, e) {
//    if (sender.hasFocus) {

//       //// App.cboDistrict.getStore().load(function () {
//       //     var curRecord = App.frmDetail.getRecord();
//       //     if (curRecord && curRecord.data.District) {
//       //         App.cboDistrict.setValue(curRecord.data.District);
//       //         HQ.combo.expand(App.cboDistrict, ',');
//       //     }
//       //     var dt = HQ.store.findInStore(App.cboDistrict.getStore(), ["District", "State", "Country"], [App.cboDistrict.getValue(), App.cboState.getValue(), App.cboCountry.getValue()]);
//       //     if (!dt) {
//       //         curRecord.data.District = '';
//       //         App.cboDistrict.setValue("");
//       //     }
//       //     else {
//       //         HQ.combo.expand(App.cboDistrict, ',');
//       //     }
//       //// });


 //Expand Territory
var cboState_Expand = function (combo) {
    if (App.cboCountry.getValue() != null && App.cboCountry.getValue() != "") {
        App.cboState.store.clearFilter();
        var country = App.cboCountry.getValue();
        var territory = App.cboTerritory.getValue();

        //HQ.combo.expand(this, ',');

        //var store = App.cboState.store;
        //// Filter data -- 
        //store.filterBy(function (record) {
        //    if (record) {
        //        if (record.data['Territory'].toString() == territory && record.data['Country'].toString() == country) {
        //            return record;
        //        }
        //    }
        //});
        var country = App.cboCountry.getValue();
        if (country == '' || country == null) {
            country = '@@@@@@@';
        }
        var territory = App.cboTerritory.getValue();
        if (territory == '' || territory == null) {
            territory = '@@@@@@';
        }
        App.cboState.store.filter('Country', country);
        App.cboState.store.filter('Territory', territory);
    }    
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
    ////if (HQ.isFirstLoad) {
    //    if (HQ.isInsert) {
    //        HQ.store.insertBlank(sto, keys);
    //    }
    //    //HQ.isFirstLoad = false; //sto load cuoi se su dung
    ////}
    ////App.stoSys_CompanyAddr.reload();
    if (HQ.isInsert) {
        var record = HQ.store.findRecord(App.stoSys_CompanyAddr, keys, ['']);
        if (!record) {
            HQ.store.insertBlank(sto, keys);
        }
    }
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
    //App.stoState.removeAll();
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
    //if (App.txtSalesState.getValue != "" && App.txtSalesState.getValue != null) {
    //    App.chkActive_All.events['change'].suspend();
    //    App.chkActive_All.setValue(true);
    //    App.chkActive_All.events['change'].resume();
    //}    
    App.winState.unmask();
};

var chkActiveAll_Change = function (sender, value, oldValue) {
    if (sender.hasFocus) {
        var store = App.stoState;
        var allRecords = App.grdState.store.allData;
        store.suspendEvents();
        allRecords.each(function (record) {
            record.set('Selected', value);
        });
        store.resumeEvents();
        App.grdState.view.refresh();
    }
};

//var stoState_FilterChange = function (sender, value, oldValue) {
        //var store = App.stoState;
        //var allRecords = store.allData;
        //var check=false;
        //for (var i = 0; i < allRecords.items.length; i++) {
        //    if(allRecords.items[i].data.Selected){
        //        check = true;
        //        break;
        //    }
        //}
        //App.chkActive_All.events['change'].suspend();
        //App.chkActive_All.setValue(check);
        //App.chkActive_All.events['change'].resume();
        //App.grdState.view.refresh();
//};

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
    //App.stoDistrict.removeAll();
    App.stoDistrict.reload();
    App.winDistrict.show();
};

var grdSys_CompanyAddr_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSys_CompanyAddr);
    frmChange();
};

var stoDistrict_Load = function (sto) {
    HQ.common.showBusy(false);
    App.winDistrict.unmask();
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

    return record.dirty.grid.filterString(record, this);
};

var frmDetail_Change = function () {
    var record = App.frmDetail.getRecord();
    if (record) {
        //App.frmDetail.updateRecord();
        frmChange();
    }
}



function cboDistrict_Focus(items) {
    App.cboDistrict.store.clearFilter();
    var country = App.cboCountry.getValue();
    if (country == ''|| country==null) {
        country = '@@@';
    }
    var state = App.cboState.getValue();
    if (state == ''||state==null) {
        state = '@@@';
    }
    App.cboDistrict.store.filter('Country', country);
    App.cboDistrict.store.filter('State', state);
}


function cboState_Focus(items) {
    App.cboState.store.clearFilter();
    var country = App.cboCountry.getValue();
    if (country == '' || country==null) {
        country = '@@@';
    }
    var territory = App.cboTerritory.getValue();
    if (territory == '' || territory==null) {
        territory = '@@@@';
    }
    App.cboState.store.filter('Country', country);
    App.cboState.store.filter('Territory', territory);
}

function cboTerritory_Focus(items) {
    //App.cboTerritory.forceSelection = false;
    App.cboTerritory.store.clearFilter();
    var zone = App.cboZone.getValue();
    if (zone == ''||zone==null) {
        zone = '@@@@';
    }
    App.cboTerritory.store.filter('Zone', zone);
}

var cboTerritory_Change = function (sender, e, oldValue) {
    if ((oldValue != undefined) || sender.hasFocus) {
        if (e != oldValue) {
            App.cboDistrict.setValue("");
            App.cboState.setValue("");
            App.cboOwner.setValue("");
        }
    }
    App.cboOwner.store.reload();
};

//function cboTerritory_Change(items, newValue, oldValue) {
//    //Có chọn có giá trị
//    //if (items.hasFocus && (items.valueModels.length > 0 || (items.valueModels == null && items.rawValue == "" && items.allowBlank)))//newValue=null là trường hợp combo trống
//    //{
//    //    //App.cboDistrict.store.clearFilter();
//    //    //App.cboState.store.clearFilter();
//    //    App.cboState.setValue('');
//    //    App.cboCity.setValue('');
//    //    App.cboDistrict.setValue('');
//    //}
 
//    App.cboOwner.getStore().load(function () {
//        var curRecord = App.frmDetail.getRecord();
//        if (curRecord != undefined)
//            if (curRecord.data.Owner) {
//                App.cboOwner.setValue(curRecord.data.Owner);
//            }
            
//        var dt = HQ.store.findInStore(App.cboOwner.getStore(), ["Owner"], [App.cboOwner.getValue()]);
//        if (!dt) {
//            if (items.hasFocus) {
//                curRecord.data.Owner = '';
//                App.cboOwner.setValue("");
//            }
//        }
//    });
//}



function cboState_Change(sender, e, oldValue) {
    
    if ((oldValue != undefined) || sender.hasFocus) {
        if (e != oldValue) {
            App.cboCity.setValue("");
            App.cboCity.store.reload();
            App.cboDistrict.setValue("");
            App.cboDistrict.store.reload();
        }
    }
}
var frmDetail_CheckChange = function () {
    var status = App.cboStatus.getValue();
    var channel = App.cboChannel.getValue();
    var country = App.cboCountry.getValue();
    var cpnyType = App.cboCpnyType.getValue();
    var territory = App.cboTerritory.getValue();
    var zone = App.cboZone.getValue();
    var state = App.cboState.getValue();
    var owner = App.cboOwner.getValue();
    var district = App.cboDistrict.getValue();
    var deposit = App.Deposit.getValue();
    var creditLimit=App.CreditLimit.getValue();
    if (status == null) {
        status = "";
    }
    if (channel == null) {
        channel = "";
    }
    if (country == null) {
        country = "";
    }
    if (cpnyType == null) {
        cpnyType = "";
    }
    if (territory == null) {
        territory = "";
    }
    if (zone == null) {
        zone = "";
    }
    if (state == null) {
        state = "";
    }
    if (owner == null) {
        owner = "";
    }
    if (district == null) {
        district = "";
    }
    if (deposit == null) {
        deposit = 0;
    }
    if (creditLimit == null) {
        creditLimit = 0;
    }
    var isChange = false;
    if (keycheckChange != undefined) {
        if (keycheckChange.data.Address != App.Address.getValue() || keycheckChange.data.Address2 != App.Address2.getValue()
            ||keycheckChange.data.CpnyID != App.txtCpnyID.getValue()|| keycheckChange.data.CpnyName != App.txtCpnyName.getValue()||
            keycheckChange.data.CpnyName != App.txtCpnyName.getValue()||keycheckChange.data.Status != status||keycheckChange.data.Channel != channel||
            keycheckChange.data.Country != country||keycheckChange.data.CpnyType != cpnyType|| keycheckChange.data.District != district||
            keycheckChange.data.Email != App.Email.getValue()||keycheckChange.data.Fax != App.Fax.getValue()||keycheckChange.data.Owner != owner||
            keycheckChange.data.State != state|| keycheckChange.data.Zone != zone|| keycheckChange.data.Territory != territory||
            keycheckChange.data.SalesDistrict != App.txtSalesDistrict.getValue()|| keycheckChange.data.SalesState != App.txtSalesState.getValue()||
            keycheckChange.data.Tel != App.Tel.getValue()|| keycheckChange.data.TaxRegNbr != App.TaxRegNbr.getValue()|| keycheckChange.data.Deposit != deposit|| 
            keycheckChange.data.CreditLimit != creditLimit) {
            isChange = true;
        }
    }
    
    return isChange;
}

var stringFilter = function (record) {
    return HQ.grid.filterString(record, this);

};

var cboBranchOld_Change = function () {
    App.cboSlsperID.store.reload();
    App.cboTDisplayID.store.reload();
    App.cboAccumulatedID.store.reload();
}

var btnAddSAManager_Click = function () {
    App.winUsers.show();
    App.stoUsers.reload();
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

var creditChkRule_render = function (value) {
    var record = HQ.store.findRecord(App.cboCreditChkRulegrd.store, ["Code"], [value]);
    return (record) ? record.data.Descr : value;
};

var stringCreditChkRule = function (record) {
    if (this.dataIndex == 'State') {
        App.cboCreditChkRulegrd.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboCreditChkRulegrd.store, "Code", "Descr");
    }

    return HQ.grid.filterString(record, this);
};