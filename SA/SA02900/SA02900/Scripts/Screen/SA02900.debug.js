//// Declare //////////////////////////////////////////////////////////
var _index = 0;
var keysTop = ['AppFolID', 'RoleID', 'Status'];
var fieldsCheckRequireTop = ["AppFolID", "RoleID", "Status", "LangStatus"];
var fieldsLangCheckRequireTop = ["AppFolID", "RoleID", "Status", "LangStatus"];

var keysBot = ['Handle'];
var fieldsCheckRequireBot = ["Handle"];
var fieldsLangCheckRequireBot = ["Handle"];

var _focusNo = 0;
var _topChange = false;

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
///////////////////////////////////////////////////////////////////////
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
                _topChange = false;
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoTop.reload();
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
            if (_focusNo == 0) {
                if (App.slmTopGrid.selected.items[0] != undefined) {
                    if (HQ.isDelete) {
                        HQ.message.show(11, '', 'deleteData');
                    }
                }
            }
            else {
                if (App.slmBotGrid.selected.items[0] != undefined) {
                    if (HQ.isDelete) {
                        HQ.message.show(11, '', 'deleteData');
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
            HQ.common.close(this);
            break;
    }

};

var GridTop_Change = function (sender, e) {
    HQ.isFirstLoad = true;
    if (HQ.isChange || _topChange) {
        HQ.message.show(20150303, '', 'refresh');
        App.slmTopGrid.select(_index);
    } else {
        App.stoBot.reload();
        _index = App.slmTopGrid.selected.items[0].index;
    }

    //if (_botChange) {
    //    HQ.message.show(20150303, '', 'refresh');
    //    App.slmTopGrid.select(_index);
    //}
    //HQ.isFirstLoad = true;
    //App.stoBot.reload();
    //_index = App.slmTopGrid.selected.items[0].index;
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
};
var grdTop_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdTop, e, keysTop);
};
var grdTop_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdTop);
    stoChangedTop(App.stoTop);
};

//Bot Grid
var grdBot_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keysBot);
};
var grdBot_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdBot, e, keysBot);
};
var grdBot_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdBot, e, keysBot);
};
var grdBot_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdBot);
    stoChangedBot(App.stoBot);
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'SA02900/Save',
            params: {
                lstTopGrid: HQ.store.getData(App.stoTop),
                lstBotGrid: HQ.store.getData(App.stoBot),
                AppFolID: App.slmTopGrid.selected.items[0] == undefined ? '' : App.slmTopGrid.selected.items[0].data.AppFolID,
                RoleID: App.slmTopGrid.selected.items[0] == undefined ? '' : App.slmTopGrid.selected.items[0].data.RoleID,
                Status: App.slmTopGrid.selected.items[0] == undefined ? '' : App.slmTopGrid.selected.items[0].data.Status
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isChange = false;
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
            App.grdTop.deleteSelected();
            stoChangedTop(App.stoTop);
        }
        else {
            App.grdBot.deleteSelected();
            stoChangedBot(App.stoBot);
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
var firstLoadTop = function () {
    HQ.isFirstLoad = true;
    App.stoTop.reload();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChangedTop = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    _topChange = HQ.store.isChange(sto);
    HQ.common.changeData(_topChange, 'SA02900');
};


//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoadTop = function (sto) {
    HQ.common.showBusy(false);
    //HQ.isChange = HQ.store.isChange(sto);
    _topChange = HQ.store.isChange(sto);
    HQ.common.changeData(_topChange, 'SA02900');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keysTop);
        }
        HQ.isFirstLoad = false;
    }
    App.slmTopGrid.select(_index);
};
//trước khi load trang busy la dang load data
var stoBeforeLoadTop = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};


//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoadBot = function () {
    HQ.isFirstLoad = true;
    //App.stoBot.reload();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChangedBot = function (sto) {
    _botChange = true;
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA02900');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoadBot = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    _botChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'SA02900');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keysBot);
        }
        HQ.isFirstLoad = false;
    }
};
//trước khi load trang busy la dang load data
var stoBeforeLoadBot = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        _topChange = false;
        HQ.isFirstLoad = true;
        App.stoTop.reload();
    }
};
///////////////////////////////////



