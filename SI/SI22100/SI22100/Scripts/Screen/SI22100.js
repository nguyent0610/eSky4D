//// Declare //////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////
var keys = ['QuestID'];
var fieldsCheckRequire = ["QuestID", "QuestDescr"];
var fieldsLangCheckRequire = ["QuestID", "Content"];

var keys1 = ['AnswerID'];
var fieldsCheckRequire1 = ["Descr"];
var fieldsLangCheckRequire1 = ["Descr"];

var _Source = 0;
var _maxSource = 1;

var _grid = 0;
var _maxGrid = 1;
var loadgridmaster = false;
var _gridSource = 0;
var _maxLoadGridSource = 2;

var indexsel = 0;
var penddingStatus = 'H';
var completStatus = 'A';
var typeDefault = 'O';

var lstAnswerDel = [];


////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////
var checkLoad = function () {
    App.stoQuestionList.reload();
};

var checkGird = function () {
    _grid += 1;
    if (_grid == _maxGrid) {

        App.stoQuestionList.commitChanges();
        _grid = 0;
        if (loadgridmaster == false) {
            HQ.store.filterStore(App.stoAnswer, 'QuestListID', '**');
        }
        else {
            var record = App.slmQuestion.selected.items[0];
            var sto = App.stoAnswer;
            if (record != undefined) {
                if (!Ext.isEmpty(record.data.QuestListID)) {
                    HQ.store.filterStore(sto, 'QuestListID', record.data.QuestListID);
                }
                else {
                    HQ.store.filterStore(sto, 'QuestListID', '**');
                }
            }
            else {
                HQ.store.filterStore(sto, 'QuestListID', '**');
            }
        }
        loadgridmaster = true;
    }
}
var checkGridLoad = function (sto) {
    _gridSource += 1;
    if (_gridSource === _maxLoadGridSource) {
        _gridSource = 0;
        if (App.cboStatus.getValue() === penddingStatus) {
            HQ.store.insertBlank(App.stoQuestion, keys);
        }
        HQ.common.showBusy(false);
    }
};
///////////////////////////////////////////////////////////////////
var menuFirstClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdQuestList);
        case "prev":
            HQ.grid.prev(App.grdQuestList);
            break;
        case "next":
            HQ.grid.next(App.grdQuestList);
            break;
        case "last":
            HQ.grid.last(App.grdQuestList);
            break;
        case "refresh":
            if (HQ.isChange == true) {
                HQ.message.show(20150303, '', 'SI22100refresh');
            }
            else {
                HQ.isFirstLoad = true;
                //App.txtSearch.setValue('');
                //App.dtpFromDay.setValue('');
                //App.dtpToDay.setValue('');
                //App.stoQuestList.reload();
                //App.dtpFromDay.setValue(HQ.bussinessDate);
                //App.dtpToDay.setValue(HQ.bussinessDate);
                App.stoQuestList.reload();
            }
            break;
        case "new":
            if (HQ.isChange) {
                HQ.message.show(150, '', 'refreshFirst');
            }
            else {
                App.frmFirst.hide();
                App.frmMain.show();
                //    App.frmMain.setTitle("New");
                App.cboType.setReadOnly(false);
                App.cboQuestListID.setValue("");
            }
            var record = HQ.store.findInStore(App.stoQuestion, ['QuestDescr'], ['']);
            if (!record) {
                HQ.store.insertBlank(App.stoQuestion, keys);
            }
            break;
    }
};
var menuClick = function (command) {
    switch (command) {
        case "first":
            if (HQ.focus == 'header')
                HQ.combo.first(App.cboQuestListID, HQ.isChange || HQ.isChange0);
            else if (HQ.focus == 'Question') {
                HQ.grid.first(App.grdQuestion);
                loadDetail();
            }
            else if (HQ.focus == 'Detail') {
                HQ.grid.first(App.grdAnswer);
            }
        
            break;
        case "prev":
            if (HQ.focus == 'header')
                HQ.combo.prev(App.cboQuestListID, HQ.isChange || HQ.isChange0);
            else if (HQ.focus == 'Question') {
                HQ.grid.prev(App.grdQuestion);
                loadDetail();
            }
            else if (HQ.focus == 'Detail') {
                HQ.grid.prev(App.grdAnswer);
            }
     
            break;
        case "next":
            if (HQ.focus == 'header')
                HQ.combo.next(App.cboQuestListID, HQ.isChange || HQ.isChange0);
            else if (HQ.focus == 'Question') {
                HQ.grid.next(App.grdQuestion);
                loadDetail();
            }
            else if (HQ.focus == 'Detail') {
                HQ.grid.next(App.grdAnswer);
            }
     
            break;

        case "last":
            if (HQ.focus == 'header')
                HQ.combo.last(App.cboQuestListID, HQ.isChange || HQ.isChange0);
            else if (HQ.focus == 'Question') {
                HQ.grid.last(App.grdQuestion);
                loadDetail();
            }
            else if (HQ.focus == 'Detail') {
                HQ.grid.last(App.grdAnswer);
            }
   
            break;
        case "refresh":
            if (HQ.isChange == true)
            {
                HQ.message.show(20150303, '', 'SI22100Mainrefresh');
            }
            else {
                HQ.isFirstLoad = true;
                if (HQ.focus == 'header') {
                    App.cboQuestListID.store.load(function () {
                        App.stoQuestionList.reload();
                    });
                }
                else
                    if (HQ.focus == 'Question') {
                        App.grdQuestion.store.reload();
                    }
                    else
                        App.grdAnswer.store.reload();
            }
            break;
        case "new":
            if (HQ.isInsert) {
                if (HQ.focus === 'header') {
                    if (HQ.isChange) {
                        HQ.message.show(150, '', '');
                    }
                    else {
                        if (HQ.focus === 'header') {
                            App.cboQuestListID.setValue('');
                            App.txtDescr.setValue('');
                            App.cboType.setValue('');
                            App.cboStatus.setValue(penddingStatus);
                            App.stoQuestionList.reload();
                     
                        }
                    }
                }
                else if (HQ.focus === 'Question') {
                    if (App.cboStatus.getValue() === penddingStatus) {
                        HQ.grid.insert(App.grdQuestion, keys);

                    }
                }
                else if (HQ.focus === 'Detail') {
                    if (App.cboStatus.getValue() === penddingStatus) {
                        HQ.grid.insert(App.grdAnswer, keys1);
                    }
                }
            }
            break;
        case "delete":
            if (HQ.isDelete) {
                if (App.cboStatus.getValue() === penddingStatus) {
                    if (HQ.focus == 'header') {
                        if (!Ext.isEmpty(App.cboQuestListID.getValue())) {
                            HQ.message.show(11, '', 'deleteData');
                        }
                        else {
                            menuClick('new');
                        }
                    }
                    else if (HQ.focus == 'Question') {
                        if (App.slmQuestion.selected.items[0] != undefined) {
                            var rowindex = HQ.grid.indexSelect(App.grdQuestion);
                            if (rowindex != '')
                                HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdQuestion), ''], 'deleteData', true);
                        }
                    }
                    else if (HQ.focus == 'Detail') {
                        if (App.slmAnswer.selected.items[0] != undefined) {
                            var rowindex = HQ.grid.indexSelect(App.grdAnswer);
                            if (rowindex != '')
                                HQ.message.show(2015020807, [HQ.grid.indexSelect(App.grdAnswer), ''], 'deleteData', true);
                        }
                    }
                }
       
            }
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain)
                    && HQ.store.checkRequirePass(App.stoQuestion, keys, fieldsCheckRequire, fieldsLangCheckRequire)
                 && checkCorrectAnswer()
                    ) {
                    save();
                }
            }
            break;
    }
};

