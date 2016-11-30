
////////////////////////////////////////////////////////////////////////
//// Declare///////////////////////////////////////////////////////////

var keys = ['BankAcct'];//khóa của record
var fieldsCheckRequire = ["BankAcct", "TrsfToBranchID", "TrsfToBankAcct", "TranAmt"];
var fieldsLangCheckRequire = ["Checking", "BranchID", "Checking", "TotAmt"];

var _Source = 0;
var _maxSource = 4;
var _SourceData = 2;//dem soure du lieu chinh cua chuong trinh, o man hinh nay la 3 source
var _maxSourceData = 2;
var _isLoadMaster = false;


////////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoadMaster = function (sto) {//các combobox có HQComboType= master thì sẽ nhảy vào hàm này để đếm source
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        App.cboBranchID.setValue(HQ.cpnyID);
        App.cboTrsfToBranchID.store.loadData(App.cboBranchID.store.data.items);
        App.cboTrsfToBankAcct.store.loadData(App.cboBankAcct.store.data.items);

        //App.cboBatNbr.getStore().reload();//sau khi load source master xong load den source combo cua store chinh
    }
};
var checkLoadDatacboBatNbr_Change = function (sto) {//load cac master phụ thuộc vào combo là key của màn hình rồi mới bind dữ liệu lên
    if (App.cboBatNbr.rawValue == "")//truong hop new moi
    {
        if (!HQ.isNew) bindHeader(true);//neu la dang new thi chi xoa ma, cho nhap ma lai
    } else {
        loadData();
    }
};
var cboBatNbr_LoadStore = function (sto) {
    App.cboBatNbr.suspendEvents();//tat su kien cho combobox
    App.cboBatNbr.setValue('');   //set giá trị '' để chút bắt sự kiện change cho combo     
    App.cboBatNbr.resumeEvents();
    App.cboBatNbr.store.loadData(App.cboBatNbr.store.data.items);
    if (HQ.BatNbr) {
        App.cboBatNbr.setValue(HQ.BatNbr);
    }
    else {
        bindHeader(true);
    }
}
var stoData_Load = function (sto) {
    _SourceData++;
    if (_SourceData == _maxSourceData) {
        bindHeader(false);
    }
}
var loadData = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    _SourceData = 0;//dem lai source data load
    App.stoHeader.reload();
    App.stoDetail.reload();

}
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
//Các sự kiện của chương trình

var firstLoad = function () {
    HQ.util.checkAccessRight(); //Kiểm tra quyền Insert Update Delete để disable các button trên topbar(Bắt buộc)
    App.frmMain.isValid(); //Require các field yêu cầu trên man hình
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    HQ.isFirstLoad = true;
    App.cboBatNbr.getStore().addListener('load', cboBatNbr_LoadStore);// add sự kiện khi load store của combo chính xong

    App.cboBranchID.getStore().addListener('load', checkLoadMaster);
    App.cboStatus.getStore().addListener('load', checkLoadMaster);
    App.cboHandle.getStore().addListener('load', checkLoadMaster);
    App.cboBankAcct.getStore().addListener('load', checkLoadMaster);
    _SourceData = 0;
    App.stoDetail.reload();
};
var frmChange = function () {
    if (App.stoHeader.getCount() > 0)
        App.frmMain.getForm().updateRecord();
    HQ.isChange = HQ.store.isChange(App.stoHeader) || HQ.store.isChange(App.stoDetail);
    HQ.common.changeData(HQ.isChange, 'CA10200');

    if (HQ.isChange) {
        App.cboBatNbr.setReadOnly(HQ.isChange);
        App.cboBranchID.setReadOnly(HQ.isChange);
    }
   
    //App.frmMain.resumeEvents();
};
var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == 'header') {
                HQ.isFirstLoad = true;
                HQ.combo.first(App.cboCpnyID, HQ.isChange);
            }
            else if (HQ.focus == 'grdDetail') {
                HQ.grid.first(App.grdDetail);
            }

            break;
        case "prev":
            if (HQ.focus == 'header') {
                HQ.isFirstLoad = true;
                HQ.combo.prev(App.cboCpnyID, HQ.isChange);
            }
            else if (HQ.focus == 'grdDetail') {
                HQ.grid.prev(App.grdDetail);
            }

            break;
        case "next":
            if (HQ.focus == 'header') {
                HQ.isFirstLoad = true;
                HQ.combo.next(App.cboCpnyID, HQ.isChange);
            }
            else if (HQ.focus == 'grdDetail') {
                HQ.grid.next(App.grdDetail);
            }

            break;
        case "last":
            if (HQ.focus == 'header') {
                HQ.isFirstLoad = true;
                HQ.combo.last(App.cboCpnyID, HQ.isChange);
            }
            else if (HQ.focus == 'grdDetail') {
                HQ.grid.last(App.grdDetail);
            }

            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain)
                    && HQ.store.checkRequirePass(App.stoDetail, keys, fieldsCheckRequire, fieldsLangCheckRequire)
                   ) {
                    if (HQ.store.getAllData(App.stoDetail, ['BankAcct'], [''], false).length == 2) {
                        HQ.message.show(2015020804, '', '');

                    }
                    else save();
                }
            }
            break;
        case "delete":
            if (HQ.isDelete && App.cboStatus.getValue() == "H") {
                if (HQ.focus == 'header') {
                    if (App.cboBatNbr.valueModels.length > 0) {
                        HQ.message.show(11, '', 'deleteData');
                    } else {
                        refresh('yes');
                    }
                }
                else if (HQ.focus == 'grdDetail') {
                    if (App.slmDetail.selected.items[0] != undefined) {
                        if (App.slmDetail.selected.items[0].data.EntryID != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdDetail)], 'deleteData', true);
                        }
                    }
                }

            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.focus == 'header') {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', 'refresh');
                    } else {
                        App.cboBatNbr.setValue('');
                    }
                }
                else if (HQ.focus == 'grdDetail' && App.cboStatus.getValue() == "H") {
                    HQ.grid.insert(App.grdDetail, keys);
                }

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
        default:
    }
};
var cboBranchID_Change = function (item, newValue, oldValue) {
    if (item.valueModels != null && App.cboBranchID.getValue() != null && !item.hasFocus) {//truong hop co chon branchid
        App.cboBatNbr.store.reload();
        //App.cboCustID.store.reload();
        //App.cboEmployeeID.store.reload();
    }
    else { //truong hop khong chon
        App.cboBatNbr.store.clearData();
    }

};
var cboBranchID_Select = function (item, newValue, oldValue) {
    if (item.hasFocus) {
        App.cboBatNbr.store.reload();
        //App.cboCustID.store.reload();
        //App.cboEmployeeID.store.reload();
    }
};

