
var selectedIndex = 0;
var _hold = "H";
var _curSlsperid = "";
var _focusrecord = 0;
var beforeedit = '';
var prefixvalue = "";
var lastfixvalue = "";
var shownextlotserial = "";
var chooseGrid = "";
var click = 0;
var tmpImageDelete = 0;
var tmpMediaDelete = 0;
var tmpSelectedNode = "";
var tmpCopyForm = "0";
var tmpCopyFormSave = "0";
var tmpOldFileName = "";
var cpnyIDTree = "";

var clickcboCustId = "0";
var tmpAddNew = "0";
var tampSaveButton = "0";
var parentRecordIDAll = "";
var parentRecordID = ""
var selectNodeAfterInsert = "0";
var tmpCountryloadStoreOrForm = "0";
HQ.focus = "";

//framework moi
var tmpCity = "";
var tmpDistrict = "";
var tmpHiddenTree = false;
var tmpHiddenTabLTT = false;
var tmpHiddenTabAdv = false;
var tmpHiddenTabSellingProd = false;
var tmpHiddenTabDispMethod = false;
var tmpAutoCustID = false;
var _index = 0;


//bien tam GridLTT top va bot
var keysTop = ['LTTContractNbr'];
var fieldsCheckRequireTop = ["LTTContractNbr"];
var fieldsLangCheckRequireTop = ["LTTContractNbr"];

var keysBot = ['Type'];
var fieldsCheckRequireBot = ["Type", "FromDate", "ToDate", "ExtDate"];
var fieldsLangCheckRequireBot = ["Type", "FromDate", "ToDate", "ExtDate"];

//bien tam GridAdv
var keysAdv = ['Type'];
var fieldsCheckRequireAdv = ["Type", "FitupDate"];
var fieldsLangCheckRequireAdv = ["Type", "FitupDate"];

//bien tam GridSellingProd
var keysSellingProd = ['Code'];
var fieldsCheckRequireSellingProd = ["Code"];
var fieldsLangCheckRequireSellingProd = ["Code"];

//bien tam GridSellingProd
var keysDispMethod = ['DispMethod'];
var fieldsCheckRequireDispMethod = ["DispMethod"];
var fieldsLangCheckRequireDispMethod = ["DispMethod"];


var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == 'header') {
                HQ.combo.first(App.cboCustId, HQ.isChange);
            } else if (HQ.focus == 'grdLTTContract') {
                HQ.grid.first(App.grdTop);
            } else if (HQ.focus == 'grdLTTContractDetail') {
                HQ.grid.first(App.grdBot);
            } else if (HQ.focus == 'grdAdv') {
                HQ.grid.first(App.grdAdv);
            } else if (HQ.focus == 'grdSellingProd') {
                HQ.grid.first(App.grdSellingProd);
            } else if (HQ.focus == 'grdDispMethod') {
                HQ.grid.first(App.grdDispMethod);
            }
            break;
        case "prev":
            if (HQ.focus == 'header') {
                HQ.combo.next(App.cboCustId, HQ.isChange);
            } else if (HQ.focus == 'grdLTTContract') {
                HQ.grid.prev(App.grdTop);
            } else if (HQ.focus == 'grdLTTContractDetail') {
                HQ.grid.prev(App.grdBot);
            } else if (HQ.focus == 'grdAdv') {
                HQ.grid.prev(App.grdAdv);
            } else if (HQ.focus == 'grdSellingProd') {
                HQ.grid.prev(App.grdSellingProd);
            } else if (HQ.focus == 'grdDispMethod') {
                HQ.grid.prev(App.grdDispMethod);
            }
            break;
        case "next":
            if (HQ.focus == 'header') {
                HQ.combo.prev(App.cboCustId, HQ.isChange);
            } else if (HQ.focus == 'grdLTTContract') {
                HQ.grid.next(App.grdTop);
            } else if (HQ.focus == 'grdLTTContractDetail') {
                HQ.grid.next(App.grdBot);
            } else if (HQ.focus == 'grdAdv') {
                HQ.grid.next(App.grdAdv);
            } else if (HQ.focus == 'grdSellingProd') {
                HQ.grid.next(App.grdSellingProd);
            } else if (HQ.focus == 'grdDispMethod') {
                HQ.grid.next(App.grdDispMethod);
            }

            break;
        case "last":
            if (HQ.focus == 'header') {
                HQ.combo.last(App.cboCustId, HQ.isChange);
            } else if (HQ.focus == 'grdLTTContract') {
                HQ.grid.last(App.grdTop);
            } else if (HQ.focus == 'grdLTTContractDetail') {
                HQ.grid.last(App.grdBot);
            } else if (HQ.focus == 'grdAdv') {
                HQ.grid.last(App.grdAdv);
            } else if (HQ.focus == 'grdSellingProd') {
                HQ.grid.last(App.grdSellingProd);
            } else if (HQ.focus == 'grdDispMethod') {
                HQ.grid.last(App.grdDispMethod);
            }
            break;
        case "refresh":
            //App.storeFormBig.reload();
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            } else {
                HQ.isChange = false;
                //App.cboMailID.getStore().load(function () { App.stoMailHeader.reload(); });

            }

            break;
        case "new":
            if (HQ.isInsert) {
                //App.cboCustId.setValue('');
                //tmpAddNew = "1";
                if (HQ.focus == 'header') {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', '');
                    }
                    else {
                        //HQ.isNew = true;
                        //App.cboCustId.setValue('');
                        //var time = new Date();
                        //var record = Ext.create("App.AR_CustomerModel", {
                        //    //CustId: "",
                        //    Status: "H",
                        //    ClassId: "CN",
                        //    CustType: "N",
                        //    PriceClassID: "NPPCH",
                        //    CrRule: "N",
                        //    Territory: "R001",
                        //    Country: "VN",
                        //    State: "0043",
                        //    City: "HCM",
                        //    District: "0043001",
                        //    ExpiryDate: HQ.businessDate,
                        //    EstablishDate: HQ.businessDate,
                        //    Birthdate: HQ.businessDate,
                        //    TaxDflt: "C",
                        //    TaxID00: "VAT10",
                        //    TaxID01: "VAT10",
                        //    TaxID02: "VAT10",
                        //    TaxID03: "VAT10"
                        //});
                        //App.storeFormBig.insert(0, record);
                        //App.frmMain.getForm().loadRecord(App.storeFormBig.getAt(0));


                        App.cboCustId.setValue('');
                        //cboCustId_Change(App.cboCustId);
                        App.storeFormBig.reload();
                    }
                } else if (HQ.focus == 'grdLTTContract') {
                    HQ.grid.insert(App.grdTop, keysTop);
                } else if (HQ.focus == 'grdLTTContractDetail') {
                    HQ.grid.insert(App.grdBot, keysBot);
                } else if (HQ.focus == 'grdAdv') {
                    HQ.grid.insert(App.grdAdv, keysAdv);
                } else if (HQ.focus == 'grdSellingProd') {
                    HQ.grid.insert(App.grdSellingProd, keysSellingProd);
                } else if (HQ.focus == 'grdDispMethod') {
                    HQ.grid.insert(App.grdDispMethod, keysDispMethod);
                }
               
            }
            break;
        case "delete":
            var curRecord = App.frmMain.getRecord();
            if (HQ.isDelete) {
                if (HQ.focus == 'header') {
                    if (App.cboCustId.value != "" && App.cboStatus.value != "A" && App.cboStatus.value != "O") {

                        HQ.message.show(11, '', 'deleteRecordForm');
                    } else if (HQ.focus == 'grdLTTContract') {
                        var rowindex = HQ.grid.indexSelect(App.grdTop);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdTop), ''], 'deleteRecordGridLTTContract', true)
                    } else if (HQ.focus == 'grdLTTContractDetail') {
                        var rowindex = HQ.grid.indexSelect(App.grdBot);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdBot), ''], 'deleteRecordGridLTTContractDetail', true)
                    } else if (HQ.focus == 'grdAdv') {
                        var rowindex = HQ.grid.indexSelect(App.grdAdv);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdAdv), ''], 'deleteRecordGridAdv', true)
                    } else if (HQ.focus == 'grdSellingProd') {
                        var rowindex = HQ.grid.indexSelect(App.grdSellingProd);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdSellingProd), ''], 'deleteRecordGridSellingProd', true)
                    } else if (HQ.focus == 'grdDispMethod') {
                        var rowindex = HQ.grid.indexSelect(App.grdDispMethod);
                        if (rowindex != '')
                            HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdDispMethod), ''], 'deleteRecordGridDispMethod', true)
                    }
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {

                if (HQ.form.checkRequirePass(App.frmMain) &&
                    HQ.store.checkRequirePass(App.storeGridLTTTop, keysTop, fieldsCheckRequireTop, fieldsLangCheckRequireTop) &&
                    HQ.store.checkRequirePass(App.storeGridLTTBot, keysBot, fieldsCheckRequireBot, fieldsLangCheckRequireBot) &&
                    HQ.store.checkRequirePass(App.storeGridAdv, keysAdv, fieldsCheckRequireAdv, fieldsLangCheckRequireAdv) &&
                    HQ.store.checkRequirePass(App.storeGridSellingProd, keysSellingProd, fieldsCheckRequireSellingProd, fieldsLangCheckRequireSellingProd) &&
                    HQ.store.checkRequirePass(App.storeGridDispMethod, keysDispMethod, fieldsCheckRequireDispMethod, fieldsLangCheckRequireDispMethod))
                {
                    // if (App.slmTree.selected.items[0].childNodes.length != 0) {
                    Save();
                // } else {
                //    HQ.message.show(8081, '', null);
                //}

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
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        if (App.cboCustId.valueModels == null) App.cboCustId.setValue('');
        App.cboCustId.getStore().reload();
        App.storeFormBig.reload();
        if (tmpHiddenTabLTT == false) {
            App.storeGridLTTTop.reload();
            App.storeGridLTTBot.reload();
        }
        if (tmpHiddenTabAdv == false) {
            App.storeGridAdv.reload();
        }
        if (tmpHiddenTabSellingProd == false) {
            App.storeGridSellingProd.reload();
        }
        if (tmpHiddenTabDispMethod == false) {
            App.storeGridDispMethod.reload();
        }
    }
};