var btnHome_Click = function () {
    if (HQ.isChange) {
        HQ.message.show(5, '', 'askClose', true);
    }
    else {
        App.frmFirst.show();
        App.frmMain.hide();
    }
};
var askClose = function (item) {
    if (item != 'yes') {
        App.stoQuestList.reload();
        App.frmMain.hide();
        App.frmFirst.show();
        HQ.common.changeData(false, 'SI22100');
    } else {
        App.frmMain.show();
        App.frmFirst.hide();
    }
}
var firstLoad = function () {
    HQ.util.checkAccessRight();  
    App.cboQuestListID.getStore().addListener('load', checkLoad);
    App.stoQuestion.addListener('load', checkGridLoad);
    App.stoAnswer.addListener('load', checkGridLoad);
    App.cboStatus.getStore().addListener('load', cboStatusLoad);
    App.cboType.getStore().addListener('load', cboTypeLoad);
};

function cboStatusLoad() {
    if (!App.cboStatus.getValue() || !App.cboQuestListID.getValue()) {
        App.cboStatus.setValue(penddingStatus);
    }
    App.cboHandle.store.reload();

}
function cboTypeLoad() {
    if (!App.cboQuestListID.getValue()) {
        App.cboType.setValue(typeDefault);
    }
}

var cboQuestListID_TriggerClick = function (sender, value) {
    if (HQ.isChange) {
        HQ.message.show(150, '', '');
    }
    else {
        App.cboQuestListID.setValue('');
        App.stoQuestionList.reload();
        //App.stoQuestion.reload();
    }

};

