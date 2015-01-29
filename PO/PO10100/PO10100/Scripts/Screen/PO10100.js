//// Declare ///////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////

var focus = 'header';
var recentRecord = null;
var lock = false;
var strKeyGridDetail = '';
var objUserDflt = null;
var objPO_Setup = null;
var lstobjSI_Tax = null;
var lstobjIN_Inventory = null;
var lstShipSiteID = null;
var strInvtID = "";
var strClassID = "";
var strStkUnit = "";
var paramInvtID = "";
var poNbr = "";
var objIN_ItemSite = null;
var cpnyID="";
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////


//// Store ///////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
var loadDataHeader = function () {
    if (App.storePO_Header.getCount() == 0) {
        App.storePO_Header.insert(0, Ext.data.Record());
        App.storePO_Header.getAt(0).data.ShiptoType = "D";
        App.storePO_Header.getAt(0).data.PODate = bussinessDate;
    }
    if (App.storePO_Header.getAt(0).data.ShipDistAddrID == "") 
        App.storePO_Header.getAt(0).data.ShipDistAddrID = App.cboBranchID.getValue();
    if (App.storePO_Header.getAt(0).data.PONbr == "") {
        App.storePO_Header.getAt(0).data.ShiptoType = "D";
        App.storePO_Header.getAt(0).data.Status = "H";
        App.storePO_Header.getAt(0).data.POType = "RO";
        App.storePO_Header.getAt(0).data.PODate = bussinessDate;
        App.storePO_Header.getAt(0).data.BlktExprDate = bussinessDate;
        
    }
    App.dataForm.getForm().loadRecord(App.storePO_Header.getAt(0));
    App.storePO_DetailLoad.reload();
    App.storePO10100_LoadTaxTrans.reload();
    App.grdTaxTrans.getView().refresh();
    calcDet();
};
var loadDataDetail = function () {
    App.storePO_DetailLoad.insert(App.storePO_DetailLoad.getCount(), Ext.data.Record());   
    calcDet();
   
};

var loadPO10100OM_UserDefault = function () {
    if (App.storePO10100OM_UserDefault.getCount() > 0) {
        objUserDflt = App.storePO10100OM_UserDefault.getAt(0).data;
    }
    //else objUserDflt = null;
};
var loadPO10100PO_Setup = function () {
    if (App.storePO10100PO_Setup.getCount() > 0) {
        objPO_Setup = App.storePO10100PO_Setup.getAt(0).data;
    }
    //else  objPO_Setup = null;
};
var loadPO10100SI_Tax = function () {
    if (App.storePO10100SI_Tax.getCount() > 0) {
        lstobjSI_Tax = App.storePO10100SI_Tax;
    }
    //else lstobjSI_Tax = null;
};
var loadPO10100IN_Inventory = function () {
    if (App.storePO10100IN_Inventory.getCount() > 0) {
        lstobjIN_Inventory = App.storePO10100IN_Inventory;
    }
    //else lstobjIN_Inventory = null;
};
var loadstorePO10100_LoadTaxTrans = function () {
    App.storePO10100_LoadTaxDoc.clearData();
    calcTaxTotal();

}; 
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////





//// Event ///////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////

