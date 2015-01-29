
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
var menuClick = function (command) {
    switch (command) {
        case "first":
            if (_focusrecord == 1) {

                var combobox = App.cboInvtID;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboInvtID.setValue(combobox.store.getAt(0).data.InvtID);

            } else if (_focusrecord == 2) {
                App.SelectionModelMailAutoDetail.select(0);
            }

            break;
        case "prev":
            if (_focusrecord == 1) {

                var combobox = App.cboInvtID;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboInvtID.setValue(combobox.store.getAt(index - 1).data.InvtID);

            } else if (_focusrecord == 2) {
                App.SelectionModelMailAutoDetail.selectPrevious();
            }

            break;
        case "next":
            if (_focusrecord == 1) {

                var combobox = App.cboInvtID;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboInvtID.setValue(combobox.store.getAt(index + 1).data.InvtID);

            } else if (_focusrecord == 2) {
                App.SelectionModelMailAutoDetail.selectNext();
            }
            break;
        case "last":
            if (_focusrecord == 1) {

                var combobox = App.cboInvtID;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboInvtID.setValue(App.cboInvtID.store.getAt(App.cboInvtID.store.getTotalCount() - 1).data.InvtID);

            } else if (_focusrecord == 2) {
                App.SelectionModelMailAutoDetail.select(App.storeGrid.getCount() - 1);
            }



            break;
        case "refresh":
            App.storeFormBig.reload();
            App.storeGrid.reload();

            break;
        case "new":
            if (isInsert) {

                if (_focusrecord == 2) {

                    App.SelectionModelMailAutoDetail.select(App.storeGrid.getCount() - 1);

                    if (App.storeGrid.getCount() == 0) {

                        App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());
                        App.SelectionModelMailAutoDetail.select(App.storeGrid.getCount() - 1);
                        App.grd.editingPlugin.startEditByPosition({ row: App.storeGrid.getCount() - 1, column: 1 });

                    } else if (App.SelectionModelMailAutoDetail.selected.items[0].data.CpnyID != "") {
                        App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());
                        App.SelectionModelMailAutoDetail.select(App.storeGrid.getCount() - 1);
                        App.grd.editingPlugin.startEditByPosition({ row: App.storeGrid.getCount() - 1, column: 1 });
                    }




                } else if (_focusrecord == 1) {
                   
                        App.cboInvtID.setValue('');
                        App.storeFormBig.reload();
                        //var index = App.ApproveStatusAll.indexOf(App.ApproveStatusAll.findRecord("Code", "H"));
                    

                   
                }

            }
            break;
        case "delete":
            var curRecord = App.dataForm.getRecord();
            if (isDelete) {
                if (App.cboInvtID.value != "") {

                    if (_focusrecord == 1) {
                        callMessage(11, '', 'deleteRecordForm');
                    } else if (_focusrecord == 2) {
                        callMessage(11, '', 'deleteRecordGrid')
                    }
                }
            }
            break;
        case "save":
            if (isUpdate || isInsert || isDelete) {

                if (isAllValid(App.storeGrid.getChangedData().Created)
                    && isAllValid(App.storeGrid.getChangedData().Updated)) {
                   // if (App.Tree.selected.items[0].childNodes.length != 0) {
                        Save();
                   // } else {
                    //    callMessage(8081, '', null);
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
var refreshTree = function (i) {
    App.Tree.select(i);
}
var refreshTree2 = function (dt) {
    App.cboInvtID.setValue(dt);
    var findNode = App.IDTree.items.items[0].store.data.items;
    for (var i = 0; i < findNode.length; i++) {
        if (findNode[i].data.id == node) {
            //click = 1;
            App.Tree.select(i);
            //setTimeout(function () { refreshTree(i); }, 7000);
            break;

        }

    }
}
function Save() {



    var curRecord = App.dataForm.getRecord();
    curRecord.data.Images = App.imgPPCStorePicReq.imageUrl;
    App.dataForm.getForm().updateRecord();
   //App.dataForm.updateRecord(curRecord);
    if (App.dataForm.isValid()) {
        App.dataForm.submit({
            waitMsg: 'Submiting...',
            url: 'IN20500/Save',
            params: {
                lstheader: Ext.encode(App.storeFormBig.getChangedData({ skipIdForPhantomRecords: false })),//,
                lstheader2: Ext.encode(App.storeFormSmall.getChangedData({ skipIdForPhantomRecords: false })),
                lstgrd: Ext.encode(App.storeGrid.getChangedData({ skipIdForPhantomRecords: false })),
                InvtID: App.cboInvtID.getValue(),
                Handle: App.cboHandle.getValue(),
                NodeID: App.Tree.selected.items[0].data.text,
                NodeLevel: App.Tree.selected.items[0].data.depth,
                hadchild: App.Tree.selected.items[0].childNodes.length,
                approveStatus: App.cboApproveStatus.getValue(),
                Public: App.chkPublic.getValue(),
                StkItem: App.chkStkItem.getValue(),
                imageChange: App.dataForm.getRecord().data.Picture
            },
            success: function (result, data) {
                App.cboInvtID.getStore().reload();
                //App.cboHandle.setValue("");
                callMessage(201405071, '', null);
                //App.cboInvtID.setValue('');
                menuClick("refresh");
                var dt = Ext.decode(data.response.responseText);
                //App.cboInvtID.setValue("");

                //setTimeout(function () { refreshTree(dt); }, 2000);
                //App.cboInvtID.setValue(dt);



                //setTimeout(function () { refreshTree(result,data); }, 1000);
                if(data.result.value3 == "addNew"){
                    var newNode = App.Tree.selected.items[0].data.id;
                    var record = App.IDTree.getStore().getNodeById(newNode);
                    var node = data.result.value + "-" + data.result.value2;
                   
                    record.appendChild({ text: node, leaf: true, id: node });
                    
                    var findNode = App.IDTree.items.items[0].store.data.items;
                    for (var i = 0; i < findNode.length; i++) {
                        if (findNode[i].data.id == node) {
                            //click = 1;
                            App.Tree.select(i);
                            //setTimeout(function () { refreshTree(i); }, 7000);
                            break;
                        }
                    }
                }
                if (data.result.value3 == "update") {
                    var newNode = App.Tree.selected.items[0].data.id;
                    var record = App.IDTree.getStore().getNodeById(newNode);
                    var node = data.result.value + "-" + data.result.value2;
                    if (newNode != node) {
                        record.parentNode.appendChild({ text: node, leaf: true, id: node });
                        var findNode = App.IDTree.items.items[0].store.data.items;
                        for (var i = 0; i < findNode.length; i++) {
                            if (findNode[i].data.id == node) {
                                //click = 1;
                                App.Tree.select(i);
                                //setTimeout(function () { refreshTree(i); }, 7000);

                            }
                        }
                        var newNode2 = App.Tree.selected.items[0].data.id;
                        var record2 = App.IDTree.getStore().getNodeById(newNode2);
                        record2.remove(true);
                        record.parentNode.replaceChild(record2, record);
                        var findNode = App.IDTree.items.items[0].store.data.items;
                        for (var i = 0; i < findNode.length; i++) {
                            if (findNode[i].data.id == node) {
                                //click = 1;
                                App.Tree.select(i);
                                //setTimeout(function () { refreshTree(i); }, 7000);
                                break;
                            }
                        }
                    }


                }



                
            }
            , failure: function (errorMsg, data) {

                //var dt = Ext.decode(data.response.responseText);
                //callMessage(dt.code, dt.colName + ',' + dt.value, null);
                if (data.result.code) {
                    callMessage(data.result.code, '', '');
                }
                else {
                    processMessage(errorMsg, data);
                }
            }
        });
    }
    else {
        var fields = App.dataForm.getForm().getFields().each(
                function (item) {
                    if (!item.isValid()) {
                        alert(item);
                    }
                }
            );
    }
}


// Xem lai
function Close() {
    if (App.dataForm.getRecord() != undefined) App.dataForm.updateRecord();
    if (App.storeGrid.getChangedData().Updated == undefined &&
        App.storeGrid.getChangedData().Deleted == undefined &&
        App.dataForm.getRecord() == undefined)
        parent.App.tabIN20500.close();
    else if (App.storeGrid.getChangedData().Updated != undefined ||
        App.storeGrid.getChangedData().Created != undefined ||
        App.storeGrid.getChangedData().Deleted != undefined ||
        storeIsChange(App.storeFormSmall, false)) {
        callMessage(5, '', 'closeScreen');
    } else {
        parent.App.tabIN20500.close();
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
        if (parent.App.tabIN20500 != null)
            parent.App.tabIN20500.close();
    }
};
// Xac nhan xoa record tren grid
var deleteRecordForm = function (item) {
    if (item == "yes") {

        try {
            App.direct.IN20500Delete(App.cboInvtID.getValue(), {
                success: function (data) {
                    menuClick('refresh');
                    var invtIDDescr = App.cboInvtID.value + "-" + App.txtDescr.value;
                    var record = App.IDTree.getStore().getNodeById(invtIDDescr);
                    App.cboInvtID.getStore().reload();
                    App.cboInvtID.setValue('');
                    App.storeFormBig.reload();
                    //App.Tree.selected().remove(true);
                    record.remove(true)

                    
                },
                failure: function () {
                    //
                },
                eventMask: { msg: '@Util.GetLang("DeletingData")', showMask: true }
            });
        } catch (ex) {
            alert(ex.message);
        }
    }
};

var deleteRecordGrid = function (item) {
    if (item == "yes") {
        App.grd.deleteSelected();

    }
};
//check value

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
    if (!isUpdate)
        return false;
    strKeyGrid = e.record.idProperty.split(',');

    if (strKeyGrid.indexOf(e.field) != -1) {
        if (e.record.data.CpnyID != "") {
            return false;
        }
    }

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
            App.SelectionModelMailAutoDetail.selected.items[0].set('CpnyName', '');
            return false;
        }
    }
    return true;
};

