
var selectedIndex = 0;
var _hold = "H";
var _curSlsperid = "";
var _focusrecord = 1;
var beforeedit = '';
var prefixvalue = "";
var lastfixvalue = "";
var shownextlotserial = "";
var dueDay = 0;
var iVat05 = "0";
var iVat10 = "0";
var noneVat = "0";
var oVat10 = "0";
var vat10 = "0";
var taxAmtTotalIVAT05 = 0;
var tranAmtTotalIVAT05 = 0;
var taxAmtTotalIVAT10 = 0;
var tranAmtTotalIVAT10 = 0;
var taxAmtTotalOVAT10 = 0;
var tranAmtTotalOVAT10 = 0;
var taxAmtTotalVAT10 = 0;
var tranAmtTotalVAT10 = 0;
var tranAmtTotalNoneVat = 0;
var totalAmountMustPay = 0;
var totalTaxMustPay = 0;
var vendIDAndDescr = "";
var taxAmt00 = 0;
var taxAmt01 = 0;
var taxAmt02 = 0;
var taxAmt03 = 0;
var taxAmt = [taxAmt00, taxAmt01, taxAmt02, taxAmt03];
var txblAmt = 0;
var countTaxAmt = 0;
var tmpChangeForm1OrForm2 = "0";

//AP10200 bien tam

