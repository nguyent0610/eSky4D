
var _beginStatus = "H";
var _Source = 0;
var _maxSource = 7;
var _isLoadMaster = false;
var CpnyID = '';
var SlsperID = '';
var _Flag;
var _HandleCombo = '';
var keys = ['CustID'];
var fieldsCheckRequire = ["SlsperId"];
var fieldsLangCheckRequire = ["SlsperId"];
var _type = '';
var _status = '';
var _position = '';
var _slsperID = '';
var lstposition = [];
var lstslsperID = [];
var _levelFrom = '';
var _lineRef = '';
var _toPercent = '';
var _accumulateID = '';
var _NewOrEdit = '';

var checkSpecial = '';
//// Declare //////////////////////////////////////////////////////////

var keyCust = ['CustID'];
var keyPro = ['InvtID'];
var keyProduct = ['InvtID', 'Lineref'];
var fieldsCheckRequire = ["CustID"];
var fieldsLangCheckRequire = ["CustID"];
var _Source = 0;
var _levelID = '';
var _maxSource = 1;
var _isLoadMaster = false;


///////////////////////////////////////////////////////////////////////
//// Store ////////////////////////////////////////////////////////////

var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        //App.stoTaxCat.reload();
        HQ.common.showBusy(false);
    }
};

var setEmpty = function ()
{
    App.txtProgramID.setValue('');
    App.txtDescr.setValue('');
    App.cboTypeAccumulationMain.setValue('');
    App.cboMonthYear.setValue('');
    App.cboApplyTo.setValue('');
    App.dtpFromDay.setValue('');
    App.cboCheckSalesBy.setValue('');
    App.dtpToDay.setValue('');
    App.ckbUsing.setValue('');
    App.cboStatus.setValue('H')
 //   App.cboHandle.setValue("Empty");
}
var setReloadGrid = function()
{
    App.stoLoadGridReality.reload();
    App.stoAccuAmt.reload();
    App.stoGridProduct.reload();
    App.stoData.reload();
}
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuMainClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdTaxCat);
            break;
        case "prev":
            HQ.grid.prev(App.grdTaxCat);
            break;
        case "next":
            HQ.grid.next(App.grdTaxCat);
            break;
        case "last":
            HQ.grid.last(App.grdTaxCat);
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.stoDisplay.reload();
                App.stoAccuAmt.reload();
                App.grdReality.store.reload();
                App.grdProduct.store.reload();
            }
            break;
        case "new":
            if (HQ.isChange) {
                HQ.message.show(150, '', 'refreshFirst');
            }
            else {
                App.frmFirst.hide();
                App.frmMain.show();
                App.frmMain.setTitle("New");
                //  frmMain_boxReady();
                App.stoDisplay.reload();
                
              
              //  App.txtSlsperID.setValue('');
            }

            break;
        case "delete":
            if (HQ.isDelete )
            {
                if (App.cboStatus.value != "C")
                {
                    if (HQ.focus == "ProgramID")
                    {
                        if (_NewOrEdit == "Edit")
                        {
                            if (App.txtProgramID.getValue())
                            {
                                HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdOM_Accumulated)], 'deleteHeader', true);
                            }
                        }
                        else
                        {
                            setEmpty();
                            setReloadGrid();
                        }
                    }
                    else
                    {
                        if (_NewOrEdit == "Edit")
                        {
                            setEmpty();
                            setReloadGrid();
                        }

                        if (App.slmCust.selected.items[0] != undefined) {
                            if (App.slmCust.selected.items[0].data.RoleID != "")
                            {
                                HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdCust)], 'deleteDataCust', true);
                            }
                        }
                        if (App.slmProduct.selected.items[0] != undefined) {
                            if (App.slmProduct.selected.items[0].data.RoleID != "") {
                                HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdProduct)], 'deleteDataProduct', true);
                            }
                        }
                        if (App.slmReality.selected.items[0] != undefined) {
                            if (App.slmReality.selected.items[0].data.RoleID != "") {
                                HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdReality)], 'deleteDataReality', true);
                            }
                        }
                        if (App.slmAccuAmt.selected.items[0] != undefined) {
                            if (App.slmAccuAmt.selected.items[0].data.RoleID != "") {
                                HQ.message.show(2015020806, [HQ.grid.indexSelect(App.GrdAccuAmt)], 'deleteDataAccuAmt', true);
                            }
                        }

                    }

                }
                else
                {
                    HQ.message.show(2018121350);
                }
            }
            
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                //if (HQ.store.checkRequirePass(App.stoTaxCat, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                //nếu từ new ra thì 
                saveData();
                ///nếu từ edit thì
                pnlDetailCTTL_Active();
                //}
            }
            break;
        case "print":
            break;
        case "close":
//HQ.common.close(this);
            break;
    }
};

function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoDisplay.reload();
        App.stoAccuAmt.reload();
        App.grdReality.store.reload();
        App.grdProduct.store.reload();
    }
};

var deleteDataProduct = function (item) {
    if (item == "yes") {
        App.grdProduct.deleteSelected();
        frmChange();
    }
};
var deleteDataCust = function (item) {
    if (item == "yes") {
        App.grdCust.deleteSelected();
        frmChange();
    }
};

var deleteDataReality = function (item) {
    if (item == "yes") {
        App.grdReality.deleteSelected();
        frmChange();
    }
}; 
var deleteDataAccuAmt = function (item) {
    if (item == "yes") {
        App.GrdAccuAmt.deleteSelected();
        frmChange();
    }
};
//Close Form Main nếu thay đổi thì thông báoo mới Close

var btnHome_Click = function () {
    if (HQ.isChange) {
        HQ.message.show(5, '', 'askClose', true);
    } else {
        App.frmFirst.show();
        App.frmMain.hide();
        App.grdOM_Accumulated.store.reload();
        //App.stoOM_Accumulated.clearData();
        //App.grdOM_Accumulated.view.refresh();
    }
};

var askClose = function (item) {
    if (item != 'yes') {
        App.grdOM_Accumulated.store.reload();
        App.frmMain.hide();
        App.frmFirst.show();
    } else {
        App.frmMain.show();
        App.frmFirst.hide();
    }
}

var menuFirstClick = function (command) {
    switch (command) {
        case "first":
            
            break;
        case "next":
            
            break;
        case "prev":
            
            break;

        case "last":
            
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'Process.refresh');
            }
            else {
                App.grdOM_Accumulated.store.reload();
            }

            break;
        case "new":
            if (HQ.isChange) {
                HQ.message.show(150, '', 'refreshFirst');
            }
            else {
                App.frmFirst.hide();
                App.frmMain.show();
                App.frmMain.setTitle("New");
                App.frmMain.isValid();
                App.txtProgramID.setValue("");
                App.stoDisplay.reload();
                App.txtProgramID.setReadOnly(false);
                _NewOrEdit = 'New';
                setEmpty();
                //  App.cboStatus.reload();
                App.cboApplyTo.setReadOnly(false);
                App.cboCheckSalesBy.setReadOnly(false);
                setReloadGrid();
                checkreadOnly();
                //set button realdy
                App.btnAddAllInvtID.enable(true);
                App.btnAddInvtID.enable(true);
                App.btnDeleteInvtID.enable(true);
                App.btnDeleteAllInvtID.enable(true);
                checkbutton();

            }

            break;
        case "delete":
            if (HQ.isDelete)
            {
                if (App.slmGrid.selected.items[0] != undefined) {
                    if (App.slmGrid.selected.items[0].data.AccumulateID != "") {
                        HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdOM_Accumulated)], 'deleteDataAll', true);
                    }
                }
            }
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
//load khi giao dien da load xong, gan  HQ.isFirstLoad=true de biet la load lan dau
var firstLoad = function () {
    HQ.util.checkAccessRight();
    debugger
    HQ.isFirstLoad = true;
  //  App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    //App.cboBuyer.getStore().addListener('load', checkLoad);
    checkLoad();
};

var frmChange = function () {
    _accumulateID = App.txtProgramID.getValue();    
    HQ.isChange = HQ.store.isChange(App.stoData)
            || HQ.store.isChange(App.stoOM_Accumulated)
            || HQ.store.isChange(App.stoLoadGridReality)
            || HQ.store.isChange(App.stoAccuAmt)
            || HQ.store.isChange(App.stoGridProduct)
            || HQ.store.isChange(App.grdOM_Accumulated.store)
            || HQ.store.isChange(App.grdCust.store)
            || HQ.store.isChange(App.grdReality.store)
            || HQ.store.isChange(App.GrdAccuAmt.store)
            || HQ.store.isChange(App.grdProduct.store)
            || HQ.store.isChange(App.stoDisplay);
    HQ.common.changeData(HQ.isChange, 'OM20320');//co thay doi du lieu gan * tren tab title header
    if (HQ.isChange) {

    }
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};


