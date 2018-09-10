var _Change = false;
var keys = ['ID'];
var _firstLoad = true;
var _hideColumn = ['SalesRouteID', 'SlsFreq', 'WeekofVisit', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun', 'VisitsPerDay'];
var _lockColumn = ['WeekofVisit', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

var hideEditCustColumn = ['EditInfo', 'EditBusinessPic', 'EditProfilePic'];
var _isEditReason = 0;
var _slsperID = '';
var _slsFreq = '';
var _isditContactName = false;
var _fromDate;
var _toDate;
var _dis = "";
var frmMain_BoxReady = function () {
    var date = new Date(HQ.LimitedYear, 0, 0);
    App.cboBrandID.store.reload();
    App.cboColSlsFreq1.store.reload();
    App.cboSizeID.store.reload();
    App.cboDisplayID.store.reload();
    App.cboStandID.store.reload();
    if (HQ.ShowExport) App.btnExport.hide();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    if (HQ.isShowCustHT) HQ.grid.show(App.grdCust, ['CustHT']);
    if (HQ.IsShowERPCust) HQ.grid.show(App.grdCust, ['ERPCustID']);
    if (HQ.showDisplayID) HQ.grid.show(App.grdCust, ['DisplayID']);
    if (HQ.showStandID) HQ.grid.show(App.grdCust, ['StandID']);
    if (HQ.showBrandID) HQ.grid.show(App.grdCust, ['BrandID']);
    if (HQ.showSizeID) HQ.grid.show(App.grdCust, ['SizeID']);
    if (HQ.showTaxCode) HQ.grid.show(App.grdCust, ['TaxCode']);
    if (HQ.hideMarketRoute) HQ.grid.hide(App.grdCust, ['Market']);
    if (HQ.showTypeCabinnets) {
        HQ.grid.show(App.grdCust, ['TypeCabinetsDescr']);
    }
    if (HQ.showVisitsPerDay) {
        HQ.grid.show(App.grdCust, ['VisitsPerDay']);
    }

    if (HQ.showClassCust) {
        HQ.grid.show(App.grdCust, ['ClassCust']);
    }
    else {
        HQ.grid.hide(App.grdCust, ['ClassCust']);
    }

    showSubRoute(0);
    App.stoAR20500_pdSubRoute.reload();
    App.stoAR20500_pdWeekofVisitAll.load(function () {
        App.cboTerritory.getStore().load(function () {
            App.cboCpnyID.getStore().load(function () {
                App.cboSlsperId.getStore().load(function () {
                    App.cboStatus.getStore().load(function () {
                        App.cboHandle.getStore().load(function () {
                            HQ.common.showBusy(false, HQ.common.getLang("loadingData"));
                            if (_firstLoad) {
                                App.cboStatus.setValue("H");
                                //App.cboHandle.setValue("N");
                                _firstLoad = false;
                                App.FromDate.setValue(HQ.bussinessDate);
                                App.ToDate.setValue(HQ.bussinessDate);
                            }
                        })
                    })
                })
            })
        })
    });
    setHideControls();
    if (HQ.isShowReason) {
        HQ.grid.show(App.grdCust, ['Reason']);
    } else {
        HQ.grid.hide(App.grdCust, ['Reason']);
    }
    if (!HQ.showVisitsPerDay)
        HQ.grid.hide(App.grdCust, ['VisitsPerDay']);
    App.clmOUnit.setVisible(HQ.showOUnit > 0);
    App.clmMobile.setVisible(HQ.showMobile > 0);
    App.dteFromDate.setMaxValue(date);
    App.dteToDate.setMaxValue(date);
    App.FromDate.setMaxValue(date);
    App.ToDate.setMaxValue(date);
    App.dtpDate1.setVisible(HQ.HideTime);
    App.dtpDate2.setVisible(HQ.HideTime);
};

var cboTerritory_Change = function (sender, e) {
    if (_Change == true) {
        HQ.message.show(20150303, '', 'refresh');
    } else {
        if (sender.valueModels != null && (!App.cboCpnyID.store.loading)) {
            App.cboCpnyID.store.reload();
        }
    }
};

var cboCpnyID_Change = function (sender, e) {
    if (_Change == true) {
        HQ.message.show(20150303, '', 'refresh');
    } else {
        if (sender.valueModels != null && (!App.cboSlsperId.store.loading || !App.cboColSalesRouteID.store.loading)) {
            App.grdCust.store.removeAll();
            App.cboSlsperId.store.reload();
            App.cboColSalesRouteID.store.reload();
        }
        //App.grdCust.removeAll();
    }
};

var cboCpnyID_Select = function (sender, e) {
    if (_Change == true) {
        HQ.message.show(20150303, '', 'refresh');
    } else {
        if (sender.valueModels != null && (!App.cboSlsperId.store.loading || !App.cboColSalesRouteID.store.loading)) {
            App.grdCust.store.removeAll();
            App.cboSlsperId.store.reload();
            App.cboColSalesRouteID.store.reload();
        }
        //App.grdCust.removeAll();
    }
};

var cboStatus_Change = function (value) {
    if (_Change == true) {
        HQ.message.show(20150303, '', 'refresh');
    } else {
        App.stoCust.rejectChanges();
        Gmap.Process.clearMap(Gmap.Declare.stopMarkers);

        //App.stoCust.removeAll();
        App.stoCust.load([], false);
        App.stoCust.removeAll();
        App.cboHandle.store.reload();
        //App.grdCust.removeAll();_b
    }    
    setTimeout(function () {
        setHideControls();
        
        App.grdCust.view.refresh();

    }, 10);
};


var btnLoad_Click = function () {
    if (HQ.form.checkRequirePass(App.frmMain)) {
        refresh('yes');
    }
};

var ColCheck_Header_Change = function (value) {
    
    if (value) {
        if (!checkEditDet()) {
            App.ColCheck_Header.events['change'].suspend();
            App.ColCheck_Header.setValue(!value.checked);
            App.ColCheck_Header.events['change'].resume();
            return false;
        }
        App.stoCust.suspendEvents();

        var allData = App.grdCust.store.snaptshot || App.grdCust.store.allData || App.grdCust.store.data;
        allData.each(function (item) {
            if (item.data.Status == App.cboStatus.getValue())
                item.set("ColCheck", value.checked);
        });
        App.stoCust.resumeEvents();
        App.grdCust.view.refresh();
        stoChanged(App.stoCust);
    }
};
var ColCheck_HeaderPG_Change = function (value) {

    if (value) {
        if (!checkEditDet()) {
            App.ColCheck_HeaderPG.events['change'].suspend();
            App.ColCheck_HeaderPG.setValue(!value.checked);
            App.ColCheck_HeaderPG.events['change'].resume();
            return false;
        }
        App.stoCustPG.suspendEvents();

        var allData = App.grdCustPG.store.snaptshot || App.grdCustPG.store.allData || App.grdCustPG.store.data;
        allData.each(function (item) {
            if (item.data.Status == App.cboStatus.getValue())
                item.set("ColCheck", value.checked);
        });
        App.stoCustPG.resumeEvents();
        App.grdCustPG.view.refresh();
        stoChanged(App.stoCustPG);
    }
};
var btnProcess_Click = function () {
    if (App.cboUpdateType.getValue() == 0 || App.cboUpdateType.getValue() == 1) {
        var count = 0;
        App.grdCust.store.clearFilter();
        var allData = App.grdCust.store.snaptshot || App.grdCust.store.allData || App.grdCust.store.data;
        for (var i = 0; i < allData.length ; i++) {
            var data = allData.items[i].data;
            if (data.ColCheck) {
                count++;
            }
        }
        if (count == 0) {
            HQ.message.show(718);
        }
        if (App.cboHandle.getValue() && count > 0) {
            var erroState = "";
            var erroDistrict = "";
            for (var i = 0; i < allData.length ; i++) {
                if (allData.items[i].data.ColCheck) {
                    if (allData.items[i].data.State == "" || allData.items[i].data.State == null) {
                        erroState = erroState + (allData.items[i].index + 1) + ",";
                    }
                    if (allData.items[i].data.District == "" || allData.items[i].data.District == null) {
                        erroDistrict = erroDistrict + (allData.items[i].index + 1) + ",";
                    }
                }
            }
            if (erroState != "") {
                HQ.message.show(2018041211, [HQ.common.getLang('State'), erroState], '', true);
                return;
            }
            else if (erroDistrict != "") {
                HQ.message.show(2018041211, [HQ.common.getLang('District'), erroDistrict], '', true);
                return;
            }

            if ((App.cboHandle.getValue() == 'A' || App.cboHandle.getValue() == 'O') && App.cboUpdateType.getValue() == 0) {

                var rowerror = '';
                var isnullclass = '';
                var isnullpriceclass = '';
                if (HQ.IsRequireRefCustID) {
                    for (var i = 0; i < allData.length ; i++) {
                        var data = allData.items[i].data;
                        if (data.ColCheck) {
                            if (Ext.isEmpty(data.ERPCustID)) {
                                HQ.message.show(1000, HQ.common.getLang('ERPCustID'), '');
                                rowerror += i + 1 + ',';
                                break;
                            }
                        }
                    }
                }
                if (rowerror != '') {
                    return;
                }
                var minDate = HQ.bussinessDate;
                var errorFreq = '';
                for (var i = 0; i < allData.length ; i++) {
                    var data = allData.items[i].data;
                    if (data.ColCheck) {
                        if (data.MinMCPDate < minDate) {
                            minDate = data.MinMCPDate;
                        }
                        if (Ext.isEmpty(data.SlsFreq)) {
                            rowerror = 'err';
                            HQ.message.show(201302071, (i + 1) + ' (' + HQ.grid.findColumnNameByIndex(App.grdCust.columns, 'SlsFreq') + ')', '', false);
                            //HQ.message.show(1000, HQ.grid.findColumnNameByIndex(App.grdCust.columns, 'SubRouteID'));
                            break;
                        }
                        if (HQ.showSubRoute && Ext.isEmpty(data.SubRouteID)) {
                            rowerror = 'err';
                            HQ.message.show(201302071, (i + 1) + ' (' + HQ.grid.findColumnNameByIndex(App.grdCust.columns, 'SubRouteID') + ')', '', false);
                            //HQ.message.show(1000, HQ.grid.findColumnNameByIndex(App.grdCust.columns, 'SubRouteID'));
                            break;
                        }
                        if (!isValidSel(data)) {
                            errorFreq += i + 1 + ',';
                        }
                        if (HQ.showVisitsPerDay && (data.VisitsPerDay > HQ.maxVisitPerDay || data.VisitsPerDay == 0)) {
                            HQ.message.show(2018010101, [(i + 1), HQ.grid.findColumnNameByIndex(App.grdCust.columns, 'VisitsPerDay'), HQ.maxVisitPerDay], '', true);
                            rowerror = 'err';
                            break;
                        }
                        if (HQ.showOUnit == 2 && (data.OUnit == '')) {
                            HQ.message.show(201302071, (i + 1) + ' (' + App.clmOUnit.text + ')', '', false);
                            rowerror = 'err';
                            break;
                        }
                        if (HQ.showMobile == 2 && (data.Mobile == '')) {
                            HQ.message.show(201302071, (i + 1) + ' (' + App.clmMobile.text + ')', '', false);
                            rowerror = 'err';
                            break;
                        }
                        //if(data.ClassId==''||data.ClassId==null)
                        //    isnullclass += i + 1 + ',';
                        //if (data.PriceClass == '' || data.PriceClass == null)
                        //    isnullpriceclass += i + 1 + ',';
                    }
                }
                if (rowerror != '') {
                    return;
                }
                if (errorFreq != '') {
                    HQ.message.show(2017022101, errorFreq, '');
                    return;
                }
                if (App.cboHandle.getValue() == 'A') {
                    App.dteFromDate.setValue(minDate);
                    App.dteToDate.setValue(HQ.EndDateYear);
                    App.dteFromDate.setMinValue(minDate);
                    App.dteFromDate.validate();
                    App.dteToDate.validate();
                    App.winProcess.show();
                } else {
                    var d = Ext.Date.parse("01/01/1990", "m/d/Y");
                    if (App.FromDate.getValue() < d || App.ToDate.getValue() < d) return;
                    var flat = false;
                    App.stoCust.data.each(function (item) {
                        if (item.data.ColCheck) {
                            flat = true;
                            return false;
                        }
                    });
                    if (flat && !Ext.isEmpty(App.cboHandle.getValue()) && App.cboHandle.getValue() != 'N') {
                        App.frmMain.submit({
                            clientValidation: false,
                            waitMsg: HQ.common.getLang("Handle"),
                            method: 'POST',
                            url: 'AR20500/Process',
                            timeout: 1800000,
                            params: {
                                lstCust: HQ.store.getAllData(App.grdCust.store, ['ColCheck'], [true]),
                                fromDate: HQ.bussinessDate,
                                toDate: HQ.bussinessDate,
                                askApprove: 0
                            },
                            success: function (msg, data) {
                                HQ.message.show(201405071);
                                App.stoCust.reload();
                            },
                            failure: function (msg, data) {
                                if (data && data.response && data.response.status == 0) {
                                    btnProcess_Click();
                                } else
                                    HQ.message.process(msg, data, true);
                            }
                        });
                    }
                }

            }
            else if (App.cboHandle.getValue() != 'A' || App.cboHandle.getValue() == 'A' && App.cboUpdateType.getValue() != 0) {
                var d = Ext.Date.parse("01/01/1990", "m/d/Y");
                if (App.FromDate.getValue() < d || App.ToDate.getValue() < d) return;
                var flat = false;
                App.stoCust.data.each(function (item) {
                    if (item.data.ColCheck) {
                        flat = true;
                        return false;
                    }
                });
                if (flat && !Ext.isEmpty(App.cboHandle.getValue()) && App.cboHandle.getValue() != 'N') {
                    App.frmMain.submit({
                        clientValidation: false,
                        waitMsg: HQ.common.getLang("Handle"),
                        method: 'POST',
                        url: 'AR20500/Process',
                        timeout: 1800000,
                        params: {
                            lstCust: HQ.store.getAllData(App.grdCust.store, ['ColCheck'], [true]),
                            fromDate: HQ.bussinessDate,
                            toDate: HQ.bussinessDate,
                            askApprove: 0
                        },
                        success: function (msg, data) {
                            HQ.message.show(201405071);
                            App.stoCust.reload();
                        },
                        failure: function (msg, data) {
                            if (data && data.response && data.response.status == 0) {
                                btnProcess_Click();
                            } else
                                HQ.message.process(msg, data, true);
                        }
                    });
                }
            }
        }
    }
    else if (App.cboUpdateType.getValue() == 2) {
        var count = 0;
        App.grdCustPG.store.clearFilter();
        var allData = App.grdCustPG.store.snaptshot || App.grdCustPG.store.allData || App.grdCustPG.store.data;
        for (var i = 0; i < allData.length ; i++) {
            var data = allData.items[i].data;
            if (data.ColCheck) {
                count++;
            }
        }
        if (count == 0) {
            HQ.message.show(718);
            return;
        }
        if (App.cboHandle.getValue() && count > 0) {
            var erroState = "";
            var erroChannel = "";
            var erroSourceID = "";
            var errorClassID = "";
            if (App.cboHandle.value != "D") {
                for (var i = 0; i < allData.length ; i++) {
                    if (allData.items[i].data.ColCheck) {
                        if (allData.items[i].data.State == "" || allData.items[i].data.State == null) {
                            erroState = erroState + (allData.items[i].index + 1) + ",";
                        }
                        if (allData.items[i].data.Channel == "" || allData.items[i].data.Channel == null) {
                            erroChannel = erroChannel + (allData.items[i].index + 1) + ",";
                        }
                        if (allData.items[i].data.SourceID == "" || allData.items[i].data.SourceID == null) {
                            erroSourceID = erroSourceID + (allData.items[i].index + 1) + ",";
                        }
                        if (allData.items[i].data.ClassID == "" || allData.items[i].data.ClassID == null) {
                            errorClassID = errorClassID + (allData.items[i].index + 1) + ",";
                        }
                    }
                }
            }
            
            if (erroState != "") {
                HQ.message.show(2018041211, [HQ.common.getLang('State'), erroState], '', true);
                return;
            }
            else if (erroChannel != "") {
                HQ.message.show(2018041211, [HQ.common.getLang('Channel'), erroChannel], '', true);
                return;
            }
            else if (erroSourceID != "") {
                HQ.message.show(2018041211, [HQ.common.getLang('SourceID'), erroSourceID], '', true);
                return;
            }
            else if (errorClassID != "") {
                HQ.message.show(2018041211, [HQ.common.getLang('ClassID'), errorClassID], '', true);
                return;
            }
        }
        if (!Ext.isEmpty(App.cboHandle.getValue()) && App.cboHandle.getValue() != 'N') {
            App.frmMain.submit({
                clientValidation: false,
                waitMsg: HQ.common.getLang("Handle"),
                method: 'POST',
                url: 'AR20500/ProcessPG',
                timeout: 1800000,
                params: {
                    lstCustPG: HQ.store.getAllData(App.grdCustPG.store, ['ColCheck'], [true]),
                    fromDate: HQ.bussinessDate,
                    toDate: HQ.bussinessDate,
                    askApprove: 0
                },
                success: function (msg, data) {
                    HQ.message.show(201405071);
                    App.stoCustPG.reload();
                },
                failure: function (msg, data) {
                    if (data && data.response && data.response.status == 0) {
                        btnProcess_Click();
                    } else
                        HQ.message.process(msg, data, true);
                }
            });
        }
    }

};

var btnOKMCP_Click = function () {
    if (App.dteFromDate.isValid() && App.dteToDate.isValid()) {
        //if (App.dteFromDate.lastValue.getFullYear() != App.dteToDate.lastValue.getFullYear()) {
        //    HQ.message.show(201506111);
        //    return;
        //}
        App.winProcess.hide();
        var d = Ext.Date.parse("01/01/1990", "m/d/Y");
        if (App.FromDate.getValue() < d || App.ToDate.getValue() < d) return;
        var flat = false;
        App.stoCust.data.each(function (item) {
            if (item.data.ColCheck) {
                flat = true;
                return false;
            }
        });
        _fromDate = App.dteFromDate.getValue();
        _toDate = App.dteToDate.getValue();
        if (flat && !Ext.isEmpty(App.cboHandle.getValue()) && App.cboHandle.getValue() != 'N') {
            App.frmMain.submit({
                clientValidation: false,
                waitMsg: HQ.common.getLang("Handle"),
                method: 'POST',
                url: 'AR20500/Process',
                timeout: 1800000,
                params: {
                    lstCust: HQ.store.getAllData(App.grdCust.store, ['ColCheck'], [true]), //lstCust: Ext.encode(App.grdCust.store.getRecordsValues()),
                    fromDate: App.dteFromDate.getValue(),
                    toDate: App.dteToDate.getValue(),
                    askApprove: 0

                },
                success: function (msg, data) {
                    HQ.message.show(201405071);
                    App.stoCust.reload();
                },
                failure: function (msg, data) {
                    if (data && data.response && data.response.status == 0) {
                        btnOKMCP_Click();
                    } else
                    HQ.message.process(msg, data, true);
                    //App.stoCust.reload();
                }
            });
        }
    }
};

var btnExit_Click = function () {
    App.winProcess.hide();
};

var dteFromDate_change = function (dtp, newValue, oldValue, eOpts) {
    App.dteToDate.setMinValue(newValue);
    App.dteToDate.validate();
};

var dteToDate_change = function (dtp, newValue, oldValue, eOpts) {
    App.dteFromDate.setMaxValue(newValue);
    App.dteFromDate.validate();
};

var askApprove = function (item) {
    if (item == 'yes') {     
        if (App.dteFromDate.getValue() == null) {
            _fromDate = HQ.bussinessDate;
        }
        else {
            _fromDate = App.dteFromDate.getValue();
        }

        if (App.dteToDate.getValue() == null) {
            _toDate = HQ.bussinessDate;
        }
        else {
            _toDate = App.dteToDate.getValue();
        }
        
        App.frmMain.submit({
            clientValidation: false,
            waitMsg: HQ.common.getLang("Handle"),
            method: 'POST',
            url: 'AR20500/Process',
            timeout: 1800000,
            params: {
                lstCust: Ext.encode(App.grdCust.store.getRecordsValues()),
                fromDate: _fromDate,
                toDate: _toDate,
                askApprove: 1
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                App.stoCust.reload();
            },
            failure: function (msg, data) {
                if (data && data.response && data.response.status == 0) {
                    askApprove('yes');
                } else {
                    HQ.message.process(msg, data, true);
                    App.stoCust.reload();
                }
            }
        });
    }
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdCust);
            break;
        case "prev":
            HQ.grid.prev(App.grdCust);
            break;
        case "next":
            HQ.grid.next(App.grdCust);
            break;
        case "last":
            HQ.grid.last(App.grdCust);
            break;
        case "refresh":
            if (_Change) {
                HQ.message.show(20150303, '', 'refresh');
            } else {
                App.stoCust.reload();
            }
            break;
        case "new":
            break;
        case "delete":
            break;
        case "save":
            break;
        case "print":
            break;
        case "close":
            break;
    }

};

