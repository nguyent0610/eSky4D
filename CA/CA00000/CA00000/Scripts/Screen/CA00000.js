//// Declare //////////////////////////////////////////////////////////
var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;
///////////////////////////////////////////////////////////////////////

//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.cboBranchID.setValue(HQ.cpnyID);
        HQ.common.showBusy(false);
    }
};
var stoLoad = function (sto) {
    HQ.isNew = false;
    HQ.common.lockItem(App.frmMain, false);
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "");
        record = sto.getAt(0);
        HQ.isNew = true; //record la new 
        HQ.isFirstLoad = true;
        HQ.common.setRequire(App.frmMain);  //to do cac o la require 
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);

    if (!HQ.isInsert && HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
        frmChange();
    }
};

//Truoc khi load store se hien Busy
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};
////////////////////////////////////////////////////////////////////////


//// Event /////////////////////////////////////////////////////////////
var firstLoad = function () {
    //HQ.util.checkAccessRight(); //Kiểm tra quyền Insert Update Delete để disable các button trên topbar(Bắt buộc)
    if (HQ.isInsert == false && HQ.isDelete == false && HQ.isUpdate == false)
        App.menuClickbtnSave.disable();
    App.frmMain.isValid(); //Require các field yêu cầu trên man hình
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    HQ.isFirstLoad = true;
    App.cboBranchID.getStore().addListener('load', checkLoad);
};

var frmChange = function () {
    if (App.storeCA_Setup.getCount() > 0)
        App.frmMain.getForm().updateRecord();

    HQ.isChange = HQ.store.isChange(App.storeCA_Setup);
    HQ.common.changeData(HQ.isChange, 'CA00000');
    App.cboBranchID.setReadOnly(HQ.isChange);
};
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.combo.first(App.cboBranchID, HQ.isChange);
            break;
        case "next":
            HQ.combo.next(App.cboBranchID, HQ.isChange);
            break;
        case "prev":
            HQ.combo.prev(App.cboBranchID, HQ.isChange);
            break;
        case "last":
            HQ.combo.last(App.cboBranchID, HQ.isChange);
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain)) {
                    save();
                }
            }
            break;
        case "refresh":
            App.storeCA_Setup.reload();
            break;
        case "new":
            if (!HQ.isChange) {
                App.cboBranchID.setValue('');
                refresh("yes");
            }
            break;
        case "delete":
            break;
        case "print":
            break;
        case "close":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                refresh("yes");
            }
            break;
    }

};
//cac store co param la branchID thi load lai sau khi cboBranchID thay doi
var cboBranchID_Change = function (item, newValue, oldValue) {
    if (item.valueModels != null && App.cboBranchID.getValue() != null && !item.hasFocus) {//truong hop co chon branchid
        App.storeCA_Setup.reload();
    }
}
var cboBranchID_Select = function (item, newValue, oldValue) {
    if (item.hasFocus) {
        App.storeCA_Setup.reload();
    }
}
/////////////////////////////////////////////////////////////////////////




//// Process Data ///////////////////////////////////////////////////////


var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.updateRecord();
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'CA00000/Save',
            params: {
                lstCA_Setup: Ext.encode(App.storeCA_Setup.getRecordsValues())
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                menuClick("refresh");
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};


function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        App.storeCA_Setup.reload();
        App.cboBranchID.setReadOnly(HQ.isChange);
    }
};
/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////



/////////////////////////////////////////////////////////////////////////








