﻿var _beginStatus = "H";
var _maxSoure = 1;
var _numSource = 0;
var _filterInfo = {
    distributor: "",
    slsperID: "",
    fromDate: null,
    toDate: null
};
var _Source = 0;
var _maxSource = 5;

var keysSurveyCriteria = ['CriteriaID'];//khóa của record
var valuesSurveyCriteria = [''];// giá trị mặc định của dòng mới

var keysSurveyInvt = ['CompID', 'CompInvtID'];//khóa của record

var fieldsCheckSurveyInvt = ["CompID", "CompInvtID"];
var fieldsLangSurveyInvt = ["CompID", "CompInvtID"];

var fieldsCheckSurveyCriteria = ["CriteriaID"];
var fieldsLangSurveyCriteria = ["CriteriaID"];

var keysInvt = ['InvtID'];
var fieldsCheckInvt = ["InvtID"];
var fieldsLangInvt = ["InvtID"];
var _invtID = "";
var regex = /^(\w*(\d|[a-zA-Z]|[\_@()+-.]))*$/
var checknew = false;
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoSaleProduct.reload();
        App.stoHeaderSurvey.reload();
        HQ.common.showBusy(false);
    }
};

var firstLoad = function () {

    HQ.util.checkAccessRight(); //Kiểm tra quyền Insert Update Delete để disable các button trên topbar(Bắt buộc)

    App.frmMain.isValid(); //Require các field yêu cầu trên man hình

    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));

    HQ.isFirstLoad = true;
    App.cboDistributor.getStore().addListener('load', checkLoad);
    App.cboStatus.getStore().addListener('load', checkLoad);
    App.cboCompID.getStore().addListener('load', checkLoad);
    App.cboCompInvtID.getStore().addListener('load', checkLoad);
    App.cboCriteriaID.getStore().addListener('load', checkLoad);
    App.slmDet.select(0)
};
var frmChange = function () {
    if (App.stoHeaderSurvey.getCount() > 0)
        App.frmMain.getForm().updateRecord();

    HQ.isChange = HQ.store.isChange(App.stoSurveyInvt) || HQ.store.isChange(App.stoSurveyCriteria) || HQ.store.isChange(App.stoHeaderSurvey) || HQ.store.isChange(App.stoSaleProduct);
    HQ.common.changeData(HQ.isChange, 'OM20090');

    //if (App.cboSurveyID.valueModels == null || HQ.isNew == true) {
    //    App.cboSurveyID.setReadOnly(false);
    //    App.cboDistributor.setReadOnly(false);
    //}
    //else {
    //    App.cboSurveyID.setReadOnly(HQ.isChange);
    //    App.cboDistributor.setReadOnly(HQ.isChange);
    //}
    if (HQ.store.isChange(App.stoSaleProduct) == true || HQ.store.isChange(App.stoSurveyInvt) == true || HQ.store.isChange(App.stoSurveyCriteria) == true)
    {
        App.cboSurveyID.setReadOnly(true);
        App.cboDistributor.setReadOnly(true);
    }
    else
    {
        App.cboSurveyID.setReadOnly(false);
        App.cboDistributor.setReadOnly(false);
    }
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoLoad = function (sto) {
    HQ.isNew = false;
    HQ.common.lockItem(App.frmMain, false);

    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "BranchID,SurveyID");
        record = sto.getAt(0);
        record.set('BranchID', App.cboDistributor.getValue());
        record.set('Status', 'H');
        record.set('FromDate', HQ.dateNow);
        record.set('ToDate', HQ.dateNow);
        HQ.isNew = true; //record la new 
        HQ.isFirstLoad = true;
        App.cboSurveyID.forceSelection = false;
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboSurveyID.focus(true); //focus ma khi tao moi
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);

    App.cboStatus.setReadOnly(true)
    App.stoSurveyInvt.reload();
    App.stoSurveyCriteria.reload();
    App.stoSaleProduct.reload();
    App.txtBranchName.setReadOnly(true);
    App.slmDet.select(0);
    IsReadOnlyHeader(App.cboStatus.getValue());
    if (!HQ.isInsert && HQ.isNew) {
        App.cboSurveyID.forceSelection = true;
        HQ.common.lockItem(App.frmMain, true);
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }

    if (_isLoadMaster) {
        HQ.common.showBusy(false);
        frmChange();
    }

    setTimeout(function () {
        //HQ.store.insertBlank(App.stoSurveyInvt, keysSurveyInvt);
        var record = HQ.store.findRecord(App.stoSurveyInvt, keysSurveyInvt, ['', '']);
        if (!record) {
            HQ.store.insertBlank(App.stoSurveyInvt, keysSurveyInvt);
        }
    }, 1000)
};
////////////