var grdTaxCat_BeforeEdit = function (editor, e) {
    if (!HQ.grid.checkBeforeEdit(e, keys)) return false;
};

var grdTaxCat_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdTaxCat, e, keys);
    frmChange();
};

var grdTaxCat_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdTaxCat, e, keys);
};

var dtpFromDay_Change = function (a,newValue, oldValue)
{
    App.dtpToDay.setMinValue(newValue);
}
var grdTaxCat_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdTaxCat);
    frmChange();
};

var grdAccuAmt_Reject = function (record) {
    
    HQ.grid.checkReject(record, App.GrdAccuAmt);
    var record = HQ.store.findInStore(App.stoAccuAmt, ['From'], ['']);
    if (!record) {
        HQ.store.insertBlank(App.stoAccuAmt, ['From']);
    }
    grdAccuAmtChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
    frmChange();
};

var grdAccuAmtChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoAccuAmt);
    HQ.common.changeData(HQ.isChange, 'OM20320');
};

/////////////////////////////////////
////////////////////////////////////
////Process
////Function menuClick
var save = function () {

    App.frmMain.getForm().updateRecord();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'OM20320/Save',
            params: {
                lstTaxCat: HQ.store.getData(App.stoTaxCat)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                HQ.isFirstLoad = true;
                App.stoTaxCat.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdTaxCat.deleteSelected();
        frmChange();
    }
};
var deleteDataAll = function (item) {
    
    if (item == "yes") {
        App.frmFirst.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang('Deleting') + "...",
            url: 'OM20320/DeleteAllAccumulateID',
            params: {
                accumulateID: _accumulateID,
                status: _status
            },
            success: function (msg, data) {
                if (data.result.msgCode) {
                    HQ.message.show(data.result.msgCode);
                }
                App.grdOM_Accumulated.deleteSelected();
                App.grdOM_Accumulated.store.reload();
                App.stoDisplay.reload();
                App.stoAccuAmt.reload();
                App.stoGridProduct.reload();
                App.stoLoadGridReality.reload();
                frmChangeFirst();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
}

var deleteHeader = function (item) {
    if (item == "yes") {
        
        App.frmFirst.submit({
            timeout: 1800000,
            waitMsg: HQ.common.getLang('Deleting') + "...",
            url: 'OM20320/DeleteAllAccumulateID',
            params: {
                accumulateID: _accumulateID
            },
            success: function (msg, data) {
                HQ.message.show(data.result.msgCode);
                App.grdOM_Accumulated.deleteSelected();
                App.grdOM_Accumulated.store.reload();
                frmChangeFirst();   
                setEmpty();
                setReloadGrid();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
}
///////////////////////////////////////////////
//////////////////////////////////////////////
//Other function
function refreshFirst(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoAR_Salesperson.reload();
    }
};

function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        HQ.isFirstLoad = true;
        App.stoTaxCat.reload();
    }
};


//// DEV : NhanLH date : 11/1/2018  ///
// store reload
var cboTypeAccumulationMain_Change = function ()  // xét trạng thái require cho fromDate todate
{
    
    //loadTreeCustomer();
    if (App.cboTypeAccumulationMain.lastValue == 'A' || App.cboStatus.value == 'C') {

        App.cboMonthYear.allowBlank = true;
        App.cboMonthYear.isValid();
        App.dtpFromDay.isValid();
        App.dtpToDay.isValid();
        App.dtpFromDay.setReadOnly(false);
        App.dtpToDay.setReadOnly(false);
        App.cboMonthYear.setReadOnly(true);
        App.cboMonthYear.setValue('');
        App.btnAddAllInvtID.disable(true);
        App.btnAddInvtID.disable(true);
        App.btnDeleteInvtID.disable(true);
        App.btnDeleteAllInvtID.disable(true);

    }
    else {
        App.btnAddAllInvtID.enable(true);
        App.btnAddInvtID.enable(true);
        App.btnDeleteInvtID.enable(true);
        App.btnDeleteAllInvtID.enable(true);
        App.cboMonthYear.setReadOnly(false);
        App.cboMonthYear.allowBlank = false;
        App.cboMonthYear.isValid();
    }
    if (App.cboTypeAccumulationMain.lastValue == 'M') {
        treeCustomer_AfterRender('M');
        App.dtpFromDay.setReadOnly(true);
        App.dtpToDay.setReadOnly(true);
    }
    if (App.cboTypeAccumulationMain.lastValue == 'T') {
        treeCustomer_AfterRender('T')
        App.dtpFromDay.setReadOnly(true);
        App.dtpToDay.setReadOnly(true);
    }
    if (App.cboTypeAccumulationMain.lastValue == 'Y') {
        treeCustomer_AfterRender('Y')
        App.dtpFromDay.setReadOnly(true);
        App.dtpToDay.setReadOnly(true);
    }
    if (App.cboTypeAccumulationMain.lastValue == 'C') {
        treeCustomer_AfterRender('C')
        App.dtpFromDay.setReadOnly(true);
        App.dtpToDay.setReadOnly(true);
    } 
    if (App.cboTypeAccumulationMain.lastValue == 'A') {
        treeCustomer_AfterRender('A')
    }
    if (_NewOrEdit == 'Edit')
    {
        App.grdCust.store.reload();
    }
    App.cboMonthYear.store.reload();
    App.cboMonthYear.clear();
    App.dtpToDay.clear();
    App.dtpFromDay.clear();
    

 //   treeCustomer_AfterRender();
};

var grdCheckSalesBy_Edit = function () {
    if (App.cboCheckSalesBy.lastValue == 'P')
    {
        App.tabReward.child('#tabBonusLevel').tab.hide()
        App.tabReward.child('#tabProduct').tab.show();
        App.tabReward.setActiveTab(1)

    }
    else
    {
        App.tabReward.child('#tabProduct').tab.hide();
        App.tabReward.child('#tabBonusLevel').tab.show();
        App.tabReward.setActiveTab(0);
    }
};

///load theo cbo
var grdMonthYear_TriggerClick = function()
{

    App.cboMonthYear.clearValue()
    App.dtpFromDay.setValue('');
    App.dtpToDay.setValue('');
}
var cboTypeAccumulationMain_TriggerClick = function()
{
    App.cboTypeAccumulationMain.clearValue()
    App.dtpFromDay.setValue('');
    App.dtpToDay.setValue('');
}
var grdMonthYear_Edit = function (item, e) {


    var obj = HQ.store.findInStore(App.cboMonthYear.store, ["TypeAccumulateAll"], [App.cboMonthYear.getValue()]);
    if (obj != undefined)
    {
        App.dtpFromDay.setValue(obj.FromDay);
        App.dtpToDay.setValue(obj.ToDay);
    }
    if (App.cboMonthYear.lastValue == '')
    {
        App.dtpFromDay.setValue('');
        App.dtpToDay.setValue('');
    }
    frmChange();
};

/// tree view



var treeApplyCustommer_checkChange = function (node, checked, eOpts) {
     
    node.childNodes.forEach(function (childNode) {
        childNode.set("checked", checked);
    });
};

var nodeClick = function (sender, value) {
    var store = value.childNodes;
    for (var i = 0; i < store.length; i++) {
        var flag = false;
        lstposition.forEach(function (item) {
            if (item == store[i].data.Type) flag = true;
        });
        if (!flag)
            lstposition.push(store[i].data.Type);
        flag = false;
        lstslsperID.forEach(function (item) {
            if (item == store[i].data.RecID) flag = true;
        });
        if (!flag)
            lstslsperID.push(store[i].data.RecID);
        var storeChild = store[i].childNodes;
        for (var j = 0; j < storeChild.length; j++) {
            flag = false;
            lstposition.forEach(function (item) {
                if (item == storeChild[j].data.Type) flag = true;
            });
            if (!flag)
                lstposition.push(storeChild[j].data.Type);
            flag = false;
            lstslsperID.forEach(function (item) {
                if (item == storeChild[j].data.RecID) flag = true;
            });
            if (!flag)
                lstslsperID.push(storeChild[j].data.RecID);
        }
    }
    _position = lstposition.join(',');
    _slsperID = lstslsperID.join(',');
    if (store.length == 0) {
        _position = value.data.Type;
        _slsperID = value.data.RecID;
    }

   // App.stoAR_Salesperson.reload();
};

var treePanelCustomer_checkChange = function (node, checked) {
    //if (App.cboStatus.getValue() == _beginStatus) {
        if (node.hasChildNodes()) {
            node.eachChild(function (childNode) {
                childNode.set('checked', checked);
                treePanelCustomer_checkChange(childNode, checked);
            });
        }
    //} else {
    //    App.treePanelCustomer.clearChecked();
    //}
}

///// Đọc Dữ Liệu

var btnLoadData_Click = function () {
    if (HQ.form.checkRequirePass(App.frmFirst)) {
        App.stoOM_Accumulated.reload();
    }
};

var stoAR_SalespersonLoad = function () {
    frmChangeFirst();
};

var frmChangeFirst = function () {
    if (App.stoOM_Accumulated.getCount() > 0) {
        if (!HQ.store.isChange(App.stoOM_Accumulated)) {
            HQ.isChange = HQ.store.isChange(App.grdOM_Accumulated.store);
        }
        else {
            HQ.isChange = true;
        }
        HQ.common.changeData(HQ.isChange, 'OM20320');

    }
};

var btnEdit_Click = function (record) {
    
    App.frmMain.show();
    App.frmFirst.hide();
    App.frmMain.setTitle("Edit");
    App.txtProgramID.setReadOnly(true);
    App.cboTypeAccumulationMain.setReadOnly(false);
    App.txtProgramID.setValue(record.data.AccumulateID);
    App.txtDescr.setValue(record.data.Descr);
    App.cboTypeAccumulationMain.setValue(record.data.Type);
    App.cboMonthYear.setValue(record.data.CycleNbr);
    App.cboStatus.setValue(record.data.Status);
    App.cboApplyTo.setValue(record.data.Applyfor);
    App.dtpFromDay.setValue(record.data.FromDate);
    App.cboCheckSalesBy.setValue(record.data.BreakBy);
    App.dtpToDay.setValue(record.data.ToDate);
    App.ckbUsing.setValue(record.data.Active);
    _NewOrEdit = 'Edit';
    App.stoDisplay.reload();
    checkreadOnly();
    App.stoAccumulatedManage.reload();
    App.stoAccumulatedReward.reload();
    checkbutton();
    var Manage = HQ.store.findInStore(App.stoAccumulatedManage, ['AccumulateID'], [record.data.AccumulateID]);
    var Reward = HQ.store.findInStore(App.stoAccumulatedReward, ['AccumulateID'], [record.data.AccumulateID]);
    if (Manage || Reward) {
        App.cboTypeAccumulationMain.setReadOnly(true); // Loại Tích Lũy
        App.txtDescr.setReadOnly(true); // Diễn giải
        App.cboMonthYear.setReadOnly(true); // THnags chu kì ămn
        App.cboApplyTo.setReadOnly(true); //Ap dụng cho
        App.dtpFromDay.setReadOnly(true); // Đên Ngày 
        App.cboCheckSalesBy.setReadOnly(true); // Kiểm tra doanh số theo 
        App.dtpToDay.setReadOnly(true); // Từ Ngày
        App.ckbUsing.setReadOnly(true); //check using
    }

};

var checkbutton = function()
{
    if (App.cboStatus.value == 'C') {
        App.btnAddAll.disable(true);
        App.btnAdd.disable(true);
        App.btnDel.disable(true);
        App.btnDelAll.disable(true);
        App.btnAddAllInvtID.disable(true);
        App.btnAddInvtID.disable(true);
        App.btnDeleteInvtID.disable(true);
        App.btnDeleteAllInvtID.disable(true);
    }
    else {
        App.btnAddAllInvtID.enable(true);
        App.btnAddInvtID.enable(true);
        App.btnDeleteInvtID.enable(true);
        App.btnDeleteAllInvtID.enable(true);
        App.btnAddAll.enable(true);
        App.btnAdd.enable(true);
        App.btnDel.enable(true);
        App.btnDelAll.enable(true);
    }
}
var checkreadOnly = function ()
{
    if(App.cboStatus.value != 'C')
    {
        App.cboTypeAccumulationMain.setReadOnly(false); // Loại Tích Lũy
        App.txtDescr.setReadOnly(false); // Diễn giải
        App.cboMonthYear.setReadOnly(false); // THnags chu kì ămn
        App.cboApplyTo.setReadOnly(true); //Ap dụng cho
        App.dtpFromDay.setReadOnly(false); // Đên Ngày 
        App.cboCheckSalesBy.setReadOnly(true); // Kiểm tra doanh số theo 
        App.dtpToDay.setReadOnly(false); // Từ Ngày
        App.ckbUsing.setReadOnly(false); //check using
        if (_NewOrEdit == "New") {
            App.cboCheckSalesBy.setReadOnly(false); // Kiểm tra doanh số theo 
            App.cboApplyTo.setReadOnly(false); //Ap dụng cho
        }
    }
    else {
      //  App.txtProgramID.setReadOnly(true); // mã chương trình
        App.cboTypeAccumulationMain.setReadOnly(true); // Loại Tích Lũy
        App.txtDescr.setReadOnly(true); // Diễn giải
        App.cboMonthYear.setReadOnly(true); // THnags chu kì ămn
        App.cboApplyTo.setReadOnly(true); //Ap dụng cho
        App.dtpFromDay.setReadOnly(true); // Đên Ngày 
        App.cboCheckSalesBy.setReadOnly(true); // Kiểm tra doanh số theo 
        App.dtpToDay.setReadOnly(true); // Từ Ngày
        App.ckbUsing.setReadOnly(true); //check using
        if(_NewOrEdit == "New")
        {
            App.cboCheckSalesBy.setReadOnly(false); // Kiểm tra doanh số theo 
            App.cboApplyTo.setReadOnly(false); //Ap dụng cho
        }
    }
}

////// btn add product

var btnAddProduct_click = function (btn, e, eOpts) {
    
    if (HQ.isUpdate) {
        //if (App.frmMain.isValid()) {
        var accumulateID = App.txtProgramID.getValue();
        //  var status = App.cboStatus.value;
        if (App.grdReality.selModel.selected.items[0]) {
            var allNodes = App.treeProduct.getCheckedNodes();
            if (allNodes && allNodes.length > 0) {
                App.stoGridProduct.suspendEvents();
                allNodes.forEach(function (node) {
                    if (node.attributes.Type == "Invt") {
                        var record = HQ.store.findInStore(App.stoGridProduct, ['InvtID', 'LineRef'], [node.attributes.InvtID, node.attributes.LineRef]);

                        if (!record) {
                            HQ.store.insertBlank(App.stoGridProduct, ['InvtID']);
                            record = App.stoGridProduct.getAt(App.grdProduct.store.getCount() - 1);
                            record.set('AccumulateID', App.txtProgramID.getValue());
                            record.set('InvtID', node.attributes.InvtID);
                            record.set('Descr', node.attributes.Descr);
                            record.set('Unit', node.attributes.Unit);
                            record.set('Percent', node.attributes.Percent);
                            record.set('Qty', '1');
                            record.set("LineRef", _lineRef);
                            //if (HQ.dateInvt == true) {
                            //    record.set("StartDateInvt", App.dtpFromDate.getValue());
                            //    record.set("EndDateInvt", App.dtpToDate.getValue());
                            //}
                        }
                    }
                });
                App.stoGridProduct.resumeEvents();
                App.stoGridProduct.pageSize = parseInt(50, 10);
                App.stoGridProduct.loadPage(1);
                App.stoGridProduct.applyPaging();
                App.treeProduct.clearChecked();
                App.grdProduct.getFilterPlugin().clearFilters();
                App.grdProduct.getFilterPlugin().getFilter('LineRef').setValue([_lineRef, '']);
                App.grdProduct.getFilterPlugin().getFilter('LineRef').setActive(true);
                if (HQ.isInsert)
                {
                    var record = HQ.store.findRecord(App.stoGridProduct, ['InvtID'], ['']);
                    if (!record)
                    {
                        HQ.store.insertBlank(App.stoGridProduct, 'InvtID');
                    }
                }
            }
        }
        else {
           // Process.showFieldInvalid(App.frmMain);
        }
    }
};

var treeSaleInvt_checkChange = function (node, checked) {
    if (App.cboStatus.getValue() == _beginStatus) {
        if (node.hasChildNodes()) {
            node.eachChild(function (childNode) {
                childNode.set('checked', checked);
                treeSaleInvt_checkChange(childNode, checked);
            });
        }
    } else 
    {
        App.treeInvt.clearChecked();
    }
};

var treeApplyCustommerID_CheckChange = function (node, checked) {
    if (App.cboStatus.getValue() == _beginStatus)
    {
        if (node.hasChildNodes())
        {
            node.eachChild(function (childNode)
            {
                childNode.set('checked', checked);
                treeApplyCustommerID_CheckChange(childNode, checked);
            });
        }
    }
    else
    {
        App.treeInvt.clearChecked();
    }
};

//// grid 

var grdCust_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdCust);
    frmChange();
};

var grdProduct_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdProduct);
    frmChange();
};

