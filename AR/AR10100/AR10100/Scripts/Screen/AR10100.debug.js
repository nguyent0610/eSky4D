
var selectedIndex = 0;
var _hold = "H";
var _curSlsperid = "";
var _focusrecord = 1;
var beforeedit = '';
var prefixvalue = "";
var lastfixvalue = "";
var shownextlotserial = "";
var dueDay = 0;
var oVat05 = "0";
var Vat00 = "0";
var noneVat = "0";
var oVat10 = "0";
var vat10 = "0";
var taxAmtTotalOVAT05 = 0;
var tranAmtTotalOVAT05 = 0;
var taxAmtTotalVAT00 = 0;
var tranAmtTotalVAT00 = 0;
var taxAmtTotalOVAT10 = 0;
var tranAmtTotalOVAT10 = 0;
var taxAmtTotalVAT10 = 0;
var tranAmtTotalVAT10 = 0;
var taxAmtTotalNoneVat = 0;
var tranAmtTotalNoneVat = 0;
var totalAmountMustPay = 0;
var totalTaxMustPay = 0;
var vendIDAndDescr = "";
var custIDAndName = "";
var taxAmt00 = 0;
var taxAmt01 = 0;
var taxAmt02 = 0;
var taxAmt03 = 0;
var taxAmt = [taxAmt00, taxAmt01, taxAmt02, taxAmt03];
var txblAmt = 0;
var countTaxAmt = 0;
var tmpChangeForm1OrForm2 = "0";


