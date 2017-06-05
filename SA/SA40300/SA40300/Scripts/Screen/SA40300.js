////Declare//////////////////////////////////////////////////////////
var focusrecord = 0;//biến tạm focusrecord để biết đang focus vào form hay Grid để xác định các xử lý các nút phía trên
//khai bao dung cho kiem tra tren grid, co nhieu luoi thi dat ten theo luoi
var keys = ['ReportID', 'ReportViewID'];
var fieldsCheckRequire = ["ReportID", "ReportViewID", "LangID", "BeforeDateParm00", "BeforeDateParm01", "BeforeDateParm02", "BeforeDateParm03"];
var fieldsLangCheckRequire = ["ReportID", "ReportViewID", "LangID", "BeforeDateParm00", "BeforeDateParm01", "BeforeDateParm02", "BeforeDateParm03"];

///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
// Load and show binding data to the form
// Command of the topbar on screen
var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == 'header') {
                HQ.combo.first(App.cboMailID, HQ.isChange);
            } else if (HQ.focus == 'grdMailDetail') {
                HQ.grid.first(App.grdMailDetail);
            }
            break;
        case "next":
            if (HQ.focus == 'header') {
                HQ.combo.next(App.cboMailID, HQ.isChange);
            } else if (HQ.focus == 'grdMailDetail') {
                HQ.grid.next(App.grdMailDetail);
            }
            break;
        case "prev":
            if (HQ.focus == 'header') {
                HQ.combo.prev(App.cboMailID, HQ.isChange);
            } else if (HQ.focus == 'grdMailDetail') {
                HQ.grid.prev(App.grdMailDetail);
            }
            break;
        
        case "last":
            if (HQ.focus == 'header') {
                HQ.combo.last(App.cboMailID, HQ.isChange);
            } else if (HQ.focus == 'grdMailDetail') {
                HQ.grid.last(App.grdMailDetail);
            }
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                App.cboMailID.getStore().load(function () { App.stoMailHeader.reload(); });
                
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.focus == 'header') {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', '');
                    }
                    else {
                        HQ.isNew = true;
                        App.cboMailID.setValue('');
                        cboMailID_Change(App.cboMailID);
                    }
                } else if (HQ.focus == 'grdMailDetail') {
                    HQ.grid.insert(App.grdMailDetail,keys);
                }
            }
            break;
        case "delete":         
            if (HQ.isDelete) {
                if (HQ.focus == 'header') {
                    HQ.message.show(11, '', 'deleteRecordForm');
                } else if (HQ.focus == 'grdMailDetail') {
                    var rowindex = HQ.grid.indexSelect(App.grdMailDetail);
                    if(rowindex!='')
                    HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdMailDetail),''], 'deleteRecordGrid',true)
                }
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                //checkRequire để kiếm tra các field yêu cầu có rỗng hay ko
                if (HQ.form.checkRequirePass(App.frmMain) && HQ.store.checkRequirePass(App.stoMailDetail, keys, fieldsCheckRequire, fieldsLangCheckRequire)) {
                    var flat = null;
                    var checkExistStore = App.chkIsUseStore.getValue();
                    App.stoMailDetail.data.each(function (item) {
                        if (!Ext.isEmpty(item.data.ReportID) && !Ext.isEmpty(item.data.ReportViewID)) {

                            if (item.data.StoreName == '') {
                                if (checkExistStore) {
                                    var index = HQ.grid.findColumnIndex(App.grdMailDetail.columns, 'StoreName');
                                    HQ.message.show(1111, ['', App.grdMailDetail.columns[index].text], '', true);
                                    flat = item;
                                    return false;
                                }
                            } else {
                                if (!checkSpecialChar(item.data.StoreName)) {
                                    HQ.message.show(2017060501, [''], '', true);
                                    flat = item;
                                    return false;
                                }
                            }
                        }                        
                    });
                    if (!Ext.isEmpty(flat)) {
                        App.slmMailDetail.select(App.stoMailDetail.indexOf(flat));
                        return false;
                    }
                    save();
                }
            }
            break;
        case "print":           
            break;
        case "close":
            HQ.common.close(this);
            break;
    }

};

//load lần đầu khi mở
var firstLoad = function () {
    App.stoMailHeader.reload();
};
var frmChange = function () {
    App.frmMain.getForm().updateRecord();
    HQ.isChange = HQ.store.isChange(App.stoMailHeader) == false ? HQ.store.isChange(App.stoMailDetail) : true;
    HQ.common.changeData(HQ.isChange, 'SA40300');//co thay doi du lieu gan * tren tab title header
    //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
    App.cboMailID.setReadOnly(HQ.isChange);
};

//xu li su kiem tren luoi giong nhu luoi binh thuong
var grdMailDetail_BeforeEdit = function (editor, e) {
    return HQ.grid.checkBeforeEdit(e, keys);
};
var grdMailDetail_Edit = function (item, e) {
    HQ.grid.checkInsertKey(App.grdMailDetail, e, keys);
    frmChange();
};
var grdMailDetail_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdMailDetail, e, keys);
};
var grdMailDetail_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdMailDetail);
    frmChange();
};

