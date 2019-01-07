var keysTab = ['CpnyID'];
var fieldsCheckRequireTab = ["CpnyID"];
var fieldsLangCheckRequireTab = ["CpnyID"];

var InvtID = '';
var _Source = 0;
var _maxSource = 22;
var _isLoadMaster = false;
var prefixvalue = "";
var lastfixvalue = "";
var shownextlotserial = "";
var _maxLevel = 0;
var _nodeID = '';
var _nodeLevel = '';
var _parentRecordID = '';
var _recordID = '';
var _root = '';
var parentRecordIDAll = '';
var parentRecordID = '';
var _copy = false;
var _treeExpandAll = false;
var isnewclick = false;
var _firstExpand = true;
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        //_Source = 0;
       // ReloadTree();
        App.cboInvtID.store.reload();
        App.stoIN_Inventory.reload();
       // HQ.common.showBusy(false);
    }
};

var firstLoad = function () {

    App.chkPublic.setVisible(!HQ.isHideChkPublic);
    App.chkKitType.setVisible(HQ.KitType);
    if (!HQ.KitType)
    {
        App.CtnKitType.setMargin("22 0 0 0");
    }
    if (HQ.isHideChkPublic) {
        App.tabDetail.child('#pnlCpnyID').tab.setDisabled(true);
        if (App.tabDetail.activeTab.id == "pnlCpnyID") {
            App.tabDetail.setActiveTab(App.pnlDfltInfo);
        }
    } else {
        App.tabDetail.child('#pnlCpnyID').tab.setDisabled(false);
    }

    setView();
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    ReloadTree();   
    App.cboInvtID.getStore().addListener('load', checkLoad);
    App.cboApproveStatus.getStore().addListener('load', checkLoad);
    App.cboStatus.getStore().addListener('load', checkLoad);
    App.cboClassID.getStore().addListener('load', checkLoad);
    App.cboPriceClassID.getStore().addListener('load', checkLoad);
    App.cboInvtType.getStore().addListener('load', checkLoad);
    App.cboSource.getStore().addListener('load', checkLoad);
    App.cboValMthd.getStore().addListener('load', checkLoad);
    App.cboLotSerTrack.getStore().addListener('load', checkLoad);
    App.cboBuyer.getStore().addListener('load', checkLoad);
    App.cboStkUnit.getStore().addListener('load', checkLoad);
    //App.cboDfltPOUnit.getStore().addListener('load', checkLoad);
    //App.cboDfltSOUnit.getStore().addListener('load', checkLoad);
    App.cboMaterialType.getStore().addListener('load', checkLoad);
    App.cboDfltSlsTaxCat.getStore().addListener('load', checkLoad);
    App.cboStyle.getStore().addListener('load', checkLoad);
    App.cboStkWtUnit.getStore().addListener('load', checkLoad);
    App.cboVendor1.getStore().addListener('load', checkLoad);
    App.cboVendor2.getStore().addListener('load', checkLoad);
    App.cboDfltLotSerAssign.getStore().addListener('load', checkLoad);
    App.cboDfltLotSerMthd.getStore().addListener('load', checkLoad);
    App.cboDfltLotSerFxdTyp.getStore().addListener('load', checkLoad);
    App.cboCpnyID.getStore().addListener('load', checkLoad);
    App.stoProductCpny.addListener('load', checkLoad);

    App.btnExport.setVisible(HQ.isShowImport);
    App.btnImport.setVisible(HQ.isShowImport);
    App.txtBarCode.setVisible(HQ.isShowBarCode);
    
    if (HQ.isBachKhang) {
        App.cboStyle.setVisible(true);
        App.cboStyle.allowBlank = false;
        App.tabDetail.child('#pnlLotSerial').tab.hide();
        App.tabDetail.child('#pnlCpnyID').tab.hide();
        App.tabDetail.child('#pnlAttribute').tab.hide();
    }
    else {
        App.cboStyle.setVisible(false);
        App.cboStyle.allowBlank = true;
        App.tabDetail.child('#pnlLotSerial').tab.show();
        App.tabDetail.child('#pnlCpnyID').tab.show();
        App.tabDetail.child('#pnlAttribute').tab.show();
    }
    App.cboStyle.isValid();
    
};