var keys = ['InvtID'];

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
                        App.slmGridTab1.select(App.storeGrid.getCount() - 1);

                        if (App.storeGrid.getCount() == 0) {

                            App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());
                            App.slmGridTab1.select(App.storeGrid.getCount() - 1);
                            for (var i = 0; i < App.cboCustId.getStore().data.length; i++) {
                                if (App.cboCustId.getStore().data.items[i].data.CustID == App.cboCustId.value) {
                                    custIDAndName = App.cboCustId.getStore().data.items[i].data.CustID + " - " + App.cboCustId.getStore().data.items[i].data.Name;
                                }
                            }
                            App.slmGridTab1.selected.items[0].set('TranDesc', custIDAndName)
                            App.grd.editingPlugin.startEditByPosition({ row: App.storeGrid.getCount() - 1, column: 1 });

                        } else if (App.slmGridTab1.selected.items[0].data.LineType != "") {
                            App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());
                            App.slmGridTab1.select(App.storeGrid.getCount() - 1);
                            for (var i = 0; i < App.cboCustId.getStore().data.length; i++) {
                                if (App.cboCustId.getStore().data.items[i].data.CustID == App.cboCustId.value) {
                                    custIDAndName = App.cboCustId.getStore().data.items[i].data.CustID + " - " + App.cboCustId.getStore().data.items[i].data.Name;
                                }
                            }
                            App.slmGridTab1.selected.items[0].set('TranDesc', custIDAndName)
                            App.grd.editingPlugin.startEditByPosition({ row: App.storeGrid.getCount() - 1, column: 1 });
                        }
                    }



                } else if (_focusrecord == 1) {
                    //tmpChangeForm1OrForm2 = "1";
                    App.cboRefNbr.setValue('');
                    //App.storeFormBot.reload();
                    setTimeout(function () { waitcboRefNbrForInsert(); }, 1500);

                } else if (_focusrecord == 3) {
                    //tmpChangeForm1OrForm2 = "2";
                    App.cboBatNbr.setValue('');

                    App.frmTop.getForm().reset(true);

                    App.cboRefNbr.getStore().reload();
                    App.cboRefNbr.setValue('');


                    setTimeout(function () { waitcboRefNbrForInsert(); }, 1500);
                    setTimeout(function () { setStatusValueH(); }, 1500);
                    setTimeout(function () { waitcboStatusReLoad(); }, 1700);
                    //cboBatNbr_Change();
                    //App.cboRefNbr.setValue('');
                    //App.storeFormTop.reload();
                    //var time = new Date();
                    //var recordTop = Ext.create("App.BatchClassModel", {
                    //    //CustId: "",
                    //    Status: "H",
                    //    OrigDocAmt: 0,
                    //    TotAmt: 0,


                    //});
                    //App.storeFormTop.insert(0, recordTop);
                    //App.storeFormTop.reload();
                    //App.cboCustId.enable(true);
                    //App.cboDocType.enable(true);

                    //var recordBot = Ext.create("App.AP_DocClassModel", {
                    //    //CustId: "",
                    //    DocType: App.cboDocType.store.data.items[0].data.Code,
                    //    DocDate: time,
                    //    InvcDate: time,
                    //    DiscDate: time,

                    //    OrigDocAmt: 0,
                    //    DocBalL: 0,
                    //    BatNbr: App.cboBatNbr.getValue(),
                    //    BranchID: App.txtBranchID.getValue(),
                    //    Terms: App.cboTerms.getValue(),

                    //});
                    //App.storeFormBot.insert(0, recordBot);
                    //App.frmBot.getForm().loadRecord(App.storeFormBot.getAt(0));
                    //cboTerms_Change();
                }

            }
            break;
        case "delete":
            var curRecord = App.frmMain.getRecord();
            if (HQ.isDelete) {
                if (App.cboBatNbr.value != "") {

                    if (_focusrecord == 3) {
                        HQ.message.show(11, '', 'deleteRecordFormTopBatch');
                    } else if (_focusrecord == 2) {
                        HQ.message.show(11, '', 'deleteRecordGrid')
                    } else if (_focusrecord == 1) {
                        HQ.message.show(11, '', 'deleteRecordFormBotAP_Doc')
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
    App.cboCustId.enable(true);
    App.cboDocType.enable(true);
    var time = new Date();
    App.txtBranchID.setValue(HQ.cpnyID);
    if (App.storeFormBot.data.items.length == 0) {
        var record = Ext.create("App.AP_DocClassModel", {
            //CustId: "",
            DocType: App.cboDocType.store.data.items[0].data.Code,
            DocDate: time,
            InvcDate: time,
            DiscDate: time,
            OrigDocAmt: 0,
            DocBalL: 0,
            BatNbr: App.cboBatNbr.getValue(),
            BranchID: App.txtBranchID.getValue(),
            Terms: App.cboTerms.getValue(),

        });
        App.storeFormBot.insert(0, record);
        App.frmBot.getForm().loadRecord(App.storeFormBot.getAt(0));
    }
    cboTerms_Change();
    App.storeGrid.reload();
}

var setStatusValueH = function () {
    App.cboStatus.setValue("H");
    App.cboHandle.getStore().reload();
}
function Save() {

    //duyet vong for de lay cac gia tri con thieu cho Grid
    for (var i = 0; i <= App.storeGrid.getCount() - 1; i++) {
        App.slmGridTab1.select(i);
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
        //duyet grid top tab 2 de lay ra cac gia tri co cung LineRef de lay TaxAmt va txblAmt  
        for (var k = 0; k <= App.storeGridTopTab2.getCount() - 1; k++) {
            App.slmGridTopTab2.select(k);
            if (App.slmGridTopTab2.selected.items[0].data.LineRef == App.storeGrid.data.items[i].data.LineRef) {
                //bien count de dem xem co bao nhieu cai cung LineRef de save vao cac bien TaxAmt 0 > 3 va bien txblAmt 0 > 3 cho dung 
                countTaxAmt++;
                if (countTaxAmt == 1) {
                    //taxAmt[0] = parseFloat(App.slmGridTopTab2.selected.items[0].data.TaxAmt);
                    //txblAmt = parseFloat(App.slmGridTopTab2.selected.items[0].data.TxblAmt);
                    App.storeGrid.data.items[i].set('TaxAmt00', parseFloat(App.slmGridTopTab2.selected.items[0].data.TaxAmt));
                    App.storeGrid.data.items[i].set('TaxAmt01', 0);
                    App.storeGrid.data.items[i].set('TaxAmt02', 0);
                    App.storeGrid.data.items[i].set('TaxAmt03', 0);
                    App.storeGrid.data.items[i].set('TxblAmt00', parseFloat(App.slmGridTopTab2.selected.items[0].data.TxblAmt));
                } else if (countTaxAmt == 2) {
                    App.storeGrid.data.items[i].set('TaxAmt01', parseFloat(App.slmGridTopTab2.selected.items[0].data.TaxAmt));
                    App.storeGrid.data.items[i].set('TxblAmt01', parseFloat(App.slmGridTopTab2.selected.items[0].data.TxblAmt));
                } else if (countTaxAmt == 3) {
                    App.storeGrid.data.items[i].set('TaxAmt02', parseFloat(App.slmGridTopTab2.selected.items[0].data.TaxAmt));
                    App.storeGrid.data.items[i].set('TxblAmt02', parseFloat(App.slmGridTopTab2.selected.items[0].data.TxblAmt));
                } else if (countTaxAmt == 4) {
                    App.storeGrid.data.items[i].set('TaxAmt03', parseFloat(App.slmGridTopTab2.selected.items[0].data.TaxAmt));
                    App.storeGrid.data.items[i].set('TxblAmt03', parseFloat(App.slmGridTopTab2.selected.items[0].data.TxblAmt));
                }
            }

        }
        countTaxAmt = 0;


    }
    //var curRecord = App.frmMain.getRecord();
    //if (tmpChangeForm1OrForm2 == "1") {
    //    App.frmBot.getForm().updateRecord();
    //} else if (tmpChangeForm1OrForm2 == "2") {
    //    App.frmMain.getForm().updateRecord();
    //}
    App.frmBot.getForm().updateRecord();

    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: 'Submiting...',
            url: 'AR10100/Save',
            params: {
                lstheaderTop: HQ.store.getData(App.storeFormTop),
                lstheaderBot: HQ.store.getData(App.storeFormBot),
                lstgrd: HQ.store.getData(App.storeGrid),
                BranchID: App.txtBranchID.getValue(),
                Handle: App.cboHandle.getValue(),
                BatNbr: App.cboBatNbr.getValue(),
                RefNbr: App.cboRefNbr.getValue(),
                DocType: App.cboDocType.getValue(),
                IntRefNbr: App.txtOrigRefNbr.getValue(),

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
                        setTimeout(function () { AfterSaveWaitBatNbrStoreReload(data.result.value2.toString()); }, 100);
                        //App.cboBatNbr.setValue(data.result.value2.toString());
                        App.cboRefNbr.getStore().reload(); //neu them vao cai BatNbr moi thi phai reset store cai RefNbr moi lay gia tri duoc
                    } else if (data.result.value2.toString() == "" && data.result.value1.toString() != "") {
                        App.cboRefNbr.getStore().reload();
                        setTimeout(function () { AfterSaveWaitRefNbrStoreReload(data.result.value1.toString()); }, 100);
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
                        setTimeout(function () { AfterSaveWaitBatNbrStoreReload(data.result.value2.toString()); }, 100);
                        //App.cboBatNbr.setValue(data.result.value2.toString());
                        App.cboRefNbr.getStore().reload(); //neu them vao cai BatNbr moi thi phai reset store cai RefNbr moi lay gia tri duoc
                    } else if (data.result.value2.toString() == "" && data.result.value1.toString() != "") {
                        App.cboRefNbr.getStore().reload();
                        setTimeout(function () { AfterSaveWaitRefNbrStoreReload(data.result.value1.toString()); }, 100);
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
        var fields = App.frmMain.getForm().getFields().each(
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
}

var AfterSaveWaitRefNbrStoreReload = function (data) {
    App.cboRefNbr.setValue(data);
}
// Xem lai
function Close() {
    if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
    if (App.storeGrid.getChangedData().Updated == undefined &&
        App.storeGrid.getChangedData().Deleted == undefined &&
        App.frmMain.getRecord() == undefined)
        parent.App.tabAR10100.close();
    else if (App.storeGrid.getChangedData().Updated != undefined ||
        App.storeGrid.getChangedData().Created != undefined ||
        App.storeGrid.getChangedData().Deleted != undefined ||
        storeIsChange(App.storeForm, false)) {
        HQ.message.show(5, '', 'closeScreen');
    } else {
        parent.App.tabAR10100.close();
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
        if (parent.App.tabAR10100 != null)
            parent.App.tabAR10100.close();
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
// Xac nhan xoa record cua form top Batch
var deleteRecordFormBotAP_Doc = function (item) {
    if (item == "yes") {

        try {
            App.direct.DeleteFormBotAP_Doc(App.cboRefNbr.getValue(), App.cboBatNbr.getValue(), App.txtBranchID.getValue(), {
                success: function (data) {
                    //menuClick('refresh');
                    App.cboRefNbr.getStore().reload();
                    App.cboRefNbr.setValue('');


                    setTimeout(function () { waitStoreRefNbrReLoad(); }, 1000);

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
};
var grd_BeforeEdit = function (editor, e) {
    if (!HQ.isUpdate)
        return false;
    //keys = e.record.idProperty.split(',');
    if (keys.indexOf(e.field) != -1) {
        if (e.record.data.tstamp != "") {
            return false;
        }
    }
    return HQ.grid.checkInput(e, keys);

};
var grd_Edit = function (item, e) {

    if (keys.indexOf(e.field) != -1) {
        if (e.value != '' && isAllValidKey(App.storeGrid.getChangedData().Created) && isAllValidKey(App.storeGrid.getChangedData().Updated))
            //App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());//Ext.data.Record() 
            //App.slmGridTab1.selected.items[0].set('TranDesc', vendIDAndDescr);

            var record = Ext.create("App.AR10100_pgLoadGridTrans_ResultModel", {
                //CustId: "",
                TranDesc: App.slmGridTab1.selected.items[0].data.TranDesc



            });
        App.storeGrid.insert(App.storeGrid.getCount(), record);
    }

};
var grd_ValidateEdit = function (item, e) {

    if (keys.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(App.grd, e, keys)) {
            HQ.message.show(1112, e.value, '');
            App.slmGridTab1.selected.items[0].set('InvtID', '');
            return false;
        }
    }
    return true;
};

var grd_CancelEdit = function (editor, e) {
    if (e.record.phantom) {
        if (App.cboLineType.getValue == "") {
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

}
var loadDataAutoHeaderTop = function () {

    //if (App.storeForm.getCount() == 0) {
    //    App.storeForm.insert(0, Ext.data.Record());
    //}
    var record = App.storeFormTop.getAt(0);
    if (record != undefined) {
        App.frmTop.getForm().loadRecord(record);
        App.cboRefNbr.getStore().reload();
        setTimeout(function () { waitStoreRefNbrReLoad(); }, 1000);
        //if (App.cboRefNbr.getStore().data.items[0] == undefined) {
        //    App.cboRefNbr.setValue("");
        //} else {
        //    App.cboRefNbr.setValue(App.cboRefNbr.getStore().data.items[0].data.RefNbr);
        //}
    }
    //else {
    //    App.frmTop.getForm().loadRecord(""); // cai nay can sua ////////////////////////////////////////////////////////////
    //    App.cboRefNbr.getStore().reload();
    //    setTimeout(function () { waitStoreRefNbrReLoad(); }, 1000);
    //}
};

var waitcboStatusReLoad = function () {
    if (App.cboStatus.value == "H") {
        setReadOnly();
        App.cboHandle.setValue(App.cboHandle.store.data.items[0].data.Code);
    } else {
        setReadOnly();
        App.cboHandle.setValue(App.cboHandle.store.data.items[1].data.Code);
    }
}
var cboStatus_Change = function () {
    App.cboHandle.getStore().reload();
    setTimeout(function () { waitcboStatusReLoad(); }, 800);
}

//var waitcboRefNbrReLoad = function () {
//    if(App.cboRefNbr.store.data.items[0] != undefined){
//        App.cboRefNbr.setValue(App.cboRefNbr.store.data.items[0].data.RefNbr);
//    }
//}
var cboBatNbr_Change = function () {
    App.cboHandle.setValue(App.cboHandle.store.data.items[0].data.Code);
    App.cboCustId.enable(true);
    App.cboDocType.enable(true);
    App.txtBranchID.setValue(HQ.cpnyID);
    App.storeFormTop.reload();
    //App.cboRefNbr.getStore().reload();
    //App.cboRefNbr.setValue(App.cboRefNbr.store.data.items[0].data.RefNbr);
    //setTimeout(function () { waitcboRefNbrReLoad(); }, 1200);
    App.cboCustId.disable(true);
    App.cboDocType.disable(true);
}

var cboRefNbr_Change = function () {

    App.storeFormBot.reload();
    setTimeout(function () { waitStoreFormBotReLoad(); }, 1000);

}

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
}

var Focus1_Change = function (sender, e) {

    _focusrecord = 1;
}

var Focus3_Change = function (sender, e) {

    _focusrecord = 3;
}

var setValueBranchID = function () {
    App.txtBranchID.setValue(HQ.cpnyID);

};


var checkRequire = function (items) {
    if (items != undefined) {
        for (var i = 0; i < items.length; i++) {
            //if (items[i]["LineType"] == undefined)
            //    continue;
            //if (items[i]["LineType"] == "")
            //    continue;
            if (HQ.grid.checkRequirePass(items[i], keys)) continue;

            if (items[i]["LineType"].trim() == "") {
                HQ.message.show(15, '@Util.GetLang("LineType")', null);
                return false;
            }
            //if (items[i]["InvtID"].trim() == "") {
            //    HQ.message.show(15, '@Util.GetLang("InvtID")', null);
            //    return false;
            //}
            if (items[i]["LineType"].trim() != "" && items[i]["Qty"] == 0) {
                HQ.message.show(15, '@Util.GetLang("Qty")', null);
                return false;
            }
            if (items[i]["LineType"].trim() != "" && items[i]["UnitPrice"] == 0) {
                HQ.message.show(15, '@Util.GetLang("UnitPrice")', null);
                return false;
            }



        }
        return true;
    }
    else {
        return true;
    }
};



var leapYear = function (year) {
    return ((year % 4 == 0) && (year % 100 != 0)) || (year % 400 == 0);
}

Date.fromDayofYear = function (n, y) {
    if (!y) y = new Date().getFullYear();
    var d = new Date(y, 0, 1);
    return new Date(d.setMonth(0, n));
}
Date.prototype.dayOfYear = function () {
    var j1 = new Date(this);
    j1.setMonth(0, 0);
    return Math.round((this - j1) / 8.64e7);
};

var cboTerms_Change = function () {
    if (App.cboTerms.value != "" && App.cboTerms.value != null) {
        //var DocDate = App.txtDocDate.value.dayOfYear();
        var time = new Date();
        var timeRight = App.txtDocDate.value.getFullYear() - time.getFullYear();
        var DocDate = 0;
        if (timeRight == 1 || timeRight == 0) {
            if (leapYear(time.getFullYear())) {
                DocDate = App.txtDocDate.value.dayOfYear() + (timeRight * 366);
            } else {
                DocDate = App.txtDocDate.value.dayOfYear() + (timeRight * 365);
            }
        } else if (timeRight < 0) {
            DocDate = App.txtDocDate.value.dayOfYear() + (timeRight * 365);
        } else if (timeRight > 1) {

        }
        var Term = parseFloat(App.cboTerms.getValue());
        dueDay = DocDate + Term;
        var x = Date.fromDayofYear(dueDay);
        App.txtDueDate.setValue(x);
    } else {
        App.txtDueDate.setValue(App.txtDocDate.value);
    }
}

var txtDocDate_Change = function (sender, e) {
    if (App.cboTerms.value != "" && App.cboTerms.value != null) {
        var time = new Date();
        var timeRight = e.getFullYear() - time.getFullYear();
        var DocDate = 0;
        if (timeRight == 1 || timeRight == 0) {
            if (leapYear(time.getFullYear())) {
                DocDate = App.txtDocDate.value.dayOfYear() + (timeRight * 366);
            } else {
                DocDate = App.txtDocDate.value.dayOfYear() + (timeRight * 365);
            }
        } else if (timeRight < 0) {
            DocDate = App.txtDocDate.value.dayOfYear() + (timeRight * 365);
        } else if (timeRight > 1) {

        }
        var Term = parseFloat(App.cboTerms.getValue());
        dueDay = DocDate + Term;
        var x = Date.fromDayofYear(dueDay);
        App.txtDueDate.setValue(x);
    } else {
        App.txtDueDate.setValue(App.txtDocDate.value);
    }
}

var tabSA_Setup_AfterRender = function (obj, padding) {
    if (this.parentAutoLoadControl != null)
        obj.setHeight(this.parentAutoLoadControl.getHeight() - padding);
    else {
        obj.setHeight(Ext.getBody().getViewSize().height - padding);
    }
};


//doi grid 1 load xong se load du lieu tu grid 1 len grid 2 , 3
var waitStoreGridTab1ReLoad = function () {

    App.tabAtAR10100.setActiveTab(App.frmBot); // set tab dang su dung sang tab 1
    App.storeGridTopTab2.removeAll();
    App.storeGridBotTab2.removeAll();
    ////duyet het tat cac dong cua grid 1 tab 1 de bat ra cac du lieu bo vao grid 2 cua tab 2
    //fillDataIntoGrid1Tab2();
    ////duyet grid 1 tab 2 de lay gia tri bo qua grid 2 tab 2 
    //fillDataIntoGrid2Tab2();

    reloadDataGrid1AndGrid2Tab2(0);


    //App.slmGridTopTab2.select(0);
    App.tabAtAR10100.setActiveTab(App.frmBot); // set tab dang su dung sang tab 1
    //App.slmGridTab1.select(0);//chon dong 1 cua grid 1 tab 1
    setTimeout(function () { waitGridLoadAndFocusIntoBatNbr(); }, 1000);

}

var waitGridLoadAndFocusIntoBatNbr = function () {
    App.cboBatNbr.focus();
}


//doi form 2 tab 1 load xong
var waitStoreFormBotReLoad = function () {
    App.cboTaxID.getStore().reload();
    //App.cboTaxID1.getStore().reload();
    App.storeGrid.reload();
    setTimeout(function () { waitStoreGridTab1ReLoad(); }, 1000);

}

//ham change cboRefNbr

//ham dung de hien thi display value la descr thay vi Code(ID) trong Grid
var renderLineType = function (value) {
    var record = App.cboLineType.getStore().findRecord("Code", value);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};

//ham dung de render bo ca TaxId01 va TaxId02 vao TaxId00 
//var RenderTaxId = function (value, p, r) {
//    //if (r.data.TaxId00.search(",") != -1) {
//    //    var taxid00 = r.data.TaxId00;
//    //    var taxid00change = taxid00.substring(0, r.data.TaxId00.search(","));
//    //    r.set('TaxId00', taxid00change);
//    //}
//    if (r.data.TaxId01 == "" && r.data.TaxId02 != "") {
//        r.set('TaxId02', "");
//    }
//    if (r.data.TaxId00 != "" && r.data.TaxId01 != "" && r.data.TaxId02 != "") {
//        return r.data['TaxId00'] + ',' + r.data['TaxId01'] + ',' + r.data['TaxId02'];
//    } else if (r.data.TaxId00 != "" && r.data.TaxId01 != "" && r.data.TaxId02 == "") {
//        return r.data['TaxId00'] + ',' + r.data['TaxId01'];
//    } else if (r.data.TaxId00 != "" && r.data.TaxId01 == "" && r.data.TaxId02 == "") {
//        return r.data['TaxId00'];
//    }

//};
//khi cboTaxCat thay doi thi
var cboTaxCat_Change = function (item) {
    App.slmGridTab1.selected.items[0].set('TaxCat', item.value);
    var index = App.slmGridTab1.selected.items[0].index;
    //xet dieu kien neu them moi thi index se bat khac di , do LineType khac rong se tao ra dong moi nen neu LineType chua co ta bat index tu dong la getCount()-1
    if (index == undefined && App.slmGridTab1.selected.items[0].data.InvtID != "") {
        for (var i = 0; i < App.storeGrid.data.length; i++) {
            if (App.storeGrid.data.items[i].data.InvtID == App.slmGridTab1.selected.items[0].data.InvtID) {
                index = i;
            }
        }
    }
    //tai lai du lieu cua grid 1 va 2 cua tab 2 do moi cap nhap o grid 1 tab 1
    reloadDataGrid1AndGrid2Tab2(index);
    //reload lai tong tien , so tien no goc , so du chung tu (TotAmt, OrigDocAmt , DocBal)
    reloadAmountMustPayTotal(index);

}





//khi cboTaxID thay doi thi
var cboTaxID_Change = function (item) {//obj = App.cboTaxID
    //obj.value
    App.slmGridTab1.selected.items[0].set('TaxId01', "");
    App.slmGridTab1.selected.items[0].set('TaxId02', "");
    App.slmGridTab1.selected.items[0].set('TaxId03', "");
    for (var i = 0; i < item.value.length; i++) {

        if (i == 0) {
            App.slmGridTab1.selected.items[0].set('TaxId00', item.value[0]);
        } else if (i == 1) {
            App.slmGridTab1.selected.items[0].set('TaxId01', item.value[1]);
        } else if (i == 2) {
            App.slmGridTab1.selected.items[0].set('TaxId02', item.value[2]);
        } else if (i == 3) {
            App.slmGridTab1.selected.items[0].set('TaxId03', item.value[3]);
        }
    }
    var index = App.slmGridTab1.selected.items[0].index;
    //xet dieu kien neu them moi thi index se bat khac di , do LineType khac rong se tao ra dong moi nen neu LineType chua co ta bat index tu dong la getCount()-1
    if (index == undefined && App.slmGridTab1.selected.items[0].data.InvtID != "") {
        for (var i = 0; i < App.storeGrid.data.length; i++) {
            if (App.storeGrid.data.items[i].data.InvtID == App.slmGridTab1.selected.items[0].data.InvtID) {
                index = i;
            }
        }
    }
    //tai lai du lieu cua grid 1 va 2 cua tab 2 do moi cap nhap o grid 1 tab 1
    reloadDataGrid1AndGrid2Tab2(index);
    //reload lai tong tien , so tien no goc , so du chung tu (TotAmt, OrigDocAmt , DocBal)
    reloadAmountMustPayTotal(index);


};

//thay doi so luong
var txtQty_Change = function (item) {
    //bat cac gia tri de chuan bi tinh toan lai
    var quantity = item.value;
    var unitprice = App.slmGridTab1.selected.items[0].data.UnitPrice;
    var index = App.slmGridTab1.selected.items[0].index;
    //xet dieu kien neu them moi thi index se bat khac di , do LineType khac rong se tao ra dong moi nen neu LineType chua co ta bat index tu dong la getCount()-1
    if (index == undefined && App.slmGridTab1.selected.items[0].data.InvtID != "") {
        for (var i = 0; i < App.storeGrid.data.length; i++) {
            if (App.storeGrid.data.items[i].data.InvtID == App.slmGridTab1.selected.items[0].data.InvtID) {
                index = i;
            }
        }
    } else if (index == undefined && App.slmGridTab1.selected.items[0].data.InvtID == "") {
        index = 0;
    }
    App.slmGridTab1.selected.items[0].set('TranAmt', quantity * unitprice);
    //tai lai du lieu cua grid 1 va 2 cua tab 2 do moi cap nhap o grid 1 tab 1
    reloadDataGrid1AndGrid2Tab2(index);
    //reload lai tong tien , so tien no goc , so du chung tu (TotAmt, OrigDocAmt , DocBal)
    reloadAmountMustPayTotal(index);
    //chon lai cot minh dang edit
    App.grd.editingPlugin.startEditByPosition({ row: index, column: 3 });

}
//thay doi gia cho 1 dv san pham
var txtUnitPrice_Change = function (item) {
    //bat cac gia tri de chuan bi tinh toan lai
    var quantity = App.slmGridTab1.selected.items[0].data.Qty;
    var unitprice = item.value;
    var index = App.slmGridTab1.selected.items[0].index;
    //xet dieu kien neu them moi thi index se bat khac di , do LineType khac rong se tao ra dong moi nen neu LineType chua co ta bat index tu dong la getCount()-1
    if (index == undefined && App.slmGridTab1.selected.items[0].data.InvtID != "") {
        for (var i = 0; i < App.storeGrid.data.length; i++) {
            if (App.storeGrid.data.items[i].data.InvtID == App.slmGridTab1.selected.items[0].data.InvtID) {
                index = i;
            }
        }

    } else if (index == undefined && App.slmGridTab1.selected.items[0].data.InvtID == "") {
        index = 0;
    }
    App.slmGridTab1.selected.items[0].set('TranAmt', quantity * unitprice);
    //tai lai du lieu cua grid 1 va 2 cua tab 2 do moi cap nhap o grid 1 tab 1
    reloadDataGrid1AndGrid2Tab2(index);
    //reload lai tong tien , so tien no goc , so du chung tu (TotAmt, OrigDocAmt , DocBal)
    reloadAmountMustPayTotal(index);
    //chon lai cot minh dang edit
    App.grd.editingPlugin.startEditByPosition({ row: index, column: 4 });
}
//thay doi gia tong cong 
var txtTranAmt_Change = function (item) {
    //bat cac gia tri de chuan bi tinh toan lai
    var tranAmt = item.value;
    var quantity = App.slmGridTab1.selected.items[0].data.Qty;
    var index = App.slmGridTab1.selected.items[0].index;
    //xet dieu kien neu them moi thi index se bat khac di , do LineType khac rong se tao ra dong moi nen neu LineType chua co ta bat index tu dong la getCount()-1
    if (index == undefined && App.slmGridTab1.selected.items[0].data.InvtID != "") {
        for (var i = 0; i < App.storeGrid.data.length; i++) {
            if (App.storeGrid.data.items[i].data.InvtID == App.slmGridTab1.selected.items[0].data.InvtID) {
                index = i;
            }
        }
    } else if (index == undefined && App.slmGridTab1.selected.items[0].data.InvtID == "") {
        index = 0;
    }
    App.slmGridTab1.selected.items[0].set('UnitPrice', tranAmt / quantity);
    //tai lai du lieu cua grid 1 va 2 cua tab 2 do moi cap nhap o grid 1 tab 1
    reloadDataGrid1AndGrid2Tab2(index);
    //reload lai tong tien , so tien no goc , so du chung tu (TotAmt, OrigDocAmt , DocBal)
    reloadAmountMustPayTotal(index);
    //chon lai cot minh dang edit
    App.grd.editingPlugin.startEditByPosition({ row: index, column: 5 });

}
//tai lai du lieu cua grid 1 va 2 cua tab 2 do moi cap nhap o grid 1 tab 1
var reloadDataGrid1AndGrid2Tab2 = function (index) {
    //xoa du lieu cu de cap nhap du lieu moi
    App.storeGridTopTab2.removeAll();
    App.storeGridBotTab2.removeAll();
    //duyet het tat cac dong cua grid 1 tab 1 de bat ra cac du lieu bo vao grid 2 cua tab 2

    fillDataIntoGrid1Tab2();


    //duyet grid 1 tab 2 de lay gia tri bo qua grid 2 tab 2 
    fillDataIntoGrid2Tab2();
    App.tabAtAR10100.setActiveTab(App.frmBot); // set tab dang su dung sang tab 1
    App.slmGridTab1.select(index);
}
//reload lai tong tien , so tien no goc , so du chung tu (TotAmt, OrigDocAmt , DocBal)
var reloadAmountMustPayTotal = function (index) {
    totalAmountMustPay = 0;
    totalTaxMustPay = 0;
    //duyet grid bot tab 2 de lay ra so tien thue phai nop
    for (var i = 0; i <= App.storeGridBotTab2.getCount() - 1; i++) {
        App.slmGridBotTab2.select(i);
        totalTaxMustPay = totalTaxMustPay + parseFloat(App.slmGridBotTab2.selected.items[0].data.TaxAmt2);
    }
    //duyet grid 1 de lay so tien goc cua minh
    for (var k = 0; k <= App.storeGrid.getCount() - 1; k++) {
        App.slmGridTab1.select(k);
        totalAmountMustPay = totalAmountMustPay + App.slmGridTab1.selected.items[0].data.TranAmt;
    }
    //set lai gia tri cua 3 o kia
    App.txtCuryCrTot.setValue(totalTaxMustPay + totalAmountMustPay);
    App.txtCuryOrigDocAmt.setValue(totalTaxMustPay + totalAmountMustPay);
    App.txtCuryDocBal.setValue(totalTaxMustPay + totalAmountMustPay);
    App.txtTxblTot.setValue(totalAmountMustPay);
    App.txtTaxTot.setValue(totalTaxMustPay);
    App.slmGridTab1.select(index);
}
//khi VendID thay doi
var cboCustId_Change = function () {
    App.cboTaxID.getStore().reload();
    App.cboTerms.setValue("10");
}


//ham validate so duong
var validatePossitiveNumber = function (field, e) {
    if (event.keyCode == e.NUM_MINUS) {
        e.stopEvent();
    }
}


var setReadOnly = function () {
    if (App.cboStatus.value != "H") {
        App.cboHandle.setReadOnly(true);
        App.cboRefNbr.setReadOnly(true);
        App.txtInvcNbr.setReadOnly(true);
        App.txtInvcNote.setReadOnly(true);
        App.txtDocDate.setReadOnly(true);
        App.cboTerms.setReadOnly(true);
        //App.txtInvcDate.setReadOnly(true);
        //App.txtPONbr.setReadOnly(true);
        //App.txtRcptNbr.setReadOnly(true);
        App.txtDiscDate.setReadOnly(true);
        App.txtDocDescr.setReadOnly(true);
        App.txtDueDate.setReadOnly(true);
        App.grd.disable(true);
    } else {
        App.cboHandle.setReadOnly(false);
        App.cboRefNbr.setReadOnly(false);
        App.txtInvcNbr.setReadOnly(false);
        App.txtInvcNote.setReadOnly(false);
        App.txtDocDate.setReadOnly(false);
        App.cboTerms.setReadOnly(false);
        //App.txtInvcDate.setReadOnly(false);
        //App.txtPONbr.setReadOnly(false);
        //App.txtRcptNbr.setReadOnly(false);
        App.txtDiscDate.setReadOnly(false);
        App.txtDocDescr.setReadOnly(false);
        App.txtDueDate.setReadOnly(false);
        App.grd.enable(true);
    }
}

var removeReadOnly = function () {

}




































//duyet grid 1 tab 2 de lay gia tri bo qua grid 2 tab 2 , sau nay se tach ra thanh function rieng
var fillDataIntoGrid2Tab2 = function () {
    //mai 3-10-2014 sua cai ham nay

    taxAmtTotalVAT00 = 0;
    tranAmtTotalVAT00 = 0;
    taxAmtTotalNoneVat = 0;
    tranAmtTotalNoneVat = 0;
    taxAmtTotalOVAT05 = 0;
    tranAmtTotalOVAT05 = 0;
    taxAmtTotalOVAT10 = 0;
    tranAmtTotalOVAT10 = 0;


    Vat00 = "0";
    noneVat = "0";
    oVat05 = "0";
    oVat10 = "0";

    for (var i = 0; i <= App.storeGridTopTab2.getCount() - 1; i++) {
        App.slmGridTopTab2.select(i);
        //cac bien tam phai khai bao de truyen gia tri vao
        // taxAmtTotalOVAT05        taxAmtTotalVAT00          oVat05             Vat00             noneVat
        // tranAmtTotalOVAT05      tranAmtTotalVAT00        tranAmtTotalNoneVat

        if (App.slmGridTopTab2.selected.items[0].data.TaxID == "OVAT05-00") {
            oVat05 = "1";
            taxAmtTotalOVAT05 = taxAmtTotalOVAT05 + parseFloat(App.slmGridTopTab2.selected.items[0].data.TaxAmt)
            tranAmtTotalOVAT05 = tranAmtTotalOVAT05 + parseFloat(App.slmGridTopTab2.selected.items[0].data.TxblAmt)
        } else if (App.slmGridTopTab2.selected.items[0].data.TaxID == "VAT00") {
            Vat00 = "1";
            taxAmtTotalVAT00 = taxAmtTotalVAT00 + parseFloat(App.slmGridTopTab2.selected.items[0].data.TaxAmt)
            tranAmtTotalVAT00 = tranAmtTotalVAT00 + parseFloat(App.slmGridTopTab2.selected.items[0].data.TxblAmt)
        } else if (App.slmGridTopTab2.selected.items[0].data.TaxID == "NONEVAT") {
            noneVat = "1";
            taxAmtTotalNoneVat = taxAmtTotalNoneVat + parseFloat(App.slmGridTopTab2.selected.items[0].data.TaxAmt)
            tranAmtTotalNoneVat = tranAmtTotalNoneVat + parseFloat(App.slmGridTopTab2.selected.items[0].data.TxblAmt)
        } else if (App.slmGridTopTab2.selected.items[0].data.TaxID == "OVAT10-00") {
            oVat10 = "1";
            taxAmtTotalOVAT10 = taxAmtTotalOVAT10 + parseFloat(App.slmGridTopTab2.selected.items[0].data.TaxAmt)
            tranAmtTotalOVAT10 = tranAmtTotalOVAT10 + parseFloat(App.slmGridTopTab2.selected.items[0].data.TxblAmt)
        }
    }
    //khai bao cac record cho tung truong hop

    var recordVAT00Grid2Tab2 = Ext.create("App.GetGrid2Tab2ResultModel", {
        TaxRate2: "0",
        TaxID2: "VAT00",
        TaxAmt2: taxAmtTotalVAT00.toString(),
        TxblAmt2: tranAmtTotalVAT00.toString(),
    });

    var recordVATNONEGrid2Tab2 = Ext.create("App.GetGrid2Tab2ResultModel", {
        TaxRate2: "0",
        TaxID2: "NONEVAT",
        TaxAmt2: taxAmtTotalNoneVat.toString(),
        TxblAmt2: tranAmtTotalNoneVat.toString(),
    });

    var recordOVAT05Grid2Tab2 = Ext.create("App.GetGrid2Tab2ResultModel", {
        TaxRate2: "5",
        TaxID2: "OVAT05-00",
        TaxAmt2: taxAmtTotalOVAT05.toString(),
        TxblAmt2: tranAmtTotalOVAT05.toString(),

    });

    var recordOVAT10Grid2Tab2 = Ext.create("App.GetGrid2Tab2ResultModel", {
        TaxRate2: "10",
        TaxID2: "OVAT10-00",
        TaxAmt2: taxAmtTotalOVAT10.toString(),
        TxblAmt2: tranAmtTotalOVAT10.toString(),
    });


    //xet tung truong hop de tao ra cac dong tuong ung
    if (oVat05 == "1") {
        App.storeGridBotTab2.insert(0, recordOVAT05Grid2Tab2);
    }
    if (Vat00 == "1") {
        App.storeGridBotTab2.insert(0, recordVAT00Grid2Tab2);
    }
    if (noneVat == "1") {
        App.storeGridBotTab2.insert(0, recordVATNONEGrid2Tab2);
    }
    if (oVat10 == "1") {
        App.storeGridBotTab2.insert(0, recordOVAT10Grid2Tab2);
    }

}








var fillDataIntoGrid1Tab2 = function () {
    for (var i = 0; i <= App.storeGrid.getCount() - 1; i++) {
        App.slmGridTab1.select(i);
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
        //lay cac gia tri can thiet khac
        var tranAmt = App.slmGridTab1.selected.items[0].data.TranAmt;
        var taxCat = App.slmGridTab1.selected.items[0].data.TaxCat;
        var taxID0 = App.slmGridTab1.selected.items[0].data.TaxId00;
        var taxID1 = App.slmGridTab1.selected.items[0].data.TaxId01;
        var taxID2 = App.slmGridTab1.selected.items[0].data.TaxId02;
        var taxID3 = App.slmGridTab1.selected.items[0].data.TaxId03;
        App.tabAtAR10100.setActiveTab(App.tabTax);

        var taxAmtVAT05 = tranAmt * 5 / 100;
        var taxAmtVAT10 = tranAmt / 10;
        var taxAmtVATNONE = 0;
        var taxAmtVAT00 = 0;

        //khai bao cac record cho tung truong hop




        var recordVATNONEGrid1Tab2 = Ext.create("App.GetGrid1Tab2ResultModel", {
            //CustId: "",
            LineRef: lineRef,
            TaxID: "NONEVAT",
            TaxRate: "0",
            TaxAmt: taxAmtVATNONE.toString(),
            TxblAmt: tranAmt.toString(),
            Level: "1"
        });

        var recordVAT00Grid1Tab2 = Ext.create("App.GetGrid1Tab2ResultModel", {
            //CustId: "",
            LineRef: lineRef,
            TaxID: "VAT00",
            TaxRate: "0",
            TaxAmt: taxAmtVAT00.toString(),
            TxblAmt: tranAmt.toString(),
            Level: "1"
        });

        var recordOVAT05Grid1Tab2 = Ext.create("App.GetGrid1Tab2ResultModel", {

            LineRef: lineRef,
            TaxID: "OVAT05-00",
            TaxRate: "5",
            TaxAmt: taxAmtVAT05.toString(),
            TxblAmt: tranAmt.toString(),
            Level: "1"
        });

        var recordOVAT10Grid1Tab2 = Ext.create("App.GetGrid1Tab2ResultModel", {
            //CustId: "",
            LineRef: lineRef,
            TaxID: "OVAT10-00",
            TaxRate: "10",
            TaxAmt: taxAmtVAT10.toString(),
            TxblAmt: tranAmt.toString(),
            Level: "1"
        });

        if (taxCat == "NONE" || taxCat == "") {
            if (taxID0 == "NONEVAT" || taxID1 == "NONEVAT" || taxID2 == "NONEVAT" || taxID3 == "NONEVAT") {
                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVATNONEGrid1Tab2);
            }
            //co the se thieu su ly sau

        } else if (taxCat == "VAT00") {
            if (taxID0 == "VAT00" || taxID1 == "VAT00" || taxID2 == "VAT00" || taxID3 == "VAT00") {
                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT00Grid1Tab2);
            }


        } else if (taxCat == "VAT05") {
            if (taxID0 == "OVAT05-00" || taxID1 == "OVAT05-00" || taxID2 == "OVAT05-00" || taxID3 == "OVAT05-00") {
                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT05Grid1Tab2);
            }

        } else if (taxCat == "VAT10") {

            if (taxID0 == "OVAT10-00" || taxID1 == "OVAT10-00" || taxID2 == "OVAT10-00" || taxID3 == "OVAT10-00") {
                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);
            }

        }



    }
}



//duyet het tat cac dong cua grid 1 tab 1 de bat ra cac du lieu bo vao grid 2 cua tab 2
//var fillDataIntoGrid1Tab2 = function () {

//    for (var i = 0; i <= App.storeGrid.getCount() - 1; i++) {
//        App.slmGridTab1.select(i);
//        //lay gia tri lineRef
//        var lineRef = "";
//        if (i < 10) {
//            lineRef = "0000" + (i + 1).toString();
//        } else if ((i + 1) >= 10 && (i + 1) < 100) {
//            lineRef = "000" + (i + 1).toString();
//        } else if ((i + 1) >= 100 && (i + 1) < 1000) {
//            lineRef = "00" + (i + 1).toString();
//        } else if ((i + 1) >= 1000 && (i + 1) < 10000) {
//            lineRef = "0" + (i + 1).toString();
//        } else if ((i + 1) >= 10000 && (i + 1) < 100000) {
//            lineRef = (i + 1).toString();
//        }
//        //lay cac gia tri can thiet khac
//        var tranAmt = App.slmGridTab1.selected.items[0].data.TranAmt;
//        var taxCat = App.slmGridTab1.selected.items[0].data.TaxCat;
//        var taxID0 = App.slmGridTab1.selected.items[0].data.TaxId00;
//        var taxID1 = App.slmGridTab1.selected.items[0].data.TaxId01;
//        var taxID2 = App.slmGridTab1.selected.items[0].data.TaxId02;
//        var taxID3 = App.slmGridTab1.selected.items[0].data.TaxId03;
//        //ham xoa cac dong co taxId01 , 02 va gop vao chung voi taxId00
//        //App.slmGridTab1.select(i);
//        //if (App.slmGridTab1.selected.items[0].data.TaxId01 != "" && App.slmGridTab1.selected.items[0].data.TaxId01 == "") {
//        //    App.slmGridTab1.selected.items[0].set('TaxId00', (taxID0 + "," + taxID1).toString());
//        //    App.slmGridTab1.selected.items[0].set('TaxId01', "");
//        //} else if (App.slmGridTab1.selected.items[0].data.TaxId01 != "" && App.slmGridTab1.selected.items[0].data.TaxId01 != "") {
//        //    App.slmGridTab1.selected.items[0].set('TaxId00', (taxID0 + "," + taxID1 + "," + taxID2).toString());
//        //    App.slmGridTab1.selected.items[0].set('TaxId01', "");
//        //    App.slmGridTab1.selected.items[0].set('TaxId02', "");
//        //}
//        //sau khi bat xong chuyen sang tab 2
//        //App.tabAtAR10100.setActiveTab(App.frmBot);
//        //App.slmGridTab1.select();
//        //App.slmGridTab1.selected.items[0].data.TaxId00;
//        App.tabAtAR10100.setActiveTab(App.tabTax);

//        var taxAmtVAT05 = tranAmt * 5 / 100;
//        var taxAmtVAT10 = tranAmt / 10;
//        var taxAmtVATNONE = 0;
//        //khai bao cac record cho tung truong hop
//        var recordIVAT05Grid1Tab2 = Ext.create("App.GetGrid1Tab2ResultModel", {

//            LineRef: lineRef,
//            TaxID: "oVat05",
//            TaxRate: "5",
//            TaxAmt: taxAmtVAT05.toString(),
//            TxblAmt: tranAmt.toString(),
//            Level: "1"
//        });

//        var recordIVAT10Grid1Tab2 = Ext.create("App.GetGrid1Tab2ResultModel", {

//            LineRef: lineRef,
//            TaxID: "Vat00",
//            TaxRate: "10",
//            TaxAmt: taxAmtVAT10.toString(),
//            TxblAmt: tranAmt.toString(),
//            Level: "1"
//        });

//        var recordVATNONEGrid1Tab2 = Ext.create("App.GetGrid1Tab2ResultModel", {
//            //CustId: "",
//            LineRef: lineRef,
//            TaxID: "NONEVAT",
//            TaxRate: "0",
//            TaxAmt: taxAmtVATNONE.toString(),
//            TxblAmt: tranAmt.toString(),
//            Level: "1"
//        });

//        var recordVAT10Grid1Tab2 = Ext.create("App.GetGrid1Tab2ResultModel", {
//            //CustId: "",
//            LineRef: lineRef,
//            TaxID: "VAT10",
//            TaxRate: "10",
//            TaxAmt: taxAmtVAT10.toString(),
//            TxblAmt: tranAmt.toString(),
//            Level: "1"
//        });

//        var recordOVAT10Grid1Tab2 = Ext.create("App.GetGrid1Tab2ResultModel", {
//            //CustId: "",
//            LineRef: lineRef,
//            TaxID: "OVAT10-00",
//            TaxRate: "10",
//            TaxAmt: taxAmtVAT10.toString(),
//            TxblAmt: tranAmt.toString(),
//            Level: "1"
//        });


//        //cac dieu kien lien quan giua taxCat va taxID
//        if (taxCat == "*") {
//            if (taxID0 == "oVat05" && taxID1 == "" && taxID2 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//            } else if (taxID0 == "Vat00" && taxID1 == "" && taxID2 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//            } else if (taxID0 == "NONEVAT" && taxID1 == "" && taxID2 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVATNONEGrid1Tab2);

//            } else if (((taxID0 == "oVat05" && taxID1 == "Vat00") || (taxID0 == "Vat00" && taxID1 == "oVat05")) && taxID2 == "") {
//                if (taxID0 == "oVat05" && taxID1 == "Vat00") {

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                } else {

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);
//                }

//            } else if (((taxID0 == "oVat05" && taxID1 == "Vat00") || (taxID0 == "Vat00" && taxID1 == "oVat05")) && taxID2 == "NONEVAT") {
//                if (taxID0 == "oVat05" && taxID1 == "Vat00") {

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVATNONEGrid1Tab2);

//                } else {

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVATNONEGrid1Tab2);
//                }

//            } else if (taxID0 == "NONEVAT" && ((taxID1 == "oVat05" && taxID2 == "Vat00") || (taxID1 == "Vat00" && taxID2 == "oVat05"))) {
//                if (taxID1 == "oVat05" && taxID2 == "Vat00") {

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVATNONEGrid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                } else {

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVATNONEGrid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);
//                }


//            } else if (taxID0 == "NONEVAT" && taxID1 == "oVat05" && taxID2 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVATNONEGrid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//            } else if (taxID0 == "NONEVAT" && taxID1 == "Vat00" && taxID2 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVATNONEGrid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//            } else if (taxID0 == "Vat00" && ((taxID1 == "oVat05" && taxID2 == "NONEVAT") || (taxID1 == "NONEVAT" && taxID2 == "oVat05"))) {
//                if (taxID1 == "oVat05" && taxID2 == "NONEVAT") {
//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVATNONEGrid1Tab2);
//                } else {

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVATNONEGrid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);
//                }

//            } else if (taxID0 == "Vat00" && taxID1 == "NONEVAT" && taxID2 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVATNONEGrid1Tab2);

//            } else if (taxID0 == "Vat00" && taxID1 == "NONEVAT" && taxID2 == "oVat05") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVATNONEGrid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//            } else if (taxID0 == "oVat05" && taxID1 == "NONEVAT" && taxID2 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVATNONEGrid1Tab2);

//            } else if (taxID0 == "oVat05" && taxID1 == "NONEVAT" && taxID2 == "Vat00") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVATNONEGrid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//            } //bat dau cho bat 4 cai taxId
//            else if ((taxID0 == "oVat05" && taxID1 == "Vat00" && taxID2 == "OVAT10-00" && taxID3 == "VAT10") ||//1
//                (taxID0 == "oVat05" && taxID1 == "Vat00" && taxID2 == "VAT10" && taxID3 == "OVAT10-00") ||//2
//                (taxID0 == "oVat05" && taxID1 == "OVAT10-00" && taxID2 == "Vat00" && taxID3 == "VAT10") ||//3
//                (taxID0 == "oVat05" && taxID1 == "OVAT10-00" && taxID2 == "VAT10" && taxID3 == "Vat00") ||//4
//                (taxID0 == "oVat05" && taxID1 == "VAT10" && taxID2 == "Vat00" && taxID3 == "OVAT10-00") ||//5
//                (taxID0 == "oVat05" && taxID1 == "VAT10" && taxID2 == "OVAT10-00" && taxID3 == "Vat00") ||//6
//                (taxID0 == "Vat00" && taxID1 == "oVat05" && taxID2 == "OVAT10-00" && taxID3 == "VAT10") ||//7
//                (taxID0 == "Vat00" && taxID1 == "oVat05" && taxID2 == "VAT10" && taxID3 == "OVAT10-00") ||//8
//                (taxID0 == "Vat00" && taxID1 == "OVAT10-00" && taxID2 == "oVat05" && taxID3 == "VAT10") ||//9
//                (taxID0 == "Vat00" && taxID1 == "OVAT10-00" && taxID2 == "VAT10" && taxID3 == "oVat05") ||//10
//                (taxID0 == "Vat00" && taxID1 == "VAT10" && taxID2 == "oVat05" && taxID3 == "OVAT10-00") ||//11
//                (taxID0 == "Vat00" && taxID1 == "VAT10" && taxID2 == "OVAT10-00" && taxID3 == "oVat05") ||//12
//                (taxID0 == "OVAT10-00" && taxID1 == "VAT10" && taxID2 == "oVat05" && taxID3 == "Vat00") ||//13
//                (taxID0 == "OVAT10-00" && taxID1 == "VAT10" && taxID2 == "Vat00" && taxID3 == "oVat05") ||//14
//                (taxID0 == "OVAT10-00" && taxID1 == "oVat05" && taxID2 == "Vat00" && taxID3 == "VAT10") ||//15
//                (taxID0 == "OVAT10-00" && taxID1 == "oVat05" && taxID2 == "VAT10" && taxID3 == "Vat00") ||//16
//                (taxID0 == "OVAT10-00" && taxID1 == "Vat00" && taxID2 == "oVat05" && taxID3 == "VAT10") ||//17
//                (taxID0 == "OVAT10-00" && taxID1 == "Vat00" && taxID2 == "VAT10" && taxID3 == "oVat05") ||//18
//                (taxID0 == "VAT10" && taxID1 == "OVAT10-00" && taxID2 == "oVat05" && taxID3 == "Vat00") ||//19
//                (taxID0 == "VAT10" && taxID1 == "OVAT10-00" && taxID2 == "Vat00" && taxID3 == "oVat05") ||//20
//                (taxID0 == "VAT10" && taxID1 == "oVat05" && taxID2 == "OVAT10-00" && taxID3 == "Vat00") ||//21
//                (taxID0 == "VAT10" && taxID1 == "oVat05" && taxID2 == "Vat00" && taxID3 == "OVAT10-00") ||//22
//                (taxID0 == "VAT10" && taxID1 == "Vat00" && taxID2 == "oVat05" && taxID3 == "OVAT10-00") ||//23
//                (taxID0 == "VAT10" && taxID1 == "Vat00" && taxID2 == "OVAT10-00" && taxID3 == "oVat05")) {//24

//                if (taxID0 == "oVat05" && taxID1 == "Vat00" && taxID2 == "OVAT10-00" && taxID3 == "VAT10") {//1

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);


//                }else if(taxID0 == "oVat05" && taxID1 == "Vat00" && taxID2 == "VAT10" && taxID3 == "OVAT10-00"){

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                }else if(taxID0 == "oVat05" && taxID1 == "OVAT10-00" && taxID2 == "Vat00" && taxID3 == "VAT10"){

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                }else if(taxID0 == "oVat05" && taxID1 == "OVAT10-00" && taxID2 == "VAT10" && taxID3 == "Vat00"){

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                }else if(taxID0 == "oVat05" && taxID1 == "VAT10" && taxID2 == "Vat00" && taxID3 == "OVAT10-00"){ //5

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                } else if (taxID0 == "oVat05" && taxID1 == "VAT10" && taxID2 == "OVAT10-00" && taxID3 == "Vat00") {

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                } else if (taxID0 == "Vat00" && taxID1 == "oVat05" && taxID2 == "OVAT10-00" && taxID3 == "VAT10") {

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                } else if (taxID0 == "Vat00" && taxID1 == "oVat05" && taxID2 == "VAT10" && taxID3 == "OVAT10-00") {

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                } else if (taxID0 == "Vat00" && taxID1 == "OVAT10-00" && taxID2 == "oVat05" && taxID3 == "VAT10") {

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                } else if (taxID0 == "Vat00" && taxID1 == "OVAT10-00" && taxID2 == "VAT10" && taxID3 == "oVat05") {//10

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                } else if (taxID0 == "Vat00" && taxID1 == "VAT10" && taxID2 == "oVat05" && taxID3 == "OVAT10-00") {

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                } else if (taxID0 == "Vat00" && taxID1 == "VAT10" && taxID2 == "OVAT10-00" && taxID3 == "oVat05") {

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                } else if (taxID0 == "OVAT10-00" && taxID1 == "VAT10" && taxID2 == "oVat05" && taxID3 == "Vat00") {

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                } else if (taxID0 == "OVAT10-00" && taxID1 == "oVat05" && taxID2 == "Vat00" && taxID3 == "VAT10") {//15

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                } else if (taxID0 == "OVAT10-00" && taxID1 == "oVat05" && taxID2 == "VAT10" && taxID3 == "Vat00") {

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                } else if (taxID0 == "OVAT10-00" && taxID1 == "Vat00" && taxID2 == "oVat05" && taxID3 == "VAT10") {

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                } else if (taxID0 == "OVAT10-00" && taxID1 == "Vat00" && taxID2 == "VAT10" && taxID3 == "oVat05") {

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                } else if (taxID0 == "VAT10" && taxID1 == "OVAT10-00" && taxID2 == "oVat05" && taxID3 == "Vat00") {

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                } else if (taxID0 == "VAT10" && taxID1 == "OVAT10-00" && taxID2 == "Vat00" && taxID3 == "oVat05") {//20

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                } else if (taxID0 == "VAT10" && taxID1 == "oVat05" && taxID2 == "OVAT10-00" && taxID3 == "Vat00") {

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                } else if (taxID0 == "VAT10" && taxID1 == "oVat05" && taxID2 == "Vat00" && taxID3 == "OVAT10-00") {

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                } else if (taxID0 == "VAT10" && taxID1 == "Vat00" && taxID2 == "oVat05" && taxID3 == "OVAT10-00") {

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                } else if (taxID0 == "VAT10" && taxID1 == "Vat00" && taxID2 == "OVAT10-00" && taxID3 == "oVat05") {//24

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                    App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);
//                }