var tmpTranAmt = 0;
var keys = [''];

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (_focusrecord == 3) {

                var combobox = App.cboBatNbr;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboBatNbr.setValue(combobox.store.getAt(0).data.BatNbr);

            } else if (_focusrecord == 2) {
                HQ.grid.first(App.grd);
            }

            break;
        case "prev":
            if (_focusrecord == 3) {

                var combobox = App.cboBatNbr;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboBatNbr.setValue(combobox.store.getAt(index - 1).data.BatNbr);

            } else if (_focusrecord == 2) {
                HQ.grid.prev(App.grd);
            }

            break;
        case "next":
            if (_focusrecord == 3) {

                var combobox = App.cboBatNbr;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboBatNbr.setValue(combobox.store.getAt(index + 1).data.BatNbr);

            } else if (_focusrecord == 2) {
                HQ.grid.next(App.grd);
            }
            break;
        case "last":
            if (_focusrecord == 3) {

                var combobox = App.cboBatNbr;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboBatNbr.setValue(App.cboBatNbr.store.getAt(App.cboBatNbr.store.getTotalCount() - 1).data.BatNbr);

            } else if (_focusrecord == 2) {
                HQ.grid.last(App.grd);
            }



            break;
        case "refresh":
            App.storeFormTop.reload();
            App.storeFormBot.reload();
            App.storeGrid.reload();

            break;
        case "new":
            if (HQ.isInsert) {

                if (_focusrecord == 2) {
                    if (App.frmBot.getForm().getRecord() != null) {
                        App.slmGrid.select(App.storeGrid.getCount() - 1);


                        if (App.storeGrid.getCount() == 0) {
                            App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());
                            App.slmGrid.select(App.storeGrid.getCount() - 1);
                            for (var i = 0; i < App.cboVendID.getStore().data.length; i++) {
                                if (App.cboVendID.getStore().data.items[i].data.VendID == App.cboVendID.value) {
                                    vendIDAndDescr = App.cboVendID.getStore().data.items[i].data.VendID + " - " + App.cboVendID.getStore().data.items[i].data.name;
                                }
                            }
                            App.slmGrid.selected.items[0].set('TranDesc', vendIDAndDescr)
                            App.grd.editingPlugin.startEditByPosition({ row: App.storeGrid.getCount() - 1, column: 1 });
                        } else if (App.slmGrid.selected.items[0].data.TranAmt != "") {
                            App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());
                            App.slmGrid.select(App.storeGrid.getCount() - 1);
                            for (var i = 0; i < App.cboVendID.getStore().data.length; i++) {
                                if (App.cboVendID.getStore().data.items[i].data.VendID == App.cboVendID.value) {
                                    vendIDAndDescr = App.cboVendID.getStore().data.items[i].data.VendID + " - " + App.cboVendID.getStore().data.items[i].data.name;
                                }
                            }
                            App.slmGrid.selected.items[0].set('TranDesc', vendIDAndDescr)
                            App.grd.editingPlugin.startEditByPosition({ row: App.storeGrid.getCount() - 1, column: 1 });
                        }
                    }



                } else if (_focusrecord == 3) {


                    App.cboBatNbr.setValue('');

                    App.frmTop.getForm().reset(true);

                    App.cboRefNbr.getStore().reload();
                    App.cboRefNbr.setValue('');

                    setTimeout(function () { waitcboRefNbrForInsert(); }, 1500);
                    setTimeout(function () { setStatusValueH(); }, 1500);
                    setTimeout(function () { waitcboStatusReLoad(); }, 1700);


                }

            }
            break;
        case "delete":
            var curRecord = App.frm.getRecord();
            if (HQ.isDelete) {
                if (App.cboBatNbr.value != "") {

                    if (_focusrecord == 3) {
                        HQ.message.show(11, '', 'deleteRecordFormTopBatch');
                    } else if (_focusrecord == 2) {
                        HQ.message.show(11, '', 'deleteRecordGrid')
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {

                if (checkRequire(App.storeGrid.getChangedData().Created)
                    && checkRequire(App.storeGrid.getChangedData().Updated)) {

                    Save();
                }


            }
            break;
        case "print":
            alert(command);
            break;
        case "close":
            Close();
            break;
    }

};

var waitcboRefNbrForInsert = function () {
    App.cboVendID.enable(true);
    App.cboDocType.enable(true);
    App.grd.enable(true);
    var time = new Date();
    App.txtBranchID.setValue(HQ.cpnyID);
    App.cboBankAcct.getStore().reload();
    App.cboPONbr.getStore().reload();
    if (App.storeFormBot.data.items.length == 0) {
        var record = Ext.create("App.AP_DocClassModel", {
            //CustId: "",
            DocType: App.cboDocType.store.data.items[0].data.Code,
            DocDate: time,

            OrigDocAmt: 0,
            DocBalL: 0,
            BatNbr: App.cboBatNbr.getValue(),
            BranchID: App.txtBranchID.getValue(),


        });
        App.storeFormBot.insert(0, record);
        App.frmBot.getForm().loadRecord(App.storeFormBot.getAt(0));
    }
    App.cboBankAcct.setValue(App.cboBankAcct.store.data.items[0].data.BankAcct);
    App.txtOrigRefNbr.setValue("");
    App.txtVendName.setValue("");
    App.txtAddr.setValue("");
    App.storeGrid.reload();
};

var setStatusValueH = function () {
    App.cboStatus.setValue("H");
    App.cboHandle.getStore().reload();
};

function Save() {
    for (var i = 0; i <= App.storeGrid.getCount() - 1; i++) {
        App.slmGrid.select(i);
        //lay gia tri lineRef
        var lineRef = "";
        if (i < 10) {
            lineRef = "0000" + (i + 1).toString();
        } else if ((i + 1) >= 10 && (i + 1) < 100) {
            lineRef = "000" + (i + 1).toString();
        } else if ((i + 1) >= 100 && (i + 1) < 1000) {
            lineRef = "00" + (i + 1).toString();
        } else if ((i + 1) >= 1000 && (i + 1) < 10000) {
            lineRef = "0" + (i + 1).toString();
        } else if ((i + 1) >= 10000 && (i + 1) < 100000) {
            lineRef = (i + 1).toString();
        }
        //lay gia tri lineRef tu vi tri tren cac cot
        if (App.storeGrid.data.items[i].data.LineRef == "") {
            App.storeGrid.data.items[i].set('LineRef', lineRef);
        }
    }
    App.frmBot.getForm().updateRecord();

    if (App.frm.isValid()) {
        App.frm.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'AP10200/Save',
            params: {
                lstheaderTop: HQ.store.getData(App.storeFormTop),
                lstheaderBot: HQ.store.getData(App.storeFormBot),
                lstgrd: HQ.store.getData(App.storeGrid),
                BranchID: App.txtBranchID.getValue(),
                Handle: App.cboHandle.getValue(),
                BatNbr: App.cboBatNbr.getValue(),
                RefNbr: App.cboRefNbr.getValue(),
                DocType: App.cboDocType.getValue(),
                VendID: App.cboVendID.getValue(),
                IntRefNbr: App.txtOrigRefNbr.getValue(),
                ReasonCD: App.cboBankAcct.getValue(),
            },
            success: function (result, data) {
                HQ.message.show(201405071, '', null);
                if (data.result.value == 0) {
                    //if (data.result.value1.toString() != "") {
                    //    App.cboRefNbr.getStore().reload();
                    //    App.cboRefNbr.setValue(data.result.value1.toString());
                    //} else {

                    //}
                    if (data.result.value3 != "0") {
                        App.cboBatNbr.getStore().reload();
                    }
                    if (data.result.value2.toString() != "" && data.result.value1.toString() != "") {
                        App.cboBatNbr.getStore().reload();
                        setTimeout(function () { AfterSaveWaitBatNbrStoreReload(data.result.value2.toString()); }, 500);
                        //App.cboBatNbr.setValue(data.result.value2.toString());
                        App.cboRefNbr.getStore().reload(); //neu them vao cai BatNbr moi thi phai reset store cai RefNbr moi lay gia tri duoc
                    } else if (data.result.value2.toString() == "" && data.result.value1.toString() != "") {
                        App.cboRefNbr.getStore().reload();
                        setTimeout(function () { AfterSaveWaitRefNbrStoreReload(data.result.value1.toString()); }, 500);
                        //App.cboRefNbr.setValue(data.result.value1.toString());
                    }
                    if (data.result.value2.toString() != "0") {
                        App.storeGrid.reload();
                    }

                    //var dt = Ext.decode(data.response.responseText);
                    //App.cboBatNbr.setValue("");
                    //App.cboBatNbr.setValue(dt.value);
                    //App.cboTaxID.getStore().reload();


                } else {
                    if (data.result.value2.toString() != "" && data.result.value1.toString() != "") {
                        App.cboBatNbr.getStore().reload();
                        setTimeout(function () { AfterSaveWaitBatNbrStoreReload(data.result.value2.toString()); }, 500);
                        //App.cboBatNbr.setValue(data.result.value2.toString());
                        App.cboRefNbr.getStore().reload(); //neu them vao cai BatNbr moi thi phai reset store cai RefNbr moi lay gia tri duoc
                    } else if (data.result.value2.toString() == "" && data.result.value1.toString() != "") {
                        App.cboRefNbr.getStore().reload();
                        setTimeout(function () { AfterSaveWaitRefNbrStoreReload(data.result.value1.toString()); }, 500);
                        //App.cboRefNbr.setValue(data.result.value1.toString());
                    } else {
                        App.storeGrid.reload();
                    }

                }
                if (data.result.value3 != "") {
                    App.storeFormTop.reload();
                }
                //menuClick("refresh");
            }
            , failure: function (errorMsg, data) {

                var dt = Ext.decode(data.response.responseText);
                HQ.message.show(dt.code, dt.colName + ',' + dt.value, null);
            }
        });
    } else {
        var fields = App.frm.getForm().getFields().each(
                function (item) {
                    if (!item.isValid()) {
                        alert(item);
                    }
                }
            );
    }
}

