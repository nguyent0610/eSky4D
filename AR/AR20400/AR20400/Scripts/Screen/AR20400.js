var keysTab_4 = ['LTTContractNbr'];
var fieldsCheckRequireTab_4 = ["LTTContractNbr"];
var fieldsLangCheckRequireTab_4 = ["LTTContractNbr"];

var keysTab_41 = ['Type'];
var fieldsCheckRequireTab_41 = ["Type"];
var fieldsLangCheckRequireTab_41 = ["Type"];

var keysTab_5 = ['Type'];
var fieldsCheckRequireTab_5 = ["Type"];
var fieldsLangCheckRequireTab_5 = ["Type"];

var keysTab_6 = ['DispMethod'];
var fieldsCheckRequireTab_6 = ["DispMethod", "Descr"];
var fieldsLangCheckRequireTab_6 = ["DispMethod", "Descr"];

var keysTab_7 = ['Code'];
var fieldsCheckRequireTab_7 = ["Code", "Descr"];
var fieldsLangCheckRequireTab_7 = ["Code", "Descr"];

var _lstAR_LTTContract;
var _lstAR_LTTContractDetail;
var CustId = '';
var _focusNo = 0;
var _Source = 0;
var _maxSource = 30;
var _isLoadMaster = false;

var _hiddenTree = '';
var _maxLevel = 0;
var _nodeID = '';
var _nodeLevel = '';
var _parentRecordID = '';
var _recordID = '';
var _root = '';
var parentRecordIDAll = '';
var parentRecordID = '';
var _Flag;
var _treeExpandAll = false;
var value1 = '';
var value2 = '';
var _addr = '';
var _country = '';
var _state = '';
var _district = '';
var _ward = '';
var _addr1 = '';
var _addr2 = '';
var _countryBill = '';
var _stateBill = '';
var _districtBill = '';
var _wardBill = '';
var _addr1Bill = '';
var _addr2Bill = '';
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        App.cboCpnyID.setValue(HQ.cpnyID);
        _isLoadMaster = true;
        _Source = 0;
        App.stoAR_Customer.reload();
        HQ.common.showBusy(false);

        if (hideContract == 'true')
            App.tabDetail.child('#pnlLTT').tab.hide();
        if (hideAdvTool == 'true')
            App.tabDetail.child('#pnlADV').tab.hide();
        if (hideSellingProduct == 'true')
            App.tabDetail.child('#pnlAR_CustSellingProducts').tab.hide();
        if (hideDisplayMethod == 'true')
            App.tabDetail.child('#pnlDispMethod').tab.hide();
        if (hideCustomerChild == 'true')
            App.tabDetail.child('#pnlAR_CustomerChild').tab.hide();
        
        if (readonlyShopType == 'true')
            App.cboShopType.setReadOnly(true);
        else
            App.cboShopType.setReadOnly(false);
    }
};
var stoAR20400_ppGetTerritoryForUser_load = function (sto) {
    if (sto.data.length > 0) {
        var objData = sto.data.items[0].data;
        value1 = objData.Territory;
        value2 = objData.Area;
    }
};
var firstLoad = function () {
    HQ.util.checkAccessRight();
    HQ.isFirstLoad = true;
    

    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboStatus.getStore().addListener('load', checkLoad);
    App.cboCpnyID.getStore().addListener('load', checkLoad);
    App.cboClassId.getStore().addListener('load', checkLoad);
    App.cboCustType.getStore().addListener('load', checkLoad);
    App.cboPriceClassID.getStore().addListener('load', checkLoad);
    App.cboTerms.getStore().addListener('load', checkLoad);
    App.cboCrRule.getStore().addListener('load', checkLoad);
    App.cboTerritory.getStore().addListener('load', checkLoad);
    App.cboArea.getStore().addListener('load', checkLoad);
    App.cboLocation.getStore().addListener('load', checkLoad);
    App.cboChannel.getStore().addListener('load', checkLoad);
    App.cboShopType.getStore().addListener('load', checkLoad);
    App.cboSellingProd.getStore().addListener('load', checkLoad);
    App.cboCountry.getStore().addListener('load', checkLoad);
    App.cboState.getStore().addListener('load', checkLoad);
    App.cboDistrict.getStore().addListener('load', checkLoad);
    App.cboCity.getStore().addListener('load', checkLoad);
    App.cboBillCountry.getStore().addListener('load', checkLoad);
    App.cboBillState.getStore().addListener('load', checkLoad);
    App.cboBillCity.getStore().addListener('load', checkLoad);
    App.cboTaxDflt.getStore().addListener('load', checkLoad);
    App.cboTaxID00.getStore().addListener('load', checkLoad);
    App.cboTaxID01.getStore().addListener('load', checkLoad);
    App.cboTaxID02.getStore().addListener('load', checkLoad);
    App.cboTaxID03.getStore().addListener('load', checkLoad);
    App.cboType.getStore().addListener('load', checkLoad);
    App.cboTypeADV.getStore().addListener('load', checkLoad);
    App.cboCodeDisplay.getStore().addListener('load', checkLoad);
    App.cboSellingProd1.getStore().addListener('load', checkLoad);
    App.cboSubTerritory.getStore().addListener('load', checkLoad);
    //App.cboCustId.getStore().addListener('load', stoCustId_Load);
    if (!Ext.isEmpty(HQ.hideColumn)) {
        var lstHide = HQ.hideColumn.split(',');
        for (var i = 0; i < lstHide.length; i++) {
            if (lstHide[i] == 'StandID') {
                App.cboAddStand.setVisible(false);
            } else if (lstHide[i] == 'BrandID') {
                App.cboAddBrand.setVisible(false);
            } else if (lstHide[i] == 'SizeID') {
                App.cboAddSize.setVisible(false);
            } else if (lstHide[i] == 'DisplayID') {
                App.cboAddDisplayID.setVisible(false);
            }
        }
    }
    App.cboCity.setVisible(!HQ.hideCity);
    App.cboBillCity.setVisible(!HQ.hideCity);
    App.lblCITY.setVisible(!HQ.hideCity);
    App.cboOunit.setVisible(!HQ.hideOUnit);
    
    App.cboPriceClassID.allowBlank = !HQ.reqPriceClassID;
    App.cboChannel.allowBlank = !HQ.reqChannel;
    App.cboSlsperId.allowBlank = !HQ.reqSlsperson;
    App.cboOunit.allowBlank = !HQ.reqOUnit;    
    App.frmMain.isValid();

    if (!HQ.showCompetitor) {
        App.tabDetail.child('#pnlCompetitor').tab.hide();
    } else {
        App.tabDetail.child('#pnlCompetitor').tab.show();
    }
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == 'header') {
                HQ.combo.first(App.cboCustId, HQ.isChange);
            }
            else if (HQ.focus == 'grdAR_LTTContract') {
                HQ.grid.first(App.grdAR_LTTContract);
            }
            else if (HQ.focus == 'grdAR_CustAdvTool') {
                HQ.grid.first(App.grdAR_CustAdvTool);
            }
            else if (HQ.focus == 'grdAR_CustSellingProducts') {
                HQ.grid.first(App.grdAR_CustSellingProducts);
            }
            else if (HQ.focus == 'grdAR_CustDisplayMethod') {
                HQ.grid.first(App.grdAR_CustDisplayMethod);
            }
            else if (HQ.focus == 'grdAR_CustomerChild') {
                HQ.grid.first(App.grdAR_CustomerChild);
            }
            break;
        case "prev":
            if (HQ.focus == 'header') {
                HQ.combo.prev(App.cboCustId, HQ.isChange);
            }
            else if (HQ.focus == 'grdAR_LTTContract') {
                HQ.grid.prev(App.grdAR_LTTContract);
            }
            else if (HQ.focus == 'grdAR_CustAdvTool') {
                HQ.grid.prev(App.grdAR_CustAdvTool);
            }
            else if (HQ.focus == 'grdAR_CustSellingProducts') {
                HQ.grid.prev(App.grdAR_CustSellingProducts);
            }
            else if (HQ.focus == 'grdAR_CustDisplayMethod') {
                HQ.grid.prev(App.grdAR_CustDisplayMethod);
            }
            else if (HQ.focus == 'grdAR_CustomerChild') {
                HQ.grid.prev(App.grdAR_CustomerChild);
            }
            break;
        case "next":
            if (HQ.focus == 'header') {
                HQ.combo.next(App.cboCustId, HQ.isChange);
            }
            else if (HQ.focus == 'grdAR_LTTContract') {
                HQ.grid.next(App.grdAR_LTTContract);
            }
            else if (HQ.focus == 'grdAR_CustAdvTool') {
                HQ.grid.next(App.grdAR_CustAdvTool);
            }
            else if (HQ.focus == 'grdAR_CustSellingProducts') {
                HQ.grid.next(App.grdAR_CustSellingProducts);
            }
            else if (HQ.focus == 'grdAR_CustDisplayMethod') {
                HQ.grid.next(App.grdAR_CustDisplayMethod);
            }
            else if (HQ.focus == 'grdAR_CustomerChild') {
                HQ.grid.next(App.grdAR_CustomerChild);
            }
            break;
        case "last":
            if (HQ.focus == 'header') {
                HQ.combo.last(App.cboCustId, HQ.isChange);
            }
            else if (HQ.focus == 'grdAR_LTTContract') {
                HQ.grid.last(App.grdAR_LTTContract);
            }
            else if (HQ.focus == 'grdAR_CustAdvTool') {
                HQ.grid.last(App.grdAR_CustAdvTool);
            }
            else if (HQ.focus == 'grdAR_CustSellingProducts') {
                HQ.grid.last(App.grdAR_CustSellingProducts);
            }
            else if (HQ.focus == 'grdAR_CustDisplayMethod') {
                HQ.grid.last(App.grdAR_CustDisplayMethod);
            }
            else if (HQ.focus == 'grdAR_CustomerChild') {
                HQ.grid.last(App.grdAR_CustomerChild);
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
                        HQ.message.show(150, '', 'refresh');
                    } else {
                        CustId = '';
                        App.cboCustId.setValue('');
                        App.cboCustId.focus();
                    }
                }
                else if (HQ.focus == 'grdAR_LTTContract') {
                    HQ.grid.insert(App.grdAR_LTTContract);
                }
                else if (HQ.focus == 'grdAR_LTTContractDetail') {
                    HQ.grid.insert(App.grdAR_LTTContractDetail);
                }
                else if (HQ.focus == 'grdAR_CustAdvTool') {
                    HQ.grid.insert(App.grdAR_CustAdvTool);
                }
                else if (HQ.focus == 'grdAR_CustSellingProducts') {
                    HQ.grid.insert(App.grdAR_CustSellingProducts);
                }
                else if (HQ.focus == 'grdAR_CustDisplayMethod') {
                    HQ.grid.insert(App.grdAR_CustDisplayMethod);
                } else if (HQ.focus == 'grdAR_CustomerChild') {
                    HQ.grid.insert(App.grdAR_CustomerChild);
                }
                
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.cboStatus.getValue() == 'H') {
                    if (HQ.focus == 'header') {
                        if (App.cboCustId.getValue()) {
                            HQ.message.show(11, '', 'deleteData');
                        } else {
                            menuClick('new');
                        }
                    }
                    else if (HQ.focus == 'grdAR_LTTContract') {
                        if (App.slmAR_LTTContract.selected.items[0] != undefined) {
                            if (App.slmAR_LTTContract.selected.items[0].data.LTTContractNbr != "") {
                                HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdAR_LTTContract)], 'deleteData', true);
                            }
                        }
                    }
                    else if (HQ.focus == 'grdAR_LTTContractDetail') {
                        if (App.slmAR_LTTContractDetail.selected.items[0] != undefined) {
                            if (App.slmAR_LTTContractDetail.selected.items[0].data.Type != "") {
                                HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdAR_LTTContractDetail)], 'deleteData', true);
                            }
                        }
                    }
                    else if (HQ.focus == 'grdAR_CustAdvTool') {
                        if (App.slmAR_CustAdvTool.selected.items[0] != undefined) {
                            if (App.slmAR_CustAdvTool.selected.items[0].data.Type != "") {
                                HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdAR_CustAdvTool)], 'deleteData', true);
                            }
                        }
                    }
                    else if (HQ.focus == 'grdAR_CustSellingProducts') {
                        if (App.slmAR_CustSellingProducts.selected.items[0] != undefined) {
                            if (App.slmAR_CustSellingProducts.selected.items[0].data.Code != "") {
                                HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdAR_CustSellingProducts)], 'deleteData', true);
                            }
                        }
                    }
                    else if (HQ.focus == 'grdAR_CustDisplayMethod') {
                        if (App.slmAR_CustDisplayMethod.selected.items[0] != undefined) {
                            if (App.slmAR_CustDisplayMethod.selected.items[0].data.DispMethod != "") {
                                HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdAR_CustDisplayMethod)], 'deleteData', true);
                            }
                        }
                    }
                    else if (HQ.focus == 'grdAR_CustomerChild') {
                        if (App.slmAR_CustomerChild.selected.items[0] != undefined) {
                            if (App.slmAR_CustomerChild.selected.items[0].data.CustChildID != "") {
                                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdAR_CustomerChild)], 'deleteData', true);
                                    }
                                }
                            } 
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain)
                    && HQ.store.checkRequirePass(App.stoAR_LTTContract, keysTab_4, fieldsCheckRequireTab_4, fieldsLangCheckRequireTab_4)
                    && HQ.store.checkRequirePass(App.stoAR_LTTContractDetail, keysTab_41, fieldsCheckRequireTab_41, fieldsLangCheckRequireTab_41)
                    && HQ.store.checkRequirePass(App.stoAR_CustAdvTool, keysTab_5, fieldsCheckRequireTab_5, fieldsLangCheckRequireTab_5)
                    && HQ.store.checkRequirePass(App.stoAR_CustSellingProducts, keysTab_7, fieldsCheckRequireTab_7, fieldsLangCheckRequireTab_7)
                    && HQ.store.checkRequirePass(App.stoAR_CustDisplayMethod, keysTab_6, fieldsCheckRequireTab_6, fieldsLangCheckRequireTab_6)) {
                    if (App.txtEMailAddr.getValue())
                        if (!HQ.util.checkEmail(App.txtEMailAddr.getValue()))
                            return false;
                    if (App.cboCustId.allowBlank == false) {
                        if (HQ.util.checkSpecialChar(App.cboCustId.getValue()) == false) {
                            HQ.message.show(20140811, App.cboCustId.fieldLabel);
                            App.cboCustId.focus();
                            App.cboCustId.selectText();
                            return false;
                        }
                    }
                    if (_hiddenTree == 'false') {
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
                    else
                        save();
                }
            }
            break;

        case "print":
            //if (!Ext.isEmpty(App.cboContract.getValue())) {
            //    App.winReport.show();
            //}
            break;
        case "close":
            break;
    }
};