//            } else if (taxID0 == "oVat05" && taxID1 == "Vat00" && taxID2 == "OVAT10-00" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//            } else if (taxID0 == "oVat05" && taxID1 == "Vat00" && taxID2 == "OVAT10-00" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//            } else if (taxID0 == "oVat05" && taxID1 == "Vat00" && taxID2 == "VAT10" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//            } else if (taxID0 == "oVat05" && taxID1 == "Vat00" && taxID2 == "" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//            } else if (taxID0 == "oVat05" && taxID1 == "OVAT10-00" && taxID2 == "Vat00" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//            } else if (taxID0 == "oVat05" && taxID1 == "OVAT10-00" && taxID2 == "VAT10" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//            } else if (taxID0 == "oVat05" && taxID1 == "OVAT10-00" && taxID2 == "" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//            } else if (taxID0 == "oVat05" && taxID1 == "VAT10" && taxID2 == "Vat00" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//            } else if (taxID0 == "oVat05" && taxID1 == "VAT10" && taxID2 == "OVAT10-00" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//            } else if (taxID0 == "oVat05" && taxID1 == "VAT10" && taxID2 == "" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//            } else if (taxID0 == "oVat05" && taxID1 == "" && taxID2 == "" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//            } else if (taxID0 == "Vat00" && taxID1 == "oVat05" && taxID2 == "OVAT10-00" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//            } else if (taxID0 == "Vat00" && taxID1 == "oVat05" && taxID2 == "VAT10" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//            } else if (taxID0 == "Vat00" && taxID1 == "oVat05" && taxID2 == "" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//            } else if (taxID0 == "Vat00" && taxID1 == "OVAT10-00" && taxID2 == "oVat05" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//            } else if (taxID0 == "Vat00" && taxID1 == "OVAT10-00" && taxID2 == "VAT10" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//            } else if (taxID0 == "Vat00" && taxID1 == "OVAT10-00" && taxID2 == "" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//            } else if (taxID0 == "Vat00" && taxID1 == "VAT10" && taxID2 == "oVat05" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//            } else if (taxID0 == "Vat00" && taxID1 == "VAT10" && taxID2 == "OVAT10-00" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//            } else if (taxID0 == "Vat00" && taxID1 == "VAT10" && taxID2 == "" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//            } else if (taxID0 == "Vat00" && taxID1 == "" && taxID2 == "" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//            } else if (taxID0 == "OVAT10-00" && taxID1 == "VAT10" && taxID2 == "oVat05" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//            } else if (taxID0 == "OVAT10-00" && taxID1 == "VAT10" && taxID2 == "Vat00" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//            } else if (taxID0 == "OVAT10-00" && taxID1 == "VAT10" && taxID2 == "" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//            } else if (taxID0 == "OVAT10-00" && taxID1 == "oVat05" && taxID2 == "Vat00" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//            } else if (taxID0 == "OVAT10-00" && taxID1 == "oVat05" && taxID2 == "VAT10" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//            } else if (taxID0 == "OVAT10-00" && taxID1 == "oVat05" && taxID2 == "" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//            } else if (taxID0 == "OVAT10-00" && taxID1 == "Vat00" && taxID2 == "oVat05" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//            } else if (taxID0 == "OVAT10-00" && taxID1 == "Vat00" && taxID2 == "VAT10" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//            } else if (taxID0 == "OVAT10-00" && taxID1 == "Vat00" && taxID2 == "" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//            } else if (taxID0 == "OVAT10-00" && taxID1 == "" && taxID2 == "" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//            } else if (taxID0 == "VAT10" && taxID1 == "OVAT10-00" && taxID2 == "oVat05" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//            } else if (taxID0 == "VAT10" && taxID1 == "OVAT10-00" && taxID2 == "Vat00" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//            } else if (taxID0 == "VAT10" && taxID1 == "OVAT10-00" && taxID2 == "" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//            } else if (taxID0 == "VAT10" && taxID1 == "oVat05" && taxID2 == "OVAT10-00" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//            } else if (taxID0 == "VAT10" && taxID1 == "oVat05" && taxID2 == "Vat00" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//            } else if (taxID0 == "VAT10" && taxID1 == "oVat05" && taxID2 == "" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//            } else if (taxID0 == "VAT10" && taxID1 == "Vat00" && taxID2 == "oVat05" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);

