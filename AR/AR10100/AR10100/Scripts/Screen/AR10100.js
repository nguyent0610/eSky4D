
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

var tmpFormChangeTopBot = false;
var keys = ['InvtId'];

var fieldsCheckRequire = ["InvtId"];
var fieldsLangCheckRequire = ["InvtId"];
var isNewRef = false;

var tmpTrantAmtWhenChange = 0;
var tmpRowSelectedEdit = 0;

var menuClick = function (command) {
    switch (command) {
        case "first":
        
            if (HQ.focus == 'headerTop') {
                HQ.combo.first(App.cboBatNbr, HQ.isChange);
            } else if (HQ.focus == 'headerBot') {
                HQ.grid.first(App.cboRefNbr, HQ.isChange);
            } else if (HQ.focus == 'Grid') {
                HQ.grid.first(App.grd);
            }
            break;
        case "prev":
 
            if (HQ.focus == 'headerTop') {
                HQ.combo.prev(App.cboBatNbr, HQ.isChange);
            } else if (HQ.focus == 'headerBot') {
                HQ.grid.prev(App.cboRefNbr, HQ.isChange);
            } else if (HQ.focus == 'Grid') {
                HQ.grid.prev(App.grd);
            }
            break;
        case "next":

            if (HQ.focus == 'headerTop') {
                HQ.combo.next(App.cboBatNbr, HQ.isChange);
            } else if (HQ.focus == 'headerBot') {
                HQ.grid.next(App.cboRefNbr, HQ.isChange);
            } else if (HQ.focus == 'Grid') {
                HQ.grid.next(App.grd);
            }
            break;
        case "last":

            if (HQ.focus == 'headerTop') {
                HQ.combo.last(App.cboBatNbr, HQ.isChange);
            } else if (HQ.focus == 'headerBot') {
                HQ.grid.last(App.cboRefNbr, HQ.isChange);
            } else if (HQ.focus == 'Grid') {
                HQ.grid.last(App.grd);
            }


            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            } else {
                HQ.isChange = false;
            }
            break;
        case "new":
            if (HQ.isInsert) {

                if (HQ.focus == 'headerTop') {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', '');
                    }
                    else {
                        App.cboBatNbr.setValue('');
                        App.storeFormTop.load();
                    }
                } else if (HQ.focus == 'headerBot') {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', '');
                    }
                    else {
                        App.cboRefNbr.setValue('');
                        App.storeFormBot.load();
                    }
                } else if (HQ.focus == 'Grid') {
                    //HQ.grid.insert(App.grd, keys);
                    
                    insertNewRecordGrid1();
                    App.slmGridTab1.select(App.storeGrid.getCount() - 1);
                    App.grd.editingPlugin.startEditByPosition({ row: App.storeGrid.getCount() - 1, column: 2 });

                  
                    
                }

            }
            break;
        case "delete":

            if (HQ.isDelete) {
                if (App.cboBatNbr.value != "" && App.cboStatus.value != "C" && App.cboStatus.value != "V") {
                    if (HQ.focus == 'headerTop') {
                            HQ.message.show(11, '', 'deleteRecordFormTopBatch');
                    } else if (HQ.focus == 'headerBot') {
                        if (App.cboRefNbr.getValue()) {
                              HQ.message.show(11, '', 'deleteRecordFormBotAR_Doc');
                        }
                    } else if (HQ.focus == 'Grid') {
                        var rowindex = HQ.grid.indexSelect(App.grd);
                        if (rowindex != '') {
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grd), ''], 'deleteRecordGrid', true)
                        }
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmTop) &&
                    HQ.form.checkRequirePass(App.frmBot) &&
                    HQ.store.checkRequirePass(App.storeGrid, keys, fieldsCheckRequire, fieldsLangCheckRequire)
                    ){

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

var WaitAddNewGridWaitToSetLineRef = function () {
    //var lineRef = HQ.store.lastLineRef(App.storeGrid);
    //App.storeGrid.data.items[App.storeGrid.getCount() - 1].set('LineRef', lineRef);
    //App.storeGrid.data.items[App.storeGrid.getCount() - 1].set('LineType', 'N');
    //App.slmGridTab1.select(App.storeGrid.getCount() - 1);
    //App.grd.editingPlugin.startEditByPosition({ row: App.storeGrid.getCount() - 1, column: 2 });
};

function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        if (App.cboBatNbr.valueModels == null) App.cboBatNbr.setValue('');
        App.cboBatNbr.getStore().load();
        App.storeFormTop.reload();
        App.storeFormBot.load();
        App.storeGrid.load();


        
    }
};