var grdReality_Reject = function (record) {
    
    HQ.grid.checkReject(record, App.grdReality);
    var record = HQ.store.findInStore(App.stoLoadGridReality, ['FromPercent'], ['']);
    if (!record) {
        HQ.store.insertBlank(App.stoLoadGridReality, ['FromPercent']);
    }
    grdRealityChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
    frmChange();
};
var checkInsertKey = function (e, keys, values) {
    if (HQ.isInsert)
        if (keys != undefined && keys.length > 0 && keys[0] != '') {
            if (keys.indexOf(e.field) != -1) {
                if (e.value != '')
                    HQ.store.insertBlank(e.grid.getStore(), keys, values, e.grid);
            }
        }
    return true;
}
var grdCust_Edit = function (item, e) 
{
    
 //   if (App.cboCustID.getValue() != null)
    if (App.cboApplyTo.getValue())
    {
        if (e.value != "" && e.value != null)
        {
            App.cboApplyTo.setReadOnly(true);
        }
    }
    if (App.cboTypeAccumulationMain.getValue())
    {
        if (e.value != "" && e.value != null) {
            App.cboTypeAccumulationMain.setReadOnly(true);
        }
    }
    
    HQ.grid.checkInsertKey(App.grdCust, e, keyCust);
    checkValidateEditCust(App.grdCust, e, keyCust, '');
    if (e.field == "CustID") {
        var a = HQ.store.findRecord(App.cboCustID.store, ['CustID'], [e.value]);
        if (e.value != "" && e.value != null)
        {
            e.record.set('RefCustID', a.data.RefCustID);
            e.record.set('CustName', a.data.CustName);
            e.record.set('PharmacyType', a.data.PharmacyType);
            e.record.set('Chain', a.data.Chain);
            e.record.set('Descr', a.data.Descr);
        }
        else {
            e.record.set('RefCustID', '');
            e.record.set('CustName', '');
            e.record.set('PharmacyType', '');
            e.record.set('Chain', '');
            e.record.set('TypeCompound', '');
        }
    }
    //if (App.cboCustID.value == null)
    //{
    //    App.grdCust.deleteSelected();
    //}
    //var a = HQ.store.findInStore(App.cboCustID.store, ['CustID'], ['']);
    //if(a != undefined)
    //{
    //    HQ.store.insertBlank(App.stoData, ['CustID']);
    //}
    frmChange();
};

