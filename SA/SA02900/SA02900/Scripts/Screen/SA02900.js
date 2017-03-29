//// Declare //////////////////////////////////////////////////////////
var _index = 0;
var keysTop = ['AppFolID', 'RoleID', 'Status'];
var fieldsCheckRequireTop = ["AppFolID", "RoleID", "Status", "LangStatus"];
var fieldsLangCheckRequireTop = ["AppFolID", "RoleID", "Status", "LangStatus"];

var keysBot = ['Handle'];
var fieldsCheckRequireBot = ["Handle"];
var fieldsLangCheckRequireBot = ["Handle"];

//var _listFilter = [];
//var _totalCount = 0;
var _lstTopGrid;
var _lstBotGrid;
// Declare
var _Source = 0;
var _maxSource = 10;
var _isLoadMaster = false;

var _Status = '';
var _AppFolID = '';
var _RoleID = '';

var _focusNo = 0;
var _topChange = false;
var _botChange = false;
var pnl_render = function (cmd) {
    cmd.getEl().on('mousedown', function () {
        if (cmd.id == 'pnlTop') {
            _focusNo = 0; //pnlTop
        }
        else {
            _focusNo = 1; //pnlBot
        }
    });
};
////////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoTop.reload();
        HQ.common.showBusy(false);
    }
};

//first load
var firstLoad = function () {
    HQ.util.checkAccessRight(); // Kiem tra quyen Insert Update Delete de disable button tren top bar
    HQ.isFirstLoad = true;
    App.frmMain.isValid(); // Require cac field yeu cau tren from

    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));

    App.cboAppFolID.getStore().addListener('load', checkLoad);
    App.cboRoleID.getStore().addListener('load', checkLoad);
    App.cboMailTo.getStore().addListener('load', checkLoad);
    App.cboMailCC.getStore().addListener('load', checkLoad);
    App.cboParam00.getStore().addListener('load', checkLoad);
    App.cboParam01.getStore().addListener('load', checkLoad);
    App.cboParam02.getStore().addListener('load', checkLoad);
    App.cboParam03.getStore().addListener('load', checkLoad);
    App.cboParam04.getStore().addListener('load', checkLoad);
    App.cboParam05.getStore().addListener('load', checkLoad);
};

////////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (_focusNo == 0) {
                HQ.grid.first(App.grdTop);
            }
            else {
                HQ.grid.first(App.grdBot);
            }
            break;
        case "prev":
            if (_focusNo == 0) {
                HQ.grid.prev(App.grdTop);
            }
            else {
                HQ.grid.prev(App.grdBot);
            }
            break;
        case "next":
            if (_focusNo == 0) {
                HQ.grid.next(App.grdTop);
            }
            else {
                HQ.grid.next(App.grdBot);
            }
            break;
        case "last":
            if (_focusNo == 0) {
                HQ.grid.last(App.grdTop);
            }
            else {
                HQ.grid.last(App.grdBot);
            }
            break;
        case "refresh":
            if (HQ.isChange) {

                HQ.message.show(20150303, '', 'refresh');
            }
            else {

                refresh("yes");
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (_focusNo == 0) {
                    HQ.grid.insert(App.grdTop, keysTop);
                }
                else {
                    HQ.grid.insert(App.grdBot, keysBot);
                }
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (_focusNo == 0) {
                    if (App.slmTopGrid.selected.items[0] != undefined) {
                        if (HQ.isDelete) {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdTop)], 'deleteData', true);
                        }
                    }
                }
                else {
                    if (App.slmBotGrid.selected.items[0] != undefined) {
                        if (HQ.isDelete) {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdBot)], 'deleteData', true);
                        }
                    }
                }
            }

            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoTop, keysTop, fieldsCheckRequireTop, fieldsLangCheckRequireTop)
                    && HQ.store.checkRequirePass(App.stoBot, keysBot, fieldsCheckRequireBot, fieldsLangCheckRequireBot)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            //HQ.common.close(this);
            break;
    }
};

var frmChange = function () {

    HQ.isChange = HQ.store.isChange(App.stoTop) == false ? (HQ.isChange = HQ.store.isChange(App.stoBot)) : true;

    HQ.common.changeData(HQ.isChange, 'SA02900');

    _topChange = HQ.store.isChange(App.stoTop);
    _botChange = HQ.store.isChange(App.stoBot);
};

var stoChangedBot = function (sto) {
    _botChange = true;
    HQ.isChange = HQ.store.isChange(sto);
    //HQ.common.changeData(HQ.isChange, 'SA02900');

};

