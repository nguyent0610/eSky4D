//// Declare //////////////////////////////////////////////////////////
var keys = ['Territory', 'DiscID', 'DiscSeq'];
var fieldsCheckRequire = ['Territory', 'DiscID', 'DiscSeq', 'StartDate', 'EndDate','Descr','Poster'];
var fieldsLangCheckRequire = ['Territory', 'DiscID', 'DiscSeq', 'StartDate', 'EndDate', 'Descr', 'Poster'];

var _Source = 0;
var _maxSource = 2;
var _isLoadMaster = false;
var imagName;
var flag = false;
var _row;
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoOM_DiscountInfor.reload();
        HQ.common.showBusy(false);
    }
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
                HQ.grid.first(App.grdOM_DiscountInfor);
            break;
        case "prev":
                HQ.grid.prev(App.grdOM_DiscountInfor);
            break;
        case "next":
                HQ.grid.next(App.grdOM_DiscountInfor);
            break;
        case "last":
                HQ.grid.last(App.grdOM_DiscountInfor);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoOM_DiscountInfor.reload();
            }          
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdOM_DiscountInfor, keys);
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.slmOM_DiscountInfor.selected.items[0] != undefined) {
                    if (App.slmOM_DiscountInfor.selected.items[0].data.CpnyID != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdOM_DiscountInfor)], 'deleteData', true);
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.store.checkRequirePass(App.stoOM_DiscountInfor, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
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
    App.cboTerritory.getStore().addListener('load', checkLoad);
    App.cboClassID.getStore().addListener('load', checkLoad);
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoOM_DiscountInfor);
    HQ.common.changeData(HQ.isChange, 'OM21800');//co thay doi du lieu gan * tren tab title header
    
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoOM_DiscountInfor_Load = function (sto) {
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            //HQ.store.insertBlank(sto, keys);
            HQ.store.insertRecord(sto, keys, { StartDate: HQ.bussinessDate, EndDate: HQ.bussinessDate });
        }
        HQ.isFirstLoad = false; //sto load cuoi se su dung
    }
    var Territory = [];
    var ClassID = [];
    App.cboTerritory.getStore().data.each(function (item) {
        if (Territory.indexOf(item.data.Territory) == -1) {
            Territory.push([item.data.Territory, item.data.Descr]);
        }
    });
    App.cboClassID.getStore().data.each(function (item) {
        if (ClassID.indexOf(item.data.ClassID) == -1) {
            ClassID.push([item.data.ClassID, item.data.Descr]);
        }
    });
    filterFeature = App.grdOM_DiscountInfor.filters;
    colAFilter = filterFeature.getFilter('Territory');
    colAFilter.menu = colAFilter.createMenu({
        options: Territory
    });
    colAFilter1 = filterFeature.getFilter('ClassID');
    colAFilter1.menu = colAFilter1.createMenu({
        options: ClassID
    });

    frmChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

var grdOM_DiscountInfor_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdOM_DiscountInfor_Edit = function (item, e) {
    if (e.field == 'Territory') {
        if (e.value) {
            var recordT = App.cboTerritoryOM21800_pcTerritory.findRecord("Territory", e.value);
            if (recordT)
                e.record.set('DescrTerr', recordT.data.Descr);
            else
                e.record.set('DescrTerr', '');
        }
    }
    else if (e.field == 'ClassID') {
        if (e.value) {
            var recordTT = App.cboClassIDOM21800_pcClassID.findRecord("ClassID", e.value);
            if (recordTT)
                e.record.set('DescrClass', recordTT.data.Descr);
            else
                e.record.set('DescrClass', '');
        }
    }
    else if (e.field == 'StartDate' || e.field == 'EndDate') {
        if (dates.compare(e.record.data.StartDate, e.record.data.EndDate) == 1) {
            HQ.message.show(201312203, e.value);
            e.record.set(e.field, e.originalValue);
            return false;
        }
    }
    //Kiem tra cac key da duoc nhap se insert them dong moi
    checkInsertKey(App.grdOM_DiscountInfor, e, keys);
    frmChange();
};

var checkInsertKey =  function (grd, e, keys) {
    if (keys.indexOf(e.field) != -1) {
        if (e.value != '')
            //HQ.store.insertBlank(grd.getStore(), keys);
            HQ.store.insertRecord(grd.getStore(), keys, { StartDate: HQ.bussinessDate, EndDate: HQ.bussinessDate });
    }
};

var grdOM_DiscountInfor_ValidateEdit = function (item, e) {
    //ko cho nhap key co ki tu dac biet, va kiem tra trung du lieu
    return checkValidateEdit(App.grdOM_DiscountInfor, e, keys);
};

var checkValidateEdit = function (grd, e, keys) {
    if (keys.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(grd, e, keys)) {
            HQ.message.show(1112, e.value);
            return false;
        }
    }
};

var grdOM_DiscountInfor_Reject = function (record) {
    //reject dong thay doi du lieu ve ban dau
    HQ.grid.checkReject(record, App.grdOM_DiscountInfor);
    frmChange();
};

////Function menuClick
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'OM21800/Save',
            params: {
                lstOM_DiscountInfor: HQ.store.getData(App.stoOM_DiscountInfor)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isFirstLoad = true;
                App.stoOM_DiscountInfor.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};
var deleteData = function (item) {
    if (item == "yes") {
        App.grdOM_DiscountInfor.deleteSelected();
        frmChange();
    }
};
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoOM_DiscountInfor.reload();
    }
};
/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////

var renderTerritory = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboTerritoryOM21800_pcTerritory.findRecord("Territory", rec.data.Territory);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};

var renderClassID = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboClassIDOM21800_pcClassID.findRecord("ClassID", rec.data.ClassID);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};

var dates = {
    convert: function (d) {
        return (
            d.constructor === Date ? d :
            d.constructor === Array ? new Date(d[0], d[1], d[2]) :
            d.constructor === Number ? new Date(d) :
            d.constructor === String ? new Date(d) :
            typeof d === "object" ? new Date(d.year, d.month, d.date) :
            NaN
        );
    },
    compare: function (a, b) {
        return (
            isFinite(a = this.convert(a).valueOf()) &&
            isFinite(b = this.convert(b).valueOf()) ?
            (a > b) - (a < b) :
            NaN
        );
    }
}

////////////////////////////////////////////////////////////////////////////////////////
var fupImage_change = function (fup, newValue, oldValue, eOpts, record) {
    if (fup.value) {
        var ext = fup.value.split(".").pop().toLowerCase();
        if (ext == "jpg" || ext == "png" || ext == "gif" || ext == "pdf" || ext == "mp4") {
            imagName = fup.value;
            _row.set('Poster', imagName);
            flag = true;
        }
        else {
            HQ.message.show(148, '', '');
        }
    }
};

var btnImage_Click = function (record) {
    _row = record;
    var uploadField = Ext.getCmp('fupImages');
    uploadField.fileInputEl.dom.click(record);
};
var btnDelete_Click = function (record) {
    if (record.data.Poster)
        record.set('Poster', '');
};