var grdCust_BeforeEdit = function (item, e) {
    if (App.cboHandle.value == null) {
        HQ.message.show(2018091001);//Phai chon xu ly trc khi edit du lieu
        return false;
    }
    if (!HQ.isShowCustHT && e.field == 'CustHT') return false;
    if (!HQ.IsShowERPCust && e.field == 'ERPCustID') return false;
    if (!checkEditDet()) {
        return false;
    }
    if (e.field == 'ContactName') {
        checkEditContactName();
        if (_isditContactName == 0) {
            return false;
        }
    }
    if (e.field == 'ERPCustID' && e.record.data.UpdateType != 0) {
        return false;
    }
    if (e.field == 'Reason') {
        if (_isEditReason == false) {
            return false;
        }
    }
    else if (e.field != 'ColCheck') {
        if (e.field == 'EditInfo' && !e.record.data.AllowChangeEditInfo ||
               e.field == 'EditBusinessPic' && !e.record.data.AllowChangeEditBusinessPic ||
               e.field == 'EditProfilePic' && !e.record.data.AllowChangeEditProfilePic) {
            return false;            
        } else if (App.cboStatus.getValue() == 'A' || App.cboStatus.getValue() == 'D') {
            if (e.field != 'EditInfo' && 
                e.field != 'EditBusinessPic' &&
                e.field != 'EditProfilePic') {
                return false;
            }
            if (e.field == 'EditInfo' && !e.record.data.AllowChangeEditInfo || 
                e.field == 'EditBusinessPic' && !e.record.data.AllowChangeEditBusinessPic || 
                e.field == 'EditProfilePic' && !e.record.data.AllowChangeEditProfilePic) {
                return false;
            }
        }
        if (HQ.showSubRoute && _lockColumn.indexOf(e.field) != -1) {
            return false;
        }
    }
    if (e.field == 'SubRouteID') {
        _slsFreq = e.record.data.SlsFreq;
        App.cboColSubRoute.store.reload();
    }
    if (e.field == 'WeekofVisit') {
        App.cboColWeekofVisit.getStore().reload();
    }
    if (['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'].indexOf(e.field) > -1) {
        var objCheck = HQ.store.findRecord(App.stoAR20500_pdWeekofVisitAll, ['SlsFreq'], [e.record.data.SlsFreq]);
        if (objCheck) {
            return objCheck.data.IsEdit;
        }
    }
    if (e.field == 'SubTerritory' || e.field == 'State') {
        App.cboColSubTerritory.store.clearFilter();
        var territory = e.record.data.Territory;
        if (territory == '') {
            territory = '@@';
        }
        App.cboColSubTerritory.store.filter('Territory', territory);
        App.cboColState.store.clearFilter();
        App.cboColState.store.filter('Territory', territory);
    } else if (e.field == 'District') {
        App.cboColDistrict.store.clearFilter();
        var state = e.record.data.State;
        if (state == '') {
            state = '@@';
        }
        App.cboColDistrict.store.filter('State', state);
    } else if (e.field == 'Market') {
        App.cboMarket.store.clearFilter();
        var StateDistrict = e.record.data.State + e.record.data.District;
        if (e.record.data.District == "" || e.record.data.State == "")
        {
            StateDistrict = '@@';
        }
        App.cboMarket.store.filter('StateDistrict', StateDistrict);
    }else if (e.field == 'ShopType') {
        App.cboColShopType.store.clearFilter();
        var channel = e.record.data.Channel;
        if (channel == '') {
            channel = '@@';
        }
        App.cboColShopType.store.filter('Channel', channel);
    }
    else if (e.field == 'SalesRouteID') {
        _slsperID = e.record.data.SlsperID;
        App.cboColSalesRouteID.store.reload();
    }
    else if (e.field == 'SalesRouteID') {
        _slsperID = e.record.data.SlsperID;
        App.cboColOUnit.store.reload();
    }
    if (e.field == 'District') {
        _dis = e.record.data.District;
    }
    
};