////// Declare //////////////////////////////////////////////////////////
//var _index = 0;
//var keysTop = ['AppFolID','RoleID','Status'];
//var fieldsCheckRequireTop = ["AppFolID", "RoleID", "Status"];
//var fieldsLangCheckRequireTop = ["AppFolID", "RoleID", "Status"];

//var keysBot = ['Handle'];
//var fieldsCheckRequireBot = ["Handle"];
//var fieldsLangCheckRequireBot = ["Handle"];

//var _focusNo = 0;

//var pnl_render = function (cmd) {
//    cmd.getEl().on('mousedown', function () {
//        if (cmd.id == 'pnlTop') {
//            _focusNo = 0; //pnlTop
//        }
//        else {
//            _focusNo = 1; //pnlBot
//        }
//    });
//};
/////////////////////////////////////////////////////////////////////////
////// Store /////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
////// Event /////////////////////////////////////////////////////////////

//var menuClick = function (command) {
//    switch (command) {
//        case "first":
//            if (_focusNo == 0) {
//                HQ.grid.first(App.grdTop);
//            }
//            else {
//                HQ.grid.first(App.grdBot);
//            }
//            break;
//        case "prev":
//            if (_focusNo == 0) {
//                HQ.grid.prev(App.grdTop);
//            }
//            else {
//                HQ.grid.prev(App.grdBot);
//            }
//            break;
//        case "next":
//            if (_focusNo == 0) {
//                HQ.grid.next(App.grdTop);
//            }
//            else {
//                HQ.grid.next(App.grdBot);
//            }
//            break;
//        case "last":
//            if (_focusNo == 0) {
//                HQ.grid.last(App.grdTop);
//            }
//            else {
//                HQ.grid.last(App.grdBot);
//            }
//            break;
//        case "refresh":
//            HQ.isFirstLoad = true;
//            App.stoTop.reload();
//            break;
//        case "new":
//            if (HQ.isInsert) {
//                if (_focusNo == 0) {
//                    HQ.grid.insert(App.grdTop, keysTop);
//                }
//                else {
//                    HQ.grid.insert(App.grdBot, keysBot);
//                }
//            }
//            break;
//        case "delete":
//            if (_focusNo == 0) {
//                if (App.slmTopGrid.selected.items[0] != undefined) {
//                    if (HQ.isDelete) {
//                        HQ.message.show(11, '', 'deleteData');
//                    }
//                }
//            }
//            else {
//                if (App.slmBotGrid.selected.items[0] != undefined) {
//                    if (HQ.isDelete) {
//                        HQ.message.show(11, '', 'deleteData');  
//                    }
//                }
//            }
            
//            break;
//        case "save":
//            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
//                if (HQ.store.checkRequirePass(App.stoTop, keysTop, fieldsCheckRequireTop, fieldsLangCheckRequireTop)
//                    && HQ.store.checkRequirePass(App.stoBot, keysBot, fieldsCheckRequireBot, fieldsLangCheckRequireBot)) {
//                    save();
//                }
//            }
//            break;
//        case "print":
//            break;
//        case "close":
//            if (HQ.isChange) {
//                HQ.message.show(5, '', 'askClose');
//            } else {
//                HQ.common.close(this);
//            }
//            break;
//    }

//};

//var GridTop_Change = function (sender, e) {
//    HQ.isFirstLoad = true;
//    App.stoBot.reload();
//    _index = App.slmTopGrid.selected.items[0].index;
//};

//var cboAppFolID_Change = function (value) {
//    var k = value.displayTplData[0].DescrScreen;
//    App.slmTopGrid.selected.items[0].set('DescrScreen', k);
//};

////Top Grid
//var grdTop_BeforeEdit = function (editor, e) {
//    return HQ.grid.checkBeforeEdit(e, keysTop);
//};
//var grdTop_Edit = function (item, e) {
//    HQ.grid.checkInsertKey(App.grdTop, e, keysTop);
//};
//var grdTop_ValidateEdit = function (item, e) {
//    return HQ.grid.checkValidateEdit(App.grdTop, e, keysTop);
//};
//var grdTop_Reject = function (record) {
//    HQ.grid.checkReject(record, App.grdTop);
//    stoChangedTop(App.stoTop);
//};