var slmAR_LTTContract_Select = function (sender, e) {
    App.grdAR_LTTContractDetail.getFilterPlugin().clearFilters();
    App.grdAR_LTTContractDetail.getFilterPlugin().getFilter('LTTContractNbr').setValue([e.data.LTTContractNbr, '']);
    App.grdAR_LTTContractDetail.getFilterPlugin().getFilter('LTTContractNbr').setActive(true);
};

var filterStore = function (store, field, value) {
    store.filterBy(function (record) {
        if (record) {
            if (record.data[field].toString().toLowerCase() == (HQ.util.passNull(value.toString()).toLowerCase())) {
                return record;
            }
        }
    });
};

var frmChange = function () {
    if (App.stoAR_Customer.getCount() > 0) {
        App.frmMain.getForm().updateRecord();
    }
    HQ.isChange = HQ.store.isChange(App.stoAR_Customer) == false ? (HQ.store.isChange(App.stoAR_CustAdvTool)
                                                        == false ? (HQ.store.isChange(App.stoAR_CustSellingProducts)
                                                        == false ? (HQ.store.isChange(App.stoAR_CustDisplayMethod)
                                                        == false ? (HQ.store.isChange(App.stoAR_LTTContract)
                                                        == false ? (HQ.store.isChange(App.stoAR_LTTContractDetail)) : true) : true) : true) : true) : true;
    HQ.common.changeData(HQ.isChange, 'AR20400');
    if (App.cboCustId.valueModels == null || HQ.isNew == true) {
        if (App.cboCustId.allowBlank == false) {
            App.cboCustId.setReadOnly(false);
            if(HQ.isChange == true)
                App.cboCpnyID.setReadOnly(true);
            else
                App.cboCpnyID.setReadOnly(false);
        }
        else {
            App.cboCustId.setReadOnly(HQ.isChange);
            App.cboCpnyID.setReadOnly(HQ.isChange);
        }
    }
    else {
        App.cboCustId.setReadOnly(HQ.isChange);
        App.cboCpnyID.setReadOnly(HQ.isChange);
    }    
    
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoLoad = function (sto) {
    //HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
    HQ.common.setForceSelection(App.frmMain, false, "cboCpnyID,cboCustId,cboHandle");
    App.cboCpnyID.forceSelection = true;
    App.cboCustId.forceSelection = true;
    HQ.isNew = false;
    App.cboDistrict.allowBlank = false;

    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, ['BranchID', 'CustId']);
        record = sto.getAt(0);
        if (App.cboCustTypeAR20400_pcCustType.data.items[0])
            record.set('CustType', App.cboCustTypeAR20400_pcCustType.data.items[0].data.Code);
        else
            record.set('CustType', 'L');
        record.set('TaxDflt', 'C');
        record.set('CrRule', 'A');
        record.set('Status', 'H');
        record.set('ExpiryDate', HQ.bussinessDate);
        record.set('EstablishDate', HQ.bussinessDate);
        record.set('Birthdate', HQ.bussinessDate);
        record.set('Territory', value1);
        record.set('Area', value2);
        HQ.isNew = true;
        App.cboCustId.forceSelection = false;
       // App.cboDistrict.allowBlank = true;
        //if (App.stoCheckAutoCustID.data.items[0].data.Flag == '1')
        if (_Flag == "1")
            App.cboCustId.forceSelection = true;
        HQ.common.setRequire(App.frmMain);
        sto.commitChanges();
    }
    else {
        var objPageCust = findRecordCombo(App.cboCustId.getValue());
        if (objPageCust) {
            var positionCust = calcPage(objPageCust.index);
            App.cboCustId.loadPage(positionCust);
        }
        //App.cboCustId.setValue(App.cboCustId.getValue());
    }
    var record = sto.getAt(0);
    App.lblName.setValue(record.data.CustName);

    App.frmMain.getForm().loadRecord(record);
    HQ.combo.expand(App.cboAddBrand, ',');
    // display image
    App.fupImages.reset();
    if (record.data.ProfilePic) {
        displayImage(App.imgImages, record.data.ProfilePic);
    }
    else {
        App.imgImages.setImageUrl("");
    }

    App.stoAR_LTTContract.reload();

    if (record.data.Status == 'I' && !HQ.IsEditAllStatus)
        setAllowBank_InActive(false);
    else
        setAllowBank_InActive(true);

    if (record.data.Status == 'H' || HQ.IsEditAllStatus) {
        HQ.common.lockItem(App.frmMain, false);
        App.fupImages.setDisabled(false);
        if (readonlyShopType == 'true')
            App.cboShopType.setReadOnly(true);
    }
    else {
        HQ.common.lockItem(App.frmMain, true);
        App.fupImages.setDisabled(true);
        App.cboDistrict.allowBlank = true;
    }
    if (!HQ.isInsert && HQ.isNew) {
        App.cboCustId.forceSelection = true;
        HQ.common.lockItem(App.frmMain, true);
        App.fupImages.setDisabled(true);
    }
    else if (!HQ.isUpdate && !HQ.isNew) {
        HQ.common.lockItem(App.frmMain, true);
        App.fupImages.setDisabled(true);
    }
    if (!HQ.isNew && _hiddenTree == 'false' ) searchNode();
    

    HQ.common.showBusy(false);

    var curRecord = App.frmMain.getRecord();
    if (curRecord != undefined) {
        if (curRecord.data.OUnit) {
            HQ.combo.expand(App.cboOunit, ',');
        }
    }

};

