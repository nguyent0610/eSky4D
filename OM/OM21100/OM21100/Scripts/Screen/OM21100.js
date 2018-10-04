var _holdStatus = "H";
var _gridForDel;
var _isNewDisc = false;
var _isNewSeq = false;
var _discLoad = "";
var _seqLoad = "";
var _selBranchID = '';
var _selTerritory = '';
var keys1 = ['CpnyID'];
var invtIDRender = '';
var lstCpnyID = [];
var _copy = 0;
var _siteID = '';
var _siteEdit = '';
var _discSeqNew = '';
var _branchID = '';
var Main = {

    Process: {
        //kiem tra key da nhap du chua
        isAllValidKey: function (items, keys) {
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
        },

        isSomeValidKey: function (items, keys) {
            if (items && items.length > 0) {
                for (var i = 0; i < items.length; i++) {
                    for (var j = 0; j < keys.length; j++) {
                        if (items[i][keys[j]]) {
                            return true;
                        }
                    }
                }
                return false;
            } else {
                return true;
            }
        },

        //kiem tra nhung field yeu cau bat buoc nhap
        checkRequire: function (title, items, checkedFields, keys) {
            if (items != undefined) {
                for (var i = 0; i < items.length; i++) {
                    if (HQ.grid.checkRequirePass(items[i], keys)) continue;

                    for (var j = 0; j < checkedFields.length; j++) {
                        if (items[i][checkedFields[j]].trim() == "") {
                            HQ.message.show(2015020808, HQ.common.getLang(checkedFields[j]) + "," + title);
                            return false;
                        }
                    }
                }
                return true;
            } else {
                return true;
            }
        },

        deleteSelectedInGrid: function (item) {
            if (item == "yes") {
                if (_gridForDel) {
                    _gridForDel.deleteSelected();
                }
            }
        },


        deleteSelectedIngrdDiscItem: function (item) {
            if (item == "yes") {
                if (_gridForDel) {
                    _gridForDel.deleteSelected();
                    var lstAlldata = App.stoDiscItem.snapshot;
                    var check = true;
                    if (lstAlldata.items.length > 0) {
                        for (var i = 0; i < lstAlldata.items.length; i++) {
                            if (lstAlldata.items[i].data.InvtID != "") {
                                check = false;
                            }
                        }
                    }
                    if (check) {
                        App.chkStockPromotion.setReadOnly(!check);
                    }
                }
            }
        },

        deleteSelectedInGridFreeItem: function (item) {
            if (item == "yes") {
                if (_gridForDel) {
                    _gridForDel.deleteSelected();
                    if (App.chkDonateGroupProduct.getValue()) {
                        var lstAlldata = App.stoFreeItem.snapshot;
                        var check = true;
                        if (lstAlldata.items.length > 0) {
                            for (var i = 0; i < lstAlldata.items.length; i++) {
                                if (lstAlldata.items[i].data.FreeItemID != "" && (lstAlldata.items[i].data.GroupItem != "" || lstAlldata.items[i].data.Priority > 0)) {
                                    check = false;
                                }
                            }
                        }
                        App.chkDonateGroupProduct.setReadOnly(!check);
                    }
                    else {
                        if (!App.chkAutoFreeItem.getValue()) {
                            var lstAlldata = App.stoFreeItem.snapshot;
                            var check = true;
                            if (lstAlldata.items.length > 0) {
                                for (var i = 0; i < lstAlldata.items.length; i++) {
                                    if (lstAlldata.items[i].data.FreeItemID != "" && (lstAlldata.items[i].data.GroupItem != "" || lstAlldata.items[i].data.Priority > 0)) {
                                        check = false;
                                    }
                                }
                            }
                            if (check) {
                                App.chkDonateGroupProduct.enable();
                            }
                        }
                        if (!App.chkDonateGroupProduct.getValue()) {
                            var lstAlldata = App.stoFreeItem.snapshot;
                            var check = true;
                            if (lstAlldata.items.length > 0) {
                                for (var i = 0; i < lstAlldata.items.length; i++) {
                                    if (lstAlldata.items[i].data.FreeItemID != "" && (lstAlldata.items[i].data.GroupItem != "" || lstAlldata.items[i].data.Priority > 0)) {
                                        check = false;
                                    }
                                }
                            }
                            if (check) {
                                App.chkAutoFreeItem.enable();
                            }
                        }
                    }

                }
            }
        },

        showFieldInvalid: function (form) {
            var done = 1;
            form.getForm().getFields().each(function (field) {
                if (!field.isValid()) {
                    HQ.message.show(15, field.fieldLabel, '');
                    done = 0;
                    return false;
                }
            });
            return done;
        },

        getParamForGridCombo: function (grid, fieldName) {
            if (grid.selModel.selected.items.length > 0) {
                var record = grid.selModel.selected.items[0];
                return record.data[fieldName];
            }
            else {
                return "";
            }
        },

        reloadAllData: function () {
            App.cboDiscID.store.load();
            App.cboDiscSeq.store.load();
            App.stoDiscInfo.reload();
            App.stoDiscSeqInfo.reload();
            App.stoSubBreakItem.reload();
        },

        checkEntireRequire: function () {
            if (Main.Process.checkRequire(App.grdDiscBreak.title, App.grdDiscBreak.store.getChangedData().Created, [], [])
                    && Main.Process.checkRequire(App.grdDiscBreak.title, App.grdDiscBreak.store.getChangedData().Updated, [], [])

                    && Main.Process.checkRequire(App.pnlFreeItem.title, App.grdFreeItem.store.getChangedData().Updated, ["FreeItemID"], ["FreeItemID"])
                    && Main.Process.checkRequire(App.pnlFreeItem.title, App.grdFreeItem.store.getChangedData().Updated, ["FreeItemID"], ["FreeItemID"])

                    && Main.Process.checkRequire(App.pnlAppComp.title, App.grdCompany.store.getChangedData().Updated, ["CpnyID"], ["CpnyID"])
                    && Main.Process.checkRequire(App.pnlAppComp.title, App.grdCompany.store.getChangedData().Updated, ["CpnyID"], ["CpnyID"])

                    && Main.Process.checkRequire(App.pnlDPII.title, App.grdDiscItem.store.getChangedData().Updated, ["InvtID"], ["InvtID"])
                    && Main.Process.checkRequire(App.pnlDPII.title, App.grdDiscItem.store.getChangedData().Updated, ["InvtID"], ["InvtID"])

                    && Main.Process.checkRequire(App.pnlDPBB.title, App.grdBundle.store.getChangedData().Updated, ["InvtID"], ["InvtID"])
                    && Main.Process.checkRequire(App.pnlDPBB.title, App.grdBundle.store.getChangedData().Updated, ["InvtID"], ["InvtID"])

                    && Main.Process.checkRequire(App.pnlDPTT.title, App.grdDiscCustClass.store.getChangedData().Updated, ["ClassID"], ["ClassID"])
                    && Main.Process.checkRequire(App.pnlDPTT.title, App.grdDiscCustClass.store.getChangedData().Updated, ["ClassID"], ["ClassID"])

                    && Main.Process.checkRequire(App.pnlDPCC.title, App.grdDiscCust.store.getChangedData().Updated, ["CustID"], ["CustID"])
                    && Main.Process.checkRequire(App.pnlDPCC.title, App.grdDiscCust.store.getChangedData().Updated, ["CustID"], ["CustID"])

                    && Main.Process.checkRequire(App.pnlDPPP.title, App.grdDiscItemClass.store.getChangedData().Updated, ["ClassID"], ["ClassID"])
                    && Main.Process.checkRequire(App.pnlDPPP.title, App.grdDiscItemClass.store.getChangedData().Updated, ["ClassID"], ["ClassID"])) {

                if (App.stoBundle.data.length > 0) {
                    var flat = null;
                    var breakByVal = App.cboBreakBy.getValue();
                    var disClassVal = App.cboDiscClass.getValue();
                    // Check Grid 
                    App.stoBundle.data.each(function (item) {
                        if (!Ext.isEmpty(item.data.InvtID) && (item.data.BundleOrItem == 'B' || disClassVal == 'BB' || disClassVal == "CB" || disClassVal == "TB")) {

                            if (item.data.BundleQty == 0 && breakByVal == 'Q') {
                                HQ.message.show(1111, [App.pnlDPBB.title, HQ.common.getLang('BreakQty')], '', true);
                                flat = item;
                                return false;
                            }
                            if (item.data.BundleAmt == 0 && breakByVal == 'A') {
                                HQ.message.show(1111, [App.pnlDPBB.title, HQ.common.getLang('BreakAmt')], '', true);
                                flat = item;
                                return false;
                            }
                        }
                    });
                    if (!Ext.isEmpty(flat)) {
                        App.slmBundle.select(App.stoBundle.indexOf(flat));
                        return false;
                    } else {
                        return true;
                    }
                } else {
                    return true;
                }
            }
            else {

                return false;
            }
        },

        checkHasData: function () {
            var hasData = false;
            if (App.cboStatus.getValue() == _holdStatus) {
                if (App.grdDiscBreak.store.getCount() > 1 ||
                    (App.grdDiscBreak.store.getCount() == 1 &&
                        (App.grdDiscBreak.store.data.items[0].data.BreakQty > 0 ||
                            App.grdDiscBreak.store.data.items[0].data.BreakAmt > 0)
                    )) {
                    var flat = null;
                    var discClassVal = App.cboDiscClass.getValue();
                    var breakByVal = App.cboBreakBy.getValue();
                    if (discClassVal == "BB" || discClassVal == "CB" || discClassVal == "TB") {
                        var allData0 = App.grdFreeItem.store.snapshot || App.grdFreeItem.store.allData || App.grdFreeItem.store.data;
                        var isValid = false;
                        var showError = true;
                        App.grdDiscBreak.store.data.each(function (item) {
                            if (item.data.BreakAmt == 0) {
                                if (item.data.BreakQty != 0 || item.data.DiscAmt != 0 || item.data.Descr != '') {
                                    HQ.message.show(1111, [App.grdDiscBreak.title, App.grdDiscBreak.columns[2].text], '', true);
                                    flat = item;
                                    hasData = false;
                                    return false;
                                }
                            } else {
                                isValid = false;
                                if (item.data.DiscAmt == 0) {

                                    for (var i = 0; i < allData0.length; i++) {
                                        if (!Ext.isEmpty(allData0.items[i].data.FreeItemID) && allData0.items[i].data.LineRef == item.data.LineRef) {
                                            if (allData0.items[i].data.FreeItemQty == 0) {
                                                flat = item;
                                                showError = false;
                                                isValid = false;
                                                HQ.message.show(1111, [App.pnlFreeItem.title, App.grdFreeItem.columns[3].text], '', true);
                                                break;
                                            }
                                            isValid = true;
                                        }
                                    }
                                } else {
                                    isValid = true;
                                }
                                hasData = true;
                            }
                        });
                        if (!Ext.isEmpty(flat)) {
                            App.slmDiscBreak.select(App.grdDiscBreak.store.indexOf(flat));
                            return false;
                        }
                        if (!isValid && showError) {
                            HQ.message.show(1798);
                            return false;
                        }
                    } else {
                        App.grdDiscBreak.store.data.each(function (item) {
                            if (breakByVal == 'A') {
                                if (item.data.BreakAmt == 0) {
                                    if (item.data.BreakQty != 0 || item.data.DiscAmt != 0 || item.data.Descr != '') {
                                        HQ.message.show(1111, [App.grdDiscBreak.title, App.grdDiscBreak.columns[2].text], '', true);
                                        flat = item;
                                        hasData = false;
                                        return false;
                                    }
                                } else {
                                    hasData = true;
                                }
                            } else if (breakByVal == 'Q') {
                                if (item.data.BreakQty == 0) {
                                    if (item.data.BreakAmt != 0 || item.data.DiscAmt != 0 || item.data.Descr != '') {
                                        HQ.message.show(1111, [App.grdDiscBreak.title, App.grdDiscBreak.columns[1].text], '', true);
                                        flat = item;
                                        hasData = false;
                                        return false;
                                    }
                                } else {
                                    hasData = true;
                                }
                            }

                        });
                        if (!Ext.isEmpty(flat)) {
                            App.slmDiscBreak.select(App.grdDiscBreak.store.indexOf(flat));
                            return false;
                        }
                    }

                    if (!hasData) {
                        if (breakByVal == 'Q')
                            HQ.message.show(2018070201, [HQ.common.getLang('DiscBreak'), HQ.common.getLang('BreakQty')], '', true);
                        else
                            HQ.message.show(2018070201, [HQ.common.getLang('DiscBreak'), HQ.common.getLang('BreakAmt')], '', true);
                        hasData = false;
                        return hasData;
                    }
                    if (App.grdCompany.store.getCount() > 0 && App.grdCompany.store.data.items[0].data.CpnyID) {
                        // 0: {Code: "BB", Descr: "Item Bundle"}
                        if (App.cboDiscClass.value == "BB") {
                            if (App.grdBundle.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPBB'), '');
                                hasData = false;
                            }
                        }
                            // 1: {Code: "CB", Descr: "Customer and Item Bundle"}
                        else if (App.cboDiscClass.value == "CB") {
                            if (App.grdBundle.store.getCount() > 1 && App.grdDiscCust.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPBB') + ' / ' + HQ.common.getLang('DPCC'), '');
                                hasData = false;
                            }
                        }
                            // 2: {Code: "CC", Descr: "Customer"}
                        else if (App.cboDiscClass.value == "CC") {
                            if (App.grdDiscCust.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPCC'), '');
                                hasData = false;
                            }
                        }
                            // 3: {Code: "CI", Descr: "Customer and Invt. Item"}
                        else if (App.cboDiscClass.value == "CI") {
                            if (App.grdDiscCust.store.getCount() > 1 && App.grdDiscItem.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPCC') + ' / ' + HQ.common.getLang('DPII'), '');
                                hasData = false;
                            }
                        }
                            // 4: {Code: "II", Descr: "Inventory Item"}
                        else if (App.cboDiscClass.value == "II") {
                            if (App.grdDiscItem.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPII'), '');
                                hasData = false;
                            }
                        }
                            // 5: {Code: "PP", Descr: "Product Group"}
                        else if (App.cboDiscClass.value == "PP") {
                            if (App.grdDiscItemClass.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPPP'), '');
                                hasData = false;
                            }
                        }
                            // 6: {Code: "TB", Descr: "Shop Type and Item Bundle"}
                        else if (App.cboDiscClass.value == "TB") {
                            if (App.grdDiscCustClass.store.getCount() > 1 && App.grdBundle.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPTT') + ' / ' + HQ.common.getLang('DPBB'), '');
                                hasData = false;
                            }
                        }
                            // 7: {Code: "TI", Descr: "Shop Type and Invt. Item"}
                        else if (App.cboDiscClass.value == "TI") {
                            if (App.grdDiscCustClass.store.getCount() > 1 && App.grdDiscItem.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPTT') + ' / ' + HQ.common.getLang('DPII'), '');
                                hasData = false;
                            }
                        }
                            // 8: {Code: "TP", Descr: "Prod. Group and Shop Type"}
                        else if (App.cboDiscClass.value == "TP") {
                            if (App.grdDiscCustClass.store.getCount() > 1 && App.grdDiscItemClass.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPTT') + ' / ' + HQ.common.getLang('DPPP'), '');
                                hasData = false;
                            }
                        }
                            // 9: {Code: "TT", Descr: "Shop Type"}
                        else if (App.cboDiscClass.value == "TT") {
                            if (App.grdDiscCustClass.store.getCount() > 1) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPTT'), '');
                                hasData = false;
                            }
                        }
                        else if (App.cboDiscClass.value == "CL") {
                            if (App.grdDiscCustCate.store.getCount() > 1
                                || App.grdDiscCustCate.store.getCount() == 1 && App.grdDiscCustCate.store.data.items[0].data.CustCateID != '') {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPCL'), '');
                                hasData = false;
                            }
                        }
                        else if (App.cboDiscClass.value == "CT") {
                            if (App.grdDiscChannel.store.getCount() > 1 
                                || App.grdDiscChannel.store.getCount() == 1 && App.grdDiscChannel.store.data.items[0].data.ChannelID != '') {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPCT'), '');
                                hasData = false;
                            }
                        }

                        else if (App.cboDiscClass.value == "IC") {
                            if ((App.grdDiscItem.store.getCount() > 1)
                                && (App.grdDiscChannel.store.getCount() > 1
                                    || App.grdDiscChannel.store.getCount() == 1 && App.grdDiscChannel.store.data.items[0].data.ChannelID != '')
                                ) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPCT') + ' / ' + HQ.common.getLang('DPII'), '');
                                hasData = false;
                            }
                        }

                        else if (App.cboDiscClass.value == "GC") {
                            if ((App.grdDiscItemClass.store.getCount() > 1 )                                
                                && (App.grdDiscChannel.store.getCount() > 1
                                    || App.grdDiscChannel.store.getCount() == 1 && App.grdDiscChannel.store.data.items[0].data.ChannelID != '')
                                ) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPCT') + ' / ' + HQ.common.getLang('DPPP'), '');
                                hasData = false;
                            }
                        }
                        else if (App.cboDiscClass.value == "GP") {
                            if ((App.grdDiscItemClass.store.getCount() > 1)
                               && (App.grdDiscCustCate.store.getCount() > 1
                                   || App.grdDiscCustCate.store.getCount() == 1 && App.grdDiscCustCate.store.data.items[0].data.CustCateID != '')
                               ) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPCL') + ' / ' + HQ.common.getLang('DPPP'), '');
                                hasData = false;
                            }
                        }
                        else if (App.cboDiscClass.value == "GI") {
                            if ((App.grdDiscItem.store.getCount() > 1)
                               && (App.grdDiscCustCate.store.getCount() > 1
                                   || App.grdDiscCustCate.store.getCount() == 1 && App.grdDiscCustCate.store.data.items[0].data.CustCateID != '')
                               ) {
                                hasData = true;
                            }
                            else {
                                HQ.message.show(1000, HQ.common.getLang('DPCL') + ' / ' + HQ.common.getLang('DPII'), '');
                                hasData = false;
                            }
                        }
                        
                        else {
                            hasData = false;
                        }
                    }
                    else {
                        HQ.message.show(1000, HQ.common.getLang('AppComp'), '');
                        hasData = false;
                    }
                }
                else {
                    if (App.cboBreakBy.getValue() == 'Q')
                        HQ.message.show(2018070201, [HQ.common.getLang('DiscBreak'), HQ.common.getLang('BreakQty')], '', true);
                    else
                        HQ.message.show(2018070201, [HQ.common.getLang('DiscBreak'), HQ.common.getLang('BreakAmt')], '', true);
                    hasData = false;
                }
            }
            else {
                hasData = true;
            }

            return hasData;
        },

        checkExistRequiredValue: function(){
            if (App.cboRequiredType.getValue() == 'Q' || App.cboRequiredType.getValue() == 'N' || App.cboRequiredType.getValue() == 'A') {
                var hasData = false;
                App.grdDiscItem.store.data.each(function (item) {                    
                    if (!Ext.isEmpty(item.data.InvtID) && item.data.RequiredValue > 0) {
                        hasData = true;
                        return false;                        
                    }                
                });
                if (!hasData) {
                    App.tabMain.setActiveTab(1);
                    HQ.message.show(2018011101, [HQ.grid.findColumnNameByIndex(App.grdDiscItem.columns , 'RequiredValue'), HQ.common.getLang('DPII')],'', true);
                    return false;
                }
            }
            return true;
        },       
        getChangedFilteredData: function (store) {
            var data = store.data,
                changedData

            store.data = store.snapshot; // does the trick
            changedData = store.getChangedData();
            store.data = data; // to revert the changes back
            return Ext.encode(changedData);
        },

        saveData: function () {
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (App.frmMain.isValid()) { //if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    //var discId = App.cboDiscID.getValue();
                    //var discSeq = App.cboDiscSeq.getValue();
                    //var status = App.cboStatus.value;
                    var line = '';
                    //if (status == _holdStatus) {
                    for (var i = 0; i < App.stoDiscBreak.data.length; i++) {
                        if (App.stoDiscBreak.data.items[i].data.BreakQty != 0 || App.stoDiscBreak.data.items[i].data.BreakAmt != 0) {
                            if (App.stoDiscBreak.data.items[i].data.SubBreakQty != 0 && App.stoSubBreakItem.data.items[i].data.InvtID == '') {
                                HQ.message.show(2018062901, [HQ.common.getLang('SubBreakItem')], '', true);
                                return false;
                            }
                            if (App.cboSubBreakType.getValue() == 'Q' && App.stoDiscBreak.data.items[i].data.SubBreakQty == 0) {
                                HQ.message.show(2018062902, [HQ.common.getLang('SubBreakQty'), HQ.common.getLang('DiscBreak')], '', true);//Khi chọn "Điều kiện 2" <> "Bình Thường" => buộc phải nhập field "SL or Số Tiền Cho Điều Kiện 2" > 0 ở tab Điều Kiện
                                return false;
                            }
                            if (App.cboSubBreakType.getValue() == 'A' && App.stoDiscBreak.data.items[i].data.SubBreakAmt == 0) {
                                HQ.message.show(2018062902, [HQ.common.getLang('SubBreakAmt'), HQ.common.getLang('DiscBreak')], '', true);//Khi chọn "Điều kiện 2" <> "Bình Thường" => buộc phải nhập field "SL or Số Tiền Cho Điều Kiện 2" > 0 ở tab Điều Kiện
                                return false;
                            }
                            if (App.cboSubBreakType.getValue() != 'N' && App.cboBreakBoundType.getValue() != 'L') {
                                if (App.stoDiscBreak.data.items[i].data.SubBreakQty != 0 && App.stoDiscBreak.data.items[i].data.SubBreakQtyUpper != 0
                                 && (App.stoDiscBreak.data.items[i].data.SubBreakQty > App.stoDiscBreak.data.items[i].data.SubBreakQtyUpper)) {
                                    HQ.message.show(2018062905);
                                    return false;
                                }
                                if (App.stoDiscBreak.data.items[i].data.SubBreakAmt != 0 && App.stoDiscBreak.data.items[i].data.SubBreakAmtUpper != 0
                                && (App.stoDiscBreak.data.items[i].data.SubBreakAmt > App.stoDiscBreak.data.items[i].data.SubBreakAmtUpper)) {
                                    HQ.message.show(2018062906);
                                    return false;
                                }
                            }
                            if (App.cboBreakBoundType.getValue() != 'L') {
                                if (App.stoDiscBreak.data.items[i].data.BreakQty != 0 && App.stoDiscBreak.data.items[i].data.BreakQtyUpper != 0
                                   && (App.stoDiscBreak.data.items[i].data.BreakQty > App.stoDiscBreak.data.items[i].data.BreakQtyUpper)) {
                                    HQ.message.show(2018062903);
                                    return false;
                                }
                                if (App.stoDiscBreak.data.items[i].data.BreakAmt != 0 && App.stoDiscBreak.data.items[i].data.BreakAmtUpper != 0
                                   && (App.stoDiscBreak.data.items[i].data.BreakAmt > App.stoDiscBreak.data.items[i].data.BreakAmtUpper)) {
                                    HQ.message.show(2018062904);
                                    return false;
                                }
                            }
                            if(App.chkConvertDiscAmtToFreeItem.getValue() == true)
                            {
                                if(App.stoDiscBreak.data.items[i].data.DiscAmt == 0)
                                {
                                    HQ.message.show(2018070202, [HQ.common.getLang("DiscBreak"), HQ.common.getLang("Discount")], '', true);
                                    return false;
                                }
                            }
                        }
                            //if(App.stoDiscBreak.data.items[i].data.BreakAmt!=0 || App.stoDiscBreak.data.items[i].data.BreakQty!=0)
                            //{
                            //    if(App.stoDiscBreak.data.items[i].data.DiscAmtBonus==0)
                            //    {
                            //        line += (i + 1) + ", ";
                            //    }
                            //}
                    }
                    //if (line != '') {
                    //    HQ.message.show(2018072360, [line, HQ.common.getLang("Const"), HQ.common.getLang("DiscBreak")], '', true);
                    //    return false;
                    //}
                    if (App.chkConvertDiscAmtToFreeItem.getValue() == true)
                    {
                        var flat = false;
                        App.stoDiscBreak.data.each(function (item) {
                            if (item.data.BreakQty != 0 || item.data.BreakAmt != 0) {
                                var obj = HQ.store.findInStore(App.stoFreeItem, ['DiscID', 'DiscSeq', 'LineRef'], [item.data.DiscID, item.data.DiscSeq, item.data.LineRef]);
                                if (obj == '' || obj == undefined || obj.FreeItemID == '')
                                    flat = true;
                            }                            
                        });
                        if (flat) {
                            HQ.message.show(2018070301, [HQ.common.getLang("FreeItemList")], '', true);//ConvertDiscAmtToFreeItem = true lưới FreeItem phải có chi tiết
                            return false;
                        }
                       
                    }
                    if (App.cboDiscID.valueModels == null && !HQ.allowAddDiscount) {
                        HQ.message.show(2017113005, '', '');
                    }
                    else {
                        if (Main.Process.checkHasData()) {
                            if (Main.Process.checkEntireRequire()) {
                                if (Main.Process.checkExistRequiredValue()) {
                                    App.frmDiscDefintionTop.updateRecord();
                                    App.frmDiscSeqInfo.updateRecord();
                                    App.frmMain.submit({
                                        waitMsg: HQ.common.getLang("SavingData"),
                                        url: 'OM21100/SaveData',
                                        timeout: 10000000,
                                        params: {
                                            isAddDiscount: HQ.allowAddDiscount,
                                            isNewDiscID: _isNewDisc,
                                            isNewDiscSeq: _isNewSeq,
                                            lstDiscInfo: Ext.encode([App.frmDiscDefintionTop.getRecord().data]),
                                            lstDiscBreakChange: HQ.store.getData(App.grdDiscBreak.store),
                                            lstFreeItemChange: Main.Process.getChangedFilteredData(App.grdFreeItem.store),
                                            lstCompanyChange: HQ.store.getData(App.grdCompany.store),
                                            lstDiscItemChange: HQ.store.getData(App.grdDiscItem.store),
                                            lstBundleChange: HQ.store.getData(App.grdBundle.store),
                                            lstDiscCustClassChange: HQ.store.getData(App.grdDiscCustClass.store),
                                            lstDiscCustChange: HQ.store.getData(App.grdDiscCust.store),
                                            lstDiscItemClassChange: HQ.store.getData(App.grdDiscItemClass.store),

                                            lstDiscSeqInfo: (function () {
                                                App.stoDiscSeqInfo.each(function (item) {
                                                    item.data.Active = App.chkActive.value ? 1 : 0;
                                                    item.data.Promo = App.chkDiscTerm.value ? 1 : 0;
                                                });
                                                return Ext.encode(App.stoDiscSeqInfo.getRecordsValues());
                                            })(),
                                            lstDiscBreak: Ext.encode(App.grdDiscBreak.store.getRecordsValues()), // data
                                            lstFreeItem: Ext.encode(Main.Process.getRecordValues(App.grdFreeItem.store.snapshot.getRange())), // record.data
                                            lstCompany: Ext.encode(App.grdCompany.store.getRecordsValues()),
                                            lstDiscItem: Ext.encode(App.grdDiscItem.store.getRecordsValues()),
                                            lstBundle: Ext.encode(App.grdBundle.store.getRecordsValues()),
                                            lstDiscCustClass: Ext.encode(App.grdDiscCustClass.store.getRecordsValues()),
                                            lstDiscCust: Ext.encode(App.grdDiscCust.store.getRecordsValues()),
                                            lstDiscItemClass: Ext.encode(App.grdDiscItemClass.store.getRecordsValues()),
                                            lstDiscCustCate: Ext.encode(App.grdDiscCustCate.store.getRecordsValues()),
                                            lstDiscChannel: Ext.encode(App.grdDiscChannel.store.getRecordsValues()),
                                            lstDiscCustCateChange: HQ.store.getData(App.grdDiscCustCate.store),
                                            lstDiscChannelChange: HQ.store.getData(App.grdDiscChannel.store),
                                            lstDiscSubBreakItem: HQ.store.getData(App.grdSubBreakItem.store)
                                        },
                                        success: function (msg, data) {
                                            if (data.result.msgCode) {
                                                HQ.message.show(data.result.msgCode);
                                            }
                                            else {
                                                HQ.message.show(201405071);
                                            }
                                            Main.Process.reloadAllData();
                                        },
                                        failure: function (msg, data) {
                                            if (data.result.msgCode) {
                                                HQ.message.show(data.result.msgCode);
                                            }
                                            else {
                                                HQ.message.process(msg, data, true);
                                            }
                                        }
                                    });
                                }
                            }
                        }

                    }
                    //}
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
        },

        getRecordValues: function (recordRange) {
            var values = [];
            recordRange.forEach(function (record) {
                values.push(record.data);
            });
            return values;
        },

        refresh: function (item) {
            if (item == 'yes') {
                HQ.isChange = false;
                HQ.isChange0 = false;
                //App.stoDiscSeqInfo.reload();
                Main.Event.menuClick("refresh");
            }
        },

        deleteDisc: function (item) {
            if (item == 'yes') {
                if (HQ.isDelete) {
                    App.frmMain.submit({
                        waitMsg: HQ.common.getLang("DeletingData"),
                        url: 'OM21100/DeleteDisc',
                        clientValidation: false,
                        timeout: 10000000,
                        params: {
                            discID: App.cboDiscID.getValue()
                        },
                        success: function (msg, data) {
                            if (data.result.msgCode) {
                                HQ.message.show(data.result.msgCode);
                            }
                            else {
                                HQ.message.show(201405071);
                            }
                            Main.Process.reloadAllData();
                        },
                        failure: function (msg, data) {
                            if (data.result.msgCode) {
                                HQ.message.show(data.result.msgCode);
                            }
                            else {
                                HQ.message.process(msg, data, true);
                            }
                        }
                    });
                }
                else {
                    HQ.message.show(4, '', '');
                }
            }
        },

        deleteDiscSeq: function (item) {
            if (item == 'yes') {
                if (HQ.isDelete) {
                    App.frmMain.submit({
                        waitMsg: HQ.common.getLang("DeletingData"),
                        url: 'OM21100/DeleteDiscSeq',
                        clientValidation: false,
                        timeout: 10000000,
                        params: {
                            discID: App.cboDiscID.getValue(),
                            discSeq: App.cboDiscSeq.getValue()
                        },
                        success: function (msg, data) {
                            if (data.result.msgCode) {
                                HQ.message.show(data.result.msgCode);
                            }
                            else {
                                App.cboDiscSeq.setValue('');
                                HQ.message.show(201405071);
                            }
                            App.cboDiscSeq.store.load();
                            App.stoDiscSeqInfo.reload();
                        },
                        failure: function (msg, data) {
                            if (data.result.msgCode) {
                                HQ.message.show(data.result.msgCode);
                            }
                            else {
                                HQ.message.process(msg, data, true);
                            }
                        }
                    });
                }
                else {
                    HQ.message.show(4, '', '');
                }
            }
        },

        checkAddFreeItem: function () {
            var isAllowInsert = true;
            //for (var i = 0; i < App.grdDiscBreak.store.data.length; i++) {
                if (App.grdDiscBreak.selModel.selected.items[0].data.DiscAmt) {
                    isAllowInsert = false;
                  //  break;
                }
           // }
            if (!isAllowInsert || App.grdDiscBreak.store.data.length == 0) {
                return false;
            }
            return true;
        }
    },

    Event: {
        frmMain_boxReady: function (frm, width, height, eOpts) {
            App.btnExport1.setVisible(HQ.allowExport);
            App.btnImport.setVisible(HQ.allowImport);
            App.cboDiscID.forceSelection = !HQ.allowAddDiscount;
            App.stoDiscInfo.reload();
            App.stoDiscSeqInfo.reload();
            if (!HQ.hideQtyType) {
                HQ.grid.show(App.grdDiscItem, ['QtyType']);
            }
            if (App.chkPctDiscountByLevel != undefined) {
                App.chkPctDiscountByLevel.disable();
            }
            //App.cboProrateAmtType.setValue("L");
            HQ.common.setRequire(App.frmMain);
            //App.chkRequiredType.setVisible(HQ.showRequiredType);
            App.cboRequiredType.setVisible(HQ.showRequiredType);
            if (!HQ.hidechkPctDiscountByLevel) {
                App.chkPctDiscountByLevel.hide();
                App.cboProrateAmtType.hide();
            }
            if (!HQ.hidechkStockPromotion) {
                App.chkStockPromotion.hide();
            }
            App.colDiscAmtBonus.setVisible(HQ.hideDiscAmtBonus);
            App.cboInvtIDSolomon.setVisible(HQ.hideInvtIDSolomon);
            App.chkDiscPrice.setVisible(HQ.hideDiscPrice);
            App.cboDiscSeqSolomon.setVisible(HQ.hideDiscSeqSolomon);
            App.btnCopy.setVisible(HQ.hideCopy);
            App.FreeITemSiteWhseLoc.setVisible(HQ.hideSite);
            App.FreeITemSiteID.setVisible(HQ.hideSite);
            App.cboSolomonSite.setVisible(HQ.hideSiteSolomon);
        },

        frmMain_fieldChange: function (frm, field, newValue, oldValue, eOpts) {
            var disc = App.frmDiscDefintionTop.getRecord();
            if (disc) {
                App.frmDiscDefintionTop.updateRecord();
                HQ.isChange0 = disc.dirty;

                var discSeq = App.frmDiscSeqInfo.getRecord();
                if (discSeq) {
                    App.frmDiscSeqInfo.updateRecord();
                    if (!discSeq.dirty) {
                        if (!HQ.store.isChange(App.grdDiscBreak.store)) {
                            if (!HQ.store.isChange(App.grdFreeItem.store)) {
                                if (!HQ.store.isChange(App.grdCompany.store)) {
                                    if (!HQ.store.isChange(App.grdDiscItem.store)) {
                                        if (!HQ.store.isChange(App.grdBundle.store)) {
                                            if (!HQ.store.isChange(App.grdDiscCustClass.store)) {
                                                if (!HQ.store.isChange(App.grdDiscCust.store)) {
                                                    HQ.isChange = HQ.store.isChange(App.grdDiscItemClass.store);
                                                }
                                                else {
                                                    HQ.isChange = true;
                                                }
                                            }
                                            else {
                                                HQ.isChange = true;
                                            }
                                        }
                                        else {
                                            HQ.isChange = true;
                                        }
                                    }
                                    else {
                                        HQ.isChange = true;
                                    }
                                }
                                else {
                                    HQ.isChange = true;
                                }
                            }
                            else {
                                HQ.isChange = true;
                            }
                        }
                        else {
                            HQ.isChange = true;
                        }
                    }
                    else {
                        HQ.isChange = true;
                    }
                }
                if (HQ.isChange || HQ.isChange0) {
                    HQ.common.changeData(true, 'OM21100');//co thay doi du lieu gan * tren tab title header
                    //HQ.form.lockButtonChange(HQ.isChange, App);//lock lai cac nut khi co thay doi du lieu
                }
                else {
                    HQ.common.changeData(false, 'OM21100');
                }
                App.cboDiscID.setReadOnly(HQ.isChange);
                App.cboDiscSeq.setReadOnly(HQ.isChange);               

                if (HQ.isChange && !App.cboDiscClass.readOnly || !Ext.isEmpty(App.cboDiscSeq.getValue())) {
                    App.cboDiscClass.setReadOnly(true);
                    App.cboDiscType.setReadOnly(true);
                } else if (!HQ.isChange && App.cboDiscSeq.store.data.length == 0) {
                    App.cboDiscClass.setReadOnly(false);
                    App.cboDiscType.setReadOnly(false);
                }
            }
        },

        sto_load: function (sto, records, successful, eOpts) {
            if (HQ.isUpdate) {
                var discId = App.cboDiscID.getValue();
                var discSeq = App.cboDiscSeq.getValue();
                var status = App.cboStatus.getValue();
                var keys = sto.HQFieldKeys ? sto.HQFieldKeys : "";
                var idxLref = keys.indexOf("LineRef");
                
                if (discId && discSeq && status == _holdStatus) {
                    if (successful) {
                        var newData = {
                            DiscID: discId,
                            DiscSeq: discSeq
                        };

                        if (idxLref != -1) {
                            newData.LineRef = HQ.store.lastLineRef(sto);
                            keys.splice(idxLref, 1);
                        }
                        if (sto.storeId == 'stoDiscCust' || sto.storeId == 'stoBundle') {// || sto.storeId == 'stoDiscItem'
                            if (sto.data.length < sto.pageSize ||
                                sto.currentPage == (sto.totalCount / sto.pageSize)
                                ) {
                                var obj = HQ.store.findRecord(sto, keys, [discId, discSeq, '', '', '', '', '']);
                                if (!obj) {
                                    var newRec = Ext.create(sto.model.modelName, newData);
                                    HQ.store.insertRecord(sto, keys, newRec, false);
                                }
                            }
                        }
                        else if (sto.storeId == 'stoDiscItem') {
                            newData.PerStockAdvance = 100;
                            if (sto.data.length < sto.pageSize ||
                                sto.currentPage == (sto.totalCount / sto.pageSize)
                                ) {
                                var obj = HQ.store.findRecord(sto, keys, [discId, discSeq, '', '', '', '', '','','']);
                                if (!obj) {
                                    var newRec = Ext.create(sto.model.modelName, newData);
                                    HQ.store.insertRecord(sto, keys, newRec, false);
                                }
                            }
                        }
                        else if (sto.storeId == 'stoDiscBreak') {
                            var obj = HQ.store.findRecord(sto, ["DiscID", "DiscSeq", "LineRef"], [discId, discSeq, '']);
                            if (!obj) {
                                var newRec = Ext.create(sto.model.modelName, newData);
                                HQ.store.insertRecord(sto, keys, newRec, false);
                            }
                        } else {
                            var obj = HQ.store.findRecord(sto, keys, [discId, discSeq, '', '', '', '', '']);
                            if (!obj) {
                                var newRec = Ext.create(sto.model.modelName, newData);
                                HQ.store.insertRecord(sto, keys, newRec, false);
                            }

                        }
                    }
                    
                }
                
            }
           
        },


        stoFreeItem_Load: function (sto, records, successful, eOpts) {
            var lstdata = App.stoFreeItem.data;
            if (!App.chkDonateGroupProduct.getValue()) {
                if (lstdata) {
                    if (lstdata.items.length > 0) {
                        if (lstdata.items[0].data.FreeItemID != "" && lstdata.items[0].data.FreeItemID != null) {
                            App.chkDonateGroupProduct.disable();
                        }
                        else {
                            App.chkDonateGroupProduct.enable();
                        }
                    }
                    else {
                        App.chkDonateGroupProduct.enable();
                        App.chkConvertDiscAmtToFreeItem.enable();
                    }
                }
                else {
                    App.chkDonateGroupProduct.enable();
                }
                if (App.chkAutoFreeItem.getValue()) {
                    App.chkDonateGroupProduct.disable();
                }
                else {
                    var check = true;
                    var lstDataFree = App.stoFreeItem.snapshot || App.stoFreeItem.allData || App.stoFreeItem.data;
                    for (var i = 0; i < lstDataFree.items.length; i++) {
                        if (lstDataFree.items[i].data.FreeItemID != "" && lstDataFree.items[i].data.FreeItemID != null) {
                            check = false;
                        }
                    }
                    if (check) {
                        App.chkDonateGroupProduct.enable();
                        App.chkConvertDiscAmtToFreeItem.enable();
                    }
                    else {
                        App.chkDonateGroupProduct.disable();
                    }

                }
            }
            else {
                if (!App.chkAutoFreeItem.getValue()) {
                    App.chkDonateGroupProduct.enable();
                }                
            }
        },

        stoDiscItem_load: function (sto, records, successful, eOpts) {
            if (HQ.isUpdate) {
                var discId = App.cboDiscID.getValue();
                var discSeq = App.cboDiscSeq.getValue();
                var status = App.cboStatus.getValue();
                var keys = sto.HQFieldKeys ? sto.HQFieldKeys : "";
                var idxLref = keys.indexOf("LineRef");

                if (discId && discSeq && status == _holdStatus) {
                    if (successful) {
                        var newData = {
                            DiscID: discId,
                            DiscSeq: discSeq,
                            CoefficientCnv: 1
                        };

                        if (idxLref != -1) {
                            newData.LineRef = HQ.store.lastLineRef(sto);
                            keys.splice(idxLref, 1);
                        }
                        newData.PerStockAdvance = 100;
                        if (sto.data.length < sto.pageSize ||sto.currentPage == (sto.totalCount / sto.pageSize)) {
                            var obj = HQ.store.findRecord(sto, keys, [discId, discSeq, '', '', '', '', '', '', '']);
                            if (!obj) {
                                var newRec = Ext.create(sto.model.modelName, newData);
                                HQ.store.insertRecord(sto, keys, newRec, false);
                            }
                        }                       
                        
                    }

                }
                var lstdata = App.stoDiscItem.data;
                if (lstdata) {
                    if (lstdata.items.length > 0) {
                        if (lstdata.items[0].data.InvtID != "" && lstdata.items[0].data.InvtID != null) {
                            App.chkStockPromotion.setReadOnly(true);
                        }
                        else {
                            App.chkStockPromotion.setReadOnly(false);
                        }
                    }
                    else {
                        App.chkStockPromotion.setReadOnly(false);
                    }
                }
                else {
                    App.chkStockPromotion.setReadOnly(false);
                }                
            }
            
        },

        stoSubBreakItem_Load: function (sto, records, successful, eOpts) {
            var record = HQ.store.findInStore(sto, ['InvtID'], ['']);
            if (!record) {
                var subBreakItem = Ext.create("App.mdlSubBreakItem", {
                    InvtID: '',
                    Descr: '',
                    UnitDesc: ''
                });
                sto.insert(0, subBreakItem);
            }
        },

        grd_beforeEdit: function (editor, e) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var status = App.cboStatus.value;
                    if (status == _holdStatus) {
                        var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

                        if (keys.indexOf(e.field) != -1) {
                            if (e.record.data.tstamp)
                                return false;
                        }
                        if (e.store.storeId == 'stoBundle') {
                            if ((e.field == 'BundleQty' && App.cboBreakBy.getValue() != 'Q')
                                || (e.field == 'BundleAmt' && App.cboBreakBy.getValue() != 'A')) {
                                return false;
                            }
                        }
                        if (e.field == 'UnitDesc') {
                            if (e.store.storeId == 'stoDiscItem') {
                                App.cboGItemUnitDescr.store.reload();
                            }
                            else if (e.store.storeId == 'stoBundle') {
                                App.cboGInvtUnitDescr.store.reload();
                            }
                            else if (e.store.storeId == 'stoFreeItem') {
                                App.cboGUnitDescr.store.reload();
                            }
                            else if (e.store.storeId == 'stoDiscItemClass') {
                                App.cboGClassUnitDescr.store.reload();
                            }
                        }
                        if (e.field == 'PriorityInvt')
                        {
                            if (e.store.storeId == 'stoDiscItem' && !App.chkIsDeductQtyAmt.getValue()) {
                                return false;
                            }
                        }
                        return HQ.grid.checkInput(e, keys);
                    }
                    else {
                        return false;
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                    return false;
                }
            }
            else {
                return false;
            }
        },

        grdSubBreakItem_beforeEdit: function (editor, e) {
            if (App.cboStatus.getValue() != _holdStatus) return false;
            if (HQ.isUpdate == false && !HQ.isNew) {
                return false;
            }
            if (e.field == 'UnitDesc')
                App.cboSubBreakItemUnitDescr.store.reload();
            return HQ.grid.checkBeforeEdit(e, ['InvtID']);
        },

        grd_edit: function (item, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

            if (keys.indexOf(e.field) != -1) {
                if (e.value != ''
                    && Main.Process.isAllValidKey(e.store.getChangedData().Created, keys)
                    && Main.Process.isAllValidKey(e.store.getChangedData().Updated, keys)) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var idxLref = keys.indexOf("LineRef")

                    var newData = {
                        DiscID: discId,
                        DiscSeq: discSeq
                    };

                    if (idxLref != -1) {
                        newData.LineRef = HQ.store.lastLineRef(e.store);
                        keys.splice(idxLref, 1);
                    }

                    var newRec = Ext.create(e.store.model.modelName, newData);
                    HQ.store.insertRecord(e.store, keys, newRec, false);

                    if (!App.cboDiscClass.readOnly) {
                        App.cboDiscClass.setReadOnly(true);
                        App.cboDiscType.setReadOnly(true);
                        
                    }
                }
                if (e.store.storeId == 'stoDiscCustCate') {
                    if (e.field == 'CustCateID') {
                        var record = HQ.store.findRecord(App.cboCustCate.store, ['Code'], [e.value]);
                        if (record) {
                            e.record.set('Descr', record.data.Descr);
                        } else {
                            e.record.set('Descr', '');
                        }
                    }
                    
                } else if (e.store.storeId == 'stoDiscChannel') {
                    if (e.field == 'ChannelID') {
                        var record = HQ.store.findRecord(App.cboChannel.store, ['Code'], [e.value]);
                        if (record) {
                            e.record.set('Descr', record.data.Descr);
                        } else {
                            e.record.set('Descr', '');
                        }
                    }
                    
                }
            }
        },


        grdDiscItem_edit: function (item, e, oldvalue) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";
            if (e.field == "PerStockAdvance" || e.field == "QtyLimit") {
                var tam = 0;
                tam = Math.floor((e.record.data.QtyLimit * e.record.data.PerStockAdvance) / 100);
                e.record.set("QtyStockAdvance", tam);
            }

            if (keys.indexOf(e.field) != -1) {
                if (e.value != ''
                    && Main.Process.isAllValidKey(e.store.getChangedData().Created, keys)
                    && Main.Process.isAllValidKey(e.store.getChangedData().Updated, keys)) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var idxLref = keys.indexOf("LineRef")

                    var newData = {
                        DiscID: discId,
                        DiscSeq: discSeq,
                        PerStockAdvance: 100,
                        CoefficientCnv: 1
                    };
                    if (idxLref != -1) {
                        newData.LineRef = HQ.store.lastLineRef(e.store);
                        keys.splice(idxLref, 1);
                    }

                    var newRec = Ext.create(e.store.model.modelName, newData);
                    HQ.store.insertRecord(e.store, keys, newRec, false);

                    if (!App.cboDiscClass.readOnly) {
                        App.cboDiscClass.setReadOnly(true);
                        App.cboDiscType.setReadOnly(true);
                    }
                }                
            }
            if (e.field == "InvtID") {
                var rec = App.cboDpiiInvtID.store.findRecord("InvtID", e.value);
                if (rec) {                    
                        App.chkStockPromotion.setReadOnly(true);
                }
            }
            
        },

        grdSubBreakItem_edit: function (item, e, oldvalue) {
            if (e.field == 'InvtID')
            {
                var objInvtID = HQ.store.findInStore(App.cboInvtIDSubBreakItem.store, ['InvtID'], [e.value]);
                if(objInvtID != undefined)
                {
                    e.record.set('Descr', objInvtID.Descr);
                    e.record.set('UnitDesc', objInvtID.StkUnit);
                }
            }
            HQ.grid.checkInsertKey(App.grdSubBreakItem, e, ['InvtID']);
        },

        grd_validateEdit: function (item, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

            if (keys.indexOf(e.field) != -1) {
                //var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/
                //if (e.value && !e.value.match(regex)) {
                //    HQ.message.show(20140811, e.column.text);
                //    return false;
                //}
                if (e.store.storeId == 'stoFreeItem') {
                    keys = ['DiscID', 'DiscSeq', 'LineRef', 'FreeItemID'];
                }
                if (e.store.storeId == 'stoDiscCust') {
                    e.record.set('BranchID', _selBranchID);
                    if (HQ.grid.checkDuplicate(e.grid, e, keys)) {
                        e.record.set('BranchID', '');                        
                        HQ.message.show(1112, e.value);
                        return false;
                    }
                    e.record.set('TerritoryName', _selTerritory);
                } else {
                    if (HQ.grid.checkDuplicate(e.grid, e, keys)) {
                        HQ.message.show(1112, e.value);
                        return false;
                    }
                }
                if (HQ.grid.checkDuplicate(e.grid, e, keys)) {
                    HQ.message.show(1112, e.value);
                    return false;
                }

            }
        },

        grdFreeItem_validateEdit: function (item, e, isCheckSpecialChar) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

            if (keys.indexOf(e.field) != -1) {
                //var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/
                //if (e.value && !e.value.match(regex)) {
                //    HQ.message.show(20140811, e.column.text);
                //    return false;
                //}
                if (e.store.storeId == 'stoFreeItem') {
                    keys = ['DiscID', 'DiscSeq', 'LineRef', 'FreeItemID'];
                }                
                if (HQ.grid.checkDuplicate(e.grid, e, keys)) {
                    HQ.message.show(1112, e.value);
                    return false;
                }

            }
            //if (e.field == "GroupItem") {
            //    //var regex = / ^(\w*(\d|[a-zA-Z]))[\_\-\(\)]*$/
            //    if (isCheckSpecialChar == undefined) isCheckSpecialChar = true;
            //    if (isCheckSpecialChar) {
            //        if (e.value)
            //            if (!HQ.util.passNull(e.value) == '' && !HQ.util.passNull(e.value.toString()).match(regex)) {
            //                HQ.message.show(2018032517, e.column.text);
            //                return false;
            //            }
            //    }
            //}            
        },

        grdDiscItem_validateEdit: function (item, e) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

            //if (e.field == "PerStockAdvance" || e.field == "QtyLimit") {
            //    if (e.field == "PerStockAdvance" && (e.value == 0 || e.value == null || e.value == "")) {
            //        HQ.message.show(2018032011, e.column.text);
            //        return false;
            //    }
            //}

            if (e.field == "QtyLimit") {
                if (e.field == "QtyLimit" && (e.value == 0 || e.value == null || e.value == "")) {
                    HQ.message.show(2018032011, e.column.text);
                    return false;
                }
            }
            if (e.field == 'PriorityInvt')
            {
                if (HQ.grid.checkDuplicate(e.grid, e, ['PriorityInvt'])) {
                    HQ.message.show(1112, e.value);
                    return false;
                }
            }
            if (keys.indexOf(e.field) != -1) {
                //var regex = /^(\w*(\d|[a-zA-Z]))[\_]*$/
                //if (e.value && !e.value.match(regex)) {
                //    HQ.message.show(20140811, e.column.text);
                //    return false;
                //}
                if (e.store.storeId == 'stoFreeItem') {
                    keys = ['DiscID', 'DiscSeq', 'LineRef', 'FreeItemID'];
                }
                if (e.store.storeId == 'stoDiscCust') {
                    e.record.set('BranchID', _selBranchID);
                    if (HQ.grid.checkDuplicate(e.grid, e, keys)) {
                        e.record.set('BranchID', '');
                        HQ.message.show(1112, e.value);
                        return false;
                    }
                    e.record.set('TerritoryName', _selTerritory);
                } else {
                    if (HQ.grid.checkDuplicate(e.grid, e, keys)) {
                        HQ.message.show(1112, e.value);
                        return false;
                    }
                }
                if (HQ.grid.checkDuplicate(e.grid, e, keys)) {
                    HQ.message.show(1112, e.value);
                    return false;
                }

            }
        },      

        grdSubBreakItem_validateEdit: function (item, e) {
            return HQ.grid.checkValidateEdit(App.grdSubBreakItem, e, ['InvtID']);
        },

        grd_reject: function (col, record) {
            var store = record.store;

            if (record.data.tstamp == '') {
                store.remove(record);
                col.grid.getView().focusRow(store.getCount() - 1);
                col.grid.getSelectionModel().select(store.getCount() - 1);
            } else {
                record.reject();
            }
        },

        grdDiscItem_reject: function (col, record) {
            var store = record.store;

            if (record.data.tstamp == '') {
                store.remove(record);
                col.grid.getView().focusRow(store.getCount() - 1);
                col.grid.getSelectionModel().select(store.getCount() - 1);
            } else {
                record.reject();
            }

            var lstAlldata = App.stoDiscItem.allData;//.snapshot;
            var check = true;
            if (lstAlldata.items.length > 0) {
                for (var i = 0; i < lstAlldata.items.length; i++) {
                    if (lstAlldata.items[i].data.InvtID != "") {///&& (lstAlldata.items[i].data.GroupItem != "" || lstAlldata.items[i].data.Priority > 0)
                        check = false;
                    }
                }
            }
            if(App.cboStatus.getValue()!="C"){
                App.chkStockPromotion.setReadOnly(!check);
            }
            
        },

        grdFreeItem_reject: function (col, record) {
            var store = record.store;

            if (record.data.tstamp == '') {
                store.remove(record);
                col.grid.getView().focusRow(store.getCount() - 1);
                col.grid.getSelectionModel().select(store.getCount() - 1);
            } else {
                record.reject();
            }
            if (App.chkDonateGroupProduct.getValue()) {
                var lstAlldata = App.stoFreeItem.snapshot;
                var check = true;
                if (lstAlldata.items.length > 0) {
                    for (var i = 0; i < lstAlldata.items.length; i++) {
                        if (lstAlldata.items[i].data.FreeItemID != "") {///&& (lstAlldata.items[i].data.GroupItem != "" || lstAlldata.items[i].data.Priority > 0)
                            check = false;
                        }
                    }
                }
                App.chkDonateGroupProduct.setReadOnly(!check);
            }
            else {
                if (!App.chkAutoFreeItem.getValue()) {
                    var lstAlldata = App.stoFreeItem.snapshot;
                    var check = true;
                    if (lstAlldata.items.length > 0) {
                        for (var i = 0; i < lstAlldata.items.length; i++) {
                            if (lstAlldata.items[i].data.FreeItemID != "") {// && (lstAlldata.items[i].data.GroupItem != "" || lstAlldata.items[i].data.Priority > 0)
                                check = false;
                            }
                        }
                    }
                    if (check) {
                        App.chkDonateGroupProduct.enable();
                    }
                }
                if (!App.chkDonateGroupProduct.getValue()) {
                    var lstAlldata = App.stoFreeItem.snapshot;
                    var check = true;
                    if (lstAlldata.items.length > 0) {
                        for (var i = 0; i < lstAlldata.items.length; i++) {
                            if (lstAlldata.items[i].data.FreeItemID != "" && (lstAlldata.items[i].data.GroupItem != "" || lstAlldata.items[i].data.Priority > 0)) {
                                check = false;
                            }
                        }
                    }
                    if (check) {
                        App.chkAutoFreeItem.enable();
                    }
                }
            }

        },

        grdSubBreakItem_reject: function (col, record) {
            HQ.grid.checkReject(record, App.grdSubBreakItem);
        },

        menuClick: function (command) {
            var focusGrid;
            var title = "Data";
            if (App[HQ.focus]) {
                if (App[HQ.focus].xtype == "grid") {
                    focusGrid = App[HQ.focus];
                    title = focusGrid.title;
                }
                else {
                    var tmpGrds = App[HQ.focus].down("gridpanel");
                    if (tmpGrds) {
                        if (!tmpGrds.length) {
                            focusGrid = tmpGrds;
                            title = App[HQ.focus].title;
                        }
                    }
                }
            }

            switch (command) {
                case "first":
                    if (HQ.focus == 'frmDiscDefintionTop') {
                        HQ.combo.first(App.cboDiscID, HQ.isChange || HQ.isChange0);
                    }
                    else if (HQ.focus == 'frmDiscSeqInfo') {
                        HQ.combo.first(App.cboDiscSeq, HQ.isChange);
                    }
                    else if (focusGrid) {
                        HQ.grid.first(focusGrid);
                    }
                    break;
                case "prev":
                    if (HQ.focus == 'frmDiscDefintionTop') {
                        HQ.combo.prev(App.cboDiscID, HQ.isChange || HQ.isChange0);
                    }
                    else if (HQ.focus == 'frmDiscSeqInfo') {
                        HQ.combo.prev(App.cboDiscSeq, HQ.isChange);
                    }
                    else if (focusGrid) {
                        HQ.grid.prev(focusGrid);
                    }
                    break;
                case "next":
                    if (HQ.focus == 'frmDiscDefintionTop') {
                        HQ.combo.next(App.cboDiscID, HQ.isChange || HQ.isChange0);
                    }
                    else if (HQ.focus == 'frmDiscSeqInfo') {
                        HQ.combo.next(App.cboDiscSeq, HQ.isChange);
                    }
                    else if (focusGrid) {
                        HQ.grid.next(focusGrid);
                    }
                    break;
                case "last":
                    if (HQ.focus == 'frmDiscDefintionTop') {
                        HQ.combo.last(App.cboDiscID, HQ.isChange || HQ.isChange0);
                    }
                    else if (HQ.focus == 'frmDiscSeqInfo') {
                        HQ.combo.last(App.cboDiscSeq, HQ.isChange);
                    }
                    else if (focusGrid) {
                        HQ.grid.last(focusGrid);
                    }
                    break;
                case "refresh":
                    if (HQ.isChange || HQ.isChange0) {
                        HQ.message.show(20150303, '', 'Main.Process.refresh');
                    }
                    else {
                        App.btnUpload.reset();
                        if (HQ.focus == 'frmDiscDefintionTop') {
                            App.cboDiscID.store.load(function (records, operation, success) {
                                App.stoDiscInfo.reload();
                            });
                        }
                        else {//if (HQ.focus == 'frmDiscSeqInfo') {
                            App.cboDiscSeq.store.load(function (records, operation, success) {
                                App.stoDiscSeqInfo.reload();
                            });
                        }
                        if (focusGrid) {
                            focusGrid.store.reload();
                            HQ.grid.first(focusGrid);
                        }
                    }

                    break;
                case "new":

                    if (HQ.isInsert) {
                        if (HQ.isChange || HQ.isChange0) {
                            HQ.message.show(20150303, '', 'Main.Process.refresh');
                        }
                        else {
                            if (HQ.focus == 'frmDiscDefintionTop') {
                                if (HQ.allowAddDiscount) {
                                    App.cboDiscID.clearValue();
                                    App.GroupItem.hide();
                                    App.Priority.hide();
                                    App.colPerStockAdvance.hide();
                                    App.colQtyLimit.hide();
                                    App.colQtyStockAdvance.hide();
                                }
                                else {
                                    HQ.message.show(2017113005, '', '');
                                }
                                
                                
                            }
                            else if (HQ.focus == 'frmDiscSeqInfo') {
                                App.cboDiscSeq.clearValue();
                                App.GroupItem.hide();
                                App.Priority.hide();
                                App.colPerStockAdvance.hide();
                                App.colQtyLimit.hide();
                                App.colQtyStockAdvance.hide();
                            }
                        }
                    }
                    break;
                case "delete":

                    if (App.cboStatus.getValue() != "C") {
                        if (HQ.isUpdate) {
                            if (HQ.focus == 'frmDiscDefintionTop') {
                                // Xoa discount
                                HQ.message.show(11, '', 'Main.Process.deleteDisc', true);
                                //Main.Process.deleteDisc();
                            }
                            else if (HQ.focus == 'frmDiscSeqInfo') {
                                // Xoa disc seq
                                HQ.message.show(11, '', 'Main.Process.deleteDiscSeq', true);
                                // Main.Process.deleteDiscSeq();
                            }
                            else if (focusGrid) {
                                var selected = focusGrid.getSelectionModel().selected.items;
                                if (selected.length > 0) {
                                    _gridForDel = focusGrid;

                                    if (HQ.focus == 'grdDiscItem') {
                                        HQ.message.show(2015020807, DiscDefintion.Process.indexSelect(App.grdDiscItem), 'Main.Process.deleteSelectedIngrdDiscItem');
                                    }else if (HQ.focus == 'grdFreeItem') {
                                        HQ.message.show(2015020807, DiscDefintion.Process.indexSelect(App.grdFreeItem), 'Main.Process.deleteSelectedInGridFreeItem');
                                    } else {
                                        if (selected[0].index != undefined) {
                                            var rowIdx = '';
                                            for (var i = 0; i < selected.length; i++) {
                                                rowIdx += (focusGrid.getSelectionModel().selected.items[i].index + 1) + ' & ';
                                            }
                                            var params = rowIdx.length > 3 ? rowIdx.substring(0, rowIdx.length - 3) : '';// + title;
                                            HQ.message.show(2015020807, params, 'Main.Process.deleteSelectedInGrid');
                                        }
                                        else {
                                            HQ.message.show(11, '', 'Main.Process.deleteSelectedInGrid');
                                        }
                                    }

                                }
                            }
                        }
                        else {
                            HQ.message.show(4, '', '');
                        }
                    }
                    
                    break;
                case "save":
                    if (App.dteStartDate.getValue() == null || App.dteStartDate.getValue() == "" ) {
                        HQ.message.show(1000, App.dteStartDate.fieldLabel);
                    }
                    else if (App.dteEndDate.getValue() == null || App.dteEndDate.getValue() == "") {
                        HQ.message.show(1000, App.dteEndDate.fieldLabel);
                    }
                    else {
                        var allDataFreeItem = App.stoFreeItem.snapshot || App.stoFreeItem.allData || App.stoFreeItem.data;
                        var keycheck = true;
                        var lsterror = '', lsterrorPrice = '';
                        for (var i = 0; i < allDataFreeItem.items.length; i++) {
                            item = allDataFreeItem.items[i];
                            if (item.data.FreeItemQty <= 0  && item.data.FreeItemID != "") {
                                keycheck = false;
                                lsterror += item.data.FreeItemID + ",";
                            }
                            if(App.chkConvertDiscAmtToFreeItem.getValue() == true)
                            {
                                if (item.data.Price <= 0 && item.data.FreeItemID != "") {
                                    keycheck = false;
                                    lsterrorPrice += item.data.FreeItemID + ",";
                                }
                            }
                        }
                        if (keycheck) {
                            if (App.chkStockPromotion.getValue()) {
                                var erro = '';
                                var idx = '';
                                var lstdiscItem = App.stoDiscItem.allData.items;
                                if (lstdiscItem.length > 0) {
                                    for (var i = 0; i < lstdiscItem.length; i++) {
                                        if (lstdiscItem[i].data.InvtID != '' && lstdiscItem[i].data.InvtID != null && (lstdiscItem[i].data.QtyLimit == 0 || lstdiscItem[i].data.QtyLimit == '')) {
                                            erro = erro + lstdiscItem[i].data.InvtID + ',';
                                            idx = idx + (i+1) + ',';
                                        }
                                    }
                                    if (erro != '') {
                                        keycheck = false;
                                        HQ.message.show(2018032012, [erro, idx], '', true);
                                    }
                                }
                            }
                            if (keycheck) {
                                if (App.chkDonateGroupProduct.getValue()) {
                                    var lsterror1 = '';
                                    for (var i = 0; i < allDataFreeItem.items.length; i++) {
                                        item = allDataFreeItem.items[i];
                                        if (item.data.GroupItem == "" && item.data.FreeItemID != "") {
                                            keycheck = false;
                                            lsterror1 += item.data.FreeItemID + ",";
                                        }
                                    }
                                    if (keycheck) {
                                        var lsterror2 = '';
                                        for (var i = 0; i < allDataFreeItem.items.length; i++) {
                                            item = allDataFreeItem.items[i];
                                            if ((item.data.Priority == "" || item.data.Priority <= 0) && item.data.FreeItemID != "") {
                                                keycheck = false;
                                                lsterror2 += item.data.FreeItemID + ",";
                                            }
                                        }
                                        if (keycheck) {
                                            for (var j = 0; j < allDataFreeItem.items.length; j++) {
                                                item = allDataFreeItem.items[j];
                                                var check = 0;
                                                var lstinvtID = '';
                                                for (var k = 0; k < allDataFreeItem.items.length; k++) {
                                                    if (item.data.GroupItem == allDataFreeItem.items[k].data.GroupItem && item.data.Priority == allDataFreeItem.items[k].data.Priority && item.data.FreeItemID != "" && item.data.LineRef == allDataFreeItem.items[k].data.LineRef) {
                                                        lstinvtID = lstinvtID + allDataFreeItem.items[k].data.FreeItemID + ',';
                                                        check++;
                                                    }
                                                }
                                                if (check > 1) {
                                                    var rec = HQ.store.findRecord(App.stoDiscBreak, ['LineRef'], [item.data.LineRef]);
                                                    if (rec) {
                                                        HQ.message.show(2018031712, [(App.stoDiscBreak.indexOf(rec) + 1), lstinvtID, item.data.GroupItem, item.data.Priority], '', true);
                                                    }
                                                    else {
                                                        HQ.message.show(2018031511, [lstinvtID, item.data.GroupItem, item.data.Priority], '', true);
                                                    }
                                                    keycheck = false;

                                                    break;
                                                }

                                            }
                                            if (keycheck) {
                                                for (var j = 0; j < allDataFreeItem.items.length; j++) {
                                                    item = allDataFreeItem.items[j];
                                                    var check = 0;
                                                    var lstinvtID = '';
                                                    for (var k = 0; k < allDataFreeItem.items.length; k++) {
                                                        if (item.data.GroupItem == allDataFreeItem.items[k].data.GroupItem && item.data.FreeItemQty != allDataFreeItem.items[k].data.FreeItemQty && item.data.FreeItemID != "" && item.data.LineRef == allDataFreeItem.items[k].data.LineRef) {
                                                            lstinvtID = lstinvtID + allDataFreeItem.items[k].data.FreeItemID + ',';
                                                        }
                                                    }
                                                    if (lstinvtID != '') {
                                                        lstinvtID = lstinvtID + item.data.FreeItemID;
                                                        var rec = HQ.store.findRecord(App.stoDiscBreak, ['LineRef'], [item.data.LineRef]);
                                                        if (rec) {
                                                            HQ.message.show(2018031713, [(App.stoDiscBreak.indexOf(rec) + 1), lstinvtID, item.data.GroupItem], '', true);
                                                        }
                                                        else {
                                                            HQ.message.show(2018031711, [lstinvtID, item.data.GroupItem], '', true);
                                                        }
                                                        keycheck = false;
                                                        break;
                                                    }

                                                }
                                                if (keycheck) {
                                                    Main.Process.saveData();
                                                }
                                            }
                                        }
                                        else {
                                            HQ.message.show(2018022214, lsterror2, '', false);
                                        }
                                    }
                                    else {
                                        HQ.message.show(2018022213, lsterror1, '', false);
                                    }
                                }
                                else {
                                    Main.Process.saveData();
                                }
                            }
                            //Main.Process.saveData();
                        }
                        else {
                            if (lsterror != "") {
                                HQ.message.show(2018022212, lsterror, '', false);
                                return false;
                            }
                            if(lsterrorPrice != "")
                            {
                                HQ.message.show(2018062601, lsterrorPrice, '', false);
                                return false;
                            }
                        }
                    }

                    break;
                case "print":
                    break;
                case "close":
                    if (App["parentAutoLoadControl"] != undefined) {
                        App["parentAutoLoadControl"].close();
                    }
                    else {
                        parentAutoLoadControl.close();
                    }
                    break;
            }
        }
    }
};