var grdCust_ValidateEdit = function (item, e) {
    if (e.field == 'Territory') {
        if (e.value == e.record.data.Territory) {
            return false;
        }
    } else if (e.field == 'State') {
        if (e.value == e.record.data.State) {
            return false;
        }
    } else if (e.field == 'Channel') {
        if (e.value == e.record.data.Channel) {
            return false;
        }
    } else if (e.field == 'SubRouteID') {
        if (e.value == e.record.data.SubRouteID) {
            return false;
        }
    }
    else if (e.field == 'SalesRouteID') {
        if (e.value == e.record.data.SalesRouteID) {
            return false;
        }
    } else if (e.field == 'VisitsPerDay') {
        if (HQ.showVisitsPerDay && (e.value > HQ.maxVisitPerDay || e.value == 0)) {
            HQ.message.show(2018010101, [(e.rowIdx + 1), HQ.grid.findColumnNameByIndex(App.grdCust.columns, 'VisitsPerDay'), HQ.maxVisitPerDay], '', true);
            return false;
        }        
    }

};
var grdCust_Edit = function (item, e, oldvalue, newvalue) {
    var det = e.record.data;
    if (e.field == 'WeekofVisit') {
        var record = HQ.store.findRecord(App.cboColWeekofVisit.getStore(), ["Code"], [e.value]);
        if (record) {
            e.record.set("Mon", record.data.Mon);
            e.record.set("Tue", record.data.Tue);
            e.record.set("Wed", record.data.Wed);
            e.record.set("Thu", record.data.Thu);
            e.record.set("Fri", record.data.Fri);
            e.record.set("Sat", record.data.Sat);
            e.record.set("Sun", record.data.Sun);
        }
    }
    if (e.field == 'SlsFreq' && e.value != e.originalValue) {
        if (e.value != 'F1' && e.value != 'F2') {
            e.record.set('WeekofVisit', 'NA');
        }
        else e.record.set('WeekofVisit', '');
        e.record.set("Mon", false);
        e.record.set("Tue", false);
        e.record.set("Wed", false);
        e.record.set("Thu", false);
        e.record.set("Fri", false);
        e.record.set("Sat", false);
        e.record.set("Sun", false);
        if (HQ.showSubRoute) {
            e.record.set("SubRouteID", '');
        }        
    }
    if (e.field == 'Phone') {
        if (isNumeric(e.value) == true) {
            e.record.set("Phone", oldvalue.fn.arguments[1].originalValue);
            if (!HQ.editPhone)
                HQ.message.show(20171118, '');
            else
                HQ.message.show(2018022701, '');
        }
    } else if (e.field == 'Mobile') {
        if (isNumeric(e.value) == true) {
            e.record.set("Mobile", oldvalue.fn.arguments[1].originalValue);
            if (!HQ.editPhone)
                HQ.message.show(20171118, '');
            else
                HQ.message.show(2018022701, '');
        }
    } else if (e.field == 'Territory') {
        e.record.set('SubTerritory', '');
        e.record.set('State', '');
        e.record.set('District', '');
    } else if (e.field == 'State') {
        e.record.set('District', '');
    } else if (e.field == 'Channel') {
        e.record.set('ShopType', '');
    } else if (e.field == 'SubRouteID') {
        if (App.cboColSubRoute.valueModels != undefined && App.cboColSubRoute.valueModels.length > 0) {
            //e.record.set('SlsFreq', App.cboColSubRoute.valueModels[0].data.SlsFreqID);
            e.record.set('WeekofVisit', App.cboColSubRoute.valueModels[0].data.WeekOfVisit);

            e.record.set('Mon', App.cboColSubRoute.valueModels[0].data.Mon);
            e.record.set('Tue', App.cboColSubRoute.valueModels[0].data.Tue);
            e.record.set('Wed', App.cboColSubRoute.valueModels[0].data.Wed);
            e.record.set('Thu', App.cboColSubRoute.valueModels[0].data.Thu);
            e.record.set('Fri', App.cboColSubRoute.valueModels[0].data.Fri);
            e.record.set('Sat', App.cboColSubRoute.valueModels[0].data.Sat);
            e.record.set('Sun', App.cboColSubRoute.valueModels[0].data.Sun);
        } else {
            //e.record.set('SlsFreq', '');
            e.record.set('WeekofVisit', '');
            e.record.set('Mon', false);
            e.record.set('Tue', false);
            e.record.set('Wed', false);
            e.record.set('Thu', false);
            e.record.set('Fri', false);
            e.record.set('Sun', false);
            e.record.set('Sat', false);
        }
    } else if (e.field == 'SalesRouteID') {
        if (App.cboColSalesRouteID.valueModels != undefined && App.cboColSalesRouteID.valueModels.length > 0) {
            e.record.set('BranchRouteID', App.cboColSalesRouteID.valueModels[0].data.BranchRouteID);
            e.record.set('PJPID', App.cboColSalesRouteID.valueModels[0].data.PJPID);
        } else {
            e.record.set('BranchRouteID', '');
            e.record.set('PJPID', '');
        }
    }
    if (e.field == "District" || e.field == "State" || e.field == "Territory") {
        if (_dis != e.value) {
            App.cboMarket.store.reload();
            e.record.set('Market', '');
        }
    }
    //HQ.grid.checkInsertKey(App.grdCust, e, keys);
    App.cboMarket.store.clearFilter();
};

