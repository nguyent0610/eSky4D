var _holdStatus = "H";
var _isFieldFocus = false;

var addFieldsFocusEvent = function (form) {
    form.getForm().getFields().each(function (field) {
        field.addEvents('onFocus', 'test');
    });
};

function test() {
    _isFieldFocus = true;
}

// Action of the top bar
var menuClick = function (command) {
    switch (command) {
        case "first":

            break;

        case "prev":
            
            break;

        case "next":
            
            break;

        case "last":
            
            break;

        case "refresh":
            App.cboBatNbr.getStore().reload();
            App.cboRefNbr.getStore().reload();
            App.GridAR_Adjust.getStore().reload();
            break;

        case "new":
            if (isInsert) {
                
            }
            else {
                callMessage(4, '', '');
            }
            break;

        case "delete":
            if (isDelete) {
                
            }
            else {
                callMessage(4, '', '');
            }
            break;

        case "save":
            if (isUpdate || isInsert || isDelete) {
                
            }
            else {
                callMessage(4, '', '');
            }
            break;

        case "print":
            alert(command);
            break;

        case "close":
            parent.App.tabAR10200.close();
            break;
    }

};

var cboBatNbr_change = function (e) {
    App.cboRefNbr.getStore().reload();
    if (e.getValue()) {
        if (e.store) {
            var record = e.store.findRecord("BatNbr", e.value);
            if (record) {
                App.headerForm.getForm().loadRecord(record);
                App.txtAutoPayment.setValue(0);
                App.txtOdd.setValue(0);
            }
        }
    }
    else {
        App.headerForm.getForm().reset(true);
    }
};

var chkAllowSort_change = function (obj) {
    App.GridAR_Adjust.sortableColumns = obj.checked;
    App.GridAR_Adjust.update();
};

var cboRefNbr_change = function (e) {
    if (e.getValue()) {
        if (e.store) {
            var record = e.store.findRecord("RefNbr", e.value);
            if (record) {
                App.docForm.getForm().loadRecord(record);
                App.GridAR_Adjust.getStore().reload();
            }
        }
    }
    else {
        App.docForm.getForm().reset(true);
        App.GridAR_Adjust.getStore().reload();
    }
};

var GridAR_Adjust_afterRender = function (obj) {
    if (this.parentAutoLoadControl != null)
        obj.setHeight(this.parentAutoLoadControl.getHeight() - 320);
    else {
        obj.setHeight(Ext.getBody().getViewSize().height - 320);
    }
};

var btnSearch_click = function () {
    App.GridAR_Adjust.getStore().reload();
};

var chkAllAdjust_change = function (obj) {
    // Prevent edit header check column if hold status
    if (App.cboStatus.getValue() != _holdStatus) {
        return false;
    }

    if (obj) {
        var store = obj.up('gridpanel').getStore();

        store.each(function (record) {
            if (record.data.InvcNbr) {
                if (obj.checked) {
                    record.set("Selected", true);
                    record.set("DocBal", 0);
                    record.set("Payment", record.data.OrigDocBal);
                }
                else {
                    record.set("Selected", false);
                    record.set("DocBal", record.data.OrigDocBal);
                    record.set("Payment", 0);
                }
            }
        });
    }
};

var GridAR_Adjust_cellBeforeEdit = function (editor, e) {
    var record = e.record;
    var value = e.value;

    // Prevent edit if hold status
    if (App.cboStatus.getValue() != _holdStatus) {
        return false;
    }

    // Prepare for editing in Selected check column
    if (e.field == 'Selected') { // value is true/false
        if (record.data.InvcNbr) {
            if (value) {
                record.set("DocBal", 0);
                record.set("Payment", record.data.OrigDocBal ? record.data.OrigDocBal : 0);
                record.set("Selected", true);
            }
            else {
                record.set("DocBal", record.data.OrigDocBal ? record.data.OrigDocBal : 0);
                record.set("Payment", 0);
                record.set("Selected", false);
            }
        }
    }

    // Prepare for editing in Payment column
    if (e.field == 'Payment') { // value is payment
        if (value >= record.data.OrigDocBal) {
            record.set("DocBal", 0);
            record.set("Payment", record.data.OrigDocBal);
            record.set("Selected", true);
            return true;
        }
        else {
            record.set("Payment", value);
            record.set("DocBal", record.data.OrigDocBal - value);
            record.set("Selected", false);
        }
    }
};

