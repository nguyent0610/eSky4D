//// Declare //////////////////////////////////////////////////////////
var _invtID='';
var _classID = ''
var focus = 'header';
var _beginStatus = 'H';
var keyCompany = ['CpnyID'];
var fieldsCheckRequireCompany = ["CpnyID"];
var fieldsLangCheckRequireCompany = ["CpnyID"];

var keyPrice = ['InvtID', 'SlsUnit'];
var fieldsCheckRequirePrice = ["InvtID", 'SlsUnit'];
var fieldsLangCheckRequirePrice = ["InvtID", 'SlsUnit'];

var keyCust = ['CustID'];
var fieldsCheckRequireCust = ["CustID"];
var fieldsLangCheckRequireCust = ["CustID"];

var source = 0;
var sourcedata = 0;
var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == 'header') {
                HQ.combo.first(App.cboPriceID, HQ.isChange);
            }
            else if (HQ.focus == 'grdPrice') {
                HQ.grid.first(App.grdPrice);
            }
            else if (HQ.focus == 'grdCust') {
                HQ.grid.first(App.grdCust);
            }
            else if (HQ.focus == 'grdCompany') {
                HQ.grid.first(App.grdCompany);
            }
            break;
        case "next":
            if (HQ.focus == 'header') {
                HQ.combo.next(App.cboPriceID, HQ.isChange);
            }
            else if (HQ.focus == 'grdPrice') {
                HQ.grid.next(App.grdPrice);
            }
            else if (HQ.focus == 'grdCust') {
                HQ.grid.next(App.grdCust);
            }
            else if (HQ.focus == 'grdCompany') {
                HQ.grid.next(App.grdCompany);
            }
           
            break;
        case "prev":
            if (HQ.focus == 'header') {
                HQ.combo.prev(App.cboPriceID, HQ.isChange);
            }
            else if (HQ.focus == 'grdPrice') {
                HQ.grid.prev(App.grdPrice);
            }
            else if (HQ.focus == 'grdCust') {
                HQ.grid.prev(App.grdCust);
            }
            else if (HQ.focus == 'grdCompany') {
                HQ.grid.prev(App.grdCompany);
            }
            break;
        case "last":
            if (HQ.focus == 'header') {
                HQ.combo.last(App.cboPriceID, HQ.isChange);
            }
            else if (HQ.focus == 'grdPrice') {
                HQ.grid.last(App.grdPrice);
            }
            else if (HQ.focus == 'grdCust') {
                HQ.grid.last(App.grdCust);
            }
            else if (HQ.focus == 'grdCompany') {
                HQ.grid.last(App.grdCompany);
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                //checkRequire để kiếm tra các field yêu cầu có rỗng hay ko
                if (HQ.form.checkRequirePass(App.frmMain)
                    && HQ.store.checkRequirePass(App.stogrdCompany, keyCompany, fieldsCheckRequireCompany, fieldsLangCheckRequireCompany)
                    && HQ.store.checkRequirePass(App.stogrdPrice, keyPrice, fieldsCheckRequirePrice, fieldsLangCheckRequirePrice)
                    && HQ.store.checkRequirePass(App.stogrdCust, keyCust, fieldsCheckRequireCust, fieldsLangCheckRequireCust)
                    ) {
                    save();
                }
            }
            break;
        case "delete":
            if (HQ.isDelete && App.cboStatus.value == 'H') {
                if (HQ.isDelete) {
                    if (HQ.focus == 'header') {
                        HQ.message.show(11, '', 'deleteHeader');
                    } else if (HQ.focus == 'grdPrice') {
                        var rowindex = HQ.grid.indexSelect(App.grdPrice);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdPrice), ''], 'deleteRecordGridPrice', true)
                    }
                    else if (HQ.focus == 'grdCust') {
                        var rowindex = HQ.grid.indexSelect(App.grdCust);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdCust), ''], 'deleteRecordGridCust', true)
                    }
                    else if (HQ.focus == 'grdCompany') {
                        var rowindex = HQ.grid.indexSelect(App.grdCompany);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdCompany), ''], 'deleteRecordGridCompany', true)
                    }
                }
                break;

            }
            break;
        case "close":
            HQ.common.close(this);
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.focus == 'header') {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', '');
                    }
                    else {
                        HQ.isNew = true;
                        App.cboPriceID.focus(true);
                        App.cboPriceID.setValue(null);
                    }
                }
                else if (HQ.focus == 'grdPrice') {
                    HQ.grid.insert(App.grdPrice, keyPrice);
                }
                else if (HQ.focus == 'grdCust') {
                    HQ.grid.insert(App.grdCust, keyCust);
                }
                else if (HQ.focus == 'grdCompany') {
                    HQ.grid.insert(App.grdCompany, keyCompany);
                }
            }
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                refresh('yes');
            }
            break;

            break;
        case "print":
            App.frmMain.submit({
                waitMsg: HQ.common.getLang("LoadReporting"),
                method: 'POST',
                url: 'PO10100/Report',
                timeout: 180000,
                success: function (msg, data) {
                    if (this.result.reportID != null) {

                        window.open('Report?ReportName=' + this.result.reportName + '&_RPTID=' + this.result.reportID, '_blank')
                    }

                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
            break;
        default:
    }
};

var frmChange = function () {
    if (App.stoHeader.getCount() > 0) {
        App.frmMain.getForm().updateRecord();
    }
    HQ.isChange = HQ.store.isChange(App.stoHeader) == false ? (HQ.store.isChange(App.stogrdCompany)
                                                                == false ? (HQ.store.isChange(App.stogrdCust)
                                                                == false ? (HQ.store.isChange(App.stogrdPrice)) : true) : true) : true;
    HQ.common.changeData(HQ.isChange, 'OM20700');
    if (App.cboPriceID.valueModels == null || HQ.isNew == true)
        App.cboPriceID.setReadOnly(false);
    else App.cboPriceID.setReadOnly(HQ.isChange);

};
//Source
var stoMaster_Load = function (sto) {
    source+=1;
    if (source == 2) {
        App.stoHeader.reload();
    }
    HQ.common.showBusy(false);
};

var stocboSlsUnit_beforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
    App.cboInvtID.getStore().reload();
    App.cboInvtID.getStore().addListener("load", stoMaster_Load);
    App.cboSlsUnit.getStore().addListener("load", stoMaster_Load);
    App.cboSlsUnit.getStore().addListener("beforeload", stocboSlsUnit_beforeLoad);
    HQ.isFirstLoad = true;  
};
var cboPriceID_Change = function (sender, newValue, oldValue) {
    if ((!HQ.isNew || sender.valueModels != null) && !App.stoHeader.loading) {
        App.stoHeader.reload();
    }

};
var cboPriceID_Select = function (sender) {
    if (sender.valueModels != null && !App.stoHeader.loading) {
        App.stoHeader.reload();
    }
};

