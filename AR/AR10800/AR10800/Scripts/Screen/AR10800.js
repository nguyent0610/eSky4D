
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
var custIDAndName = "";
var taxAmt00 = 0;
var taxAmt01 = 0;
var taxAmt02 = 0;
var taxAmt03 = 0;
var taxAmt = [taxAmt00, taxAmt01, taxAmt02, taxAmt03];
var txblAmt = 0;
var countTaxAmt = 0;
var tmpChangeForm1OrForm2 = "0";











//AR10800 bien tam

var tmpTranAmt = 0;
//AR10800 bien tam
var tmpLoadGrid = 0;




var menuClick = function (command) {
    switch (command) {
        case "first":
            if (_focusrecord == 3) {

                var combobox = App.cboInvcNbr;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboInvcNbr.setValue(combobox.store.getAt(0).data.InvcNbr);

            } else if (_focusrecord == 2) {
                App.SelectionRowOnGrid.select(0);
            }
            
            break;
        case "prev":
            if (_focusrecord == 3) {

                var combobox = App.cboInvcNbr;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboInvcNbr.setValue(combobox.store.getAt(index - 1).data.InvcNbr);

            } else if (_focusrecord == 2) {
                App.SelectionRowOnGrid.selectPrevious();
            }
            
            break;
        case "next":
            if (_focusrecord == 3) {
                            
                var combobox = App.cboInvcNbr;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboInvcNbr.setValue(combobox.store.getAt(index + 1).data.InvcNbr);
               
            } else if (_focusrecord == 2) {
                App.SelectionRowOnGrid.selectNext();
            }
            break;
        case "last":
            if (_focusrecord == 3) {

                var combobox = App.cboInvcNbr;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboInvcNbr.setValue(App.cboBatNbr.store.getAt(App.cboInvcNbr.store.getTotalCount() - 1).data.InvcNbr);

            } else if (_focusrecord == 2) {
                App.SelectionRowOnGrid.select(App.storeGrid.getCount() - 1);
            }
            

            
            break;
        case "refresh":
            App.storeFormTop.reload();
            //App.storeFormBot.reload();
            App.storeGrid.reload();
       
            break;
        case "new":
            if (isInsert) {
              
                
                 
             
                App.cboInvcNbr.setValue('');
                //App.storeFormTop.reload();
                   // App.dataFormTop.getForm().reset(true);
               
                    
                setTimeout(function () { waitcboInvcNbrForInsert(); }, 500);


                    setTimeout(function () { setStatusValueH(); }, 1500);
                    setTimeout(function () { waitcboStatusReLoad(); }, 1700);

 
                
                  
            }
            break;
        case "delete":
            var curRecord = App.dataForm.getRecord();
            if (isDelete) {
                if (App.cboInvcNbr.getValue() != "") {

                    if (_focusrecord == 3) {
                        callMessage(11, '', 'deleteRecordFormTopBatch');
                    } else if (_focusrecord == 2) {
                        callMessage(11, '', 'deleteRecordGrid')
                    } 
                }
            }
            break;
        case "save":
            if (isUpdate || isInsert || isDelete) {
              
                    if (isAllValid(App.storeGrid.getChangedData().Created)
                        && isAllValid(App.storeGrid.getChangedData().Updated)) {
                      
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

var waitcboInvcNbrForInsert = function () {

    //App.cboDocType.enable(true);
    App.grd.enable(true);
    var time = new Date();
    var record = Ext.create("App.AR_RedInvoiceDocModel", {
        //CustId: "",
        InvcNote: "",
        DocDate: time,
        CuryTaxAmt: 0,
        CuryTxblAmt: 0,
        DiscAmt: 0,
        CustID:"",
        SlsPerID:"",
        SOFee: 0,
        BranchID: App.txtBranchID.getValue(),
        TaxID: "",

        DocDesc: "",

    });
    App.storeFormTop.insert(0, record);
    App.dataFormTop.getForm().loadRecord(App.storeFormTop.getAt(0));
   // App.cboBankAcct.setValue(App.cboBankAcct.store.data.items[0].data.BankAcct);
   // App.txtOrigRefNbr.setValue("");

   // App.txtCuryCrTot.setValue(0);
    App.storeGrid.reload();
};

var setStatusValueH = function () {
    App.cboStatus.setValue("H");
    App.cboHandle.getStore().reload();
};

function Save() {

    var curRecord = App.dataFormTop.getRecord();
    App.dataFormTop.getForm().updateRecord();
    
    if (App.dataForm.isValid()) {
        App.dataForm.submit({
            waitMsg: 'Submiting...',
            url: 'AR10800/Save',
            params: {
                lstheaderTop: Ext.encode(App.storeFormTop.getChangedData({ skipIdForPhantomRecords: false })),//,
                
                lstgrd: Ext.encode(App.storeGrid.getChangedData({ skipIdForPhantomRecords: false })),
                InvcNbr: App.cboInvcNbr.getValue(),
                BranchID: App.txtBranchID.getValue(),
                Handle: App.cboHandle.getValue(),
                InvcNote: App.txtInvcNote.getValue(),
                


            },
            success: function (result, data) {
                callMessage(201405071, '', null);
                App.cboInvcNbr.getStore().reload();

                menuClick("refresh");
            }
            , failure: function (errorMsg, data) {
              
                var dt = Ext.decode(data.response.responseText);
                callMessage(dt.code, dt.colName + ',' + dt.value, null);
            }
        });
    } else {
        var fields = App.dataForm.getForm().getFields().each(
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
    if (App.dataForm.getRecord() != undefined) App.dataForm.updateRecord();
    if (App.storeGrid.getChangedData().Updated == undefined &&
        App.storeGrid.getChangedData().Deleted == undefined &&
        App.dataForm.getRecord() == undefined)
        parent.App.tabAR10800.close();
    else if (App.storeGrid.getChangedData().Updated != undefined ||
        App.storeGrid.getChangedData().Created != undefined ||
        App.storeGrid.getChangedData().Deleted != undefined ||
        storeIsChange(App.storeForm, false)) {
        callMessage(5, '', 'closeScreen');
    } else {
        parent.App.tabAR10800.close();
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
        if (parent.App.tabAR10800 != null)
            parent.App.tabAR10800.close();
    }
};
// Xac nhan xoa record cua form bot AR_Doc
var deleteRecordFormTopBatch = function (item) {
    if(item == "yes"){
        
        try {
            App.direct.DeleteFormTopAndGridIncluded(App.txtBranchID.getValue(), App.cboInvcNbr.getValue(), {
                success: function (data) {
                    //menuClick('refresh');
                    
                    App.cboInvcNbr.setValue('');
                    App.cboInvcNbr.getStore().reload();
                    menuClick("refresh");
                    App.dataFormTop.getForm().reset(true);
                    btnLoadData_Click();
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
var isAllValidKey=function(items){      
    if(items!=undefined){
        for(var i=0; i<items.length; i++){
            for(var j=0;j<strKeyGrid.length;j++)
            {
                if (items[i][strKeyGrid[j]] == '' || items[i][strKeyGrid[j]] == undefined)
                    return false;  
            }
        }
        return true;
    }else{
        return true;
    }
};

function selectRecord (grid, record) {
    var record = grid.store.getById(id);            
    grid.store.loadPage(grid.store.findPage(record), {
        callback : function () {
            grid.getSelectionModel().select(record);
        }
    });            
}

var grd_BeforeEdit = function (editor, e) {
    if (!isUpdate)
        return false;
    strKeyGrid = e.record.idProperty.split(',');
    if (strKeyGrid.indexOf(e.field) != -1) {
        //if (e.record.data.TranAmt != "") {
        //    return false;
        //}
    }

};

var grd_Edit = function (item, e) {
        
    if (strKeyGrid.indexOf(e.field) != -1) {
        if (e.value != '' && isAllValidKey(App.storeGrid.getChangedData().Created) && isAllValidKey(App.storeGrid.getChangedData().Updated)) {
            //App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());//Ext.data.Record() 
            //App.SelectionRowOnGrid.selected.items[0].set('TranDesc', custIDAndName);

            //var record = Ext.create("App.AP_TransResultModel", {
            //    //CustId: "",
            //    TranDesc: App.SelectionRowOnGrid.selected.items[0].data.TranDesc



            //});
            //App.storeGrid.insert(App.storeGrid.getCount(), record);
        }
    }
};

var grd_ValidateEdit = function (item, e) {
       
    if (strKeyGrid.indexOf(e.field) != -1)
    {
        //if (duplicated(App.storeGrid, e))
        //{
        //    callMessage(1112, e.value, '');
        //    App.SelectionRowOnGrid.selected.items[0].set('TranAmt', '');
        //    return false;
        //}
    }
    return true;
};
    
var grd_CancelEdit = function (editor, e) {
    if (e.record.phantom) {
        if (App.txtTranAmt.getValue == "")
        {
            e.store.remove(e.record);
        }
    }
};
var grd_Reject= function (record){
    if(record.data.tstamp=='')
    {         
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
        //var form
        App.dataFormTop.getForm().loadRecord(record);
        App.cboHandle.getStore().reload();
        //App.cboRefNbr.getStore().reload();
        //setTimeout(function () { waitStoreRefNbrReLoad(); }, 1000);

    }
};

var waitcboStatusReLoad = function () {
    if (App.cboStatus.value == "H") {
        //setReadOnly();
        App.cboHandle.setValue(App.cboHandle.store.data.items[0].data.Code);
    } else {
        //setReadOnly();
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
var cboInvcNbr_Change = function (sender,value) {
    App.cboHandle.setValue(App.cboHandle.store.data.items[0].data.Code);
    //App.cboVendID.enable(true);
    //App.cboDocType.enable(true);
    var combobox = App.cboInvcNbr;
 
    var record = combobox.findRecord(combobox.valueField || combobox.displayField, value);

    var index = combobox.store.indexOf(record);
    App.txtTmpLoadGrid.setValue("0");
    if (record != false) {
        App.txtTmpInvcNote.setValue(combobox.store.getAt(index).data.InvcNote);
        App.storeFormTop.reload();
        App.storeGrid.reload();
    }
   


    //if (App.cboInvcNbr.displayTplData[0] != undefined) {
    //    App.txtTmpInvcNote.setValue(App.cboInvcNbr.displayTplData[0].InvcNote);
    //    App.storeFormTop.reload();
    //    App.storeGrid.reload();
    //}



    //App.cboRefNbr.getStore().reload();
    //App.cboRefNbr.setValue(App.cboRefNbr.store.data.items[0].data.RefNbr);
    //setTimeout(function () { waitcboRefNbrReLoad(); }, 1200);
    //App.cboVendID.disable(true);
    //App.cboDocType.disable(true);
};



var loadDataAutoHeaderBot = function () {

    //if (App.storeFormBot.getCount() == 0) {
    //    App.storeFormBot.insert(0, Ext.data.Record());
    //}
    var record = App.storeFormBot.getAt(0);
    if (record) {
        App.dataFormBot.getForm().loadRecord(record);
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




var waitGridLoadAndFocusIntoBatNbr = function () {
    App.cboBatNbr.focus();
}


//doi form 2 tab 1 load xong
var waitStoreFormBotReLoad = function () {

    App.storeGrid.reload();


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
        //App.cboRefNbr.setReadOnly(true);

        App.txtOrigRefNbr.setReadOnly(true);
        App.txtDocDate.setReadOnly(true);
       
        App.txtInvcNbr.setReadOnly(true);
       
        App.cboBankAcct.setReadOnly(true);
        App.cboDebtCollector.setReadOnly(true);
        App.cboCustId.setReadOnly(true);
        App.txtDocDescr.setReadOnly(true);
        App.grd.disable(true);
    } else {
        App.cboHandle.setReadOnly(false);
        //App.cboRefNbr.setReadOnly(false);

        App.txtOrigRefNbr.setReadOnly(false);
        App.txtDocDate.setReadOnly(false);
        App.txtInvcNbr.setReadOnly(false);
        App.cboBankAcct.setReadOnly(false);
        App.cboDebtCollector.setReadOnly(false);
        App.cboCustId.setReadOnly(false);
        App.txtDocDescr.setReadOnly(false);
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
    var TranAmtRaw = App.SelectionRowOnGrid.selected.items[0].data.TranAmt;


    for (var k = 0; k < App.grd.store.data.length; k++) {
        if (App.grd.store.data.items[k].data.TranAmt == TranAmtRaw) {
            index = k;

        }
    }

    for (var i = 0; i < App.storeGrid.data.length; i++) {
        App.SelectionRowOnGrid.select(i);
        TotalTranAmt = TotalTranAmt + App.SelectionRowOnGrid.selected.items[0].data.TranAmt;
    }
    TotalTranAmt = TotalTranAmt - TranAmtRaw;
    //set lai gia tri cua 3 o kia
    App.txtCuryCrTot.setValue(TotalTranAmt);
    App.txtCuryOrigDocAmt.setValue(TotalTranAmt);
    App.txtCuryDocBal.setValue(TotalTranAmt);

        App.SelectionRowOnGrid.select(index);
        App.grd.editingPlugin.startEditByPosition({ row: index, column: 1 });



};
var waitSelectRowInGrid = function (index) {
    App.grd.editingPlugin.startEditByPosition({ row: index, column: 1 });
};

//ham xu ly neu check vao checkbox o tren header grid 
var SelectedCheckAll_Change = function (value) {
    //var colIdx = 3;
    if (value) {
        App.grd.getStore().each(function (item) {
            if ((value.checked == true && this.data.Selected == false) || (value.checked == false && this.data.Selected == true)) {
                item.set("Selected", value.checked);
                SelectedCheckEveryRow_Change(0, 0, item);
            }
        });
    }
};

// ham xu ly khi check vao 1 checkbox  nho trong Grid 
var SelectedCheckEveryRow_Change = function (item, Value, newValue) {
    //select dong ma minh da check chon
    if (newValue.data.Selected == true) {
        //sau khi chon xong thay doi gia tri trong o SOFee, txtCuryTxblAmt va txtCuryTaxAmt , DiscountAmount va TotalAmount
        App.txtSOFee.setValue(App.txtSOFee.value + newValue.data.SOFee);
        App.txtCuryTxblAmt.setValue(App.txtCuryTxblAmt.value + newValue.data.TxblAmt);
        App.txtCuryTaxAmt.setValue(App.txtCuryTaxAmt.value + newValue.data.TaxAmt);
        App.txtDiscAmt.setValue(App.txtDiscAmt.value + newValue.data.DiscAmt)
        
        //ChangeWhenSelectCheckBoxGrid();
    } else {
        App.txtSOFee.setValue(App.txtSOFee.value - newValue.data.SOFee);
        App.txtCuryTxblAmt.setValue(App.txtCuryTxblAmt.value - newValue.data.TxblAmt);
        App.txtCuryTaxAmt.setValue(App.txtCuryTaxAmt.value - newValue.data.TaxAmt);
        App.txtDiscAmt.setValue(App.txtDiscAmt.value - newValue.data.DiscAmt)
        
        //ChangeWhenSelectCheckBoxGrid();
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





var btnLoadData_Click = function () {
    App.txtTmpLoadGrid.setValue("1");
    //if (App.cboInvcNbr.displayTplData[0] != undefined) {
        
    //    App.txtTmpInvcNote.setValue(App.cboInvcNbr.displayTplData[0].InvcNote);
    //} else {
    //    App.txtTmpInvcNote.setValue("");
    //}

    var combobox = App.cboInvcNbr;
    var v = combobox.getValue();
    var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
    var index = combobox.store.indexOf(record);
    if (record != false) {
        App.txtTmpInvcNote.setValue(combobox.store.getAt(index).data.InvcNote);
    } else {
        App.txtTmpInvcNote.setValue("");
    }



    App.storeGrid.reload();
};









































