////////////Kiem tra combo chinh CustID
//khi co su thay doi du lieu cua cac conttol tren form
//load lần đầu khi mở
var firstLoad = function () {
    App.cboCustId.getStore().reload();
    App.storeFormBig.reload();
};
var frmChange = function () {
    //đề phòng trường hợp nếu store chưa có gì cả giao diện chưa load xong mà App.frmMain.getForm().updateRecord(); được gọi sẽ gây lỗi
    if (App.storeFormBig.data.length > 0) {
        App.frmMain.getForm().updateRecord();

        HQ.isChange = HQ.store.isChange(App.storeFormBig);
        HQ.common.changeData(HQ.isChange, 'AR20400');//co thay doi du lieu gan * tren tab title header
        //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
        if (App.cboCustId.valueModels == null || HQ.isNew == true ) {//App.cboCustId.valueModels == null khi ko co select item nao
            App.cboCustId.setReadOnly(false);
        }
        //else if (HQ.isNew == true && tmpAutoCustID == true && (App.cboCustId.getValue() != "" && App.cboCustId.getValue() != null)) {
        //    App.cboCustId.setReadOnly(true);
        //}
        else {
            App.cboCustId.setReadOnly(HQ.isChange);
        }
  
    }

};

var refreshTree = function (i) {
    App.slmTree.select(i);
}
var refreshTree2 = function (dt) {
    App.cboInvtID.setValue(dt);
    var findNode = App.IDTree.items.items[0].store.data.items;
    for (var i = 0; i < findNode.length; i++) {
        if (findNode[i].data.id == node) {
            //click = 1;
            App.slmTree.select(i);
            //setTimeout(function () { refreshTree(i); }, 7000);
            break;

        }

    }
}
function Save() {

    var curRecord = App.frmMain.getRecord();

    App.frmMain.getForm().updateRecord();
    //App.frmMain.updateRecord(curRecord);
    if (tmpHiddenTree == false) { // neu co Tree chay ham nay Save Tree
        if (App.frmMain.isValid()) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang('Submiting...'),
                url: 'AR20400/SaveTree',
                params: {
                    lstheader: Ext.encode(App.storeFormBig.getChangedData({ skipIdForPhantomRecords: false })),//,
                    CustID: App.cboCustId.getValue(),
                    Handle: App.cboHandle.getValue(),
                    NodeID: App.slmTree.selected.items[0].data.text,
                    NodeLevel: App.slmTree.selected.items[0].data.depth,
                    ParentRecordID: parentRecordID,
                    hadchild: App.slmTree.selected.items[0].childNodes.length,
                    Status: App.cboStatus.getValue(),
                    tmpSelectedNode: tmpSelectedNode,
                    branchID: App.cboCpnyID.value,
                    custName: App.txtCustName.value,
                    isNew: HQ.isNew,
                    lstGridLTTContract: tmpHiddenTabLTT == false ? HQ.store.getData(App.storeGridLTTTop) : "",
                    lstGridLTTContractDetail: tmpHiddenTabLTT == false ? HQ.store.getData(App.storeGridLTTBot) : "",
                    lTTContractNbr: App.slmTopGrid.selected.items[0] == undefined ? '' : App.slmTopGrid.selected.items[0].data.LTTContractNbr,
                    lstGridAdv: tmpHiddenTabAdv == false ? HQ.store.getData(App.storeGridAdv) : "",
                    lstGridSellingProd: tmpHiddenTabSellingProd == false ? HQ.store.getData(App.storeGridSellingProd) : "",
                    lstGridDispMethod: tmpHiddenTabDispMethod == false ? HQ.store.getData(App.storeGridDispMethod) : "",
                    //tmpHiddenTree: tmpHiddenTree,
                },
                success: function (result, data) {
                    //if (data.result.statusAfterAll == "H") {
                    //    enableForm();
                    //}
                    if (data.result.tmpHiddenTree == false)

                        tmpCopyForm = "0";
                    App.cboCustId.getStore().reload();

                    //App.cboHandle.setValue("");
                    HQ.message.show(201405071, '', null);
                    //App.cboInvtID.setValue('');
                    App.storeFormBig.reload();
                    //tampSaveButton = "1";
                    if (data.result.addNewOrUpdate == "addNew") {
                        if (data.result.changeTreeBranch == "0") {//nếu ko chuyển nhánh khác cho node
            
                            var newNode = App.slmTree.selected.items[0].data.id;
                            var record = App.IDTree.getStore().getNodeById(newNode);
                            var node = data.result.custID + "-" + data.result.custName;
                            record.appendChild({ text: node, leaf: true, id: node });

                            //cach lam tim node tren cay de select vao`
                            //var findNode = App.IDTree.items.items[0].store.data.items;
                            //for (var i = 0; i < findNode.length; i++) {
                            //    if (findNode[i].data.id == node) {
                            //        //click = 1;
                            //        //selectNodeAfterInsert = "1";
                            //        App.slmTree.select(i);
                            //        break;
                            //        //setTimeout(function () { refreshTree(i); }, 7000);

                            //    }
                            //}

                            //cach lam set value lai cho ID
                            App.cboCustId.setValue("");
                            setTimeout(function () { waitReloadTreeDone(data.result.custID); }, 4000);
                      
                        } else if (data.result.changeTreeBranch == "1") {//nếu chuyển nhánh khác cho node
                          
                            var tmpSelectedNode = data.result.value5;
                            var recordtmpSelectedNode = App.IDTree.getStore().getNodeById(tmpSelectedNode);
                            recordtmpSelectedNode.remove(true);

                            var newNode = App.slmTree.selected.items[0].data.id;
                            var record = App.IDTree.getStore().getNodeById(newNode);

                            var node = data.result.custID + "-" + data.result.custName;

                            record.appendChild({ text: node, leaf: true, id: node });

                            App.cboCustId.setValue("");
                            setTimeout(function () { waitReloadTreeDone(data.result.custID); }, 4000);
                       



                        }
                    }
                    if (data.result.addNewOrUpdate == "update") {
       
                        var newNode = App.slmTree.selected.items[0].data.id;
                        var record = App.IDTree.getStore().getNodeById(newNode);
                        var node = data.result.custID + "-" + data.result.custName;
                        if (newNode != node) {
                            record.parentNode.appendChild({ text: node, leaf: true, id: node });
                            var findNode = App.IDTree.items.items[0].store.data.items;
                            for (var i = 0; i < findNode.length; i++) {
                                if (findNode[i].data.id == node) {
                                    //click = 1;
                                    App.slmTree.select(i);
                                    continue;
                            

                                }
                            }
                            var newNode2 = App.slmTree.selected.items[0].data.id;
                            var record2 = App.IDTree.getStore().getNodeById(newNode2);
                            record2.remove(true);
                            record.parentNode.replaceChild(record2, record);
                            var findNode = App.IDTree.items.items[0].store.data.items;
                            for (var i = 0; i < findNode.length; i++) {
                                if (findNode[i].data.id == node) {
                              
                                    App.slmTree.select(i);
                               
                                    break;
                                }
                            }
                        }

               


                    }
                    App.storeFormBig.reload();
                    //cac tab an
                    if (tmpHiddenTabLTT == false) {
                        App.storeGridLTTTop.reload();
                        App.storeGridLTTBot.reload();
                    }
                    if (tmpHiddenTabAdv == false) {
                        App.storeGridAdv.reload();
                    }
                    if (tmpHiddenTabSellingProd == false) {
                        App.storeGridSellingProd.reload();
                    }
                    if (tmpHiddenTabDispMethod == false) {
                        App.storeGridDispMethod.reload();
                    }
                    App.cboDfltShipToId.getStore().reload();

                }
                , failure: function (msg, data) {

                    if (data.result.msgCode) {
                        if (data.result.msgCode == 2000)//loi trung key ko the add
                            HQ.message.show(data.result.msgCode, [App.cboCustId.fieldLabel, App.cboCustId.getValue()], '', true);
                        else HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                    }
                    else {
                        HQ.message.process(msg, data, true);
                    }
                }
            });
        }
        else {
            var fields = App.frmMain.getForm().getFields().each(
                    function (item) {
                        if (!item.isValid()) {
                            alert(item);
                        }
                    }
                );
        }
    } else { // neu an tree chay ham nay Save no Tree
        if (App.frmMain.isValid()) {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang('Submiting...'),
                url: 'AR20400/SaveNoTree',
                params: {
                    lstheader: Ext.encode(App.storeFormBig.getChangedData({ skipIdForPhantomRecords: false })),//,
                    CustID: App.cboCustId.getValue(),
                    Handle: App.cboHandle.getValue(),
                    Status: App.cboStatus.getValue(),
                    tmpSelectedNode: tmpSelectedNode,
                    branchID: App.cboCpnyID.value,
                    custName: App.txtCustName.value,
                    isNew: HQ.isNew,
                    lstGridLTTContract: tmpHiddenTabLTT == false ? HQ.store.getData(App.storeGridLTTTop) : "",
                    lstGridLTTContractDetail: tmpHiddenTabLTT == false ? HQ.store.getData(App.storeGridLTTBot) : "",
                    lTTContractNbr: App.slmTopGrid.selected.items[0] == undefined ? '' : App.slmTopGrid.selected.items[0].data.LTTContractNbr,
                    lstGridAdv: tmpHiddenTabAdv == false ? HQ.store.getData(App.storeGridAdv) : "",
                    lstGridSellingProd: tmpHiddenTabSellingProd == false ? HQ.store.getData(App.storeGridSellingProd) : "",
                    lstGridDispMethod: tmpHiddenTabDispMethod == false ? HQ.store.getData(App.storeGridDispMethod) : "",
                    
                    //tmpHiddenTree: tmpHiddenTree,
                },
                success: function (result, data) {
                    if (data.result.statusAfterAll == "H") {
                        enableForm();
                    }
                    //if (data.result.tmpHiddenTree == false)
                    HQ.message.show(201405071, '', '');
                    var custId = data.result.custID;
                    App.cboCustId.getStore().load(function () {
                        App.cboCustId.setValue(custId);
                        App.storeFormBig.reload();
                        if (tmpHiddenTabLTT == false) {
                            App.storeGridLTTTop.reload();
                            App.storeGridLTTBot.reload();
                        }
                        if (tmpHiddenTabAdv == false) {
                            App.storeGridAdv.reload();
                        }
                        if(tmpHiddenTabSellingProd == false){
                            App.storeGridSellingProd.reload();
                        }
                        if (tmpHiddenTabDispMethod == false) {
                            App.storeGridDispMethod.reload();
                        }
                    });
                    App.cboDfltShipToId.getStore().reload();
                  




                }
                , failure: function (msg, data) {

                    if (data.result.msgCode) {
                        if (data.result.msgCode == 2000)//loi trung key ko the add
                            HQ.message.show(data.result.msgCode, [App.cboCustId.fieldLabel, App.cboCustId.getValue()], '', true);
                        else HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                    }
                    else {
                        HQ.message.process(msg, data, true);
                    }
                }
            });
        }
        else {
            var fields = App.frmMain.getForm().getFields().each(
                    function (item) {
                        if (!item.isValid()) {
                            alert(item);
                        }
                    }
                );
        }


    }
}