var grdCustPG_BeforeEdit = function (item, e) {
    if (App.cboHandle.value == null)
    {
        HQ.message.show(2018091001);//Phai chon xu ly trc khi edit du lieu
        return false;
    }
    if (e.field == 'State') {
        var territory = e.record.data.Territory;
        if (territory == '') {
            territory = '@@';
        }
        App.cboColStatePG.store.clearFilter();
        App.cboColStatePG.store.filter('Territory', territory);
    } else if (e.field == 'District') {
        App.cboColDistrictPG.store.clearFilter();
        var state = e.record.data.State;
        if (state == '') {
            state = '@@';
        }
        App.cboColDistrictPG.store.filter('State', state);
    }
};
var grdCustPG_Edit = function (item, e) {
    if (e.field == 'Phone') {
        if (isNumeric(e.value) == true) {
            e.record.set("Phone", oldvalue.fn.arguments[1].originalValue);
            if (!HQ.editPhone)
                HQ.message.show(20171118, '');
            else
                HQ.message.show(2018022701, '');
        }
    }
    else if (e.field == 'Territory') {
        e.record.set('State', '');
        e.record.set('District', '');
        e.record.set('Ward', '');
        e.record.set('Addr1', '');
        e.record.set('Addr2', '');
    } else if (e.field == 'State') {
        e.record.set('District', '');
        e.record.set('Ward', '');
        e.record.set('Addr1', '');
        e.record.set('Addr2', '');
    } else if (e.field == 'District') {
        e.record.set('Ward', '');
        e.record.set('Addr1', '');
        e.record.set('Addr2', '');
    }

};
var grdCustPG_ValidateEdit = function (item, e) {
    if (e.field == 'Territory') {
        if (e.value == e.record.data.Territory) {
            return false;
        }
    } else if (e.field == 'State') {
        if (e.value == e.record.data.State) {
            return false;
        }
    } else if (e.field == 'Channel') {
        if (e.value == e.record.data.Channel) {
            return false;
        }
    }
};
var grdCustPG_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdCust);
    stoChanged(App.stoCustPG);
};
function isNumeric(n) {
    if (!HQ.editPhone) {
        var regex = /^([0-9()-.;#/ ])*$/;
        return !HQ.util.passNull(n.toString()).match(regex);
    }
    else {
        var regex = /\D/;
        if (HQ.util.passNull(n.toString()).match(regex))
            return true;
    }
}

var grdCust_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdCust);
    stoChanged(App.stoCust);
};