var Process = {
    
  
};

var Event = {
    Form: {

        cboDistributor_change: function (cbo, newValue, oldValue, eOpts) {
            if (newValue != null && newValue != "") {
                var BrachName = App.cboDistributor.store.findRecord("BranchID", newValue).data.CpnyName;
                App.txtBranchName.setValue(BrachName);
            }
            else {
                App.txtBranchName.setValue("");
            }
            App.cboSurveyID.setValue("");
            App.cboSurveyID.store.reload();
        },


        cboSurveyID_change: function (sender, value) {
            //if (!HQ.util.passNull(value) == '' && !HQ.util.passNull(value.toString()).match(regex)) {
            //    HQ.message.show(20140811);
            //    App.cboSurveyID.setValue("");
            //    return false;
            //}
            
            HQ.isFirstLoad = true;
            App.stoSaleProduct.reload();
            if (sender.valueModels != null && !App.stoHeaderSurvey.loading) {
                App.stoHeaderSurvey.reload();
            }
            checkSpecialChar(App.cboSurveyID.getValue());
            App.slmDet.select(0);
        },

        cboStatus_Change: function (cbo, newValue, oldValue, eOpts) {
            App.cboHandle.getStore().reload();
            App.cboHandle.setValue("N")
        },

        dtpFromDate_change: function (dtp, newValue, oldValue, eOpts) {
            App.dtpToDate.setMinValue(newValue);
            App.dtpToDate.validate();
            //if (checknew == false)
            //{
            //    if (App.dtpToDate.getValue() != null) {
            //        if (App.dtpToDate.getValue() < App.dtpFromDate.getValue()) {
            //            HQ.message.show(2016070401, [App.dtpToDate.fieldLabel, App.dtpFromDate.fieldLabel], '', true);
            //            App.dtpToDate.setValue('');
            //            App.dtpToDate.isValid();
            //        }
            //    }
            //}
            checknew = false;
        },

        menuClick : function (command) {
            switch (command) {
                case "first":
                    if (HQ.focus == 'header') {
                        HQ.isFirstLoad = true;
                        HQ.combo.first(App.cboSurveyID, HQ.isChange);
                    }
                    else if (HQ.focus == 'SaleProduct') {
                        HQ.grid.first(App.grdInvt);
                    }
                    else if (HQ.focus == 'Condition') {
                        HQ.grid.first(App.grdSurveyCriteria);
                    }
                    else if (HQ.focus == 'SurveyInvt') {
                        HQ.grid.first(App.grdSurveyInvt);
                    }
                    break;
                case "prev":
                    if (HQ.focus == 'header') {
                        HQ.isFirstLoad = true;
                        HQ.combo.prev(App.cboSurveyID, HQ.isChange);
                    }
                    else if (HQ.focus == 'SaleProduct') {
                        HQ.grid.prev(App.grdInvt);
                    }
                    else if (HQ.focus == 'Condition') {
                        HQ.grid.prev(App.grdSurveyCriteria);
                    }
                    else if (HQ.focus == 'SurveyInvt') {
                        HQ.grid.prev(App.grdSurveyInvt);
                    }
                    break;
                case "next":
                    if (HQ.focus == 'header') {
                        HQ.isFirstLoad = true;
                        HQ.combo.next(App.cboSurveyID, HQ.isChange);
                    }
                    else if (HQ.focus == 'SaleProduct') {
                        HQ.grid.next(App.grdInvt);
                    }
                    else if (HQ.focus == 'Condition') {
                        HQ.grid.next(App.grdSurveyCriteria);
                    }
                    else if (HQ.focus == 'SurveyInvt') {
                        HQ.grid.next(App.grdSurveyInvt);
                    }
                    break;
                case "last":
                    if (HQ.focus == 'header') {
                        HQ.isFirstLoad = true;
                        HQ.combo.last(App.cboSurveyID, HQ.isChange);
                    }
                    else if (HQ.focus == 'SaleProduct') {
                        HQ.grid.last(App.grdInvt);
                    }
                    else if (HQ.focus == 'Condition') {
                        HQ.grid.last(App.grdSurveyCriteria);
                    }
                    else if (HQ.focus == 'SurveyInvt') {
                        HQ.grid.last(App.grdSurveyInvt);
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
                        checknew = true;
                        if (HQ.focus == 'header') {
                            if (HQ.isChange) {
                                HQ.message.show(150, '', 'refresh');
                            } else {
                                
                                App.cboSurveyID.setValue('');


                                //App.cboSurveyID.store.reload();
                                //App.stoHeaderSurvey.reload();
                            }
                        }
                        else if (HQ.focus == 'Condition') {
                            HQ.grid.insert(App.grdSurveyCriteria, keysSurveyCriteria);
                        }
                        else if (HQ.focus == 'SurveyInvt') {
                            HQ.grid.insert(App.grdSurveyInvt, keysSurveyInvt);
                        }
                    }
                    break;
                case "delete":

                    if (HQ.isDelete) {
                        if (App.cboStatus.getValue() == _beginStatus) {
                            if (HQ.focus == 'header') {

                                if (App.cboSurveyID.getValue() != "" || App.cboSurveyID.getValue() != null) {

                                    HQ.message.show(11, '', 'deleteData');

                                } else {

                                    refresh('yes');

                                }

                            }

                            else if (HQ.focus == 'Condition') {

                                if (App.slmSurveyCriteria.selected.items[0] != undefined) {

                                    if (App.slmSurveyCriteria.selected.items[0].data.CriteriaID != "") {

                                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSurveyCriteria)], 'deleteData', true);

                                    }

                                }

                            }

                            else if (HQ.focus == 'SurveyInvt') {

                                if (App.slmDetailDet.selected.items[0] != undefined) {

                                    if (App.slmDetailDet.selected.items[0].data.CompID != "") {

                                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSurveyInvt)], 'deleteData', true);

                                    }

                                }

                            }
                            else if(HQ.focus=='SaleProduct')
                            {
                                if (App.slmDet.selected.items[0] != undefined) {
                                    if (HQ.isDelete) {
                                        if (App.slmDet.selected.items[0] != undefined) {
                                            if (App.slmDet.selected.items[0].data.InvtID != "") {
                                                HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdInvt)], 'deleteData', true);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else {
                            refresh('yes');
                        }
                    }

                    break;
                    //if (HQ.isDelete) {
                    //    if (App.cboSurveyID.getValue()) {
                    //        HQ.message.show(11, '', 'deleteData');
                    //    } else {
                    //        menuClick('new');
                    //    }
                    //}
                    //break;
                case "save":
                    if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                        if (HQ.store.checkRequirePass(App.stoSurveyInvt, keysSurveyInvt, fieldsCheckSurveyInvt, fieldsLangSurveyInvt)
                                && HQ.store.checkRequirePass(App.stoSurveyCriteria, keysSurveyCriteria, fieldsCheckSurveyCriteria, fieldsLangSurveyCriteria)) {
                            lstInvtID = App.grdInvt.store.snapshot || App.grdInvt.store.allData || App.grdInvt.store.data;
                            lstSurveyInvt =App.grdSurveyInvt.store.snapshot || App.grdSurveyInvt.store.allData || App.grdSurveyInvt.store.data;
                            var error = "";
                            if (lstInvtID != undefined)
                            {
                                for(var i =0 ;i< lstInvtID.length;i++)
                                {
                                    var objRecord = HQ.store.findInStore(App.stoSurveyInvt, ['InvtID'], [lstInvtID.items[i].data.InvtID]);
                                    if(objRecord==undefined)
                                    {
                                        error += (i + 1) + ", ";
                                    }
                                }
                            }
                            if (App.stoSurveyCriteria.data.items.length > 1) {
                                if (error != "")
                                {
                                    HQ.message.show(2018070561, [error], '', true);
                                    return;
                                }
                                else
                                {
                                    if (!App.frmMain.isValid()) {
                                        showFieldInvalid(App.frmMain);
                                        return false;
                                    }
                                    else {
                                        save();
                                    }
                                }
                            }
                            else {
                                HQ.message.show(20180615);
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
        }
    },

    Grid: {

        stoDet_load: function (sto) {
            _numSource++;
            if (_numSource == _maxSoure) {             

               
                App.slmDet.select(0);
                App.grdInvt.view.refresh();
                HQ.common.showBusy(false);
            }
            //if (HQ.isFirstLoad) {
            //    if (HQ.isInsert) {
            //        HQ.store.insertBlank(sto, keysInvt);
            //    }
            //}
            if (HQ.isInsert) {
                var record = HQ.store.findRecord(sto, keysInvt, ['', '']);
                if (!record) {
                    HQ.store.insertBlank(sto, keysInvt);
                }
            }
            App.slmDet.select(0);
        },

        stoSurveyInvt_load: function(sto){
            HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
            if (HQ.isFirstLoad) {
                if (HQ.isInsert) {
                    //HQ.store.insertBlank(sto, keysSurveyInvt);
                    var record = HQ.store.findRecord(sto, keysSurveyInvt, ['', '']);
                    if (!record) {
                        HQ.store.insertBlank(sto, keysSurveyInvt);
                    }
                }
            }
            frmChange();
            if (_isLoadMaster) {
                HQ.common.showBusy(false);
            }
        },

        stoSurveyCriteria_load: function (sto) {
            HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
            if (HQ.isFirstLoad) {
                //if (HQ.isInsert) {
                //    HQ.store.insertBlank(sto, keysSurveyCriteria);
                //    HQ.isFirstLoad = false;
                //}
                var record = HQ.store.findRecord(App.stoSurveyCriteria, keysSurveyCriteria, ['']);
                if (!record) {
                    HQ.store.insertBlank(sto, keysSurveyCriteria);
                }
            }
            frmChange();
            if (_isLoadMaster) {
                HQ.common.showBusy(false);
            }
        },

        grdSurveyInvt_Edit: function (item, e) {
            HQ.grid.checkInsertKey(App.grdSurveyInvt, e, keysSurveyInvt);
            var objDetail = e.record.data;
            if (App.slmDet.selected.items[0].data.InvtID == "")
            {
                e.record.set('CompID', '');
                e.record.set('CompName', '');
                HQ.message.show(2018070560, [HQ.common.getLang("SaleProduct")], '', true);
            }
            var objCompID = HQ.store.findInStore(App.cboCompID.store, ["CompID"], [objDetail.CompID]);
            var objCompInvtID = HQ.store.findInStore(App.cboCompInvtID.store, ["CompInvtID"], [objDetail.CompInvtID]);
            e.record.set('InvtID', App.slmDet.selected.items[0].data.InvtID);
            if (objCompID != null) {
                if (e.field == "CompID") {
                    e.record.set('CompName', objCompID.CompName);
                }
            }
            if (objCompInvtID != null) {
                if (e.field == "CompInvtID") {
                    e.record.set('CompInvtName', objCompInvtID.CompInvtName);
                }
            }
            frmChange();
        },

        grdSurveyInvt_BeforeEdit: function (editor, e) {
            //if (App.dtpToDate.getValue() < App.dtpFromDate.getValue()) {
            //    HQ.message.show(2018062001);
            //    return false;
            //}
            //if (HQ.store.isChange(App.stoSaleProduct) == false)
            //{
            //    HQ.message.show(2018070560, [HQ.common.getLang("SaleProduct")], '', true);
            //}
            if (HQ.form.checkRequirePass(App.frmMain) && App.cboStatus.getValue() == _beginStatus) {
                return HQ.grid.checkBeforeEdit(e, keysSurveyInvt);
            }
            else {
                return false;
            }
            frmChange();
        },

        grdSurveyInvt_ValidateEdit: function (item, e) {
            //return HQ.grid.checkValidateEdit(App.grdSurveyInvt, e, keysSurveyInvt);
        },

        grdSurveyInvt_Reject : function (record) {
            HQ.grid.checkReject(record, App.grdSurveyInvt);
            frmChange();
        },

        stoSurveyCriteria_Edit: function (item, e) {
            HQ.grid.checkInsertKey(App.grdSurveyCriteria, e, keysSurveyCriteria);
            var objDetail = e.record.data;

            var objCompID = HQ.store.findInStore(App.cboCriteriaID.store, ["CriteriaID"], [objDetail.CriteriaID]);
            if (objCompID != null) {
                if (e.field == "CriteriaID") {
                    e.record.set('CriteriaName', objCompID.CriteriaName);
                }
            }
            frmChange();
        },

        stoSurveyCriteria_BeforeEdit: function (editor, e) {
            //if (App.dtpToDate.getValue() < App.dtpFromDate.getValue()) {
            //    HQ.message.show(2018062001);
            //    return false;
            //}
            if (HQ.form.checkRequirePass(App.frmMain) && App.cboStatus.getValue() == _beginStatus) {
                return HQ.grid.checkBeforeEdit(e, keysSurveyCriteria);
            }
            else {
                return false;
            }
        },

        stoSurveyCriteria_ValidateEdit: function (item, e) {
            return HQ.grid.checkValidateEdit(App.grdSurveyCriteria, e, keysSurveyCriteria);
        },

        stoSurveyCriteria_Reject: function (record) {
            HQ.grid.checkReject(record, App.grdSurveyCriteria);
            frmChange();
        },
        grdInvt_BeforeEdit: function (editor, e) {
            _invtID = e.record.data.InvtID;
            if (HQ.form.checkRequirePass(App.frmMain) && App.cboStatus.getValue() == _beginStatus) {
                return HQ.grid.checkBeforeEdit(e, keysInvt);
            }
            else {
                return false;
            }
        },
        grdInvt_Edit: function (item, e) {
            HQ.grid.checkInsertKey(App.grdInvt, e, keysInvt);
            var objDetail = e.record.data;

            var objCompID = HQ.store.findInStore(App.cboInvtID.store, ["InvtID"], [objDetail.InvtID]);
            if (objCompID != null) {
                if (e.field == "InvtID") {
                    e.record.set('Descr', objCompID.Descr);
                }
            }
            frmChange();
        },
        grdInvt_ValidateEdit: function (item, e) {
            return HQ.grid.checkValidateEdit(App.grdInvt, e, ["InvtID"]);
        },
        grdInvt_Reject: function (record) {
            HQ.grid.checkReject(record, App.grdInvt);
            frmChange();
        },
    }
};

