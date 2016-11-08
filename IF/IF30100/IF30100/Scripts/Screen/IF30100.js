//// Declare //////////////////////////////////////////////////////////
HQ.parm = [];
var stoDet_Load = function (store, records, success) {
    store.loaded = true;
    HQ.common.showBusy(false);
    HQ.isFirstLoad = true;
    HQ.common.showBusy(false);
    HQ.isChange = HQ.store.isChange(store);
}
var stoDet_BeforeLoad  = function (store, records, success) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
}
var stoReport_Load = function () {
    if (App.cboReport.store.data.items.length > 0) {
        App.cboReport.setValue(App.cboReport.store.data.items[0].data.ReportNbr);
    }
}
var menuClick = function (command) {
    switch (command) {
        case "refresh":           
            HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
            App.grdDet.getStore().reload();
            App.lblResult.setText('');
            break;

    }
};


//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {

    HQ.isFirstLoad = true;

    HQ.common.showBusy(true, HQ.waitMsg);
    App.cboType.store.addListener('load', function () {
        HQ.common.showBusy(false);
        if (HQ.screenNbr != 'IF30100') {
            App.cboType.setValue('P');
        } else {
            App.cboType.setValue('E');
        }
        App.btnTemplate.hide();
        App.cboReport.store.reload();
    });

  
};

var getComboValue = function (val) {
    if (val == null) return '';
    return val.join(',');
}
var getDate = function (val,type) {
    if (type == 'Y') {
        new Date(val.getYear(), 0, 1)
    } else if (type == 'M') {
        new Date(val.getYear(), val.getMonth(), 1)
    } else if (type == 'D') {
        return val;
    }
  
}
var getValue = function (val) {
    if (val == null) return '';
    return val;
}
var getGridValue = function (grd) {
    var data = grd.store.allData == undefined ? grd.store.data : grd.store.allData;
    var selData = '';
    data.each(function (item) {
        if (item.data.Sel) {
            selData += item.data[grd.tag] + ',';
        }
    });
    return selData;
}

var chkHeader_Change = function (ct, column, e, t) {
    var data = column.grid.store.allData == undefined ? column.grid.store.data : column.grid.store.allData;
    if (Ext.fly(t).hasCls("my-header-checkbox")) {
        data.each(function (item) {
            item.set('Sel', t.checked ? true : false);
        });
    }
    
}
var getParm = function () {
    var result = []
    Ext.each(HQ.parm,function(item,index){
        if (item.type == 'text') {
            result.push({ Name: item.id, Value: App[item.id].getValue() });
        } else if (item.type == 'combo') {
            result.push({ Name: item.id, Value: Ext.isEmpty(App[item.id].getValue()) ? '' : (typeof App[item.id].getValue() === 'string' ? App[item.id].getValue() : App[item.id].getValue().join(',')) });
        } else if (item.type == 'date') {
            result.push({ Name: item.id, Value: App[item.id].getValue() });
        } else if (item.type == 'grid') {
            var value = '';
            var grid = App[item.id];
            var data = grid.store.allData == undefined ? grid.store.data : grid.store.allData;
            data.each(function (item2) {
                if (item2.data.Sel == true) {
                    value += item2.data[grid.tag] + ';';
                }
            });
            result.push({ Name: item.id, Value: value });
        }
    });
    return Ext.encode(result);
}
var getView = function(){
    var report = HQ.store.findInStore(App.cboReport.store, ['ReportNbr'], [App.cboReport.getValue()])
    if (!Ext.isEmpty(report)) {
        return report.ReportView;
    }
    return '';
}

