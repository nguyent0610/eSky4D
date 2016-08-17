var keysTab_2 = ['DocDate', 'InvcNbr', 'BatNbr', 'BranchID', 'RefNbr', 'VendID', 'Name'];
var fieldsCheckRequireTab_2 = ['VendID', 'Descr'];
var fieldsLangCheckRequireTab_2 = ['VendID', 'Descr'];

var BatNbr = '';
var _Source = 0;
var _maxSource = 2;
var _isLoadMaster = false;

var tmpApplyAmount = 0;
//3 bien tam grid 2
var tmpVendorBalance = 0;
var tmpApplicationTotal = 0;
var tmplUnApplyTotal = 0;
//var autoApplyOnActive = 0;
var tmpAutoApplyForTotalAmountAndPayment = 0;
var tmpPayment = 0;

var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        App.txtBranchID.setValue(HQ.cpnyID);
        _isLoadMaster = true;
        _Source = 0;
        App.stoHeader.reload();
        HQ.common.showBusy(false);
      
    }
};

var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboBatNbr.getStore().addListener('load', checkLoad);
    App.cboStatus.getStore().addListener('load', checkLoad);
   // App.cboDocType.getStore().addListener('load', checkLoad);
    //App.cboCustId.getStore().addListener('load', checkLoad);
   // App.cboDebtCollector.getStore().addListener('load', checkLoad);
    //App.cboBankAcct.getStore().addListener('load', checkLoad);
   
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus  == 'header') {
                HQ.combo.first(App.cboBatNbr, HQ.isChange);
            }
            else if (HQ.focus  == 'pnlDetail') {
                HQ.grid.first(App.grdDetail);
            }
            break;
        case "prev":
            if (HQ.focus  == 'header') {
                HQ.combo.prev(App.cboBatNbr, HQ.isChange);
            }
            else if (HQ.focus  == 'pnlDetail') {
                HQ.grid.prev(App.grdDetail);
            }
            break;
        case "next":
            if (HQ.focus == 'header') {
                HQ.combo.next(App.cboBatNbr, HQ.isChange);
            }
            else if (HQ.focus  == 'pnlDetail') {
                HQ.grid.next(App.grdDetail);
            }
            break;
        case "last":
            if (HQ.focus  == 'header') {
                HQ.combo.last(App.cboBatNbr, HQ.isChange);
            }
            else if (HQ.focus  == 'pnlDetail') {
                HQ.grid.last(App.grdDetail);
            }
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                refresh("yes");
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.focus  == 'header') {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', 'refresh');
                    } else {
                        BatNbr = '';
                        App.cboBatNbr.setValue('');
                    }
                }
                else if (HQ.focus  == 'pnlDetail') {
                    HQ.grid.insert(App.grdDetail);
                }
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.cboStatus.getValue() == 'H') {
                    if (HQ.focus == 'header') {
                        if (App.cboBatNbr.value) {
                            HQ.message.show(11, '', 'deleteData');
                        } else {
                            menuClick('new');
                        }
                    }
                    else if (HQ.focus == 'pnlDetail') {
                        if (App.slmDetail.selected.items[0] != undefined) {
                            //   if (App.slmDetail.selected.items[0].data.Payment != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdDetail)], 'deleteData', true);
                            // }
                        }
                    }
                }
                else {
                    HQ.message.show(2016080802, App.cboBatNbr.getValue());
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain))
                //    && HQ.store.checkRequirePass(App.stoDetail, keysTab_2, fieldsCheckRequireTab_2, fieldsLangCheckRequireTab_2)
                {
                    //if (checkGrid(App.stoDetail,'Payment')==true) {
                        save();
                    //}
                    //else {
                    //    HQ.message.show(2015123110, HQ.common.getLang('Grid'));
                    //}
                }
            }
            break;
        case "print":
            break;
        case "close":
            break;
    }
};

var checkGrid = function (store, field) {
    var count = 0;
    var allRecords = store.snapshot || store.allData || store.data;
    allRecords.each(function (record) {
        if (record.data[field]) {
            count++;
            return false;
        }
    });
    if (count > 0)
        return true;
    else
        return false;
};

