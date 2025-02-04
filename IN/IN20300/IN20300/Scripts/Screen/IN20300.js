﻿var keys = ['SiteId'];
var fieldsCheckRequire = ["SiteId"];
var fieldsLangCheckRequire = ["SiteId"];

///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;

var checkLoad = function (sto) {
    _Source += 1;
    debugger 
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoIN_Site.reload();
        App.chkAllowedSales.setVisible(HQ.allowedSales);
        App.txtPrioritize.setVisible(HQ.prioritize);
        App.txtPrioritize.allowBlank = !HQ.prioritize;
        if (!HQ.prioritize) {
            App.txtPrioritize.setMinValue(0);
        }
        App.txtPrioritize.isValid();
        HQ.common.showBusy(false);
    }
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.isFirstLoad = true;
            HQ.combo.first(App.cboSiteId, HQ.isChange);
            break;
        case "prev":
            HQ.isFirstLoad = true;
            HQ.combo.prev(App.cboSiteId, HQ.isChange);
            break;
        case "next":
            HQ.isFirstLoad = true;
            HQ.combo.next(App.cboSiteId, HQ.isChange);
            break;
        case "last":
            HQ.isFirstLoad = true;
            HQ.combo.last(App.cboSiteId, HQ.isChange);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                if (App.cboSiteId.valueModels == null) App.cboSiteId.setValue('');
                App.cboSiteId.getStore().load(function () { App.stoIN_Site.reload(); });
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.isChange) {
                    HQ.message.show(150, '', '');
                }
                else {
                    App.cboSiteId.setValue('');
                }
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                var curRecord = App.frmMain.getRecord();
                if (curRecord) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (App.txtEmailAddr.getValue() != "") {
                    if (HQ.util.checkEmail(App.txtEmailAddr.value) && HQ.form.checkRequirePass(App.frmMain)) {
                        save();
                    }
                }
                else if (App.txtPrioritize.getValue() == 0 && HQ.prioritize) {
                    HQ.message.show(2019012360, [App.txtPrioritize.fieldLabel], '', true);
                    return;

                }
                else {
                    if (HQ.form.checkRequirePass(App.frmMain)) {
                      //  if (HQ.util.checkSpecialChar(App.cboSiteId.getValue()) == true) {
                            save();
                       // }
                        //else {
                        //    HQ.message.show(2015123111, App.cboSiteId.fieldLabel);
                        //    App.cboSiteId.focus();
                        //    App.cboSiteId.selectText();
                        //}
                    }

                }

            }
                     break;
        case "print":
            break;
        case "close":
            break;
    }   
};

var cboBranchID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    SiteId = '';
    if (sender.valueModels != null) {
        App.cboSiteId.store.reload();
        App.stoCheckPrioritize.reload();
    }
};

var cboBranchID_Select = function (sender, value) {
    HQ.isFirstLoad = true;
    SiteId = '';
    if (sender.valueModels != null && !App.stoIN_Site.loading) {
        App.cboSiteId.store.reload();
        App.stoCheckPrioritize.reload();
    }
};

var cboSiteId_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null) {
        App.stoIN_Site.reload();
        App.stoCheckPrioritize.reload();
    }
};

var cboSiteId_Select = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoIN_Site.loading) {
        App.stoIN_Site.reload();
    }
};


var cboCountry_Changed = function (sender, newValue, oldValue) {
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
            cboState_Changed(App.cboState, curRecord.data.State);
        }
    });
};

var cboState_Changed = function () {
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
            }
            var dt = HQ.store.findInStore(App.cboDistrict.getStore(), ["District"], [App.cboDistrict.getValue()]);
            if (!dt) {
                curRecord.data.District = '';
                App.cboDistrict.setValue("");
            }
        });
    });
};

var firstLoad = function () {
   
    HQ.util.checkAccessRight(); // kiểm tra các quyền update,insert,del
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
   
    checkLoad(); // Mới
};

