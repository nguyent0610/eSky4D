var keys = ['CpnyID'];
var fieldsCheckRequire = ["CpnyID"];
var fieldsLangCheckRequire = ["CpnyID"];

var prefixvalue = "";
var lastfixvalue = "";
var shownextlotserial = "";
var _focusNo = 0;
var _Source = 0;
var _maxSource = 13;
var _isLoadMaster = false;

var pnl_render = function (cmd) {
    cmd.getEl().on('mousedown', function () {
        if (cmd.id == 'pnlCpnyID') {
            _focusNo = 1;
        }
        else {//pnlHeader
            _focusNo = 0;
        }
    });
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (_focusNo == 0) {
                HQ.combo.first(App.cboClassID, HQ.isChange);
            }
            else if (_focusNo == 1) {
                HQ.grid.first(App.grdCpny);
            }
            break;
        case "prev":
            if (_focusNo == 0) {
                HQ.combo.prev(App.cboClassID, HQ.isChange);
            }
            else if (_focusNo == 1) {
                HQ.grid.prev(App.grdCpny);
            }
            break;
        case "next":
            if (_focusNo == 0) {
                HQ.combo.next(App.cboClassID, HQ.isChange);
            }
            else if (_focusNo == 1) {
                HQ.grid.next(App.grdCpny);
            }
            break;
        case "last":
            if (_focusNo == 0) {
                HQ.combo.last(App.cboClassID, HQ.isChange);
            }
            else if (_focusNo == 1) {
                HQ.grid.last(App.grdCpny);
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
            if (_focusNo == 0) {
                App.cboClassID.clearValue();
                App.stoIN_ProductClass.reload();
            }
            else{
                if (HQ.isInsert) {
                    HQ.grid.insert(App.grdCpny);
                }
            }
            break;
        case "delete":
            if (_focusNo == 0) {
                if (App.cboClassID.getValue()) {
                    if (HQ.isDelete) {
                        HQ.message.show(11, '', 'deleteData');
                    }
                } else {
                    menuClick('new');
                }
            }
            else {
                if (App.slmCpny.selected.items[0] != undefined) {
                    if (App.slmCpny.selected.items[0].data.CpnyID != "") {
                        if (HQ.isDelete) {
                            HQ.message.show(11, '', 'deleteData');
                        }
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain)
                    && HQ.store.checkRequirePass(App.stoCpny, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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

var checkLoad = function () {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        HQ.common.showBusy(false);
        _Source = 0;
    }
};

var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    App.tabDetail.child('#pnlLotSerial').tab.hide();

    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboClassID.getStore().addListener('load', checkLoad);
    App.cboDfltInvtType.getStore().addListener('load', checkLoad);
    App.cboDfltSource.getStore().addListener('load', checkLoad);
    App.cboDfltValMthd.getStore().addListener('load', checkLoad);
    App.cboDfltLotSerTrack.getStore().addListener('load', checkLoad);
    App.cboBuyer.getStore().addListener('load', checkLoad);
    App.cboMaterialType.getStore().addListener('load', checkLoad);
    App.cboDfltSite.getStore().addListener('load', checkLoad);
    App.cboDfltSlsTaxCat.getStore().addListener('load', checkLoad);
    App.cboDfltLotSerAssign.getStore().addListener('load', checkLoad);
    App.cboDfltLotSerMthd.getStore().addListener('load', checkLoad);
    App.cboDfltLotSerFxdTyp.getStore().addListener('load', checkLoad);
    App.cboCpnyID.getStore().addListener('load', checkLoad);

    App.cboDfltStkUnit.getStore().addListener('load', checkLoad_cboDfltStkUnit);

    App.cboClassID.getStore().reload();
    App.cboDfltInvtType.getStore().reload();
    App.cboDfltSource.getStore().reload();
    App.cboDfltValMthd.getStore().reload();
    App.cboDfltLotSerTrack.getStore().reload();
    App.cboBuyer.getStore().reload();
    App.cboDfltStkUnit.getStore().reload();
    App.cboMaterialType.getStore().reload();
    App.cboDfltSite.getStore().reload();
    App.cboDfltSlsTaxCat.getStore().reload();
    App.cboDfltLotSerAssign.getStore().reload();
    App.cboDfltLotSerMthd.getStore().reload();
    App.cboDfltLotSerFxdTyp.getStore().reload();
    App.cboCpnyID.getStore().reload();
};

var checkLoad_cboDfltStkUnit = function () {
    App.cboDfltPOUnit.store.reload();
    App.cboDfltSOUnit.store.reload();
};

var cboClassID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoIN_ProductClass.loading) {
        App.stoIN_ProductClass.reload();
    }
};

var cboClassID_Select = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoIN_ProductClass.loading) {
        App.stoIN_ProductClass.reload();
    }
};

var cboClassID_Expand = function (sender, value) {
    if (HQ.isChange) {
        App.cboClassID.collapse();
    }
};

var cboClassID_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {
        menuClick('new');
    }
};

var cboDfltStkUnit_Change = function (sender, value) {
    //HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoIN_ProductClass.loading && !App.cboDfltStkUnit.store.loading && sender.hasFocus) {
        App.cboDfltPOUnit.store.reload();
        App.cboDfltSOUnit.store.reload();
    }
};