var AdjustedCheckAll_Change = function (value) {
    //var colIdx = 3;
    if (value) {
        App.grdDetail.getStore().each(function (item) {
            //vong if nay de check neu 1 so o da duoc check hay bi bo ko dong bo voi cac o con lai
            if ((value.checked == true && this.data.Selected == false) || (value.checked == false && this.data.Selected == true)) {
                item.set("Selected", value.checked);
                AdjustedCheckEveryRow_Change(0, 0, item);
            }

        });

    }
};

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
    for (var i = 0; i < App.stoDetail.data.length; i++) {
        tmpApplicationTotal += App.stoDetail.data.items[i].data.Payment;
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


var cboInvcNbr_change = function (item, newValue, oldValue) {
    for (var i = 0; i < App.cboInvcNbr.store.data.length; i++) {
        if (App.cboInvcNbr.getStore().data.items[i].data.InvcNbr == newValue) {
            App.slmDetail.selected.items[0].set('DocDate', App.cboInvcNbr.getStore().data.items[i].data.DocDate);
            App.slmDetail.selected.items[0].set('DocBal', App.cboInvcNbr.getStore().data.items[i].data.DocBal);
            App.slmDetail.selected.items[0].set('OrigDocBal', App.cboInvcNbr.getStore().data.items[i].data.OrigDocBal);
            App.slmDetail.selected.items[0].set('VendID', App.cboInvcNbr.getStore().data.items[i].data.VendID);
            App.slmDetail.selected.items[0].set('Descr', App.cboInvcNbr.getStore().data.items[i].data.Descr);
            App.slmDetail.selected.items[0].set('BranchID', App.cboInvcNbr.getStore().data.items[i].data.BranchID);
            App.slmDetail.selected.items[0].set('DocType', App.cboInvcNbr.getStore().data.items[i].data.DocType);

            App.slmDetail.selected.items[0].set('Name', App.cboInvcNbr.getStore().data.items[i].data.Name);
            App.slmDetail.selected.items[0].set('BatNbr', App.cboInvcNbr.getStore().data.items[i].data.BatNbr);
            App.slmDetail.selected.items[0].set('RefNbr', App.cboInvcNbr.getStore().data.items[i].data.RefNbr);
        }
    }
    //load so lieu tu grid len Form Bot va Top
    waitstoreGridReload();
};

//load du lieu tu grid len Form Bot va Top
var waitstoreGridReload = function () {
    for (var i = 0; i < App.stoDetail.data.length; i++) {
        tmpVendorBalance += App.stoDetail.data.items[i].data.OrigDocBal;
        tmpApplicationTotal += App.stoDetail.data.items[i].data.Payment;
        tmplUnApplyTotal += App.stoDetail.data.items[i].data.DocBal;
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
    for (var i = 0; i < App.stoDetail.data.length; i++) {
        //neu dong dang chon bang voi data trong store cua hang do thi xu ly
        if (App.slmDetail.selected.items[0].data == App.stoDetail.data.items[i].data) {
            //dk neu gia tri moi edit cua payment lon hon gia tri original cua hang do thi set payment = gia tri original , set docbal = 0 va check vao checkbox hang do 
            if (newValue < 0)
                newValue = 0;
            if (newValue >= App.stoDetail.data.items[i].data.OrigDocBal) {
                App.stoDetail.data.items[i].set("Payment", App.stoDetail.data.items[i].data.OrigDocBal);
                App.stoDetail.data.items[i].set("DocBal", 0);
                App.stoDetail.data.items[i].set("Selected", true)
            } else {// nguoc lai 
                App.stoDetail.data.items[i].set("Payment", newValue);
                App.stoDetail.data.items[i].set("DocBal", App.stoDetail.data.items[i].data.OrigDocBal - newValue);
                App.stoDetail.data.items[i].set("Selected", false)
            }
        }
        tmpApplicationTotal += App.stoDetail.data.items[i].data.Payment;
    }
    App.txtPaid.setValue(tmpApplicationTotal);
    App.txtUnTotPayment.setValue(App.txtOrigDocAmt.value - App.txtPaid.value);
    App.txtCuryCrTot.setValue(App.txtPaid.value);
    tmpApplicationTotal = 0;
};

var cboBatNbr_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoHeader.loading) {
        BatNbr = value;
        App.stoHeader.reload();
    }
};

var cboBatNbr_Select = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoHeader.loading) {
        BatNbr = value;
        App.stoHeader.reload();
    }
};