var waitReloadTreeDone = function (value) {
    //App.cboCustId.setValue(data.result.custID);
    App.cboCustId.setValue(value);
}



// Xem lai
function Close() {
    if (App.frmMain.getRecord() != undefined) App.frmMain.updateRecord();
    if (storeIsChange(App.storeFormBig, false)) {
        HQ.message.show(5, '', 'closeScreen');

    } else {
        parent.App.tabAR20400.close();
    }
}
// Xem lai
function storeIsChange(store, isCreate) {
    if (isCreate == undefined) isCreate = true;
    if ((isCreate == true ? store.getChangedData().Created.length > 1 : false)
        || store.getChangedData().Updated != undefined
        || store.getChangedData().Deleted != undefined) {
        return true;
    }
    return false;
};
var closeScreen = function (item) {
    if (item == "yes") {

        Save();
    }
    else {
        if (parent.App.tabAR20400 != null)
            parent.App.tabAR20400.close();
    }
};
// Xac nhan xoa record tren grid
var deleteRecordForm = function (item) {
    if (item == "yes") {

        App.frmMain.submit({
            clientValidation: false,
            timeout: 1800000,
            waitMsg: HQ.common.getLang('DeletingData'),
            url: 'AR20400/AR20400DeleteHeader',
            params: {
                custID: App.cboCustId.getValue(),
                cpnyID: App.cboCpnyID.getValue(),
                status: App.cboStatus.value
            },
            //App.direct.AR20400Delete(App.cboCustId.getValue(),App.cboCpnyID.value,App.cboStatus.value, {
            success: function (data) {

                menuClick('refresh');
                
                App.cboCustId.getStore().reload();
                App.cboCustId.setValue('');
                App.storeFormBig.reload();
                //App.slmTree.selected().remove(true);
                if (tmpHiddenTree == false) {
                    var custIDcustName = App.cboCustId.value + "-" + App.txtCustName.value;
                    var record = App.IDTree.getStore().getNodeById(custIDcustName);
                    record.remove(true)
                }

                if (tmpHiddenTabLTT == false) {
                    App.storeGridLTTTop.reload();
                    App.storeGridLTTBot.reload();
                }
                if (tmpHiddenTabAdv == false) {
                    App.storeGridAdv.reload();
                }
                if (tmpHiddenTabSellingProd == false) {
                    App.storeGridSellingProd.reload();
                }
                if (tmpHiddenTabDispMethod == false) {
                    App.storeGridDispMethod.reload();
                }


            },
            failure: function () {
                //
                if (data.result.msgCode) {
                    if (data.result.msgCode == 2000)//loi trung key ko the add
                        HQ.message.show(data.result.msgCode, [App.cboCustId.fieldLabel, App.cboCustId.getValue()], '', true);
                    else HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                }
                else {
                    HQ.message.process(msg, data, true);
                }
            },
            eventMask: { msg: '@Util.GetLang("DeletingData")', showMask: true }
        });

    }
};



