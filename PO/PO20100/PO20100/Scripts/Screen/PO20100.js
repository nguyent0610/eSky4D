var keys = ['InvtID'];
var keysCpny = ['CpnyID'];
var paramInvtID = "";
var _focusNo = 0;

var pnl_render = function (cmd) {
    cmd.getEl().on('mousedown', function () {
        if (cmd.id == 'pnlPO_Price') {
            _focusNo = 1;
        }
        else if (cmd.id == 'pnlPO_PriceCpny') {
            _focusNo = 2;
        }
        else {//pnlHeader
            _focusNo = 0;
        }
    });
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (_focusNo == 0) {
                App.cboPriceID.setValue(App.cboPriceID.getStore().first().get('PriceID'));
            }
            else if (_focusNo == 1) {
                HQ.grid.first(App.grdPO_Price);
            }
            if (_focusNo == 2) {
                HQ.grid.first(App.grdPO_PriceCpny);
            }
            //if (_focusNo == 0) {
            //    HQ.grid.first(App.grdPO_PriceCpny);
            //}
            break;
        case "prev":
            if (_focusNo == 0) {
                var combobox = App.cboPriceID;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboPriceID.setValue(combobox.store.getAt(index - 1).data.PriceID);
            }
            else if (_focusNo == 1) {
                HQ.grid.prev(App.grdPO_Price);
            }
            else if (_focusNo == 2) {
                HQ.grid.prev(App.grdPO_PriceCpny);
            }
            break;
        case "next":
            if (_focusNo == 0) {
                var combobox = App.cboPriceID;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboPriceID.setValue(combobox.store.getAt(index + 1).data.PriceID);
            }
            else if (_focusNo == 1) {
                HQ.grid.next(App.grdPO_Price);
            }
            else if (_focusNo == 2) {
                HQ.grid.next(App.grdPO_PriceCpny);
            }
            break;
        case "last":
            if (_focusNo == 0) {
                App.cboPriceID.setValue(App.cboPriceID.getStore().last().get('PriceID'));
            }
            else if (_focusNo == 1) {
                HQ.grid.last(App.grdPO_Price);
            }
            else if (_focusNo == 2) {
                HQ.grid.last(App.grdPO_PriceCpny);
            }
            break;
        case "refresh":
            App.stoPO_Price.reload();
            App.stoPO_PriceCpny.reload();
            App.stoPOPriceHeader.load();
            break;
        case "new":
            if (_focusNo == 0) {
                App.cboPriceID.setValue("");
                App.stoPOPriceHeader.reload();
            }
            else if (_focusNo == 1) {
                if (HQ.isInsert) {
                    HQ.grid.insert(App.grdPO_Price);
                }
            }
            else if (_focusNo == 2) {
                if (HQ.isInsert) {
                    HQ.grid.insert(App.grdPO_PriceCpny);
                }
            }

            break;
        case "delete":
            if (_focusNo == 0) {
                if (App.cboPriceID.value) {
                    if (HQ.isDelete) {
                        HQ.message.show(11, '', 'deleteData');
                    }
                } else {
                    menuClick('new');
                }
            }
            else if (_focusNo == 1) {
                if (App.slmPO_Price.selected.items[0] != undefined) {
                    if (HQ.isDelete) {
                        HQ.message.show(11, '', 'deleteData');
                    }
                }
            }
            else if (_focusNo == 2) {
                if (App.slmPO_PriceCpny.selected.items[0] != undefined) {
                    if (HQ.isDelete) {
                        HQ.message.show(11, '', 'deleteData');
                    }
                }
            }

            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (checkRequire(App.stoPO_Price.getChangedData().Created) && checkRequire(App.stoPO_Price.getChangedData().Updated) || checkRequire(App.stoPO_PriceCpny.getChangedData().Created) && checkRequire(App.stoPO_PriceCpny.getChangedData().Updated)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
            if (HQ.store.isChange(App.stoPO_Price) || HQ.store.isChange(App.stoPO_PriceCpny) || HQ.store.isChange(App.stoPOPriceHeader)) {
                HQ.message.show(7, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};

var tabPO_Setup_AfterRender = function (obj) {
    if (this.parentAutoLoadControl != null)
        obj.setHeight(this.parentAutoLoadControl.getHeight() - 100);
    else {
        obj.setHeight(Ext.getBody().getViewSize().height - 100);
    }
};

var cboPriceID_Change = function (item, newValue, oldValue) {
    App.stoPOPriceHeader.load();
    App.grdPO_Price.store.reload();
    App.grdPO_PriceCpny.store.reload();
};
var loadData = function () {
    if (App.stoPOPriceHeader.getCount() == 0) {
        var record = Ext.create("App.mdlPOPriceHeader", {
            PriceID: App.cboPriceID.getValue()
        });
        App.stoPOPriceHeader.insert(0, record);
    }
    App.frmMain.getForm().loadRecord(App.stoPOPriceHeader.getAt(0));
};
function btnFill_Click() {
    App.stoPO_Price.each(function (item, index, totalItems) {
        item.set('Disc', App.txtFill.getValue());

    });

};

var chkPublic_Change = function (checkbox, checked) {
    if (checked) {
        App.tabPO20100.closeTab(App.pnlPO_PriceCpny);
    }
    else {
        App.tabPO20100.addTab(App.pnlPO_PriceCpny);
    }
};

var cboInvtID_Change = function (item, newValue, oldValue) {
    //var r = App.cboInvtID.valueModels[0];

    //if (r == null) {
    //    App.slmPO_Price.getSelection()[0].set('Descr', "");
    //}
    //else {
    //    App.slmPO_Price.getSelection()[0].set('Descr', r.data.Descr);
    //}

};

var grdPO_Price_BeforeEdit = function (editor, e) {
    if (!HQ.isUpdate) return false;
    if (keys.indexOf(e.field) != -1) {
        if (e.record.data.tstamp != "")
            return false;
    }
    paramInvtID = e.record.data.InvtID;
    App.cboUOM.getStore().load();
    return HQ.grid.checkInput(e, keys);
};

var grdPO_Price_Edit = function (item, e) {

    if (keys.indexOf(e.field) != -1) {
        if (e.value != '' && isAllValidKey(App.stoPO_Price.getChangedData().Created) && isAllValidKey(App.stoPO_Price.getChangedData().Updated))
            HQ.store.insertBlank(App.stoPO_Price);
    }

    if (e.field == 'InvtID') {
        var selectedRecord = App.cboInvtID.store.findRecord(e.field, e.value);
        if (selectedRecord) {
            e.record.set("Descr", selectedRecord.data.Descr);
        }
        else {
            e.record.set("Descr", "");
        }
    }
};

var grdPO_Price_ValidateEdit = function (item, e) {
    if (keys.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(App.grdPO_Price, e, keys)) {
            HQ.message.show(1112, e.value);
            return false;
        }
        var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/
        if (!e.value.match(regex)) {
            HQ.message.show(20140811, e.column.text);
            return false;
        }
    }
};

var grdPO_Price_Reject = function (record) {
    if (record.data.tstamp == '') {
        App.stoPO_Price.remove(record);
        App.grdPO_Price.getView().focusRow(App.stoPO_Price.getCount() - 1);
        App.grdPO_Price.getSelectionModel().select(App.stoPO_Price.getCount() - 1);
    } else {
        record.reject();
    }
};

var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.updateRecord();
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'PO20100/Save',
            params: {
                lstPOPriceHeader: HQ.store.getData(App.frmMain.getRecord().store),
                lstPO_Price: HQ.store.getData(App.stoPO_Price),
                lstPO_PriceCpny: HQ.store.getData(App.stoPO_PriceCpny)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                App.cboPriceID.getStore().reload();
                menuClick("refresh");
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        if (_focusNo == 0) {
            App.direct.DeleteAll(App.cboPriceID.getValue(), {
                success: function () {
                    App.cboPriceID.getStore().load();
                    menuClick("new");
                },
                eventMask: { msg: 'HQ.common.getLang("DeletingData")', showMask: true }
            });
        }
        else if (_focusNo == 1) {
            App.grdPO_Price.deleteSelected();
        }
        else if (_focusNo == 2) {
            App.grdPO_PriceCpny.deleteSelected();
        }
    }
};

//kiem tra key da nhap du chua
var isAllValidKey = function (items) {
    if (items != undefined) {
        if (_focusNo == 1) {
            for (var i = 0; i < items.length; i++) {
                for (var j = 0; j < keys.length; j++) {
                    if (items[i][keys[j]] == '' || items[i][keys[j]] == undefined)
                        return false;
                }
            }
        }
        if (_focusNo == 2) {
            for (var i = 0; i < items.length; i++) {
                for (var j = 0; j < keysCpny.length; j++) {
                    if (items[i][keysCpny[j]] == '' || items[i][keysCpny[j]] == undefined)
                        return false;
                }
            }
        }
        return true;
    } else {
        return true;
    }
};

//kiem tra nhung field yeu cau bat buoc nhap
var checkRequire = function (items) {
    if (items != undefined) {
        for (var i = 0; i < items.length; i++) {
            if (HQ.grid.checkRequirePass(items[i], keys)) continue;
            if (items[i]["InvtID"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("InvtID"));
                return false;
            }
            if (items[i]["UOM"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("UOM"));
                return false;
            }

        }
        return true;
    } else {
        return true;
    }
};

/////grdPOPriceCnpy
var grdPO_PriceCpny_BeforeEdit = function (editor, e) {
    if (!HQ.isUpdate) return false;
    //keys = e.record.idProperty.split(',');
    return HQ.grid.checkInput(e, keysCpny);
};
var grdPO_PriceCpny_Edit = function (item, e) {

    if (keysCpny.indexOf(e.field) != -1) {
        if (e.value != '' && isAllValidKey(App.stoPO_PriceCpny.getChangedData().Created) && isAllValidKey(App.stoPO_PriceCpny.getChangedData().Updated))
            HQ.store.insertBlank(App.stoPO_PriceCpny);
    }

    if (e.field == 'CpnyID') {
        var selectedRecord = App.cboCpnyID.store.findRecord("BranchID", e.value);
        if (selectedRecord) {
            e.record.set("CpnyName", selectedRecord.data.BranchName);
        }
        else {
            e.record.set("CpnyName", "");
        }
    }
};
var grdPO_PriceCpny_ValidateEdit = function (item, e) {
    if (keysCpny.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(App.grdPO_PriceCpny, e, keysCpny)) {
            HQ.message.show(1112, e.value);
            return false;
        }
        var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/
        if (e.value && !e.value.match(regex)) {
            HQ.message.show(20140811, e.column.text);
            return false;
        }
    }
};
var grdPO_PriceCpny_Reject = function (record) {
    record.reject();
};

//// Other Functions ////////////////////////////////////////////////////

var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.close(this);
    }
};