var AfterSaveWaitBatNbrStoreReload = function (data) {
    App.cboBatNbr.setValue(data);
};

var AfterSaveWaitRefNbrStoreReload = function (data) {
    App.cboRefNbr.setValue(data);
};

// Xem lai
function Close() {
    if (App.frm.getRecord() != undefined) App.frm.updateRecord();
    if (App.storeGrid.getChangedData().Updated == undefined &&
        App.storeGrid.getChangedData().Deleted == undefined &&
        App.frm.getRecord() == undefined)
        parent.App.tabAP10200.close();
    else if (App.storeGrid.getChangedData().Updated != undefined ||
        App.storeGrid.getChangedData().Created != undefined ||
        App.storeGrid.getChangedData().Deleted != undefined ||
        storeIsChange(App.storeForm, false)) {
        HQ.message.show(5, '', 'closeScreen');
    } else {
        parent.App.tabAP10200.close();
    }
}

// Xem lai
function storeIsChange(store, isCreate) {
    if (isCreate == undefined) isCreate = true;
    if ((isCreate == true ? store.getChangedData().Created.length > 1 : false)
        || store.getChangedData().Updated != undefined
        || store.getChangedData().Deleted != undefined) {
        return true;
    }
    return false;
};
var closeScreen = function (item) {
    if (item == "yes") {

        Save();
    }
    else {
        if (parent.App.tabAP10200 != null)
            parent.App.tabAP10200.close();
    }
};
// Xac nhan xoa record cua form bot AP_Doc
var deleteRecordFormTopBatch = function (item) {
    if (item == "yes") {

        try {
            App.direct.DeleteFormTopBatch(App.cboBatNbr.getValue(), App.txtBranchID.getValue(), {
                success: function (data) {
                    //menuClick('refresh');

                    App.cboBatNbr.setValue('');
                    App.cboBatNbr.getStore().reload();

                    App.frmTop.getForm().reset(true);
                    App.cboRefNbr.getStore().reload();
                    App.cboRefNbr.setValue('');
                    App.frmBot.getForm().reset(true);
                    setTimeout(function () { waitStoreRefNbrReLoad(); }, 1000);
                    App.cboStatus.setValue("H");
                    App.cboHandle.getStore().reload();
                    //waitcboStatusReLoad();
                    setTimeout(function () { waitcboStatusReLoad(); }, 700);
                    App.cboRefNbr.setValue('');
                    setReadOnly();

                    //window.location.reload();
                },
                failure: function () {
                    //
                    alert("ko delete duoc ");
                },
                eventMask: { msg: '@Util.GetLang("DeletingData")', showMask: true }
            });
        } catch (ex) {
            alert(ex.message);
        }
    }
};
//// Xac nhan xoa record cua form top Batch
//var deleteRecordFormBotAP_Doc = function (item) {
//    if (item == "yes") {