var setStatusValueH = function () {
    App.cboStatus.setValue("H");
    App.cboHandle.getStore().load(function (records, operation, success) {
        if (records.length) {
            if (App.cboStatus.value == "H") {
                //setReadOnly();
                App.cboHandle.setValue("N");
            } else {
                //setReadOnly();
                App.cboHandle.setValue("N");
            }
        }
    });
}
////////Save////////////////
////////////////////////////
function Save() {

    //duyet vong for de lay cac gia tri con thieu cho Grid

    

    //for (var i = 0; i <= App.storeGrid.getCount() - 1; i++) {

    //    //duyet grid top tab 2 de lay ra cac gia tri co cung LineRef de lay TaxAmt va txblAmt  
    //    for (var k = 0; k <= App.storeGridTopTab2.getCount() - 1; k++) {
    //        App.slmGridTopTab2.select(k);
    //        if (App.slmGridTopTab2.selected.items[0].data.LineRef == App.storeGrid.data.items[i].data.LineRef) {
    //            //bien count de dem xem co bao nhieu cai cung LineRef de save vao cac bien TaxAmt 0 > 3 va bien txblAmt 0 > 3 cho dung 
    //            countTaxAmt++;
    //            if (countTaxAmt == 1) {
    //                //taxAmt[0] = parseFloat(App.slmGridTopTab2.selected.items[0].data.TaxAmt);
    //                //txblAmt = parseFloat(App.slmGridTopTab2.selected.items[0].data.TxblAmt);
    //                App.storeGrid.data.items[i].set('TaxAmt00', parseFloat(App.slmGridTopTab2.selected.items[0].data.TaxAmt));
    //                App.storeGrid.data.items[i].set('TaxAmt01', 0);
    //                App.storeGrid.data.items[i].set('TaxAmt02', 0);
    //                App.storeGrid.data.items[i].set('TaxAmt03', 0);
    //                App.storeGrid.data.items[i].set('TxblAmt00', parseFloat(App.slmGridTopTab2.selected.items[0].data.TxblAmt));
    //            } else if (countTaxAmt == 2) {
    //                App.storeGrid.data.items[i].set('TaxAmt01', parseFloat(App.slmGridTopTab2.selected.items[0].data.TaxAmt));
    //                App.storeGrid.data.items[i].set('TxblAmt01', parseFloat(App.slmGridTopTab2.selected.items[0].data.TxblAmt));
    //            } else if (countTaxAmt == 3) {
    //                App.storeGrid.data.items[i].set('TaxAmt02', parseFloat(App.slmGridTopTab2.selected.items[0].data.TaxAmt));
    //                App.storeGrid.data.items[i].set('TxblAmt02', parseFloat(App.slmGridTopTab2.selected.items[0].data.TxblAmt));
    //            } else if (countTaxAmt == 4) {
    //                App.storeGrid.data.items[i].set('TaxAmt03', parseFloat(App.slmGridTopTab2.selected.items[0].data.TaxAmt));
    //                App.storeGrid.data.items[i].set('TxblAmt03', parseFloat(App.slmGridTopTab2.selected.items[0].data.TxblAmt));
    //            }
    //        }

    //    }
    //    countTaxAmt = 0;


    //}

    App.frmTop.getForm().updateRecord();
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
                isNew: HQ.isNew,
                isNewRef: isNewRef,
            },
            success: function (result, data) {
                if (App.cboHandle.getValue() == "V" || App.cboHandle.getValue() == "C" || App.cboHandle.getValue() == "R") {
                    HQ.message.process(errorMsg, data, true);
                }
                else HQ.message.show(201405071, '', null);
                App.cboBatNbr.getStore().load();
                App.cboRefNbr.getStore().load();

                if (App.cboBatNbr.getValue() == "" || App.cboBatNbr.getValue() == null) { // tao moi BatNbr , RefNbr va Grid
                    App.cboBatNbr.getStore().load(function () {
                        App.cboBatNbr.setValue(data.result.tmpBatNbr);

                    });
                } else { // tao moi RefNbr, co the ca Grid
                    if (App.cboRefNbr.getValue() == "" || App.cboRefNbr.getValue() == null) { // neu tao moi ca RefNbr va Grid
                        App.cboRefNbr.getStore().load(function () {
                            App.cboRefNbr.setValue(data.result.tmpRefNbr);

                        });
                    } else { // neu chi tao moi Grid
                        App.storeGrid.load();
                    }

                }
                //refresh('yes');
                setTimeout(function () { refresh('yes'); }, 2000);

            }
            , failure: function (errorMsg, data) {
                if (App.cboHandle.getValue() == "V" || App.cboHandle.getValue() == "C" || App.cboHandle.getValue() == "R") {
                    HQ.message.process(errorMsg, data, true);
                }
                else {
                    var dt = Ext.decode(data.response.responseText);
                    HQ.message.show(dt.code, dt.colName + ',' + dt.value, null);
                }
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

///////////////Delete////////////////////////////
/////////////////////////////////////////////////
//xac nhan xoa Record form Top Batch
var deleteRecordFormTopBatch = function (item) {
    if (item == 'yes') {
        App.frmMain.submit({
            clientValidation: false,
            timeout: 1800000,
            waitMsg: HQ.common.getLang('DeletingData'),
            url: 'AR10100/DeleteFormTopBatch',
            params: {
                batNbr: App.cboBatNbr.getValue(),
                branchID: App.txtBranchID.getValue(),
            },
            success: function (action, data) {
                App.cboBatNbr.setValue("");
                App.cboBatNbr.getStore().load(function () { cboBatNbr_Change(App.cboBatNbr); });

            },

            failure: function (action, data) {
                if (data.result.msgCode) {
                    HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                }
            }
        });
    }
};
//xac nhan xoa Record form BotAR_Doc
var deleteRecordFormBotAR_Doc = function (item) {
    if (item == 'yes') {
        App.frmMain.submit({
            clientValidation: false,
            timeout: 1800000,
            waitMsg: HQ.common.getLang('DeletingData'),
            url: 'AR10100/DeleteFormBotAP_Doc',
            params: {
                refNbr: App.cboRefNbr.getValue(),
                batNbr: App.cboBatNbr.getValue(),
                branchID: App.txtBranchID.getValue(),
            },
            success: function (action, data) {
                App.cboRefNbr.setValue("");
                App.cboRefNbr.getStore().load(function () { cboRefNbr_Change(App.cboRefNbr); });

            },

            failure: function (action, data) {
                if (data.result.msgCode) {
                    HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                }
            }
        });
    }
};
// Xac nhan xoa record cua Grid
var deleteRecordGrid = function (item) {
    if (item == "yes") {
        //var index = App.slmGridTab1.selected.items[0].index;
        App.grd.deleteSelected();
        //tai lai du lieu cua grid 1 va 2 cua tab 2 do moi cap nhap o grid 1 tab 1
        reloadDataGrid1AndGrid2Tab2();
        //reload lai tong tien , so tien no goc , so du chung tu (TotAmt, OrigDocAmt , DocBal)
        reloadAmountMustPayTotal();
        frmChange();
    }
};







///////////Kiem tra combo chinh BatNbr//////////////////////
//khi co su thay doi du lieu cua cac conttol tren form
//load lần đầu khi mở///////////////////////////////////////////
/////////////////////////////////////////////////////////////////
var firstLoad = function () {
    App.cboBatNbr.getStore().reload();
    App.cboRefNbr.getStore().reload();
    App.storeFormTop.reload();
    App.storeFormBot.load();

};
var frmChange = function () {
    if (HQ.focus == 'headerTop' || HQ.focus == 'Grid') {
        //đề phòng trường hợp nếu store chưa có gì cả giao diện chưa load xong mà App.frmMain.getForm().updateRecord(); được gọi sẽ gây lỗi
        if (App.storeFormTop.data.length > 0) {
            App.frmTop.getForm().updateRecord();

            HQ.isChange = HQ.store.isChange(App.storeFormTop);
            HQ.common.changeData(HQ.isChange, 'AR10100');//co thay doi du lieu gan * tren tab title header
            //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
            if (App.cboBatNbr.valueModels == null || HQ.isNew == true) {//App.cboCustId.valueModels == null khi ko co select item nao
                App.cboBatNbr.setReadOnly(false);
            }

            else {
                App.cboBatNbr.setReadOnly(HQ.isChange);
                App.cboRefNbr.setReadOnly(HQ.isChange);
                tmpFormChangeTopBot = HQ.isChange;
            }

        }

        if (App.storeFormBot.data.length > 0) {
            App.frmBot.getForm().updateRecord();

            HQ.isChange = HQ.store.isChange(App.storeFormBot);
            HQ.common.changeData(HQ.isChange, 'AR10100');//co thay doi du lieu gan * tren tab title header
            //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
            if (App.cboRefNbr.valueModels == null || HQ.isNew == true) {//App.cboCustId.valueModels == null khi ko co select item nao
                App.cboRefNbr.setReadOnly(false);
            }

            else {
                if (tmpFormChangeTopBot == true) {
                    App.cboBatNbr.setReadOnly(tmpFormChangeTopBot);
                    App.cboRefNbr.setReadOnly(tmpFormChangeTopBot);
                    HQ.isChange = tmpFormChangeTopBot;
                } else {
                    App.cboBatNbr.setReadOnly(HQ.isChange);
                    App.cboRefNbr.setReadOnly(HQ.isChange);
                }
            }

        }
    } else if (HQ.focus == 'headerBot') {
        if (App.storeFormBot.data.length > 0) {
            App.frmBot.getForm().updateRecord();

            HQ.isChange = HQ.store.isChange(App.storeFormBot);
            HQ.common.changeData(HQ.isChange, 'AR10100');//co thay doi du lieu gan * tren tab title header
            //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
            if (App.cboRefNbr.valueModels == null || HQ.isNew == true) {//App.cboCustId.valueModels == null khi ko co select item nao
                App.cboRefNbr.setReadOnly(false);
            }

            else {
               
                App.cboBatNbr.setReadOnly(HQ.isChange);
                App.cboRefNbr.setReadOnly(HQ.isChange);
                tmpFormChangeTopBot = HQ.isChange;
            }

        }

        if (App.storeFormTop.data.length > 0) {
            App.frmTop.getForm().updateRecord();

            HQ.isChange = HQ.store.isChange(App.storeFormTop);
            HQ.common.changeData(HQ.isChange, 'AR10100');//co thay doi du lieu gan * tren tab title header
            //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
            if (App.cboBatNbr.valueModels == null || HQ.isNew == true) {//App.cboCustId.valueModels == null khi ko co select item nao
                App.cboBatNbr.setReadOnly(false);
            }

            else {
                if (tmpFormChangeTopBot == true) {
                    App.cboBatNbr.setReadOnly(tmpFormChangeTopBot);
                    App.cboRefNbr.setReadOnly(tmpFormChangeTopBot);
                    HQ.isChange = tmpFormChangeTopBot;
                } else {
                    App.cboBatNbr.setReadOnly(HQ.isChange);
                    App.cboRefNbr.setReadOnly(HQ.isChange);
                }
            }

        }
    }



};
//////////////Form Top///////////////////////////
/////////////////////////////////////////////////
var storeBeforeLoadTop = function (store) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'), App.frmTop);
};

