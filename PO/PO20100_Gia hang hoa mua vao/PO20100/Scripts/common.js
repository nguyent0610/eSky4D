function callMessage(code, parm, fn) {
    App.direct.CallMessage(code, parm, fn, { success: function (result) { }
    });
};
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

function printDate(date) {
    if (date == null) return '';
    return date.getFullYear().toString() + '-' +
            (date.getMonth() + 1).toString() + '-' +
            date.getDate().toString();
};

function FilterCombo(control, stkeyFilter) {   
    var store = control.getStore();
    var value=control.getValue();
    store.clearFilter();
    if (control.valueModels==null) {

        store.on('datachanged', function () {
            console.log(store.getCount());
        });
        store.filterBy(function (record) {
            var isMap = false;
            stkeyFilter.split(',').forEach(function (key) {
                var fieldData = record.data[key].toLowerCase().indexOf(value.toLowerCase());
                if (fieldData > -1) {
                    isMap = true;
                    return record;
                }
            });
            if (isMap == true) return record
        });
    }

    //var peoplefilter = new Ext.util.Filter({
    //    filterFn: function (item) {
    //        return item.data.Descr.toLowerCase().indexOf(value) > -1 ? true : false
    //    }
    //});
  
    //store.filter(peoplefilter);
};
//var cboFilter = function (item, newValue, oldValue, stkeyFilter) {

//    var store = item.getStore();
//    store.clearFilter();
//    if (item.displayTplData == null) {

//        store.on('datachanged', function () {
//            console.log(store.getCount());
//        });
//        store.filterBy(function (record) {
//            var isMap = false;
//            stkeyFilter.split(',').forEach(function (key) {
//                var fieldData = record.data[key].toLowerCase().indexOf(newValue.toLowerCase());
//                if (fieldData > -1) {
//                    isMap = true;
//                    return record;
//                }
//            });
//            if (isMap == true) return record
//        });
//    }
//};