//            } else if (taxID0 == "VAT10" && taxID1 == "Vat00" && taxID2 == "OVAT10-00" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);

//            } else if (taxID0 == "VAT10" && taxID1 == "Vat00" && taxID2 == "" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);

//            } else if (taxID0 == "VAT10" && taxID1 == "" && taxID2 == "" && taxID3 == "") {

//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);

//            }



//            //ngoac ket thuc taxCat == *
//        } else if (taxCat == "NONE" || taxCat == "") {
//            if (taxID0 == "NONEVAT" || taxID1 == "NONEVAT" || taxID2 == "NONEVAT") {
//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVATNONEGrid1Tab2);
//            }
//            //co the se thieu su ly sau

//        } else if (taxCat == "VAT00") {
//            //neu co gi xu ly sau


//        } else if (taxCat == "VAT05") {
//            if (taxID0 == "oVat05" || taxID1 == "oVat05" || taxID2 == "oVat05") {
//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT05Grid1Tab2);
//            }

//        } else if (taxCat == "VAT10") {
//            if (taxID0 == "Vat00" || taxID1 == "Vat00" || taxID2 == "Vat00" || taxID3 == "Vat00") {
//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordIVAT10Grid1Tab2);
//            }
//            if (taxID0 == "OVAT10-00" || taxID1 == "OVAT10-00" || taxID2 == "OVAT10-00" || taxID3 == "OVAT10-00") {
//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordOVAT10Grid1Tab2);
//            }
//            if (taxID0 == "VAT10" || taxID1 == "VAT10" || taxID2 == "VAT10" || taxID3 == "VAT10") {
//                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordVAT10Grid1Tab2);
//            }
//        }

