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
    if (HQ.screenNbr != 'IF30100') {//Nếu là gọi màn hình từ mã khác
        //ẩn combo type đi
        App.cboType.hide();
    }
    HQ.isFirstLoad = true;

    HQ.common.showBusy(true, HQ.waitMsg);
    App.cboType.store.addListener('load', stocboType_load);
    App.cboReport.store.addListener('load', stocboReport_load);
  
    App.cboType.store.reload();
    App.cboReport.events['change'].suspend();
};

var stocboReport_load = function (sto) {
    if(sto.data.items.length>0)
        if (HQ.screenNbr != 'IF30100') {//Nếu là gọi màn hình từ mã khác
            //ẩn combo type đi
            App.cboType.hide();
            App.cboType.setValue(sto.data.items[0].data.Type);
            setTimeout(function () {
                App.cboReport.events['change'].resume();
                App.cboReport.setValue(sto.data.items[0].data.ReportNbr);
            }, 100);
        } else {
            App.cboType.show();
            App.cboReport.events['change'].resume();
            App.cboReport.setValue(sto.data.items[0].data.ReportNbr);
        }
}
var stocboType_load = function (sto) {
    HQ.common.showBusy(false);
    //if (HQ.screenNbr != 'IF30100') {
    //    App.cboType.setValue('P');
    //} else {
    App.cboType.setValue(HQ.type);
    //}
    App.btnTemplate.hide();
    if (HQ.screenNbr != 'IF30100') {
        App.cboReport.store.reload();
    }
}
var getComboValue = function (val) {
    if (val == null) return '';

    if (val.constructor === Array) return val.join(',')
    else return val;
}
var getDate = function (val,type) {
    if (type == 'Y') {
        return new Date(val.getFullYear(), 0, 1)
    } else if (type == 'M') {
        return new Date(val.getFullYear(), val.getMonth(), 1)
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

function grdChoiceColumn_BeforeEdit(sender, e) {
    if (e.field == 'Format' && !e.record.data.IsFormat) return false;
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
            App.grdDet.hide();
            App.pnlFilter.hide();
        } else {
            App.grdDet.hide();
            App.pnlProc.hide();
            App.pnlFilter.show();

        }
        //App.btnTemplate.show();
        App.pnlFilterHeader.hide();
        App.pnlFilterHeader.items.clear();
        App.tabFilterGrid.hide();
        App.tabFilterGrid.items.clear();
        if (HQ.screenNbr == 'IF30100') {//Nếu là gọi màn hình từ IF30100
            App.cboReport.events['change'].suspend();
            App.cboReport.setValue('');
            App.cboReport.events['change'].resume();
            App.cboReport.store.reload();
        } else {
        }
    }

};

