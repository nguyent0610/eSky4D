var _beginStatus = "H";
var _recordEdit;

var Process = {
    renderStatus: function (value) {
        var record = App.cboStatusFilter.store.findRecord("Code", value);
        if (record) {
            return record.data.Descr;
        }
        else {
            return value;
        }
    },
    renderPOType: function (value) {
        var record = App.stoPOType.findRecord("Code", value);
        if (record) {
            return record.data.Descr;
        }
        else {
            return value;
        }
    },
    saveData: function () {
        if (HQ.store.getAllData(App.grdDet.store, ["Selected"], [true]).length == 2) {
            HQ.message.show(704, '', '');
            return;
        }
        if(HQ.form.checkRequirePass(App.frmMain) && App.cboHandleApprove.getValue()!='N')
        App.frmMain.submit({
            url: 'PO10400/SaveData',
            waitMsg: HQ.common.getLang('Submiting') + "...",
            timeout: 1800000,
            params: {
                lstDetChange: HQ.store.getAllData(App.grdDet.store, ["Selected"], [true])
            },
            success: function (msg, data) {
                HQ.message.process(msg, data, true);
                App.grdDet.store.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var Store = {

};

var Event = {
    Form: {
        frmMain_boxReady: function () {
            HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
            App.cboZone.getStore().load(function () {
                HQ.common.showBusy(false);
            });
            App.dtpFromDate.setValue(HQ.bussinessDate);
            App.dtpToDate.setValue(HQ.bussinessDate);
            App.frmMain.validate();

            App.cboStatusApprove.getStore().load(function () {
                if (!App.cboStatusApprove.getValue() && App.cboStatusApprove.store.data.items[0]) {
                    App.cboStatusApprove.setValue(App.cboStatusApprove.store.data.items[0]);
                    App.cboHandleApprove.store.reload();
                }
            });            
        },

        dtpFromDate_change: function (dtp, newValue, oldValue, eOpts) {
            App.dtpToDate.setMinValue(newValue);
            App.dtpToDate.validate();
        },

        btnLoad_click: function (btn, e) {
            App.grdDet.store.reload();

        },

        btnImport_click: function (btn, e) {
            Ext.Msg.alert("Import", "Coming soon!");
        },

        btnHideTrigger_click: function (ctr) {
            ctr.clearValue();
            if (ctr.id == "cboZone" ) {
                App.cboTerritory.getStore().load({
                    scope: this,
                    callback: function (records, operation, success) {
                        Event.Form.chk_Click(App.chkTerritory);
                    }
                });
            } else if (ctr.id == "cboTerritory" ) {
                App.cboBranchID.getStore().load({
                    scope: this,
                    callback: function (records, operation, success) {
                        Event.Form.chk_Click(App.chkBranchID);
                    }
                });

            } else if (ctr.id == "cboStatusApprove") {
                App.chkSelectHeader.setValue(false);
                App.grdDet.store.each(function (record) {
                    record.set("Selected", false);
                });
                App.cboHandle.store.reload();
            }
        },
        chk_Click: function (ctr) {
            if (ctr.value) {
                if (ctr.id == "chkZone") {
                    HQ.common.showBusy(true);
                    App.chkTerritory.disable();
                    HQ.combo.selectAll(App.cboZone);                   
                    App.cboTerritory.setValue('');                  
                 
                    App.cboTerritory.getStore().load({
                        scope: this,
                        callback: function (records, operation, success) {
                            Event.Form.chk_Click(App.chkTerritory);
                            //App.cboTerritory.getStore().events['load'].resume();
                        }
                    });
                    //App.cboTerritory.getStore().events['load'].suspend();
                }
                else if (ctr.id == "chkTerritory") {
                    App.chkBranchID.disable();
                    HQ.combo.selectAll(App.cboTerritory);
                    App.cboBranchID.setValue('');
                    //App.cboBranchID.getStore().events['load'].suspend();
                    //App.cboBranchID.getStore().events['load'].resume();
                    App.cboBranchID.getStore().load({
                        scope: this,
                        callback: function (records, operation, success) {
                            Event.Form.chk_Click(App.chkBranchID);                            
                        }
                    });
                }
                else if (ctr.id == "chkBranchID") {
                    HQ.combo.selectAll(App.cboBranchID);
                    App.chkTerritory.enable();
                    App.chkBranchID.enable();
                    HQ.common.showBusy(false);
                }
                else if (ctr.id == "chkStatus") {
                    HQ.combo.selectAll(App.cboStatusFilter);
                }
            } else {
                if (ctr.id == "chkZone") {
                    HQ.common.showBusy(true);
                    App.chkTerritory.disable();
                    App.cboZone.setValue('');
                    App.cboTerritory.setValue('');
                    //App.cboTerritory.getStore().events['load'].suspend();
                    //App.cboTerritory.getStore().events['load'].resume();
                    App.cboTerritory.getStore().load({
                        scope: this,
                        callback: function (records, operation, success) {
                            Event.Form.chk_Click(App.chkTerritory);
                        }
                    });
                }
                else if (ctr.id == "chkTerritory") {
                    App.chkBranchID.disable();
                    App.cboTerritory.setValue('');
                    //App.cboBranchID.getStore().events['load'].suspend();
                    //App.cboBranchID.getStore().events['load'].resume();
                    App.cboBranchID.getStore().load({
                        scope: this,
                        callback: function (records, operation, success) {
                            Event.Form.chk_Click(App.chkBranchID);                          
                        }
                    });
                }
                else if (ctr.id == "chkBranchID") {
                    App.cboBranchID.setValue('');
                    App.chkTerritory.enable();
                    App.chkBranchID.enable();
                    HQ.common.showBusy(false);
                }
                else if (ctr.id == "chkStatus") {
                    App.cboStatusFilter.setValue('');
                }
            }
        },
        cbo_Change: function (ctr)
        {
            if (ctr.id == "cboZone" && ctr.hasFocus) {
                //App.cboTerritory.getStore().events['load'].suspend();
                //App.cboTerritory.getStore().events['load'].resume();
                App.cboTerritory.getStore().load({
                    scope   : this,
                    callback: function (records, operation, success) {
                        if (App.chkTerritory) {
                            Event.Form.chk_Click(App.chkTerritory);
                           
                        }
                    }                                                                   
                });
            } else if (ctr.id == "cboTerritory" && ctr.hasFocus) {
                //App.cboBranchID.getStore().events['load'].suspend();
                //App.cboBranchID.getStore().events['load'].resume();
                App.cboBranchID.getStore().load({
                    scope   : this,
                    callback: function (records, operation, success) {
                        if (App.chkBranchID) {
                            Event.Form.chk_Click(App.chkBranchID);
                            HQ.common.showBusy(false);
                          
                        }
                    }                                                                   
                });
               
            } else if (ctr.id == "cboStatusApprove") { // && ctr.hasFocus
                App.chkSelectHeader.setValue(false);                
                App.grdDet.store.each(function (record) {
                    record.set("Selected", false);
                });               
                App.cboHandleApprove.store.reload();
            }
        },
          
        menuClick: function (command) {
            switch (command) {
                case "first":
                    HQ.grid.first(App.grdDet);
                    break;
                case "next":
                    HQ.grid.next(App.grdDet);
                    break;
                case "prev":
                    HQ.grid.prev(App.grdDet);
                    break;
                case "last":
                    HQ.grid.last(App.grdDet);
                    break;
                case "refresh":
                    App.grdDet.store.reload();
                    break;
                case "save":
                    if (HQ.isUpdate) {

                        Process.saveData();
                    }
                    else {
                        HQ.message.show(4, '', '');
                    }
                    break;
            }
        }
    },

    Grid: {
        grd_reject: function (col, record) {
            //var grd = col.up('grid');
            //if (!record.data.tstamp) {
            //    grd.getStore().remove(record, grd);
            //    grd.getView().focusRow(grd.getStore().getCount() - 1);
            //    grd.getSelectionModel().select(grd.getStore().getCount() - 1);
            //} else {
                record.reject();
            //}
        },

        grdDet_beforeEdit: function (editor, e) {
            var record = e.record;
            if (record.data.Status != App.cboStatusApprove.getValue() || !record.data.IsApprove) return false;

           
        },

        chkSelectHeader_click: function (chk, newValue, oldValue, eOpts) {
            if (newValue) {
                App.grdDet.store.each(function (record) {
                    record.set("Selected", (record.data.Status == App.cboStatusApprove.getValue() && record.data.IsApprove));
                });

            }
            else {
                App.grdDet.store.each(function (record) {
                    record.set("Selected", false);
                });
            }
        }
    },
};
var Other = {
    calQtyAmt: function () {
        var TotAmt = 0;
        var TotalQty = 0;
        App.grdDet.store.each(function (rec) {
            if (rec.data.Selected) {
                TotAmt += rec.get('TotAmt');
                TotalQty += rec.get('TotalQty');
            }
        });
        App.txtTotAmt.setValue(TotAmt);
        App.txtTotQty.setValue(TotalQty);
        if (App.txtTotAmt.getValue() == 0) {
            HQ.common.changeData(false, 'PO10400');
        } else {
            HQ.common.changeData(true, 'PO10400');
        }
    }
   , loadPopup: function (record) {
       App.stoPO10400_pgDetail.pageSize = 50;
       App.cboPageSize.setValue('50');
      
       _recordEdit = record.data;
       ////App.cboPOSM.setValue('');
       ////App.dataPopup.reset(true);
       //posCode = record.data.POSCode;
       //dSM = record.data.DSM;
       //month = record.data.Month;
       //App.cboPOSM.getStore().reload();
       App.txtBranchID.setValue(_recordEdit.BranchID);
       App.txtPONbr.setValue(_recordEdit.PONbr);
       App.txtVendID.setValue(_recordEdit.VendID);
       App.txtBranchName.setValue(_recordEdit.BranchName);     
       App.winDetail.show();
   }
};


//// Declare //////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
// Cac ham cho xu li Popup
var _keys = ['InvtID'];
var _fieldsCheckRequire = ["InvtID", "PurchaseType", "SiteID", "PurchUnit"];
var _fieldsLangCheckRequire = ["InvtID", "PurchaseType", "SiteID", "PurchUnit"];

var _objUserDflt = null;
var _objPO_Setup = null;
var _objIN_ItemSite = null;
var _firstLoad = true;

var _invtID = "";//dung cho boby combo load store cboPurchUnit
var _classID = "";//dung cho boby combo load store cboPurchUnit
var _stkUnit = "";//dung cho boby combo load store cboPurchUnit
var _purUnit = "";//dung cho boby combo load store cboPurchUnit
var Popup = {
    //////////////////////////////////////////////////////////////////
    //// Store ///////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////
    winDetail_BeforeClose: function (sender)    
    {
        if (HQ.isChange) {
            HQ.message.show(5, '', 'Popup.closeWinDetail');
            return false;
        }
        else {
            HQ.isChange = false;
            return true;
        }
        return false;
    },
    close:function()
    {
        App.txtBranchID.setValue('');
        App.txtPONbr.setValue('');
        App.txtVendID.setValue('');
        App.txtBranchName.setValue('');
		App.cboHandle.setValue('');
        App.stoPO_Header.reload();
        App.grdDet.store.reload();
        App.txtTotAmt.setValue(0);
        App.txtTotQty.setValue(0);
    },
    loadSourcePopup: function () {
      
        HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));       
        App.stoPO10400_pdOM_UserDefault.load(function () {
            App.stoPO10400_pdPO_Setup.load(function () {
                App.stoPO10400_pdAP_VenDorTaxes.load(function () {
                    App.cboTaxID.getStore().load(function () {
                        App.cboSiteID.getStore().load(function () {
                            _objUserDflt = App.stoPO10400_pdOM_UserDefault.data.length > 0 ? App.stoPO10400_pdOM_UserDefault.getAt(0).data : { POSite: '', };;
                            if (App.stoPO10400_pdPO_Setup.data.length == 0) {                               
                                    App.txtPONbr.setValue('');
                                    App.stoPO_Header.reload();                                
                                    HQ.message.show(20404, 'PO_Setup', '');
                                    Popup.lockControl(true);
                            }
                            else {
                                _objPO_Setup = App.stoPO10400_pdPO_Setup.getAt(0).data;
                                App.stoPO_Header.reload();
                            }

                        });
                    });
                });
            });
        });
    },
    loadDataHeader: function (sto) {
        _firstLoad = true;
        HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));     
        HQ.common.setForceSelection(App.frmDetail, false, "");
        HQ.isFirstLoad = true;
        HQ.isNew = false;
        if (sto.data.length == 0) {
            HQ.store.insertBlank(sto, "PONbr");
        }
        var record = sto.getAt(0);   
        HQ.currRecord = sto.getAt(0);
        App.frmDetail.getForm().loadRecord(record);
        App.stoPO10400_pgDetail.reload();
        App.stoPO10400_pgLoadTaxTrans.reload();
        HQ.isChange = false;
        App.dtPODate.setReadOnly(!_recordEdit.IsEditPODate);
        //HQ.common.changeData(HQ.isChange, 'PO10400');
        if (!App.cboStatusApprove.getValue()) {
            App.cboStatusApprove.setValue(App.cboStatusApprove.store.select(0));
        }
    },
    loadDataDetail: function (sto) {
        //neu sto da co du lieu thi ko duoc sua cac combo ben duoi
        if (sto.data.length > 0) {
            App.txtVendID.setReadOnly(true);
        }
        else {
            App.txtVendID.setReadOnly(false);
        }
        if (_firstLoad) {
            HQ.store.insertBlank(sto, _keys);
            _firstLoad = false;
        }
        Popup.calcDet();
        Popup.frmChange();
        HQ.common.showBusy(false);
    }, loadPO10400_pdSI_Tax: function () {

    },
    loadPO10400_pdIN_Inventory: function () {

    },
    loadPO10400_pdIN_UnitConversion: function () {

    },
    loadstoPO10400_pgLoadTaxTrans: function () {
        App.stoPO10400_LoadTaxDoc.clearData();
        Popup.calcTaxTotal();
    },
    //////////////////////////////////////////////////////////////////
    //// Event ///////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////
    menufrmDetail_Click: function (command) {
        if (App.frmDetail.body.isMasked()) return;
        switch (command) {
            case "first":
                if (HQ.focus == 'grdDetail') {
                    HQ.grid.first(App.grdDetail);
                }
                break;
            case "next":
                if (HQ.focus == 'grdDetail') {
                    HQ.grid.next(App.grdDetail);
                }
                break;
            case "prev":
                if (HQ.focus == 'grdDetail') {
                    HQ.grid.prev(App.grdDetail);
                }
                break;
            case "last":
                if (HQ.focus == 'grdDetail') {
                    HQ.grid.last(App.grdDetail);
                }
                break;
            case "save":
                if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                    //checkRequire để kiếm tra các field yêu cầu có rỗng hay ko
                    if (HQ.form.checkRequirePass(App.frmDetail) && HQ.store.checkRequirePass(App.stoPO10400_pgDetail, _keys, _fieldsCheckRequire, _fieldsLangCheckRequire)) {
                        Popup.save();
                    }
                }
                break;            
            case "delete":
                if (!_recordEdit.IsEditOrder) return false;
                if (HQ.focus == 'grdDetail') {
                    var rowindex = HQ.grid.indexSelect(App.grdDetail);
                    if (rowindex != '')
                        HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdDetail), ''], 'Popup.deleteRecordGrid', true)
                }
                break;
            case "close":
                HQ.common.close(this);
                break;
            case "new":
                if (HQ.isInsert && _recordEdit.IsEditOrder) {                    
                        HQ.grid.insert(App.grdDetail, _keys);                    
                }               
                break;
            case "refresh":
                if (HQ.isChange) {
                    HQ.message.show(20150303, '', 'Popup.refresh');
                }
                else {
                    HQ.isChange = false;                 
                    App.stoPO_Header.reload();             
                }
                break;

                break;
            case "print":                
                break;
            default:
        }
    },
    frmChange: function (sender) {
        HQ.isChange = HQ.store.isChange(App.stoPO_Header) == false ? HQ.store.isChange(App.stoPO10400_pgDetail) : true;
        HQ.common.changeData(HQ.isChange, 'PO10400');//co thay doi du lieu gan * tren tab title header
    },
    stoChanged: function (sto) {
        Popup.frmChange();
    },
    grdPO_Detail_BeforeEdit: function (editor, e) {
        
        if (!_recordEdit.IsEditOrder) return false;
       
        var det = e.record.data;
        if (e.field == "DiscAmt" && det.ExtCost == 0) return false;
        _purUnit = e.record.data.PurchUnit;

        if (det.PurchaseType == "") {
            e.record.set("PurchaseType", "GI");
            e.record.set("SiteID", _objUserDflt == undefined ? "" : _objUserDflt.POSite);
            e.record.set("VouchStage", 'N');
            e.record.set("RcptStage", 'N');
            var valueTax = '';
            App.cboTaxID.getStore().data.each(function (det) {
                valueTax += det.data.taxid + ',';

            });
            valueTax = valueTax.length > 0 ? valueTax.substring(0, valueTax.length - 1) : '';
            e.record.set("TaxID", valueTax);

            e.record.set("ReqdDate", HQ.bussinessDate);
            e.record.set("PromDate", HQ.bussinessDate);
            return false;
        }
        if (det.PurchaseType == "" && e.column.dataIndex != "PurchaseType") {
            HQ.message.show(15, e.grid.columns[1].text, '');
            return false;
        }
        if (!_objPO_Setup.EditablePOPrice && e.column.dataIndex == "UnitCost") {
            return false;
        }
        if (Ext.isEmpty(det.LineRef)) {
            e.record.set('LineRef', Popup.lastLineRef(App.stoPO10400_pgDetail));
        }
        if (e.field == 'PurchUnit') {
            _purUnit = e.value;
            var objIN_Inventory = HQ.store.findInStore(App.stoPO10400_pdIN_Inventory, ["InvtID"], [det.InvtID]);
            _invtID = objIN_Inventory.InvtID;
            _classID = objIN_Inventory.ClassID;
            _stkUnit = objIN_Inventory.StkUnit;
            App.cboPurchUnit.getStore().reload();
        }


    },
    grdPO_Detail_ValidateEdit: function (item, e) {      
        if (_keys.indexOf(e.field) != -1) {
            if (HQ.grid.checkDuplicate(App.grdDetail, e, _keys)) {
                HQ.message.show(1112, e.value, '');
                return false;
            }
        }
        if (e.field == "InvtID") {
            var r = HQ.store.findInStore(App.cboInvtID.getStore(), ["InvtID"], [e.value]);
            var objdet = App.slmPO_Detail.getSelection()[0];

            if (r == undefined) {
                objdet.set('TranDesc', "");
                objdet.set('PurchUnit', "");
                //objdet.set('TranDesc', "");
            }
            else {
                var objIN_Inventory = HQ.store.findInStore(App.stoPO10400_pdIN_Inventory, ["InvtID"], [r.InvtID]);
                _invtID = objIN_Inventory.InvtID;
                _classID = objIN_Inventory.ClassID;
                _stkUnit = objIN_Inventory.StkUnit;

                App.cboPurchUnit.getStore().reload();

                if (objdet.get("SiteID") == "") {
                    if (_objUserDflt != null) {
                        objdet.set('SiteID', _objUserDflt.POSite);
                    }                   
                    else {
                        objdet.set('SiteID', objIN_Inventory.DfltSite);
                    }

                }
                objdet.set('TaxCat', objIN_Inventory.TaxCat == null ? "" : objIN_Inventory.TaxCat);
                objdet.set('PurchUnit', objIN_Inventory.DfltPOUnit == null ? "" : objIN_Inventory.DfltPOUnit);
                objdet.set('UnitWeight', objIN_Inventory.StkWt);
                objdet.set('UnitVolume', objIN_Inventory.StkVol);
                objdet.set('TranDesc', r.Descr);
            }
        }
    },
    grdPO_Detail_Edit: function (item, e) {
        HQ.common.showBusy(true,'',App.frmDetail);
        var objDetail = e.record.data;
        var objIN_Inventory = HQ.store.findInStore(App.stoPO10400_pdIN_Inventory, ["InvtID"], [objDetail.InvtID]);
        objIN_Inventory = objIN_Inventory == null ? "" : objIN_Inventory;

        if (e.field == "PurchUnit" || e.field == "InvtID") {
            var cnv = Popup.setUOM(objIN_Inventory.InvtID, objIN_Inventory.ClassID, objIN_Inventory.StkUnit, objDetail.PurchUnit);
            if (!Ext.isEmpty(cnv)) {
                var cnvFact = cnv.CnvFact;
                var unitMultDiv = cnv.MultDiv;
                objDetail.CnvFact = cnv.CnvFact;
                objDetail.UnitMultDiv = cnv.MultDiv;
                e.record.set('CnvFact', cnvFact);
                e.record.set('UnitMultDiv', unitMultDiv);
            } else {
                e.record.set('PurchUnit', "");
                //return;
            }
            HQ.grid.checkInsertKey(App.grdDetail, e, _keys);
        }
        if (e.field == "QtyOrd") {
            if (objDetail.PurchaseType == "FA") {
                if (objDetail.QtyOrd > 1) {

                    HQ.message.show(58, '', '');
                    HQ.common.showBusy(false, '', App.frmDetail);
                    return false;

                }
            }

            StkQty = Math.round((objDetail.UnitMultDiv == "D" ? (objDetail.QtyOrd / objDetail.CnvFact) : (objDetail.QtyOrd * objDetail.CnvFact)));
            e.record.set("DiscAmt", HQ.util.mathRound((objDetail.UnitCost * objDetail.QtyOrd * objDetail.DiscPct) / 100, 2));
            e.record.set("ExtCost", objDetail.QtyOrd * objDetail.UnitCost - objDetail.DiscAmt);
            objDetail.POFee = StkQty * objIN_Inventory.POFee;

            e.record.set("ExtWeight", objDetail.QtyOrd * objDetail.UnitWeight);
            e.record.set("ExtVolume", objDetail.QtyOrd * objDetail.UnitVolume);
        }
        else if (e.field == "UnitWeight") {
            e.record.set("ExtWeight", objDetail.QtyOrd * objDetail.UnitWeight);

        }
        else if (e.field == "UnitCost") {
            e.record.set("DiscAmt", HQ.util.mathRound((objDetail.UnitCost * objDetail.QtyOrd * objDetail.DiscPct) / 100, 2));
            e.record.set("ExtCost", objDetail.QtyOrd * objDetail.UnitCost - objDetail.DiscAmt);

        }
        else if (e.field == "UnitVolume") {
            e.record.set("ExtVolume", objDetail.QtyOrd * objDetail.UnitVolume);

        }
        else if (e.field == "DiscAmt") {
            if (e.value > objDetail.UnitCost * objDetail.QtyOrd)
                e.record.set("DiscAmt", 0);
            e.record.set("ExtCost", objDetail.UnitCost * objDetail.QtyOrd - objDetail.DiscAmt);
            if (objDetail.QtyOrd != 0) {
                e.record.set("DiscPct", HQ.util.mathRound((objDetail.DiscAmt / (objDetail.UnitCost * objDetail.QtyOrd)) * 100, 2));//Math.round((objDetail.DiscAmt / (objDetail.UnitCost * objDetail.QtyOrd)) * 100, 2));
            }
        }
        else if (e.field == "DiscPct") {
            if (objDetail.ExtCost != 0) {
                e.record.set("DiscAmt", HQ.util.mathRound((objDetail.UnitCost * objDetail.QtyOrd * objDetail.DiscPct) / 100, 2));//Math.round((objDetail.UnitCost * objDetail.QtyOrd * objDetail.DiscPct) / 100, 2));
            }
            e.record.set("ExtCost", objDetail.UnitCost * objDetail.QtyOrd - objDetail.DiscAmt);
        }
        else if (e.field == "PurchUnit" || e.field == "InvtID" || e.field == "SiteID") {
            if (_objPO_Setup.DfltLstUnitCost == "A" || _objPO_Setup.DfltLstUnitCost == "L") {
                //HQ.common.showBusy(true);
                App.direct.PO10400ItemSitePrice(
                    App.txtBranchID.getValue(), objDetail.InvtID, objDetail.SiteID,
                   {
                       success: function (result) {
                           _objIN_ItemSite = result;
                           UnitCost = result == null ? 0 : (_objPO_Setup.DfltLstUnitCost == "A" ? result.AvgCost : result.LastPurchasePrice);
                           UnitCost = Math.round((objDetail.UnitMultDiv == "D" ? (UnitCost / objDetail.CnvFact) : (UnitCost * objDetail.CnvFact)));
                           e.record.set("UnitCost", UnitCost);
                           e.record.set("DiscAmt", HQ.util.mathRound((UnitCost * objDetail.QtyOrd * objDetail.DiscPct) / 100, 2));
                           e.record.set("ExtCost", UnitCost * objDetail.QtyOrd - objDetail.DiscAmt);

                           HQ.common.showBusy(false, '', App.frmDetail);
                           Popup.delTax(e.rowIdx);
                           Popup.calcTax(e.rowIdx);
                           Popup.calcTaxTotal();
                       },
                       failure: function (result) {
                           HQ.common.showBusy(false, '', App.frmDetail);
                       }

                   });
            }
            else if (_objPO_Setup.DfltLstUnitCost == "P") {
               // HQ.common.showBusy(true);
                App.direct.PO10400POPrice(
                   App.txtBranchID.getValue(), objDetail.InvtID, objDetail.PurchUnit, Ext.Date.format(App.dtPODate.getValue(), 'Y-m-d'),
                    {
                        success: function (result) {
                            UnitCost = result;
                            e.record.set("UnitCost", result);
                            e.record.set("DiscAmt", HQ.util.mathRound((result * objDetail.QtyOrd * objDetail.DiscPct) / 100, 2));
                            e.record.set("ExtCost", result * objDetail.QtyOrd - objDetail.DiscAmt);
                            HQ.common.showBusy(false, '', App.frmDetail);
                            Popup.delTax(e.rowIdx);
                            Popup.calcTax(e.rowIdx);
                            Popup.calcTaxTotal();
                        },
                        failure: function (result) {
                            HQ.common.showBusy(false, '', App.frmDetail);
                        }
                    });

            }
            else if (_objPO_Setup.DfltLstUnitCost == "I") {
                var UnitCost = objIN_Inventory.POPrice;
                UnitCost = Math.round((objDetail.UnitMultDiv == "D" ? (UnitCost / objDetail.CnvFact) : (UnitCost * objDetail.CnvFact)));
                e.record.set("UnitCost", UnitCost);
                e.record.set("DiscAmt", HQ.util.mathRound((UnitCost * objDetail.QtyOrd * objDetail.DiscPct) / 100, 2));
                e.record.set("ExtCost", UnitCost * objDetail.QtyOrd - objDetail.DiscAmt);
                Popup.delTax(e.rowIdx);
                Popup.calcTax(e.rowIdx);
                Popup.calcTaxTotal();
                HQ.common.showBusy(false, '', App.frmDetail);
            }
        }
        if (objDetail.PurchaseType == "PR") {
            e.record.set("UnitCost", 0);
            e.record.set("ExtCost", 0);
            e.record.set("DiscPct", 0);
            e.record.set("DiscAmt", 0);
        }
        if (e.field == "QtyOrd" || e.field == "DiscPct" || e.field == "DiscAmt" || e.field == "UnitCost") {
            Popup.delTax(e.rowIdx);
            Popup.calcTax(e.rowIdx);
            Popup.calcTaxTotal();
            HQ.common.showBusy(false, '', App.frmDetail);
        }
       
        //calcDet();

    },
    grdPO_Detail_Deselect: function (item, e) {
        Popup.calcDet();
        Popup.delTax(e.rowIdx);
        Popup.calcTaxTotal();
    },
    grdPO_Detail_Reject: function (record) {
        if (record.data.tstamp == '') {
            var index = App.stoPO10400_pgDetail.indexOf(record);
            Popup.delTax(index);
            Popup.calcTaxTotal();
            App.grdDetail.getStore().remove(record, App.grdDetail);
            App.grdDetail.getView().focusRow(App.grdDetail.getStore().getCount() - 1);
            App.grdDetail.getSelectionModel().select(App.grdDetail.getStore().getCount() - 1);
            Popup.calcDet();
        } else {
            var index = App.stoPO10400_pgDetail.indexOf(record);
            record.reject();
            Popup.delTax(index);
            Popup.calcTax(index);
            Popup.calcTaxTotal();
            Popup.calcDet();

        }
    },
    cboGInvtID_Change: function (item, newValue, oldValue) {


    },

   
    //Cac store co param la VendID thi load lai sau khi VendID thay doi
   
    cboStatus_Change: function (item, newValue, oldValue) {
        App.cboHandle.getStore().reload();
       
    },
   
    ///////DataProcess///
    save: function () {
        App.frmDetail.getForm().updateRecord();
        if (App.frmDetail.isValid()) {
            App.frmDetail.submit({
                waitMsg: HQ.common.getLang('SavingData'),
                method: 'POST',
                url: 'PO10400/SaveEdit',
                timeout: 1800000,
                params: {
                    lstDet: Ext.encode(App.stoPO10400_pgDetail.getRecordsValues()),
                    lstHeader: Ext.encode(App.stoPO_Header.getRecordsValues())

                },
                success: function (msg, data) {
                    HQ.message.process(msg, data, true);                       
                    App.stoPO_Header.reload();                   
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                    App.stoPO_Header.reload();
                }
            });
        }
    },
    deleteRecordGrid: function (item) {

        if (item == 'yes') {
            if (App.slmPO_Detail.selected.items[0].data.tstamp != "") {
                App.grdDetail.deleteSelected();
                Popup.delTaxMutil();
                Popup.calcDet();
                App.frmDetail.getForm().updateRecord();
                if (App.frmDetail.isValid()) {
                    App.frmDetail.submit({
                        waitMsg: HQ.common.getLang('DeletingData'),
                        method: 'POST',
                        url: 'PO10400/DeleteGrd',
                        timeout: 180000,
                        params: {
                            lstDel: HQ.store.getData(App.stoPO10400_pgDetail),
                            lstDet: Ext.encode(App.stoPO10400_pgDetail.getRecordsValues()),
                            lstHeader: Ext.encode(App.stoPO_Header.getRecordsValues())

                        },
                        success: function (msg, data) {
                            HQ.message.process(msg, data, true);                              
                            App.stoPO_Header.reload();
                        },
                        failure: function (msg, data) {
                            HQ.message.process(msg, data, true);                               
                            App.stoPO_Header.reload();
                             
                        }
                    });
                }
            }
            else {
                App.grdDetail.deleteSelected();
                Popup.delTaxMutil();
                Popup.calcDet();
            }
        }
        
    },
    //////////////////////////////////////////////////////////////////
    //// Function ////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////

    //HAM TINH THUNG LE
    calCTNPCS: function () {

        var CTN = 0;
        var PCS = 0;
        var CnvFact = 1;
        for (var j = 0; j < App.stoPO10400_pgDetail.data.length; j++) {
            var det = App.stoPO10400_pgDetail.data.items[j];
            CnvFact = Popup.oM_GetCnvFactToUnit(det.data.InvtID, det.data.PurchUnit);
            if (CnvFact == 1) {
                CTN += det.data.QtyOrd;
            }
            else if (CnvFact > 1) {
                CTN += Math.Floor(det.dataS.QtyOrd / CnvFact);
                PCS += det.data.QtyOrd % CnvFact;
            }
        };
        App.txtCA.setValue(Math.round(CTN, 0));
        App.txtEA.setValue(Math.round(PCS, 0));

    },
    //Cal tax
    calcDet: function () {
        //if (App.OrderType.getValue() == null) return;
        var taxAmt00 = 0;
        var taxAmt01 = 0;
        var taxAmt02 = 0;
        var taxAmt03 = 0;
        var taxAmt03 = 0;
        var extvol = 0;
        var extwei = 0;
        var extCost = 0;
        var discount = 0;
        var poFee = 0;
        var CnvFact = 0;
        var CTN = 0;
        var PCS = 0;

        for (var j = 0; j < App.stoPO10400_pgDetail.allData.length; j++) {
            var det = App.stoPO10400_pgDetail.allData.items[j];
            taxAmt00 += det.data.TaxAmt00;
            taxAmt01 += det.data.TaxAmt01;
            taxAmt02 += det.data.TaxAmt02;
            taxAmt03 += det.data.TaxAmt03;
            poFee += det.data.POFee;
            extvol += det.data.ExtVolume;
            extwei += det.data.ExtWeight;
            extCost += det.data.ExtCost;
            discount += det.data.DiscAmt
            //ordQty += det.data.LineQty;

            //tinh thung le  
            CnvFact = Popup.oM_GetCnvFactToUnit(det.data.InvtID, det.data.PurchUnit);
            if (CnvFact == 1) {
                CTN += det.data.QtyOrd;
            }
            else if (CnvFact > 1) {
                CTN += Math.round(det.data.QtyOrd / CnvFact, 0);
                PCS += det.data.QtyOrd % CnvFact;
            }

        };

        App.txtCA.setValue(Math.round(CTN, 0));
        App.txtEA.setValue(Math.round(PCS, 0));
        App.POFeeTot.setValue(Math.round(poFee, 0));
        App.TAX.setValue(Math.round(taxAmt00 + taxAmt01 + taxAmt02 + taxAmt03, 0));
        App.EXTVOL.setValue(Math.round(extvol, 0));
        App.EXTWEIGHT.setValue(Math.round(extwei, 0));

        App.POAmt.setValue(Math.round(taxAmt00 + taxAmt01 + taxAmt02 + taxAmt03, 0) + Math.round(extCost, 0) + Math.round(poFee, 0));
        //App.CuryLineAmt.setValue(Math.round(curyLineAmt, 0));
        App.DISCOUNT.setValue(Math.round(discount, 0));
        App.txtRcptTotAmt.setValue(Math.round(extCost, 0) + Math.round(poFee, 0));

    },
    delTaxMutil: function () {

        for (var i = App.stoPO10400_pgLoadTaxTrans.data.length - 1; i >= 0; i--) {
            var data = HQ.store.findInStore(App.stoPO10400_pgDetail, ['LineRef'], [App.stoPO10400_pgLoadTaxTrans.data.items[i].data.LineRef])
            if (!data) App.stoPO10400_pgLoadTaxTrans.data.removeAt(i);
        }
        Popup.calcTaxTotal();
    },
    delTax: function delTax(index) {
        //if (App.cboStatus != "H" ) return false;
        var lineRef = App.stoPO10400_pgDetail.data.items[index].data.LineRef;

        for (var j = App.stoPO10400_pgLoadTaxTrans.data.length - 1; j >= 0; j--) {
            if (App.stoPO10400_pgLoadTaxTrans.data.items[j].data.LineRef == lineRef)
                App.stoPO10400_pgLoadTaxTrans.data.removeAt(j);
        }
        Popup.clearTax(index);
        Popup.calcTaxTotal();
        Popup.calcDet();
        return true;

    },
    clearTax: function (index) {
        App.stoPO10400_pgDetail.data.items[index].set('TaxID00', '');
        App.stoPO10400_pgDetail.data.items[index].set('TaxAmt00', 0);
        App.stoPO10400_pgDetail.data.items[index].set('TxblAmt00', 0);

        App.stoPO10400_pgDetail.data.items[index].set('TaxID01', '');
        App.stoPO10400_pgDetail.data.items[index].set('TaxAmt01', 0);
        App.stoPO10400_pgDetail.data.items[index].set('TxblAmt01', 0);

        App.stoPO10400_pgDetail.data.items[index].set('TaxID02', '');
        App.stoPO10400_pgDetail.data.items[index].set('TaxAmt02', 0);
        App.stoPO10400_pgDetail.data.items[index].set('TxblAmt02', 0);

        App.stoPO10400_pgDetail.data.items[index].set('TaxID03', '');
        App.stoPO10400_pgDetail.data.items[index].set('TaxAmt03', 0);
        App.stoPO10400_pgDetail.data.items[index].set('TxblAmt03', 0);
    },
    calcTax: function (index) {

        var det = App.stoPO10400_pgDetail.data.items[index].data;
        var record = App.stoPO10400_pgDetail.data.items[index];
        if (index < 0) return true;


        var dt = [];
        if (det.TaxID == "*") {
            for (var j = 0; j < App.stoPO10400_pdAP_VenDorTaxes.data.length; j++) {
                var item = App.stoPO10400_pdAP_VenDorTaxes.data.items[j];
                dt.push(item.data);
            };
        }
        else {
            var strTax = det.TaxID.split(',');
            if (strTax.length > 0) {
                for (var k = 0; k < strTax.length; k++) {
                    for (var j = 0; j < App.stoPO10400_pdAP_VenDorTaxes.data.length; j++) {
                        if (strTax[k] == App.stoPO10400_pdAP_VenDorTaxes.data.items[j].data.taxid) {
                            dt.push(App.stoPO10400_pdAP_VenDorTaxes.data.items[j].data);
                            break;
                        }
                    }
                }
            }
            else {
                if (Ext.isEmpty(det.TaxID) || Ext.isEmpty(det.TaxCat))
                    App.stoPO10400_pgDetail.data.items[i].set('TxblAmt00', det.ExtCost);
                return false;
            }
        }

        var taxCat = det.TaxCat;
        var prcTaxInclRate = 0, totPrcTaxInclAmt = 0, txblAmtL1 = 0, txblAmtAddL2 = 0;
        for (var j = 0; j < dt.length; j++) {
            var objTax = HQ.store.findInStore(App.stoPO10400_pdAP_VenDorTaxes, ['taxid'], [dt[j].taxid]);
            if (!Ext.isEmpty(objTax) && !Ext.isEmpty(taxCat)) {
                if (taxCat == "*" || (objTax.CatFlg == "A" && objTax.CatExcept00 != taxCat && objTax.CatExcept01 != taxCat
                                                           && objTax.CatExcept02 != taxCat && objTax.CatExcept03 != taxCat
                                                           && objTax.CatExcept04 != taxCat && objTax.CatExcept05 != taxCat)
                                  || (objTax.CatFlg == "N" && (objTax.CatExcept00 == taxCat || objTax.CatExcept01 == taxCat ||
                                                                objTax.CatExcept02 == taxCat || objTax.CatExcept03 == taxCat ||
                                                                objTax.CatExcept04 == taxCat || objTax.CatExcept05 == taxCat))) {

                    if (objTax.TaxCalcLvl == "1" && objTax.PrcTaxIncl != "0") {
                        prcTaxInclRate = prcTaxInclRate + objTax.TaxRate;
                    }
                }
            }
        }


        if (prcTaxInclRate == 0)
            txblAmtL1 = Math.round(det.ExtCost, 0);
        else
            txblAmtL1 = Math.round((det.ExtCost) / (1 + prcTaxInclRate / 100), 0);


        record.set('TxblAmt00', txblAmtL1);

        for (var j = 0; j < dt.length; j++) {

            var taxID = "", lineRef = "";
            var taxRate = 0, taxAmtL1 = 0;
            var objTax = HQ.store.findInStore(App.stoPO10400_pdAP_VenDorTaxes, ['taxid'], [dt[j].taxid]);
            if (!Ext.isEmpty(objTax) && !Ext.isEmpty(taxCat)) {
                if (taxCat == "*" || (objTax.CatFlg == "A" && objTax.CatExcept00 != taxCat && objTax.CatExcept01 != taxCat
                                                           && objTax.CatExcept02 != taxCat && objTax.CatExcept03 != taxCat
                                                           && objTax.CatExcept04 != taxCat && objTax.CatExcept05 != taxCat)
                                  || (objTax.CatFlg == "N" && (objTax.CatExcept00 == taxCat || objTax.CatExcept01 == taxCat ||
                                                                objTax.CatExcept02 == taxCat || objTax.CatExcept03 == taxCat ||
                                                                objTax.CatExcept04 == taxCat || objTax.CatExcept05 == taxCat))) {
                    if (objTax.TaxCalcLvl == "1") {
                        taxID = dt[j].taxid;
                        lineRef = det.LineRef;
                        taxRate = objTax.TaxRate;
                        taxAmtL1 = HQ.util.mathRound(txblAmtL1 * objTax.TaxRate / 100, 2);//Math.round(txblAmtL1 * objTax.TaxRate / 100, 2);

                        if (objTax.Lvl2Exmpt == 0) txblAmtAddL2 += txblAmtL1;

                        if (objTax.PrcTaxIncl != "0") {
                            var chk = false;
                            if (j < dt.length - 1) {
                                for (var k = j + 1; k < dt.length; k++) {
                                    objTax = dt[k];
                                    if (!Ext.isEmpty(objTax) && !Ext.isEmpty(taxCat)) {
                                        if (taxCat == "*" || (objTax.CatFlg == "A" && objTax.CatExcept00 != taxCat &&
                                                                objTax.CatExcept01 != taxCat && objTax.CatExcept02 != taxCat &&
                                                                objTax.CatExcept03 != taxCat && objTax.CatExcept04 != taxCat &&
                                                                objTax.CatExcept05 != taxCat)
                                                          || (objTax.CatFlg == "N" && (objTax.CatExcept00 == taxCat || objTax.CatExcept01 == taxCat ||
                                                                                        objTax.CatExcept02 == taxCat || objTax.CatExcept03 == taxCat ||
                                                                                        objTax.CatExcept04 == taxCat || objTax.CatExcept05 == taxCat))) {
                                            if (objTax.TaxCalcLvl == "1" && objTax.PrcTaxIncl != "0") {
                                                chk = false;
                                                break;
                                            }
                                        }
                                    }
                                    chk = true;
                                }
                            }
                            else {
                                chk = true;
                            }

                            if (chk) {

                                if (totPrcTaxInclAmt + taxAmtL1 + txblAmtL1 != det.ExtCost)
                                    taxAmtL1 = HQ.util.mathRound(det.ExtCost - (totPrcTaxInclAmt + txblAmtL1), 2); //Math.round(det.ExtCost - (totPrcTaxInclAmt + txblAmtL1), 2);

                            }
                            else
                                totPrcTaxInclAmt += totPrcTaxInclAmt + taxAmtL1;
                        }

                        Popup.insertUpdateTax(taxID, lineRef, taxRate, taxAmtL1, txblAmtL1, 1);

                    }
                }
            }
        }

        for (var j = 0; j < dt.Count; j++) {
            var taxID = "", lineRef = "";
            var taxRate = 0, txblAmtL2 = 0, taxAmtL2 = 0;
            var objTax = HQ.store.findInStore(App.stoPO10400_pdAP_VenDorTaxes, ['taxid'], [dt[j].taxid]);
            if (!Ext.isEmpty(objTax) && !Ext.isEmpty(taxCat)) {
                if (taxCat == "*" || (objTax.CatFlg == "A" && objTax.CatExcept00 != taxCat && objTax.CatExcept01 != taxCat
                                                           && objTax.CatExcept02 != taxCat && objTax.CatExcept03 != taxCat
                                                           && objTax.CatExcept04 != taxCat && objTax.CatExcept05 != taxCat)
                                  || (objTax.CatFlg == "N" && (objTax.CatExcept00 == taxCat || objTax.CatExcept01 == taxCat ||
                                                                objTax.CatExcept02 == taxCat || objTax.CatExcept03 == taxCat ||
                                                                objTax.CatExcept04 == taxCat || objTax.CatExcept05 == taxCat))) {
                    if (objTax.TaxCalcLvl == "2") {
                        taxID = dt[j].taxid;
                        lineRef = det.LineRef;
                        taxRate = objTax.TaxRate;
                        txblAmtL2 = Math.round(txblAmtAddL2 + txblAmtL1, 0);
                        taxAmtL2 = HQ.util.mathRound(txblAmtAddL2 * objTax.TaxRate / 100, 2);//Math.round(txblAmtAddL2 * objTax.TaxRate / 100, 2);
                        Popup.insertUpdateTax(taxID, lineRef, taxRate, taxAmtL2, txblAmtL2, 2);
                    }
                }
            }
        }
        Popup.updateTax(index);
        Popup.calcDet();
        return true;
    },
    insertUpdateTax: function (taxID, lineRef, taxRate, taxAmt, txblAmt, taxLevel) {
        var flat = false;
        for (var i = 0; i < App.stoPO10400_pgLoadTaxTrans.data.length; i++) {
            if (App.stoPO10400_pgLoadTaxTrans.data.items[i].data.taxid == taxID && App.stoPO10400_pgLoadTaxTrans.data.items[i].data.LineRef == lineRef) {
                var tax = App.stoPO10400_pdAP_VenDorTaxes.data.items[i];
                tax.set('BranchID', App.txtBranchID.getValue()),
                tax.set('TaxID', taxID);
                tax.set('LineRef', lineRef);
                tax.set('TaxRate', taxRate);
                tax.set('TaxLevel', taxLevel.toString());
                tax.set('TaxAmt', taxAmt)
                tax.set('TxblAmt', txblAmt);
                flat = true;
                break;
            }
        }
        if (!flat) {
            var newTax = Ext.create('App.ModelPO10400_pgLoadTaxTrans_Result');
            newTax.data.BranchID = App.txtBranchID.getValue();
            newTax.data.TaxID = taxID;
            newTax.data.LineRef = lineRef;
            newTax.data.TaxRate = taxRate;
            newTax.data.TaxLevel = taxLevel.toString();
            newTax.data.TaxAmt = taxAmt;
            newTax.data.TxblAmt = txblAmt;

            App.stoPO10400_pgLoadTaxTrans.data.add(newTax);
        }
        App.stoPO10400_pgLoadTaxTrans.sort('LineRef', "ASC");
        Popup.calcDet();

    },
    updateTax: function (index) {

        if (index < 0) return;
        var j = 0;
        var det = App.stoPO10400_pgDetail.data.items[index].data;
        var record = App.stoPO10400_pgDetail.data.items[index];
        for (var i = 0; i < App.stoPO10400_pgLoadTaxTrans.data.length; i++) {
            var item = App.stoPO10400_pgLoadTaxTrans.data.items[i];
            if (item.data.LineRef == det.LineRef) {
                if (j == 0) {
                    record.set('TaxID00', item.data.TaxID);
                    record.set('TxblAmt00', item.data.TxblAmt);
                    record.set('TaxAmt00', item.data.TaxAmt);
                }
                else if (j == 1) {
                    record.set('TaxID01', item.data.TaxID);
                    record.set('TxblAmt01', item.data.TxblAmt);
                    record.set('TaxAmt01', item.data.TaxAmt);
                }
                else if (j == 2) {
                    record.set('TaxID02', item.data.TaxID);
                    record.set('TxblAmt02', item.data.TxblAmt);
                    record.set('TaxAmt02', item.data.TaxAmt);
                }
                else if (j == 3) {
                    record.set('TaxID03', item.data.TaxID);
                    record.set('TxblAmt03', item.data.TxblAmt);
                    record.set('TaxAmt03', item.data.TaxAmt);
                }
                j++;
            }
            if (j != 0 && item.data.LineRef != det.LineRef)
                return false;
        };

    },
    calcTaxTotal: function () {
        App.stoPO10400_LoadTaxDoc.clearData();
        var flat = false;
        for (var i = 0; i < App.stoPO10400_pgLoadTaxTrans.data.length; i++) {
            var tax = App.stoPO10400_pgLoadTaxTrans.data.items[i];
            flat = true;
            for (var j = 0; j < App.stoPO10400_LoadTaxDoc.data.length; j++) {
                var taxDoc = App.stoPO10400_LoadTaxDoc.data.items[j];
                if (tax.data.PONbr == taxDoc.data.PONbr && tax.data.TaxID == taxDoc.data.TaxID) {
                    taxDoc.data.TxblAmt += tax.data.TxblAmt;
                    taxDoc.data.TaxAmt += tax.data.TaxAmt;
                    flat = false;
                    taxDoc.commit();
                    break;
                }
            };
            if (flat) {
                var newTaxDoc = Ext.create('App.mdlPO10400_pgLoadTaxTransDoc');
                newTaxDoc.data.BranchID = tax.data.BranchID;
                newTaxDoc.data.PONbr = tax.data.PONbr;
                newTaxDoc.data.TaxID = tax.data.TaxID;
                newTaxDoc.data.TaxAmt = tax.data.TaxAmt;
                newTaxDoc.data.TaxRate = tax.data.TaxRate;
                newTaxDoc.data.TxblAmt = tax.data.TxblAmt;

                App.stoPO10400_LoadTaxDoc.data.add(newTaxDoc);
                // newTaxDoc.commit();
            }

        };
        App.grdTaxTrans.getView().refresh(false);
        App.grdTaxDoc.getView().refresh(false);

    },
    lastLineRef: function (store) {
        var num = 0;
        for (var j = 0; j < store.data.length; j++) {
            var item = store.data.items[j];

            if (!Ext.isEmpty(item.data.LineRef) && parseInt(item.data.LineRef) > num) {
                num = parseInt(item.data.LineRef);
            }
        };
        num++;
        var lineRef = num.toString();
        var len = lineRef.length;
        for (var i = 0; i < 5 - len; i++) {
            lineRef = "0" + lineRef;
        }
        return lineRef;
    },
    setUOM: function (invtID, classID, stkUnit, fromUnit) {
        if (!Ext.isEmpty(fromUnit)) {
            var data = HQ.store.findInStore(App.stoPO10400_pdIN_UnitConversion, ['UnitType', 'ClassID', 'InvtID', 'FromUnit', 'ToUnit'], ["3", "*", invtID, fromUnit, stkUnit]);
            if (!Ext.isEmpty(data)) {
                return data;
            }

            data = HQ.store.findInStore(App.stoPO10400_pdIN_UnitConversion, ['UnitType', 'ClassID', 'InvtID', 'FromUnit', 'ToUnit'], ["2", classID, "*", fromUnit, stkUnit]);
            if (!Ext.isEmpty(data)) {
                return data;
            }

            data = HQ.store.findInStore(App.stoPO10400_pdIN_UnitConversion, ['UnitType', 'ClassID', 'InvtID', 'FromUnit', 'ToUnit'], ["1", "*", "*", fromUnit, stkUnit]);
            if (!Ext.isEmpty(data)) {
                return data;
            }
            HQ.message.show(25, invtID, '');
            return null;
        }
        return null;
    },
    oM_GetCnvFactToUnit: function (invtID, unitDesc) {
        var cnvFact = 1;
        var data;
        App.stoPO10400_pdIN_UnitConversion.data.each(function (item) {
            if (item.data.InvtID == invtID && item.data.FromUnit != unitDesc && item.data.ToUnit == unitDesc) {
                data = item;
                return;
            }
        });
        if (data != null) {
            if (data.data.MultDiv == "D")
                cnvFact = 1 / data.data.CnvFact;
            else
                cnvFact = data.data.CnvFact;
        }

        return cnvFact;
    },

    //// Other Functions ////////////////////////////////////////////////////
    lockControl: function (isLock) {
        HQ.common.lockItem(App.frmDetail, isLock);
    },
    refresh: function refresh(item) {
        if (item == 'yes') {
            HQ.isChange = false;           
              App.stoPO_Header.reload();
            
        }
    },
    renderPurchaseType: function (value) {
        var obj = App.PurchaseType.getStore().findRecord("Code", value);
        if (obj) {
            return obj.data.Descr;
        }
        return value;
    },
    renderVouchStage: function (value) {
        var obj = App.VouchStage.getStore().findRecord("Code", value);
        if (obj) {
            return obj.data.Descr;
        }
        return value;
    },
    renderRcptStage: function (value) {
        var obj = App.RcptStage.getStore().findRecord("Code", value);
        if (obj) {
            return obj.data.Descr;
        }
        return value;
    },
    renderSiteID: function (value) {
        var obj = App.cboSiteID.getStore().findRecord("SiteID", value);
        if (obj) {
            return obj.data.Name;
        }
        return value;
    },
    renderTaxID: function (value) {
        //App.stoPO10400_pgDetail.data.each(function (det) {
        //    value.split(',')

        //});

        //if (obj) {
        //    return obj.data.Descr;
        //}
        //return value;
    },
  

    closeWinDetail: function (item) {
        if (item == "no" || item == "ok") {
            HQ.isChange = false;
            App.winDetail.close();
        }
        else HQ.isChange = true;
    }

    ///////////////////////////////////
}