//        try {
//            App.direct.DeleteFormBotAP_Doc(App.cboRefNbr.getValue(), App.cboBatNbr.getValue(), App.txtBranchID.getValue(), {
//                success: function (data) {
//                    //menuClick('refresh');
//                    App.cboRefNbr.getStore().reload();
//                    App.cboRefNbr.setValue('');


//                    setTimeout(function () { waitStoreRefNbrReLoad(); }, 1000);

//                },
//                failure: function () {
//                    //
//                    alert("ko delete duoc ");
//                },
//                eventMask: { msg: '@Util.GetLang("DeletingData")', showMask: true }
//            });
//        } catch (ex) {
//            alert(ex.message);
//        }
//    }
//};
var deleteRecordGrid = function (item) {
    if (item == "yes") {
        App.grd.deleteSelected();

    }
};
//check value

//check value
var isAllValidKey = function (items) {
    if (items != undefined) {
        for (var i = 0; i < items.length; i++) {
            for (var j = 0; j < keys.length; j++) {
                if (items[i][keys[j]] == '' || items[i][keys[j]] == undefined)
                    return false;
            }
        }
        return true;
    } else {
        return true;
    }
};

function selectRecord(grid, record) {
    var record = grid.store.getById(id);
    grid.store.loadPage(grid.store.findPage(record), {
        callback: function () {
            grid.getSelectionModel().select(record);
        }
    });
}

var grd_BeforeEdit = function (editor, e) {
    if (!HQ.isUpdate)
        return false;
    //keys = e.record.idProperty.split(',');
    if (keys.indexOf(e.field) != -1) {
        //if (e.record.data.TranAmt != "") {
        //    return false;
        //}
    }

};

var grd_Edit = function (item, e) {

    if (keys.indexOf(e.field) != -1) {
        if (e.value != '' && isAllValidKey(App.storeGrid.getChangedData().Created) && isAllValidKey(App.storeGrid.getChangedData().Updated)) {
            //App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());//Ext.data.Record() 
            //App.slmGrid.selected.items[0].set('TranDesc', vendIDAndDescr);

            //var record = Ext.create("App.AP_TransResultModel", {
            //    //CustId: "",
            //    TranDesc: App.slmGrid.selected.items[0].data.TranDesc



            //});
            //App.storeGrid.insert(App.storeGrid.getCount(), record);
        }
    }
};

