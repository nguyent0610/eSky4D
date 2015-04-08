
var selectedIndex = 0;
var _hold = "H";
var _curSlsperid = "";
var _focusrecord = 0;
var beforeedit = '';
var tmpAreaChange = 0;
   
var menuClick = function (command) {
    switch (command) {
        case "first":       

            break;
        case "prev":
    
            break;
        case "next":
  
            break;
        case "last":
    
            break;
        case "refresh":

            break;
        case "new":

            break;
        case "delete":

            break;
        case "save":

            break;
        case "print":
            alert(command);
            break;
        case "close":
            HQ.common.close(this);
            break;
    }
       
};

var dateKPI_expand = function (dte, eOpts) {
    dte.picker.setWidth(100);
    dte.picker.monthEl.hide();
    dte.picker.monthEl.setWidth(0);
};

var dateKPI_Select = function (sender, e) {
    App.cboCycle.store.reload();
};

var cboCycle_Change = function (sender, e) {
    if (!e) {
        App.txtStartDate.setValue('');
        App.txtEndDate.setValue('');
    }
};

var cboCycle_Select = function (sender, e) {
    if (e) {
        var start = sender.displayTplData[0].StartDate;
        App.txtStartDate.setValue(Ext.Date.format(start, 'm/d/Y'));
        var end = sender.displayTplData[0].EndDate;
        App.txtEndDate.setValue(Ext.Date.format(end, 'm/d/Y'));
    }
    else {
        App.txtStartDate.setValue('');
        App.txtEndDate.setValue('');
    }
};

var cboBranchID_Change = function (sender, e) {
    if (!e) {
        App.txtBranchName.setValue('');
    }
};

var cboBranchID_Select = function (sender, e) {
    if (e) {
        var BranchName = sender.displayTplData[0].BranchName;
        App.txtBranchName.setValue(BranchName);
    }
    else {
        App.txtBranchName.setValue('');
    }
};


var ImportExel = function () {
    App.dataForm.submit({
        waitMsg: "Importing....",
        url: 'OM22100/OM22100Import',
        timeout:18000000,
        clientValidation: false,
        method: 'POST',
        params: {
            //BranchID: App.cboBranchID.getValue()
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
        url: 'OM22100/Export',
        type: 'POST',
        timeout: 1000000,
        params: {
            BranchID: App.cboBranchID.getValue(),
            BranchName: App.txtBranchName.getValue(),
            CycleNbr: App.cboCycle.getValue(),
            StartDate: App.txtStartDate.getValue(),
            EndDate: App.txtEndDate.getValue()
        },
        success: function (msg, data) {
            var filePath = data.result.filePath;
            if (filePath) {
               // window.location = "OM22100/Download?filePath=" + filePath + "&fileName=MCP_" + App.cboBranchID.getValue();
            }
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });
};