var storeLoadTop = function (store) {


    setFocusAllCombo();
    HQ.isFirstLoad = true;
    HQ.common.showBusy(false, '', App.frmTop);
    HQ.isNew = false;

    App.cboCustId.setReadOnly(true);
    App.cboDocType.setReadOnly(true);
    if (store.data.length == 0) {
   
        HQ.store.insertBlank(store, "BatNbr");
        record = store.getAt(0);

        record.data.Status = 'H';
        record.data.BranchID = App.txtBranchIDHide.getValue();
        record.data.JrnlType = 'AR';
        record.data.EditScrnNbr = 'AR10100';
        record.data.Module = 'AR';
        record.data.DateEnt = HQ.businessDate;

        store.commitChanges();//commit cho record thanh updated muc dich de dung ham HQ.store.isChange
        HQ.isNew = true;//record la new
        HQ.common.setRequire(App.frmMain);  //to do cac o la require
        App.cboCustId.setReadOnly(false);
        App.cboDocType.setReadOnly(false);

    }
    var record = store.getAt(0);
    App.frmTop.getForm().loadRecord(record);
 
    App.cboRefNbr.getStore().reload();
    setTimeout(function () { waitcboRefNbrReLoadToLoadFirstValue(); }, 1500);
    frmChange();



};

var waitcboRefNbrReLoadToLoadFirstValue = function () {
    if (App.cboRefNbr.getStore().data.items[0] == undefined) {
        App.cboRefNbr.setValue("");
    } else {
        App.cboRefNbr.setValue(App.cboRefNbr.getStore().data.items[0].data.RefNbr);
    }

}

var cboBatNbr_Change = function (sender, value) {

    if ((!HQ.isNew || sender.valueModels != null) && !App.storeFormTop.loading) {
        App.cboHandle.setValue(App.cboHandle.store.data.items[0].data.Code);
        App.storeFormTop.reload();

    }
}

var cboBatNbr_Select = function (sender, value) {
    if (sender.valueModels != null && !App.storeFormTop.loading) {
        App.cboHandle.setValue(App.cboHandle.store.data.items[0].data.Code);
        App.storeFormTop.reload();
    }

};

//khi nhan combo xo ra, neu da thay doi thi ko xo ra
var cboBatNbr_Expand = function (sender, value) {
    if (App.cboBatNbr.getStore().data.length == 0) {
        App.cboBatNbr.getStore().load();
    }
    if (HQ.isChange && App.cboBatNbr.getValue()) {
        App.cboBatNbr.collapse();

    }
};

//////////////Form Bot///////////////////////////
/////////////////////////////////////////////////
var storeBeforeLoadBot = function (store) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'), App.frmBot);
};
var storeLoadBot = function (store) {


    setFocusAllCombo();
    HQ.isFirstLoad = true;
    HQ.common.showBusy(false, '', App.frmBot);
    isNewRef = false;
    App.cboCustId.setReadOnly(true);
    App.cboDocType.setReadOnly(true);
    if (store.data.length == 0) {

        HQ.store.insertBlank(store, "RefNbr");
        record = store.getAt(0);


        record.data.DocDate = HQ.businessDate;
        record.data.InvcDate = HQ.businessDate;
        record.data.DiscDate = HQ.businessDate;
        record.data.DocType = "IN";
        record.data.OrigDocAmt = 0;
        record.data.DocBalL = 0;
        record.data.OrigDocAmt = 0;
   

        store.commitChanges();//commit cho record thanh updated muc dich de dung ham HQ.store.isChange
        isNewRef = true;//record la new
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboCustId.setReadOnly(false);
        App.cboDocType.setReadOnly(false);



    }
    var record = store.getAt(0);
    App.frmBot.getForm().loadRecord(record);

    frmChange();
};

var cboRefNbr_Change = function () {

    App.storeFormBot.load(function (records, operation, success) {
        
            StoreFormBotReLoad();
        
    });


}

var cboRefNbr_Select = function (sender, value) {
    if (sender.valueModels != null && !App.storeFormBot.loading) {

        App.storeFormBot.load(function (records, operation, success) {
       
                StoreFormBotReLoad();
            
        });
    }

};