var grd_ValidateEdit = function (item, e) {

    if (keys.indexOf(e.field) != -1) {
        //if (duplicated(App.storeGrid, e))
        //{
        //     HQ.message.show(1112, e.value, '');
        //    App.slmGrid.selected.items[0].set('TranAmt', '');
        //    return false;
        //}
    }
    return true;
};

var grd_CancelEdit = function (editor, e) {
    if (e.record.phantom) {
        if (App.txtTranAmt.getValue == "") {
            e.store.remove(e.record);
        }
    }
};
var grd_Reject = function (record) {
    if (record.data.tstamp == '') {
        App.storeGrid.remove(record);
        App.grd.getView().focusRow(App.storeGrid.getCount() - 1);
        App.grd.getSelectionModel().select(App.storeGrid.getCount() - 1);
    } else record.reject();
};

//Phan trang tren grid
var onComboBoxSelect = function (combo) {
    var store = combo.up("gridpanel").getStore();
    store.pageSize = parseFloat(combo.getValue(), 10);
    store.reload();

};
var waitStoreRefNbrReLoad = function () {
    if (App.cboRefNbr.getStore().data.items[0] == undefined) {
        App.cboRefNbr.setValue("");
    } else {
        App.cboRefNbr.setValue(App.cboRefNbr.getStore().data.items[0].data.RefNbr);
    }

};
var loadDataAutoHeaderTop = function () {

    var record = App.storeFormTop.getAt(0);
    if (record != undefined) {
        App.frmTop.getForm().loadRecord(record);
        App.cboHandle.getStore().reload();
        App.cboRefNbr.getStore().reload();
        setTimeout(function () { waitStoreRefNbrReLoad(); }, 1000);

    }
};

var waitcboStatusReLoad = function () {
    if (App.cboStatus.value == "H") {
        setReadOnly();
        App.cboHandle.setValue(App.cboHandle.store.data.items[0].data.Code);
    } else {
        setReadOnly();
        App.cboHandle.setValue(App.cboHandle.store.data.items[1].data.Code);
    }
};
var cboStatus_Change = function () {
    App.cboHandle.getStore().reload();
    setTimeout(function () { waitcboStatusReLoad(); }, 1200);
};

//var waitcboRefNbrReLoad = function () {
//    if(App.cboRefNbr.store.data.items[0] != undefined){
//        App.cboRefNbr.setValue(App.cboRefNbr.store.data.items[0].data.RefNbr);
//    }
//}
var cboBatNbr_Change = function () {
    App.cboHandle.setValue(App.cboHandle.store.data.items[0].data.Code);
    App.txtBranchID.setValue(HQ.cpnyID);
    App.cboBankAcct.getStore().reload();
    App.cboPONbr.getStore().reload();
    App.cboVendID.enable(true);
    App.cboDocType.enable(true);
    App.storeFormTop.reload();
    //App.cboRefNbr.getStore().reload();
    //App.cboRefNbr.setValue(App.cboRefNbr.store.data.items[0].data.RefNbr);
    //setTimeout(function () { waitcboRefNbrReLoad(); }, 1200);
    App.cboVendID.disable(true);
    App.cboDocType.disable(true);
};

var cboRefNbr_Change = function () {

    App.storeFormBot.reload();
    setTimeout(function () { waitStoreFormBotReLoad(); }, 1000);

};

var loadDataAutoHeaderBot = function () {

    //if (App.storeFormBot.getCount() == 0) {
    //    App.storeFormBot.insert(0, Ext.data.Record());
    //}
    var record = App.storeFormBot.getAt(0);
    if (record) {
        App.frmBot.getForm().loadRecord(record);
        //App.cboRefNbr.getStore().reload();
        //setTimeout(function () { waitStoreRefNbrReLoad(); }, 1000);
    }
};
var Focus2_Change = function (sender, e) {

    _focusrecord = 2;
};