var setView = function () {
    App.frmMain.suspendLayouts();
    App.tabDetail.child('#pnlLotSerial').tab.setDisabled(true);
    App.fupPPCStorePicReq.button.btnEl.setWidth(100);
    App.fupPPCStorePicReq.button.setWidth(100);
    App.fupPPCStoreMediaReq.button.btnEl.setWidth(100);
    App.fupPPCStoreMediaReq.button.setWidth(100);
    App.frmMain.resumeLayouts(true);
    App.frmMain.doLayout();
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == 'header') {
                HQ.combo.first(App.cboInvtID, HQ.isChange);
            }
            else if (HQ.focus == 'pnlCpnyID') {
                HQ.grid.first(App.grdCpny);
            }
            break;
        case "prev":
            if (HQ.focus == 'header') {
                HQ.combo.prev(App.cboInvtID, HQ.isChange);
            }
            else if (HQ.focus == 'pnlCpnyID') {
                HQ.grid.prev(App.grdCpny);
            }
            break;
        case "next":
            if (HQ.focus == 'header') {
                HQ.combo.next(App.cboInvtID, HQ.isChange);
            }
            else if (HQ.focus == 'pnlCpnyID') {
                HQ.grid.next(App.grdCpny);
            }
            break;
        case "last":
            if (HQ.focus == 'header') {
                HQ.combo.last(App.cboInvtID, HQ.isChange);
            }
            else if (HQ.focus == 'pnlCpnyID') {
                HQ.grid.last(App.grdCpny);
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
                if (HQ.focus == 'header') {
                    if (HQ.isChange) {
                        HQ.message.show(150);
                    } else {
                        isnewclick = true;
                        App.txtInvtIDCopy.setValue('');
                        _copy = false;
                        InvtID = '';
                        App.cboInvtID.setValue('');
                        App.cboInvtID.focus();
                    }
                }
                else if (HQ.focus == 'pnlCpnyID') {
                    HQ.grid.insert(App.grdCpny);
                }
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                //if (App.cboApproveStatus.getValue() == 'H') {
                    if (HQ.focus == 'header') {
                        if (App.cboInvtID.getValue()) {
                            HQ.message.show(11, '', 'deleteData');
                        } else {
                            menuClick('new');
                        }
                    }
                    else if (HQ.focus == 'pnlCpnyID') {
                        if (App.slmCpny.selected.items[0] != undefined) {
                            if (App.slmCpny.selected.items[0].data.CpnyID != "") {
                                HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdCpny)], 'deleteData', true);
                            }
                        }
                    }
               // }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain)
                    && HQ.store.checkRequirePass(App.stoCpny, keysTab, fieldsCheckRequireTab, fieldsLangCheckRequireTab)) {
                    if (HQ.util.checkSpecialChar(App.cboInvtID.getValue()) == false) {
                        HQ.message.show(20140811, App.cboInvtID.fieldLabel);
                        App.cboInvtID.focus();
                        App.cboInvtID.selectText();
                        return false;
                    } else if (!Ext.isEmpty(App.txtBarCode.getValue()) && HQ.util.checkSpecialChar(App.txtBarCode.getValue()) == false) {
                        HQ.message.show(20140811, App.txtBarCode.fieldLabel);
                        App.txtBarCode.focus();
                        App.txtBarCode.selectText();
                        return false;
                    }
                    if (_recordID != '' && _parentRecordID != '' && _nodeID != '' && _nodeLevel != '') {
                        if (_root == 'true') {
                            HQ.message.show(2015040901, '', '');
                            return false;
                        }
                        save();
                    }
                    else {
                        HQ.message.show(2015090701, '', '');
                        return false;
                    }
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
    if (App.stoIN_Inventory.getCount() > 0) {
        App.frmMain.getForm().updateRecord();
    }
    HQ.isChange = HQ.store.isChange(App.stoIN_Inventory) == false ? (HQ.store.isChange(App.stoCpny)) : true;
    HQ.common.changeData(HQ.isChange, 'IN20500');
    if (App.cboInvtID.valueModels == null || HQ.isNew == true)
        App.cboInvtID.setReadOnly(false);
    else
        App.cboInvtID.setReadOnly(HQ.isChange);
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoLoad = function (sto) {
    //HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
    HQ.common.setForceSelection(App.frmMain, false, "cboInvtID,cboHandle");
    App.cboInvtID.forceSelection = true;
    if(_copy == false)
        HQ.isNew = false;

    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, '');
        record = sto.getAt(0);
        record.set('ApproveStatus', 'H');
        record.set('Status', 'AC');
        record.set('Public', HQ.isHideChkPublic);
        record.set('ValMthd', 'A');
        HQ.isNew = true;
        App.cboInvtID.forceSelection = false;
        HQ.common.setRequire(App.frmMain);
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    if (_copy == true) {
        record.data.ApproveStatus = 'H';
    }
    App.frmMain.getForm().loadRecord(record);
    App.tabDetail.setActiveTab(App.pnlDfltInfo);
    checkLotSerial(record.data.LotSerFxdTyp);
    //image
    App.fupPPCStorePicReq.reset();
    if (!Ext.isEmpty(record.data.Picture)) {
        displayImage(App.imgPPCStorePicReq, record.data.Picture);
    }
    else {
        App.imgPPCStorePicReq.setImageUrl("");
    }
    //media
    App.fupPPCStoreMediaReq.reset();
    if (!Ext.isEmpty(record.data.Media)) {
        displayImage(App.imgPPCStoreMediaReq, record.data.Media);
    }
    else {
        App.imgPPCStoreMediaReq.setImageUrl("");
    }
    App.stoCpny.reload();
    var isLock = false;
    HQ.common.lockItem(App.frmMain, isLock);
    App.fupPPCStorePicReq.setDisabled(isLock);
    App.fupPPCStoreMediaReq.setDisabled(isLock);
    App.txtDfltLotSerFxdVal.setReadOnly(isLock);

    if (!HQ.isInsert && HQ.isNew) {
        App.cboInvtID.forceSelection = true;
        HQ.common.lockItem(App.frmMain, true);
        App.txtDfltLotSerFxdVal.setReadOnly(true);
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
        App.txtDfltLotSerFxdVal.setReadOnly(true);
    }
    setReadOnly(record.data.EditableInfo);
  //  isnewclick = false;
    if (!HQ.isNew && !Ext.isEmpty(App.cboInvtID.getValue()))
        searchNode();
    if (HQ.DfltValMthd)
        App.cboValMthd.show();
    else
        App.cboValMthd.hide();
    if (HQ.GiftPoint)
        App.txtGiftPoint.show();
    else
        App.txtGiftPoint.hide();
};


var checkLotSerial = function (type) {
    //App.txtDfltLotSerFxdVal.setReadOnly(false);
    if (type == 'D') {
        App.txtDfltLotSerFxdVal.setReadOnly(true);
        App.txtDfltLotSerFxdLen.setReadOnly(true);
    }
    else {
        App.txtDfltLotSerFxdVal.setReadOnly(false);
        App.txtDfltLotSerFxdLen.setReadOnly(false);
    }
};