/////////////////////////////// GIRD AR_LTTContract /////////////////////////////////
var stoAR_LTTContract_Load = function (sto) {
    //HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            //HQ.store.insertBlank(sto, keysTab_4);
            HQ.store.insertRecord(sto, keysTab_4, { FromDate: HQ.bussinessDate, ToDate: HQ.bussinessDate, ExtDate: HQ.bussinessDate });
        }
        //HQ.isFirstLoad = false; //sto load cuoi se su dung
    }
    var TypeLTTDetail = [];
    App.cboType.getStore().data.each(function (item) {
        if (TypeLTTDetail.indexOf(item.data.Code) == -1) {
            TypeLTTDetail.push([item.data.Code, item.data.Descr]);
        }
    });
    filterFeature1 = App.grdAR_LTTContractDetail.filters;
    colAFilter1 = filterFeature1.getFilter('Type');
    colAFilter1.menu = colAFilter1.createMenu({
        options: TypeLTTDetail
    });

    App.stoAR_LTTContractDetail.reload();
    frmChange();
};

var grdAR_LTTContract_BeforeEdit = function (editor, e) {
    if (App.cboStatus.getValue() != 'H' && !HQ.IsEditAllStatus) return false;
    if (e.field == 'FromDate') {
        if (e.record.data.ToDate) {
            App.dtpFromDate.setMaxValue(e.record.data.ToDate);
        }
    }
    else if (e.field == 'ToDate') {
        if (e.record.data.FromDate) {
            App.dtpToDate.setMinValue(e.record.data.FromDate);
        }
        else return false;
    }

    if (!HQ.grid.checkBeforeEdit(e, keysTab_4)) return false;
};

var grdAR_LTTContract_Edit = function (item, e) {
    if (e.field == 'LTTContractNbr') {
        if (e.value) {
            e.record.set('FromDate', HQ.bussinessDate);
            e.record.set('ToDate', HQ.bussinessDate);
            e.record.set('ExtDate', HQ.bussinessDate);
        }
    }
    checkInsertKey_grdAR_LTTContract(App.grdAR_LTTContract, e, keysTab_4);
    frmChange();
};