//hàm này chạy sau khi store của form trong Controller vừa chạy xong đã load dữ liệu vào store của form
var loadDataAutoHeader = function (sto) {
    HQ.isFirstLoad = true;
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
    HQ.isNew = false;
    if (sto.data.length == 0) {
        HQ.store.insertBlank(sto, "MailID");
        record = sto.getAt(0);
        //gan du lieu mac dinh ban dau
        record.data.DateTime = HQ.bussinessDate;;
        record.data.Time = HQ.bussinessTime;
        
        record.data.TypeAuto = 'M';
        HQ.isNew = true;//record la new    
        HQ.common.setRequire(App.frmMain);  //to do cac o la require                       
        sto.commitChanges();
    }
    var record = sto.getAt(0);
    record.data.MailCC = record.data.MailCC.replace(new RegExp(';', 'g'), ',');//cat dua ve du lieu dau , de set su thay doi
    record.data.MailTo = record.data.MailTo.replace(new RegExp(';', 'g'), ',');//cat dua ve du lieu dau , de set su thay doi
    //sto.commitChanges();//commit cho record thanh updated muc dich de dung ham HQ.store.isChange
    //record = sto.getAt(0);
    App.frmMain.getForm().loadRecord(record);
    App.stoMailDetail.reload();
    if (Ext.isEmpty(App.cboMailID.getValue()))
        App.cboMailID.focus(true);//focus ma khi tao moi
};
var loadDataAutoDetail = function (sto) {
    if (HQ.isFirstLoad) {
        if (HQ.isInsert) {
            HQ.store.insertBlank(sto, keys);
        }
        HQ.isFirstLoad = false;
    }
    frmChange();
    HQ.common.showBusy(false);
};
var cboMailID_Change = function (sender, newValue, oldValue) {
    if ((!HQ.isNew || sender.valueModels != null) && !App.stoMailHeader.loading ) {
        App.stoMailHeader.reload();
    }
   
};
var cboMailID_Select = function (sender) {  
    if (sender.valueModels != null && !App.stoMailHeader.loading) {
        App.stoMailHeader.reload();
    }
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

var chkIsUseStore_Change = function (sender, e) {
    App.txtTemplateFile.allowBlank = App.chkIsUseStore.getValue();
    App.txtTemplateFile.validate();
}
var cboReportID_Change = function (sender, e) {
    App.slmMailDetail.selected.items[0].set('ReportViewID', '');
    App.cboReportViewID.getStore().load();
}

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
// Submit the changed data (created, updated) into server side
function save() {
    //dòng này để bắt các thay đổi của form 
    App.frmMain.getForm().updateRecord();
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            timeout: 1800000,
            waitMsg: 'Submiting...',
            url: 'SA40300/Save',
            params: {
                lstheader: Ext.encode(App.stoMailHeader.getChangedData({ skipIdForPhantomRecords: false })),//,
                lstgrd: Ext.encode(App.stoMailDetail.getChangedData({ skipIdForPhantomRecords: false })),
                isNew: HQ.isNew

            },
            success: function (result, data) {
                HQ.message.show(201405071, '', '');
                var mailId = data.result.mailId;
                App.cboMailID.getStore().load(function () {
                    App.cboMailID.setValue(mailId);
                    App.stoMailHeader.reload();
                });                                         
            }
            , failure: function (errorMsg, data) {
                if (data.result.msgCode) {
                    if (data.result.msgCode == 2000)//loi trung key ko the add
                        HQ.message.show(data.result.msgCode, [App.cboMailID.fieldLabel, App.cboMailID.getValue()], '', true);
                    else HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                }
                else {
                    HQ.message.process(msg, data, true);
                }
            }
        });
    }
}
// Xac nhan xoa record tren grid
var deleteRecordForm = function (item) {
    if (item == 'yes') {
        App.frmMain.submit({
            clientValidation: false,
            timeout: 1800000,
            waitMsg: HQ.common.getLang('DeletingData'),
            url: 'SA40300/Delete',
            params: {
                mailId: App.cboMailID.getValue()
            },
            success: function (action, data) {
                App.cboMailID.setValue("");
                App.cboMailID.getStore().load(function () { cboMailID_Change(App.cboMailID); });

            },

            failure: function (action, data) {
                if (data.result.msgCode) {
                    HQ.message.show(data.result.msgCode, data.result.msgParam, '');
                }
            }
        });
    }
};
var deleteRecordGrid = function (item) {
    if (item == "yes") {
        App.grdMailDetail.deleteSelected();
        frmChange();
    }
};
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        HQ.isChange = false;
        App.cboMailID.getStore().load(function () { App.stoMailHeader.reload(); });
        
    }
};

var checkSpecialChar = function (value) {
    var regex = /^[a-zA-Z0-9_]+$/; //var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/ 20160913: Cho phép nhập ký tự '-'
    if (!HQ.util.passNull(value.toString()).match(regex))
        return false;
    for (var i = 0, n = value.length; i < n; i++) {
        if (value.charCodeAt(i) > 127) {
            return false;
        }
    }
    return true;
}
///////////////////////////////////