var cboReport_Change = function (sender, newValue, oldValue) {
    HQ.parm = [];
    if (sender.valueModels != null && App.cboReport.value) {
        if (App.cboType.getValue() == 'E') {
            App.btnTemplate.hide();
            if (sender.valueModels[0].data.SourceType.toUpperCase().indexOf('V')==0)
            {
                App.grdDet.show();
                App.pnlProc.hide();
                App.stoDet.reload();
            }
            else if (sender.valueModels[0].data.SourceType.toUpperCase().indexOf('P')==0)
            {
                App.grdDet.hide();
                App.pnlProc.show();
                HQ.common.showBusy(true, HQ.waitMsg);
                var i = 0;
                loadParam(i,sender.valueModels[0].data.ReportNbr, sender.valueModels[0].data.ReportView);
            }
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
    if(App.cboType.getValue() != 'E')
    {   
        if (HQ.form.checkRequirePass(App.frmMain)) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang("Exporting"),
                url: 'IF30100/ExportPivot',
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
    else if (App.cboReport.valueModels[0].data.SourceType.toUpperCase().indexOf('V')==0) {
        exportExcelView();
    }
    else if (App.cboReport.valueModels[0].data.SourceType.toUpperCase().indexOf('P')==0) {
        exportExcelProc();
    }
}

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
        getWhere(App.cboReport.valueModels[0].data.ReportView);
        App.stoDet.resumeEvents();
        if (App.grdDet.view)
        App.grdDet.view.refresh();
    }
};
var ColCheckChoiceColumn_Change = function (value) {
    if (value) {
        App.stoChoiceColumn.suspendEvents();
        App.stoChoiceColumn.each(function (item) {
            item.set("Checked", value.checked);
        });
        App.stoChoiceColumn.resumeEvents();
        if(App.grdstoChoiceColumn.view)
        App.grdstoChoiceColumn.view.refresh();
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

var loadParam = function (i, reportNbr, reportView) {

    App.direct.IF30100LoadRPTParm(reportNbr, reportView, {
        success: function (result) {
            setTimeout(function () {
                App.btnLoadParamList.fireEvent("click");
                App.stoChoiceColumn.reload();
                HQ.common.showBusy(false);
            }, 200);
        }
    });

}
//function exportExcelProc() {
//    HQ.common.showBusy(true, HQ.common.getLang("Exporting"));
//    App.direct.IF30100ExportProc(        
//        {
//        timeout: 180000,
//        success: function (msg, data) {
//            if (!Ext.isEmpty(msg.name)) {
//                window.location = 'IF30100/DownloadFile?name=' + msg.name + '&id=' + msg.id;
//                HQ.common.showBusy(false);
//            }
//           // HQ.message.process(msg, data, true);
//        }
//            ,
//            failure: function (msg, data) {
//                //HQ.message.process(msg, data, true);
//                HQ.common.showBusy(false);
//            }
//        });
//}
function exportExcelProc() {
    if (checkGrid(App.stoChoiceColumn, 'Checked') == false) {
        HQ.message.show(201710091);
        App.pnlProc.setActiveTab(1);
        return;
    }
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("Exporting"),
            method: 'POST',
            timeout: 18000000,
            url: 'IF30100/ExportProcFileName',
            params: {
                name: App.cboReport.valueModels[0].data.ReportName
            },
            success: function (msg, data) {
                if (!Ext.isEmpty(data.result.id)) {
                    exportExcelProcFile(data.result.id, data.result.name);
                }
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
}
function downloadFile(idfile, name) {
    
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("CheckingFile"),
            method: 'POST',
            timeout: 18000000,
            url: 'IF30100/CheckFile',
            params: {
                name: name,
                id: idfile
            },
            success: function (msg, data) {
                if (!Ext.isEmpty(data.result.name)) {
                    //setTimeout(function () {                       
                        window.location = 'IF30100/DownloadAndDelete?name=' + data.result.name + '&id=' + data.result.id;
                    //}, 5000);
                }
                //HQ.message.process(msg, data, true);
            },
            failure: function (msg, data) {
                if (data.failureType == "connect")
                    downloadFile(idfile, name);
                else HQ.message.process(msg, data, true);                
            }
        });
    }
}

function exportExcelProcFile(idfile,name) {
    if (checkGrid(App.stoChoiceColumn, 'Checked') == false) {
        HQ.message.show(201710091);
        App.pnlProc.setActiveTab(1);
        return;
    }
    var List0 = '';
    var List1 = '';
    var List2 = '';
    var List3 = '';
    var ChoiceColumn = '';
    App.List0.store.suspendEvents();
    var allData0 = App.List0.store.snapshot || App.List0.store.allData || App.List0.store.data;
    allData0.each(function (record) {
        if (record.data.Selected)
            List0 += record.data[App.List0.tag] + ',';
    });
    App.List0.store.resumeEvents();

    App.List1.store.suspendEvents();
    var allData1 = App.List1.store.snapshot || App.List1.store.allData || App.List1.store.data;
    allData1.each(function (record) {
        if (record.data.Selected)
            List1 += record.data[App.List1.tag] + ',';
    });
    App.List1.store.resumeEvents();

    App.List2.store.suspendEvents();
    var allData2 = App.List2.store.snapshot || App.List2.store.allData || App.List2.store.data;
    allData2.each(function (record) {
        if (record.data.Selected)
            List2 += record.data[App.List2.tag] + ',';
    });
    App.List2.store.resumeEvents();

    App.List3.store.suspendEvents();
    var allData3 = App.List3.store.snapshot || App.List3.store.allData || App.List3.store.data;
    allData3.each(function (record) {
        if (record.data.Selected)
            List3 += record.data[App.List3.tag] + ',';
    });
    App.List3.store.resumeEvents();
    App.stoChoiceColumn.suspendEvents();
    var allChoiceColumn = App.stoChoiceColumn.snapshot || App.stoChoiceColumn.allData || App.stoChoiceColumn.data;
    allChoiceColumn.each(function (record) {
        if (record.data.Checked)
            ChoiceColumn += record.data.ColumnName + '@';
    });
    App.stoChoiceColumn.resumeEvents();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("Exporting"),
            method: 'POST',
            timeout: 18000000,
            url: 'IF30100/ExportProc',
            params: {
                list0: List0,//App.List0.getSelectionSubmit().getSelectionModelField().getValue(),
                list1: List1,//App.List1.getSelectionSubmit().getSelectionModelField().getValue(),
                list2: List2,//App.List2.getSelectionSubmit().getSelectionModelField().getValue(),
                list3: List3,//App.List3.getSelectionSubmit().getSelectionModelField().getValue(),
                choiceColumn: ChoiceColumn,
                listChoice: Ext.encode(App.stoChoiceColumn.getRecordsValues()),
                proc: App.cboReport.valueModels[0].data.ReportView,
                name: name,
                reportNbr: App.cboReport.valueModels[0].data.ReportNbr,
                IsReadOnly: App.cboReport.valueModels[0].data.IsReadOnly,
                id:idfile

            },
            success: function (msg, data) {
                if (!Ext.isEmpty(data.result.name)) {
                    window.location = 'IF30100/DownloadAndDelete?name=' + data.result.name + '&id=' + data.result.id;
                }
                HQ.message.process(msg, data, true);
            },
            failure: function (msg, data) {
                if(data.failureType=="connect")
                    downloadFile(idfile, name);
                else HQ.message.process(msg, data, true);
            }
        });
    }
};
function exportExcelView()
{
    if (checkGrid(App.stoDet, 'Checked') == false) {
        HQ.message.show(2016110710);
    } else {
        if (HQ.form.checkRequirePass(App.frmMain)) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang("Exporting"),
                url:  'IF30100/ExportView' ,
                type: 'POST',
                timeout: 2000000,
                clientValidation: false,
                params: {
                    lstDet: Ext.encode(App.stoDet.getRecordsValues()),
                    view: App.cboReport.valueModels[0].data.ReportView,
                    name: App.cboReport.valueModels[0].data.ReportName,                   
                    proc: App.lblResult.getText(),
                    IsReadOnly: App.cboReport.valueModels[0].data.IsReadOnly,
                    data: getParm()
                },
                success: function (msg, data) {
                    if (!Ext.isEmpty(data.result.name)) {
                        window.location = 'IF30100/DownloadAndDelete?name=' + data.result.name + '&id=' + data.result.id;
                    }

                    HQ.message.process(msg, data, true);
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    }
}
var chkList0_change = function (value) {
    App.List0.store.suspendEvents();
    var allData = App.List0.store.allData || App.List0.store.data;
    allData.each(function (record) {
        record.set("Selected", value.checked);
    });
    App.List0.store.resumeEvents();
    App.List0.view.refresh();
}

var chkList1_change = function (value) {
    App.List1.store.suspendEvents();
    var allData = App.List1.store.allData || App.List1.store.data;
    allData.each(function (record) {
        record.set("Selected", value.checked);
    });
    App.List1.store.resumeEvents();
    App.List1.view.refresh();
}

var chkList2_change = function (value) {
    App.List2.store.suspendEvents();
    var allData = App.List2.store.allData || App.List2.store.data;
    allData.each(function (record) {
        record.set("Selected", value.checked);
    });
    App.List2.store.resumeEvents();
    App.List2.view.refresh();
}
var chkList3_change = function (value) {
    App.List3.store.suspendEvents();
    var allData = App.List3.store.allData || App.List3.store.data;
    allData.each(function (record) {
        record.set("Selected", value.checked);
    });
    App.List3.store.resumeEvents();
    App.List3.view.refresh();
}
var List_Load = function (countStore) {
    HQ.SourceList++;
    if (HQ.SourceList == countStore) {
        setTimeout(function () {        
            HQ.common.showBusy(false);           
        }, 100);
    }
}
var tabList_TabChange = function (sender, item, active) {
    item.view.refresh();
}