var stoChanged = function (sto) {
    _Change = HQ.store.isChange(sto);
    HQ.common.changeData(_Change, 'AR20500');
    App.cboStatus.setReadOnly(_Change);
    App.btnLoad.setDisabled(_Change);
    App.cboCpnyID.setReadOnly(_Change);
    App.cboSlsperId.setReadOnly(_Change);

    App.cboTerritory.setReadOnly(_Change);
    App.cboUpdateType.setReadOnly(_Change);
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.waitMsg);
};

var isValidSel = function (data) {
    if (Ext.isEmpty(data.SalesRouteID) || Ext.isEmpty(data.SlsFreq) || Ext.isEmpty(data.WeekofVisit))
        return false;
    var iVisit = 0;
    if (data.Mon) {
        iVisit = iVisit + 1;
    }
    if (data.Tue) {
        iVisit = iVisit + 1;
    }
    if (data.Wed) {
        iVisit = iVisit + 1;
    }
    if (data.Thu) {
        iVisit = iVisit + 1;
    }
    if (data.Fri) {
        iVisit = iVisit + 1;
    }
    if (data.Sat) {
        iVisit = iVisit + 1;
    }
    if (data.Sun) {
        iVisit = iVisit + 1;
    }
    if (data.SlsFreq) {
        switch (data.SlsFreq) {
            case "F1":
            case "F2":
            case "F4":
            case "F4A":
                if (iVisit != 1) {
                    return false;
                }
                break;
            case "F8":
            case "F8A":
                if (iVisit != 2) {
                    return false;
                }
                break;
            case "F12":
                if (iVisit != 3) {
                    return false;
                }
                break;
            case "F16":
                if (iVisit != 4) {
                    return false;
                }
                break;
            case "F20":
                if (iVisit != 5) {
                    return false;
                }
                break;
            case "F24":
                if (iVisit != 6) {
                    return false;
                }
                break;
            case "A":
                if (iVisit == 0) {
                    return false;
                }
                break;
        }
    }
    return true;
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto, records, successful, eOpts) {
    if (successful) {
        _Change = HQ.store.isChange(sto);
        HQ.common.changeData(HQ.isChange, 'AR20500');


        stoChanged(App.stoCust);

        var markers = [];
        records.forEach(function (record) {
            var marker = {
                "index": record.index,
                "id": record.index + 1,
                "title": record.data.CustID + ": " + record.data.OutletName,
                "lat": record.data.Lat,
                "lng": record.data.Lng,
                "ImageFileName": record.data.ImageFileName,
                "description":
                    '<div id="content">' +
                        '<div id="siteNotice">' +
                        '</div>' +
                        '<h1 id="firstHeading" class="firstHeading">' +
                            record.data.OutletName +
                        '</h1>' +
                        '<div id="bodyContent">' +
                            '<p>' +
                                record.data.Phone +
                            '</p>' +
                        '</div>' +
                    '</div>'
            }
            markers.push(marker);
        });
        if (records.length == 0) {
            HQ.common.showBusy(false);
        }
        Gmap.Process.drawMCP(markers, false);
    } else {
        HQ.common.showBusy(false);
    }
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        _Change = false;
        App.ColCheck_Header.setValue(false);
        if (App.cboUpdateType.getValue() == 0 || App.cboUpdateType.getValue() == 1)
            App.stoCust.reload();
        else if (App.cboUpdateType.getValue() == 2)
            App.stoCustPG.reload();
    }
};

var rendererUpdateType = function (value) {
    if (value == 0) {
        return HQ.common.getLang("AR20500NewCust");
    } else if (value == 1) {
        return HQ.common.getLang("AR20500UpdCust");
    }
    return value;
};