var GridAR_Adjust_cellEdit = function (editor, e) {
    var record = e.record;
    var value = e.value;

    // Edited in Selected check column
    if (e.field == 'Selected') { // value is true/false
        if (value) {
            record.set("DocBal", 0);
            record.set("Payment", record.data.OrigDocBal);
            record.set("Selected", true);
        }
        else {
            record.set("DocBal", record.data.OrigDocBal);
            record.set("Payment", 0);
            record.set("Selected", false);
        }
    }

    // Edited in Payment column
    if (e.field == 'Payment') { // value is payment
        if (value >= e.record.data.OrigDocBal) {
            record.set("DocBal", 0);
            record.set("Payment", record.data.OrigDocBal);
            record.set("Selected", true);
        }
        else {
            record.set("Payment", value);
            record.set("DocBal", record.data.OrigDocBal - value);
            record.set("Selected", false);
        }
    }
};

var btnAutoPayment_click = function () {
    var autoPayment = App.txtAutoPayment.getValue() ? App.txtAutoPayment.getValue() : 0;
    var store = App.GridAR_Adjust.getStore();
    var sumPayment = 0;

    if (autoPayment && autoPayment > 0) {
        store.each(function (record) {
            record.set("Payment", 0);
            record.set("Selected", false);

            if (autoPayment >= record.data.OrigDocBal) {
                autoPayment -= record.data.OrigDocBal;

                record.set("Selected", true);
                record.set("Payment", record.data.OrigDocBal);
                record.set("DocBal", 0);
            }
            else if (autoPayment > 0) {
                record.set("Selected", false);
                record.set("Payment", autoPayment);
                record.set("DocBal", record.data.OrigDocBal - autoPayment);
                autoPayment = 0;
            }
            else {
                record.set("Selected", false);
                record.set("Payment", autoPayment);
                record.set("DocBal", record.data.OrigDocBal - autoPayment);
            }

            sumPayment += record.data.Payment;
        });

        // Calc Odd
        var odd = App.txtAutoPayment.getValue() - sumPayment;
        App.txtOdd.setValue(odd);
        App.txtAutoPayment.setValue(0);
    }
};

var storeGridAR_Adjust_load = function () {
    var batNbr = App.cboBatNbr.getValue();
    var refNbr = App.cboRefNbr.getValue();
    var status = App.cboStatus.getValue();

    if (batNbr && refNbr && status && status == _holdStatus) {
        App.fcontainerNewRecord.show();
        //App.cboNewRowInvcNbr.getStore().reload();
    }
    else {
        App.fcontainerNewRecord.hide();
    }
};

var storeGridAR_Adjust_dataChanged = function (store) {
    var paymentTotal = 0;
    var unpaymentTotal = 0;

    store.each(function (record) {
        paymentTotal += record.data.Payment;
        unpaymentTotal += record.data.DocBal;
    });

    App.txtTotApply.setValue(paymentTotal);
    App.txtUnTotApply.setValue(unpaymentTotal);
};

var updateTotal = function (grid, container) {
    if (!grid.view.rendered) {
        return;
    }

    var field,
        value,
        width,
        data = { test1: 0, test2: 0, test3: 0 },
        c,
        cs = grid.headerCt.getVisibleGridColumns();

    //for (var j = 0, jlen = grid.store.getCount() ; j < jlen; j++) {
    //    var r = grid.store.getAt(j);

    //    for (var i = 0, len = cs.length; i < len; i++) {
    //        c = cs[i];
    //        if (c.dataIndex) {
    //            data[c.dataIndex] += r.get(c.dataIndex);
    //        }
    //    }
    //}

    container.suspendLayout = true;
    for (var i = 0; i < cs.length; i++) {
        c = cs[i];
        value = data[c.dataIndex];

        field = container.down('component[name="' + c.dataIndex + '"]');
        if (field) {
            container.remove(field, false);
            container.insert(i, field);
            width = c.getWidth();
            field.setWidth(width - 1);
            field.setValue(c.renderer ? (c.renderer)(value, {}, {}, 0, i, grid.store, grid.view) : value);
        }
    }

    container.items.each(function (field) {
        var column = grid.headerCt.down('component[dataIndex="' + field.name + '"]');
        field.setVisible(column.isVisible());
    });

    container.suspendLayout = false;
    container.updateLayout();
};