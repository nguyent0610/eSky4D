
var keys = ['TranAmt'];
var fieldsCheckRequire = [ 'TranAmt'];
var fieldsLangCheckRequire = [ 'TranAmt'];
var menuClick = function (command) {
    switch (command) {
        case "first":
        
            if (HQ.focus == 'header_Batch') {
                HQ.combo.first(App.cboBatNbr, HQ.isChange);          
            } else if (HQ.focus == 'grdAR_Trans') {
                HQ.grid.first(App.grdAR_Trans);
            }
            break;
        case "prev":
 
            if (HQ.focus == 'header_Batch') {
                HQ.combo.prev(App.cboBatNbr, HQ.isChange);          
            } else if (HQ.focus == 'grdAR_Trans') {
                HQ.grid.prev(App.grdAR_Trans);
            }
            break;
        case "next":

            if (HQ.focus == 'header_Batch') {
                HQ.combo.next(App.cboBatNbr, HQ.isChange);         
            } else if (HQ.focus == 'grdAR_Trans') {
                HQ.grid.next(App.grdAR_Trans);
            }
            break;
        case "last":

            if (HQ.focus == 'header_Batch') {
                HQ.combo.last(App.cboBatNbr, HQ.isChange);          
            } else if (HQ.focus == 'grdAR_Trans') {
                HQ.grid.last(App.grdAR_Trans);
            }


            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            } else {
                HQ.isChange = false;
                refresh('yes');
            }
            break;
        case "new":
            if (HQ.isInsert) {

                if (HQ.focus == 'header_Batch' ) {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', '');
                    }
                    else {
                        App.cboBatNbr.setValue('');                                                       
                    }              
                } else if (HQ.focus == 'grdAR_Trans') {
                    HQ.grid.insert(App.grdAR_Trans, keys);                                   
                }

            }
            break;
        case "delete":

            if (HQ.isDelete) {
                if (App.cboBatNbr.value != "" && App.cboStatus.value != "C" && App.cboStatus.value != "V") {
                    if (HQ.focus == 'header_Batch') {
                            HQ.message.show(11, '', 'deleteRecordFormTopBatch');                  
                    } else if (HQ.focus == 'grdAR_Trans') {
                        var rowindex = HQ.grid.indexSelect(App.grdAR_Trans);
                        if (rowindex != '') {
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdAR_Trans), ''], 'deleteRecordGrid', true)
                        }
                    }
                }
            }
            break;
        case "save":          
            if (App.cboStatus.getValue() == "V" || ((Ext.isEmpty(App.cboHandle.getValue()) || App.cboHandle.getValue() == "N") && App.cboStatus.getValue() == "C")) return;
            if (App.txtCuryDocBal.getValue()=='0') {
                HQ.message.show(20151111901, '', '');
                return;
            }
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain) &&                 
                    HQ.store.checkRequirePass(App.stoAR_Trans, keys, fieldsCheckRequire, fieldsLangCheckRequire)
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
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        if (App.cboBatNbr.valueModels == null) App.cboBatNbr.setValue('');
        App.cboBatNbr.getStore().load(function () {             
                App.stoAR10100_pdHeader.reload();               
        });
       
    }
};
////////Save////////////////
////////////////////////////
function Save() {
  
    App.frmMain.getForm().updateRecord();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: 'Submiting...',
            url: 'AR10100/Save',
            params: {
                lstheader_Batch: Ext.encode(App.stoAR10100_pdHeader.getRecordsValues()),             
                lstgrd: HQ.store.getData(App.stoAR_Trans)              
            },
            success: function (msg, data) {
                HQ.message.process(msg, data, true);
                var BatNbr = '';
                var RefNbr = '';
                if (this.result.data != undefined && this.result.data.BatNbr != null) {
                    BatNbr = this.result.data.BatNbr;
                    RefNbr = this.result.data.RefNbr;
                }
                if (BatNbr != '') {
                    App.cboBatNbr.getStore().load(function () {
                        if(Ext.isEmpty(App.cboBatNbr.getValue()))
                        {
                            App.cboBatNbr.setValue(BatNbr);                                                                            
                        }else   App.stoAR10100_pdHeader.reload();
                         
                    });
                }
             
            }
            , failure: function (msg, data) {
                HQ.message.process(msg, data, true);
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

///////////////Delete////////////////////////////
/////////////////////////////////////////////////
//xac nhan xoa Record form Top Batch
var deleteRecordFormTopBatch = function (item) {
    if (item == 'yes') {
        App.frmMain.submit({
            clientValidation: false,
            timeout: 1800000,
            waitMsg: HQ.common.getLang('DeletingData'),
            url: 'AR10100/Delete_Batch',
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

// Xac nhan xoa record cua Grid
var deleteRecordGrid = function (item) {
    if (item == "yes") {
        //var index = App.slmgrdAR_Trans.selected.items[0].index;
        App.grdAR_Trans.deleteSelected();     
        delTaxMutil();
        calcDet();
        frmChange();
    }
};

///////////Kiem tra combo chinh BatNbr//////////////////////
//khi co su thay doi du lieu cua cac conttol tren form
//load lần đầu khi mở///////////////////////////////////////////
/////////////////////////////////////////////////////////////////
var firstLoad = function () {
    App.txtBranchID.setValue(HQ.cpnyID);
    App.cboBatNbr.getStore().load(function () { App.stoAR10100_pdHeader.reload(); });
   
};
var frmChange = function () {
    if (App.stoAR10100_pdHeader.data.length > 0 ) {
        App.frmMain.getForm().updateRecord();
        HQ.isChange = HQ.store.isChange(App.stoAR10100_pdHeader) == false ? HQ.store.isChange(App.stoAR_Trans): true;
        HQ.common.changeData(HQ.isChange, 'AR10100');//co thay doi du lieu gan * tren tab title header      
        App.cboBatNbr.setReadOnly(HQ.isChange);
       
    }
    else {
        HQ.isChange = false;
        HQ.common.changeData(HQ.isChange, 'AR10100');
    }
};
//////////////Form Batch///////////////////////////
/////////////////////////////////////////////////
var stoAR10100_pdHeader_BeforeLoad = function (store) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'), App.frmMain);
};
var stoAR10100_pdHeader_Load = function (store) {    
    HQ.isFirstLoad = true;
   
    HQ.isNew = false;

    App.cboCustId.setReadOnly(true);
    App.cboDocType.setReadOnly(true);
    if (store.data.length == 0) {
   
        HQ.store.insertBlank(store, "RefNbr");
        record = store.getAt(0);

        record.data.Status = 'H';
        //record.data.BranchID = HQ.cpnyID;
        //record.data.JrnlType = 'AR';
        //record.data.EditScrnNbr = 'AR10100';
        //record.data.Module = 'AR';
        //record.data.DateEnt = HQ.businessDate;
      
        record.data.DocDate = HQ.businessDate;
        record.data.DueDate = HQ.businessDate;
        record.data.DiscDate = HQ.businessDate;
        record.data.DocType = "IN";
        record.data.OrigDocAmt = 0;
        record.data.DocBal = 0;
        record.data.OrigDocAmt = 0;

        store.commitChanges();//commit cho record thanh updated muc dich de dung ham HQ.store.isChange
        HQ.isNew = true;//record la new
        HQ.common.setRequire(App.frmMain);  //to do cac o la require
        App.cboCustId.setReadOnly(false);
        App.cboDocType.setReadOnly(false);

    }
    var record = store.getAt(0);
    App.frmMain.getForm().loadRecord(record);    
  
    frmChange();
    App.stoAR_Trans.reload();
    App.stoAR10100_pgLoadTaxTrans.reload();


};
var cboBatNbr_Change = function (sender, value) {
    if ((!HQ.isNew || sender.valueModels != null) && !App.stoAR10100_pdHeader.loading) {       
        App.stoAR10100_pdHeader.reload();
    }
};
var cboBatNbr_Select = function (sender, value) {
    if (sender.valueModels != null && !App.stoAR10100_pdHeader.loading) {
      
        App.stoAR10100_pdHeader.reload();
    }    
};
var cboStatus_Change = function (item, newValue, oldValue) {
    if (newValue == 'H' && HQ.isInsert && HQ.isUpdate) {
        HQ.common.lockItem(App.frmMain, false);
    }
    else HQ.common.lockItem(App.frmMain, true);
    App.cboHandle.getStore().load(function (records, operation, success) {
        App.cboHandle.setValue("N");
    });
}

var cboCustId_Change = function (sender, e) {
    App.cboTaxID.getStore().reload();
  
    var obj = HQ.store.findInStore(App.cboCustId.getStore(), ["CustID"], [e]);
    if (obj != undefined && App.cboCustId.hasFocus) {
        App.cboTerms.setValue(obj.Terms);
        var obj = HQ.store.findInStore(App.cboTerms.getStore(), ["TermsID"], [obj.Terms]);
        if (obj != undefined) {
            App.dteDueDate.setValue(App.dteDocDate.value ? App.dteDocDate.getValue().addDays(obj.DueIntrv) : App.dteDueDate.value);
        }
    }
   
}
var cboTerms_Change = function (sender, e) {
    var obj = HQ.store.findInStore(App.cboTerms.getStore(), ["TermsID"], [e]);
    if (obj != undefined && App.cboTerms.hasFocus) {
        App.dteDueDate.setValue(App.dteDocDate.value ? App.dteDocDate.getValue().addDays(obj.DueIntrv) : App.dteDueDate.value);
    } 
}
var dteDocDate_Change = function (sender, e) {
    var obj = HQ.store.findInStore(App.cboTerms.getStore(), ["TermsID"], [App.cboTerms.getValue()]);
    if (obj != undefined && App.dteDocDate.hasFocus) {
        App.dteDueDate.setValue(App.dteDocDate.value ? App.dteDocDate.getValue().addDays(obj.DueIntrv) : App.dteDueDate.value);
    }
}
var cboHandle_Change = function (sender, e) {
    if (App.cboHandle.getValue() == "R") {
        App.txtInvcNbr.allowBlank = false;
        App.txtInvcNote.allowBlank = false;
        
    }
    else {
        App.txtInvcNbr.allowBlank = true;
        App.txtInvcNote.allowBlank = true;
    }
    App.txtInvcNbr.validate();
    App.txtInvcNote.validate();
}
/////////Grid////////////////////////////////
//////////////////////////////////////////////////
var loadDataGrid = function (store) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(store, keys);
        }
        HQ.isFirstLoad = false;
    }
    frmChange();
    calcDet();
    HQ.common.showBusy(false, '', App.frmMain);
}
var loadstoAR10100_pgLoadTaxTrans = function () {
    App.stoAR10100_LoadTaxDoc.clearData();
    calcTaxTotal();
};


var stoChanged = function (sto) {
    if (!Ext.isEmpty(App.cboBatNbr.getValue()) || App.stoAR_Trans.data.length > 1) {
        App.cboCustId.setReadOnly(true);
        App.cboDocType.setReadOnly(true);
    }
    else {
        App.cboCustId.setReadOnly(false);
        App.cboDocType.setReadOnly(false);     
    }
    frmChange();
};
var grd_BeforeEdit = function (editor, e) {
    var det = e.record.data;
   
    if (App.cboStatus.value != "H") {
        return false;
    }
    if (Ext.isEmpty(App.cboCustId.getValue())) {
        HQ.message.show(15, App.cboCustId.fieldLabel, '');
        return false;
    }
    else if (Ext.isEmpty(App.txtDocDescr.getValue())) {
        HQ.message.show(15, App.txtDocDescr.fieldLabel, '');
        return false;
    }
  
    if (Ext.isEmpty(det.LineRef)) {
        e.record.set("LineType", "N");
        var indexCustIdToGetDescr = App.cboCustId.store.indexOf(App.cboCustId.store.findRecord("CustID", App.cboCustId.getValue()));
        var obj = HQ.store.findInStore(App.cboCustId.getStore(), ["CustID"], [App.cboCustId.getValue()]);
        var tranDesc = obj == undefined ? "" : obj.CustID + ' - ' + obj.Name;
        e.record.set("TranDesc",tranDesc != "" ? tranDesc : "");
       
             
        var valueTax = '';
        App.cboTaxID.getStore().data.each(function (det) {
            valueTax += det.data.TaxID + ',';

        });
        valueTax = valueTax.length > 0 ? valueTax.substring(0, valueTax.length - 1) : '';
        e.record.set("TaxID", valueTax);
        e.record.set('LineRef', HQ.store.lastLineRef(App.stoAR_Trans));
        //return false;
    }
   
    
    

};
var grd_Edit = function (item, e) {
    var r = e.record.data;    
    if (e.field == 'Qty') {
        var quantity = e.value;     
        e.record.set('TranAmt', quantity * r.UnitPrice);    
    } else if (e.field == 'UnitPrice') {
        var unitprice = e.value;    
        e.record.set('TranAmt', r.Qty * unitprice);      
    } else if (e.field == 'TranAmt') {
        var tranAmt = e.value;    
        if (r.Qty != 0) {
            e.record.set('UnitPrice', tranAmt / r.Qty);
        }       
    }
    if (r.TranAmt >0)
        HQ.grid.checkInsertKey(App.grdAR_Trans, e, keys);
    if (e.field == 'InvtId') {
        var obj = App.cboInvtID.getStore().findRecord("InvtID", e.value);
        e.record.set("TaxCat", obj == undefined ? '' : obj.data.TaxCat);
    }



    delTax(e.rowIdx);
    calcTax(e.rowIdx);
    calcTaxTotal();
};
var grd_ValidateEdit = function (item, e) {

    if (e.field=='InvtId' && !Ext.isEmpty(e.value)) {
        if (HQ.grid.checkDuplicate(App.grdAR_Trans, e, ['InvtId'])) {
            HQ.message.show(85, e.value, '');          
            return false;
        }
    }
    return true;
};
var grd_Reject = function (record) {
    if (record.data.tstamp == '') {
        var index = App.stoAR_Trans.indexOf(record);
        delTax(index);
        calcTaxTotal();

        App.stoAR_Trans.remove(record);
        App.grdAR_Trans.getView().focusRow(App.stoAR_Trans.getCount() - 1);
        App.grdAR_Trans.getSelectionModel().select(App.stoAR_Trans.getCount() - 1);
    } else {
        var index = App.stoAR_Trans.indexOf(record);
        record.reject();
        delTax(index);
        calcTax(index);
        calcTaxTotal();
        calcDet();
    }
};

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
var renderRowNumber = function (value, meta, record) {
    return App.stoAR_Trans.allData.indexOf(record) + 1;
}
var expand = function (cbo, delimiter) {
    var value = App.slmgrdAR_Trans.selected.items[0].data;
    cbo.setValue(value.TaxID.toString().replace(new RegExp(delimiter, 'g'), ',').split(','));
}

//other
function calcDet() {
    if (App.cboStatus.getValue() != 'H') return;
    //if (App.OrderType.getValue() == null) return;
    var taxAmt00 = 0;
    var taxAmt01 = 0;
    var taxAmt02 = 0;
    var taxAmt03 = 0;


    var TxblAmt00 = 0;
    var TxblAmt01 = 0;
    var TxblAmt02 = 0;
    var TxblAmt03 = 0;

    var TranAmt = 0;
    var TotAmt = 0; 

    for (var j = 0; j < App.stoAR_Trans.allData.length; j++) {
        var det = App.stoAR_Trans.allData.items[j];
        taxAmt00 += det.data.TaxAmt00;
        taxAmt01 += det.data.TaxAmt01;
        taxAmt02 += det.data.TaxAmt02;
        taxAmt03 += det.data.TaxAmt03;
     
        TxblAmt00 += det.data.TxblAmt00;
        TxblAmt01 += det.data.TxblAmt01;
        TxblAmt02 += det.data.TxblAmt02;
        TxblAmt03 += det.data.TxblAmt03;

        TranAmt += det.data.TranAmt;
      
     
    };
    //for (var j = 0; j < App.cboRefNbr.getStore().data.length; j++) {
    //    var det = App.cboRefNbr.getStore().data.items[j].data;
    //    if(det.RefNbr != App.cboRefNbr.getValue() && det.Rlsed != -1)
    //    {
    //        TotAmt += det.OrigDocAmt;
    //    }
    //    else if (det.RefNbr == App.cboRefNbr.getValue() && det.Rlsed != -1) {
    //        TotAmt += taxAmt00 + taxAmt01 + taxAmt02 + taxAmt03 + TxblAmt00 + TxblAmt01 + TxblAmt02 + TxblAmt03;
    //    }         
    //};
    //if (Ext.isEmpty(App.cboRefNbr.getValue())) TotAmt += taxAmt00 + taxAmt01 + taxAmt02 + taxAmt03 + TxblAmt00 + TxblAmt01 + TxblAmt02 + TxblAmt03;
    TotAmt = taxAmt00 + taxAmt01 + taxAmt02 + taxAmt03 + TxblAmt00 + TxblAmt01 + TxblAmt02 + TxblAmt03;
   
    App.txtCuryCrTot.setValue(TotAmt);
    App.txtTxblTot.setValue(TxblAmt00 + TxblAmt01 + TxblAmt02 + TxblAmt03);
    App.txtTaxTot.setValue(taxAmt00 + taxAmt01 + taxAmt02 + taxAmt03);
    App.txtCuryOrigDocAmt.setValue(TotAmt);
    App.txtCuryDocBal.setValue(TotAmt);
}
function delTaxMutil() {

    for (var i = App.stoAR10100_pgLoadTaxTrans.data.length - 1; i >= 0; i--) {
        var data = HQ.store.findInStore(App.stoAR_Trans, ['LineRef'], [App.stoAR10100_pgLoadTaxTrans.data.items[i].data.LineRef])
        if (!data) App.stoAR10100_pgLoadTaxTrans.data.removeAt(i);
    }
    calcTaxTotal();
};
function delTax(index) {
    //if (App.cboStatus != "H" ) return false;
    var lineRef = App.stoAR_Trans.data.items[index].data.LineRef;

    for (var j = App.stoAR10100_pgLoadTaxTrans.data.length - 1; j >= 0; j--) {
        if (App.stoAR10100_pgLoadTaxTrans.data.items[j].data.LineRef == lineRef)
            App.stoAR10100_pgLoadTaxTrans.data.removeAt(j);
    }
    clearTax(index);
    calcTaxTotal();
    calcDet();
    return true;

}
function clearTax(index) {
    App.stoAR_Trans.data.items[index].set('TaxId00', '');
    App.stoAR_Trans.data.items[index].set('TaxAmt00', 0);
    App.stoAR_Trans.data.items[index].set('TxblAmt00', 0);

    App.stoAR_Trans.data.items[index].set('TaxId01', '');
    App.stoAR_Trans.data.items[index].set('TaxAmt01', 0);
    App.stoAR_Trans.data.items[index].set('TxblAmt01', 0);

    App.stoAR_Trans.data.items[index].set('TaxId02', '');
    App.stoAR_Trans.data.items[index].set('TaxAmt02', 0);
    App.stoAR_Trans.data.items[index].set('TxblAmt02', 0);

    App.stoAR_Trans.data.items[index].set('TaxId03', '');
    App.stoAR_Trans.data.items[index].set('TaxAmt03', 0);
    App.stoAR_Trans.data.items[index].set('TxblAmt03', 0);
}
function calcTax(index) {

    var det = App.stoAR_Trans.data.items[index].data;
    var record = App.stoAR_Trans.data.items[index];
    if (index < 0) return true;


    var dt = [];
    if (det.TaxID == "*") {
        for (var j = 0; j < App.cboTaxID.getStore().data.length; j++) {
            var item = App.cboTaxID.getStore().data.items[j];
            dt.push(item.data);
        };
    }
    else {
        var strTax = det.TaxID.split(',');
        if (strTax.length > 0) {
            for (var k = 0; k < strTax.length; k++) {
                for (var j = 0; j < App.cboTaxID.getStore().data.length; j++) {
                    if (strTax[k] == App.cboTaxID.getStore().data.items[j].data.TaxID) {
                        dt.push(App.cboTaxID.getStore().data.items[j].data);
                        break;
                    }
                }
            }
        }
        else {
            if (Ext.isEmpty(det.TaxID) || Ext.isEmpty(det.TaxCat))
                App.stoAR_Trans.data.items[i].set('TxblAmt00', det.TranAmt);
            return false;
        }
    }

    var taxCat = det.TaxCat;
    var prcTaxInclRate = 0, totPrcTaxInclAmt = 0, txblAmtL1 = 0, txblAmtAddL2 = 0;
    for (var j = 0; j < dt.length; j++) {
        var objTax = HQ.store.findInStore(App.stoAR10100_pdSI_Tax, ['TaxID'], [dt[j].TaxID]);
        if (!Ext.isEmpty(objTax) && !Ext.isEmpty(taxCat)) {
            if (taxCat == "*" || (objTax.CatFlg == "A" && objTax.CatExcept00 != taxCat && objTax.CatExcept01 != taxCat
                                                       && objTax.CatExcept02 != taxCat && objTax.CatExcept03 != taxCat
                                                       && objTax.CatExcept04 != taxCat && objTax.CatExcept05 != taxCat)
                              || (objTax.CatFlg == "N" && (objTax.CatExcept00 == taxCat || objTax.CatExcept01 == taxCat ||
                                                            objTax.CatExcept02 == taxCat || objTax.CatExcept03 == taxCat ||
                                                            objTax.CatExcept04 == taxCat || objTax.CatExcept05 == taxCat))) {

                if (objTax.TaxCalcLvl == "1" && objTax.PrcTaxIncl != "0") {
                    prcTaxInclRate = prcTaxInclRate + objTax.TaxRate;
                }
            }
        }
    }


    if (prcTaxInclRate == 0)
        txblAmtL1 = Math.round(det.TranAmt, 0);
    else
        txblAmtL1 = Math.round((det.TranAmt) / (1 + prcTaxInclRate / 100), 0);


    record.set('TxblAmt00', txblAmtL1);

    for (var j = 0; j < dt.length; j++) {

        var TaxID = "", lineRef = "";
        var taxRate = 0, taxAmtL1 = 0;
        var objTax = HQ.store.findInStore(App.stoAR10100_pdSI_Tax, ['TaxID'], [dt[j].TaxID]);
        if (!Ext.isEmpty(objTax) && !Ext.isEmpty(taxCat)) {
            if (taxCat == "*" || (objTax.CatFlg == "A" && objTax.CatExcept00 != taxCat && objTax.CatExcept01 != taxCat
                                                       && objTax.CatExcept02 != taxCat && objTax.CatExcept03 != taxCat
                                                       && objTax.CatExcept04 != taxCat && objTax.CatExcept05 != taxCat)
                              || (objTax.CatFlg == "N" && (objTax.CatExcept00 == taxCat || objTax.CatExcept01 == taxCat ||
                                                            objTax.CatExcept02 == taxCat || objTax.CatExcept03 == taxCat ||
                                                            objTax.CatExcept04 == taxCat || objTax.CatExcept05 == taxCat))) {
                if (objTax.TaxCalcLvl == "1") {
                    TaxID = dt[j].TaxID;
                    lineRef = det.LineRef;
                    taxRate = objTax.TaxRate;
                    taxAmtL1 = HQ.util.mathRound(txblAmtL1 * objTax.TaxRate / 100, 2);//Math.round(txblAmtL1 * objTax.TaxRate / 100, 2);

                    if (objTax.Lvl2Exmpt == 0) txblAmtAddL2 += txblAmtL1;

                    if (objTax.PrcTaxIncl != "0") {
                        var chk = false;
                        if (j < dt.length - 1) {
                            for (var k = j + 1; k < dt.length; k++) {
                                objTax = dt[k];
                                if (!Ext.isEmpty(objTax) && !Ext.isEmpty(taxCat)) {
                                    if (taxCat == "*" || (objTax.CatFlg == "A" && objTax.CatExcept00 != taxCat &&
                                                            objTax.CatExcept01 != taxCat && objTax.CatExcept02 != taxCat &&
                                                            objTax.CatExcept03 != taxCat && objTax.CatExcept04 != taxCat &&
                                                            objTax.CatExcept05 != taxCat)
                                                      || (objTax.CatFlg == "N" && (objTax.CatExcept00 == taxCat || objTax.CatExcept01 == taxCat ||
                                                                                    objTax.CatExcept02 == taxCat || objTax.CatExcept03 == taxCat ||
                                                                                    objTax.CatExcept04 == taxCat || objTax.CatExcept05 == taxCat))) {
                                        if (objTax.TaxCalcLvl == "1" && objTax.PrcTaxIncl != "0") {
                                            chk = false;
                                            break;
                                        }
                                    }
                                }
                                chk = true;
                            }
                        }
                        else {
                            chk = true;
                        }

                        if (chk) {

                            if (totPrcTaxInclAmt + taxAmtL1 + txblAmtL1 != det.TranAmt)
                                taxAmtL1 = HQ.util.mathRound(det.TranAmt - (totPrcTaxInclAmt + txblAmtL1), 2); //Math.round(det.TranAmt - (totPrcTaxInclAmt + txblAmtL1), 2);

                        }
                        else
                            totPrcTaxInclAmt += totPrcTaxInclAmt + taxAmtL1;
                    }

                    insertUpdateTax(TaxID, lineRef, taxRate, taxAmtL1, txblAmtL1, 1);

                }
            }
        }
    }

    for (var j = 0; j < dt.Count; j++) {
        var TaxID = "", lineRef = "";
        var taxRate = 0, txblAmtL2 = 0, taxAmtL2 = 0;
        var objTax = HQ.store.findInStore(App.stoAR10100_pdSI_Tax, ['TaxID'], [dt[j].TaxID]);
        if (!Ext.isEmpty(objTax) && !Ext.isEmpty(taxCat)) {
            if (taxCat == "*" || (objTax.CatFlg == "A" && objTax.CatExcept00 != taxCat && objTax.CatExcept01 != taxCat
                                                       && objTax.CatExcept02 != taxCat && objTax.CatExcept03 != taxCat
                                                       && objTax.CatExcept04 != taxCat && objTax.CatExcept05 != taxCat)
                              || (objTax.CatFlg == "N" && (objTax.CatExcept00 == taxCat || objTax.CatExcept01 == taxCat ||
                                                            objTax.CatExcept02 == taxCat || objTax.CatExcept03 == taxCat ||
                                                            objTax.CatExcept04 == taxCat || objTax.CatExcept05 == taxCat))) {
                if (objTax.TaxCalcLvl == "2") {
                    TaxID = dt[j].TaxID;
                    lineRef = det.LineRef;
                    taxRate = objTax.TaxRate;
                    txblAmtL2 = Math.round(txblAmtAddL2 + txblAmtL1, 0);
                    taxAmtL2 = HQ.util.mathRound(txblAmtAddL2 * objTax.TaxRate / 100, 2);//Math.round(txblAmtAddL2 * objTax.TaxRate / 100, 2);
                    insertUpdateTax(TaxID, lineRef, taxRate, taxAmtL2, txblAmtL2, 2);
                }
            }
        }
    }
    updateTax(index);
    calcDet();
    return true;
}
function insertUpdateTax(TaxID, lineRef, taxRate, taxAmt, txblAmt, taxLevel) {
    var flat = false;
    for (var i = 0; i < App.stoAR10100_pgLoadTaxTrans.data.length; i++) {
        if (App.stoAR10100_pgLoadTaxTrans.data.items[i].data.TaxID == TaxID && App.stoAR10100_pgLoadTaxTrans.data.items[i].data.LineRef == lineRef) {
            var tax = App.cboTaxID.getStore().data.items[i];
            tax.set('BranchID', App.txtBranchID.getValue()),
            tax.set('TaxID', TaxID);
            tax.set('LineRef', lineRef);
            tax.set('TaxRate', taxRate);
            tax.set('TaxLevel', taxLevel.toString());
            tax.set('TaxAmt', taxAmt)
            tax.set('TxblAmt', txblAmt);
            flat = true;
            break;
        }
    }
    if (!flat) {
        var newTax = Ext.create('App.ModelAR10100_pgLoadTaxTrans_Result');
        newTax.data.BranchID = App.txtBranchID.getValue();
        newTax.data.TaxID = TaxID;
        newTax.data.LineRef = lineRef;
        newTax.data.TaxRate = taxRate;
        newTax.data.TaxLevel = taxLevel.toString();
        newTax.data.TaxAmt = taxAmt;
        newTax.data.TxblAmt = txblAmt;

        App.stoAR10100_pgLoadTaxTrans.data.add(newTax);
    }
    App.stoAR10100_pgLoadTaxTrans.sort('LineRef', "ASC");
    calcDet();

}
function updateTax(index) {

    if (index < 0) return;
    var j = 0;
    var det = App.stoAR_Trans.data.items[index].data;
    var record = App.stoAR_Trans.data.items[index];
    for (var i = 0; i < App.stoAR10100_pgLoadTaxTrans.data.length; i++) {
        var item = App.stoAR10100_pgLoadTaxTrans.data.items[i];
        if (item.data.LineRef == det.LineRef) {
            if (j == 0) {
                record.set('TaxId00', item.data.TaxID);
                record.set('TxblAmt00', item.data.TxblAmt);
                record.set('TaxAmt00', item.data.TaxAmt);
            }
            else if (j == 1) {
                record.set('TaxId01', item.data.TaxID);
                record.set('TxblAmt01', item.data.TxblAmt);
                record.set('TaxAmt01', item.data.TaxAmt);
            }
            else if (j == 2) {
                record.set('TaxId02', item.data.TaxID);
                record.set('TxblAmt02', item.data.TxblAmt);
                record.set('TaxAmt02', item.data.TaxAmt);
            }
            else if (j == 3) {
                record.set('TaxId03', item.data.TaxID);
                record.set('TxblAmt03', item.data.TxblAmt);
                record.set('TaxAmt03', item.data.TaxAmt);
            }
            j++;
        }
        if (j != 0 && item.data.LineRef != det.LineRef)
            return false;
    };

}
function calcTaxTotal() {
    App.stoAR10100_LoadTaxDoc.clearData();
    var flat = false;
    for (var i = 0; i < App.stoAR10100_pgLoadTaxTrans.data.length; i++) {
        var tax = App.stoAR10100_pgLoadTaxTrans.data.items[i];
        flat = true;
        for (var j = 0; j < App.stoAR10100_LoadTaxDoc.data.length; j++) {
            var taxDoc = App.stoAR10100_LoadTaxDoc.data.items[j];
            if (tax.data.PONbr == taxDoc.data.PONbr && tax.data.TaxID == taxDoc.data.TaxID) {
                taxDoc.data.TxblAmt += tax.data.TxblAmt;
                taxDoc.data.TaxAmt += tax.data.TaxAmt;
                flat = false;
                taxDoc.commit();
                break;
            }
        };
        if (flat) {
            var newTaxDoc = Ext.create('App.mdlAR10100_pgLoadTaxTransDoc');
            newTaxDoc.data.BranchID = tax.data.BranchID;
            newTaxDoc.data.BatNbr = tax.data.BatNbr;
            newTaxDoc.data.RefNbr = tax.data.RefNbr;
            newTaxDoc.data.TaxID = tax.data.TaxID;
            newTaxDoc.data.TaxAmt = tax.data.TaxAmt;
            newTaxDoc.data.TaxRate = tax.data.TaxRate;
            newTaxDoc.data.TxblAmt = tax.data.TxblAmt;

            App.stoAR10100_LoadTaxDoc.data.add(newTaxDoc);
            // newTaxDoc.commit();
        }

    };
    App.grdTaxTrans.getView().refresh(false);
    App.grdTaxDoc.getView().refresh(false);

}