var grd_CancelEdit = function (editor, e) {
    if (e.record.phantom) {
        if (App.cboCpnyID.getValue == "") {
            e.store.remove(e.record);
        }
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

var loadDataAutoHeaderBig = function () {

    if (App.storeFormBig.getCount() == 0) {
        App.storeFormBig.insert(0, Ext.data.Record());
    }
    var record = App.storeFormBig.getAt(0);
    if (record) {
        App.dataForm.getForm().loadRecord(record);
        App.cboCpnyID.getStore().reload();
    }
};

var loadDataAutoHeaderSmall = function () {

    if (App.storeFormSmall.getCount() == 0) {
        App.storeFormSmall.insert(0, Ext.data.Record());
    }
    var record = App.storeFormSmall.getAt(0);
    if (record) {
        App.dataForm.getForm().loadRecord(record);
        App.cboCpnyID.getStore().reload();
    }
};

var loadGrid = function () {

    
}
var setValueApproveStatus = function () {
    App.IDTree.store.tree.root.collapse();
}
var choosefromcboInvtID = function () {
    //tu dong bat node tree khi chon 1 cai tu cboInvtID
    var invtIDDescr = App.cboInvtID.value + "-" + App.txtDescr.value;
    
    var record = App.IDTree.getStore().getNodeById(invtIDDescr);
    var depth = App.IDTree.getStore().getNodeById(invtIDDescr).data.depth;
    if (depth == 3) {
        setTimeout(function () { waitCollacpse(); }, 1500);
        //App.IDTree.store.tree.root.collapse();
        App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.parentNode.parentNode.expand();
        App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.parentNode.expand();
        App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.expand();
    } else if (depth == 2) {
        setTimeout(function () { waitCollacpse(); }, 1500);
        //App.IDTree.store.tree.root.collapse();
        App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.parentNode.expand();
        App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.expand();
    } else if (depth == 1) {
        setTimeout(function () { waitCollacpse(); }, 1500);
        //App.IDTree.store.tree.root.collapse();
        App.IDTree.getStore().getNodeById(invtIDDescr).parentNode.expand();
    }
    var findNode = App.IDTree.items.items[0].store.data.items;
    for (var i = 0; i < findNode.length; i++) {
        if (findNode[i].data.id == invtIDDescr) {
            App.Tree.select(i);
            break;
        }

    }
   
    //App.IDTree.getStore().getNodeById(invtIDDescr).select();
}
var waitapproveStatus = function () {
    //load len cboapprovestatus
    if (App.cboApproveStatus.value == "" || App.cboApproveStatus.value == null) {
        App.cboApproveStatus.setValue("H");
        if (App.cboHandle.value == null || App.cboHandle.value == "") {
            App.cboHandle.setValue("N");
        }
    }
    //load hinh len khi chon invtID
    var curRecord = App.dataForm.getRecord();
    if (curRecord) {
        if (curRecord.data.Picture) {
            App.direct.GetImages(curRecord.data.Picture);
        } else {
            App.imgPPCStorePicReq.setImageUrl("");
        }
        if (curRecord.data.Media) {
            setMediaImage();
        }
    }else{
        App.imgPPCStorePicReq.setImageUrl("");

    }



}
var cboInvtID_Change = function (sender, e) {
    
    
    App.chooseGrid.setValue("1");
    App.storeFormBig.load();
    App.storeGrid.load();
    //if (App.cboInvtID.value != "" && App.Tree.selected.items[0].childNodes[0] == undefined) {
        setTimeout(function () { choosefromcboInvtID(); }, 2000);
    //}
    setTimeout(function () { waitapproveStatus(); }, 2000);
    
   
};

var setValueToGrid = function () {
    App.tabAtIN20500.setActiveTab(App.tabCompany);
    if (App.chooseGrid.value == "2" && App.tabAtIN20500.activeTab.id == "tabCompany") {
        for (var i = 0; i <= App.storeGrid.getCount() - 1; i++) {
            App.SelectionModelMailAutoDetail.select(0);
            var cpnyid = App.SelectionModelMailAutoDetail.selected.items[0].data.CpnyID;
            var cpnyname = App.SelectionModelMailAutoDetail.selected.items[0].data.CpnyName;
            App.grd.deleteSelected();
            App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());
            App.SelectionModelMailAutoDetail.select(App.storeGrid.getCount() - 1);
            App.SelectionModelMailAutoDetail.selected.items[0].set('CpnyID', cpnyid);
            App.SelectionModelMailAutoDetail.selected.items[0].set('CpnyName', cpnyname);
        }
    }
    App.tabAtIN20500.setActiveTab(App.tabInfo)
}

var cboClassID_Change = function (sender, e) {
    if (_focusrecord == 3) {
        App.chooseGrid.setValue("2");
        App.storeFormSmall.load();
        App.storeGrid.load();
        setTimeout(function () { setValueToGrid(); }, 1500);
    }
  





};

var txtDescr_Change = function (sender, e) {
    //var combobox = App.cboInvtID;
    //var v = combobox.getValue();
    //var d = App.cboInvtID.value + "-" + App.txtDescr.value;
    //var record = combobox.findRecord(d, v);
    //var index = combobox.store.indexOf(record);
    //App.Tree.select(index);
}

var chkIsAttachFile_Change = function (sender, e) {
    if (e) {
        App.chkIsDeleteFile.setValue(true);
        App.chkIsDeleteFile.disable();
    }
    else {
        App.chkIsDeleteFile.enable();
    }


}

var Focus2_Change = function (sender, e) {


    _focusrecord = 2;

}

var Focus1_Change = function (sender, e) {



    _focusrecord = 1;
}

var Focus3_Change = function (sender, e) {



    _focusrecord = 3;
}
//var cboReportID_Change = function (sender, e) {
//    //App.SelectionModelMailAutoDetail.selected.items[0].set('ReportViewID', '');

//    App.cboReportViewID.getStore().load();

//}

var tabSA_Setup_AfterRender = function (obj, padding) {
    if (this.parentAutoLoadControl != null) {
        obj.setHeight(this.parentAutoLoadControl.getHeight() - padding);
    }
    else {
        obj.setHeight(Ext.getBody().getViewSize().height - padding);
    }
};
var frmloadAfterRender = function (obj) {
    App.storePOPriceHeader.load();
    App.storePOPrice.load();
    App.storePOPriceCpny.load();
    lockcontrol();
};




var cboStkUnit_Change = function (sender, e) {

    App.cboDfltPOUnit.getStore().reload();
    App.cboDfltSOUnit.getStore().reload();

}

var PrefixValue_Change = function (sender, e) {
    if (App.txtLotSerFxdVal.value.length != App.txtLotSerFxdLen.value) {
        callMessage(22, e.value, '');
    } else {
        prefixvalue = App.txtLotSerFxdVal.getValue();
        shownextlotserial = prefixvalue + lastfixvalue;
        App.lblShowNextLotSerial.setText(shownextlotserial);
        App.lblShowNextLotSerial.show();
    }

}

var LastFixValue_Change = function (sender, e) {
    if (App.txtLotSerNumVal.value.length != App.txtLotSerNumLen.value) {
        callMessage(22, e.value, '');
    } else {
        lastfixvalue = App.txtLotSerNumVal.getValue();
        shownextlotserial = prefixvalue + lastfixvalue;
        App.lblShowNextLotSerial.setText(shownextlotserial);
        App.lblShowNextLotSerial.show();

    }
}

var cboLotSerTrack_Change = function () {
    if(App.cboLotSerTrack.value == null)
    {
        App.tabLotSerial.tab.disable(true);
        App.tabLotSerial.disable(true);
        App.storeGrid.reload();
    }
    else if (App.cboLotSerTrack.displayTplData[0].Code == "L" || App.cboLotSerTrack.displayTplData[0].Code == "S") {
        App.tabLotSerial.tab.enable(true);
        App.tabLotSerial.enable(true);
        prefixvalue = App.txtLotSerFxdVal.getValue();
        lastfixvalue = App.txtLotSerNumVal.getValue();
        shownextlotserial = prefixvalue + lastfixvalue;
        App.lblShowNextLotSerial.setText(shownextlotserial);
        App.lblShowNextLotSerial.show();
    } else if(App.cboLotSerTrack.displayTplData[0].Code == "N"){
        App.tabLotSerial.tab.disable(true);
        App.tabLotSerial.disable(true);
        App.storeGrid.reload();

    }

}

var chkPublic_Change = function (item) {

    if (App.chkPublic.getValue() == true) {
        App.tabCompany.tab.disable(true);
        App.tabCompany.disable(true);
        if (App.tabAtIN20500.activeTab.id == "tabCompany") {
            App.tabAtIN20500.setActiveTab(App.tabInfo);
        }
    } else {

        App.tabCompany.tab.enable(true);
        App.tabCompany.enable(true);

    }


}

var cboCpnyID_Change = function (value) {
    var k = value.displayTplData[0].CpnyName;
    App.SelectionModelMailAutoDetail.selected.items[0].set('CpnyName', k);


}

var NextShowNextLotSerial_AfterRender = function (value) {
    prefixvalue = App.txtLotSerFxdVal.getValue();
    lastfixvalue = App.txtLotSerNumVal.getValue();
    shownextlotserial = prefixvalue + lastfixvalue;
    App.lblShowNextLotSerial.setText(shownextlotserial);
    App.lblShowNextLotSerial.show();

}

var NodeSelected_Change = function (store, operation, options) {
    //if (click == 0) {
        if (operation.childNodes.length == 0) {
            var invtall = options.textContent.split("-")
            var invt1 = invtall[0];
            App.cboInvtID.setValue(invt1);
        }

   // }
    //click = 0;

}


var cboApproveStatus_Change = function () {
    //App.cboApproveStatus.setValue("H");
    App.cboHandle.store.reload();
    


}




var Tab_Change = function (sender, e)
{

}


function UploadImage() {
    App.dataForm.submit({
        waitMsg: 'Uploading your file...',
        url: 'IN20500/Upload',

        success: function (result) {
            //
            var curRecord = App.dataForm.getRecord();
            curRecord.data.Picture = App.imgPPCStorePicReq.imageUrl;
            //App.txtImageChange.setValue("1");
            curRecord.setDirty();
        },
        failure: function (error) {
            //
        }
    });
};


var upload = function () {
   
}

// Event when uplPPCStorePicReq is change a file
var NamePPCStorePicReq_Change = function (sender, e) {
    var fileName = sender.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (ext == "jpg" || ext == "png" || ext == "gif") {
        UploadImage();
        //setTimeout(function () { upload(); }, 5000);
        //var curRecord = App.dataForm.getRecord();
        //curRecord.data.Picture = App.imgPPCStorePicReq.imageUrl;
        //curRecord.setDirty();
    } else {
        alert("Please choose a picture! (.jpg, .png, .gif)");
        sender.reset();
    }
};

// Click to clear image of sales person
var btnClearImage_Click = function (sender, e) {
    App.imgPPCStorePicReq.setImageUrl("");
    var curRecord = App.dataForm.getRecord();
    curRecord.data.Images = "";
    curRecord.setDirty();
};

function UploadMedia() {
    App.dataForm.submit({
        waitMsg: 'Uploading your file...',
        url: 'IN20500/UploadMedia',
        params: {
            
            InvtID: App.cboInvtID.getValue(),
            Descr: App.txtDescr.getValue()

        },
        success: function (result, data) {
            //

            //var curRecord = App.dataForm.getRecord();
            //curRecord.data.Picture = App.imgPPCStorePicReq.imageUrl;
            //curRecord.setDirty();

            //App.imgPPCStoreMediaReq.setimageUrl(data.result.value);
            alert("da upload xong");
            App.imgPPCStoreMediaReq.setImageUrl(data.result.value);
            var curRecord = App.dataForm.getRecord();
            curRecord.data.Media = data.result.value2;
            curRecord.setDirty();
            //App.Window.show();
        },
        failure: function (error) {
            alert("fail");
        }
    });
};

var NamePPCStoreMediaReq_Change = function (sender, e) {
    //App.Window.show();
    //runProgress4(App.Progress4, App.ButtonProgress);
    var fileName = sender.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (ext == "mp4" || ext == "wmv") {
        UploadMedia();
        //setTimeout(function () { upload(); }, 5000);
        //var curRecord = App.dataForm.getRecord();
        //curRecord.data.Picture = App.imgPPCStorePicReq.imageUrl;
        //curRecord.setDirty();
    } else {
        alert("Please choose a Media! (.mp4, .wmv)");
        sender.reset();
    }
};

var btnClearMedia_Click = function (sender, e) {
    App.imgPPCStorePicReq.setImageUrl("");
    var curRecord = App.dataForm.getRecord();
    curRecord.data.Images = "";
    curRecord.setDirty();
};

function setMediaImage() {
    App.dataForm.submit({
        
        url: 'IN20500/SetMediaImage',

        success: function (result,data) {
            //
            App.imgPPCStoreMediaReq.setImageUrl(data.result.value);
        },
        failure: function (error) {
            //
        }
    });
};

var PlayVideo = function (sender, e) {
    var curRecord = App.dataForm.getRecord();
    if (curRecord) {
        if (curRecord.data.Media) {
            App.direct.PlayMedia(curRecord.data.Media);
        }
    }
};