//// Declare //////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
var focus = 'header';
var recentRecord = null;
var lock = false;
var keygrdDetail = ['InvtID'];
var objUserDflt = null;
var objPO_Setup = null;
var lstobjSI_Tax = null;
var lstobjIN_Inventory = null;
var lstobjIN_UnitConversion = null;
var lstShipSiteID = null;
var strInvtID = "";
var strClassID = "";
var strStkUnit = "";
var paramInvtID = "";
var poNbr = "";
var objIN_ItemSite = null;
var cpnyID = "";
var handle = "";
var purUnit = "";
//////////////////////////////////////////////////////////////////
//// Store ///////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////

var loadDataHeader = function () {
    if (App.stoPO_Header.getCount() == 0) {
        App.stoPO_Header.insert(0, Ext.data.Record());     
    }
    if (App.stoPO_Header.getAt(0).data.ShipDistAddrID == "")
        App.stoPO_Header.getAt(0).data.ShipDistAddrID = App.cboBranchID.getValue();
    if (App.stoPO_Header.getAt(0).data.PONbr == "") {
        App.stoPO_Header.getAt(0).data.ShiptoType = "D";
        App.stoPO_Header.getAt(0).data.Status = "H";
        App.stoPO_Header.getAt(0).data.POType = "RO";
        App.stoPO_Header.getAt(0).data.PODate = HQ.bussinessDate;
        App.stoPO_Header.getAt(0).data.BlktExprDate = HQ.bussinessDate;
        App.stoPO_Header.getAt(0).data.BranchID = App.cboBranchID.getValue();
        App.stoPO_Header.getAt(0).data.PODate = HQ.bussinessDate;

    }
    App.frmMain.getForm().loadRecord(App.stoPO_Header.getAt(0));
   
    App.stoPO10100_pgDetail.reload();
    App.stoPO10100_pgLoadTaxTrans.reload();
    App.grdTaxTrans.getView().refresh();
    calcDet();
    
};
var loadDataDetail = function () {
    App.stoPO10100_pgDetail.insert(App.stoPO10100_pgDetail.getCount(), Ext.data.Record());
    calcDet();

};
var loadPO10100_pdOM_UserDefault = function () {
    if (App.stoPO10100_pdOM_UserDefault.getCount() > 0) {
        objUserDflt = App.stoPO10100_pdOM_UserDefault.getAt(0).data;

    }
    else {     
        objUserDflt = {POSite:'',};
    }
};
var loadPO10100_pdPO_Setup = function () {
    if (App.stoPO10100_pdPO_Setup.getCount() > 0) {
        objPO_Setup = App.stoPO10100_pdPO_Setup.getAt(0).data;
    }
    //else  objPO_Setup = null;
};
var loadPO10100_pdSI_Tax = function () {
    if (App.stoPO10100_pdSI_Tax.getCount() > 0) {
        lstobjSI_Tax = App.stoPO10100_pdSI_Tax;
    }
    //else lstobjSI_Tax = null;
};
var loadPO10100_pdIN_Inventory = function () {
    if (App.stoPO10100_pdIN_Inventory.getCount() > 0) {
        lstobjIN_Inventory = App.stoPO10100_pdIN_Inventory;
    }

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
            if (focus == 'Header') {
                MoveFirst(App.cboPONbr, 'PONbr');
            }
            else if (focus == 'grdDetail') {
                App.slmPO_Detail.select(0);
            }
            break;
        case "next":
            if (focus == 'Header') {
                MoveNext(App.cboPONbr, 'PONbr');
            }
            else if (focus == 'grdDetail') {
                App.slmPO_Detail.selectNext();
            }
            break;
        case "prev":
            if (focus == 'Header') {
                MovePrev(App.cboPONbr, 'PONbr');
            }
            else if (focus == 'grdDetail') {
                App.slmPO_Detail.selectPrevious();
            }
            break;
        case "last":
            if (focus == 'Header') {
                MoveLast(App.cboPONbr, 'PONbr');
            }
            else if (focus == 'grdDetail') {
                App.slmPO_Detail.select(App.storegrdDetail.data.items.length - 1);
            }
            break;
        case "save":
            if (HQ.isInsert || HQ.isUpdate) {
                save();
            }           
            break;
        case "delete":
            if (HQ.isDelete && App.cboStatus.value=='H') {
                if (focus == 'Header') {
                    if (App.cboPONbr.value) {
                        HQ.message.show(11, '', 'deleteHeader');
                    } else {
                        menuClick('new');
                    }
                } else if (focus == 'Detail') {                   
                    if (!App.cboPONbr.value) {
                        App.grdDetail.deleteSelected();
                        delTaxMutil();
                        calcDet();
                       
                    }
                    else {
                        HQ.message.show(11, '', 'deleteGrd');
                    }
                }
            }
            break;
        case "close":
            if (HQ.store.isChange(App.stoPO_Header) || HQ.store.isChange(App.stoPO10100_pgDetail)) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
        case "new":
            App.cboPONbr.setValue(null);
            break;
        case "refresh":
            App.stoPO_Header.reload();
            App.stoPO10100_pgDetail.reload();
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

var closeScreen = function (item) {
    if (item == "no") {
        this.parentAutoLoadControl.close()
    }
    else if (item == "yes") {
        Save();
    }
};
var grdPO_Detail_BeforeEdit = function (editor, e) {
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

        //_complete = false;
        //e.Cancel = true;
        //tabThongTinGiaoHang.IsSelected = true;
        //cboDistAddr.Focus();

        return false;
    }
    var det = e.record.data;
    purUnit = e.record.data.PurchUnit;
    keygrdDetail = e.record.idProperty.split(',');
    if (App.cboStatus.getValue() != "H") return false;
    var strkey = e.record.idProperty.split(',');
    if (e.record.phantom == false && strkey.indexOf(e.column.dataIndex) != -1)
        return false;


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
    if (objPO_Setup.EditablePOPrice && e.column.dataIndex == "UnitCost") {
        return false;
    }
    if (Ext.isEmpty(det.LineRef)) {
        e.record.set('LineRef', lastLineRef(App.stoPO10100_pgDetail));
    }
    if (e.field == 'PurchUnit') {
        var objIN_Inventory = objectFindByKey(lstobjIN_Inventory.data.items, "InvtID", det.InvtID).data
        strInvtID = objIN_Inventory.InvtID;
        strClassID = objIN_Inventory.ClassID;
        strStkUnit = objIN_Inventory.StkUnit;
        App.cboPurchUnit.getStore().reload();
    }


};
var grdPO_Detail_ValidateEdit = function (item, e) {
    if (keygrdDetail.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(App.grdDetail, e, keygrdDetail)) {
            HQ.message.show(1112, e.value, '');
            return false;
        }
    }
};
var grdPO_Detail_Edit = function (item, e) {
    var objDetail = e.record.data;
    var objIN_Inventory = objectFindByKey(lstobjIN_Inventory.data.items, "InvtID", objDetail.InvtID).data;
    objIN_Inventory = objIN_Inventory == null ? "" : objIN_Inventory;
    if (e.field == 'InvtID') {
        if (e.value) {
            var flat = App.stoPO10100_pgDetail.findBy(function (record, id) {
                if (!record.get('InvtID')) {
                    return true;
                }


                return false;
            });
            if (flat == -1) {
                App.stoPO10100_pgDetail.insert(App.stoPO10100_pgDetail.getCount(), Ext.data.Record());
            }     
        }
    }
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
            HQ.grid.showBusy(App.grdDetail, true);                     
            App.direct.PO10100ItemSitePrice(
                App.cboBranchID.getValue(), objDetail.InvtID, objDetail.SiteID,
               {
                   success: function (result) {
                       objIN_ItemSite = result;
                       UnitCost = result == null ? 0 : result.AvgCost;
                       UnitCost = Math.round((objDetail.UnitMultDiv == "D" ? (UnitCost / objDetail.CnvFact) : (UnitCost * objDetail.CnvFact)));
                       objDetail.UnitCost = UnitCost;
                       objDetail.ExtCost = UnitCost * objDetail.QtyOrd - objDetail.DiscAmt;
                       HQ.grid.showBusy(App.grdDetail, false);
                   },
                   failure: function (result) {
                       HQ.grid.showBusy(App.grdDetail, false);
                   }
                          
               });
        }
        else if (objPO_Setup.DfltLstUnitCost == "P") {
            HQ.grid.showBusy(App.grdDetail, true);
            App.direct.PO10100POPrice(
               App.cboBranchID.getValue(), objDetail.InvtID, objDetail.PurchUnit, Ext.Date.format(App.dtPODate.getValue(), 'Y-m-d'),
                {
                    success: function (result) {
                        UnitCost = result;
                        e.record.set("UnitCost", result);
                        e.record.set("ExtCost", result * objDetail.QtyOrd - objDetail.DiscAmt);
                        HQ.grid.showBusy(App.grdDetail, false);
                    },
                    failure: function (result) {
                        HQ.grid.showBusy(App.grdDetail, false);
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
var cboGInvtID_Change = function (item, newValue, oldValue) {
    var r = objectFindByKey(App.cboInvtID.getStore().data.items, "InvtID", newValue).data   
    var objdet = App.slmPO_Detail.getSelection()[0];
    if (r==undefined) {
        objdet.set('TranDesc', "");
        objdet.set('PurchUnit', "");
        //objdet.set('TranDesc', "");
    }
    else {
        var objIN_Inventory = objectFindByKey(lstobjIN_Inventory.data.items, "InvtID", r.InvtID).data
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

};
var cboGroupIDCompany_Change = function (item, newValue, oldValue) {
    var r = App.GroupIDCompany.valueModels[0];
    if (r == null) {
        App.SelectModelSYS_UserCompany.getSelection()[0].set('ListCpny', "");
        App.SelectModelSYS_UserCompany.getSelection()[0].set('Descr', "");
    }
    else {
        App.SelectModelSYS_UserCompany.getSelection()[0].set('ListCpny', r.data.ListCpny);
        App.SelectModelSYS_UserCompany.getSelection()[0].set('Descr', r.data.Descr);
    }

};
var cboBranchID_Change = function (item, newValue, oldValue) {

    App.cboBranchID.setValue(newValue);
    cpnyID = App.cboBranchID.getValue();
    App.BranchName.setValue("");
    App.cboDistAddr.getStore().load();
    App.cboPONbr.getStore().load();
    App.stoPO10100_pdOM_UserDefault.load();
    App.stoPO10100_pdPO_Setup.load();
    if (item.displayTplData.length > 0) {     
        App.BranchName.setValue(item.displayTplData[0].BranchName);                
    }

    App.cboDistAddr.setValue(App.cboBranchID.getValue());
    App.cboPONbr.setValue("");
    loadDataHeader();

};
var cboVendID_Change = function (item, newValue, oldValue) {
    App.cboTaxID.getStore().load();
    App.stoPO10100_pdAP_VenDorTaxes.reload();
}
var cboShiptoType_Change = function (item, newValue, oldValue) {
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
            App.cboShipVendID.setValue(App.cboVendID.getValue());
        }
        else if (item.displayTplData[0].Code == "O") {
            App.cboShipAddrID.enable(false);
        }

    }
};
var cboShipDistAddrID_Change = function (item, newValue, oldValue) {
   
    if (item.displayTplData.length > 0  && HQ.util.passNull(App.cboPONbr.getValue())!='' && App.cboDistAddr.disabled == false) {
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


};
var cboShipAddrID_Change = function (item, newValue, oldValue) {
    
    if (item.displayTplData.length > 0 && HQ.util.passNull(App.cboPONbr.getValue()) == "" && App.cboShipAddrID.disabled == false) {
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
   


};
var cboShipVendAddrID_Change = function (item, newValue, oldValue) {
   
    if (item.displayTplData.length > 0 && HQ.util.passNull(App.cboPONbr.getValue()) == "" && App.cboShipVendAddrID.disabled == false) {
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


};
var cboShipVendID_Change = function (item, newValue, oldValue) {
    App.cboShipVendAddrID.getStore().reload();


};
var cboShiptoID_Change = function (item, newValue, oldValue) {
    if (item.displayTplData.length > 0 && HQ.util.passNull(App.cboPONbr.getValue()) == "" && App.cboShiptoID.disabled == false) {
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


};
var cboShipCustID_Change = function (item, newValue, oldValue) {
    if (item.displayTplData.length > 0 && HQ.util.passNull(App.cboPONbr.getValue()) == "" && App.cboShipCustID.disabled == false) {
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


};
var cboShipSiteID_Change = function (item, newValue, oldValue) {
   
    if (item.displayTplData.length > 0 && HQ.util.passNull(App.cboPONbr.getValue()) == "" && App.cboShipSiteID.disabled == false) {
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
  


};
function disableComboAddress() {
    App.cboDistAddr.setValue("");
    App.cboShipSiteID.setValue("");
    App.cboShipCustID.setValue("");
    App.cboShiptoID.setValue("");
    App.cboShipVendID.setValue("");
    App.cboShipVendAddrID.setValue("");
    App.cboShipAddrID.setValue("");

    App.cboDistAddr.disable(true);
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
var cboPONbr_Change = function (item, newValue, oldValue) {
    App.stoPO_Header.clearData();
    App.stoPO10100_pgDetail.clearData();
    //if (item.valueModels.length != 0) {
    //    oldValue = null;
    App.stoPO_Header.reload();

    //}

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
            timeout: 180000,
            params: {
                lstDet: Ext.encode(App.stoPO10100_pgDetail.getRecordsValues()),
                lstHeader: Ext.encode(App.stoPO_Header.getRecordsValues())

            },
            success: function (msg, data) {
                if (this.result.PONbr != null) {
                    App.cboPONbr.getStore().reload();
                    App.cboPONbr.setValue(this.result.PONbr);
                }
                HQ.message.show(201405071);
                menuClick('refresh');
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
function deleteGrd(item) {
    if (item == 'yes') {
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
                        App.cboPONbr.getStore().reload();
                        App.cboPONbr.setValue(this.result.PONbr);
                    }
                    HQ.message.show(201405071);
                    menuClick('refresh');
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
};
var exportExcel = function () {
    App.frmMain.submit({
        waitMsg: HQ.common.getLang("Exporting"),      
        url: 'PO10100/ExportPOSuggest',
        timeout: 1800,
        params: {
            type: 'O',
            branchID: App.cboBranchID.getValue(),
            pODate: App.dtPODate.getValue(),
            vendID: App.cboVendID.getValue()

        },
        success: function (msg, data) {          
            var filePath = data.result.filePath;
            if (filePath) {
                window.location = "PO10100/Download?filePath=" + filePath;
            }
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });


}
//////////////////////////////////////////////////////////////////
//// Function ////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
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
    busyIndicator.IsBusy = false;
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
            tax.set('BranchID', cpnyID),
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
        newTax.data.BranchID = cpnyID;
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
function objectFindByKey(arrayitems, key, value) {
    for (var i = 0; i < arrayitems.length; i++) {
        if (arrayitems[i].data[key] === value) {
            return arrayitems[i];
        }
    }
    return "";
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
var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.close(this);
    }
};
/////////////////////////////////////////////////////////////////////////