var btnSearch_Click = function () {
    if (App.dteFromDate.getValue() != null && App.dteToDate.getValue() != null && App.dteFromDate.isValid() && App.dteToDate.isValid()) {
        if (App.dteFromDate.isValid() && App.dteToDate.isValid()) {
            App.cboInvcNbr.getStore().reload();
            App.stoDetail.reload();
        }
    } else {
        if (App.dteFromDate.getValue()==null)
            HQ.message.show(15, HQ.common.getLang('FromDate'), '');
        else
            HQ.message.show(15, HQ.common.getLang('ToDate'), '');
    }
};

var txtFromDate_Change = function () {
    if (App.dteFromDate.getValue() != null && App.dteToDate.getValue() != null) {
        if (App.dteFromDate.isValid()) {
            App.cboInvcNbr.getStore().reload();
            App.stoDetail.reload();
        }
    }
    //else {
    //    HQ.message.show(15, HQ.common.getLang('FromDate'), '');
    //}
}

var txtToDate_Change = function () {
    if (App.dteFromDate.getValue() != null && App.dteToDate.getValue() != null) {
        if (App.dteToDate.isValid()) {
            App.cboInvcNbr.getStore().reload();
            App.stoDetail.reload();
        }
    }
    //else {
    //    HQ.message.show(15, HQ.common.getLang('ToDate'), '');
    //}
}

var cboVendID_Change = function () {
    App.cboInvcNbr.getStore().reload();
    App.stoDetail.reload();

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
        for (var i = 0; i < App.stoDetail.data.length; i++) {
            App.stoDetail.data.items[i].set("Payment", 0);
            App.stoDetail.data.items[i].set("DocBal", App.stoDetail.data.items[i].data.OrigDocBal);
            App.stoDetail.data.items[i].set("Selected", false)
        }
        App.txtCuryCrTot.setValue(App.txtPayment.value);
        App.txtOdd.setValue(0);
        tmpAutoApplyForTotalAmountAndPayment = App.txtCuryCrTot.value; // dat bien tam cho de xai de hieu sau nay quyet dinh xoa hay ko sau
        //vong for duyet trong storegrid 2 de di qua hang nao thi xu ly hang do trong grid 2 neu TotalAmount lon hon tung hang trong grid 2
        for (var i = 0; i < App.stoDetail.data.length; i++) {
            if (tmpAutoApplyForTotalAmountAndPayment != 0) {
                if (tmpAutoApplyForTotalAmountAndPayment < App.stoDetail.data.items[i].data.OrigDocBal) {
                    App.stoDetail.data.items[i].set("Payment", tmpAutoApplyForTotalAmountAndPayment);
                    App.stoDetail.data.items[i].set("DocBal", App.stoDetail.data.items[i].data.OrigDocBal - tmpAutoApplyForTotalAmountAndPayment);
                    tmpAutoApplyForTotalAmountAndPayment -= App.stoDetail.data.items[i].data.OrigDocBal;
                    //giam den 1 luc nao do no se - cho cai original thanh 0 luon thi se ket thuc ko lam dang tiep theo do gia tri bien tam = 0 roi
                    if (tmpAutoApplyForTotalAmountAndPayment < 0) {
                        tmpAutoApplyForTotalAmountAndPayment = 0;
                    }
                } else { // tmpAutoApplyForTotalAmountAndPayment >= App.storeGrid2.data.items[i].data.OrigDocBal
                    //theo thu tu set lai gia tri payment la original, docbal thanh 0 , check ca vao o checkbox, giam bien tam = bien tam - gia tri original
                    App.stoDetail.data.items[i].set("Payment", App.stoDetail.data.items[i].data.OrigDocBal);
                    App.stoDetail.data.items[i].set("DocBal", 0);
                    App.stoDetail.data.items[i].set("Selected", true)
                    tmpAutoApplyForTotalAmountAndPayment -= App.stoDetail.data.items[i].data.OrigDocBal;

                }
                tmpApplicationTotal += App.stoDetail.data.items[i].data.Payment;
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
        for (var i = 0; i < App.stoDetail.data.length; i++) {
            App.stoDetail.data.items[i].set("Payment", App.stoDetail.data.items[i].data.OrigDocBal);
            App.stoDetail.data.items[i].set("DocBal", 0);
            App.stoDetail.data.items[i].set("Selected", true)
        }
        App.txtPaid.setValue(App.txtOrigDocAmt.value);
        App.txtUnTotPayment.setValue(0);
        App.txtPayment.setValue(0);
    }

};

var frmChange = function () {
    if (App.stoHeader.getCount() > 0) {
        App.frmMain.getForm().updateRecord();
    }
    HQ.isChange = HQ.store.isChange(App.stoHeader) == false ? (HQ.store.isChange(App.stoDetail)): true;
    HQ.common.changeData(HQ.isChange, 'AP10400');
    if (App.cboBatNbr.valueModels == null || HQ.isNew == true) {
        App.cboBatNbr.setReadOnly(false);
    }
    else {
        App.cboBatNbr.setReadOnly(HQ.isChange);
    }
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoLoad = function (sto) {
    HQ.isNew = false;
    HQ.common.setForceSelection(App.frmMain, false, "cboBatNbr");
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "BatNbr");
        record = sto.getAt(0);
        record.set('Status', 'H');
        record.set('DocType', 'CC');
        record.set('DocDate', HQ.bussinessDate);
        if (App.cboBankAcct.store.data.items[0] != undefined) {
            record.set("ReasonCD", App.cboBankAcct.store.data.items[0].data.BankAcct);
        }
        HQ.isNew = true;
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboBatNbr.focus(true);//focus ma khi tao moi
        sto.commitChanges();
       // total();

    }
    // App.cboDateType.setValue('0');//.SelectedItems(new Ext.Net.ListItem { Index = 0})
   
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);
   
    App.stoDetail.reload();
    
    if (record.data.Status == 'H')
        HQ.common.lockItem(App.frmMain, false);
    else
        HQ.common.lockItem(App.frmMain, true);

    if (!HQ.isInsert && HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }
    App.txtOdd.setReadOnly(true);
    App.txtUnTotPayment.setReadOnly(true);
   
};