//khi nhan combo xo ra, neu da thay doi thi ko xo ra
var cboRefNbr_Expand = function (sender, value) {
    if (App.cboRefNbr.getStore().data.length == 0) {
        App.cboRefNbr.getStore().reload();
    }
    if (HQ.isChange && App.cboRefNbr.getValue()) {
  
        App.cboRefNbr.collapse();
    }
};
/////////Grid////////////////////////////////
//////////////////////////////////////////////////
var loadDataGrid = function (store) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            if (App.slmGridTab1.selected.items[0] != undefined) {
                insertNewRecordGrid1();
                HQ.store.lastLineRef(App.storeGrid);
            }
        }
        HQ.isFirstLoad = false;
    }
    //khi refresh thi update lai cho Grid1 va Grid2 cua Tab2
    if (App.slmGridTab1.selected.items[0] != undefined) {
        //var index = App.slmGridTab1.selected.items[0].index;

        //tai lai du lieu cua grid 1 va 2 cua tab 2 do moi cap nhap o grid 1 tab 1
        reloadDataGrid1AndGrid2Tab2();

        //reload lai tong tien , so tien no goc , so du chung tu (TotAmt, OrigDocAmt , DocBal)
        reloadAmountMustPayTotal();
    }
    frmChange();

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
    //if (keys.indexOf(e.field) != -1) {
    //    if (e.record.data.tstamp != "") {
    //        return false;
    //    }
    //}
    //return HQ.grid.checkInput(e, keys);

    if (App.cboStatus.value != "H") {
        return false;
    }
};
var grd_Edit = function (item, e) {

    if (keys.indexOf(e.field) != -1) {
        if (e.value != '' && HQ.store.checkRequirePass(App.storeGrid, keys, fieldsCheckRequire, fieldsLangCheckRequire) && App.storeGrid.data.items[App.storeGrid.getCount() - 1].data.InvtID != "") {
            //App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());//Ext.data.Record() 
            //App.slmGridTab1.selected.items[0].set('TranDesc', vendIDAndDescr);
            insertNewRecordGrid1();
        }
    }

    if (e.field == 'Qty') {
        var quantity = e.value;
        var unitprice = e.record.data.UnitPrice;
        e.record.set('TranAmt', quantity * unitprice);
        //tai lai du lieu cua grid 1 va 2 cua tab 2 do moi cap nhap o grid 1 tab 1
        reloadDataGrid1AndGrid2Tab2();
        //reload lai tong tien , so tien no goc , so du chung tu (TotAmt, OrigDocAmt , DocBal)
        reloadAmountMustPayTotal();
    } else if (e.field == 'UnitPrice') {
        var unitprice = e.value;
        var quantity = e.record.data.Qty;
        e.record.set('TranAmt', quantity * unitprice);
        //tai lai du lieu cua grid 1 va 2 cua tab 2 do moi cap nhap o grid 1 tab 1
        reloadDataGrid1AndGrid2Tab2();
        //reload lai tong tien , so tien no goc , so du chung tu (TotAmt, OrigDocAmt , DocBal)
        reloadAmountMustPayTotal();
    } else if (e.field == 'TranAmt') {
        var tranAmt = e.value;
        var quantity = e.record.data.Qty;
        if (quantity != 0) {
            e.record.set('UnitPrice', tranAmt / quantity);
        }
        //tai lai du lieu cua grid 1 va 2 cua tab 2 do moi cap nhap o grid 1 tab 1
        reloadDataGrid1AndGrid2Tab2();
        //reload lai tong tien , so tien no goc , so du chung tu (TotAmt, OrigDocAmt , DocBal)
        reloadAmountMustPayTotal();
    } else if (e.field == 'TaxCat') {
        //tai lai du lieu cua grid 1 va 2 cua tab 2 do moi cap nhap o grid 1 tab 1
        reloadDataGrid1AndGrid2Tab2();
        //reload lai tong tien , so tien no goc , so du chung tu (TotAmt, OrigDocAmt , DocBal)
        reloadAmountMustPayTotal();
    } else if (e.field == "TaxID") { // TaxID
        if (e.value.length == 4) {
            App.storeGrid.data.items[tmpRowSelectedEdit].set('TaxId00', e.value[0])
            App.storeGrid.data.items[tmpRowSelectedEdit].set('TaxId01', e.value[1])
            App.storeGrid.data.items[tmpRowSelectedEdit].set('TaxId02', e.value[2])
            App.storeGrid.data.items[tmpRowSelectedEdit].set('TaxId03', e.value[3])

        } else if (e.value.length == 3) {
            App.storeGrid.data.items[tmpRowSelectedEdit].set('TaxId00', e.value[0])
            App.storeGrid.data.items[tmpRowSelectedEdit].set('TaxId01', e.value[1])
            App.storeGrid.data.items[tmpRowSelectedEdit].set('TaxId02', e.value[2])
            App.storeGrid.data.items[tmpRowSelectedEdit].set('TaxId03', '')
        } else if (e.value.length == 2) {
            App.storeGrid.data.items[tmpRowSelectedEdit].set('TaxId00', e.value[0])
            App.storeGrid.data.items[tmpRowSelectedEdit].set('TaxId01', e.value[1])
            App.storeGrid.data.items[tmpRowSelectedEdit].set('TaxId02', '')
            App.storeGrid.data.items[tmpRowSelectedEdit].set('TaxId03', '')
        } else if (e.value.length == 1) {
            App.storeGrid.data.items[tmpRowSelectedEdit].set('TaxId00', e.value)
            App.storeGrid.data.items[tmpRowSelectedEdit].set('TaxId01', '')
            App.storeGrid.data.items[tmpRowSelectedEdit].set('TaxId02', '')
            App.storeGrid.data.items[tmpRowSelectedEdit].set('TaxId03', '')
        }
        //tai lai du lieu cua grid 1 va 2 cua tab 2 do moi cap nhap o grid 1 tab 1
        reloadDataGrid1AndGrid2Tab2();
        //reload lai tong tien , so tien no goc , so du chung tu (TotAmt, OrigDocAmt , DocBal)
        reloadAmountMustPayTotal();
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


var insertNewRecordGrid1 =  function(){
    var lineRef = HQ.store.lastLineRef(App.storeGrid);
    //tim trong store de kiem cai Descr do vao TranDescr trong Grid
    var indexCustIdToGetDescr = App.cboCustId.store.indexOf(App.cboCustId.store.findRecord("CustID", App.cboCustId.getValue()));
    
    var tranDesc = App.cboCustId.store.data.items[indexCustIdToGetDescr].data.Name;
    var record = Ext.create("App.AR10100_pgLoadInvoiceMemo_ResultModel", {
        //CustId: "",
        LineRef: lineRef,
        LineType: "N",
        TranDesc: tranDesc != "" ? tranDesc : "",
        TaxCat: "*",
        TaxID: App.cboTaxID.store.data.items[0] != undefined ? App.cboTaxID.store.data.items[0].data.TaxID : "",

    });
    App.storeGrid.insert(App.storeGrid.getCount(), record);
};








var cboStatus_Change = function () {
    App.cboHandle.getStore().load(function (records, operation, success) {
        if (records.length) {
            if (App.cboStatus.value == "H") {
                setReadOnly();
                App.cboHandle.setValue("N");
            } else {
                setReadOnly();
                App.cboHandle.setValue("N");
            }
        }
    });
    //setTimeout(function () { waitcboStatusReLoad(); }, 800);
}

//var waitcboRefNbrReLoad = function () {
//    if(App.cboRefNbr.store.data.items[0] != undefined){
//        App.cboRefNbr.setValue(App.cboRefNbr.store.data.items[0].data.RefNbr);
//    }
//}


var setValueBranchID = function () {
    App.txtBranchID.setValue(HQ.cpnyID);
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
var StoreGridTab1ReLoad = function () {

    //App.tabAtAR10100.setActiveTab(App.frmBot); // set tab dang su dung sang tab 1
    App.storeGridTopTab2.removeAll();
    App.storeGridBotTab2.removeAll();
    ////duyet het tat cac dong cua grid 1 tab 1 de bat ra cac du lieu bo vao grid 2 cua tab 2
    //fillDataIntoGrid1Tab2();
    ////duyet grid 1 tab 2 de lay gia tri bo qua grid 2 tab 2 
    //fillDataIntoGrid2Tab2();

    reloadDataGrid1AndGrid2Tab2(0);


    //App.slmGridTopTab2.select(0);
    //App.tabAtAR10100.setActiveTab(App.frmBot); // set tab dang su dung sang tab 1
    //App.slmGridTab1.select(0);//chon dong 1 cua grid 1 tab 1
    setTimeout(function () { waitGridLoadAndFocusIntoBatNbr(); }, 1000);

}

var waitGridLoadAndFocusIntoBatNbr = function () {
    App.cboBatNbr.focus();
}


//doi form 2 tab 1 load xong
var StoreFormBotReLoad = function () {
    App.cboTaxID.getStore().reload();
    //App.cboTaxID1.getStore().reload();
    if (App.cboDocType.readOnly == false && HQ.isNew) {
        App.cboDocType.setValue('IN')
    }

      
    App.storeGrid.load(function (records, operation, success) {
        
            StoreGridTab1ReLoad();
        
    });
 

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


//khi cboTaxID thay doi thi
var cboTaxID_Change = function (item) {//obj = App.cboTaxID

    for (var i = 0; i < App.storeGrid.getCount() ; i++) {
        if (App.slmGridTab1.selected.items[0].data.LineRef == App.storeGrid.data.items[i].data.LineRef) {
            tmpRowSelectedEdit = i;
        }
    }



};


//tai lai du lieu cua grid 1 va 2 cua tab 2 do moi cap nhap o grid 1 tab 1
var reloadDataGrid1AndGrid2Tab2 = function () {
    //xoa du lieu cu de cap nhap du lieu moi
    App.storeGridTopTab2.removeAll();
    App.storeGridBotTab2.removeAll();
    //duyet het tat cac dong cua grid 1 tab 1 de bat ra cac du lieu bo vao grid 2 cua tab 2

    fillDataIntoGrid1Tab2();


    //duyet grid 1 tab 2 de lay gia tri bo qua grid 2 tab 2 
    //setTimeout(function () { fillDataIntoGrid2Tab2(); }, 2000);
    //fillDataIntoGrid2Tab2();
    //App.tabAtAR10100.setActiveTab(App.frmBot); // set tab dang su dung sang tab 1
    //if (!isNaN(index)) {
    //    return true;
    //} else {
        //App.slmGridTab1.select(index);
    //}
}
//reload lai tong tien , so tien no goc , so du chung tu (TotAmt, OrigDocAmt , DocBal)
var reloadAmountMustPayTotal = function () {
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
    //if (!isNaN(index)) {
    //    return true;
    //} else {
        //App.slmGridTab1.select(index);
    //}
}
//khi VendID thay doi
var cboCustId_Change = function (sender,e) {
    App.cboTaxID.getStore().reload();
    App.cboTerms.setValue("10");
    if (HQ.isNew && e != "") {
        insertNewRecordGrid1();
    }
}


//ham validate so duong
var validatePossitiveNumber = function (field, e) {
    if (event.keyCode == e.NUM_MINUS) {
        e.stopEvent();
    }
}


var setReadOnly = function () {
    if (App.cboStatus.value != "H") {
        App.txtOrigRefNbr.setReadOnly(true);
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
        //App.grd.disable(true);
    } else {
        App.txtOrigRefNbr.setReadOnly(false);
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
        //App.grd.enable(true);
    }
}

var removeReadOnly = function () {

}

var setFocusAllCombo = function () {
    App.cboHandle.forceSelection = false;
    App.cboDocType.forceSelection = false;
    App.cboSlsperId.forceSelection = false;
    App.cboCustId.forceSelection = false;
    App.cboTerms.forceSelection = false;

};




//duyet grid 1 tab 2 de lay gia tri bo qua grid 2 tab 2 , sau nay se tach ra thanh function rieng
var fillDataIntoGrid2Tab2 = function () {


    taxAmtTotalVAT00 = 0;
    tranAmtTotalVAT00 = 0;

    taxAmtTotalNoneVat = 0;
    tranAmtTotalNoneVat = 0;

    taxAmtTotalOVAT05 = 0;
    tranAmtTotalOVAT05 = 0;

    taxAmtTotalOVAT10 = 0;
    tranAmtTotalOVAT10 = 0;

    taxAmtTotalIVAT05 = 0;
    tranAmtTotalIVAT05 = 0;

    taxAmtTotalIVAT10 = 0;
    tranAmtTotalIVAT10 = 0;

    taxAmtTotalVAT10 = 0;
    tranAmtTotalVAT10 = 0;

    taxAmtTotalVAT02 = 0;
    tranAmtTotalVAT02 = 0;

    Vat00 = false;
    noneVat = false;
    oVat05 = false;
    oVat10 = false;
    iVAT05 = false;
    iVAT10 = false;
    vAT10 = false;
    vAT02 = false;

    for (var i = 0; i < App.storeGridTopTab2.getCount(); i++) {
        App.slmGridTopTab2.select(i);
        //cac bien tam phai khai bao de truyen gia tri vao
        // taxAmtTotalOVAT05        taxAmtTotalVAT00          oVat05             Vat00             noneVat
        // tranAmtTotalOVAT05      tranAmtTotalVAT00        tranAmtTotalNoneVat
        if (App.slmGridTopTab2.selected.items[0] != undefined) {
            if (App.slmGridTopTab2.selected.items[0].data.TaxID == "OVAT05-00") {
                oVat05 = true;
                taxAmtTotalOVAT05 = taxAmtTotalOVAT05 + parseFloat(App.slmGridTopTab2.selected.items[0].data.TaxAmt)
                tranAmtTotalOVAT05 = tranAmtTotalOVAT05 + parseFloat(App.slmGridTopTab2.selected.items[0].data.TxblAmt)
            } else if (App.slmGridTopTab2.selected.items[0].data.TaxID == "VAT00") {
                Vat00 = true;
                taxAmtTotalVAT00 = taxAmtTotalVAT00 + parseFloat(App.slmGridTopTab2.selected.items[0].data.TaxAmt)
                tranAmtTotalVAT00 = tranAmtTotalVAT00 + parseFloat(App.slmGridTopTab2.selected.items[0].data.TxblAmt)
            } else if (App.slmGridTopTab2.selected.items[0].data.TaxID == "NONEVAT") {
                noneVat = true;
                taxAmtTotalNoneVat = taxAmtTotalNoneVat + parseFloat(App.slmGridTopTab2.selected.items[0].data.TaxAmt)
                tranAmtTotalNoneVat = tranAmtTotalNoneVat + parseFloat(App.slmGridTopTab2.selected.items[0].data.TxblAmt)
            } else if (App.slmGridTopTab2.selected.items[0].data.TaxID == "OVAT10-00") {
                oVat10 = true;
                taxAmtTotalOVAT10 = taxAmtTotalOVAT10 + parseFloat(App.slmGridTopTab2.selected.items[0].data.TaxAmt)
                tranAmtTotalOVAT10 = tranAmtTotalOVAT10 + parseFloat(App.slmGridTopTab2.selected.items[0].data.TxblAmt)
            } else if (App.slmGridTopTab2.selected.items[0].data.TaxID == "IVAT05") {
                iVAT05 = true;
                taxAmtTotalIVAT05 = taxAmtTotalIVAT05 + parseFloat(App.slmGridTopTab2.selected.items[0].data.TaxAmt)
                tranAmtTotalIVAT05 = tranAmtTotalIVAT05 + parseFloat(App.slmGridTopTab2.selected.items[0].data.TxblAmt)
            } else if (App.slmGridTopTab2.selected.items[0].data.TaxID == "IVAT10") {
                iVAT10 = true;
                taxAmtTotalIVAT10 = taxAmtTotalIVAT10 + parseFloat(App.slmGridTopTab2.selected.items[0].data.TaxAmt)
                tranAmtTotalIVAT10 = tranAmtTotalIVAT10 + parseFloat(App.slmGridTopTab2.selected.items[0].data.TxblAmt)
            } else if (App.slmGridTopTab2.selected.items[0].data.TaxID == "VAT10") {
                vAT10 = true;
                taxAmtTotalVAT10 = taxAmtTotalVAT10 + parseFloat(App.slmGridTopTab2.selected.items[0].data.TaxAmt)
                tranAmtTotalVAT10 = tranAmtTotalVAT10 + parseFloat(App.slmGridTopTab2.selected.items[0].data.TxblAmt)
            } else if (App.slmGridTopTab2.selected.items[0].data.TaxID == "VAT02") {
                vAT02 = true;
                taxAmtTotalVAT02 = taxAmtTotalVAT02 + parseFloat(App.slmGridTopTab2.selected.items[0].data.TaxAmt)
                tranAmtTotalVAT02 = tranAmtTotalVAT02 + parseFloat(App.slmGridTopTab2.selected.items[0].data.TxblAmt)
            }
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

    var recordIVAT05Grid2Tab2 = Ext.create("App.GetGrid2Tab2ResultModel", {
        TaxRate2: "5",
        TaxID2: "IVAT05",
        TaxAmt2: taxAmtTotalIVAT05.toString(),
        TxblAmt2: tranAmtTotalIVAT05.toString(),
    });

    var recordIVAT10Grid2Tab2 = Ext.create("App.GetGrid2Tab2ResultModel", {
        TaxRate2: "10",
        TaxID2: "IVAT10",
        TaxAmt2: taxAmtTotalIVAT10.toString(),
        TxblAmt2: tranAmtTotalIVAT10.toString(),
    });

    var recordVAT10Grid2Tab2 = Ext.create("App.GetGrid2Tab2ResultModel", {
        TaxRate2: "10",
        TaxID2: "VAT10",
        TaxAmt2: taxAmtTotalVAT10.toString(),
        TxblAmt2: tranAmtTotalVAT10.toString(),
    });

    var recordVAT02Grid2Tab2 = Ext.create("App.GetGrid2Tab2ResultModel", {
        TaxRate2: "10",
        TaxID2: "VAT02",
        TaxAmt2: taxAmtTotalVAT02.toString(),
        TxblAmt2: tranAmtTotalVAT02.toString(),
    });

    //xet tung truong hop de tao ra cac dong tuong ung
    if (oVat05 == true) {
        App.storeGridBotTab2.insert(0, recordOVAT05Grid2Tab2);
    }
    if (Vat00 == true) {
        App.storeGridBotTab2.insert(0, recordVAT00Grid2Tab2);
    }
    if (noneVat == true) {
        App.storeGridBotTab2.insert(0, recordVATNONEGrid2Tab2);
    }
    if (oVat10 == true) {
        App.storeGridBotTab2.insert(0, recordOVAT10Grid2Tab2);
    }
    if (iVAT05 == true) {
        App.storeGridBotTab2.insert(0, recordIVAT05Grid2Tab2);
    }
    if (iVAT10 == true) {
        App.storeGridBotTab2.insert(0, recordIVAT10Grid2Tab2);
    }
    if (vAT10 == true) {
        App.storeGridBotTab2.insert(0, recordVAT10Grid2Tab2);
    }
    if (vAT02 == true) {
        App.storeGridBotTab2.insert(0, recordVAT02Grid2Tab2);
    }

}


var fillDataIntoGrid1Tab2 = function () {
    var PrcTaxInclRate00 = 0;
    var PrcTaxInclRate01 = 0;
    var PrcTaxInclRate02 = 0;
    var PrcTaxInclRate03 = 0;
    var TotPrcTaxInclAmt = 0;
    var TxblAmtL1 = 0;
    var TxblAmtAddL2 = 0;
    for (var i = 0; i < App.storeGrid.getCount() ; i++) {

        App.slmGridTab1.select(i);

        //lay cac gia tri can thiet khac
        var lineRef = App.slmGridTab1.selected.items[0].data.LineRef;
        //truong hop i == 0  nay la dung de fix loi khi TrantAmt thay doi thi ko set duoc gia tri
        //ben grid 1 tab 2 thay doi ngay
        if (i == 0) {
            if (tmpTrantAmtWhenChange != 0) {
                var tranAmt = tmpTrantAmtWhenChange;
            } else {
                var tranAmt = App.slmGridTab1.selected.items[0].data.TranAmt;
            }
        } else {
            var tranAmt = App.slmGridTab1.selected.items[0].data.TranAmt;
        }
        var taxCat = App.slmGridTab1.selected.items[0].data.TaxCat;
        //var taxID = App.slmGridTab1.selected.items[0].data.TaxID;
        //var taxIDArray = taxID.split(',');
        var taxID0 = App.slmGridTab1.selected.items[0].data.TaxId00;
        var taxID1 = App.slmGridTab1.selected.items[0].data.TaxId01;
        var taxID2 = App.slmGridTab1.selected.items[0].data.TaxId02;
        var taxID3 = App.slmGridTab1.selected.items[0].data.TaxId03;



        //xu ly taxID , taxCat
        
        
        var indexTaxRate00 = App.storeSITaxAll.indexOf(App.storeSITaxAll.findRecord("TaxID", taxID0));
        if (indexTaxRate00 != -1) { // neu ton tai taxID00
            var taxRate00 = App.storeSITaxAll.data.items[indexTaxRate00].data.TaxRate;
            var objTax00 = App.storeSITaxAll.data.items[indexTaxRate00].data;

            if (taxCat == "*" || (objTax00.CatFlg == "A" && objTax00.CatExcept00 != taxCat && objTax00.CatExcept01 != taxCat && objTax00.CatExcept02 != taxCat && objTax00.CatExcept03 != taxCat && objTax00.CatExcept04 != taxCat && objTax00.CatExcept05 != taxCat) ||
                (objTax00.CatFlg == "N" && (objTax00.CatExcept00 == taxCat || objTax00.CatExcept01 == taxCat || objTax00.CatExcept02 == taxCat || objTax00.CatExcept03 == taxCat || objTax00.CatExcept04 == taxCat || objTax00.CatExcept05 == taxCat))) {
                if (objTax00.TaxCalcLvl == "1" && objTax00.PrcTaxIncl != "0") {
                    PrcTaxInclRate00 = PrcTaxInclRate00 + objTax00.TaxRate;
                } else {
                    PrcTaxInclRate00 = objTax00.TaxRate;
                }
                //insert dong moi vao
                var taxAmtTaxID00 = tranAmt * PrcTaxInclRate00 / 100;

                var recordGrid1Tab2TaxID00 = Ext.create("App.GetGrid1Tab2ResultModel", {
                    //CustId: "",
                    LineRef: lineRef,
                    TaxID: taxID0,
                    TaxRate: taxRate00,
                    TaxAmt: taxAmtTaxID00,
                    TxblAmt: tranAmt.toString(),
                    Level: "1"
                });
                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordGrid1Tab2TaxID00);
                App.slmGridTab1.selected.items[0].set('TaxAmt00', taxAmtTaxID00)
              

            }
        }


        var indexTaxRate01 = App.storeSITaxAll.indexOf(App.storeSITaxAll.findRecord("TaxID", taxID1));
        if (indexTaxRate01 != -1) {// neu ton tai taxID01
            var taxRate01 = App.storeSITaxAll.data.items[indexTaxRate01].data.TaxRate;
            var objTax01 = App.storeSITaxAll.data.items[indexTaxRate01].data;

            if (taxCat == "*" || (objTax01.CatFlg == "A" && objTax01.CatExcept00 != taxCat && objTax01.CatExcept01 != taxCat && objTax01.CatExcept02 != taxCat && objTax01.CatExcept03 != taxCat && objTax01.CatExcept04 != taxCat && objTax01.CatExcept05 != taxCat) ||
                (objTax01.CatFlg == "N" && (objTax01.CatExcept00 == taxCat || objTax01.CatExcept01 == taxCat || objTax01.CatExcept02 == taxCat || objTax01.CatExcept03 == taxCat || objTax01.CatExcept04 == taxCat || objTax01.CatExcept05 == taxCat))) {
                if (objTax01.TaxCalcLvl == "1" && objTax01.PrcTaxIncl != "0") {
                    PrcTaxInclRate01 = PrcTaxInclRate01 + objTax01.TaxRate;
                } else {
                    PrcTaxInclRate01 = objTax01.TaxRate;
                }

                //insert dong moi vao
                var taxAmtTaxID01 = tranAmt * PrcTaxInclRate01 / 100;
                var recordGrid1Tab2TaxID01 = Ext.create("App.GetGrid1Tab2ResultModel", {
                    //CustId: "",
                    LineRef: lineRef,
                    TaxID: taxID1,
                    TaxRate: taxRate01,
                    TaxAmt: taxAmtTaxID01,
                    TxblAmt: tranAmt.toString(),
                    Level: "1"
                });
                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordGrid1Tab2TaxID01);
                App.slmGridTab1.selected.items[0].set('TaxAmt01', taxAmtTaxID01)
            }
        }

        var indexTaxRate02 = App.storeSITaxAll.indexOf(App.storeSITaxAll.findRecord("TaxID", taxID2));
        if (indexTaxRate02 != -1) {// neu ton tai taxID02
            var taxRate02 = App.storeSITaxAll.data.items[indexTaxRate02].data.TaxRate;
            var objTax02 = App.storeSITaxAll.data.items[indexTaxRate02].data;

            if (taxCat == "*" || (objTax02.CatFlg == "A" && objTax02.CatExcept00 != taxCat && objTax02.CatExcept01 != taxCat && objTax02.CatExcept02 != taxCat && objTax02.CatExcept03 != taxCat && objTax02.CatExcept04 != taxCat && objTax02.CatExcept05 != taxCat) ||
                (objTax02.CatFlg == "N" && (objTax02.CatExcept00 == taxCat || objTax02.CatExcept01 == taxCat || objTax02.CatExcept02 == taxCat || objTax02.CatExcept03 == taxCat || objTax02.CatExcept04 == taxCat || objTax02.CatExcept05 == taxCat))) {
                if (objTax02.TaxCalcLvl == "1" && objTax02.PrcTaxIncl != "0") {
                    PrcTaxInclRate02 = PrcTaxInclRate02 + objTax02.TaxRate;
                } else {
                    PrcTaxInclRate02 = objTax02.TaxRate;
                }
                //insert dong moi vao
                var taxAmtTaxID02 = tranAmt * PrcTaxInclRate02 / 100;
                var recordGrid1Tab2TaxID02 = Ext.create("App.GetGrid1Tab2ResultModel", {
                    //CustId: "",
                    LineRef: lineRef,
                    TaxID: taxID2,
                    TaxRate: taxRate02,
                    TaxAmt: taxAmtTaxID02,
                    TxblAmt: tranAmt.toString(),
                    Level: "1"
                });
                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordGrid1Tab2TaxID02);
                App.slmGridTab1.selected.items[0].set('TaxAmt02', taxAmtTaxID02)
            }
        }

        var indexTaxRate03 = App.storeSITaxAll.indexOf(App.storeSITaxAll.findRecord("TaxID", taxID3));
        if (indexTaxRate03 != -1) {// neu ton tai taxID03
            var taxRate03 = App.storeSITaxAll.data.items[indexTaxRate03].data.TaxRate;
            var objTax03 = App.storeSITaxAll.data.items[indexTaxRate03].data;

            if (taxCat == "*" || (objTax03.CatFlg == "A" && objTax03.CatExcept00 != taxCat && objTax03.CatExcept01 != taxCat && objTax03.CatExcept02 != taxCat && objTax03.CatExcept03 != taxCat && objTax03.CatExcept04 != taxCat && objTax03.CatExcept05 != taxCat) ||
                (objTax03.CatFlg == "N" && (objTax03.CatExcept00 == taxCat || objTax03.CatExcept01 == taxCat || objTax03.CatExcept02 == taxCat || objTax03.CatExcept03 == taxCat || objTax03.CatExcept04 == taxCat || objTax03.CatExcept05 == taxCat))) {
                if (objTax03.TaxCalcLvl == "1" && objTax03.PrcTaxIncl != "0") {
                    PrcTaxInclRate03 = PrcTaxInclRate03 + objTax03.TaxRate;
                } else {
                    PrcTaxInclRate03 = objTax01.TaxRate;
                }
                //insert dong moi vao
                var taxAmtTaxID03 = tranAmt * PrcTaxInclRate03 / 100;
                var recordGrid1Tab2TaxID03 = Ext.create("App.GetGrid1Tab2ResultModel", {
                    //CustId: "",
                    LineRef: lineRef,
                    TaxID: taxID3,
                    TaxRate: taxRate03,
                    TaxAmt: taxAmtTaxID03,
                    TxblAmt: tranAmt.toString(),
                    Level: "1"
                });
                App.storeGridTopTab2.insert(App.storeGridTopTab2.data.length, recordGrid1Tab2TaxID03);
                App.slmGridTab1.selected.items[0].set('TaxAmt03', taxAmtTaxID03)
            }
        }

        App.slmGridTab1.selected.items[0].set('TxblAmt00', tranAmt * (1 + PrcTaxInclRate00 + PrcTaxInclRate01 + PrcTaxInclRate02 + PrcTaxInclRate03) / 100);

    }//ngoac ket thuc vong for
    //fillDataIntoGrid2Tab2();
    setTimeout(function () { fillDataIntoGrid2Tab2(); }, 3000);

}



////khi cboTaxCat thay doi thi
//var cboTaxCat_Change = function (item) {
//    App.slmGridTab1.selected.items[0].set('TaxCat', item.value);
//    //var index = App.slmGridTab1.selected.items[0].index;
//    ////xet dieu kien neu them moi thi index se bat khac di , do LineType khac rong se tao ra dong moi nen neu LineType chua co ta bat index tu dong la getCount()-1
//    //if (index == undefined && App.slmGridTab1.selected.items[0].data.InvtID != "") {
//    //    for (var i = 0; i < App.storeGrid.data.length; i++) {
//    //        if (App.storeGrid.data.items[i].data.InvtID == App.slmGridTab1.selected.items[0].data.InvtID) {
//    //            index = i;
//    //        }
//    //    }
//    //}
//    //tai lai du lieu cua grid 1 va 2 cua tab 2 do moi cap nhap o grid 1 tab 1
//    reloadDataGrid1AndGrid2Tab2();
//    //reload lai tong tien , so tien no goc , so du chung tu (TotAmt, OrigDocAmt , DocBal)
//    reloadAmountMustPayTotal();

//}

////thay doi so luong
//var txtQty_Blur = function (item) {
//    //bat cac gia tri de chuan bi tinh toan lai
//    var quantity = item.value;

//    //var unitprice = App.slmGridTab1.selected.items[0].data.UnitPrice;
//    //var index = App.slmGridTab1.selected.items[0].index;

//    var unitprice = App.grd.editingPlugin.activeRecord.data.UnitPrice;
//    //var index = App.slmGridTab1.selected.items[0].index;
//    //xet dieu kien neu them moi thi index se bat khac di , do LineType khac rong se tao ra dong moi nen neu LineType chua co ta bat index tu dong la getCount()-1
//    //if (index == undefined && App.slmGridTab1.selected.items[0].data.InvtID != "") {
//    //    for (var i = 0; i < App.storeGrid.data.length; i++) {
//    //        if (App.storeGrid.data.items[i].data.InvtID == App.slmGridTab1.selected.items[0].data.InvtID) {
//    //            index = i;
//    //        }
//    //    }
//    //} else if (index == undefined && App.slmGridTab1.selected.items[0].data.InvtID == "") {
//    //    index = 0;
//    //}
//    //App.slmGridTab1.selected.items[0].set('TranAmt', quantity * unitprice);
//    App.grd.editingPlugin.activeRecord.set('TranAmt', quantity * unitprice);
//    //if (quantity == 0) {
//    //    App.storeGrid.data.items[tmpRowSelectedEdit].set('Qty', 1);
//    //    App.storeGrid.data.items[tmpRowSelectedEdit].set('Qty', quantity);
//    //} else {
//    //    App.storeGrid.data.items[tmpRowSelectedEdit].set('Qty', 0);
//    //    App.storeGrid.data.items[tmpRowSelectedEdit].set('Qty', quantity);
//    //}
//    //App.grd.editingPlugin.activeRecord.set('Qty', quantity);
//    //tai lai du lieu cua grid 1 va 2 cua tab 2 do moi cap nhap o grid 1 tab 1
//    reloadDataGrid1AndGrid2Tab2();
//    //reload lai tong tien , so tien no goc , so du chung tu (TotAmt, OrigDocAmt , DocBal)
//    reloadAmountMustPayTotal();
//    //chon lai cot minh dang edit
//    //App.grd.editingPlugin.startEditByPosition({ row: index, column: 3 });

//}
////thay doi gia cho 1 dv san pham
//var txtUnitPrice_Blur = function (item) {
//    //bat cac gia tri de chuan bi tinh toan lai
//    //var quantity = App.slmGridTab1.selected.items[0].data.Qty;
//    var quantity = App.grd.editingPlugin.activeRecord.data.Qty;
//    var unitprice = item.value;
//    //var index = App.slmGridTab1.selected.items[0].index;
//    //xet dieu kien neu them moi thi index se bat khac di , do LineType khac rong se tao ra dong moi nen neu LineType chua co ta bat index tu dong la getCount()-1
//    //if (index == undefined && App.slmGridTab1.selected.items[0].data.InvtID != "") {
//    //    for (var i = 0; i < App.storeGrid.data.length; i++) {
//    //        if (App.storeGrid.data.items[i].data.InvtID == App.slmGridTab1.selected.items[0].data.InvtID) {
//    //            index = i;
//    //        }
//    //    }

//    //} else if (index == undefined && App.slmGridTab1.selected.items[0].data.InvtID == "") {
//    //    index = 0;
//    //}
//    //App.slmGridTab1.selected.items[0].set('TranAmt', quantity * unitprice);
//    App.grd.editingPlugin.activeRecord.set('TranAmt', quantity * unitprice);
//    //App.grd.editingPlugin.activeRecord.set('UnitPrice', unitprice);
//    ////tai lai du lieu cua grid 1 va 2 cua tab 2 do moi cap nhap o grid 1 tab 1
//    //reloadDataGrid1AndGrid2Tab2();

//    //tai lai du lieu cua grid 1 va 2 cua tab 2 do moi cap nhap o grid 1 tab 1
//    reloadDataGrid1AndGrid2Tab2();
//    //reload lai tong tien , so tien no goc , so du chung tu (TotAmt, OrigDocAmt , DocBal)
//    reloadAmountMustPayTotal();
//    //chon lai cot minh dang edit
//    //App.grd.editingPlugin.startEditByPosition({ row: index, column: 4 });
//}
////thay doi gia tong cong 
//var txtTranAmt_Blur = function (item) {
//    //bat cac gia tri de chuan bi tinh toan lai
//    var tranAmt = item.value;
//    var quantity = App.grd.editingPlugin.activeRecord.data.Qty;
//    //var quantity = App.slmGridTab1.selected.items[0].data.Qty;
//    //var index = App.slmGridTab1.selected.items[0].index;
//    //xet dieu kien neu them moi thi index se bat khac di , do LineType khac rong se tao ra dong moi nen neu LineType chua co ta bat index tu dong la getCount()-1
//    //if (index == undefined && App.slmGridTab1.selected.items[0].data.InvtID != "") {
//    //    for (var i = 0; i < App.storeGrid.data.length; i++) {
//    //        if (App.storeGrid.data.items[i].data.InvtID == App.slmGridTab1.selected.items[0].data.InvtID) {
//    //            index = i;
//    //        }
//    //    }
//    //} else if (index == undefined && App.slmGridTab1.selected.items[0].data.InvtID == "") {
//    //    index = 0;
//    //}
//    if (quantity != 0) {
//        App.grd.editingPlugin.activeRecord.set('UnitPrice', tranAmt / quantity);
//        //App.grd.editingPlugin.activeRecord.set('TranAmt', tranAmt);
//        //App.slmGridTab1.selected.items[0].set('UnitPrice', tranAmt / quantity);
//    }
//    //bien tam tmpTrantAmtWhenChange dung de fix bug khi thay doi TrantAmt dong dau tien thi grid 1 tab 2 ko thay doi
//    tmpTrantAmtWhenChange = tranAmt;
//    //tai lai du lieu cua grid 1 va 2 cua tab 2 do moi cap nhap o grid 1 tab 1
//    reloadDataGrid1AndGrid2Tab2();
//    //reload lai tong tien , so tien no goc , so du chung tu (TotAmt, OrigDocAmt , DocBal)
//    reloadAmountMustPayTotal();
//    //chon lai cot minh dang edit
//    //App.grd.editingPlugin.startEditByPosition({ row: index, column: 5 });

//}


//ham dung doi de doi refresh form sau do insert
//var waitcboRefNbrForInsert = function () {
//    App.cboCustId.enable(true);
//    App.cboDocType.enable(true);
//    var time = new Date();
//    App.txtBranchID.setValue(HQ.cpnyID);
//    if (App.storeFormBot.data.items.length == 0) {
//        var record = Ext.create("App.AP_DocClassModel", {
//            //CustId: "",
//            DocType: App.cboDocType.store.data.items[0].data.Code,
//            DocDate: time,
//            InvcDate: time,
//            DiscDate: time,
//            OrigDocAmt: 0,
//            DocBalL: 0,
//            BatNbr: App.cboBatNbr.getValue(),
//            BranchID: App.txtBranchID.getValue(),
//            Terms: App.cboTerms.getValue(),

//        });
//        App.storeFormBot.insert(0, record);
//        App.frmBot.getForm().loadRecord(App.storeFormBot.getAt(0));
//    }
//    cboTerms_Change();
//    App.storeGrid.reload();
//}



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
//    reloadDataGrid1AndGrid2Tab2();
//    //reload lai tong tien , so tien no goc , so du chung tu (TotAmt, OrigDocAmt , DocBal)
//    reloadAmountMustPayTotal();
//    var taxid00 = App.slmGridTab1.selected.items[0].data.TaxId00;
//    //if(taxid00.search(","){
//    //    var taxid00change = taxid00.substring(0, r.data.TaxId00.search(","));
//    //    r.set('TaxId00', taxid00change);
//    //}

//}