var grdProduct_Edit = function (item, e) {

    if (e.field == "InvtID") {
        e.record.set("LineRef", _lineRef);
    }

    if (App.cboProductID.getValue() != null)
    if (_NewOrEdit == 'Edit' && e.value != "" && e.value != null) {
        App.txtProgramID.setReadOnly(true);
    }
    HQ.grid.checkInsertKey(App.grdProduct, e, keyPro);
    if (e.field == "InvtID") {
        var a = HQ.store.findRecord(App.cboProductID.store, ['InvtID'], [e.value]);
        if (e.value != "" && e.value != null) {
            e.record.set('Descr', a.data.Descr);
            e.record.set('Unit', a.data.Unit);
            e.record.set('Pct', a.data.Pct);
        }
        else {
            e.record.set('Descr', '');
            e.record.set('Unit', '');
            e.record.set('Pct', '');
        }
    }
    var record = HQ.store.findInStore(App.stoGridProduct, ['InvtID'], ['']);
    if (!record) {
        HQ.store.insertBlank(App.stoGridProduct, ['InvtID']);
    }
    if (e.value != "" && e.value != null) {
        App.cboCheckSalesBy.setReadOnly(true);
    }

    checkValidateEdit(App.grdProduct, e, keyProduct, '');
    frmChange();
};

var grdCust_ValidateEdit = function (item, e) {
   // return HQ.grid.checkValidateEdit(App.grdCust, e, keyCust);
};

/// sto insertBlank

var stoApplyCust_Load = function (sto) { 
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
    var record = HQ.store.findInStore(App.stoData, ['CustID'], ['']);
    if (!record) {
        HQ.store.insertBlank(App.stoData, ['CustID']);
    }
    grdApplyCustChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

var grdApplyCustChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoData);
    HQ.common.changeData(HQ.isChange, 'OM20320');
};

var grdProduct_Load = function (sto) {
    
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
    var record = HQ.store.findInStore(App.stoGridProduct, ['InvtID'], ['']);
    if (!record) {
        HQ.store.insertBlank(App.stoGridProduct, ['InvtID']);
    }
    grdProductChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

var grdProductChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoGridProduct);
    HQ.common.changeData(HQ.isChange, 'OM20320');
};

var grdReality_Load = function (sto) {
    
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
    var record = HQ.store.findInStore(App.stoLoadGridReality, ['FromPercent'], ['']);
    if (!record) {
        HQ.store.insertBlank(App.stoLoadGridReality, ['FromPercent']);
    }
    grdRealityChange();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

var grdRealityChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoLoadGridReality);
    HQ.common.changeData(HQ.isChange, 'OM20320');
};

var grdAccuAmt_Load = function (sto) {
    
    HQ.common.showBusy(false, HQ.common.getLang('loadingData'));
    var record = HQ.store.findInStore(App.stoAccuAmt, ['From'], ['']);
    if (!record) {
        HQ.store.insertBlank(App.stoAccuAmt, ['From']);
    }
    grdAccuAmt();
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};

var grdAccuAmt = function () {
    var record = HQ.store.findInStore(App.stoAccuAmt, ['From'], ['']);
    if (!record) {
        HQ.store.insertBlank(App.stoAccuAmt, ['From']);
    }
    HQ.isChange = HQ.store.isChange(App.stoAccuAmt);
    HQ.common.changeData(HQ.isChange, 'OM20320');
};

//var cboCustID_Change = function (item, e)
//{
//    
//    HQ.grid.checkInsertKey(App.grdCust, e, ['CustID']);
//}

var checkSpecialChar = function (value) {
    var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/
    if (!HQ.util.passNull(value.toString()).match(regex))
    {
        HQ.message.show(2018121850);
        checkSpecial = "Error";
        App.txtProgramID.focus();
        return false;
    }
    else
    {
        checkSpecial = "ok";
    }
    for (var i = 0, n = value.length; i < n; i++) {
        if (value.charCodeAt(i) > 127)
        {
            HQ.message.show(2018121850);
            checkSpecial = "Error";
            App.txtProgramID.focus();
            return false;
        }
        else
        {
            checkSpecial = "ok";
        }
    }
    return true;
}

var txtProgramID_ChangeClick = function (value) {
    if (_NewOrEdit == 'Edit')
    {
       // App.stoDisplay.reload();
        App.stoData.reload();
        App.stoLoadGridReality.reload();
        App.stoAccuAmt.reload();
        App.stoGridProduct.reload();
    }

    App.grdReality.selModel.select(0)
   // frmChange();
    //set ReadOnly
 
};

var grdProduct_BeforeEdit = function ()
{
    if (App.cboTypeAccumulationMain.value == "A" || App.cboTypeAccumulationMain.value == null || App.cboTypeAccumulationMain.value == ""  || App.cboStatus.value == "C") {
        return false;
    }
}
var grdAccuAmt_BeforeEdit = function () {
    if (App.cboTypeAccumulationMain.value == "A" || App.cboTypeAccumulationMain.value == null || App.cboTypeAccumulationMain.value == "" || App.cboStatus.value == "C") {
        return false;
    }
}

var grdReality_BeforeEdit = function (record, e) {
    _levelFrom = e.record.data.FromPercent;
    _lineRef = e.record.data.LineRef;
    _toPercent = e.record.data.ToPercent;
    if (App.cboTypeAccumulationMain.value == "A" || App.cboTypeAccumulationMain.value == null || App.cboTypeAccumulationMain.value == "" || App.cboStatus.value == "C" )
    {
        return false;
    }
    
};

var grdCust_BeforeEdit = function () {
    if ( App.cboStatus.value == "C") {
        return false;
    }
    if (App.txtProgramID.getValue() == "")
        App.cboCustID.setReadOnly(true);
    else App.cboCustID.setReadOnly(false);
};