//var lockControl = function (value) {
//    //setTimeout(function () {
//        App.cboVendor.setReadOnly(value);
//        App.cboCash.setReadOnly(value);
//        App.txtDescr.setReadOnly(value);
//        App.lblPODate.setReadOnly(value)
//    //}, 300);
//};

/////////////////////////////// GIRD AP_Trans /////////////////////////////////
var stoDetail_Load = function (sto) {
    if (App.dteFromDate.getValue() > App.dteToDate.getValue()) {
        HQ.message.show(2015061501, '', '');
    }
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
          //  HQ.grid.insert(App.grdDetail);
            HQ.store.insertBlank(sto, keysTab_2);
            var record = App.stoDetail.getAt(App.stoDetail.getCount() - 1);
            record.set('DocDate', HQ.bussinessDate);

        }
        HQ.isFirstLoad = false; //sto load cuoi se su dung
        
    }
    else
    {
        if (App.stoDetail.getCount() > 0) {
            var record = App.stoDetail.getAt(App.stoDetail.getCount() - 1);
            if (record.data.InvcNbr != "") {
                HQ.store.insertBlank(sto, keysTab_2);
                record = sto.getAt(sto.getCount() - 1);
                record.set('DocDate', HQ.bussinessDate);
            }
        }
        else {
            HQ.store.insertBlank(sto, keysTab_2);
            var record = App.stoDetail.getAt(0);
            if (record.data.InvcNbr != "") {
                HQ.store.insertBlank(sto, keysTab_2);
                record = sto.getAt(sto.getCount() - 1);
                record.set('DocDate', HQ.bussinessDate);
            }
        }
    }

    frmChange();
   // if(HQ.isChange)
    total();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

var grdDetail_BeforeEdit = function (editor, e) {
    if (App.cboStatus.getValue() != 'H') return false;
    if (!HQ.grid.checkBeforeEdit(e, keysTab_2)) return false;
};

var grdDetail_Edit = function (item, e) {
    if (e.field == 'Payment') {
      
        if (!Ext.isEmpty(e.value)) {
            e.record.set('LineRef', HQ.store.lastLineRef(App.stoDetail));
            total();
        }
    }
    //Kiem tra cac key da duoc nhap se insert them dong moi
    HQ.grid.checkInsertKey(App.grdDetail, e, keysTab_2);
    frmChange();
};

var grdDetail_ValidateEdit = function (item, e) {
    //ko cho nhap key co ki tu dac biet, va kiem tra trung du lieu
    return HQ.grid.checkValidateEdit(App.grdDetail, e, keysTab_2,false);
};

var grdDetail_Reject = function (record) {
    //reject dong thay doi du lieu ve ban dau
    HQ.grid.checkReject(record, App.grdDetail);
    frmChange();
};