var renderSubTerritory = function (value) {
    var obj = App.cboColSubTerritory.store.findRecord("Code", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};
var renderChannel = function (value) {
    var obj = App.cboColChannel.store.findRecord("Code", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};
var renderSourceID = function (value) {
    var obj = App.cboColSourceID.store.findRecord("SourceID", value);
    if (obj) {
        return obj.data.SourceDescr;
    }
    return value;
};
var renderLocation = function (value) {
    var obj = App.cboColLocation.store.findRecord("Code", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};

var renderClassCust = function (value) {
    var obj = App.cboClassCust.store.findRecord("Code", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};

var renderStandID = function (value) {
    var obj = App.cboStandID.store.findRecord("StandID", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};
var renderSlsFreq = function (value) {
    var obj = App.cboColSlsFreq1.store.findRecord("Code", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};




var renderBrandID = function (value, metaData, record, rowIndex, colIndex, store) {
    return getDescriptionByCode1(value, App.cboBrandID.store);
}
var getDescriptionByCode1 = function (value, sto) {
    var lstBrandID = value.split(',');
    var description = '';
    for (var i = 0; i < lstBrandID.length; i++) {
        var rec = HQ.store.findRecord(sto, ['Code'], [lstBrandID[i]]);
        if (rec) {
            description += rec.data.Descr + ',';
        } else {
            description += value;
        }
    }
    return description;
}
var renderSubRouteID = function (value) {
    var obj = App.stoAR20500_pdSubRoute.findRecord("SubRouteID", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};


var renderSizeID = function (value) {
    var obj = App.cboSizeID.store.findRecord("Code", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};

var renderDisplayID = function (value) {
    var obj = App.cboDisplayID.store.findRecord("Code", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};



var renderShopType = function (value) {
    var obj = App.cboColShopType.store.findRecord("Code", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};

var renderReason = function (value, metaData, record, rowIndex, colIndex, store) {
    return getDescriptionByCode(value, App.cboColReason.store);
}
var getDescriptionByCode = function (value, sto) {
    var lstReason = value.split(',');
    var description = '';
    for (var i = 0; i < lstReason.length; i++) {
        var rec = HQ.store.findRecord(sto, ['Code'], [lstReason[i]]);
        if (rec) {
            description += rec.data.Descr + ',';
        } else {
            description += value;
        }
    }
    return description;
}
var cboColReason_Focus = function () {
    if (App.grdCust.selModel.selected.length > 0) {
        HQ.combo.expand(this, ',');
        this.forceSelection = true;
    }    
}

var stringFilter = function (record) {
    if (this.dataIndex == 'Zone') {
        return false;
    }
    else if (this.dataIndex == 'SalesRouteID') {
        App.cboColSalesRouteID.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboColSalesRouteID.store, "Code", "Descr");
    }

    else if (this.dataIndex == 'Territory') {
        App.cboColTerritory.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboColTerritory.store, "Code", "Descr");
    }
    else if (this.dataIndex == 'State') {
        App.cboColState.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboColState.store, "Code", "Descr");
    }
    else if (this.dataIndex == 'District') {
        App.cboColDistrict.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboColDistrict.store, "Code", "Descr");
    }
    else if (this.dataIndex == 'Channel') {
        App.cboColChannel.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboColChannel.store, "Code", "Descr");
    } else if (this.dataIndex == 'ClassId') {
        App.cboColCustClass.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboColCustClass.store, "Code", "Descr");
    }
    else if (this.dataIndex == 'SubTerritory') {
        App.cboColSubTerritory.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboColSubTerritory.store, "Code", "Descr");
    }
    else if (this.dataIndex == 'ShopType') {
        App.cboColShopType.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboColShopType.store, "Code", "Descr");
    }
    //else if (this.dataIndex == 'Classification') {
    //    App.cboColClassification.store.clearFilter();
    //    return HQ.grid.filterComboDescr(record, this, App.cboColClassification.store, "Code", "Descr");
    //}
    else if (this.dataIndex == 'Location') {
        App.cboColLocation.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboColLocation.store, "Code", "Descr");
    }
    else if (this.dataIndex == 'SlsFreq') {
        App.cboColSlsFreq1.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboColSlsFreq1.store, "Code", "Descr");
    }
    
    else if (this.dataIndex == 'BrandID') {
            App.cboBrandID.store.clearFilter();
            return HQ.grid.filterComboDescr(record, this, App.cboBrandID.store, "Code", "Descr");
    }
    else if (this.dataIndex == 'StandID') {
        App.cboStandID.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboStandID.store, "StandID", "Descr");
    }
    else if (this.dataIndex == 'SizeID') {
        App.cboSizeID.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboSizeID.store, "Code", "Descr");
    }
    else if (this.dataIndex == 'SubRouteID') {
        App.cboColSubRoute.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboColSubRoute.store, "SubRouteID", "Descr");
    }
    else if (this.dataIndex == 'DisplayID') {
        App.cboDisplayID.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboDisplayID.store, "Code", "Descr");
    }
    
    else if (this.dataIndex == 'WeekofVisit') {
        App.cboColWeekofVisit.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboColWeekofVisit.store, "Code", "Descr");
    }
    else if (this.dataIndex == 'Market') {
        App.cboMarket.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboMarket.store, "Market", "Descr");
    }
    else if (this.dataIndex == 'ClassID') {
        App.cboClassCustPG.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboClassCustPG.store, "Code", "Descr");
    }
    else if (this.dataIndex == 'SourceID') {
        App.cboColSourceID.store.clearFilter();
        return HQ.grid.filterComboDescr(record, this, App.cboColSourceID.store, "SourceID", "SourceDescr");
    }
}