var cboDfltStkUnit_Select = function (sender, value) {
    //HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoIN_ProductClass.loading && !App.cboDfltStkUnit.store.loading && sender.hasFocus) {
        App.cboDfltPOUnit.store.reload();
        App.cboDfltSOUnit.store.reload();
    }
};
var chkPublic_Change = function (item,value,oldValue) {
    if (value) {
        App.tabDetail.child('#pnlCpnyID').tab.hide();
        if (App.tabDetail.activeTab.id == "pnlCpnyID") {
            App.tabDetail.setActiveTab(App.pnlDfltInfo);
        }
    } else {
        App.tabDetail.child('#pnlCpnyID').tab.show();
    }
};

var cboDfltLotSerTrack_Change = function (a, value) {
    if (value) {
        if ((value == "L" || value == "S")) {
            App.tabDetail.child('#pnlLotSerial').tab.show();
            prefixvalue = App.txtDfltLotSerFxdVal.getValue();
            lastfixvalue = App.txtDfltLotSerNumVal.getValue();
            shownextlotserial = prefixvalue + lastfixvalue;
            App.lblShowNextLotSerial.setValue(shownextlotserial);
        } else {
            App.tabDetail.child('#pnlLotSerial').tab.hide();
        }
    }
};

var frmChange = function () {
    if (App.stoIN_ProductClass.getCount() > 0) {
        App.frmMain.getForm().updateRecord();
    }
    HQ.isChange = HQ.store.isChange(App.stoIN_ProductClass) == false ? (HQ.store.isChange(App.stoCpny)
                                                                //== false ? (HQ.store.isChange(App.stoOM_SalesOffCust)
                                                                //== false ? (HQ.store.isChange(App.stoOM_SalesOffCustClass)
                                                                //== false ? HQ.store.isChange(App.stoOM_SalesOffCpny) : true) : true) : true
                                                                ): true;
    HQ.common.changeData(HQ.isChange, 'IN20200');
    if (App.cboClassID.valueModels == null || HQ.isNew == true)
        App.cboClassID.setReadOnly(false);
    else App.cboClassID.setReadOnly(HQ.isChange);

};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoLoad = function (sto) {
    HQ.isNew = false;
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboClassID.forceSelection = true;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "ClassID");
        record = sto.getAt(0);
        HQ.isNew = true;   
        App.cboClassID.forceSelection = false;
        HQ.common.setRequire(App.frmMain);        
        App.cboClassID.focus(true);
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);

    App.stoCpny.reload();
};

var stoCpny_Load = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
    frmChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

var grdCpny_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdCpny_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdCpny, e, keys);
    frmChange();
};

var grdCpny_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdCpny, e, keys);
};

var grdCpny_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdCpny);
    frmChange();
};

var cboDfltLotSerFxdTyp_Change = function (sender, e) {
    if (e) {
        App.txtDfltLotSerFxdVal.setValue('');
        if (e == "D") {
            App.txtDfltLotSerFxdLen.setValue('8');
            App.txtDfltLotSerFxdVal.setValue(HQ.IN20200Date);
        }
    }
};

var cboDfltStkUnit_Change = function (sender, e) {
    App.cboDfltPOUnit.getStore().reload();
    App.cboDfltSOUnit.getStore().reload();
};

var PrefixValue_Change = function (sender, e) {
    if (App.txtDfltLotSerFxdVal.value.length > App.txtDfltLotSerFxdLen.value) {
        HQ.message.show(22, e.value);
        App.txtDfltLotSerFxdVal.setValue(sender.originalValue);
    } else {
        prefixvalue = App.txtDfltLotSerFxdVal.getValue();
        shownextlotserial = prefixvalue + lastfixvalue;
        App.lblShowNextLotSerial.setValue(shownextlotserial);
    }
};

var LastFixValue_Change = function (sender, e) {
    if (App.txtDfltLotSerNumVal.value.length > App.txtDfltLotSerNumLen.value) {
        HQ.message.show(22, e.value);
        App.txtDfltLotSerNumVal.setValue(sender.originalValue);
    } else {
        lastfixvalue = App.txtDfltLotSerNumVal.getValue();
        shownextlotserial = prefixvalue + lastfixvalue;
        App.lblShowNextLotSerial.setValue(shownextlotserial);
    }
};

var NextShowNextLotSerial_AfterRender = function (value) {
    prefixvalue = App.txtDfltLotSerFxdVal.getValue();
    lastfixvalue = App.txtDfltLotSerNumVal.getValue();
    shownextlotserial = prefixvalue + lastfixvalue;
    App.lblShowNextLotSerial.setValue(shownextlotserial);
};


var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.updateRecord();
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'IN20200/Save',
            params: {
                lstIN_ProductClass: Ext.encode(App.stoIN_ProductClass.getRecordsValues()),
                lstCpny: HQ.store.getData(App.stoCpny)
               
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                var ClassID = data.result.ClassID;
                App.cboClassID.getStore().load(function () {
                    App.cboClassID.setValue(ClassID);
                });
                HQ.isFirstLoad = true;
                App.stoIN_ProductClass.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        if (_focusNo == 0) {
            if (App.frmMain.isValid()) {
                App.frmMain.updateRecord();
                App.frmMain.submit({
                    waitMsg: HQ.common.getLang("DeletingData"),
                    url: 'IN20200/DeleteAll',
                    timeout: 7200,
                    success: function (msg, data) {
                        App.cboClassID.getStore().load();
                        menuClick("new");
                    },
                    failure: function (msg, data) {
                        HQ.message.process(msg, data, true);
                    }
                });
            }

        }
        else if (_focusNo == 1) {
            App.grdCpny.deleteSelected();
            frmChange();
        }
    }
};

function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoIN_ProductClass.reload();
    }
};

