
var selectedIndex = 0;
var _hold = "H";
var _curSlsperid = "";
var _focusrecord = 0;
var beforeedit = '';
var prefixvalue = "";
var lastfixvalue = "";
var shownextlotserial = "";

var menuClick = function (command) {
    switch (command) {
        case "first":
            if (_focusrecord == 1) {

                var combobox = App.cboClassID;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboClassID.setValue(combobox.store.getAt(0).data.ClassID);

            } else if (_focusrecord == 2) {
                App.SelectionModelMailAutoDetail.select(0);
            }
            
            break;
        case "prev":
            if (_focusrecord == 1) {

                var combobox = App.cboClassID;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboClassID.setValue(combobox.store.getAt(index - 1).data.ClassID);

            } else if (_focusrecord == 2) {
                App.SelectionModelMailAutoDetail.selectPrevious();
            }
            
            break;
        case "next":
            if (_focusrecord == 1) {
                            
                var combobox = App.cboClassID;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboClassID.setValue(combobox.store.getAt(index + 1).data.ClassID);
               
            } else if (_focusrecord == 2) {
                App.SelectionModelMailAutoDetail.selectNext();
            }
            break;
        case "last":
            if (_focusrecord == 1) {

                var combobox = App.cboClassID;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                App.cboClassID.setValue(App.cboClassID.store.getAt(App.cboClassID.store.getTotalCount() - 1).data.ClassID);

            } else if (_focusrecord == 2) {
                App.SelectionModelMailAutoDetail.select(App.storeGrid.getCount() - 1);
            }
            

            
            break;
        case "refresh":
            App.storeForm.reload();
            App.storeGrid.reload();
       
            break;
        case "new":
            if (isInsert) {
              
                if (_focusrecord == 2 )
                {
                   
                    App.SelectionModelMailAutoDetail.select(App.storeGrid.getCount() -1);
                    
                    if (App.storeGrid.getCount() == 0)
                    {
                        
                        App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());
                        App.SelectionModelMailAutoDetail.select(App.storeGrid.getCount() - 1);
                        App.grd.editingPlugin.startEditByPosition({ row: App.storeGrid.getCount() - 1, column: 1 });
                        
                    } else if (App.SelectionModelMailAutoDetail.selected.items[0].data.CpnyID != "")
                    {
                        App.storeGrid.insert(App.storeGrid.getCount(), Ext.data.Record());
                        App.SelectionModelMailAutoDetail.select(App.storeGrid.getCount() - 1);
                        App.grd.editingPlugin.startEditByPosition({ row: App.storeGrid.getCount() - 1, column: 1 });
                    }
                
   
               

                } else if (_focusrecord == 1)
                {
                    App.cboClassID.setValue('');
                    App.storeForm.reload();
                   
                }
                  
            }
            break;
        case "delete":
            var curRecord = App.dataForm.getRecord();
            if (isDelete) {
                if (App.cboClassID.value != "") {

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


   
    var curRecord = App.dataForm.getRecord();
    App.dataForm.getForm().updateRecord();
    if (App.dataForm.isValid()) {
        App.dataForm.submit({
            waitMsg: 'Submiting...',
            url: 'IN20200/Save',
            params: {
                lstheader: Ext.encode(App.storeForm.getChangedData({ skipIdForPhantomRecords: false })),//,
            
                lstgrd: Ext.encode(App.storeGrid.getChangedData({ skipIdForPhantomRecords: false })),
                classID: App.cboClassID.getValue()
            },
            success: function (result, data) {
                App.cboClassID.getStore().reload();
                
                callMessage(201405071, '', null);

                var dt = Ext.decode(data.response.responseText);
                App.cboClassID.setValue(dt.value);

                menuClick("refresh");
            }
            , failure: function (errorMsg, data) {
              
                var dt = Ext.decode(data.response.responseText);
                callMessage(dt.code, dt.colName + ',' + dt.value, null);
            }
        });
    }
}


// Xem lai
function Close() {
    if (App.dataForm.getRecord() != undefined) App.dataForm.updateRecord();
    if (App.storeGrid.getChangedData().Updated == undefined &&
        App.storeGrid.getChangedData().Deleted == undefined &&
        App.dataForm.getRecord() == undefined)
        parent.App.tabIN20200.close();
    else if (App.storeGrid.getChangedData().Updated != undefined ||
        App.storeGrid.getChangedData().Created != undefined ||
        App.storeGrid.getChangedData().Deleted != undefined ||
        storeIsChange(App.storeForm, false)) {
        callMessage(5, '', 'closeScreen');
    } else {
        parent.App.tabIN20200.close();
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
        if (parent.App.tabIN20200 != null)
            parent.App.tabIN20200.close();
    }
};
// Xac nhan xoa record tren grid
var deleteRecordForm = function(item) {
    if(item == "yes"){
        
        try {
            App.direct.IN20200Delete(App.cboClassID.getValue(), {
                success: function (data) {
                    menuClick('refresh');
                    App.cboClassID.getStore().reload();
                    App.cboClassID.setValue('');
                    App.storeForm.reload();

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
var isAllValidKey=function(items){      
    if(items!=undefined){
        for(var i=0; i<items.length; i++){
            for(var j=0;j<strKeyGrid.length;j++)
            {
                if (items[i][strKeyGrid[j]] == '' || items[i][strKeyGrid[j]] == undefined)
                    return false;  
            }
        }
        return true;
    }else{
        return true;
    }
}; 




    function selectRecord (grid, record) {
        var record = grid.store.getById(id);            
        grid.store.loadPage(grid.store.findPage(record), {
            callback : function () {
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
        } else {
            return true;
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
       
    if (strKeyGrid.indexOf(e.field) != -1)
    {
        if (duplicated(App.storeGrid, e))
        {
            callMessage(1112, e.value, '');
            if (App.SelectionModelMailAutoDetail.selected.items[0].data.CpnyID == "") {
                App.SelectionModelMailAutoDetail.selected.items[0].set('CpnyName', '');
            } else {
                App.SelectionModelMailAutoDetail.select(App.storeGrid.getCount() - 1);
                App.SelectionModelMailAutoDetail.selected.items[0].set('CpnyName', '');
            }
            return false;
        }
    }
    return true;
};
    
var grd_CancelEdit = function (editor, e) {
    if (e.record.phantom) {
        if (App.cboCpnyID.getValue == "")
        {
            e.store.remove(e.record);
        }
    }
};
var grd_Reject= function (record){
    if(record.data.tstamp=='')
    {         
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

var loadDataAutoHeader = function () {
   
    if (App.storeForm.getCount() == 0) {
        App.storeForm.insert(0, Ext.data.Record());
    }
    var record = App.storeForm.getAt(0);
    if (record) {
        App.dataForm.getForm().loadRecord(record);
        App.cboCpnyID.getStore().reload();
    }
};




   
var cboClassID_Change = function (sender, e) {

    App.storeForm.reload();
    App.storeGrid.reload();
   
   
};

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


//var cboReportID_Change = function (sender, e) {
//    //App.SelectionModelMailAutoDetail.selected.items[0].set('ReportViewID', '');

//    App.cboReportViewID.getStore().load();

//}

var tabSA_Setup_AfterRender = function (obj, padding) {
    if (this.parentAutoLoadControl != null)
        obj.setHeight(this.parentAutoLoadControl.getHeight() - padding);
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




var cboDfltStkUnit_Change = function (sender, e){

    App.cboDfltPOUnit.getStore().reload();
    App.cboDfltSOUnit.getStore().reload();

}

var PrefixValue_Change = function (sender, e) {
    if (App.txtDfltLotSerFxdVal.value.length != App.txtDfltLotSerFxdLen.value) {
        callMessage(22, e.value, '');
    } else {
        prefixvalue = App.txtDfltLotSerFxdVal.getValue();
        shownextlotserial = prefixvalue + lastfixvalue;
        App.lblShowNextLotSerial.setText(shownextlotserial);
        App.lblShowNextLotSerial.show();
    }

}

var LastFixValue_Change = function (sender, e) {
    if (App.txtDfltLotSerNumVal.value.length != App.txtDfltLotSerNumLen.value) {
        callMessage(22, e.value, '');
    } else {
        lastfixvalue = App.txtDfltLotSerNumVal.getValue();
        shownextlotserial = prefixvalue + lastfixvalue;
        App.lblShowNextLotSerial.setText(shownextlotserial);
        App.lblShowNextLotSerial.show();

    }
}

var cboDfltLotSerTrack_Change = function () {
    if (App.cboDfltLotSerTrack.getValue()) {
        if (App.cboDfltLotSerTrack.displayTplData[0].Code == "L" || App.cboDfltLotSerTrack.displayTplData[0].Code == "S") {
            App.tabLotSerial.tab.enable(true);
            App.tabLotSerial.enable(true);
            prefixvalue = App.txtDfltLotSerFxdVal.getValue();
            lastfixvalue = App.txtDfltLotSerNumVal.getValue();
            shownextlotserial = prefixvalue + lastfixvalue;
            App.lblShowNextLotSerial.setText(shownextlotserial);
            App.lblShowNextLotSerial.show();
        } else {
            App.tabLotSerial.tab.disable(true);
            App.tabLotSerial.disable(true);
            App.storeGrid.reload();

        }
    }
}

var chkPublic_Change = function (item) {
    
    if (App.chkPublic.getValue() == true) {
        App.tabCompany.tab.disable(true);
        App.tabCompany.disable(true);
        if (App.tabAtIN20200.activeTab.id == "tabCompany") {
            App.tabAtIN20200.setActiveTab(App.tabInfo);
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
    prefixvalue = App.txtDfltLotSerFxdVal.getValue();
    lastfixvalue = App.txtDfltLotSerNumVal.getValue();
    shownextlotserial = prefixvalue + lastfixvalue;
    App.lblShowNextLotSerial.setText(shownextlotserial);
    App.lblShowNextLotSerial.show();
    
}
