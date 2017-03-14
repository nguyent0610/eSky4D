//// Declare //////////////////////////////////////////////////////////
var keys = ['RoleID'];
var fieldsCheckRequire = ["RoleID","Desc"];
var fieldsLangCheckRequire = ["RoleID","Desc"];

var _Source = 0;
var _maxSource = 3;
var _SourceData = 0;
var _maxSourceData = 4;


///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _Source = 0;      
        HQ.common.showBusy(false);
        App.cboBatNbr.getStore().reload();
    }
};
var cboBatNbr_LoadStore = function (sto) {
    App.cboBatNbr.suspendEvents();//tat su kien cho combobox
    App.cboBatNbr.setValue('');   //set giá trị '' để chút bắt sự kiện change cho combo     
    App.cboBatNbr.resumeEvents();
    if (HQ.batNbr) {
        App.cboBatNbr.setValue(HQ.batNbr);
    }
    else {
        bindHeader(true);
    }
}

var stoData_Load = function (sto) {
    _SourceData++;
    if (_SourceData == _maxSourceData) {
        setTimeout(function () {
            App.slmOrder.select(-1);
            App.slmOrder.select(0);
        }, 100);
     
        if (App.stoHeader.data.items.length > 0) {
           
            bindHeader(false);
        }
        else
            bindHeader(true);
    }
};
var checkLoadDatacboBatNbr_Change = function () {//load cac master phụ thuộc vào combo là key của màn hình rồi mới bind dữ liệu lên
    if (App.cboBatNbr.rawValue == "")//truong hop new moi
    {
        if (!HQ.isNew) bindHeader(true);//neu la dang new thi chi xoa ma, cho nhap ma lai
    } else {
        loadData();
    }

};
var loadData = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    _SourceData = 0;//dem lai source data load
    App.stoHeader.reload();
    App.stoOrder.reload();
    App.stoDet.reload();
    App.stoDelivery.reload();
   
}
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
var menuClick = function (command) {
    switch (command) {
        case "first":
                HQ.grid.first(App.grdSYS_Role);
            break;
        case "prev":
                HQ.grid.prev(App.grdSYS_Role);
            break;
        case "next":
                HQ.grid.next(App.grdSYS_Role);
            break;
        case "last":
                HQ.grid.last(App.grdSYS_Role);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                refresh('yes');
            }          
            break;
        case "new":
            if (HQ.isInsert) {
                App.cboBatNbr.setValue('');
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.cboStatus.getValue() == 'H' && App.cboBatNbr.getValue())
                HQ.message.show(11, '', 'deleteData');
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain)) {
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
var btnPopupOk_Click = function () {
    if (!Ext.isEmpty(App.cboPopupCpny.getValue())) {
        App.winPopup.hide();
        window.location.href = 'OM10800?branchID=' + App.cboPopupCpny.getValue();
    } else {
        HQ.message.show(1000, [HQ.common.getLang('branchid')], '', true);
    }
}
var firstLoad = function () {
    App.txtBranchID.setValue(HQ.branchID);
    HQ.util.checkAccessRight();
    App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData")); 
    App.cboHandle.getStore().addListener('load', checkLoad);
    App.cboStatus.getStore().addListener('load', checkLoad);   
    App.cboLicensePlate.getStore().addListener('load', checkLoad);


    App.cboHandle.getStore().reload();
    App.cboStatus.getStore().reload();
    App.cboLicensePlate.getStore().reload();

    App.cboBatNbr.getStore().addListener('load', cboBatNbr_LoadStore);
};
var frmChange = function () {    
    HQ.isChange = HQ.store.isChange(App.stoDet) || HQ.store.isChange(App.stoDelivery);
    HQ.common.changeData(HQ.isChange, 'OM10800');//co thay doi du lieu gan * tren tab title header
    if (App.cboStatus.getValue() == 'H') {
        if (HQ.isChange) {
            App.btnLoad.disable();
        } else App.btnLoad.enable();
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
//khi nhan combo xo ra, neu da thay doi thi ko xo ra
function cboBatNbr_Expand(sender, value) {
    if (HQ.isChange) {
        App.cboBatNbr.collapse();
    }
};
var slmOrder_Select = function (slm, selRec, idx, eOpts) {   
    App.grdDet.store.filterBy(function (record) {
        if (record.data.OrderNbr == selRec.data.OrderNbr) {
            return record;
        }
    });
}
var chkSelectHeaderOrder_Change = function (chk, newValue, oldValue, eOpts) {
    if (App.cboStatus.getValue() != 'H') return;
    App.stoOrder.suspendEvents();
    var allData = App.stoOrder.snapshot || App.stoOrder.allData || App.stoOrder.data;
    allData.each(function (record) {
        record.set('Selected', chk.value);
    });
    App.stoOrder.resumeEvents();
    App.grdOrder.view.refresh();
}
var chkSelectHeaderDelivery_Change = function (chk, newValue, oldValue, eOpts) {
    if (App.cboStatus.getValue() != 'H') return;
    App.stoDelivery.suspendEvents();
    var allData = App.stoDelivery.snapshot || App.stoDelivery.allData || App.stoDelivery.data;
    allData.each(function (record) {
        record.set('Selected',chk.value);
    });
    App.stoDelivery.resumeEvents();
    App.grdDelivery.view.refresh();
}
var grdOrder_BeforeEdit = function (editor, context, eOpts) {
    if (App.cboStatus.getValue() != 'H') return false;
}
var grdDelivery_BeforeEdit = function (editor, context, eOpts) {
    if (App.cboStatus.getValue() != 'H') return false;
    var obj = HQ.store.findRecord(App.stoDelivery, ["Selected"], [true]);
    if (obj && obj.data.SlsPerID != context.record.data.SlsPerID) {
        obj.set('Selected', false);
    }
}
var grdDelivery_Edit = function (editor, context, eOpts) {
    //var obj = HQ.store.findRecord(App.stoDelivery, ["Selected"], [true]);
    //if (obj && obj.data.SlsPerID != context.record.data.SlsPerID) {
    //    obj.set('Selected', false);
    //}
}

var btnLoad_click = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
   
    _SourceData = 1;
    App.stoOrder.reload();
    App.stoDet.reload();
    App.stoDelivery.reload();
}
//// Process Data ///////////////////////////////////////////////////////
// Xử lí các hàm liên quan tới Controller hoặc các nút trên menu
var bindHeader = function (isNew) {
    App.frmMain.suspendEvents();
    HQ.isNew = false;
    HQ.common.lockItem(App.frmMain, false);
    App.cboBatNbr.forceSelection = true;
    //các combo phu thuoc de phai clear het filter de lay gia tri cho dung
    App.cboHandle.getStore().clearFilter();  
    if (isNew) {
        _SourceData = 1;
        App.stoOrder.reload();
        App.stoDet.reload();
        App.stoDelivery.reload();

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
    }
    else if(!App.cboBatNbr.getValue()) App.frmMain.getForm().updateRecord();
    var record = App.stoHeader.getAt(0);
    //filter cho cac combo phu thuoc    
    App.cboHandle.store.filter('Status', record.data.Status);
    App.cboHandle.setValue('N');
    App.frmMain.getForm().loadRecord(record);

    if (!HQ.isInsert && HQ.isNew) {
        App.cboBatNbr.forceSelection = true;
        HQ.common.lockItem(App.frmMain, true);
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }
    
    if (record.data.Status != 'H') {
        App.cboBatNbr.forceSelection = true;
        HQ.common.lockItem(App.frmMain, true);
    }

    HQ.common.showBusy(false);
    frmChange();
    
    App.frmMain.resumeEvents();

}
var save = function () {
    var objOrder = HQ.store.findRecord(App.stoOrder, ['Selected'], [true]);
    if (!objOrder) {
        HQ.message.show(1000, App.tabInfo.items.items[0].title, '');
        return;
    }
    var objDelivery = HQ.store.findRecord(App.stoDelivery, ['Selected'], [true]);
    if (!objDelivery) {
        HQ.message.show(1000, App.tabInfo.items.items[1].title, '');
        return;
    }
    App.frmMain.getForm().updateRecord();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'OM10800/Save',
            params: {
                lstHeader: Ext.encode(App.stoHeader.getRecordsValues()),
                lstOrder: HQ.store.getData(App.stoOrder),
                lstDelivery: HQ.store.getData(App.stoDelivery)
            },
            success: function (msg, data) {
                HQ.message.process(msg, data, true);
                if (!App.cboBatNbr.getValue()) {
                    _SourceByCboCpnyID = 1;
                    HQ.batNbr = data.result.data.BatNbr;
                    App.cboBatNbr.store.reload();//load lai combo de co du lieu;
                } else {
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
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("DeletingData"),
            url: 'OM10800/DeleteHeader',
            timeout: 7200,
            success: function (msg, data) {
                _SourceByCboCpnyID = 1;
                HQ.batNbr = '';
                App.cboBatNbr.store.reload();//load lai combo de co du lieu;               
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};
function refresh(item) {
    if (item == 'yes') {
        HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
        loadData();
    }
};
/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
//Các hàm tính toán, hỗ trợ cho các sự kiện của chương trình
var renderRowNumber = function (value, meta, record) {
    return App.stoDet.data.indexOf(record) + 1;
}
var stringFilter = function (record) {
    return HQ.grid.filterString(record, this);
}
/////////////////////////////////////////////////////////////////////////