var cboQuestListID_Change = function (sender, newValue, oldValue) {
    if (!newValue || !HQ.isNew) {
        App.stoQuestionList.reload();
    }
};


var cboQuestListID_Blur = function (sender, newValue, oldValue) {

    var val = !App.cboQuestListID.getValue() ? 'a' : App.cboQuestListID.getValue();
    if (!HQ.util.checkSpecialChar(val)) {
        App.cboQuestListID.setValue('');
        HQ.message.show(226);
    }
    else if(!HQ.isChange)  {
        App.stoQuestionList.reload();
    }
    //App.stoQuestion.reload();
};

var btnLoadData_Click = function () {
    
    if (HQ.form.checkRequirePass(App.frmFirst)) {
        App.stoQuestList.reload();
    }
};


function cboStatus_Change() {
    App.cboHandle.store.reload();
}

var cboQuestListID_Select = function (sender, newValue, oldValue) {
    if (App.cboQuestListID.valueModels && App.cboQuestListID.valueModels[0]) {
        App.stoQuestionList.reload();
        if (HQ.isInsert === true)
            HQ.isFirstLoad = true;
        HQ.isNew = false;
        //App.stoQuestion.reload();
        frmChange();
    }
};


var loadDetail = function () {
    var record = App.slmQuestion.selected.items[0];
    indexsel = App.slmQuestion.selected.items[0].index;
    var sto = App.stoAnswer;
    if (record != undefined) {
        var temp = HQ.store.findRecord(sto, ['QuestID', 'AnswerID'], [record.data.QuestID, '']);
        if (temp) {
            sto.remove(temp);
        }
        if (!Ext.isEmpty(record.data.QuestListID) && !Ext.isEmpty(record.data.QuestID)) {
            sto.filterBy(function (item) {
                  if (item.data.QuestListID === record.data.QuestListID && item.data.QuestID === record.data.QuestID.toString()) {
                        return item;
                }
            });
        }
        else {
            sto.filterBy(function () {
                    return false;
            });
        }
        if (HQ.isInsert) {
            insertAnswer(sto);
        }
    }
};