////Bot Grid
//var grdBot_BeforeEdit = function (editor, e) {
//    return HQ.grid.checkBeforeEdit(e, keysBot);
//};
//var grdBot_Edit = function (item, e) {
//    HQ.grid.checkInsertKey(App.grdBot, e, keysBot);
//};
//var grdBot_ValidateEdit = function (item, e) {
//    return HQ.grid.checkValidateEdit(App.grdBot, e, keysBot);
//};
//var grdBot_Reject = function (record) {
//    HQ.grid.checkReject(record, App.grdBot);
//    stoChangedBot(App.stoBot);
//};

///////////////////////////////////////////////////////////////////////////
////// Process Data ///////////////////////////////////////////////////////
//var save = function () {
//    if (App.frmMain.isValid()) {
//        App.frmMain.submit({
//            waitMsg: HQ.common.getLang("SavingData"),
//            url: 'SA02900/Save',
//            params: {
//                lstTopGrid: HQ.store.getData(App.stoTop),
//                lstBotGrid: HQ.store.getData(App.stoBot),
//                AppFolID: App.slmTopGrid.selected.items[0]==undefined? '' : App.slmTopGrid.selected.items[0].data.AppFolID,
//                RoleID: App.slmTopGrid.selected.items[0] == undefined ? '' : App.slmTopGrid.selected.items[0].data.RoleID,
//                Status: App.slmTopGrid.selected.items[0] == undefined ? '' : App.slmTopGrid.selected.items[0].data.Status
//            },
//            success: function (msg, data) {
//                HQ.message.show(201405071);
//                menuClick("refresh");
//            },
//            failure: function (msg, data) {
//                HQ.message.process(msg, data, true);
//            }
//        });
//    }
//};

//var deleteData = function (item) {
//    if (item == "yes") {
//        if (_focusNo == 0) {
//            App.grdTop.deleteSelected();
//            stoChangedTop(App.stoTop);
//        }
//        else {
//            App.grdBot.deleteSelected();
//            stoChangedBot(App.stoBot);
//        }
//    }
//};


///////////////////////////////////////////////////////////////////////////
////// Other Functions ////////////////////////////////////////////////////
//var askClose = function (item) {
//    if (item == "no" || item == "ok") {
//        HQ.common.changeData(false, 'SA02900');//khi dong roi gan lai cho change la false
//        HQ.common.close(this);
//    }
//};
////load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
//var firstLoadTop = function () {
//    HQ.isFirstLoad = true;
//    App.stoTop.reload();
//}
////khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
//var stoChangedTop = function (sto) {
//    HQ.isChange = HQ.store.isChange(sto);
//    HQ.common.changeData(HQ.isChange, 'SA02900');
//};


////load lai trang, kiem tra neu la load lan dau thi them dong moi vao
//var stoLoadTop = function (sto) {
//    HQ.common.showBusy(false);
//    HQ.isChange = HQ.store.isChange(sto);
//    HQ.common.changeData(HQ.isChange, 'SA02900');
//    if (HQ.isFirstLoad) {
//        if (HQ.isInsert) {
//            HQ.store.insertBlank(sto, keysTop);
//        }
//        HQ.isFirstLoad = false;
//    }
//   App.slmTopGrid.select(_index);
//};
////trước khi load trang busy la dang load data
//var stoBeforeLoadTop = function (sto) {
//    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
//};


////load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
//var firstLoadBot = function () {
//    HQ.isFirstLoad = true;
//    //App.stoBot.reload();
//}
////khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
//var stoChangedBot = function (sto) {
//    HQ.isChange = HQ.store.isChange(sto);
//    HQ.common.changeData(HQ.isChange, 'SA02900');
//};
////load lai trang, kiem tra neu la load lan dau thi them dong moi vao
//var stoLoadBot = function (sto) {
//    HQ.common.showBusy(false);
//    HQ.isChange = HQ.store.isChange(sto);
//    HQ.common.changeData(HQ.isChange, 'SA02900');
//    if (HQ.isFirstLoad) {
//        if (HQ.isInsert) {
//            HQ.store.insertBlank(sto, keysBot);
//        }
//        HQ.isFirstLoad = false;
//    }
//};
////trước khi load trang busy la dang load data
//var stoBeforeLoadBot = function (sto) {
//    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
//};
