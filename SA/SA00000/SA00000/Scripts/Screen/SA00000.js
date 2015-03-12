var keys = ['AddrID'];
var fieldsCheckRequire = ["AddrID"];
var fieldsLangCheckRequire = ["AddrID"];

var keys1 = ['SubCpnyID'];
var fieldsCheckRequire1 = ["SubCpnyID"];
var fieldsLangCheckRequire1 = ["SubCpnyID"];

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


var loadComboGrid = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboCountry_grd.getStore().load(function () {
        App.cboState_grd.getStore().load(function () {
            App.cboSubCpnyID.getStore().load(function (){
                App.stoSys_CompanyAddr.reload();
               // App.stoSYS_SubCompany.reload();
                HQ.common.showBusy(false, HQ.common.getLang("loadingData"));
            })
        })
    });
};
var pnl_render = function (cmd) {
    cmd.getEl().on('mousedown', function () {
        if (cmd.id == 'pnlSys_CompanyAddr') {
            _focusNo = 1;
        }
        else if (cmd.id == 'pnlSYS_SubCompany') {
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
                HQ.grid.first(App.grdSys_CompanyAddr);
            }
            else if (_focusNo == 2) {
                HQ.grid.first(App.grdSYS_SubCompany);
            }
            break;
        case "prev":
            if (_focusNo == 0) {
                HQ.combo.prev(App.cboCpnyID, HQ.isChange);
            }
            else if (_focusNo == 1) {
                HQ.grid.prev(App.grdSys_CompanyAddr);
            }
            else if (_focusNo == 2) {
                HQ.grid.prev(App.grdSYS_SubCompany);
            }
            break;
        case "next":
            if (_focusNo == 0) {
                HQ.combo.next(App.cboCpnyID, HQ.isChange);
            }
            else if (_focusNo == 1) {
                HQ.grid.next(App.grdSys_CompanyAddr);
            }
            else if (_focusNo == 2) {
                HQ.grid.next(App.grdSYS_SubCompany);
            }
            break;
        case "last":
            if (_focusNo == 0) {
                HQ.combo.last(App.cboCpnyID, HQ.isChange);
            }
            else if (_focusNo == 1) {
                HQ.grid.last(App.grdSys_CompanyAddr);
            }
            else if (_focusNo == 2) {
                HQ.grid.last(App.grdSYS_SubCompany);
            }
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                if (App.cboCpnyID.valueModels == null) App.cboCpnyID.setValue('');
                App.cboCpnyID.getStore().load(function () { App.stoSYS_Company.reload(); });
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (_focusNo == 0) {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', '');
                    }
                    else {
                        App.cboCpnyID.setValue('');
                    }
                }
                else if (_focusNo == 1) {
                    HQ.grid.insert(App.grdSys_CompanyAddr,keys);
                }
                else if (_focusNo == 2) {
                    HQ.grid.insert(App.grdSYS_SubCompany,keys1);
                }
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (_focusNo == 0) {
                    var curRecord = App.frmMain.getRecord();
                    if (curRecord) {
                        HQ.message.show(11, '', 'deleteData');
                    }
                }
                else if (_focusNo == 1) {
                    if (App.slmSys_CompanyAddr.selected.items[0] != undefined) {
                        var rowindex = HQ.grid.indexSelect(App.grdSys_CompanyAddr);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdSys_CompanyAddr), ''], 'deleteData', true)
                    }
                }
                else if (_focusNo == 2) {
                    if (App.slmSYS_SubCompany.selected.items[0] != undefined) {
                        var rowindex = HQ.grid.indexSelect(App.grdSYS_SubCompany);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdSYS_SubCompany), ''], 'deleteData', true)
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
            curRecord.data.District = '';
            App.cboDistrict.setValue("");
        }
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

