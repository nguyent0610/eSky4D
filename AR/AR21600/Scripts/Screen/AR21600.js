

var menuClick = function (command) {
    switch (command) {
        case "first":
            App.SelectionModelSI_Area.select(0);
            break;
        case "prev":
            App.SelectionModelSI_Area.selectPrevious();
            break;
        case "next":
            App.SelectionModelSI_Area.selectNext();
            break;
        case "last":
            App.SelectionModelSI_Area.select(App.storeGrid.getCount() - 1);
            break;
        case "refresh":
            if (App.cboObject.getValue() != "" && App.cboFromBranch.getValue() != "") {
                if (App.cboObject.getValue() == "AR20400") {
                    App.storeGridDetailFromCust.reload();
                    App.storeGridDetailToCust.reload();
                } else if (App.cboObject.getValue() == "AR20200") {
                    App.storeGridDetailFromSls.reload();
                    App.storeGridDetailToSls.reload();
                }

            }
            break;
        case "new":
            if (isInsert) {
                //var createdItems = App.storeGrid.getChangedData().Created;
                //if (createdItems != undefined) {
                //    App.storeGrid.loadPage(Math.ceil(App.storeGrid.totalCount / App.storeGrid.pageSize), {
                //        callback: function () {
                //            App.SelectionModelSI_Area.select(App.storeGrid.getCount() - 1);
                //            App.grd.editingPlugin.startEditByPosition({ row: App.storeGrid.getCount() - 1, column: 1 });

                //        }
                //    });
                //    return;
                //}
                //App.storeGrid.loadPage(Math.ceil(App.storeGrid.totalCount / App.storeGrid.pageSize), {
                //    callback: function () {
                //        App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());//Ext.data.Record()
                //        App.SelectionModelSI_Area.select(App.storeGrid.getCount() - 1);
                //        App.grd.editingPlugin.startEditByPosition({ row: App.storeGrid.getCount() - 1, column: 1 });
                //    }
                //});



            }
            break;
        case "delete":

            //if (isDelete) {
            //    if (App.SelectionModelSI_Area.selected.items[0] != undefined) {
            //        callMessage(11, '', 'deleteRecord');
            //    }
            //}
            break;
        case "save":
     
            if (isUpdate || isInsert || isDelete) {
                if (App.storeGridDetailFromCust.getChangedData().Deleted || App.storeGridDetailFromSls.getChangedData().Deleted) {
                    if(App.storeGridDetailFromCust.getChangedData().Created || App.storeGridDetailToCust.getChangedData().Created){
                       Save();
                    } else if (App.storeGridDetailFromCust.getChangedData().Created && App.storeGridDetailToCust.getChangedData().Created){
                       Save();
                    } else if (App.storeGridDetailFromSls.getChangedData().Created || App.storeGridDetailToSls.getChangedData().Created) {
                        Save();
                    } else if (App.storeGridDetailFromSls.getChangedData().Created && App.storeGridDetailToSls.getChangedData().Created) {
                        Save();
                    }
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
    if (App.dataForm.isValid()) {
        App.dataForm.submit({
            waitMsg: 'Submiting...',
            url: 'AR21600/Save',
            params: {
                lstgrdFromCust: Ext.encode(App.storeGridDetailFromCust.getChangedData({ skipIdForPhantomRecords: false })),
                lstgrdFromSls: Ext.encode(App.storeGridDetailFromSls.getChangedData({ skipIdForPhantomRecords: false })),
                lstgrdToCust: Ext.encode(App.storeGridDetailToCust.getChangedData({ skipIdForPhantomRecords: false })),
                lstgrdToSls: Ext.encode(App.storeGridDetailToSls.getChangedData({ skipIdForPhantomRecords: false })),
                object1: App.cboObject.getValue(),
                task: App.cboTask.getValue(),
                fromBranch: App.cboFromBranch.getValue(),
                toBranch: App.cboToBranch.getValue(),
                fromStatus: App.cboFromStatus.getValue(),
                toStatus: App.cboToStatus.getValue(),
                reason: App.txtReason.getValue(),
            },
            success: function (f, result) {
                menuClick("refresh");
                callMessage(201405071, '', null);

            }
            , failure: function (f, errorMsg) {
                callMessage(19, '', null);
            }
        });
    }
}
// Xem lai
function Close() {
    if (App.storeGrid.getChangedData().Updated == undefined && App.storeGrid.getChangedData().Deleted == undefined)
        parent.App.tabAR21600.close();
    else if (App.storeGrid.getChangedData().Updated != undefined || App.storeGrid.getChangedData().Created != undefined || App.storeGrid.getChangedData().Deleted != undefined) {
        App.direct.AskClose({
            success: function (result) {

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
        if (parent.App.tabAR21600 != null)
            parent.App.tabAR21600.close();
    }
};
// Xac nhan xoa record tren grid
var deleteRecord = function (item) {
    if (item == "yes") {
        App.grd.deleteSelected();

    }
};

//check value
var isAllValidKey = function (items) {
    if (items != undefined) {
        for (var i = 0; i < items.length; i++) {
            for (var j = 0; j < strKeyGrid.length; j++) {
                if (items[i][strKeyGrid[j]] == '' || items[i][strKeyGrid[j]] == undefined)
                    return false;
            }
        }
        return true;
    } else {
        return true;
    }
};

function selectRecord(grid, record) {
    var record = grid.store.getById(id);
    grid.store.loadPage(grid.store.findPage(record), {
        callback: function () {
            grid.getSelectionModel().select(record);
        }
    });
};
var grd_BeforeEdit = function (editor, e) {
    if (!isUpdate) return false;
    
    strKeyGrid = e.record.idProperty.split(',');
    var time = new Date();


    //if (App.SelectionModelSI_Area.selected.items[0].data.DateInMonth.getDate() < time.getDate()) {
    //    return false;
    //}

    


    if (strKeyGrid.indexOf(e.field) != -1) {
        //if (e.record.data.AreaCode.trim() == "") {
        //    return true;
        //} else {
        //    return false;
        //}
    }
    //if (e.record.data.AreaCode.trim() == "") {
    //    return false;
    //}

};
var grd_Edit = function (item, e) {

    if (strKeyGrid.indexOf(e.field) != -1) {
        if (e.value != '' && isAllValidKey(App.storeGrid.getChangedData().Created) && isAllValidKey(App.storeGrid.getChangedData().Updated))
            App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());//Ext.data.Record() 
    }
};
var grd_ValidateEdit = function (item, e) {

    if (strKeyGrid.indexOf(e.field) != -1) {
        if (duplicated(App.storeGrid, e)) {
            callMessage(1112, e.value, '');
            return false;
        }
    }
};

var grd_CancelEdit = function (editor, e) {
    if (e.record.phantom) {
        e.store.remove(e.record);
    }
};
var grd_Reject = function (record) {
    if (record.data.tstamp == '') {
        App.storeGrid.remove(record);
        App.grd.getView().focusRow(App.storeGrid.getCount() - 1);
        App.grd.getSelectionModel().select(App.storeGrid.getCount() - 1);
    } else record.reject();
};

//Phan trang tren grid
var onComboBoxSelect = function (combo) {
    var store = combo.up("gridpanel").getStore();
    store.pageSize = parseInt(combo.getValue(), 10);
    store.reload();
};

var LoadDefaulValue = function () {

};

var cboObject_Change = function (sender,e) {
    setTimeout(function () { waitcboObjectChangeToReloadStoreStatus(); }, 1000);
    if (App.cboObject.getValue() == "AR20400") {
        App.grdFromCust.show();
        App.grdFromSls.hide();
        App.grdToCust.show();
        App.grdToSls.hide();
    } else if (App.cboObject.getValue() == "AR20200") {
        App.grdFromSls.show();
        App.grdFromCust.hide();
        App.grdToSls.show();
        App.grdToCust.hide();
    }
};

var waitcboObjectChangeToReloadStoreStatus = function () {
    App.cboFromStatus.getStore().reload();
    App.cboToStatus.getStore().reload();
    
};

var cboFromBranch_Change = function () {
    if (App.cboToBranch.getValue() == App.cboFromBranch.getValue()) {
        App.cboToBranch.setValue("")
    }
    if (App.cboObject.getValue() && App.cboFromStatus.getValue()) {
        if (App.cboObject.getValue() == "AR20400") {
            App.storeGridDetailFromCust.reload();
        } else if (App.cboObject.getValue() == "AR20200") {
            App.storeGridDetailFromSls.reload();
        }

    }
};

var cboFromStatus_Change = function () {
    if (App.cboToStatus.getValue() == App.cboFromStatus.getValue()) {
        App.cboToStatus.setValue("")
    }
    if (App.cboObject.getValue() && App.cboFromBranch.getValue()) {
        if (App.cboObject.getValue() == "AR20400") {
            App.storeGridDetailFromCust.reload();
        } else if (App.cboObject.getValue() == "AR20200") {
            App.storeGridDetailFromSls.reload();
        }
        
    }
};



var cboTask_Change = function () {
    if (App.cboTask.getValue() == "CB") {
        App.cboToBranch.enable(true);
        App.cboToStatus.setValue("")
        App.cboToStatus.disable(true);
    } else if (App.cboTask.getValue() == "CS") {
        App.cboToStatus.enable(true);
        App.cboToBranch.setValue("")
        App.cboToBranch.disable(true);
       
    }
};

//check ko cho 2 cai FromBranch va ToBranch chon trung
var cboToBranch_Change = function () {
    if (App.cboToBranch.getValue() == App.cboFromBranch.getValue()) {
        App.cboToBranch.setValue("")
    }
}
//check ko cho 2 cai FromStatus va ToStatus chon trung
var cboToStatus_Change = function () {
    if (App.cboToStatus.getValue() == App.cboFromStatus.getValue()) {
        App.cboToStatus.setValue("")
    }
}



























//4 nut bam chuyen record qua lai giua 2 Grid

var btnAllLeftToRight = function () {
    if (App.cboObject.getValue() == "AR20400" ) {
        if ((App.cboTask.getValue() == "CB" && App.cboToBranch.getValue()) || (App.cboTask.getValue() == "CS" && App.cboToStatus.getValue())) {
            for (var i = 0; i <= App.storeGridDetailFromCust.data.length; i++) {
                App.SelectionModelDetailFromCust.select(0);
                var recordGrdToCust = Ext.create("App.ObjectModelToCust", {
                    ToCustId: App.SelectionModelDetailFromCust.selected.items[0].data.CustId,
                    ToCustName: App.SelectionModelDetailFromCust.selected.items[0].data.CustName,
                    ToCustAddress: App.SelectionModelDetailFromCust.selected.items[0].data.Address,
                    ToCustStatus: App.SelectionModelDetailFromCust.selected.items[0].data.Status,
                });
                App.storeGridDetailToCust.insert(App.storeGridDetailToCust.getCount(), recordGrdToCust);
                App.grdFromCust.deleteSelected();
                i = 0;
            }
        }
    } else if (App.cboObject.getValue() == "AR20200") {
        if ((App.cboTask.getValue() == "CB" && App.cboToBranch.getValue() ) || (App.cboTask.getValue() == "CS" && App.cboToStatus.getValue() )) {
            for (var i = 0; i <= App.storeGridDetailFromSls.data.length; i++) {
                App.SelectionModelDetailFromSls.select(0);
                var recordGrdToSls = Ext.create("App.ObjectModelToSls", {
                    ToSlsId: App.SelectionModelDetailFromSls.selected.items[0].data.SlsperId,
                    ToSlsName: App.SelectionModelDetailFromSls.selected.items[0].data.Name,
                    ToSlsAddress: App.SelectionModelDetailFromSls.selected.items[0].data.Address,
                    ToSlsStatus: App.SelectionModelDetailFromSls.selected.items[0].data.Status,
                });
                App.storeGridDetailToSls.insert(App.storeGridDetailToSls.getCount(), recordGrdToSls);
                App.grdFromSls.deleteSelected();
                i = 0;
            }
        }
    }
};

var btnLeftToRight = function () {
    if (App.cboObject.getValue() == "AR20400") {
        if ((App.cboTask.getValue() == "CB" && App.cboToBranch.getValue()) || (App.cboTask.getValue() == "CS" && App.cboToStatus.getValue())) {
            for (var i = 0; i < App.SelectionModelDetailFromCust.selected.length; i++) {

                var recordGrdToCust = Ext.create("App.ObjectModelToCust", {
                    ToCustId: App.SelectionModelDetailFromCust.selected.items[i].data.CustId,
                    ToCustName: App.SelectionModelDetailFromCust.selected.items[i].data.CustName,
                    ToCustAddress: App.SelectionModelDetailFromCust.selected.items[i].data.Address,
                    ToCustStatus: App.SelectionModelDetailFromCust.selected.items[i].data.Status,
                });
                App.storeGridDetailToCust.insert(App.storeGridDetailToCust.getCount(), recordGrdToCust);


            }
            App.grdFromCust.deleteSelected();
        }
        
    } else if (App.cboObject.getValue() == "AR20200") {
        if ((App.cboTask.getValue() == "CB" && App.cboToBranch.getValue()) || (App.cboTask.getValue() == "CS" && App.cboToStatus.getValue())) {
            for (var i = 0; i < App.SelectionModelDetailFromSls.selected.length; i++) {

                var recordGrdToSls = Ext.create("App.ObjectModelToSls", {
                    ToSlsId: App.SelectionModelDetailFromSls.selected.items[i].data.SlsperId,
                    ToSlsName: App.SelectionModelDetailFromSls.selected.items[i].data.Name,
                    ToSlsAddress: App.SelectionModelDetailFromSls.selected.items[i].data.Address,
                    ToSlsStatus: App.SelectionModelDetailFromSls.selected.items[i].data.Status,
                });
                App.storeGridDetailToSls.insert(App.storeGridDetailToSls.getCount(), recordGrdToSls);


            }
            App.grdFromSls.deleteSelected();
        }
    }
};

var btnRightToLeft = function () {
    if (App.cboObject.getValue() == "AR20400") {
        if ((App.cboTask.getValue() == "CB" && App.cboToBranch.getValue() ) || (App.cboTask.getValue() == "CS" && App.cboToStatus.getValue() )) {
            for (var i = 0; i < App.SelectionModelDetailToCust.selected.length; i++) {

                var recordGrdFromCust = Ext.create("App.ObjectModelFromCust", {
                    CustId: App.SelectionModelDetailToCust.selected.items[i].data.ToCustId,
                    CustName: App.SelectionModelDetailToCust.selected.items[i].data.ToCustName,
                    Address: App.SelectionModelDetailToCust.selected.items[i].data.ToCustAddress,
                    Status: App.SelectionModelDetailToCust.selected.items[i].data.ToCustStatus,
                });
                App.storeGridDetailFromCust.insert(App.storeGridDetailFromCust.getCount(), recordGrdFromCust);


            }
            App.grdToCust.deleteSelected();
        }

    } else if (App.cboObject.getValue() == "AR20200") {
        if ((App.cboTask.getValue() == "CB" && App.cboToBranch.getValue()) || (App.cboTask.getValue() == "CS" && App.cboToStatus.getValue())) {
            for (var i = 0; i < App.SelectionModelDetailToSls.selected.length; i++) {

                var recordGrdFromSls = Ext.create("App.ObjectModelFromSls", {
                    SlsperId: App.SelectionModelDetailToSls.selected.items[i].data.ToSlsId,
                    Name: App.SelectionModelDetailToSls.selected.items[i].data.ToSlsName,
                    Address: App.SelectionModelDetailToSls.selected.items[i].data.ToSlsAddress,
                    Status: App.SelectionModelDetailToSls.selected.items[i].data.ToSlsStatus,
                });
                App.storeGridDetailFromSls.insert(App.storeGridDetailFromSls.getCount(), recordGrdFromSls);


            }
            App.grdToSls.deleteSelected();
        }
    }
};

var btnAllRightToLeft = function () {
    if (App.cboObject.getValue() == "AR20400") {
        if ((App.cboTask.getValue() == "CB" && App.cboToBranch.getValue()) || (App.cboTask.getValue() == "CS" && App.cboToStatus.getValue())) {
            for (var i = 0; i <= App.storeGridDetailToCust.data.length; i++) {
                App.SelectionModelDetailToCust.select(0);
                var recordGrdFromCust = Ext.create("App.ObjectModelFromCust", {
                    CustId: App.SelectionModelDetailToCust.selected.items[0].data.ToCustId,
                    CustName: App.SelectionModelDetailToCust.selected.items[0].data.ToCustName,
                    Address: App.SelectionModelDetailToCust.selected.items[0].data.ToCustAddress,
                    Status: App.SelectionModelDetailToCust.selected.items[0].data.ToCustStatus,
                });
                App.storeGridDetailFromCust.insert(App.storeGridDetailFromCust.getCount(), recordGrdFromCust);
                App.grdToCust.deleteSelected();
                i = 0;
            }
        }
    } else if (App.cboObject.getValue() == "AR20200") {
        if ((App.cboTask.getValue() == "CB" && App.cboToBranch.getValue()) || (App.cboTask.getValue() == "CS" && App.cboToStatus.getValue() )) {
            for (var i = 0; i <= App.storeGridDetailToSls.data.length; i++) {
                App.SelectionModelDetailToSls.select(0);
                var recordGrdFromSls = Ext.create("App.ObjectModelFromSls", {
                    SlsperId: App.SelectionModelDetailToSls.selected.items[0].data.ToSlsId,
                    Name: App.SelectionModelDetailToSls.selected.items[0].data.ToSlsName,
                    Address: App.SelectionModelDetailToSls.selected.items[0].data.ToSlsAddress,
                    Status: App.SelectionModelDetailToSls.selected.items[0].data.ToSlsStatus,
                });
                App.storeGridDetailFromSls.insert(App.storeGridDetailFromSls.getCount(), recordGrdFromSls);
                App.grdToSls.deleteSelected();
                i = 0;
            }
        }
    }
};