var setReadOnly = function (isReadOnly) {
    App.cboClassID.setReadOnly(isReadOnly);
    App.cboStkUnit.setReadOnly(isReadOnly);
    App.cboDfltPOUnit.setReadOnly(isReadOnly);
    App.cboDfltSOUnit.setReadOnly(isReadOnly);

    App.cboLotSerTrack.setReadOnly(isReadOnly);
    App.cboValMthd.setReadOnly(isReadOnly);
    HQ.common.lockItem(App.pnlLotSerial, isReadOnly);

    //App.cboDfltLotSerAssign.setReadOnly(isReadOnly);
    //App.cboClassID.setReadOnly(isReadOnly);
    //App.cboClassID.setReadOnly(isReadOnly);
    //App.cboClassID.setReadOnly(isReadOnly);
    //App.cboClassID.setReadOnly(isReadOnly);
    //App.cboClassID.setReadOnly(isReadOnly);
    //App.cboClassID.setReadOnly(isReadOnly);
    //App.cboClassID.setReadOnly(isReadOnly);
    //App.cboClassID.setReadOnly(isReadOnly);
    //App.cboClassID.setReadOnly(isReadOnly);
    //App.cboClassID.setReadOnly(isReadOnly);
    //App.cboClassID.setReadOnly(isReadOnly);
    //App.cboClassID.setReadOnly(isReadOnly);
    //App.cboClassID.setReadOnly(isReadOnly);
    if (isReadOnly) {
        App.menuClickbtnDelete.disable();
    } else {
        App.menuClickbtnDelete.enable();
    }
}

/////////////////////////////// GIRD AR_CustDisplayMethod /////////////////////////////////
var stoCpny_Load = function (sto) {
    //HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
    //if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keysTab);
        }
        HQ.isFirstLoad = false; //sto load cuoi se su dung
    //}
    frmChange();
    if (_isLoadMaster) {
        if (_copy == true) {
            App.cboInvtID.forceSelection = false;
            App.cboInvtID.setValue('');
        }
        HQ.common.showBusy(false);
    }
};

var grdCpny_BeforeEdit = function (editor, e) {
  //  if (App.cboApproveStatus.getValue() != 'H') return false;
    if (!HQ.grid.checkBeforeEdit(e, keysTab)) return false;
};

var grdCpny_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdCpny, e, keysTab);
    frmChange();
};

var grdCpny_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdCpny, e, keysTab, false);
};

var grdCpny_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdCpny);
    frmChange();
};

var focusControl = function (invalidField) {
    if (App[invalidField] && !App[invalidField].hasFocus) {
        var tab = App[invalidField].findParentByType('tabpanel');
        if (tab == undefined) {
            App[invalidField].focus();
        }
        else {
            HQ.util.focusControlInTab(tab, invalidField);
        }
    }
};

//Process
var save = function () {
    if (!App.chkPublic.getValue()) {
        if (App.stoCpny.getCount() == 1) {
            HQ.message.show(1888, '', '');
            App.tabDetail.setActiveTab(App.pnlCpnyID);
            return;
        }
    }
    if (App.cboLotSerTrack.getValue() != 'N') {
        if (!App.txtDfltWarrantyDays.getValue() > 0)
        {
            invalidField = App.txtDfltWarrantyDays.id;
            HQ.message.show(2015110901, App.txtDfltWarrantyDays.fieldLabel, 'HQ.util.focusControl');
            return;
        }
        else if (!App.cboDfltLotSerAssign.getValue()) {
            invalidField = App.cboDfltLotSerAssign.id;
            HQ.message.show(1000, App.cboDfltLotSerAssign.fieldLabel, 'HQ.util.focusControl');
            return;
        } else if (!App.cboDfltLotSerMthd.getValue()) {
            invalidField = App.cboDfltLotSerMthd.id;
            HQ.message.show(1000, App.cboDfltLotSerMthd.fieldLabel, 'HQ.util.focusControl');
            return;
        } else if (App.txtDfltLotSerShelfLife.getValue() < 1) {
            invalidField = App.txtDfltLotSerShelfLife.id;
            HQ.message.show(2015110901, App.txtDfltLotSerShelfLife.fieldLabel, 'HQ.util.focusControl');
            return;
        } else if (App.txtDfltLotSerFxdLen.getValue() > 0 && !App.txtDfltLotSerFxdVal.getValue()) {
            invalidField = App.txtDfltLotSerFxdVal.id;
            HQ.message.show(1000, App.txtDfltLotSerFxdVal.fieldLabel, 'HQ.util.focusControl');
            return;
        } else if (!App.txtDfltLotSerNumLen.getValue() && App.chkLotSerRcptAuto.checked == true) {
            invalidField = App.txtDfltLotSerNumLen.id;
            HQ.message.show(1000, App.txtDfltLotSerNumLen.fieldLabel, 'HQ.util.focusControl');
            return;
        } else if (App.txtDfltLotSerNumLen.getValue() > 0 && !App.txtDfltLotSerNumVal.getValue() ) {
            invalidField = App.txtDfltLotSerNumVal.id;
            HQ.message.show(1000, App.txtDfltLotSerNumVal.fieldLabel, 'HQ.util.focusControl');
            return;
        } else if (!App.lblShowNextLotSerial.getValue() && App.chkLotSerRcptAuto.checked == true) {
            invalidField = App.lblShowNextLotSerial.id;
            HQ.message.show(1000, App.lblShowNextLotSerial.fieldLabel, 'HQ.util.focusControl');
            return;
        } else if (App.txtDfltLotSerFxdVal.getValue().length != App.txtDfltLotSerFxdLen.getValue()) {
            //HQ.message.show(2016061401, App.txtDfltLotSerFxdLen.getValue());
            //App.tabDetail.setActiveTab(App.pnlLotSerial);
            //App.txtDfltLotSerFxdVal.focus();
            invalidField = App.txtDfltLotSerFxdVal.id;
            HQ.message.show(2016061401, App.txtDfltLotSerFxdLen.getValue(), 'HQ.util.focusControl');
            return;
        } else if (App.txtDfltLotSerNumVal.getValue().length != App.txtDfltLotSerNumLen.getValue()) {
            //HQ.message.show(2016061401, App.txtDfltLotSerNumLen.getValue());
            //App.tabDetail.setActiveTab(App.pnlLotSerial);
            //App.txtDfltLotSerNumVal.focus();
            invalidField = App.txtDfltLotSerNumVal.id;
            HQ.message.show(2016061401, App.txtDfltLotSerNumLen.getValue(), 'HQ.util.focusControl');
            return;
        }
    }
    if (App.frmMain.isValid()) {
        App.frmMain.updateRecord();
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            timeout: 1800000,
            url: 'IN20500/Save',
            params: {
                NodeID: _nodeID,
                NodeLevel: _nodeLevel,
                ParentRecordID: _parentRecordID,
                Copy: _copy,
                lstIN_Inventory: Ext.encode(App.stoIN_Inventory.getRecordsValues()),
                lstCpny: HQ.store.getAllData(App.stoCpny)
                
            },
            success: function (msg, data) {
                App.txtInvtIDCopy.setValue('');
                _copy = false;
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                HQ.message.show(201405071);
                HQ.common.showBusy(true, HQ.common.getLang("WaitMsg"));
                InvtID = data.result.InvtID;
                App.cboInvtID.getStore().reload({
                    callback: function () {
                        
                        ReloadTree('save', data.result.InvtID);
                    }
                });
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
                    url: 'IN20500/DeleteAll',
                    timeout: 7200,
                    success: function (msg, data) {
                        InvtID = '';
                        isnewclick = true;
                        ReloadTree('delete');
                        menuClick("new");
                    },
                    failure: function (msg, data) {
                        HQ.message.process(msg, data, true);
                    }
                });
            }
        }
        else if (HQ.focus == 'pnlCpnyID') {
            App.grdCpny.deleteSelected();
            frmChange();
        }
    }
};