var btnAdd_Click = function () {
    App.frmTemplate.submit({
        //waitMsg: HQ.common.getLang("Exporting"),
        url: 'IF30100/Save',
        type: 'POST',
        timeout: 1000000,
        clientValidation: false,
        params: {
        },
        success: function (msg, data) {
            HQ.message.process(msg, data, true);
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });
}

var cboType_Change = function (sender, newValue, oldValue) {
    if (sender.valueModels != null) {
        if (newValue == 'E') {
            App.grdDet.show();
            App.pnlFilter.hide();
        } else {
            App.grdDet.hide();
            App.pnlFilter.show();

        }
        //App.btnTemplate.show();
        App.pnlFilterHeader.hide();
        App.pnlFilterHeader.items.clear();
        App.tabFilterGrid.hide();
        App.tabFilterGrid.items.clear();
        App.cboReport.events['change'].suspend();
        App.cboReport.setValue('');
        App.cboReport.events['change'].resume();
        App.cboReport.store.reload();
    }

};

var cboReport_Change = function (sender, newValue, oldValue) {
    HQ.parm = [];
    if (sender.valueModels != null ) {
        if (App.cboType.getValue() == 'E') {
            App.btnTemplate.hide();
            App.stoDet.reload();
        } else {
            //App.btnTemplate.show();
            App.pnlFilterHeader.items.each(function (item) {
                App.pnlFilterHeader.remove(item);
                App[item.id] = null;
            })
            App.tabFilterGrid.items.each(function (item) {
                App.tabFilterGrid.remove(item)
                App[item.id] = null;
            })
            App.tabFilterGrid.hide();
            HQ.common.showBusy(true, HQ.waitMsg);
            App.direct.IF30100Filter(App.cboReport.getValue(), {
                success: function (result) {
                    Ext.each(HQ.parm, function (item, index) {
                        if (item.type == 'grid') {
                            App[item.id].store.reload();
                        }
                    });

                    HQ.common.showBusy(false);
                }
            });
        }
           
    }
};

var btnImport_Click = function (c, e) {
    
};

var checkGrid = function (store, field) {
    var count = 0;
    var allRecords = store.snapshot || store.allData || store.data;
    allRecords.each(function (record) {
        if (record.data[field]) {
            count++;
            return false;
        }
    });
    if (count > 0)
        return true;
    else
        return false;
};


var btnExport_Click = function () {
    if (checkGrid(App.stoDet, 'Checked') == false) {
        HQ.message.show(2016110710);
    } else {
        if (HQ.form.checkRequirePass(App.frmMain)) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang("Exporting"),
                url: App.cboType.getValue() == 'E' ? 'IF30100/Export' : 'IF30100/ExportPivot',
                type: 'POST',
                timeout: 2000000,
                clientValidation: false,
                params: {
                    lstDet: Ext.encode(App.stoDet.getRecordsValues()),
                    view: App.cboReport.valueModels[0].data.ReportView,
                    name: App.cboReport.valueModels[0].data.ReportName,
                    proc: App.lblResult.getText(),
                    data: getParm()
                },
                success: function (msg, data) {
                    if (!Ext.isEmpty(data.result.name)) {
                        window.location = 'IF30100/DownloadFile?name=' + data.result.name + '&id=' + data.result.id;
                    }

                    HQ.message.process(msg, data, true);
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
};

var btnTemplate_Click = function () {
    App.winTemplate.show();
};

//trước khi load trang busy la dang load data
var stoBeforeLoad = function (sto) {
    //HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};

var colCheck_Header_Change = function (value) {
    if (value) {
        App.stoDet.suspendEvents();
        App.stoDet.each(function (item) {
            item.set("Checked", value.checked);
        });
        App.stoDet.resumeEvents();
        App.grdDet.view.refresh();
    }
};

var frmMain_BoxReady = function () {
    if (HQ.screenNbr != 'IF30100') {
        App.cboType.hide();
        App.grdDet.hide();
    }
    App.cboReport.store.addListener('load', stoReport_Load);
};


var grdDet_Edit = function (item, e) {
    if (App.cboReport.valueModels != null && App.cboReport.valueModels.length > 0) {
        getWhere(App.cboReport.valueModels[0].data.ReportView);
    }
};

var getWhere = function (view) {
    var select = '';
    var param = '';
    var proc = '';
    var store = App.stoDet;
    var allRecords = store.snapshot || store.allData || store.data;
    store.suspendEvents();
    allRecords.each(function (record) {
        if (record.data.Checked == true) {
            select += record.data.Column_Name + ",";
            if (record.data.Operator.toUpperCase().trim() == "BETWEEN") {
                param += record.data.Column_Name + " BETWEEN " + (record.data.Data_Type.toUpperCase() == "NVARCHAR" ? "N'" : "'") + record.data.Value1 + "' AND " + (record.data.Data_Type.toUpperCase() == "NVARCHAR" ? "N'" : "'") + record.data.Value2 + "' AND ";
            }
            else if (record.data.Operator.toUpperCase().trim() == "AND") {
                param += record.data.Column_Name + " = " + (record.data.Data_Type.toUpperCase() == "NVARCHAR" ? "N'" : "'") + record.data.Value1 + "' AND " + record.data.Column_Name + " = " + (record.data.Data_Type.toUpperCase() == "NVARCHAR" ? "N'" : "'") + record.data.Value2 + "' AND ";
            }
            else if (record.data.Operator.toUpperCase().trim() == "OR") {
                param += record.data.Column_Name + " = " + (record.data.Data_Type.toUpperCase() == "NVARCHAR" ? "N'" : "'") + record.data.Value1 + "' OR " + record.data.Column_Name + " = " + (record.data.Data_Type.toUpperCase() == "NVARCHAR" ? "N'" : "'") + record.data.Value2 + "' AND ";
            }
            else if (record.data.Operator.toUpperCase().trim() == "IN") {
                param += record.data.Column_Name + " IN('" + record.data.Value1.replace(",", "','") + "') AND ";
            }
            else if(record.data.Operator) param += record.data.Column_Name + " " + record.data.Operator + " " + (record.data.Data_Type.toUpperCase() == "NVARCHAR" ? "N'" : "'") + record.data.Value1 + "' AND ";
        }
    });
    store.resumeEvents();
    param = param.length > 3 ? " WHERE " + param.substring(0, param.length - 4) : param;
    proc = "SELECT " + select.replace(/(^,)|(,$)/g, "") + " FROM " + view + param;
    App.lblResult.setText(proc);
};