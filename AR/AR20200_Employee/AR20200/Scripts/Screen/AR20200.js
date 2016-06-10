// Declare
var _beginStatus = "H";
var _Source = 0;
var _maxSource = 7;
var _isLoadMaster = false;
var CpnyID = '';
var SlsperID = '';
var _Flag;
var _HandleCombo = '';
////////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        App.cboBranchID.setValue(HQ.cpnyID);
        _isLoadMaster = true;
        _Source = 0;
        App.stoSalesPerson.reload();
        HQ.common.showBusy(false);
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
    App.cboCountry.getStore().addListener('load', checkLoad);
    App.cboState.getStore().addListener('load', checkLoad);
    App.cboDistrict.getStore().addListener('load', checkLoad);
    App.cboProductGroup.getStore().addListener('load', checkLoad);
};

////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
// Load and show binding data to the form
// Command of the topbar on screen
var menuClick = function (command) {
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
                    App.cboSlsperid.setValue('');
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
                        menuClick('new');
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
       
        HQ.common.changeData(HQ.isChange, 'AR20200');
        if (App.cboSlsperid.valueModels == null || HQ.isNew == true) {
            App.cboSlsperid.setReadOnly(false);
            App.cboBranchID.setReadOnly(false);
        }
        else {
            App.cboSlsperid.setReadOnly(HQ.isChange);
            App.cboBranchID.setReadOnly(HQ.isChange);
        }

    }
    
};

var stoLoad = function (sto) {
    HQ.isNew = false;
    HQ.common.setForceSelection(App.frmMain, false, "cboBranchID,cboSlsperid")
    App.cboBranchID.forceSelection = true;
    App.cboSlsperid.forceSelection = true;

    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, ['BranchID', 'SlsperId']);
        record = sto.getAt(0);
        record.set('Active', true);
        record.set('Status', _beginStatus);
        HQ.isNew = true;
        App.cboSlsperid.forceSelection = false;
        if (_Flag == "1")
            App.cboSlsperid.forceSelection = true;
        //if (App.stoAR20200_pdCheckAutoSales.data.items[0].data.Result == "1")
        //    App.cboSlsperid.forceSelection = true;
        HQ.common.setRequire(App.frmMain);
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);
    // display image
    App.fupImages.reset();
    if (record.data.Images) {
        displayImage(App.imgImages, record.data.Images);
    }
    else {
        App.imgImages.setImageUrl("");
    }

    var selRec = HQ.store.findInStore(App.cboBranchID.store, ['BranchID'], [App.cboBranchID.getValue()]);

    App.grdSlsperCpnyAddr.store.reload();
    if (selRec && selRec.Channel == "MT") {
        App.pnlSlsperCpnyAddr.show();
        //App.grdSlsperCpnyAddr.store.reload();
    }
    else {
        App.pnlSlsperCpnyAddr.hide();
        App.grdSlsperCpnyAddr.store.removeAll();
    }

    if (record.data.Status == 'H') {
        HQ.common.lockItem(App.frmMain, false);
        App.fupImages.setDisabled(false);
    }
    else {
        HQ.common.lockItem(App.frmMain, true);
        App.fupImages.setDisabled(true);
    }

    if (!HQ.isInsert && HQ.isNew) {
        App.cboSlsperid.forceSelection = true;
        HQ.common.lockItem(App.frmMain, true);
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
    }

    if (HQ.isDelete) {
        App.btnDel.enable();
        App.btnDelAll.enable();
    }
    else {
        App.btnDel.disable();
        App.btnDelAll.disable();
    }
    
   
};
//Grid
var stoSlsperCpnyAddr_load = function (sto) {
    frmChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

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



//Truoc khi load store se hien Busy
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoAR20200_pdCheckAutoSales_load = function (sto) {
    if (sto.data.items[0].data.Result == "1") {
        _Flag = "1";
        App.cboSlsperid.allowBlank = true;
        App.cboSlsperid.isValid(false);
        App.cboSlsperid.forceSelection = true;
    }
    else {
        _Flag = "0";
        App.cboSlsperid.allowBlank = false;
        App.cboSlsperid.isValid(true);
        App.cboSlsperid.forceSelection = false;
    }
};


var cboBranchID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    SlsperID = '';
    if (sender.valueModels != null && !App.stoSalesPerson.loading) {
        App.cboSlsperid.setValue('');
        App.cboSlsperid.store.reload();
        App.cboDeliveryMan.setValue('');
        App.cboDeliveryMan.store.reload();
        App.cboSupID.setValue('');
        App.cboSupID.store.reload();
        App.cboVendID.setValue('');
        App.cboVendID.store.reload();
        App.stoAR20200_pdCheckAutoSales.reload();
    }
};