function refresh(item) {
    if (item == 'yes') {
        if (HQ.isNew) {
            InvtID = '';
            App.cboInvtID.setValue('');
        }
        App.txtInvtIDCopy.setValue('');
        _copy = false;
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoIN_Inventory.reload();
    }
};

//////////////////////////////////////////////////////////////////////////////////

var renderCpnyID = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboCpnyIDIN20500_pcClassCpnyID.findRecord("CpnyID", rec.data.CpnyID);
    if (record) {
        return record.data.CpnyName;
    }
    else {
        return value;
    }
};

////////////////////////// Event Change & Select Control//////////////////////

var chkPublic_Change = function (item, value, oldValue) {
    HQ.isFirstLoad = true;
    if (value) {
        App.tabDetail.child('#pnlCpnyID').tab.setDisabled(true);
        if (App.tabDetail.activeTab.id == "pnlCpnyID") {
            App.tabDetail.setActiveTab(App.pnlDfltInfo);
        }
    } else {
        App.tabDetail.child('#pnlCpnyID').tab.setDisabled(false);
    }
};

var chkLotSerRcptAuto_Change = function (sender, value, oldValue) {
    if (value == true)
        setAllowBlankLot(false, '0');
    else
        setAllowBlankLot(true, '0');
};

var setAllowBlankLot = function (value,special) {
    App.txtDfltLotSerNumLen.allowBlank = value;
    App.txtDfltLotSerNumLen.validate();
    App.txtDfltLotSerNumVal.allowBlank = value;
    App.txtDfltLotSerNumVal.validate();
    if (special == '1') {
        App.cboDfltLotSerAssign.allowBlank = true;
        App.cboDfltLotSerAssign.validate();
        App.cboDfltLotSerMthd.allowBlank = true;
        App.cboDfltLotSerMthd.validate();
    }
    App.frmMain.validate();
};

var cboLotSerTrack_Change = function (sender, value, oldValue) {
    if (value) {
        if ((value == "L" || value == "S" || value == 'Q')) {
            if (Ext.isEmpty(App.cboDfltLotSerAssign.getValue())) App.cboDfltLotSerAssign.setValue('R');
            App.tabDetail.child('#pnlLotSerial').tab.setDisabled(false);
            prefixvalue = App.txtDfltLotSerFxdVal.getValue();
            lastfixvalue = App.txtDfltLotSerNumVal.getValue();
            shownextlotserial = prefixvalue + lastfixvalue;
            App.lblShowNextLotSerial.setValue(shownextlotserial);
            if (App.chkLotSerRcptAuto.checked == true) {
                setAllowBlankLot(false, '1');
            }
            else {
                setAllowBlankLot(true, '1');
            }
           
            
        } else {
            App.tabDetail.child('#pnlLotSerial').tab.setDisabled(true);
            setAllowBlankLot(true, '1');
            
        }
        App.txtQRCnvFact.setMinValue(value == 'Q' ? 1 : 0);
        App.txtQRCnvFact.validate();
        App.txtQRCnvFact.setVisible(value == 'Q');

        App.cboStkUnit.forceSelection = value != 'Q';
        App.cboStkUnit.setReadOnly(value == 'Q')

        App.cboDfltPOUnit.forceSelection = value != 'Q';
        App.cboDfltPOUnit.setReadOnly(value == 'Q')
        App.cboDfltSOUnit.forceSelection = value != 'Q';
        App.cboDfltSOUnit.setReadOnly(value == 'Q')
        if (value == 'Q') {
            App.cboStkUnit.setValue('KG');
            App.cboDfltPOUnit.setValue('KG');
            App.cboDfltSOUnit.setValue('KG');
        } else if (oldValue == 'Q') {
            App.cboStkUnit.setValue('');
            App.cboDfltPOUnit.setValue('');
            App.cboDfltSOUnit.setValue('');
        }
    }
};

