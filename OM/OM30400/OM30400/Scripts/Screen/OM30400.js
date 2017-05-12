var _FlagZeroResult = false;

var pointsArray = [];

var frmMain_BoxReady = function () {
    App.pnlInfo.setActiveTab(2);
    if (hideButtonPosition == 'true')
        App.btnGetCurrentLocation.hide();
    else
        App.btnGetCurrentLocation.show();
};

// JS Code for Index View
var Index = {
    joinParams: function (multiCombo) {
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
    },

    menuClick: function (cmd) {
        switch (cmd) {
            case "refresh":
                {
                    if (!App.pnlMCP.hidden) {
                        if (App.pnlMCP.isValid()) {
                            App.grdVisitCustomerPlan.store.reload();
                        }
                        else {
                            App.pnlMCP.getForm().getFields().each(
                                function (item) {
                                    if (!item.isValid()) {
                                        item.focus();
                                        return false;
                                    }
                                }
                            );
                        }
                    }
                    else if (!App.pnlMCL.hidden) {
                        if (App.pnlMCL.isValid()) {
                            App.grdMCL.store.reload();
                        }
                        else {
                            App.pnlMCL.getForm().getFields().each(
                                function (item) {
                                    if (!item.isValid()) {
                                        item.focus();
                                        return false;
                                    }
                                }
                            );
                        }
                    }
                    else if (!App.pnlActualVisit.hidden) {
                        if (App.pnlActualVisit.isValid()) {
                            if (App.radSalesmanAll.value) {
                                App.grdAllCurrentSalesman.store.reload();
                            }
                            else {
                                App.stoVisitPlan.reload();
                                //App.grdVisitCustomerActual.store.reload();
                                //App.storeMapActualVisit.reload();
                                //App.storeVisitCustomerActual.reload();
                            }
                        }
                        else {
                            App.pnlActualVisit.getForm().getFields().each(
                                function (item) {
                                    if (!item.isValid()) {
                                        item.focus();
                                        return false;
                                    }
                                }
                            );
                        }
                    }
                    else if (!App.pnlCustHistory.hidden) {
                        if (App.pnlCustHistory.isValid()) {
                            App.grdCustHistory.store.reload();
                        }
                        else {
                            App.pnlCustHistory.getForm().getFields().each(
                                function (item) {
                                    if (!item.isValid()) {
                                        item.focus();
                                        return false;
                                    }
                                }
                            );
                        }
                    }
                }
                break;

            case "close":
                break;
        }
    },

    removeStoreData: function () {
        var stores = [
                App.grdVisitCustomerPlan.store,
                App.grdMCL.store,
                App.grdAllCurrentSalesman.store,
                App.grdVisitCustomerActual.store,
                App.storeMapActualVisit,
                //App.storeVisitCustomerActual,
                App.grdCustHistory.store,
                App.stoVisitPlan
        ]

        for (var i = 0; i < stores.length; i++) {
            var store = stores[i];
            if (store.getCount()) {
                store.removeAll();
            }
        }
    },

    btnHideTrigger_click: function (sender) {
        sender.clearValue();
    },

    // tab MCP

    cboAreaPlan_change: function (cbo, newValue, oldValue, eOpts) {
        App.cboProvincePlan.store.reload();
        App.cboDistributorPlan.store.reload();
    },

    cboDistributorPlan_change: function (cbo, newValue, oldValue, eOpts) {
        App.cboSalesManPlan.store.reload();
    },

    addDays: function (theDate, days) {
        return new Date(theDate.getTime() + days * 24 * 60 * 60 * 1000);
    },

    getMonday: function (d) {
        d = new Date(d);
        var day = d.getDay(),
            diff = d.getDate() - day + (day == 0 ? -6 : 1); // adjust when day is sunday
        return new Date(d.setDate(diff));
    },

    btnLoadDataPlan_click: function (btn, e, eOpts) {
        if (App.pnlMCP.isValid()) {
            Index.removeStoreData();
            App.grdVisitCustomerPlan.store.reload();
        }
        else {
            App.pnlMCP.getForm().getFields().each(
                function (item) {
                    if (!item.isValid()) {
                        item.focus();
                        return false;
                    }
                }
            );
        }
    },

    storeVisitCustomerPlan_load: function (store, records, successful, eOpts) {
        if (successful) {
            var markers = [];
            records.forEach(function (record) {
                var marker = {
                    "id": record.index + 1,
                    "title": record.data.CustId + ": " + record.data.CustName,
                    "lat": record.data.Lat,
                    "lng": record.data.Lng,
                    "visitSort": record.data.VisitSort,
                    "description":
                        '<div id="content">' +
                            '<div id="siteNotice">' +
                            '</div>' +
                            '<h1 id="firstHeading" class="firstHeading">' +
                                record.data.CustName +
                            '</h1>' +
                            '<div id="bodyContent">' +
                                '<p>' +
                                    record.data.Addr +
                                '</p>' +
                            '</div>' +
                        '</div>'
                }
                markers.push(marker);
            });
            PosGmap.drawMCP(markers, false);
           
        }
        App.frmMain.getEl().unmask();
    },

    selectionModelVisitCustomerPlan_Select: function (rowModel, record, index, eOpts) {
        if (record) {
            PosGmap.navMapCenterByLocation(record.data.Lat, record.data.Lng, record.index + 1);
        }
    },

    grdVisitCustomerActual_cellClick: function (gridview, td, cellIndex, record, tr, rowIndex, e, eOpts) {
        var clickCol = gridview.up('grid').columns[cellIndex];
        if (clickCol) {
            if (record.data.TypeMapPlan == '0') {
                if (clickCol.dataIndex === "Checkout") {
                    var id = record.data.Checkout + "_" + record.data.CoLat + "_" + record.data.CoLng;
                    PosGmap.navMapCenterByLocation(record.data.CoLat, record.data.CoLng, id);
                }
                else if (clickCol.dataIndex === "Checkin") {
                    var id = record.data.Checkin + "_" + record.data.CiLat + "_" + record.data.CiLng;
                    PosGmap.navMapCenterByLocation(record.data.CiLat, record.data.CiLng, id);
                }
                else {
                    if (record.data.CustId && App.chkShowAgent.value) {
                        var id = record.data.CustId + "_" + record.data.CustLat + "_" + record.data.CustLng;
                        PosGmap.navMapCenterByLocation(record.data.CustLat, record.data.CustLng, id);
                    }
                    else {
                        var id = record.data.Checkin + "_" + record.data.CiLat + "_" + record.data.CiLng;
                        PosGmap.navMapCenterByLocation(record.data.CiLat, record.data.CiLng, id);
                    }
                }
            }
            else {
                var id = '1_' + record.data.CustId + '_' + record.data.CustLat + '_' + record.data.CustLng;
                PosGmap.navMapCenterByLocation(record.data.CustLat, record.data.CustLng, id);
            }
        }
    },
    // tab MCL

    cboAreaMCL_change: function (cbo, newValue, oldValue, eOpts) {
        App.cboProvinceMCL.store.reload();
        App.cboDistributorMCL.store.reload();
    },

    cboDistributorMCL_change: function (cbo, newValue, oldValue, eOpts) {
        App.cboSalesManMCL.store.reload();
    },

    btnLoadDataMCL_click: function (btn, e, eOpts) {
        if (App.pnlMCL.isValid()) {
            Index.removeStoreData();
            App.grdMCL.store.reload();
        }
        else {
            App.pnlMCL.getForm().getFields().each(
                function (item) {
                    if (!item.isValid()) {
                        item.focus();
                        return false;
                    }
                }
            );
        }
    },

    pnlGridMCL_viewGetRowClass: function (record, rowIndex, rowParams, store) {
        return "row-" + record.data.Color;
    },

    onComboBoxSelect: function (cbo) {
        App.grdMCL.store.pageSize = parseInt(cbo.getValue(), 10);
        App.grdMCL.store.reload();
    },

    getDisplayDaysOfWeek: function (record) {
        var totalStrs = [];
        if (record.data.Sun) {
            totalStrs.push(HQ.common.getLang("Sun"));
        }
        if (record.data.Mon) {
            totalStrs.push(HQ.common.getLang("Mon"));
        }
        if (record.data.Tue) {
            totalStrs.push(HQ.common.getLang("Tue"));
        }
        if (record.data.Wed) {
            totalStrs.push(HQ.common.getLang("Wed"));
        }
        if (record.data.Thu) {
            totalStrs.push(HQ.common.getLang("Thu"));
        }
        if (record.data.Fri) {
            totalStrs.push(HQ.common.getLang("Fri"));
        }
        if (record.data.Sat) {
            totalStrs.push(HQ.common.getLang("Sat"));
        }
        return totalStrs.join(", ");
    },

    createMarkerDescription: function (record) {
        var dayOfWeekStr = Index.getDisplayDaysOfWeek(record);
        return '<div id="content">' +
            '<div id="siteNotice">' +
                '<b>' +
                (record.data.Name ? (record.data.Name + ', ' + record.data.Distributor) : (record.data.Distributor)) +
                '</b>' +
            '</div>' +
            '<h2 id="firstHeading" class="firstHeading">' +
                record.data.CustName +
            '</h2>' +
            '<h3>' +
                record.data.CustId +
            '</h3>' +
            '<div id="bodyContent">' +
                '<p>' +
                    record.data.Addr1 +
                '</p>' +
                '<p>' +
                    (record.data.SlsFreq ? (HQ.common.getLang("SLSFREQ") + ': ' + record.data.SlsFreq + '<br/>') : '') +
                    (record.data.WeekofVisit ? (HQ.common.getLang(record.data.WeekofVisit) + '<br/>') : '') +
                    (record.data.VisitSort ? (HQ.common.getLang("VisitSort") + ': ' + record.data.VisitSort + '<br/>') : '') +
                    (dayOfWeekStr ? (' (' + dayOfWeekStr + ')') : '') +
                '</p>' +
                '<a class="x-btn-default-toolbar-small-icon" href="javascript: McpInfo.editFromMap(\'' + record.data.CustId + '\',\'' + record.data.SlsperId + '\',\'' + record.data.BranchID + '\') ">edit</a>' +
            '</div>' +
        '</div>';
    },

    storeMCL_load: function (store, records, successful, eOpts) {
        if (successful) {
            var markers = [];

            store.each(function (record) {
                var marker = {
                    "id": record.index + 1,
                    "title": record.data.CustId + ": " + record.data.CustName,
                    "lat": record.data.Lat,
                    "lng": record.data.Lng,
                    "color": record.data.Color,
                    "description": Index.createMarkerDescription(record),
                    "custId": record.data.CustId
                }
                markers.push(marker);
            });
            PosGmap.drawMCL(markers, false);
            if (markers.length) {
                PosGmap.prepairDrawing();
            }
        }
        App.frmMain.getEl().unmask();

        //drawingManager.setOptions({
        //    drawingControl: true
        //});
    },

    grdMCL_commandEdit: function (item, command, record, index, eOpts) {
        if (command == "Edit") {
            //App.storeMcpInfo.load({
            //    params: {
            //        brachID: record.data.BranchID,
            //        custID: record.data.CustId,
            //        slsPerID: record.data.SlsperId
            //    }, scope: this
            //});
            App.txtCustIDMcpInfo.setValue(record.data.CustId);
            App.txtCustNameMcpInfo.setValue(record.data.CustName);
            App.txtAddressMcpInfo.setValue(record.data.Addr1);
            App.hdnSlsperIDMcpInfo.setValue(record.data.SlsperId);
            App.txtSlsperIDMcpInfo.setValue(record.data.SlsperId + "_" + record.data.Name);
            App.hdnBranchIDMcpInfo.setValue(record.data.BranchID);
            App.txtDistributorMcpInfo.setValue(record.data.Distributor);
            App.chkCustStatusMcpInfo.setValue(record.data.Status == "A" ? true : false);

            App.storeMcpInfo.serverProxy.url =
                Ext.String.format("OM30400/LoadSalesRouteMaster?branchID={0}&custID={1}&slsPerID={2}", record.data.BranchID, record.data.CustId, record.data.SlsperId);
            App.storeMcpInfo.reload();
        }
    },

    selectionModelMCL_Select: function (rowModel, record, index, eOpts) {
        if (record) {
            PosGmap.navMapCenterByLocation(record.data.Lat, record.data.Lng, record.index + 1);
        }
    },

    // tab Actual visit
    cboDistributorActual_change: function (cbo, newValue, oldValue, eOpts) {
        App.cboSalesManActual.store.reload();
    },

    radSalesmanAll_change: function (rad, newValue, oldValue, eOpts) {
        if (rad.value) {
            App.cboSalesManActual.clearValue();
            App.cboSalesManActual.disable();
            App.btnGetCurrentLocation.disable();
            App.chkRealTime.disable();
            App.chkShowAgent.disable();
            App.chkMapPlan.disable();
            App.btnExportExcelActual.disable();

            App.grdVisitCustomerActual.hide();
            App.grdAllCurrentSalesman.show();
        }
        else {
            App.cboSalesManActual.enable();
            App.btnGetCurrentLocation.enable();
            App.chkRealTime.enable();
            App.chkShowAgent.enable();
            App.chkMapPlan.enable();
            App.btnExportExcelActual.enable();

            App.grdVisitCustomerActual.show();
            App.grdAllCurrentSalesman.hide();
        }
    },

    btnLoadDataActual_click: function (btn, e, eOpts) {
        if (App.pnlActualVisit.isValid()) {
            App.chkMapPlan.setValue(false);
            Index.removeStoreData();
            clearMaps();
            if (App.radSalesmanAll.value) {
                App.grdAllCurrentSalesman.store.reload();
            }
            else {
                App.stoVisitPlan.reload();

                //App.grdVisitCustomerActual.store.reload();
                //App.storeMapActualVisit.reload();
                
                //App.storeVisitCustomerActual.reload();
            }
        }
        else {
            App.pnlActualVisit.getForm().getFields().each(
                function (item) {
                    if (!item.isValid()) {
                        item.focus();
                        return false;
                    }
                }
            );
        }
    },

    grdVisitCustomerActual_viewGetRowClass: function (record, rowIndex, rowParams, store) {
        if (record.data.CustId && !record.data.Amt) {
            return "row-FF0000";
        }
    },

    renderColorInOut: function (value, metaData, record, rowIndex, colIndex, store) {
        if (value) {
            if (record.data.CustId) {
                if (metaData.column.dataIndex == "Checkin") {
                    metaData.tdAttr = 'style="background-color: #CCFF33"';//#CCFF33
                }
                else if (metaData.column.dataIndex == "Checkout") {
                    metaData.tdAttr = 'style="background-color: #FF0000"';
                }
            }
        }

        return value;
    },

    renderColor: function (value, metaData, record, rowIndex, colIndex, store) {
        metaData.style = "color:#" + record.data.Color + "!IMPORTANT;";

        if (metaData.column.dataIndex == "Amt") {
            return Ext.util.Format.number(value, '0,000');
        }
        else if (metaData.column.dataIndex == "Distance") {
            return Ext.util.Format.number(value, '0,000');
        }

        return value;
    },

    renderShopDistance: function (value, metaData, record, rowIndex, colIndex, store) {
        var lat1 = 0;
        var lng1 = 0;
        var lat2 = record.data.CiLat;
        var lng2 = record.data.CiLng;
        var distance = 0;

        if (rowIndex > 0) {
            var record1 = store.getAt(rowIndex - 1);
            if (record1) {
                lat1 = record1.data.CiLat;
                lng1 = record1.data.CiLng;
            }
        }

        if (lat1 && lng1 && lat2 && lng2) {
            distance = google.maps.geometry.spherical.computeDistanceBetween(new google.maps.LatLng(lat1, lng1), new google.maps.LatLng(lat2, lng2));
        }

        Index.renderColor(value, metaData, record, rowIndex, colIndex, store);
        return Ext.util.Format.number(Math.round(distance), '0,000');
    },

    btnGetCurrentLocation_click: function (btn, e, eOpts) {
        var store = App.storeGridActualVisit;
        if (store.getCount() > 0) {
            var lastRecord = store.getAt(store.getCount() - 1);
            PosGmap.navMapCenterByLocation(lastRecord.data.CoLat, lastRecord.data.CoLng, lastRecord.index + 1);

            App.SelectionModelVisitCustomerActual.select(store.getCount() - 1);
        }
    },

    chkMapPlan_change: function (chk, newValue, oldValue, eOpts) {
        if (chk.hasFocus) {
            if (newValue) {
                for (i = 0; i < PosGmap.planMarkers.length; i++) {
                    if (PosGmap.planMarkers[i].type == 'plan') {
                        PosGmap.planMarkers[i].setMap(true ? PosGmap.map : null);
                    }
                }
                for (i = 0; i < PosGmap.directionsDisplays.length; i++) {
                    if (PosGmap.directionsDisplays[i].type == 'plan') {
                        PosGmap.directionsDisplays[i].setMap(true ? PosGmap.map : null);
                    }
                }
                App.grdVisitCustomerActual.getFilterPlugin().clearFilters();
                App.grdVisitCustomerActual.getFilterPlugin().getFilter('TypeMapPlan').setValue(['0','1']);
                App.grdVisitCustomerActual.getFilterPlugin().getFilter('TypeMapPlan').setActive(true);
            }
            else {
                for (i = 0; i < PosGmap.planMarkers.length; i++) {
                    if (PosGmap.planMarkers[i].type == 'plan') {
                        PosGmap.planMarkers[i].setMap(false ? PosGmap.map : null);
                    }
                }
                for (i = 0; i < PosGmap.directionsDisplays.length; i++) {
                    if (PosGmap.directionsDisplays[i].type == 'plan') {
                        PosGmap.directionsDisplays[i].setMap(false ? PosGmap.map : null);
                    }
                }
                App.grdVisitCustomerActual.getFilterPlugin().clearFilters();
                App.grdVisitCustomerActual.getFilterPlugin().getFilter('TypeMapPlan').setValue('0');
                App.grdVisitCustomerActual.getFilterPlugin().getFilter('TypeMapPlan').setActive(true);
            }
        }
    },

    chkRealTime_change: function (chk, newValue, oldValue, eOpts) {
        Index.btnLoadDataActual_click();
    },

    chkShowAgent_change: function (chk, newValue, oldValue, eOpts) {
        PosGmap.stopMarkers.forEach(function (marker) {
            if (marker.type && marker.type == "CC") {
                marker.set("visible", chk.value);
            }
        });
    },

    btnExportExcelActual_click: function (btn, eOpts) {
        if (App.pnlActualVisit.isValid()) {
            Ext.net.DirectMethod.request({
                url: "OM30400/ExportExcelActual",
                isUpload: true,
                formProxyArg: "pnlActualVisit",
                cleanRequest: true,
                timeout: 1000000,
                params: {
                    distributor: Index.joinParams(App.cboDistributorActual),
                    slsperId: App.cboSalesManActual.value,
                    visitDate: App.dateVisit.value.toDateString(),
                    realTime: App.chkRealTime.value
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    },

    stoVisitPlan_load:function(store,records,successful,eOpts){
        if (successful) {
            if (store.totalCount > 0) {
                HQ.common.showBusy(true, HQ.common.getLang('Loading Maps'));
                PosGmap.drawMap_Visit();
                App.grdVisitCustomerActual.getFilterPlugin().clearFilters();
                App.grdVisitCustomerActual.getFilterPlugin().getFilter('TypeMapPlan').setValue('0');
                App.grdVisitCustomerActual.getFilterPlugin().getFilter('TypeMapPlan').setActive(true);
            }
            else {
                App.grdVisitCustomerActual.store.reload();
                App.storeMapActualVisit.reload();
            }
        }
    },

    storeGridActualVisit_load: function (store, records, successful, eOpts) {
        if (successful) {
            if (store.totalCount == 0) {
                HQ.common.showBusy(false);
                _FlagZeroResult = false;
            }
            else if (store.totalCount > 0 && App.stoVisitPlan.totalCount == 0)
                HQ.common.showBusy(true, HQ.common.getLang('Loading Maps'));
            var index = 0;
            var markers = [];
            records.forEach(function (record) {
                // chi ve nhung TypeMapPlan = 0 , = 1 la cua plan
                if (record.data.TypeMapPlan == '0') {
                    if (record.data.CustId) {
                        index = index + 1;
                    }
                    else if (App.chkRealTime.value) {
                        index = index + 1;
                    }

                    var markeri = {
                        "id": record.data.Checkin + "_" + record.data.CiLat + "_" + record.data.CiLng,
                        "title": record.data.CustId ? (record.data.CustId + ": " + record.data.CustName) : "",
                        "lat": record.data.CiLat,
                        "lng": record.data.CiLng,
                        "label": record.data.CustId ? index : (App.chkRealTime.value ? index : ""),
                        "type": record.data.CustId ? "IO" : "",
                        "color": record.data.Color,
                        "isNotVisited": record.data.IsNotVisited,
                        "description":
                            '<div id="content">' +
                                '<div id="siteNotice">' +
                                '</div>' +
                                '<h1 id="firstHeading" class="firstHeading">' +
                                    record.data.CustName +
                                '</h1>' +
                                '<div id="bodyContent">' +
                                    '<p>' +
                                        'Thời gian check in - out: ' + record.data.Checkin + ' - ' + record.data.Checkout +
                                    '</p>' +
                                    '<p>' +
                                        'Doanh số: ' + record.data.TurnOver +
                                    '</p>' +
                                    '<p>' +
                                        'Địa chỉ: ' + record.data.Addr +
                                    '</p>' +
                                    (!record.data.PicPath ? '' : ('<a target="_blank" href="' + record.data.PicPath + '">' +
                                        '<img width="200px" src="' + record.data.PicPath + '" />' +
                                    '</a>')) +
                                '</div>' +
                            '</div>'
                    }
                    markers.push(markeri);

                    if (record.data.CustId) {
                        var markero = {
                            "id": record.data.Checkout + "_" + record.data.CoLat + "_" + record.data.CoLng,
                            "title": record.data.CustId + ": " + record.data.CustName,
                            "lat": record.data.CoLat,
                            "lng": record.data.CoLng,
                            "label": index,
                            "type": "OO",
                            "color": "C60BBF",
                            "isNotVisited": record.data.IsNotVisited,
                            "description":
                                '<div id="content">' +
                                    '<div id="siteNotice">' +
                                    '</div>' +
                                    '<h1 id="firstHeading" class="firstHeading">' +
                                        record.data.CustName +
                                    '</h1>' +
                                    '<div id="bodyContent">' +
                                        '<p>' +
                                            record.data.Checkin + ' - ' + record.data.Checkout +
                                        '</p>' +
                                        '<p>' +
                                            record.data.Addr +
                                        '</p>' +
                                        (!record.data.PicPath ? '' : ('<a target="_blank" href="' + record.data.PicPath + '">' +
                                        '<img width="200px" src="' + record.data.PicPath + '" />' +
                                    '</a>')) +
                                    '</div>' +
                                '</div>'
                        }
                        markers.push(markero);

                        var markerc = {
                            "id": record.data.CustId + "_" + record.data.CustLat + "_" + record.data.CustLng,
                            "title": record.data.CustId + ": " + record.data.CustName,
                            "lat": record.data.CustLat,
                            "lng": record.data.CustLng,
                            "label": index,
                            "type": "CC",
                            "color": "01DFD7",
                            "isNotVisited": record.data.IsNotVisited,
                            "description":
                                '<div id="content">' +
                                    '<div id="siteNotice">' +
                                    '</div>' +
                                    '<h1 id="firstHeading" class="firstHeading">' +
                                        record.data.CustName +
                                    '</h1>' +
                                    '<div id="bodyContent">' +
                                        '<p>' +
                                            record.data.Checkin + ' - ' + record.data.Checkout +
                                        '</p>' +
                                        '<p>' +
                                            record.data.Addr +
                                        '</p>' +
                                        (!record.data.PicPath ? '' : ('<a target="_blank" href="' + record.data.PicPath + '">' +
                                        '<img width="200px" src="' + record.data.PicPath + '" />' +
                                    '</a>')) +
                                    '</div>' +
                                '</div>'
                        }
                        markers.push(markerc);
                    }
                }
            });
            PosGmap.drawAVC1(markers, true, App.chkRealTime.value, App.chkShowAgent.value);
        }
        App.frmMain.getEl().unmask();
    },

    // Store nay tam thoi ko dung toi nua
    storeVisitCustomerActual_load: function (store, records, successful, eOpts) {
        if (successful) {
            var markers = [];
            records.forEach(function (record) {
                var marker = {
                    "id": Ext.Date.format(record.data.VisitDate, "H:i:s") + "_" + record.data.Lat + "_" + record.data.Lng, // lay gio
                    "title": record.data.CustId + ": " + record.data.CustName,
                    "lat": record.data.Lat,
                    "lng": record.data.Lng,
                    "type": record.data.Type,
                    "color": record.data.Color,
                    "description":
                        '<div id="content">' +
                            '<div id="siteNotice">' +
                            '</div>' +
                            '<h1 id="firstHeading" class="firstHeading">' +
                                record.data.CustName +
                            '</h1>' +
                            '<div id="bodyContent">' +
                                '<p>' +
                                    record.data.Addr +
                                '</p>' +
                            '</div>' +
                        '</div>'
                }
                markers.push(marker);
            });
            PosGmap.drawAVC(markers, true);
        }

        // pretend to change chkRealTime check
        // Index.chkRealTime_change(App.chkRealTime);
        App.frmMain.getEl().unmask();
    },

    storeAllCurrentSalesman_load: function (store, records, successful, eOpts) {
        if (successful) {
            var markers = [];
            records.forEach(function (record) {
                var marker = {
                    "id": record.index + 1,
                    "title": record.data.SlsperID + ": " + record.data.Name,
                    "lat": record.data.Lat,
                    "lng": record.data.Lng,
                    "description":
                        '<div id="content">' +
                            '<div id="siteNotice">' +
                            '</div>' +
                            '<h1 id="firstHeading" class="firstHeading">' +
                                record.data.SlsperID + ": " + record.data.Name +
                            '</h1>' +                            
                            '<div id="bodyContent">' +
                                '<p style = "font-size: 18px;font-weight: bold;">' +
                                        (record.data.Phone ? (HQ.common.getLang('Phone') + ": " + record.data.Phone) : "") +
                                '</p>' +
                                '<p>' 
                                      +  
                                    (record.data.CustId ? (record.data.CustId + ": " + record.data.CustName + " - ") : "") + record.data.Addr +
                                '</p>' +
                            '</div>' +
                        '</div>'
                }
                markers.push(marker);
            });
            PosGmap.drawRoutes(markers, false);
        }
        App.frmMain.getEl().unmask();
    },

    // tab history
    cboDistributorHistory_change: function (cbo, newValue, oldValue, eOpts) {
        App.cboSalesManHistory.store.reload();
        App.cboCustomerHistory.store.reload();
    },

    cboSalesManHistory_change: function (cbo, newValue, oldValue, eOpts) {
        App.cboCustomerHistory.store.reload();
    },

    btnLoadDataHistory_click: function (btn, e, eOpts) {
        if (App.pnlCustHistory.isValid()) {
            Index.removeStoreData();
            App.grdCustHistory.store.reload();
        }
        else {
            App.pnlCustHistory.getForm().getFields().each(
                function (item) {
                    if (!item.isValid()) {
                        item.focus();
                        return false;
                    }
                }
            );
        }
    },

    storeCustHistory_load: function (store, records, successful, eOpts) {
        if (successful) {
            var markers = [];
            records.forEach(function (record) {
                var marker = {
                    "id": record.index + 1,
                    "title": record.data.CustID + ": " + record.data.CustName,
                    "lat": record.data.Lat,
                    "lng": record.data.Lng,
                    "description":
                        '<div id="content">' +
                            '<div id="siteNotice">' +
                            '</div>' +
                            '<h1 id="firstHeading" class="firstHeading">' +
                                Ext.Date.format(record.data.VisitDate, 'd-m-Y H:i') +
                            '</h1>' +
                            '<div id="bodyContent">' +
                                '<p>' +
                                    record.data.Addr +
                                '</p>' +
                            '</div>' +
                        '</div>'
                }
                markers.push(marker);
            });
            PosGmap.drawRoutes(markers, false);
        }
        App.frmMain.getEl().unmask();
    },

    //

    selectionModelPOS_ActualVisit_Select: function (rowModel, record, index, eOpts) {
        if (record) {
            var id = record.data.Checkin + "_" + record.data.CiLat + "_" + record.data.CiLng;
            PosGmap.navMapCenterByLocation(record.data.CiLat, record.data.CiLng, id);
        }
    },

    grdActualVisit_cellClick: function (gridview, td, cellIndex, record, tr, rowIndex, e, eOpts) {
        var clickCol = App.grdActualVisit.columns[cellIndex];
        if (clickCol) {
            if (clickCol.dataIndex === "CheckOut") {
                var id = record.data.Checkout + "_" + record.data.CoLat + "_" + record.data.CoLng;
                PosGmap.navMapCenterByLocation(record.data.CoLat, record.data.CoLng, id);
            }
            else if (clickCol.dataIndex === "CheckIn") {
                var id = record.data.Checkin + "_" + record.data.CiLat + "_" + record.data.CiLng;
                PosGmap.navMapCenterByLocation(record.data.CiLat, record.data.CiLng, id);
            }
        }
    },

    // Show the tooltip in grid
    onShow: function (toolTip, grid, isHtmlEncode) {
        var view = grid.getView(),
            store = grid.getStore(),
            record = view.getRecord(view.findItemByChild(toolTip.triggerElement)),
            column = view.getHeaderByCell(toolTip.triggerElement),
            data = record.get(column.dataIndex);

        if (data) {
            if (isHtmlEncode) {
                toolTip.update(Ext.util.Format.htmlEncode(data));
            }
            else {
                toolTip.update(data);
            }
        }
        else {
            toolTip.hide();
        }
    },

    exportCustomer: function (custIDs) {
        Ext.net.DirectMethod.request({
            url: "OM30400/ExportCustomer",
            isUpload: true,
            formProxyArg: "frmMain",
            cleanRequest: true,
            timeout: 1000000,
            params: {
                custIDs: custIDs
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

// JS Code for Mcp Info window

var McpInfo = {
    editFromMap: function (custId, slsperId, branchId) {
        var record = HQ.store.findRecord(App.grdMCL.store,
                        ["CustId", "SlsperId", "BranchID"],
                        [custId, slsperId, branchId]);
        if (record) {
            Index.grdMCL_commandEdit(null, "Edit", record, 0);
        }
    },

    storeMcpInfo_load: function (store, records, successful, eOpts) {
        App.winMcpInfo.show();
        var record;
        if (store.getCount() > 0) {
            App.btnDeleteMcpInfo.enable();
        }
        else {
            record = Ext.create("App.OM_SalesRouteMasterModel", {
                BranchID: App.hdnBranchIDMcpInfo.value,
                CustID: App.txtCustIDMcpInfo.value,
                SlsperID: App.hdnSlsperIDMcpInfo.value
            });
            store.insert(0, record);
            App.btnDeleteMcpInfo.disable();
        }
        var record = store.getAt(0);
        App.frmMcpInfo.loadRecord(record);
    },

    chkCustStatusMcpInfo_change: function (chk, value) {
        if (value) {
            App.fdsContentMcp.enable();
        }
        else {
            App.fdsContentMcp.disable();
        }
    },

    btnSaveMcpInfo_click: function () {
        McpInfo.saveMcp();
    },

    btnDeleteMcpInfo_click: function () {
        HQ.message.show(11, '', 'McpInfo.deleteMcpInfo');
    },

    btnCancelMcpInfo_click: function () {
        App.winMcpInfo.close();
    },

    deleteMcpInfo: function (item) {
        if (item == "yes") {
            App.storeMcpInfo.removeAt(0);
            McpInfo.saveMcp();
        }
    },

    saveMcp: function () {
        if (App.frmMcpInfo.isValid()) {
            App.frmMcpInfo.updateRecord();
            //var record = App.frmMcpInfo.getRecord();
            App.frmMcpInfo.submit({
                waitMsg: 'Submiting...',
                url: 'OM30400/SaveMcp',
                params: {
                    custActive: App.chkCustStatusMcpInfo.value,
                    lstMcpInfo: Ext.encode(App.storeMcpInfo.getChangedData({ skipIdForPhantomRecords: false }))
                },
                success: function (action, data) {
                    if (data.result.msgcode) {
                        HQ.message.show(data.result.msgCode, (data.result.msgParam ? data.result.msgParam : ''), '');
                    }
                    App.winMcpInfo.close();

                    var record = HQ.store.findRecord(App.grdMCL.store,
                        ["CustId", "SlsperId", "BranchID"],
                        [data.result.CustID, data.result.SlsPerID, data.result.BranchID]);
                    if (record) {
                        var fields = ["Color", "SlsFreq", "WeekofVisit", "VisitSort", "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Status"];
                        for (var i = 0; i < fields.length; i++) {
                            if (data.result[fields[i]] != undefined) {
                                record.set(fields[i], data.result[fields[i]]);
                            }

                            if (i == fields.length - 1) {
                                var selectedMarker = PosGmap.find_closest_marker(record.data.Lat, record.data.Lng);
                                if (selectedMarker) {
                                    google.maps.event.addListener(selectedMarker, "click", function (e) {
                                        infoWindow.setContent(Index.createMarkerDescription(record));
                                        infoWindow.open(map, selectedMarker);
                                    });

                                    selectedMarker.icon = "Images/OM30400/circle_" + data.result.Color + ".png";
                                    if (selectedMarker.getAnimation() != null) {
                                        selectedMarker.setAnimation(null);
                                    } else {
                                        selectedMarker.setAnimation(google.maps.Animation.BOUNCE);
                                        setTimeout(function () {
                                            selectedMarker.setAnimation(null);
                                        }, 1400);
                                    }
                                }
                            }
                        }
                    }
                },

                failure: function (errorMsg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode, (data.result.msgParam ? data.result.msgParam : ''), '');
                    }
                    else {
                        HQ.message.process(errorMsg, data, true);
                    }
                }
            });
        }
    }
};

var clearMaps = function () {
    if (PosGmap.planMarkers != null) {
        for (var i = 0; i < PosGmap.planMarkers.length; i++) {
            PosGmap.planMarkers[i].setMap(null);
        }
    }
    if (PosGmap.directionsDisplays != null) {
        for (var i = 0; i < PosGmap.directionsDisplays.length; i++) {
            PosGmap.directionsDisplays[i].setMap(null);
        }
    }
    if (PosGmap.stopMarkers != null) {
        for (var i = 0; i < PosGmap.stopMarkers.length; i++) {
            PosGmap.stopMarkers[i].setMap(null);
        }
    }
    PosGmap.planPoints = [];
    PosGmap.stopMarkers = [];
    PosGmap.directionsDisplays = [];
    //for (i = 0; i < stopMarkers.length; i++) {
    //    stopMarkers[i].setMap(null);

    //    if (i == stopMarkers.length - 1) {
    //        stopMarkers = [];
    //    }
    //}
    //PosGmap.directionsDisplay.setMap(PosGmap.map);
};

// JS Code for POS Gmap
var PosGmap = {
    map_canvas: {},
    map: {},
    directionsService: {},
    directionsDisplay: {},
    directionsDisplays: [],
    infoWindow: {},
    stopMarkers: [],
    drawingManager: {},
    planMarkers: [],
    planPoints: [],
    //timeoutRequest: [500,900,700,900],
    //y: 0,


    initialize: function () {
        PosGmap.map_canvas = document.getElementById("map_canvas");
        var viewlocate = defaultLocation.split(',');
        var initLatLng = new google.maps.LatLng(Number(viewlocate[0]), Number(viewlocate[1]), Number(viewlocate[2]));
        var myOptions = {
            center: initLatLng,
            zoom: 16,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        };

        PosGmap.map = new google.maps.Map(PosGmap.map_canvas, myOptions);
        PosGmap.directionsService = new google.maps.DirectionsService();
        PosGmap.directionsDisplay = new google.maps.DirectionsRenderer();
        //PosGmap.directionsDisplay = new google.maps.DirectionsRenderer({
        //    map: PosGmap.map,
        //    type: 'actual',
        //    polylineOptions: {
        //        icons: [
        //          {
        //              icon:
        //              {
        //                  path: google.maps.SymbolPath.FORWARD_CLOSED_ARROW,
        //                  scale: 2,
        //                  fillOpacity: 1,
        //                  strokeColor: match ? '#4285F4' : '#FF3328',
        //                  fillColor: match ? '#4285F4' : '#FF3328',
        //                  strokeOpacity: 1
        //              },
        //              repeat: '300px'
        //          }
        //        ],
        //        strokeColor: match ? '#4285F4' : '#FF3328',
        //        strokeOpacity: 1
        //    }
        //});

        PosGmap.infoWindow = new google.maps.InfoWindow();

        //var marker = new google.maps.Marker({
        //    position: initLatLng,
        //    map: map,
        //    title: "HQ Soft"
        //});
        PosGmap.stopMarkers = [];

        App.stoColorHint.load(function () {
            PosGmap.showCustomControl();
        });
    },

    prepairDrawing: function () {

        PosGmap.drawingManager = new google.maps.drawing.DrawingManager({
            //drawingMode: google.maps.drawing.OverlayType.MARKER,
            drawingControl: true,
            drawingControlOptions: {
                position: google.maps.ControlPosition.TOP_CENTER,
                drawingModes: [
                  //google.maps.drawing.OverlayType.MARKER,
                  //google.maps.drawing.OverlayType.CIRCLE,
                  google.maps.drawing.OverlayType.POLYGON
                  //google.maps.drawing.OverlayType.POLYLINE,
                  //google.maps.drawing.OverlayType.RECTANGLE
                ]
            },
            circleOptions: {
                fillColor: '#ffff00',
                fillOpacity: 0,
                strokeWeight: 5,
                clickable: true,
                editable: true,
                zIndex: 1
            }
        });
        PosGmap.drawingManager.setMap(PosGmap.map);

        if (!google.maps.Polygon.prototype.getBounds) {
            google.maps.Polygon.prototype.getBounds = function () {
                var bounds = new google.maps.LatLngBounds();
                this.getPath().forEach(function (element, index) {
                    bounds.extend(element);
                });
                return bounds;
            }
        }

        // Polygon containsLatLng - method to determine if a latLng is within a polygon
        google.maps.Polygon.prototype.containsLatLng = function (latLng) {
            // Exclude points outside of bounds as there is no way they are in the poly
            var lat, lng;
            //arguments are a pair of lat, lng variables
            if (arguments.length == 2) {
                if (typeof arguments[0] == "number" && typeof arguments[1] == "number") {
                    lat = arguments[0];
                    lng = arguments[1];
                }
            } else if (arguments.length == 1) {
                var bounds = this.getBounds();

                if (bounds !== null && !bounds.contains(latLng)) {
                    return false;
                }
                lat = latLng.lat();
                lng = latLng.lng();
            } else {
                console.log("Wrong number of inputs in google.maps.Polygon.prototype.contains.LatLng");
            }

            // Raycast point in polygon method
            var inPoly = false;

            var numPaths = this.getPaths().getLength();
            for (var p = 0; p < numPaths; p++) {
                var path = this.getPaths().getAt(p);
                var numPoints = path.getLength();
                var j = numPoints - 1;

                for (var i = 0; i < numPoints; i++) {
                    var vertex1 = path.getAt(i);
                    var vertex2 = path.getAt(j);

                    if (vertex1.lng() < lng && vertex2.lng() >= lng || vertex2.lng() < lng && vertex1.lng() >= lng) {
                        if (vertex1.lat() + (lng - vertex1.lng()) / (vertex2.lng() - vertex1.lng()) * (vertex2.lat() - vertex1.lat()) < lat) {
                            inPoly = !inPoly;
                        }
                    }

                    j = i;
                }
            }

            return inPoly;
        }

        google.maps.event.addListener(PosGmap.drawingManager, 'overlaycomplete', function (event) {
            if (event.type == google.maps.drawing.OverlayType.POLYGON) {
                var custIDs = [];
                for (var i = 0; i < PosGmap.stopMarkers.length; i++) {
                    if (event.overlay.containsLatLng(PosGmap.stopMarkers[i].position)) {
                        custIDs.push(PosGmap.stopMarkers[i].custId);
                    }
                }
                if (custIDs.length) {
                    var labelPoint = event.overlay.getPath().getAt(0);
                    var label = new MarkerWithLabel({
                        icon: " ",
                        position: labelPoint,
                        draggable: false,
                        map: PosGmap.map,
                        labelContent: custIDs.length.toString(),
                        labelAnchor: new google.maps.Point(22, 0),
                        labelClass: "labels", // the CSS class for the label
                        labelStyle: { opacity: 0.75 }
                    });
                }
                PosGmap.showContextMenu(map, event.overlay, label, custIDs);
            }
        });
    },

    showContextMenu: function (objMap, polygon, label, custIDs) {
        //	create the ContextMenuOptions object
        var contextMenuOptions = {};
        contextMenuOptions.classNames = { menu: 'context_menu', menuSeparator: 'context_menu_separator' };

        //	create an array of ContextMenuItem objects
        var menuItems = [];
        menuItems.push({ className: 'context_menu_item', eventName: 'export_excel', label: 'Export excel' });
        menuItems.push({ className: 'context_menu_item', eventName: 'clear_zone', label: 'Clear zone' });
        //	a menuItem with no properties will be rendered as a separator
        menuItems.push({});
        menuItems.push({ className: 'context_menu_item', eventName: 'zoom_in_click', label: 'Zoom in' });
        menuItems.push({ className: 'context_menu_item', eventName: 'zoom_out_click', label: 'Zoom out' });
        //	a menuItem with no properties will be rendered as a separator
        menuItems.push({});
        menuItems.push({ className: 'context_menu_item', eventName: 'center_map_click', label: 'Center map here' });
        contextMenuOptions.menuItems = menuItems;

        //	create the ContextMenu object
        var contextMenu = new ContextMenu(objMap, contextMenuOptions);

        //	display the ContextMenu on a Map right click
        google.maps.event.addListener(polygon, 'rightclick', function (mouseEvent) {
            //if (polygon.containsLatLng(mouseEvent.latLng)) {
            contextMenu.show(mouseEvent.latLng);
            //}
        });

        //	listen for the ContextMenu 'menu_item_selected' event
        google.maps.event.addListener(contextMenu, 'menu_item_selected', function (latLng, eventName) {
            //	latLng is the position of the ContextMenu
            //	eventName is the eventName defined for the clicked ContextMenuItem in the ContextMenuOptions
            switch (eventName) {
                case 'export_excel':
                    if (custIDs.length) {
                        Index.exportCustomer(custIDs);
                    }
                    contextMenu.hide();
                    break;
                case 'clear_zone':
                    polygon.setMap(null);
                    label.setMap(null);
                    contextMenu.hide();
                    break;
                case 'zoom_in_click':
                    map.setZoom(objMap.getZoom() + 1);
                    contextMenu.hide();
                    break;
                case 'zoom_out_click':
                    map.setZoom(objMap.getZoom() - 1);
                    contextMenu.hide();
                    break;
                case 'center_map_click':
                    map.panTo(latLng);
                    contextMenu.hide();
                    break;
            }
        });
    },

    navMapCenterByLocation: function (lat, lng, id) {
        var selectedMarker;
        var myLatlng = new google.maps.LatLng(lat, lng);
        PosGmap.map.setCenter(myLatlng);
        PosGmap.map.setZoom(PosGmap.map.getZoom());

        if (id) {
            selectedMarker = PosGmap.find_marker_id(id);
            if (!selectedMarker) {
                selectedMarker = PosGmap.find_closest_marker(lat, lng);
            }
        }
        else {
            selectedMarker = PosGmap.find_closest_marker(lat, lng);
        }

        if (selectedMarker) {
            var visible = selectedMarker.visible;

            if (selectedMarker.getAnimation() != null) {
                selectedMarker.setAnimation(null);
            } else {
                if (!visible) {
                    selectedMarker.visible = true;
                }
                selectedMarker.setAnimation(google.maps.Animation.BOUNCE);
                setTimeout(function () {
                    selectedMarker.setAnimation(null);
                    if (!visible) {
                        selectedMarker.visible = false;
                    }
                }, 1400);
            }
        }
    },

    find_marker_id: function (id) {
        for (i = 0; i < PosGmap.planMarkers.length; i++) {
            if (PosGmap.planMarkers[i].data.id == id) {
                return PosGmap.planMarkers[i];
            }
        }

        for (i = 0; i < PosGmap.stopMarkers.length; i++) {
            if (PosGmap.stopMarkers[i].id == id) {
                return PosGmap.stopMarkers[i];
            }
        }
        return null;
    },

    find_closest_marker: function (lat1, lon1) {
        var pi = Math.PI;
        var R = 6371; //equatorial radius
        var distances = [];
        var closest = -1;

        for (i = 0; i < PosGmap.stopMarkers.length; i++) {
            var lat2 = PosGmap.stopMarkers[i].position.lat();
            var lon2 = PosGmap.stopMarkers[i].position.lng();

            var chLat = lat2 - lat1;
            var chLon = lon2 - lon1;

            var dLat = chLat * (pi / 180);
            var dLon = chLon * (pi / 180);

            var rLat1 = lat1 * (pi / 180);
            var rLat2 = lat2 * (pi / 180);

            var a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
                        Math.sin(dLon / 2) * Math.sin(dLon / 2) * Math.cos(rLat1) * Math.cos(rLat2);
            var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
            var d = R * c;

            distances[i] = d;
            if (closest == -1 || d < distances[closest]) {
                closest = i;
            }
        }

        // (debug) The closest marker is:
        return PosGmap.stopMarkers[closest];
    },

    prepairMap: function () {
        if (PosGmap.directionsDisplay) {
            PosGmap.directionsDisplay.setMap(null);
            if (PosGmap.directionsDisplays && PosGmap.directionsDisplays.length > 0) {
                for (var i = 0; i < PosGmap.directionsDisplays.length; i++) {
                    PosGmap.directionsDisplays[i].setMap(null);
                    //directionsDisplay[i] = new google.maps.DirectionsRenderer();
                }
            }
        }
        PosGmap.directionsService = new google.maps.DirectionsService();
        PosGmap.directionsDisplay = new google.maps.DirectionsRenderer();

    },

    clearMap: function (stopMarkers) {
        for (i = 0; i < stopMarkers.length; i++) {
            stopMarkers[i].setMap(null);

            if (i == stopMarkers.length - 1) {
                stopMarkers = [];
            }
        }
        PosGmap.directionsDisplay.setMap(PosGmap.map);

    },

    drawMCP: function (markers, showDirections) {
        PosGmap.prepairMap();

        if (markers.length > 0) {
            PosGmap.stopMarkers = [];
            // List of locations
            var lat_lng = new Array();

            // For each marker in list
            for (i = 0; i < markers.length; i++) {
                var data = markers[i];
                if (data.lat && data.lng) {
                    var myLatlng = new google.maps.LatLng(data.lat, data.lng);

                    // pin color
                    var pinColor = "FE6256";//"BDBDBD";//FE6256

                    // Push the location to list
                    lat_lng.push(myLatlng);

                    // Maps center at the first location
                    if (i == 0) {
                        var myOptions = {
                            center: myLatlng,
                            zoom: 16,
                            mapTypeId: google.maps.MapTypeId.ROADMAP
                        };
                        map = new google.maps.Map(map_canvas, myOptions);
                    }

                    // Make the marker at each location
                    var markerLabel = data.visitSort;
                    var marker = new google.maps.Marker({
                        id: data.id,
                        position: myLatlng,
                        map: PosGmap.map,
                        title: data.title,
                        icon: Ext.String.format('http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld={0}|{1}|000000', markerLabel, pinColor)
                    });

                    // Set info display of the marker
                    (function (marker, data) {
                        google.maps.event.addListener(marker, "click", function (e) {
                            PosGmap.infoWindow.setContent(data.description);
                            PosGmap.infoWindow.open(PosMap.map, marker);

                            // Set animation of marker
                            if (marker.getAnimation() != null) {
                                marker.setAnimation(null);
                            } else {
                                marker.setAnimation(google.maps.Animation.BOUNCE);
                                setTimeout(function () {
                                    marker.setAnimation(null);
                                }, 1400);
                            }
                        });
                    })(marker, data);

                    PosGmap.stopMarkers.push(marker);
                }
            }

            PosGmap.directionsDisplay.setMap(PosGmap.map);
            //directionsDisplay.setOptions({ suppressMarkers: true });

            if (showDirections) {
                PosGmap.calcRoute(lat_lng);
            }
        }
        else {
            PosGmap.clearMap(PosGmap.stopMarkers);
        }
    },

    drawMCL: function (markers, showDirections) {
        PosGmap.prepairMap();

        if (markers.length > 0) {
            PosGmap.stopMarkers = [];
            // List of locations
            var lat_lng = new Array();

            // For each marker in list
            for (i = 0; i < markers.length; i++) {
                var data = markers[i];
                if (data.lat && data.lng) {
                    var myLatlng = new google.maps.LatLng(data.lat, data.lng);

                    // Maps center at the first location
                    if (i == 0) {
                        var myOptions = {
                            center: myLatlng,
                            zoom: 16,
                            mapTypeId: google.maps.MapTypeId.ROADMAP
                        };
                        map = new google.maps.Map(map_canvas, myOptions);
                    }

                    // Make the marker at each location
                    var marker = new google.maps.Marker({
                        id: data.id,
                        position: myLatlng,
                        map: PosGmap.map,
                        title: data.title,
                        icon: Ext.String.format('Images/OM30400/circle_{0}.png', data.color ? data.color : "white"),
                        //animation: google.maps.Animation.DROP,
                        custId: data.custId
                    });

                    // Set info display of the marker
                    (function (marker, data) {
                        google.maps.event.addListener(marker, "click", function (e) {
                            PosGmap.infoWindow.setContent(data.description);
                            PosGmap.infoWindow.open(PosGmap.map, marker);

                            // Set animation of marker
                            if (marker.getAnimation() != null) {
                                marker.setAnimation(null);
                            } else {
                                marker.setAnimation(google.maps.Animation.BOUNCE);
                                setTimeout(function () {
                                    marker.setAnimation(null);
                                }, 1400);
                            }
                        });
                    })(marker, data);

                    PosGmap.stopMarkers.push(marker);
                }
            }

            PosGmap.directionsDisplay.setMap(PosGmap.map);
            //directionsDisplay.setOptions({ suppressMarkers: true });

            if (showDirections) {
                PosGmap.calcRoute(lat_lng);
            }

        }
        else {
            PosGmap.clearMap(PosGmap.stopMarkers);
        }
    },

    drawAVC1: function (markers, showDirections, realTime, showAgent) {
        PosGmap.prepairMap();

        if (markers.length > 0) {
            PosGmap.clearMap(PosGmap.stopMarkers);
            PosGmap.stopMarkers = [];
            // List of locations
            var lat_lng = new Array();
            var bounds = new google.maps.LatLngBounds();
            // For each marker in list
            for (i = 0; i < markers.length; i++) {
                var data = markers[i];
                if (data.lat && data.lng) {
                    var myLatlng = new google.maps.LatLng(data.lat, data.lng);
                    bounds.extend(myLatlng);
                    // Change color for Checkin and checkout point (for Actual Visit)
                    //var pinColor = "BDBDBD";//FE6256
                    var icon = Ext.String.format('http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld={0}|{1}|000000', data.label, data.color ? data.color : "BDBDBD");
                    var visible = true;
                    if (data.type) {
                        if (data.type == "IO") { // Check in
                            //    //pinColor = "CCFF33";
                            //    icon = Ext.String.format('http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld={0}|{1}|000000', data.label, data.color?data.color:"CCFF33");
                            // Push the location to list
                            if (!data.isNotVisited) {
                                lat_lng.push(myLatlng);
                            }
                        }
                        else if (data.type == "OO") { // Check out
                            //pinColor = "FF0000";
                            //icon = Ext.String.format('http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld={0}|{1}|000000', data.label, "FF0000");
                            visible = false;
                        }
                        else if (data.type == "CC") {
                            icon = Ext.String.format('https://chart.googleapis.com/chart?chst=d_map_xpin_letter&chld=pin_star|{0}|{1}|000000|FFFF00', data.label, data.color ? data.color : "01DFD7");
                            visible = showAgent;
                        }
                    }
                    else {
                        // Push the location to list
                        if (!data.isNotVisited) {
                            visible = realTime;
                            lat_lng.push(myLatlng);
                        }
                    }

                    // Maps center at the first location
                    //if (i == 0) {
                    //    var myOptions = {
                    //        center: myLatlng,
                    //        zoom: 16,
                    //        mapTypeId: google.maps.MapTypeId.ROADMAP
                    //    };
                    //    PosGmap.map = new google.maps.Map(PosGmap.map_canvas, myOptions);
                    //}

                    var markerLabel = data.label;
                    var marker = new google.maps.Marker({
                        id: data.id,
                        position: myLatlng,
                        map: PosGmap.map,
                        title: data.title,
                        icon: icon,
                        type: data.type,
                        //animation: google.maps.Animation.DROP,
                        visible: visible
                    });

                    // Set info display of the marker
                    (function (marker, data) {
                        google.maps.event.addListener(marker, "click", function (e) {
                            PosGmap.infoWindow.setContent(data.description);
                            PosGmap.infoWindow.open(PosGmap.map, marker);

                            // Set animation of marker
                            if (marker.getAnimation() != null) {
                                marker.setAnimation(null);
                            } else {
                                marker.setAnimation(google.maps.Animation.BOUNCE);
                                setTimeout(function () {
                                    marker.setAnimation(null);
                                }, 1400);
                            }
                        });
                    })(marker, data);

                    PosGmap.stopMarkers.push(marker);
                }
            }
            PosGmap.directionsDisplay.setMap(PosGmap.map);
            PosGmap.map.initialZoom = true;
            PosGmap.map.fitBounds(bounds);
            if (showDirections) {
                PosGmap.calcRoute(lat_lng);
            }

        }
        else {
            PosGmap.clearMap(PosGmap.stopMarkers);
        }
        PosGmap.showCustomControl();
    },

    drawAVC: function (markers, showDirections) {
        PosGmap.prepairMap();

        if (markers.length > 0) {
            PosGmap.stopMarkers = [];
            // List of locations
            var lat_lng = new Array();

            // For each marker in lisr
            for (i = 0; i < markers.length; i++) {
                var data = markers[i];
                if (data.lat && data.lng) {
                    var myLatlng = new google.maps.LatLng(data.lat, data.lng);

                    // Change color for Checkin and checkout point (for Actual Visit)
                    var pinColor = "BDBDBD";//FE6256
                    if (data.type) {
                        if (data.type == "IO") { // Check in
                            pinColor = data.color;
                            // Push the location to list
                            lat_lng.push(myLatlng);
                        }
                        else if (data.type == "OO") { // Check out
                            pinColor = "FF0000";
                        }
                    }
                    else {
                        // Push the location to list
                        lat_lng.push(myLatlng);
                    }

                    // Maps center at the first location
                    if (i == 0) {
                        var myOptions = {
                            center: myLatlng,
                            zoom: 16,
                            mapTypeId: google.maps.MapTypeId.ROADMAP
                        };
                        PosGmap.map = new google.maps.Map(PosGmap.map_canvas, myOptions);
                    }

                    var markerLabel = i + 1;
                    var marker = new google.maps.Marker({
                        id: data.id,
                        position: myLatlng,
                        map: PosGmap.map,
                        title: data.title,
                        icon: Ext.String.format('http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld={0}|{1}|000000', markerLabel, pinColor),
                        //animation: google.maps.Animation.DROP,
                        visible: data.type == "OO" ? false : true
                    });

                    // Set info display of the marker
                    (function (marker, data) {
                        google.maps.event.addListener(marker, "click", function (e) {
                            PosGmap.infoWindow.setContent(data.description);
                            PosGmap.infoWindow.open(PosGmap.map, marker);

                            // Set animation of marker
                            if (marker.getAnimation() != null) {
                                marker.setAnimation(null);
                            } else {
                                marker.setAnimation(google.maps.Animation.BOUNCE);
                                setTimeout(function () {
                                    marker.setAnimation(null);
                                }, 1400);
                            }
                        });
                    })(marker, data);

                    PosGmap.stopMarkers.push(marker);
                }
            }

            PosGmap.directionsDisplay.setMap(PosGmap.map);

            if (showDirections) {
                PosGmap.calcRoute(lat_lng);
            }

        }
        else {
            PosGmap.clearMap(PosGmap.stopMarkers);
        }
    },

    drawRoutes: function (markers, showDirections) {
        PosGmap.prepairMap();

        if (markers.length > 0) {
            PosGmap.clearMap(PosGmap.stopMarkers);
            PosGmap.stopMarkers = [];
            // List of locations
            var lat_lng = new Array();

            // For each marker in list
            for (i = 0; i < markers.length; i++) {
                var data = markers[i];
                if (data.lat && data.lng) {
                    var myLatlng = new google.maps.LatLng(data.lat, data.lng);

                    // Pin color
                    var pinColor = "FE6256";

                    // Push the location to list
                    lat_lng.push(myLatlng);

                    // Maps center at the first location
                    if (i == 0) {
                        var myOptions = {
                            center: myLatlng,
                            zoom: 16,
                            mapTypeId: google.maps.MapTypeId.ROADMAP
                        };
                        PosGmap.map = new google.maps.Map(PosGmap.map_canvas, myOptions);
                    }

                    // Make the marker at each location
                    var markerLabel = i + 1;
                    var marker = new google.maps.Marker({
                        id: data.id,
                        position: myLatlng,
                        map: PosGmap.map,
                        title: data.title,
                        icon: Ext.String.format('http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld={0}|{1}|000000', markerLabel, pinColor),
                    });

                    // Set info display of the marker
                    (function (marker, data) {
                        google.maps.event.addListener(marker, "click", function (e) {
                            PosGmap.infoWindow.setContent(data.description);
                            PosGmap.infoWindow.open(PosGmap.map, marker);

                            // Set animation of marker
                            if (marker.getAnimation() != null) {
                                marker.setAnimation(null);
                            } else {
                                marker.setAnimation(google.maps.Animation.BOUNCE);
                                setTimeout(function () {
                                    marker.setAnimation(null);
                                }, 1400);
                            }
                        });
                    })(marker, data);

                    PosGmap.stopMarkers.push(marker);
                }
            }

            PosGmap.directionsDisplay.setMap(PosGmap.map);

            if (showDirections) {
                PosGmap.calcRoute(lat_lng);
            }
        }
        else {
            PosGmap.clearMap(PosGmap.stopMarkers);
        }
    },

    calcRoute: function (lat_lng) {
        var x = 0;
        var lat_lngCols = [];

        for (var i = 0; i < lat_lng.length; i++) {
            if (i > 0 && i % 8 == 0) {
                x++;
            }
            if (!lat_lngCols[x]) {
                lat_lngCols[x] = [];
                PosGmap.directionsDisplays.push(new google.maps.DirectionsRenderer());
            }
            if (i > 0 && i % 8 == 0) {
                lat_lngCols[x].push(lat_lng[i - 1]);
                lat_lngCols[x].push(lat_lng[i]);
            }
            else {
                lat_lngCols[x].push(lat_lng[i]);
            }
        }

        if (lat_lngCols.length > 0) {
            var idx = 0;
            //PosGmap.y = 0;
            PosGmap.requestForWaysRoute(lat_lngCols, idx);
        }
        else
            HQ.common.showBusy(false);
    },

    requestForWaysRoute: function (lat_lngCols, idx) {
        var start;
        var end;
        var waypts = [];

        for (var i = 0; i < lat_lngCols[idx].length; i++) {
            // Set start location
            if (i == 0) {
                start = lat_lngCols[idx][i];
            }

            // Set end location
            if (i == lat_lngCols[idx].length - 1) {
                end = lat_lngCols[idx][i];
            }

            // Set waypts locations
            if (i > 0 && i < lat_lngCols[idx].length - 1) {
                waypts.push({
                    location: lat_lngCols[idx][i],
                    stopover: true
                });
            }
        }

        if (_FlagZeroResult == false) {
            var request = {
                origin: start,
                destination: end,
                waypoints: waypts,
                optimizeWaypoints: false,
                travelMode: google.maps.TravelMode.WALKING
            };
        }
        else {
            var request = {
                origin: start,
                destination: end,
                waypoints: waypts,
                optimizeWaypoints: false,
                travelMode: google.maps.TravelMode.DRIVING
            };
        }

        directionsDisplay = new google.maps.DirectionsRenderer({
            map: PosGmap.map,
            type: 'actual',
            polylineOptions: {
                icons: [
                  {
                      icon:
                      {
                          path: google.maps.SymbolPath.FORWARD_CLOSED_ARROW,
                          scale: 2,
                          fillOpacity: 1,
                          strokeColor: '#28FF44',
                          fillColor: '#28FF44',
                          strokeOpacity: 1
                      },
                      repeat: '400px'
                  }
                ],
                strokeColor: '#28FF44',
                strokeOpacity: 1
            }
        });
        directionsDisplay.setMap(PosGmap.map);
        directionsDisplay.setOptions({ preserveViewport: true, suppressMarkers: true });

        PosGmap.directionsService.route(request, function (response, status) {
            if (status == google.maps.DirectionsStatus.OK) {
                directionsDisplay.setDirections(response);
                PosGmap.directionsDisplays.push(directionsDisplay);
                idx = idx + 1;
                var lat_lngCol = lat_lngCols[idx];
                if (lat_lngCol && lat_lngCol.length > 0) {
                    setTimeout(function () {
                        PosGmap.requestForWaysRoute(lat_lngCols, idx);
                   }, 400); //PosGmap.timeoutRequest[PosGmap.y]);
                    //if (PosGmap.y == 3)
                    //    PosGmap.y = 0;
                    //else
                    //    PosGmap.y += 1;
                }
                else {
                    HQ.common.showBusy(false);
                    _FlagZeroResult = false;
                }
            }

            else if (status == google.maps.DirectionsStatus.NOT_FOUND) {
                //alert("NOT_FOUND");
            }
            else if (status == google.maps.DirectionsStatus.ZERO_RESULTS) {
                //alert("ZERO_RESULTS");
                if (_FlagZeroResult == true) {
                    HQ.common.showBusy(false);
                    _FlagZeroResult = false;
                } else {
                    _FlagZeroResult = true;
                    App.grdVisitCustomerActual.store.reload();
                    App.storeMapActualVisit.reload();
                }
            }
            else if (status == google.maps.DirectionsStatus.MAX_WAYPOINTS_EXCEEDED) {
                alert("MAX_WAYPOINTS_EXCEEDED");
            }
            else if (status == google.maps.DirectionsStatus.INVALID_REQUEST) {
                alert("INVALID_REQUEST");
            }
            else if (status == google.maps.DirectionsStatus.OVER_QUERY_LIMIT) {
                //alert("OVER_QUERY_LIMIT");
                setTimeout(function () {
                    PosGmap.requestForWaysRoute(lat_lngCols, idx);
                }, 400);
            }
            else if (status == google.maps.DirectionsStatus.REQUEST_DENIED) {
                alert("REQUEST_DENIED");
            }
            else {
                alert("UNKNOWN_ERROR");
            }
        });

        //PosGmap.directionsService.route(request, function (response, status) {
        //    if (status == google.maps.DirectionsStatus.OK) {
        //        PosGmap.directionsDisplays[idx].setMap(PosGmap.map);
        //        PosGmap.directionsDisplays[idx].setOptions({ preserveViewport: true, suppressMarkers: true });
        //        PosGmap.directionsDisplays[idx].setDirections(response);

        //        idx = idx + 1;

        //        var lat_lngCol = lat_lngCols[idx];
        //        if (lat_lngCol && lat_lngCol.length > 0) {
        //            setTimeout(function () {
        //                PosGmap.requestForWaysRoute(lat_lngCols, idx);
        //            }, 1000);
        //        }
        //    }
        //    else if (status == google.maps.DirectionsStatus.NOT_FOUND) {
        //        //alert("NOT_FOUND");
        //    }
        //    else if (status == google.maps.DirectionsStatus.ZERO_RESULTS) {
        //        //alert("ZERO_RESULTS");
        //        _FlagZeroResult = true;
        //        App.grdVisitCustomerActual.store.reload();
        //        App.storeMapActualVisit.reload();
        //    }
        //    else if (status == google.maps.DirectionsStatus.MAX_WAYPOINTS_EXCEEDED) {
        //        //alert("MAX_WAYPOINTS_EXCEEDED");
        //    }
        //    else if (status == google.maps.DirectionsStatus.INVALID_REQUEST) {
        //        //alert("INVALID_REQUEST");
        //    }
        //    else if (status == google.maps.DirectionsStatus.OVER_QUERY_LIMIT) {
        //        //alert("OVER_QUERY_LIMIT");
        //    }
        //    else if (status == google.maps.DirectionsStatus.REQUEST_DENIED) {
        //        //alert("REQUEST_DENIED");
        //    }
        //    else {
        //        //alert("UNKNOWN_ERROR");
        //    }
        //});
    },

    drawRouteByMarkers: function (markers) {
        var mapOptions = {
            center: new google.maps.LatLng(markers[0].lat, markers[0].lng),
            zoom: 8,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        };
        var path = new google.maps.MVCArray();
        var service = new google.maps.DirectionsService();

        var infoWindow = new google.maps.InfoWindow();
        var map = new google.maps.Map(document.getElementById("map_canvas"), mapOptions);
        var poly = new google.maps.Polyline({ map: map, strokeColor: 'green' });
        var lat_lng = new Array();

        for (i = 0; i < markers.length; i++) {
            var data = markers[i]
            var myLatlng = new google.maps.LatLng(data.lat, data.lng);
            lat_lng.push(myLatlng);
            var marker = new google.maps.Marker({
                position: myLatlng,
                map: map,
                title: data.title
            });
            (function (marker, data) {
                google.maps.event.addListener(marker, "click", function (e) {
                    PosGmap.infoWindow.setContent(data.description);
                    PosGmap.infoWindow.open(PosGmap.map, marker);
                });
            })(marker, data);
        }

        for (var i = 0; i < lat_lng.length; i++) {
            if ((i + 1) < lat_lng.length) {
                var src = lat_lng[i];
                var des = lat_lng[i + 1];
                path.push(src);
                poly.setPath(path);
                service.route({
                    origin: src,
                    destination: des,
                    travelMode: google.maps.TravelMode.WALKING
                }, function (result, status) {
                    if (status == google.maps.DirectionsStatus.OK) {
                        for (var i = 0, len = result.routes[0].overview_path.length; i < len; i++) {
                            path.push(result.routes[0].overview_path[i]);
                        }
                    }
                });
            }
        }
    },

    customControl: function (controlDiv, map, storeColor) {
        // Set CSS for the control border.
        var controlUI = document.createElement('div');
        controlUI.style.backgroundColor = '#fff';//'transparent';//#fff
        controlUI.style.border = '2px solid #fff';
        controlUI.style.borderRadius = '3px';
        controlUI.style.boxShadow = '0 2px 6px rgba(0,0,0,.3)';
        controlUI.style.cursor = 'pointer';
        controlUI.style.marginTop = '10px';
        controlUI.style.textAlign = 'center';
        //controlUI.title = 'Click to recenter the map';
        controlDiv.appendChild(controlUI);

        // Set CSS for the control interior.
        var controlTitle = document.createElement('div');
        //controlText.style.color = '#399239';//'rgb(25,25,25)';
        controlTitle.style.background = 'url("Images/OM30400/paint.png") no-repeat';
        controlTitle.style.width = "24px";
        controlTitle.style.height = "24px";
        //controlTitle.style.textShadow = '2px 1px #ffffff';
        //controlTitle.style.fontFamily = 'Roboto,Arial,sans-serif';
        //controlTitle.style.fontSize = '16px';
        //controlTitle.style.lineHeight = '28px';
        //controlTitle.style.paddingLeft = '5px';
        //controlTitle.style.paddingRight = '5px';
        //controlTitle.innerHTML = HQ.common.getLang('Note');
        controlUI.appendChild(controlTitle);

        var controlTextDiv = document.createElement('div');
        controlTextDiv.hidden = true;

        storeColor.each(function (record) {
            var controlItem = document.createElement('div');
            controlItem.style.margin = '5px';
            //controlItem.style.display = "inline-block";

            if (record.data.Type == '0') {
                var controlBox = document.createElement('div');
                controlBox.style.backgroundColor = "#" + record.data.Code;
                controlBox.style.width = "20px";
                controlBox.style.height = "20px";
                controlBox.style.float = "left";
                controlBox.style.marginRight = '5px';
                controlItem.appendChild(controlBox);

                var controlText = document.createElement('div');
                controlText.style.color = '#' + record.data.Code;
                controlText.style.textShadow = '2px 1px #ffffff';
                controlText.style.fontFamily = 'Roboto,Arial,sans-serif';
                controlText.style.fontSize = '13px';
                controlText.style.lineHeight = '20px';
                controlText.style.textAlign = 'left';
                controlText.innerHTML = record.data.Descr;
                controlItem.appendChild(controlText);
            }
            else if (record.data.Type == '1') {
                var controlBox = document.createElement('div');
                controlBox.style.backgroundColor = "#FFFFFF"; 
                controlBox.style.width = "50px";
                controlBox.style.height = "20px";
                controlBox.style.float = "left";
                controlBox.style.marginRight = '5px';
                controlBox.innerHTML = "<hr size=5 color=#" + record.data.Code + " noshade>";
                controlItem.appendChild(controlBox);

                var controlText = document.createElement('div');
                controlText.style.color = '#' + record.data.Code;
                controlText.style.textShadow = '2px 1px #ffffff';
                controlText.style.fontFamily = 'Roboto,Arial,sans-serif';
                controlText.style.fontSize = '13px';
                controlText.style.lineHeight = '20px';
                controlText.style.textAlign = 'left';
                controlText.innerHTML = record.data.Descr;
                controlItem.appendChild(controlText);
            }

            controlTextDiv.appendChild(controlItem);
        });
        controlUI.appendChild(controlTextDiv);

        // Setup the click event listeners: simply set the map to Chicago.
        controlUI.addEventListener('mouseover', function () {
            controlTextDiv.hidden = false;
        });

        // Setup the click event listeners: simply set the map to Chicago.
        controlUI.addEventListener('mouseout', function () {
            controlTextDiv.hidden = true;
        });
    },

    showCustomControl: function () {
        var divID = 'divHint'
        var div = document.getElementById(divID);
        if (!div) {
            // Create the DIV to hold the control and call the CenterControl() constructor
            // passing in this DIV.
            var centerControlDiv = document.createElement('div');
            centerControlDiv.id = divID;
            var centerControl = new PosGmap.customControl(centerControlDiv, PosGmap.map, App.stoColorHint);

            centerControlDiv.index = 1;
            PosGmap.map.controls[google.maps.ControlPosition.TOP_LEFT].push(centerControlDiv);
        }
    },

    drawMap_Visit: function () {
    PosGmap.clearMap(PosGmap.planMarkers);
    PosGmap.planMarkers = [];

    App.stoVisitPlan.data.each(function (item) {
        var data = item.data;
        var latLng = new google.maps.LatLng(item.data.Lat, item.data.Lng);
        var marker = new google.maps.Marker({
            position: latLng,
            icon: new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=" + item.data.VisitSort + "|" + "0CF1F9" + '|000000',
                    new google.maps.Size(21, 34),
                    new google.maps.Point(0, 0)
                    //new google.maps.Point(0, 0),
                    //new google.maps.Size(20, 35)
                ),
            //map: PosGmap.map,
            title: Ext.String.format('{0} - {1}', item.data.CustId, item.data.CustName)
        });

        // Set info display of the marker
        (function (marker, data) {
            data.id = '1_' + data.CustId + '_' + data.Lat + '_' + data.Lng;
            data.description =
            '<div id="content">' +
                '<div id="siteNotice">' +
                '</div>' +
                '<h1 id="firstHeading" class="firstHeading">' +
                    data.CustName +
                '</h1>' +
                '<div id="bodyContent">' +
                    '<p>' +
                        'Địa chỉ: ' + data.Addr +
                    '</p>' +
                    '<p>' +
                        'Thứ tự viếng thăm: ' + data.VisitSort +
                    '</p>' +
                    '<p>' +
                        'Tọa độ: ' + data.Lat + ',' + data.Lng +
                    '</p>' +
            //(!record.data.PicPath ? '' : ('<a target="_blank" href="' + record.data.PicPath + '">' +
            //    '<img width="200px" src="' + record.data.PicPath + '" />' +
            //'</a>')) +
                '</div>' +
            '</div>';

            google.maps.event.addListener(marker, "click", function (e) {
                PosGmap.infoWindow.setContent(data.description);
                PosGmap.infoWindow.open(PosGmap.map, marker);

                // Set animation of marker
                if (marker.getAnimation() != null) {
                    marker.setAnimation(null);
                } else {
                    marker.setAnimation(google.maps.Animation.BOUNCE);
                    setTimeout(function () {
                        marker.setAnimation(null);
                    }, 1400);
                }
            });
        })(marker, data);

        if (marker != null) {
            marker.setZIndex(1000);
            marker.type = 'plan';
            //marker.CustID = item.data.CustId;
            //marker.SlsPerID = item.data.SlsPerID;
            marker.data = item.data;
            PosGmap.planMarkers.push(marker);
        }
    });

    planPoints = [];

    var x = 0;
    for (var i = 0 ; i < PosGmap.planMarkers.length; i++) {
        if (i > 0 && i % 8 == 0) {
            x++;
        }
        if (!planPoints[x]) {
            planPoints[x] = [];
        }
        if (i > 0 && i % 8 == 0) {
            planPoints[x].push(PosGmap.planMarkers[i - 1].position);
            planPoints[x].push(PosGmap.planMarkers[i].position);
        }
        else {
            planPoints[x].push(PosGmap.planMarkers[i].position);
        }
    }

    if (planPoints.length > 0) {
        var idxPlan = 0;
        PosGmap.getRoute(planPoints, idxPlan);
    } else {

    }
},

    getRoute: function (planPoints, idxPlan) {
    var start;
    var end;
    var waypts = [];

    for (var i = 0; i < planPoints[idxPlan].length; i++) {
        // Set start location
        if (i == 0) {
            start = planPoints[idxPlan][i];
        }

        // Set end location
        if (i == planPoints[idxPlan].length - 1) {
            end = planPoints[idxPlan][i];
        }

        // Set waypts locations
        if (i > 0 && i < planPoints[idxPlan].length - 1) {
            waypts.push({
                location: planPoints[idxPlan][i],
                stopover: true
            });
        }
    }

    var request = {
        origin: start,
        destination: end,
        waypoints: waypts,
        optimizeWaypoints: false,
        travelMode: google.maps.TravelMode.DRIVING
    };
    directionsDisplay = new google.maps.DirectionsRenderer({
        //map: PosGmap.map,
        type: 'plan',
        polylineOptions: {
            icons: [
              {
                  icon:
                  {
                      path: google.maps.SymbolPath.FORWARD_CLOSED_ARROW,
                      scale: 2,
                      fillOpacity: 1,
                      strokeColor: '#0CF1F9',
                      fillColor: '#0CF1F9',
                      strokeOpacity: 1
                  },
                  repeat: '400px'
              }
            ],
            strokeColor: '#0CF1F9',
            strokeOpacity: 1
        }
    });
    //directionsDisplay.setMap(PosGmap.map);
    directionsDisplay.setOptions({ preserveViewport: true, suppressMarkers: true });

    PosGmap.directionsService.route(request, function (response, status) {
        if (status == google.maps.DirectionsStatus.OK) {
            directionsDisplay.setDirections(response);
            PosGmap.directionsDisplays.push(directionsDisplay);
            idxPlan = idxPlan + 1;
            var planPoint = planPoints[idxPlan];
            if (planPoint && planPoint.length > 0) {
                //PosGmap.getRoute();
                setTimeout(function () {
                    PosGmap.getRoute(planPoints, idxPlan);
                }, 400);
            } else {
                App.grdVisitCustomerActual.store.reload();
                App.storeMapActualVisit.reload();
            }
        }

        else if (status == google.maps.DirectionsStatus.NOT_FOUND) {
            alert("NOT_FOUND");
        }
        else if (status == google.maps.DirectionsStatus.ZERO_RESULTS) {
            //alert("ZERO_RESULTS");
            directionsDisplay.setDirections(response);
            PosGmap.directionsDisplays.push(directionsDisplay);
            idxPlan = idxPlan + 1;
            var planPoint = planPoints[idxPlan];
            if (planPoint && planPoint.length > 0) {
                //PosGmap.getRoute();
                setTimeout(function () {
                    PosGmap.getRoute(planPoints, idxPlan);
                }, 400);
            } else {
                App.grdVisitCustomerActual.store.reload();
                App.storeMapActualVisit.reload();
            }
        }
        else if (status == google.maps.DirectionsStatus.MAX_WAYPOINTS_EXCEEDED) {
            alert("MAX_WAYPOINTS_EXCEEDED");
        }
        else if (status == google.maps.DirectionsStatus.INVALID_REQUEST) {
            alert("INVALID_REQUEST");
        }
        else if (status == google.maps.DirectionsStatus.OVER_QUERY_LIMIT) {
            //alert("OVER_QUERY_LIMIT");
            setTimeout(function () {
                PosGmap.getRoute(planPoints, idxPlan);
            }, 400);
        }
        else if (status == google.maps.DirectionsStatus.REQUEST_DENIED) {
            alert("REQUEST_DENIED");
        }
        else {
            alert("UNKNOWN_ERROR");
        }
    });
}
};