var stoLoad = function (sto) {
    HQ.isNew = false;

    HQ.common.lockItem(App.frmMain, false);

    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            if (sto.allData.items[sto.allData.items.length - 1].data.AppFolID != '' ||
                sto.allData.items[sto.allData.items.length - 1].data.RoleID != '' ||
                sto.allData.items[sto.allData.items.length - 1].data.Status != '') {
                HQ.store.insertBlank(sto, keysTop);
            }
        }
        //HQ.isFirstLoad = false;
    }

    App.stoBot.reload()


    if (!HQ.isInsert && HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }


};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoLoadBot = function (sto) {

    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keysBot);
        }
        HQ.isFirstLoad = false;
    }

    frmChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
        //setTimeout(function () {
        //    App.grdTop.getSelectionModel().select(_index);
        //    HQ.common.showBusy(false);
        //}, 100);
        App.grdTop.getSelectionModel().select(_index);
        //HQ.isFirstLoad = false;
    }
    
};

var GridTop_Change = function (sender, e) {
    HQ.isFirstLoad = true;
    filterBot();
    _index = App.slmTopGrid.getCurrentPosition().row;
    
};

function filterBot() {
    _AppFolID = App.slmTopGrid.selected.items[0] == undefined ? '' : App.slmTopGrid.selected.items[0].data.AppFolID;
    _RoleID = App.slmTopGrid.selected.items[0] == undefined ? '' : App.slmTopGrid.selected.items[0].data.RoleID;
    _Status = App.slmTopGrid.selected.items[0] == undefined ? '' : App.slmTopGrid.selected.items[0].data.Status;
    var grid = App.grdBot;
    grid.getFilterPlugin().clearFilters();
    grid.getFilterPlugin().getFilter('AppFolID').setValue([_AppFolID, '']);
    grid.getFilterPlugin().getFilter('AppFolID').setActive(true);
    grid.getFilterPlugin().getFilter('RoleID').setValue([_RoleID, '']);
    grid.getFilterPlugin().getFilter('RoleID').setActive(true);
    grid.getFilterPlugin().getFilter('Status').setValue([_Status, '']);
    grid.getFilterPlugin().getFilter('Status').setActive(true);
    
    //var store = App.stoBot;
    //store.clearFilter();
    //if (store.getCount() < _totalCount) {
    //    HQ.store.insertBlank(store, keysBot);
    //}
    //var listFilter = [];
    //store.filter(function (record) {
    //    if (record) {
    //        if (record.data['AppFolID'] == _AppFolID && record.data['RoleID'] == _RoleID && record.data['Status'] == _Status) {
    //            listFilter.push(record);
    //            return record;
    //        }
    //        else if (record.data['Handle'] == '' && record.data['AppFolID'] == '' && record.data['RoleID'] == '') {
    //            listFilter.push(record);
    //            return record;
    //        }
    //    }
    //});

};

var cboAppFolID_Change = function (value) {
    var k = value.displayTplData[0].DescrScreen;
    App.slmTopGrid.selected.items[0].set('DescrScreen', k);
};

//Top Grid
var grdTop_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keysTop);
};
var grdTop_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdTop, e, keysTop);

    _AppFolID = e.record.data.AppFolID;
    _RoleID = e.record.data.RoleID;
    _Status = e.record.data.Status;

    frmChange();
};
var grdTop_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdTop, e, keysTop);
};
var grdTop_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdTop);
    frmChange();
    //stoChangedTop(App.stoTop);
};

//Bot Grid
var grdBot_BeforeEdit = function (editor, e) {
    if (_AppFolID == '' || _RoleID == '' || _Status == '') return false;
    return HQ.grid.checkBeforeEdit(e, keysBot);
};
var grdBot_Edit = function (item, e) {

    //var store = App.stoBot;
    ////store.clearFilter();
    ////var data = Ext.data.Record();
    if (e.field == 'Handle') {
        if (e.value) {
            e.record.set('AppFolID',_AppFolID);
            e.record.set('RoleID', _RoleID);
            e.record.set('Status', _Status);
        }
    }
    HQ.grid.checkInsertKey(App.grdBot, e, keysBot);
    
    //var grid = App.grdBot;
    //grid.getStore().data
    ////var count = e.store.snapshot.items.length - 1;
    //var count = e.store.allData.items.length - 2;
    //var count2 = store.snapshot.items.length - 2;
    //if (e.record.index == undefined) {
    //    if (e.store.allData.items[count].data.AppFolID == '') {
    //        e.store.allData.items[count].data.AppFolID = _AppFolID;
    //        //store.data.items[store.getCount() - 2].data.AppFolID = _AppFolID;
    //        store.snapshot.items[count2].data.AppFolID = _AppFolID;
    //    }
    //    if (e.store.allData.items[count].data.RoleID == '') {
    //        e.store.allData.items[count].data.RoleID = _RoleID;
    //        //store.data.items[store.getCount() - 2].data.RoleID = _RoleID;
    //        store.snapshot.items[count2].data.RoleID = _RoleID;
    //    }
    //    if (e.store.allData.items[count].data.Status == '') {
    //        e.store.allData.items[count].data.Status = _Status;
    //        //store.data.items[store.getCount() - 2].data.Status = _Status;
    //        store.snapshot.items[count2].data.Status = _Status;
    //    }

    //}
    frmChange();
};
var grdBot_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdBot, e, ['Handle','AppFolID', 'RoleID', 'Status']);
};
var grdBot_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdBot);
    frmChange();
    //stoChangedBot(App.stoBot);
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////