//        //ngoac ket thuc vong for de duyet grid 1 tab 1 bo du lieu vao grid 2 tab 2
//    }

//}
















































//var cboTaxID_Change = function (item) {
//    //xoa value 2 cai taxid01 voi taxid02 truoc
//    App.slmGridTab1.selected.items[0].set('TaxId01', "");
//    App.slmGridTab1.selected.items[0].set('TaxId02', "");
//    //xet dieu kien de them vao lai taxid01 voi taxid02
//    if (item.rawValue.search(", oVat05, Vat00") != -1) {
//        App.slmGridTab1.selected.items[0].set('TaxId00', item.value[0]);
//        App.slmGridTab1.selected.items[0].set('TaxId01', "oVat05");
//        App.slmGridTab1.selected.items[0].set('TaxId02', "Vat00");
//    } else if (item.rawValue.search(", oVat05, NONEVAT") != -1) {
//        App.slmGridTab1.selected.items[0].set('TaxId00', item.value[0]);
//        App.slmGridTab1.selected.items[0].set('TaxId01', "oVat05");
//        App.slmGridTab1.selected.items[0].set('TaxId02', "NONEVAT");
//    } else if (item.rawValue.search(", Vat00, oVat05") != -1) {
//        App.slmGridTab1.selected.items[0].set('TaxId00', item.value[0]);
//        App.slmGridTab1.selected.items[0].set('TaxId01', "Vat00");
//        App.slmGridTab1.selected.items[0].set('TaxId02', "oVat05");
//    } else if (item.rawValue.search(", Vat00, NONEVAT") != -1) {
//        App.slmGridTab1.selected.items[0].set('TaxId00', item.value[0]);
//        App.slmGridTab1.selected.items[0].set('TaxId01', "Vat00");
//        App.slmGridTab1.selected.items[0].set('TaxId02', "NONEVAT");
//    } else if (item.rawValue.search(", NONEVAT, oVat05") != -1) {
//        App.slmGridTab1.selected.items[0].set('TaxId00', item.value[0]);
//        App.slmGridTab1.selected.items[0].set('TaxId01', "NONEVAT");
//        App.slmGridTab1.selected.items[0].set('TaxId02', "oVat05");
//    } else if (item.rawValue.search(", NONEVAT, Vat00") != -1) {
//        App.slmGridTab1.selected.items[0].set('TaxId00', item.value[0]);
//        App.slmGridTab1.selected.items[0].set('TaxId01', "NONEVAT");
//        App.slmGridTab1.selected.items[0].set('TaxId02', "Vat00");
//    } else if (item.rawValue.search(", oVat05") != -1) {
//        App.slmGridTab1.selected.items[0].set('TaxId00', item.value[0]);
//        App.slmGridTab1.selected.items[0].set('TaxId01', "oVat05");
//    } else if (item.rawValue.search(", Vat00") != -1) {
//        App.slmGridTab1.selected.items[0].set('TaxId00', item.value[0]);
//        App.slmGridTab1.selected.items[0].set('TaxId01', "Vat00");
//    } else if (item.rawValue.search(", NONEVAT") != -1) {
//        App.slmGridTab1.selected.items[0].set('TaxId00', item.value[0]);
//        App.slmGridTab1.selected.items[0].set('TaxId01', "NONEVAT");
//    } else if (item.rawValue == "oVat05" || item.rawValue == "Vat00" || item.rawValue == "NONEVAT") {
//        App.slmGridTab1.selected.items[0].set('TaxId00', item.value[0]);
//    }
//    var index = App.slmGridTab1.selected.items[0].index;
//    //tai lai du lieu cua grid 1 va 2 cua tab 2 do moi cap nhap o grid 1 tab 1
//    reloadDataGrid1AndGrid2Tab2(index);
//    //reload lai tong tien , so tien no goc , so du chung tu (TotAmt, OrigDocAmt , DocBal)
//    reloadAmountMustPayTotal(index);
//    var taxid00 = App.slmGridTab1.selected.items[0].data.TaxId00;
//    //if(taxid00.search(","){
//    //    var taxid00change = taxid00.substring(0, r.data.TaxId00.search(","));
//    //    r.set('TaxId00', taxid00change);
//    //}

//}