var checkInsertKey_grdAR_LTTContract = function (grd, e, keys) {
    if (keys.indexOf(e.field) != -1) {
        if (e.value != '')
            HQ.store.insertRecord(grd.getStore(), keys, { FromDate: HQ.bussinessDate, ToDate: HQ.bussinessDate, ExtDate: HQ.bussinessDate });
    }
};

var grdAR_LTTContract_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdAR_LTTContract, e, keysTab_4, false);
};

var grdAR_LTTContract_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdAR_LTTContract);
    frmChange();
};

/////////////////////////////// GIRD AR_LTTContractDetail /////////////////////////////////
var stoAR_LTTContractDetail_Load = function (sto) {
    //HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keysTab_41);
        }
        //HQ.isFirstLoad = false; //sto load cuoi se su dung
    }
    App.stoAR_CustAdvTool.reload();
    frmChange();
};

var grdAR_LTTContractDetail_BeforeEdit = function (editor, e) {
    if (App.cboStatus.getValue() != 'H' && !HQ.IsEditAllStatus) return false;
    if (App.slmAR_LTTContract.selected.items[0] == undefined) {
        HQ.message.show(2002);
        return false;
    }
};

var grdAR_LTTContractDetail_Edit = function (item, e) {
    if (e.field == 'Type') {
        e.record.set('LTTContractNbr', App.slmAR_LTTContract.selected.items[0] == undefined ? '' : App.slmAR_LTTContract.selected.items[0].data.LTTContractNbr);
    }
    HQ.grid.checkInsertKey(App.grdAR_LTTContractDetail, e, keysTab_41);
    frmChange();
};

var grdAR_LTTContractDetail_ValidateEdit = function (item, e) {
};

var grdAR_LTTContract_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdAR_LTTContract);
    frmChange();
};

/////////////////////////////// GIRD AR_CustAdvTool /////////////////////////////////
var stoAR_CustAdvTool_Load = function (sto) {
    //HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            //HQ.store.insertBlank(sto, keysTab_5);
            HQ.store.insertRecord(sto, keysTab_5, { FitupDate: HQ.bussinessDate });
        }
        //HQ.isFirstLoad = false; //sto load cuoi se su dung
    }

    var TypeAdv = [];
    App.cboTypeADV.getStore().data.each(function (item) {
        if (TypeAdv.indexOf(item.data.Code) == -1) {
            TypeAdv.push([item.data.Code, item.data.Descr]);
        }
    });
    filterFeature = App.grdAR_CustAdvTool.filters;
    colAFilter = filterFeature.getFilter('Type');
    colAFilter.menu = colAFilter.createMenu({
        options: TypeAdv
    });

    App.stoAR_CustSellingProducts.reload();
    frmChange();
};

var grdAR_CustAdvTool_BeforeEdit = function (editor, e) {
    if (App.cboStatus.getValue() != 'H' && !HQ.IsEditAllStatus) return false;
    //if (!HQ.grid.checkBeforeEdit(e, keysTab_5)) return false;
};

var grdAR_CustAdvTool_Edit = function (item, e) {
    if (e.field == 'Type') {
        if (e.value) {
            var objType = App.cboTypeADVAR20400_pcCustAdvToolType.findRecord(['Code'], [e.value]);
            if (objType) {
                e.record.set('Descr', objType.data.Descr);
                e.record.set('FitupDate', HQ.bussinessDate);
            }
        }
    }
    checkInsertKey_grdAR_CustAdvTool(App.grdAR_CustAdvTool, e, keysTab_5);
    frmChange();
};

var checkInsertKey_grdAR_CustAdvTool = function (grd, e, keys) {
    if (keys.indexOf(e.field) != -1) {
        if (e.value != '')
            HQ.store.insertRecord(grd.getStore(), keys, { FitupDate: HQ.bussinessDate });
    }
};

var grdAR_CustAdvTool_ValidateEdit = function (item, e) {

};

var grdAR_CustAdvTool_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdAR_CustAdvTool);
    frmChange();
};

/////////////////////////////// GIRD AR_CustSellingProducts /////////////////////////////////
var stoAR_CustSellingProducts_Load = function (sto) {
    //HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keysTab_7);
        }
        //HQ.isFirstLoad = false; //sto load cuoi se su dung
    }
    frmChange();
    App.stoAR_CustDisplayMethod.reload();
};

var grdAR_CustSellingProducts_BeforeEdit = function (editor, e) {
    if (App.cboStatus.getValue() != 'H' && !HQ.IsEditAllStatus) return false;
    if (!HQ.grid.checkBeforeEdit(e, keysTab_7)) return false;
};

var grdAR_CustSellingProducts_Edit = function (item, e) {
    if (e.field == 'Code') {
        if (e.value) {
            var objSellingProd = App.cboSellingProd1AR20400_pcSellingProducts.findRecord(['SellingProd'], [e.value]);
            if (objSellingProd) {
                e.record.set('Descr', objSellingProd.data.Descr);
            }
        }
    }
    HQ.grid.checkInsertKey(App.grdAR_CustSellingProducts, e, keysTab_7);
    frmChange();
};

var grdAR_CustSellingProducts_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdAR_CustSellingProducts, e, keysTab_7, false);
};

var grdAR_CustSellingProducts_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdAR_CustSellingProducts);
    frmChange();
};

/////////////////////////////// GIRD AR_CustDisplayMethod /////////////////////////////////
var stoAR_CustDisplayMethod_Load = function (sto) {
    //HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keysTab_6);
        }
    }
    frmChange();
    App.stoAR_CustomerChild.reload();
};

var grdAR_CustDisplayMethod_BeforeEdit = function (editor, e) {
    if (App.cboStatus.getValue() != 'H' && !HQ.IsEditAllStatus) return false;
    if (!HQ.grid.checkBeforeEdit(e, keysTab_6)) return false;
};

var grdAR_CustDisplayMethod_Edit = function (item, e) {
    if (e.field == 'DispMethod') {
        if (e.value) {
            var objDisplayMethod = App.cboCodeDisplayAR20400_pcDisplayMethod.findRecord(['DispMethod'], [e.value]);
            if (objDisplayMethod) {
                e.record.set('Descr', objDisplayMethod.data.Descr);
            }
        }
    }
    HQ.grid.checkInsertKey(App.grdAR_CustDisplayMethod, e, keysTab_6);
    frmChange();
};

var grdAR_CustDisplayMethod_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdAR_CustDisplayMethod, e, keysTab_6, false);
};

var grdAR_CustDisplayMethod_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdAR_CustDisplayMethod);
    frmChange();
};


/////////////////////////////// GIRD AR_CustomerChild /////////////////////////////////
var stoAR_CustomerChild_Load = function (sto) {
    //HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, ["CustChildID"]);
        }
        HQ.isFirstLoad = false; //sto load cuoi se su dung
    }
    frmChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
    App.stoCompetitor.reload();
};

var grdAR_CustomerChild_BeforeEdit = function (editor, e) {
    if (App.cboStatus.getValue() != 'H' && !HQ.IsEditAllStatus) return false;
    if (!HQ.grid.checkBeforeEdit(e, ["CustChildID"])) return false;
};