//load lần đầu khi mở
var firstLoad = function () {
    HQ.isFirstLoad = true;
    loadSourceCombo();
};
////////////Kiem tra combo chinh CpnyID
//khi co su thay doi du lieu cua cac conttol tren form
var frmChange = function () {
    App.frmMain.getForm().updateRecord();
    HQ.isChange = (HQ.store.isChange(App.stoSYS_Company) == false ? HQ.store.isChange(App.stoSys_CompanyAddr) : true) || (HQ.store.isChange(App.stoSYS_Company) == false ? HQ.store.isChange(App.stoSYS_SubCompany) : true);
    HQ.common.changeData(HQ.isChange, 'SA00000');//co thay doi du lieu gan * tren tab title header
    //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
    if (App.cboCpnyID.valueModels == null || HQ.isNew == true)
        App.cboCpnyID.setReadOnly(false);
    else App.cboCpnyID.setReadOnly(HQ.isChange);

};
var grdSYS_SubCompany_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys1);
};
var grdSYS_SubCompany_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSYS_SubCompany, e, keys1);
    frmChange();
};
var grdSYS_SubCompany_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSYS_SubCompany, e, keys1);
};
var grdSYS_SubCompany_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSYS_SubCompany);
    //stoChanged(App.stoSYS_SubCompany);
    frmChange();
};


//xu li su kiem tren luoi giong nhu luoi binh thuong
var grdSys_CompanyAddr_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdSys_CompanyAddr_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSys_CompanyAddr, e, keys);
    frmChange();
};
var grdSys_CompanyAddr_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSys_CompanyAddr, e, keys);
};
var grdSys_CompanyAddr_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSys_CompanyAddr);
    //stoChanged(App.stoSys_CompanyAddr);
    frmChange();
};
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA00000');
};

//load store khi co su thay doi CpnyID
var stoLoad = function (sto) {
    HQ.isFirstLoad = true;
    HQ.common.showBusy(false);
    HQ.isNew = false;
    App.cboCpnyID.forceSelection = true;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "CpnyID");
        record = sto.getAt(0);
       
        HQ.isNew = true;//record la new    
        App.cboCpnyID.forceSelection = false;
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboCpnyID.focus(true);//focus ma khi tao moi
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);
    //App.stoSys_CompanyAddr.reload();
    loadComboGrid();
};


var stoLoadSys_CompanyAddr = function (sto) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        App.stoSYS_SubCompany.reload();
        //HQ.isFirstLoad = false;
    }
    frmChange();
};
var stoLoadSYS_SubCompany = function (sto) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys1);
        }
        HQ.isFirstLoad = false;
    }
    frmChange();
};
//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};



// Event when cboVendID is changed or selected item 
var cboCpnyID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null) {
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

function save() {
    //dòng này để bắt các thay đổi của form 
    App.frmMain.getForm().updateRecord();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: 'Submiting...',
            url: 'SA00000/Save',
            params: {
                lstSYS_Company: Ext.encode(App.stoSYS_Company.getChangedData({ skipIdForPhantomRecords: false })),
                lstSys_CompanyAddr: Ext.encode(App.stoSys_CompanyAddr.getChangedData({ skipIdForPhantomRecords: false })),
                lstSYS_SubCompany: Ext.encode(App.stoSYS_SubCompany.getChangedData({ skipIdForPhantomRecords: false })),
                isNew: HQ.isNew
            },
            success: function (result, data) {
                HQ.message.show(201405071, '', '');
                var CpnyID = data.result.CpnyID;
                App.cboCpnyID.getStore().load(function () {
                    App.cboCpnyID.setValue(CpnyID);
                    App.stoSYS_Company.reload();
                });
            },
            failure: function (msg, data) {
            HQ.message.process(msg, data, true);
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
        else if (_focusNo == 1) {
                    App.grdSys_CompanyAddr.deleteSelected();
                }
                else if (_focusNo == 2) {
                    App.grdSYS_SubCompany.deleteSelected();
                }
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
//            App.grdSys_CompanyAddr.deleteSelected();
//        }
//        else if (_focusNo == 2) {
//            App.grdSYS_SubCompany.deleteSelected();
//        }
//    }
//};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        var cpnyid = '';
        if (App.cboCpnyID.valueModels != null) cpnyid = App.cboCpnyID.getValue();
        App.cboCpnyID.getStore().load(function () { App.cboCpnyID.setValue(cpnyid); App.stoSYS_Company.reload(); });

        //App.cboCpnyID.getStore().load(function () { App.stoSYS_Company.reload(); });
    }
};
///////////////////////////////////