//var loadDataAutoHeaderSmall = function () {

//    if (App.storeFormSmall.getCount() == 0) {
//        App.storeFormSmall.insert(0, Ext.data.Record());
//    }
//    var record = App.storeFormSmall.getAt(0);
//    if (record) {
//        App.frmMain.getForm().loadRecord(record);
//        App.cboCpnyID.getStore().reload();
//    }
//};

//var loadGrid = function () {


//}
var setValueApproveStatus = function () {
    App.IDTree.store.tree.root.collapse();
}
var chooseNodeFromTree = function () {
    if (tmpHiddenTree == false) {//neu tree ko an
        //tu dong bat node tree khi chon 1 cai tu cboCustId
        //if (App.cboStatus.value == "O" || App.cboStatus.value == "A") {
        //    disableForm();
        //} else if (App.cboStatus.value == "H") {
        //    enableForm();
        //}
        if (!HQ.isNew) {
            var custIDcustName = App.cboCustId.value + "-" + App.txtCustName.value;

            var record = App.IDTree.getStore().getNodeById(custIDcustName);
            if (App.IDTree.getStore().getNodeById(custIDcustName)) {
                var depth = App.IDTree.getStore().getNodeById(custIDcustName).data.depth;
                if (depth == 3) {
                    //setTimeout(function () { waitCollacpse(); }, 1500);
                    //App.IDTree.store.tree.root.collapse();
                    App.IDTree.getStore().getNodeById(custIDcustName).parentNode.parentNode.parentNode.expand();
                    App.IDTree.getStore().getNodeById(custIDcustName).parentNode.parentNode.expand();
                    App.IDTree.getStore().getNodeById(custIDcustName).parentNode.expand();
                } else if (depth == 2) {
                    //setTimeout(function () { waitCollacpse(); }, 1500);
                    //App.IDTree.store.tree.root.collapse();
                    App.IDTree.getStore().getNodeById(custIDcustName).parentNode.parentNode.expand();
                    App.IDTree.getStore().getNodeById(custIDcustName).parentNode.expand();
                } else if (depth == 1) {
                    //setTimeout(function () { waitCollacpse(); }, 1500);
                    //App.IDTree.store.tree.root.collapse();
                    App.IDTree.getStore().getNodeById(custIDcustName).parentNode.expand();
                }
                var findNode = App.IDTree.items.items[0].store.data.items;
                for (var i = 0; i < findNode.length; i++) {
                    if (findNode[i].data.id == custIDcustName) {
                        App.slmTree.select(i);
                        tmpSelectedNode = App.slmTree.selected.items[0].data.id;
                        break;
                    }

                }
            }
        }
    }
    //App.IDTree.getStore().getNodeById(custIDcustName).select();
}
//var waitapproveStatus = function () {
//    //load len cboapprovestatus
//    if (App.cboApproveStatus.value == "" || App.cboApproveStatus.value == null) {
//        App.cboApproveStatus.setValue("H");
//        setTimeout(function () { waitToLoadHandle(); }, 1000);
//    }
//    //load hinh len khi chon invtID
//    var curRecord = App.frmMain.getRecord();
//    if (curRecord) {
//        if (curRecord.data.Picture) {
//            App.direct.GetImages(curRecord.data.Picture);
//        } else {
//            App.imgPPCStorePicReq.setImageUrl("");
//        }
//        if (curRecord.data.Media) {
//            setMediaImage();
//        }
//    } else {
//        App.imgPPCStorePicReq.setImageUrl("");

//    }



//}

var waitToLoadHandle = function () {
    if (App.cboHandle.value == null || App.cboHandle.value == "") {
        App.cboHandle.setValue("N");
    }
}
//var cboInvtID_Change = function (sender, e) {
//    if (tmpCopyForm == "1") {
//        tmpCopyFormSave = "1";
//        App.cboApproveStatus.setValue("H");
//        setTimeout(function () { waitToLoadHandle(); }, 1000);
//    } else {
//        App.imgPPCStoreMediaReq.setImageUrl("");
//        App.chooseGrid.setValue("1");
//        App.storeFormBig.load();
//        App.storeGrid.load();
//        //if (App.cboInvtID.value != "" && App.slmTree.selected.items[0].childNodes[0] == undefined) {
//        setTimeout(function () { choosefromcboInvtID(); }, 2000);
//        //}
//        setTimeout(function () { waitapproveStatus(); }, 2000);
//    }
//    //tmpCopyForm = "0";
//};


var NodeSelected_Change = function (store, operation, options) {
    //if (click == 0) {
    if (tmpHiddenTree == false)//neu tree ko an
    {
        parentRecordIDAll = options.textContent.split("-");
        parentRecordID = parentRecordIDAll[0];
        //if (selectNodeAfterInsert == "1") {
        //    selectNodeAfterInsert = "0";
        //} else {
        //enableForm();
        if (operation.childNodes.length == 0) {
            var custIDall = options.textContent.split("-")
            var custID1 = custIDall[0];
            App.cboCustId.setValue(custID1);
        }
    }


}


var cboApproveStatus_Change = function () {
    //App.cboApproveStatus.setValue("H");
    App.cboHandle.store.reload();
}