//load store khi co su thay doi
var stoLoad = function (sto) {
    //HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
    HQ.isNew = false;
    App.cboCountry.forceSelection = false;
    App.cboState.forceSelection = false;
    App.cboDistrict.forceSelection = false;
    App.cboCity.forceSelection = false;
    App.cboSiteId.forceSelection = false;
    App.cboCountry.store.clearFilter();
    App.cboState.store.clearFilter();
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "BranchID");
        record = sto.getAt(0);
        record.set('SiteType', 'C');
        record.set('Prioritize', 1);
        sto.commitChanges();//commit cho record thanh updated muc dich de dung ham HQ.store.isChange
        HQ.isNew = true;//record la new
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);
    if (App.cboCountry.value == record.data.Country) {
        cboCountry_Changed(App.cboCountry, record.data.Country);
    }
    else if (App.cboState.value == record.data.State) {
        cboState_Changed(App.cboState, record.data.State);
    }
    frmChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
    if (HQ.isNew == false) {
        if (HQ.isUpdate == false) {
            HQ.common.lockItem(App.frmMain, true);
        }
    }
    else {
        HQ.common.lockItem(App.frmMain, false);
       
    }
};

//khi co su thay doi du lieu cua cac conttol tren form
var frmChange = function () {
    App.frmMain.getForm().updateRecord();
    HQ.isChange = HQ.store.isChange(App.stoIN_Site);
    HQ.common.changeData(HQ.isChange, 'IN20300');//co thay doi du lieu gan * tren tab title header
    //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
    if (App.cboSiteId.valueModels == null || HQ.isNew == true) {
        App.cboSiteId.setReadOnly(false);
    }
    else App.cboSiteId.setReadOnly(HQ.isChange);
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

function save() {
    if (HQ.isInsert || HQ.isUpdate) {
        var regex = /^(\w*(\d|([a-zA-Z])|-|\_))*$/;
        var value = App.cboSiteId.getValue();
        if (!HQ.util.passNull(value.toString()).match(regex)) {
            HQ.message.show(20140811, App.cboSiteId.fieldLabel, '');
            return
        }
        var curRecord = App.frmMain.getRecord();
        //curRecord.data.Name = App.txtName.getValue();
        App.frmMain.getForm().updateRecord();
        if (App.frmMain.isValid()) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang('Submiting...'),
                url: 'IN20300/Save',
                params: {
                    lstheader: Ext.encode(App.stoIN_Site.getChangedData({ skipIdForPhantomRecords: false }))
                },
                success: function (action, data) {
                    HQ.message.show(201405071, '', '');
                    SiteId = data.result.SiteId;
                    App.cboSiteId.getStore().load(function () {
                        App.cboSiteId.setValue(SiteId);
                    });
                    App.stoIN_Site.reload();
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
};

function deleteData(item) {
    if (item == 'yes') {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang('DeletingData'),
            url: 'IN20300/DeleteAll',
            success: function (action, data) {
                App.cboSiteId.setValue("");
                App.cboSiteId.getStore().load(function () { cboSiteId_Change(App.cboSiteId); });

            },
            failure: function (msg, data) {
                if (data.result.msgCode) {
                    HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                }
                else HQ.message.process(msg, data, true);
            }
        });
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoIN_Site.reload();
    }
};
///////////////////////////////////
var txtPrioritize_Blur = function (a,b,c) {
    if (App.txtPrioritize.getValue() != 0) {
        if (App.stoIN_Site.data.items[0].modified.Prioritize != undefined) {
            var record = HQ.store.findInStore(App.stoCheckPrioritize, ["Prioritize"], [App.txtPrioritize.getValue()]);
            if (record != undefined) {
                App.txtPrioritize.setValue(0);
                HQ.message.show(2018120760, [App.cboSiteId.getValue(), record.SiteId], '', true);
            }
        }
    }
}