var Focus1_Change = function (sender, e) {

    _focusrecord = 1;
};

var Focus3_Change = function (sender, e) {

    _focusrecord = 3;
};

var setValueBranchID = function () {
    App.txtBranchID.setValue(HQ.cpnyID);

};


var waitGridLoadAndFocusIntoBatNbr = function () {
    App.cboBatNbr.focus();
}


//doi form 2 tab 1 load xong
var waitStoreFormBotReLoad = function () {

    App.storeGrid.reload();


};





//khi VendID thay doi
var cboVendID_Change = function () {
    App.cboPONbr.getStore().reload();
    //do trong store cua vendID va bat gia tri ten va dia chi bo vao 2 cai txtVendName va txtAddress
    for (var i = 0; i < App.cboVendID.store.data.length; i++) {
        if (App.cboVendID.value == App.cboVendID.store.data.items[i].data.VendID) {
            App.txtVendName.setValue(App.cboVendID.store.data.items[i].data.name);
            App.txtAddr.setValue(App.cboVendID.store.data.items[i].data.Address);
        }
    }

};


//ham validate so duong
var validatePossitiveNumber = function (field, e) {
    if (event.keyCode == e.NUM_MINUS) {
        e.stopEvent();
    }
};


var setReadOnly = function () {
    if (App.cboStatus.value != "H") {
        App.cboHandle.setReadOnly(true);
        App.cboRefNbr.setReadOnly(true);

        App.txtOrigRefNbr.setReadOnly(true);
        App.dteDocDate.setReadOnly(true);

        App.txtInvcNbr.setReadOnly(true);

        App.cboBankAcct.setReadOnly(true);

        App.cboPONbr.setReadOnly(true);

        App.grd.disable(true);
    } else {
        App.cboHandle.setReadOnly(false);
        App.cboRefNbr.setReadOnly(false);

        App.txtOrigRefNbr.setReadOnly(false);
        App.dteDocDate.setReadOnly(false);
        App.txtInvcNbr.setReadOnly(false);
        App.cboBankAcct.setReadOnly(false);
        App.cboPONbr.setReadOnly(false);


        App.grd.enable(true);
    }
};

var removeReadOnly = function () {

};

var cboBankAcct_Change = function () {
    //App.cboPONbr.setValue(App.cboPONbr.getStore().data.items[0].data.BankAcct);
};

var txtTranAmt_Change = function (item) {
    var TotalTranAmt = item.value;
    var index = 0;
    var TranAmtRaw = App.slmGrid.selected.items[0].data.TranAmt;


    for (var k = 0; k < App.grd.store.data.length; k++) {
        if (App.grd.store.data.items[k].data.TranAmt == TranAmtRaw) {
            index = k;

        }
    }

    for (var i = 0; i < App.storeGrid.data.length; i++) {
        App.slmGrid.select(i);
        TotalTranAmt = TotalTranAmt + App.slmGrid.selected.items[0].data.TranAmt;
    }
    TotalTranAmt = TotalTranAmt - TranAmtRaw;
    //set lai gia tri cua 3 o kia
    App.dteCuryCrTot.setValue(TotalTranAmt);
    App.txtCuryOrigDocAmt.setValue(TotalTranAmt);
    App.txtCuryDocBal.setValue(TotalTranAmt);

    App.slmGrid.select(index);
    App.grd.editingPlugin.startEditByPosition({ row: index, column: 1 });



};
var waitSelectRowInGrid = function (index) {
    App.grd.editingPlugin.startEditByPosition({ row: index, column: 1 });
};


var checkRequire = function (items) {
    if (items != undefined) {
        for (var i = 0; i < items.length; i++) {
            //if (items[i]["TranAmt"] == undefined)
            //    continue;
            //if (items[i]["TranAmt"] == "")
            //    continue;
            if (HQ.grid.checkRequirePass(items[i], keys)) continue;

            if (items[i]["TranAmt"] == 0) {
                HQ.message.show(15, '@Util.GetLang("TranAmt")', null);
                return false;
            }


        }
        return true;
    }
    else {
        return true;
    }
};












































