var cboCpnyID_Change = function (sender, newValue, oldValue) {
    //cpnyIDTree = App.cboCpnyID.value;
    App.cboCustId.getStore().reload();
    App.frmMain.submit({
        clientValidation: false,
        timeout: 1800000,
        waitMsg: HQ.common.getLang('checkingTreeAndHiddenTab'),
        url: 'AR20400/checkTreeAndHiddenTab',
        params: {
            cpnyID: App.cboCpnyID.getValue(),
        },
        success: function (action, data) {
            ReloadTree();
            //set bien tam va an hay hien tabLTT
            if (data.result.tmpHiddenTabContract == false) {
                tmpHiddenTabLTT = false;
            } else {
                tmpHiddenTabLTT = true;
                App.tabAtAR20400.tabBar.items.items[3].hide();
            }
            //set bien tam va an hay hien tabAdv
            if (data.result.tmpHiddenTabAdvTool == false) {
                tmpHiddenTabAdv = false;
            } else {
                tmpHiddenTabAdv = true;
                App.tabAtAR20400.tabBar.items.items[4].hide();
            }

            //set bien tam va an hay hien tabSellingProd
            if (data.result.tmpHiddenTabSellingProduct == false) {
                tmpHiddenTabSellingProd = false;
            } else {
                tmpHiddenTabSellingProd = true;
                App.tabAtAR20400.tabBar.items.items[5].hide();
            }

            //set bien tam va an hay hien tabDispMethod
            if (data.result.tmpHiddenTabDisplayMethod == false) {
                tmpHiddenTabDispMethod = false;
            } else {
                tmpHiddenTabDispMethod = true;
                App.tabAtAR20400.tabBar.items.items[6].hide();
            }

            //set bien tam AutoCustID
            if (data.result.tmpAutoCustID == false) {
                tmpAutoCustID = false;
            } else {
                tmpAutoCustID = true;

            }
        },
        failure: function (action, data) {
            tmpHiddenTree = true;
            App.frmTree.hide();
            //set bien tam va an hay hien tabLTT
            if (data.result.tmpHiddenTabContract == false) {
                tmpHiddenTabLTT = false;
            } else {
                tmpHiddenTabLTT = true;
                App.tabAtAR20400.tabBar.items.items[3].hide()
            }
            //set bien tam va an hay hien tabAdv
            if (data.result.tmpHiddenTabAdvTool == false) {
                tmpHiddenTabAdv = false;
            } else {
                tmpHiddenTabAdv = true;
                App.tabAtAR20400.tabBar.items.items[4].hide();
            }

            //set bien tam va an hay hien tabSellingProd
            if (data.result.tmpHiddenTabSellingProduct == false) {
                tmpHiddenTabSellingProd = false;
            } else {
                tmpHiddenTabSellingProd = true;
                App.tabAtAR20400.tabBar.items.items[5].hide();
            }

            //set bien tam va an hay hien tabDispMethod
            if (data.result.tmpHiddenTabDisplayMethod == false) {
                tmpHiddenTabDispMethod = false;
            } else {
                tmpHiddenTabDispMethod = true;
                App.tabAtAR20400.tabBar.items.items[6].hide();
            }

            //set bien tam AutoCustID
            if (data.result.tmpAutoCustID == false) {
                tmpAutoCustID = false;
            } else {
                tmpAutoCustID = true;
       
            }


        }
    });



    ////check coi co tab nao an ko
    //App.frmMain.submit({
    //    clientValidation: false,
    //    timeout: 1800000,
    //    waitMsg: HQ.common.getLang('checkingHiddenTab'),
    //    url: 'AR20400/checkHiddenTab',
    //    params: {
    //        cpnyID: App.cboCpnyID.getValue(),
    //    },
    //    success: function (action, data) {
            
    //    },
    //    failure: function (action, data) {
            
           
    //    }
    //});
    //tmpHiddenTabLTT
}

//dang DirectMethod co the load duoc node root slmTree
function ReloadTree() {
    try {
        App.direct.ReloadTreeAR20400(App.cboCpnyID.getValue(), {
            success: function (data) {
                //if (tampSaveButton == "1") {
                //    tampSaveButton = "0";
                //} else {
                //App.cboCustId.getStore().reload();
                //App.cboCountry.setValue("VN");
                //setTimeout(function () { waitStateLoad(); }, 1500);
                App.cboHandle.getStore().reload();
                
                // }
            },
            failure: function () {
                //
                alert("fail");
            },
            eventMask: { msg: 'Loading Tree', showMask: true }
        });
    } catch (ex) {
        alert(ex.message);
    }

};


var cboStatus_Change = function (sender, newValue, oldValue) {
    App.cboHandle.getStore().reload();
    if (App.cboStatus.value == "O" || App.cboStatus.value == "A") {
        disableForm();
    } else if (App.cboStatus.value == "H") {
        enableForm();
    }
}


//trước khi load trang busy la dang load data
var storeBeforeLoad = function (store) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'),App.frmMain);
};
var storeLoad = function (store) {



    setFocusAllCombo();

    HQ.isFirstLoad = true;
    HQ.common.showBusy(false);
    HQ.isNew = false;
    App.cboCustId.forceSelection = true;
    if (store.data.length == 0) {
        //cach cu
        //var record = Ext.create("App.AR_CustomerModel", {
        //    //CustId: "",
        //    Status: "H",
        //    ClassId: "CN",
        //    CustType: "N",
        //    PriceClassID: "NPPCH",
        //    CrRule: "N",
        //    Territory: "R001",
        //    Country: "VN",
        //    State: "0043",
        //    City: "HCM",
        //    District: "0043001",
        //    ExpiryDate: HQ.businessDate,
        //    EstablishDate: HQ.businessDate,
        //    Birthdate: HQ.businessDate,
        //    TaxDflt: "C",
        //    TaxID00: "VAT10",
        //    TaxID01: "VAT10",
        //    TaxID02: "VAT10",
        //    TaxID03: "VAT10"



        //});
        //App.storeFormBig.insert(0, record);
        //App.frmMain.getForm().loadRecord(App.storeFormBig.getAt(0));
    
        
        //var dateParts = HQ.businessDate.split("/");
        //var dateParts2 = dateParts[2].split(" ");
        //var date = new Date(dateParts[0] + "/" + dateParts[1] + "/" + dateParts2[0]);



        HQ.store.insertBlank(store, "CustId");
        record = store.getAt(0);
        
        record.data.Status = 'H';
        record.data.ClassId = 'CN';
        record.data.CustType = 'N';
        record.data.PriceClassID = 'NPPCH';
        record.data.CrRule = 'N';
        record.data.Territory = 'R001';
        record.data.Country = 'VN';
        //record.data.State = '0043';
        //record.data.City = 'HCM';
        //record.data.District = '0043001';

        record.data.ExpiryDate = HQ.businessDate;
        record.data.EstablishDate = HQ.businessDate;
        record.data.Birthdate = HQ.businessDate;
        record.data.TaxDflt = 'C';
        record.data.TaxID00 = 'OVAT10-00';
        record.data.TaxID01 = 'OVAT05-00';
        record.data.TaxID02 = 'VAT00';
        record.data.TaxID03 = 'NONEVAT';


        store.commitChanges();//commit cho record thanh updated muc dich de dung ham HQ.store.isChange
        HQ.isNew = true;//record la new
        if (tmpAutoCustID == false) {
            App.cboCustId.forceSelection = false;
        } else {
            App.cboCustId.forceSelection = true;
        }
        HQ.common.setRequire(App.frmMain);  //to do cac o la require            
        App.cboCustId.focus(true);//focus ma khi tao moi

        //setTimeout(function () { waitToSetAutoCustID(); }, 3000);
     
    }


    var record = store.getAt(0);
    tmpCity = store.getAt(0).data.City;
    tmpDistrict = store.getAt(0).data.District;
    App.frmMain.getForm().loadRecord(record);
    
    frmChange();
};

