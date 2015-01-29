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

function duplicated(store, row) {
    var found = false;
    var strkey = row.record.idProperty.split(',');
    for (var i = 0; i < store.allData.items.length; i++) {
        var record = store.allData.items[i];
        var data = '';
        var rowdata = '';
        for (var jkey = 0; jkey < strkey.length; jkey++) {
            data = record.data[strkey[jkey]].toString().toLowerCase() + ',';
            if (row.field == strkey[jkey])
                rowdata = row.value.toString().toLowerCase() + ',';
            else
                rowdata = row.record.data[strkey[jkey]].toString().toLowerCase() + ',';
        }
        if (found = (data == rowdata && record.id != row.record.id) ? true : false) {

            break;
        };
    }
    return found;
};

function processMessage(errorMsg, result) {
    try {
        var data = "";
        if (result.failureType == 'connect') {
            Ext.Msg.alert('Error', "Can't connect to server! Please try again.");
        }
        else {
            if (data.responseText != undefined)
                data = Ext.decode(result.responseText);
            else
                data = Ext.decode(result.response.responseText);

            if (!Ext.isEmpty(data)) {
                if (data.type == 'message') {
                    callMessage(data.code, data.parm, data.fn);
                }
                else if (data.type == "error") {
                    Ext.Msg.alert('Error', data.errorMsg);
                }
            }
        }
    }
    catch (e) {
        Ext.Msg.alert('Error', errorMsg);
    }

}