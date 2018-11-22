//// Declare //////////////////////////////////////////////////////////

var keys ;
var fieldsCheckRequire ;
var fieldsLangCheckRequire;
var _Channel = '';
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":

            HQ.grid.first(App.grdChannel);
            break;
        case "prev":
            HQ.grid.prev(App.grdChannel);
            break;
        case "next":
            HQ.grid.next(App.grdChannel);
            break;
        case "last":
            HQ.grid.last(App.grdChannel);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isFirstLoad = true;
                App.stoChannel.reload();
            }
           
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdChannel, keys);
            }
            break;
        case "delete":
            App.stocheckDelete.reload();
            if (HQ.isDelete) {
                if (App.slmChannel.selected.items[0] != undefined) {
                    setTimeout(function () {
                        if (_delete == 1) {
                            HQ.message.show(2018112060, [_Channel], 'deleteData', true);
                        }
                        else {
                            if (App.slmChannel.selected.items[0].data.Code != "") {
                                HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdChannel)], 'deleteData', true);
                            }
                        }
                        HQ.common.showBusy(false);
                    }, 1000)
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoChannel, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.isChange) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};

var grdChannel_BeforeEdit = function (editor, e) {
    _Channel = e.record.data.Code;
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdChannel_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdChannel, e, keys);
};
var grdChannel_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdChannel, e, keys);
};
var grdChannel_Reject = function (record) {
    
    HQ.grid.checkReject(record, App.grdChannel);
    stoChanged(App.stoChannel);
};
/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'AR21100/Save',
            params: {
                lstChannel: HQ.store.getData(App.stoChannel)
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

var deleteData = function (item) {
    if (item == "yes") {
        App.grdChannel.deleteSelected();
        stoChanged(App.stoChannel);
    }
};

function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoChannel.reload();
    }
};
/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.changeData(false, 'AR21100');//khi dong roi gan lai cho change la false
        HQ.common.close(this);
    }
};
var _Source = 0;
var _maxSource = 1;
var checkLoad = function () {
    _Source += 1;
    if (_Source == _maxSource) {
        _Source = 0;
        HQ.isFirstLoad = true;
        App.stoChannel.reload();
    }
};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    App.cboChannelType.getStore().addListener('load', checkLoad);
    
    if (HQ.channelTypeView == false)
    {
        App.colChannelType.hide();
        keys = ["Code"];
        fieldsCheckRequire = ["Code", "Descr"];
        fieldsLangCheckRequire = ["Code", "Descr"];
    }
    else
    {
        keys = ['Code'];
        fieldsCheckRequire = ["Code", "Descr", "Type"];
        fieldsLangCheckRequire = ["Code", "Descr", "Type"];
        App.colChannelType.show();
    }
    checkLoad();
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'AR21100');
};
//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'AR21100');
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
};
//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

var renderTypeName = function (value, metaData, rec, rowIndex, colIndex, store) {    
    var record = App.cboChannelType.findRecord("Code", value);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};
/////////////////////////////////////////////////////////////////////////
var stocheckDelete_Load = function () {
    _delete = App.stocheckDelete.data.items[0].data.Result;
}
var slmTerritory_Channel = function (slm, selRec, idx, eOpts) {
    _Channel = selRec.data.Code;
}







