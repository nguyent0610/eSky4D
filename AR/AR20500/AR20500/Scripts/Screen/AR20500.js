var _Change = false;
var keys = ['ID'];
var _firstLoad = true;
var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.stoAR20500_WeekofVisitAll.load(function () {
        App.cboCpnyID.getStore().load(function () {
            App.cboSlsperId.getStore().load(function () {
                App.cboStatus.getStore().load(function () {
                    App.cboHandle.getStore().load(function () {
                        HQ.common.showBusy(false, HQ.common.getLang("loadingData"));
                        if (_firstLoad) {
                            App.cboStatus.setValue("H");
                            App.cboHandle.setValue("N");
                            _firstLoad = false;
                        }
                    })
                })
            })
        })
    });
};
var cboCpnyID_Change = function (value) {
    if (_Change == true) {
        HQ.message.show(20150303, '', 'refresh');
    } else {
        App.grdCust.store.removeAll();
        App.cboSlsperId.store.reload();
        App.cboColSalesRouteID.store.reload();
        //App.grdCust.removeAll();
    }
};
var cboStatus_Change = function (value) {
    if (_Change == true) {
        HQ.message.show(20150303, '', 'refresh');
    } else {
        App.grdCust.store.removeAll();
        App.cboHandle.store.reload();
        //App.grdCust.removeAll();
    }
};

var btnLoad_Click = function () {
    App.stoCust.reload();
};

var ColCheck_Header_Change = function (value) {
    if (value) {
        App.stoCust.each(function (item) {
            if(item.data.Status==App.cboStatus.getValue())
            item.set("ColCheck", value.checked);
        });
    }
};

var btnProcess_Click = function () {
    var rowerror = '';
    var count = 0;
    for (var i = 0; i < App.grdCust.store.getCount() ; i++) {
        var data = App.grdCust.store.data.items[i].data;
        if (data.ColCheck) {
            count++;
            if (!isValidSel(data)) rowerror += i+1 + ',';
        }
    }
    if (rowerror != '') {
        HQ.message.show(201302071, rowerror, '');
    }
    else if (App.cboHandle.getValue() && count>0) {
        if (App.cboHandle.getValue() == 'A') {
            App.dteFromDate.setValue(HQ.bussinessDate);
            App.dteToDate.setValue(HQ.bussinessDate);
            App.winProcess.show();
        }
        else {
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
                    timeout: 180000,
                    params: {
                        lstCust: Ext.encode(App.grdCust.store.getRecordsValues())                        
                    },
                    success: function (msg, data) {
                        HQ.message.show(201405071);
                        App.stoCust.reload();
                    },
                    failure: function (msg, data) {
                        HQ.message.process(msg, data, true);
                    }
                });
            }
        }
    }
};
var btnOKMCP_Click = function () {
    if (App.dteFromDate.isValid() && App.dteToDate.isValid()) {
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
        if (flat && !Ext.isEmpty(App.cboHandle.getValue()) && App.cboHandle.getValue() != 'N') {
            App.frmMain.submit({
                clientValidation: false,
                waitMsg: HQ.common.getLang("Handle"),
                method: 'POST',
                url: 'AR20500/Process',
                timeout: 180000,
                params: {
                    lstCust: Ext.encode(App.grdCust.store.getRecordsValues()),
                    fromDate:App.dteFromDate.getValue(),
                    toDate: App.dteToDate.getValue()
                    
                },
                success: function (msg, data) {
                    HQ.message.show(201405071);
                    App.stoCust.reload();
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                    App.stoCust.reload();
                }
            });
        }
    }
}
var btnExit_Click = function () {
    App.winProcess.hide();
}

var dteFromDate_change= function (dtp, newValue, oldValue, eOpts) {
    App.dteToDate.setMinValue(newValue);
    App.dteToDate.validate();
}
var dteToDate_change = function (dtp, newValue, oldValue, eOpts) {
    App.dteFromDate.setMaxValue(newValue);
    App.dteFromDate.validate();
}
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
            HQ.common.close(this);
            break;
    }

};

var grdCust_BeforeEdit = function (item, e) {
    if (e.field != 'ColCheck'&&App.cboStatus.getValue() != 'H') return false;
    if (e.field == 'WeekofVisit') {
        App.cboColWeekofVisit.getStore().reload();
    }
    if (['Mon','Tue','Wed','Thu','Fri','Sat','Sun'].indexOf(e.field)>-1) {       
        var objCheck = HQ.store.findRecord(App.stoAR20500_WeekofVisitAll, [ 'SlsFreq'], [ e.record.data.SlsFreq]);
        if (objCheck) {
            return objCheck.data.IsEdit;
        }
    }
}
var grdCust_Edit = function (item, e) {
    var det=e.record.data;
    if (e.field == 'WeekofVisit' ) {
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
    //HQ.grid.checkInsertKey(App.grdCust, e, keys);
};
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
};
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true);
}

var isValidSel = function (data) {
    if (Ext.isEmpty(data.SalesRouteID) || Ext.isEmpty(data.SlsFreq) || Ext.isEmpty(data.WeekofVisit))
        return false;
    var iVisit = 0;
    if (data.Mon)
    {
        iVisit = iVisit + 1;
    }
    if (data.Tue)
    {
        iVisit = iVisit + 1;
    }
    if (data.Wed)
    {
        iVisit = iVisit + 1;
    }
    if (data.Thu)
    {
        iVisit = iVisit + 1;
    }
    if (data.Fri)
    {
        iVisit = iVisit + 1;
    }
    if (data.Sat)
    {
        iVisit = iVisit + 1;
    }
    if (data.Sun)
    {
        iVisit = iVisit + 1;
    }
    if (data.SlsFreq)
    {
        switch (data.SlsFreq)
        {
            case "F1":
            case "F2":
            case "F4":
            case "F4A":
                if (iVisit != 1)
                {
                    return false;
                }
                break;
            case "F8":
            case "F8A":
                if (iVisit != 2)
                {
                    return false;
                }
                break;
            case "F12":
                if (iVisit != 3)
                {
                    return false;
                }
                break;
            case "F16":
                if (iVisit != 4)
                {
                    return false;
                }
                break;
            case "F20":
                if (iVisit != 5)
                {
                    return false;
                }
                break;
            case "F24":
                if (iVisit != 6)
                {
                    return false;
                }
                break;
            case "A":
                if (iVisit == 0)
                {
                    return false;
                }
                break;
        }
    }
    return true;
}

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto) {
    _Change = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'AR20500');

    HQ.common.showBusy(false);
    stoChanged(App.stoCust);
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        _Change = false;
        App.stoCust.reload();
    }
};
var renderWeekofVisit = function (value) {
    var obj = App.stoAR20500_WeekofVisitAll.findRecord("Code", value);
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

///////////////////////////////////////////////////////////////////////