var save = function () {
    if (App.frmMain.isValid()) {
        App.stoTop.clearFilter();
        _lstTopGrid = App.stoTop.getRecordsValues();
        App.stoBot.clearFilter();
        _lstBotGrid = App.stoBot.getRecordsValues();

        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            timeout: 10000000,
            url: 'SA02900/Save',
            params: {
                lstTopGrid: Ext.encode(_lstTopGrid),
                lstBotGrid: Ext.encode(_lstBotGrid)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                refresh('yes');
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

//var saveBOT = function () {
//    if (App.frmMain.isValid()) {
//        if (_AppFolID != '' && _RoleID != '' && _Status != '')
//            App.frmMain.submit({
//                //waitMsg: HQ.common.getLang("SavingData"),
//                url: 'SA02900/SaveBot',
//                params: {
//                    lstTopGrid: HQ.store.getData(App.stoTop),
//                    lstBotGrid: HQ.store.getData(App.stoBot),
//                    AppFolID: _AppFolID,
//                    RoleID: _RoleID,
//                    Status: _Status
//                },
//                success: function (msg, data) {
//                    //HQ.message.show(201405071);
//                    HQ.isFirstLoad = true;
//                    //App.stoTop.reload();
//                    App.stoBot.reload();
//                    //menuClick("refresh");
//                },
//                failure: function (msg, data) {
//                    HQ.message.process(msg, data, true);
//                }
//            });
//    }
//};

var deleteData = function (item) {
    if (item == "yes") {
        if (_focusNo == 0) {
            App.grdTop.deleteSelected();
            App.grdBot.store.removeAll();
            frmChange();
        }
        else {
            App.grdBot.deleteSelected();
            frmChange();
        }
    }
};
// Submit the deleted data into server side
//function deleteData(item) {
//    if (item == 'yes') {
//        if (_focusNo == 0) {
//            //App.grdTop.deleteSelected();
//            //stoChangedTop(App.stoTop);
//            App.frmMain.submit({
//                waitMsg: HQ.common.getLang('DeletingData'),
//                url: 'SA02900/DeleteAll',
//                timeout: 1800000,
//                params:{
//                    AppFolID : App.slmTopGrid.selected.items[0].data.AppFolID,
//                    RoleID: App.slmTopGrid.selected.items[0].data.RoleID,
//                    Status: App.slmTopGrid.selected.items[0].data.Status
//                },
//                success: function (action, data) {

//                    stoChangedTop(App.stoTop);

//                },
//                failure: function (action, data) {
//                    if (data.result.msgCode) {
//                        HQ.message.show(data.result.msgCode, data.result.msgParam, '');
//                    }
//                }
//            });
//        }
//        else {
//            App.grdBot.deleteSelected();
//            stoChangedBot(App.stoBot);
//        }

//    }
//};


//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
//var firstLoadTop = function () {
//    HQ.isFirstLoad = true;
//    App.stoTop.reload();
//}
////khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
//var stoChangedTop = function (sto) {
//    HQ.isChange = HQ.store.isChange(sto);
//    _topChange = HQ.store.isChange(sto);
//    HQ.common.changeData(_topChange, 'SA02900');
//};


////load lai trang, kiem tra neu la load lan dau thi them dong moi vao
//var stoLoadTop = function (sto) {
//    HQ.common.showBusy(false);
//    //HQ.isChange = HQ.store.isChange(sto);
//    _topChange = HQ.store.isChange(sto);
//    HQ.common.changeData(_topChange, 'SA02900');
//    if (HQ.isFirstLoad) {
//        if (HQ.isInsert) {
//            HQ.store.insertBlank(sto, keysTop);
//        }
//        HQ.isFirstLoad = false;
//    }

//    setTimeout(function () { App.slmTopGrid.select(_index) }, 300);
//};
//trước khi load trang busy la dang load data
//var stoBeforeLoadTop = function (sto) {
//    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
//};


//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
//var firstLoadBot = function () {
//    HQ.isFirstLoad = true;
//    //App.stoBot.reload();
//}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
//var stoChangedBot = function (sto) {
//    _botChange = true;
//    HQ.isChange = HQ.store.isChange(sto);
//    HQ.common.changeData(HQ.isChange, 'SA02900');
//};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao

////trước khi load trang busy la dang load data
//var stoBeforeLoadBot = function (sto) {
//    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
//};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        _topChange = false;
        _botChange = false;
        HQ.isFirstLoad = true;
        App.stoTop.reload();

    }
};
///////////////////////////////////

//var filters = Ext.create('Ext.ux.grid.GridFilters', {
//    filters: [{
//        type: 'list',
//        dataIndex: 'AppFolID',

//    }]
//});

//_index = App.slmTopGrid.getCurrentPosition().row;


//var filtersAux = [];

//keysTop.forEach(function (item) {
//    if (item == 'AppFolID') {
//        //store.filter(item, _AppFolID);
//        var s = [{property : item, value : _AppFolID}];
//        filtersAux.push(s);
//    }
//    else if (item == 'RoleID') {
//        var s = [{ property: item, value: _RoleID }];
//        filtersAux.push(s);

//    }
//    else if (item == 'Status') {
//        var s = [{ property: item, value: _Status }];
//        filtersAux.push(s);
//    }      
//});

//store.filterBy(function (record) {
//    return filtersAux.
//});

//HQ.grid.filterStore(store, 'AppFolID', _AppFolID);
//HQ.grid.filterStore(store, 'RoleID', _RoleID);
//HQ.grid.filterStore(store, 'Status', _Status);
//HQ.store.insertBlank(store, keysBot);
//var i = 0;

//store.filter(filtersAux[0]);
//if (_botChange == true) {
//    if (_topChange == false) {
//        //HQ.message.show(20150303, '', 'refresh');
//        App.slmTopGrid.select(_index);
//    }
//    else {
//        App.stoTop.reload();
//        App.slmTopGrid.select(_index);
//    }
//}
//else {

//    _index = App.slmTopGrid.getCurrentPosition().row;
//}
//grid.filters.filters.items[0].inputItem.value = _AppFolID;
//grid.filters.filters.items[1].inputItem.value = _RoleID;
//grid.filters.filters.items[2].inputItem.value = _Status;
//var gridFilter = grid.filters.getFilter();
//grid.filters.forEach(function (filter) {
//    filter.setActive(true);
//});
//grid.filters.filters.items.forEach(function (item) {
//    if (item) {
//    }
//});

//grid.filters.applyState(grid, listFilter);

//listFilter;

//var filtersState = grid.filters.saveState(grid, {});
//grid.filters.applyState(grid, filtersState);
//store.filters.add(i, new Ext.util.Filter({
//    property: 'AppFolID',
//                value: _AppFolID
//}));
//store.filters.add(i, new Ext.util.Filter({
//    property: 'RoleID',
//                value: _RoleID
//            }));
//    store.filters.add(i, new Ext.util.Filter({
//        property: 'Status',
//                    value: _Status
//    }));
//keysTop.forEach(function (item) {
//    i++;
//    if (item == 'AppFolID') {
//        store.filter(item, _AppFolID);
//        store.filters.add(i, new Ext.util.Filter({
//            property: item,
//            value: _AppFolID
//        }));
//        listFilter.push(new Ext.util.Filter({
//            property: item,
//            value: _AppFolID
//        }));
//    }
//    else if (item == 'RoleID') {
//        store.filter(item, _RoleID);
//        store.filters.add(i, new Ext.util.Filter({
//            property: item,
//            value: _RoleID
//        }));
//        listFilter.push(new Ext.util.Filter({
//            property: item,
//            value: _RoleID
//        }));
//    }
//    else if (item == 'Status') {
//        store.filter(item, _Status);
//        store.filters.add(i, new Ext.util.Filter({
//            property: item,
//            value: _Status
//        }));
//        listFilter.push(new Ext.util.Filter({
//            property: item,
//            value: _Status
//        }));
//    }
//    if (i == keysTop.length) {
//        HQ.store.insertBlank(store, keysBot);
//    }
//});
//store.filters.addAll(listFilter);