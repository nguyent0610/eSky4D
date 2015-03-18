//// Declare //////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
var lock = false;
var keys = ['InvtID'];
var fieldsCheckRequire = ["InvtID", "PurchaseType", "SiteID", "PurchUnit"];
var fieldsLangCheckRequire = ["InvtID", "PurchaseType", "SiteID", "PurchUnit"];

var objUserDflt = null;
var objPO_Setup = null;
var lstobjSI_Tax = null;
var lstobjIN_UnitConversion = null;
var lstShipSiteID = null;
var strInvtID = "";
var strClassID = "";
var strStkUnit = "";
var paramInvtID = "";
var poNbr = "";
var objIN_ItemSite = null;
var handle = "";
var purUnit = "";
//////////////////////////////////////////////////////////////////
//// Store ///////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
var loadDataHeader = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
    //App.cboVendID.forceSelection=false;
    //App.cboVendAddrID.forceSelection = false;
   
    HQ.common.setForceSelection(App.frmMain, false, "cboBranchID,cboPONbr");
    HQ.isFirstLoad = true;
    
    HQ.isNew = false;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "PONbr");
        var record = sto.getAt(0);
        //gan du lieu mac dinh ban dau
        record.data.ShiptoType = "D";
        record.data.Status = "H";
        record.data.POType = "RO";
        record.data.PODate = HQ.bussinessDate;
        record.data.BlktExprDate = HQ.bussinessDate;
        record.data.BranchID = App.cboBranchID.getValue();
        record.data.PODate = HQ.bussinessDate;
        App.cboDistAddr.setValue(HQ.util.passNull(App.cboBranchID.value));// danh cho truong hop new gan lai App.cboDistAddr=ma nha phan phoi
        // lay du lieu ban dau cua address
        if(App.cboDistAddr.displayTplData.length > 0)
        {
            var item = App.cboDistAddr.displayTplData[0];
            record.data.ShipDistAddrID = App.cboBranchID.getValue();//neu co addressID = BranchID dang chon thi gan lai cho no de bindsource khong thay doi du lieu
            record.data.ShipName = item.Name;
            record.data.ShipAttn = item.Attn;
            record.data.ShipAddr1 = item.Addr1;
            record.data.ShipAddr2 = item.Addr2;
            record.data.ShipCity = item.City;
            record.data.ShipState = item.State;
            record.data.ShipZip = item.Zip;
            record.data.ShipCountry = item.Country;
            
            record.data.ShipPhone = item.Phone;
            record.data.ShipFax = item.Fax;
            
        }

        HQ.isNew = true;//record la new    
        HQ.common.setRequire(App.frmMain);  //to do cac o la require   
        if (App.cboBranchID.valueModels != undefined && App.cboBranchID.value != null && !App.cboPONbr.hasFocus)
            App.cboPONbr.focus(true);//focus ma khi tao moi
        else  if (App.cboBranchID.valueModels == undefined) App.cboBranchID.focus(true);//focus branch khi branch =''

        sto.commitChanges();
    }
    var record = sto.getAt(0);
    //record.data.MailCC = record.data.MailCC.replace(new RegExp(';', 'g'), ',');//cat dua ve du lieu dau , de set su thay doi
    //record.data.MailTo = record.data.MailTo.replace(new RegExp(';', 'g'), ',');//cat dua ve du lieu dau , de set su thay doi
    HQ.currRecord = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);
    App.stoPO10100_pgDetail.reload();
    App.stoPO10100_pgLoadTaxTrans.reload();
    HQ.isChange = false;
    HQ.common.changeData(HQ.isChange, 'PO10100');
   
    //App.stoPO10100_pgDetail.reload();
    //App.stoPO10100_pgLoadTaxTrans.reload();
    //App.grdTaxTrans.getView().refresh();
    //calcDet();
    if (App.stoPO10100_pdPO_Setup.data.length == 0) {// chua cai dat PO_Setup thong bao 
        HQ.isChange = false;
        HQ.common.changeData(HQ.isChange, 'PO10100');
        lockControl(true);
        App.cboBranchID.setReadOnly(false);
    }
};
var loadDataDetail = function (sto) {
    HQ.store.insertBlank(sto, keys);
    calcDet();
    frmChange();
    HQ.common.showBusy(false);
};
var loadPO10100_pdSI_Tax = function () {
    if (App.stoPO10100_pdSI_Tax.getCount() > 0) {
        lstobjSI_Tax = App.stoPO10100_pdSI_Tax;
    }
};
var loadPO10100_pdIN_Inventory = function () {
   
};
var loadPO10100_pdIN_UnitConversion = function () {
    if (App.stoPO10100_pdIN_UnitConversion.getCount() > 0) {
        lstobjIN_UnitConversion = App.stoPO10100_pdIN_UnitConversion;
    }
};
var loadstoPO10100_pgLoadTaxTrans = function () {
    App.stoPO10100_LoadTaxDoc.clearData();
    calcTaxTotal();
};
//////////////////////////////////////////////////////////////////
//// Event ///////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == 'header') {
                HQ.combo.first(App.cboPONbr, HQ.isChange);             
            }
            else if (HQ.focus == 'grdDetail') {
                HQ.grid.first(App.grdDetail);
            }
            break;
        case "next":
            if (HQ.focus == 'header') {
                HQ.combo.next(App.cboPONbr, HQ.isChange);
            }
            else if (HQ.focus == 'grdDetail') {
                HQ.grid.next(App.grdDetail);
            }
            break;
        case "prev":
            if (HQ.focus == 'header') {
                HQ.combo.prev(App.cboPONbr, HQ.isChange);
            }
            else if (HQ.focus == 'grdDetail') {
                HQ.grid.prev(App.grdDetail);
            }
            break;
        case "last":
            if (HQ.focus == 'header') {
                HQ.combo.last(App.cboPONbr, HQ.isChange);
            }
            else if (HQ.focus == 'grdDetail') {
                HQ.grid.last(App.grdDetail);
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                //checkRequire để kiếm tra các field yêu cầu có rỗng hay ko
                if (HQ.form.checkRequirePass(App.frmMain) && HQ.store.checkRequirePass(App.stoPO10100_pgDetail, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
            break;
        case "delete":
            if (HQ.isDelete && App.cboStatus.value == 'H') {
                if (HQ.isDelete) {
                    if (HQ.focus == 'header') {
                        HQ.message.show(11, '', 'deleteHeader');
                    } else if (HQ.focus == 'grdDetail') {
                        var rowindex = HQ.grid.indexSelect(App.grdDetail);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdDetail), ''], 'deleteRecordGrid', true)
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
                        App.cboPONbr.focus(true);
                        App.cboPONbr.setValue(null);
                    }
                } else if (HQ.focus == 'grdDetail') {
                    HQ.grid.insert(App.grdDetail, keys);
                }
            }
            break;
           
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                App.cboPONbr.getStore().load(function(){
                    App.stoPO_Header.reload(); 
                });                            
            }
            break;
           
            break;
        case "print":            
                App.dataForm.submit({
                    waitMsg: HQ.common.getLang("LoadReporting"),
                    method: 'POST',
                    url: 'PO10100/Report',
                    timeout: 180000,
                    params: {
                        POSCode: record.data.POSCode
                    },
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
//load lần đầu khi mở
var firstLoad = function () {
    
};
var frmChange = function (sender) {
    if (App.stoPO_Header.data.length > 0 && App.cboBranchID.getValue()!=null) {
        App.frmMain.getForm().updateRecord();
        HQ.isChange = HQ.store.isChange(App.stoPO_Header) == false ? HQ.store.isChange(App.stoPO10100_pgDetail) : true;
        HQ.common.changeData(HQ.isChange, 'PO10100');//co thay doi du lieu gan * tren tab title header
        //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
        App.cboBranchID.setReadOnly(HQ.isChange);
        App.cboPONbr.setReadOnly(HQ.isChange);
    }
    else {
        HQ.isChange = false;
        HQ.common.changeData(HQ.isChange, 'PO10100');
    }
    if (App.stoPO10100_pdPO_Setup.data.length == 0) {
        HQ.isChange = false;
        HQ.common.changeData(HQ.isChange, 'PO10100');
        lockControl(true);
        App.cboBranchID.setReadOnly(false);
    }

    if (App.cboStatus.getValue() != "H") App.btnImport.disable();
    if (objPO_Setup == null) {
        App.btnImport.disable();

    }
    else if (HQ.util.passNull(App.cboBranchID.getValue()) == "") {
        App.btnImport.disable();
    }
    else if (HQ.util.passNull(App.cboPONbr.getValue()) == "" && objPO_Setup.AutoRef == 0) {
        App.btnImport.disable();
    }
    else if (HQ.util.passNull(App.cboVendID.getValue()) == "") {
        App.btnImport.disable();

    }
    else if (App.dtPODate.getValue() == "") {
        App.btnImport.disable();
    }
    else if (App.cboDistAddr.getValue() == "") {
        App.btnImport.disable();
    }
    else App.btnImport.enable();
};
var grdPO_Detail_BeforeEdit = function (editor, e) {
    if (App.cboStatus.getValue() != "H") return false;
    if (objPO_Setup == null) {
        HQ.message.show(20404, 'PO_Setup', '');
        return false;

    }
    else if (HQ.util.passNull(App.cboPONbr.getValue()) == "" && objPO_Setup.AutoRef == 0) {
        HQ.message.show(15, App.cboPONbr.fieldLabel, '');
        return false;
    }
    else if (HQ.util.passNull(App.cboVendID.getValue()) == "") {
        HQ.message.show(41, '', '');
        return false;

    }
    else if (App.dtPODate.getValue() == "") {
        HQ.message.show(15, App.dtPODate.fieldLabel, '');
        return false;

    }
    else if (App.cboDistAddr.getValue() == "") {
        HQ.message.show(15, HQLangCode("DISTADDR"), '');
        return false;
    }
    var det = e.record.data;
    purUnit = e.record.data.PurchUnit;
  
    if (det.PurchaseType == "") {
        e.record.set("PurchaseType", "GI");
        e.record.set("SiteID", objUserDflt == undefined ? "" : objUserDflt.POSite);
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
    if (!objPO_Setup.EditablePOPrice && e.column.dataIndex == "UnitCost") {
        return false;
    }
    if (Ext.isEmpty(det.LineRef)) {
        e.record.set('LineRef', lastLineRef(App.stoPO10100_pgDetail));
    }
    if (e.field == 'PurchUnit') {
        var objIN_Inventory = HQ.store.findInStore(App.stoPO10100_pdIN_Inventory, ["InvtID"], [det.InvtID]);
        strInvtID = objIN_Inventory.InvtID;
        strClassID = objIN_Inventory.ClassID;
        strStkUnit = objIN_Inventory.StkUnit;
        App.cboPurchUnit.getStore().reload();
    }


};
var grdPO_Detail_ValidateEdit = function (item, e) {
    if (keys.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(App.grdDetail, e, keys)) {
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
            var objIN_Inventory = HQ.store.findInStore(App.stoPO10100_pdIN_Inventory, ["InvtID"], [r.InvtID]);
            strInvtID = objIN_Inventory.InvtID;
            strClassID = objIN_Inventory.ClassID;
            strStkUnit = objIN_Inventory.StkUnit;

            App.cboPurchUnit.getStore().reload();
            lstShipSiteID = App.cboShipSiteID.getStore();
            if (objdet.get("SiteID") == "") {
                if (objUserDflt != null) {
                    objdet.set('SiteID', objUserDflt.POSite);
                }
                else if (lstShipSiteID.getCount() > 0) {
                    objdet.set('SiteID', lstShipSiteID.getAt(0).data.SiteID);
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
};
var grdPO_Detail_Edit = function (item, e) {
    var objDetail = e.record.data;
    var objIN_Inventory = HQ.store.findInStore(App.stoPO10100_pdIN_Inventory, ["InvtID"], [objDetail.InvtID]);
    objIN_Inventory = objIN_Inventory == null ? "" : objIN_Inventory;
  
    if (e.field == "PurchUnit" || e.field == "InvtID") {
        var cnv = setUOM(objIN_Inventory.InvtID, objIN_Inventory.ClassID, objIN_Inventory.StkUnit, objDetail.PurchUnit);
        if (!Ext.isEmpty(cnv)) {
            var cnvFact = cnv.CnvFact;
            var unitMultDiv = cnv.MultDiv;
            objDetail.CnvFact = cnv.CnvFact;
            objDetail.UnitMultDiv = cnv.MultDiv;
            e.record.set('CnvFact', cnvFact);
            e.record.set('UnitMultDiv', unitMultDiv);
        } else {
            e.record.set('PurchUnit', "");
            return;
        }
        HQ.grid.checkInsertKey(App.grdDetail, e, keys);
    }
    if (e.field == "QtyOrd") {
        if (objDetail.PurchaseType == "FA") {
            if (objDetail.QtyOrd > 1) {

                HQ.message.show(58, '', '');
                return false;

            }
        }

        StkQty = Math.round((objDetail.UnitMultDiv == "D" ? (objDetail.QtyOrd / objDetail.CnvFact) : (objDetail.QtyOrd * objDetail.CnvFact)));
        e.record.set("ExtCost", objDetail.QtyOrd * objDetail.UnitCost - objDetail.DiscAmt);
        objDetail.POFee = StkQty * objIN_Inventory.POFee;

        e.record.set("ExtWeight", objDetail.QtyOrd * objDetail.UnitWeight);
        e.record.set("ExtVolume", objDetail.QtyOrd * objDetail.UnitVolume);
    }
    else if (e.field == "UnitWeight") {
        e.record.set("ExtWeight", objDetail.QtyOrd * objDetail.UnitWeight);

    }
    else if (e.field == "UnitCost") {
        e.record.set("ExtCost", objDetail.QtyOrd * objDetail.UnitCost - objDetail.DiscAmt);

    }
    else if (e.field == "UnitVolume") {
        e.record.set("ExtVolume", objDetail.QtyOrd * objDetail.UnitVolume);

    }
    else if (e.field == "DiscAmt") {
        e.record.set("ExtCost", objDetail.UnitCost * objDetail.QtyOrd - objDetail.DiscAmt);
        if (objDetail.QtyOrd != 0) {
            e.record.set("DiscPct", Math.round((objDetail.DiscAmt / (objDetail.UnitCost * objDetail.QtyOrd)) * 100, 2));
        }

    }
    else if (e.field == "DiscPct") {
        if (objDetail.ExtCost != 0) {
            e.record.set("DiscAmt", Math.round((objDetail.UnitCost * objDetail.QtyOrd * objDetail.DiscPct) / 100, 2));
        }
        e.record.set("ExtCost", objDetail.UnitCost * objDetail.QtyOrd - objDetail.DiscAmt);
    }
    else if (e.field == "PurchUnit" || e.field == "InvtID" || e.field == "SiteID") {
        if (objPO_Setup.DfltLstUnitCost == "A") {
            //HQ.grid.showBusy(App.grdDetail, true);                     
            App.direct.PO10100ItemSitePrice(
                App.cboBranchID.getValue(), objDetail.InvtID, objDetail.SiteID,
               {
                   success: function (result) {
                       objIN_ItemSite = result;
                       UnitCost = result == null ? 0 : result.AvgCost;
                       UnitCost = Math.round((objDetail.UnitMultDiv == "D" ? (UnitCost / objDetail.CnvFact) : (UnitCost * objDetail.CnvFact)));
                       objDetail.UnitCost = UnitCost;
                       objDetail.ExtCost = UnitCost * objDetail.QtyOrd - objDetail.DiscAmt;
                       //HQ.grid.showBusy(App.grdDetail, false);
                   },
                   failure: function (result) {
                       //HQ.grid.showBusy(App.grdDetail, false);
                   }
                          
               });
        }
        else if (objPO_Setup.DfltLstUnitCost == "P") {
            //HQ.grid.showBusy(App.grdDetail, true);
            App.direct.PO10100POPrice(
               App.cboBranchID.getValue(), objDetail.InvtID, objDetail.PurchUnit, Ext.Date.format(App.dtPODate.getValue(), 'Y-m-d'),
                {
                    success: function (result) {
                        UnitCost = result;
                        e.record.set("UnitCost", result);
                        e.record.set("ExtCost", result * objDetail.QtyOrd - objDetail.DiscAmt);
                        //HQ.grid.showBusy(App.grdDetail, false);
                    },
                    failure: function (result) {
                        //HQ.grid.showBusy(App.grdDetail, false);
                    }
                });

        }
        else if (objPO_Setup.DfltLstUnitCost == "I") {
            var UnitCost = objIN_Inventory.POPrice;
            UnitCost = Math.round((objDetail.UnitMultDiv == "D" ? (UnitCost / objDetail.CnvFact) : (UnitCost * objDetail.CnvFact)));
            e.record.set("UnitCost", UnitCost);
            e.record.set("ExtCost", UnitCost * objDetail.QtyOrd - objDetail.DiscAmt);


        }
    }
    if (objDetail.PurchaseType == "PR") {
        e.record.set("UnitCost", 0);
    }

    calcDet();
    delTax(e.rowIdx);
    calcTax(e.rowIdx);
    calcTaxTotal();
};
var grdPO_Detail_Deselect = function (item, e) {
    calcDet();
    delTax(e.rowIdx); 
    calcTaxTotal();
};
var grdPO_Detail_Reject = function (record) {
    if (record.data.tstamp == '') {
        var index = App.stoPO10100_pgDetail.indexOf(record);
        delTax(index);
        calcTaxTotal();
        App.grdDetail.getStore().remove(record, App.grdDetail);
        App.grdDetail.getView().focusRow(App.grdDetail.getStore().getCount() - 1);
        App.grdDetail.getSelectionModel().select(App.grdDetail.getStore().getCount() - 1);
        calcDet();
    } else {
        var index = App.stoPO10100_pgDetail.indexOf(record);       
        record.reject();
        delTax(index);
        calcTax(index);
        calcTaxTotal();
        calcDet();

    }
};
var cboGInvtID_Change = function (item, newValue, oldValue) {
    

};

//cac store co param la branchID thi load lai sau khi cboBranchID thay doi
var cboBranchID_Change = function (item, newValue, oldValue) {
    if (item.valueModels != null && App.cboBranchID.getValue() != null) {//truong hop co chon branchid
        App.txtBranchName.setValue(App.cboBranchID.valueModels[0].data.BranchName);
        _cpnyID = App.cboBranchID.valueModels[0].data.BranchID;
        HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
        App.stoPO10100_pdOM_UserDefault.load(function () {
            App.stoPO10100_pdPO_Setup.load(function () {
                App.cboDistAddr.getStore().load(function () {
                    App.cboShipSiteID.getStore().load(function () {
                        App.cboShipCustID.getStore().load(function () {
                            App.cboSiteID.getStore().load(function () {
                                objUserDflt = App.stoPO10100_pdOM_UserDefault.data.length > 0 ? App.stoPO10100_pdOM_UserDefault.getAt(0).data : { POSite: '', };;
                                if (App.stoPO10100_pdPO_Setup.data.length == 0) {
                                    if (item.hasFocus) {
                                        App.cboPONbr.setValue('');
                                        App.stoPO_Header.reload();
                                    }
                                    HQ.message.show(20404, 'PO_Setup', '');
                                    lockControl(true);
                                    App.cboBranchID.setReadOnly(false);
                                }
                                else {
                                    lockControl(false);
                                    objPO_Setup = App.stoPO10100_pdPO_Setup.getAt(0).data;
                                    if (objPO_Setup.AutoRef == 1) App.cboPONbr.forceSelection = true;
                                    else App.cboPONbr.forceSelection = false;
                                    App.cboPONbr.getStore().load(function () {
                                        App.cboPONbr.setValue('');
                                        App.stoPO_Header.reload();
                                        App.cboDistAddr.setValue(App.cboBranchID.getValue());
                                    });
                                }
                               
                            });
                        });
                    });
                });
            });
        });
    }
    else { //truong hop khong chon
        App.txtBranchName.setValue('');
        _cpnyID = '';
        App.stoPO10100_pdPO_Setup.load(function () {
            App.cboPONbr.setValue('');
            App.stoPO_Header.reload();          
        });
    }
    
};
var cboPONbr_Change = function (item, newValue, oldValue) {   
    if ((!HQ.isNew || item.valueModels != null) && !App.stoPO_Header.loading) {
        App.stoPO_Header.reload();
    }

};
var cboPONbr_Select = function (item) {
    if (item.valueModels != null && !App.stoPO_Header.loading) {
        App.stoPO_Header.reload();
    }
};

//cac store co param la VendID thi load lai sau khi VendID thay doi
var cboVendID_Change = function (item, newValue, oldValue) {
    App.stoPO10100_pdAP_VenDorTaxes.load(function () {
        App.cboTaxID.getStore().load(function () {
            App.cboVendAddrID.getStore().load(function () {
                if (App.cboVendID.hasFocus) {//co chon cboVendID thi gan cboVendAddrID= gia tri chon
                    App.cboVendAddrID.setValue(newValue);                    
                    if (item.valueModels != null) {
                        if (item.valueModels[0] != undefined) {
                            var objVendor = item.valueModels[0].data;
                            App.cboTerms.setValue(objVendor.Terms);
                        }
                        else  App.cboTerms.setValue('');
                    }
                }
                else {
                    App.cboVendAddrID.setValue(HQ.currRecord.data.VendAddrID);
                }
            });

        });
    });
}
var cboVendAddrID_Change = function (item, newValue, oldValue) {
    if (item.hasFocus || HQ.isNew || App.cboVendID.hasFocus) {// change khi chon combo hoac du lieu load la new khi chon vendID
        App.cboVendAddrID.forceSelection = true;
        if (item.valueModels != null) {
            if (item.valueModels[0] != undefined) {
                var objVendAddr = item.valueModels[0].data;
                App.txtVendName.setValue(objVendAddr.Name);
                App.txtVendAttn.setValue(objVendAddr.Attn);
                App.txtVendAddr1.setValue(objVendAddr.Addr1);
                App.txtVendAddr2.setValue(objVendAddr.Addr2);


                App.txtVendZip.setValue(objVendAddr.Zip);
                App.cboVendCountry.setValue(objVendAddr.Country);
                App.cboVendState.setValue(objVendAddr.State);
                App.cboVendCity.setValue(objVendAddr.City);
                App.txtVendPhone.setValue(objVendAddr.Phone);
                App.txtVendFax.setValue(objVendAddr.Fax);
                App.txtVendEmail.setValue("");
            }
            else {
                App.txtVendName.setValue("");
                App.txtVendAttn.setValue("");
                App.txtVendAddr1.setValue("");
                App.txtVendAddr2.setValue("");


                App.txtVendZip.setValue("");
                App.cboVendCountry.setValue("");
                App.cboVendState.setValue("");
                App.cboVendCity.setValue("");
                App.txtVendPhone.setValue("");
                App.txtVendFax.setValue("");
                App.txtVendEmail.setValue("");
            }
        }
    } 
}
var cboShiptoType_Change = function (item, newValue, oldValue) {
    if (App.cboShiptoType.hasFocus||App.cboPONbr.getValue()==null) {//co chon combo
        disableComboAddress();
        if (item.displayTplData.length > 0) {
            if (item.displayTplData[0].Code == "D") {
                App.cboDistAddr.enable(false);
                App.cboDistAddr.setValue(App.cboBranchID.getValue());
            }
            else if (item.displayTplData[0].Code == "S") {
                App.cboShipSiteID.enable(false);
            }
            else if (item.displayTplData[0].Code == "C") {
                App.cboShipCustID.enable(false);
                App.cboShiptoID.enable(false);

            }
            else if (item.displayTplData[0].Code == "V") {

                App.cboShipVendID.enable(false);
                App.cboShipVendAddrID.enable(false);
                App.cboShipVendID.setValue(App.cboPONbr.getValue());
            }
            else if (item.displayTplData[0].Code == "O") {
                App.cboShipAddrID.enable(false);
            }
        }
    }
};
var cboDistAddr_Change = function (item, newValue, oldValue) {
    if (App.cboDistAddr.hasFocus) {//co chon combo
        if (item.displayTplData.length > 0 && App.cboDistAddr.disabled == false) {// du lieu change khi co chon combo
            App.txtShipName.setValue(item.displayTplData[0].Name);
            App.txtShipAttn.setValue(item.displayTplData[0].Attn);
            App.txtShipAddr1.setValue(item.displayTplData[0].Addr1);
            App.txtShipAddr2.setValue(item.displayTplData[0].Addr2);
            App.cboShipCity.setValue(item.displayTplData[0].City);
            App.cboShipState.setValue(item.displayTplData[0].State);
            App.txtShipZip.setValue(item.displayTplData[0].Zip);
            App.cboShipCountry.setValue(item.displayTplData[0].Country);
            App.cboShipVia.setValue("");
            App.txtShipPhone.setValue(item.displayTplData[0].Phone);
            App.txtShipFax.setValue(item.displayTplData[0].Fax);
            App.txtShipEmail.setValue("");
        }
        else reSetValueAddress();
    }   
};
var cboShipAddrID_Change = function (item, newValue, oldValue) {
    if (App.cboShipAddrID.hasFocus) {
        if (item.displayTplData.length > 0 && App.cboShipAddrID.disabled == false) {
            App.txtShipName.setValue(item.displayTplData[0].Name);
            App.txtShipAttn.setValue(item.displayTplData[0].Attn);
            App.txtShipAddr1.setValue(item.displayTplData[0].Addr1);
            App.txtShipAddr2.setValue(item.displayTplData[0].Addr2);
            App.cboShipCity.setValue(item.displayTplData[0].City);
            App.cboShipState.setValue(item.displayTplData[0].State);
            App.txtShipZip.setValue(item.displayTplData[0].Zip);
            App.cboShipCountry.setValue(item.displayTplData[0].Country);
            App.cboShipVia.setValue("");
            App.txtShipPhone.setValue(item.displayTplData[0].Phone);
            App.txtShipFax.setValue(item.displayTplData[0].Fax);
            App.txtShipEmail.setValue("");
        } else reSetValueAddress();
    }
};
var cboShipVendAddrID_Change = function (item, newValue, oldValue) {
    if (App.cboShipVendAddrID.disabled == false && item.hasFocus) {
        if (item.displayTplData.length > 0 ) {
            App.txtShipName.setValue(item.displayTplData[0].Name);
            App.txtShipAttn.setValue(item.displayTplData[0].Attn);
            App.txtShipAddr1.setValue(item.displayTplData[0].Addr1);
            App.txtShipAddr2.setValue(item.displayTplData[0].Addr2);
            App.cboShipCity.setValue(item.displayTplData[0].City);
            App.cboShipState.setValue(item.displayTplData[0].State);
            App.txtShipZip.setValue(item.displayTplData[0].Zip);
            App.cboShipCountry.setValue(item.displayTplData[0].Country);
            App.cboShipVia.setValue("");
            App.txtShipPhone.setValue(item.displayTplData[0].Phone);
            App.txtShipFax.setValue(item.displayTplData[0].Fax);
            App.txtShipEmail.setValue("");
        } else reSetValueAddress();
    }
};
var cboShipVendID_Change = function (item, newValue, oldValue) {
    if (item.hasFocus) {
        App.cboShipVendAddrID.getStore().reload();
    }
};
var cboShipCustID_Change = function (item, newValue, oldValue) {
    if (item.hasFocus) {
        if (item.valueModels != null && App.cboShipCustID.getValue() != null && App.cboShipCustID.disabled == false) {//truong hop co chon cboShipCustID
            App.cboShiptoID.load(function () {
                //App.cboShiptoID.setValue(ShiptoID)
                App.txtShipName.setValue(item.displayTplData[0].Name);
                App.txtShipAttn.setValue(item.displayTplData[0].Attn);
                App.txtShipAddr1.setValue(item.displayTplData[0].Addr1);
                App.txtShipAddr2.setValue(item.displayTplData[0].Addr2);
                App.cboShipCity.setValue(item.displayTplData[0].City);
                App.cboShipState.setValue(item.displayTplData[0].State);
                App.txtShipZip.setValue(item.displayTplData[0].Zip);
                App.cboShipCountry.setValue(item.displayTplData[0].Country);
                App.cboShipVia.setValue("");
                App.txtShipPhone.setValue(item.displayTplData[0].Phone);
                App.txtShipFax.setValue(item.displayTplData[0].Fax);
                App.txtShipEmail.setValue("");
            });
        } else reSetValueAddress();
    }

};
var cboShiptoID_Change = function (item, newValue, oldValue) {
    if (item.hasFocus) {
        if (item.displayTplData.length > 0 && App.cboShiptoID.disabled == false) {
            App.txtShipName.setValue(item.displayTplData[0].Name);
            App.txtShipAttn.setValue(item.displayTplData[0].Attn);
            App.txtShipAddr1.setValue(item.displayTplData[0].Addr1);
            App.txtShipAddr2.setValue(item.displayTplData[0].Addr2);
            App.cboShipCity.setValue(item.displayTplData[0].City);
            App.cboShipState.setValue(item.displayTplData[0].State);
            App.txtShipZip.setValue(item.displayTplData[0].Zip);
            App.cboShipCountry.setValue(item.displayTplData[0].Country);
            App.cboShipVia.setValue("");
            App.txtShipPhone.setValue(item.displayTplData[0].Phone);
            App.txtShipFax.setValue(item.displayTplData[0].Fax);
            App.txtShipEmail.setValue("");
        } else reSetValueAddress();
    }

};
var cboShipSiteID_Change = function (item, newValue, oldValue) {
    if (item.hasFocus) {
        if (item.displayTplData.length > 0 && App.cboShipSiteID.disabled == false) {
            App.txtShipName.setValue(item.displayTplData[0].Name);
            App.txtShipAttn.setValue(item.displayTplData[0].Attn);
            App.txtShipAddr1.setValue(item.displayTplData[0].Addr1);
            App.txtShipAddr2.setValue(item.displayTplData[0].Addr2);
            App.cboShipCity.setValue(item.displayTplData[0].City);
            App.cboShipState.setValue(item.displayTplData[0].State);
            App.txtShipZip.setValue(item.displayTplData[0].Zip);
            App.cboShipCountry.setValue(item.displayTplData[0].Country);
            App.cboShipVia.setValue("");
            App.txtShipPhone.setValue(item.displayTplData[0].Phone);
            App.txtShipFax.setValue(item.displayTplData[0].Fax);
            App.txtShipEmail.setValue("");
        } else reSetValueAddress();
    }
};
var cboStatus_Change = function (item, newValue, oldValue) {
    App.cboHandle.getStore().reload();
    if (newValue == 'H' && HQ.isInsert && HQ.isUpdate) {
        HQ.common.lockItem(App.frmMain, false);       
    }
    else HQ.common.lockItem(App.frmMain, true);
    {
        App.cboBranchID.setReadOnly(false);
        App.cboHandle.setReadOnly(false);
        App.cboPONbr.setReadOnly(false);
    }
};
///////DataProcess///
function save() {
    App.frmMain.getForm().updateRecord();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang('SavingData'),
            method: 'POST',
            url: 'PO10100/Save',
            timeout: 1800000,
            params: {
                lstDet: Ext.encode(App.stoPO10100_pgDetail.getRecordsValues()),
                lstHeader: Ext.encode(App.stoPO_Header.getRecordsValues())

            },
            success: function (msg, data) {              
                HQ.message.show(201405071, '', '');
                var PONbr = data.result.PONbr;
                App.cboPONbr.getStore().load(function () {
                    App.cboPONbr.setValue(PONbr);
                    App.stoPO_Header.reload();
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
                url: 'PO10100/DeleteHeader',
                timeout: 180000,
                params: {
                    lstHeader: Ext.encode(App.stoPO_Header.getRecordsValues())

                },
                success: function (msg, data) {
                    App.cboPONbr.setValue("");
                    HQ.message.show(201405071);
                    menuClick('refresh');
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
};
var deleteRecordGrid = function (item) {
    if (item == "yes") {
        if (item == 'yes') {
            if (App.slmPO_Detail.selected.items[0].data.tstamp != "") {
                App.grdDetail.deleteSelected();
                delTaxMutil();
                calcDet();
                App.frmMain.getForm().updateRecord();
                if (App.frmMain.isValid()) {
                    App.frmMain.submit({
                        waitMsg: HQ.common.getLang('DeletingData'),
                        method: 'POST',
                        url: 'PO10100/DeleteGrd',
                        timeout: 180000,
                        params: {
                            lstDel: HQ.store.getData(App.stoPO10100_pgDetail),
                            lstDet: Ext.encode(App.stoPO10100_pgDetail.getRecordsValues()),
                            lstHeader: Ext.encode(App.stoPO_Header.getRecordsValues())

                        },
                        success: function (msg, data) {
                            if (this.result.PONbr != null) {
                                var PONbr = this.result.PONbr;
                                App.cboPONbr.getStore().load(function () {
                                    App.cboPONbr.setValue(PONbr);
                                    HQ.message.show(201405071);
                                    App.stoPO_Header.reload();
                                });
                            }


                        },
                        failure: function (msg, data) {
                            HQ.message.process(msg, data, true);
                            var poNbr = App.cboPONbr.value;
                            App.cboPONbr.getStore().reload();
                            App.cboPONbr.setValue(poNbr);
                            menuClick('refresh');
                        }
                    });
                }
            }
            else {
                App.grdDetail.deleteSelected();
                delTaxMutil();
                calcDet();
            }
        }
    }
};
var btnImport_Click = function (c, e) {
    
    var fileName = c.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (ext == "xls" || ext == "xlsx") {
        App.frmMain.submit({
            waitMsg: HQ.waitMsg,
            clientValidation: false,
            method: 'POST',
            url: 'PO10100/Import',
            timeout: 1000000,
            params: {
                lstDet: Ext.encode(App.stoPO10100_pgDetail.getRecordsValues())
            },
            success: function (msg, data) {
                if (this.result.data.lstTrans != undefined) {                   
                    HQ.store.insertBlank(App.stoPO10100_pgDetail, keys);                  
                        this.result.data.lstTrans.forEach(function (item) {
                            insertItemGrid(App.grdDetail, item);
                            //grdPO_Detail_ValidateEdit(App.grdDetail, App.stoPO10100_pgDetail.data.items[0]);
                            //HQ.store.insertRecord(App.stoPO10100_pgDetail, 'InvtID', newTrans, true);
                        });

                     
                        //HQ.store.insertRecord(App.stoPO10100_pgDetail, "InvtID", Ext.create('App.mdlPO10100_pgDetailModel'), true);
                        //calculate();
                        //checkTransAdd();
                   
                    if (!Ext.isEmpty(this.result.data.message)) {
                        HQ.message.show('2013103001', [this.result.data.message], '', true);
                    }
                } else {
                    HQ.message.process(msg, data, true);
                }

            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
                App.btnImport.reset();
            }
        });
    } else {
        HQ.message.show('2014070701', [ext], '', true);
        App.btnImport.reset();
    }
};
var btnExport_Click = function () {
    if (Ext.isEmpty(App.cboBranchID.getValue())) {
        HQ.message.show('1000', [HQ.common.getLang('BranchID')], '', true);
        return;
    }
    else if (Ext.isEmpty(App.cboVendID.getValue())) {
        HQ.message.show('1000', [HQ.common.getLang('VendID')], '', true);
        return;
    }
     
    App.frmMain.submit({
        //waitMsg: HQ.common.getLang("Exporting"),
        url: 'PO10100/Export',
        type: 'POST',
        timeout: 1000000,
        clientValidation: false,
        params: {
            type: 'O',
        },
        success: function (msg, data) {
            alert('sus');
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });
};
var insertItemGrid = function (grd, item) {
    var objDetail = App.stoPO10100_pgDetail.data.items[App.stoPO10100_pgDetail.getCount() - 1];
   
    var valueTax = '';
    App.cboTaxID.getStore().data.each(function (det) {
        valueTax += det.data.taxid + ',';

    });
    valueTax = valueTax.length > 0 ? valueTax.substring(0, valueTax.length - 1) : '';

    objDetail.set('PurchaseType', 'GI');
    objDetail.set("VouchStage", 'N');
    objDetail.set("RcptStage", 'N');   
    objDetail.set("TaxID", valueTax);
    objDetail.set("ReqdDate", HQ.bussinessDate);
    objDetail.set("PromDate", HQ.bussinessDate);


    objDetail.set('InvtID', item.InvtID);
    objDetail.set('QtyOrd', item.QtyOrd);
    objDetail.set('PurchUnit', item.PurchUnit);
    objDetail.set('CnvFact', item.CnvFact);
    objDetail.set('UnitMultDiv', item.UnitMultDiv);
    objDetail.set('TranDesc', item.TranDesc);
    objDetail.set('SiteID', item.SiteID);

    objDetail.set('UnitWeight', item.UnitWeight);
    objDetail.set('UnitVolume', item.UnitVolume);
    objDetail.set('ExtVolume', item.ExtVolume);
    objDetail.set('ExtWeight', item.ExtWeight);
    objDetail.set("TaxCat", item.TaxCat);

    objDetail.set('UnitCost', item.UnitCost);
    objDetail.set('ExtCost', item.ExtCost);
    objDetail.set('LineRef', lastLineRef(App.stoPO10100_pgDetail));

    
    calcDet();
    delTax(App.stoPO10100_pgDetail.getCount() - 1);
    calcTax(App.stoPO10100_pgDetail.getCount() - 1);
    calcTaxTotal();
    //HQ.grid.insert(App.grdDetail, keys);
    HQ.store.insertBlank(App.stoPO10100_pgDetail, keys);
};
//////////////////////////////////////////////////////////////////
//// Function ////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////

//HAM TINH THUNG LE
var calCTNPCS = function () {

    var CTN = 0;
    var PCS = 0;
    var CnvFact = 1;
    for (var j = 0; j < App.stoPO10100_pgDetail.data.length; j++) {
        var det = App.stoPO10100_pgDetail.data.items[j];
        CnvFact = oM_GetCnvFactToUnit(det.data.InvtID, det.data.PurchUnit);
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
   
};
//Cal tax
function calcDet() {
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

    for (var j = 0; j < App.stoPO10100_pgDetail.data.length; j++) {
        var det = App.stoPO10100_pgDetail.data.items[j];
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
        CnvFact = oM_GetCnvFactToUnit(det.data.InvtID, det.data.PurchUnit);
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
    App.RcptTotAmt.setValue(Math.round(extCost, 0) + Math.round(poFee, 0));
    App.POAmt.setValue(Math.round(taxAmt00 + taxAmt01 + taxAmt02 + taxAmt03, 0) + Math.round(extCost, 0) + Math.round(poFee, 0));
    //App.CuryLineAmt.setValue(Math.round(curyLineAmt, 0));
    App.DISCOUNT.setValue(Math.round(discount, 0));

}
function delTaxMutil() {

    for (var i = App.stoPO10100_pgLoadTaxTrans.data.length - 1; i >= 0; i--) {
        var data = HQ.store.findInStore(App.stoPO10100_pgDetail, ['LineRef'], [App.stoPO10100_pgLoadTaxTrans.data.items[i].data.LineRef])
        if (!data) App.stoPO10100_pgLoadTaxTrans.data.removeAt(i);
    }
    calcTaxTotal();
};
function delTax(index) {
    //if (App.cboStatus != "H" ) return false;
    var lineRef = App.stoPO10100_pgDetail.data.items[index].data.LineRef;

    for (var j = App.stoPO10100_pgLoadTaxTrans.data.length - 1; j >= 0; j--) {
        if (App.stoPO10100_pgLoadTaxTrans.data.items[j].data.LineRef == lineRef)
            App.stoPO10100_pgLoadTaxTrans.data.removeAt(j);
    }
    clearTax(index);
    calcTaxTotal();
    calcDet();
    return true;

}
function clearTax(index) {
    App.stoPO10100_pgDetail.data.items[index].set('TaxID00', '');
    App.stoPO10100_pgDetail.data.items[index].set('TaxAmt00', 0);
    App.stoPO10100_pgDetail.data.items[index].set('TxblAmt00', 0);

    App.stoPO10100_pgDetail.data.items[index].set('TaxID01', '');
    App.stoPO10100_pgDetail.data.items[index].set('TaxAmt01', 0);
    App.stoPO10100_pgDetail.data.items[index].set('TxblAmt01', 0);

    App.stoPO10100_pgDetail.data.items[index].set('TaxID02', '');
    App.stoPO10100_pgDetail.data.items[index].set('TaxAmt02', 0);
    App.stoPO10100_pgDetail.data.items[index].set('TxblAmt02', 0);

    App.stoPO10100_pgDetail.data.items[index].set('TaxID03', '');
    App.stoPO10100_pgDetail.data.items[index].set('TaxAmt03', 0);
    App.stoPO10100_pgDetail.data.items[index].set('TxblAmt03', 0);
}
function calcTax(index) {

    var det = App.stoPO10100_pgDetail.data.items[index].data;
    var record = App.stoPO10100_pgDetail.data.items[index];
    if (index < 0) return true;


    var dt = [];
    if (det.TaxID == "*") {
        for (var j = 0; j < App.stoPO10100_pdAP_VenDorTaxes.data.length; j++) {
            var item = App.stoPO10100_pdAP_VenDorTaxes.data.items[j];
            dt.push(item.data);
        };
    }
    else {
        var strTax = det.TaxID.split(',');
        if (strTax.length > 0) {
            for (var k = 0; k < strTax.length; k++) {
                for (var j = 0; j < App.stoPO10100_pdAP_VenDorTaxes.data.length; j++) {
                    if (strTax[k] == App.stoPO10100_pdAP_VenDorTaxes.data.items[j].data.taxid) {
                        dt.push(App.stoPO10100_pdAP_VenDorTaxes.data.items[j].data);
                        break;
                    }
                }
            }
        }
        else {
            if (Ext.isEmpty(det.TaxID) || Ext.isEmpty(det.TaxCat))
                App.stoPO10100_pgDetail.data.items[i].set('TxblAmt00', det.ExtCost);
            return false;
        }
    }

    var taxCat = det.TaxCat;
    var prcTaxInclRate = 0, totPrcTaxInclAmt = 0, txblAmtL1 = 0, txblAmtAddL2 = 0;
    for (var j = 0; j < dt.length; j++) {
        var objTax = HQ.store.findInStore(App.stoPO10100_pdAP_VenDorTaxes, ['taxid'], [dt[j].taxid]);
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
        var objTax = HQ.store.findInStore(App.stoPO10100_pdAP_VenDorTaxes, ['taxid'], [dt[j].taxid]);
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
                    taxAmtL1 = Math.round(txblAmtL1 * objTax.TaxRate / 100, 0);

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

                            if (totPrcTaxInclAmt + taxAmtL1 + txblAmtL1 != _lstOrdDet[i].ExtCost)
                                taxAmtL1 = Math.round(det.ExtCost - (totPrcTaxInclAmt + txblAmtL1), 0);

                        }
                        else
                            totPrcTaxInclAmt += totPrcTaxInclAmt + taxAmtL1;
                    }

                    insertUpdateTax(taxID, lineRef, taxRate, taxAmtL1, txblAmtL1, 1);

                }
            }
        }
    }

    for (var j = 0; j < dt.Count; j++) {
        var taxID = "", lineRef = "";
        var taxRate = 0, txblAmtL2 = 0, taxAmtL2 = 0;
        var objTax = HQ.store.findInStore(App.stoPO10100_pdAP_VenDorTaxes, ['taxid'], [dt[j].taxid]);
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
                    taxAmtL2 = Math.round(txblAmtAddL2 * objTax.TaxRate / 100, 0);
                    insertUpdateTax(taxID, lineRef, taxRate, taxAmtL2, txblAmtL2, 2);
                }
            }
        }
    }
    updateTax(index);
    calcDet();
    return true;
}
function insertUpdateTax(taxID, lineRef, taxRate, taxAmt, txblAmt, taxLevel) {
    var flat = false;
    for (var i = 0; i < App.stoPO10100_pgLoadTaxTrans.data.length; i++) {
        if (App.stoPO10100_pgLoadTaxTrans.data.items[i].data.taxid == taxID && App.stoPO10100_pgLoadTaxTrans.data.items[i].data.LineRef == lineRef) {
            var tax = App.stoPO10100_pdAP_VenDorTaxes.data.items[i];
            tax.set('BranchID', _cpnyID),
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
        var newTax = Ext.create('App.ModelPO10100_pgLoadTaxTrans_Result');
        newTax.data.BranchID = _cpnyID;
        newTax.data.TaxID = taxID;
        newTax.data.LineRef = lineRef;
        newTax.data.TaxRate = taxRate;
        newTax.data.TaxLevel = taxLevel.toString();
        newTax.data.TaxAmt = taxAmt;
        newTax.data.TxblAmt = txblAmt;

        App.stoPO10100_pgLoadTaxTrans.data.add(newTax);
    }
    App.stoPO10100_pgLoadTaxTrans.sort('LineRef', "ASC");
    calcDet();

}
function updateTax(index) {

    if (index < 0) return;
    var j = 0;
    var det = App.stoPO10100_pgDetail.data.items[index].data;
    var record = App.stoPO10100_pgDetail.data.items[index];
    for (var i = 0; i < App.stoPO10100_pgLoadTaxTrans.data.length; i++) {
        var item = App.stoPO10100_pgLoadTaxTrans.data.items[i];
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

}
function calcTaxTotal() {
    App.stoPO10100_LoadTaxDoc.clearData();
    var flat = false;
    for (var i = 0; i < App.stoPO10100_pgLoadTaxTrans.data.length; i++) {
        var tax = App.stoPO10100_pgLoadTaxTrans.data.items[i];
        flat = true;
        for (var j = 0; j < App.stoPO10100_LoadTaxDoc.data.length; j++) {
            var taxDoc = App.stoPO10100_LoadTaxDoc.data.items[j];
            if (tax.data.PONbr == taxDoc.data.PONbr && tax.data.TaxID == taxDoc.data.TaxID) {
                taxDoc.data.TxblAmt += tax.data.TxblAmt;
                taxDoc.data.TaxAmt += tax.data.TaxAmt;
                flat = false;
                taxDoc.commit();
                break;
            }
        };
        if (flat) {
            var newTaxDoc = Ext.create('App.mdlPO10100_pgLoadTaxTransDoc');
            newTaxDoc.data.BranchID = tax.data.BranchID;
            newTaxDoc.data.PONbr = tax.data.PONbr;
            newTaxDoc.data.TaxID = tax.data.TaxID;
            newTaxDoc.data.TaxAmt = tax.data.TaxAmt;
            newTaxDoc.data.TaxRate = tax.data.TaxRate;
            newTaxDoc.data.TxblAmt = tax.data.TxblAmt;

            App.stoPO10100_LoadTaxDoc.data.add(newTaxDoc);
            // newTaxDoc.commit();
        }

    };
    App.grdTaxTrans.getView().refresh(false);
    App.grdTaxDoc.getView().refresh(false);
   
}

function lastLineRef(store) {
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
}
function setUOM(invtID, classID, stkUnit, fromUnit) {
    if (!Ext.isEmpty(fromUnit)) {
        var data = HQ.store.findInStore(App.stoPO10100_pdIN_UnitConversion, ['UnitType', 'ClassID', 'InvtID', 'FromUnit', 'ToUnit'], ["3", "*", invtID, fromUnit, stkUnit]);
        if (!Ext.isEmpty(data)) {
            return data;
        }

        data = HQ.store.findInStore(App.stoPO10100_pdIN_UnitConversion, ['UnitType', 'ClassID', 'InvtID', 'FromUnit', 'ToUnit'], ["2", classID, "*", fromUnit, stkUnit]);
        if (!Ext.isEmpty(data)) {
            return data;
        }

        data = HQ.store.findInStore(App.stoPO10100_pdIN_UnitConversion, ['UnitType', 'ClassID', 'InvtID', 'FromUnit', 'ToUnit'], ["1", "*", "*", fromUnit, stkUnit]);
        if (!Ext.isEmpty(data)) {
            return data;
        }
        HQ.message.show(25, invtID, '');
        return null;
    }
    return null;
};
function oM_GetCnvFactToUnit(invtID, unitDesc) {
    var cnvFact = 1;
    var data;
    App.stoPO10100_pdIN_UnitConversion.data.each(function (item) {
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
};

//// Other Functions ////////////////////////////////////////////////////
function lockControl(isLock) {
    HQ.common.lockItem(App.frmMain, isLock);
}
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        App.cboPONbr.getStore().load(function () {
            App.stoPO_Header.reload();
        });
    }
};
function disableComboAddress() {
    App.cboDistAddr.setValue("");
    App.cboShipSiteID.setValue("");
    App.cboShipCustID.setValue("");
    App.cboShiptoID.setValue("");
    App.cboShipVendID.setValue("");
    App.cboShipVendAddrID.setValue("");
    App.cboShipAddrID.setValue("");

    App.cboDistAddr.disable(false);
    App.cboShipSiteID.disable(true);
    App.cboShipCustID.disable(true);
    App.cboShiptoID.disable(true);
    App.cboShipVendID.disable(true);
    App.cboShipVendAddrID.disable(true);
    App.cboShipAddrID.disable(true);
}
function reSetValueAddress() {
    App.txtShipName.setValue("");
    App.txtShipAttn.setValue("");
    App.txtShipAddr1.setValue("");
    App.txtShipAddr2.setValue("");
    App.cboShipCity.setValue("");
    App.cboShipState.setValue("");
    App.txtShipZip.setValue("");
    App.cboShipCountry.setValue("");
    App.cboShipVia.setValue("");
    App.txtShipPhone.setValue("");
    App.txtShipFax.setValue("");
    App.txtShipEmail.setValue("");
}
var renderPurchaseType = function (value) {
    var obj = App.PurchaseType.getStore().findRecord("Code", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};
var renderVouchStage = function (value) {
    var obj = App.VouchStage.getStore().findRecord("Code", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};
var renderRcptStage = function (value) {
    var obj = App.RcptStage.getStore().findRecord("Code", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};
var renderTaxID = function (value) {
    //App.stoPO10100_pgDetail.data.each(function (det) {
    //    value.split(',')

    //});

    //if (obj) {
    //    return obj.data.Descr;
    //}
    //return value;
};
///////////////////////////////////