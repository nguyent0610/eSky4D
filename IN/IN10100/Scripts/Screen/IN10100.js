var _focusrecord = 1;

var menuClick = function (command) {
    switch (command) {
        case "first":
            
            if (_focusrecord == 1) {

                var combobox = App.cboBatNbr;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboBatNbr.setValue(combobox.store.getAt(0).data.BatNbr);

            } else if (_focusrecord == 2) {
                App.SelectionRowOnGrid.select(0);
            }
            break;
        case "prev":
            if (_focusrecord == 1) {

                var combobox = App.cboBatNbr;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboBatNbr.setValue(combobox.store.getAt(index - 1).data.BatNbr);

            } else if (_focusrecord == 2) {
                App.SelectionRowOnGrid.selectPrevious();
            }
            
            break;
        case "next":
            if (_focusrecord == 1) {

                var combobox = App.cboBatNbr;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboBatNbr.setValue(combobox.store.getAt(index + 1).data.BatNbr);

            } else if (_focusrecord == 2) {
                App.SelectionRowOnGrid.selectNext();
            }
            
            break;
        case "last":
            if (_focusrecord == 1) {

                var combobox = App.cboBatNbr;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboBatNbr.setValue(App.cboBatNbr.store.getAt(App.cboBatNbr.store.getTotalCount() - 1).data.BatNbr);

            } else if (_focusrecord == 2) {
                App.SelectionRowOnGrid.select(App.storeGrid.getCount() - 1);
            }
            
            break;
        case "refresh":
            App.storeForm.reload();
            App.storeGrid.reload();
            break;
        case "new":
            if (isInsert) {
                if (_focusrecord == 2) {

                    App.SelectionRowOnGrid.select(App.storeGrid.getCount() - 1);

                    if (App.storeGrid.getCount() == 0) {

                        App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());
                        App.SelectionRowOnGrid.select(App.storeGrid.getCount() - 1);
                        App.grd.editingPlugin.startEditByPosition({ row: App.storeGrid.getCount() - 1, column: 1 });

                    } else if (App.SelectionRowOnGrid.selected.items[0].data.InvtID != "") {
                        App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());
                        App.SelectionRowOnGrid.select(App.storeGrid.getCount() - 1);
                        App.grd.editingPlugin.startEditByPosition({ row: App.storeGrid.getCount() - 1, column: 1 });
                    }




                } else if (_focusrecord == 1) {

                    App.cboBatNbr.setValue('');
                    App.storeForm.reload();
                    //var index = App.ApproveStatusAll.indexOf(App.ApproveStatusAll.findRecord("Code", "H"));
                    App.txtTotQty.setValue(0);
                    App.txtTotAmt.setValue(0);
                    App.txtRcptNbr.setValue('');
                    App.cboTrnsferNbr.setValue('');
                    App.cboReasonCD.setValue('');
                    App.txtIntNbr.setValue('');
                    App.cboSlsPerID.setValue('');
                    App.txtDescr.setValue('');
                    App.txtCancelledBatNbr.setValue('');
                    App.cboReasonCD.setValue('');
                    App.cboReasonCD.setValue('');

                }



            }
            break;
        case "delete":

            if (isDelete) {
                if (App.cboBatNbr.value != "") {

                    if (_focusrecord == 1) {
                        callMessage(11, '', 'deleteRecordFormTopBatch');
                    } else if (_focusrecord == 2) {
                        App.grd.deleteSelected();
                    } 
                }
            }
            
            break;
        case "save":
            if (App.SelectionRowOnGrid.selectNext() == false) {
                App.SelectionRowOnGrid.selectPrevious();
            } else {
                App.SelectionRowOnGrid.selectNext();
            }

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
function Save() {
    for (var i = 0; i <= App.storeGrid.getCount() - 1; i++) {
        App.SelectionRowOnGrid.select(i);
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
    App.dataForm.getForm().updateRecord();
    if (App.dataForm.isValid()) {
        App.dataForm.submit({
            waitMsg: 'Submiting...',
            url: 'IN10100/Save',
            params: {
                lstheader: Ext.encode(App.storeForm.getChangedData({ skipIdForPhantomRecords: false })),//,
                lstgrd: Ext.encode(App.storeGrid.getChangedData({ skipIdForPhantomRecords: false })),
                BranchID: App.txtBranchID.getValue(),
                Handle: App.cboHandle.getValue(),
                BatNbr: App.cboBatNbr.getValue(),

            },
            success: function (f, data) {
                App.cboBatNbr.getStore().reload();
                menuClick("refresh");
                callMessage(201405071, '', null);
                if (data.result.value != "") {
                    setTimeout(function () { waitstoreRefreshAndReloadBatNbr(data.result.value); }, 2000);
                }
            }
            , failure: function (f, errorMsg) {
                //if (errorMsg.result.value == "NoRecordGrid") {
                //    callMessage(8002, '', null);
                //}
                callMessage(8002, '', null);
            }
        });
    }
}

var waitstoreRefreshAndReloadBatNbr = function (batNbr) {
    App.cboBatNbr.setValue(batNbr);
    setReadOnly();
}

var deleteRecordFormTopBatch = function (item) {
    if (item == "yes") {

        try {
            App.direct.DeleteFormTopBatchIN10100(App.cboBatNbr.getValue(), App.txtBranchID.getValue(), {
                success: function (data) {
                    //menuClick('refresh');

                    App.cboBatNbr.setValue('');
                    App.cboBatNbr.getStore().reload();

                    App.dataForm.getForm().reset(true);
                
               
             
                    App.cboStatus.setValue("H");
                    App.cboHandle.getStore().reload();
                    //waitcboStatusReLoad();
                    setTimeout(function () { waitcboStatusReLoad(); }, 700);
      
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

var waitcboStatusReLoad = function () {
    if (App.cboStatus.value == "H") {
        //setReadOnly();
        App.cboHandle.setValue(App.cboHandle.store.data.items[0].data.Code);
    } else {
       // setReadOnly();
        App.cboHandle.setValue(App.cboHandle.store.data.items[1].data.Code);
    }
}













var loadDataAutoHeader = function () {
    var time = new Date();
    if (App.storeForm.getCount() == 0) {
        var newRecord = Ext.create("App.ppv_INReceiptBatch_ResultModel", {

            Status: "H",
            DateEnt: time,
            TotQty: 0,
            TotAmt: 0,
        });

        App.storeForm.insert(0, newRecord);
    }
    var record = App.storeForm.getAt(0);
    if (record != undefined) {
        App.dataForm.getForm().loadRecord(record);
        //App.cboHandle.getStore().reload();
        //App.cboRefNbr.getStore().reload();
        //setTimeout(function () { waitStoreRefNbrReLoad(); }, 1000);

    }
};

var cboBatNbr_Change = function () {
    App.storeForm.reload();
    App.storeGrid.reload();
    setTimeout(function () { waitstoreReloadAndSetValueSomeField(); }, 2000);
};

//thay doi lai so luong sau khi loadGrid
var waitstoreReloadAndSetValueSomeField = function () {
    //thay doi lai txtQty sau khi loadGrid
    App.SelectionRowOnGrid.select(0);
    var TotalQty = 0;
    for (var i = 0; i < App.storeGrid.data.length; i++) {
        App.SelectionRowOnGrid.select(i);
        TotalQty = TotalQty + App.SelectionRowOnGrid.selected.items[0].data.Qty;
    }
    //set lai gia tri cua total qty
    App.txtTotQty.setValue(TotalQty);

}



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

// Xac nhan xoa record tren grid
var deleteRecord = function (item) {
    if (item == "yes") {
        App.grd.deleteSelected();

    }
};






















// Xem lai
function Close() {
    if (App.storeGrid.getChangedData().Updated == undefined && App.storeGrid.getChangedData().Deleted == undefined)
        parent.App.tabIN10100.close();
    else if (App.storeGrid.getChangedData().Updated != undefined || App.storeGrid.getChangedData().Created != undefined || App.storeGrid.getChangedData().Deleted != undefined) {
        App.direct.AskClose({
            success: function (result) {

            }
        });
    }
}
// Xem lai
var askClose = function (item) {
    if (item == "yes") {
        Save();
    }
    else {
        if (parent.App.tabIN10100 != null)
            parent.App.tabIN10100.close();
    }
};
//check value
var isAllValidKey = function (items) {
    if (items != undefined) {
        for (var i = 0; i < items.length; i++) {
            for (var j = 0; j < strKeyGrid.length; j++) {
                if (items[i][strKeyGrid[j]] == '' || items[i][strKeyGrid[j]] == undefined)
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
    if (!isUpdate) return false;
    
    strKeyGrid = e.record.idProperty.split(',');
    var time = new Date();

    if (strKeyGrid.indexOf(e.field) != -1) {
        if (e.record.data.InvtID.trim() == "") {
            return true;
        } else {
            return false;
        }
    }
    if (e.record.data.InvtID.trim() == "") {
        return false;
    }

};
var grd_Edit = function (item, e) {

    if (strKeyGrid.indexOf(e.field) != -1) {
        if (e.value != '' && isAllValidKey(App.storeGrid.getChangedData().Created) && isAllValidKey(App.storeGrid.getChangedData().Updated))
            App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());//Ext.data.Record() 
    }
};
var grd_ValidateEdit = function (item, e) {

    if (strKeyGrid.indexOf(e.field) != -1) {
        if (duplicated(App.storeGrid, e)) {
            if (App.SelectionRowOnGrid.selected.items[0].data.InvtID == "") {
                App.SelectionRowOnGrid.selected.items[0].set('TranDesc', '');
            } else {
                App.SelectionRowOnGrid.select(App.storeGrid.getCount() - 1);
                App.SelectionRowOnGrid.selected.items[0].set('TranDesc', '');
            }
            callMessage(1112, e.value, '');
            return false;
        }
    }
};

var grd_CancelEdit = function (editor, e) {
    if (e.record.phantom) {
        e.store.remove(e.record);
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
    store.pageSize = parseInt(combo.getValue(), 10);
    store.reload();
};

var Focus1_Change = function (sender, e) {
    _focusrecord = 1;
}

var Focus2_Change = function (sender, e) {
    _focusrecord = 2;
}

var cboInvtID_Change = function (value) {
    var invtID = value.displayTplData[0].InvtID;
    var descr = value.displayTplData[0].Descr;
    App.SelectionRowOnGrid.selected.items[0].set('InvtID', invtID);
    App.SelectionRowOnGrid.selected.items[0].set('TranDesc', descr);
    
    App.cboUnitDescCol.getStore().reload();
    //do du lieu mac dinh dau tien vao 2 cai field nay
    if (App.cboReasonCD.getValue()) {
        App.SelectionRowOnGrid.selected.items[0].set('ReasonCD', App.cboReasonCD.getValue());
    }
    
    if (App.cboSiteID.getValue()) {
        App.SelectionRowOnGrid.selected.items[0].set('SiteID', App.cboSiteID.getValue());
    }
    //doi cai cboUnitDescCol reload xong cai store roi do du lieu mac dinh dau tien vao
    setTimeout(function () { waitcboUnitDescColReLoad(); }, 700);
  

}
var waitcboUnitDescColReLoad = function(){
    if (App.cboUnitDescCol.store.data.items.length != 0) {
        App.SelectionRowOnGrid.selected.items[0].set('UnitDesc', App.cboUnitDescCol.store.data.items[0].data.ToUnit);
    }
};


var ExportExel = function () {
    App.dataForm.submit({
        waitMsg: "Exporting....",
        url: 'IN10100/Export1',
        timeout: 1800,
        params: {
            BatNbr: App.cboBatNbr.getValue(),//,
            BranchID: App.txtBranchID.getValue(),
            TranDate: App.cboTranDate.value.toDateString()
        },
        success: function (msg, data) {
            //processMessage(msg, data, true);
            //menuClick('refresh');
            var filePath = data.result.filePath;
            if (filePath) {
                window.location = "IN10100/Download?filePath=" + filePath;
            }
        },
        failure: function (msg, data) {
            processMessage(msg, data, true);
        }
    });


};

    var ImportExel = function () {
        App.dataForm.submit({
            waitMsg: "Importing....",
            url: 'IN10100/Import',
            timeout: 1800,
            clientValidation: false,
            method: 'POST',
            params: {
                BatNbr: App.cboBatNbr.getValue(),//,
                BranchID: App.txtBranchID.getValue(),
                RefNbr: App.txtRcptNbr.getValue(),
                Handle: App.cboHandle.getValue(),
                ReasonCD: App.cboReasonCD.getValue(),
                SiteID: App.cboSiteID.getValue(),
     
            },
            success: function (msg, data) {
                //processMessage(msg, data, true);
                menuClick('refresh');
                //var value = data.result.value;
            
                alert(data.result.value);
                //callMessage(201412011, value + "," + value1, "");
            },
            failure: function (msg, data) {
                menuClick('refresh');
                var value = this.result.value;
                
                //callMessage(201412012, value + "," + value1, null);
                //processMessage(msg, data, true);
            }
        });
    }

    var ImportTemplate_Change = function (sender, e) {

        var fileName = sender.getValue();
        var ext = fileName.split(".").pop().toLowerCase();
        if (ext == "xls" || ext == "xlsx") {
            ImportExel();

        } else {
            alert("Please choose a Media! (.xls, .xlsx)");
            sender.reset();
        }
    };



    var setReadOnly = function () {
        if (App.cboStatus.value != "H") {
            App.cboHandle.setReadOnly(true);
            App.cboTranDate.setReadOnly(true);
            App.cboReasonCD.setReadOnly(true);
            App.cboFromSiteID.setReadOnly(true);
            App.cboSiteID.setReadOnly(true);
            App.txtIntNbr.setReadOnly(true);
            App.cboSlsPerID.setReadOnly(true);
            App.txtDescr.setReadOnly(true);
    
            App.grd.disable(true);
        } else {
            App.cboHandle.setReadOnly(false);
            App.cboTranDate.setReadOnly(false);
            App.cboReasonCD.setReadOnly(false);
            App.cboFromSiteID.setReadOnly(false);
            App.cboSiteID.setReadOnly(false);
            App.txtIntNbr.setReadOnly(false);
            App.cboSlsPerID.setReadOnly(false);
            App.txtDescr.setReadOnly(false);
            App.grd.enable(true);
        }
    }





















    //khi thay doi so luong trong bang
    var txtQty_Change = function (sender, value) {
        //xu ly thay bat tong so Qty trong grid va load len txtQty tren form

        //var oldQty = App.txtTotQty.getValue();
        var TotalQty = value;
        var index = 0;
        App.SelectionRowOnGrid.selected.items[0].set('Qty', value);
        var QtyRaw = App.SelectionRowOnGrid.selected.items[0].data.Qty;

        //chay het cac record co trong grid de bat cai record minh dang selected va luu lai vao index
        for (var k = 0; k < App.grd.store.data.length; k++) {
            if (App.grd.store.data.items[k].data.Qty == QtyRaw) {
                index = k;

            }
        }
        //bat tung dong de cong vao totalQty
        for (var i = 0; i < App.storeGrid.data.length; i++) {
            App.SelectionRowOnGrid.select(i);
            TotalQty = TotalQty + App.SelectionRowOnGrid.selected.items[0].data.Qty;
        }
        // - di cai oldQty dang co tren txtTotQty
        TotalQty = TotalQty - value;
        //set lai gia tri cua total qty
        App.txtTotQty.setValue(TotalQty);

        App.SelectionRowOnGrid.select(index);
        App.grd.editingPlugin.startEditByPosition({ row: index, column: 1 });

        //thay doi sang total Amount = Qty x UnitPrice
        var oldTranAmt = App.SelectionRowOnGrid.selected.items[0].data.TranAmt;
        App.SelectionRowOnGrid.selected.items[0].set('TranAmt', value * App.SelectionRowOnGrid.selected.items[0].data.UnitPrice)

        //thay doi sang txtTotAmt tren form
        App.txtTotAmt.setValue(App.SelectionRowOnGrid.selected.items[0].data.TranAmt - oldTranAmt + App.txtTotAmt.getValue());

    }
    //thay doi gia UnitPrice
    var txtUnitPrice_Change = function (sender,value) {
        //thay doi sang total Amount = Qty x UnitPrice
        var oldTranAmt = App.SelectionRowOnGrid.selected.items[0].data.TranAmt;
        App.SelectionRowOnGrid.selected.items[0].set('TranAmt', value * App.SelectionRowOnGrid.selected.items[0].data.Qty)
        //thay doi sang txtTotAmt tren form
        App.txtTotAmt.setValue(App.SelectionRowOnGrid.selected.items[0].data.TranAmt - oldTranAmt + App.txtTotAmt.getValue());
    }