var slmReality_select = function (slm, selRec, idx, eOpts) {
    
    //if (App.cboApplyTo.getValue()) {
    //    if (e.value != "" && e.value != null) {
    //        App.cboApplyTo.setReadOnly(true);
    //    }
    //}
    _lineRef = selRec.data.LineRef;
    if (App.cboCheckSalesBy.getValue() == "P") {
        App.grdProduct.getFilterPlugin().clearFilters();
        App.grdProduct.getFilterPlugin().getFilter('LineRef').setValue([selRec.data.LineRef, '']);
        App.grdProduct.getFilterPlugin().getFilter('LineRef').setActive(true);
        if (HQ.isInsert) {
            var record = HQ.store.findRecord(App.stoGridProduct, ['InvtID'], ['']);
            if (!record) {
                HQ.store.insertBlank(App.stoGridProduct, 'InvtID');
            }
        }
    }
    else {
        App.GrdAccuAmt.getFilterPlugin().clearFilters();
        App.GrdAccuAmt.getFilterPlugin().getFilter('LineRef').setValue([selRec.data.LineRef, '']);
        App.GrdAccuAmt.getFilterPlugin().getFilter('LineRef').setActive(true);
        if (HQ.isInsert) {

            var record = HQ.store.findRecord(App.stoAccuAmt, ['From'], ['']);
            if (!record) {
                HQ.store.insertBlank(App.stoAccuAmt, 'From');
            }
        }
    }
};

///// click 4 button

var btnProductDelAll_click = function (btn, e, eOpts) {
    if (HQ.isUpdate)
    {
        //    if (App.frmMain.isValid()) {
        var status = App.cboStatus.value;
           if (status == _beginStatus) {
        HQ.message.show(2015020806, '', 'deleteAllProduct');
          }
    }
    else {
       // Process.showFieldInvalid(App.frmMain);
    }
    // }
    // else {
    //     HQ.message.show(4, '', '');
    //  }
};

var deleteAllProduct = function (item) {
    
    if (item == "yes")
    {
        App.stoGridProduct.loadData([], false);
        App.stoGridProduct.submitData();
        //  /// App.stoGridProduct.view.refresh();
        App.grdProduct.view.refresh()
        App.stoGridProduct.loadPage(1);
        App.treeProduct.clearChecked();
        var invtBlank = HQ.store.findRecord(App.grdProduct.store, ['InvtID'], ['']);
        if (!invtBlank) {
            App.grdProduct.store.insert(0, Ext.create("App.OM20320_pgProduct", { InvtID: '' }));
        }
        //   Event.Form.frmMain_fieldChange();

    }
};

var btnAddAllProduct_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        //if (App.frmMain.isValid())
        //{
            var accumulateID = App.txtProgramID.getValue();
            if (App.grdReality.selModel.selected.items[0]) {
                if (HQ.isUpdate) {
                    var allNodes = getLeafNodes(App.treeProduct.getRootNode());
                    if (allNodes && allNodes.length > 0) {
                        App.stoGridProduct.suspendEvents();
                        allNodes.forEach(function (node) {
                            if (node.data.Type == "Invt") {
                                var record = HQ.store.findInStore(App.stoGridProduct, ['InvtID'], [node.data.InvtID]); // App.grdOM21700.store
                                if (!record) {
                                    HQ.store.insertBlank(App.stoGridProduct, ['InvtID']);
                                    record = App.stoGridProduct.getAt(App.grdProduct.store.getCount() - 1);
                                    record.set('AccumulateID', App.txtProgramID.getValue());
                                    record.set('InvtID', node.data.InvtID);
                                    record.set('Descr', node.data.Descr);
                                    record.set('Unit', node.data.Unit);
                                    record.set('Percent', node.data.Percent);
                                    record.set('Qty', '1');
                                    record.set("LineRef", _lineRef);
                                    //if (HQ.dateInvt) {
                                    //    record.set("StartDateInvt", App.dtpFromDate.getValue());
                                    //    record.set("EndDateInvt", App.dtpToDate.getValue());
                                    //}

                                }
                            }
                        });
                        App.stoGridProduct.resumeEvents();
                        // App.stoSetup.loadPage(1);
                        App.grdProduct.view.refresh();

                        var record = App.stoGridProduct.getAt(App.stoGridProduct.getCount() - 1);
                        App.treeProduct.clearChecked();

                        App.stoGridProduct.pageSize = parseInt(50, 10);
                        App.stoGridProduct.loadPage(1);
                        App.grdProduct.getFilterPlugin().clearFilters();
                        App.grdProduct.getFilterPlugin().getFilter('LineRef').setValue([_lineRef, '']);
                        App.grdProduct.getFilterPlugin().getFilter('LineRef').setActive(true);
                        if (HQ.isInsert) {
                            var record = HQ.store.findRecord(App.stoGridProduct, ['InvtID'], ['']);
                            if (!record) {
                                HQ.store.insertBlank(App.stoGridProduct, 'InvtID');
                            }
                        }
                    }
                }
                else {
                    HQ.message.show(4, '', '');
                }
            }
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var getLeafNodes = function (node) {
    var childNodes = [];
    node.eachChild(function (child) {
        if (child.isLeaf()) {
            childNodes.push(child);
        }
        else {
            var children = getLeafNodes(child);
            if (children.length) {
                children.forEach(function (nill) {
                    childNodes.push(nill);
                });
            }
        }
    });
    
    return childNodes;
};

var btnDelProduct_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        //if (App.frmMain.isValid())
        //{
            var status = App.cboStatus.value;
            if (status == _beginStatus) {
                var selRecs = App.grdProduct.selModel.selected.items;
                if (selRecs.length > 0) {
                    var params = [];
                    selRecs.forEach(function (record) {
                        params.push(record.data.InvtID);
                    });
                    HQ.message.show(2015020806,
                        params.join(" & ") + ",",
                        'deleteSelectedProduct');
                }
            }
        //}
        //else {
        //    Process.showFieldInvalid(App.frmMain);
        //}
    }
    else {
        HQ.message.show(4, '', '');
    }
};

var deleteSelectedProduct = function (item) {
    if (item == "yes") {
        App.grdProduct.deleteSelected();
        App.treeProduct.clearChecked();
      //  Event.Form.frmMain_fieldChange();
    }
};

///edit\
var grdReality_Edit = function (item, e) {
    var record = HQ.store.findInStore(App.stoLoadGridReality, ['FromPercent'], ['']);
    var record2 = HQ.store.findInStore(App.stoLoadGridReality, ['ToPercent'], ['']);
    if (!record || !record2) {
        HQ.store.insertBlank(App.stoLoadGridReality, ['FromPercent']);
    }
    if (e.record.data.LineRef == '') {
        e.record.set('LineRef', HQ.store.lastLineRef(App.stoLoadGridReality));
    }
    if (e.value != "" && e.value != null) {
        App.cboCheckSalesBy.setReadOnly(true);
        App.cboTypeAccumulationMain.setReadOnly(true);
    }
    grdPercent_ValidateEdit(item, e);
};



    
var grdAccuAmt_Edit = function (item, e) {
    
    var lin = 0;
    var lstAccuAmt = App.GrdAccuAmt.store.snapshot || App.GrdAccuAmt.store.allData || App.GrdAccuAmt.store.data;
    if (lstAccuAmt != undefined) {
        for (var i = 0; i < lstAccuAmt.length; i++) {
            if (lstAccuAmt.length == 1) {
                e.record.set("Line", 1);
            }
            else {
                if (lstAccuAmt.items[i].data.Line > lin) {
                    lin = lstAccuAmt.items[i].data.Line;
                }
            }
            
        }
    }
    e.record.set("Line", lin + 1);
    var record = HQ.store.findInStore(App.stoAccuAmt, ['From'], ['']);
    if (!record) {
        HQ.store.insertBlank(App.stoAccuAmt, ['From']);
    }
    if (e.field == "From") {
        e.record.set("LineRef", _lineRef);
    }
    if (e.value != "" && e.value != null) {
        App.cboCheckSalesBy.setReadOnly(true)
    }
};

var checkStoreMasterLoad = function (sto) {
    HQ.masterSource++;
    if (HQ.masterSource == HQ.maxMasterSource) {
        HQ.masterSource = 0;
        //  App.stoDisplay.reload();
    }
};

//var btnAddAllCust_click = function ()
//{
//    if (HQ.isUpdate) {
//        //if (App.frmMain.isValid())
//        //{
//        var accumulateID = App.txtProgramID.getValue();
//        var status = App.cboStatus.value;

//        if (accumulateID && status == _beginStatus)
//        {
//            if (HQ.isUpdate) {
//                var allNodes = getLeafNodes(App.treeApplyCustommer.getRootNode());
//                if (allNodes && allNodes.length > 0) {
//                    App.stoData.suspendEvents();
//                    allNodes.forEach(function (node) {
                    
