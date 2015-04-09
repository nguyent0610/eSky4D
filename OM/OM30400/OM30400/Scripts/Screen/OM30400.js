var pointsArray = [];
// JS Code for Index View
var Index = {
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
                                App.grdVisitCustomerActual.store.reload();
                                App.storeVisitCustomerActual.reload();
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
                {
                    HQ.common.close(this);
                }
                break;
        }
    },

    removeStoreData: function () {
        var stores = [
                App.grdVisitCustomerPlan.store,
                App.grdMCL.store,
                App.grdAllCurrentSalesman.store,
                App.grdVisitCustomerActual.store,
                App.storeVisitCustomerActual,
                App.grdCustHistory.store
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
            PosGmap.drawRoutes(markers, false);
        }
    },

    selectionModelVisitCustomerPlan_Select: function (rowModel, record, index, eOpts) {
        if (record) {
            PosGmap.navMapCenterByLocation(record.data.Lat, record.data.Lng);
        }
    },

    grdVisitCustomerActual_cellClick: function (gridview, td, cellIndex, record, tr, rowIndex, e, eOpts) {
        var clickCol = gridview.up('grid').columns[cellIndex];
        if (clickCol) {
            if (clickCol.dataIndex === "Checkout") {
                PosGmap.navMapCenterByLocation(record.data.CoLat, record.data.CoLng);
            }
            else if (clickCol.dataIndex === "Checkin") {
                PosGmap.navMapCenterByLocation(record.data.CiLat, record.data.CiLng);
            }
            else if (!record.data.CustId) {
                PosGmap.navMapCenterByLocation(record.data.CiLat, record.data.CiLng);
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
                    "title": record.data.CustId + ": " + record.data.CustName,
                    "lat": record.data.Lat,
                    "lng": record.data.Lng,
                    "stop": true,
                    "color": record.data.Color,
                    "description": Index.createMarkerDescription(record)
                }
                markers.push(marker);
            });
            PosGmap.drawRoutes(markers, false);
        }
        App.dataForm.getEl().unmask();

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
                Ext.String.format("OM30400/LoadSalesRouteMaster?brachID={0}&custID={1}&slsPerID={2}", record.data.BranchID, record.data.CustId, record.data.SlsperId);
            App.storeMcpInfo.reload();
        }
    },

    selectionModelMCL_Select: function (rowModel, record, index, eOpts) {
        if (record) {
            PosGmap.navMapCenterByLocation(record.data.Lat, record.data.Lng);
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

            App.grdVisitCustomerActual.hide();
            App.grdAllCurrentSalesman.show();
        }
        else {
            App.cboSalesManActual.enable();
            App.btnGetCurrentLocation.enable();
            App.chkRealTime.enable();

            App.grdVisitCustomerActual.show();
            App.grdAllCurrentSalesman.hide();
        }
    },

    btnLoadDataActual_click: function (btn, e, eOpts) {
        if (App.pnlActualVisit.isValid()) {
            Index.removeStoreData();
            if (App.radSalesmanAll.value) {
                App.grdAllCurrentSalesman.store.reload();
            }
            else {
                App.grdVisitCustomerActual.store.reload();
                App.storeVisitCustomerActual.reload();
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

    grdVisitCustomerActual_viewGetRowClass: function (record) {
        if (record.data.Type == "IO") {
            return "ci-row"
        }
        else if (record.data.Type == "OO") {
            return "co-row";
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

    btnGetCurrentLocation_click: function (btn, e, eOpts) {
        var store = App.storeGridActualVisit;
        if (store.getCount() > 0) {
            var lastRecord = store.getAt(store.getCount() - 1);
            PosGmap.navMapCenterByLocation(lastRecord.data.CoLat, lastRecord.data.CoLng);

            App.SelectionModelVisitCustomerActual.select(store.getCount() - 1);
        }
    },

    chkRealTime_change: function (chk, newValue, oldValue, eOpts) {
        if (chk.value) {
            App.grdVisitCustomerActual.store.clearFilter();
        }
        else {
            App.grdVisitCustomerActual.store.filterBy(function (record) {
                if (record.data.CustId) {
                    return record;
                }
            });
        }
    },

    storeVisitCustomerActual_load: function (store, records, successful, eOpts) {
        if (successful) {
            var markers = [];
            records.forEach(function (record) {
                var marker = {
                    "title": record.data.CustId + ": " + record.data.CustName,
                    "lat": record.data.Lat,
                    "lng": record.data.Lng,
                    "type": record.data.Type,
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
            PosGmap.drawRoutes(markers, true);
        }

        // pretend to change chkRealTime check
        Index.chkRealTime_change(App.chkRealTime);
    },

    storeAllCurrentSalesman_load: function (store, records, successful, eOpts) {
        if (successful) {
            var markers = [];
            records.forEach(function (record) {
                var marker = {
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
                                '<p>' +
                                    (record.data.CustId ? (record.data.CustId + ": " + record.data.CustName + " - ") : "") +
                                    record.data.Addr +
                                '</p>' +
                            '</div>' +
                        '</div>'
                }
                markers.push(marker);
            });
            PosGmap.drawRoutes(markers, false);
        }
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
    },

    //

    selectionModelPOS_ActualVisit_Select: function (rowModel, record, index, eOpts) {
        if (record) {
            PosGmap.navMapCenterByLocation(record.data.CiLat, record.data.CiLng);
        }
    },

    grdActualVisit_cellClick: function (gridview, td, cellIndex, record, tr, rowIndex, e, eOpts) {
        var clickCol = App.grdActualVisit.columns[cellIndex];
        if (clickCol) {
            if (clickCol.dataIndex === "CheckOut") {
                PosGmap.navMapCenterByLocation(record.data.CoLat, record.data.CoLng);
            }
            else if (clickCol.dataIndex === "CheckIn") {
                PosGmap.navMapCenterByLocation(record.data.CiLat, record.data.CiLng);
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

    initialize: function () {
        map_canvas = document.getElementById("map_canvas");

        var initLatLng = new google.maps.LatLng(10.782171, 106.654012, 17);
        var myOptions = {
            center: initLatLng,
            zoom: 16,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        };

        map = new google.maps.Map(map_canvas, myOptions);
        directionsService = new google.maps.DirectionsService();
        directionsDisplay = new google.maps.DirectionsRenderer();
        infoWindow = new google.maps.InfoWindow();

        //var marker = new google.maps.Marker({
        //    position: initLatLng,
        //    map: map,
        //    title: "HQ Soft"
        //});
        stopMarkers: [];

        drawingManager = new google.maps.drawing.DrawingManager({
            //drawingMode: google.maps.drawing.OverlayType.MARKER,
            drawingControl: false,
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
                fillOpacity: 1,
                strokeWeight: 5,
                clickable: false,
                editable: true,
                zIndex: 1
            }
        });
        drawingManager.setMap(map);
    },

    navMapCenterByLocation: function (lat, lng) {
        var myLatlng = new google.maps.LatLng(lat, lng);
        map.setCenter(myLatlng);
        map.setZoom(map.getZoom());

        var selectedMarker = PosGmap.find_closest_marker(lat, lng);

        if (selectedMarker) {
            if (selectedMarker.getAnimation() != null) {
                selectedMarker.setAnimation(null);
            } else {
                selectedMarker.setAnimation(google.maps.Animation.BOUNCE);
                setTimeout(function () {
                    selectedMarker.setAnimation(null);
                }, 1400);
            }
        }
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

    drawRoutes: function (markers, showDirections) {
        if (directionsDisplay) {
            directionsDisplay.setMap(null);
            if (PosGmap.directionsDisplays && PosGmap.directionsDisplays.length > 0) {
                for (var i = 0; i < PosGmap.directionsDisplays.length; i++) {
                    PosGmap.directionsDisplays[i].setMap(null);
                    //directionsDisplay[i] = new google.maps.DirectionsRenderer();
                }
            }
        }
        directionsService = new google.maps.DirectionsService();
        directionsDisplay = new google.maps.DirectionsRenderer();

        if (markers.length > 0) {
            PosGmap.stopMarkers = [];
            // List of locations
            var lat_lng = new Array();

            // For each marker in lisr
            for (i = 0; i < markers.length; i++) {
                var data = markers[i];
                var myLatlng = new google.maps.LatLng(data.lat, data.lng);

                // Change color for Checkin and checkout point (for Actual Visit)
                var pinColor = "BDBDBD";//FE6256
                if (data.type) {
                    if (data.type == "IO") { // Check in
                        pinColor = "CCFF33";
                    }
                    else if (data.type == "OO") { // Check out
                        pinColor = "FF0000";
                    }
                }

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
                if (data.stop) {
                    var marker = new google.maps.Marker({
                        position: myLatlng,
                        map: map,
                        title: data.title,
                        icon: Ext.String.format('Images/OM30400/circle_{0}.png', data.color ? data.color : "white")
                        //animation: google.maps.Animation.DROP
                    });
                }
                else {
                    var markerLabel = i + 1;
                    if (data.visitSort != undefined) {
                        markerLabel = data.visitSort;
                    }
                    var marker = new google.maps.Marker({
                        position: myLatlng,
                        map: map,
                        title: data.title,
                        icon: Ext.String.format('http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld={0}|{1}|000000', markerLabel, pinColor)
                        //animation: google.maps.Animation.DROP
                    });
                }

                // Set info display of the marker
                (function (marker, data) {
                    google.maps.event.addListener(marker, "click", function (e) {
                        infoWindow.setContent(data.description);
                        infoWindow.open(map, marker);

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

            directionsDisplay.setMap(map);
            //directionsDisplay.setOptions({ suppressMarkers: true });

            if (showDirections) {
                PosGmap.calcRoute(lat_lng);
            }

        }
        else {
            //PosGmap.initialize();
            for (i = 0; i < PosGmap.stopMarkers.length; i++) {
                PosGmap.stopMarkers[i].setMap(null);

                if (i == PosGmap.stopMarkers.length - 1) {
                    PosGmap.stopMarkers = [];
                }
            }
            directionsDisplay.setMap(map);
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
            PosGmap.requestForWaysRoute(lat_lngCols, idx);
        }
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

        var request = {
            origin: start,
            destination: end,
            waypoints: waypts,
            optimizeWaypoints: false,
            travelMode: google.maps.TravelMode.DRIVING
        };
        directionsService.route(request, function (response, status) {
            if (status == google.maps.DirectionsStatus.OK) {
                PosGmap.directionsDisplays[idx].setMap(map);
                PosGmap.directionsDisplays[idx].setOptions({ preserveViewport: true, suppressMarkers: true });
                PosGmap.directionsDisplays[idx].setDirections(response);

                idx = idx + 1;

                var lat_lngCol = lat_lngCols[idx];
                if (lat_lngCol && lat_lngCol.length > 0) {
                    setTimeout(function () {
                        PosGmap.requestForWaysRoute(lat_lngCols, idx);
                    }, 300);
                }
            }
            else if (status == google.maps.DirectionsStatus.NOT_FOUND) {
                //alert("NOT_FOUND");
            }
            else if (status == google.maps.DirectionsStatus.ZERO_RESULTS) {
                //alert("ZERO_RESULTS");
            }
            else if (status == google.maps.DirectionsStatus.MAX_WAYPOINTS_EXCEEDED) {
                //alert("MAX_WAYPOINTS_EXCEEDED");
            }
            else if (status == google.maps.DirectionsStatus.INVALID_REQUEST) {
                //alert("INVALID_REQUEST");
            }
            else if (status == google.maps.DirectionsStatus.OVER_QUERY_LIMIT) {
                //alert("OVER_QUERY_LIMIT");
            }
            else if (status == google.maps.DirectionsStatus.REQUEST_DENIED) {
                //alert("REQUEST_DENIED");
            }
            else {
                //alert("UNKNOWN_ERROR");
            }
        });
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
                    infoWindow.setContent(data.description);
                    infoWindow.open(map, marker);
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
    }
}