var grdPrice_BeforeEdit = function (item, e) {
   
    if (App.cboStatus.getValue() != _beginStatus || (!App.cboPriceID.value && !HQ.isInsert) || (App.cboPriceID.value && !HQ.isUpdate)) {
        return false;
    }   
    if (e.field == 'SlsUnit') {
        objInvtID = HQ.store.findRecord(App.cboInvtID.getStore(), ["InvtID"], [e.record.data.InvtID]);
        if (objInvtID) {
            _invtID = e.record.data.InvtID;
            _classID = e.record.data.ClassID;
            App.cboSlsUnit.getStore().load();
        }
    }
};

var grdPrice_ValidateEdit = function (item, e) {
    var _keys = ["InvtID","SlsUnit"];
    var objdet = e.record;
    if (_keys.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(App.grdPrice, e, _keys)) {
            if (e.field == "InvtID")
                HQ.message.show(1112, [objdet.data.SlsUnit + ',' + e.value], '', true);
            else HQ.message.show(1112, [e.value + ',' + objdet.data.InvtID], '', true);
            return false;
        }
    }
};

var grdPrice_Edit = function (item, e) {
    var _keys = ['InvtID', 'SlsUnit'];
    var obj = App.cboInvtID.getStore().findRecord("InvtID", e.record.data.InvtID);
    if (obj) {
        e.record.set("Descr", obj.data.Descr);
    }
    else e.record.set("Descr", '');   
    HQ.grid.checkInsertKey(App.grdPrice, e, _keys);
    frmChange();
};

