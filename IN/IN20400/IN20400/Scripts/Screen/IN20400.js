//// Declare //////////////////////////////////////////////////////////

var keys = ['WhseLoc'];
var fieldsCheckRequire = ["WhseLoc", "Descr"];
var fieldsLangCheckRequire = ["WhseLoc", "Descr"];
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var _Source = 0;
var _maxSource = 1;
var _isLoadMaster = false;


var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoIN_SiteLocation.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == 'header') {
                HQ.isFirstLoad = true;
                HQ.combo.first(App.cboSiteID, HQ.isChange);

            }
            else if (HQ.focus == 'grdIN_SiteLocation') {
                HQ.grid.first(App.grdIN_SiteLocation);
            }
            break;
        case "prev":
            if (HQ.focus == 'header') {
                HQ.isFirstLoad = true;
                HQ.combo.prev(App.cboSiteID, HQ.isChange);

            }
            else if (HQ.focus == 'grdIN_SiteLocation') {
                HQ.grid.prev(App.grdIN_SiteLocation);
            }
            break;
        case "next":
            if (HQ.focus == 'header') {
                HQ.isFirstLoad = true;
                HQ.combo.next(App.cboSiteID, HQ.isChange);

            }
            else if (HQ.focus == 'grdIN_SiteLocation') {
                HQ.grid.next(App.grdIN_SiteLocation);
            }
            break;
        case "last":
            if (HQ.focus == 'header') {
                HQ.isFirstLoad = true;
                HQ.combo.last(App.cboSiteID, HQ.isChange);

            }
            else if (HQ.focus == 'grdIN_SiteLocation') {
                HQ.grid.last(App.grdIN_SiteLocation);
            }
            break;
        case "refresh":
	
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoIN_SiteLocation.reload();
            }          
            break;
        case "new":
            if (HQ.isInsert && App.frmMain.isValid()) {
                HQ.grid.insert(App.grdIN_SiteLocation, keys);
            }
            break;
        case "delete":
            if (HQ.focus == 'header') {
                if (App.cboSiteID.getValue() != null) {
                    if (App.stoCheckSiteID.data.length > 0) {
                        HQ.message.show(2018061611, [App.cboSiteID.getValue()], '',true);
                    }
                    else {
                        HQ.message.show(11, '', 'deleteData');
                    }                   

                } else {

                    refresh('yes');

                }
            }
            else if (HQ.focus == 'grdIN_SiteLocation')
            {
                if (App.slmIN_SiteLocation.selected.items[0] != undefined) {
                    if (HQ.isDelete) {
                        if (App.slmIN_SiteLocation.selected.items[0] != undefined) {
                            if (App.slmIN_SiteLocation.selected.items[0].data.Code != "") {
                                HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdIN_SiteLocation)], 'deleteData', true);
                            }
                        }
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoIN_SiteLocation, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":         
            break;
    }

};
var cboSiteID_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {
        App.cboSiteID.setValue('');
        App.stoIN_SiteLocation.reload();
    }
};
var cboSiteID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    App.stoIN_SiteLocation.reload();
    App.stoCheckSiteID.reload();
};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.util.checkAccessRight(); // kiểm tra các quyền update,insert,del
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    App.cboSiteID.getStore().addListener('load', checkLoad);
    App.cboSiteID.getStore().reload();
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoIN_SiteLocation);
    HQ.common.changeData(HQ.isChange, 'IN20400');//co thay doi du lieu gan * tren tab title header
    App.cboSiteID.setReadOnly(HQ.isChange);
};
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoIN_SiteLocation.reload();
    }
};


//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false; //sto load cuoi se su dung
    }
    frmChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};
//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
var grdIN_SiteLocation_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdIN_SiteLocation_Edit = function (item, e) {    
    HQ.grid.checkInsertKey(App.grdIN_SiteLocation, e, keys);
    frmChange();
};
var grdIN_SiteLocation_ValidateEdit = function (item, e) {

    return checkValidateEditIN20400(App.grdIN_SiteLocation, e, keys);
};
var grdIN_SiteLocation_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdIN_SiteLocation);
    frmChange();
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.cboSiteID.store.findRecord("SiteId", App.cboSiteID.getValue()) == null)
    {
        HQ.message.show(20180605);
        return;
    }
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'IN20400/Save',
            params: {
                lstIN_SiteLocation: HQ.store.getData(App.stoIN_SiteLocation)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isChange = false;
                refresh("yes");
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

                App.frmMain.submit({
                    waitMsg: HQ.common.getLang("DeletingData"),
                    url: 'IN20400/DeleteAll',
                    timeout: 7200,

                    success: function (msg, data) {
                        App.cboSiteID.setValue("");
                    },

                    failure: function (msg, data) {
                        HQ.message.process(msg, data, true);

                    }
                });
            }
        }
        else if (HQ.focus == 'grdIN_SiteLocation') {
            var lstDel = App.grdIN_SiteLocation.selModel.selected.items;
            var line = "";
            for (var i = 0; i < lstDel.length; i++) {
                var obj = HQ.store.findRecord(App.stoCheckSiteID, ['WhseLoc'], [lstDel[i].data.WhseLoc]);
                if (obj != undefined) {
                    line = line + (i + 1) + ",";
                }
            }
            if (line != "") {
                HQ.message.show(2018061612, [line], '', true);
            }
            else {
                App.grdIN_SiteLocation.deleteSelected();
                frmChange();
            }            
        }
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoIN_SiteLocation.reload();
    }
};


var checkValidateEditIN20400 = function (grd, e, keys, isCheckSpecialChar) {
    if (keys.indexOf(e.field) != -1) {
        var regex = /^(\w*(\d|[a-zA-Z]|[\_@()+-.]))*$/;
        if (isCheckSpecialChar == undefined) isCheckSpecialChar = true;
        if (isCheckSpecialChar) {
            if (e.value)
                if (!HQ.util.passNull(e.value) == '' && !HQ.util.passNull(e.value.toString()).match(regex)) {
                    HQ.message.show(20140811, e.column.text);
                    return false;
                }
        }
        if (HQ.grid.checkDuplicate(grd, e, keys)) {
            if (e.column.xtype == "datecolumn")
                HQ.message.show(1112, Ext.Date.format(e.value, e.column.format));
            else HQ.message.show(1112, e.value);
            return false;
        }

    }
}




///////////////////////////////////