//Process
var save = function () {
    if (App.stoDetail.data.length == 0) {
        HQ.message.show(1000, HQ.common.getLang('pnlDetail'), '');
        return false;
    }
    else {
        var store = App.stoDetail;
        var allRecords = store.snapshot || store.allData || store.data;
        var totalOrigDocBal = 0;
        var totaDocBal = 0;
        var totalPayment = 0;
        allRecords.each(function (record) {
            if (record.data.Selected) {
                if (record.data.OrigDocBal) {
                    totalOrigDocBal += record.data.OrigDocBal;
                }
                if (record.data.DocBal) {
                    totaDocBal += record.data.DocBal;
                }
                if (record.data.Payment) {
                    totalPayment += record.data.Payment;
                }
            }
        });
        
        App.txtPaid.setValue(totalPayment);
        App.txtOrigDocAmt.setValue(totalOrigDocBal);
        App.txtUnTotPayment.setValue(totaDocBal);
    }
    if (App.frmMain.isValid() && App.stoDetail.data.length>0) {
        App.frmMain.updateRecord();
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            timeout: 18000,
            url: 'AP10400/Save',
            params: {
                lstHeader: Ext.encode(App.stoHeader.getRecordsValues()),
                lstgrd: HQ.store.getData(App.stoDetail),
                lstAp_Adjust:Ext.encode(App.stoDetail.getRecordsValues())
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                BatNbr = data.result.BatNbr;
               
                HQ.isFirstLoad = true;
                App.cboHandle.setValue('');
                App.cboBatNbr.getStore().load({
                    callback: function () {
                        if (Ext.isEmpty(App.cboBatNbr.getValue())) {
                            App.cboBatNbr.setValue(BatNbr);
                            HQ.isFirstLoad = true;
                            App.cboBatNbr.getStore().load();
                            App.stoHeader.reload();
                        }
                        else {
                            App.cboBatNbr.setValue(BatNbr);
                            HQ.isFirstLoad = true;
                            App.cboBatNbr.getStore().load();
                            App.stoHeader.reload();
                        }
                        App.cboInvcNbr.store.reload();
                    }
                });
                //total();
                HQ.isChange = false;
                HQ.common.changeData(HQ.isChange, 'AP10400');
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        if (HQ.focus == 'header') {
            if (App.frmMain.isValid()) {
                App.frmMain.updateRecord();
                App.frmMain.submit({
                    waitMsg: HQ.common.getLang("DeletingData"),
                    url: 'AP10400/DeleteAll',
                    timeout: 7200,
                    success: function (msg, data) {
                        App.cboBatNbr.getStore().load();
                        menuClick("new");
                    },
                    failure: function (msg, data) {
                        HQ.message.process(msg, data, true);
                    }
                });
            }

        }
        else if (HQ.focus == 'pnlDetail') {
            App.grdDetail.deleteSelected();
            frmChange();
            total();
        }
    }
};

function refresh(item) {
    if (item == 'yes') {
        if (HQ.isNew) {
            BatNbr = ''
            App.cboBatNbr.setValue('');
        }
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoHeader.reload();
    }
};

var total = function () {
    //var totalAmt = 0;
    var store = App.stoDetail;
    var allRecords = store.snapshot || store.allData || store.data;
    var totalOrigDocBal = 0;
    var totaDocBal = 0;
    var totalPayment = 0;
    allRecords.each(function (record) {
        if (record.data.OrigDocBal) {
            totalOrigDocBal += record.data.OrigDocBal;
        }
        if (record.data.DocBal) {
            totaDocBal += record.data.DocBal;
        }
        if (record.data.Payment) {
            totalPayment += record.data.Payment;
        }
    });
    // App.txtCuryCrTot.setValue(totalAmt);
    App.txtPaid.setValue(totalPayment);
   // App.txtOrigDocAmt.setValue(totalOrigDocBal);
    App.txtOrigDocAmt.setValue(totaDocBal + totalPayment);
    App.txtUnTotPayment.setValue(totaDocBal);
   // App.txtCuryDocBal.setValue(totalAmt);
};

var btnPopupOk_Click = function () {
    if (!Ext.isEmpty(App.cboPopupCpny.getValue())) {
        App.winPopup.hide();
        window.location.href = 'AP10400?branchID=' + App.cboPopupCpny.getValue();
    } else {
        HQ.message.show(1000, [HQ.common.getLang('branchid')], '', true);
    }
};