//var waitToSetAutoCustID = function () {
//    if (tmpAutoCustID == true) {
//        App.cboCustId.setEditable = false;
//    } else {
//        App.cboCustId.setEditable = true;
//    }
//};

var cboCustId_Change = function (sender, value) {
    if ((!HQ.isNew || sender.valueModels != null) && !App.storeFormBig.loading) {
        tmpCountryloadStoreOrForm = "1";
        App.storeFormBig.reload();
        App.cboDfltShipToId.getStore().reload();
        App.cboLTTContract.getStore().reload();

        setTimeout(function () { chooseNodeFromTree(); }, 1000);

        if (App.cboStatus.value == "O" || App.cboStatus.value == "A") {
            disableForm();
        } else if (App.cboStatus.value == "H") {
            enableForm();
        }

        if (tmpHiddenTabLTT == false) { // neu tab Contract ko bi an thi moi load store 
            App.storeGridLTTTop.reload();
            App.storeGridLTTBot.reload();// de neu co du lieu thi refresh trong cho grid bot LTT
        }

        if (tmpHiddenTabAdv == false) {// neu tab Adv ko bi an thi moi load store 
            App.storeGridAdv.reload();
        }

        if (tmpHiddenTabSellingProd == false) {// neu tab SellingProd ko bi an thi moi load store 
            App.storeGridSellingProd.reload();
        }

        if (tmpHiddenTabDispMethod == false) {// neu tab DispMethod ko bi an thi moi load store 
            App.storeGridDispMethod.reload();
        }

   

    }

}

var cboCustId_Select = function (sender, value) {
    if (sender.valueModels != null && !App.storeFormBig.loading) {
        tmpCountryloadStoreOrForm = "1";
        App.storeFormBig.reload();
        App.cboDfltShipToId.getStore().reload();
        App.cboLTTContract.getStore().reload();
     
        if (App.cboStatus.value == "O" || App.cboStatus.value == "A") {
            disableForm();
        } else if (App.cboStatus.value == "H") {
            enableForm();
        }

        if (tmpHiddenTabLTT == false) { // neu tab Contract ko bi an thi moi load store 
            App.storeGridLTTTop.reload();
            App.storeGridLTTBot.reload();// de neu co du lieu thi refresh trong cho grid bot LTT
        }

        if (tmpHiddenTabAdv == false) {// neu tab Adv ko bi an thi moi load store 
            App.storeGridAdv.reload();
        }

        if (tmpHiddenTabSellingProd == false) {// neu tab SellingProd ko bi an thi moi load store 
            App.storeGridSellingProd.reload();
        }

        if (tmpHiddenTabDispMethod == false) {// neu tab DispMethod ko bi an thi moi load store 
            App.storeGridDispMethod.reload();
        }
    }

};


//khi nhan combo xo ra, neu da thay doi thi ko xo ra
var cboCustId_Expand = function (sender, value) {
    if (App.cboCustId.getStore().data.length == 0) {
        App.cboCustId.getStore().reload();
    }
    if (HQ.isChange && App.cboCustId.getValue()) {
        App.cboCustId.collapse();
    }
};








var cboCountry_Change = function (sender, newValue, oldValue) {
    //bien tam tmpCountryloadStoreOrForm de bat neu chon CustID thi load form len du sau do set ve trang thai cu
    //cach cu
    //if (tmpCountryloadStoreOrForm == "1") {
    //    App.cboState.getStore().reload();
    //    App.frmMain.getForm().loadRecord(App.frmMain.getRecord());

    //} else {
    //    App.cboState.getStore().reload();
    //}

    //cach moi
    App.cboState.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord != undefined)
            if (curRecord.data.State) {
                App.cboState.setValue(curRecord.data.State);
            }
        var dt = HQ.store.findInStore(App.cboState.getStore(), ["State"], [App.cboState.getValue()]);
        if (!dt && App.cboCountry.forceSelection == true) {
            curRecord.data.State = '';
            App.cboState.setValue("");
        }

    });
}


var cboState_Change = function (sender, newValue, oldValue) {
    //cach cu
    //if (tmpCountryloadStoreOrForm == "1") {
    //    App.cboCity.getStore().reload();
    //    App.cboDistrict.getStore().reload();
    //    App.frmMain.getForm().loadRecord(App.frmMain.getRecord());
    //    App.cboCity.setValue(tmpCity);
    //    App.cboDistrict.setValue(tmpDistrict);
    //    tmpCountryloadStoreOrForm = "0";
    //} else {
    //    App.cboCity.getStore().reload();
    //    App.cboDistrict.getStore().reload();
    //}
    //cach moi
    App.cboCity.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord != undefined)
            if (curRecord.data.City) {
                App.cboCity.setValue(curRecord.data.City);
            }
        var dt = HQ.store.findInStore(App.cboCity.getStore(), ["City"], [App.cboCity.getValue()]);
        if (!dt && App.cboState.forceSelection == true) {
            curRecord.data.City = '';
            App.cboCity.setValue("");
        }

    });

    App.cboDistrict.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord != undefined)
            if (curRecord.data.District) {
                App.cboDistrict.setValue(curRecord.data.District);
            }
        var dt = HQ.store.findInStore(App.cboCity.getStore(), ["District"], [App.cboDistrict.getValue()]);
        if (!dt && App.cboState.forceSelection == true) {
            curRecord.data.City = '';
            App.cboDistrict.setValue("");
        }

    });
}



var cboBillCountry_Change = function (sender, newValue, oldValue) {
    //if (tmpCountryloadStoreOrForm == "1") {
    //    App.cboBillState.getStore().reload();
    //    App.frmMain.getForm().loadRecord(App.frmMain.getRecord());
    //} else {
    //    App.cboBillState.getStore().reload();
    //}
    //cach moi
    App.cboBillState.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord != undefined)
            if (curRecord.data.State) {
                App.cboBillState.setValue(curRecord.data.BillState);
            }
        var dt = HQ.store.findInStore(App.cboBillState.getStore(), ["BillState"], [App.cboBillState.getValue()]);
        if (!dt && App.cboBillState.forceSelection == true) {
            curRecord.data.State = '';
            App.cboBillState.setValue("");
        }

    });
}

var cboBillState_Change = function (sender, newValue, oldValue) {
    //if (tmpCountryloadStoreOrForm == "1") {
    //    App.cboBillCity.getStore().reload();
    //    App.frmMain.getForm().loadRecord(App.frmMain.getRecord());
    //    tmpCountryloadStoreOrForm = "0";
    //} else {
    //    App.cboBillCity.getStore().reload();
    //}
    //cach moi
    App.cboBillCity.getStore().load(function () {
        var curRecord = App.frmMain.getRecord();
        if (curRecord != undefined)
            if (curRecord.data.City) {
                App.cboBillCity.setValue(curRecord.data.BillCity);
            }
        var dt = HQ.store.findInStore(App.cboBillCity.getStore(), ["BillCity"], [App.cboBillCity.getValue()]);
        if (!dt && App.cboBillCity.forceSelection == true) {
            curRecord.data.City = '';
            App.cboBillCity.setValue("");
        }

    });
}