//                        if (node.data.Type == "CustID") { // chỉ lấy dữ liệu của lớp con
//                            var record = HQ.store.findInStore(App.stoData, ['CustID'], [node.data.CustID]); // App.grdOM21700.store
//                            if (!record) {
//                                HQ.store.insertBlank(App.stoData, ['CustID']);
//                                record = App.stoData.getAt(App.grdCust.store.getCount() - 1);
//                                record.set('AccumulateID', App.txtProgramID.getValue());
//                                record.set('CustID', node.data.CustID);
//                                record.set('RefCustID', node.data.RefCustID);
//                                record.set('CustName', node.data.CustName);
//                                record.set('PharmacyType', node.data.PharmacyType);
//                                record.set('Chain', node.data.Chain);
//                                record.set('Descr', node.data.Descr);
//                                record.set('Qty', '1');
//                                //if (HQ.dateInvt) {
//                                //    record.set("StartDateInvt", App.dtpFromDate.getValue());
//                                //    record.set("EndDateInvt", App.dtpToDate.getValue());
//                                //}

//                            }
//                        }
//                    });

//                    App.treeApplyCustommer.clearChecked();
//                    App.stoData.resumeEvents();
//                    App.stoData.pageSize = parseInt(50, 10);
//                    App.stoData.loadPage(1);
//                    App.stoData.applyPaging();


//                    //App.stoData.resumeEvents();
//                    //App.grdCust.view.refresh();
//                    //var record = App.stoData.getAt(App.stoData.getCount() - 1);
//                    //App.treeApplyCustommer.clearChecked();
//                    //App.stoData.pageSize = parseInt(50, 10);
//                    //App.stoData.loadPage(1);
//                    //App.stoData.applyPaging();
//                    //App.treeApplyCustommer.clearChecked();
//                }
//            }
//            else
//            {
//                HQ.message.show(4, '', '');
//            }
//          }
//        //}
//        //else {
//        //    Process.showFieldInvalid(App.frmMain);
//        //}
//    }
//    else {
//        HQ.message.show(4, '', '');
//    }
//};


var btnAddAllCust_click = function () {
    if (HQ.isUpdate) {
        //if (App.frmMain.isValid())
        //{
        var accumulateID = App.txtProgramID.getValue();
        var status = App.cboStatus.value;

        if (accumulateID && status == _beginStatus) {
            if (HQ.isUpdate) {
                var allNodes = getLeafNodes(App.treeApplyCustommer.getRootNode());
                if (allNodes && allNodes.length > 0) {
                    App.stoData.clearData();
                    App.stoData.suspendEvents();
                    var invtBlank = HQ.store.findRecord(App.stoData, ['CustID'], ['']);
                    //var invtBlank = HQ.store.findRecord(App.stoData, ['CustID'], ['']);
                    //if (invtBlank) {
                    //    App.stoData.remove(invtBlank);
                    //}
                    var lstnode = [];
                    allNodes.forEach(function (node) {

                        if (node.data.Type == "CustID") { // chỉ lấy dữ liệu của lớp con
                            var record = Ext.create('App.mdlSI20700_pgLoadCust');
                            record.set('AccumulateID', App.txtProgramID.getValue());
                            record.set('CustID', node.data.CustID);
                            record.set('RefCustID', node.data.RefCustID);
                            record.set('CustName', node.data.CustName);
                            record.set('PharmacyType', node.data.PharmacyType);
                            record.set('Chain', node.data.Chain);
                            record.set('Descr', node.data.Descr);
                            record.set('Qty', '1');
                            lstnode.push(record);

                        }
                    });
                    App.stoData.add(0,lstnode);
                    App.treeApplyCustommer.clearChecked();
                    App.stoData.resumeEvents();
                    App.stoData.pageSize = parseInt(50, 10);
                    App.stoData.loadPage(1);
                    App.stoData.applyPaging();
                    var invtBlank = HQ.store.findRecord(App.stoData, ['CustID'], ['']);
                    if (invtBlank) {
                        HQ.store.insertBlank(App.stoData, ['CustID']);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        }
        //}
        //else {
        //    Process.showFieldInvalid(App.frmMain);
        //}
    }
    else {
        HQ.message.show(4, '', '');
    }
};


var btnAddCust_click = function (btn, e, eOpts) {
    
    if (HQ.isUpdate) {
        //if (App.frmMain.isValid()) {
        var accumulateID = App.txtProgramID.getValue();
        var status = App.cboStatus.value;
        if (accumulateID && status == _beginStatus)
        {
            var allNodes = App.treeApplyCustommer.getCheckedNodes();
            if (allNodes && allNodes.length > 0) {
                App.stoData.suspendEvents();
                allNodes.forEach(function (node) {
                    if (node.attributes.Type == "CustID") {
                        var record = HQ.store.findInStore(App.stoData, ['CustID'], [node.attributes.CustID]); // App.grdOM21700.store
                        if (!record) {
                            HQ.store.insertBlank(App.stoData, ['CustID']);
                            record = App.stoData.getAt(App.grdCust.store.getCount() - 1);
                            record.set('AccumulateID', App.txtProgramID.getValue());
                            record.set('CustID', node.attributes.CustID);
                            record.set('RefCustID', node.attributes.RefCustID);
                            record.set('ContractType', node.attributes.ContractType);
                            record.set('ClassID', node.attributes.ClassID);
                            record.set('CustName', node.attributes.CustName);
                            record.set('PharmacyType', node.attributes.PharmacyType);
                            record.set('Chain', node.attributes.Chain);
                            record.set('Descr', node.attributes.Descr);
                            record.set('Qty', '1');
                        }
                    }
                });
                App.stoData.resumeEvents();
                App.stoData.pageSize = parseInt(50, 10);
                App.stoData.loadPage(1);
                App.stoData.applyPaging();
                App.treeApplyCustommer.clearChecked();
            }
        }
        else {
           // Process.showFieldInvalid(App.frmMain);
        }
    }
    //else {
    //    HQ.message.show(4, '', '');
    //}
};

var btndeleteAllCust_click = function (item) {

    if (HQ.isUpdate)
    {
        //    if (App.frmMain.isValid()) {
        var status = App.cboStatus.value;
        if (status == _beginStatus)
        {
            HQ.message.show(2015020806, '', 'deleteAllCust');
         }
    }
    else
    {
      //  Process.showFieldInvalid(App.frmMain);
    }
    // }
    // else {
    //     HQ.message.show(4, '', '');
    //  }
};

var deleteAllCust = function (item) {

    if (item == "yes") {
        App.stoData.loadData([], false);
        App.stoData.submitData();
        //  /// App.stoData.view.refresh();
        App.grdCust.view.refresh()
        App.stoData.loadPage(1);
        App.treeApplyCustommer.clearChecked();
        var invtBlank = HQ.store.findRecord(App.grdCust.store, ['CustID'], ['']);
        if (!invtBlank) {
            App.grdCust.store.insert(0, Ext.create("App.OM20320_pgApplyCust", { CustID: '' }));
        }
        //   Event.Form.frmMain_fieldChange();

    }
};


var btnDelCust_click = function (btn, e, eOpts) {
    if (HQ.isUpdate) {
        var selRecs = App.grdCust.selModel.selected.items;
        if (selRecs.length > 0) {
            var params = [];
            selRecs.forEach(function (record) {
                params.push(record.data.CustID);
            });
            HQ.message.show(2015020806,params.join(" & ") + ",",'deleteSelectedCust');
        }
    }
    else {
        HQ.message.show(4, '', '');
    }
}

var deleteSelectedCust = function (item) {
    if (item == "yes") {
        App.grdCust.deleteSelected();
        App.treeApplyCustommer.clearChecked();
    }
};

var checkSaveEmpty = function()
{
    if (App.cboTypeAccumulationMain.value != "A")
    {
        if (App.stoLoadGridReality.data.length == 0 || App.stoLoadGridReality.data.length == 1 && App.stoLoadGridReality.data.items[0].data.LineRef == '') {
            HQ.message.show(2017060601, [App.TabApplyCustommer.title], '', true);
            return false;
        }
        if (App.cboCheckSalesBy.value == "S") {
            if (App.stoAccuAmt.data.length == 0 || App.stoAccuAmt.data.length == 1 && App.stoAccuAmt.data.items[0].data.LineRef == '') {
                HQ.message.show(2017060601, [App.tabBonusLevel.title], '', true);
                return false;
            }
        }
        if (App.cboCheckSalesBy.value == "P") {
            if (App.stoGridProduct.data.length == 0 || App.stoGridProduct.data.length == 1 && App.stoGridProduct.data.items[0].data.InvtID == '') {
                HQ.message.show(2017060601, [App.tabProduct.title], '', true);
                return false;
            }

        }
    }
    if (App.stoData.data.length == 0 || App.stoData.data.length == 1 && App.stoData.data.items[0].data.CustID == '') {
        HQ.message.show(2017060601, [App.tabApplyCust.title], '', true);
        return false;
    }
    
   
    return true;
}


var saveData = function () 
{

    checkSpecialChar(App.txtProgramID.value)
    if (checkSpecial == "Error")
    {
        return false;
    }
    if (!App.frmMain.isValid()) // check require
    {
        showFieldInvalid(App.frmMain); 
        return false;
    }
    if (checkSaveEmpty()) // CHeck trống grid
    {
        if (_NewOrEdit == 'New') {
            // checkPercent();
            // CheckHasValue();
            App.grdProduct.store.clearFilter();
            App.GrdAccuAmt.store.clearFilter();
            App.frmMain.submit({
                url: 'OM20320/SaveNewData',
                waitMsg: HQ.common.getLang('Submiting') + "...",
                timeout: 1800000,
                params: {
                    //f lstCustinfo: HQ.store.getData(App.grdOM_Accumulated.store),
                    lstCustSave: HQ.store.getData(App.grdCust.store),
                    lstCustDetailAll: Ext.encode(App.grdCust.store.getRecordsValues()),
                    lstRealitySave: HQ.store.getData(App.grdReality.store),
                    lstRealityDetailAll: Ext.encode(App.grdReality.store.getRecordsValues()),
                    lstRealityAmtSave: HQ.store.getData(App.GrdAccuAmt.store),
                    lstRealityAmtDetailAll: Ext.encode(App.GrdAccuAmt.store.getRecordsValues()),
                    lstRealityInvtSave: HQ.store.getData(App.grdProduct.store),
                    lstRealityInvtDetailAll: Ext.encode(App.grdProduct.store.getRecordsValues()),
                    isNew: HQ.isNew,
                    active: App.ckbUsing.getValue(),
                },
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    _NewOrEdit = 'NewOfNew';
                    HQ.isChange = false;
                    App.stoData.reload();
                    App.stoOM_Accumulated.reload();
                    App.stoLoadGridReality.reload();
                    App.stoAccuAmt.reload();
                    App.grdProduct.store.reload();
                    App.stoDisplay.reload();
                    App.txtProgramID.setReadOnly(true);
                },
                failure: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode);
                    }
                    else {
                        HQ.message.process(msg, data, true);
                    }
                }
            });
        }
        else {
            if (_NewOrEdit == 'NewOfNew') {
                App.grdProduct.store.clearFilter();
                App.GrdAccuAmt.store.clearFilter();
                App.frmMain.submit({
                    url: 'OM20320/SaveNewOfNewData',
                    waitMsg: HQ.common.getLang('Submiting') + "...",
                    timeout: 1800000,
                    params: {
                        lstCustSave: HQ.store.getData(App.grdCust.store),
                        lstCustDetailAll: Ext.encode(App.grdCust.store.getRecordsValues()),
                        lstRealitySave: HQ.store.getData(App.grdReality.store),
                        lstRealityDetailAll: Ext.encode(App.grdReality.store.getRecordsValues()),
                        lstRealityAmtSave: HQ.store.getData(App.GrdAccuAmt.store),
                        lstRealityAmtDetailAll: Ext.encode(App.GrdAccuAmt.store.getRecordsValues()),
                        lstRealityInvtSave: HQ.store.getData(App.grdProduct.store),
                        lstRealityInvtDetailAll: Ext.encode(App.grdProduct.store.getRecordsValues()),
                        isNew: HQ.isNew,
                        active: App.ckbUsing.getValue(),
                    },
                    success: function (msg, data) {
                        if (data.result.msgCode) {
                            HQ.message.show(data.result.msgCode);
                        }
                        HQ.isChange = false;
                        App.stoData.reload();
                        App.stoOM_Accumulated.reload();
                        App.stoLoadGridReality.reload();
                        App.stoAccuAmt.reload();
                        App.grdProduct.store.reload();
                        App.txtProgramID.setReadOnly(true);
                        App.stoDisplay.reload();
                        //if (App.cboHandle.getValue()) {
                        //    App.cboStatus.setValue(App.cboHandle.getValue());
                        //}
                    },
                    failure: function (msg, data) {
                        if (data.result.msgCode) {
                            HQ.message.show(data.result.msgCode);
                        }
                        else {
                            HQ.message.process(msg, data, true);
                        }
                    }
                });
            }
            else
            {
                App.grdProduct.store.clearFilter();
                App.GrdAccuAmt.store.clearFilter();
                App.frmMain.submit({
                    url: 'OM20320/SaveEditData',
                    waitMsg: HQ.common.getLang('Submiting') + "...",
                    timeout: 1800000,
                    params: {
                        lstCustSave: HQ.store.getData(App.grdCust.store),
                        lstCustDetailAll: Ext.encode(App.grdCust.store.getRecordsValues()),
                        lstRealitySave: HQ.store.getData(App.grdReality.store),
                        lstRealityDetailAll: Ext.encode(App.grdReality.store.getRecordsValues()),
                        lstRealityAmtSave: HQ.store.getData(App.GrdAccuAmt.store),
                        lstRealityAmtDetailAll: Ext.encode(App.GrdAccuAmt.store.getRecordsValues()),
                        lstRealityInvtSave: HQ.store.getData(App.grdProduct.store),
                        lstRealityInvtDetailAll: Ext.encode(App.grdProduct.store.getRecordsValues()),
                        isNew: HQ.isNew,
                        active: App.ckbUsing.getValue(),
                    },
                    success: function (msg, data) {
                        if (data.result.msgCode) {
                            HQ.message.show(data.result.msgCode);
                        }
                        HQ.isChange = false;
                        App.stoData.reload();
                        App.stoOM_Accumulated.reload();
                        App.stoLoadGridReality.reload();
                        App.stoAccuAmt.reload();
                        App.grdProduct.store.reload();
                        App.txtProgramID.setReadOnly(true);
                        App.stoDisplay.reload();
                        //if (App.cboHandle.getValue()) {
                        //    App.cboStatus.setValue(App.cboHandle.getValue());
                        //}
                    },
                    failure: function (msg, data) {
                        if (data.result.msgCode) {
                            HQ.message.show(data.result.msgCode);
                        }
                        else {
                            HQ.message.process(msg, data, true);
                        }
                    }
                });
            }
            
        }
    }
   
}


