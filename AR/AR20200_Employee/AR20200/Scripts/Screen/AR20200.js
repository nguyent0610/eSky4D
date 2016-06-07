// Declare
var _beginStatus = "H";
var _Source = 0;
var _maxSource = 4;
var _isLoadMaster = false;
var CpnyID = '';

////////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.stoSalesPerson.reload();
        HQ.common.showBusy(false);
        App.cboBranchID.setValue(HQ.cpnyID);
    }
};

////////////////////////////////////////////////////////////////////////
//// First Load ////////////////////////////////////////////////////////
var firstLoad = function () {
    HQ.util.checkAccessRight(); // Kiem tra quyen Insert Update Delete de disable button tren top bar
    HQ.isFirstLoad = true;
    HQ.focus = "header";
    App.frmMain.isValid(); // Require cac field yeu cau tren from

    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));

    App.cboBranchID.getStore().addListener('load', checkLoad);
    App.cboPosition.getStore().addListener('load', checkLoad);
    App.cboStatus.getStore().addListener('load', checkLoad);
    App.cboCountryId.getStore().addListener('load', checkLoad);
    
    
    
};

////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
// Load and show binding data to the form
// Command of the topbar on screen
var menuClick = function (command) {
    if (App.stoSalesPerson.isLoading()) return;
    switch (command) {
        case "first":           
            if (HQ.focus == 'header') {
                HQ.combo.first(App.cboSlsperid, HQ.isChange);

            }
            else if (HQ.focus == 'grid') {
                HQ.grid.first(App.grdSlsperCpnyAddr);
            }
            break;
        case "prev":
            if (HQ.focus == 'header') {
                HQ.combo.prev(App.cboSlsperid, HQ.isChange);
            }
            else if (HQ.focus == 'grid') {
                HQ.grid.prev(App.grdSlsperCpnyAddr);
            }
            break;
        case "next":
            if (HQ.focus == 'header') {
                HQ.combo.next(App.cboSlsperid, HQ.isChange);
            }
            else if (HQ.focus == 'grid') {
                HQ.grid.next(App.grdSlsperCpnyAddr);
            }
            break;
        case "last":
            if (HQ.focus == 'header') {
                HQ.combo.last(App.cboSlsperid, HQ.isChange);
            }
            else if (HQ.focus == 'grid') {
                HQ.grid.last(App.grdSlsperCpnyAddr);
            }
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
                if (HQ.isChange) {
                    HQ.message.show(150, '', 'refresh');
                } else {
                    App.cboBranchID.setValue('');
                    App.cboSlsperid.reload();
                }
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (HQ.focus == 'header') {
                    if (App.cboSlsperid.getValue() && App.cboStatus.getValue() == _beginStatus) {
                        HQ.message.show(11, '', 'deleteData');
                    }
                    else {
                        HQ.message.show(20140306, '', '');
                    }
                }
                else if (HQ.focus == 'grid') {
                    var selected = App.grdSlsperCpnyAddr.getSelectionModel().selected.items;
                    if (selected.length > 0) {
                        if (selected[0].index != undefined) {
                            var params = selected[0].index + 1 + ',' + App.pnlSlsperCpnyAddr.title;
                            HQ.message.show(2015020807, params, 'deleteSlsperCpnyAddr');
                        }
                        else {
                            HQ.message.show(11, '', 'deleteSlsperCpnyAddr');
                        }
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
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
            //HQ.common.close(this);
            break;
    }
};

var frmChange = function () {
    
    if (App.stoSalesPerson.getCount() > 0) {
        App.frmMain.updateRecord();
        if (!HQ.store.isChange(App.stoSalesPerson)) {
            HQ.isChange = HQ.store.isChange(App.grdSlsperCpnyAddr.store);
        }
        else {
            HQ.isChange = true;
        }
        
        HQ.common.changeData(HQ.isChange, 'AR20200');//co thay doi du lieu gan * tren tab title header
        //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
        App.cboBranchID.setReadOnly(HQ.isChange);
        App.cboSlsperid.setReadOnly(HQ.isChange);
    }
    
};

var stoLoad = function (sto) {
    HQ.isNew = false;
    HQ.common.lockItem(App.frmMain, false);
    HQ.common.setForceSelection(App.frmMain, false, "cboBranchID,cboSlsperid")

    HQ.isNew = false;
    if (sto.getCount() == 0) {
        var newSlsper = Ext.create("App.mdlAR_Salesperson", {
            SlsperId: App.cboSlsperid.getValue(),
            BranchID: App.cboBranchID.getValue(),
            Active: true,
            Status: _beginStatus
        });
        sto.insert(0, newSlsper);
        sto.commitChanges();
        HQ.isNew = true;
    }
    var frmRecord = sto.getAt(0);
    App.frmMain.loadRecord(frmRecord);

    // display image
    App.fupImages.reset();
    if (frmRecord.data.Images) {
       displayImage(App.imgImages, frmRecord.data.Images);
    }
    else {
        App.imgImages.setImageUrl("");
    }

    var selRec = HQ.store.findInStore(App.cboBranchID.store, ['BranchID'], [App.cboBranchID.getValue()]);

    if (selRec && selRec.Channel == "MT") {
        App.pnlSlsperCpnyAddr.show();
        App.grdSlsperCpnyAddr.store.reload();
    }
    else {
        App.pnlSlsperCpnyAddr.hide();
        App.grdSlsperCpnyAddr.store.removeAll();
    }
    var isLock = frmRecord.data.Status != _beginStatus ? true : false;
    HQ.common.lockItem(App.frmMain, isLock);
    if (isLock) {
        App.btnClearImage.disable();
        App.fupImages.disable()
    } else {
        App.btnClearImage.enable();
        App.fupImages.enable()
    }


    App.fupImages.setReadOnly(isLock);
    

    if (!HQ.isInsert && HQ.isNew) {
        App.cboBranchID.forceSelection = true;
        HQ.common.lockItem(App.frmMain, true);
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }

    if (_isLoadMaster) {
        HQ.common.showBusy(false);
        frmChange();
    }
};

//Truoc khi load store se hien Busy
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoAR20200_pdCheckAutoSales_load = function (sto) {
    if (sto.data.items[0].data.Result == "1") {
        App.cboSlsperid.allowBlank = true;
        App.cboSlsperid.forceSelection = true;
    }
    else {
        App.cboSlsperid.allowBlank = false;
        App.cboSlsperid.forceSelection = false;
    }
};

//



var cboBranchID_change = function (sender, value) {
    HQ.isFirstLoad = true;
    App.cboSlsperid.clearValue();
    if (sender.valueModels != null && ! App.cboSlsperid.store.loading) {
        CpnyID = value;
        App.cboSlsperid.getStore().load(function (records) {
            if (records.length > 0) {
                App.cboSlsperid.setValue(records[0].data.SlsperId);
            }
        });
        
        App.cboDeliveryMan.store.reload();
        App.cboSupID.store.reload();
        App.cboVendID.store.reload();
        App.stoAR20200_pdCheckAutoSales.reload();
    }
    

    
    
};

var cboBranchID_Select = function (sender, value) {
    if (sender.valueModels != null && !App.stoSalesPerson.loading) {
        CpnyID = value;
        App.cboSlsperid.store.reload();
        App.stoSalesPerson.reload();
        App.stoAR20200_pdCheckAutoSales.reload();
        App.cboVendID.store.reload();
    }
};

//khi nhan combo xo ra, neu da thay doi thi ko xo ra
var cboBranchID_Expand = function (sender, value) {
    if (HQ.isChange) {
        App.cboBranchID.collapse();
    }
};

//khi nhan X xoa tren combo, neu du lieu thay doi thi ko cho xoa, du lieu chua thay doi thi add new
var cboBranchID_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {
        App.cboBranchID.setValue('');
    }
};

//cboSlsperid_change
var cboSlsperid_change = function (sender, e) {
    App.stoSalesPerson.reload();
};

//cboStatus_change
var cboStatus_change = function (sender, e) {
    App.cboHandle.store.reload();
};


// Event when cboCountry is changed or selected item
var cboCountryId_change = function (sender, e) {
    App.cboState.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord != undefined)
            if (curRecord.data.State) {
                App.cboState.setValue(curRecord.data.State);
            }
        var dt = HQ.store.findInStore(App.cboState.getStore(), ["State"], [App.cboState.getValue()]);
        if (!dt) {
            curRecord.data.State = '';
            App.cboState.setValue("");
        }

    });
};
// Event when cboState is changed or selected item
var cboState_change = function (sender, e) {
    App.cboDistrict.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord != undefined)
            if (curRecord.data.District) {
                App.cboDistrict.setValue(curRecord.data.District);
            }
        var dt = HQ.store.findInStore(App.cboDistrict.getStore(), ["District"], [App.cboDistrict.getValue()]);
        if (!dt) {
            curRecord.data.District = '';
            App.cboDistrict.setValue("");
        }
    });
};