var grdAR_CustomerChild_Edit = function (item, e) {
    if (e.field == 'CustChildID') {
        if (e.value) {
            var objDisplayMethod = App.cboCustChildID.store.findRecord(['CustId'], [e.value]);
            if (objDisplayMethod) {
                e.record.set('CustName', objDisplayMethod.data.CustName);
            }
        }
    }
    HQ.grid.checkInsertKey(App.grdAR_CustomerChild, e, ["CustChildID"]);
    frmChange();
};

var grdAR_CustomerChild_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdAR_CustomerChild, e, ["CustChildID"],false);
};

var grdAR_CustomerChild_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdAR_CustomerChild);
    frmChange();
};

//Process
var save = function () {
    if (App.frmMain.isValid()) {
        App.stoAR_LTTContract.clearFilter();
        _lstAR_LTTContract = App.stoAR_LTTContract.getRecordsValues();
        App.stoAR_LTTContractDetail.clearFilter();
        _lstAR_LTTContractDetail = App.stoAR_LTTContractDetail.getRecordsValues();

        App.frmMain.updateRecord();
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'AR20400/Save',
            params: {
                NodeID: _nodeID,
                NodeLevel: _nodeLevel,
                ParentRecordID: _parentRecordID,
                HiddenTree: _hiddenTree,
                lstAR_Customer: Ext.encode(App.stoAR_Customer.getRecordsValues()),
                lstAR_CustAdvTool: HQ.store.getData(App.stoAR_CustAdvTool),
                lstAR_CustSellingProducts: HQ.store.getData(App.stoAR_CustSellingProducts),
                lstAR_CustDisplayMethod: HQ.store.getData(App.stoAR_CustDisplayMethod),
                lstAR_CustomerChild: HQ.store.getData(App.stoAR_CustomerChild),
                lstAR_LTTContract: Ext.encode(_lstAR_LTTContract),
                lstAR_LTTContractDetail: Ext.encode(_lstAR_LTTContractDetail)
            },
            success: function (msg, data) {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                HQ.message.show(201405071);
                CustId = data.result.CustId;
                HQ.common.showBusy(true, HQ.common.getLang("WaitMsg"));
                App.cboCustId.getStore().reload({
                    callback: function () {
                        //ReloadTree('save', CustId);
                        var allData = App.cboCustId.store.snapshot || App.cboCustId.store.allData || App.cboCustId.store.data;      
                        App.cboCustChildID.store.loadData(allData.items);
                        if (Ext.isEmpty(App.cboCustId.getValue())) {
                            if (_hiddenTree == 'false' && data.result.isNew == 'true')
                                ReloadTree('save', CustId);
                            else
                            {
                                App.cboCustId.forceSelection = false;
                                var objPageCust = findRecordCombo(data.result.CustId);
                                if (objPageCust) {
                                    var positionCust = calcPage(objPageCust.index);
                                    App.cboCustId.loadPage(positionCust);
                                }
                                App.cboCustId.setValue(CustId);
                                if(!App.stoAR_Customer.loading)
                                App.stoAR_Customer.reload();
                            }
                           
                        }
                        else {
                            //var custNameTmp = App.cboCustId.getValue() + '-' + App.txtCustName.getValue();
                            if (_hiddenTree == 'false' && data.result.isNew == 'true')
                                ReloadTree('save', CustId);
                            else
                            {
                                App.cboCustId.forceSelection = false;
                                var objPageCust = findRecordCombo(CustId);
                                if (objPageCust) {
                                    var positionCust = calcPage(objPageCust.index);
                                    App.cboCustId.loadPage(positionCust);
                                }
                                //App.slmtreeCust.selected.items[0].set('text', custNameTmp);
                                App.cboCustId.setValue(CustId);
                                // App.stoAR_Customer.reload();

                                ReloadTree('save', CustId);
                            }
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

var deleteData = function (item) {
    if (item == "yes") {
        if (HQ.focus == 'header') {
            if (App.frmMain.isValid()) {
                App.frmMain.updateRecord();
                App.frmMain.submit({
                    waitMsg: HQ.common.getLang("DeletingData"),
                    url: 'AR20400/DeleteAll',
                    timeout: 7200,
                    success: function (msg, data) {
                        if (_hiddenTree == 'false')
                            ReloadTree('delete');
                        else {
                            CustId = '';
                            App.cboCustId.getStore().reload();
                          
                            //menuClick("new");
                        }
                        
                    },
                    failure: function (msg, data) {
                        HQ.message.process(msg, data, true);
                    }
                });
            }
        }
        else if (HQ.focus == 'grdAR_LTTContract') {
            App.grdAR_LTTContract.deleteSelected();
            frmChange();
        }
        else if (HQ.focus == 'grdAR_LTTContractDetail') {
            App.grdAR_LTTContractDetail.deleteSelected();
            frmChange();
        }
        else if (HQ.focus == 'grdAR_CustAdvTool') {
            App.grdAR_CustAdvTool.deleteSelected();
            frmChange();
        }
        else if (HQ.focus == 'grdAR_CustSellingProducts') {
            App.grdAR_CustSellingProducts.deleteSelected();
            frmChange();
        }
        else if (HQ.focus == 'grdAR_CustDisplayMethod') {
            App.grdAR_CustDisplayMethod.deleteSelected();
            frmChange();
        }
        else if (HQ.focus == 'grdAR_CustomerChild') {
            App.grdAR_CustomerChild.deleteSelected();
            frmChange();
        }
    }
};

//var btnShowReport_Click = function () {
//    if (App.cboReport.validate()) {
//        App.frmMain.submit({
//            waitMsg: HQ.waitMsg,
//            method: 'POST',
//            url: 'AR20400/Report',
//            timeout: 180000,
//            params: {
//                reportNbr: App.cboReport.valueModels[0].data.ReportNbr,
//                reportName: App.cboReport.valueModels[0].data.ReportName
//            },
//            success: function (msg, data) {
//                if (this.result.reportID != null) {
//                    window.open('Report?ReportName=' + this.result.reportName + '&_RPTID=' + this.result.reportID, '_blank');
//                }
//                App.winReport.close();
//                HQ.message.process(msg, data, true);
//            },
//            failure: function (msg, data) {
//                HQ.message.process(msg, data, true);
//            }
//        });
//    }

//}

function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        if(!App.stoAR_Customer.loading)
        App.stoAR_Customer.reload();
    }
};
/////

var renderCustomer = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboCustomerAR20400_pcCustomer.findRecord("CustId", rec.data.CustID);
    if (record) {
        return record.data.CustName;
    }
    else {
        return value;
    }
};

var renderClassID = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboClassIDAR20400_pcPriceClass.findRecord("PriceClassID", rec.data.ClassID);
    if (record) {
        return record.data.Descr;
    }
    else {
        return value;
    }
};

var renderBranchName = function (value, metaData, rec, rowIndex, colIndex, store) {
    var record = App.cboBranchIDAR20400_pcCompany.findRecord("CpnyID", rec.data.CpnyID);
    if (record) {
        return record.data.CpnyName;
    }
    else {
        return value;
    }
};

var filterCate = function () {
    App.cboCate.store.reload();
    //App.cboCate.store.clearFilter();
    //App.cboCate.store.filter('TypeID', App.cboCateType.getValue());
};

////////////////////////// Event Change & Select Control//////////////////////
var txtCustName_Change = function (sender, value) {
    App.lblName.setValue(this.getValue());
    //var regex = /</[a-zA-Z][\s\S]*>/
    //if (value.match(regex)) {
    //    alert("Error");
    //}
};

var cboCustId_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoAR_Customer.loading) {
        if (App.cboCustId.allowBlank == false && HQ.isChange == true) {
            HQ.message.show(150, '', '');
            sender.setValue(sender.originalValue);
            return;
        }
        CustId = value;
        App.stoAR_Customer.reload();
        App.cboDfltShipToId.setValue('');
        App.cboDfltShipToId.store.reload();
        App.cboLTTContract.setValue('');
        App.cboLTTContract.store.reload();
        //if(_hiddenTree == 'false' && value)
        //    searchNode();
    }
};

var cboCustId_Select = function (sender, value) {
    HQ.isFirstLoad = true;
    if (sender.valueModels != null && !App.stoAR_Customer.loading) {
        if (App.cboCustId.allowBlank == false && HQ.isChange == true) {
            HQ.message.show(150, '', '');
            sender.setValue(sender.originalValue);
            return;
        }
        CustId = value;
        App.stoAR_Customer.reload();
        App.cboDfltShipToId.setValue('');
        App.cboDfltShipToId.store.reload();
        App.cboLTTContract.setValue('');
        App.cboLTTContract.store.reload();
        //if(_hiddenTree == 'false' && value)
        //    searchNode();
    }
};

var cboCustId_TriggerClick = function (sender, value) {
    if (App.cboCustId.allowBlank == false && HQ.isChange == true) {
        HQ.message.show(150, '', '');
        return;
    }
    App.cboCustId.clearValue();
    CustId = '';
};

var cboCpnyID_Change = function (sender, value) {
    HQ.isFirstLoad = true;
    CustId = '';
    if (sender.valueModels != null && !App.stoAR_Customer.loading) {
        App.cboCustId.setValue('');
        App.cboCustId.store.reload();
        App.cboCustChildID.store.reload();
        App.cboSlsperId.setValue('');
        App.cboSlsperId.store.reload();
        App.cboDeliveryID.setValue('');
        App.cboDeliveryID.store.reload();
        App.cboSupID.setValue('');
        App.cboSupID.store.reload();
        App.cboSiteId.setValue('');
        App.cboSiteId.store.reload();
        App.cboDfltSalesRouteID.setValue('');
        App.cboDfltSalesRouteID.store.reload();

        App.stoCheckAutoCustID.reload();
        App.stoCheckHiddenTree.reload();
    }
};

var cboCpnyID_Select = function (sender, value) {
    HQ.isFirstLoad = true;
    CustId = '';
    if (sender.valueModels != null && !App.stoAR_Customer.loading) {
        App.cboCustId.setValue('');
        App.cboCustId.store.reload();
        App.cboCustChildID.store.reload();
        App.cboSlsperId.setValue('');
        App.cboSlsperId.store.reload();
        App.cboDeliveryID.setValue('');
        App.cboDeliveryID.store.reload();
        App.cboSupID.setValue('');
        App.cboSupID.store.reload();
        App.cboSiteId.setValue('');
        App.cboSiteId.store.reload();
        App.cboDfltSalesRouteID.setValue('');
        App.cboDfltSalesRouteID.store.reload();

        App.stoCheckAutoCustID.reload();
        App.stoCheckHiddenTree.reload();
    }
};

var cboCountry_Change = function (sender, value) {
    if (sender.valueModels != null && !App.stoAR_Customer.loading) {
        App.cboState.setValue('');
        App.cboCity.setValue('');
        App.cboDistrict.setValue('');
    }
};

var cboCountry_Select = function (sender, value) {
    if (sender.valueModels != null && !App.stoAR_Customer.loading) {
        App.cboState.setValue('');
        App.cboCity.setValue('');
        App.cboDistrict.setValue('');
    }
};

var btnCopytoDiffDB_Click = function () {
    App.txtBillName.setValue(App.lblName.getValue());
    App.txtBillAttn.setValue(App.txtAttn.getValue());
    App.txtBillSalut.setValue(App.txtSalut.getValue());
    App.txtBillAddr1.setValue(App.txtAddr1.getValue());
    App.txtBillAddr2.setValue(App.txtAddr2.getValue());
    App.cboBillCountry.setValue(App.cboCountry.getValue());
    App.cboBillState.setValue(App.cboState.getValue());
    App.cboBillCity.setValue(App.cboCity.getValue());
    App.cboBillDistrict.setValue(App.cboDistrict.getValue());
    App.txtBillZip.setValue(App.txtZip.getValue());
    App.txtBillPhone.setValue(App.txtPhone.getValue());
    App.txtBillFax.setValue(App.txtFax.getValue());
    App.txtBillWard.setValue(App.txtWard.getValue());
};

var btnCopy = function () {

};

var cboClassId_Change = function (sender,e) {
    if (sender.valueModels != null && sender.hasFocus) {
        var objTmp = sender.valueModels[0];
        if (objTmp) {
            App.cboCountry.setValue(objTmp.data.Country);
            App.cboState.setValue(objTmp.data.State);
            App.cboCity.setValue(objTmp.data.City);
            App.cboDistrict.setValue(objTmp.data.District);
            App.cboPriceClassID.setValue(objTmp.data.PriceClass);
            App.cboTerms.setValue(objTmp.data.Terms);
            App.cboTerritory.setValue(objTmp.data.Territory);
            App.txtTradeDisc.setValue(objTmp.data.TradeDisc);
            App.cboTaxDflt.setValue(objTmp.data.TaxDflt);
            App.cboTaxID00.setValue(objTmp.data.TaxID00);
            App.cboTaxID01.setValue(objTmp.data.TaxID01);
            App.cboTaxID02.setValue(objTmp.data.TaxID02);
            App.cboTaxID03.setValue(objTmp.data.TaxID03);
        }
    }
};

var cboClassId_Select = function (sender, e) {
    if (sender.valueModels != null && sender.hasFocus) {
        var objTmp = sender.valueModels[0];
        if (objTmp) {
            App.cboCountry.setValue(objTmp.data.Country);
            App.cboState.setValue(objTmp.data.State);
            App.cboCity.setValue(objTmp.data.City);
            App.cboDistrict.setValue(objTmp.data.District);
            App.cboPriceClassID.setValue(objTmp.data.PriceClass);
            App.cboTerms.setValue(objTmp.data.Terms);
            App.cboTerritory.setValue(objTmp.data.Territory);
            App.txtTradeDisc.setValue(objTmp.data.TradeDisc);
            App.cboTaxDflt.setValue(objTmp.data.TaxDflt);
            App.cboTaxID00.setValue(objTmp.data.TaxID00);
            App.cboTaxID01.setValue(objTmp.data.TaxID01);
            App.cboTaxID02.setValue(objTmp.data.TaxID02);
            App.cboTaxID03.setValue(objTmp.data.TaxID03);
        }
    }
};

var cboClassId_TriggerClick = function (sender, e) {
    App.cboClassId.setValue('');
    App.cboCountry.setValue('');
    App.cboState.setValue('');
    App.cboCity.setValue('');
    App.cboDistrict.setValue('');
    App.cboPriceClassID.setValue('');
    App.cboTerms.setValue('');
    App.cboTerritory.setValue('');
    App.txtTradeDisc.setValue('');
    App.cboTaxDflt.setValue('');
    App.cboTaxID00.setValue('');
    App.cboTaxID01.setValue('');
    App.cboTaxID02.setValue('');
    App.cboTaxID03.setValue('');
}

var cboSlsperId_Change = function (sender, e) {
    App.cboOunit.store.reload();
    if (sender.hasFocus) {
        App.cboOunit.setValue('');
    }
}

var cboTerritory_Change_Select = function (sender, e) {
    if (sender.hasFocus) {
        App.cboState.setValue('');
        App.cboCity.setValue('');
        App.cboDistrict.setValue('');
        App.cboSubTerritory.setValue('');
    }
  
};
var cboSubTerritory_Focus = function (sender, e) {
    var code = App.cboTerritory.getValue();
    App.cboSubTerritory.store.clearFilter();
    App.cboSubTerritory.store.filter("Territory", code);
    App.cboSubTerritory.forceSelection = true;
}
var cboShopType_Focus = function (sender, e) {
    var code = App.cboChannel.getValue();
    App.cboShopType.store.clearFilter();
    App.cboShopType.store.filter("Channel", code);
    App.cboShopType.forceSelection = true;
}

var cboChannel_Change_Select = function (sender, e) {
    if (sender.hasFocus) {
        App.cboShopType.setValue('');     
    }
    var code = App.cboChannel.getValue();
    App.cboShopType.store.clearFilter();
    App.cboShopType.store.filter("Channel", code);
};
var cboStatus_Change = function (sender, e)
{
    App.cboHandle.store.reload();
    if (App.cboStatus.value == "H" || HQ.IsEditAllStatus) {
        App.cboDistrict.allowBlank = true;
    }
    else {
        App.cboDistrict.allowBlank = false;
    }
}

var filterComboSate = function (sender, e) {
    if (sender.hasFocus) {
        App.cboState.setValue('');
        App.cboCity.setValue('');
        App.cboDistrict.setValue('');
        App.txtAddr.setValue('');
    }
    
    var code = App.cboCountry.getValue();
    App.cboState.store.clearFilter();
    App.cboState.store.filter("Country", code);
    if (sender.valueModels != null) {
        if (sender.valueModels.length > 0) {
            _country = sender.valueModels[0].data.Descr;
        }
        else {
            _country = "";
        }
    }    
    var tam = _addr1 + ", " + _ward + ", " + _district + ", " + _state + ", " + _country;
    App.txtAddr.setValue(tam);
    
    
};

var filterComboCityDistrict = function (sender, e) {
    if (sender.hasFocus) {
        App.cboCity.setValue('');
        App.cboDistrict.setValue('');
    }
    var code = App.cboCountry.getValue() + App.cboState.getValue();
    App.cboCity.store.clearFilter();
    App.cboCity.store.filter("CountryState", code);
    App.cboDistrict.store.clearFilter();
    App.cboDistrict.store.filter("CountryState", code);
    if (sender.valueModels != null) {
        if (sender.valueModels.length > 0) {
            _state = sender.valueModels[0].data.Descr;
        }
        else {
            _state = '';
        }
    }
    
    var tam = _addr1 + ", " + _addr2 + ", " + _ward + ", " + _district + ", " + _state + ", " + _country;
    App.txtAddr.setValue(tam);
};

var cboDistrict_Change = function (sender, e) {
    if (sender.valueModels != null) {
        if (sender.valueModels.length > 0) {
            _district = sender.valueModels[0].data.Name;
        }
        else {
            _district = '';
        }
    }    
    var tam = _addr1 + ", " + _addr2 + ", " + _ward + ", " + _district + ", " + _state + ", " + _country;
    App.txtAddr.setValue(tam);
}
var txtAddr1_Change = function (sender, e) {
    _addr1 = App.txtAddr1.getValue();
    var tam = _addr1 + ", " + _addr2 + ", " + _ward + ", " + _district + ", " + _state + ", " + _country;
    App.txtAddr.setValue(tam);
}

var txtAddr2_Change = function (sender, e) {
    _addr2 = App.txtAddr2.getValue();
    var tam = _addr1 + ", " + _addr2 + ", " + _ward + ", " + _district + ", " + _state + ", " + _country;
    App.txtAddr.setValue(tam);
}
var txtBillAddr1_Change = function (sender, e) {
    _addr1Bill = App.txtBillAddr1.getValue();
    var tam = _addr1Bill + ", " + _addr2Bill + ", " + _wardBill + ", " + _districtBill + ", " + _stateBill + ", " + _countryBill;
    App.txtBillAddr.setValue(tam);
}
var txtBillAddr2_Change = function (sender, e) {
    _addr2Bill = App.txtBillAddr2.getValue();
    var tam = _addr1Bill + ", " + _addr2Bill + ", " + _wardBill + ", " + _districtBill + ", " + _stateBill + ", " + _countryBill;
    App.txtBillAddr.setValue(tam);
}


var txtWard_Change = function (sender, e) {
    _ward = App.txtWard.getValue();
    var tam = _addr1 + ", " + _addr2 + ", " + _ward + ", " + _district + ", " + _state + ", " + _country;
    App.txtAddr.setValue(tam);
    
}

var txtBillWard_Change = function (sender, e) {
    _wardBill = App.txtBillWard.getValue();
    var tam = _addr1Bill + ", " + _addr2Bill + ", " + _wardBill + ", " + _districtBill + ", " + _stateBill + ", " + _countryBill;
    App.txtBillAddr.setValue(tam);

}

var filterComboBillSate = function (sender, e) {
    if (sender.hasFocus) {
        App.cboBillState.setValue('');
        App.cboBillCity.setValue('');
    }
        
    var code = App.cboBillCountry.getValue();
    App.cboBillState.store.clearFilter();
    App.cboBillState.store.filter("Country", code);
    if (sender.valueModels != null) {
        if (sender.valueModels.length > 0) {
            _countryBill = sender.valueModels[0].data.Descr;
        }
        else {
            _countryBill = "";
        }
    }
    var tam = _addr1Bill + ", " + _addr2Bill + ", " + _wardBill + ", " + _districtBill + ", " + _stateBill + ", " + _countryBill;
    App.txtBillAddr.setValue(tam);

};

var filterComboBillCity = function (sender, e) {
    if (sender.hasFocus) {
        App.cboBillCity.setValue('');
        App.cboBillDistrict.setValue('');
    }
    var code = App.cboBillCountry.getValue() + App.cboBillState.getValue();
    App.cboBillCity.store.clearFilter();
    App.cboBillCity.store.filter("CountryState", code);
    App.cboBillDistrict.store.clearFilter();
    App.cboBillDistrict.store.filter("CountryState", code);
    if (sender.valueModels != null) {
        if (sender.valueModels.length > 0) {
            _stateBill = sender.valueModels[0].data.Descr;
        }
        else {
            _stateBill = "";
        }
    }
    var tam = _addr1Bill + ", " + _addr2Bill + ", " + _wardBill + ", " + _districtBill + ", " + _stateBill + ", " + _countryBill;
    App.txtBillAddr.setValue(tam);
};


var cboBillDistrict_Change = function (sender, e) {
    if (sender.valueModels != null) {
        if (sender.valueModels.length > 0) {
            _districtBill = sender.valueModels[0].data.Name;
        }
        else {
            _districtBill = '';
        }
    }
    var tam = _addr1Bill + ", " + _addr2Bill + ", " + _wardBill + ", " + _districtBill + ", " + _stateBill + ", " + _countryBill;
    App.txtBillAddr.setValue(tam);
}

var stoCheckAutoCustID_Load = function () {
    if (App.stoCheckAutoCustID.data.items[0].data.Flag == '1') {
        _Flag = "1";
        App.cboCustId.allowBlank = true;
        App.cboCustId.isValid(false);
        App.cboCustId.forceSelection = true;
    }
    else {
        _Flag = "0";
        App.cboCustId.allowBlank = false;
        App.cboCustId.isValid(true);
        App.cboCustId.forceSelection = false;
    }
};

var cboCustId_Focus = function () {
    if (_Flag == "1") {
        App.cboCustId.forceSelection = true;
    }
    else {
        if (HQ.isNew == true)
            App.cboCustId.forceSelection = false;
        else
            App.cboCustId.forceSelection = true;
    }
    if (!HQ.isInsert && HQ.isNew)
        App.cboCustId.forceSelection = true;
};

var stoGetMaxHierarchyLevel_Load = function () {
    if (App.stoGetMaxHierarchyLevel.data.items[0]) {
        _maxLevel = App.stoGetMaxHierarchyLevel.data.items[0].data.NodeLevel;
    }
};

var stoCheckHiddenTree_Load = function () {
    if (App.stoCheckHiddenTree.data.items[0]) {
        if (App.stoCheckHiddenTree.data.items[0].data.HiddenHierarchy == true) {
            _hiddenTree = 'true';
            App.pnlWest.collapse();
        }
        else {
            _hiddenTree = 'false';
            App.pnlWest.expand();
            App.stoGetMaxHierarchyLevel.reload();
            ReloadTree();
        }
    }
    else {
        if (App.cboCpnyID.getValue()) {
            HQ.message.show(2016030901, '', '');
        }
    }
};

var cboHandle_Change = function (sender, value) {
    if (value == "I") {
        setAllowBank_InActive(false);
        if (HQ.isUpdate) {
            App.txtInActive.setReadOnly(false);
        }
    }
    else {
        setAllowBank_InActive(true);
        if (App.cboStatus.getValue() != 'H' && !HQ.IsEditAllStatus)
            App.txtInActive.setReadOnly(true);
    }
};

var setAllowBank_InActive = function (value) {
    App.txtInActive.allowBlank = value;
    App.txtInActive.isValid(!value);
};

///////////////////////// Tree ///////////////////////////
var btnExpand_click = function (btn, e, eOpts) {
    //App.treeCust.expandAll();
    Ext.suspendLayouts();
    _treeExpandAll = true;
    expandAll(App.treeCust);
    Ext.resumeLayouts(true);
};

var btnCollapse_click = function (btn, e, eOpts) {
    //App.treeCust.collapseAll();
    collapseAll(App.treeCust);
};

var nodeSelected_Change = function (store, operation, options) {
   
    if (operation.internalId != 'root') {
        _root = 'false';
        var CustID1 = '';
        _nodeID = operation.raw.NodeID;
        _nodeLevel = operation.raw.NodeLevel;
        _parentRecordID = operation.raw.ParentRecordID;
        _recordID = operation.raw.RecordID;
        CustID1 = operation.raw.CustID;
        //_leaf = operation.data.leaf;
        //parentRecordIDAll = operation.internalId.split("-");
        //if (parentRecordIDAll[1] != '|') {
        //    _nodeID = parentRecordIDAll[0];
        //    _nodeLevel = parentRecordIDAll[1];
        //    _parentRecordID = parentRecordIDAll[2];
        //    _recordID = parentRecordIDAll[3];
        //} else {
        //    parentRecordIDAll = operation.data.parentId.split("-");
        //    _nodeID = parentRecordIDAll[0];
        //    _nodeLevel = parentRecordIDAll[1];
        //    _parentRecordID = parentRecordIDAll[2];
        //    _recordID = parentRecordIDAll[3];
        //    var custIDall = operation.data.id.split("-");
        //    CustID1 = custIDall[0];
        //    //CustId = custIDall[0];
        //}
    } else {
        _root = 'true';
        _nodeID = '';
        _nodeLevel = '1';
        _parentRecordID = '0';
        _recordID = '0';
    }

    if (CustID1) {
        if (HQ.isChange && (CustID1 != CustId)) {
            HQ.message.show(150);
            var objRecord = App.treeCust.getRootNode().findChild('id', CustId + '-|', true);
            if (objRecord)
                App.treeCust.getSelectionModel().select(objRecord);
        }
        else {
            CustId = CustID1;
            App.cboCustId.forceSelection = false;
            var objPageCust = findRecordCombo(CustId);
            if (objPageCust)
            {
                var positionCust = calcPage(objPageCust.index);
                App.cboCustId.loadPage(positionCust);
            }
            App.cboCustId.setValue(CustID1);
        }
    }
    
};

//var stoCustId_Load = function () {
//    //App.cboCustId.setValue(CustId);
//};

var findRecordCombo = function (value) {
    var data = null;
    var store = App.cboCustId.store;
    var allRecords = store.snapshot || store.allData || store.data;
    allRecords.each(function (record) {
        if (record.data.CustId == value) {
            data = record;
            return false;
        }
    });
    return data;
};
    
var ReloadTree = function (type, valueCustId) {
    try {       
        _treeExpandAll = false;
        
        App.direct.ReloadTreeAR20400(App.cboCpnyID.getValue(), {            
            success: function (data) {
                if (type == 'save') {
                    App.cboCustId.forceSelection = false;
                    var objPageCust = findRecordCombo(valueCustId);
                    if (objPageCust) {
                        var positionCust = calcPage(objPageCust.index);
                        App.cboCustId.loadPage(positionCust);
                    }
                    App.cboCustId.setValue(valueCustId);
                    if(!App.stoAR_Customer.loading)
                    App.stoAR_Customer.reload();
                }
                else if (type =='delete')
                {
                    CustId = '';
                    App.cboCustId.getStore().reload();
                }
            },
            failure: function () {
                alert("fail");
            },
            //eventMask: { msg: 'loadingTree', showMask: true }
        });
    } catch (ex) {
        alert(ex.message);
    }
};

var searchNode = function () {
    Ext.suspendLayouts();
    var objRecord = App.treeCust.getRootNode().findChild('id', App.cboCustId.getValue() + '-|', true);
    if (_treeExpandAll == false)
        collapseAll(App.treeCust);
    if (objRecord) {
        App.treeCust.getSelectionModel().deselectAll();
        App.treeCust.getRootNode().expand();
        expandParentNode(objRecord);
        App.treeCust.getSelectionModel().select(objRecord, true);
    }
    Ext.resumeLayouts();
};
var node_Expand = function (item) {
    Ext.suspendLayouts();
    App.treeCust.view.focusRow(App.slmtreeCust.getSelection()[0]);
    Ext.resumeLayouts();
};
var expandParentNode = function (node) {
    Ext.suspendLayouts();
    var parentNode = node.parentNode;
    if (parentNode) {
           
        expandParentNode(parentNode);
        parentNode.expand();
    }
    Ext.resumeLayouts();
};

var tabDetail_Change = function (tabPanel, newCard, oldCard, eOpts) {
    HQ.focus = tabPanel.activeTab.id;
};

var calcPage = function (value) {
    var tmpValue = (Number(value) + 1) / 20;
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
    Ext.suspendLayouts();
    this.updateTreeView(tree, function(root) {
        root.cascadeBy(function(node) {
            if (!node.isRoot() || tree.rootVisible) {
                node.data.expanded = false;
            }
        });
        return tree.rootVisible ? [root] : root.childNodes;
    });
    Ext.resumeLayouts();
};

var expandAll = function (tree) {
    Ext.suspendLayouts();
    App.treeCust.getRootNode().expand(true);    
    Ext.resumeLayouts(true);
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
var displayImage = function (imgControl, fileName) {
    Ext.Ajax.request({
        url: 'AR20400/ImageToBin',
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


