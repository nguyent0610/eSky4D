
var selectedIndex = 0;
var _hold = "H";
var _curSlsperid = "";
var _focusrecord = 0;
var beforeedit = '';
var tmpAreaChange = 0;
   
var menuClick = function (command) {
    switch (command) {
        case "first":       
            App.SelectionModelMailAutoDetail.select(0);
            break;
        case "prev":
            App.SelectionModelMailAutoDetail.selectPrevious();
            break;
        case "next":
            App.SelectionModelMailAutoDetail.selectNext();
            break;
        case "last":
            App.SelectionModelMailAutoDetail.select(App.storeMailHeader.getCount() - 1);
            break;
        case "refresh":
            App.storeMailHeader.reload();
          
            break;
        case "new":
            if (isInsert) {
                    App.SelectionModelMailAutoDetail.select(App.storeMailHeader.getCount() -1);
                    
                    if (App.storeMailHeader.getCount() == 0) {

                        App.storeMailHeader.insert(App.storeMailHeader.getCount(), Ext.data.Record());
                        App.SelectionModelMailAutoDetail.select(App.storeMailHeader.getCount() - 1);
                        App.grd.editingPlugin.startEditByPosition({ row: App.storeMailHeader.getCount() - 1, column: 1 });
                    } else if (App.SelectionModelMailAutoDetail.selected.items[0].data.AreaCode != "") {
                        App.storeMailHeader.insert(App.storeMailHeader.getCount(), Ext.data.Record());
                        App.SelectionModelMailAutoDetail.select(App.storeMailHeader.getCount() - 1);
                        App.grd.editingPlugin.startEditByPosition({ row: App.storeMailHeader.getCount() - 1, column: 1 });
                    } 
            }
            break;
        case "delete":
            if (App.SelectionModelMailAutoDetail.selected.items[0] != undefined) {
                if (isDelete) {

                    HQ.message.show(11, '', 'deleteRecordGrid')

                }
            }
            break;
        case "save":
            if (App.SelectionModelMailAutoDetail.selectNext() == false) {
                App.SelectionModelMailAutoDetail.selectPrevious();
            } else {
                App.SelectionModelMailAutoDetail.selectNext();
            }

            if (isUpdate || isInsert || isDelete) {
              
                if (isAllValid(App.storeMailHeader.getChangedData().Created)
                        && isAllValid(App.storeMailHeader.getChangedData().Updated)) {
                      
                        Save();
                    }
           
                  
            }
            break;
        case "print":
            alert(command);
            break;
        case "close":
            Close();
            break;
    }
       
}; 
function Save() {


    var tmpCheckSDSMDSM = 0;
    if (App.storeMailHeader.getChangedData().Updated != undefined) {
        for (var i = 0; i < App.storeMailHeader.getChangedData().Updated.length; i++) {
            if (App.storeMailHeader.getChangedData().Updated[i].SDSMCode == "" && App.storeMailHeader.getChangedData().Updated[i].DSMCode == "") {
                tmpCheckSDSMDSM = 1;
                HQ.message.show(20142811, '', null);
            }
        }
    }
    if (tmpCheckSDSMDSM == 0) {
        //App.dataForm.getForm().updateRecord();
        if (App.dataForm.isValid()) {
            App.dataForm.submit({
                waitMsg: 'Submiting...',
                url: 'OM22100/Save',
                params: {
                    lstgrd: Ext.encode(App.storeMailHeader.getChangedData({ skipIdForPhantomRecords: false })),//,



                },
                success: function (result, data) {
                    //App.cboCategoryType.getStore().reload();
                    menuClick("refresh");
                    HQ.message.show(201405071, '', null);

                    //var dt = Ext.decode(data.response.responseText);


                    App.storeMailHeader.reload();
                }
                , failure: function (errorMsg, data) {

                    var dt = Ext.decode(data.response.responseText);
                    HQ.message.show(dt.code, dt.colName + ',' + dt.value, null);
                }
            });
        }
    }
}
// Xem lai
function Close() {
    if (App.storeMailHeader.getChangedData().Updated == undefined && App.storeMailHeader.getChangedData().Deleted == undefined && App.storeMailHeader.getChangedData().Updated == undefined && App.storeMailHeader.getChangedData().Deleted == undefined)
        parent.App.tabOM22100.close();
    else if (App.storeMailHeader.getChangedData().Updated != undefined || App.storeMailHeader.getChangedData().Created != undefined || App.storeMailHeader.getChangedData().Deleted != undefined || App.storeMailHeader.getChangedData().Updated != undefined || App.storeMailHeader.getChangedData().Created != undefined || App.storeMailHeader.getChangedData().Deleted != undefined)
    {
        App.direct.AskClose({ success: function (result) {
                    
        }
        });
    }
}
// Xem lai
var askClose = function (item) {
    if (item == "yes") {
        
            Save();
    }
    else {
        if (parent.App.tabOM22100 != null)
            parent.App.tabOM22100.close();
    }
};


var ImportExel = function () {
    App.dataForm.submit({
        waitMsg: "Importing....",
        url: 'OM22100/OM22100Import',
        timeout:1800,
        clientValidation: false,
        method: 'POST',
        params: {
            BranchID: App.cboBranchID.getValue()
           

        },
        success: function (msg, data) {
            if (!Ext.isEmpty(this.result.data.message)) {
                HQ.message.show('2013103001', [this.result.data.message], '', true);
            }
            else {
                HQ.message.process(msg, data, true);
            }

        },
        failure: function (msg, data) {           
           HQ.message.process(msg, data, true);
        }
    });
}

var ImportTemplate_Change = function (sender, e) {

    var fileName = sender.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (ext == "xls" || ext == "xlsx") {
        ImportExel();

    } else {
        alert("Please choose a Media! (.xls, .xlsx)");
        sender.reset();
    }
};


var ExportExel = function () {
    App.dataForm.submit({
        //waitMsg: HQ.common.getLang("Exporting")+"...",
        url: 'OM22100/Export',
        type: 'POST',
        timeout: 1000000,
        params: {
            BranchID: App.cboBranchID.getValue(),//,
            BranchName: App.cboBranchID.getDisplayValue(),
            SlsPerID: App.cboSlsPerID.getValue(),
            RouteID: App.cboRouteID.getValue()
        },
        success: function (msg, data) {
            //processMessage(msg, data, true);
            //menuClick('refresh');
            var filePath = data.result.filePath;
            if (filePath) {
                window.location = "OM22100/Download?filePath=" + filePath + "&fileName=MCP_" + App.cboBranchID.getValue();
            }
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });


            //try {
            //    App.direct.Export1(App.cboMailID.getValue(), {
            //        success: function (data) {
            //        processMessage(msg, data, true);
            //        menuClick('refresh');
            //        },
            //        failure: function () {
            //            //
            //        },
            //        eventMask: { msg: '@Util.GetLang("Exported")', showMask: true }
            //    });
            //} catch (ex) {
            //    alert(ex.message);
            //}

}
var cboBranchID_Change = function () {
    App.cboSlsPerID.getStore().reload();
    App.cboRouteID.getStore().reload();
};
