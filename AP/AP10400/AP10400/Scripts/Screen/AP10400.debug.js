
var selectedIndex = 0;
var _hold = "H";
var _curSlsperid = "";
var _focusrecord = 1;
var beforeedit = '';
var prefixvalue = "";
var lastfixvalue = "";
var shownextlotserial = "";
var dueDay = 0;

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

//AP10400 bien tam


var tmpTranAmt = 0;

//bien tam grid 1

var tmpApplyAmount = 0;
//3 bien tam grid 2
var tmpVendorBalance = 0;
var tmpApplicationTotal = 0;
var tmplUnApplyTotal = 0;
//var autoApplyOnActive = 0;
var tmpAutoApplyForTotalAmountAndPayment = 0;
var tmpTranAmt = 0;

var keys = ['InvcNbr', 'DocDate', 'DocBal', 'VendID', 'Descr'];


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
                App.slmGrid.select(0);
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
                App.slmGrid.selectPrevious();
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
                App.slmGrid.selectNext();
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
                App.slmGrid.select(App.storeGrid.getCount() - 1);
            }



            break;
        case "refresh":
            App.storeFormTop.reload();
            App.storeFormBotRight.reload();
            App.storeGrid.reload();

            break;
        case "new":
            if (HQ.isInsert) {

                if (_focusrecord == 2) {
                    var time = new Date();
                    if (App.cboBatNbr.value != "" && App.cboRefNbr != "") {
                        App.slmGrid.select(App.storeGrid.getCount() - 1);
                        if (App.storeGrid.getCount() == 0) {
                            App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());
                            App.slmGrid.select(App.storeGrid.getCount() - 1);
                            App.slmGrid.selected.items[0].set('DocDate', time);

                            App.grd.editingPlugin.startEditByPosition({ row: App.storeGrid.getCount() - 1, column: 1 });
                        } else if (App.slmGrid.selected.items[0].data.InvcNbr != "") {
                            App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());
                            App.slmGrid.select(App.storeGrid.getCount() - 1);
                            App.slmGrid.selected.items[0].set('DocDate', time);

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
    //App.cboVendID.enable(true);
    //App.cboDocType.enable(true);
    var time = new Date();
    App.txtBranchID.setValue(HQ.cpnyID);
    var record = Ext.create("App.AP_DocClassModel", {
        //CustId: "",

        DocDate: time,

        OrigDocAmt: 0,
        DocBalL: 0,
        BatNbr: App.cboBatNbr.getValue(),
        BranchID: App.txtBranchID.getValue(),
        InvcNbr: "",
        DocDesc: "",

    });
    App.storeFormBotRight.insert(0, record);
    App.frmBotRight.getForm().loadRecord(App.storeFormBotRight.getAt(0));
    App.cboBankAcct.setValue(App.cboBankAcct.store.data.items[0].data.BankAcct);
    //App.txtOrigRefNbr.setValue("");

    App.storeGrid.reload();
};

var setStatusValueH = function () {
    App.cboStatus.setValue("H");
    App.cboHandle.getStore().reload();
};

function Save() {

    App.frmBotRight.getForm().updateRecord();

    if (App.frm.isValid()) {
        App.frm.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'AP10400/Save',
            params: {
                lstheaderTop: HQ.store.getData(App.storeFormTop),
                lstheaderBotRight: HQ.store.getData(App.storeFormBotRight),
                lstgrd: HQ.store.getData(App.storeGrid),
                BranchID: App.txtBranchID.getValue(),
                Handle: App.cboHandle.getValue(),
                BatNbr: App.cboBatNbr.getValue(),
                RefNbr: App.cboRefNbr.getValue(),
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
        parent.App.tabAP10400.close();
    else if (App.storeGrid.getChangedData().Updated != undefined ||
        App.storeGrid.getChangedData().Created != undefined ||
        App.storeGrid.getChangedData().Deleted != undefined ||
        storeIsChange(App.storeForm, false)) {
        HQ.message.show(5, '', 'closeScreen');
    } else {
        parent.App.tabAP10400.close();
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
        if (parent.App.tabAP10400 != null)
            parent.App.tabAP10400.close();
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
                    App.frmBotRight.getForm().reset(true);
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
        if (e.record.data.InvcNbr != "") {
            return false;
        }
    }

};

var grd_Edit = function (item, e) {

    if (keys.indexOf(e.field) != -1) {
        if (e.value != '' && isAllValidKey(App.storeGrid.getChangedData().Created) && isAllValidKey(App.storeGrid.getChangedData().Updated)) {
            //App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());//Ext.data.Record() 
            var time = new Date();
            App.slmGrid.selected.items[0].set('DocDate', time);
            var record = Ext.create("App.AP10400_BindingGrid_ResultModel", {
                //CustId: "",
                DocDate: time,
            });
            App.storeGrid.insert(App.storeGrid.getCount(), record);
        }
    }
};

var grd_ValidateEdit = function (item, e) {

    if (keys.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(App.grd, e, keys)) {
            HQ.message.show(1112, e.value, '');
            App.slmGrid.selected.items[0].set('InvcNbr', '');
            return false;
        }
    }
    return true;
};

var grd_CancelEdit = function (editor, e) {
    if (e.record.phantom) {
        if (App.cboInvcNbr.getValue == "") {
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
        App.frm.getForm().loadRecord(record);
        App.cboHandle.getStore().reload();
        App.cboRefNbr.getStore().reload();
        setTimeout(function () { waitStoreRefNbrReLoad(); }, 1500);

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
    //App.cboVendID.enable(true);
    //App.cboDocType.enable(true);
    App.storeFormTop.reload();
    //App.cboRefNbr.getStore().reload();
    //App.cboRefNbr.setValue(App.cboRefNbr.store.data.items[0].data.RefNbr);
    //setTimeout(function () { waitcboRefNbrReLoad(); }, 1200);
    //App.cboVendID.disable(true);
    //App.cboDocType.disable(true);
};

var cboRefNbr_Change = function () {

    App.storeFormBotRight.reload();
    setTimeout(function () { waitStoreFormBotReLoad(); }, 1000);

};

var loadDataAutoHeaderBotRight = function () {

    //if (App.storeFormBot.getCount() == 0) {
    //    App.storeFormBot.insert(0, Ext.data.Record());
    //}
    var record = App.storeFormBotRight.getAt(0);
    if (record) {
        App.frmBotRight.getForm().loadRecord(record);
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


var checkRequire = function (items) {
    if (items != undefined) {
        for (var i = 0; i < items.length; i++) {
            //if (items[i]["InvcNbr"] == undefined)
            //    continue;
            //if (items[i]["InvcNbr"] == "")
            //    continue;
            if (HQ.grid.checkRequirePass(items[i], keys)) continue;

            if (items[i]["InvcNbr"] == "") {
                HQ.message.show(15, '@Util.GetLang("InvcNbr")', null);
                return false;
            }


        }
        return true;
    }
    else {
        return true;
    }
};


var waitGridLoadAndFocusIntoBatNbr = function () {
    App.cboBatNbr.focus();
}


//doi form 2 tab 1 load xong
var waitStoreFormBotReLoad = function () {
    App.cboInvcNbr.getStore().reload();
    App.storeGrid.reload();


};







//ham validate so duong
var validatePossitiveNumber = function (field, e) {
    if (event.keyCode == e.NUM_MINUS) {
        e.stopEvent();
    }
};


var setReadOnly = function () {
    //    if (App.cboStatus.value != "H") {
    //        App.cboHandle.setReadOnly(true);
    //        App.cboRefNbr.setReadOnly(true);

    //        //App.txtOrigRefNbr.setReadOnly(true);
    //        App.dteDocDate.setReadOnly(true);

    //        App.txtInvcNbr.setReadOnly(true);

    //        App.cboBankAcct.setReadOnly(true);

    //        App.cboPONbr.setReadOnly(true);

    //        App.grd.disable(true);
    //    } else {
    //        App.cboHandle.setReadOnly(false);
    //        App.cboRefNbr.setReadOnly(false);

    //        //App.txtOrigRefNbr.setReadOnly(false);
    //        App.dteDocDate.setReadOnly(false);
    //        App.txtInvcNbr.setReadOnly(false);
    //        App.cboBankAcct.setReadOnly(false);
    //        App.cboPONbr.setReadOnly(false);


    //        App.grd.enable(true);
    //    }
};

var removeReadOnly = function () {

};

var cboBankAcct_Change = function () {
    //App.cboPONbr.setValue(App.cboPONbr.getStore().data.items[0].data.BankAcct);
};




var btnSearch_Click = function () {
    App.cboInvcNbr.getStore().reload();
    App.storeGrid.reload();
};

var txtFromDate_Change = function () {
    App.cboInvcNbr.getStore().reload();
    App.storeGrid.reload();
}

var txtToDate_Change = function () {
    App.cboInvcNbr.getStore().reload();
    App.storeGrid.reload();
}

//khi VendID thay doi
var cboVendID_Change = function () {
    App.cboInvcNbr.getStore().reload();
    App.storeGrid.reload();

};
//load du lieu tu grid len Form Bot va Top
var waitstoreGridReload = function () {
    for (var i = 0; i < App.storeGrid.data.length; i++) {
        tmpVendorBalance += App.storeGrid.data.items[i].data.OrigDocBal;
        tmpApplicationTotal += App.storeGrid.data.items[i].data.Payment;
        tmplUnApplyTotal += App.storeGrid.data.items[i].data.DocBal;
    }
    App.txtOrigDocAmt.setValue(tmpVendorBalance);
    App.txtPaid.setValue(tmpApplicationTotal);
    App.txtUnTotPayment.setValue(tmplUnApplyTotal);

    tmpVendorBalance = 0;
    tmpApplicationTotal = 0;
    tmplUnApplyTotal = 0;

}

//khi thay doi Payment trong Grid  thi
var txtPaymentGrid_change = function (item, newValue, oldValue) {
    for (var i = 0; i < App.storeGrid.data.length; i++) {
        //neu dong dang chon bang voi data trong store cua hang do thi xu ly
        if (App.slmGrid.selected.items[0].data == App.storeGrid.data.items[i].data) {
            //dk neu gia tri moi edit cua payment lon hon gia tri original cua hang do thi set payment = gia tri original , set docbal = 0 va check vao checkbox hang do 
            if (newValue >= App.storeGrid.data.items[i].data.OrigDocBal) {
                App.storeGrid.data.items[i].set("Payment", App.storeGrid.data.items[i].data.OrigDocBal);
                App.storeGrid.data.items[i].set("DocBal", 0);
                App.storeGrid.data.items[i].set("Selected", true)
            } else {// nguoc lai 
                App.storeGrid.data.items[i].set("Payment", newValue);
                App.storeGrid.data.items[i].set("DocBal", App.storeGrid.data.items[i].data.OrigDocBal - newValue);
                App.storeGrid.data.items[i].set("Selected", false)
            }
        }
        tmpApplicationTotal += App.storeGrid.data.items[i].data.Payment;
    }
    App.txtPaid.setValue(tmpApplicationTotal);
    App.txtUnTotPayment.setValue(App.txtOrigDocAmt.value - App.txtPaid.value);
    App.txtCuryCrTot.setValue(App.txtPaid.value);
    tmpApplicationTotal = 0;
};

//khi gia tri o Balance nho hon 0 thi set bang 0
var txtOdd_Change = function (item, newValue, oldValue) {
    if (newValue < 0) {
        App.txtOdd.setValue(0);
    }
}

// ham xu ly khi check vao 1 checkbox  nho trong Grid 
var AdjustedCheckEveryRow_Change = function (item, Value, newValue) {
    //select dong ma minh da check chon
    if (newValue.data.Selected == true) {
        //sau khi chon xong thay doi gia tri trong o pPayment va DocBal 
        newValue.set("Payment", newValue.data.DocBal + newValue.data.Payment);
        newValue.set("DocBal", 0);
        ChangeWhenSelectCheckBoxGrid();
    } else {
        newValue.set("DocBal", newValue.data.OrigDocBal)
        newValue.set("Payment", 0);
        ChangeWhenSelectCheckBoxGrid();
    }
};

//cap nhap lai Application Total va Unapply Total sau khi chon checkbox cua Grid 
var ChangeWhenSelectCheckBoxGrid = function (item, newValue, oldValue) {
    //duyet trong store va lay tong cua cac payment dua vao o Application Total va thay doi ca trong UnApply Total
    for (var i = 0; i < App.storeGrid.data.length; i++) {
        tmpApplicationTotal += App.storeGrid.data.items[i].data.Payment;
    }

    App.txtPaid.setValue(tmpApplicationTotal);
    App.txtUnTotPayment.setValue(App.txtOrigDocAmt.value - App.txtPaid.value);
    //dk neu Apply Amount lon hon hoac bang ApplicationTotal , neu lon hon thi lay gia tri ApplicationTotal 
    if (tmpApplicationTotal >= App.txtPaid.value) {
        App.txtCuryCrTot.setValue(App.txtPaid.value);
    } else {  // neu nho hon lay gia tri Apply Amount
        App.txtCuryCrTot.setValue(tmpApplicationTotal);
    }
    tmpApplicationTotal = 0;
};

//ham xu ly neu check vao checkbox o tren header grid 2
var AdjustedCheckAll_Change = function (value) {
    //var colIdx = 3;
    if (value) {
        App.grd.getStore().each(function (item) {
            //vong if nay de check neu 1 so o da duoc check hay bi bo ko dong bo voi cac o con lai
            if ((value.checked == true && this.data.Selected == false) || (value.checked == false && this.data.Selected == true)) {
                item.set("Selected", value.checked);
                AdjustedCheckEveryRow_Change(0, 0, item);
            }

        });

    }
};

//Khi an nut Auto Apply dem gia tri cua o Balance sang o Total Amount
var AutoAssign_Click = function () {
    if (App.txtPayment.value < App.txtOrigDocAmt.value) {
        //moi vo set lai gia tri mac dinh cho cai grid 2 da tinh sau
        for (var i = 0; i < App.storeGrid.data.length; i++) {
            App.storeGrid.data.items[i].set("Payment", 0);
            App.storeGrid.data.items[i].set("DocBal", App.storeGrid.data.items[i].data.OrigDocBal);
            App.storeGrid.data.items[i].set("Selected", false)
        }
        App.txtCuryCrTot.setValue(App.txtPayment.value);
        App.txtOdd.setValue(0);
        tmpAutoApplyForTotalAmountAndPayment = App.txtCuryCrTot.value; // dat bien tam cho de xai de hieu sau nay quyet dinh xoa hay ko sau
        //vong for duyet trong storegrid 2 de di qua hang nao thi xu ly hang do trong grid 2 neu TotalAmount lon hon tung hang trong grid 2
        for (var i = 0; i < App.storeGrid.data.length; i++) {
            if (tmpAutoApplyForTotalAmountAndPayment != 0) {
                if (tmpAutoApplyForTotalAmountAndPayment < App.storeGrid.data.items[i].data.OrigDocBal) {
                    App.storeGrid.data.items[i].set("Payment", tmpAutoApplyForTotalAmountAndPayment);
                    App.storeGrid.data.items[i].set("DocBal", App.storeGrid.data.items[i].data.OrigDocBal - tmpAutoApplyForTotalAmountAndPayment);
                    tmpAutoApplyForTotalAmountAndPayment -= App.storeGrid.data.items[i].data.OrigDocBal;
                    //giam den 1 luc nao do no se - cho cai original thanh 0 luon thi se ket thuc ko lam dang tiep theo do gia tri bien tam = 0 roi
                    if (tmpAutoApplyForTotalAmountAndPayment < 0) {
                        tmpAutoApplyForTotalAmountAndPayment = 0;
                    }
                } else { // tmpAutoApplyForTotalAmountAndPayment >= App.storeGrid2.data.items[i].data.OrigDocBal
                    //theo thu tu set lai gia tri payment la original, docbal thanh 0 , check ca vao o checkbox, giam bien tam = bien tam - gia tri original
                    App.storeGrid.data.items[i].set("Payment", App.storeGrid.data.items[i].data.OrigDocBal);
                    App.storeGrid.data.items[i].set("DocBal", 0);
                    App.storeGrid.data.items[i].set("Selected", true)
                    tmpAutoApplyForTotalAmountAndPayment -= App.storeGrid.data.items[i].data.OrigDocBal;

                }
                tmpApplicationTotal += App.storeGrid.data.items[i].data.Payment;
            }
        }
        App.txtPaid.setValue(tmpApplicationTotal);
        App.txtPayment.setValue(0);
        App.txtUnTotPayment.setValue(App.txtOrigDocAmt.value - App.txtPaid.value);
        tmpApplicationTotal = 0;
    } else { //App.txtPayment.value >= App.txtOrigDocAmt.value
        //do gia tri lon hon nen luc nay o Balance se co gia tri = o Vendor Balance con o Total Amount = o Apply AMount - o Vendor Balance
        App.txtCuryCrTot.setValue(App.txtOrigDocAmt.value);
        App.txtOdd.setValue(App.txtPayment.value - App.txtOrigDocAmt.value);
        //sau do set checkAll cho cac o checkbox o grid 2 va max gia tri Payment luon , giam DocBal con 0
        for (var i = 0; i < App.storeGrid.data.length; i++) {
            App.storeGrid.data.items[i].set("Payment", App.storeGrid.data.items[i].data.OrigDocBal);
            App.storeGrid.data.items[i].set("DocBal", 0);
            App.storeGrid.data.items[i].set("Selected", true)
        }
        App.txtPaid.setValue(App.txtOrigDocAmt.value);
        App.txtUnTotPayment.setValue(0);
        App.txtPayment.setValue(0);
    }

};

var cboInvcNbr_change = function (item, newValue, oldValue) {
    for (var i = 0; i < App.cboInvcNbr.store.data.length; i++) {
        if (App.cboInvcNbr.getStore().data.items[i].data.InvcNbr == newValue) {
            App.slmGrid.selected.items[0].set('DocBal', App.cboInvcNbr.getStore().data.items[i].data.DocBal);
            App.slmGrid.selected.items[0].set('OrigDocBal', App.cboInvcNbr.getStore().data.items[i].data.OrigDocBal);
            App.slmGrid.selected.items[0].set('VendID', App.cboInvcNbr.getStore().data.items[i].data.VendID);
            App.slmGrid.selected.items[0].set('Descr', App.cboInvcNbr.getStore().data.items[i].data.Descr);
            App.slmGrid.selected.items[0].set('BatNbr', App.cboInvcNbr.getStore().data.items[i].data.BatNbr);
            App.slmGrid.selected.items[0].set('RefNbr', App.cboInvcNbr.getStore().data.items[i].data.RefNbr);
        }
    }
    //load so lieu tu grid len Form Bot va Top
    waitstoreGridReload();
};























































