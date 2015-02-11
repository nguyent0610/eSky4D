
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

//AP10300 bien tam

var tmpTranAmt = 0;

//bien tam grid 1

var tmpApplyAmount = 0;
//3 bien tam grid 2
var tmpVendorBalance = 0;
var tmpApplicationTotal = 0;
var tmplUnApplyTotal = 0;
//var autoApplyOnActive = 0;
var tmpAutoApplyForTotalAmountAndPayment = 0;
var keys = ['InvcNbr', 'BatNbr', 'RefNbr', 'DocBal', 'VendID', 'Descr'];

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (_focusrecord == 3) {

                var combobox = App.cboBatNbr;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboBatNbr.setValue(combobox.store.getAt(0).data.BatNbr);

            } else if (_focusrecord == 1) {
                HQ.grid.first(App.grdAdjusting);

            } else if (_focusrecord == 2) {
                HQ.grid.first(App.grdAdjusted);
            }

            break;
        case "prev":
            if (_focusrecord == 3) {

                var combobox = App.cboBatNbr;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboBatNbr.setValue(combobox.store.getAt(index - 1).data.BatNbr);

            } else if (_focusrecord == 1) {
                HQ.grid.prev(App.grdAdjusting);

            } else if (_focusrecord == 2) {
                HQ.grid.prev(App.grdAdjusted);
            }

            break;
        case "next":
            if (_focusrecord == 3) {

                var combobox = App.cboBatNbr;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboBatNbr.setValue(combobox.store.getAt(index + 1).data.BatNbr);

            } else if (_focusrecord == 1) {
                HQ.grid.next(App.grdAdjusting);

            } else if (_focusrecord == 2) {
                HQ.grid.next(App.grdAdjusted);
            }
            break;
        case "last":
            if (_focusrecord == 3) {

                var combobox = App.cboBatNbr;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboBatNbr.setValue(App.cboBatNbr.store.getAt(App.cboBatNbr.store.getTotalCount() - 1).data.BatNbr);

            } else if (_focusrecord == 1) {
                HQ.grid.last(App.grdAdjusting);

            } else if (_focusrecord == 2) {
                HQ.grid.last(App.grdAdjusted);
            }



            break;
        case "refresh":
            App.storeFormTop.reload();
            App.storeFormBot.reload();
            App.storeGrid1.reload();
            App.storeGrid2.reload();

            break;
        case "new":
            if (HQ.isInsert) {
                App.cboBatNbr.setValue('');
                App.frm.getForm().reset(true);

                setTimeout(function () { waitcboBatNbrForInsert(); }, 1000);

            }
            break;
        case "delete":
            var curRecord = App.frm.getRecord();
            if (HQ.isDelete) {
                if (App.cboBatNbr.value != "") {
                    HQ.message.show(11, '', 'deleteRecordFormTopBatch');

                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {

                if (App.txtCuryCrTot.value != "") {
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

var waitcboBatNbrForInsert = function () {

    var time = new Date();
    App.txtBranchID.setValue(HQ.cpnyID);
    if (App.storeFormTop.data.items.length == 0) {
        var record = Ext.create("App.BatchClassModel", {

        });
        App.storeFormTop.insert(0, record);
        App.frmTop.getForm().loadRecord(App.storeFormTop.getAt(0));
    }
    App.cboDocType.setValue("PP");
    App.txtDocDate.setValue(time);
    App.storeGrid1.reload();
    App.storeGrid2.reload();
    App.cboHandle.getStore().reload();
    App.cboHandle.setValue("N");
    setStatusValueH();
    App.cboVendID.setReadOnly(false);
    App.cboDocType.setReadOnly(false);
    App.txtDocDate.setReadOnly(false);
};

var setStatusValueH = function () {
    App.cboStatus.setValue("H");
    App.cboHandle.getStore().reload();
};

function Save() {

    App.frmTop.getForm().updateRecord();

    if (App.frm.isValid()) {
        App.frm.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'AP10300/Save',
            params: {
                lstheaderTop: HQ.store.getData(App.storeFormTop),
                lstgrd1: HQ.store.getData(App.storeGrid1),
                lstgrd2: HQ.store.getData(App.storeGrid2),
                BranchID: App.txtBranchID.getValue(),
                Handle: App.cboHandle.getValue(),
                BatNbr: App.cboBatNbr.getValue(),
                DocType: App.cboDocType.getValue(),
                VendID: App.cboVendID.getValue(),

            },
            success: function (result, data) {
                HQ.message.show(201405071, '', null);
                if (data.result.value1 == "") {
                    if (data.result.value2 == "C") {
                        App.cboStatus.setValue("C");
                    } else if (data.result.value2 == "H") {
                        App.cboStatus.setValue("H");
                    }
                    App.storeGrid1.reload();
                    App.storeGrid2.reload();
                } else {
                    App.cboBatNbr.getStore().reload();
                    setTimeout(function () { AfterSaveWaitBatNbrStoreReload(data.result.value1.toString()); }, 700);
                    App.cboVendID.setReadOnly(true);
                    App.cboDocType.setReadOnly(true);
                    App.txtDocDate.setReadOnly(true);
                    if (data.result.value2 == "C") {
                        App.cboStatus.setValue("C");
                    } else if (data.result.value2 == "H") {
                        App.cboStatus.setValue("H");
                    }
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


// Xem lai
function Close() {
    if (App.frmTop.getRecord() != undefined) App.frmTop.updateRecord();
    if (App.storeGrid1.getChangedData().Updated == undefined &&
        App.storeGrid1.getChangedData().Deleted == undefined &&
        App.storeGrid2.getChangedData().Updated == undefined &&
        App.storeGrid2.getChangedData().Deleted == undefined &&
        App.frmTop.getRecord() == undefined)
        parent.App.tabAP10300.close();
    else if (App.storeGrid1.getChangedData().Updated != undefined ||
        App.storeGrid1.getChangedData().Created != undefined ||
        App.storeGrid1.getChangedData().Deleted != undefined ||
        App.storeGrid2.getChangedData().Updated != undefined ||
        App.storeGrid2.getChangedData().Created != undefined ||
        App.storeGrid2.getChangedData().Deleted != undefined ||
        storeIsChange(App.storeForm, false)) {
        HQ.message.show(5, '', 'closeScreen');
    } else {
        parent.App.tabAP10300.close();
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
        if (parent.App.tabAP10300 != null)
            parent.App.tabAP10300.close();
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
                    App.frm.getForm().reset(true);


                    App.cboStatus.setValue("H");
                    App.cboHandle.getStore().reload();
                    //waitcboStatusReLoad();
                    setTimeout(function () { waitcboStatusReLoad(); }, 700);
                    App.cboVendID.setReadOnly(false);
                    App.cboDocType.setReadOnly(false);
                    App.txtDocDate.setReadOnly(false);
                    App.cboDocType.setValue("PP");
                    App.txtDocDate.setValue(time);
                    App.storeGrid1.reload();
                    App.storeGrid2.reload();

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
        return false;
    }

};

var grd_Edit = function (item, e) {

    if (keys.indexOf(e.field) != -1) {
        if (e.value != '' && isAllValidKey(App.storeGrid.getChangedData().Created) && isAllValidKey(App.storeGrid.getChangedData().Updated)) {
            //App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());//Ext.data.Record() 
            //App.SelectionRowOnGrid.selected.items[0].set('TranDesc', vendIDAndDescr);

            //var record = Ext.create("App.AP_TransResultModel", {
            //    //CustId: "",
            //    TranDesc: App.SelectionRowOnGrid.selected.items[0].data.TranDesc



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
        //    App.SelectionRowOnGrid.selected.items[0].set('TranAmt', '');
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
        waitcboStatusReLoad(); // chay lai gia tri cai Handle

        //App.cboRefNbr.getStore().reload();
        //setTimeout(function () { waitStoreRefNbrReLoad(); }, 1000);

    }
};



var cboBatNbr_Change = function () {

    //App.cboVendID.enable(true);
    //App.cboDocType.enable(true);
    App.txtBranchID.setValue(HQ.cpnyID);
    App.storeFormTop.reload();
    App.storeFormBot.reload();
    App.cboHandle.setValue(App.cboHandle.store.data.items[0].data.Code);
    App.cboVendID.setReadOnly(true);
    App.cboDocType.setReadOnly(true);
    App.txtDocDate.setReadOnly(true);
    //App.cboVendID.disable(true);
    //App.cboDocType.disable(true);
};
//ham nay giong ham ChangeWhenSelectCheckBoxGrid1 chi la doi ten cho phu hop 
var waitstoreGrid1Reload = function () {
    //duyet trong store va lay tong cua cac payment dua vao o ApplyAmount
    for (var i = 0; i < App.storeGrid1.data.length; i++) {
        tmpApplyAmount += App.storeGrid1.data.items[i].data.Payment;
    }
    App.txtPayment.setValue(tmpApplyAmount);
    App.txtOdd.setValue(0);
    tmpApplyAmount = 0;

};

var loadDataAutoHeaderBot = function () {

    //if (App.storeFormBot.getCount() == 0) {
    //    App.storeFormBot.insert(0, Ext.data.Record());
    //}
    var record = App.storeFormBot.getAt(0);
    if (record) {
        App.frmBot.getForm().loadRecord(record);
        App.storeGrid1.reload();
        App.storeGrid2.reload();
        App.cboHandle.getStore().reload();
    }
};

// nhom javascript status va handle
var waitcboStatusReLoad = function () {
    if (App.cboStatus.value == "H") {
        App.cboHandle.setValue(App.cboHandle.store.data.items[0].data.Code);
        //setReadOnly();

    } else {
        App.cboHandle.setValue(App.cboHandle.store.data.items[1].data.Code);
        //setReadOnly();

    }
};

var waitFormLoadToReloadCboHandle = function () {
    App.cboHandle.getStore().reload();
}

var cboStatus_Change = function () {
    setTimeout(function () { waitFormLoadToReloadCboHandle(); }, 1000);
    setTimeout(function () { waitcboStatusReLoad(); }, 2000);
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







//ham validate so duong
var validatePossitiveNumber = function (field, e) {
    if (event.keyCode == e.NUM_MINUS) {
        e.stopEvent();
    }
};

//javascript tat mo chuc nang edit 1 so field
var setReadOnly = function () {
    if (App.cboStatus.value != "H") {
        App.cboHandle.setReadOnly(true);


        App.txtDescr.setReadOnly(true);
        App.txtDocDate.setReadOnly(true);




        App.grdAdjusting.disable(true);
        App.grdAdjusted.disable(true);
    } else {
        App.cboHandle.setReadOnly(false);

        App.cboVendID.setReadOnly(false);
        App.txtDescr.setReadOnly(false);
        App.txtDocDate.setReadOnly(false);
        App.txtInvcNbr.setReadOnly(false);
        App.cboBankAcct.setReadOnly(false);
        App.cboPONbr.setReadOnly(false);


        App.grdAdjusting.enable(true);
        App.grdAdjusted.enable(true);
    }
};


//ham xu ly neu check vao checkbox o tren header grid 1
var AdjustingCheckAll_Change = function (value) {
    //var colIdx = 3;
    if (value) {
        App.grdAdjusting.getStore().each(function (item) {
            //vong if nay de check neu 1 so o da duoc check hay bi bo ko dong bo voi cac o con lai
            if ((value.checked == true && this.data.Selected == false) || (value.checked == false && this.data.Selected == true)) {
                item.set("Selected", value.checked);
                AdjustingCheckEveryRow_Change(0, 0, item);
            }
        });
    }
};
//ham xu ly neu check vao checkbox o tren header grid 2
var AdjustedCheckAll_Change = function (value) {
    //var colIdx = 3;
    if (value) {
        App.grdAdjusted.getStore().each(function (item) {
            //vong if nay de check neu 1 so o da duoc check hay bi bo ko dong bo voi cac o con lai
            if ((value.checked == true && this.data.Selected == false) || (value.checked == false && this.data.Selected == true)) {
                item.set("Selected", value.checked);
                AdjustedCheckEveryRow_Change(0, 0, item);
            }

        });

    }
};

// ham xu ly khi check vao 1 checkbox  nho trong Grid 1
var AdjustingCheckEveryRow_Change = function (item, Value, newValue) {
    //select dong ma minh da check chon
    if (newValue.data.Selected == true) {
        //sau khi chon xong thay doi gia tri trong o pPayment va DocBal 
        newValue.set("Payment", newValue.data.DocBal + newValue.data.Payment);
        newValue.set("DocBal", 0)
        ChangeWhenSelectCheckBoxGrid1();
    } else {
        newValue.set("DocBal", newValue.data.OrigDocBal)
        newValue.set("Payment", 0);
        ChangeWhenSelectCheckBoxGrid1();
    }
};

// ham xu ly khi check vao 1 checkbox  nho trong Grid 2
var AdjustedCheckEveryRow_Change = function (item, Value, newValue) {
    //select dong ma minh da check chon
    if (newValue.data.Selected == true) {
        //sau khi chon xong thay doi gia tri trong o pPayment va DocBal 
        newValue.set("Payment", newValue.data.DocBal + newValue.data.Payment);
        newValue.set("DocBal", 0);
        ChangeWhenSelectCheckBoxGrid2();
    } else {
        newValue.set("DocBal", newValue.data.OrigDocBal)
        newValue.set("Payment", 0);
        ChangeWhenSelectCheckBoxGrid2();
    }
};

//khi VendID thay doi
var cboVendID_Change = function () {
    if (App.cboVendID.value != null && App.cboDocType.value != null) {
        //if (App.cboDocType.value == "AD") {
        App.storeGrid2.reload();
        //} else if (App.cboDocType.value == "PP") {
        App.storeGrid1.reload();
        //}
        setTimeout(function () { waitstoreGrid2Reload(); }, 1000);
    }
};



var cboDocType_Change = function () {
    if (App.cboVendID.value != null && App.cboDocType.value != null) {
        //if (App.cboDocType.value == "AD") {
        App.storeGrid2.reload();
        //} else if (App.cboDocType.value == "PP") {
        App.storeGrid1.reload();
        //}
    }
};
//doi sau khi load xong se set gia tri ban dau cho Vendor Balance , Application Total va UnApply TOtal
var waitstoreGrid2Reload = function () {
    for (var i = 0; i < App.storeGrid2.data.length; i++) {
        tmpVendorBalance += App.storeGrid2.data.items[i].data.OrigDocBal;
        tmpApplicationTotal += App.storeGrid2.data.items[i].data.Payment;
        tmplUnApplyTotal += App.storeGrid2.data.items[i].data.DocBal;
    }
    App.txtOrigDocAmt.setValue(tmpVendorBalance);
    App.txtPaid.setValue(tmpApplicationTotal);
    App.txtUnTotPayment.setValue(tmplUnApplyTotal);

    tmpVendorBalance = 0;
    tmpApplicationTotal = 0;
    tmplUnApplyTotal = 0;
};
//cap nhap lai Application Total va Unapply Total sau khi chon checkbox cua Grid 1
var ChangeWhenSelectCheckBoxGrid1 = function (item, newValue, oldValue) {
    //duyet trong store va lay tong cua cac payment dua vao o ApplyAmount
    for (var i = 0; i < App.storeGrid1.data.length; i++) {
        tmpApplyAmount += App.storeGrid1.data.items[i].data.Payment;
    }
    App.txtPayment.setValue(tmpApplyAmount);
    App.txtOdd.setValue(App.txtPayment.value - App.txtCuryCrTot.value);
    tmpApplyAmount = 0;
};

//cap nhap lai Application Total va Unapply Total sau khi chon checkbox cua Grid 2
var ChangeWhenSelectCheckBoxGrid2 = function (item, newValue, oldValue) {
    //duyet trong store va lay tong cua cac payment dua vao o Application Total va thay doi ca trong UnApply Total
    for (var i = 0; i < App.storeGrid2.data.length; i++) {
        tmpApplicationTotal += App.storeGrid2.data.items[i].data.Payment;
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

//khi thay doi Payment trong Grid 1 thi
var txtPaymentGrid1_change = function (item, newValue, oldValue) {

    for (var i = 0; i < App.storeGrid1.data.length; i++) {
        //tmpApplicationTotal += App.storeGrid2.data.items[i].data.Payment;
        if (App.slmGrid1.selected.items[0].data == App.storeGrid1.data.items[i].data) {
            //dk neu gia tri moi edit cua payment lon hon gia tri original cua hang do thi set payment = gia tri original , set docbal = 0 va check vao checkbox hang do 
            if (newValue >= App.storeGrid1.data.items[i].data.OrigDocBal) {
                App.storeGrid1.data.items[i].set("Payment", App.storeGrid1.data.items[i].data.OrigDocBal);
                App.storeGrid1.data.items[i].set("DocBal", 0);
                App.storeGrid1.data.items[i].set("Selected", true)
            } else { //dk neu gia tri moi edit cua payment nho hon gia tri original cua hang do thi set payment = gia tri moi edit , set docbal = gia tri original - gia tri moi edit va bo check cua checkbox hang do 
                App.storeGrid1.data.items[i].set("Payment", newValue);
                App.storeGrid1.data.items[i].set("DocBal", App.storeGrid1.data.items[i].data.OrigDocBal - newValue);
                App.storeGrid1.data.items[i].set("Selected", false)
            }
        }
        tmpApplyAmount += App.storeGrid1.data.items[i].data.Payment;
    }
    //dk neu Apply Amount lon hon hoac bang ApplicationTotal , neu lon hon thi lay gia tri ApplicationTotal 
    if (tmpApplyAmount >= App.txtPaid.value) {
        App.txtCuryCrTot.setValue(App.txtPaid.value);
    } else {  // neu nho hon lay gia tri Apply Amount
        App.txtCuryCrTot.setValue(tmpApplyAmount);
    }
    App.txtPayment.setValue(tmpApplyAmount);


    tmpApplyAmount = 0;
};




//khi thay doi Payment trong Grid 2 thi
var txtPaymentGrid2_change = function (item, newValue, oldValue) {
    for (var i = 0; i < App.storeGrid2.data.length; i++) {
        //neu dong dang chon bang voi data trong store cua hang do thi xu ly
        if (App.slmGrid2.selected.items[0].data == App.storeGrid2.data.items[i].data) {
            //dk neu gia tri moi edit cua payment lon hon gia tri original cua hang do thi set payment = gia tri original , set docbal = 0 va check vao checkbox hang do 
            if (newValue >= App.storeGrid2.data.items[i].data.OrigDocBal) {
                App.storeGrid2.data.items[i].set("Payment", App.storeGrid2.data.items[i].data.OrigDocBal);
                App.storeGrid2.data.items[i].set("DocBal", 0);
                App.storeGrid2.data.items[i].set("Selected", true)
            } else {// nguoc lai 
                App.storeGrid2.data.items[i].set("Payment", newValue);
                App.storeGrid2.data.items[i].set("DocBal", App.storeGrid2.data.items[i].data.OrigDocBal - newValue);
                App.storeGrid2.data.items[i].set("Selected", false)
            }
        }
        tmpApplicationTotal += App.storeGrid2.data.items[i].data.Payment;
    }
    App.txtPaid.setValue(tmpApplicationTotal);
    //dk neu value  ApplicationTotal nho hon  o Apply AMount lay gia tri ApplicationTotal
    if (tmpApplicationTotal <= App.txtPayment.value) {
        App.txtCuryCrTot.setValue(tmpApplicationTotal)
    } else {  // neu lon hon lay gia tri cua Apply AMount
        App.txtCuryCrTot.setValue(App.txtPayment.value)
    }
    tmpApplicationTotal = 0;
};

//khi gia tri o Balance nho hon 0 thi set bang 0
var txtOdd_Change = function (item, newValue, oldValue) {
    if (newValue < 0) {
        App.txtOdd.setValue(0);
    }
}

//Khi an nut Auto Apply dem gia tri cua o Balance sang o Total Amount
var AutoAssign_Click = function () {
    if (App.txtPayment.value < App.txtOrigDocAmt.value) {
        //moi vo set lai gia tri mac dinh cho cai grid 2 da tinh sau
        for (var i = 0; i < App.storeGrid2.data.length; i++) {
            App.storeGrid2.data.items[i].set("Payment", 0);
            App.storeGrid2.data.items[i].set("DocBal", App.storeGrid2.data.items[i].data.OrigDocBal);
            App.storeGrid2.data.items[i].set("Selected", false)
        }
        App.txtCuryCrTot.setValue(App.txtOdd.value + App.txtCuryCrTot.value);
        App.txtOdd.setValue(0);
        tmpAutoApplyForTotalAmountAndPayment = App.txtCuryCrTot.value; // dat bien tam cho de xai de hieu sau nay quyet dinh xoa hay ko sau
        //vong for duyet trong storegrid 2 de di qua hang nao thi xu ly hang do trong grid 2 neu TotalAmount lon hon tung hang trong grid 2
        for (var i = 0; i < App.storeGrid2.data.length; i++) {
            if (tmpAutoApplyForTotalAmountAndPayment != 0) {
                if (tmpAutoApplyForTotalAmountAndPayment < App.storeGrid2.data.items[i].data.OrigDocBal) {
                    App.storeGrid2.data.items[i].set("Payment", tmpAutoApplyForTotalAmountAndPayment);
                    App.storeGrid2.data.items[i].set("DocBal", App.storeGrid2.data.items[i].data.OrigDocBal - tmpAutoApplyForTotalAmountAndPayment);
                    tmpAutoApplyForTotalAmountAndPayment -= App.storeGrid2.data.items[i].data.OrigDocBal;
                    //giam den 1 luc nao do no se - cho cai original thanh 0 luon thi se ket thuc ko lam dang tiep theo do gia tri bien tam = 0 roi
                    if (tmpAutoApplyForTotalAmountAndPayment < 0) {
                        tmpAutoApplyForTotalAmountAndPayment = 0;
                    }
                } else { // tmpAutoApplyForTotalAmountAndPayment >= App.storeGrid2.data.items[i].data.OrigDocBal
                    //theo thu tu set lai gia tri payment la original, docbal thanh 0 , check ca vao o checkbox, giam bien tam = bien tam - gia tri original
                    App.storeGrid2.data.items[i].set("Payment", App.storeGrid2.data.items[i].data.OrigDocBal);
                    App.storeGrid2.data.items[i].set("DocBal", 0);
                    App.storeGrid2.data.items[i].set("Selected", true)
                    tmpAutoApplyForTotalAmountAndPayment -= App.storeGrid2.data.items[i].data.OrigDocBal;

                }
                tmpApplicationTotal += App.storeGrid2.data.items[i].data.Payment;
            }
        }
        App.txtPaid.setValue(tmpApplicationTotal);
        tmpApplicationTotal = 0;
    } else { //App.txtPayment.value >= App.txtOrigDocAmt.value
        //do gia tri lon hon nen luc nay o Balance se co gia tri = o Vendor Balance con o Total Amount = o Apply AMount - o Vendor Balance
        App.txtCuryCrTot.setValue(App.txtOrigDocAmt.value);
        App.txtOdd.setValue(App.txtPayment.value - App.txtOrigDocAmt.value);
        //sau do set checkAll cho cac o checkbox o grid 2 va max gia tri Payment luon , giam DocBal con 0
        for (var i = 0; i < App.storeGrid2.data.length; i++) {
            App.storeGrid2.data.items[i].set("Payment", App.storeGrid2.data.items[i].data.OrigDocBal);
            App.storeGrid2.data.items[i].set("DocBal", 0);
            App.storeGrid2.data.items[i].set("Selected", true)
        }
        App.txtPaid.setValue(0);
    }

};
//ham check thay doi cua o Total Amount se Update lai gia tri o Balance
var txtCuryCrTot_Change = function () {
    App.txtOdd.setValue(App.txtPayment.value - App.txtCuryCrTot.value);
};
//ham check thay doi cua o Apply Amount se Update lai gia tri o Balance
var txtPayment_Change = function () {
    App.txtOdd.setValue(App.txtPayment.value - App.txtCuryCrTot.value);
}
//ham check thay doi cua o Application total se Update lai gia tri o UnApplyTotal
var txtPaid_Change = function () {
    App.txtUnTotPayment.setValue(App.txtOrigDocAmt.value - App.txtPaid.value);
};




























