var cboBranchID_Select = function (sender, value) {
    HQ.isFirstLoad = true;
    SlsperID = '';
    if (sender.valueModels != null && !App.stoSalesPerson.loading) {
        App.cboSlsperid.setValue('');
        App.cboSlsperid.store.reload();
        App.cboDeliveryMan.setValue('');
        App.cboDeliveryMan.store.reload();
        App.cboSupID.setValue('');
        App.cboSupID.store.reload();
        App.cboVendID.setValue('');
        App.cboVendID.store.reload();
        App.stoAR20200_pdCheckAutoSales.reload();
    }
};

var cboSlsperid_change = function (sender, e) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoSalesPerson.loading) {
        App.stoSalesPerson.reload();
    }
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
    if (HQ.isInsert) {
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
    if (HQ.isInsert) {
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

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
// Submit the changed data (created, updated) into server side
var save = function () {
    if (App.frmMain.isValid()) {
        var selRec = HQ.store.findInStore(App.cboBranchID.store, ['BranchID'], [App.cboBranchID.getValue()]);
        App.frmMain.updateRecord();
        if (App.txtEMailAddr.value != "") {
            if (!HQ.util.checkEmail(App.txtEMailAddr.value)) {
                return;
            }
        }
        if (App.cboSlsperid.allowBlank == false) {
            if (HQ.util.checkSpecialChar(App.cboSlsperid.getValue()) == false) {
                HQ.message.show(20140811, App.cboSlsperid.fieldLabel);
                App.cboSlsperid.focus();
                App.cboSlsperid.selectText();
                return false;
            }
        }
        App.frmMain.submit({
            url: 'AR20200/SaveData',
            waitMsg: HQ.common.getLang('Submiting') + "...",
            timeout: 1800000,
            params: {
                lstSalesPerson: Ext.encode(App.stoSalesPerson.getRecordsValues()),
                lstSlsperCpnyAddr: HQ.store.getData(App.grdSlsperCpnyAddr.store),
                channel: selRec ? selRec.Channel : "",
                isNew: HQ.isNew,
                HandleCombo: _HandleCombo
            },
            success: function (msg, data) {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                HQ.message.show(201405071);
                SlsperID = data.result.slsperID;

                App.cboSlsperid.getStore().load({
                    callback: function () {
                        if (Ext.isEmpty(App.cboSlsperid.getValue())) {
                            App.cboSlsperid.setValue(SlsperID);
                            App.stoSalesPerson.reload();
                        }
                        else {
                            App.cboSlsperid.setValue(SlsperID);
                            App.stoSalesPerson.reload();
                        }
                    }
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
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoSalesPerson.reload();
    }
};
///////////////////////////////////

var filterComboSate = function (sender, e) {
    if (sender.hasFocus) {
        App.cboState.setValue('');
        App.cboDistrict.setValue('');
    }

    var code = App.cboCountry.getValue();
    App.cboState.store.clearFilter();
    App.cboState.store.filter("Country", code);
};


var filterComboProduct = function (sender, e) {
    if (sender.hasFocus) {
        App.cboProductGroup.setValue('');
    }

    var code = App.cboPosition.getValue();
    App.cboProductGroup.store.clearFilter();
    App.cboProductGroup.store.filter("Position", code);
};

var cboProductGroup_Expand = function(){
    if (Ext.isEmpty(App.cboPosition.getValue()))
        App.cboProductGroup.collapse();
    else {
        var code = App.cboPosition.getValue();
        App.cboProductGroup.store.clearFilter();
        App.cboProductGroup.store.filter("Position", code);
    }
};  

var filterComboCityDistrict = function (sender, e) {
    if (sender.hasFocus) {
        App.cboDistrict.setValue('');
    }
    var code = App.cboCountry.getValue() + App.cboState.getValue();
    App.cboDistrict.store.clearFilter();
    App.cboDistrict.store.filter("CountryState", code);
};

var cboHandle_Change = function (sender, value) {
    if (sender.valueModels != null && sender.valueModels.length > 0) {
        _HandleCombo = sender.valueModels[0].data.Code;
    }
};