var grdPrice_Reject = function (record) {
    if (record.data.tstamp == '') {    
        App.grdPrice.getStore().remove(record, App.grdPrice);
        App.grdPrice.getView().focusRow(App.grdPrice.getStore().getCount() - 1);
        App.grdPrice.getSelectionModel().select(App.grdPrice.getStore().getCount() - 1);
      
    } else {   
        record.reject();
    }
    frmChange();
}


var grdCust_BeforeEdit = function (item, e) {

    if (App.cboStatus.getValue() != _beginStatus || (!App.cboPriceID.value && !HQ.isInsert) || (App.cboPriceID.value && !HQ.isUpdate)) {
        return false;
    }
};

var grdCust_ValidateEdit = function (item, e) {
    var _keys = ["CustID", "BranchID"];
    var objdet = e.record;
    if (_keys.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(App.grdCust, e, _keys)) {
            if (e.field == "CustID")
                HQ.message.show(1112, [objdet.data.BranchID + ',' + e.value], '', true);
            else HQ.message.show(1112, [e.value + ',' + objdet.data.CustID], '', true);
            return false;
        }
    }
};

var grdCust_Edit = function (item, e) {
    var _keys = ["CustID", "BranchID"];   
    HQ.grid.checkInsertKey(App.grdCust, e, _keys);
    frmChange();
};

var grdCust_Reject = function (record) {
    if (record.data.tstamp == '') {
        App.grdCust.getStore().remove(record, App.grdCust);
        App.grdCust.getView().focusRow(App.grdCust.getStore().getCount() - 1);
        App.grdCust.getSelectionModel().select(App.grdCust.getStore().getCount() - 1);

    } else {
        record.reject();
    }
    frmChange();
}


var treeBranch_BeforeLoad = function (store, operation, options) {
    var node = operation.node;

    App.direct.OM20700NodeLoad(node.getId(), node.data.Type, node.data.Value, {
        success: function (result) {
            node.set('loading', false);
            node.set('loaded', true);
            var data = Ext.decode(result);
            if (!Ext.isEmpty(data)) {
                node.appendChild(data, undefined, true);
                node.expand();
            }

        },

        failure: function (errorMsg) {
            Ext.Msg.alert('Failure', errorMsg);
        }
    });

    return false;
};
var treeBranch_NotifyDrop = function (dd, e, data) {
    if (data.records[0].data.Type == 'Territory') {
        App.direct.OM20700NodeLoad("", data.records[0].data.Type, data.records[0].data.Value, {
            success: function (result) {
                //grdPriceCompany.set('loading', false);
                //grdPriceCompany.set('loaded', true);
                var data = Ext.decode(result);
                if (!Ext.isEmpty(data)) {
                    data.forEach(function (item) {
                        addDataToGrid(item)
                    });
                }

            },

            failure: function (errorMsg) {
                Ext.Msg.alert('Failure', errorMsg);
            }
        });
    } else if (data.records[0].data.Type == 'Company') {
        addDataToGrid(data.records[0].data)
    }
    return true;
};