//var cboDfltLotSerTrack_Select = function (sender, value) {
    //if (value) {
    //    if ((value.raw.Code == "L" || value.raw.Code == "S")) {
    //        App.tabDetail.child('#pnlLotSerial').tab.setDisabled(false);
    //        prefixvalue = App.txtDfltLotSerFxdVal.getValue();
    //        lastfixvalue = App.txtDfltLotSerNumVal.getValue();
    //        shownextlotserial = prefixvalue + lastfixvalue;
    //        App.lblShowNextLotSerial.setValue(shownextlotserial);
    //    } else {
    //        App.tabDetail.child('#pnlLotSerial').tab.setDisabled(true);
    //    }
    //}
//};

var cboDfltLotSerFxdTyp_Change = function (sender, value) {
    if (value) {
        checkLotSerial(value);
        if (sender.hasFocus) {
            App.txtDfltLotSerFxdLen.setValue(0);
            App.txtDfltLotSerFxdVal.setValue('');
            if (value == "D") {
                App.txtDfltLotSerFxdLen.setValue('8');
                App.txtDfltLotSerFxdVal.setValue(HQ.IN20500Date);
            }
        }
    }
};

var txtDfltLotSerNumLen_Change = function (sender, value) {
    App.txtDfltLotSerNumVal.setValue('');
    //if(value)
    //    sender.setValue(value.replaceAll('-', ''));
};

var event_KeyDown = function (sender, e) {
    if ((e.ctrlKey == true && e.keyCode == 86) ||
            (((e.keyCode < 48 || e.keyCode > 57) && (e.keyCode < 96 || e.keyCode > 105) && (e.keyCode < 37 || e.keyCode > 40))
            && (e.keyCode != 8 && e.keyCode != 9 && e.keyCode != 35 && e.keyCode != 36 && e.keyCode != 46 && e.keyCode != 45 && e.keyCode != 110 && e.keyCode != 190 && e.ctrlKey == false)
            )
           ) {
        e.stopEvent();
    }
};

var txtDfltLotSerFxdVal_KeyDown = function (sender, e) {
    if (App.cboDfltLotSerFxdTyp.getValue() == 'C') {
        //if (e.ctrlKey == true && e.keyCode == 86)
        //    e.stopEvent();
        if (
            (e.ctrlKey == true && e.keyCode == 86)||
            (((e.keyCode < 48 || e.keyCode > 57) && (e.keyCode < 96 || e.keyCode > 105) && (e.keyCode < 37 || e.keyCode > 40))
            && (e.keyCode != 8 && e.keyCode != 9 && e.keyCode != 35 && e.keyCode != 36 && e.keyCode != 46 && e.keyCode!= 45 && e.ctrlKey == false)
            )
           ) {
            e.stopEvent();
        }
            
    }
};

var txtDfltLotSerFxdVal_Blur = function (sender, e) {
    //if (sender.focus) {
    //    if (sender.lastValue.length > App.txtDfltLotSerFxdLen.getValue()) {
    //        HQ.message.show(22, e.value);
    //        App.txtDfltLotSerFxdVal.setValue(sender.originalValue);
    //        App.txtDfltLotSerFxdVal.focus();
    //    }
    //    else if ((sender.lastValue.length < App.txtDfltLotSerFxdLen.getValue()) && (sender.lastValue.length != App.txtDfltLotSerFxdLen.getValue())) {
    //        HQ.message.show(2016061401, App.txtDfltLotSerFxdLen.getValue());
    //        App.txtDfltLotSerFxdVal.focus();
    //    }
    //}
};

var txtDfltLotSerNumVal_Blur = function (sender, e) {
    //if (sender.focus) {
    //    if (sender.lastValue.length > App.txtDfltLotSerNumLen.getValue()) {
    //        HQ.message.show(22, e.value);
    //        App.txtDfltLotSerNumVal.setValue(sender.originalValue);
    //        App.txtDfltLotSerNumVal.focus();
    //    }
    //    else if ((sender.lastValue.length < App.txtDfltLotSerNumLen.getValue()) && (sender.lastValue.length != App.txtDfltLotSerNumLen.getValue())) {
    //        HQ.message.show(2016061401, App.txtDfltLotSerNumLen.getValue());
    //        App.txtDfltLotSerNumVal.focus();
    //    }
    //}
};

var PrefixValue_Change = function (sender, e) {
    if (App.txtDfltLotSerFxdVal.value.length > App.txtDfltLotSerFxdLen.value) {
        //HQ.message.show(22, e.value);
        //App.txtDfltLotSerFxdVal.setValue(sender.originalValue);
    } else {
        prefixvalue = App.txtDfltLotSerFxdVal.getValue();
        shownextlotserial = prefixvalue + lastfixvalue;
        App.lblShowNextLotSerial.setValue(shownextlotserial);
    }
};

var LastFixValue_Change = function (sender, e) {
    if (App.txtDfltLotSerNumVal.value.length > App.txtDfltLotSerNumLen.value) {
        //HQ.message.show(22, e.value);
        //App.txtDfltLotSerNumVal.setValue(sender.originalValue);
    } else {
        lastfixvalue = App.txtDfltLotSerNumVal.getValue();
        shownextlotserial = prefixvalue + lastfixvalue;
        App.lblShowNextLotSerial.setValue(shownextlotserial);
    }
};

var btnCopy_Click = function () {
    if (!Ext.isEmpty(App.txtInvtIDCopy.getValue())) {
        _tmpOldFileName = App.txtInvtIDCopy.getValue() + ".mp4";
        var barcode = App.txtInvtIDCopy.value;
        App.cboInvtID.setValue(barcode);
        HQ.isNew = true;
        _copy = true;
        App.cboInvtID.forceSelection = false;
    }
};

var cboInvtID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoIN_Inventory.loading && _copy == false) {
        //if(HQ.isChange == true) {
        //    HQ.message.show(150, '', '');
        //    sender.setValue(sender.originalValue);
        //    return;
        //}
        if(!isnewclick)
        InvtID = value;
        App.cboDfltPOUnit.store.reload();
        App.cboDfltSOUnit.store.reload();
        App.stoIN_Inventory.reload();
        //if (!Ext.isEmpty(value))
        //    searchNode();
    }
};

