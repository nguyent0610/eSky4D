var menuClick = function (command) {
    switch (command) {
        case "first":       
            App.SelectionModelAR_Area.select(0);
            break;
        case "prev":
            App.SelectionModelAR_Area.selectPrevious();
            break;
        case "next":
            App.SelectionModelAR_Area.selectNext();
            break;
        case "last":
            App.SelectionModelAR_Area.select(App.Store1.getCount() - 1);
            break;
        case "refresh":
            App.grd.getStore().reload();
            App.SelectionModelAR_Area.select(0);
            break;
        case "new":
            if(isInsert){
                var createdItems=App.Store1.getChangedData().Created;
                if(createdItems!=undefined){
                    App.Store1.loadPage(Math.ceil(App.Store1.totalCount/App.Store1.pageSize), {
                        callback : function () {
                            App.SelectionModelAR_Area.select(App.Store1.getCount()-1);                   
                            App.grd.editingPlugin.startEditByPosition({row: App.Store1.getCount()-1, column: 1});                  
                               
                        }
                    });   
                    return;
                }
                App.Store1.loadPage(Math.ceil(App.Store1.totalCount/App.Store1.pageSize), {
                    callback : function () {
                        App.Store1.insert(App.Store1.getCount(), Ext.data.Record());//Ext.data.Record()
                        App.SelectionModelAR_Area.select(App.Store1.getCount()-1);                   
                        App.grd.editingPlugin.startEditByPosition({row: App.Store1.getCount()-1, column: 1});
                    }
                });      
                    
                    
                  
            }
            break;
        case "delete":

            if(isDelete){
                callMessage(11, '', 'deleteRecord');
            }
            break;
        case "save":
            if(isUpdate||isInsert||isDelete){
                if(isAllValid(App.Store1.getChangedData().Created)
                    && isAllValid(App.Store1.getChangedData().Updated)){
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
            url: 'AR21300/Save',
            params: {
                lstgrd: Ext.encode(App.Store1.getChangedData({ skipIdForPhantomRecords: false }))
            },
            success: function (f, result) {
                menuClick("refresh");
                callMessage(201405071,'',null);                    
                   
            }
            ,failure: function (f, errorMsg) {
                callMessage(19, '', null);
            }
        });
    }
}
// Xem lai
function Close() {
    if (App.Store1.getChangedData().Updated == undefined  && App.Store1.getChangedData().Deleted == undefined )
        parent.App.tabAR21300.close();
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
        if (parent.App.tabAR21300 != null)
            parent.App.tabAR21300.close();
    }
};
// Xac nhan xoa record tren grid
var deleteRecord = function (item){
    if(item == "yes"){
        App.grd.deleteSelected();

    }
};
//check value
var isAllValid=function(items){
    if(items!=undefined){
        for(var i=0; i<items.length; i++){
            if(items[i]["Area"]==undefined) continue;
            if(items[i]["Area"].trim()=="")
            {
                callMessage(15,'@Util.GetLang("Area")',null);
                return false;
            }
            if(items[i]["Descr"].trim()=="")
            {
                callMessage(15,'@Util.GetLang("Descr")',null);
                return false;
            }               
                
        }
        return true;
    }else{
        return true;
    }
};
//check value
var isAllValidKey=function(items){      
    if(items!=undefined){
        for(var i=0; i<items.length; i++){
            for(var j=0;j<strKeyGrid.length;j++)
            {
                if(items[i][strKeyGrid[j]]==''||items[i][strKeyGrid[j]]==undefined)
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
    if(!isUpdate) return false;
    strKeyGrid = e.record.idProperty.split(',');
    if (strKeyGrid.indexOf(e.field) != -1) {
        if (e.record.data.tstamp!="")
            return false;
    }
       
};
var grd_Edit = function (item, e) {
        
    if (strKeyGrid.indexOf(e.field) != -1) {
        if(e.value!='' &&  isAllValidKey(App.Store1.getChangedData().Created)&&  isAllValidKey(App.Store1.getChangedData().Updated))           
            App.Store1.insert(App.Store1.getCount(), Ext.data.Record());//Ext.data.Record() 
    }       
};
var grd_ValidateEdit = function (item, e) {
       
    if (strKeyGrid.indexOf(e.field) != -1)
    {
        if(duplicated(App.Store1,e))
        {
            callMessage(1112, e.value, '');
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
