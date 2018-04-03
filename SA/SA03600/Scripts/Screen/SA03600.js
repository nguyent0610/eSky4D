//// Declare //////////////////////////////////////////////////////////
var keys = ['ScreenNumber'];
var fieldsCheckRequire = ["ScreenNumber", "Descr"];
var fieldsLangCheckRequire = ["ScreenNumber", "Descr"];

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
        App.stoApp_ScreenSummary.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdApp_ScreenSummary);
            break;
        case "prev":
            HQ.grid.prev(App.grdApp_ScreenSummary);
            break;
        case "next":
            HQ.grid.next(App.grdApp_ScreenSummary);
            break;
        case "last":
            HQ.grid.last(App.grdApp_ScreenSummary);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoApp_ScreenSummary.reload();
            }          
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdApp_ScreenSummary, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmApp_ScreenSummary.selected.items[0] != undefined) {
                    if (App.slmApp_ScreenSummary.selected.items[0].data.Code != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdApp_ScreenSummary)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                
                if (HQ.store.checkRequirePass(App.stoApp_ScreenSummary, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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

//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboScreen.getStore().addListener('load', checkLoad);
    //checkLoad();
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoApp_ScreenSummary);
    HQ.common.changeData(HQ.isChange, 'SA03600');//co thay doi du lieu gan * tren tab title header
};
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoApp_ScreenSummary_Load = function (sto) {
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false; //sto load cuoi se su dung
    }
    //Sto tiep theo
    frmChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

var grdApp_ScreenSummary_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys))
        return false;
};

var grdApp_ScreenSummary_Edit = function (item, e) {
    if (e.field == "ScreenNumber") {
        var a = HQ.store.findRecord(App.cboScreen.store, ['ScreenNumber'], [e.value]);
        if (e.value != "" && e.value !=null)
        {
            e.record.set('Descr_Screen', a.data.Descr);
        }
        else
        {
            e.record.set('Descr_Screen', "");
        }
    }
    HQ.grid.checkInsertKey(App.grdApp_ScreenSummary, e, keys);
    frmChange();
};

var grdApp_ScreenSummary_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdApp_ScreenSummary, e, keys);
};

var grdApp_ScreenSummary_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdApp_ScreenSummary);
    frmChange();
};

////Function menuClick
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SA03600/Save',
            params: {
                lstApp_ScreenSummary: HQ.store.getData(App.stoApp_ScreenSummary)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isFirstLoad = true;
                App.stoApp_ScreenSummary.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdApp_ScreenSummary.deleteSelected();
        frmChange();
    }
};

function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoApp_ScreenSummary.reload();
    }
};


var onShow = function (toolTip, grid, isHtmlEncode) {
    var view = grid.getView(),
        store = grid.getStore(),
        record = view.getRecord(view.findItemByChild(toolTip.triggerElement)),
        column = view.getHeaderByCell(toolTip.triggerElement),
        data = record.get(column.dataIndex);

    if (data) {
        var viewData = data;

        if (isHtmlEncode) {
            toolTip.update(Ext.util.Format.htmlEncode(viewData));
        }
        else {
            toolTip.update(viewData);
        }
    }
    else {
        toolTip.hide();
    }
};
