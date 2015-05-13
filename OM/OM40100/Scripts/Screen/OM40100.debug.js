
//// Store ///////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////


var stoOrder_Load = function () {
    bindOrder();
}

var store_Load = function () {
    HQ.numSource++;
    checkSetDefault();
}

var checkSetDefault = function () {
    if (HQ.numSource == HQ.maxSource) {
        defaultOnNew();
    }
}

//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////



//// Event ///////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////

var frmMain_BoxReady = function () {
    HQ.numSource = 0;
    HQ.maxSource = 5;



    App.cboProcessType.getStore().addListener('load', store_Load);
    App.cboBranchID.getStore().addListener('load', store_Load);
    App.cboCustomer.getStore().addListener('load', store_Load);
    App.cboDeliveryID.getStore().addListener('load', store_Load);
    App.cboSlsperID.getStore().addListener('load', store_Load);
    App.chkHeader = null;
    HQ.common.showBusy(true, HQ.waitMsg);
}

var menuClick = function (command) {
    switch (command) {
        case "first":

            HQ.grid.first(App.grdOrder);

            break;
        case "next":
            HQ.grid.next(App.grdOrder);
            break;
        case "prev":
            HQ.grid.prev(App.grdOrder);
            break;
        case "last":
            HQ.grid.last(App.grdOrder);
            break;
        case "save":
            break;
        case "delete":
            break;
        case "close":
            HQ.common.close(this);
            break;
        case "new":
            break;
        case "refresh":
            break;
        case "print":
            break;
        default:
    }
}

var btnProcess_Click = function () {
    var flat = false;
    var lstOrd = [];
    App.stoOrder.data.each(function (item) {
        if (item.data.Sel == true) {
            flat = true;
            lstOrd.push(item.data);
        }
    });

    if (!flat) {
        return;
    }

    if (!HQ.isRelease) {
        HQ.message.show(728, '', '', true);
        return;
    }

    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.waitMsg,
            method: 'POST',
            url: 'OM40100/Process',
            timeout: 180000,
            params: {
                lstOrder: Ext.encode(lstOrd),
            },
            success: function (msg, data) {

                HQ.message.process(msg, data, true);

                App.btnLoad.fireEvent('click');
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
}
var btnLoad_Click = function () {
    if (App.chkHeader != null) App.chkHeader.checked = false;

    App.stoOrder.reload();
}

var cboProcessType_Change = function () {
    if (App.chkHeader != null) App.chkHeader.checked = false;
    App.stoOrder.clearData();
    App.grdOrder.view.refresh();
}
var cboBranchID_Change = function () {
    cboProcessType_Change();
    App.cboCustomer.setValue('');
    App.cboDeliveryID.setValue('');
    App.cboSlsperID.setValue('');
    App.cboCustomer.store.reload();
    App.cboDeliveryID.store.reload();
    App.cboSlsperID.store.reload();
}
var grdOrder_HeaderClick = function (ct, column, e, t) {
    if (Ext.fly(t).hasCls("my-header-checkbox")) {
        App.chkHeader = t;
        App.stoOrder.data.each(function (item) {
            item.data.Sel = t.checked ? true : false;
        });
        App.grdOrder.view.refresh();
    }
};
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////


//// Function ////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////

///// data ///////////////////////

var bindOrder = function () {
    HQ.common.showBusy(false, HQ.waitMsg);
}

//////////////////////////////////


var defaultOnNew = function () {
    App.cboBranchID.setValue(HQ.cpnyID);
    App.cboProcessType.setValue('R');
    App.txtFromDate.setValue(HQ.businessDate);
    App.txtToDate.setValue(HQ.businessDate);
    App.frmMain.validate();

    HQ.common.showBusy(false);
}

var rdrTrans_QtyAmt = function (value) {
    return Ext.util.Format.number(value, '0,000');
}

var setStatusForm = function () {

    var lock = true;

    if (!Ext.isEmpty(HQ.objBatch.data.BatNbr)) {
        if (HQ.objBatch.data.JrnlType == 'PO') {
            lock = true;
        } else if (HQ.objBatch.data.Status == 'H') {
            lock = false;
        }
        App.btnImport.setDisabled(true);
    } else {
        lock = !HQ.isInsert;
        App.btnImport.setDisabled(false);
    }

    HQ.common.lockItem(App.frmMain, lock);
    App.grdOrder.isLock = lock;
    App.BatNbr.setReadOnly(false);
    App.Handle.setReadOnly(false);
    App.RvdBatNbr.setReadOnly(true);
    App.Status.setReadOnly(true);
    App.RefNbr.setReadOnly(true);
    App.TotQty.setReadOnly(true);
    App.TotAmt.setReadOnly(true);
}

var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.close(this);
    }
}

//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////