var setGrid_ReadOnly = function()
{
    //App.grdReality.setReadOnly(true);
    //App.GrdAccuAmt.setReadOnly(true);
    //App.grdProduct.setReadOnly(true);
    //App.PnlBtnProduct.setReadOnly(true);
}

var stoDisplay_load = function (sto, records, successful, eOpts)
{
    debugger
    HQ.isNew = false;
    if (sto.getCount() == 0)
    {
        var newDisplay = Ext.create("App.mdlOM_Accumulated",
        {
            Status: _beginStatus
        });
        HQ.store.insertRecord(sto, [], newDisplay, true);
        HQ.isNew = true;
    }

    var frmRecord = sto.getAt(0);
    App.frmMain.loadRecord(frmRecord);

    _source = 0;
    App.stoOM_Accumulated.reload();
    var record = HQ.store.findInStore(App.stoOM_Accumulated, ['AccumulateID'], [App.txtProgramID.getValue()]);
    if (_NewOrEdit != "New")
    {
        App.stoData.reload();
        App.stoLoadGridReality.reload();
        App.stoAccuAmt.reload();
        App.stoGridProduct.reload();
    }

}
var pnlDetailCTTL_Active = function () {
    App.grdReality.selModel.select(0);
}
var slmGrid_Select = function (slm, selRec, idx, eOpts) {
    _accumulateID = selRec.data.AccumulateID;
    _status = selRec.data.Status;
}

var focusOnInvalidField = function (item) {
    if (item == "ok") {
        App.frmMain.getForm().getFields().each(function (field) {
            if (!field.isValid()) {
                field.focus();
                return false;
            }
        });
    }
};
var showFieldInvalid = function (form) {
    var done = 1;
    form.getForm().getFields().each(function (field) {
        if (!field.isValid()) {
            HQ.message.show(15, field.fieldLabel, 'focusOnInvalidField');
            done = 0;
            return false;
        }
    });
    return done;
};