var waitBillCountryLoad = function () {
    App.cboBillState.setValue(App.cboState.value);
}
var waitBillStateLoad = function () {
    App.cboBillCity.setValue(App.cboCity.value);
}
var CopyFormToBill_Click = function () {
    tmpCountryloadStoreOrForm = "0"
    App.txtBillName.setValue(App.txtCustNam2.value);
    App.txtBillAttn.setValue(App.txtAttn.value);
    App.txtBillSalut.setValue(App.txtSalut.value);
    App.txtBillAddr1.setValue(App.txtAddr1.value);
    App.txtBillAddr2.setValue(App.txtAddr2.value);
    App.cboBillCountry.setValue(App.cboCountry.value);
    setTimeout(function () { waitBillCountryLoad(); }, 500);
    setTimeout(function () { waitBillStateLoad(); }, 500);
    App.txtBillZip.setValue(App.txtZip.value);
    App.txtBillPhone.setValue(App.txtPhone.value);
    App.txtBillFax.setValue(App.txtFax.value);

}

//Grid LTT/////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////
var GridTop_Change = function (sender, e) {
    HQ.isFirstLoad = true;
    App.storeGridLTTBot.reload();
    _index = App.slmTopGrid.selected.items[0].index;
};

var loadDataGridLTTContract = function (store) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            if (App.cboCustId.getValue() != "") {
                HQ.store.insertBlank(store, keysTop);
            }
        }
        HQ.isFirstLoad = false;
    }
    frmChange();
};

var loadDataGridLTTContractDetail = function (store) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            if (App.slmTopGrid.selected.items[0] != undefined) {
                HQ.store.insertBlank(store, keysBot);
            }
        }
        HQ.isFirstLoad = false;
    }
    frmChange();
};

var deleteRecordGridLTTContract = function (item) {
    if (item == "yes") {
        App.grdTop.deleteSelected();
        frmChange();
    }
};

var deleteRecordGridLTTContractDetail = function (item) {
    if (item == "yes") {
        App.grdBot.deleteSelected();
        frmChange();
    }
};

//Top Grid LTTContract
var grdTop_BeforeEdit = function (editor, e) {
    if (App.cboStatus.value == "O" || App.cboStatus.value == "A") {
        return false;
    } else if (App.cboStatus.value == "H") {
        return HQ.grid.checkBeforeEdit(e, keysTop);
    }

};
var grdTop_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdTop, e, keysTop);
};
var grdTop_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdTop, e, keysTop);
};
var grdTop_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdTop);
    stoChangedTop(App.storeGridLTTTop);
};

//Bot Grid LTTContractDetail
var grdBot_BeforeEdit = function (editor, e) {

    if (App.cboStatus.value == "O" || App.cboStatus.value == "A") {
        return false;
    } else if (App.cboStatus.value == "H") {
        return HQ.grid.checkBeforeEdit(e, keysBot);
    }
};
var grdBot_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdBot, e, keysBot);
};
var grdBot_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdBot, e, keysBot);
};
var grdBot_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdBot);
    stoChangedBot(App.storeGridLTTBot);
};

//Grid Adv/////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////
var loadDataGridAdv = function (store) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            if (App.cboCustId.getValue() != "") {
                HQ.store.insertBlank(store, keysAdv);
            }
        }
        HQ.isFirstLoad = false;
    }
    frmChange();
};

var deleteRecordGridAdv = function (item) {
    if (item == "yes") {
        App.grdAdv.deleteSelected();
        frmChange();
    }
};

//Grid Adv
var grdAdv_BeforeEdit = function (editor, e) {
    if (App.cboStatus.value == "O" || App.cboStatus.value == "A") {
        return false;
    } else if (App.cboStatus.value == "H") {
        return HQ.grid.checkBeforeEdit(e, keysAdv);
    }

};
var grdAdv_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdAdv, e, keysAdv);
};
var grdAdv_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdAdv, e, keysAdv);
};
var grdAdv_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdAdv);
    stoChangedTop(App.storeGridAdv);
};



//Grid SellingProd/////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////

var loadDataGridSellingProd = function (store) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            if (App.cboCustId.getValue() != "") {
                HQ.store.insertBlank(store, keysSellingProd);
            }
        }
        HQ.isFirstLoad = false;
    }
    frmChange();
};

var deleteRecordGridSellingProd = function (item) {
    if (item == "yes") {
        App.grdSellingProd.deleteSelected();
        frmChange();
    }
};


//Grid SellingProd
var grdSellingProd_BeforeEdit = function (editor, e) {
    if (App.cboStatus.value == "O" || App.cboStatus.value == "A") {
        return false;
    } else if (App.cboStatus.value == "H") {
        return HQ.grid.checkBeforeEdit(e, keysSellingProd);
    }

};
var grdSellingProd_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdSellingProd, e, keysSellingProd);
};
var grdSellingProd_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdSellingProd, e, keysSellingProd);
};
var grdSellingProd_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdSellingProd);
    stoChangedTop(App.storeGridSellingProd);
};


//Grid DispMethod/////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////

var loadDataGridDispMethod = function (store) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            if (App.cboCustId.getValue() != "") {
                HQ.store.insertBlank(store, keysDispMethod);
            }
        }
        HQ.isFirstLoad = false;
    }
    frmChange();
};

var deleteRecordGridDispMethod = function (item) {
    if (item == "yes") {
        App.grdDispMethod.deleteSelected();
        frmChange();
    }
};

//Grid DispMethod
var grdDispMethod_BeforeEdit = function (editor, e) {
    if (App.cboStatus.value == "O" || App.cboStatus.value == "A") {
        return false;
    } else if (App.cboStatus.value == "H") {
        return HQ.grid.checkBeforeEdit(e, keysDispMethod);
    }

};
var grdDispMethod_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdDispMethod, e, keysDispMethod);
};
var grdDispMethod_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdDispMethod, e, keysDispMethod);
};
var grdDispMethod_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdDispMethod);
    stoChangedTop(App.storeGridDispMethod);
};


var disableForm = function () {
    //App.cboCustId.setReadOnly(true);
    App.cboCpnyID.setReadOnly(true);
    App.cboClassId.setReadOnly(true);
    App.cboCustType.setReadOnly(true);
    App.txtCustName.setReadOnly(true);
    App.cboPriceClassID.setReadOnly(true);
    App.cboTerms.setReadOnly(true);
    App.txtTradeDisc.setReadOnly(true);
    App.cboCrRule.setReadOnly(true);
    App.txtCrLmt.setReadOnly(true);
    App.txtGracePer.setReadOnly(true);
    App.cboTerritory.setReadOnly(true);
    App.cboArea.setReadOnly(true);
    App.cboLocation.setReadOnly(true);
    App.cboChannel.setReadOnly(true);
    App.cboShopType.setReadOnly(true);
    App.chkGiftExch.setReadOnly(true);
    App.chkHasPG.setReadOnly(true);
    App.cboSlsperId.setReadOnly(true);
    App.cboDeliveryID.setReadOnly(true);
    App.cboSupID.setReadOnly(true);
    App.cboSiteId.setReadOnly(true);
    App.cboDfltShipToId.setReadOnly(true);
    App.txtCustFillPriority.setReadOnly(true);
    App.cboLTTContract.setReadOnly(true);
    App.cboDfltSalesRouteID.setReadOnly(true);
    App.txtEmpNum.setReadOnly(true);
    App.calExpiryDate.setReadOnly(true);
    App.cboEstablishDate.setReadOnly(true);
    App.cboBirthday.setReadOnly(true);
    App.txtCustNam2.setReadOnly(true);
    App.txtAttn.setReadOnly(true);
    App.txtSalut.setReadOnly(true);
    App.txtAddr1.setReadOnly(true);
    App.txtAddr2.setReadOnly(true);
    App.cboCountry.setReadOnly(true);
    App.cboState.setReadOnly(true);
    App.cboCity.setReadOnly(true);
    App.cboDistrict.setReadOnly(true);
    App.txtZip.setReadOnly(true);
    App.txtPhone.setReadOnly(true);
    App.txtFax.setReadOnly(true);
    App.txtEMailAddr.setReadOnly(true);
    
    App.txtBillName.setReadOnly(true);
    App.txtBillAttn.setReadOnly(true);
    App.txtBillSalut.setReadOnly(true);
    App.txtBillAddr1.setReadOnly(true);
    App.txtBillAddr2.setReadOnly(true);
    App.cboBillCountry.setReadOnly(true);
    App.cboBillState.setReadOnly(true);
    App.cboBillCity.setReadOnly(true);
    App.txtBillZip.setReadOnly(true);
    App.txtBillPhone.setReadOnly(true);
    App.txtBillFax.setReadOnly(true);
    App.cboTaxDflt.setReadOnly(true);
    App.txtTaxRegNbr.setReadOnly(true);
    App.txtTaxLocId.setReadOnly(true);
    App.cboTaxID00.setReadOnly(true);
    App.cboTaxID01.setReadOnly(true);
    App.cboTaxID02.setReadOnly(true);
    App.cboTaxID03.setReadOnly(true);

    //App.btnCopy.disable = true;
    App.btnCopy.disabled = true;
    //App.frmTopLeftTab1.setReadOnly(true);
    //App.frmTopRightTab1.setReadOnly(true);
    //App.frmBotLeftTab1.setReadOnly(true);
    //App.frmBotRightTab1.setReadOnly(true);
    //App.frmLeftTab2.setReadOnly(true);
    //App.frmRightTab2.setReadOnly(true);
    //App.frmMidTab3.setReadOnly(true);

}

