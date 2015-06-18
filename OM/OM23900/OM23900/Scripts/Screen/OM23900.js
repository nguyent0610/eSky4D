
var keys = ['InvtID'];
var fieldsCheckRequire = ["InvtID"];
var fieldsLangCheckRequire = ["InvtID"];


var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdOM_DiscConsumers);
            break;
        case "prev":
            HQ.grid.prev(App.grdOM_DiscConsumers);
            break;
        case "next":
            HQ.grid.next(App.grdOM_DiscConsumers);
            break;
        case "last":
            HQ.grid.last(App.grdOM_DiscConsumers);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoOM_DiscConsumers.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdOM_DiscConsumers, keys);
            }
            break;
        case "delete":
            if (App.slmOM_DiscConsumers.selected.items[0] != undefined) {
                var rowindex = HQ.grid.indexSelect(App.grdOM_DiscConsumers);
                if (rowindex != '')
                    HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdOM_DiscConsumers), ''], 'deleteData', true)
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain) && HQ.store.checkRequirePass(App.stoOM_DiscConsumers, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            HQ.common.close(this);
            break;
    }

};

var tabOM23900_AfterRender = function (obj) {
    if (this.parentAutoLoadControl != null)
        obj.setHeight(this.parentAutoLoadControl.getHeight() - 100);
    else {
        obj.setHeight(Ext.getBody().getViewSize().height - 100);
    }
};

var cboBranchID_Change = function (item, newValue, oldValue) {
    HQ.isFirstLoad = true;
    if (item.valueModels != null && !Ext.isEmpty(App.cboBranchID.getValue()) && !item.hasFocus) {
        App.txtBranchName.setValue(item.valueModels[0].data.BranchName);
        
    } else {
        
        App.txtBranchName.setValue('');
    }
    App.stoOM_DiscConsumers.reload();
    
};

var cboBranchID_Select = function (item, newValue, oldValue) {
    App.txtBranchName.setValue(App.cboBranchID.valueModels[0].data.BranchName);
    //App.stoOM_DiscConsumers.reload();
};

var grdOM_DiscConsumers_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdOM_DiscConsumers_Edit = function (item, e) {
    //Kiem tra cac key da duoc nhap se insert them dong moi
    HQ.grid.checkInsertKey(App.grdOM_DiscConsumers, e, keys);
    if (e.field == "InvtID") {
        var selectedRecord = App.cboInvt.store.findRecord(e.field, e.value);
        if (selectedRecord) {
            e.record.set("Descr", selectedRecord.data.Descr);
        }
        else {
            e.record.set("Descr", "");
        }
    }
};

var grdOM_DiscConsumers_ValidateEdit = function (item, e) {
    //ko cho nhap key co ki tu dac biet, va kiem tra trung du lieu
    return HQ.grid.checkValidateEdit(App.grdOM_DiscConsumers, e, keys);
};

var grdOM_DiscConsumers_Reject = function (record) {
    //reject dong thay doi du lieu ve ban dau
    HQ.grid.checkReject(record, App.grdOM_DiscConsumers);
    stoChanged(App.stoOM_DiscConsumers);
};

var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'OM23900/Save',
            params: {
                lstOM_DiscConsumers: HQ.store.getData(App.stoOM_DiscConsumers)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isChange = false;
                App.cboBranchID.getStore().reload();
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
        App.grdOM_DiscConsumers.deleteSelected();
        stoChanged(App.stoOM_DiscConsumers);
    }
};


//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.isFirstLoad = true;
    App.stoOM_DiscConsumers.reload();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM23900');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'OM23900');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
    App.cboInvt.store.reload();
};
//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

var cboInvt_Change = function (value) {
    var k = value.displayTplData[0].Descr;
    App.slmOM_DiscConsumers.selected.items[0].set('Descr', k);
};



var renderInvtName = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboInvtOM23900_pcInventoryActiveByBranch.findRecord("InvtID", rec.data.InvtID);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoOM_DiscConsumers.reload();
    }
};
///////////////////////////////////