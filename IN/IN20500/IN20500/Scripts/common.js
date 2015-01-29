
function callMessage(code, parm, fn) {
    App.direct.CallMessage(code, Ext.encode(parm), fn, {
        success: function (result) { }
    });
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
function dateToString(date, format) {
    if (date == null) return '';
    if (format == 'm/d/y') {
        return (date.getMonth() + 1).toString() + '/' + date.getDate().toString() + '/' + date.getFullYear().toString();
    }
    return ''

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
//TrungHT 
Ext.define("ThousandSeparatorNumberField", {
    override: "Ext.form.field.Number",

    /**
    * @cfg {Boolean} useThousandSeparator
    */
    useThousandSeparator: true,
    //decimalPrecision: decimalPrecision == null ? 0 : decimalPrecision,
    style: 'text-align: right',
    fieldStyle: "text-align:right;",
    /**
     * @inheritdoc
     */
    toRawNumber: function (value) {
        return String(value).replace(this.decimalSeparator, '.').replace(new RegExp(Ext.util.Format.thousandSeparator, "g"), '');
    },

    /**
     * @inheritdoc
     */
    getErrors: function (value) {
        if (!this.useThousandSeparator)
            return this.callParent(arguments);
        var me = this,
            errors = Ext.form.field.Text.prototype.getErrors.apply(me, arguments),
            format = Ext.String.format,
            num;

        value = Ext.isDefined(value) ? value : this.processRawValue(this.getRawValue());

        if (value.length < 1) { // if it's blank and textfield didn't flag it then it's valid
            return errors;
        }

        value = me.toRawNumber(value);

        if (isNaN(value.replace(Ext.util.Format.thousandSeparator, ''))) {
            errors.push(format(me.nanText, value));
        }

        num = me.parseValue(value);

        if (me.minValue === 0 && num < 0) {
            errors.push(this.negativeText);
        }
        else if (num < me.minValue) {
            errors.push(format(me.minText, me.minValue));
        }

        if (num > me.maxValue) {
            errors.push(format(me.maxText, me.maxValue));
        }

        return errors;
    },

    /**
     * @inheritdoc
     */
    valueToRaw: function (value) {
        if (!this.useThousandSeparator)
            return this.callParent(arguments);
        var me = this;

        var format = "000,000";
        for (var i = 0; i < me.decimalPrecision; i++) {
            if (i == 0)
                format += ".";
            format += "0";
        }
        value = me.parseValue(Ext.util.Format.number(value, format));
        value = me.fixPrecision(value);
        value = Ext.isNumber(value) ? value : parseFloat(me.toRawNumber(value));
        value = isNaN(value) ? '' : String(Ext.util.Format.number(value, format)).replace('.', me.decimalSeparator);
        return value;
    },

    /**
     * @inheritdoc
     */
    getSubmitValue: function () {
        if (!this.useThousandSeparator)
            return this.callParent(arguments);
        var me = this,
            value = me.callParent();

        if (!me.submitLocaleSeparator) {
            value = me.toRawNumber(value);
        }
        return value;
    },

    /**
     * @inheritdoc
     */
    setMinValue: function (value) {
        if (!this.useThousandSeparator)
            return this.callParent(arguments);
        var me = this,
            allowed;

        me.minValue = Ext.Number.from(value, Number.NEGATIVE_INFINITY);
        me.toggleSpinners();

        // Build regexes for masking and stripping based on the configured options
        if (me.disableKeyFilter !== true) {
            allowed = me.baseChars + '';

            if (me.allowExponential) {
                allowed += me.decimalSeparator + 'e+-';
            }
            else {
                allowed += Ext.util.Format.thousandSeparator;
                if (me.allowDecimals) {
                    allowed += me.decimalSeparator;
                }
                if (me.minValue < 0) {
                    allowed += '-';
                }
            }

            allowed = Ext.String.escapeRegex(allowed);
            me.maskRe = new RegExp('[' + allowed + ']');
            if (me.autoStripChars) {
                me.stripCharsRe = new RegExp('[^' + allowed + ']', 'gi');
            }
        }
    },

    /**
     * @private
     */
    parseValue: function (value) {
        if (!this.useThousandSeparator)
            return this.callParent(arguments);
        value = parseFloat(this.toRawNumber(value));
        return isNaN(value) ? null : value;
    }
});