var renderWeekofVisit = function (value) {
    var obj = App.stoAR20500_pdWeekofVisitAll.findRecord("Code", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};

var renderSalesRouteID = function (value) {
    var obj = App.cboColSalesRouteID.store.findRecord("Code", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};

var renderIndex = function (value, metaData, record, rowIndex, colIndex, store) {
    return App.stoCust.indexOf(record) + 1;
}
var renderIndexPG = function (value, metaData, record, rowIndex, colIndex, store) {
    return App.stoCustPG.indexOf(record) + 1;
}
var slmCust_Select = function (rowModel, record, index, eOpts) {
    if (record[0]) {
        if (record[0].data.Lat && record[0].data.Lng) {
            Gmap.Process.navMapCenterByLocation(record[0].data.Lat, record[0].data.Lng, record.index + 1);
            //displayImage(App.imgImages, record[0].data.ImageFileName);// get image theo binary
            //displayImage(App.imgImages1, record[0].data.BusinessPic);// get image theo binary
        }
        displayImage(App.imgImages, record[0].data.ImageFileName);// get image theo binary
        displayImage(App.imgImages1, record[0].data.BusinessPic);// get image theo binary
    }
};

var displayImage = function (imgControl, fileName) {
    Ext.Ajax.request({
        url: 'AR20500/ImageToBin',
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
            if (errorMsg && errorMsg.status == 0) {
                displayImage(imgControl, fileName);
            } else
                HQ.message.process(errorMsg, data, true);
        }
    });
};

var pnlGridMCL_viewGetRowClass = function (record) {
    if (record.data.Color != '0' && record.data.Color != '')
        return 'hightlight-row'
};

var cboUpdateType_Change = function () {

    Ext.suspendLayouts();
    if (App.cboUpdateType.getValue() == 0 || App.cboUpdateType.getValue() == 1)
    {
        App.grdCust.show();
        App.grdCustPG.hide();
    }
    else if (App.cboUpdateType.getValue() == 2) {
        App.grdCust.hide();
        App.grdCustPG.show();
    }
    if (App.cboUpdateType.getValue() == 0) {     
        HQ.grid.show(App.grdCust, _hideColumn);
    } else if (App.cboUpdateType.getValue() == 1) {
        HQ.grid.hide(App.grdCust, _hideColumn);
    }
    
    if (!HQ.showVisitsPerDay) {
        HQ.grid.hide(App.grdCust, ['VisitsPerDay']);
    }
    showSubRoute(App.cboUpdateType.getValue());
    Ext.resumeLayouts();

    setHideControls();
    App.grdCust.store.loadData([],false);
    App.grdCust.view.refresh();

}
///////////////////////////////////////////////////////////////////////

var Gmap = {
    Declare: {
        map_canvas: {},
        map: {},
        directionsService: {},
        directionsDisplay: {},
        directionsDisplays: [],
        infoWindow: {},
        stopMarkers: [],
        drawingManager: {}
    },

    Process: {
        initialize: function () {
            Gmap.Declare.map_canvas = document.getElementById("map_canvas");

            var initLatLng = new google.maps.LatLng(10.782171, 106.654012, 17);
            var myOptions = {
                center: initLatLng,
                zoom: 16,
                mapTypeId: google.maps.MapTypeId.ROADMAP
            };

            Gmap.Declare.map = new google.maps.Map(Gmap.Declare.map_canvas, myOptions);
            Gmap.Declare.directionsService = new google.maps.DirectionsService();
            Gmap.Declare.directionsDisplay = new google.maps.DirectionsRenderer();
            Gmap.Declare.infoWindow = new google.maps.InfoWindow();

            Gmap.Declare.stopMarkers = [];
        },

        navMapCenterByLocation: function (lat, lng, id) {
            var selectedMarker;
            var myLatlng = new google.maps.LatLng(lat, lng);
            Gmap.Declare.map.setCenter(myLatlng);
            Gmap.Declare.map.setZoom(Gmap.Declare.map.getZoom());

            if (id) {
                selectedMarker = Gmap.Process.find_marker_id(id);
                if (!selectedMarker) {
                    selectedMarker = Gmap.Process.find_closest_marker(lat, lng);
                }
            }
            else {
                selectedMarker = Gmap.Process.find_closest_marker(lat, lng);
            }

            if (selectedMarker) {
                var visible = selectedMarker.visible;

                if (selectedMarker.getAnimation() != null) {
                    selectedMarker.setAnimation(null);
                } else {
                    if (!visible) {
                        selectedMarker.visible = true;
                    }
                    selectedMarker.setAnimation(google.maps.Animation.BOUNCE);
                    setTimeout(function () {
                        selectedMarker.setAnimation(null);
                        if (!visible) {
                            selectedMarker.visible = false;
                        }
                    }, 1400);
                }
            }
        },

        find_marker_id: function (id) {
            for (i = 0; i < Gmap.Declare.stopMarkers.length; i++) {
                if (Gmap.Declare.stopMarkers[i].id == id) {
                    return Gmap.Declare.stopMarkers[i];
                }
            }
            return null;
        },

        find_closest_marker: function (lat1, lon1) {
            var pi = Math.PI;
            var R = 6371; //equatorial radius
            var distances = [];
            var closest = -1;

            for (i = 0; i < Gmap.Declare.stopMarkers.length; i++) {
                var lat2 = Gmap.Declare.stopMarkers[i].position.lat();
                var lon2 = Gmap.Declare.stopMarkers[i].position.lng();

                var chLat = lat2 - lat1;
                var chLon = lon2 - lon1;

                var dLat = chLat * (pi / 180);
                var dLon = chLon * (pi / 180);

                var rLat1 = lat1 * (pi / 180);
                var rLat2 = lat2 * (pi / 180);

                var a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
                            Math.sin(dLon / 2) * Math.sin(dLon / 2) * Math.cos(rLat1) * Math.cos(rLat2);
                var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
                var d = R * c;

                distances[i] = d;
                if (closest == -1 || d < distances[closest]) {
                    closest = i;
                }
            }

            // (debug) The closest marker is:
            return Gmap.Declare.stopMarkers[closest];
        },

        prepairMap: function () {
            if (Gmap.Declare.directionsDisplay) {
                Gmap.Declare.directionsDisplay.setMap(null);
                if (Gmap.Declare.directionsDisplays && Gmap.Declare.directionsDisplays.length > 0) {
                    for (var i = 0; i < Gmap.Declare.directionsDisplays.length; i++) {
                        Gmap.Declare.directionsDisplays[i].setMap(null);
                        //Gmap.Declare.directionsDisplay[i] = new google.maps.DirectionsRenderer();
                    }
                }
            }
            Gmap.Declare.directionsService = new google.maps.DirectionsService();
            Gmap.Declare.directionsDisplay = new google.maps.DirectionsRenderer();
        },

        clearMap: function (stopMarkers) {
            for (i = 0; i < stopMarkers.length; i++) {
                Gmap.Declare.stopMarkers[i].setMap(null);

                if (i == stopMarkers.length - 1) {
                    Gmap.Declare.stopMarkers = [];
                }
            }
            Gmap.Declare.directionsDisplay.setMap(Gmap.Declare.map);
        },

        drawMCP: function (markers) {
            Gmap.Process.prepairMap();

            if (markers.length > 0) {
                Gmap.Declare.stopMarkers = [];
                // List of locations
                var lat_lng = new Array();

                // For each marker in list
                for (i = 0; i < markers.length; i++) {
                    var data = markers[i];
                    if (data.lat && data.lng) {
                        var myLatlng = new google.maps.LatLng(data.lat, data.lng);

                        // pin color
                        var pinColor = "FE6256";//"BDBDBD";//FE6256

                        // Push the location to list
                        lat_lng.push(myLatlng);

                        // Maps center at the first location
                        if (i == 0) {
                            var myOptions = {
                                center: myLatlng,
                                zoom: 16,
                                mapTypeId: google.maps.MapTypeId.ROADMAP
                            };
                            Gmap.Declare.map = new google.maps.Map(Gmap.Declare.map_canvas, myOptions);
                        }

                        // Make the marker at each location
                        var markerLabel = data.visitSort;
                        var marker = new google.maps.Marker({
                            id: data.id,
                            position: myLatlng,
                            map: Gmap.Declare.map,
                            title: data.title,
                            icon: Ext.String.format('https://chart.googleapis.com/chart?chst=d_map_spin&chld=0.6|0|{1}|10|_|{0}', i + 1, pinColor)
                           // icon: Ext.String.format('http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld={0}|{1}|000000', i + 1, pinColor)
                        });

                        // Set info display of the marker
                        (function (marker, data) {
                            google.maps.event.addListener(marker, "click", function (e) {
                                App.slmCust.select(data.index);
                                //displayImage(App.imgImages, data.ImageFileName);
                                Gmap.Declare.infoWindow.setContent(data.description);
                                Gmap.Declare.infoWindow.open(Gmap.Declare.map, marker);

                                // Set animation of marker
                                if (marker.getAnimation() != null) {
                                    marker.setAnimation(null);
                                } else {
                                    marker.setAnimation(google.maps.Animation.BOUNCE);
                                    setTimeout(function () {
                                        marker.setAnimation(null);
                                    }, 1400);
                                }
                            });
                        })(marker, data);

                        Gmap.Declare.stopMarkers.push(marker);
                    }
                }
                HQ.common.showBusy(false);
                Gmap.Declare.directionsDisplay.setMap(Gmap.Declare.map);
                //directionsDisplay.setOptions({ suppressMarkers: true });
            }
            else {
                HQ.common.showBusy(false);
                Gmap.Process.clearMap(Gmap.Declare.stopMarkers);
            }
        },
    }
};
var isFirstSubTerritoryExpand = true;
// Expand State
var cboColSubTerritory_Expand = function (combo) {
    //var store = App.cboColSubTerritory.store;
    //App.cboColSubTerritory.store.clearFilter();
    //var territory = '';
    //if (App.grdCust.selModel.selected.length > 0) {
    //    territory = App.grdCust.selModel.selected.items[0].data.Territory;
    //} else {
    //    territory = App.cboColTerritory.getValue();
    //}
    ////// Filter data -- 
    //store.filterBy(function (record) {
    //    if (record) {
    //        if (record.data['Territory'].toString() == territory) {
    //            return record;
    //        }
    //    }
    //});
};
var cboColSubTerritory_Collapse = function (cbombo) {
    //App.cboColSubTerritory.store.clearFilter();
};

// Expand State
var cboColState_Expand = function (combo) {
    //App.cboColState.store.clearFilter();
    //var territory = '';
    //if (App.grdCust.selModel.selected.length > 0) {
    //    territory = App.grdCust.selModel.selected.items[0].data.Territory;
    //} else {
    //    territory = App.cboColTerritory.getValue();
    //}

    //var store = App.cboColState.store;
    //// Filter data -- 
    //store.filterBy(function (record) {
    //    if (record) {
    //        if (record.data['Territory'].toString() == territory) {
    //            return record;
    //        }
    //    }
    //});
};
var cboColState_Collapse = function (cbombo) {
    App.cboColState.store.clearFilter();
};

// Expand District
var cboColDistrict_Expand = function (combo) {
    //App.cboColDistrict.store.clearFilter();
    //var state = '';
    //if (App.grdCust.selModel.selected.length > 0) {
    //    state = App.grdCust.selModel.selected.items[0].data.State;
    //} else {
    //    state = App.cboColState.getValue();
    //}
    //var store = App.cboColDistrict.store;
    //// Filter data -- 
    //store.filterBy(function (record) {
    //    if (record) {
    //        if (record.data['State'].toString() == state) {
    //            return record;
    //        }
    //    }
    //});
};
var cboColDistrict_Collapse = function (cbombo) {
    App.cboColDistrict.store.clearFilter();
};


var renderWeekofVisit = function (value) {
    var obj = App.stoAR20500_pdWeekofVisitAll.findRecord("Code", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};

var renderTerritory = function (value) {
    var obj = App.cboColTerritory.store.findRecord("Territory", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};

var renderState = function (value) {
    var obj = App.cboColState.store.findRecord("Code", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};

var renderDistrict = function (value) {
    var obj = App.cboColDistrict.store.findRecord("Code", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
}

var renderMarket = function (value) {
    var obj = App.cboMarket.store.findRecord("Market", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
}
var btnExport_Click = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("Exporting"),
            url: 'AR20500/ExportExcel',
            type: 'POST',
            timeout: 1800000,
            clientValidation: false,
            params: {
                reportNbr: 'RA205',
                ReportName: 'AR_ApprMCL'
                ////lstCustTD: Ext.encode(App.stoDiscBreak.getRecordsValues()),// HQ.store.getData(App.stoAR_CustomerTD)
                ////lstCustTD1: Ext.encode(App.cboExport.getValue()),
                //templateExport: App.cboExport.getValue()
            },
            success: function (msg, data) {
                window.location = 'AR20500/DownloadAndDelete?file=' + data.result.fileName;
            },
            failure: function (msg, data) {
                if (data && data.response && data.response.status == 0) {
                    btnExport_Click();
                } else
                    HQ.message.process(msg, data, true);
            }
        });
    }
};

var btnSave_Click = function () {
    var allData = App.grdCust.store.snaptshot || App.grdCust.store.allData || App.grdCust.store.data;
    var count = 0;
    for (var i = 0; i < allData.length ; i++) {
        var data = allData.items[i].data;
        if (data.ColCheck) {
            count++;
        }
    }
    if (count == 0) {
        HQ.message.show(718);
        return false;
    }
    var showMess = false;
    var objEditBusinessPic = HQ.store.findRecord(App.stoCust, ['ColCheck', 'EditBusinessPic'], [true, true]);
    if (objEditBusinessPic) {
        showMess = true;
    } else {
        var objEditProfilePic = HQ.store.findRecord(App.stoCust, ['ColCheck', 'EditProfilePic'], [true, true]);
        if (objEditProfilePic) {
            showMess = true;
        } 
    }
    if (!showMess) {
        App.frmMain.submit({
            clientValidation: false,
            waitMsg: HQ.common.getLang("Handle"),
            method: 'POST',
            url: 'AR20500/SaveEdit',
            timeout: 1800000,
            params: {
                lstCust: HQ.store.getAllData(App.grdCust.store, ['ColCheck'], [true])
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                if (App.grdCust.selModel.selected.length > 0) {
                    var data = App.grdCust.selModel.selected.items[0].data;
                    if (data.EditBusinessPic == true) {
                        App.imgImages1.setImageUrl('');
                    } else if (data.EditProfilePic) {
                        App.imgImages.setImageUrl('');
                    }                                        
                }
                

                App.stoCust.reload();
            },
            failure: function (msg, data) {
                if (data && data.response && data.response.status == 0) {
                    btnSave_Click();
                } else
                HQ.message.process(msg, data, true);
            }
        });
    }
    else {
        HQ.message.show(2017121301, '', 'confirmSave', false);
    }
};

var confirmSave = function (item) {
    if (item == 'yes') {
        App.frmMain.submit({
            clientValidation: false,
            waitMsg: HQ.common.getLang("Handle"),
            method: 'POST',
            url: 'AR20500/SaveEdit',
            timeout: 1800000,
            params: {
                lstCust: HQ.store.getAllData(App.grdCust.store, ['ColCheck'], [true])
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                if (App.grdCust.selModel.selected.length > 0) {
                    var data = App.grdCust.selModel.selected.items[0].data;
                    if (data.EditBusinessPic == true) {
                        App.imgImages1.setImageUrl('');
                    } else if (data.EditProfilePic) {
                        App.imgImages.setImageUrl('');
                    }
                }
                App.stoCust.reload();
            },
            failure: function (msg, data) {
                if (data && data.response && data.response.status == 0) {
                    confirmSave('yes');
                } else
                HQ.message.process(msg, data, true);
            }
        });
    }
}

var setHideControls = function () {
    Ext.suspendLayouts();
    var isShow = false;
    if (HQ.AllowSave == '' || HQ.IshowEditCust == false) {
        isShow = false;
    } else {
        var value = App.cboStatus.getValue() + App.cboUpdateType.getValue();
        var items = HQ.AllowSave.split(',');
        
        for (var i = 0; i < items.length; i++) {
            if (items[i] == value) {
                isShow = true;
                break;
            }
        }
    }
   App.btnSave.setVisible(isShow);
    if (isShow) {
        HQ.grid.show(App.grdCust, hideEditCustColumn);
    } else {
        if (HQ.AllowApproveEditCust != '') {
            var isShow = false;
            var value = App.cboStatus.getValue() + App.cboUpdateType.getValue() + App.cboHandle.getValue();
            var items = HQ.AllowApproveEditCust.split(',');

            for (var i = 0; i < items.length; i++) {
                if (items[i] == value) {
                    isShow = true;
                    break;
                }
            }
            if (isShow) {
                HQ.grid.show(App.grdCust, hideEditCustColumn);
            } else {
                _isEditReason = true;
                HQ.grid.hide(App.grdCust, hideEditCustColumn);
            }
        } else {
            HQ.grid.hide(App.grdCust, hideEditCustColumn);
        }
        
    }
    
    checkEditReason();
    Ext.resumeLayouts();
};

var checkEditReason = function () {
    _isEditReason = false;
    var value = App.cboStatus.getValue() + App.cboUpdateType.getValue();
    var items = HQ.AllowEditReason.split(',');
    for (var i = 0; i < items.length; i++) {
        if (items[i] == value) {
            _isEditReason = true;
            break;
        }
    }
};




var checkEditContactName = function () {
    _isditContactName = false;
    var value = App.cboStatus.getValue() + App.cboUpdateType.getValue();
    var items = HQ.AllowEditContactName.split(',');
    for (var i = 0; i < items.length; i++) {
        if (items[i] == value) {
            _isditContactName = 1;
            break;
        }
    }
};



var cboHandle_Change = function () {    
    if (_Change && App.cboHandle.store.data.length > 1) {
        HQ.message.show(2017121601, '', 'confirmChangeHanle');
    } else {
        setHideControls();
        App.stoCust.rejectChanges();
        App.grdCust.view.refresh();
    }
}
var checkEditDet = function () {
    if (App.cboHandle.store.data.length > 1 && Ext.isEmpty(App.cboHandle.getValue())) {
        HQ.message.show(1000, App.cboHandle.fieldLabel);
        return false;
    }
    return true;
}
var currentStatus = '';
var cboHandle_BeforeChange = function () {
    currentStatus = App.cboHandle.getValue();
    
}

var confirmChangeHanle = function (item) {
    if (item == 'yes') {
        setHideControls();
        if (App.cboUpdateType.getValue() == 2) {
            App.stoCustPG.rejectChanges();
            App.grdCustPG.view.refresh();
        }
        else if (App.cboUpdateType.getValue() == 0 || App.cboUpdateType.getValue() == 1) {
            App.stoCust.rejectChanges();
            App.grdCust.view.refresh();
        }

    } else {
        App.cboHandle.events['change'].suspend();
        App.cboHandle.setValue(currentStatus);
        App.cboHandle.events['change'].resume();
        
    }
}

var ColCheck_Header_ValidityChange = function () {
    if (!checkEditDet()) {
        return false;
    }
}
var showSubRoute = function (updateType) {
    if (HQ.showSubRoute && updateType == 0) {
        HQ.grid.show(App.grdCust, ['SubRouteID']);
    } else {
        HQ.grid.hide(App.grdCust, ['SubRouteID']);
    }
}
var cboBrandID_Focus = function () {
    if (App.grdCust.selModel.selected.length > 0) {
        HQ.combo.expand(this, ',');
        this.forceSelection = true;
    }
}
var cboColOUnit_Focus = function () {
    if (App.grdCust.selModel.selected.length > 0) {
        HQ.combo.expand(this, ',');
        this.forceSelection = true;
    }
}


var renderOUnit = function (value, metaData, record, rowIndex, colIndex, store) {
    return getDescriptionByCode(value, App.cboColOUnit.store);
}


