var strKeyGridGrid='';

   
var menuClick = function (command) {
    switch (command) {
        case "first":       
            App.SelectionModelSYS_ScreenCat.select(0);
            break;
        case "prev":
            App.SelectionModelSYS_ScreenCat.selectPrevious();
            break;
        case "next":
            App.SelectionModelSYS_ScreenCat.selectNext();
            break;
        case "last":
            App.SelectionModelSYS_ScreenCat.select(App.Store1.getCount() - 1);
            break;
        case "refresh":
            App.grd.getStore().reload();
            App.SelectionModelSYS_ScreenCat.select(0);
            break;
        case "new":
            if(isInsert){
                
                
                App.SelectionModelSYS_ScreenCat.select(App.Store1.getCount() - 1);

                if (App.Store1.getCount() == 0) {

                    App.Store1.insert(App.Store1.getCount(), Ext.data.Record());
                    App.SelectionModelSYS_ScreenCat.select(App.Store1.getCount() - 1);
                    App.grd.editingPlugin.startEditByPosition({ row: App.Store1.getCount() - 1, column: 1 });

                } else if (App.SelectionModelSYS_ScreenCat.selected.items[0].data.CarrierID != "") {
                    App.Store1.insert(App.Store1.getCount(), Ext.data.Record());
                    App.SelectionModelSYS_ScreenCat.select(App.Store1.getCount() - 1);
                    App.grd.editingPlugin.startEditByPosition({ row: App.Store1.getCount() - 1, column: 1 });
                }
                    
                    
                  
            }
            break;
        case "delete":

            if (App.SelectionModelSYS_ScreenCat.selected.items[0] != undefined) {
                if (isDelete) {
                    callMessage(11, '', 'deleteRecord');
                }
            }
            break;
        case "save":
            if(isUpdate||isInsert||isDelete){
                if(isAllValid(App.Store1.getChangedData().Created)
                    && isAllValid(App.Store1.getChangedData().Updated))
                {
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
    if (App.pnlScreen.isValid()) {
        App.pnlScreen.submit({
            waitMsg: 'Submiting...',
            url: 'SI21300/Save',
            params: {
                lstgrd: Ext.encode(App.Store1.getChangedData({ skipIdForPhantomRecords: false }))
            },
            success: function (f, result) {
                menuClick("refresh");
                callMessage(201405071,'',null);                    
                   
            }
            ,failure: function (f, errorMsg) {
                //   callMessage(201405072,errorMsg.result.errorMsg,null);
               // var dt = Ext.decode(data.response.responseText)
                callMessage(19, '', null);
            }
        });
    }
}

// Xem lai
function Close() {
    if (App.Store1.getChangedData().Updated == undefined  && App.Store1.getChangedData().Deleted == undefined )
        parent.App.tabSI21300.close();
    else if (App.Store1.getChangedData().Updated != undefined || App.Store1.getChangedData().Created != undefined || App.Store1.getChangedData().Deleted != undefined)
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
        if (parent.App.tabSI21300 != null)
            parent.App.tabSI21300.close();
    }
};
// Xac nhan xoa record tren grid
var deleteRecord = function (item){
    if(item == "yes"){
        App.grd.deleteSelected();

    }
};

//check value
var isAllValidKey=function(items){      
    if(items!=undefined){
        for(var i=0; i<items.length; i++){
            for(var j=0;j<strKeyGrid.length;j++)
            {
                if (items[i][strKeyGrid[j]] == '' || items[i][strKeyGrid[j]] == undefined)
                {
                    
                    return false;
                }
                
                   
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
        if (e.record.data.tstamp != "" || e.record.data.CarrierID.trim() == "")
            if (e.record.data.tstamp != "") {
                return false;
            } else {
                return true;
            }
    }
    if (e.record.data.CarrierID.trim() == "") {
        return false
    }
       
};
var grd_Edit = function (item, e) {
        
    if (strKeyGrid.indexOf(e.field) != -1) {
        if(e.value!='' &&  isAllValidKey(App.Store1.getChangedData().Created) &&  isAllValidKey(App.Store1.getChangedData().Updated))           
            App.Store1.insert(App.Store1.getCount(), Ext.data.Record());//Ext.data.Record() 
    }       
};
var grd_ValidateEdit = function (item, e) {
       
    if (strKeyGrid.indexOf(e.field) != -1)
    {
        if(duplicated(App.Store1,e))
        {
            callMessage(1112, e.value, '');
            //e.store.remove(e.record);
            //menuClick("refresh");
            return false;

        }
        var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/
        if (e.value.match(regex)) {
            return true;

        } else {
            callMessage(20140811, e.column.text, '');
            return false;
        }
    }
};
    
var grd_CancelEdit = function (editor, e){
    if(e.record.phantom){
        e.store.remove(e.record);
    }
};
var grd_Reject= function (record){
    if(record.data.tstamp=='')
    {         
        App.Store1.remove(record);
        App.grd.getView().focusRow(App.Store1.getCount()-1);
        App.grd.getSelectionModel().select(App.Store1.getCount()-1);
    } else record.reject();
};
    
//Phan trang tren grid
var onComboBoxSelect = function (combo) {
    var store = combo.up("gridpanel").getStore();
    store.pageSize = parseInt(combo.getValue(), 10);
    store.reload();
};
   
var loadDefault = function () {

    App.SelectionModelSYS_ScreenCat.select(App.Store1.getAt(0));


};