var refresh = function (item) {
    if (item == 'yes') {
        if (HQ.isNew)
            App.cboSurveyID.setValue('');
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoHeaderSurvey.reload();
        
    }
};
var save = function () {

    App.frmMain.getForm().updateRecord();
    if (!HQ.util.passNull(App.cboSurveyID.getValue()) == '' && !HQ.util.passNull(App.cboSurveyID.getValue().toString()).match(regex)) {
        HQ.message.show(20140811, App.cboSurveyID.fieldLabel);
        App.cboSurveyID.validate();
        App.cboSurveyID.focus();
        return false;
    }
    if (App.frmMain.isValid()) {
        App.stoSurveyInvt.clearFilter();
        App.frmMain.submit({

            waitMsg: HQ.common.getLang("SavingData"),

            url: 'OM20090/Save',

            params: {
                lstHeader: Ext.encode(App.stoHeaderSurvey.getRecordsValues()),
                lstSurveyCriteria: HQ.store.getData(App.stoSurveyCriteria),
                lstSurveyInvt: HQ.store.getData(App.stoSurveyInvt),
                lstDel: Ext.encode(App.stoDel4Save.getRecordsValues())
            },

            success: function (msg, data) {

                HQ.message.process(msg, data, true);
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                if (HQ.isNew) {
                    App.cboSurveyID.getStore().load({
                        callback: function () {
                            if (Ext.isEmpty(App.cboSurveyID.getValue())) {
                                App.cboSurveyID.setValue(data.result.data.SurveyID);
                                App.stoHeaderSurvey.reload();
                            }
                            else {
                                App.cboSurveyID.setValue(data.result.data.SurveyID);
                                App.stoHeaderSurvey.reload();
                            }
                        }
                    });

                } else {

                    refresh('yes')

                }
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};
var deleteData = function (item) {

    if (item == "yes") {

        if (HQ.focus == 'header') {
            if (App.frmMain.isValid()) {
                App.frmMain.updateRecord();
                App.frmMain.submit({
                    waitMsg: HQ.common.getLang("DeletingData"),
                    url: 'OM20090/DeleteAll',
                    timeout: 1000000,
                    success: function (msg, data) {
                        
                        App.cboSurveyID.setValue('');
                        //App.stoHeaderSurvey.reload();
                        App.cboSurveyID.store.reload();
                        App.cboSurveyID.forceSelection = true;
                    },

                    failure: function (msg, data) {
                        HQ.message.process(msg, data, true);
                    }
                });
            }
        }

        else if (HQ.focus == 'Condition') {

            App.grdSurveyCriteria.deleteSelected();

            frmChange();

        }

        else if (HQ.focus == 'SurveyInvt') {

            App.grdSurveyInvt.deleteSelected();

            frmChange();

        }
        else if (HQ.focus == 'SaleProduct')
        {
            var storeAll = App.grdSurveyInvt.store.snapshot || App.grdSurveyInvt.store.allData || App.grdSurveyInvt.store.data;
            var lstdel = App.grdInvt.selModel.selected.items;
            if (lstdel != undefined) {
                for (var k = 0; k < lstdel.length; k++) {
                    for (var i = storeAll.length - 1; i >= 0; i--) {
                        if (storeAll.items[i].data.InvtID == lstdel[k].data.InvtID) {
                            App.stoDel4Save.data.add(storeAll.items[i]);
                            storeAll.remove(storeAll.items[i]);
                        }
                    }
                }
            }

            
            App.grdInvt.deleteSelected();
            frmChange();
            setTimeout(function () {
                App.slmDet.select(0);
            }, 100);
        }

    }

};
//var rendererRowNbr = function (value, meta, record) {
//    return App.stoDetailDet.data.indexOf(record) + 1;
//}
var slmDet_select = function (slm, selRec, idx, eOpts) {
    HQ.common.showBusy(true, HQ.waitMsg);
    App.grdSurveyInvt.getFilterPlugin().clearFilters();
    var invtID=selRec.data.InvtID;
    if (selRec.data.InvtID == "" || selRec.data.InvtID == null) {
        invtID = '@@@@@@@';
    }
    
    App.grdSurveyInvt.getFilterPlugin().getFilter('InvtID').setValue(invtID);
    App.grdSurveyInvt.getFilterPlugin().getFilter('InvtID').setActive(true);
    //App.grdSurveyInvt.view.refresh();

    HQ.common.showBusy(false);
    setTimeout(function(){
        
        var lstSurveryInvt = App.grdSurveyInvt.store.data;
        var checkInsert = true;
        for(var i=0;i<lstSurveryInvt.length;i++)
        {
            if(lstSurveryInvt.items[i].data.InvtID=="")
            {
                checkInsert = false;
            }
        }
        if(checkInsert==true)
        {
            HQ.store.insertBlank(App.stoSurveyInvt, keysSurveyInvt);
        }
    }, 1000)

}
var rendererRowNbr = function (value, meta, record) {
    return App.stoSurveyInvt.data.indexOf(record) + 1;
}
var IsReadOnlyHeader = function(value) {
    App.txtSurCompetitorName.setReadOnly(value != "H");
    App.dtpToDate.setReadOnly(value != "H");
    App.dtpFromDate.setReadOnly(value != "H");
}
var checkSpecialChar = function (value) {

    var regex = /^(\w*(\d|[a-zA-Z]|[\_@()+-.]))*$/;
    if (value)
        if (!HQ.util.passNull(value) == '' && !HQ.util.passNull(value.toString()).match(regex)) {
            HQ.message.show(20140811, App.cboSurveyID.fieldLabel);
            App.cboSurveyID.setValue('');
            return false;
        }
};

var focusOnInvalidField = function (item) {
    if (item == "ok") {
        App.frmMain.getForm().getFields().each(function (field) {
            if (!field.isValid()) {
                field.focus();
                return false;
            }
        });
    }
};
var showFieldInvalid = function (form) {
    var done = 1;
    form.getForm().getFields().each(function (field) {
        if (!field.isValid()) {
            HQ.message.show(15, field.fieldLabel, 'focusOnInvalidField');
            done = 0;
            return false;
        }
    });
    return done;
};