var grdPercent_ValidateEdit = function (item, e) {
    if (e.field == "FromPercent" || e.field == "ToPercent") {
        var lst = App.grdReality.store.snapshot || App.grdReality.store.allData || App.grdReality.store.data;
        if (lst != undefined) {
            var minValue = 0;
            var maxValue = 0;
            for (var i = 0; i < lst.length; i++) {
                
                if (lst.items[i].data.FromPercent <= e.value && e.value <= lst.items[i].data.ToPercent && e.record.data.LineRef != lst.items[i].data.LineRef && e.value != 0) {
                    HQ.message.show(20181123, [e.value.toPrecision(3), Ext.util.Format.number(lst.items[i].data.FromPercent, "0,000.00"), Ext.util.Format.number(lst.items[i].data.ToPercent, "0,000.00")], "", true);
                    return false;
                }
                if (e.field == "FromPercent" && e.record.data.ToPercent != 0 && e.value != 0) {
                    if (lst.items[i].data.FromPercent >= e.value && e.record.data.ToPercent >= lst.items[i].data.ToPercent && e.record.data.LineRef != lst.items[i].data.LineRef) {
                        HQ.message.show(20181123, [e.value.toPrecision(3), Ext.util.Format.number(lst.items[i].data.FromPercent, "0,000.00"), Ext.util.Format.number(lst.items[i].data.ToPercent, "0,000.00")], "", true);
                        return false;
                    }
                }

                if (e.field == "ToPercent" && e.record.data.FromPercent != 0 && e.value != 0) {
                    if (lst.items[i].data.FromPercent >= e.record.data.FromPercent && e.value >= lst.items[i].data.ToPercent && e.record.data.LineRef != lst.items[i].data.LineRef) {
                        HQ.message.show(20181123, [e.value.toPrecision(3), Ext.util.Format.number(lst.items[i].data.FromPercent, "0,000.00"), Ext.util.Format.number(lst.items[i].data.ToPercent, "0,000.00")], "", true);
                        return false;
                    }
                }
            }
        }
    }
    return HQ.grid.checkValidateEdit(App.grdReality, e, keys, true);
};

var grdAccuAmt_ValidateEdit = function (item, e) {
    
    if (e.field == "From" || e.field == "To")
    {
        var lst =  App.GrdAccuAmt.store.allData || App.GrdAccuAmt.store.data;
        if (lst != undefined) {
            var minValue = 0;
            var maxValue = 0;
            for (var i = 0; i < lst.length; i++) {

                if (lst.items[i].data.From <= e.value && e.value <= lst.items[i].data.To && e.record.data.Line != lst.items[i].data.Line && e.value != 0) {
                    HQ.message.show(20181123, [e.value.toPrecision(3), Ext.util.Format.number(lst.items[i].data.From, "0,000.00"), Ext.util.Format.number(lst.items[i].data.To, "0,000.00")], "", true);
                    return false;
                }
                if (e.field == "From" && e.record.data.To != 0 && e.value != 0) {
                    if (lst.items[i].data.From >= e.value && e.record.data.To >= lst.items[i].data.To && e.record.data.Line != lst.items[i].data.Line) {
                        HQ.message.show(20181123, [e.value.toPrecision(3), Ext.util.Format.number(lst.items[i].data.From, "0,000.00"), Ext.util.Format.number(lst.items[i].data.To, "0,000.00")], "", true);
                        return false;
                    }
                }

                if (e.field == "To" && e.record.data.From != 0 && e.value != 0) {
                    if (lst.items[i].data.From >= e.record.data.From && e.value >= lst.items[i].data.To && e.record.data.Line != lst.items[i].data.Line) {
                        HQ.message.show(20181123, [e.value.toPrecision(3), Ext.util.Format.number(lst.items[i].data.From, "0,000.00"), Ext.util.Format.number(lst.items[i].data.To, "0,000.00")], "", true);
                        return false;
                    }
                }
            }
        }
    }
    return HQ.grid.checkValidateEdit(App.GrdAccuAmt, e, keys, true);
};


var checkValidateEdit = function (grd, e, keys, isCheckSpecialChar) {
    if (keys.indexOf(e.field) != -1) {
        var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/
        if (isCheckSpecialChar == undefined) isCheckSpecialChar = true;
        if (isCheckSpecialChar) {
            if (e.value)
                if (!HQ.util.passNull(e.value) == '' && !HQ.util.passNull(e.value.toString()).match(regex)) {
                    HQ.message.show(20140811, e.column.text);
                    App.grdProduct.deleteSelected();
                    return false;
                }
        }
        if (checkDuplicate(grd, e, keys)) {
            if (e.column.xtype == "datecolumn") {
                HQ.message.show(1112, Ext.Date.format(e.value, e.column.format));
            }
            else
            {
                HQ.message.show(1112, e.value);
                App.grdProduct.deleteSelected();
            }
            return false;
        }

    }
}

var checkDuplicate = function (grd, row, keys) {
    var found = false;
    var store = grd.getStore();
    if (keys == undefined) keys = row.record.idProperty.split(',');
    // var allData = store.snapshot //|| store.allData || store.data;
    var allDatacheck = store.allData ;  // store.snapshot //|| store.allData || store.data;
    for (var i = 0; i < allDatacheck.items.length; i++) {
        var record = allDatacheck.items[i];
        var data = '';
        var rowdata = '';
        for (var jkey = 0; jkey < keys.length; jkey++) {
            if (record.data[keys[jkey]] != undefined) {
                data += record.data[keys[jkey]].toString().toLowerCase() + ',';
                if (row.field == keys[jkey])
                    rowdata += (row.value == null ? "" : row.value.toString().toLowerCase()) + ',';
                else
                    rowdata += (row.record.data[keys[jkey]] ? row.record.data[keys[jkey]].toString().toLowerCase() : '') + ',';
            }
        }
        if (found = (data == rowdata && record.id != row.record.id) ? true : false) {
            break;
        };
    }
    return found;
}

var checkValidateEditCust = function (grd, e, keys, isCheckSpecialChar) {
    if (App.cboCustID.value != null)
    {
        if (keys.indexOf(e.field) != -1) {
            var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/
            if (isCheckSpecialChar == undefined) isCheckSpecialChar = true;
            if (isCheckSpecialChar) {
                if (e.value)
                    if (!HQ.util.passNull(e.value) == '' && !HQ.util.passNull(e.value.toString()).match(regex)) {
                        HQ.message.show(20140811, e.column.text);
                        App.grdCust.deleteSelected();
                        return false;
                    }
            }
            if (checkDuplicateCust(grd, e, keys)) {
                if (e.column.xtype == "datecolumn") {
                    HQ.message.show(1112, Ext.Date.format(e.value, e.column.format));
                }
                else {
                    HQ.message.show(1112, e.value);
                    App.grdCust.deleteSelected();
                }
                return false;
            }

        }
    }
}

var checkDuplicateCust = function (grd, row, keys) {
    

    var found = false;
    var store = grd.getStore();
    if (keys == undefined) keys = row.record.idProperty.split(',');
    // var allData = store.snapshot //|| store.allData || store.data;
    var allDatacheck = store.allData ;  // store.snapshot //|| store.allData || store.data;
    for (var i = 0; i < allDatacheck.items.length; i++) {
        var record = allDatacheck.items[i];
        var data = '';
        var rowdata = '';
        for (var jkey = 0; jkey < keys.length; jkey++) {
            if (record.data[keys[jkey]] != undefined) {
                data += record.data[keys[jkey]].toString().toLowerCase() + ',';
                if (row.field == keys[jkey])
                    rowdata += (row.value == null ? "" : row.value.toString().toLowerCase()) + ',';
                else
                    rowdata += (row.record.data[keys[jkey]] ? row.record.data[keys[jkey]].toString().toLowerCase() : '') + ',';
            }
        }
        if (found = (data == rowdata && record.id != row.record.id) ? true : false) {
            break;
        };
    }
    return found;
}


// truyền type cho cây tree

var loadTreeCustomer = function (typeAccumulation) {

    
    HQ.common.showBusy(true, HQ.waitMsg);
    App.direct.OM20320GetTreeData('typeAccumulation', 'C',
        {
            success: function (result)
            {
            App.treeApplyCustommer.getRootNode().expand();
            HQ.common.showBusy(false);
        }
    });
}

///

var treeCustomer_AfterRender = function (typeAccumulation) {
    
    HQ.common.showBusy(true, HQ.waitMsg);
    lstCpnyID = [];
    //for (var i = 0; i < App.grdCompany.store.getRecordsValues().length; i++) {
    //    if (App.grdCompany.store.getRecordsValues()[i].CpnyID != "")
    //        lstCpnyID.push(App.grdCompany.store.getRecordsValues()[i].CpnyID);
    //}
    
    App.direct.OM20320GetTreeData(typeAccumulation, {
        success: function (result) {
            App.treeApplyCustommer.getRootNode().expand();
            HQ.common.showBusy(false, HQ.waitMsg);
        }
    });
  //  App.cboGCustID.store.reload();
}