function insertAnswer(sto) {
    var record = App.slmQuestion.selected.items[0];
    var newAnswer;
    if (App.cboType.getValue() === "Y") {
        if (sto.data.items.length === 0) {
            if (!HQ.store.findInStore(App.stoAnswer, ['QuestID'], [record.data.QuestID.toString()]) && record.data.QuestID) {
                sto.suspendEvents();
                HQ.store.insertRecord(sto, ["AnswerID"], Ext.create('App.mdlAnswer'), false);
                newAnswer = sto.data.items[0];
                newAnswer.set('QuestListID', record.data.QuestListID);
                newAnswer.set('QuestID', record.data.QuestID.toString());
                newAnswer.set('AnswerID', '1');
                newAnswer.set('AnswerDescr', 'Yes');
                newAnswer.set('Correct', false);

                HQ.store.insertRecord(sto, ["AnswerID"], Ext.create('App.mdlAnswer'), false);
                newAnswer = sto.data.items[1];
                newAnswer.set('QuestListID', record.data.QuestListID);
                newAnswer.set('QuestID', record.data.QuestID.toString());
                newAnswer.set('AnswerID', '2');
                newAnswer.set('AnswerDescr', 'No');
                newAnswer.set('Correct', false);
                sto.resumeEvents();
                App.grdAnswer.view.refresh();
            }
           
        }
    } else if (App.cboStatus.getValue() === penddingStatus) {
        if (!HQ.store.findInStore(App.stoAnswer, ['QuestID'], [record.data.QuestID.toString()]) && record.data.QuestID) {
            sto.suspendEvents();
            HQ.store.insertRecord(sto, ["AnswerID"], Ext.create('App.mdlAnswer'), false);
            newAnswer = sto.data.items[0];
            newAnswer.set('QuestListID', record.data.QuestListID);
            newAnswer.set('QuestID', record.data.QuestID.toString());
            newAnswer.set('AnswerID', '');
            sto.resumeEvents();
            App.grdAnswer.view.refresh();
        } 
    }

    if (App.cboType.getValue() !== 'T') {
        HQ.store.insertBlank(App.grdAnswer.store, keys1);
    }

 }
var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingdata'));
};
//khi có sự thay đổi thêm xóa sửa trên lưới gọi tới để set * cho header de biết đã có sự thay đổi của grid


var stoQuestionList_Load = function (sto,e,success) {
    if (HQ.isInsert === true)
        HQ.isFirstLoad = true;
    HQ.isNew = false;
    //App.cboQuestListID.forceSelection = true;
    var record;
    if (sto.data.length === 0 && HQ.isInsert) {
        HQ.isNew = true;//record la new  
        //App.cboQuestListID.forceSelection = false;
        HQ.store.insertRecord(sto, ["QuestListID"], Ext.create('App.mdlQuestionList'), false);
        record = !sto.getAt(0) ? sto.data.items[0] : sto.getAt(0);
        HQ.common.setRequire(App.frmMain);  //to do cac o la require  
        App.cboType.setReadOnly(false);
        App.txtDescr.setReadOnly(false);
        record.set('QuestTListDescr', '');
        record.set('Status', penddingStatus);
        record.set('Type', typeDefault);
        App.frmMain.loadRecord(record);
        App.cboHandle.setValue('');

    }
   
    if (!HQ.isInsert && HQ.isNew) {
        //App.cboQuestListID.forceSelection = false;
        HQ.common.lockItem(App.frmMain, true);
        HQ.store.insertBlank(sto, keys);
    }
    else if (!HQ.isUpdate) {
        HQ.common.lockItem(App.frmMain, true);
    }

    if (sto.data.length > 0) {
        record = sto.getAt(0);
        if (record.data.QuestListID) {
            App.frmMain.loadRecord(record);
             App.cboType.setReadOnly(true);
             App.cboType.setValue(record.data.Type);
            if (App.cboStatus.getValue() ==='C') {
                App.txtDescr.setReadOnly(true);
         }
        }
    }


    if (success) {
        App.stoQuestion.reload();
       
    }
   
    frmChange();
    HQ.common.showBusy(false);
    App.cboHandle.store.reload();
};

var stoQuestion_Load = function (sto,e,success) {
    App.stoAnswer.reload();
    App.stoAnswer.addListener('load', checkGird);

};

var stoAnswer_Load = function (sto) {
    App.stoQuestionList.commitChanges();
};

////// grd///////////////
var grdQuestion_Reject = function (record) {
    var questListID = record.data.QuestListID;
        // delete grid Detail //
        var length = App.stoAnswer.count();
        for (var i = 0; i < length; i++) {
            var item = App.stoAnswer.findRecord('QuestListID', questListID);
            if (item != undefined)
                App.grdAnswer.store.remove(item);
        }
        HQ.grid.checkReject(record, App.grdQuestion); 
        frmChange();
        // xóa tat ca record trên Detail
};