function cboBatNbr_Change(items, newValue, oldValue) {
    if ((items.rawValue == "" || !items.hasFocus)) {// ko phai la truong hop new, neu xoa combo thi new moi hoặc trường hợp ko focus combo co thay doi thi load lai source        
        checkLoadDatacboBatNbr_Change();
    }
}
function cboBatNbr_Select(items, newValue, oldValue) {
    checkLoadDatacboBatNbr_Change();
}


var grdDetail_BeforeEdit = function (editor, e) {
    if (App.cboStatus.getValue() != "H") return false;
    if (!e.record.data.LineRef) {
        e.record.data.LineRef = HQ.store.lastLineRef(App.stoDetail);
    }
    if (e.field == 'BankAcct') {
        App.cboBankAcct.store.clearFilter();
        if (e.record.data.TrsfToBranchID == App.cboBranchID.getValue())           
            App.cboBankAcct.store.loadData(filterStore(App.cboBankAcct.store, 'BankAcct', e.record.data.TrsfToBankAcct, false).data.items);
        else App.cboBankAcct.loadPage();                 
    }
    else if (e.field == 'TrsfToBankAcct') {
        App.cboTrsfToBankAcct.store.clearFilter();
        if (e.record.data.TrsfToBranchID == App.cboBranchID.getValue())
            App.cboTrsfToBankAcct.store.loadData(filterStore(App.cboTrsfToBankAcct.store, 'BankAcct', e.record.data.BankAcct, false).data.items);
        else App.cboTrsfToBankAcct.loadPage();                 
    }
    else if (e.field == 'TrsfToBranchID') {
        App.cboTrsfToBranchID.store.clearFilter();
        if (e.record.data.TrsfToBankAcct == e.record.data.BankAcct) {
            App.cboTrsfToBranchID.store.loadData(filterStore(App.cboTrsfToBranchID.store, 'BranchID', App.cboBranchID.getValue(), false).data.items);
        }
        else App.cboTrsfToBranchID.loadPage();
    }
    return HQ.grid.checkBeforeEdit(e, ["LineRef"]);
};
var grdDetail_Edit = function (item, e) {
    if (e.field == "EntryID") {
        var obj = HQ.store.findInStore(App.cboEntryID.store, ["EntryID", "RcptDisbFlg"], [e.value, App.cboType.getValue()]);
        if (obj) {
            e.record.set('TranDesc', obj.Descr);
        }
    }
    if (e.field == 'TranAmt')
        totalAmt();

    HQ.grid.checkInsertKey(App.grdDetail, e, keys);
    frmChange();
};
var grdDetail_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdDetail, e, ["LineRef"]);
};
var grdDetail_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdDetail);
    frmChange();
    totalAmt();
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
// Xử lí các hàm liên quan tới Controller hoặc các nút trên menu
var bindHeader = function (isNew) {
    App.frmMain.suspendEvents();
    HQ.isNew = false;
    HQ.common.lockItem(App.frmMain, false);
    App.cboBatNbr.forceSelection = true;
    App.cboBranchID.setReadOnly(false);
    App.cboBatNbr.setReadOnly(false);
    //các combo phu thuoc de phai clear het filter de lay gia tri cho dung
    App.cboHandle.getStore().clearFilter();
    App.cboBankAcct.getStore().clearFilter();

    if (isNew) {
        App.stoHeader.clearData();
        HQ.store.insertBlank(App.stoHeader);
        record = App.stoHeader.getAt(0);
        record.data.Status = 'H';
        record.data.DateEnt = HQ.bussinessDate;
        HQ.isNew = true; //record la new 
        HQ.isFirstLoad = true;
        App.cboBatNbr.forceSelection = false;
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        if (!App.cboBatNbr.hasFocus) App.cboBatNbr.focus(true); //focus ma khi tao moi

        App.stoHeader.commitChanges();

        App.stoDetail.clearData();
        App.grdDetail.view.refresh();
    }
    var record = App.stoHeader.getAt(0);
    //filter cho cac combo phu thuoc    
    App.cboHandle.store.filter('Status', record.data.Status);
    App.cboBankAcct.store.filter('BranchID', App.cboBranchID.getValue());
    App.cboHandle.setValue("N");
    App.frmMain.getForm().loadRecord(record);

    if ((!HQ.isInsert && HQ.isNew) || record.data.Status != 'H') {
        App.cboBatNbr.forceSelection = true;
        HQ.common.lockItem(App.frmMain, true);
    }
    else if ((!HQ.isUpdate && !HQ.isNew) || record.data.Status != 'H') {
        HQ.common.lockItem(App.frmMain, true);
    }
    else if (record.data.Status == 'H') { //Neu co quyen insert thi them dong moi cho luoi
        HQ.store.insertBlank(App.stoDetail, keys);
    }

    if (_isLoadMaster && record.data.Status == 'H') {
        HQ.common.showBusy(false);
        frmChange();
    }
    App.frmMain.resumeEvents();
    HQ.common.showBusy(false);
}
var save = function () {
    App.frmMain.getForm().updateRecord();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            url: 'CA10200/Save',
            params: {
                lstHeader: Ext.encode(App.stoHeader.getRecordsValues()),
                lstDetail: HQ.store.getData(App.stoDetail)
            },
            success: function (msg, data) {
                HQ.message.process(msg, data, true);
                if (HQ.isNew || App.cboHandle.getValue() == "C") {
                    HQ.BatNbr = data.result.data;
                    App.cboBatNbr.store.reload();//load lai combo de co du lieu;
                } else {
                    _SourceData = 2;
                    refresh('yes')
                }
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
                App.frmMain.updateRecord();
                App.frmMain.submit({
                    waitMsg: HQ.common.getLang("DeletingData"),
                    url: 'CA10200/DeleteAll',
                    timeout: 7200,
                    success: function (msg, data) {
                        HQ.BatNbr = '';
                        App.cboBatNbr.getStore().reload();
                    },
                    failure: function (msg, data) {
                        HQ.message.process(msg, data, true);
                    }
                });
            }

        }
        else if (HQ.focus == 'grdDetail') {
            App.grdDetail.deleteSelected();
            frmChange();
        }

    }
};
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        if (!App.cboBatNbr.getValue()) {
            bindHeader(true);
        }
        else {
            if (_SourceData >= 2) {
                loadData();
            }
        }
    }
};
/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
//Các hàm tính toán, hỗ trợ cho các sự kiện của chương trình
var totalAmt = function () {
    var allData = App.stoDetail.snapshot || App.stoDetail.allData || App.stoDetail.data;
    var amt = 0;
    allData.each(function (item) {
        amt += item.data.TranAmt;
    });
    App.txtTotal.setValue(amt);
}
var filterStore = function (store, field, value, isEqual) {
    if (isEqual == undefined || isEqual == true) {
        store.filterBy(function (record) {
            if (record) {
                if (record.data[field].toString().toLowerCase() == (HQ.util.passNull(value).toLowerCase())) {
                    return record;
                }
            }
        });
    } else {
        store.filterBy(function (record) {
            if (record) {
                if (record.data[field].toString().toLowerCase() != (HQ.util.passNull(value).toLowerCase())) {
                    return record;
                }
            }
        });
    }
    return store;
}
/////////////////////////////////////////////////////////////////////////