/////////////////////////////////
//Image
var btnClearImage_click = function (sender, e) {
    App.fupImages.reset();
    App.imgImages.setImageUrl("");
    App.hdnImages.setValue("");
};

var fupImages_change = function (fup, newValue, oldValue, eOpts) {
    if (fup.value) {
        var ext = fup.value.split(".").pop().toLowerCase();
        if (ext == "jpg" || ext == "png" || ext == "gif") {
            App.hdnImages.setValue(fup.value);
            readImage(fup, App.imgImages);
        }
        else {
            HQ.message.show(148, '', '');
        }
    }
};

///////////////////////////////
//Tree

//var treePanelCpnyAddr_checkChange = function (node) {
//    node.childNodes.forEach(function (childNode) {
//        childNode.set("checked", checked);
//    });
//};

var treePanelCpnyAddr_checkChange = function (node, checked, eOpts) {
    if (node.hasChildNodes()) {
        node.eachChild(function (childNode) {
            childNode.set('checked', checked);
            treePanelCpnyAddr_checkChange(childNode, checked);
        });
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


var btnExpand_click = function (sender, e) {
    App.treePanelCpnyAddr.expandAll();
};

var btnCollapse_click = function (sender, e) {
    App.treePanelCpnyAddr.collapseAll();
};

var btnAddAll_click = function (sender, e) {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            var branchID = App.cboBranchID.getValue();
            var slperID = App.cboSlsperid.getValue();
            //var status = App.cboStatus.getValue();

            if (branchID && slperID) {
                var allNodes = getDeepAllLeafNodes(App.treePanelCpnyAddr.getRootNode(), true);
                if (allNodes && allNodes.length > 0) {
                    allNodes.forEach(function (node) {
                        if (node.raw.Type == "Addr") {
                            var idx = App.grdSlsperCpnyAddr.store.getCount();
                            var record = HQ.store.findInStore(App.grdSlsperCpnyAddr.store,
                                ['CpnyAddrID'],
                                [node.raw.RecID]);
                            if (!record) {
                                App.grdSlsperCpnyAddr.store.insert(idx, Ext.create("App.mdlSlsperCpnyAddr", {
                                    CpnyAddrID: node.raw.RecID,
                                    Addr1: node.raw.Addr1,
                                    Name: node.raw.AddrName
                                }));
                            }
                        }
                    });
                    App.treePanelCpnyAddr.clearChecked();
                }
            }
        }
        else {
            Process.showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var btnAdd_click = function (sender, e) {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            var branchID = App.cboBranchID.getValue();
            var slperID = App.cboSlsperid.getValue();
            //var status = App.cboStatus.getValue();

            if (branchID && slperID) {
                var allNodes = App.treePanelCpnyAddr.getChecked();
                if (allNodes && allNodes.length > 0) {
                    allNodes.forEach(function (node) {
                        if (node.raw.Type == "Addr") {
                            var idx = App.grdSlsperCpnyAddr.store.getCount();
                            var record = HQ.store.findInStore(App.grdSlsperCpnyAddr.store,
                                ['CpnyAddrID'],
                                [node.raw.RecID]);
                            if (!record) {
                                App.grdSlsperCpnyAddr.store.insert(idx, Ext.create("App.mdlSlsperCpnyAddr", {
                                    CpnyAddrID: node.raw.RecID,
                                    Addr1: node.raw.Addr1,
                                    Name: node.raw.AddrName
                                }));
                            }
                        }
                    });
                    App.treePanelCpnyAddr.clearChecked();
                }
            }
        }
        else {
            Process.showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var deleteSlsperCpnyAddr = function (item) {
    if (item == "yes") {
        App.grdSlsperCpnyAddr.deleteSelected();
    }
};

var deleteAllCpnyAddrs = function (item) {
    if (item == "yes") {
        App.grdSlsperCpnyAddr.store.removeAll();
    }
};

var btnDel_click = function (sender, e) {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            //var status = App.cboStatus.value;

            //if (status == _beginStatus) {
            var selRecs = App.grdSlsperCpnyAddr.selModel.selected.items;
            if (selRecs.length > 0) {
                var params = [];
                selRecs.forEach(function (record) {
                    params.push(record.data.CpnyAddrID);
                });
                HQ.message.show(2015020806,
                    params.join(" & ") + "," + HQ.common.getLang("SlsperCpnyAddr"),
                    'deleteSlsperCpnyAddr');
            }
            //}
        }
        else {
            Process.showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var btnDelAll_click = function (sender, e) {
    if (HQ.isUpdate) {
        if (App.frmMain.isValid()) {
            //var status = App.cboStatus.value;
            //if (status == _beginStatus) {
            HQ.message.show(11, '', 'deleteAllCpnyAddrs');
            //}
        }
        else {
            Process.showFieldInvalid(App.frmMain);
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var renderAddr = function (value, metaData, record, rowIndex, colIndex, store) {
    var rec = App.cboCpnyAddrID.store.findRecord("AddrID", record.data.CpnyAddrID);
    var returnValue = value;
    if (rec) {
        if (metaData.column.dataIndex == "Addr1" && !record.data.Addr1) {
            returnValue = rec.data.Addr1;
        }
        else if (metaData.column.dataIndex == "Addr2" && !record.data.Addr2) {
            returnValue = rec.data.Addr2;
        }
    }

    return returnValue;
};

///////////////////////////////////////////
//Grid

var grdSlsperCpnyAddr_beforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdSlsperCpnyAddr_edit = function (editor, e) {
    var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

    if (keys.indexOf(e.field) != -1) {
        if (e.value != ''
            && Process.isAllValidKey(e.store.getChangedData().Created, keys)
            && Process.isAllValidKey(e.store.getChangedData().Updated, keys)) {
            var branchID = App.cboBranchID.getValue();
            var slperID = App.cboSlsperid.getValue();
            var status = App.cboStatus.getValue();
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

            if (branchID && slperID && status == _beginStatus) {
                var newData = {
                    SlsperID: slperID,
                    BranchID: branchID
                };

                var newRec = Ext.create(e.store.model.modelName, newData);
                HQ.store.insertRecord(e.store, keys, newRec, false);
            }
        }
    }
};

var grdSlsperCpnyAddr_validateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSlsperCpnyAddr, e, keys);
};

var grdSlsperCpnyAddr_reject = function (record) {
    HQ.grid.checkReject(record, App.grdSlsperCpnyAddr);
    frmChange();
};

var stoSlsperCpnyAddr_load = function (sto) {
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
// Submit the changed data (created, updated) into server side
var save = function () {
    if (App.frmMain.isValid()) {
        var selRec = HQ.store.findInStore(App.cboBranchID.store, ['BranchID'], [App.cboBranchID.getValue()]);
        App.frmMain.updateRecord();

        App.frmMain.submit({
            url: 'AR20200/SaveData',
            waitMsg: HQ.common.getLang('Submiting') + "...",
            timeout: 1800000,
            params: {
                lstSalesPerson: Ext.encode(App.stoSalesPerson.getRecordsValues()),
                lstSlsperCpnyAddr: HQ.store.getData(App.grdSlsperCpnyAddr.store),
                channel: selRec ? selRec.Channel : "",
                isNew: HQ.isNew
            },
            success: function (msg, data) {
                if (data.result.msgCode) {
                    HQ.message.show(data.result.msgCode);
                }
                App.cboSlsperid.store.load(function () {
                    App.cboSlsperid.setValue(data.result.slsperID);
                    App.stoSalesPerson.reload();

                });
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};


// Submit the deleted data into server side
var deleteData = function (item) {
    if (item == "yes") {
        App.frmMain.submit({
            url: 'AR20200/Delete',
            clientValidation: false,
            waitMsg: HQ.common.getLang('Deleting') + "...",
            timeout: 1800000,
            params: {
                branchID: App.cboBranchID.getValue(),
                slsperID: App.cboSlsperid.getValue(),
                isNew: HQ.isNew
            },
            success: function (msg, data) {
                if (data.result.msgCode) {
                    HQ.message.show(data.result.msgCode);
                }
                App.cboSlsperid.store.load();
                App.cboSlsperid.clearValue();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var displayImage = function (imgControl, fileName) {
    Ext.Ajax.request({
        url: 'AR20200/ImageToBin',
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        params: JSON.stringify({
            fileName: fileName
        }),
        success: function (result) {
            var jsonData = Ext.decode(result.responseText);
            if (jsonData.imgSrc) {
                imgControl.setImageUrl(jsonData.imgSrc);
            }
            else {
                imgControl.setImageUrl("");
            }
        },
        failure: function (errorMsg, data) {
            HQ.message.process(errorMsg, data, true);
        }
    });
};

var readImage = function (fup, imgControl) {
    var files = fup.fileInputEl.dom.files;
    if (files && files[0]) {
        var FR = new FileReader();
        FR.onload = function (e) {
            imgControl.setImageUrl(e.target.result);
        };
        FR.readAsDataURL(files[0]);
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        App.cboSlsperid.store.load(function () {
            App.stoSalesPerson.reload();
        });
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        //App.stoSalesPerson.reload();
    }
};
///////////////////////////////////