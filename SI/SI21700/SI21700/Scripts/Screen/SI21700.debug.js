//// Declare //////////////////////////////////////////////////////////

var keys = ['Country','State','District'];
///////////////////////////////////////////////////////////////////////

//// Store /////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////


//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdDistrict);
            break;
        case "prev":
            HQ.grid.prev(App.grdDistrict);
            break;
        case "next":
            HQ.grid.next(App.grdDistrict);
            break;
        case "last":
            HQ.grid.last(App.grdDistrict);
            break;
        case "refresh":
            App.stoDistrict.reload();
            HQ.grid.first(App.grdDistrict);
            break;
        case "new":
            if (HQ.isInsert) {
                HQ.grid.insert(App.grdDistrict);
            }
            break;
        case "delete":
            if (App.slmDistrict.selected.items[0] != undefined) {
                if (HQ.isDelete) {
                    HQ.message.show(11, '', 'deleteData');
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (checkRequire(App.stoDistrict.getChangedData().Created) && checkRequire(App.stoDistrict.getChangedData().Updated)) {
                    save();
                }
            }
            break;
        case "print":
            break;
        case "close":
            if (HQ.store.isChange(App.stoDistrict)) {
                HQ.message.show(5, '', 'askClose');
            } else {
                HQ.common.close(this);
            }
            break;
    }

};
var grdDistrict_BeforeEdit = function (editor, e) {
    if (!HQ.isUpdate) return false;
    //keys = e.record.idProperty.split(',');

    if (keys.indexOf(e.field) != -1) {
        //if (e.record.data.tstamp != "")
        //    return false;
    }
    //return HQ.grid.checkInput(e, keys);

    if (e.field == "State") {
        App.cboState.getStore().reload();
    }


};


var cboCountry_Change = function () {
    App.cboState.getStore().reload();
};


var grdDistrict_Edit = function (item, e) {

    if (keys.indexOf(e.field) != -1) {
        if (e.value != '' && isAllValidKey(App.stoDistrict.getChangedData().Created) && isAllValidKey(App.stoDistrict.getChangedData().Updated))
            HQ.store.insertBlank(App.stoDistrict);
    }

    //if (e.field == "Country") {
    //    App.cboState.store.readParameters = function (operation) {
    //        return {
    //            apply: {
    //                "param0": e.value,
    //                "procName": "SI21700_pcStateAll",
    //                "procParam": "@Country,",
    //                "sys": "False"
    //            }
    //        };
    //    }
    //    App.cboState.store.reload();
    //}
};
var grdDistrict_ValidateEdit = function (item, e) {
    if (keys.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(App.grdDistrict, e, keys)) {
            HQ.message.show(1112, e.value);
            return false;
        }
        var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/

        if (!e.value.match(regex)) {
            HQ.message.show(20140811, e.column.text);
            return false;
        }        
    }
};

var grdDistrict_CancelEdit = function (editor, e) {
    if (e.record.phantom) {
        e.store.remove(e.record);
    }
};

var grd_Reject = function (record) {
    if (record.data.tstamp == '') {
        App.stoDistrict.remove(record);
        App.grdDistrict.getView().focusRow(App.stoDistrict.getCount() - 1);
        App.grdDistrict.getSelectionModel().select(App.stoDistrict.getCount() - 1);
    } else {
        record.reject();
    }
};

/////////////////////////////////////////////////////////////////////////



//// Process Data ///////////////////////////////////////////////////////

var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            url: 'SI21700/Save',
            params: {
                lstDistrict: HQ.store.getData(App.stoDistrict)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                menuClick("refresh");
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var deleteData = function (item) {
    if (item == "yes") {
        App.grdDistrict.deleteSelected();
    }
};
//kiem tra key da nhap du chua
var isAllValidKey = function (items) {
    if (items != undefined) {
        for (var i = 0; i < items.length; i++) {
            for (var j = 0; j < keys.length; j++) {
                if (items[i][keys[j]] == '' || items[i][keys[j]] == undefined)
                    return false;
            }
        }
        return true;
    } else {
        return true;
    }
};
//kiem tra nhung field yeu cau bat buoc nhap
var checkRequire = function (items) {
    if (items != undefined) {
        for (var i = 0; i < items.length; i++) {
            if (HQ.grid.checkRequirePass(items[i], keys)) continue;
            if (items[i]["Country"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("Country"));
                return false;
            }
          
            if (items[i]["State"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("State"));
                return false;
            }
            if (items[i]["District"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("District"));
                return false;
            }
            if (items[i]["Name"].trim() == "") {
                HQ.message.show(15, HQ.common.getLang("Name"));
                return false;
            }
        }
        return true;
    } else {
        return true;
    }
};
/////////////////////////////////////////////////////////////////////////



//// Other Functions ////////////////////////////////////////////////////

var askClose = function (item) {
    if (item == "no" || item == "ok") {
        HQ.common.close(this);
    }
};


/////////////////////////////////////////////////////////////////////////








