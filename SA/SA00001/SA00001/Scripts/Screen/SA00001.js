//// Declare //////////////////////////////////////////////////////////
var keys = ['CpnyID', 'AddrID'];
var fieldsCheckRequire = [""];
var fieldsLangCheckRequire = [""];

var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;


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
            App.Address1.setValue('');
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
            App.Email.setValue('');
            App.Owner.setValue('');
            App.Plant.setValue('');
            App.Deposit.setValue('');
            App.CreditLimit.setValue('');
            App.MaxValue.setValue('');
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
                lstSys_CompanyAddr: HQ.store.getData(App.stoSys_CompanyAddr)
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
    if (sender.hasFocus) {
        App.cboState.getStore().load(function () {
            var curRecord = App.frmDetail.getRecord();
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
    }
};

var cboState_Change = function (sender, e) {
    if (sender.hasFocus) {
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

            App.cboDistrict.getStore().load(function () {
                var curRecord = App.frmDetail.getRecord();
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
    }
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
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        //HQ.isFirstLoad = false; //sto load cuoi se su dung
    }
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
    return HQ.grid.checkValidateEdit(App.grdSys_CompanyAddr, e, keys);
};