function Save() {
    App.dataForm.getForm().updateRecord();
    if (App.dataForm.isValid()) {
        App.dataForm.submit({
            waitMsg: waitMsg,
            method: 'POST',
            url: 'PO10100/Save',
            timeout: 180000,
            params: {
                lstDet: Ext.encode(App.storePO_DetailLoad.getRecordsValues()),
                lstHeader: Ext.encode(App.storePO_Header.getRecordsValues())
            },
            success: function (msg, data) {
                if (this.result.PONbr != null) {
                    App.cboPONbr.getStore().reload();
                    App.cboPONbr.setValue(this.result.PONbr);
                    //App.cboPONbr.getStore().events['cboPONbr_Change'].suspend();
                    //App.cboPONbr.getStore().setValue(this.result.PONbr);
                    //App.cboPONbr.getStore().events['cboPONbr_Change'].resume();
                }
                processMessage(msg, data, true);
                menuClick('refresh');
            },
            failure: function (msg, data) {
                processMessage(msg, data, true);
            }
        });
    }
}
function Delete(item) {
    if (item == 'yes') {
        App.direct.PO10100Delete(App.cboPONbr.getValue(), {
            success: function (result) {
                App.cboPONbr.getStore().load();
                menuClick('new');
            },
            eventMask: { msg: '@Util.GetLang("DeletingData")', showMask: true }
        });
    }
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (focus == 'Header') {
                MoveFirst(App.cboPONbr, 'PONbr');
            }
            else if (focus == 'grdDetail') {
                App.SelectModelPO_Detail.select(0);
            } 
            break;
        case "next":
            if (focus == 'Header') {
                MoveNext(App.cboPONbr, 'PONbr');
            }
            else if (focus == 'grdDetail') {
                App.SelectModelPO_Detail.selectNext();
            } 
            break;
        case "prev":
            if (focus == 'Header') {
                MovePrev(App.cboPONbr, 'PONbr');
            }
            else if (focus == 'grdDetail') {
                App.SelectModelPO_Detail.selectPrevious();
            }
            break;
        case "last":
            if (focus == 'Header') {
                MoveLast(App.cboPONbr, 'PONbr');
            }
            else if (focus == 'grdDetail') {
                App.SelectModelPO_Detail.select(App.storegrdDetail.data.items.length - 1);
            } 
            break;
        case "save":
            Save();
            break;
        case "delete":
            if (focus == 'Header') {
                if (App.cboPONbr.value) {
                    if (isDelete) {
                        callMessage(11, '', 'Delete');
                    }
                } else {
                    menuClick('new');
                }
            } else if (focus == 'grdDetail') {
                if ((App.cboPONbr.value && isUpdate && isDelete) || (!App.cboPONbr.value && isInsert)) {
                    App.grdDetail.deleteSelected();
                }
            } 
            break;
        case "close":
            //if (App.dataForm.getRecord() != undefined) App.dataForm.updateRecord();
            //if (storeIsChange(App.storePO_Header, false) || storeIsChange(App.storePO_DetailLoad) || storeIsChange(App.storegrdDetail)) {
            //    callMessage(5, '', 'closeScreen');
            //} else {
            this.parentAutoLoadControl.close();
            //}
            break;
        case "new":
            App.cboPONbr.setValue(null);
            break;
        case "refresh":
            App.storePO_Header.reload();
            App.storePO_DetailLoad.reload();        
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
        callMessage(20404, 'PO_Setup', '');
        return false;

    }
    else if (passNull(App.cboPONbr.getValue()) == "" && objPO_Setup.AutoRef == 0) {
        callMessage(15, App.cboPONbr.fieldLabel, '');
        return false;
    }
    else if (passNull(App.cboVendID.getValue()) == "") {
        callMessage(41, '', '');
        return false;

    }
    else if (App.PODate.getValue() == "") {
        callMessage(15,App.PODate.fieldLabel, '');
        return false;

    }
    else if (App.cboDistAddr.getValue() == "") {
        callMessage(15, HQLangCode("DISTADDR"), '');

        //_complete = false;
        //e.Cancel = true;
        //tabThongTinGiaoHang.IsSelected = true;
        //cboDistAddr.Focus();

        return false;
    }
    det = e.record.data;
    strKeyGrid = e.record.idProperty.split(',');
    if (App.Status.getValue() != "H") return false;
    var strkey = e.record.idProperty.split(',');
    if (e.record.phantom == false && strkey.indexOf(e.column.dataIndex) != -1)
        return false;
   

    if (det.PurchaseType == "" ) {
        e.record.set("PurchaseType", "GI");
        e.record.set("SiteID", objUserDflt.POSite);
        e.record.set("VouchStage", 'N');
        e.record.set("RcptStage", 'N');
        var valueTax = '';
        App.cboTaxID.getStore().data.each(function (det) {
            valueTax += det.data.taxid + ',';

        });
        valueTax = valueTax.length>0?valueTax.substring(0, valueTax.length - 1):'';
        e.record.set("TaxID", valueTax);

        e.record.set("ReqdDate", bussinessDate);
        e.record.set("PromDate", bussinessDate);
        return false;
    }
    if (det.PurchaseType == "" && e.column.dataIndex != "PurchaseType") {
        callMessage(15, e.grid.columns[1].text, '');
        return false;
    }
    if (objPO_Setup.EditablePOPrice && e.column.dataIndex == "UnitCost") {      
        return false;
    }
    if (Ext.isEmpty(det.LineRef)) {
        e.record.set('LineRef', lastLineRef(App.storePO_DetailLoad));
    }


};
var grdPO_Detail_ValidateEdit = function (item, e) {
    if (strKeyGrid.indexOf(e.field) != -1) {
        if (duplicated(App.storePO_DetailLoad, e)) {
            callMessage(1112, e.value, '');
            return false;
        }
    }
};
var grdPO_Detail_Edit = function (item, e) {
    var objDetail = e.record.data;
    var objIN_Inventory = objectFindByKey(lstobjIN_Inventory.data.items, "InvtID", objDetail.InvtID).data;
    if (e.field == 'InvtID') {
        if (e.value) {
            var flat = App.storePO_DetailLoad.findBy(function (record, id) {
                if (!record.get('InvtID')) {
                    return true;
                }


                return false;
            });
            if (flat == -1) {
                App.storePO_DetailLoad.insert(App.storePO_DetailLoad.getCount(), Ext.data.Record());
            }

            //if (objDetail.PurchaseType.toUpperCase().trim() != "PR") {
            //    //Kiem tra neu ma tai san ton tai 2 lan thi thong bao cancel ko cho chon ma tai san
            //    if (strInvtId != objDetail.InvtID)// && this.GridPO_Detail.Rows.Where(p => strInvtId.toUpperCase().trim() == ((p.Data as PO_DetailLoad_Result).InvtID.toUpperCase().trim()) && objDetail.PurchaseType.toUpperCase().trim() != "PR" && "PR" != ((p.Data as PO_DetailLoad_Result).PurchaseType.toUpperCase().trim())).Count() > 0)//&& this.GridPO_Detail.Rows.Where(c => objDetail.PurchaseType.toUpperCase().trim() != ((c.Data as PO_DetailLoad_Result).PurchaseType.toUpperCase().trim())).Count() > 0)
            //    {
            //        callMessage(1093, strInvtId, null);
            //        return;
            //    }
            //}  
           
           
        }
    }
    if (e.field == "PurchUnit" || e.field == "InvtID" ) {
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

                callMessage(58, '', '');
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
        e.record.set("ExtCost",objDetail.UnitCost * objDetail.QtyOrd - objDetail.DiscAmt);
        if (objDetail.QtyOrd != 0)
        {
            e.record.set("DiscPct", Math.round((objDetail.DiscAmt / (objDetail.UnitCost * objDetail.QtyOrd)) * 100, 2));            
        }

    }
    else if (e.field == "DiscAmt") {                                                
        if (objDetail.ExtCost != 0)
        {
            e.record.set("DiscAmt",Math.round((objDetail.UnitCost * objDetail.QtyOrd * objDetail.DiscPct) / 100, 2));
        }
        e.record.set("ExtCost",objDetail.UnitCost * objDetail.QtyOrd - objDetail.DiscAmt);
    }   
    else if (e.field == "PurchUnit" || e.field == "InvtID" || e.field == "SiteID") {
        if (objPO_Setup.DfltLstUnitCost == "A") {          
            App.direct.ItemSitePrice(
                App.cboBranchID.getValue(), objDetail.InvtID, objDetail.SiteID,
               {
                   success: function (result) {
                       objIN_ItemSite = result;
                       UnitCost = result == null ? 0 : result.AvgCost;
                       UnitCost = Math.round((objDetail.UnitMultDiv == "D" ? (UnitCost / objDetail.CnvFact) : (UnitCost * objDetail.CnvFact)));
                       objDetail.UnitCost = UnitCost;
                       objDetail.ExtCost = UnitCost * objDetail.QtyOrd - objDetail.DiscAmt;
                   }
               });
        }
        else if (objPO_Setup.DfltLstUnitCost == "P") {

            App.direct.POPrice(
               App.cboBranchID.getValue(), objDetail.InvtID, objDetail.PurchUnit, Ext.Date.format(App.PODate.getValue(), 'Y-m-d'),
                {
                    success: function (result) {
                        UnitCost = result;
                        e.record.set("UnitCost", result);
                        e.record.set("ExtCost", result * objDetail.QtyOrd - objDetail.DiscAmt);
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

var cboGInvtID_Change = function (item, newValue, oldValue) {
    var r = null;
    if (item.value!=null)
    r = App.InvtID.valueModels[0];
    var objdet = App.SelectModelPO_Detail.getSelection()[0];
    if (r == null) {
        objdet.set('TranDesc', "");
        objdet.set('PurchUnit', "");
        //objdet.set('TranDesc', "");
    }
    else {
        var objIN_Inventory = objectFindByKey(lstobjIN_Inventory.data.items, "InvtID", r.data.InvtID).data
        strInvtID = objIN_Inventory.InvtID;
        strClassID = objIN_Inventory.ClassID;
        strStkUnit = objIN_Inventory.StkUnit;
        var strInvtId = r.data.InvtID;

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
        objdet.set('TranDesc', r.data.Descr);

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
    cpnyID= App.cboBranchID.getValue();
    if (item.displayTplData.length > 0) {

        App.storePO10100OM_UserDefault.load();   
        App.storePO10100PO_Setup.load();
        App.BranchName.setValue(item.displayTplData[0].BranchName);
        App.cboDistAddr.getStore().load();
        App.cboPONbr.getStore().load();
      
    }
   
    App.cboDistAddr.setValue(App.cboBranchID.getValue());
    App.cboPONbr.setValue("");
    loadDataHeader();

};
var cboVendID_Change = function (item, newValue, oldValue) {
    App.cboTaxID.getStore().load();
    App.storeTax.reload();
}
var ShiptoType_Change = function (item, newValue, oldValue) {
    DisableComboAddress();
    if (item.displayTplData.length > 0) {
        if (item.displayTplData[0].Code == "D") {

            App.cboDistAddr.enable(false);
            App.cboDistAddr.setValue(App.cboBranchID.getValue());
        }
        else if (item.displayTplData[0].Code == "S") {
            App.cboShipSiteID.enable(false);
        }
        else if (item.displayTplData[0].Code == "C") {
            App.ShipCustID.enable(false);
            App.ShiptoID.enable(false);

        }
        else if (item.displayTplData[0].Code == "V") {

            App.ShipVendID.enable(false);
            App.ShipVendAddrID.enable(false);
            App.ShipVendID.setValue(App.cboVendID.getValue());
        }
        else if (item.displayTplData[0].Code == "O") {
            App.ShipAddrID.enable(false);
        }

    }
};
var ShipDistAddrID_Change = function (item, newValue, oldValue) {
    if (item.displayTplData.length > 0 && App.cboPONbr.getValue() == "" && App.cboDistAddr.disabled == false) {
        App.ShipName.setValue(item.displayTplData[0].Name);
        App.ShipAttn.setValue(item.displayTplData[0].Attn);
        App.ShipAddr1.setValue(item.displayTplData[0].Addr1);
        App.ShipAddr2.setValue(item.displayTplData[0].Addr2);
        App.ShipCity.setValue(item.displayTplData[0].City);
        App.ShipState.setValue(item.displayTplData[0].State);
        App.ShipZip.setValue(item.displayTplData[0].Zip);
        App.ShipCountry.setValue(item.displayTplData[0].Country);
        App.ShipVia.setValue("");
        App.ShipPhone.setValue(item.displayTplData[0].Phone);
        App.ShipFax.setValue(item.displayTplData[0].Fax);
        App.ShipEmail.setValue("");
    }


};
var ShipAddrID_Change = function (item, newValue, oldValue) {
    if (item.displayTplData.length > 0 && App.cboPONbr.getValue() == "" && App.ShipAddrID.disabled == false) {
        App.ShipName.setValue(item.displayTplData[0].Name);
        App.ShipAttn.setValue(item.displayTplData[0].Attn);
        App.ShipAddr1.setValue(item.displayTplData[0].Addr1);
        App.ShipAddr2.setValue(item.displayTplData[0].Addr2);
        App.ShipCity.setValue(item.displayTplData[0].City);
        App.ShipState.setValue(item.displayTplData[0].State);
        App.ShipZip.setValue(item.displayTplData[0].Zip);
        App.ShipCountry.setValue(item.displayTplData[0].Country);
        App.ShipVia.setValue("");
        App.ShipPhone.setValue(item.displayTplData[0].Phone);
        App.ShipFax.setValue(item.displayTplData[0].Fax);
        App.ShipEmail.setValue("");
    }


};
var ShipVendAddrID_Change = function (item, newValue, oldValue) {
    if (item.displayTplData.length > 0 && App.cboPONbr.getValue() == "" && App.ShipVendAddrID.disabled == false) {
        App.ShipName.setValue(item.displayTplData[0].Name);
        App.ShipAttn.setValue(item.displayTplData[0].Attn);
        App.ShipAddr1.setValue(item.displayTplData[0].Addr1);
        App.ShipAddr2.setValue(item.displayTplData[0].Addr2);
        App.ShipCity.setValue(item.displayTplData[0].City);
        App.ShipState.setValue(item.displayTplData[0].State);
        App.ShipZip.setValue(item.displayTplData[0].Zip);
        App.ShipCountry.setValue(item.displayTplData[0].Country);
        App.ShipVia.setValue("");
        App.ShipPhone.setValue(item.displayTplData[0].Phone);
        App.ShipFax.setValue(item.displayTplData[0].Fax);
        App.ShipEmail.setValue("");
    }


};
var ShipVendID_Change = function (item, newValue, oldValue) {
    App.ShipVendAddrID.getStore().reload();


};
var ShiptoID_Change = function (item, newValue, oldValue) {
    if (item.displayTplData.length > 0 && App.cboPONbr.getValue() == "" && App.ShiptoID.disabled == false) {
        App.ShipName.setValue(item.displayTplData[0].Name);
        App.ShipAttn.setValue(item.displayTplData[0].Attn);
        App.ShipAddr1.setValue(item.displayTplData[0].Addr1);
        App.ShipAddr2.setValue(item.displayTplData[0].Addr2);
        App.ShipCity.setValue(item.displayTplData[0].City);
        App.ShipState.setValue(item.displayTplData[0].State);
        App.ShipZip.setValue(item.displayTplData[0].Zip);
        App.ShipCountry.setValue(item.displayTplData[0].Country);
        App.ShipVia.setValue("");
        App.ShipPhone.setValue(item.displayTplData[0].Phone);
        App.ShipFax.setValue(item.displayTplData[0].Fax);
        App.ShipEmail.setValue("");
    }


};
var ShipCustID_Change = function (item, newValue, oldValue) {
    if (item.displayTplData.length > 0 && App.cboPONbr.getValue() == "" && App.ShipCustID.disabled == false) {
        App.ShipName.setValue(item.displayTplData[0].Name);
        App.ShipAttn.setValue(item.displayTplData[0].Attn);
        App.ShipAddr1.setValue(item.displayTplData[0].Addr1);
        App.ShipAddr2.setValue(item.displayTplData[0].Addr2);
        App.ShipCity.setValue(item.displayTplData[0].City);
        App.ShipState.setValue(item.displayTplData[0].State);
        App.ShipZip.setValue(item.displayTplData[0].Zip);
        App.ShipCountry.setValue(item.displayTplData[0].Country);
        App.ShipVia.setValue("");
        App.ShipPhone.setValue(item.displayTplData[0].Phone);
        App.ShipFax.setValue(item.displayTplData[0].Fax);
        App.ShipEmail.setValue("");
    }


};
var ShipSiteID_Change = function (item, newValue, oldValue) {
    if (item.displayTplData.length > 0 && App.cboPONbr.getValue() == "" && App.cboShipSiteID.disabled == false) {
        App.ShipName.setValue(item.displayTplData[0].Name);
        App.ShipAttn.setValue(item.displayTplData[0].Attn);
        App.ShipAddr1.setValue(item.displayTplData[0].Addr1);
        App.ShipAddr2.setValue(item.displayTplData[0].Addr2);
        App.ShipCity.setValue(item.displayTplData[0].City);
        App.ShipState.setValue(item.displayTplData[0].State);
        App.ShipZip.setValue(item.displayTplData[0].Zip);
        App.ShipCountry.setValue(item.displayTplData[0].Country);
        App.ShipVia.setValue("");
        App.ShipPhone.setValue(item.displayTplData[0].Phone);
        App.ShipFax.setValue(item.displayTplData[0].Fax);
        App.ShipEmail.setValue("");
    }


};

function DisableComboAddress() {
    App.cboDistAddr.setValue('');
    App.cboShipSiteID.setValue('');
    App.ShipCustID.setValue('');
    App.ShiptoID.setValue('');
    App.ShipVendID.setValue('');
    App.ShipVendAddrID.setValue('');
    App.ShipAddrID.setValue('');

    App.cboDistAddr.disable(true);
    App.cboShipSiteID.disable(true);
    App.ShipCustID.disable(true);
    App.ShiptoID.disable(true);
    App.ShipVendID.disable(true);
    App.ShipVendAddrID.disable(true);
    App.ShipAddrID.disable(true);
}
var cboPONbr_Change = function (item, newValue, oldValue) {
    App.storePO_Header.clearData();
    App.storePO_DetailLoad.clearData();
    //if (item.valueModels.length != 0) {
    //    oldValue = null;
        App.storePO_Header.reload();

    //}

};
var cboStatus_Change = function (item, newValue, oldValue) {
    App.Handle.getStore().reload();
    lockcontrol();
};

function lockcontrol() {
    lock = (App.Status.getValue() == 'H' && isInsert && isUpdate) ? false : true;
    if (App.cboPONbr.valueModels.length == 0) {
       
        App.PODate.setReadOnly(lock);
   
    }
    else {
        
        App.PODate.setReadOnly(lock);
       
    }
};
function storeIsChange(store, isCreate) {
    if (isCreate == undefined) isCreate = true;
    if ((isCreate == true ? store.getChangedData().Created.length > 1 : false) || store.getChangedData().Updated != undefined || store.getChangedData().Deleted != undefined) {
        return true;
    }
    return false;
};

//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////




//// Function ////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
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
    //App.storePO_DetailLoad.data.each(function (det) {
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
    for (var j = 0; j < App.storePO_DetailLoad.data.length; j++) {
        var det = App.storePO_DetailLoad.data.items[j];
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

for (var j = 0; j < App.storePO_DetailLoad.data.length; j++) {
    var det = App.storePO_DetailLoad.data.items[j];
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
        CTN +=  det.data.QtyOrd;
    }
    else if (CnvFact > 1) {
        CTN += Math.round(det.data.QtyOrd / CnvFact,0);
        PCS +=  det.data.QtyOrd % CnvFact;
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

function delTax(index) {
    //if (App.Status != "H" ) return false;
    var lineRef = App.storePO_DetailLoad.data.items[index].data.LineRef;

    for (var j = App.storePO10100_LoadTaxTrans.data.length - 1; j >= 0; j--) {
        if (App.storePO10100_LoadTaxTrans.data.items[j].data.LineRef == lineRef)
            App.storePO10100_LoadTaxTrans.data.removeAt(j);
    }
    clearTax(index);
    calcTaxTotal();
    calcDet();
    return true;

}
function clearTax(index) {
    App.storePO_DetailLoad.data.items[index].set('TaxID00', '');
    App.storePO_DetailLoad.data.items[index].set('TaxAmt00', 0);
    App.storePO_DetailLoad.data.items[index].set('TxblAmt00', 0);

    App.storePO_DetailLoad.data.items[index].set('TaxID01', '');
    App.storePO_DetailLoad.data.items[index].set('TaxAmt01', 0);
    App.storePO_DetailLoad.data.items[index].set('TxblAmt01', 0);

    App.storePO_DetailLoad.data.items[index].set('TaxID02', '');
    App.storePO_DetailLoad.data.items[index].set('TaxAmt02', 0);
    App.storePO_DetailLoad.data.items[index].set('TxblAmt02', 0);

    App.storePO_DetailLoad.data.items[index].set('TaxID03', '');
    App.storePO_DetailLoad.data.items[index].set('TaxAmt03', 0);
    App.storePO_DetailLoad.data.items[index].set('TxblAmt03', 0);
}
function calcTax(index) {
    console.log('calcTax');
    var det = App.storePO_DetailLoad.data.items[index].data;
    var record = App.storePO_DetailLoad.data.items[index];  
    if (index < 0) return true;
  

    var dt = [];
    if (det.TaxID == "*") {
        for (var j = 0; j < App.storeTax.data.length; j++) {
            var item = App.storeTax.data.items[j];      
            dt.push(item.data);
        };
    }
    else {
        var strTax = det.TaxID.split(',');
        if (strTax.length > 0) {
            for (var k = 0; k < strTax.length; k++) {
                for (var j = 0; j < App.storeTax.data.length; j++) {
                    if (strTax[k] == App.storeTax.data.items[j].data.taxid) {
                        dt.push(App.storeTax.data.items[j].data);
                        break;
                    }
                }
            }
        }
        else {
            if (Ext.isEmpty(det.TaxID) || Ext.isEmpty(det.TaxCat))
                App.storePO_DetailLoad.data.items[i].set('TxblAmt00', det.ExtCost);
            return false;
        }
    }

    var taxCat = det.TaxCat;
    var prcTaxInclRate = 0, totPrcTaxInclAmt = 0, txblAmtL1 = 0, txblAmtAddL2 = 0;
    for (var j = 0; j < dt.length; j++) {
        var objTax = findInStore(App.storeTax, ['taxid'], [dt[j].taxid]);
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
        var objTax = findInStore(App.storeTax, ['taxid'], [dt[j].taxid]);
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
        var objTax = findInStore(App.storeTax, ['taxid'], [dt[j].taxid]);
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
    for (var i = 0; i < App.storePO10100_LoadTaxTrans.data.length; i++) {
        if (App.storePO10100_LoadTaxTrans.data.items[i].data.taxid == taxID && App.storePO10100_LoadTaxTrans.data.items[i].data.LineRef == lineRef) {
            var tax = App.storeTax.data.items[i];         
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
        var newTax = Ext.create('App.ModelPO10100_LoadTaxTrans_Result');
        newTax.data.BranchID = cpnyID;     
        newTax.data.TaxID = taxID;
        newTax.data.LineRef = lineRef;
        newTax.data.TaxRate = taxRate;
        newTax.data.TaxLevel = taxLevel.toString();
        newTax.data.TaxAmt = taxAmt;
        newTax.data.TxblAmt = txblAmt;

        App.storePO10100_LoadTaxTrans.data.add(newTax);
    }
    App.storePO10100_LoadTaxTrans.sort('LineRef', "ASC");
    calcDet();

}

function updateTax(index) {
    console.log('updateTax(index)');
    if (index < 0) return;
    var j = 0;
    var det = App.storePO_DetailLoad.data.items[index].data;
    var record = App.storePO_DetailLoad.data.items[index];
    for (var i = 0; i < App.storePO10100_LoadTaxTrans.data.length; i++) {
        var item = App.storePO10100_LoadTaxTrans.data.items[i];
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
    App.storePO10100_LoadTaxDoc.clearData();
    var flat = false;
    for (var i = 0; i < App.storePO10100_LoadTaxTrans.data.length; i++) {
        var tax = App.storePO10100_LoadTaxTrans.data.items[i];
        flat = true;
        for (var j = 0; j < App.storePO10100_LoadTaxDoc.data.length; j++) {
            var taxDoc = App.storePO10100_LoadTaxDoc.data.items[j];
            if (tax.data.PONbr == taxDoc.data.PONbr && tax.data.TaxID == taxDoc.data.TaxID) {
                taxDoc.data.TxblAmt += tax.data.TxblAmt;
                taxDoc.data.TaxAmt += tax.data.TaxAmt;              
                flat = false;
                taxDoc.commit();
                break;
            }
        };
        if (flat) {
            var newTaxDoc = Ext.create('App.ModelPO10100_LoadTaxTransDoc');
            newTaxDoc.data.BranchID = tax.data.BranchID;
            newTaxDoc.data.PONbr = tax.data.PONbr;
            newTaxDoc.data.TaxID = tax.data.TaxID;
            newTaxDoc.data.TaxAmt = tax.data.TaxAmt;
            newTaxDoc.data.TaxRate = tax.data.TaxRate;
            newTaxDoc.data.TxblAmt = tax.data.TxblAmt;
           
            App.storePO10100_LoadTaxDoc.data.add(newTaxDoc);
           // newTaxDoc.commit();
        }
     
    };
    App.grdTaxDoc.getView().refresh(false);
}


function objectFindByKey(arrayitems, key, value) {
    for (var i = 0; i < arrayitems.length; i++) {
        if (arrayitems[i].data[key] === value) {
            return arrayitems[i];
        }
    }
    return null;
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

function setUOM(invtID, classID, stkUnit, fromUnit){
    if (!Ext.isEmpty(fromUnit)) {
        var data = findInStore(App.StoreUnit,['UnitType','ClassID','InvtID','FromUnit','ToUnit'],["3", "*", invtID, fromUnit, stkUnit]);
        if(!Ext.isEmpty(data)){
            return data;
        }
        
        data = findInStore(App.StoreUnit,['UnitType','ClassID','InvtID','FromUnit','ToUnit'],["2", classID, "*", fromUnit, stkUnit]);
        if(!Ext.isEmpty(data)){
            return data;
        }

        data = findInStore(App.StoreUnit,['UnitType','ClassID','InvtID','FromUnit','ToUnit'],["1", "*", "*", fromUnit, stkUnit]);
        if(!Ext.isEmpty(data)){
            return data;
        }
        callMessage(25,invtID,'');
        return null;
    }
    return null;
};
function oM_GetCnvFactToUnit( invtID,  unitDesc)
{
    var cnvFact = 1;
    var data;
    App.StoreUnit.data.each(function (item) {
        if (item.data.InvtID == invtID && item.data.FromUnit != unitDesc && item.data.ToUnit == unitDesc)
        {
            data = item;
            return;
        }
    });
    if (data != null)
    {
        if (data.data.MultDiv == "D")
            cnvFact = 1 / data.data.CnvFact;
        else
            cnvFact = data.data.CnvFact;
    }

    return cnvFact;
};
function findInStore(store, fields, values) {
    var data;
    store.data.each(function (item) {
        var int = 0;
        for (var i = 0; i < fields.length; i++) {
            if (item.get(fields[i]) == values[i]) {
                int++;
            }
        }
        if (int == fields.length) {
            data = item.data;
            return false;
        }
    });
    return data;
};


//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////