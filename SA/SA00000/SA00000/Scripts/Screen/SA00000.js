//var keys = ['GroupID'];
//var keysHeader = ['UserName'];

//var fieldsCheckRequireHeader = ["FirstName"];//, "LastName", "Address", "Name", "Email", "Tel", "Password", "PasswordQuestion", "PasswordAnswer", "CpnyID", "UserTypes", "Channel", "ExpireDay"];
//var fieldsLangCheckRequireHeader = ["FirstName"];//, "LastName", "Address", "Name", "Email", "Tel", "Password", "PasswordQuestion", "PasswordAnswer", "CpnyID", "UserTypes", "Channel", "ExpireDay"];

//var fieldsCheckRequireUserGroup = ["GroupID"];
//var fieldsLangCheckRequireUserGroup = ["GroupID"];

//var fieldsCheckRequireUserCompany = ["GroupID"];
//var fieldsLangCheckRequireUserCompany = ["GroupID"];

var _focusNo = 0;

var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboCpnyID.getStore().load(function () {
        App.cboCountry.getStore().load(function () {
            App.cboCpnyType.getStore().load(function (){
                App.cboType.getStore().load(function (){
                    App.stoSYS_Company.reload();
                })
            })
        })
    });
};

var pnl_render = function (cmd) {
    cmd.getEl().on('mousedown', function () {
        if (cmd.id == 'pnlUserGroup') {
            _focusNo = 1;
        }
        else if (cmd.id == 'pnlUserCompany') {
            _focusNo = 2;
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
                HQ.combo.first(App.cboCpnyID, HQ.isChange);
            }
            else if (_focusNo == 1) {
                HQ.grid.first(App.grdSYS_UserGroup);
            }
            else if (_focusNo == 2) {
                HQ.grid.first(App.grdSYS_UserCompany);
            }
            break;
        case "prev":
            if (_focusNo == 0) {
                HQ.combo.prev(App.cboCpnyID, HQ.isChange);
            }
            else if (_focusNo == 1) {
                HQ.grid.prev(App.grdSYS_UserGroup);
            }
            else if (_focusNo == 2) {
                HQ.grid.prev(App.grdSYS_UserCompany);
            }
            break;
        case "next":
            if (_focusNo == 0) {
                HQ.combo.next(App.cboCpnyID, HQ.isChange);
            }
            else if (_focusNo == 1) {
                HQ.grid.next(App.grdSYS_UserGroup);
            }
            else if (_focusNo == 2) {
                HQ.grid.next(App.grdSYS_UserCompany);
            }
            break;
        case "last":
            if (_focusNo == 0) {
                HQ.combo.last(App.cboCpnyID, HQ.isChange);
            }
            else if (_focusNo == 1) {
                HQ.grid.last(App.grdSYS_UserGroup);
            }
            else if (_focusNo == 2) {
                HQ.grid.last(App.grdSYS_UserCompany);
            }
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                if (App.cboCpnyID.valueModels == null) App.cboCpnyID.setValue('');
                App.cboCpnyID.getStore().reload();
                App.stoSYS_Company.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (_focusNo == 0) {
                    //App.cboUserID.setValue("");
                    //App.stoSYS_UserGroup.reload();
                    //App.stoSYS_UserCompany.reload()
                    if (HQ.isChange) {
                        HQ.message.show(150, '', '');
                    }
                    else {
                        App.cboCpnyID.setValue('');
                    }
                }
                else if (_focusNo == 1) {
                    if (HQ.isInsert) {
                        HQ.grid.insert(App.grdSYS_UserGroup);
                    }
                }
                else if (_focusNo == 2) {
                    if (HQ.isInsert) {
                        HQ.grid.insert(App.grdSYS_UserCompany);
                    }
                }
            }
            break;
        case "delete":
            if (_focusNo == 0) {
                //if (App.cboUserID.value) {
                //    if (HQ.isDelete) {
                //        HQ.message.show(11, '', 'deleteData');
                //    }
                //} else {
                //    menuClick('new');
                //}
                var curRecord = App.frmMain.getRecord();
                if (curRecord) {
                    if (HQ.isDelete) {
                        HQ.message.show(11, '', 'deleteData');
                    }
                }
            }
            else if (_focusNo == 1) {
                if (App.slmSYS_UserGroup.selected.items[0] != undefined) {
                    if (HQ.isDelete) {
                        HQ.message.show(11, '', 'deleteData');
                    }
                }
            }
            else if (_focusNo == 2) {
                if (App.slmSYS_UserCompany.selected.items[0] != undefined) {
                    if (HQ.isDelete) {
                        HQ.message.show(11, '', 'deleteData');
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                //if (HQ.form.checkRequirePass(App.frmMain) && HQ.util.checkEmail(App.Email.value) && HQ.store.checkRequirePass(App.stoSYS_UserGroup, keys, fieldsCheckRequireUserGroup, fieldsLangCheckRequireUserGroup)
                //        && HQ.store.checkRequirePass(App.stoSYS_UserCompany, keys, fieldsCheckRequireUserCompany, fieldsLangCheckRequireUserCompany)) {
                //    save();
                //}
                if (HQ.form.checkRequirePass(App.frmMain)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
            if (HQ.store.isChange(App.stoUser) || HQ.store.isChange(App.stoSYS_UserCompany) || HQ.store.isChange(App.stoSYS_UserGroup)) {
                HQ.message.show(7, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }
};

var cboCountry_Change = function (sender, e) {
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
            //App.cboCity.clearValue();
            curRecord.data.City = '';
            App.cboCity.setValue("");
        }
    });

    App.cboDistrict.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord && curRecord.data.District) {
            App.cboDistrict.setValue(curRecord.data.District);
            HQ.combo.expand(App.cboDistrict, ',');
        }
        var dt = HQ.store.findInStore(App.cboDistrict.getStore(), ["District"], [App.cboDistrict.getValue()]);
        if (!dt) {
            //App.cboCity.clearValue();
            curRecord.data.District = '';
            App.cboDistrict.setValue("");
        }
    });

};



