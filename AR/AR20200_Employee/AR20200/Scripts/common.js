function callMessage(code, parm, fn, array) {
    if (array == true) {
        App.direct.CallMessage(code, Ext.encode(parm), fn, true, {
            success: function (result) { }
        });
    } else {
        App.direct.CallMessage(code, Ext.encode(parm), fn, false, {
            success: function (result) { }
        });
    }
};
function gridInsertBlank(store, key) {
    var flat = store.findBy(function (record, id) {
        if (!record.get(key)) {
            return true;
        }
        return false;
    });

    if (flat == -1) {
        store.insert(store.getCount(), Ext.data.Record());
    }
}
function gridInsertRecord(store, key, newRecord) {
    var flat = store.findBy(function (record, id) {
        if (!record.get(key)) {
            return true;
        }
        return false;
    });

    if (flat == -1) {
        store.insert(store.getCount(), newRecord);
    }
}
function lockItem(ctr, lock) {
  
    if (typeof (ctr.items) != "undefined") {
        ctr.items.each(function (itm) {
            if (typeof (itm.setReadOnly) != "undefined") {
                itm.setReadOnly(lock)

            }
            lockItem(itm, lock);
        });
    }
}
function processMessage(errorMsg, obj,array) {
   
    try
    {
        if (array == null) array = false;
        if (obj.result != undefined) {
               
            if (obj.result.type == 'message') {
                callMessage(obj.result.code, obj.result.parm, obj.result.fn, array);
            }
            else if (obj.result.type == "error") {
                Ext.Msg.alert('Error', obj.result.errorMsg);
            }   
        } else if (obj.responseText != undefined) {
            var data = Ext.decode(obj.responseText);
            if (data.type == 'message') {
                callMessage(data.code, data.parm, data.fn, array);
            }
            else if (data.type == "error") {
                Ext.Msg.alert('Error', obj.errorMsg);
            }
            else {
                Ext.Msg.alert('Error', data);
            }
          
        } else if (obj.response.responseText != undefined) {
            Ext.Msg.alert('Error', obj.response.responseText);
        } else {
            if (obj.failureType != undefined) {
                Ext.Msg.alert('Error', "Can't connect to server! Please try again.");
            }
        }
    }
    catch(e)
    {
        Ext.Msg.alert('Error', errorMsg);
    }
        
}
function strToBool(str) {
    if (str.toLowerCase() == 'false') {
        return false;
    } else if (str.toLowerCase() == 'true') {
        return true;
    } else {
        return undefined;
    }
};
function hexToRgb(hex) {
    // Expand shorthand form (e.g. "03F") to full form (e.g. "0033FF")
    var shorthandRegex = /^#?([a-f\d])([a-f\d])([a-f\d])$/i;
    hex = hex.replace(shorthandRegex, function (m, r, g, b) {
        return r + r + g + g + b + b;
    });

    var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result ? {
        r: parseInt(result[1], 16),
        g: parseInt(result[2], 16),
        b: parseInt(result[3], 16)
    } : null;
};
function passNull(str) {
    if (str == null) {
        return "";
    } else return str;
};
function printDate(date) {
    if (date == null) return '';
    return date.getFullYear().toString() + '-' +
            (date.getMonth() + 1).toString() + '-' +
            date.getDate().toString();
};
function convertToDate(date) {
    var dt = date.split(/\s/)
    return new Date(dt[0] + ' ' + dt[1]);
}
function duplicated(store, row) {
    var found = false;
    var strkey = row.record.idProperty.split(',');
    if (store.allData == undefined) {
        for (var i = 0; i < store.data.items.length; i++) {
            var record = store.data.items[i];
            var data = '';
            var rowdata = '';
            for (var jkey = 0; jkey < strkey.length; jkey++) {
                if (record.data[strkey[jkey]] != undefined) {
                    data += record.data[strkey[jkey]].toString().toLowerCase() + ',';
                    if (row.field == strkey[jkey])
                        rowdata += (row.value == null ? "" : row.value.toString().toLowerCase()) + ',';
                    else
                        rowdata += row.record.data[strkey[jkey]].toString().toLowerCase() + ',';
                }
            }
            if (found = (data == rowdata && record.id != row.record.id) ? true : false) {

                break;
            };
        }
    } else {
        for (var i = 0; i < store.allData.items.length; i++) {
            var record = store.allData.items[i];
            var data = '';
            var rowdata = '';
            for (var jkey = 0; jkey < strkey.length; jkey++) {
                if (record.data[strkey[jkey]] != undefined) {
                    data += record.data[strkey[jkey]].toString().toLowerCase() + ',';
                    if (row.field == strkey[jkey])
                        rowdata += (row.value == null ? "" : row.value.toString().toLowerCase()) + ',';
                    else
                        rowdata += row.record.data[strkey[jkey]].toString().toLowerCase() + ',';
                }
            }
            if (found = (data == rowdata && record.id != row.record.id) ? true : false) {

                break;
            };
        }
    }
    return found;
};
function checkRequire(value,langField) {
    if (value.trim() == "") {
        callMessage(15,langField, null);
        return false;
    }
};
function loadDefault(fileNameStore, cbo) {
    if (fileNameStore.data.items.length > 0) {       
        cbo.setValue(fileNameStore.getAt(0).get(cbo.valueField));

    }
};
var cboFilter = function (item, newValue, oldValue,stkeyFilter) {

    var store = item.getStore();
    store.clearFilter();
    if (item.displayTplData == null) {

        store.on('datachanged', function () {
            console.log(store.getCount());
        });
        store.filterBy(function (record) {
             var isMap=false;
            stkeyFilter.split(',').forEach(function(key){
                var fieldData = record.data[key].toLowerCase().indexOf(newValue.toLowerCase());
                if (fieldData > -1)
			    {   isMap=true;    
                    return record;				
			    }
            });
			if(isMap==true) return record
        });
    }
};
function FilterCombo(control, stkeyFilter) {   
    var store = control.getStore();
    var value=control.getValue();
    store.clearFilter();
    if (control.valueModels==null||control.valueModels.length==0) {
        store.filterBy(function (record) {
            var isMap = false;
            stkeyFilter.split(',').forEach(function (key) {
                var fieldData = record.data[key].toLowerCase().indexOf(passNull(value).toLowerCase());
                if (fieldData > -1) {
                    isMap = true;
                    return record;
                }
            });
            if (isMap == true) return record
        });
    }  
};