var cboInvtID_Select = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoIN_Inventory.loading && _copy == false) {
        //if (HQ.isChange == true) {
        //    HQ.message.show(150, '', '');
        //    sender.setValue(sender.originalValue);
        //    return;
        //}
        if (!isnewclick)
        InvtID = value;
        App.cboDfltPOUnit.store.reload();
        App.cboDfltSOUnit.store.reload();
        App.stoIN_Inventory.reload();
        //if (!Ext.isEmpty(value))
        //    searchNode();
    }
};

var cboInvtID_TriggerClick = function (sender, value) {
    if (HQ.isChange == true) {
        HQ.message.show(150, '', '');
        return;
    }
    App.cboInvtID.clearValue();
    InvtID = '';
    setView();
};

var cboInvtID_Expand = function (sender, value) {
    if (HQ.isChange) {
        App.cboInvtID.collapse();
    }
};

var cboInvtID_Focus = function () {
    if(HQ.isNew == true)
        App.cboInvtID.forceSelection = false;
    else
        App.cboInvtID.forceSelection = true;
    if (!HQ.isInsert && HQ.isNew)
        App.cboCustId.forceSelection = true;
};

var cboClassID_Change = function (sender, e) {
    if (sender.valueModels != null && sender.hasFocus) {
        var objTmp = sender.valueModels[0];
        if (objTmp) {
            App.chkPublic.setValue(objTmp.data.Public);
            App.chkStkItem.setValue(objTmp.data.StkItem);
            App.cboInvtType.setValue(objTmp.data.InvtType);
            App.cboSource.setValue(objTmp.data.Source);
            App.cboValMthd.setValue(objTmp.data.ValMthd);
            App.cboLotSerTrack.setValue(objTmp.data.LotSerTrack);
            App.cboBuyer.setValue(objTmp.data.Buyer);
            App.cboStkUnit.setValue(objTmp.data.StkUnit);
            App.cboDfltPOUnit.forceSelection = false;
            App.cboDfltSOUnit.forceSelection = false;
            App.cboDfltPOUnit.setValue(objTmp.data.DfltPOUnit);
            App.cboDfltSOUnit.setValue(objTmp.data.DfltSOUnit);
            App.cboMaterialType.setValue(objTmp.data.MaterialType);
            App.cboDfltSlsTaxCat.setValue(objTmp.data.TaxCat);
            App.cboDfltLotSerAssign.setValue(objTmp.data.SerAssign);
            App.cboDfltLotSerMthd.setValue(objTmp.data.LotSerIssMthd);
            App.txtDfltLotSerShelfLife.setValue(objTmp.data.ShelfLife);
            App.txtDfltWarrantyDays.setValue(objTmp.data.WarrantyDays);
            App.cboDfltLotSerFxdTyp.setValue(objTmp.data.LotSerFxdTyp);
            App.txtDfltLotSerFxdLen.setValue(objTmp.data.LotSerFxdLen);
            App.txtDfltLotSerFxdVal.setValue(objTmp.data.LotSerFxdVal);
            App.txtDfltLotSerNumLen.setValue(objTmp.data.LotSerNumLen);
            App.txtDfltLotSerNumVal.setValue(objTmp.data.LotSerNumVal);

            App.cboDfltPOUnit.store.reload();
            App.cboDfltSOUnit.store.reload();
            if (objTmp.data.Public == false) {
                App.stoProductCpny.clearFilter();
                App.stoProductCpny.filter('ClassID', objTmp.data.ClassID);
                App.stoProductCpny.each(function (record) {
                    if (!Ext.isEmpty(record.data.CpnyID)) {
                        var objCpny = HQ.store.findRecord(App.stoCpny, ['CpnyID'], [record.data.CpnyID]);
                        if (objCpny == null) {
                            var objDetail = App.stoCpny.data.items[App.stoCpny.getCount() - 1];
                            objDetail.set('CpnyID', record.data.CpnyID);
                            HQ.store.insertBlank(App.stoCpny, keysTab);
                        }
                    }
                });
            }
        }
    }
};

var cboClassID_TriggerClick = function (sender, e) {
    sender.clearValue();
    App.chkPublic.setValue('');
    App.chkStkItem.setValue('');
    App.cboInvtType.setValue('');
    App.cboSource.setValue('');
    App.cboValMthd.setValue('');
    App.cboLotSerTrack.setValue('');
    App.cboBuyer.setValue('');
    App.cboStkUnit.setValue('');
    App.cboDfltPOUnit.setValue('');
    App.cboDfltSOUnit.setValue('');
    App.cboMaterialType.setValue('');
    App.cboDfltSlsTaxCat.setValue('');
    App.cboDfltLotSerAssign.setValue('');
    App.cboDfltLotSerMthd.setValue('');
    App.txtDfltLotSerShelfLife.setValue('');
    App.txtDfltWarrantyDays.setValue('');
    App.cboDfltLotSerFxdTyp.setValue('');
    App.txtDfltLotSerFxdLen.setValue('');
    App.txtDfltLotSerFxdVal.setValue('');
    App.txtDfltLotSerNumLen.setValue('');
    App.txtDfltLotSerNumVal.setValue('');

    App.cboDfltPOUnit.store.reload();
    App.cboDfltSOUnit.store.reload();
};



var cboStkUnit_Change = function (sender, e) {
    if (sender.valueModels != null && sender.hasFocus) {
        App.cboDfltPOUnit.setValue('');
        App.cboDfltSOUnit.setValue('');
        App.cboDfltPOUnit.store.reload();
        App.cboDfltSOUnit.store.reload();
    }
};