//load lần đầu khi mở
var firstLoad = function () {
    loadSourceCombo();
};

//load store khi co su thay doi CpnyID
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isNew = false;
    App.cboCpnyID.forceSelection = true;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "CpnyID");
        record = sto.getAt(0);
        sto.commitChanges();//commit cho record thanh updated muc dich de dung ham HQ.store.isChange
        HQ.isNew = true;//record la new
        App.cboCpnyID.forceSelection = false;
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboCpnyID.focus(true);//focus ma khi tao moi
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);
    frmChange();
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

////////////Kiem tra combo chinh CpnyID
//khi co su thay doi du lieu cua cac conttol tren form
var frmChange = function () {
    App.frmMain.getForm().updateRecord();
    HQ.isChange = HQ.store.isChange(App.stoSYS_Company);
    HQ.common.changeData(HQ.isChange, 'SA00000');//co thay doi du lieu gan * tren tab title header
    //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
    if (App.cboCpnyID.valueModels == null || HQ.isNew == true)//App.cboVendID.valueModels == null khi ko co select item nao
        App.cboCpnyID.setReadOnly(false);
    else App.cboCpnyID.setReadOnly(HQ.isChange);

};

// Event when cboVendID is changed or selected item 
var cboCpnyID_Change = function (sender, value) {
    if (sender.valueModels != null) {
        App.cboCountry.setValue('');
        
        App.stoSYS_Company.reload();
    }

};


function save() {
    //dòng này để bắt các thay đổi của form 
    App.frmMain.getForm().updateRecord();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: 'Submiting...',
            url: 'SA00000/Save',
            params: {
                lstSYS_Company: Ext.encode(App.stoSYS_Company.getChangedData({ skipIdForPhantomRecords: false })),
                isNew: HQ.isNew
            },
            success: function (result, data) {
                HQ.message.show(201405071, '', '');
                var CpnyID = data.result.CpnyID;
                App.cboCpnyID.getStore().load(function () {
                    App.cboCpnyID.setValue(CpnyID);
                    App.stoSYS_Company.reload();
                });
            }
            , failure: function (errorMsg, data) {
                if (data.result.msgCode) {
                    if (data.result.msgCode == 2000)//loi trung key ko the add
                        HQ.message.show(data.result.msgCode, [App.cboCpnyID.fieldLabel, App.cboCpnyID.getValue()], '', true);
                    else HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                }
                else {
                    HQ.message.process(msg, data, true);
                }
            }
        });
    }
};

// Submit the deleted data into server side
function deleteData(item) {
    if (item == 'yes') {
        if (_focusNo == 0) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang('DeletingData'),
                url: 'SA00000/DeleteAll',
                success: function (action, data) {
                    App.cboCpnyID.setValue("");
                    App.cboCpnyID.getStore().load(function () { cboCpnyID_Change(App.cboCpnyID); });

                },
                failure: function (action, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                    }
                }
            });
        }
            //else if (_focusNo == 1) {
            //            App.grdSYS_UserGroup.deleteSelected();
            //        }
            //        else if (_focusNo == 2) {
            //            App.grdSYS_UserCompany.deleteSelected();
            //        }
    }
};

//var deleteData = function (item) {
//    if (item == "yes") {
//        if (_focusNo == 0) {
//            if (App.frmMain.isValid()) {
//                App.frmMain.updateRecord();
//                App.frmMain.submit({
//                    waitMsg: HQ.common.getLang("DeletingData"),
//                    url: 'SA00000/DeleteAll',
//                    timeout: 7200,
//                    success: function (msg, data) {
//                        App.cboUserID.getStore().load();
//                        menuClick("new");
//                    },
//                    failure: function (msg, data) {
//                        HQ.message.process(msg, data, true);
//                    }
//                });
//            }

//        }
//        else if (_focusNo == 1) {
//            App.grdSYS_UserGroup.deleteSelected();
//        }
//        else if (_focusNo == 2) {
//            App.grdSYS_UserCompany.deleteSelected();
//        }
//    }
//};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        App.cboCpnyID.getStore().load(function () { App.stoSYS_Company.reload(); });
    }
};
///////////////////////////////////