var DiscDefintion = {
    Process: {
        enableATabInList: function (tabNames) {
            var listTabs = ["pnlDPII", "pnlDPBB", "pnlDPTT", "pnlDPCC", "pnlDPPP", "pnlDPCL", "pnlDPCT"];

            for (var j = 0; j < listTabs.length; j++) {
                App[listTabs[j]].disable();
            }
            var disable = true;
            for (var i = 0; i < tabNames.length; i++) {
                App[tabNames[i]].enable();                
            }
            if (App.pnlDPCC.isDisabled()) {
                App.chkExcludeOtherDisc.disable();
            } else {
                App.chkExcludeOtherDisc.enable();
            }
            if (App.pnlDPBB.isDisabled()) {
                App.chkExactQty.disable();
            } else {
                App.chkExactQty.enable();
            }
            //App.chkExcludeOtherDisc.setVisible(!App.pnlDPCC.isDisabled());

            //App.chkExactQty.setVisible(!App['pnlDPBB'].isDisabled());
        },

        getDeepAllLeafNodes: function (node, onlyLeaf) {
            var allNodes = new Array();
            if (!Ext.value(node, false)) {
                return [];
            }
            if (node.isLeaf()) {
                return node;
            } else {
                node.eachChild(
                 function (Mynode) {
                     allNodes = allNodes.concat(Mynode.childNodes);
                 });
            }
            return allNodes;
        },

        getLeafNodes: function (node) {
            var childNodes = [];
            node.eachChild(function (child) {
                if (child.isLeaf()) {
                    childNodes.push(child);
                }
                else {
                    var children = DiscDefintion.Process.getLeafNodes(child);
                    if (children.length) {
                        children.forEach(function (nill) {
                            childNodes.push(nill);
                        });
                    }
                }
            });
            return childNodes;
        },

        renderCpnyName: function (value) {
            var record = App.cboGCpnyID.store.findRecord("CpnyID", value);
            if (record) {
                return record.data.CpnyName;
            }
            else {
                return value;
            }
        },

        renderFreeItemName: function (value, metaData, record, rowIndex, colIndex, store) {
            var rec = HQ.store.findRecord(App.cboFreeItemID.store, ["InvtID"], [record.data.FreeItemID]);
            var returnValue = value;
            if (rec) {
                if (metaData.column.dataIndex == "Descr" && !record.data.Descr) {
                    returnValue = rec.data.Descr;
                }
                else if (metaData.column.dataIndex == "UnitDescr" && !record.data.UnitDescr) {
                    returnValue = rec.data.StkUnit;
                    //record.set("UnitDescr", rec.data.StkUnit);
                    record.data.UnitDescr = returnValue;
                    //if (!record.data.TypeUnit) {
                    //    if (rec.data.OtherUnit != null && rec.data.OtherUnit != "" && rec.data.OtherUnit==rec.data.StkUnit) {
                    //        record.set("TypeUnit",3);
                    //    }
                    //    else {
                    //        record.set("TypeUnit", 1);
                    //    }
                    //}

                }
                else if (metaData.column.dataIndex == "TypeUnit" && !record.data.TypeUnit) {
                    if (rec.data.OtherUnit != null && rec.data.OtherUnit != "" && rec.data.OtherUnit==rec.data.StkUnit) {
                        returnValue = 3;
                    }
                    else {
                        returnValue = 1;
                    }
                    record.data.TypeUnit = returnValue;
                }
            }

            return returnValue;
        },

        renderDpiiInvtName: function (value, metaData, record, rowIndex, colIndex, store) {
            //var rec = App.cboDpiiInvtID.store.findRecord("InvtID", record.data.InvtID);
            var rec = HQ.store.findRecord(App.cboDpiiInvtID.store, ["InvtID"], [record.data.InvtID]);
            var returnValue = value;
            if (rec) {
                if (metaData.column.dataIndex == "Descr" && !record.data.Descr) {
                    returnValue = rec.data.Descr;
                }
                else if (metaData.column.dataIndex == "UnitDesc" && !record.data.UnitDesc) {

                    returnValue = rec.data.StkUnit;
                    //record.set("UnitDesc", rec.data.StkUnit);
                    record.data.UnitDesc = returnValue;
                }
            }

            return returnValue;
        },

        //.Renderer("DiscDefintion.Process.renderDpiiInvtName")
        renderCustomerName: function (value, metaData, record, rowIndex, colIndex, store) {
            var rec = HQ.store.findRecord(App.cboGCustID.store, ["CustID"], [record.data.CustID]);
            var returnValue = value;
            if (rec) {
                if (metaData.column.dataIndex == "BranchID" && !record.data.BranchID) {
                    returnValue = rec.data.BranchID;
                }
                else if (metaData.column.dataIndex == "TerritoryName" && !record.data.TerritoryName) {
                    returnValue = rec.data.TerritoryName;
                }
            }
            return returnValue;
        },

        renderGInvtName: function (value, metaData, record, rowIndex, colIndex, store) {
            var rec = App.cboGInvtID.store.findRecord("InvtID", record.data.InvtID);
            var returnValue = value;
            if (rec) {
                if (metaData.column.dataIndex == "Descr" && !record.data.Descr) {
                    returnValue = rec.data.Descr;
                }
                else if (metaData.column.dataIndex == "UnitDesc" && !record.data.UnitDesc) {
                    returnValue = rec.data.StkUnit;
                    //record.set("UnitDesc", rec.data.StkUnit);
                    record.data.UnitDesc = returnValue;
                }
            }

            return returnValue;
        },
        renderQtyType : function (value, metaData, record, row, col, store, gridView) {
            var r = HQ.store.findRecord(App.cboQtyType.store, ['Code'], [record.data.QtyType])
            if (Ext.isEmpty(r)) {
                return value;
            }
            return r.data.Descr;
        },

        renderFreeITemSiteWhseLoc: function (value, metaData, record, row, col, store, gridView) {
            var r = HQ.store.findRecord(App.cboFreeITemSiteWhseLoc.store, ['WhseLoc'], [record.data.FreeITemSiteWhseLoc]);
            if (Ext.isEmpty(r)) {
                return value;
            }
            return r.data.Descr;
        },
        
        renderFreeITemSiteID: function (value, metaData, record, row, col, store, gridView) {
            var r = HQ.store.findRecord(App.cboFreeITemSiteID.store, ['SiteId'], [record.data.FreeITemSiteID]);
            if (Ext.isEmpty(r)) {
                return value;
            }
            return r.data.Name;
        },

        indexSelect: function (grd) {
            var index = '';
            var allData = grd.store.data;
            var arr = grd.getSelectionModel().selected.items;
            arr.forEach(function (itm) {
                index += (allData.indexOfKey(itm.internalId) + 1) + ' & ';
            });
            return index.substring(0, index.length - 3);
        },

        deleteSelectedCompanies: function (item) {
            if (item == "yes") {
                App.grdCompany.deleteSelected();
            }
        },

        deleteAllCompanies: function (item) {
            if (item == "yes") {
                App.grdCompany.store.removeAll();
                var record = HQ.store.findRecord(App.stoCompany, keys1, ['']);
                if (!record) {
                    HQ.store.insertBlank(App.stoCompany, keys1);
                }
            }
        },

        deleteAllFreeItem: function (item) {
            if (item == "yes") {
                App.grdFreeItem.store.suspendEvents();
                while (App.grdFreeItem.store.data.length > 0) {
                    App.grdFreeItem.store.removeAt(0);
                }
                App.grdFreeItem.store.resumeEvents();
                App.grdFreeItem.view.refresh();

                if (App.chkDonateGroupProduct.getValue()) {
                    var lstAlldata = App.stoFreeItem.snapshot;
                    var check = true;
                    if (lstAlldata.items.length > 0) {
                        for (var i = 0; i < lstAlldata.items.length; i++) {
                            if (lstAlldata.items[i].data.FreeItemID != "" && (lstAlldata.items[i].data.GroupItem != "" || lstAlldata.items[i].data.Priority > 0)) {
                                check = false;
                            }
                        }
                    }
                    App.chkDonateGroupProduct.setReadOnly(!check);
                }
                else {
                    if (!App.chkAutoFreeItem.getValue()) {
                        var lstAlldata = App.stoFreeItem.snapshot;
                        var check = true;
                        if (lstAlldata.items.length > 0) {
                            for (var i = 0; i < lstAlldata.items.length; i++) {
                                if (lstAlldata.items[i].data.FreeItemID != "") {//&& (lstAlldata.items[i].data.GroupItem != "" || lstAlldata.items[i].data.Priority > 0)
                                    check = false;
                                }
                            }
                        }
                        if (check) {
                            App.chkDonateGroupProduct.enable();
                        }
                    }
                    if (!App.chkDonateGroupProduct.getValue()) {
                        var lstAlldata = App.stoFreeItem.snapshot;
                        var check = true;
                        if (lstAlldata.items.length > 0) {
                            for (var i = 0; i < lstAlldata.items.length; i++) {
                                if (lstAlldata.items[i].data.FreeItemID != "" && (lstAlldata.items[i].data.GroupItem != "" || lstAlldata.items[i].data.Priority > 0)) {
                                    check = false;
                                }
                            }
                        }
                        if (check) {
                            App.chkAutoFreeItem.enable();
                        }
                    }
                }


                var lineRef = App.grdDiscBreak.selModel.selected.items[0].data.LineRef;
                var invtBlank = HQ.store.findRecord(App.grdFreeItem.store, ['FreeItemID', 'LineRef'], ['', lineRef]);
                if (!invtBlank) {
                    App.grdFreeItem.store.insert(0, Ext.create("App.mdlFreeItem", {
                        DiscID: App.cboDiscID.getValue(),
                        DiscSeq: App.cboDiscSeq.getValue(),
                        FreeItemID: '',
                        LineRef: lineRef
                    }));
                }
            }
        },

        deleteAllInvts: function (item) {
            if (item == "yes") {


                //App.grdDiscItem.store.suspendEvents();
                //var store = App.grdDiscItem.store.snapshot || App.grdDiscItem.store.allData || App.grdDiscItem.store.data;
                //var index = 0;
                //while (store.length > 0) {
                //    App.grdDiscItem.getSelectionModel().select(store.items[0]);
                //    App.grdDiscItem.deleteSelected();
                //    index++;
                //    if (index >= App.grdDiscItem.store.pageSize) {
                //        App.grdDiscItem.view.refresh();
                //        App.grdDiscItem.store.loadPage(1);
                //        index = 0;
                //    }
                //}
                //App.grdDiscItem.store.resumeEvents();

                App.grdDiscItem.store.removeAll();
                // App.grdDiscItem.store.submitData();
                App.chkStockPromotion.setReadOnly(false);
                App.grdDiscItem.view.refresh();
                App.grdDiscItem.store.loadPage(1);
                var invtBlank = HQ.store.findRecord(App.grdDiscItem.store, ['InvtID'], ['']);
                if (!invtBlank) {
                    App.grdDiscItem.store.insert(0, Ext.create("App.mdlDiscItem", {
                        DiscID: App.cboDiscID.getValue(),
                        DiscSeq: App.cboDiscSeq.getValue(),
                        InvtID: '',
                        UnitDesc: '',
                        Descr: ''
                    }));
                }
            }
        },

        deleteAllBundle: function (item) {
            if (item == "yes") {
                App.grdBundle.store.removeAll();
                App.grdBundle.store.submitData();
                App.grdBundle.view.refresh();
                App.grdBundle.store.loadPage(1);
                var invtBlank = HQ.store.findRecord(App.grdBundle.store, ['InvtID'], ['']);
                if (!invtBlank) {
                    App.grdBundle.store.insert(0, Ext.create("App.mdlBundle", {
                        DiscID: App.cboDiscID.getValue(),
                        DiscSeq: App.cboDiscSeq.getValue(),
                        InvtID: '',
                        UnitDesc: '',
                        Descr: '',
                        BundleQty: 0,
                        BundleAmt: 0
                    }));
                }
            }
        },

        deleteAllCust: function (item) {
            if (item == "yes") {
                App.grdDiscCust.store.removeAll();
                App.grdDiscCust.store.submitData();
                App.grdDiscCust.view.refresh();
                App.grdDiscCust.store.loadPage(1);
                var invtBlank = HQ.store.findRecord(App.grdDiscCust.store, ['CustID'], ['']);
                if (!invtBlank) {
                    App.grdDiscCust.store.insert(0, Ext.create("App.mdlDiscCust", {
                        DiscID: App.cboDiscID.getValue(),
                        DiscSeq: App.cboDiscSeq.getValue(),
                        CustID: ''
                    }));
                }

            }
        },
    },

    Event: {
        btnTmpUpload_Click: function(){
            if (Ext.isEmpty(App.cboDiscSeq.getValue())) {
                HQ.message.show(15, [App.cboDiscSeq.fieldLabel], '', true);
                return false;
            }
            if (document.getElementById('btnUpload-inputEl')) {
                document.getElementById('btnUpload-inputEl').click();
            } else if (document.getElementById('btnUpload-button-fileInputEl')) {
                document.getElementById('btnUpload-button-fileInputEl').click();
            }
        }

        , btnUpload_change: function (fup, newValue, oldValue, eOpts) {
            if (Ext.isEmpty(App.cboDiscSeq.getValue())) {
                App.btnUpload.reset();
                HQ.message.show(15, [App.cboDiscSeq.fieldLabel], '', true);
                return false;
            }

            if (fup.value) {
                var ext = fup.value.split(".").pop().toLowerCase();
                if (ext == "jpg" || ext == "png" || ext == "gif" || ext == "pdf" || ext == "mp4" || ext == "jpeg" || ext == "docx" || ext == "xlsx" || ext == "doc" || ext == "xls") {
                    App.txtProfile.setValue(App.cboDiscID.value + App.cboDiscSeq.value + fup.value);               
                }
                else {
                    HQ.message.show(148, '', '');
                }
            }
        },

        btnClearProfile_click: function (sender, e) {
            App.btnUpload.reset();
            App.txtProfile.setValue("");
        },
        btnSubBreakItem_Click: function (sender, e) {
            App.cboInvtIDSubBreakItem.store.reload();
            //App.stoSubBreakItem.reload();
            App.winSubBreakItem.show();
        },
        btnSubBreakItemOK_Click: function (sender, e) {
            if (HQ.store.checkRequirePass(App.stoSubBreakItem, ['InvtID'], ["InvtID", "UnitDesc"], ["InvtID", "UOM"])) {
                App.winSubBreakItem.hide();
            }
        },
        btnSubBreakItemDelete_Click: function (sender, e) {
            if (App.cboStatus.getValue() == _holdStatus) {
                if (App.slmSubBreakItem.selected.items[0] != undefined) {
                    if (App.slmSubBreakItem.selected.items[0] != undefined) {
                        if (App.slmSubBreakItem.selected.items[0].data.InvtID != "") {
                            HQ.message.show(2015020806, [HQ.grid.indexSelect(App.grdSubBreakItem)], 'DiscDefintion.Event.deleteDataSubBreakItem', true);
                        }
                    }
                }
            }           
        },
        deleteDataSubBreakItem: function(item){
            if (item == "yes") {
                App.grdSubBreakItem.deleteSelected();
            }
        },
        storeGridLoaded : function(sto) {
            HQ.gridSource++;
            if (HQ.gridSource == 10) {
                App.frmMain.unmask();
                App.grdFreeItem.store.filterBy(function (record) { });
                HQ.common.showBusy(false);
                //HQ.gridSource = 0;
            }
        }

        , stoDiscInfo_load: function (sto, records, successful, eOpts) {
            
            if (sto.getCount() > 0) {
                _isNewDisc = false;
            }
            else {
                _isNewDisc = true;
                App.cboDiscSeq.setValue("");
                var discRec = Ext.create("App.mdlDiscInfo", {
                    DiscID: App.cboDiscID.getValue()
                });
                sto.insert(0, discRec);
            }
            var frmRec = sto.getAt(0);
            App.frmDiscDefintionTop.loadRecord(frmRec);
            _discLoad = frmRec.data.DiscID;

            if (frmRec.data.tstamp) {
                App.cboDiscClass.setReadOnly(true);
                App.cboDiscType.setReadOnly(true);
            }
            else {
                App.cboDiscClass.setReadOnly(false);
                App.cboDiscType.setReadOnly(false);
            }
            App.cboDpiiInvtID.store.reload();
            App.cboFreeItemID.store.reload();
            Main.Event.frmMain_fieldChange();
        },

        stoDiscSeqInfo_load: function (sto, records, successful, eOpts) {
            if (sto.getCount() > 0) {
                _isNewSeq = false;
            }
            else {
                App.cboBreakBy.setValue('');
                _isNewSeq = true;
                var discSeqRec = Ext.create("App.mdlDiscSeqInfo", {
                    DiscID: App.cboDiscID.getValue(),
                    DiscSeq: App.cboDiscSeq.getValue(),
                    POStartDate: _dateNow,
                    POEndDate: _dateNow,
                    StartDate: _dateNow,
                    EndDate: _dateNow,
                    Status: _holdStatus,
                    POUse: false,
                    Active: false,
                    Promo: false,
                    AutoFreeItem: false,
                    AllowEditDisc: false,
                    RequiredType: '',
                    SubBreakType: 'N',
                    BreakBoundType: 'L'
                });
                sto.insert(0, discSeqRec);
            }

            var discSeqRec = sto.getAt(0);
            App.chkConvertDiscAmtToFreeItem.setValue(discSeqRec.data.ConvertDiscAmtToFreeItem);
            App.frmDiscSeqInfo.loadRecord(discSeqRec);
            //txtRequiredType_Change();
            _seqLoad = discSeqRec.data.DiscSeq;
            HQ.combo.expand(App.cboExcludePromo, ';');
            if (discSeqRec.data.tstamp) {
                App.cboBreakBy.setReadOnly(true);
                App.cboDiscFor.setReadOnly(true);
                App.cboRequiredType.setReadOnly(true);
                App.cboSubBreakType.setReadOnly(true);
                App.cboBreakBoundType.setReadOnly(true);
                
                //App.chkConvertDiscAmtToFreeItem.setReadOnly(true);
            }
            else {
                App.cboBreakBy.setReadOnly(false);
                App.cboDiscFor.setReadOnly(false);
                App.cboRequiredType.setReadOnly(false);
                App.cboSubBreakType.setReadOnly(false);
                App.cboBreakBoundType.setReadOnly(false);
                
                //App.chkConvertDiscAmtToFreeItem.setReadOnly(false);
            }

            if (discSeqRec.data.Status == _holdStatus) {
                App.cboProAplForItem.setReadOnly(false);
                App.dteStartDate.setReadOnly(false);
                App.dteEndDate.setReadOnly(false);
                App.chkDiscTerm.setReadOnly(false);
                App.chkDonateGroupProduct.setReadOnly(false);
                App.chkAutoFreeItem.setReadOnly(false);
                App.cboBudgetID.setReadOnly(false);
                App.txtSeqDescr.setReadOnly(false);
                App.chkExactQty.setReadOnly(false);
                App.cboExcludePromo.setReadOnly(false);
                App.txtPriorityPromo.setReadOnly(false);
                //App.cboRequiredType.setReadOnly(false);
                //App.chkRequiredType.setReadOnly(false);
                App.btnUpload.enable();
                App.chkStockPromotion.setReadOnly(false);
                App.chkIsDeductQtyAmt.setReadOnly(false);
                App.chkConvertDiscAmtToFreeItem.setReadOnly(false);
                //App.btnTmpUpload.enable();
                App.btnDelImg.enable();
                App.cboProrateAmtType.setReadOnly(false);
                //App.chkDiscAmtBonus.setReadOnly(false);
                //App.btnSubBreakItem.enable();
                App.cboSolomonSite.setReadOnly(false);
                App.cboInvtIDSolomon.setReadOnly(false);
                App.cboDiscSeqSolomon.setReadOnly(false);
                App.chkDiscPrice.setReadOnly(false);
            }
            else {
                App.cboProAplForItem.setReadOnly(true);
                App.dteStartDate.setReadOnly(true);
                App.dteEndDate.setReadOnly(true);
                App.chkDiscTerm.setReadOnly(true);
                App.chkDonateGroupProduct.setReadOnly(true);
                App.chkAutoFreeItem.setReadOnly(true);
                App.cboBudgetID.setReadOnly(true);
                App.txtSeqDescr.setReadOnly(true);
                App.chkExactQty.setReadOnly(true);
                App.cboExcludePromo.setReadOnly(true);
                App.txtPriorityPromo.setReadOnly(true);
                //App.cboRequiredType.setReadOnly(true);
                //App.chkRequiredType.setReadOnly(true);
                App.btnUpload.disable();
                App.chkStockPromotion.setReadOnly(true);
                App.chkIsDeductQtyAmt.setReadOnly(true);
                App.chkConvertDiscAmtToFreeItem.setReadOnly(true);
                //App.btnTmpUpload.enable();
                App.btnDelImg.disable();
                App.cboProrateAmtType.setReadOnly(true);
               // App.chkDiscAmtBonus.setReadOnly(false);
                //App.btnSubBreakItem.disable();
                App.cboSolomonSite.setReadOnly(true);
                App.cboInvtIDSolomon.setReadOnly(true);
                App.cboDiscSeqSolomon.setReadOnly(true);
                App.chkDiscPrice.setReadOnly(true);
            }

            HQ.gridSource = 0;

            App.grdDiscBreak.getStore().addListener('load', DiscDefintion.Event.storeGridLoaded);
            App.grdFreeItem.getStore().addListener('load', DiscDefintion.Event.storeGridLoaded);
            App.grdCompany.getStore().addListener('load', DiscDefintion.Event.storeGridLoaded);
            App.grdDiscItem.getStore().addListener('load', DiscDefintion.Event.storeGridLoaded);
            App.grdBundle.getStore().addListener('load', DiscDefintion.Event.storeGridLoaded);
            App.grdDiscCustClass.getStore().addListener('load', DiscDefintion.Event.storeGridLoaded);
            App.grdDiscCust.getStore().addListener('load', DiscDefintion.Event.storeGridLoaded);
            App.grdDiscItemClass.getStore().addListener('load', DiscDefintion.Event.storeGridLoaded);

            App.grdDiscCustCate.getStore().addListener('load', DiscDefintion.Event.storeGridLoaded);
            App.grdDiscChannel.getStore().addListener('load', DiscDefintion.Event.storeGridLoaded);

            App.frmMain.mask();

            App.grdFreeItem.store.reload();                        
            App.grdCompany.store.reload();
            App.grdDiscItem.store.reload();
            App.grdBundle.store.reload();
            App.grdDiscCustClass.store.reload();
            App.grdDiscCust.store.reload();
            App.grdDiscItemClass.store.reload();
            App.grdDiscCustCate.getStore().reload();
            App.grdDiscChannel.getStore().reload();
            App.grdDiscBreak.store.reload();
            App.cboDpiiInvtID.store.reload();
            App.cboFreeItemID.store.reload();
            Main.Event.frmMain_fieldChange();
        },

        cboDiscID_change: function (cbo, newValue, oldValue, eOpts) {
            //var selRec = HQ.store.findInStore(cbo.store, ["DiscID"], [cbo.getValue()]);
            //if (selRec || !_isNewDisc) {
            //App.cboDiscSeq.store.clearFilter();
            if (HQ.util.passNull(cbo.getValue()) != _discLoad && !cbo.hasFocus) {
                App.stoDiscInfo.reload();
                App.cboDiscSeq.store.load(function () {
                    if (App.cboDiscSeq.store.getCount()) {
                        var discSeqValue = App.cboDiscSeq.store.getAt(0).data.DiscSeq;
                        if (_copy == 1) {
                            discSeqValue = _discSeqNew;
                        }
                        if (discSeqValue == App.cboDiscSeq.getValue()) {
                            App.cboDiscSeq.setValue(discSeqValue);
                            App.stoDiscSeqInfo.reload();
                        }
                        else {
                            App.cboDiscSeq.setValue(discSeqValue);
                        }
                    }
                    else {
                        App.cboDiscSeq.clearValue();
                    }
                });
            }
            _copy = 0;
            
            //}
        },

        cboDiscID_select: function (cbo, newValue, oldValue, eOpts) {
            App.stoDiscInfo.reload();
            App.cboDiscSeq.store.load(function () {
                if (App.cboDiscSeq.store.getCount()) {
                    var discSeqValue = App.cboDiscSeq.store.getAt(0).data.DiscSeq;
                    if (discSeqValue == App.cboDiscSeq.getValue()) {
                        App.cboDiscSeq.setValue(discSeqValue);
                        App.stoDiscSeqInfo.reload();
                    }
                    else {
                        App.cboDiscSeq.setValue(discSeqValue);
                    }
                }
                else {
                    App.cboDiscSeq.clearValue();
                }
            });
        },

        cboDiscType_change: function (cbo, newValue, oldValue, eOpts) {
            var frmRec = App.frmDiscDefintionTop.getRecord();
            if (App.cboDiscType.getValue() == 'L') {
                if (App.chkPctDiscountByLevel != undefined) {
                    App.chkPctDiscountByLevel.enable();
                }
                
            }
            else {
                if (App.chkPctDiscountByLevel != undefined) {
                    App.chkPctDiscountByLevel.disable();
                }                
                if (App.cboDiscType.getValue() == 'G' && App.cboDiscClass.getValue() == 'II' && App.cboBreakBy.getValue() == 'Q') {
                    //App.chkRequiredType.enable();
                }
                else {
                    App.cboRequiredType.setVisible(HQ.showRequiredType);
                    //App.chkRequiredType.disable();
                    //App.chkRequiredType.setValue(false);
                }
                if (HQ.hidePriorityPromo) {
                    if (App.cboDiscType.getValue() == 'D')
                        App.txtPriorityPromo.hide()
                    else
                        App.txtPriorityPromo.show()
                }               
            }
            //if (!frmRec.data.tstamp) {
            //    if (cbo.value) {
            //        if (oldValue) {
            //            if (cbo.value
            //                || App.grdDiscBreak.store.getCount() > 1
            //                || App.grdBundle.store.getCount() > 1
            //                || App.grdDiscCust.store.getCount() > 1
            //                || App.grdDiscCustClass.store.getCount() > 1
            //                || App.grdDiscItem.store.getCount() > 1
            //                || App.grdDiscItemClass.store.getCount() > 1) {
            //                HQ.message.show(43, "", "");

            //                cbo.suspendEvents(false);
            //                cbo.setValue(oldValue);
            //                cbo.resumeEvents();
            //                return;
            //            }

            //            if (App.cboDiscFor.value == "X" && cbo.value != "L") {
            //                HQ.message.show(53, "", "");

            //                cbo.suspendEvents(false);
            //                cbo.setValue(oldValue);
            //                cbo.resumeEvents();
            //                return;
            //            }
            //        }


            //        if (cbo.value == "L" || cbo.value == "G") {
            //            App.cboDiscClass.setValue("II");

            //        }
            //        else {
            //            App.cboDiscClass.setValue("CC");
            //            App.cboBreakBy.setValue("A");
            //        }
            //    }
            //}
            App.cboDiscClass.clearValue();
            App.cboDiscClass.store.load(function () {
                if (frmRec.data.tstamp) {
                    App.cboDiscClass.setValue(frmRec.raw.DiscClass);
                }
            });
        },

        cboDiscClass_change: function (cbo, newValue, oldValue, eOpts) {
            // 0: {Code: "BB", Descr: "Item Bundle"}
            var isbb = false;
            if (cbo.value == "BB") {
                DiscDefintion.Process.enableATabInList(["pnlDPBB"]);
                isbb = true;
                App.chkStockPromotion.disable();
            }
                // 1: {Code: "CB", Descr: "Customer and Item Bundle"}
            else if (cbo.value == "CB") {
                DiscDefintion.Process.enableATabInList(["pnlDPBB", "pnlDPCC"]);
                isbb = true;
                App.chkStockPromotion.disable();
            }
                // 2: {Code: "CC", Descr: "Customer"}
            else if (cbo.value == "CC") {
                DiscDefintion.Process.enableATabInList(["pnlDPCC"]);
                App.chkStockPromotion.disable();
            }
                // 3: {Code: "CI", Descr: "Customer and Invt. Item"}
            else if (cbo.value == "CI") {
                DiscDefintion.Process.enableATabInList(["pnlDPCC", "pnlDPII"]);
                App.chkStockPromotion.enable();
            }
                // 4: {Code: "II", Descr: "Inventory Item"}
            else if (cbo.value == "II") {
                DiscDefintion.Process.enableATabInList(["pnlDPII"]);
                App.chkStockPromotion.enable();
            }
                // 5: {Code: "PP", Descr: "Product Group"}
            else if (cbo.value == "PP") {
                DiscDefintion.Process.enableATabInList(["pnlDPPP"]);
                App.chkStockPromotion.disable();
            }
                // 6: {Code: "TB", Descr: "Shop Type and Item Bundle"}
            else if (cbo.value == "TB") {
                DiscDefintion.Process.enableATabInList(["pnlDPTT", "pnlDPBB"]);
                App.chkStockPromotion.disable();
                isbb = true;
            }
                // 7: {Code: "TI", Descr: "Shop Type and Invt. Item"}
            else if (cbo.value == "TI") {
                DiscDefintion.Process.enableATabInList(["pnlDPTT", "pnlDPII"]);
                App.chkStockPromotion.enable();
            }
                // 8: {Code: "TP", Descr: "Prod. Group and Shop Type"}
            else if (cbo.value == "TP") {
                DiscDefintion.Process.enableATabInList(["pnlDPTT", "pnlDPPP"]);
                App.chkStockPromotion.disable();
            }
                // 9: {Code: "TT", Descr: "Shop Type"}
            else if (cbo.value == "TT") {
                DiscDefintion.Process.enableATabInList(["pnlDPTT"]);
                App.chkStockPromotion.disable();
            }

            else if (cbo.value == "CL") {
                DiscDefintion.Process.enableATabInList(["pnlDPCL"]);
                App.chkStockPromotion.disable();
            }
            else if (cbo.value == "CT") {
                DiscDefintion.Process.enableATabInList(["pnlDPCT"]);
                App.chkStockPromotion.disable();
            }
            else if (cbo.value == "GC") {
                DiscDefintion.Process.enableATabInList(["pnlDPCT", "pnlDPPP"]);
                App.chkStockPromotion.disable();
            }
            else if (cbo.value == "GP") {
                DiscDefintion.Process.enableATabInList(["pnlDPPP", "pnlDPCL"]);
                App.chkStockPromotion.disable();
            }
            else if (cbo.value == "GI") {
                DiscDefintion.Process.enableATabInList(["pnlDPII", "pnlDPCL"]);
                App.chkStockPromotion.enable();
            }
            else if (cbo.value == "IC") {
                DiscDefintion.Process.enableATabInList(["pnlDPCT", "pnlDPII"]);
                App.chkStockPromotion.enable();
            }
            else {
                DiscDefintion.Process.enableATabInList([]);
                App.chkStockPromotion.disable();
            }


            if (App.cboDiscType.getValue() == 'G' && App.cboDiscClass.getValue() == 'II' && App.cboBreakBy.getValue() == 'Q') {
                //App.chkRequiredType.enable();
            }
            else {
                //App.chkRequiredType.disable();
                //App.chkRequiredType.setValue(false);
            }            
            //if (App.cboDiscType.getValue() == 'G' && (App.cboDiscClass.getValue() == 'II' || App.cboDiscClass.getValue() == 'PP'))
            //    App.chkConvertDiscAmtToFreeItem.enable();
            //else
            //    App.chkConvertDiscAmtToFreeItem.disable();
            if (HQ.showRequiredType) {
                if (App.cboDiscType.getValue() == 'G' && cbo.value == 'II') {
                    App.cboRequiredType.show();
                }
                else {
                    App.cboRequiredType.hide();
                }
            }
            if (HQ.hidePriorityPromo)
            {
                if (App.cboDiscType.getValue() == 'D')
                    App.txtPriorityPromo.hide()
                else
                    App.txtPriorityPromo.show()
            }
            if (HQ.hideExcludePromo) 
                App.cboExcludePromo.show();
            else
                App.cboExcludePromo.hide();
            if (HQ.hideIsDeductQtyAmt)
                App.chkIsDeductQtyAmt.show();
            else
                App.chkIsDeductQtyAmt.hide();
            if (HQ.hideSubBreakType) {
                App.cboSubBreakType.show();
                //HQ.grid.show(App.grdDiscBreak, ['SubBreakQty']);
                //HQ.grid.show(App.grdDiscBreak, ['SubBreakAmt']);
                //HQ.grid.show(App.grdDiscBreak, ['SubBreakQtyUpper']);
                //HQ.grid.show(App.grdDiscBreak, ['SubBreakAmtUpper']);
            }
            else {
                App.cboSubBreakType.hide();
                HQ.grid.hide(App.grdDiscBreak, ['SubBreakQty']);
                HQ.grid.hide(App.grdDiscBreak, ['SubBreakAmt']);
                HQ.grid.hide(App.grdDiscBreak, ['SubBreakQtyUpper']);
                HQ.grid.hide(App.grdDiscBreak, ['SubBreakAmtUpper']);
            }
            if (HQ.hideBreakBoundType) {
                App.cboBreakBoundType.show();
                //HQ.grid.show(App.grdDiscBreak, ['BreakQtyUpper']);
                //HQ.grid.show(App.grdDiscBreak, ['BreakAmtUpper']);
            }
            else {
                App.cboBreakBoundType.hide();
                HQ.grid.hide(App.grdDiscBreak, ['BreakQtyUpper']);
                HQ.grid.hide(App.grdDiscBreak, ['BreakAmtUpper']);
            }
            if (HQ.hideConvertDiscAmtToFreeItem) {
                App.chkConvertDiscAmtToFreeItem.show();
                //HQ.grid.show(App.grdFreeItem, ['Price']);
            }
            else {
                App.chkConvertDiscAmtToFreeItem.hide();
                HQ.grid.hide(App.grdFreeItem, ['Price']);
            }
            //if (App.cboProAplForItem.value == "M"
            //        && cbo.value
            //        && cbo.value.substring(1) != "I") {
            //    App.cboProAplForItem.setValue("A");
            //}
            //if (cbo.value != null) {
            //    var recordcboDiscClass = App.cboDiscClassOM21100_pcDiscClass.findRecord('Code', cbo.value);
            //    if (recordcboDiscClass != null) {
            //        if (!recordcboDiscClass.data.ProAplForItem) {
            //            App.cboProAplForItem.setValue("A");
            //            App.cboProAplForItem.setReadOnly(true);
            //        }
            //        else {
            //            App.cboProAplForItem.setReadOnly(false);
            //        }
            //    }
            //}
            //else {
            //    App.cboProAplForItem.setReadOnly(false);
            //}
            if (isbb) {
                App.grdDiscBreak.columns[3].setText(HQ.common.getLang('NoBundle'));
            } else {
                App.grdDiscBreak.columns[3].setText(HQ.common.getLang('BreakAmt'));
            }
        },

        cboDiscSeq_change: function (cbo, newValue, oldValue, eOpts) {
            if (HQ.util.passNull(cbo.getValue()) != _seqLoad && !cbo.hasFocus) {
                App.chkAutoFreeItem.enable();
                App.chkDonateGroupProduct.enable();
                App.chkConvertDiscAmtToFreeItem.enable();
                App.stoDiscSeqInfo.reload();
                var enddate = new Date(1900, 01, 1);
                App.dteEndDate.setMinValue(enddate);
                App.colQtyLimit.hide();
                App.colPerStockAdvance.hide();
                App.colQtyStockAdvance.hide();
                //App.chkDonateGroupProduct.enable();
            }
            App.stoSubBreakItem.reload();
        },

        cboDiscSeq_select: function (cbo, newValue, oldValue, eOpts) {
            if (!App.stoDiscSeqInfo.loading) {
                //App.chkDonateGroupProduct.enable();
                App.stoDiscSeqInfo.reload();
            }
        },

        cboDiscFor_change: function (cbo, newValue, oldValue, eOpts) {
            var frmRec = App.frmDiscSeqInfo.getRecord();
            if (!frmRec.data.tstamp) {
                if (cbo.value && oldValue &&
                    (App.grdDiscBreak.store.getCount() > 1
                    || App.grdBundle.store.getCount() > 1
                    || App.grdDiscItem.store.getCount() > 1
                    || App.grdDiscCustClass.store.getCount() > 1
                    || App.grdDiscItemClass.store.getCount() > 1)) {
                    HQ.message.show(43, "", "");

                    cbo.suspendEvents(false);
                    cbo.setValue(oldValue);
                    cbo.resumeEvents();
                    return;
                }
                if (cbo.value == "X" && App.cboDiscType.value != "L") {
                    HQ.message.show(53, "", "");

                    cbo.suspendEvents(false);
                    cbo.setValue(oldValue);
                    cbo.resumeEvents();
                    return;
                }
            }

            var discAmtCol = App.grdDiscBreak.down('[dataIndex=DiscAmt]');
            var discAmtField = discAmtCol.getEditor().field;

            if (App.cboDiscFor.getValue() == "A") {
                discAmtCol.format = "0,000";
                discAmtField.decimalPrecision = 0;
            }
            else if (App.cboDiscFor.getValue() == "P") {
                discAmtCol.format = "0,000.00";
                discAmtField.decimalPrecision = 2;
            }
        },

        cboBreakBy_change: function (cbo, newValue, oldValue, eOpts) {
            var discSeqRec = App.frmDiscSeqInfo.getRecord();
            if (!discSeqRec.data.tstamp) {
                if (App.cboDiscSeq.getValue() && oldValue && cbo.value
                    && (App.grdDiscBreak.store.getCount() > 1
                    || App.grdBundle.store.getCount() > 1
                    || App.grdDiscItem.store.getCount() > 1
                    || App.grdDiscItemClass.store.getCount() > 1
                    || App.grdDiscCust.store.getCount() > 1
                    || App.grdDiscCustClass.store.getCount() > 1)) {
                    HQ.message.show(43, "", "");

                    cbo.suspendEvents(false);
                    cbo.setValue(oldValue);
                    cbo.resumeEvents();
                }
                else if ((App.cboDiscClass.value == "CC"
                    || App.cboDiscClass.value == "TT"
                    || App.cboDiscType.value == "D")
                    && cbo.value == "Q") {
                    HQ.message.show(53, "", "");

                    cbo.suspendEvents(false);
                    cbo.setValue(oldValue);
                    cbo.resumeEvents();
                }
                else if (App.grdDiscBreak.store.getCount() > 1 &&
                    (App.grdDiscBreak.store.getAt(0).BreakAmt > 0 || App.grdDiscBreak.store.getAt(0).BreakQty > 0)) {
                    HQ.message.show(53, "", "");

                    cbo.suspendEvents(false);
                    cbo.setValue(oldValue);
                    cbo.resumeEvents();
                }
            }
            if (cbo.value == "W") {
                App.grdDiscBreak.down('[dataIndex=BreakQty]').setText(HQ.common.getLang("weight"));
            }
            else {
                App.grdDiscBreak.down('[dataIndex=BreakQty]').setText(HQ.common.getLang("breakqty"));
            }
            var isEnable = (!App['pnlDPBB'].isDisabled() && cbo.value == "Q");
            if (App.pnlDPBB.isDisabled() && cbo.value == "Q") {
                App.chkExactQty.disable();
            } else {
                App.chkExactQty.enable();
            }
            if ((App.cboDiscClass.getValue() != 'BB' || App.cboDiscClass.getValue() != 'CB' || App.cboDiscClass.getValue() != 'TB') && App.cboBreakBy.getValue() == 'A')
                App.chkExactQty.disable();
            else
                App.chkExactQty.enable();
            var isEnableRequiredType = (!App['pnlDPII'].isDisabled() && cbo.value == "Q");
            
            if (App.cboDiscType.getValue() == 'G' && App.cboDiscClass.getValue() == 'II' && App.cboBreakBy.getValue() == 'Q') {
                //App.chkRequiredType.enable();
            }
            else {
                //App.chkRequiredType.disable();
                //App.chkRequiredType.setValue(false);
            }
        },
        cboSubBreakType_change: function (cbo, newValue, oldValue, eOpts) {
            if (App.cboDiscSeq.getValue() && oldValue && cbo.value != 'N'
                   && (App.grdDiscBreak.store.getCount() > 1
                   || App.grdBundle.store.getCount() > 1
                   || App.grdDiscItem.store.getCount() > 1
                   || App.grdDiscItemClass.store.getCount() > 1
                   || App.grdDiscCust.store.getCount() > 1
                   || App.grdDiscCustClass.store.getCount() > 1) && HQ.isNew) {
                HQ.message.show(43, "", "");

                cbo.suspendEvents(false);
                cbo.setValue(oldValue);
                cbo.resumeEvents();
            }
            if (cbo.value != 'N') {
                App.btnSubBreakItem.enable();
                if (cbo.value == 'A') {
                    if (App.cboBreakBoundType.getValue() == 'L') {
                        HQ.grid.show(App.grdDiscBreak, ['SubBreakAmt']);
                        HQ.grid.hide(App.grdDiscBreak, ['SubBreakAmtUpper']);
                        if (App.stoDiscBreak.data.length > 0) {
                            App.stoDiscBreak.data.items[0].set('SubBreakAmtUpper', 0);
                        }
                    }
                    else if (App.cboBreakBoundType.getValue() == 'B')
                    {
                        HQ.grid.show(App.grdDiscBreak, ['SubBreakAmt']);
                        HQ.grid.show(App.grdDiscBreak, ['SubBreakAmtUpper']);
                    }
                    HQ.grid.hide(App.grdDiscBreak, ['SubBreakQty']);
                    HQ.grid.hide(App.grdDiscBreak, ['SubBreakQtyUpper']);
                    if (App.stoDiscBreak.data.length > 0) {
                        App.stoDiscBreak.data.items[0].set('SubBreakQty', 0);
                        App.stoDiscBreak.data.items[0].set('SubBreakQtyUpper', 0);
                    }
                }
                else if (cbo.value == 'Q') {
                    if (App.cboBreakBoundType.getValue() == 'L') {
                        HQ.grid.show(App.grdDiscBreak, ['SubBreakQty']);
                        HQ.grid.hide(App.grdDiscBreak, ['SubBreakQtyUpper']);
                        if (App.stoDiscBreak.data.length > 0) {
                            App.stoDiscBreak.data.items[0].set('SubBreakQtyUpper', 0);
                        }
                    }
                    else if (App.cboBreakBoundType.getValue() == 'B') {
                        HQ.grid.show(App.grdDiscBreak, ['SubBreakQty']);
                        HQ.grid.show(App.grdDiscBreak, ['SubBreakQtyUpper']);
                    }
                    HQ.grid.hide(App.grdDiscBreak, ['SubBreakAmt']);
                    HQ.grid.hide(App.grdDiscBreak, ['SubBreakAmtUpper']);
                    if (App.stoDiscBreak.data.length > 0) {
                        App.stoDiscBreak.data.items[0].set('SubBreakAmt', 0);
                        App.stoDiscBreak.data.items[0].set('SubBreakAmtUpper', 0);
                    }
                }
            }
            else {
                App.btnSubBreakItem.disable();
                HQ.grid.hide(App.grdDiscBreak, ['SubBreakQty']);
                HQ.grid.hide(App.grdDiscBreak, ['SubBreakAmt']);
                HQ.grid.hide(App.grdDiscBreak, ['SubBreakQtyUpper']);
                HQ.grid.hide(App.grdDiscBreak, ['SubBreakAmtUpper']);
            }
        },
        cboBreakBoundType_change: function (cbo, newValue, oldValue, eOpts) {
            if (App.cboDiscSeq.getValue() && oldValue && cbo.value != 'L'
                  && (App.grdDiscBreak.store.getCount() > 1
                  || App.grdBundle.store.getCount() > 1
                  || App.grdDiscItem.store.getCount() > 1
                  || App.grdDiscItemClass.store.getCount() > 1
                  || App.grdDiscCust.store.getCount() > 1
                  || App.grdDiscCustClass.store.getCount() > 1) && HQ.isNew) {
                HQ.message.show(43, "", "");

                cbo.suspendEvents(false);
                cbo.setValue(oldValue);
                cbo.resumeEvents();
            }
            if(cbo.value != 'L')
            {
                HQ.grid.show(App.grdDiscBreak, ['BreakQtyUpper']);
                HQ.grid.show(App.grdDiscBreak, ['BreakAmtUpper']);
                if (App.cboSubBreakType.getValue() == 'A')
                {
                    HQ.grid.show(App.grdDiscBreak, ['SubBreakAmtUpper']);
                }
                else if (App.cboSubBreakType.getValue() == 'Q')
                {
                    HQ.grid.show(App.grdDiscBreak, ['SubBreakQtyUpper']);
                }
            }
            else
            {
                HQ.grid.hide(App.grdDiscBreak, ['BreakQtyUpper']);
                HQ.grid.hide(App.grdDiscBreak, ['BreakAmtUpper']);
                HQ.grid.hide(App.grdDiscBreak, ['SubBreakAmtUpper']);
                HQ.grid.hide(App.grdDiscBreak, ['SubBreakQtyUpper']);
            }
        },
        cboProAplForItem_change: function (cbo, newValue, oldValue, eOpts) {
            var discSeqRec = App.frmDiscSeqInfo.getRecord();
            //if (discSeqRec.data.tstamp) {
            //    if (Ext.isEmpty(newValue)) {
            //        App.cboProAplForItem.suspendEvents(false);
            //        App.cboProAplForItem.setValue(oldValue);                    
            //        App.cboProAplForItem.resumeEvents();
            //        return;
            //    }                
            //}
                var recordcboDiscClass = App.cboDiscClassOM21100_pcDiscClass.findRecord('Code', App.cboDiscClass.value);
                if (recordcboDiscClass != null) {
                    if (!recordcboDiscClass.data.ProAplForItem) {
                        App.cboProAplForItem.suspendEvents(false);
                        if (Ext.isEmpty(App.cboDiscSeq.getValue())) {
                            App.cboProAplForItem.setValue("");
                        } else {
                            App.cboProAplForItem.setValue("A");
                        }                        
                        App.cboProAplForItem.resumeEvents();
                    }
                }
                
           

              
                //if (cbo.value == "M"
                //    && App.cboDiscClass.value
                //    && App.cboDiscClass.value.substring(1) != "I") {

                //    cbo.suspendEvents(false);
                //    cbo.setValue("A");
                //    cbo.resumeEvents();
                //}
                //else {
                    
                //}

                if (cbo.value == "A") {
                    
                    App.chkAutoFreeItem.setValue(false);
                    App.chkAutoFreeItem.setDisabled(false);
                }
                else if (cbo.value == "M") {
                    
                    App.chkAutoFreeItem.setValue(true);
                    App.chkAutoFreeItem.setDisabled(true);
                }
                else {
                    
                    App.chkAutoFreeItem.setValue(false);
                    App.chkAutoFreeItem.setDisabled(false);
                }
            //}
        },
        cboInvtIDSubBreakItem_Change: function () {
            App.slmSubBreakItem.getSelection()[0].set('UnitDesc', '');
        },
        cboProAplForItem_Expand: function () {
            var store = App.cboProAplForItem.store;
            store.clearFilter();
            var recordcboDiscClass = App.cboDiscClassOM21100_pcDiscClass.findRecord('Code', App.cboDiscClass.value);
            if (recordcboDiscClass != null) {               
                if (!recordcboDiscClass.data.ProAplForItem) {
                    store.filterBy(function (record) {
                        if (record) {
                            if (record.data.Code == 'A') {
                                return record;
                            }
                        }                    
                    });
                }
            } else {
                store.filterBy(function (record) {
                });
            }
            App.cboProAplForItem.store.update();
        },

        checkExpandCombo: function() {
            if (Ext.isEmpty(App.cboDiscSeq.getValue())) {
                HQ.message.show(15, [App.cboDiscSeq.fieldLabel], '', true);
                return false;
            }
            App.cboBreakBoundType.forceSelection = true;
            App.cboSubBreakType.forceSelection = true;
        },
        checkExpandComboExcludePromo: function () {
            if (Ext.isEmpty(App.cboDiscSeq.getValue())) {
                HQ.message.show(15, [App.cboDiscSeq.fieldLabel], '', true);
                return false;
            }
            App.cboExcludePromo.forceSelection = true;
        },
        checkChangeCheckBox: function (chk, newValue, oldValue, eOpts) {
            if (Ext.isEmpty(App.cboDiscSeq.getValue())) {
                HQ.message.show(15, [App.cboDiscSeq.fieldLabel], '', true);
                chk.suspendEvents();
                chk.setValue(false);
                chk.resumeEvents();
                return false;
            }
        },

        chkStockPromotion_Focus: function (chk, newValue, oldValue, eOpts) {
            if (Ext.isEmpty(App.cboDiscSeq.getValue())) {
                HQ.message.show(15, [App.cboDiscSeq.fieldLabel], '', true);
                chk.suspendEvents();
                chk.setValue(false);
                chk.resumeEvents();
                return false;
            }
            else {
                if (App.chkStockPromotion.readOnly) {
                    HQ.message.show(2018032516,'', '', true);
                }
            }
        },

        chkStockPromotion_Change: function (chk, newValue, oldValue, eOpts) {
            if (Ext.isEmpty(App.cboDiscSeq.getValue())) {
                if (chk.hasFocus) {
                    HQ.message.show(15, [App.cboDiscSeq.fieldLabel], '', true);
                }                
                chk.suspendEvents();
                chk.setValue(false);
                chk.resumeEvents();
                return false;
            }
            else {
                if (newValue) {
                    //HQ.grd.show(App.grdDiscItem, ['QtyStockAdvance', 'QtyLimit', 'PerStockAdvance']);
                    App.colPerStockAdvance.show();
                    App.colQtyLimit.show();
                    App.colQtyStockAdvance.show();
                }
                else {
                    //HQ.grd.hide(App.grdDiscItem, ['QtyStockAdvance', 'QtyLimit', 'PerStockAdvance']);
                    App.colPerStockAdvance.hide();
                    App.colQtyLimit.hide();
                    App.colQtyStockAdvance.hide();
                }
            }
        },

        chkAutoFreeItem_Change: function (chk, newValue, oldValue, eOpts) {
            if (newValue) {
                App.chkDonateGroupProduct.disable();
                App.chkConvertDiscAmtToFreeItem.disable();
            }
            else {
                var check=true;
                var lstDataFree = App.stoFreeItem.snapshot || App.stoFreeItem.allData || App.stoFreeItem.data;
                for (var i = 0; i < lstDataFree.items.length; i++) {
                    if (lstDataFree.items[i].data.FreeItemID != "" && lstDataFree.items[i].data.FreeItemID != null) {
                        check = false;
                    }
                }
                if (check) {
                    App.chkDonateGroupProduct.enable();
                    App.chkConvertDiscAmtToFreeItem.enable();
                }
                else {
                    App.chkDonateGroupProduct.disable();
                    App.chkConvertDiscAmtToFreeItem.disable();
                }

            }
        },

        chkDonateGroupProduct: function (chk, newValue, oldValue, eOpts) {
            if (Ext.isEmpty(App.cboDiscSeq.getValue())) {
                if (chk.hasFocus) {
                    HQ.message.show(15, [App.cboDiscSeq.fieldLabel], '', true);
                }               
                chk.suspendEvents();
                App.chkDonateGroupProduct.setValue(false);
                chk.resumeEvents();
                return false;
            }
            else {                
                var lstFreeItemcheck = App.stoFreeItem.snapshot || App.stoFreeItem.allData || App.stoFreeItem.data;
                if (lstFreeItemcheck.items.length > 0) {
                    for (var i = 0; i < lstFreeItemcheck.items.length; i++) {
                        if ((lstFreeItemcheck.items[i].data.GroupItem != null && lstFreeItemcheck.items[i].data.GroupItem != "") || (lstFreeItemcheck.items[i].data.Priority >0)) {
                            HQ.message.show(2018022211, '', '', true);
                            //App.chkDonateGroupProduct.setReadOnly(false);
                            chk.suspendEvents();
                            //App.chkDonateGroupProduct.setReadOnly(true);
                            chk.resumeEvents();
                            return false;
                            break;                            
                        }
                    }
                }
            }
        },
        //chkDiscAmtBonus_Focus: function (chk, newValue, oldValue, eOpts) {
        //    if (Ext.isEmpty(App.cboDiscSeq.getValue())) {
        //        if (chk.hasFocus) {
        //            HQ.message.show(15, [App.cboDiscSeq.fieldLabel], '', true);
        //        }
        //        chk.suspendEvents();
        //        App.chkDiscAmtBonus.setValue(false);
        //        chk.resumeEvents();
        //        return false;
        //    }
        //},
        chkConvertDiscAmtToFreeItem_Change: function (chk, newValue, oldValue, eOpts) {
            var storeDiscBreak = App.stoDiscBreak.snapshot || App.stoDiscBreak.data;
            var storeFreeItem = App.stoFreeItem.snapshot || App.stoFreeItem.data;
            var obj = HQ.store.findInStore(App.stoFreeItem, ['DiscID', 'DiscSeq'], [App.cboDiscID.getValue(), App.cboDiscSeq.getValue()]);

            if (storeDiscBreak.items[0] != undefined && storeFreeItem.items[0] != undefined && storeDiscBreak.items[0].data.DiscAmt != 0 && storeFreeItem.items[0].data.FreeItemID != '' && obj != undefined && obj.Price > 0) {
                chk.suspendEvents();
                chk.setValue(oldValue);
                chk.resumeEvents();
                return false;
            }
            else {
                if (newValue) {
                    if (!App.chkAutoFreeItem.getValue()) {
                        App.chkAutoFreeItem.disable();
                    }
                    HQ.grid.show(App.grdFreeItem, ['Price']);
                   // HQ.grid.show(App.grdFreeItem, ['Priority']);
                }
                else {
                    HQ.grid.hide(App.grdFreeItem, ['Price']);
                    //HQ.grid.hide(App.grdFreeItem, ['Priority']);
                    if (App.cboProAplForItem.getValue() != 'M') {
                        App.chkAutoFreeItem.enable();
                    }
                    else {
                        App.chkAutoFreeItem.disable();
                    }
                }
            }            
        },
        //chkDiscAmtBonus_Change: function (chk, newValue, oldValue, eOpts) {
        //    App.colDiscAmtBonus.setVisible(App.chkDiscAmtBonus.getValue());
        //},
        cboGCustID_Change: function (item, newValue, oldValue, eOpts) {
            _selBranchID = '';
            _selTerritory = '';
            if (item.valueModels != undefined && item.valueModels[0]) {
                _selBranchID = item.valueModels[0].data.BranchID;
                _selTerritory = item.valueModels[0].data.TerritoryName;
            }
            else
            {
                var obj = HQ.store.findInStore(App.cboGCustID.store, ['CustID'], [newValue]);
                if(obj != undefined)
                {
                    _selBranchID = obj.BranchID;
                    _selTerritory = obj.TerritoryName;
                }
            }
        },

        treeBranch_AfterRender: function (id) {
            HQ.common.showBusy(true, HQ.waitMsg);
            App.direct.OM21100GetTreeBranch( id, {
                success: function (result) {
                    App.treePanelBranch.getRootNode().expand();
                    HQ.common.showBusy(false, HQ.waitMsg);
                }
            });
        },

        treeInventory_AfterRender: function (id) {
            HQ.common.showBusy(true, HQ.waitMsg);
            App.direct.OM21100LoadTreeInventory(id, {
                success: function (result) {
                    App.treePanelInvt.getRootNode().expand();
                    HQ.common.showBusy(false, HQ.waitMsg);
                }
            });
        },

        treeBundleItem_AfterRender: function (id) {
            HQ.common.showBusy(true, HQ.waitMsg);
            App.direct.OM21100LoadTreeBundleItem(id, {
                success: function (result) {
                    App.treePanelBundle.getRootNode().expand();
                    HQ.common.showBusy(false, HQ.waitMsg);
                }
            });
        },

        treeCustomer_AfterRender: function (id) {
            HQ.common.showBusy(true, HQ.waitMsg);
            lstCpnyID = [];
            for (var i = 0; i < App.grdCompany.store.getRecordsValues().length; i++) {
                if (App.grdCompany.store.getRecordsValues()[i].CpnyID != "")
                    lstCpnyID.push(App.grdCompany.store.getRecordsValues()[i].CpnyID);
            }
            App.direct.OM21100GetTreeCustomer(id, lstCpnyID.join(','), {
                success: function (result) {
                    App.treePanelCustomer.getRootNode().expand();
                    HQ.common.showBusy(false, HQ.waitMsg);
                }
            });
            App.cboGCustID.store.reload();
        },


        treePanelBranch_checkChange: function (node, checked, eOpts) {
            if (App.cboStatus.getValue() == _holdStatus) {
                DiscDefintion.Event.checkNode(checked, node);
                DiscDefintion.Event.checkParentNode(checked, node);
                //node.childNodes.forEach(function (childNode) {
                //    childNode.set("checked", checked);
                //});
            } else {
                App.treePanelBranch.clearChecked();
            }
        },

        checkNode : function (checked, node) {
            if (node.childNodes.length > 0) {
                for (var i = 0; i < node.childNodes.length; i++) {
                    node.set('checked', checked)
                    DiscDefintion.Event.checkNode(checked, node.childNodes[i]);
                }
            }
            node.set('checked', checked);
        },
        checkParentNode: function (checked, node) {
            

            if (node.parentNode != undefined) {
                node.parentNode.set('checked', checked)
                DiscDefintion.Event.checkParentNode(checked, node.parentNode);
            }
        },


        treePanelFreeItem_checkChange: function (node, checked) {
            if (App.cboStatus.getValue() == _holdStatus && App.grdDiscBreak.selModel.selected.length > 0 && Main.Process.checkAddFreeItem()) {
                if (node.hasChildNodes()) {
                    node.eachChild(function (childNode) {
                        childNode.set('checked', checked);
                        DiscDefintion.Event.treePanelFreeItem_checkChange(childNode, checked);
                    });
                }
            } else {
                App.treePanelFreeItem.clearChecked();
            }
        },

        treePanelInvt_checkChange: function (node, checked) {
            if (App.cboStatus.getValue() == _holdStatus) {
                if (node.hasChildNodes()) {
                    node.eachChild(function (childNode) {
                        childNode.set('checked', checked);
                        DiscDefintion.Event.treePanelInvt_checkChange(childNode, checked);
                    });
                }
            } else {
                App.treePanelInvt.clearChecked();
            }
        },

        treePanelBundle_checkChange: function (node, checked) {
            if (App.cboStatus.getValue() == _holdStatus) {
                if (node.hasChildNodes()) {
                    node.eachChild(function (childNode) {
                        childNode.set('checked', checked);
                        DiscDefintion.Event.treePanelBundle_checkChange(childNode, checked);
                    });
                }
            } else {
                App.treePanelBundle.clearChecked();
            }
        },

        treePanelCustomer_checkChange: function (node, checked) {
            if (App.cboStatus.getValue() == _holdStatus) {
                if (node.hasChildNodes()) {
                    node.eachChild(function (childNode) {
                        childNode.set('checked', checked);
                        DiscDefintion.Event.treePanelCustomer_checkChange(childNode, checked);
                    });
                }
            } else {
                App.treePanelCustomer.clearChecked();
            }
        },

        btnExpand_click: function (btn, e, eOpts) {
            App.treePanelBranch.expandAll();
        },

        btnCollapse_click: function (btn, e, eOpts) {
            App.treePanelBranch.collapseAll();
        },

        btnFreeItemExpand_click: function (btn, e, eOpts) {
            App.treePanelFreeItem.getStore().suspendEvents();
            App.treePanelFreeItem.expandAll();
            App.treePanelFreeItem.getStore().resumeEvents();
        },

        btnFreeItemCollapse_click: function (btn, e, eOpts) {
            App.treePanelFreeItem.collapseAll();
        },

        btnInvtExpand_click: function (btn, e, eOpts) {
            App.treePanelInvt.getStore().suspendEvents();
            App.treePanelInvt.expandAll();
            App.treePanelInvt.getStore().resumeEvents();
        },

        btnInvtCollapse_click: function (btn, e, eOpts) {
            App.treePanelInvt.collapseAll();
        },

        btnBundleExpand_click: function (btn, e, eOpts) {
            App.treePanelBundle.getStore().suspendEvents();
            App.treePanelBundle.expandAll();
            App.treePanelBundle.getStore().resumeEvents();
        },

        btnBundleCollapse_click: function (btn, e, eOpts) {
            App.treePanelBundle.collapseAll();
        },

        btnCustomerExpand_click: function (btn, e, eOpts) {
            App.treePanelCustomer.getStore().suspendEvents();
            App.treePanelCustomer.expandAll();
            App.treePanelCustomer.getStore().resumeEvents();
        },

        btnCustomerCollapse_click: function (btn, e, eOpts) {
            App.treePanelCustomer.collapseAll();
        },

        // Company
        btnAddAll_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var status = App.cboStatus.value;

                    if (status == _holdStatus) {
                        var allNodes = DiscDefintion.Process.getLeafNodes(App.treePanelBranch.getRootNode(), true);
                        if (allNodes && allNodes.length > 0) {
                            App.grdCompany.store.suspendEvents();
                            allNodes.forEach(function (node) {
                                if (node.data.Type == "Company") {
                                    var idx = App.grdCompany.store.getCount();
                                    if (idx > 0)
                                        idx = idx - 1;
                                    var record = HQ.store.findInStore(App.grdCompany.store,
                                        ['DiscID', 'DiscSeq', 'CpnyID'],
                                        [discId, discSeq, node.data.RecID]);
                                    if (!record) {
                                        App.grdCompany.store.insert(idx, Ext.create("App.mdlCompany", {
                                            DiscID: discId,
                                            DiscSeq: discSeq,
                                            CpnyID: node.data.RecID
                                        }));
                                    }
                                }
                            });
                            App.treePanelBranch.clearChecked();
                            App.grdCompany.store.resumeEvents();
                            App.grdCompany.view.refresh();
                        }
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnAdd_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var status = App.cboStatus.value;

                    if (status == _holdStatus) {
                        var allNodes = App.treePanelBranch.getCheckedNodes();
                        if (allNodes && allNodes.length > 0) {
                            App.grdCompany.store.suspendEvents();

                            var invtBlank = HQ.store.findRecord(App.grdCompany.store, ['CpnyID'], ['']);
                            if (invtBlank) {
                                App.grdCompany.store.remove(invtBlank);
                            }

                            allNodes.forEach(function (node) {
                                if (node.attributes.Type == "Company") {
                                    var idx = App.grdCompany.store.getCount();
                                    var record = HQ.store.findInStore(App.grdCompany.store,
                                        ['DiscID', 'DiscSeq', 'CpnyID'],
                                        [discId, discSeq, node.attributes.RecID]);
                                    if (!record) {
                                        App.grdCompany.store.insert(idx, Ext.create("App.mdlCompany", {
                                            DiscID: discId,
                                            DiscSeq: discSeq,
                                            CpnyID: node.attributes.RecID
                                        }));
                                    }
                                }
                            });

                            var invtBlank = HQ.store.findRecord(App.grdCompany.store, ['InvtID'], ['']);
                            if (!invtBlank) {
                                var idx = App.grdCompany.store.data.length > 0 ? App.grdCompany.store.data.length - 1 : 0;
                                App.grdCompany.store.insert(idx, Ext.create("App.mdlCompany", {
                                    DiscID: discId,
                                    DiscSeq: discSeq,
                                    CpnyID: ''
                                }));
                            }
                            App.treePanelBranch.clearChecked();
                            App.grdCompany.store.resumeEvents();
                            App.grdCompany.view.refresh();
                        }
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnDel_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var status = App.cboStatus.value;

                    if (status == _holdStatus) {
                        Main.Event.menuClick('delete');
                        //var selRecs = App.grdCompany.selModel.selected.items;
                        //if (selRecs.length > 0) {
                        //    var params = [];
                        //    selRecs.forEach(function (record) {
                        //        params.push(record.data.CpnyID);
                        //    });
                        //    HQ.message.show(2015020806,
                        //        params.join(" & ") + "," + HQ.common.getLang("AppComp"),
                        //        'DiscDefintion.Process.deleteSelectedCompanies');
                        //}
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnDelAll_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var status = App.cboStatus.value;
                    if (status == _holdStatus) {
                        HQ.message.show(11, '', 'DiscDefintion.Process.deleteAllCompanies');
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        // Free Item

        btnAddAllFreeItem_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {

                    if (!HQ.addSameKind) {
                        var discId = App.cboDiscID.getValue();
                        var discSeq = App.cboDiscSeq.getValue();
                        var status = App.cboStatus.value;

                        if (status == _holdStatus && App.grdDiscBreak.selModel.selected.length > 0 && Main.Process.checkAddFreeItem()) {
                            var lineRef = App.grdDiscBreak.selModel.selected.items[0].data.LineRef;
                            var allNodes = DiscDefintion.Process.getLeafNodes(App.treePanelFreeItem.getRootNode());
                            if (allNodes && allNodes.length > 0) {
                                App.grdFreeItem.store.suspendEvents();

                                var invtBlank = HQ.store.findRecord(App.grdFreeItem.store, ['FreeItemID', 'LineRef'], ['', lineRef]);
                                if (!invtBlank) {
                                    var idx = App.grdFreeItem.store.data.length > 0 ? App.grdFreeItem.store.data.length - 1 : 0;
                                    App.grdFreeItem.store.insert(idx, Ext.create("App.mdlFreeItem", {
                                        DiscID: discId,
                                        DiscSeq: discSeq,
                                        FreeItemID: '',
                                        LineRef: lineRef
                                    }));
                                }
                                var allNodeLength = allNodes.length;
                                var idx = App.grdFreeItem.store.getCount() - 1;
                                for (var i = 0; i < allNodeLength; i++) {
                                    if (allNodes[i].data.Type == "Invt") {
                                        var record = HQ.store.findInStore(App.grdFreeItem.store,
                                            ['DiscID', 'DiscSeq', 'FreeItemID', 'LineRef'],
                                            [discId, discSeq, allNodes[i].data.InvtID, lineRef]);
                                        if (!record) {
                                            App.grdFreeItem.store.insert(idx, Ext.create("App.mdlFreeItem", {
                                                DiscID: discId,
                                                DiscSeq: discSeq,
                                                FreeItemID: allNodes[i].data.InvtID,
                                                LineRef: lineRef
                                            }));
                                            idx++;
                                        }
                                    }
                                }

                                App.treePanelFreeItem.clearChecked();
                                App.grdFreeItem.store.resumeEvents();
                                App.grdFreeItem.view.refresh();
                            }
                        }
                    }
                    else {
                        var checkAddFreeItem = true;
                        var lstDiscBreak =App.stoDiscBreak.data|| App.stoDiscBreak.snapshot || App.stoDiscBreak.allData ;
                        for (var i = 0; i < lstDiscBreak.items.length; i++) {
                            if (lstDiscBreak.items[i].data.BreakQty > 0 || lstDiscBreak.items[i].data.BreakAmt) {
                                if (lstDiscBreak.items[i].data.DiscAmt != 0) {
                                    checkAddFreeItem = false;
                                }
                            }
                        }
                        if (checkAddFreeItem) {
                            var discId = App.cboDiscID.getValue();
                            var discSeq = App.cboDiscSeq.getValue();
                            var status = App.cboStatus.value;

                            if (status == _holdStatus && App.grdDiscBreak.selModel.selected.length > 0 && Main.Process.checkAddFreeItem()) {
                                var lineRef = App.grdDiscBreak.selModel.selected.items[0].data.LineRef;
                                var allNodes = DiscDefintion.Process.getLeafNodes(App.treePanelFreeItem.getRootNode());
                                if (allNodes && allNodes.length > 0) {
                                    App.grdFreeItem.store.suspendEvents();

                                    var invtBlank = HQ.store.findRecord(App.grdFreeItem.store, ['FreeItemID', 'LineRef'], ['', lineRef]);
                                    if (!invtBlank) {
                                        var idx = App.grdFreeItem.store.data.length > 0 ? App.grdFreeItem.store.data.length - 1 : 0;
                                        App.grdFreeItem.store.insert(idx, Ext.create("App.mdlFreeItem", {
                                            DiscID: discId,
                                            DiscSeq: discSeq,
                                            FreeItemID: '',
                                            LineRef: lineRef
                                        }));
                                    }
                                    var allNodeLength = allNodes.length;
                                    var idx = App.grdFreeItem.store.getCount() - 1;
                                    for (var i = 0; i < allNodeLength; i++) {
                                        if (allNodes[i].data.Type == "Invt") {
                                            var record = HQ.store.findInStore(App.grdFreeItem.store,
                                                ['DiscID', 'DiscSeq', 'FreeItemID', 'LineRef'],
                                                [discId, discSeq, allNodes[i].data.InvtID, lineRef]);
                                            if (!record) {
                                                App.grdFreeItem.store.insert(idx, Ext.create("App.mdlFreeItem", {
                                                    DiscID: discId,
                                                    DiscSeq: discSeq,
                                                    FreeItemID: allNodes[i].data.InvtID,
                                                    LineRef: lineRef
                                                }));
                                                idx++;
                                            }
                                        }
                                    }

                                    App.treePanelFreeItem.clearChecked();
                                    App.grdFreeItem.store.resumeEvents();
                                    App.grdFreeItem.view.refresh();
                                }
                            }
                        }
                    }

                    if (App.chkDonateGroupProduct.getValue()) {
                        var lstAlldata = App.stoFreeItem.snapshot;
                        var check = true;
                        if (lstAlldata.items.length > 0) {
                            for (var i = 0; i < lstAlldata.items.length; i++) {
                                if (lstAlldata.items[i].data.FreeItemID != "") {///&& (lstAlldata.items[i].data.GroupItem != "" || lstAlldata.items[i].data.Priority > 0)
                                    check = false;
                                }
                            }
                        }
                        App.chkDonateGroupProduct.setReadOnly(!check);
                    }
                    else {
                        if (!App.chkAutoFreeItem.getValue() && !App.chkDonateGroupProduct.getValue()) {
                            var lstAlldata = App.stoFreeItem.snapshot;
                            var check = true;
                            if (lstAlldata.items.length > 0) {
                                for (var i = 0; i < lstAlldata.items.length; i++) {
                                    if (lstAlldata.items[i].data.FreeItemID != "") {// && (lstAlldata.items[i].data.GroupItem != "" || lstAlldata.items[i].data.Priority > 0)
                                        check = false;
                                    }
                                }
                            }
                            if (check) {
                                App.chkDonateGroupProduct.enable();
                            }
                            else {
                                App.chkDonateGroupProduct.disable();
                            }
                        }
                        if (!App.chkDonateGroupProduct.getValue()) {
                            var lstAlldata = App.stoFreeItem.snapshot;
                            var check = true;
                            if (lstAlldata.items.length > 0) {
                                for (var i = 0; i < lstAlldata.items.length; i++) {
                                    if (lstAlldata.items[i].data.FreeItemID != "" && (lstAlldata.items[i].data.GroupItem != "" || lstAlldata.items[i].data.Priority > 0)) {
                                        check = false;
                                    }
                                }
                            }
                            if (check) {
                                App.chkAutoFreeItem.enable();
                            }
                        }
                    }

                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnAddFreeItem_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    
                    if (!HQ.addSameKind) {
                        var discId = App.cboDiscID.getValue();
                        var discSeq = App.cboDiscSeq.getValue();
                        var status = App.cboStatus.value;

                        if (status == _holdStatus && App.grdDiscBreak.selModel.selected.length > 0 && Main.Process.checkAddFreeItem()) {
                            var lineRef = App.grdDiscBreak.selModel.selected.items[0].data.LineRef;
                            var allNodes = App.treePanelFreeItem.getCheckedNodes();
                            if (allNodes && allNodes.length > 0) {
                                App.grdFreeItem.store.suspendEvents();
                                var invtBlank = HQ.store.findRecord(App.grdFreeItem.store, ['FreeItemID', 'LineRef'], ['', lineRef]);
                                if (!invtBlank) {
                                    //HQ.store.insertBlank(App.grdDiscItem.store, ['InvtID']);
                                    var idx = App.grdFreeItem.store.data.length > 0 ? App.grdFreeItem.store.data.length - 1 : 0;
                                    App.grdFreeItem.store.insert(idx, Ext.create("App.mdlFreeItem", {
                                        DiscID: discId,
                                        DiscSeq: discSeq,
                                        FreeItemID: '',
                                        LineRef: lineRef
                                    }));
                                }
                                var idx = App.grdFreeItem.store.getCount() - 1;
                                allNodes.forEach(function (node) {
                                    if (node.attributes.Type == "Invt") {

                                        var record = HQ.store.findInStore(App.grdFreeItem.store,
                                            ['DiscID', 'DiscSeq', 'FreeItemID', 'LineRef'],
                                            [discId, discSeq, node.attributes.InvtID, lineRef]);
                                        if (!record) {
                                            App.grdFreeItem.store.insert(idx, Ext.create("App.mdlFreeItem", {
                                                DiscID: discId,
                                                DiscSeq: discSeq,
                                                FreeItemID: node.attributes.InvtID,
                                                LineRef: lineRef
                                            }));
                                        }
                                    }
                                });

                                App.treePanelFreeItem.clearChecked();
                                App.grdFreeItem.store.resumeEvents();
                                App.grdFreeItem.view.refresh();

                            }
                        }
                    }
                    else {
                        var checkAddFreeItem = true;
                        var lstDiscBreak =App.stoDiscBreak.data|| App.stoDiscBreak.snapshot || App.stoDiscBreak.allData ;
                        for (var i = 0; i < lstDiscBreak.items.length; i++) {
                            if (lstDiscBreak.items[i].data.BreakQty > 0 || lstDiscBreak.items[i].data.BreakAmt) {
                                if (lstDiscBreak.items[i].data.DiscAmt != 0) {
                                    checkAddFreeItem = false;
                                }
                            }
                        }
                        if (checkAddFreeItem) {
                            var discId = App.cboDiscID.getValue();
                            var discSeq = App.cboDiscSeq.getValue();
                            var status = App.cboStatus.value;

                            if (status == _holdStatus && App.grdDiscBreak.selModel.selected.length > 0 && Main.Process.checkAddFreeItem()) {
                                var lineRef = App.grdDiscBreak.selModel.selected.items[0].data.LineRef;
                                var allNodes = App.treePanelFreeItem.getCheckedNodes();
                                if (allNodes && allNodes.length > 0) {
                                    App.grdFreeItem.store.suspendEvents();
                                    var invtBlank = HQ.store.findRecord(App.grdFreeItem.store, ['FreeItemID', 'LineRef'], ['', lineRef]);
                                    if (!invtBlank) {
                                        //HQ.store.insertBlank(App.grdDiscItem.store, ['InvtID']);
                                        var idx = App.grdFreeItem.store.data.length > 0 ? App.grdFreeItem.store.data.length - 1 : 0;
                                        App.grdFreeItem.store.insert(idx, Ext.create("App.mdlFreeItem", {
                                            DiscID: discId,
                                            DiscSeq: discSeq,
                                            FreeItemID: '',
                                            LineRef: lineRef
                                        }));
                                    }
                                    var idx = App.grdFreeItem.store.getCount() - 1;
                                    allNodes.forEach(function (node) {
                                        if (node.attributes.Type == "Invt") {

                                            var record = HQ.store.findInStore(App.grdFreeItem.store,
                                                ['DiscID', 'DiscSeq', 'FreeItemID', 'LineRef'],
                                                [discId, discSeq, node.attributes.InvtID, lineRef]);
                                            if (!record) {
                                                App.grdFreeItem.store.insert(idx, Ext.create("App.mdlFreeItem", {
                                                    DiscID: discId,
                                                    DiscSeq: discSeq,
                                                    FreeItemID: node.attributes.InvtID,
                                                    LineRef: lineRef
                                                }));
                                            }
                                        }
                                    });

                                    App.treePanelFreeItem.clearChecked();
                                    App.grdFreeItem.store.resumeEvents();
                                    App.grdFreeItem.view.refresh();

                                }
                            }
                        }
                    }

                    if (App.chkDonateGroupProduct.getValue()) {
                        var lstAlldata = App.stoFreeItem.snapshot;
                        var check = true;
                        if (lstAlldata.items.length > 0) {
                            for (var i = 0; i < lstAlldata.items.length; i++) {
                                if (lstAlldata.items[i].data.FreeItemID != "") {///&& (lstAlldata.items[i].data.GroupItem != "" || lstAlldata.items[i].data.Priority > 0)
                                    check = false;
                                }
                            }
                        }
                        App.chkDonateGroupProduct.setReadOnly(!check);
                    }
                    else {
                        if (!App.chkAutoFreeItem.getValue()&& !App.chkDonateGroupProduct.getValue()) {
                            var lstAlldata = App.stoFreeItem.snapshot;
                            var check = true;
                            if (lstAlldata.items.length > 0) {
                                for (var i = 0; i < lstAlldata.items.length; i++) {
                                    if (lstAlldata.items[i].data.FreeItemID != "") {// && (lstAlldata.items[i].data.GroupItem != "" || lstAlldata.items[i].data.Priority > 0)
                                        check = false;
                                    }
                                }
                            }
                            if (check) {
                                App.chkDonateGroupProduct.enable();
                            }
                            else {
                                App.chkDonateGroupProduct.disable();
                            }
                        }
                        if (!App.chkDonateGroupProduct.getValue()) {
                            var lstAlldata = App.stoFreeItem.snapshot;
                            var check = true;
                            if (lstAlldata.items.length > 0) {
                                for (var i = 0; i < lstAlldata.items.length; i++) {
                                    if (lstAlldata.items[i].data.FreeItemID != "" && (lstAlldata.items[i].data.GroupItem != "" || lstAlldata.items[i].data.Priority > 0)) {
                                        check = false;
                                    }
                                }
                            }
                            if (check) {
                                App.chkAutoFreeItem.enable();
                            }
                        }
                    }
                    
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }



            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnDelFreeItem_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    if (App.cboStatus.value == _holdStatus) {
                        //HQ.message.show(2015020807, DiscDefintion.Process.indexSelect(App.grdFreeItem), 'Main.Process.deleteSelectedInGrid');//
                        Main.Event.menuClick('delete');
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnDelAllFreeItem_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    if (App.cboStatus.value == _holdStatus && App.grdDiscBreak.selModel.selected.length > 0) {
                        HQ.message.show(2015020806, '', 'DiscDefintion.Process.deleteAllFreeItem');
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        // Item
        btnAddAllInvt_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var status = App.cboStatus.value;

                    if (status == _holdStatus) {
                        var allNodes = DiscDefintion.Process.getLeafNodes(App.treePanelInvt.getRootNode());
                        if (allNodes && allNodes.length > 0) {
                            App.chkStockPromotion.setReadOnly(true);
                            App.grdDiscItem.store.suspendEvents();

                            var invtBlank = HQ.store.findRecord(App.grdDiscItem.store, ['InvtID'], ['']);
                            if (invtBlank) {
                                App.grdDiscItem.store.remove(invtBlank);
                            }

                            var allNodeLength = allNodes.length;
                            for (var i = 0; i < allNodeLength; i++) {
                                if (allNodes[i].data.Type == "Invt") {
                                    var record = HQ.store.findInStore(App.grdDiscItem.store,
                                        ['DiscID', 'DiscSeq', 'InvtID'],
                                        [discId, discSeq, allNodes[i].data.InvtID]);
                                    if (!record) {
                                        App.grdDiscItem.store.insert(0, Ext.create("App.mdlDiscItem", {
                                            DiscID: discId,
                                            DiscSeq: discSeq,
                                            InvtID: allNodes[i].data.InvtID,
                                            UnitDesc: allNodes[i].data.Unit,
                                            Descr: allNodes[i].data.Descr,
                                            PerStockAdvance:100
                                        }));
                                        // idx++;
                                    }
                                }
                            }
                            App.treePanelInvt.clearChecked();
                            App.grdDiscItem.store.resumeEvents();
                            App.grdDiscItem.view.refresh();
                            App.grdDiscItem.store.loadPage(1);
                            var invtBlank = HQ.store.findRecord(App.grdDiscItem.store, ['InvtID'], ['']);
                            if (!invtBlank) {
                                var idx = App.grdDiscItem.store.data.length > 0 ? App.grdDiscItem.store.data.length - 1 : 0;
                                App.grdDiscItem.store.insert(idx, Ext.create("App.mdlDiscItem", {
                                    DiscID: discId,
                                    DiscSeq: discSeq,
                                    InvtID: '',
                                    UnitDesc: '',
                                    Descr: '',
                                    PerStockAdvance: 100
                                }));
                            }

                        }
                    }

                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnAddInvt_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var status = App.cboStatus.value;

                    if (status == _holdStatus) {
                        var allNodes = App.treePanelInvt.getCheckedNodes();
                        if (allNodes && allNodes.length > 0) {
                            App.chkStockPromotion.setReadOnly(true);
                            App.grdDiscItem.store.suspendEvents();
                            var invtBlank = HQ.store.findRecord(App.grdDiscItem.store, ['InvtID'], ['']);
                            if (invtBlank) {
                                App.grdDiscItem.store.remove(invtBlank);
                            }
                            allNodes.forEach(function (node) {
                                if (node.attributes.Type == "Invt") {
                                    var idx = App.grdDiscItem.store.getCount() - 1;
                                    var record = HQ.store.findInStore(App.grdDiscItem.store,
                                        ['DiscID', 'DiscSeq', 'InvtID'],
                                        [discId, discSeq, node.attributes.InvtID]);
                                    if (!record) {
                                        App.grdDiscItem.store.insert(idx, Ext.create("App.mdlDiscItem", {
                                            DiscID: discId,
                                            DiscSeq: discSeq,
                                            InvtID: node.attributes.InvtID,
                                            UnitDesc: node.attributes.Unit,
                                            Descr: node.attributes.Descr,
                                            PerStockAdvance: 100
                                        }));
                                    }
                                }
                            });

                            App.treePanelInvt.clearChecked();
                            App.grdDiscItem.store.resumeEvents();
                            App.grdDiscItem.view.refresh();
                            App.grdDiscItem.store.loadPage(1);
                            var invtBlank = HQ.store.findRecord(App.grdDiscItem.store, ['InvtID'], ['']);
                            if (!invtBlank) {
                                var idx = App.grdDiscItem.store.data.length > 0 ? App.grdDiscItem.store.data.length - 1 : 0;
                                App.grdDiscItem.store.insert(idx, Ext.create("App.mdlDiscItem", {
                                    DiscID: discId,
                                    DiscSeq: discSeq,
                                    InvtID: '',
                                    UnitDesc: '',
                                    Descr: '',
                                    PerStockAdvance: 100
                                }));
                            }
                        }
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnDelInvt_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    if (App.cboStatus.value == _holdStatus) {
                        Main.Event.menuClick('delete');
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnDelAllInvt_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    if (App.cboStatus.value == _holdStatus) {
                        HQ.message.show(2015020806, '', 'DiscDefintion.Process.deleteAllInvts');
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        // Bundle
        btnAddAllBundle_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var status = App.cboStatus.value;

                    if (status == _holdStatus) {
                        var allNodes = DiscDefintion.Process.getLeafNodes(App.treePanelBundle.getRootNode());
                        if (allNodes && allNodes.length > 0) {
                            App.grdBundle.store.suspendEvents();
                            var invtBlank = HQ.store.findRecord(App.grdBundle.store, ['InvtID'], ['']);
                            if (invtBlank) {
                                App.grdBundle.store.remove(invtBlank);
                            }
                            allNodes.forEach(function (node) {
                                if (node.data.Type == "Invt") {
                                    var idx = App.grdBundle.store.getCount() - 1;
                                    var record = HQ.store.findInStore(App.grdBundle.store,
                                        ['DiscID', 'DiscSeq', 'InvtID'],
                                        [discId, discSeq, node.data.InvtID]);
                                    if (!record) {
                                        App.grdBundle.store.insert(idx, Ext.create("App.mdlBundle", {
                                            DiscID: discId,
                                            DiscSeq: discSeq,
                                            InvtID: node.data.InvtID,
                                            UnitDesc: node.data.Unit,
                                            Descr: node.data.Descr,
                                            BundleQty: 0,
                                            BundleAmt: 0
                                        }));
                                    }
                                }
                            });

                            App.treePanelBundle.clearChecked();
                            App.grdBundle.store.resumeEvents();
                            App.grdBundle.view.refresh();
                            App.grdBundle.store.loadPage(1);
                            var invtBlank = HQ.store.findRecord(App.grdBundle.store, ['InvtID'], ['']);
                            if (!invtBlank) {
                                var idx = App.grdBundle.store.data.length > 0 ? App.grdBundle.store.data.length - 1 : 0;
                                App.grdBundle.store.insert(idx, Ext.create("App.mdlBundle", {
                                    DiscID: discId,
                                    DiscSeq: discSeq,
                                    InvtID: '',
                                    UnitDesc: '',
                                    Descr: '',
                                    BundleQty: 0,
                                    BundleAmt: 0
                                }));
                            }
                        }
                    }

                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnAddBundle_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var status = App.cboStatus.value;

                    if (status == _holdStatus) {
                        var allNodes = App.treePanelBundle.getCheckedNodes();
                        if (allNodes && allNodes.length > 0) {
                            App.grdBundle.store.suspendEvents();
                            var invtBlank = HQ.store.findRecord(App.grdBundle.store, ['InvtID'], ['']);
                            if (invtBlank) {
                                App.grdBundle.store.remove(invtBlank);
                            }
                            var idx = App.grdBundle.store.getCount() - 1;
                            var allNodeLength = allNodes.length;
                            for (var i = 0; i < allNodeLength; i++) {
                                if (allNodes[i].attributes.Type == "Invt") {

                                    var record = HQ.store.findInStore(App.grdBundle.store,
                                        ['DiscID', 'DiscSeq', 'InvtID'],
                                        [discId, discSeq, allNodes[i].attributes.InvtID]);
                                    if (!record) {
                                        App.grdBundle.store.insert(idx, Ext.create("App.mdlBundle", {
                                            DiscID: discId,
                                            DiscSeq: discSeq,
                                            InvtID: allNodes[i].attributes.InvtID,
                                            UnitDesc: allNodes[i].attributes.Unit,
                                            Descr: allNodes[i].attributes.Descr,
                                            BundleQty: 0,
                                            BundleAmt: 0
                                        }));
                                        idx++;
                                    }
                                }
                            }
                            App.treePanelBundle.clearChecked();
                            App.grdBundle.store.resumeEvents();
                            App.grdBundle.view.refresh();
                            App.grdBundle.store.loadPage(1);
                            if (!invtBlank) {
                                var idx = App.grdBundle.store.data.length > 0 ? App.grdBundle.store.data.length - 1 : 0;
                                App.grdBundle.store.insert(idx, Ext.create("App.mdlBundle", {
                                    DiscID: discId,
                                    DiscSeq: discSeq,
                                    InvtID: '',
                                    UnitDesc: '',
                                    Descr: '',
                                    BundleQty: 0,
                                    BundleAmt: 0
                                }));
                            }
                        }
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnDelBundle_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    if (App.cboStatus.value == _holdStatus) {
                        Main.Event.menuClick('delete');
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnDelAllBundle_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    if (App.cboStatus.value == _holdStatus) {
                        HQ.message.show(2015020806, '', 'DiscDefintion.Process.deleteAllBundle');
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        // Customer
        btnAddAllCustomer_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var status = App.cboStatus.value;

                    if (status == _holdStatus) {
                        // App.frmMain.body.mask();
                        var allNodes = DiscDefintion.Process.getLeafNodes(App.treePanelCustomer.getRootNode());
                        if (allNodes && allNodes.length > 0) {
                            App.grdDiscCust.store.suspendEvents();
                            //while (App.grdDiscCust.store.data.length > 0) {
                            //    App.grdDiscCust.store.removeAt(0);
                            //}
                            //App.grdDiscCust.store.removeAll();
                            //App.grdDiscCust.store.submitData();
                            var invtBlank = HQ.store.findRecord(App.grdDiscCust.store, ['CustID'], ['']);
                            if (invtBlank) {
                                App.grdDiscCust.store.remove(invtBlank);
                            }
                            var allNodeLength = allNodes.length;
                            var idx = App.grdDiscCust.store.getCount() - 1;
                            for (var i = 0; i < allNodeLength; i++) {
                                if (allNodes[i].data.Type == "CustID") {
                                    var branch = "";
                                    var territory = "";
                                    var rec = HQ.store.findRecord(App.cboGCustID.store, ["CustID"], [allNodes[i].data.RecID]);
                                    if (rec != undefined) {
                                        branch = rec.data.BranchID;
                                        territory = rec.data.TerritoryName;
                                    }
                                    var record = HQ.store.findInStore(App.grdDiscCust.store,
                                        ['DiscID', 'DiscSeq', 'CustID', 'BranchID'],
                                        [discId, discSeq, allNodes[i].data.RecID, allNodes[i].data.BranchID]);
                                    if (!record) {
                                        App.grdDiscCust.store.insert(idx, Ext.create("App.mdlDiscCust", {
                                            DiscID: discId,
                                            DiscSeq: discSeq,
                                            CustID: allNodes[i].data.RecID,
                                            BranchID: branch,
                                            TerritoryName: territory
                                        }));
                                        idx++;
                                    }
                                }
                            }
                            App.treePanelCustomer.clearChecked();
                            App.grdDiscCust.store.resumeEvents();
                            App.grdDiscCust.view.refresh();
                            App.grdDiscCust.store.loadPage(1);
                            var invtBlank = HQ.store.findRecord(App.grdDiscCust.store, ['CustID'], ['']);
                            if (!invtBlank) {
                                var idx = App.grdDiscCust.store.data.length > 0 ? App.grdDiscCust.store.data.length - 1 : 0;
                                App.grdDiscCust.store.insert(idx, Ext.create("App.mdlDiscCust", {
                                    DiscID: discId,
                                    DiscSeq: discSeq,
                                    CustID: '',
                                    BranchID: '',
                                    TerritoryName: ''
                                }));
                            }
                            //App.frmMain.body.unmask();
                        }
                    }

                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnAddCustomer_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();
                    var status = App.cboStatus.value;

                    if (status == _holdStatus) {
                        var allNodes = App.treePanelCustomer.getCheckedNodes();
                        if (allNodes && allNodes.length > 0) {
                            App.grdDiscCust.store.suspendEvents();
                            var invtBlank = HQ.store.findRecord(App.grdDiscCust.store, ['CustID', 'BranchID'], ['', '']);
                            if (invtBlank) {
                                App.grdDiscCust.store.remove(invtBlank);
                            }

                            var idx = App.grdDiscCust.store.data.length > 0 ? App.grdDiscCust.store.data.length - 1 : 0;
                            allNodes.forEach(function (node) {
                                if (node.attributes.Type == "CustID") {
                                    var branch = "";
                                    var territory = "";
                                    var rec = HQ.store.findRecord(App.cboGCustID.store, ["CustID"], [node.attributes.RecID]);
                                    if (rec != undefined) {
                                        branch = rec.data.BranchID;
                                        territory = rec.data.TerritoryName;
                                    }
                                    var record = HQ.store.findInStore(App.grdDiscCust.store,
                                        ['DiscID', 'DiscSeq', 'CustID', 'BranchID'],
                                        [discId, discSeq, node.attributes.RecID, node.attributes.BranchID]);
                                    if (!record) {
                                        App.grdDiscCust.store.insert(idx, Ext.create("App.mdlDiscCust", {
                                            DiscID: discId,
                                            DiscSeq: discSeq,
                                            CustID: node.attributes.RecID,
                                            BranchID: branch,
                                            TerritoryName: territory
                                        }));
                                    }
                                }
                            });
                            App.treePanelCustomer.clearChecked();
                            App.grdDiscCust.store.resumeEvents();
                            App.grdDiscCust.view.refresh();
                            App.grdDiscCust.store.loadPage(1);
                            var invtBlank = HQ.store.findRecord(App.grdDiscCust.store, ['CustID', 'BranchID'], ['', '']);
                            if (!invtBlank) {
                                var idx = App.grdDiscCust.store.data.length > 0 ? App.grdDiscCust.store.data.length - 1 : 0;
                                App.grdDiscCust.store.insert(idx, Ext.create("App.mdlDiscCust", {
                                    DiscID: discId,
                                    DiscSeq: discSeq,
                                    CustID: '',
                                    BranchID: '',
                                    TerritoryName: ''
                                }));
                            }
                        }
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnDelCustomer_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    if (App.cboStatus.value == _holdStatus) {
                        Main.Event.menuClick('delete');
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnDelAllCustomer_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    if (App.cboStatus.value == _holdStatus) {
                        HQ.message.show(2015020806, '', 'DiscDefintion.Process.deleteAllCust');
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnHideTrigger_click: function (ctr) {
            if (ctr.id == 'cboDiscSeq') {
                App.cboDiscSeq.store.clearFilter();
            }
            ctr.clearValue();
        },

        slmDiscBreak_selectChange: function (grid, selected, eOpts) {
            var store = App.grdFreeItem.store;
            var keys = store.HQFieldKeys ? store.HQFieldKeys : "";
            var discID = App.cboDiscID.getValue();
            var discSeq = App.cboDiscSeq.getValue();

            store.clearFilter();
            if (selected.length > 0) {
                var objFree = HQ.store.findRecord(store, ['DiscID', 'DiscSeq', 'LineRef', 'FreeItemID'], [discID, discSeq, selected[0].data.LineRef, '']);
                if (objFree) {
                    store.remove(objFree);
                }
                store.filterBy(function (record) {
                    if (record.data.LineRef == selected[0].data.LineRef) {
                        return record;
                    }
                });

                if (selected[0].data.LineRef && App.cboStatus.getValue() == _holdStatus) {
                    if (Main.Process.isSomeValidKey(store.getRecordsValues(), keys)) {
                        var newData = {
                            DiscID: discID,
                            DiscSeq: discSeq,
                            LineRef: selected[0].data.LineRef
                        };
                        var newRec = Ext.create(store.model.modelName, newData);
                        HQ.store.insertRecord(store, keys, newRec, false);
                    }
                }
            }
            else {
                store.filterBy(function (record) {
                    //if (record.data.InvtIDGroup == selected[0].data.InvtIDGroup) {
                    //    return record;
                    //}
                });
            }
        },

        grdDiscBreak_beforeEdit: function (editor, e) {
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var status = App.cboStatus.value;
                    if (status == _holdStatus) {
                        var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

                        if (keys.indexOf(e.field) != -1) {
                            if (e.record.data.tstamp)
                                return false;
                        }

                        if (App.cboBreakBy.value == "A" && (e.field == "BreakQty" || e.field == 'BreakQtyUpper')) {
                            return false;
                        }
                        else if (App.cboBreakBy.value == "Q" && App.cboDiscClass.value != "BB"
                            && App.cboDiscClass.value != "CB" && App.cboDiscClass.value != "TB"
                            && (e.field == "BreakAmt" || e.field == 'BreakAmtUpper')) {
                            return false;
                        }
                        else if ((App.cboDiscClass.value == "BB" || App.cboDiscClass.value == "CB"
                            || App.cboDiscClass.value == "TB") && e.field == "BreakQty") {
                            return false;
                        } else if (e.field == "DiscAmt") {
                            if (App.grdFreeItem.store.getCount() > 1 && App.chkConvertDiscAmtToFreeItem.getValue() == false
                                && App.grdFreeItem.store.data.items[0].data.LineRef == e.record.data.LineRef
                                && !Ext.isEmpty(App.grdFreeItem.store.data.items[0].data.FreeItemID)) {
                                //console.log(App.grdFreeItem.store.data.items[0].data.FreeItemID);
                                return false;
                            } else {
                                var allFreeItem = App.stoFreeItem.snapshot || App.stoFreeItem.allData || App.stoFreeItem.data; // 
                                var hasFreeItem = false;
                                if (!HQ.addSameKind) {
                                    for (var i = 0; i < allFreeItem.length; i++) {
                                        if (allFreeItem.items[i].data.LineRef == e.record.data.LineRef && !Ext.isEmpty(allFreeItem.items[i].data.FreeItemID)) {
                                            hasFreeItem = true;
                                            break;
                                        }
                                    }
                                } else {
                                    for (var i = 0; i < allFreeItem.length; i++) {
                                        if (!Ext.isEmpty(allFreeItem.items[i].data.FreeItemID)) {
                                            hasFreeItem = true;
                                            break;
                                        }
                                    }
                                }
                                if (hasFreeItem && App.chkConvertDiscAmtToFreeItem.getValue() == false) {
                                    return false;
                                }
                            }
                        }
                        else if ((e.field == 'BreakQtyUpper' || e.field == 'BreakAmtUpper') && App.cboBreakBoundType.getValue() == 'L')
                            return false;
                        else if ((e.field == 'SubBreakAmt' || e.field == 'SubBreakQty' || e.field == 'SubBreakQtyUpper' || e.field == 'SubBreakAmtUpper') && App.cboSubBreakType.getValue() == 'N')
                            return false;
                        else if (App.cboSubBreakType.getValue() == 'A' && (e.field == 'SubBreakQty' || e.field == 'SubBreakQtyUpper')) return false;
                        else if (App.cboSubBreakType.getValue() == 'Q' && (e.field == 'SubBreakAmt' || e.field == 'SubBreakAmtUpper')) return false;
                        //else if (e.rowIdx > 0 && e.store.getAt(e.rowIdx - 1).data.DiscAmt == 0 && e.field == "DiscAmt") {

                        //    //return false;
                        //}
                        //if (App.cboSubBreakType.getValue() != 'N')
                        //{
                        //    if (e.rowIdx != 0) return false;
                        //}
                        return HQ.grid.checkInput(e, keys);
                    }
                    else {
                        return false;
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                    return false;
                }
            }
            else {
                return false;
            }
        },

        grdFreeItem_beforeEdit: function (editor, e) {
            _siteEdit = e.record.data.FreeITemSiteID;
            if (HQ.isUpdate) {
                if (App.frmDiscDefintionTop.isValid() && App.frmDiscSeqInfo.isValid()) {
                    var status = App.cboStatus.value;
                    if (status == _holdStatus) {
                        var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";

                        if (keys.indexOf(e.field) != -1) {
                            if (e.record.data.tstamp)
                                return false;
                        }
                        if (!App.chkDonateGroupProduct.getValue() && (e.field == "GroupItem" || e.field == "Priority") && !App.chkConvertDiscAmtToFreeItem.getValue()) {
                            return false;
                        }                        
                        // Dòng đã KM tiền thì ko tặng hàng
                        var selRec = App.slmDiscBreak.getSelection();
                        if (selRec) {
                            if (selRec[0].data.DiscAmt && App.chkConvertDiscAmtToFreeItem.getValue() == false) {
                                return false;
                            }
                        }
                        else {
                            return false;
                        }
                        // Cùng 1 DiscSeq có thể KM Tiền ở mức này và KM tặng hàng ở mức khác
                        if (HQ.addSameKind) {
                            var isAllowInsert = true;
                            for (var i = 0; i < App.grdDiscBreak.store.data.length; i++) {
                                if (App.grdDiscBreak.store.data.items[i].data.DiscAmt) {
                                    isAllowInsert = false;
                                    break;
                                }
                            }
                            if (!isAllowInsert && App.chkConvertDiscAmtToFreeItem.getValue() == false) {
                                return false;
                            }
                        }
                        if (e.store.storeId == 'stoFreeItem') {
                            App.cboGUnitDescr.store.reload();
                        }
                        return HQ.grid.checkInput(e, keys);
                    }
                    else {
                        return false;
                    }
                }
                else {
                    if (Main.Process.showFieldInvalid(App.frmDiscDefintionTop)) {
                        Main.Process.showFieldInvalid(App.frmDiscSeqInfo);
                    }
                    return false;
                }
            }
            else {
                return false;
            }
        },

        grdDiscBreak_edit: function (editor, e) {
            var totalKeys = ["BreakQty", "BreakAmt", "DiscAmt"];

            var keys = [];
            if (totalKeys.indexOf(e.field) > -1) {
                keys.push(e.field);
            }
            if (Ext.isEmpty(e.record.data.LineRef)) {
                e.record.set('LineRef', HQ.store.lastLineRef(e.store));
            }
            if (Main.Process.isSomeValidKey(e.store.getRecordsValues(), keys)) {
                var discId = App.cboDiscID.getValue();
                var discSeq = App.cboDiscSeq.getValue();

                var newData = {
                    DiscID: discId,
                    DiscSeq: discSeq,
                    LineRef: HQ.store.lastLineRef(e.store)
                };
                //if (App.cboSubBreakType.getValue() == 'N') {
                    var newRec = Ext.create(e.store.model.modelName, newData);
                    HQ.store.insertRecord(e.store, (keys.length ? keys : totalKeys), newRec, false);
                //}

                if (!App.cboDiscClass.readOnly) {
                    App.cboDiscClass.setReadOnly(true);
                    App.cboDiscType.setReadOnly(true);
                }
            }
            if (e.field == 'BreakQty' || e.field == 'BreakQtyUpper' || e.field == 'BreakAmt' || e.field == 'BreakAmtUpper' || e.field == 'SubBreakQty' || e.field == 'SubBreakAmt')
            {
                if (e.record.data.BreakQty > 0 || e.record.data.BreakAmt > 0)
                    App.cboBreakBy.setReadOnly(true);
                else
                    App.cboBreakBy.setReadOnly(false);
                if((e.record.data.SubBreakAmt > 0 || e.record.data.SubBreakQty > 0) && (e.record.data.BreakQty > 0 || e.record.data.BreakAmt > 0))
                {
                    App.cboSubBreakType.setReadOnly(true);
                    App.cboBreakBoundType.setReadOnly(true);
                }
                else
                {
                    App.cboSubBreakType.setReadOnly(false);
                    App.cboBreakBoundType.setReadOnly(false);
                }
            }
            if (e.field == "DiscAmtBonus" && (e.record.data.BreakAmt != 0 || e.record.data.BreakQty!=0)) {
                if (e.record.data.DiscAmtBonus == 0) {
                    HQ.message.show(15, [HQ.common.getLang("Const")], '', true);
                }
            }
        },

        grdFreeItem_edit: function (editor, e,value,old) {
            var keys = e.store.HQFieldKeys ? e.store.HQFieldKeys : "";
            var selRecs = App.grdDiscBreak.selModel.selected;
            if (Ext.isEmpty(e.record.data.LineRef) && selRecs.length > 0) {
                e.record.set('LineRef', selRecs.items[0].data.LineRef);
            }
            if (keys.indexOf(e.field) != -1) {
                var recordValues = e.store.getRecordsValues();
                if (e.value != ''
                    && Main.Process.isAllValidKey(recordValues, keys)) {
                    var discId = App.cboDiscID.getValue();
                    var discSeq = App.cboDiscSeq.getValue();

                    if (selRecs.length > 0) {
                        var newData = {
                            DiscID: discId,
                            DiscSeq: discSeq,
                            LineRef: selRecs.items[0].data.LineRef
                        };

                        var newRec = Ext.create(e.store.model.modelName, newData);
                        HQ.store.insertRecord(e.store, keys, newRec, false);

                        if (!App.cboDiscClass.readOnly) {
                            App.cboDiscClass.setReadOnly(true);
                            App.cboDiscType.setReadOnly(true);
                        }

                    }
                }
            }
            if (e.field == "FreeItemID") {
                var rec = App.cboFreeItemID.store.findRecord("InvtID", e.value);
               if (rec) {
                   e.record.set('TypeUnit', rec.data.TypeUnit);
                   if (!App.chkDonateGroupProduct.getValue()) {
                       App.chkDonateGroupProduct.disable();
                   }
               }
               
            }
            if (e.field == "UnitDescr") {
                if (App.chkDonateGroupProduct.getValue()) {
                    var type = e.record.data.TypeUnit;
                    var rec = App.cboGUnitDescr.store.findRecord("FromUnit", e.value);
                    if (rec) {
                        key = true;
                        var listdata = App.grdFreeItem.store.data.items;
                        for (var i = 0; i < listdata.length; i++) {
                            if (listdata[i].data.GroupItem == e.record.data.GroupItem && listdata[i].data.FreeItemID != e.record.data.FreeItemID) {
                                if (listdata[i].data.TypeUnit != rec.data.TypeUnit && rec.data.TypeUnit != 3 && listdata[i].data.TypeUnit != 3 && e.record.data.GroupItem != "" && e.record.data.GroupItem != null) {
                                    key = false;
                                    
                                }
                            }
                        }
                        if (key) {
                            e.record.set('TypeUnit', rec.data.TypeUnit);
                        }
                        else {
                            e.record.set('UnitDescr', '');
                            e.record.set('TypeUnit', type);
                            HQ.message.show(2018031411, [e.record.data.FreeItemID, e.rowIdx + 1, e.record.data.GroupItem], '', true);                            
                            return false;
                        }
                    }
                }
                else {
                    var rec = App.cboGUnitDescr.store.findRecord("FromUnit", e.value);
                    if (rec) {
                        e.record.set('TypeUnit', rec.data.TypeUnit);
                    }
                }
                
            }
            if (e.field == "GroupItem" || e.field == "Priority") {
                var lstAlldata = App.stoFreeItem.snapshot;
                var check = true;
                if (lstAlldata.items.length > 0) {
                    for (var i = 0; i < lstAlldata.items.length; i++) {
                        if (lstAlldata.items[i].data.FreeItemID != "" && (lstAlldata.items[i].data.GroupItem != "" || lstAlldata.items[i].data.Priority > 0)) {
                            check = false;
                        }
                    }
                }
                App.chkDonateGroupProduct.setReadOnly(!check);
            }
            
            if (App.chkDonateGroupProduct.getValue()) {
                var listdata = App.grdFreeItem.store.data.items;
                for (var i = 0; i < listdata.length; i++) {
                    if (listdata[i].data.GroupItem == e.record.data.GroupItem) {
                        if (listdata[i].data.TypeUnit != e.record.data.TypeUnit && e.record.data.TypeUnit != 3 && listdata[i].data.TypeUnit != 3 && e.record.data.GroupItem!="" && e.record.data.GroupItem!=null) {
                            if (e.field == 'GroupItem') {
                                e.record.set('GroupItem', '');
                            }
                            else if (e.field == 'UnitDescr') {
                                e.record.set('UnitDescr', '');
                            }    
                            HQ.message.show(2018031411, [e.record.data.FreeItemID, e.rowIdx + 1, e.record.data.GroupItem], '', true);
                            return false;
                        }
                    }
                }
            }
            if(e.field=="FreeITemSiteID")
            {
                //var lstCompany = App.grdCompany.store.snapshot || App.grdCompany.store.allData || App.grdCompany.store.data;
                //if(lstCompany!=undefined){
                //    for(var i=0;i<lstCompany.length;i++){
                //        _branchID +=lstCompany.items[i].data.CpnyID + ",";
                //    }
                //}

                //if (_branchID == '')
                //{
                //    HQ.message.show(2018081663, [HQ.common.getLang("AppComp")], '', true);
                //    return false;
                //}
                var tam = "@@@@@@@@@@";
                App.cboFreeITemSiteWhseLoc.store.clearFilter();
                if (e.record.data.FreeITemSiteID != "" && e.record.data.FreeITemSiteID != null) {
                    tam = e.record.data.FreeITemSiteID;
                }
                App.cboFreeITemSiteWhseLoc.store.filter('SiteID', tam);
                if (e.record.data.FreeITemSiteID != _siteEdit) {
                    e.record.set("FreeITemSiteWhseLoc", "");
                }
            }
            
        },
    }
};

/// ///////// Expand Collapse /////////////////////////////////////////////////////////////
var expandAll = function (tree) {
    if (tree.getRootNode() != undefined) {
        Ext.suspendLayouts();
        tree.getRootNode().expand();
        var length = tree.getRootNode().childNodes.length;
        if (length > 0) {
            for (var i = 0; i < length; i++) {
                if (tree.getRootNode().childNodes[i].childNodes.length > 0) {
                    expandNode(tree.getRootNode().childNodes[i]);
                }
            }
        }
        Ext.resumeLayouts(true);
    }
}

var expandNode = function (node) {
    node.expand();
    if (node.childNodes.length > 0) {
        for (var i = 0; i < node.childNodes.length; i++) {
            expandNode(node.childNodes[i]);
        }
    }
}

var collapseNode = function (node) {
    if (node != undefined) {
        node.collapse();
        if (node.childNodes.length > 0) {
            for (var i = 0; i < node.childNodes.length; i++) {
                collapseNode(node.childNodes[i]);
            }
        }
    }
}

var collapseAll = function (tree) {
    if (tree.getRootNode() != undefined) {
        Ext.suspendLayouts();
        tree.getRootNode().collapse();
        var length = tree.getRootNode().childNodes.length;
        if (length > 0) {
            for (var i = 0; i < length; i++) {
                if (tree.getRootNode().childNodes[i].childNodes.length > 0) {
                    collapseNode(tree.getRootNode().childNodes[i]);
                }
            }
        }
        Ext.resumeLayouts(true);
    }
}


var tree_ItemCollapse = function (a, b) {
    collapseNode(a);
}
////////////////////////////////////////////////////////////
////Export/////////////////////////////////////////////

//var cboExport_Select = function (sender, value) {

//    //HQ.isFirstLoad = true;
//    //if (App.cboExport.valueModels) {
//    App.txtExport.value = App.cboExport.getValue();
//    //}


//};


var btnExport1_Click = function (record) {

    App.winExport.setTitle("Export")
    App.winExport.show();

};
var btnExportCancel_Click = function (sender, value) {
    App.winExport.hide();
}

//btnExportTemplate_Click
var forcuscboExport = function () {
    App['cboExport'].focus();
};
var btnExport_Click = function () {
    if (App.cboExport.getValue() != null && App.cboExport.getValue() != "") {
        App.frmMain.submit({
           waitMsg: HQ.common.getLang("Exporting"),
            url: 'OM21100/Export',
            type: 'POST',
            timeout: 1000000,
            clientValidation: false,
            params: {
                //lstCustTD: Ext.encode(App.stoDiscBreak.getRecordsValues()),// HQ.store.getData(App.stoAR_CustomerTD)
                //lstCustTD1: Ext.encode(App.cboExport.getValue()),
                templateExport: App.cboExport.getValue()
            },
            success: function (msg, data) {
                window.location = 'OM21100/DownloadAndDelete?file=' + data.result.fileName;
                App.winExport.hide();
                App.cboExport.setValue('');
                //setTimeout(function () {
                //    HQ.message.show(5);
                //}, 5000);
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
    else {
        HQ.message.show(1000, App.cboExport.fieldLabel, 'forcuscboExport');
    }
    
};

/////////////////////Import///////////////////////
var btnImport_Click = function (sender, e) {
    var fileName = sender.getValue();
    var ext = fileName.split(".").pop().toLowerCase();
    if (ext == "xls" || ext == "xlsx") {
        App.frmMain.submit({
            waitMsg: "Importing....",
            url: 'OM21100/Import',
            timeout: 18000000,
            clientValidation: false,
            method: 'POST',
            params: {
            },
            success: function (msg, data) {
                HQ.isChange = false;
                HQ.isFirstLoad = true;
                App.cboDiscID.store.reload();
                App.cboDiscSeq.store.reload();
                if (!Ext.isEmpty(this.result.data.message)) {
                    HQ.message.show('2013103001', [this.result.data.message], '', true);
                }
                else {
                    HQ.message.process(msg, data, true);
                }
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
    else {
        HQ.message.show('2014070701', ext, '');
        sender.reset();
    }
};
var cboRequiredType_Change = function () {
    var colRequiredValueIndex = HQ.grid.findColumnIndex(App.grdDiscItem.columns, 'RequiredValue');
    if (HQ.showRequiredType) {
        if (App.cboRequiredType.getValue() == 'Q' || App.cboRequiredType.getValue() == 'N') {
            App.grdDiscItem.columns[colRequiredValueIndex].setText(HQ.common.getLang('RequiredValue'));
            HQ.grid.show(App.grdDiscItem, ['RequiredValue']);
        }
        else {
            if (App.cboRequiredType.getValue() == 'A') {
                App.grdDiscItem.columns[colRequiredValueIndex].setText(HQ.common.getLang('RequiredValueAmount'));
                HQ.grid.show(App.grdDiscItem, ['RequiredValue']);
            }
            else
                HQ.grid.hide(App.grdDiscItem, ['RequiredValue']);
        }
    }
};
//var chkRequiredType_Change = function () {
//    var reqType = '';
    
//    if (App.chkRequiredType.getValue() && App.cboDiscType.getValue() == 'G') {
//        reqType = 'Q';
//        HQ.grid.show(App.grdDiscItem, ['RequiredValue']);
//    } else {
//        HQ.grid.hide(App.grdDiscItem, ['RequiredValue']);
//    }

//    //var isEnableRequiredType = (!App['pnlDPII'].isDisabled() && cbo.value == "Q");
//    //if (!isEnableRequiredType) {
//    //    App.chkRequiredType.setValue(false);
//    //    //App.txtRequiredType.setValue('');
//    //    HQ.grid.hide(App.grdDiscItem, ['RequiredValue']);
//    //} else {
//    //    HQ.grid.show(App.grdDiscItem, ['RequiredValue']);
//    //}


//    App.txtRequiredType.events['change'].suspend();
//    App.txtRequiredType.setValue(reqType);
//    App.txtRequiredType.events['change'].resume();
//}

//var txtRequiredType_Change = function () {
//    var isCheck = false;
//    if (App.txtRequiredType.getValue() == 'Q' && App.cboDiscType.getValue() == 'G') {
//        isCheck = true;
//        HQ.grid.show(App.grdDiscItem, ['RequiredValue']);
//    } else {
//        HQ.grid.hide(App.grdDiscItem, ['RequiredValue']);
//    }
//    App.chkRequiredType.events['change'].suspend();
//    App.chkRequiredType.setValue(isCheck);
//    App.chkRequiredType.events['change'].resume();
//}
var Status_Change = function () {
    App.cboHandle.store.reload();
    if (App.cboStatus.getValue() == "C") {
        //App.chkRequiredType.setReadOnly(true);
        App.chkPctDiscountByLevel.setReadOnly(true);
    }
    else {
        //App.chkRequiredType.setReadOnly(false);
        App.chkPctDiscountByLevel.setReadOnly(false);
    }
}


var dteStartDate_Change = function (item) {
    if (item.hasFocus) {
        var a = App.dteStartDate.getValue();
        var b = App.dteEndDate.getValue();
        try {
            var year = a.getFullYear();
            var day = a.getDate();
            var month = a.getMonth();
        }
        catch (e) {

        }
        if (year != undefined && day != undefined && month != undefined) {
            if (a == "" || a == null) {
                App.dteEndDate.setValue('');
            }
            else {
                //App.dteEndDate.setMinValue(App.dteStartDate.getValue());
                if (b != "" && b != null) {
                    if (a > b) {
                        App.dteEndDate.setValue('');
                    }
                }
            }
        }        
    }   
}
var dteEndDate_Change = function () {
    App.dteStartDate.setMaxValue(App.dteEndDate.getValue());
    if (App.dteEndDate.getValue() != null) {
         if (App.dteEndDate.getValue() < App.dteStartDate.getValue()) {
                App.dteStartDate.setValue(App.dteEndDate.getValue());
            }
    }
   
}
var dteEndDate_Focus = function () {
    if (Ext.isEmpty(App.cboDiscSeq.getValue())) {
        HQ.message.show(15, [App.cboDiscSeq.fieldLabel], '', true);
        return false;
    }
    else {

        //App.dteEndDate.setMinValue(App.dteStartDate.getValue());
    }
}
var chkDonateGroupProduct_Change = function (chk, newValue, oldValue, eOpts) {
    if (Ext.isEmpty(App.cboDiscSeq.getValue())) {
        if (chk.hasFocus) {
            HQ.message.show(15, [App.cboDiscSeq.fieldLabel], '', true);
            chk.suspendEvents();
            App.chkDonateGroupProduct.setValue(false);
            chk.resumeEvents();
            return false;
        }        
    }
    else {
        if (newValue) {
            //App.chkAutoFreeItem.setValue(false);
            //App.chkAutoFreeItem.setReadOnly(true);
            App.GroupItem.show();
            App.Priority.show();
            if (!App.chkAutoFreeItem.getValue()) {
                App.chkAutoFreeItem.disable();
            }
        }
        else {
            App.GroupItem.hide();
            App.Priority.hide();
            var check = true;
            //var lstDataFree = App.stoFreeItem.snapshot || App.stoFreeItem.allData || App.stoFreeItem.data;
            //for (var i = 0; i < lstDataFree.items.length; i++) {
            //    if (lstDataFree.items[i].data.FreeItemID != "" && lstDataFree.items[i].data.FreeItemID != null) {
            //        check = false;
            //    }
            //}
            //if (check) {
            //    App.chkAutoFreeItem.enable();
            //}
            //else {
            //    App.chkAutoFreeItem.disable();
            //}
            if (App.cboProAplForItem.getValue() != 'M') {
                App.chkAutoFreeItem.enable();
            }
            else {
                App.chkAutoFreeItem.disable();
            }
            

        }
        if (chk.hasFocus) {
            var lstFreeItemcheck = App.stoFreeItem.snapshot || App.stoFreeItem.allData || App.stoFreeItem.data;
            if (lstFreeItemcheck.items.length > 0) {
                for (var i = 0; i < lstFreeItemcheck.items.length; i++) {
                    if ((lstFreeItemcheck.items[i].data.GroupItem != null && lstFreeItemcheck.items[i].data.GroupItem != "") || (lstFreeItemcheck.items[i].data.Priority > 0)) {
                        HQ.message.show(2018022211, '', '', true);
                        chk.suspendEvents();
                        App.chkDonateGroupProduct.setValue(oldValue);
                        chk.resumeEvents();
                        return false;
                        break;
                    }
                }
            }
        }        
    }
}

var chkDiscPrice_Change = function (chk, newValue, oldValue, eOpts) {
    if (Ext.isEmpty(App.cboDiscSeq.getValue())) {
        if (chk.hasFocus) {
            HQ.message.show(15, [App.cboDiscSeq.fieldLabel], '', true);
            chk.suspendEvents();
            App.chkDiscPrice.setValue(false);
            chk.resumeEvents();
            return false;
        }
    }
};



var rowindex = function (lineRef) {
    var lstdata=App.grdDiscBreak.store.snapshot.items;
    if (lstdata.length > 0) {
        var index = 0;
        for (var i = 0; i < lstdata.length; i++) {
            if (lstdata[i].data.LineRef == lineRef) {
                index = i + 1;
                break;
            }
        }
    }
    return index;
}

function tabMain_Change(obj, tab, c, func){
    if (tab.id == "pnlDPCC") {
        var checkActive = false;
        var lstDataCpny = App.stoCompany.data
        for (var i = 0; i < App.stoCompany.data.length; i++) {
            if (!Ext.isEmpty(App.stoCompany.data.items[i].data.CpnyID)) {
                checkActive = true;
            }
        }
        if (checkActive) {
            DiscDefintion.Event.treeCustomer_AfterRender('treeCustomer');
        }
    }       

    if (tab.id == "pnlDPII") {
        var colRequiredValueIndex = HQ.grid.findColumnIndex(App.grdDiscItem.columns, 'RequiredValue');
        if (HQ.showRequiredType) {
            if (App.cboRequiredType.getValue() == 'Q' || App.cboRequiredType.getValue() == 'N') {
                App.grdDiscItem.columns[colRequiredValueIndex].setText(HQ.common.getLang('RequiredValue'));
                HQ.grid.show(App.grdDiscItem, ['RequiredValue']);
            }
            else {
                if (App.cboRequiredType.getValue() == 'A') {
                    App.grdDiscItem.columns[colRequiredValueIndex].setText(HQ.common.getLang('RequiredValueAmount'));
                    HQ.grid.show(App.grdDiscItem, ['RequiredValue']);
                }
                else
                    HQ.grid.hide(App.grdDiscItem, ['RequiredValue']);
            }
        }
        if(HQ.hideCoefficientCnv)
            HQ.grid.show(App.grdDiscItem, ['CoefficientCnv']);
        else
            HQ.grid.hide(App.grdDiscItem, ['CoefficientCnv']);
        if(HQ.hidePriorityPromo)
        {
            if (App.cboDiscType.getValue() != 'D')
                HQ.grid.show(App.grdDiscItem, ['PriorityInvt']);
            else
                HQ.grid.hide(App.grdDiscItem, ['PriorityInvt']);
        }
    }
    if (tab.id == "pnlDiscDefintion")
    {
        if (App.stoDiscItem.data.length == 1 && App.stoDiscItem.data.items[0].data.InvtID == "")
            App.cboRequiredType.setReadOnly(false);
        else
            App.cboRequiredType.setReadOnly(true);
    }
}
var btnCopy_Click = function () {
    App.winCopy.show();
    App.cboDiscIDCopy.isValid();
    App.txtDiscSeq.isValid();
}
var cboDiscIDCopy_Change = function () {
    App.storPromotionsCopy.reload();
}
var grdPromotionsCopy_beforeEdit = function (editor, e) {
    if (e.record.data.Selected == false) {
        var count = 0;
        for (var j = 0; j < App.storPromotionsCopy.data.length; j++) {
            if (App.grdPromotionsCopy.store.data.items[j].data.Selected == true) {
                count++;
            }
        }
        if (count > 0) {
            return false;
        }
    }
}
var btnSaveCopy_click = function () {
    if (App.cboDiscIDCopy.getValue() == '' || App.cboDiscIDCopy.getValue() == null) {
        HQ.message.show(15, [App.cboDiscIDCopy.fieldLabel], '', true);
        return false;
    }
    else if (App.txtDiscSeq.getValue() == '' || App.txtDiscSeq.getValue() == null) {
        HQ.message.show(15, [App.txtDiscSeq.fieldLabel], '', true);
        return false;
    }
    else if (HQ.store.findInStore(App.storPromotionsCopy, ["Selected"], ["true"]) == undefined) {
        HQ.message.show(2018080960);
        return false;
    }
    App.frmCopy.submit({
        waitMsg: HQ.common.getLang("SavingData"),
        url: 'OM21100/SaveDataCopy',
        timeout: 10000000,
        params: {
            lstPromotionsCopy: Ext.encode(App.storPromotionsCopy.getRecordsValues()),
            DiscIDNew : App.cboDiscIDCopy.getValue() ,
            DiscSeqNew: App.txtDiscSeq.getValue()
        },
        success: function (msg, data) {
            if (data.result.msgCode) {
                HQ.message.show(data.result.msgCode);
            }
            else {
                HQ.message.show(201405071);
            }
            App.cboDiscID.setValue(App.cboDiscIDCopy.getValue());
            App.cboDiscSeq.setValue(App.txtDiscSeq.getValue());
            _discSeqNew = App.txtDiscSeq.getValue();
            //Main.Process.reloadAllData();
            App.cboDiscIDCopy.clearValue();
            App.storPromotionsCopy.reload();
            App.txtDiscSeq.setValue('');
            _copy = 1;
            App.winCopy.close();
        },
        failure: function (msg, data) {
            //if (data.result.msgCode) {
            //    HQ.message.show(data.result.msgCode);
            //}
            //else {
                HQ.message.process(msg, data, true);
            //}
        }
    });
}
var slmFreeItem_select = function (slm, selRec, idx, eOpts) {
    _siteID = selRec.data.FreeITemSiteID;
    App.cboFreeITemSiteWhseLoc.store.reload();
}
var cboFreeITemSiteID_change = function(chk, newValue, oldValue, eOpts)
{
    _siteID = newValue;
    App.cboFreeITemSiteWhseLoc.store.reload();
    App.cboFreeITemSiteWhseLoc.clearValue();
}
var joinParams = function (multiCombo) {
    var returnValue = "";
    if (multiCombo.value && multiCombo.value.length) {
        returnValue = multiCombo.value.join();
    }
    else {
        if (multiCombo.getValue()) {
            returnValue = multiCombo.rawValue;
        }
    }

    return returnValue;
};
var txtDiscSeq_Blur = function () {
    var regex = /^(\w*(\d|[a-zA-Z]|[\_@()+-.]))*$/;
    checkSpecialChar(App.txtDiscSeq.getValue());
    if (!HQ.util.passNull(App.txtDiscSeq.getValue()) == '' && !HQ.util.passNull(App.txtDiscSeq.getValue().toString()).match(regex)) {
        App.txtDiscSeq.setValue('');
    }
    
}
var checkSpecialChar = function (value) {
    var regex = /^(\w*(\d|[a-zA-Z]|[\_@()+-.]))*$/;
    if (value)
        if (!HQ.util.passNull(value) == '' && !HQ.util.passNull(value.toString()).match(regex)) {
            HQ.message.show(20140811, App.txtDiscSeq.fieldLabel);
            return false;
        }
};