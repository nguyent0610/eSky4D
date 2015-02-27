
var selectedIndex = 0;
var _hold = "H";
var _curSlsperid = "";
//biến tạm focusrecord để biết đang focus vào form hay Grid để xác định các xử lý các nút phía trên
var _focusrecord = 0;
var beforeedit = '';
//lưu ý cái biến tạm _recycleMailID dùng để tránh vòng lặp đệ quy khi ta set lại giá trị 
//cho cboMailID trong function cboMailID_Change vì nó sẽ lại chạy function cboMailID_Change
var _recycleMailID = 0;

var keys = ['ReportID', 'ReportViewID'];


var menuClick = function (command) {
    switch (command) {
        case "first":
            if (_focusrecord == 1) {

                var combobox = App.cboMailID;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);
                //dòng này để bắt các thay đổi của form 
                App.frm.getForm().updateRecord();
                //neu ko co thay doi trong form hoac Grid
                if (App.storeMailDetail.getChangedData().Updated == undefined && App.storeMailDetail.getChangedData().Deleted == undefined && App.storeMailHeader.getChangedData().Updated == undefined && App.storeMailHeader.getChangedData().Deleted == undefined)
                    App.cboMailID.setValue(combobox.store.getAt(0).data.MailID);
                    //neu co thay doi trong form hoac Grid
                else if (App.storeMailDetail.getChangedData().Updated != undefined || App.storeMailDetail.getChangedData().Created != undefined || App.storeMailDetail.getChangedData().Deleted != undefined || App.storeMailHeader.getChangedData().Updated != undefined || App.storeMailHeader.getChangedData().Created != undefined || App.storeMailHeader.getChangedData().Deleted != undefined) {
                    //hien thong bao du lieu da thay doi
                    HQ.message.show(150, '', '');

                }


            } else if (_focusrecord == 2) {
                HQ.grid.first(App.grd);
            }
            break;
        case "prev":
            if (_focusrecord == 1) {

                var combobox = App.cboMailID;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);

                //dòng này để bắt các thay đổi của form 
                App.frm.getForm().updateRecord();
                //neu ko co thay doi trong form hoac Grid
                if (App.storeMailDetail.getChangedData().Updated == undefined && App.storeMailDetail.getChangedData().Deleted == undefined && App.storeMailHeader.getChangedData().Updated == undefined && App.storeMailHeader.getChangedData().Deleted == undefined)
                    App.cboMailID.setValue(combobox.store.getAt(index - 1).data.MailID);
                    //neu co thay doi trong form hoac Grid
                else if (App.storeMailDetail.getChangedData().Updated != undefined || App.storeMailDetail.getChangedData().Created != undefined || App.storeMailDetail.getChangedData().Deleted != undefined || App.storeMailHeader.getChangedData().Updated != undefined || App.storeMailHeader.getChangedData().Created != undefined || App.storeMailHeader.getChangedData().Deleted != undefined) {
                    //hien thong bao du lieu da thay doi
                    HQ.message.show(150, '', '');

                }

            } else if (_focusrecord == 2) {
                HQ.grid.prev(App.grd);
            }
            break;
        case "next":
            if (_focusrecord == 1) {

                var combobox = App.cboMailID;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);

                //dòng này để bắt các thay đổi của form 
                App.frm.getForm().updateRecord();
                //neu ko co thay doi trong form hoac Grid
                if (App.storeMailDetail.getChangedData().Updated == undefined && App.storeMailDetail.getChangedData().Deleted == undefined && App.storeMailHeader.getChangedData().Updated == undefined && App.storeMailHeader.getChangedData().Deleted == undefined)
                    App.cboMailID.setValue(combobox.store.getAt(index + 1).data.MailID);
                    //neu co thay doi trong form hoac Grid
                else if (App.storeMailDetail.getChangedData().Updated != undefined || App.storeMailDetail.getChangedData().Created != undefined || App.storeMailDetail.getChangedData().Deleted != undefined || App.storeMailHeader.getChangedData().Updated != undefined || App.storeMailHeader.getChangedData().Created != undefined || App.storeMailHeader.getChangedData().Deleted != undefined) {
                    //hien thong bao du lieu da thay doi
                    HQ.message.show(150, '', '');

                }

            } else if (_focusrecord == 2) {
                HQ.grid.next(App.grd);
            }
            break;
        case "last":
            if (_focusrecord == 1) {

                var combobox = App.cboMailID;
                var v = combobox.getValue();
                var record = combobox.findRecord(combobox.valueField || combobox.displayField, v);
                var index = combobox.store.indexOf(record);

                //dòng này để bắt các thay đổi của form 
                App.frm.getForm().updateRecord();
                //neu ko co thay doi trong form hoac Grid
                if (App.storeMailDetail.getChangedData().Updated == undefined && App.storeMailDetail.getChangedData().Deleted == undefined && App.storeMailHeader.getChangedData().Updated == undefined && App.storeMailHeader.getChangedData().Deleted == undefined)
                    App.cboMailID.setValue(App.cboMailID.store.getAt(App.cboMailID.store.getTotalCount() - 1).data.MailID);
                    //neu co thay doi trong form hoac Grid
                else if (App.storeMailDetail.getChangedData().Updated != undefined || App.storeMailDetail.getChangedData().Created != undefined || App.storeMailDetail.getChangedData().Deleted != undefined || App.storeMailHeader.getChangedData().Updated != undefined || App.storeMailHeader.getChangedData().Created != undefined || App.storeMailHeader.getChangedData().Deleted != undefined) {
                    //hien thong bao du lieu da thay doi
                    HQ.message.show(150, '', '');

                }

            } else if (_focusrecord == 2) {
                HQ.grid.last(App.grd);
            }
            break;
        case "refresh":
            App.storeMailHeader.reload();
            App.storeMailDetail.reload();

            break;
        case "new":
            if (HQ.isInsert) {

                if (_focusrecord == 2) {

                    HQ.grid.insert(App.grd);



                } else if (_focusrecord == 1) {
                    //2 hàng này nhằm set giá trị rỗng và reload store cho store load rỗng ra để reset form và sẵn 
                    //tiện insert 1 record vào form ,do sau khi load store của form sẽ chạy function loadDataAutoHeader
                    App.cboMailID.setValue('');
                    App.storeMailHeader.reload();

                }

            }
            break;
        case "delete":
            var curRecord = App.frm.getRecord();
            if (HQ.isDelete) {
                if (_focusrecord == 1) {
                    HQ.message.show(11, '', 'deleteRecordForm');
                } else if (_focusrecord == 2) {
                    HQ.message.show(11, '', 'deleteRecordGrid')
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                //checkRequire để kiếm tra các field yêu cầu có rỗng hay ko
                if (checkRequire(App.storeMailDetail.getChangedData().Created)
                    && checkRequire(App.storeMailDetail.getChangedData().Updated)) {

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



    //dòng này để bắt các thay đổi của form 
    App.frm.getForm().updateRecord();

    if (App.frm.isValid()) {
        App.frm.submit({
            waitMsg: 'Submiting...',
            url: 'SA40300/Save',
            params: {
                lstheader: Ext.encode(App.storeMailHeader.getChangedData({ skipIdForPhantomRecords: false })),//,

                lstgrd: Ext.encode(App.storeMailDetail.getChangedData({ skipIdForPhantomRecords: false })),
                mailId: App.cboMailID.getValue()
            },
            success: function (result, data) {
                App.cboMailID.getStore().reload();
                menuClick("refresh");
                HQ.message.show(201405071, '', null);

                var dt = Ext.decode(data.response.responseText);
                App.cboMailID.setValue(dt.value);

                App.storeMailDetail.reload();
            }
            , failure: function (errorMsg, data) {

                var dt = Ext.decode(data.response.responseText);
                HQ.message.show(dt.code, dt.colName + ',' + dt.value, null);
            }
        });
    }
}


// Xem lai
function Close() {
    if (App.storeMailDetail.getChangedData().Updated == undefined && App.storeMailDetail.getChangedData().Deleted == undefined && App.storeMailHeader.getChangedData().Updated == undefined && App.storeMailHeader.getChangedData().Deleted == undefined)
        parent.App.tabSA40300.close();
    else if (App.storeMailDetail.getChangedData().Updated != undefined || App.storeMailDetail.getChangedData().Created != undefined || App.storeMailDetail.getChangedData().Deleted != undefined || App.storeMailHeader.getChangedData().Updated != undefined || App.storeMailHeader.getChangedData().Created != undefined || App.storeMailHeader.getChangedData().Deleted != undefined) {
        App.direct.AskClose({
            success: function (result) {

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
        if (parent.App.tabSA40300 != null)
            parent.App.tabSA40300.close();
    }
};
//

// Xac nhan xoa record tren grid
var deleteRecordForm = function (item) {
    if (item == "yes") {

        try {
            App.direct.SA40300Delete(App.cboMailID.getValue(), {
                success: function (data) {
                    menuClick('refresh');
                    App.cboMailID.getStore().reload();
                    App.cboMailID.setValue('');
                    App.storeMailHeader.reload();

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




function selectRecord(grid, record) {
    var record = grid.store.getById(id);
    grid.store.loadPage(grid.store.findPage(record), {
        callback: function () {
            grid.getSelectionModel().select(record);
        }
    });
};

//hàm kiểm tra trước khi Edit 1 ô nào đó coi có thỏa điều kiện hay ko
//thường là các ô key phải được điền hết mới được điền các ô ko phải key
var grd_BeforeEdit = function (editor, e) {

    if (!HQ.isUpdate)
        return false;
    //keys = e.record.idProperty.split(',');

    if (keys.indexOf(e.field) != -1) {
        if (e.record.data.tstamp != "")
            return false;
    }
    return HQ.grid.checkInput(e, keys);

};
var grd_Edit = function (item, e) {
    //check nếu các ô key đã được điền thì tự động tạo thêm dòng trống phía dưới để tạo mới cho lẹ
    if (keys.indexOf(e.field) != -1) {
        if (e.value != '' && isAllValidKey(App.storeMailDetail.getChangedData().Created) && isAllValidKey(App.storeMailDetail.getChangedData().Updated))
            App.storeMailDetail.insert(App.storeMailDetail.getCount(), Ext.data.Record());//Ext.data.Record() 
    }

};
//Sau khi điền vào ô nào trong Grid xong sẽ chạy function này để check trùng key hay 
//ko và key có theo quy định mình đặt ra ko
var grd_ValidateEdit = function (item, e) {

    if (keys.indexOf(e.field) != -1) {
        if (HQ.grid.checkDuplicate(App.grd, e, keys)) {
            HQ.message.show(1112, e.value, '');
            return false;
        }
    }
    ////Regex quy định ID chỉ gồm các ký tự chữ và số và _ ko bao gồm các ký tự khác
    //var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/

    //if (!e.value.match(regex)) {
    //    HQ.message.show(20140811, e.column.text);
    //    return false;
    //}
    return true;
};

//hàm này dùng để xóa dòng trống nếu ko điền gì vào 
var grd_CancelEdit = function (editor, e) {
    if (e.record.phantom) {
        if (App.cboReportID.getValue == "" && App.cboReportViewID.getValue() == "") {
            e.store.remove(e.record);
        }
    }
};
//Reject chạy khi ấn vào Icon Reject phía bên phải mỗi dòng của Grid
var grd_Reject = function (record) {
    if (record.data.tstamp == '') {
        App.storeMailDetail.remove(record);
        App.grd.getView().focusRow(App.storeMailDetail.getCount() - 1);
        App.grd.getSelectionModel().select(App.storeMailDetail.getCount() - 1);
    } else record.reject();
};

//Phan trang tren grid
var onComboBoxSelect = function (combo) {
    var store = App.storeMailDetail;// combo.up("gridpanel").getStore();
    store.pageSize = parseInt(combo.getValue(), 10);
    store.reload();

};
//hàm này chạy sau khi store của form trong Controller vừa chạy xong đã load dữ liệu vào store của form
var loadDataAutoHeader = function () {
    //nều chưa có record mặc định nào để thêm mới thì tạo 1 record vào store của Form
    if (App.storeMailHeader.getCount() == 0) {
        App.storeMailHeader.insert(0, Ext.data.Record());
    }
    //lấy record đầu tiên của store , nếu store đó có rồi thì load dữ liệu lên ko có thì reset
    //dữ liệu rỗng cho các ô khác trong form
    var record = App.storeMailHeader.getAt(0);
    if (record) {
        App.frm.getForm().loadRecord(record);
    }


    var values = record.get('MailTo').replace(new RegExp(',', 'g'), ';').split(';');
    App.cboMailTo.setValue(values);
    var values1 = record.get('MailCC').replace(new RegExp(',', 'g'), ';').split(';');
    App.cboMailCC.setValue(values1);
};





var cboMailID_Change = function (sender, newValue, oldValue) {

    if (App.frm.getForm()._record) {
        //dòng này để bắt các thay đổi của form 
        App.frm.getForm().updateRecord();
        //neu ko co thay doi trong form hoac Grid
        if (App.storeMailDetail.getChangedData().Updated == undefined &&
            App.storeMailDetail.getChangedData().Deleted == undefined &&
            App.storeMailHeader.getChangedData().Updated == undefined &&
            App.storeMailHeader.getChangedData().Deleted == undefined) {
            _recycleMailID = 0;
            App.storeMailHeader.reload();
            App.storeMailDetail.reload();

        } //neu co thay doi trong form hoac Grid
        else if ((App.storeMailDetail.getChangedData().Updated != undefined ||
            App.storeMailDetail.getChangedData().Created != undefined ||
            App.storeMailDetail.getChangedData().Deleted != undefined ||
            App.storeMailHeader.getChangedData().Updated != undefined ||
            App.storeMailHeader.getChangedData().Created != undefined ||
            App.storeMailHeader.getChangedData().Deleted != undefined) && _recycleMailID == 0) {
            //hien thong bao du lieu da thay doi
            //lưu ý cái biến tạm _recycleMailID dùng để tránh vòng lặp đệ quy khi ta set lại giá trị 
            //cho cboMailID vì nó sẽ lại chạy function cboMailID_Change
            _recycleMailID = 1;
            HQ.message.show(150, '', '');
            App.cboMailID.setValue(oldValue);

        } else {
            return false;
        }
    } else {
        _recycleMailID = 0;
        App.storeMailHeader.reload();
        App.storeMailDetail.reload();
    }

};

var chkIsAttachFile_Change = function (sender, e) {
    _recycleMailID = 0;
    if (e) {
        App.chkIsDeleteFile.setValue(true);
        App.chkIsDeleteFile.disable();
    }
    else {
        App.chkIsDeleteFile.enable();
    }


}
//2 hàm focus nhằm bắt xem mình đang click vào form hay grid
var Focus2_Change = function (sender, e) {
    _focusrecord = 2;
}

var Focus1_Change = function (sender, e) {
    _focusrecord = 1;
}

//hàm kiểm tra các ô mình quy định ko được null của Grid , nếu của form thì xài AllowBlank(false) , ko xài AllowBlank cho Grid
var checkRequire = function (items) {
    if (items != undefined) {
        for (var i = 0; i < items.length; i++) {
            if (HQ.grid.checkRequirePass(items[i], keys)) continue;

            if (items[i]["ReportID"].trim() == "") {
                HQ.message.show(15, '@Util.GetLang("ReportID")', null);
                return false;
            }

            if (items[i]["ReportViewID"].trim() == "") {
                HQ.message.show(15, '@Util.GetLang("ReportViewID")', null);
                return false;
            }
            if (isNaN(items[i]["LangID"])) {
                HQ.message.show(15, '@Util.GetLang("LangID")', null);
                return false;
            }
            if (isNaN(items[i]["BeforeDateParm00"])) {
                HQ.message.show(15, '@Util.GetLang("BeforeDateParm00")', null);
                return false;
            }
            if (isNaN(items[i]["BeforeDateParm01"])) {
                HQ.message.show(15, '@Util.GetLang("BeforeDateParm01")', null);
                return false;
            }
            if (isNaN(items[i]["BeforeDateParm02"])) {
                HQ.message.show(15, '@Util.GetLang("BeforeDateParm02")', null);
                return false;
            }
            if (isNaN(items[i]["BeforeDateParm03"])) {
                HQ.message.show(15, '@Util.GetLang("BeforeDateParm03")', null);
                return false;
            }

        }
        return true;
    }
    else {
        return true;
    }
};


var cboReportID_Change = function (sender, e) {
    App.slmGrid.selected.items[0].set('ReportViewID', '');

    App.cboReportViewID.getStore().load();

}

var _recycleMailID_Change = function () {
    _recycleMailID = 0;
    if (App.frm.getForm()._record) {
        //dòng này để bắt các thay đổi của form 
        App.frm.getForm().updateRecord();
        //neu ko co thay doi trong form hoac Grid
        HQ.isChange = HQ.store.isChange(App.storeMailHeader);
        HQ.common.changeData(HQ.isChange, 'SA40300');
    }
}