var grdQuestion_BeforeEdit = function (editor, e) {
    if (App.cboStatus.value == "C") {
        return false;
    }
    if (!HQ.form.checkRequirePass(App.frmMain)) {
        return false;
    }
    if (e.field === 'QuestID') {
        return false;
    }
};
var dtpFromDay_Change = function (a, newValue, oldValue) {
    App.dtpToDay.setMinValue(newValue);
}
var grdQuestion_Edit = function (item, e) {
  
    //if (e.field === 'QuestDescr') {
    //    e.record.data.QuestListID = App.cboQuestListID.getValue();
    //    e.record.data.QuestID = App.stoQuestion.allData.length;
    //    App.grdQuestion.view.refresh();
    //    var record = HQ.store.findInStore(App.stoQuestion, ['QuestDescr'], ['']);
    //    if (!record) {
    //        HQ.store.insertBlank(App.stoQuestion, keys);  
    //    }
    //    //if (App.cboType.getValue() === "Y") {
    //        insertAnswer(App.stoAnswer);
    //    //}

    //}

  if (e.field === 'QuestDescr' && !e.record.data.QuestListID && e.value)
  {
      e.record.set('QuestListID', App.cboQuestListID.getValue());
      e.record.set('QuestID', lastLineRefQuestion(App.stoQuestion));
      HQ.store.insertBlank(App.stoQuestion, keys);
      insertAnswer(App.stoAnswer);
  }
    //HQ.grid.checkInsertKey(App.grdQuestion, e, keys);
  loadDetail();
    frmChange();
    indexsel = e.rowIdx;
};

var grdQuestion_ValidateEdit = function (item, e) {
    return HQ.grid.checkValidateEdit(App.grdQuestion, e, keys);
};

var grdAnswer_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdAnswer);
    frmChange();
};

var grdAnswer_BeforeEdit = function (editor, e) {
    if (HQ.isUpdate || HQ.isInsert) {
        if (App.cboStatus.value == "C") {
            return false;
        }

        if (App.cboType.getValue() === 'T') {
            return false;
        }
        if (!HQ.form.checkRequirePass(App.frmMain)) {
            return false;
        }
        if (App.slmQuestion.selected.items[0] == undefined) {
            return false;
        }
        if (App.slmQuestion.selected.items[0] != undefined) {
            if (Ext.isEmpty(App.slmQuestion.selected.items[0].data.QuestListID) || Ext.isEmpty(App.slmQuestion.selected.items[0].data.QuestID)) {
                return false;
            }

            //if (App.slmQuestion.selected.items[0].data.Type !== 'M' && App.slmQuestion.selected.items[0].data.Type !== 'O') {
            //    return false;
            //}
        } else {
            return false;
        }
        if (e.field === 'AnswerDescr' && App.cboType.getValue() === "Y") {
            return false;
        }
        if (e.field === 'Correct' && !e.record.data.AnswerDescr) {
            return false;
        }

        //return HQ.grid.checkBeforeEdit(e, keys1);
    }
    else
        return false;
};

var grdAnswer_Edit = function (item, e) {
    if (e.field === 'AnswerDescr') {
        e.record.data.AnswerID = lastLineRefAnswer(App.stoAnswer);
        e.record.data.QuestListID = App.cboQuestListID.getValue();
        e.record.data.QuestID = App.grdQuestion.selModel.selected.items[0].data.QuestID;
    }
    if (App.cboType.getValue() === 'O' || App.cboType.getValue() === "Y") {
        if (e.field === 'Correct' && e.value === true) {
            setOneChoice(e.record.data.AnswerID);
        }
    }

    if (App.cboType.getValue() !== 'T') {
        HQ.grid.checkInsertKey(App.grdAnswer, e, 'AnswerDescr');
    }
    frmChange();
};

var grdAnswer_ValidateEdit = function (item, e) {
    if (e.field === 'AnswerDescr' && !e.value) {
        return false;
    }

    return HQ.grid.checkValidateEdit(App.grdAnswer, e, keys1);
};