function addDataToGrid(data) {
    if (App.cboStatus.getValue() != _beginStatus || (!App.cboPriceID.value && !HQ.isInsert) || (App.cboPriceID.value && !HQ.isUpdate)) {
        return false;
    }
    var r = App.stogrdCompany.getById(data.Value);
    if (Ext.isEmpty(r)) {
        var model = Ext.create('App.ModelPriceCompany');
        model.data.CpnyID = data.Value;
        model.data.CpnyName = data.Descr;
        App.stogrdCompany.insert(App.stogrdCompany.getCount(), model);
    }
}
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid
var stoChanged = function (sto) {
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stogrd_Load = function (sto) {
    if (HQ.isFirstLoad) {
        if (sto.storeId == "stogrdPrice") {
            if (HQ.isInsert && App.cboStatus.getValue() == _beginStatus) {
                setTimeout(function () {
                    App.stogrdPrice.insert(App.stogrdPrice.getTotalCount(), Ext.data.Record());                   
                }, 100);
            }
        }
        
        else if (sto.storeId == "stogrdCompany") {
            if (HQ.isInsert && App.cboStatus.getValue() == _beginStatus) {
                setTimeout(function () {
                    App.stogrdCompany.insert(App.stogrdCompany.getTotalCount(), Ext.data.Record());
                }, 100);
            }
        }
        else if (sto.storeId == "stogrdCust") {
            if (HQ.isInsert && App.cboStatus.getValue() == _beginStatus) {
                setTimeout(function () {
                    App.stogrdCust.insert(App.stogrdCust.getTotalCount(), Ext.data.Record());
                }, 100);
            }
        }
        
    }
    sourcedata++;
    if (sourcedata == 3) {
        frmChange();
        HQ.common.showBusy(false);
        HQ.isFirstLoad = false;
        App.stogrdPrice.loadPage(1);
        App.stogrdCompany.loadPage(1);
        App.stogrdCust.loadPage(1);
    }
 
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    //HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
var loadDataHeader = function (sto) {
    sourcedata = 0;
    HQ.isFirstLoad = true;
    App.cboPriceCat.setValue('');
    App.tabSalesPrice.setActiveTab(0);
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
    //HQ.common.setForceSelection(App.frmMain, false, "cboPriceID,cboHandle");
    if (sto.data.length == 0) {
        HQ.isNew = true;
        HQ.store.insertBlank(sto, "PriceID");
        var record = sto.getAt(0);
        //gan du lieu mac dinh ban dau
        record.data.Status = "H";
        record.data.FromDate=HQ.bussinessDate;
        record.data.ToDate = HQ.bussinessDate;
        record.data.PriceCat = "IT";
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    var isLock = record.data.Status == "H" ? false : true;
    HQ.common.lockItem(App.frmMain, isLock);

    App.frmMain.getForm().loadRecord(record);
    App.stogrdPrice.reload();
    App.stogrdCust.reload();
    App.stogrdCompany.reload();  
 
};
var chkPublic_Change = function () {
    if (App.chkPublic.checked)
        App.tabBranch.setDisabled(true);
    else
        App.tabBranch.setDisabled(false);
}
var cboPriceCat_Change = function (item, newValue, oldValue) {
    if (newValue == "IL") {
        App.cboClassID.setReadOnly(false);
        App.cboClassID.allowBlank = false;
        App.tabCust.setDisabled(true);
    } else if (newValue == "IC") {
        App.cboClassID.setReadOnly(true);
        App.cboClassID.setValue(null);
        App.cboClassID.allowBlank = true;
        App.tabCust.setDisabled(false);
    }
    else if (newValue == "IT") {
        App.cboClassID.setReadOnly(true);
        App.cboClassID.setValue(null);
        App.cboClassID.allowBlank = true;
        App.tabCust.setDisabled(true);
    }
    if (item.hasFocus) App.cboClassID.setValue('');
    App.cboClassID.validate();
};
var colCheck_Header_Change = function (value) {
    if (value) {
        App.stogrdPrice.suspendEvents();
        App.stogrdPrice.data.each(function (item) {
            item.set("Check", value.checked);           
        });
        App.stogrdPrice.resumeEvents();
        App.grdPrice.view.refresh();
    }
}
var cboCustID_Change = function (item, newValue, oldValue) {
   
    var r = HQ.store.findRecord(App.cboCustID.getStore(), ["CustID", "BranchID"], [item.valueModels[0].data.CustID,item.valueModels[0].data.BranchID])
    if (Ext.isEmpty(r)) {
        App.slmgrdCust.getSelection()[0].set('CustName', "");
        App.slmgrdCust.getSelection()[0].set('BranchID', "");
        App.slmgrdCust.getSelection()[0].set('CustID', "");
    }
    else {
        App.slmgrdCust.getSelection()[0].set('CustName', r.data.Name);
        App.slmgrdCust.getSelection()[0].set('BranchID', r.data.BranchID);
        App.slmgrdCust.getSelection()[0].set('CustID', r.data.CustID);
    }
};
var cboStatus_Change = function () {
    App.cboHandle.store.reload();
}
//DataProcess
function save() {
    var allRecordsCompany = App.stogrdCompany.snapshot || App.stogrdCompany.allData || App.stogrdCompany.data;
    var allRecordsPrice = App.stogrdPrice.snapshot || App.stogrdPrice.allData || App.stogrdPrice.data;

    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("SavingData"),
            method: 'POST',
            timeout: 30000,
            url: 'OM20700/Save',
            params: {
                lstHeader: Ext.encode(App.stoHeader.getRecordsValues()),
                lstCompany: Ext.encode(App.stogrdCompany.getChangedData({ skipIdForPhantomRecords: false })),
                lstPrice: Ext.encode(App.stogrdPrice.getChangedData({ skipIdForPhantomRecords: false })),
                lstCust: Ext.encode(App.stogrdCust.getChangedData({ skipIdForPhantomRecords: false })),
                lstAllCompany: HQ.store.getAllData(App.stogrdCompany),
                lstAllPrice: HQ.store.getAllData(App.stogrdPrice),
                lstAllCust: HQ.store.getAllData(App.stogrdCust)
            },
            success: function (msg, data) {
                HQ.message.process(msg, data, true);
                var priceID = '';
                if (this.result.data != undefined && this.result.data.priceID != null) {
                    priceID = this.result.data.priceID;
                }
                App.cboPriceID.getStore().load(function () {
                    App.cboPriceID.setValue(priceID);
                    App.stoHeader.reload();
                });
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};
function deleteHeader(item) {
    if (item == 'yes') {
        if (App.frmMain.isValid()) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang('DeletingData'),
                method: 'POST',
                url: 'OM20700/DeleteHeader',
                timeout: 180000,
                params: {
                    lstHeader: Ext.encode(App.stoHeader.getRecordsValues())
                },
                success: function (msg, data) {

                    HQ.message.process(msg, data, true);
                    App.cboPriceID.getStore().load(function () {
                        App.cboPriceID.setValue('');
                        App.stoHeader.reload();
                    });
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
};
var deleteRecordGridPrice = function (item) {
    if (item == 'yes') {       
        App.grdPrice.deleteSelected();
        frmChange();
    }
}
var deleteRecordGridCust = function (item) {
    if (item == 'yes') {
        App.grdCust.deleteSelected();
        frmChange();
    }
}
var deleteRecordGridCompany = function (item) {
    if (item == 'yes') {
        App.grdCompany.deleteSelected();
        frmChange();
    }
}


var renderBranchName = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboBranchID.findRecord("BranchID", rec.data.CpnyID);
    if (record) {
        return record.data.BranchName;
    }
    else {
        return value;
    }
};

///////////////////////////////////////// TREE VIEW ////////////////////////////////////
var beforenodedrop = function (node, data, overModel, dropPosition, dropFn) {
    if (Ext.isArray(data.records)) {
        var records = data.records;
        data.records = [];
        HQ.store.insertBlank(App.stogrdCompany, keyCompany);
        addNode(records[0]);
    }
};

var treePanelBranch_checkChange = function (node, checked, eOpts) {
    node.childNodes.forEach(function (childNode) {
        childNode.set("checked", checked);
    });
};
var btnCopy_click = function (btn, e, eOpts) {
    if (HQ.isNew && App.cboPriceIDCopy.value) {
        HQ.common.showBusy(true, HQ.common.getLang('Copyingdata'));
        App.stoHeaderCopy.load({
            callback: function () {
                var recordHeader = App.stoHeaderCopy.data.items[0].data;
                App.txtDescr.setValue(recordHeader.Descr);
                App.dteFromDate.setValue(recordHeader.FromDate);
                App.dteToDate.setValue(recordHeader.ToDate);
                App.cboPriceCat.setValue(recordHeader.PriceCat);
                App.cboClassID.setValue(recordHeader.CustClassID);
                App.chkProm.setValue(recordHeader.Prom);
                App.chkPublic.setValue(recordHeader.Public);               
                App.cboStatus.setValue('H');
                App.stogrdCompanyCopy.load({
                    callback: function () {
                        HQ.store.insertBlank(App.stogrdCompany, keyCompany);
                        setTimeout(function () {
                            App.stogrdCompany.suspendEvents();
                            App.stogrdCompanyCopy.data.each(function (item) {
                                var objDetail = App.stogrdCompany.data.items[App.stogrdCompany.getCount() - 1];
                                objDetail.set('CpnyID', item.data.CpnyID);
                                objDetail.set('CpnyName', item.data.CpnyName);
                                App.stogrdCompany.insert(App.stogrdCompany.getCount(), Ext.data.Record());
                            });
                            App.stogrdCompany.resumeEvents();
                            App.grdCompany.view.refresh();
                            App.stogrdCompany.loadPage(1);
                        }, 300);
                        App.stogrdPriceCopy.load({
                            callback: function () {
                                HQ.store.insertBlank(App.stogrdPrice, keyPrice);
                                setTimeout(function () { 
                                    App.stogrdPrice.suspendEvents();
                                    App.stogrdPriceCopy.data.each(function (item) {
                                        //var objDetail = App.stogrdPrice.data.items[App.stogrdPrice.getCount() - 1];
                                      
                                        var objDetail = App.stogrdPrice.data.items[App.stogrdPrice.getCount() - 1];
                                        objDetail.set('InvtID', item.data.InvtID);
                                        objDetail.set('Descr', item.data.Descr);
                                        objDetail.set('SlsUnit', item.data.SlsUnit);
                                        objDetail.set('QtyBreak', item.data.QtyBreak);
                                        objDetail.set('Price', item.data.Price);

                                        App.stogrdPrice.insert(App.stogrdPrice.getCount(), Ext.data.Record());
                                      
                                    });
                                    App.stogrdPrice.resumeEvents();
                                    App.grdPrice.view.refresh();
                                    App.stogrdPrice.loadPage(1);
                                }, 300);
                                App.stogrdCustCopy.load({
                                    callback: function () {
                                        HQ.store.insertBlank(App.stogrdCust, keyCust);
                                        setTimeout(function () {
                                            App.stogrdCust.suspendEvents();
                                            App.stogrdCustCopy.data.each(function (item) {                                              
                                                var objDetail = App.stogrdCust.data.items[App.stogrdCust.getCount() - 1];
                                                objDetail.set('CustID', item.data.CustID);
                                                objDetail.set('CustName', item.data.CustName);
                                                objDetail.set('BranchID', item.data.BranchID);
                                                App.stogrdCust.insert(App.stogrdCust.getCount(), Ext.data.Record());

                                            });
                                            App.stogrdCust.resumeEvents();
                                            App.grdCust.view.refresh();
                                            App.stogrdCust.loadPage(1);
                                        }, 300);
                                        HQ.common.showBusy(false);
                                    }
                                })
                            }
                        })
                    }                      
                })
            }
        })
    }
       
};

var btnExpand_click = function (btn, e, eOpts) {
    App.treePanelBranch.expandAll();
};

var btnCollapse_click = function (btn, e, eOpts) {
    App.treePanelBranch.collapseAll();
};

var btnAddAll_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        var allNodes = getDeepAllLeafNodes(App.treePanelBranch.getRootNode(), true);
        if (allNodes && allNodes.length > 0) {
            allNodes.forEach(function (node) {
                HQ.store.insertBlank(App.stogrdCompany, keyCompany);
                if (node.data.Type == "Company") {
                    var record = HQ.store.findInStore(App.grdCompany.store,
                        ['CpnyID'],
                        [node.data.RecID]);
                    if (!record) {
                        record = App.stogrdCompany.getAt(App.grdCompany.store.getCount() - 1);
                        record.set('CpnyID', node.data.RecID);
                    }
                }
            });
            HQ.store.insertBlank(App.stogrdCompany, keyCompany);
            var record = App.stogrdCompany.getAt(App.stogrdCompany.getCount() - 1);
            App.treePanelBranch.clearChecked();
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var addNode = function (node) {
    if (node.data.Type == "Company") {
        var record = HQ.store.findInStore(App.grdCompany.store,
            ['CpnyID'],
            [node.data.RecID]);
        if (!record) {
            record = App.stogrdCompany.getAt(App.grdCompany.store.getCount() - 1);
            record.set('CpnyID', node.data.RecID);
        }
        HQ.store.insertBlank(App.stogrdCompany, keyCompany);
        var record = App.stogrdCompany.getAt(App.stogrdCompany.getCount() - 1);
    }
    else if (node.childNodes) {
        node.childNodes.forEach(function (itm) {
            addNode(itm);
        });
    }
    frmChange();
};

var btnAdd_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        var allNodes = App.treePanelBranch.getCheckedNodes();
        if (allNodes && allNodes.length > 0) {
            allNodes.forEach(function (node) {
                HQ.store.insertBlank(App.stogrdCompany, keyCompany);
                if (node.attributes.Type == "Company") {
                    var record = HQ.store.findInStore(App.grdCompany.store,
                        ['CpnyID'],
                        [node.attributes.RecID]);
                    if (!record) {
                        record = App.stogrdCompany.getAt(App.grdCompany.store.getCount() - 1);
                        record.set('CpnyID', node.attributes.RecID);
                    }
                }
            });
            HQ.store.insertBlank(App.stogrdCompany, keyCompany);
            var record = App.stogrdCompany.getAt(App.stogrdCompany.getCount() - 1);
            App.treePanelBranch.clearChecked();
        }
        frmChange();
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var btnDel_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        var selRecs = App.grdCompany.selModel.selected.items;
        if (selRecs.length > 0) {
            var params = [];
            selRecs.forEach(function (record) {
                params.push(record.data.CpnyID);
            });
            HQ.message.show(2015020806,
                params.join(" & ") + "," + HQ.common.getLang("AppComp"),
                'deleteSelectedCompanies');
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var btnDelAll_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        HQ.message.show(11, '', 'deleteAllCompanies');
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var getDeepAllLeafNodes = function (node, onlyLeaf) {
    var allNodes = new Array();
    if (!Ext.value(node, false)) {
        return [];
    }
    if (node.isLeaf()) {
        return node;
    } else {
        node.eachChild(
         function (Mynode) {
             allNodes = allNodes.concat(Mynode.childNodes);
         }
        );
    }
    return allNodes;
};

var deleteSelectedCompanies = function (item) {
    if (item == "yes") {
        App.grdCompany.deleteSelected();
        frmChange();
    }
};

var deleteAllCompanies = function (item) {
    if (item == "yes") {
        App.grdCompany.store.removeAll();
        frmChange();
    }
};

/////////////////////////////// GIRD Company /////////////////////////////////

var grdCompany_BeforeEdit = function (editor, e) {
    if (App.cboStatus.getValue() != _beginStatus || (!App.cboPriceID.value && !HQ.isInsert) || (App.cboPriceID.value && !HQ.isUpdate)) {
        return false;
    }
    if (!HQ.grid.checkBeforeEdit(e, keyCompany)) return false;
};

var grdCompany_Edit = function (item, e) {
    //Kiem tra cac key da duoc nhap se insert them dong moi
    HQ.grid.checkInsertKey(App.grdCompany, e, keyCompany);
    frmChange();
};

var grdCompany_ValidateEdit = function (item, e) {
    //ko cho nhap key co ki tu dac biet, va kiem tra trung du lieu
    return HQ.grid.checkValidateEdit(App.grdCompany, e, keyCompany);
};

var grdCompany_Reject = function (record) {
    //reject dong thay doi du lieu ve ban dau
    HQ.grid.checkReject(record, App.grdCompany);
    frmChange();
};

////Other////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        App.cboPriceID.getStore().load(function () {
            App.stoHeader.reload();
        });
    }
};
var renderNumberColumnCpny = function (value, meta, record) {
    return App.stogrdCompany.data.indexOf(record) + 1;
}