var enableForm = function () {
    //App.cboCustId.readOnly = false;
    App.cboCpnyID.setReadOnly(false);
    App.cboClassId.setReadOnly(false);
    App.cboCustType.setReadOnly(false);
    App.txtCustName.setReadOnly(false);
    App.cboPriceClassID.setReadOnly(false);
    App.cboTerms.setReadOnly(false);
    App.txtTradeDisc.setReadOnly(false);
    App.cboCrRule.setReadOnly(false);
    App.txtCrLmt.setReadOnly(false);
    App.txtGracePer.setReadOnly(false);
    App.cboTerritory.setReadOnly(false);
    App.cboArea.setReadOnly(false);
    App.cboLocation.setReadOnly(false);
    App.cboChannel.setReadOnly(false);
    App.cboShopType.setReadOnly(false);
    App.chkGiftExch.setReadOnly(false);
    App.chkHasPG.setReadOnly(false);
    App.cboSlsperId.setReadOnly(false);
    App.cboDeliveryID.setReadOnly(false);
    App.cboSupID.setReadOnly(false);
    App.cboSiteId.setReadOnly(false);
    App.cboDfltShipToId.setReadOnly(false);
    App.txtCustFillPriority.setReadOnly(false);
    App.cboLTTContract.setReadOnly(false);
    App.cboDfltSalesRouteID.setReadOnly(false);
    App.txtEmpNum.setReadOnly(false);
    App.calExpiryDate.setReadOnly(false);
    App.cboEstablishDate.setReadOnly(false);
    App.cboBirthday.setReadOnly(false);
    App.txtCustNam2.setReadOnly(false);
    App.txtAttn.setReadOnly(false);
    App.txtSalut.setReadOnly(false);
    App.txtAddr1.setReadOnly(false);
    App.txtAddr2.setReadOnly(false);
    App.cboCountry.setReadOnly(false);
    App.cboState.setReadOnly(false);
    App.cboCity.setReadOnly(false);
    App.cboDistrict.setReadOnly(false);
    App.txtZip.setReadOnly(false);
    App.txtPhone.setReadOnly(false);
    App.txtFax.setReadOnly(false);
    App.txtEMailAddr.setReadOnly(false);
    
    App.txtBillName.setReadOnly(false);
    App.txtBillAttn.setReadOnly(false);
    App.txtBillSalut.setReadOnly(false);
    App.txtBillAddr1.setReadOnly(false);
    App.txtBillAddr2.setReadOnly(false);
    App.cboBillCountry.setReadOnly(false);
    App.cboBillState.setReadOnly(false);
    App.cboBillCity.setReadOnly(false);
    App.txtBillZip.setReadOnly(false);
    App.txtBillPhone.setReadOnly(false);
    App.txtBillFax.setReadOnly(false);
    App.cboTaxDflt.setReadOnly(false);
    App.txtTaxRegNbr.setReadOnly(false);
    App.txtTaxLocId.setReadOnly(false);
    App.cboTaxID00.setReadOnly(false);
    App.cboTaxID01.setReadOnly(false);
    App.cboTaxID02.setReadOnly(false);
    App.cboTaxID03.setReadOnly(false);

    App.btnCopy.enable();
    //App.frmTopLeftTab1.readOnly = false;
    //App.frmTopRightTab1.readOnly = false;
    //App.frmBotLeftTab1.readOnly = false;
    //App.frmBotRightTab1.readOnly = false;
    //App.frmLeftTab2.readOnly = false;
    //App.frmRightTab2.readOnly = false;
    //App.frmMidTab3.readOnly = false;

}

var setFocusAllCombo = function () {
    App.cboCpnyID.forceSelection = false;
    App.cboClassId.forceSelection = false;
    App.cboCustType.forceSelection = false;
    App.cboHandle.forceSelection = false;
    App.cboPriceClassID.forceSelection = false;
    App.cboTerms.forceSelection = false;
    App.cboCrRule.forceSelection = false;
    App.cboTerritory.forceSelection = false;
    App.cboArea.forceSelection = false;
    App.cboLocation.forceSelection = false;
    App.cboChannel.forceSelection = false;
    App.cboShopType.forceSelection = false;
    App.cboSlsperId.forceSelection = false;
    App.cboDeliveryID.forceSelection = false;
    App.cboSupID.forceSelection = false;
    App.cboSiteId.forceSelection = false;
    App.cboDfltShipToId.forceSelection = false;
    App.cboLTTContract.forceSelection = false;
    App.cboDfltSalesRouteID.forceSelection = false;
    App.cboCountry.forceSelection = false;
    App.cboState.forceSelection = false;
    App.cboCity.forceSelection = false;
    App.cboDistrict.forceSelection = false;
    App.cboBillCountry.forceSelection = false;
    App.cboBillState.forceSelection = false;
    App.cboBillCity.forceSelection = false;
    App.cboTaxDflt.forceSelection = false;
    App.cboTaxID00.forceSelection = false;
    App.cboTaxID01.forceSelection = false;
    App.cboTaxID02.forceSelection = false;
    App.cboTaxID03.forceSelection = false;

};

var tabSA_Setup_AfterRender = function (obj, padding) {
    if (this.parentAutoLoadControl != null)
        obj.setHeight(this.parentAutoLoadControl.getHeight() - padding);
    else {
        obj.setHeight(Ext.getBody().getViewSize().height - padding);
    }
};

var cboClassId_Change = function () {
  
        for (var i = 0; i < App.cboClassId.store.data.length; i++) {
            if (App.cboClassId.value == App.cboClassId.store.data.items[i].data.ClassID) {
                App.cboTaxID00.setValue(App.cboClassId.store.data.items[i].data.TaxID00);
                App.cboTaxID01.setValue(App.cboClassId.store.data.items[i].data.TaxID01);
                App.cboTaxID02.setValue(App.cboClassId.store.data.items[i].data.TaxID02);
                App.cboTaxID03.setValue(App.cboClassId.store.data.items[i].data.TaxID03);
            }
        }
    
};