function setOneChoice(id) {
    var lstAnswer = App.stoAnswer.data.items;
    for (var i = 0; i < lstAnswer.length; i++) {
        if (lstAnswer[i].data.AnswerID !== id) {
            lstAnswer[i].data.Correct = false;
        }
    }
    App.grdAnswer.view.refresh();
}
//bug Thông báo -- bỏ trống trên nhiều dòng
function checkCorrectAnswer() {
    
    var check = '' ;
    var dataQuestion = App.stoQuestion.snapshot || App.stoQuestion.allData || App.stoQuestion.data;
    if (App.cboType.getValue() === 'Y' || App.cboType.getValue() === 'M' || App.cboType.getValue() === 'O') {
        for (var j = 0; j < dataQuestion.length; j++) {

            if (dataQuestion.items[j].data.QuestID) {
                var record = HQ.store.findInStore(App.stoAnswer, ['QuestID', 'Correct'], [dataQuestion.items[j].data.QuestID, true]);
                if (record == undefined) {
                    check += (dataQuestion.items[j].data.QuestID + ' ');
                }
            }
        }
        if (check != "") {
            HQ.message.show(2018112950, check);
            return false;
        }
    }

    //if (App.cboType.getValue() === 'M' || App.cboType.getValue() === 'O') {
    //    for (var j = 0; j < dataQuestion.length; j++) {
    //        if (dataQuestion.items[j].data.QuestID) {
    //            if (!checkAnswer(App.stoAnswer, dataQuestion.items[j].data.QuestID)) {
    //                return false;
    //            }
                
    //        }
    //    }
    //}
    return true;
}

function checkAnswer(sto, questID) {
    var item = 0;
    var data = sto.snapshot || sto.allData || sto.data;
    for (var i = 0; i < data.length; i++) {
        if (data.items[i].data.QuestID === questID && data.items[i].data.AnswerID) {
            item ++;
        }
    }
    if (item <= 1) {
        HQ.message.show(2018082751);
        return false;
    }
    return true;
}
//////////////////////////// NPP /////////////////////////////////////
var frmChange = function () {
    debugger
        App.frmMain.getForm().updateRecord();
        HQ.isChange = HQ.store.isChange(App.stoQuestionList)
                || HQ.store.isChange(App.stoQuestion)
                || HQ.store.isChange(App.stoAnswer)
                || HQ.store.isChange(App.grdQuestion.store)
                || HQ.store.isChange(App.grdAnswer.store);
        HQ.common.changeData(HQ.isChange, 'SI22100');//co thay doi du lieu gan * tren tab title header  
        if (!HQ.isChange) {
            HQ.isChange = HQ.store.isChange(App.stoQuestion);
            App.cboQuestListID.setReadOnly(HQ.isChange);
        }

        if (!HQ.isChange) {
            HQ.isChange = HQ.store.isChange(App.stoAnswer);
            App.cboQuestListID.setReadOnly(HQ.isChange);
        }

        if (App.cboQuestListID.valueModels) {
            if (App.cboQuestListID.valueModels.length > 0 || !HQ.isNew) {
                App.cboType.setReadOnly(true);
            }
        }
        if (App.cboQuestListID.valueModels == null || HQ.isNew === true) {
            App.cboQuestListID.setReadOnly(false);
        }
        else {
            App.cboQuestListID.setReadOnly(HQ.isChange);

        }
        
};

var SI22100refresh = function (item) {
    if (item === 'yes') {
        if (HQ.isNew) {
            App.cboQuestListID.setValue('');
        }
        HQ.isChange = false;
        HQ.isFirstLoad = true;

        App.stoQuestionList.reload();
    }
};
var SI22100Mainrefresh = function (item) {
    if (item === 'yes') {
        if (HQ.isNew) {
            App.cboQuestListID.setValue('');
        }
        HQ.isChange = false;
        HQ.isFirstLoad = true;

        App.stoQuestionList.reload();
    }
};

