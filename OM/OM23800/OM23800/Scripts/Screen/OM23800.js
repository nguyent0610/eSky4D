var Declare = {
    selBranchID: '',
    selBranchName: '',
    _overLays: '',
    _ID: 0,
    _zIndex: 0
};

var Process = {
    createMarkerDescription: function (record, hightLight) {
        var dayOfWeekStr = Process.getDisplayDaysOfWeek(record);
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
                '</p>' + (hightLight ? '' :
                '<a class="x-btn-default-toolbar-small-icon" href="javascript: McpInfo.editFromMap(\'' + record.data.CustId + '\',\'' + record.data.SlsperId + '\',\'' + record.data.BranchID + '\') ">edit</a>') +
            '</div>' +
        '</div>';
    },

    getDisplayDaysOfWeek: function (record) {
        var totalStrs = [];
        if (record.data.Sun) {
            totalStrs.push(HQ.common.getLang("Sunday"));
        }
        if (record.data.Mon) {
            totalStrs.push(HQ.common.getLang("Monday"));
        }
        if (record.data.Tue) {
            totalStrs.push(HQ.common.getLang("Tuesday"));
        }
        if (record.data.Wed) {
            totalStrs.push(HQ.common.getLang("Wednesday"));
        }
        if (record.data.Thu) {
            totalStrs.push(HQ.common.getLang("Thursday"));
        }
        if (record.data.Fri) {
            totalStrs.push(HQ.common.getLang("Friday"));
        }
        if (record.data.Sat) {
            totalStrs.push(HQ.common.getLang("Saturday"));
        }
        return totalStrs.join(", ");
    },

    exportSelectedCust: function (custIDs, branchID, branchName,pJPID,routeID) {
        Ext.net.DirectMethod.request({
            url: "OM23800/ExportSelectedCust",
            isUpload: true,
            formProxyArg: "pnlMCL",
            cleanRequest: true,
            timeout: 1000000,
            params: {
                custIDs: custIDs,
                pBranchID: branchID,
                pBranchName: branchName,
                pJPID: pJPID,
                routeID: routeID
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    },

    updateMcpCusts: function (listMcpCusts) {
        App.winMcpCusts.show();
        App.stoMCPCusts.removeAll();
        listMcpCusts.forEach(function (record) {
            App.stoMCPCusts.insert(App.stoMCPCusts.getCount(), record);
        });
    },

    deleteOverLays: function(item){
        if (item == 'yes') {
            App.frmMain.submit({
                waitMsg: HQ.common.getLang("Deleting") + "...",
                url: 'OM23800/DeleteOverLays',
                type: 'POST',
                timeout: 1000000,
                params: {
                    iID: Declare._ID
                },
                success: function (msg, data) {
                    if (data.result.msgCode) {
                        HQ.message.show(data.result.msgCode, (data.result.msgParam ? data.result.msgParam : ''), '');
                    }
                    var tmpPolygon =  Gmap.Process.find_overlays_id(Declare._ID);
                    tmpPolygon.setMap(null);
                    var tmpLabel = Gmap.Process.find_labels_id(Declare._ID);
                    tmpLabel.setMap(null);
                    //contextMenu.hide();
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
        }
    },

    passNullValue: function (combo) {
        if (combo.value) {
            return combo.value;
        }
        else {
            return "";
        }
    },

    passNullRawValue: function (combo) {
        if (combo.value) {
            return combo.rawValue;
        }
        else {
            return "";
        }
    },

    genColor: function () {
        var color = Process.rgbToHex(Math.floor(Math.random() * 255),
            Math.floor(Math.random() * 255),
            Math.floor(Math.random() * 255));
        return color;
    },

    rgbToHex: function (red, green, blue) {
        var rgb = blue | (green << 8) | (red << 16);
        return (0x1000000 + rgb).toString(16).slice(1);
    },

    renderColor: function (value, metaData, record, rowIndex, colIndex, store) {
        if (App.chkHightLight.checked) {
            metaData.style = "color:#" + record.data.Color + "!IMPORTANT;";
        }
        return value;
    }
};

var Event = {
    Store: {
        stoMCL_load: function (store, records, successful, eOpts) {
            if (successful) {
                var markers = [];
                records.forEach(function (record) {
                    var marker = {
                        "id": record.index + 1,
                        "title": record.data.CustId + ": " + record.data.CustName,
                        "lat": record.data.Lat,
                        "lng": record.data.Lng,
                        "color": record.data.Color,
                        "description": Process.createMarkerDescription(record, App.chkHightLight.checked),
                        "custId": record.data.CustId,
                        "record": record
                    }
                    markers.push(marker);
                });

                //google.maps.event.addListener(Gmap.Declare.map, 'idle', function () {
                    Gmap.Process.drawMCP(markers, App.chkHightLight.checked, App.radMcp.getValue());
                //});

                
                if (markers.length) {
                    Gmap.Process.prepairDrawing();
                }
            }
            if (App.chkOverlays.checked == false)
                App.frmMain.getEl().unmask();
            else
                App.stoOverLays.reload();
        },

        stoOverLays_load: function (store, records, successful, eOpts) {
            if (successful) {
                records.forEach(function (record) {
                    var triangleCoords = [];
                    var tmpOverLays = record.data.LatLng.split(',');
                    var x = 0;
                    for (var i = 0; i < (tmpOverLays.length / 2) ; i++) {
                        triangleCoords.push({ lat: Number(tmpOverLays[x]), lng: Number(tmpOverLays[x + 1]) });
                        x += 2;
                    }
                    if (triangleCoords) {
                        Gmap.Process.prepairDrawingOverlays(triangleCoords, record.data.ID);
                    }
                });
            }
            App.frmMain.getEl().unmask();
        }


    },

    Form: {
        frmMain_boxReady: function () {
            if (HQ.allowModifyCust) {
                App.mniImportCust.enable();
                App.mniTemplateCust.enable();
            }
            else {
                App.mniImportCust.disable();
                App.mniTemplateCust.disable();
            }
        },

        btnHideTrigger_click: function (sender) {
            sender.clearValue();
            if (sender.id == 'cboBranchID_ImExMcp') {
                App.cboPJPID_ImExMcp.clearValue();
                App.cboSlsPerID_ImExMcp.clearValue();
                App.cboRouteID_ImExMcp.clearValue();
            }
        },

        cboAreaMCL_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboProvinceMCL.store.reload();
            App.cboDistributorMCL.store.reload();
        },

        cboProvinceMCL_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboDistributorMCL.store.reload();
        },

        cboDistributorMCL_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboSalesManMCL.store.reload();
            App.cboPJPIDMCL.store.reload();
            App.cboRouteIDMCL.store.reload();
        },

        btnLoadDataPlan_click: function (btn, e, eOpts) {
            if (App.pnlMCL.isValid()) {
                Declare.selBranchID = App.cboDistributorMCL.value;
                Declare.selBranchName = App.cboDistributorMCL.rawValue;
                Declare.selPJPID = App.cboPJPIDMCL.value;
                Declare.selRouteID = App.cboRouteIDMCL.value;
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

        btnExportMCL_click: function (btn, e, eOpts) {
            if (App.pnlMCL.isValid()) {
                Ext.net.DirectMethod.request({
                    url: "OM23800/ExportMCL",
                    isUpload: true,
                    formProxyArg: "pnlMCL",
                    cleanRequest: true,
                    timeout: 1000000,
                    params: {
                        
                        
                        routeID: App.cboRouteIDMCL.getValue(),
                        pjpID: App.cboPJPIDMCL.getValue(),
                        channel: Process.passNullValue(App.cboChannelMCL),
                        channelDescr: Process.passNullRawValue(App.cboChannelMCL),
                        territory: Process.passNullValue(App.cboAreaMCL),
                        territoryDescr: Process.passNullRawValue(App.cboAreaMCL),
                        province: Process.passNullValue(App.cboProvinceMCL),
                        provinceDescr: Process.passNullRawValue(App.cboProvinceMCL),
                        distributor: Process.passNullValue(App.cboDistributorMCL),
                        distributorDescr: Process.passNullRawValue(App.cboDistributorMCL),
                        shopType: Process.passNullValue(App.cboShopTypeMCL),
                        shopTypeDescr: Process.passNullRawValue(App.cboShopTypeMCL),
                        slsperId: Process.passNullValue(App.cboSalesManMCL),
                        slsperIdDescr: Process.passNullRawValue(App.cboSalesManMCL),
                        daysOfWeek: Process.passNullValue(App.cboDayOfWeek),
                        daysOfWeekDescr: Process.passNullRawValue(App.cboDayOfWeek),
                        weekOfVisit: Process.passNullValue(App.cboWeekOfVisit),
                        weekOfVisitDescr: Process.passNullRawValue(App.cboWeekOfVisit),
                    },
                    failure: function (msg, data) {
                        HQ.message.process(msg, data, true);
                    }
                });
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

        mniTemplateMCP_click: function (mni, e, eOpts) {
            App.winImExMcp.isImport = false;
            App.winImExMcp.show();
        },

        mniImportMCP_click: function (mni, e, eOpts) {
            if (HQ.isUpdate) {
                App.winImExMcp.isImport = true;
                App.winImExMcp.show();
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        mniTemplateCust_click: function (mni, e, eOpts) {
            App.winImExCust.isImport = false;
            App.winImExCust.show();
        },

        mniImportCust_click: function (mni, e, eOpts) {
            if (HQ.isUpdate) {
                App.winImExCust.isImport = true;
                App.winImExCust.show();
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        cboBranchID_ImExCust_change: function (cbo, newValue, oldValue, eOpts) {
            if (!App.cboProvinceMCL.value) {
                var selRec = HQ.store.findRecord(cbo.store, ["BranchID"], [cbo.getValue()]);
                if (selRec) {
                    App.cboProvince_ImExCust.setValue(selRec.data.State);
                }
            }
        },

        chkHightLight_change: function (cbo, newValue, oldValue, eOpts) {
            if (cbo.checked) {
                App.ctnMCP.hide();
                App.ctnHighLight.show();
                App.cboColorFor.allowBlank = false;
                App.cboColorFor.validate();
            }
            else {
                App.ctnMCP.show();
                App.ctnHighLight.hide();
                App.cboColorFor.allowBlank = true;
                App.cboColorFor.validate();
            }
        }
    },

    Grid: {
        slmMCP_Select: function (rowModel, record, index, eOpts) {
            if (record && record.data.Lat && record.data.Lng){
                Gmap.Process.navMapCenterByLocation(record.data.Lat, record.data.Lng, record.index + 1);
            }
        },

        grdMCL_viewGetRowClass: function (record, rowIndex, rowParams, store) {
            if (App.chkHightLight.checked) {
                //var clsName = "row-" + record.data.Color,
                //    clsStyle = Ext.String.format(".{0} .x-grid-cell{ color: #{1} !important; }", clsName, record.data.Color);

                //Ext.net.ResourceMgr.registerCssClass(clsName, clsStyle);

                //return clsName;
            }
            else {
                return "row-" + record.data.Color;
            }
        },

        grdMCL_commandEdit: function (item, command, record, index, eOpts) {
            if (HQ.isUpdate) {
                if (command == "Edit") {
                    App.frmHeaderMcp.loadRecord(record);
                    //App.txtCustIDMcpInfo.setValue(record.data.CustId);
                    //App.txtCustNameMcpInfo.setValue(record.data.CustName);
                    //App.txtAddressMcpInfo.setValue(record.data.Addr1);
                    //App.hdnSlsperIDMcpInfo.setValue(record.data.SlsperId);
                    //App.txtSlsperIDMcpInfo.setValue(record.data.SlsperId + "_" + record.data.Name);
                    //App.hdnBranchIDMcpInfo.setValue(record.data.BranchID);
                    //App.txtDistributorMcpInfo.setValue(record.data.Distributor);
                    App.chkCustStatusMcpInfo.setValue(record.data.Status == "A" ? true : false);

                    App.storeMcpInfo.serverProxy.url =
                        Ext.String.format("OM23800/LoadSalesRouteMaster?branchID={0}&custID={1}&slsPerID={2}&pJPID={3}", record.data.BranchID, record.data.CustId, record.data.SlsperId, record.data.PJPID);
                    App.storeMcpInfo.reload();
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
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
            Event.Grid.grdMCL_commandEdit(null, "Edit", record, 0);
        }
    },

    storeMcpInfo_load: function (store, records, successful, eOpts) {
        App.cboRouteIDMcpInfo.store.reload();
        App.winMcpInfo.show();
        var frmHeaderMcpRec = App.frmHeaderMcp.getRecord();
        var record;
        if (store.getCount() > 0) {
            App.btnDeleteMcpInfo.enable();
        }
        else {
            record = Ext.create("App.OM_SalesRouteMasterModel", {
                BranchID: frmHeaderMcpRec.data.BranchID,
                CustID: frmHeaderMcpRec.data.CustId,
                SlsperID: frmHeaderMcpRec.data.SlsperId
            });
            store.insert(0, record);
            App.btnDeleteMcpInfo.disable();
        }
        var record = store.getAt(0);
        App.frmContentMcp.loadRecord(record);
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
        if (HQ.isUpdate) {
            McpInfo.saveMcp();
        }
        else {
            HQ.message.show(4, '', '');
        }
    },

    btnDeleteMcpInfo_click: function () {
        if (HQ.isDelete) {
            HQ.message.show(11, '', 'McpInfo.deleteMcpInfo');
        }
        else {
            HQ.message.show(4, '', '');
        }
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
            App.frmContentMcp.updateRecord();
            var frmHeaderMcpRec = App.frmHeaderMcp.getRecord();

            App.frmMcpInfo.submit({
                waitMsg: 'Submiting...',
                url: 'OM23800/SaveMcp',
                params: {
                    custActive: App.chkCustStatusMcpInfo.value,
                    custID: frmHeaderMcpRec.data.CustId,
                    slsperID: frmHeaderMcpRec.data.SlsperId,
                    branchID: frmHeaderMcpRec.data.BranchID,
                    pJPID: frmHeaderMcpRec.data.PJPID,
                    routeID: App.cboRouteIDMcpInfo.getValue(),
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
                                var selectedMarker = Gmap.Process.find_closest_marker(record.data.Lat, record.data.Lng);
                                if (selectedMarker) {
                                    google.maps.event.addListener(selectedMarker, "click", function (e) {
                                        Gmap.Declare.infoWindow.setContent(Process.createMarkerDescription(record, false));
                                        Gmap.Declare.infoWindow.open(Gmap.Declare.map, selectedMarker);
                                    });
                                    google.maps.event.addListener(Gmap.Declare.map, "click", function (event) {
                                        Gmap.Declare.infoWindow.close();
                                    });

                                    //selectedMarker.icon = "Images/OM30400/circle_" + data.result.Color + ".png";
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

var McpCusts = {
    winMcpCusts_show: function (win, eOpts, tmpOverlays) {
        App.cboRouteIDMcpCusts.store.reload();
        App.cboSlsFreqMcpCusts.store.reload();
        App.dtpStartDateMcpCusts.setValue(HQ.dateNow);
        App.dtpEndDateMcpCusts.setValue(HQ.dateNow);
        //App.cboWeekofVisitMcpCusts.store.reload();
        App.frmMcpCusts.validate();
    },

    dtpStartDateMcpCusts_change: function (dtp, newValue, oldValue, eOpts) {
        App.dtpEndDateMcpCusts.setMinValue(App.dtpStartDateMcpCusts.getValue());
        if (App.dtpEndDateMcpCusts.getValue() < App.dtpStartDateMcpCusts.getValue()) {
            App.dtpEndDateMcpCusts.setValue(App.dtpStartDateMcpCusts.getValue());
        }
    },

    btnSaveMcpCusts_click: function (btn, eOpts) {
        if (App.grdMCPCusts.selModel.selected.items.length > HQ.CountCust) {
            HQ.message.show(201512301, HQ.CountCust, 'McpCusts.saveMcpCust');
        } else {
            McpCusts.saveMcpCust('yes');
        }
      
    },
    saveMcpCust:function(item)
    {
        if (item == 'yes') {
            if (App.frmMcpCusts.isValid()) {
                if (App.grdMCPCusts.selModel.selected.items.length) {
                    App.frmMcpCusts.submit({
                        waitMsg: HQ.common.getLang("Submiting") + "...",
                        url: 'OM23800/SaveMcpCusts',
                        type: 'POST',
                        timeout: 1000000,
                        params: {
                            lstMcpCusts: Ext.encode(App.grdMCPCusts.getRowsValues({ selectedOnly: true }))
                            , routeID: App.cboRouteIDMcpCusts.getValue()
                            , salesFreq: App.cboSlsFreqMcpCusts.getValue()
                            , weekOfVisit: App.cboWeekofVisitMcpCusts.getValue()
                            , sun: App.chkSunMcpCusts.value
                            , mon: App.chkMonMcpCusts.value
                            , tue: App.chkTueMcpCusts.value
                            , wed: App.chkWedMcpCusts.value
                            , thu: App.chkThuMcpCusts.value
                            , fri: App.chkFriMcpCusts.value
                            , sat: App.chkSatMcpCusts.value
                            , startDate: App.dtpStartDateMcpCusts.getValue()
                            , endDate: App.dtpEndDateMcpCusts.getValue()
                            , overLays: Declare._overLays
                            , iID: Declare._ID
                            , cboDistributorMCL: App.cboDistributorMCL.getValue()
                            , cboPJPIDMCL: App.cboPJPIDMCL.getValue()
                        },
                        success: function (msg, data) {
                            if (data.result.msgCode) {
                                HQ.message.show(data.result.msgCode, (data.result.msgParam ? data.result.msgParam : ''), '');
                            }
                            var tmpOverLays = Gmap.Process.find_overlays_index(Number(Declare._zIndex));
                            tmpOverLays.iID = data.result.ID;
                            Gmap.Declare.overlays.push(tmpOverLays);
                            //var tmpLabels = Gmap.Process.find_labels_index(Number(Declare._zIndex));
                            //tmpLabels.iID = data.result.ID;
                            //Gmap.Declare.overlays.push(tmpLabels);
                            App.winMcpCusts.close();
                        },
                        failure: function (msg, data) {
                            HQ.message.process(msg, data, true);
                        }
                    });
                }
                else {
                    HQ.message.show(718, '', '');
                }
            }
        }
    },
    btnCancelMcpCusts_click: function (btn, eOpts) {
        App.winMcpCusts.close();
    }
};

// JS Code for Import/Export MCP window
var ImExMcp = {
    cboBranchID_Change: function (cbo, newValue, oldValue, eOpts) {
        App.cboSlsPerID_ImExMcp.store.reload();
        App.cboRouteID_ImExMcp.store.reload();
        App.cboPJPID_ImExMcp.store.reload();
    },
    cboPJPID_Change: function (cbo, newValue, oldValue, eOpts) {      
    },
    winImExMcp_show: function (win, eOpts) {
        HQ.common.setRequire(App.frmMain_ImExMcp);
        App.cboBranchID_ImExMcp.setValue(App.cboDistributorMCL.value);
        App.cboSlsPerID_ImExMcp.setValue(App.cboSalesManMCL.value);
        App.cboRouteID_ImExMcp.clearValue();
        App.cboPJPID_ImExMcp.clearValue();
        if (win.isImport) {
            App.fupImport_ImExMcp.show();
            App.btnExport_ImExMcp.hide();
        }
        else {
            App.fupImport_ImExMcp.hide();
            App.btnExport_ImExMcp.show();
        }
    },

    fupImport_ImExMcp_change: function (fup, newValue, oldValue, eOpts) {
        if (App.frmMain_ImExMcp.isValid()) {
            var fileName = fup.getValue();
            var ext = fileName.split(".").pop().toLowerCase();
            if (ext == "xls" || ext == "xlsx") {
                ImExMcp.importMCP();

            } else {
                alert("Please choose a Media! (.xls, .xlsx)");
                fup.reset();
            }
        }
        else {
            fup.reset();
        }
    },

    btnExport_ImExMcp_click: function (btn, e, eOpts) {
        if (App.frmMain_ImExMcp.isValid()) {
            App.frmMain_ImExMcp.submit({
                //waitMsg: HQ.common.getLang("Exporting")+"...",
                url: 'OM23800/ExportMCP',
                type: 'POST',
                timeout: 1000000,
                clientValidation: false,
                params: {
                    BranchID: App.cboBranchID_ImExMcp.getValue(),//,
                    BranchName: App.cboBranchID_ImExMcp.getRawValue(),
                    SlsPerID: App.cboSlsPerID_ImExMcp.getValue(),
                    RouteID: App.cboRouteID_ImExMcp.getValue(),
                    PJPID: App.cboPJPID_ImExMcp.getValue()
                },
                success: function (msg, data) {
                    //processMessage(msg, data, true);
                    //menuClick('refresh');
                    var filePath = data.result.filePath;
                    if (filePath) {
                        window.location = "OM23800/Download?filePath=" + filePath + "&fileName=MCP_" + App.cboBranchID_ImExMcp.getValue();
                    }
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
            App.winImExMcp.close();
        }
        else {
            App.frmMain_ImExMcp.getForm().getFields().each(
                function (item) {
                    if (!item.isValid()) {
                        item.focus();
                        return false;
                    }
                }
            );
        }
    },

    importMCP: function () {
        App.frmMain_ImExMcp.submit({
            waitMsg: HQ.common.getLang("Importing"),
            url: 'OM23800/ImportMCP',
            timeout: 18000,
            clientValidation: false,
            method: 'POST',
            params: {
                BranchID: App.cboBranchID_ImExMcp.getValue()
            },
            success: function (msg, data) {
                if (!Ext.isEmpty(this.result.data.message)) {
                    HQ.message.show('2013103001', [this.result.data.message], '', true);
                }
                else {
                    HQ.message.process(msg, data, true);
                }
                App.winImExMcp.close();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

// JS Code for Import/Export Cust window
var ImExCust = {
    winImExCust_show: function (win, eOpts) {
        HQ.common.setRequire(App.frmMain_ImExCust);
        App.cboBranchID_ImExCust.setValue(App.cboDistributorMCL.value);
        App.cboProvince_ImExCust.setValue(App.cboProvinceMCL.value);
        if (win.isImport) {
            App.cboProvince_ImExCust.hide();
            App.cboProvince_ImExCust.allowBlank = true;
            //App.radgActionCust.show();
            //App.radgActionCust.allowBlank = false;

            App.fupImport_ImExCust.show();
            App.btnExport_ImExCust.hide();
        }
        else {
            App.cboProvince_ImExCust.show();
            App.cboProvince_ImExCust.allowBlank = false;
            //App.radgActionCust.hide();
            //App.radgActionCust.allowBlank = true;

            App.fupImport_ImExCust.hide();
            App.btnExport_ImExCust.show();
        }
    },

    fupImport_ImExCust_change: function (fup, newValue, oldValue, eOpts) {
        if (App.frmMain_ImExCust.isValid()) {
            var fileName = fup.getValue();
            var ext = fileName.split(".").pop().toLowerCase();
            if (ext == "xls" || ext == "xlsx") {
                ImExCust.importCust();

            } else {
                alert("Please choose a Media! (.xls, .xlsx)");
                fup.reset();
            }
        }
        else {
            fup.reset();
        }
    },

    btnExport_ImExCust_click: function (btn, e, eOpts) {
        if (App.frmMain_ImExCust.isValid()) {
            App.frmMain_ImExCust.submit({
                //waitMsg: HQ.common.getLang("Exporting")+"...",
                url: 'OM23800/ExportCust',
                type: 'POST',
                timeout: 1000000,
                clientValidation: false,
                params: {
                    branchID: App.cboBranchID_ImExCust.getValue(),//,
                    branchName: App.cboBranchID_ImExCust.getRawValue(),
                    provinces: App.cboProvince_ImExCust.getValue(),
                    provinceRawValue: App.cboProvince_ImExCust.getRawValue(),
                    isUpdated: App.radUpdateCust.value ? true : false
                },
                success: function (msg, data) {
                    //processMessage(msg, data, true);
                    //menuClick('refresh');
                    var filePath = data.result.filePath;
                    if (filePath) {
                        window.location = "OM23800/Download?filePath=" + filePath + "&fileName=MCP_" + App.cboBranchID_ImExCust.getValue();
                    }
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
            App.winImExCust.close();
        }
        else {
            App.frmMain_ImExCust.getForm().getFields().each(
                function (item) {
                    if (!item.isValid()) {
                        item.focus();
                        return false;
                    }
                }
            );
        }
    },

    importCust: function () {
        App.frmMain_ImExCust.submit({
            waitMsg: HQ.common.getLang("Importing"),
            url: 'OM23800/ImportCust',
            timeout: 1000000,
            clientValidation: false,
            method: 'POST',
            params: {
                branchID: App.cboBranchID_ImExCust.getValue(),
                isUpdated: App.radUpdateCust.value ? true : false
            },
            success: function (msg, data) {
                if (!Ext.isEmpty(this.result.data.message)) {
                    HQ.message.show('2013103001', [this.result.data.message], '', true);
                }
                else {
                    HQ.message.process(msg, data, true);
                }
                App.winImExCust.close();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
                App.fupImport_ImExCust.reset();
            }
        });
    }
};

var Gmap = {
    Declare: {
        map_canvas: {},
        map: {},
        directionsService: {},
        directionsDisplay: {},
        directionsDisplays: [],
        infoWindow: {},
        stopMarkers: [],
        drawingManager: {},
        overlays: [],
        labels: []
    },

    Process: {
        initialize: function () {
            Gmap.Declare.map_canvas = document.getElementById("map_canvas");

            var initLatLng = new google.maps.LatLng(10.782171, 106.654012, 17);
            var myOptions = {
                center: initLatLng,
                zoom: 16,
                mapTypeId: google.maps.MapTypeId.ROADMAP
            };

            Gmap.Declare.map = new google.maps.Map(Gmap.Declare.map_canvas, myOptions);
            Gmap.Declare.directionsService = new google.maps.DirectionsService();
            Gmap.Declare.directionsDisplay = new google.maps.DirectionsRenderer();
            Gmap.Declare.infoWindow = new google.maps.InfoWindow();

            Gmap.Declare.stopMarkers = [];
            Gmap.Declare.overlays = [];
            Gmap.Declare.labels = [];
        },

        prepairDrawing: function () {
            if (Gmap.Declare.drawingManager.drawingControl) {
                Gmap.Declare.drawingManager.setMap(Gmap.Declare.map);
            }
            else{
                //var triangleCoords = [
                //    { lat: 25.774, lng: -80.190 },
                //    { lat: 18.466, lng: -66.118 },
                //    { lat: 32.321, lng: -64.757 }
                //];
                Gmap.Declare.drawingManager = new google.maps.drawing.DrawingManager({
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
                        //paths: triangleCoords,
                        fillColor: '#ffff00',
                        fillOpacity: 0,
                        strokeWeight: 5,
                        clickable: true,
                        editable: true,
                        zIndex: 1,
                        iID: 0
                    }//,
                    //paths: triangleCoords
                });
                Gmap.Declare.drawingManager.setMap(Gmap.Declare.map);

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

                // sau khi ve xong: lay ds khach hang trong khung
                google.maps.event.addListener(Gmap.Declare.drawingManager, 'overlaycomplete', function (event) {
                    if (event.type == google.maps.drawing.OverlayType.POLYGON) {
                        var custIDs = [];
                        var listMcpCusts = [];
                        for (var i = 0; i < Gmap.Declare.stopMarkers.length; i++) {
                            if (event.overlay.containsLatLng(Gmap.Declare.stopMarkers[i].position)) {
                                custIDs.push(Gmap.Declare.stopMarkers[i].custId);
                                var mcpCustRec = Gmap.Declare.stopMarkers[i].record;
                                //var mcpCustData = HQ.store.findInStore(App.stoMCPCusts, ["CustId", "SlsperId", "BranchID"], [mcpCustRec.data.CustId, mcpCustRec.data.SlsperId, mcpCustRec.data.BranchID]);
                                //if (!mcpCustData) {
                                listMcpCusts.push(mcpCustRec);
                                //}
                            }
                        }
                        if (custIDs.length) {
                            var labelPoint = event.overlay.getPath().getAt(0);
                            var label = new MarkerWithLabel({
                                icon: " ",
                                position: labelPoint,
                                draggable: false,
                                map: Gmap.Declare.map,
                                labelContent: custIDs.length.toString(),
                                labelAnchor: new google.maps.Point(22, 0),
                                labelClass: "labels", // the CSS class for the label
                                labelStyle: { opacity: 0.75 },
                                zIndex: 1,
                                iID: 0
                            });
                        }
                        Gmap.Declare.overlays.push(event.overlay);
                        Gmap.Declare.labels.push(label);
                        Gmap.Process.showContextMenu(Gmap.Declare.map, event.overlay, label, custIDs, listMcpCusts);
                    }
                });
            }
        },

        showContextMenu: function (objMap, polygon, label, custIDs, listMcpCusts) {
            //	create the ContextMenuOptions object
            var contextMenuOptions = {};
            contextMenuOptions.classNames = { menu: 'context_menu', menuSeparator: 'context_menu_separator' };

            //	create an array of ContextMenuItem objects
            var menuItems = [];
            menuItems.push({ className: 'context_menu_item', eventName: 'update_mcp', label: HQ.common.getLang('UpdateMcp') });
            menuItems.push({ className: 'context_menu_item', eventName: 'export_excel', label: HQ.common.getLang('ExportExcel') });
            menuItems.push({ className: 'context_menu_item', eventName: 'clear_zone', label: HQ.common.getLang('ClearZone') });
            //	a menuItem with no properties will be rendered as a separator
            menuItems.push({});
            menuItems.push({ className: 'context_menu_item', eventName: 'zoom_in_click', label: HQ.common.getLang('ZoomIn') });
            menuItems.push({ className: 'context_menu_item', eventName: 'zoom_out_click', label: HQ.common.getLang('ZoomOut') });
            //	a menuItem with no properties will be rendered as a separator
            menuItems.push({});
            menuItems.push({ className: 'context_menu_item', eventName: 'center_map_click', label: HQ.common.getLang('CenterMapHere') });
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
                    case 'update_mcp':
                        if (listMcpCusts.length) {
                            //Get Array LatLng Polygon 
                            Declare._overLays = '';
                            if(polygon.iID != undefined)
                                Declare._ID = polygon.iID;
                            if (polygon.zIndex != undefined)
                                Declare._zIndex = polygon.zIndex;

                            var vertices = polygon.getPath();
                            for (var i = 0; i < vertices.getLength() ; i++) {
                                var xy = vertices.getAt(i);
                                if(Declare._overLays == '')
                                    Declare._overLays += xy.lat() + ',' + xy.lng();
                                else
                                    Declare._overLays += ',' + xy.lat() + ',' + xy.lng();
                            }
                            
                            Process.updateMcpCusts(listMcpCusts);
                        }
                        contextMenu.hide();
                        break;
                    case 'export_excel':
                        if (custIDs.length) {
                            Process.exportSelectedCust(custIDs, Declare.selBranchID, Declare.selBranchName, Declare.selPJPID, Declare.selRouteID);
                        }
                        contextMenu.hide();
                        break;
                    case 'clear_zone':
                        if (polygon.iID == undefined) {
                            polygon.setMap(null);
                            label.setMap(null);
                            contextMenu.hide();
                        }
                        else {
                            Declare._ID = polygon.iID;
                            label.iID = polygon.iID;
                            HQ.message.show(2016070501, '', 'Process.deleteOverLays');
                        }
                        break;
                    case 'zoom_in_click':
                        Gmap.Declare.map.setZoom(objMap.getZoom() + 1);
                        contextMenu.hide();
                        break;
                    case 'zoom_out_click':
                        Gmap.Declare.map.setZoom(objMap.getZoom() - 1);
                        contextMenu.hide();
                        break;
                    case 'center_map_click':
                        Gmap.Declare.map.panTo(latLng);
                        contextMenu.hide();
                        break;
                }
            });
        },

        // focus toi diem dc chon tren luoi
        navMapCenterByLocation: function (lat, lng, id) {
            var selectedMarker;
            var myLatlng = new google.maps.LatLng(lat, lng);
            Gmap.Declare.map.setCenter(myLatlng);
            Gmap.Declare.map.setZoom(Gmap.Declare.map.getZoom());

            if (id) {
                selectedMarker = Gmap.Process.find_marker_id(id);
                if (!selectedMarker) {
                    selectedMarker = Gmap.Process.find_closest_marker(lat, lng);
                }
            }
            else {
                selectedMarker = Gmap.Process.find_closest_marker(lat, lng);
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
        // tim overlays theo zIndex
        find_overlays_index: function (index) {
            for (i = 0; i < Gmap.Declare.overlays.length; i++) {
                if (Gmap.Declare.overlays[i].zIndex == index) {
                    return Gmap.Declare.overlays[i];
                }
            }
            return null;
        },

        // tim overlays de xoa theo id
        find_overlays_id: function(id){
            for (i = 0; i < Gmap.Declare.overlays.length; i++) {
                if (Gmap.Declare.overlays[i].iID == id) {
                    return Gmap.Declare.overlays[i];
                }
            }
            return null;
        },
        // tim label de xoa theo id
        find_labels_id: function (id) {
            for (i = 0; i < Gmap.Declare.labels.length; i++) {
                if (Gmap.Declare.labels[i].iID == id) {
                    return Gmap.Declare.labels[i];
                }
            }
            return null;
        },
        // tim diem theo id (marker)
        find_marker_id: function (id) {
            for (i = 0; i < Gmap.Declare.stopMarkers.length; i++) {
                if (Gmap.Declare.stopMarkers[i].id == id) {
                    return Gmap.Declare.stopMarkers[i];
                }
            }
            return null;
        },

        // tim diem gan nhat
        find_closest_marker: function (lat1, lon1) {
            var pi = Math.PI;
            var R = 6371; //equatorial radius
            var distances = [];
            var closest = -1;

            for (i = 0; i < Gmap.Declare.stopMarkers.length; i++) {
                var lat2 = Gmap.Declare.stopMarkers[i].position.lat();
                var lon2 = Gmap.Declare.stopMarkers[i].position.lng();

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
            return Gmap.Declare.stopMarkers[closest];
        },

        prepairMap: function () {
            if (Gmap.Declare.directionsDisplay) {
                Gmap.Declare.directionsDisplay.setMap(null);
                if (Gmap.Declare.directionsDisplays && Gmap.Declare.directionsDisplays.length > 0) {
                    for (var i = 0; i < Gmap.Declare.directionsDisplays.length; i++) {
                        Gmap.Declare.directionsDisplays[i].setMap(null);
                        //Gmap.Declare.directionsDisplay[i] = new google.maps.DirectionsRenderer();
                    }
                }
            }
            Gmap.Declare.directionsService = new google.maps.DirectionsService();
            Gmap.Declare.directionsDisplay = new google.maps.DirectionsRenderer();
        },

        clearMap: function (stopMarkers, overlays, labels) {
            for (i = 0; i < overlays.length; i++) {
                overlays[i].setMap(null);
            }

            for (i = 0; i < labels.length; i++) {
                try
                {
                    labels[i].setMap(null);
                }catch(ex)
                {
                }
            }

            for (i = 0; i < stopMarkers.length; i++) {
                stopMarkers[i].setMap(null);

                if (i == stopMarkers.length - 1) {
                    stopMarkers = [];
                }
            }
            Gmap.Declare.directionsDisplay.setMap(Gmap.Declare.map);
        },

        drawMCP: function (markers, hightLight, isMcp) {
            Gmap.Process.prepairMap();
            Gmap.Process.clearMap(Gmap.Declare.stopMarkers, Gmap.Declare.overlays, Gmap.Declare.labels);

            if (markers.length > 0) {
                Gmap.Declare.stopMarkers = [];
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
                            Gmap.Declare.map = new google.maps.Map(Gmap.Declare.map_canvas, myOptions);
                        }

                        // Make the marker at each location
                        if (hightLight) {
                            var marker = new google.maps.Marker({
                                custId: data.custId,
                                record: data.record,
                                id: data.id,
                                position: myLatlng,
                                map: Gmap.Declare.map,
                                title: data.title,
                                icon: isMcp?
                                    Ext.String.format('http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld={0}|{1}|000000', i+1, data.color)
                                    : {
                                        path: google.maps.SymbolPath.CIRCLE,
                                        scale: 5,
                                        fillColor: "#" + data.color,
                                        fillOpacity: 1,
                                        strokeWeight: 0.5
                                    }
                            });
                        }
                        else {
                            var marker = new google.maps.Marker({
                                custId: data.custId,
                                record: data.record,
                                id: data.id,
                                position: myLatlng,
                                map: Gmap.Declare.map,
                                title: data.title,
                                //icon: Ext.String.format('http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld={0}|{1}|000000', i + 1, pinColor)
                                icon: Ext.String.format('Images/OM23800/circle_{0}.png', data.color ? data.color : "white")
                            });
                        }

                        // Set info display of the marker
                        (function (marker, data) {
                            google.maps.event.addListener(marker, "click", function (e) {
                                Gmap.Declare.infoWindow.setContent(data.description);
                                Gmap.Declare.infoWindow.open(Gmap.Declare.map, marker);

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

                            google.maps.event.addListener(Gmap.Declare.map, "click", function (event) {
                                Gmap.Declare.infoWindow.close();
                            });
                        })(marker, data);

                        Gmap.Declare.stopMarkers.push(marker);
                    }
                }

                //Gmap.Declare.directionsDisplay.setMap(Gmap.Declare.map);
                //directionsDisplay.setOptions({ suppressMarkers: true });
            }
            else {
                Gmap.Process.clearMap(Gmap.Declare.stopMarkers, Gmap.Declare.overlays, Gmap.Declare.labels);
            }
        },

        prepairDrawingOverlays: function (triangleCoords, OverLaysID) {

            var bermudaTriangle = new google.maps.Polygon({
                paths: triangleCoords,
                //fillColor: '#ffff00',
                fillOpacity: 0.3,
                strokeWeight: 3,
                clickable: true,
                editable: false,
                iID: OverLaysID
            });
            bermudaTriangle.setMap(Gmap.Declare.map);
            var custIDs = [];
            var listMcpCusts = [];
            for (var i = 0; i < Gmap.Declare.stopMarkers.length; i++) {
                if (bermudaTriangle.containsLatLng(Gmap.Declare.stopMarkers[i].position)) {
                    custIDs.push(Gmap.Declare.stopMarkers[i].custId);
                    var mcpCustRec = Gmap.Declare.stopMarkers[i].record;
                    //var mcpCustData = HQ.store.findInStore(App.stoMCPCusts, ["CustId", "SlsperId", "BranchID"], [mcpCustRec.data.CustId, mcpCustRec.data.SlsperId, mcpCustRec.data.BranchID]);
                    //if (!mcpCustData) {
                    listMcpCusts.push(mcpCustRec);
                    //}
                }
            }
            if (custIDs.length) {
                var labelPoint = bermudaTriangle.getPath().getAt(0);
                var label = new MarkerWithLabel({
                    icon: " ",
                    position: labelPoint,
                    draggable: false,
                    map: Gmap.Declare.map,
                    labelContent: custIDs.length.toString(),
                    labelAnchor: new google.maps.Point(22, 0),
                    labelClass: "labels", // the CSS class for the label
                    labelStyle: { opacity: 0.75 },
                    iID: OverLaysID
                });
            }
            Gmap.Declare.overlays.push(bermudaTriangle);
            Gmap.Declare.labels.push(label);
            Gmap.Process.showContextMenu(Gmap.Declare.map, bermudaTriangle, label, custIDs, listMcpCusts);
            
        }
    }
};