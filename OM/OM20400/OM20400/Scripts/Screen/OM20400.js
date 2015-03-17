var keys = ['BranchID'];
var fieldsCheckRequire = ["BranchID"];
var fieldsLangCheckRequire = ["BranchID"];

var _focusNo = 0;

var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboOrderType_Main.getStore().load(function () {
        App.cboOrderType_Sub.getStore().load(function () {
            App.cboARDOCTYPE.getStore().load(function () {
                App.cboINDocType.getStore().load(function () {
                    App.cboDfltCustID.getStore().load(function () {
                        App.cboSalesType.getStore().load(function () {
                            App.cboDiscType.getStore().load(function () {
                                HQ.common.showBusy(false, HQ.common.getLang("loadingData"));
                                App.stoOM_OrderType.reload();
                            })
                        })
                    })
                })
            })
        })
    });
};

var pnl_render = function (cmd) {
    cmd.getEl().on('mousedown', function () {
        if (cmd.id == 'pnlOM_Numbering') {
            _focusNo = 1;
        }
        else {
            _focusNo = 0;
        }
    });
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (_focusNo == 0) {
                HQ.combo.first(App.cboOrderType_Main, HQ.isChange);
            }
            else {
                HQ.grid.first(App.grdOM_DocNumbering);
            }
            break;
        case "prev":
            if (_focusNo == 0) {
                HQ.combo.prev(App.cboOrderType_Main, HQ.isChange);
            }
            else {
                HQ.grid.prev(App.grdOM_DocNumbering);
            }
            break;
        case "next":
            if (_focusNo == 0) {
                HQ.combo.next(App.cboOrderType_Main, HQ.isChange);
            }
            else {
                HQ.grid.next(App.grdOM_DocNumbering);
            }
            break;
        case "last":
            if (_focusNo == 0) {
                HQ.combo.last(App.cboOrderType_Main, HQ.isChange);
            }
            else {
                HQ.grid.last(App.grdOM_DocNumbering);
            }
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                if (App.cboOrderType_Main.valueModels == null) App.cboOrderType_Main.setValue('');
                App.cboOrderType_Main.getStore().load(function () { App.stoOM_OrderType.reload(); });
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (_focusNo == 0) {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', '');
                    }
                    else {
                        App.cboOrderType_Main.setValue('');
                    }
                }
                else {
                    HQ.grid.insert(App.grdOM_DocNumbering,keys);
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
                else{
                    if (App.slmOM_DocNumbering.selected.items[0] != undefined) {
                        var rowindex = HQ.grid.indexSelect(App.grdOM_DocNumbering);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdOM_DocNumbering), ''], 'deleteData', true)
                    }
                }
                
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain)
                    && HQ.store.checkRequirePass(App.stoOM_OrderType, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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

//load lần đầu khi mở
var firstLoad = function () {
    HQ.isFirstLoad = true;
    loadSourceCombo();
};
////////////Kiem tra combo chinh CpnyID
//khi co su thay doi du lieu cua cac conttol tren form
var frmChange = function () {
    App.frmMain.getForm().updateRecord();
    HQ.isChange = (HQ.store.isChange(App.stoOM_OrderType) == false ? HQ.store.isChange(App.stoOM_DocNumbering) : true);
    HQ.common.changeData(HQ.isChange, 'OM20400');//co thay doi du lieu gan * tren tab title header
    //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
    if (App.cboOrderType_Main.valueModels == null || HQ.isNew == true)
        App.cboOrderType_Main.setReadOnly(false);
    else App.cboOrderType_Main.setReadOnly(HQ.isChange);
};


//xu li su kiem tren luoi giong nhu luoi binh thuong
var grdOM_DocNumbering_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdOM_DocNumbering_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdOM_DocNumbering, e, keys);
    frmChange();
};
var grdOM_DocNumbering_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdOM_DocNumbering, e, keys);
};
var grdOM_DocNumbering_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdOM_DocNumbering);
    //stoChanged(App.stoSys_CompanyAddr);
    frmChange();
};
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM20400');
};

//load store khi co su thay doi CpnyID
var stoLoad = function (sto) {
    HQ.isFirstLoad = true;
    HQ.common.showBusy(false);
    HQ.isNew = false;
    App.cboOrderType_Main.forceSelection = true;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "OrderType");
        record = sto.getAt(0);
       
        HQ.isNew = true;//record la new    
        App.cboOrderType_Main.forceSelection = false;
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboOrderType_Main.focus(true);//focus ma khi tao moi
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);
    App.stoOM_DocNumbering.reload();
   
};

var stoLoadOM_DocNumbering = function (sto) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
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
var cboOrderType_Main_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null) {
        App.stoOM_OrderType.reload();
        //App.stoOM_DocNumbering.reload();
    }
};

//khi nhan combo xo ra, neu da thay doi thi ko xo ra
var cboOrderType_Main_Expand = function (sender, value) {
    if (HQ.isChange) {
        App.cboOrderType_Main.collapse();
    }
};
//khi nhan X xoa tren combo, neu du lieu thay doi thi ko cho xoa, du lieu chua thay doi thi add new
var cboOrderType_Main_TriggerClick = function (sender, value) {
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
            url: 'OM20400/Save',
            params: {
                lstOM_OrderType: Ext.encode(App.stoOM_OrderType.getChangedData({ skipIdForPhantomRecords: false })),
                lstOM_DocNumbering: Ext.encode(App.stoOM_DocNumbering.getChangedData({ skipIdForPhantomRecords: false })),
                isNew: HQ.isNew
            },
            success: function (result, data) {
                HQ.message.show(201405071, '', '');
                var OrderType = data.result.OrderType;
                App.cboOrderType_Main.getStore().load(function () {
                    App.cboOrderType_Main.setValue(OrderType);
                    App.stoOM_OrderType.reload();
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
                url: 'OM20400/DeleteAll',
                success: function (action, data) {
                    App.cboOrderType_Main.setValue("");
                    App.cboOrderType_Main.getStore().load(function () { cboOrderType_Main_Change(App.cboOrderType_Main); });

                },
                failure: function (action, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                    }
                }
            });
        }
        else {
            App.grdOM_DocNumbering.deleteSelected();
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
//                    url: 'OM20400/DeleteAll',
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
//            App.grdOM_DocNumbering.deleteSelected();
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
        var OrderType = '';
        if (App.cboOrderType_Main.valueModels != null) OrderType = App.cboOrderType_Main.getValue();
        App.cboOrderType_Main.getStore().load(function () {
            App.cboOrderType_Main.setValue(OrderType);
            App.stoOM_OrderType.reload();
        });

        //App.cboOrderType_Main.getStore().load(function () { App.stoSYS_Company.reload(); });
    }
};
///////////////////////////////////