/////////////////////////////////////////////////////////////////////////
//// Process Data ///////////////////////////////////////////////////////
var deleteData = function (e) {
    if (e === 'yes') {
        if (HQ.focus === 'header') {
            App.frmMain.submit({
                url: 'SI22100/Delete',
                params: {
                    questListID: App.cboQuestListID.getValue()
                },
                success: function () {
                    App.cboQuestListID.setValue('');
                    App.cboQuestListID.store.reload();
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        } 
        else if (HQ.focus === 'Question') {
            var questListID = App.slmQuestion.selected.items[0].data.QuestListID;
                // delete grid Detail //
                var length = App.stoAnswer.count();
                for (var i = 0; i < length; i++)
                {
                    var item = App.stoAnswer.findRecord('QuestListID', questListID);
                    if (item != undefined)
                        App.grdAnswer.store.remove(item);
                }

                App.grdQuestion.deleteSelected();
                HQ.grid.insert(App.grdQuestion, keys);
                frmChange();

        }
        else if (HQ.focus === 'Detail' && App.cboType.getValue() !== 'Y') {
            var data = App.grdAnswer.selModel.selected.items[0].data;
            lstAnswerDel.push(data);
            App.grdAnswer.deleteSelected();
            frmChange();
        }
    }
};

var save = function () {

    if (App.stoQuestion.data.length === 1) {
        var data = HQ.store.findInStore(App.stoQuestion, ['QuestListID'], ['']);
        if (data) {
            HQ.message.show(2016091901);
            return;
        }
    }

    if (App.stoQuestion.data.length < 1) {
        HQ.message.show(2016091901);
        return;
    }
    if (Ext.isEmpty(App.cboQuestListID.getValue())) {
        HQ.message.show(15, App.cboID.fieldLabel);
        return;
    }


    if (Ext.isEmpty(App.txtDescr.getValue())) {
        HQ.message.show(15, App.txtDescr.fieldLabel);
        return;
    }


    var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/;
    var value = App.cboQuestListID.getValue();
    if (!HQ.util.passNull(value.toString()).match(regex) || HQ.util.checkStrUnicode(value.toString())) {
        HQ.message.show(20140811, App.cboQuestListID.fieldLabel, '');
        return;
    }

    // get all change Data stoAnswer//
    var store = App.stoAnswer, data = store.data;
    store.allData = store.snapshot; 
    store.data = data;

    App.frmMain.submit({
        timeout: 1800000,
        waitMsg: HQ.common.getLang("SavingData"),
        url: 'SI22100/Save',
        type: 'POST',
        params: {
            questionList: Ext.encode(App.stoQuestionList.getRecordsValues()),
            listQuestion: HQ.store.getData(App.stoQuestion),
            listAnswer: HQ.store.getAllData(App.stoAnswer),
            lstAnswerDel: Ext.encode(lstAnswerDel),
            //listAnswerAll: Ext.encode(getRecordValues(App.grdAnswer.store.snapshot.getRange())),
            listNew: HQ.isNew
        },
        success: function (msg, data) {
            HQ.isNew = false;
            App.cboQuestListID.store.reload();
            App.stoAnswer.reload();
            App.stoQuestion.reload();
            App.cboType.setReadOnly(true);
            App.cboHandle.setValue('');
            App.cboHandle.store.reload();
   
            lstAnswerDel = [];
            HQ.message.show(201405071);
            App.cboType.setReadOnly(true);
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });

};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ///////////////////////////////////////////////////
var lastLineRefQuestion = function (sto) {
    var num = 0;
    sto.data.each(function (item) {
        if (!Ext.isEmpty(item.data.QuestID) && parseInt(item.data.QuestID) > num) {
            num = parseInt(item.data.QuestID);
        }
    });
    num++;
    return num.toString();
};
var lastLineRefAnswer = function (sto) {
    var num = 0;
    sto.data.each(function (item) {
        if (!Ext.isEmpty(item.data.AnswerID) && parseInt(item.data.AnswerID) > num) {
            num = parseInt(item.data.AnswerID);
        }
    });
    num++;
    //var lineRef = num.toString();
    //var len = lineRef.length;
    //for (var i = 0; i < 5 - len; i++) {
    //    lineRef = "0" + lineRef;
    //}
    return num.toString();
};

var checkFocus = function() {
    if (!App.cboQuestListID.getValue()) {
        HQ.form.checkRequirePass(App.frmMain);
    }
}
////////////////renderer///////////////////////////



/// check ton tai cua recod j trong cboParentQuestion function checkExitData
//true // ko tồn tại//
// false // có tồn tại//
var checkExitData = function (sto, record) {  
    var res  = true;
    var store = sto.data;
    store.each(function(item) {
        if (item.data.QuestListID === record) {
            res = false;
            return false;
        }
    });
    return res;
};

var setValueCombobox = function (cbo, sto, recordQuestListID, recordParent) {
    var store = sto.data;
    store.each(function(item) {
        if (checkExitData(cbo, item.data.QuestListID) === true) {
            if (recordQuestListID !== item.data.QuestListID) {
                var model = Ext.create(cbo.model,
                {
                    QuestListID: item.data.QuestListID,
                    Question: item.data.Question
                });
                cbo.autoSync = false;
                cbo.insert(0, model);
            }
        } else {
            if (recordQuestListID == item.data.QuestListID) {
                if (HQ.store.findRecord(cbo, ['QuestListID'], [recordQuestListID]) != undefined) {
                    cbo.remove(HQ.store.findRecord(cbo, ['QuestListID'], [recordQuestListID]));
                }
            }
        }
    });

    store.each(function(item) {
        if (!Ext.isEmpty(item.data.ParentQuestion)) {
            if (item.data.ParentQuestion != recordParent) {
                var res = item.data.ParentQuestion;
                var remove = HQ.store.findRecord(cbo, ['QuestListID'], [res]);
                if (remove != undefined) {
                    cbo.remove(remove);
                }
            }

        }

    });
};

var setQuestListID = function (sto, questList , quest) {
};

var checkDeleteQuestion = function (questListID, sto) {
    var store = sto.data;
    var res ="";
    store.each(function(item) {
        if (item.data.ParentQuestion === questListID) {
            res = item.data.QuestListID;
        }
    });
    return res;
};

var stringFilter = function (record) {
     return HQ.grid.filterString(record, this);
};

var getRecordValues= function (recordRange) {
    var values = [];
    recordRange.forEach(function (record) {
        values.push(record.data);
    });
    return values;
};
///////////////////////////////////
//Import
var btnImport_Click = function (sender, e) {
    var fileName = sender.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (ext === "xls" || ext === "xlsx") {
        App.frmFirst.submit({
            waitMsg: "Importing....",
            url: 'SI22100/Import',
            timeout: 18000000,
            clientValidation: false,
            method: 'POST',
            params: {
            },
            success: function (msg, data) {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.cboQuestListID.store.reload();

                SI22100refresh('yes');
                if (!Ext.isEmpty(this.result.data.message)) {
                    HQ.message.show('2013103001', [this.result.data.message], '', true);
                }
                else {
                    HQ.message.process(msg, data, true);
                }
                App.stoQuestList.reload();

            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
    else {
        HQ.message.show('2014070701', '', '');
        sender.reset();
    }
};
///////////////////////////////////
//Export
var btnExport_Click = function () {
    App.frmFirst.submit({

        url: 'SI22100/Export',
        type: 'POST',
        timeout: 1000000,
        clientValidation: false,
        params: {
            questListID : App.cboQuestListID.getValue()
        },
        success: function (msg, data) {
            alert('sus');
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });
};


var btnEdit_Click = function (record) {
    
    App.frmMain.show();
    App.frmFirst.hide();
    App.cboQuestListID.setValue(record.data.QuestListID);
    App.cboType.setValue(record.data.Type);
    App.cboStatus.setValue(record.data.Status);
    App.txtDescr.setValue(record.data.QuestTListDescr)
};
var frmFirst_BoxReady = function ()
{
    App.dtpFromDay.setValue(HQ.bussinessDate);
    App.dtpToDay.setValue(HQ.bussinessDate);
}


