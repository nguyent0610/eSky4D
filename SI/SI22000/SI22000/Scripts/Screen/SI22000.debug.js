////Declare//////////////////////////////////////////////////////////
var focusrecord = 0;//biến tạm focusrecord để biết đang focus vào form hay Grid để xác định các xử lý các nút phía trên
//khai bao dung cho kiem tra tren grid, co nhieu luoi thi dat ten theo luoi
var keys = ['CycleNbr'];
var fieldsCheckRequire = ["CycleNbr", "StartDate", "EndDate"];
var fieldsLangCheckRequire = ["CycleNbr", "StartDate", "EndDate"];

var holdStatus="H";
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
// Load and show binding data to the form
// Command of the topbar on screen
var menuClick = function (command) {
    switch (command) {
        case "first":
             if (HQ.focus == 'grdCycle') {
                HQ.grid.first(App.grdCycle);
            }
            break;
        case "next":
             if (HQ.focus == 'grdCycle') {
                HQ.grid.next(App.grdCycle);
            }
            break;
        case "prev":
          if (HQ.focus == 'grdCycle') {
                HQ.grid.prev(App.grdCycle);
            }
            break;
        
        case "last":
             if (HQ.focus == 'grdCycle') {
                HQ.grid.last(App.grdCycle);
            }
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.DialogCycle.close();
                App.stoCycle.reload();
            }
            break;
        case "new": break;
        case "delete":break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                //checkRequire để kiếm tra các field yêu cầu có rỗng hay ko
                if ( HQ.store.checkRequirePass(App.stoCycle, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    save();
                }
            }
            break;
        case "print":           
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
    App.stoCycle.reload();
};

var loadSourceCombo = function () {
    App.cboStatus.getStore().load(function () {
        App.cboHandle.getStore().load();
        App.stoCycle.reload();
    });
};

//load lần đầu khi mở
var firstLoad = function () {
    HQ.isFirstLoad = true;
    loadSourceCombo();
   
   
    if (HQ.config.split(',').length > 1) {
        if (HQ.config.split(',')[1] == "1") {
            App.cboStatus.show();
            
        }
        else {
            App.cboStatus.hide();
           
        }
    }
    if (HQ.config.split(',').length > 2) {
        if (HQ.config.split(',')[2] == "1") {
            App.cboHandle.show();
           
        }
        else {
            App.cboHandle.hide();
          
        }
    }
};

var frmChange = function () {
    //App.frmMain.getForm().updateRecord
    if (HQ.store.isChange(App.storeCon)) {
        HQ.isChange = HQ.store.isChange(App.stoCycle);
    }
    else {
        HQ.isChange = true;
    }
    HQ.common.changeData(HQ.isChange, 'SI22000');//co thay doi du lieu gan * tren tab title header
    //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
};

//xu li su kiem tren luoi giong nhu luoi binh thuong
var grdCycle_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};

var grdCycle_Edit = function (item, e) {
    if (HQ.config.split(',').length > 0) {
        if (HQ.config.split(',')[0] == "1") {
            HQ.grid.checkInsertKey(App.grdCycle, e, keys);
        }
    }
   
};

var grdCycle_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdCycle, e, keys);
};

var grdCycle_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdCycle);
    frmChange();
};

function cboYearNbr_click() {
    App.stoCycle.reload();
};

function btnCalendar_click() {
    if (App.dateKPI.getRawValue()) {
        App.DialogCycle.show();
        var date = new Date(App.dateKPI.getRawValue(), 1, 1);
        while (date.getDay() != 1) {
            date = addDays(date, 1);
        }
        App.df_STARTDATE.setValue(date);
    }
};

var loadDataAutoDetail = function (sto) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            var newRecord = Ext.create("App.MdlSI_Cycle", {
                Status: holdStatus
            });
            HQ.store.insertRecord(sto, keys, newRecord);
        }
        HQ.isFirstLoad = false;
    }
    var record = sto.getAt(0);
    if (record) {
        App.cboStatus.setValue(record.data.Status);
    }
    //firstLoad();
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
// Submit the changed data (created, updated) into server side
function save() {
    //dòng này để bắt các thay đổi của form 
    //App.frmMain.getForm().updateRecord();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: 'Submiting...',
            url: 'SI22000/Save',
            params: {
                lstgrd: HQ.store.getData(App.stoCycle),
            },
            success: function (msg, data) {
                HQ.message.show(201405071, '', '');
                HQ.isChange = false;
                menuClick("refresh");
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};


var btn_OK_click = function () {
    
    var rad1= App.rad1.checked;
    var rad2= App.rad2.checked;
    var rad3 = App.rad3.checked;

    if (rad1) {
        calCyle( 4, 4, 5);
    } else if (rad2) {
        calCyle( 4, 5, 4);
    } else if (rad3) {
        calCyle( 5, 4, 4);
    }
    
};

function calCyle(n1, n2, n3) {
    var startdate = new Date(App.df_STARTDATE.getValue());
    var enddate = new Date();
    var lstCycle = App.stoCycle;

    //if (lstCycle.getCount() > 0) {
    //    var control = 0;
    //    for (var i = 0; i < lstCycle.getCount() ; i++) {
    //        lstCycle.getAt(i).data.StartDate = startdate;
    //        if (control == 0) {
    //            startdate = addDays(startdate, 7 * n1);
    //            enddate = startdate;
    //            control++;
    //        }
    //        else if (control == 1) {
    //            startdate = addDays(startdate, 7 * n2);
    //            enddate = startdate;
    //            control++;
    //        }
    //        else if (control == 2) {
    //            startdate = addDays(startdate, 7 * n3);
    //            enddate = startdate;
    //            control = 0;
    //        }
    //        while (enddate.getDay() != 6) {
    //            enddate = addDays(enddate, -1);
    //        }

    //        lstCycle.getAt(i).set("EndDate", enddate);

    //        startdate = enddate;
    //        while (startdate.getDay() != 1) {
    //            startdate = addDays(startdate, 1);
    //        }
    //    }
    //} else {
    if (HQ.store.getAllData(App.stoCycle, ['CycleNbr'], [''], false).length == 2) {
        App.stoCycle.removeAll();
        var control = 0;
        var j = 0;
        for (var i = 0; i < 12; i++) {
            j++;
            var data = {
                CycleNbr: App.dateKPI.getRawValue() + (j < 10 ? ('0' + j) : j),
                StartDate: startdate
            };
            var record = Ext.create("App.MdlSI_Cycle", data); // convert data thanh record cua store lstCycle
            lstCycle.insert(i, record);

            if (control == 0) {
                startdate = addDays(startdate, 7 * n1);
                enddate = startdate;
                control++;
            }
            else if (control == 1) {
                startdate = addDays(startdate, 7 * n2);
                enddate = startdate;
                control++;
            }
            else if (control == 2) {
                startdate = addDays(startdate, 7 * n3);
                enddate = startdate;
                control = 0;
            }
            while (enddate.getDay() != 6) {
                enddate = addDays(enddate, -1);
            }

            lstCycle.getAt(i).set("EndDate", enddate);

            startdate = enddate;
            while (startdate.getDay() != 1) {
                startdate = addDays(startdate, 1);
            }
        }
    }
    //}
    App.DialogCycle.close();
};

function addDays(theDate, days) {
    return new Date(theDate.getTime() + days * 24 * 60 * 60 * 1000);
};


//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = true;
        App.DialogCycle.close();
        App.stoCycle.reload();
    }
};
///////////////////////////////////