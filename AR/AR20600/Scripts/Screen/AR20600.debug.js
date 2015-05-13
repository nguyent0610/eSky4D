var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboCustId.getStore().load(function () {
        App.cboShipToId.getStore().load(function () {
            App.cboCountry.getStore().load(function () {
                //App.cboState.getStore().load(function(){
                //    App.cboCity.getStore().load(function(){
                //        App.cboDistrict.getStore().load(function(){

                //        })
                //    })
                //})
            })
        })
    });
};


// Command of the topbar on screen
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.combo.first(App.cboShipToId, HQ.isChange);
            break;
        case "next":
            HQ.combo.next(App.cboShipToId, HQ.isChange);
            break;
        case "prev":
            HQ.combo.prev(App.cboShipToId, HQ.isChange);
            break;
        case "last":
            HQ.combo.last(App.cboShipToId, HQ.isChange);
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain))//check require truoc khi save
                    save();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.isChange) {
                    HQ.message.show(150, '', '');
                }
                else {
                    App.cboShipToId.setValue('');
                }
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                var curRecord = App.frmMain.getRecord();
                if (curRecord) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                App.stoSOAddress.reload();
            }
            break;
        case "close":
            HQ.common.close(this);
            break;
        default:
    }
};

var cboCustID_Change = function (sender, e) {
    HQ.isFirstLoad = true;
    App.cboShipToId.setValue("");
    App.cboShipToId.getStore().reload();
    App.cboShipToId.store.reload();

};

var cboShipToId_Change = function (sender, e) {
    HQ.isFirstLoad = true;
    App.stoSOAddress.reload();
};

var cboCountry_Change = function (sender, e) {
    App.cboState.getStore().load(function () {
    });
};

var cboState_Change = function (sender, e) {
    App.cboCity.getStore().load(function () {

        App.cboDistrict.getStore().load(function () {

        });
    });
};


// Submit the changed data (created, updated) into server side
function save() {
    if (HQ.isInsert || HQ.isUpdate) {
        var curRecord = App.frmMain.getRecord();
        //curRecord.data.Name = App.txtName.getValue();
        App.frmMain.getForm().updateRecord();
        if (App.frmMain.isValid()) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang('Submiting...'),
                url: 'AR20600/Save',
                params: {
                    lstAR_SOAddress: Ext.encode(App.stoSOAddress.getChangedData({ skipIdForPhantomRecords: false }))
                },
                success: function (action, data) {
                    HQ.message.show(201405071, '', '');

                    var ShipToId = data.result.ShipToId;
                    App.cboShipToId.getStore().load(function () {
                        App.cboShipToId.setValue(ShipToId);
                        App.stoSOAddress.reload();
                    });
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
};

function deleteData(item) {
    if (item == 'yes') {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang('DeletingData'),
            url: 'AR20600/DeleteAll',
            success: function (action, data) {
                App.cboShipToId.setValue("");
                App.cboShipToId.getStore().load(function () { cboShipToId_Change(App.cboShipToId); });

            },
            failure: function (action, data) {
                if (data.result.msgCode) {
                    HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                }
            }
        });
    }
};


var firstLoad = function () {
    HQ.isFirstLoad = true;
    loadSourceCombo();
};

//load store khi co su thay doi
var stoLoad = function (sto) {
    HQ.isFirstLoad = true;
    HQ.common.showBusy(false);
    HQ.isNew = false;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "BranchID,CustId,ShipToId");
        record = sto.getAt(0);
        sto.commitChanges();//commit cho record thanh updated muc dich de dung ham HQ.store.isChange
        HQ.isNew = true;//record la new
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
    }
    App.frmMain.getForm().loadRecord(App.stoSOAddress.getAt(0));
    frmChange();
};

//khi co su thay doi du lieu cua cac conttol tren form
var frmChange = function () {
    App.frmMain.getForm().updateRecord();
    HQ.isChange = HQ.store.isChange(App.stoSOAddress);
    HQ.common.changeData(HQ.isChange, 'AR20600');//co thay doi du lieu gan * tren tab title header
    //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};



/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        App.stoSOAddress.reload();
    }
};
///////////////////////////////////