var cboStkUnit_TriggerClick = function (sender,e) {
    sender.clearValue();
    App.cboDfltPOUnit.setValue('');
    App.cboDfltSOUnit.setValue('')
};
///////////////////////// Tree ///////////////////////////
var btnExpand_click = function (btn, e, eOpts) {
    //App.treeInvt.expandAll();
    Ext.suspendLayouts();
    _treeExpandAll = true;
    expandAll(App.treeInvt);
    Ext.resumeLayouts(true);
};

var btnCollapse_click = function (btn, e, eOpts) {
    //App.treeInvt.collapseAll();
    collapseAll(App.treeInvt);
};

var nodeSelected_Change = function (store, operation, options) {
    Ext.suspendLayouts();
    if (operation.internalId != 'root') {
        _root = 'false';
        var InvtID1 = '';
        _nodeID = operation.raw.NodeID;
        _nodeLevel = operation.raw.NodeLevel;
        _parentRecordID = operation.raw.ParentRecordID;
        _recordID = operation.raw.RecordID;
        InvtID1 = operation.raw.InvtID;

        ////_leaf = operation.data.leaf;
        ////parentRecordIDAll = operation.internalId.split("-");
        //if (operation.raw.InvtID!='') {
        //    _nodeID = operation.raw.NodeID;
        //    _nodeLevel = operation.raw.NodeID;
        //    _parentRecordID = operation.raw.NodeID;
        //    _recordID = operation.raw.NodeID;
        //} else {
        //    //parentRecordIDAll = operation.data.parentId.split("-");
        //    _nodeID = parentRecordIDAll[0];
        //    _nodeLevel = parentRecordIDAll[1];
        //    _parentRecordID = parentRecordIDAll[2];
        //    _recordID = parentRecordIDAll[3];
        //    var InvtIDall = operation.data.id.split("-");
        //    InvtID1 = operation.raw.InvtID;// InvtIDall[0];
        //    //InvtID = InvtIDall[0];
        //}
    } else {
        _root = 'true';
        _nodeID = '';
        _nodeLevel = '1';
        _parentRecordID = '0';
        _recordID = '0';
    }

    if (InvtID1) {
        if (HQ.isChange && (InvtID1 != InvtID)) {
            HQ.message.show(150);
            var objRecord = App.treeInvt.getRootNode().findChild('id', InvtID + '-|', true);
            if (objRecord)
                App.treeInvt.getSelectionModel().select(objRecord);
        }
        else {
            InvtID = InvtID1;
            App.cboInvtID.forceSelection = false;
            var objPage = findRecordCombo(InvtID);
            if (objPage) {
                var positionInvtID = calcPage(objPage.index);
                App.cboInvtID.loadPage(positionInvtID);
            }
            App.cboInvtID.setValue(InvtID);
        }
    }
    Ext.resumeLayouts(true);
};

var findRecordCombo = function (value) {
    var data = null;
    var store = App.cboInvtID.store;
    store.suspendEvents();
    var allRecords = store.snapshot || store.allData || store.data;
    allRecords.each(function (record) {
        if (record.data.InvtID == value) {
            data = record;
            return false;
        }
    });
    store.resumeEvents();
    return data;
};

var ReloadTree = function (type, valueInvtID) {
    try {
        _treeExpandAll = false;
        App.direct.ReloadTreeIN20500({
            success: function (data) {                
                if (type == 'save') {

                    App.cboInvtID.forceSelection = false;
                    var objPage = findRecordCombo(valueInvtID);
                    if (objPage) {
                        var positionInvtID = calcPage(objPage.index);
                        App.cboInvtID.loadPage(positionInvtID);
                    }
                    if (!isnewclick) {
                        App.cboInvtID.setValue(valueInvtID);
                        //App.stoIN_Inventory.reload();
                    }
                    else isnewclick = true;
                        //App.cboInvtID.setValue(InvtID);
                       
                    App.stoIN_Inventory.reload();
                }
                else if (type =='delete')
                {
                    InvtID = '';
                    App.cboInvtID.getStore().reload();
                }
                if (_firstExpand || type == 'delete') {
                    App.frmMain.mask();
                    App.treeInvt.getRootNode().expand();
                    App.frmMain.unmask();
                    _firstExpand = false;
                }
                
                HQ.common.showBusy(false);
            },
            failure: function () {
                HQ.common.showBusy(false);
                alert("fail");
            },
            //eventMask: { msg: 'loadingTree', showMask: true }
        });
    } catch (ex) {
        alert(ex.message);
    }
};

var searchNode = function () {
    App.frmMain.suspendLayouts();
    var nodeInvt = App.treeInvt.getRootNode().findChild('id', App.cboInvtID.getValue() + '-|', true);
    if (_treeExpandAll == false)
        collapseAll(App.treeInvt);
    if (nodeInvt) {
        App.treeInvt.getSelectionModel().deselectAll();
        App.treeInvt.getRootNode().expand();
        expandParentNode(nodeInvt);
        App.treeInvt.getSelectionModel().select(nodeInvt, true);
    }
    App.frmMain.resumeLayouts(true);
};

var expandParentNode = function (node) {
    if (node.parentNode) {
        expandParentNode(node.parentNode);
        node.parentNode.expand();

        //node.parentNode.expand();
        //expandParentNode(node.parentNode);
    }
};

var tabDetail_Change = function (tabPanel, newCard, oldCard, eOpts) {
    HQ.focus = tabPanel.activeTab.id;
};

//Image
var displayImage = function (imgControl, fileName) {
    Ext.Ajax.request({
        url: 'IN20500/ImageToBin',
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

var fupPPCStorePicReq_Change = function (fup, newValue, oldValue, eOpts) {
    if (fup.value) {
        var ext = fup.value.split(".").pop().toLowerCase();
        if (ext == "jpg" || ext == "png" || ext == "gif") {
            HQ.common.showBusy(true, HQ.common.getLang('Uploading...'), App.frmMain);
            readImage(fup, App.imgPPCStorePicReq, App.hdnPPCStorePicReq);
        }
        else {
            HQ.message.show(148, '', '');
        }
    }
};

//var readImage = function (fup, imgControl) {
//    var files = fup.fileInputEl.dom.files;
//    if (files && files[0]) {
//        var FR = new FileReader();
//        FR.onload = function (e) {
//            imgControl.setImageUrl(e.target.result);
//        };
//        FR.readAsDataURL(files[0]);
//    }
//};

var deleteImage = function (item) {
    if (item == "yes") {
        App.fupPPCStorePicReq.reset();
        App.imgPPCStorePicReq.setImageUrl('');
        App.hdnPPCStorePicReq.setValue('');
    }
};

var deleteMedia = function (item) {
    if (item == "yes") {
        App.fupPPCStoreMediaReq.reset();
        App.imgPPCStoreMediaReq.setImageUrl('');
        App.hdnPPCStoreMediaReq.setValue('');
    }
};

var btnClearImage_click = function (btn, eOpts) {
    if (App.hdnPPCStorePicReq.getValue())
        HQ.message.show(2016090601, '', 'deleteImage');
};

//Video
// Event when uplPPCStorePicReq is change a file
var btnDeleteMedia_Click = function (sender, e) {
    if (App.hdnPPCStoreMediaReq.getValue())
        HQ.message.show(2016090601, '', 'deleteMedia');
};

var fupPPCStoreMediaReq_Change = function (fup, newValue, oldValue, eOpts) {
    if (fup.value) {
        var ext = fup.value.split(".").pop().toLowerCase();
        if (ext == "pdf" || ext == "ppt" || ext == "pptx" ||
            ext == "rar" || ext == "xls" || ext == "xlsx" ||
            ext == "mp3" || ext == "mp4" || ext == "txt" ||
            ext == "doc" || ext == "docx" || ext == "avi" ||
            ext == "mpg" || ext == "wmv" || ext == "ogm" ||
            ext == "mpge" || ext == "iso" || ext == "mkv" ||
            ext == "rm" || ext == "rmvb" || ext == "mov" ) {
            HQ.common.showBusy(true, HQ.common.getLang('Uploading...'), App.frmMain);
            readImage(fup, App.imgPPCStoreMediaReq, App.hdnPPCStoreMediaReq);
        }
        else {
            HQ.message.show(148, '', '');
        }
    }
};

var PlayVideo = function (sender, e) {
    var curRecord = App.frmMain.getRecord();
    if (curRecord) {
        if (curRecord.data.Media) {
            App.direct.PlayMedia(curRecord.data.Media);
        }
    }
};

var readImage = function (fup, imgControl, ctr) {
    var files = fup.fileInputEl.dom.files;
    if (files && files[0]) {
        if (files[0].size / 1024 / 1024 > 20) {
            HQ.message.show(20150403, ['20'], '', true);
            HQ.common.showBusy(false);
        }
        else {
            ctr.setValue(fup.value);
            var FR = new FileReader();
            FR.onload = function (e) {
                imgControl.setImageUrl(e.target.result);
                HQ.common.showBusy(false);
            };
            FR.readAsDataURL(files[0]);

            var ext = fup.value.split(".").pop().toLowerCase();
            if (ext != "jpg" || ext != "png" || ext != "gif") {
                displayImage(App.imgPPCStoreMediaReq, fup.value);
            }
        }
    }
};

var calcPage = function (value) {
    var tmpValue = Number(value) / 20;
    if (Number(tmpValue) == 0)
        return 1;
    if (Number.isInteger(tmpValue))
        return Number(tmpValue);
    else
        return Math.floor(Number(tmpValue)) + 1;
};

var updateTreeView = function (tree, fn) {
    var view = tree.getView();
    view.getStore().loadRecords(fn(tree.getRootNode()));
    view.refresh();
};

var collapseAll = function (tree) {
    this.updateTreeView(tree, function (root) {
        root.cascadeBy(function (node) {
            if (!node.isRoot() || tree.rootVisible) {
                node.data.expanded = false;
            }
        });
        return tree.rootVisible ? [root] : root.childNodes;
    });
};

var expandAll = function (tree) {
    this.updateTreeView(tree, function (root) {
        var nodes = [];
        root.cascadeBy(function (node) {
            if (!node.isRoot() || tree.rootVisible) {
                node.data.expanded = true;
                nodes.push(node);
            }
        });
        return nodes;
    });
};




/////////////excel
var btnExport_Click = function () {
    App.frmMain.submit({
        url: 'IN20500/Export',
        type: 'POST',
        timeout: 1000000,
        clientValidation: false,
        params: {

        },
        success: function (msg, data) {
            var filePath = data.result.filePath;
            if (filePath) {
                window.location = "IN20500/Download?filePath=" + filePath + "&fileName=data_";
            }
          
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });

}
var btnImport_Click = function () {
    App.frmMain.submit({
        waitMsg: HQ.common.getLang("WaitMsg"),
        url: 'IN20500/Import',
        type: 'POST',
        timeout: 1000000,
        clientValidation: false,
        success: function (msg, data) {
            if (this.result.data.message) {
                HQ.message.show('2013103001', [this.result.data.message], '', true);
            }
            else {
                HQ.message.process(msg, data, true);
            }
            refresh("yes");
            App.cboInvtID.store.reload();
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
            App.btnImport.reset();
        }
    });
}
var joinParams = function (multiCombo) {
    var returnValue = "";
    if (multiCombo.value && multiCombo.value.length) {
        returnValue = multiCombo.value.join();
    }
    else {
        if (multiCombo.getValue()) {
            returnValue = multiCombo.rawValue;
        }
    }
    return returnValue;
}