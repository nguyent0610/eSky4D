var Declare = {
    selBranchID: '',
    selBranchName: '',
    _overLays: '',
    _ID: 0,
    _zIndex: 0,
    _stoDetMasterLoad: false
};
var _Source = 0;
var _maxSource = 12;
var _autoCust = "0";
var dsr = [];
var choiceData = [];
var _editCust = false;
var _selectedSales = '';
var _btnLoadClick = false;
var _mapTypeControl = false;
var _scale_factor = 0.6;
var slsMarker;
var slsMarker1;

var Process = {
    createMarkerDescription: function (record, hightLight) {
        var dayOfWeekStr = Process.getDisplayDaysOfWeek(record);
        var link = '<div id="linkContainer" style="width: 100%;">' +
                    '<a class="x-btn-default-toolbar-small-icon" style="float: left;width: 120px;" href="javascript: McpInfo.editFromMap(\'' + record.data.CustId + '\',\'' + record.data.SlsperId + '\',\'' + record.data.BranchID + '\')">' + HQ.common.getLang('OM23800EditMCP') + '</a>' +
                    '<a class="x-btn-default-toolbar-small-icon" style="float:right;width: 160px;" href="javascript: McpInfo.changeCustInfo(\'' + record.data.CustId + '\',\'' + record.data.SlsperId + '\',\'' + record.data.BranchID + '\')">' + HQ.common.getLang('ChangeCustInfo') + '</a>' +
                    '<a class="x-btn-default-toolbar-small-icon" style="margin: 0 10px 0px 10px;width: 120px;"  href="javascript: McpInfo.suggetPosition(\'' + record.data.CustId + '\',\'' + record.data.CustName + '\',\'' + record.data.Addr1 + '\',\'' + record.data.Lat + '\',\'' + record.data.Lng + '\')">' + HQ.common.getLang('SuggestLocation') + '</a>' +
                '<a class="x-btn-default-toolbar-small-icon" style="margin: 0 10px 0px 10px;width: 120px;"  href="javascript: McpInfo.suggetAddr(\'' + record.data.CustId + '\',\'' + record.data.SlsperId + '\',\'' + record.data.BranchID + '\')">' + HQ.common.getLang('SuggestAddress') + '</a>';
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
                    //(record.data.WeekofVisit ? (HQ.common.getLang(record.data.WeekofVisit) + '<br/>') : '') +
                    (record.data.VisitSort ? (HQ.common.getLang("VisitSort") + ': ' + record.data.VisitSort + '<br/>') : '') +
                    (dayOfWeekStr ? (' (' + dayOfWeekStr + ')') : '') +
                '</p>'
                    + (
                        hightLight ? link : link

                    ) +
            '</div>' +
            '</div>' +
        '</div>';

        //        '<a class="x-btn-default-toolbar-small-icon" href="javascript: McpInfo.editFromMap(\'' + record.data.CustId + '\',\'' + record.data.SlsperId + '\',\'' + record.data.BranchID + '\') ">MCP</a>'        
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
                    var tmpPolygon = Gmap.Process.find_overlays_id(Declare._ID);
                    if(tmpPolygon) tmpPolygon.setMap(null);
                    var tmpLabel = Gmap.Process.find_labels_id(Declare._ID);
                    if (tmpLabel) tmpLabel.setMap(null);
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
        if (!Ext.isEmpty(record.data.Color)) { //App.chkHightLight.checked
            metaData.style = "color:#" + record.data.Color + "!IMPORTANT;";
        }
        return value;
    }
};

var Event = {
    Store: {
        StoreRouteIDMCLcheckLoad: function (store, records, successful, eOpts) {
            if (store.data.length > 0) {
                App.cboRouteIDMCL.setValue(store.data.items[0].data.Code);
            }
        },

        StorePJPIDMCLcheckLoad: function (store, records, successful, eOpts) {
            if (store.data.length > 0) {
                App.cboPJPIDMCL.setValue(store.data.items[0].data.PJPID);
            }
        },

        stoMCL_load: function (store, records, successful, eOpts) {
            if (successful) {
                if (_btnLoadClick == true) {
                    App.chkMcpAll.setValue(false);
                    _btnLoadClick = false;
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
            }
            if (App.chkOverlays.checked == false)
                App.frmMain.getEl().unmask();
            else
                App.stoOverLays.reload();
            App.frmMain.resumeLayouts();
            App.grdMCL.view.refresh();
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

            App.cboRouteIDMCL.store.addListener('load', Event.Store.StoreRouteIDMCLcheckLoad);
            App.cboPJPIDMCL.store.addListener('load', Event.Store.StorePJPIDMCLcheckLoad);
        },

        btnHideTrigger_click: function (sender) {
            sender.clearValue();
            if (sender.id == 'cboBranchID_ImExMcp') {
                App.cboPJPID_ImExMcp.clearValue();
                App.cboSlsPerID_ImExMcp.clearValue();
                App.cboRouteID_ImExMcp.clearValue();
            }
            else if (sender.id == 'cboSalesManMCL') {
                App.cboSalesManMCL.store.clearFilter();
            }
        },

        cboAreaMCL_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboProvinceMCL.store.reload();
            App.cboSalesManMCL.store.reload();
        },

        cboProvinceMCL_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboSalesManMCL.store.reload();
        },

        cboDistributorMCL_change: function (cbo, newValue, oldValue, eOpts) {
          //  App.cboSalesManMCL.store.reload();
            App.cboPJPIDMCL.store.reload();
            App.cboRouteIDMCL.store.reload();
        },

        btnLoadDataPlan_click: function (btn, e, eOpts) {
            if (HQ.form.checkRequirePass(App.frmMain)) {
                if (_selectedSales == '') {
                    HQ.message.show(20412, [HQ.common.getLang('Employees')], '', true);
                    return;
                }
                Declare.selBranchID = App.cboDistributorMCL.value;
                Declare.selBranchName = App.cboDistributorMCL.rawValue;
                Declare.selPJPID = App.cboPJPIDMCL.value;
                Declare.selRouteID = App.cboRouteIDMCL.value;
                _btnLoadClick = true;
                App.grdMCL.store.reload();
            }
        },

        btnExportMCL_click: function (btn, e, eOpts) {
            if (HQ.form.checkRequirePass(App.frmMain)) {
                if (_selectedSales == '') {
                    HQ.message.show(20412, [HQ.common.getLang('Employees')], '', true);
                    return;
                }
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
                        distributor: _selectedSales, //Process.passNullValue(App.cboDistributorMCL),
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
            //if (!App.cboProvinceMCL.value) {
            //    var selRec = HQ.store.findRecord(cbo.store, ["BranchID"], [cbo.getValue()]);
            //    if (selRec) {
            //        App.cboProvince_ImExCust.setValue(selRec.data.State);
            //    }
            //}
            App.cboSlsperID_ImExCust.store.reload();
        },

        cboBranchID_ImExCustMCP_change: function (cbo, newValue, oldValue, eOpts) {
            //if (!App.cboProvinceMCL.value) {
            //    var selRec = HQ.store.findRecord(cbo.store, ["BranchID"], [cbo.getValue()]);
            //    if (selRec) {
            //        App.cboProvince_ImExCust.setValue(selRec.data.State);
            //    }
            //}
            App.cboSlsperID_ImExCustMCP.store.reload();
        },
        

        chkHightLight_change: function (cbo, newValue, oldValue, eOpts) {
            //if (cbo.checked) {
            //    //App.ctnNumbering.show();
            //    App.ctnHighLight.show();
            //    App.cboColorFor.allowBlank = false;
            //    App.cboColorFor.validate();
            //}
            //else {
            //    //App.ctnNumbering.hide();
            //    App.ctnHighLight.hide();
            //    App.cboColorFor.allowBlank = true;
            //    App.cboColorFor.validate();
            //}
        }
        , cboColorFor_Change: function (cbo, newValue, oldValue, eOpts) {            
            App.chkHightLight.setChecked(!Ext.isEmpty(newValue));
            App.cboMarkFor.store.reload();
        }

        , btnImportCustMCP_click: function (mni, e, eOpts) {
            if (HQ.isUpdate) {
                App.winImExCustMCP.show();
                App.btnImpCustMCP.show();
                App.btnExpCustMCP.hide();
                App.cboBranchID_ImExCustMCP.allowBlank = true;
                App.cboBranchID_ImExCustMCP.validate();
            } else {
                HQ.message.show(4, '', '');
            }
        }

        , btnExportCustMCP_click: function (mni, e, eOpts) {
            App.btnImpCustMCP.hide();
            App.btnExpCustMCP.show();
            App.winImExCustMCP.show();
            App.cboBranchID_ImExCustMCP.allowBlank = false;
            App.cboBranchID_ImExCustMCP.validate();
        }

        , btnAddNewCustomer_Click: function () {
            if (HQ.isUpdate && HQ.isInsert) {
                App.winAddNewCustomer.show();
            } else {
                HQ.message.show(4);
            }
        }

        , cboChannelMCL_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboShopTypeMCL.clearValue();
            App.cboShopTypeMCL.store.reload();
        }
        , cboChannelMCL_select: function () {
            App.cboShopTypeMCL.clearValue();
            App.cboShopTypeMCL.store.reload();
        }
    },

    Grid: {
        slmMCP_Select: function (rowModel, record, index, eOpts) {
            if (record && record.data.Lat && record.data.Lng) {
                Gmap.Process.navMapCenterByLocation(record.data.Lat, record.data.Lng, McpInfo.getMarkerID(record.index));
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

        chkMcpAll_change: function (chk, newValue, oldValue, eOpts) {
            var record;
            var length = App.grdMCL.store.snapshot.length;
            for (var i = 0; i < length; i++) {
                record = App.grdMCL.store.snapshot.items[i];
                record.data.Selected = chk.value;
            }
            App.grdMCL.store.commitChanges();
            App.grdMCL.view.refresh();    
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

    changeCustInfo: function(custId, slsperId, branchId) {
        if (HQ.isUpdate && HQ.isInsert) {
            _editCust = true;
            App.winAddNewCustomer.show();
            HQ.common.setForceSelection(App.frmDetail, false, "cboAddBranchID,cboAddCustID");
            App.cboAddBranchID.setValue(branchId);
            App.cboAddCustID.setValue(custId);
            
        } else {
            HQ.message.show(4);
        }
    },

    suggetPosition: function (custId, custName, addr, lat, lng) {
        if (HQ.isUpdate && HQ.isInsert) {

            Gmap.Declare.geocoder = new google.maps.Geocoder();
            if (Gmap.Declare.suggestMarker) {
                Gmap.Declare.suggestMarker.setMap(null);
                Gmap.Declare.suggestMarker = null;
            }
            Gmap.Declare.geocoder.geocode({ 'address': addr }, function (results, status) {
                if (status == google.maps.GeocoderStatus.OK) {
                    var location = results[0].geometry.location;
                    if (location.lat() != lat || location.lng() != lng) {
                        Gmap.Declare.map.setCenter(location);
                        Gmap.Declare.suggestMarker = new google.maps.Marker({
                            map: Gmap.Declare.map,
                            position: location,
                            icon: 'Images/OM23800/flaggreen.png',
                            animation: google.maps.Animation.DROP,
                           // record: record
                        });

                        google.maps.event.addListener(Gmap.Declare.suggestMarker, "click", function (e) {
                            // Set animation of marker
                            if (Gmap.Declare.suggestMarker.getAnimation() != null) {
                                Gmap.Declare.suggestMarker.setAnimation(null);
                            } else {
                                Gmap.Declare.suggestMarker.setAnimation(google.maps.Animation.BOUNCE);
                                setTimeout(function () {
                                    Gmap.Declare.suggestMarker.setAnimation(null);
                                    McpInfo.showSuggestPopup(custId, custName, addr, lat, lng, location.lat(), location.lng(), Gmap.Declare.suggestMarker);
                                }, 1400);
                            }
                        });
                    }
                    else {
                        HQ.message.show(20150618, '', '');
                    }
                } else {
                    HQ.message.show(20150612, status, '');
                }
            });            

        } else {
            HQ.message.show(4);
        }
    },

    showSuggestPopup: function (custId, custName, addr, lat, lng, newLat, newLng, suggestMarker) {
        //suggestMarker.position.A or F
        // App.frmMain_Suggest.loadRecord(record);
        App.txtCustID_Suggest.setValue(custId);
        App.txtCustName_Suggest.setValue(custName);
        App.txtAddr_Suggest.setValue(addr);
        App.frmMain_Suggest.suggestMarker = suggestMarker;
        App.frmMain_Suggest.newLat = newLat;
        App.frmMain_Suggest.newLng = newLng;

        if (lat && lng) {
            var crrAddr = "https://maps.googleapis.com/maps/api/staticmap?key=" + HQ.googleAPIKey + "&zoom=16&size=300x200&maptype=roadmap&markers=color:red%7Clabel:B%7C";
            App.imgMarkerOld_Suggest.setImageUrl(
                crrAddr
                + lat + ","
                + lng);
        }
        else {
            App.imgMarkerOld_Suggest.setImageUrl("Images/OM23800/maps.jpg");
        }
        var addrNew = "https://maps.googleapis.com/maps/api/staticmap?key=" + HQ.googleAPIKey + "&zoom=16&size=300x200&maptype=roadmap&markers=color:green%7Clabel:A%7C";
        App.imgMarkerNew_Suggest.setImageUrl(
            addrNew 
            + newLat + "," + newLng);

        App.winSuggest.show();

    },

    btnSuggestPosition_click: function (btn, e, eOpts) {
        if (HQ.isUpdate) {
            if (App.grdMCL.selModel.selected.items.length) { // chỉ chọn 1 dòng
                var selected = App.grdMCL.selModel.selected.items[0];
                McpInfo.suggetPosition(selected.data.CustId, selected.data.CustName, selected.data.Addr1, selected.data.Lat, selected.data.Lng);
            }
            else {
                HQ.message.show(718, '', '');
            }
        }
        else {
            HQ.message.show(4, '', '');
        }
    },

    updateNewPostion: function () {
        var record = HQ.store.findRecord(App.grdMCL.store, ["CustId"], [App.txtCustID_Suggest.getValue()]);
        var newLat = App.frmMain_Suggest.newLat;
        var newLng = App.frmMain_Suggest.newLng;
        var suggestMarker = App.frmMain_Suggest.suggestMarker;
        App.frmMain_Suggest.submit({
            waitMsg: 'Submiting...',
            url: 'OM23800/UpdateNewPosition',
            timeout: 1800000,
            params: {
                lstSelCust: (function () {
                    values = []
                    values.push(record.data);
                    return Ext.encode(values);
                })(),
                newLat: newLat,
                newLng: newLng
            },
            success: function (action, data) {
                if (data.result.msgcode) {
                    HQ.message.show(data.result.msgCode, (data.result.msgParam ? data.result.msgParam : ''), '');
                }
                var rec = HQ.store.findRecord(App.grdMCL.store, ["CustId"], [App.txtCustID_Suggest.getValue()]);
                if (rec) {
                    rec.set('Lat', newLat);
                    rec.set('Lng', newLng);
                    setTimeout(function () {
                        var latlng = new google.maps.LatLng(newLat, newLng);
                        Gmap.Declare.geocoder.geocode({
                            'latLng': latlng
                        }, function (results, status) {
                            if (status === google.maps.GeocoderStatus.OK) {
                                if (results[0]) {
                                    var suggested = {
                                        Lat: newLat,
                                        Lng: newLng,
                                        Addr: results[0].formatted_address
                                    };
                                    rec.set("SuggestAddr", results[0].formatted_address);
                                } else {
                                    console.log('No results found. CustID: ' + rec.data.CustId);
                                }
                            } else {
                                console.log('Geocoder failed due to: ' + status + '. CustID: ' + rec.data.CustId);
                            }
                        });
                    }, 2000);

                    if (data.result.tstamp) {
                        rec.set('tstamp', data.result.tstamp);
                    }
                    var markerId = McpInfo.getMarkerID(rec.index);
                    var marker;
                    if (markerId) {
                        marker = Gmap.Process.find_marker_id(markerId);
                    }

                    if (marker) {
                        var myLatlng = new google.maps.LatLng(newLat, newLng);
                        marker.setPosition(myLatlng);
                        marker.setAnimation(google.maps.Animation.BOUNCE);
                        marker.visible = true;
                        setTimeout(function () {
                            marker.setAnimation(null);
                        }, 1400);
                    }
                    else {

                        var markerData = {
                            "id": markerId,
                            "title": record.data.CustId + ": " + record.data.CustName,
                            "lat": record.data.Lat,
                            "lng": record.data.Lng,
                            "color": record.data.Color,
                            "description": Process.createMarkerDescription(record, App.chkHightLight.checked),
                            "custId": record.data.CustId,
                            "record": record
                        }                        
                        Gmap.Process.makeMarker(markerData, markerId, App.chkHightLight.checked, App.radMcp.getValue());
                    }
                }
                App.winSuggest.close();
                if (suggestMarker) {
                    suggestMarker.setMap(null);
                    suggestMarker = null;
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
    },

    btnUpdate_Suggest_click: function (btn, e, eOpts) {
        if (HQ.isUpdate) {
            McpInfo.updateNewPostion();
        }
        else {
            HQ.message.show(4, '', '');
        }
    },

    btnUpdateAddr_click: function () {
        if (HQ.isUpdate) {
            if (McpInfo.isCustSelected(App.grdMCL.store)) {
                HQ.message.show(50, '', 'McpInfo.updateAddress');
            }
            else {
                HQ.message.show(20412, HQ.common.getLang('Customer'), '');
            }
        }
        else {
            HQ.message.show(4, '', '');
        }
    },

    updateAddress: function (item) {
        if (item == "yes") {
            if (App.frmMain.isValid()) {
                App.frmMain.submit({
                    waitMsg: HQ.waitMsg,
                    url: 'OM23800/UpdateAddress',
                    timeout: 1800000,
                    params: {
                        lstSelCust: (function () {
                            values = []
                            App.grdMCL.store.snapshot.each(function (record) {
                                if (record.data.Selected == true && record.data.SuggestAddr != '') {
                                    values.push(record.data);
                                }
                            });
                            return Ext.encode(values);
                        })()
                    },
                    success: function (action, data) {
                        if (data.result.msgcode) {
                            HQ.message.show(data.result.msgCode, (data.result.msgParam ? data.result.msgParam : ''), '');
                        }
                        var listCustDone = Ext.decode(data.result.listCustDone);

                        if (listCustDone && listCustDone.length) {
                            App.grdMCL.store.snapshot.each(function (record) {
                                listCustDone.forEach(function (custDone) {
                                    if (record.data.Selected == true && record.data.CustId == custDone.CustId) { 
                                        record.set('Addr1', custDone.Addr1);
                                        record.set('SuggestAddr', "");
                                        record.set('Selected', false);
                                        record.set('CustTstamp', custDone.tstamp);
                                        record.commit();
                                        return false;
                                    }
                                });
                            });
                            App.grdMCL.store.commitChanges();
                            App.grdMCL.view.refresh();
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
    },

    suggetAddr: function (custId, slsperId, branchId) {
        if (HQ.isUpdate && HQ.isInsert) {
            if (custId == undefined) {
                if (McpInfo.isCustSelected(App.grdMCL.store)) {
                    
                    var totalSelRow = 0;
                    App.grdMCL.store.snapshot.each(function (record) {
                        if (record.data.Selected == true) {
                            totalSelRow++;
                        }
                    });
                    var minute = Math.round((totalSelRow * 2) / 60);// Math.round((App.grdMCL.store.snapshot.length * 2) / 60);
                    if (minute == 0) {
                        minute = 1;
                    }
                    HQ.message.show(2015102601, minute, 'McpInfo.suggestAddress');
                }
            } else {                
                var record = HQ.store.findRecord(App.grdMCL.store, ['CustId', 'BranchID'], [custId, branchId]);
                if (record) {
                    App.frmMain.getEl().mask(HQ.common.getLang('FindingAddress'), 'x-mask-loading');
                    McpInfo.getAddressByGeo(App.grdMCL.store, 0, record);
                }                
            }            
        } else {
            HQ.message.show(4);
        }
    },

    getAddressByGeo: function (store, idx, selRecord) {
        var isContinued = false;        
        if (selRecord != undefined) {
            var record = selRecord;
        } else {
            var record = store.snapshot.getAt(idx);
        }
        if (record) {
            if (
                (record.data.Selected == true || selRecord != undefined)
                && record.data.Lat && record.data.Lng
                && record.data.Lat != 1 && record.data.Lng != 1
                && !record.data.SuggestAddr) {
                var useService = true;
                Gmap.Declare.bankOfSuggested.forEach(function (suggested) {
                    if (suggested.Lat == record.data.Lat && suggested.Lng == record.data.Lng) {
                        record.set("SuggestAddr", suggested.Addr);
                        useService = false;
                        return false;
                    }
                });

                if (useService) {
                    setTimeout(function () {
                        var latlng = new google.maps.LatLng(record.data.Lat, record.data.Lng);
                        Gmap.Declare.geocoder.geocode({
                            'latLng': latlng
                        }, function (results, status) {
                            if (status === google.maps.GeocoderStatus.OK) {
                                if (results[0]) {
                                    var suggested = {
                                        Lat: record.data.Lat,
                                        Lng: record.data.Lng,
                                        Addr: results[0].formatted_address
                                    };
                                    Gmap.Declare.bankOfSuggested.push(suggested);
                                    //App.grdMCL.selModel.select(record);
                                    record.set("SuggestAddr", results[0].formatted_address);
                                } else {
                                    console.log('No results found. CustID: ' + record.data.CustId);
                                }
                            } else {
                                console.log('Geocoder failed due to: ' + status + '. CustID: ' + record.data.CustId);
                            }                            
                            if (selRecord != undefined) {
                                App.frmMain.getEl().unmask();
                            } else {
                                idx = idx + 1;
                                McpInfo.getAddressByGeo(store, idx);
                            }                           
                        });
                    }, 2000);
                }
                else {
                    isContinued = true;
                }
            }
            else {
                isContinued = true;
            }

            if (selRecord != undefined) {
                App.frmMain.getEl().unmask();                
            }
            else if (isContinued) {
                idx = idx + 1;
                McpInfo.getAddressByGeo(store, idx);
            }
        }
        else {
            App.frmMain.getEl().unmask();
        }
    },

    suggestAddress: function (item) {
        if (item == "yes") {
            App.frmMain.getEl().mask(HQ.common.getLang('FindingAddress'), 'x-mask-loading');           
            McpInfo.getAddressByGeo(App.grdMCL.store, 0);
        }
    },

    isCustSelected: function (store) {
        var selected = false;
        store.each(function (record) {
            if (record.data.Selected) {
                selected = true;
                return false;
            }
        });
        return selected;
    },

    btnResetGeo_click: function (btn, e, eOpts) {
        if (HQ.isUpdate) {
            if (McpInfo.isCustSelected(App.grdMCL.store)) {
                HQ.message.show(2015052201, '', 'McpInfo.resetGeo');
            }
            else {
                HQ.message.show(20412, HQ.common.getLang('Customer'), '');
            }
        }
        else {
            HQ.message.show(4, '', '');
        }
    },
    resetGeo: function (item) {
        if (item == "yes") {
           // if (App.grdMCL.selModel.selected.items.length > 0) {
                var values = [];
                App.grdMCL.store.snapshot.each(function (record) {
                    if (record.data.Selected == true) {
                        values.push(record.data);
                    }
                });
                App.frmMain.submit({
                    waitMsg: HQ.waitMsg,
                    url: 'OM23800/ResetGeo',
                    timeout: 1800000,
                    params: {
                        lstSelCust: Ext.encode(values)
                    },
                    success: function (action, data) {
                        if (data.result.msgcode) {
                            HQ.message.show(data.result.msgCode, (data.result.msgParam ? data.result.msgParam : ''), '');
                        }
                        var listCustDone = Ext.decode(data.result.listCustDone);

                        if (listCustDone && listCustDone.length) {
                            App.grdMCL.store.snapshot.each(function (record) {
                                listCustDone.forEach(function (custDone) {
                                    if (record.data.Selected == true && record.data.CustId == custDone.CustID) {
                                        record.data.Lat = 0;
                                        record.data.Lng = 0;
                                        record.data.Selected = false;
                                        record.data.tstamp = custDone.tstamp;
                                        record.commit();
                                        var markerId = McpInfo.getMarkerID(record.index);
                                        var marker;
                                        if (markerId) {
                                            marker = Gmap.Process.find_marker_id(markerId);
                                        }

                                        if (marker) {
                                            marker.setAnimation(google.maps.Animation.BOUNCE);
                                            setTimeout(function () {
                                                marker.setAnimation(null);
                                                marker.visible = false;
                                            }, 1400);
                                        }
                                        return false;
                                    }
                                });
                            });
                            App.grdMCL.store.commitChanges();
                            App.grdMCL.view.refresh();
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
            //}
        }
    },

    dtpStartDateMcpInfo_change: function (dtp, newValue, oldValue, eOpts) {
        if(App.dtpStartDateMcpInfo.getValue())
            App.dtpEndDateMcpInfo.setMinValue(App.dtpStartDateMcpInfo.getValue());
        if (App.dtpEndDateMcpInfo.getValue() < App.dtpStartDateMcpInfo.getValue()) {
            App.dtpEndDateMcpInfo.setValue(App.dtpStartDateMcpInfo.getValue());
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
        HQ.common.showBusy(false);
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
        if (HQ.form.checkRequirePass(App.frmMcpInfo)) {
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
                    startDate: App.dtpStartDateMcpInfo.getValue(),
                    endDate: App.dtpEndDateMcpInfo.getValue(),
                    salesFreq: App.cboSlsFreqMcpInfo.getValue(),
                    weekOfVisit: App.cboWeekofVisitMcpInfo.getValue(),
                    visitSort: App.numVisitSortMcpInfo.getValue(),
                    sun: App.chkSunMcpInfo.value,
                    mon: App.chkMonMcpInfo.value,
                    tue: App.chkTueMcpInfo.value,
                    wed: App.chkWedMcpInfo.value,
                    thu: App.chkThuMcpInfo.value,
                    fri: App.chkFriMcpInfo.value,
                    sat: App.chkSatMcpInfo.value
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
                            if (data.result[fields[i]] != "undefined") {
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
                        Event.Grid.grdMCL_viewGetRowClass(record);// Event.Form.btnLoadDataPlan_click(); ///
                    }
                    
                },

                failure: function (errorMsg, data) {
                    HQ.message.process(errorMsg, data, true);
                    
                    //if (data.result.msgCode) {
                    //    HQ.message.show(data.result.msgCode, (data.result.msgParam ? data.result.msgParam : ''), '');
                    //}
                    //else {
                    //    HQ.message.process(errorMsg, data, true);
                    //}
                }
            });
        }
    }

    , getMarkerID: function (idx) {
        //(App.grdMCL.store.currentPage - 1) * App.grdMCL.store.pageSize
        return (idx + 1);

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
            if (HQ.form.checkRequirePass(App.frmMcpCusts)) {
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
        if (HQ.form.checkRequirePass(App.frmMain_ImExMcp)) {
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
        //App.cboChannel_ImExCust.setValue('');
        App.cboSlsperID_ImExCust.setValue('');
        if (!Ext.isEmpty(App.cboDistributorMCL.value)) {
            App.cboBranchID_ImExCust.setValue(App.cboDistributorMCL.value);
        } else {
            App.cboBranchID_ImExCust.setValue('');
        }
        
        App.cboProvince_ImExCust.setValue(App.cboProvinceMCL.value);
        if (win.isImport) {
            App.winImExCust.setHeight(140);
            App.cboBranchID_ImExCust.allowBlank = true;

            //App.cboChannel_ImExCust.hide();
            //App.cboChannel_ImExCust.allowBlank = true;
            App.cboTerritory_ImExCust.hide();
            App.cboProvince_ImExCust.hide();
            App.cboProvince_ImExCust.allowBlank = true;

            App.cboSlsperID_ImExCust.hide();

            App.fupImport_ImExCust.show();
            App.btnExport_ImExCust.hide();
        }
        else {
            App.winImExCust.setHeight(240);
            App.cboBranchID_ImExCust.allowBlank = false;

            //App.cboChannel_ImExCust.show();
            //App.cboChannel_ImExCust.allowBlank = false;
            App.cboTerritory_ImExCust.show();
            App.cboProvince_ImExCust.show();

            App.cboSlsperID_ImExCust.show();

            App.fupImport_ImExCust.hide();
            App.btnExport_ImExCust.show();
        }
        HQ.common.setRequire(App.frmMain_ImExCust);
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
        if (HQ.form.checkRequirePass(App.frmMain_ImExCust)) {
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
                    slsperID: joinParams(App.cboSlsperID_ImExCust),
                  //  channel: App.cboChannel_ImExCust.getValue(),
                    territory: App.cboTerritory_ImExCust.getValue(),
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
                if (this.result.data && !Ext.isEmpty(this.result.data.message)) {
                    HQ.message.show('2013103001', [this.result.data.message], '', true);
                } else if (!Ext.isEmpty(this.result.code)) {
                    HQ.message.show(this.result.code, [this.result.parm[0]], '', true);
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

    , btnImport_ImExCustMCP_change: function (fup, newValue, oldValue, eOpts) {
        if (HQ.form.checkRequirePass(App.frmMain_ImExCustMCP)) {
            var fileName = fup.getValue();
            var ext = fileName.split(".").pop().toLowerCase();
            if (ext == "xls" || ext == "xlsx") {
                ImExCust.importCustMCP();

            } else {
                alert("Please choose a Media! (.xls, .xlsx)");
                fup.reset();
            }
        }
        else {
            fup.reset();
        }
    }

    , btnExport_ImExCustMCP_click: function (btn, e, eOpts) {
        if (HQ.form.checkRequirePass(App.frmMain_ImExCustMCP)) {
            App.frmMain_ImExCustMCP.submit({
                //waitMsg: HQ.common.getLang("Exporting")+"...",
                url: 'OM23800/ExportCustMCP',
                type: 'POST',
                timeout: 1000000,
                clientValidation: false,
                params: {
                    branchID: App.cboBranchID_ImExCustMCP.getValue(),//,
                    branchName: App.cboBranchID_ImExCustMCP.getRawValue(),
                    provinces: App.cboProvince_ImExCustMCP.getValue(),
                    provinceRawValue: App.cboProvince_ImExCustMCP.getRawValue(),
                    slsperID: joinParams(App.cboSlsperID_ImExCustMCP),                    
                    territory: App.cboTerritory_ImExCustMCP.getValue(),
                    isUpdated: App.radUpdateCustMCP.value ? true : false
                },
                success: function (msg, data) {
                    var filePath = data.result.filePath;
                    if (filePath) {
                        window.location = "OM23800/Download?filePath=" + filePath + "&fileName=MCP_" + App.cboBranchID_ImExCustMCP.getValue();
                    }
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });
            App.winImExCustMCP.close();
        }        
    }

    , importCustMCP: function () {
        App.frmMain_ImExCustMCP.submit({
            waitMsg: HQ.common.getLang("Importing"),
            url: 'OM23800/ImportCustMCP',
            timeout: 1000000,
            clientValidation: false,
            method: 'POST',
            params: {
                branchID: App.cboBranchID_ImExCustMCP.getValue(),
                isUpdated: App.radUpdateCustMCP.value ? true : false
            },
            success: function (msg, data) {
                if (this.result.data && !Ext.isEmpty(this.result.data.message)) {
                    HQ.message.show('2013103001', [this.result.data.message], '', true);
                } else if (!Ext.isEmpty(this.result.code)) {
                    var param = '';
                    if (this.result.parm != undefined && this.result.parm[0] != undefined) {
                        param = this.result.parm[0];
                    }
                    HQ.message.show(this.result.code, [param], '', true);
                }
                else {
                    HQ.message.process(msg, data, true);
                }
                App.winImExCustMCP.close();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
                App.btnImpCustMCP.reset();
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
        labels: [],
        seaMarkers: [],
        seaMarkers1: [],

        geocoder: {},
        suggestMarker: null,
        bankOfSuggested: []
    },

    Process: {
        initialize: function () {
            Gmap.Declare.map_canvas = document.getElementById("map_canvas");

            var initLatLng = new google.maps.LatLng(10.782171, 106.654012, 17);
            var myOptions = {
                center: initLatLng,
                zoom: 16,
                mapTypeId: google.maps.MapTypeId.ROADMAP
                , mapTypeControl: _mapTypeControl
            };

            Gmap.Declare.map = new google.maps.Map(Gmap.Declare.map_canvas, myOptions);
            Gmap.Declare.directionsService = new google.maps.DirectionsService();
            Gmap.Declare.directionsDisplay = new google.maps.DirectionsRenderer();
            Gmap.Declare.infoWindow = new google.maps.InfoWindow();
            Gmap.Declare.geocoder = new google.maps.Geocoder();
            Gmap.Declare.stopMarkers = [];
            Gmap.Declare.overlays = [];
            Gmap.Declare.labels = [];

            Gmap.Process.scaleImage1(Gmap.Declare.map.getZoom());
            Gmap.Declare.map.addListener('zoom_changed', function () {
                Gmap.Process.scaleImage(Gmap.Declare.map.getZoom());
                Gmap.Process.scaleImage1(Gmap.Declare.map.getZoom());
            });
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
            menuItems.push({ className: 'ccboDistributorMCL_changeontext_menu_item', eventName: 'update_mcp', label: HQ.common.getLang('UpdateMcp') });
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
                            if(label!= undefined)
                                label.iID = polygon.iID;
                            HQ.message.show(2016070501, '', 'Process.deleteOverLays');
                            contextMenu.hide();
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
            var selectedMarker = null;
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
                if (Gmap.Declare.overlays[i] != undefined) {
                    if (Gmap.Declare.overlays[i].iID == id && Gmap.Declare.overlays[i].H != undefined) {
                        return Gmap.Declare.overlays[i];
                    }
                }
            }
            return null;
        },
        // tim label de xoa theo id
        find_labels_id: function (id) {
            for (i = 0; i < Gmap.Declare.labels.length; i++) {
                if (Gmap.Declare.labels[i] != undefined) {
                    if (Gmap.Declare.labels[i].iID == id) {
                        return Gmap.Declare.labels[i];
                    }
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
                _scale_factor = 0.5;
                if (markers.length > 1000) {
                    _scale_factor = 0.7;
                } else if (markers.length > 100) {
                    _scale_factor = 0.6;
                }
                Gmap.Declare.stopMarkers = [];
                var bounds = new google.maps.LatLngBounds();
                // For each marker in list
                for (i = 0; i < markers.length; i++) {
                    var data = markers[i];
                    Gmap.Process.makeMarker(data, i, hightLight, isMcp, bounds);
                }
                Gmap.Declare.map.fitBounds(bounds);
                //Gmap.Declare.directionsDisplay.setMap(Gmap.Declare.map);
                //directionsDisplay.setOptions({ suppressMarkers: true });
            }
            else {
                Gmap.Process.clearMap(Gmap.Declare.stopMarkers, Gmap.Declare.overlays, Gmap.Declare.labels);
            }
        },

        makeMarker: function (data, i, hightLight, isMcp, bounds) {
            if (data.lat && data.lng) {
                var myLatlng = new google.maps.LatLng(data.lat, data.lng);
                if (bounds) {
                    bounds.extend(myLatlng);
                }
                
                // Maps center at the first location
                if (i == 0) {
                    var myOptions = {
                        center: myLatlng,
                        zoom: 16,
                        mapTypeId: google.maps.MapTypeId.ROADMAP
                        , mapTypeControl: _mapTypeControl
                    };
                    Gmap.Declare.map = new google.maps.Map(Gmap.Declare.map_canvas, myOptions);
                    Gmap.Process.scaleImage1(Gmap.Declare.map.getZoom());
                    Gmap.Declare.map.addListener('zoom_changed', function () {
                        Gmap.Process.scaleImage(Gmap.Declare.map.getZoom());
                        Gmap.Process.scaleImage1(Gmap.Declare.map.getZoom());
                    });
                }

                // Make the marker at each location
                // if (hightLight) {

                
                var color = '000000';
 
                //Use scaledSize instead of size:
                //var labelIcon = App.chkNumberingCust.getValue() == true ? i + 1 : '';
                if (App.chkNumberingCust.getValue()) {                                       
                    var image = {
                        url: Ext.String.format('https://chart.googleapis.com/chart?chst=d_map_spin&chld={3}|0|{1}|10|_|{0}', i + 1, data.color, color, _scale_factor), // image is 512 x 512
                        // scaledSize: new google.maps.Size(22, 32)
                    };
                    //var image = {
                    //    url: Ext.String.format('http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld={0}|{1}|{2}', i + 1, data.color, color), // image is 512 x 512
                    //    // scaledSize: new google.maps.Size(22, 32)
                    //};
                    
                } else {
                    var image = {
                        url: Ext.String.format('http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld={0}|{1}|{2}', '', data.color, color), // image is 512 x 512
                        scaledSize: new google.maps.Size(10, 15)
                    };
                }
                    var marker = new google.maps.Marker({
                        custId: data.custId,
                        record: data.record,
                        id: data.id,
                        position: myLatlng,
                        map: Gmap.Declare.map,
                        title: data.title,
                        draggable: true,
                        icon: isMcp ?
                            image //Ext.String.format('http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld={0}|{1}|000000', i + 1, data.color)
                            : {
                                path: google.maps.SymbolPath.CIRCLE,
                                scale: 3,
                                fillColor: "#" + data.color,
                                fillOpacity: 1,
                                strokeWeight: 0.5
                            }
                    });
                //}
                //else {
                //    var marker = new google.maps.Marker({
                //        custId: data.custId,
                //        record: data.record,
                //        id: data.id,
                //        position: myLatlng,
                //        map: Gmap.Declare.map,
                //        title: data.title,
                //        draggable: true,
                //        //icon: Ext.String.format('http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld={0}|{1}|000000', i + 1, pinColor)
                //        icon: Ext.String.format('Images/OM23800/circle_{0}.png', data.color ? data.color : "white")
                //    });
                //}

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

                    google.maps.event.addListener(marker, 'dragend', function (e) {
                        var newLat = e.latLng.lat();
                        var newLng = e.latLng.lng();
                        var record = HQ.store.findRecord(App.grdMCL.store, ["CustId"], [data.custId]);
                        if (record) {
                            McpInfo.showSuggestPopup(record.data.CustId, record.data.CustName, record.data.Addr1, record.data.Lat, record.data.Lng, newLat, newLng);
                            var oldLatLng = new google.maps.LatLng(record.data.Lat, record.data.Lng);
                            marker.setPosition(oldLatLng);
                        } else {
                            marker.setPosition(myLatlng);
                        }
                    });


                    google.maps.event.addListener(Gmap.Declare.map, "click", function (event) {
                        Gmap.Declare.infoWindow.close();
                    });
                })(marker, data);

                Gmap.Declare.stopMarkers.push(marker);
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

        , scaleImage: function (zoom) {
            var lat = 13.471608;
            var lng = 109.669605;
            var size = zoom > 8 ? zoom * 35 : 1;
        
            var height = size;
            if (zoom > 14) {
                height = (size + 350) * 2;
                lat = 13.576581291896971;
                lng = 109.68578338623047;
            }
            else if (zoom == 14) {
                height = (size + 350) * 2;
                //  lat = 14.071608;            
            }
            else if (zoom == 13) {
                height = (size + 350);
              //  lat = 14.071608;            
            } else {
                size = size - 150;
                if (size < 0) {
                    size = 1;
                }
            }
            var image1 = {
                url: 'Images/OM23800/EastSea.png',
                scaledSize: new google.maps.Size(size, height), // scaled size
            };
            Gmap.Process.clearMap(Gmap.Declare.seaMarkers);
            Gmap.Declare.seaMarkers = [];
       
            var markers = [];        
            var
                marker = {
                    "id": "00001", // Zoom 8, 10, 11
                    "title": "East Vietnam Sea",
                    "lat": lat,
                    "lng": lng,
                    "color": 'f44e42',
                    "Icon": image1,
                    "Zindex": 1
            }
            markers.push(marker);

            if (markers.length > 0) {
                if (slsMarker != undefined) {
                    slsMarker.setVisible(false);
                    slsMarker = null;
                }
                // List of locations
                var lat_lng = new Array();

                // For each marker in list
                for (i = 0; i < markers.length; i++) {
                    var data = markers[i];
                    if (data.lat && data.lng) {
                        var myLatlng = new google.maps.LatLng(data.lat, data.lng);

                        // Push the location to list
                        lat_lng.push(myLatlng);

                        // Make the marker at each location
                        var markerLabel = i + 1;
                        slsMarker = new google.maps.Marker({
                            id: data.id,
                            position: myLatlng,
                            map: Gmap.Declare.map,
                            title: data.title,
                            icon: data.Icon,
                            zIndex: data.Zindex
                        });
                        Gmap.Declare.seaMarkers.push(slsMarker);
                    }
                }

                Gmap.Declare.directionsDisplay.setMap(Gmap.Declare.map);
            }
            else {
                Gmap.Process.clearMap(Gmap.Declare.seaMarkers);
            }
        },

        scaleImage1: function (zoom) {
            var lat1 = 15.288091715896193;
            if (zoom < 9) {
                lat1 = 14.488091715896193;
            }
            var size = zoom > 2 ? zoom * 35 : 1;
            var height = zoom > 2 ? zoom * 35 : 1;
            if (zoom > 13) {
                lat1 = 15.480151358815984;
                height = height * 1;
            }
            else if (zoom == 13) {
                lat1 = 15.480151358815984;
                height = height * 1;
            } else if (zoom == 12) {
               // lat1 = 14.487261;
                height = height * 2;
            }
            else  if ( zoom < 12 && zoom > 9) {
            
            }
            else if (zoom < 9 && zoom > 2) {            
                if (zoom == 8) {
                    size = size / 2;
                    height = height / 2;
                    lat1 = 14.888091715896193;
                }
                else if (zoom == 7) {
                    size = size / 2;
                    height = height / 2;
                }
                else if (zoom == 6) {
                    size = size / 2;
                    height = height / 2;                
                }
                else if (zoom == 5) {
                    size = size / 3;
                    height = height / 5;
                } else if (zoom == 4) {
                    size = 55;
                    height = 25;
                } else if (zoom == 3) {
                    size = 45;
                    height = 25;
                    lat1 = 14.000091715896193;
                } else {
                    size = 45;
                    height = 25;                
                }
            } else if (zoom <= 2) {
                size = size / 2;
                height = height / 2;
            }
       
            var image = {
                url: 'Images/OM23800/EastSea.png',
                scaledSize: new google.maps.Size(size, height), // scaled size
            };
            Gmap.Process.clearMap(Gmap.Declare.seaMarkers1);
            Gmap.Declare.seaMarkers1 = [];

            var markers = [];
            var
            marker = {
                "id": "00002", // 9
                "title": "",
                "lat": lat1,
                "lng": 114.40475296229124,
                "color": 'f44e42',
                "Icon": image,
                "Zindex": 1
            }
            markers.push(marker);


            if (markers.length > 0) {
                if (slsMarker1 != undefined) {
                    slsMarker1.setVisible(false);
                    slsMarker1 = null;
                }
                // List of locations
                var lat_lng = new Array();

                // For each marker in list
                for (i = 0; i < markers.length; i++) {
                    var data = markers[i];
                    if (data.lat && data.lng) {
                        var myLatlng = new google.maps.LatLng(data.lat, data.lng);

                        // Push the location to list
                        lat_lng.push(myLatlng);

                        // Make the marker at each location
                        slsMarker1 = new google.maps.Marker({
                            id: data.id,
                            position: myLatlng,
                            map: Gmap.Declare.map,
                            title: data.title,
                            icon: data.Icon,
                            zIndex: data.Zindex
                        });
                        Gmap.Declare.seaMarkers1.push(slsMarker1);
                    }
                }

                Gmap.Declare.directionsDisplay.setMap(Gmap.Declare.map);
            }
            else {
                Gmap.Process.clearMap(Gmap.Declare.seaMarkers1);
            }
        },
        clearMap: function (stopMarkers) {
            for (i = 0; i < stopMarkers.length; i++) {
                stopMarkers[i].setMap(null);

                if (i == stopMarkers.length - 1) {
                    stopMarkers = [];
                }
            }
            Gmap.Declare.directionsDisplay.setMap(Gmap.Declare.map);
        },
    }
};

joinParams = function (multiCombo) {
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
}

var Popup = {
    winAddNewCustomer_Show: function () {
        if (!_editCust) {
            App.cboAddBranchID.clearValue();
            App.cboAddCustID.setValue('');
        }
    },

    winAddNewCustomer_Close: function () {
        Popup.frmChange();
        if (HQ.isDetailChange) {
            HQ.message.show(5, '', 'Popup.confirmClose', '');
        } else {
            App.winAddNewCustomer.close();
        }
    },

    confirmClose: function(item){
        if (item != 'yes') {
            App.winAddNewCustomer.close();
            HQ.isDetailChange = false;
            App.stoAR_Customer.reload();
        }
    },

    stoAR_Customer_Load: function(sto) {
        HQ.common.setForceSelection(App.frmDetail, false, "cboAddBranchID,cboAddCustID");
        App.cboAddBranchID.forceSelection = true;
      //  App.cboAddCustID.forceSelection = true;
        HQ.isNew = false;

        if (sto.data.length == 0) {
            HQ.store.insertBlank(sto, ['BranchID', 'CustId']);
            record = sto.getAt(0);
            record.set('CustType', 'L');
            record.set('TaxDflt', 'C');
            record.set('CrRule', 'A');
            record.set('Status', 'A');
            //record.set('ExpiryDate', HQ.bussinessDate);
            //record.set('EstablishDate', HQ.bussinessDate);
            //record.set('Birthdate', HQ.bussinessDate);
            //record.set('Territory', value1);
            //record.set('Area', value2);
            HQ.isNew = true;
           // App.cboAddCustID.forceSelection = false;
            if (_autoCust == "1") {
            //    App.cboAddCustID.forceSelection = true;
            }
            HQ.common.setRequire(App.frmDetail);
            sto.commitChanges();
        }
        else {            
           // Popup.setPage4Cust(App.cboAddCustID.getValue());
        }
        var record = sto.getAt(0);
        App.frmDetail.getForm().loadRecord(record);

        // display image
        App.fupProfilePicImages.reset();
        if (record.data.ProfilePic) {
            Popup.displayImage(App.imgProfilePicImages, record.data.ProfilePic);
        }
        else {
            App.imgProfilePicImages.setImageUrl("");
        }
        // display image
        App.fupBusinessPicImages.reset();
        if (record.data.BusinessPic) {
            Popup.displayImage(App.imgBusinessPic, record.data.BusinessPic);
        }
        else {
            App.imgBusinessPic.setImageUrl("");
        }

       // HQ.common.lockItem(App.frmDetail, record.data.Status != 'H');

        if (!HQ.isInsert && HQ.isNew) {
            //App.cboAddCustID.forceSelection = true;
            HQ.common.lockItem(App.frmDetail, true);           
        }
        else if (!HQ.isUpdate && !HQ.isNew) {
            HQ.common.lockItem(App.frmDetail, true);           
        }        
        HQ.common.showBusy(false, '', App.frmDetail);
        Popup.frmChange();
    },

    checkLoad: function (sto) {
        _Source += 1;
        if (_Source == _maxSource) {
            _stoMasterDetLoad = true;
            _Source = 0;
            App.stoAR_Customer.reload();
            HQ.common.showBusy(false, '', App.frmDetail);            
        }
        
    },

    stoCheckAutoCustID_Load : function () {
        if (App.stoCheckAutoCustID.data.items[0].data.Flag == '1') {
            _autoCust = "1";
            App.cboAddCustID.allowBlank = true;
            App.cboAddCustID.isValid(false);
            App.cboAddCustID.setReadOnly(true);
            //App.cboAddCustID.forceSelection = true;
        }
        else {
            _autoCust = "0";
            App.cboAddCustID.allowBlank = false;
            App.cboAddCustID.isValid(true);
            App.cboAddCustID.setReadOnly(false);
            //App.cboAddCustID.forceSelection = false;
        }
    },

    firstLoad: function () {
        _stoMasterDetLoad = false;
        HQ.common.showBusy(true, HQ.waitMsg, App.frmDetail);

        App.cboAddBranchID.getStore().addListener('load', Popup.checkLoad);
        App.cboAddCustChain.getStore().addListener('load', Popup.checkLoad);
        App.cboAddCustClass.getStore().addListener('load', Popup.checkLoad);
        App.cboAddTerritory.getStore().addListener('load', Popup.checkLoad);
        App.cboAddLocation.getStore().addListener('load', Popup.checkLoad);

        App.cboAddChannel.getStore().addListener('load', Popup.checkLoad);
        App.cboAddShopType.getStore().addListener('load', Popup.checkLoad);
        App.cboAddState.getStore().addListener('load', Popup.checkLoad);
        App.cboAddDistrict.getStore().addListener('load', Popup.checkLoad);

        App.cboAddDeliveryUnit.getStore().addListener('load', Popup.checkLoad);
        App.cboAddSalesProvince.getStore().addListener('load', Popup.checkLoad);
        App.cboAddClassification.getStore().addListener('load', Popup.checkLoad);
        //App.cboState.getStore().addListener('load', Popup.checkLoad);
        //App.cboDistrict.getStore().addListener('load', Popup.checkLoad);


        App.cboAddBranchID.getStore().reload();
        App.cboAddCustChain.getStore().reload();
        App.cboAddCustClass.getStore().reload();
        App.cboAddTerritory.getStore().reload();
        App.cboAddLocation.getStore().reload();

        App.cboAddChannel.getStore().reload();
        App.cboAddShopType.getStore().reload();
        App.cboAddState.getStore().reload();
        App.cboAddDistrict.getStore().reload();
        
        App.cboAddDeliveryUnit.store.reload();
        App.cboAddSalesProvince.store.reload();
        App.cboAddClassification.getStore().reload();
        App.frmDetail.isValid();
    },

    menufrmDetail_Click: function (command) {
        if (App.frmDetail.body.isMasked()) return;
        switch (command) {            
            case "save":
                if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                    //checkRequire để kiếm tra các field yêu cầu có rỗng hay ko
                    if (HQ.form.checkRequirePass(App.frmDetail)) {
                        Popup.save();
                    }
                }
                break;
            case "delete":
                if (HQ.isDelete && App.stoAR_Customer.data.length > 0 && App.stoAR_Customer.data.items[0].data.Status == 'H') {
                    if (App.cboAddCustID.getValue()) {
                        HQ.message.show(11, '', 'Popup.deleteData');
                    } else {
                        menuClick('new');
                    }
                }                
                break;
            case "new":
                if (HQ.isInsert) {
                    if (HQ.isDetailChange) {
                        HQ.message.show(20150303, '', 'Popup.refresh');
                    } else {
                        App.cboAddCustID.setValue('');
                    }                    
                }
                break;
            case "refresh":
                if (HQ.isDetailChange) {
                    HQ.message.show(20150303, '', 'Popup.refresh');
                }
                else {
                    HQ.isDetailChange = false;
                    App.stoAR_Customer.reload();
                }
                break;
            default:
        }
    },
    frmChange: function (sender) {

        if (App.stoAR_Customer.getCount() > 0) {
            App.frmDetail.updateRecord();
        }
        HQ.isDetailChange = HQ.store.isChange(App.stoAR_Customer);
        App.cboAddBranchID.setReadOnly(HQ.isDetailChange);
        if (HQ.isDetailChange) {
            App.btnFindCust.disable();
        } else {
            App.btnFindCust.enable();
        }
        if (_autoCust == "0") {
            App.cboAddCustID.setReadOnly(HQ.isDetailChange);
        }
       // App.btnFindCust.setReadOnly(HQ.isDetailChange);
        //HQ.common.changeData(HQ.isDetailChange, 'OM');//co thay doi du lieu gan * tren tab title header
    },
    stoChanged: function (sto) {
        Popup.frmChange();
    }


    , cboAddBranchID_Change: function (sender, value) {
        if (!_editCust) {
            App.cboAddCustID.setValue('');
        } else {
            _editCust = false;
        }
        
        App.cboAddSlsperID.setValue('');
        if (sender.valueModels != null) {
            
            App.cboAddSlsperID.store.reload();
           // App.cboAddCustID.store.reload();
            App.stoCheckAutoCustID.reload();
        } else {
            App.cboAddSlsperID.store.clearData();
            //App.cboAddCustID.store.clearData();
            App.stoCheckAutoCustID.clearData();
        }
    }, 
    cboAddBranchID_Select: function (sender, value) {  
        if (sender.valueModels != null && !App.stoAR_Customer.loading) {
           // HQ.common.showBusy(true, HQ.waitMsg, App.frmDetail);
            App.cboAddCustID.setValue('');

            App.cboAddSlsperID.setValue('');
            App.cboAddSlsperID.store.reload();

            //App.cboAddCustID.store.reload();
            App.stoCheckAutoCustID.reload();
        }
    }

    , cboAddCustID_Change : function (sender, value) {
        //if (sender.valueModels != null && !App.stoAR_Customer.loading) {
            if (App.cboAddCustID.allowBlank == false && HQ.isDetailChange == true) {
                HQ.message.show(150, '', '');
                sender.setValue(sender.originalValue);
                return;
            }
            HQ.isNew = false;
            App.stoAR_Customer.reload();
        //} else {
        //    HQ.isNew = true;
        //}
    }

    , cboAddCustID_Select : function (sender, value) {
        HQ.isFirstLoad = true;
      //  if (sender.valueModels != null && !App.stoAR_Customer.loading) {
            if (App.cboAddCustID.allowBlank == false && HQ.isChange == true) {
                HQ.message.show(150, '', '');
                sender.setValue(sender.originalValue);
                return;
            }
            App.stoAR_Customer.reload();
     //   }
    }
    , cboAddCustID_TriggerClick : function (sender, value) {
        if (App.cboAddCustID.allowBlank == false && HQ.isDetailChange == true) {
            HQ.message.show(150, '', '');
            return;
        }
        App.cboAddCustID.setValue('');
    }
    ///////DataProcess///
    , save: function () {
        if (HQ.isUpdate) {
            App.frmDetail.mask();
            App.frmDetail.getForm().updateRecord();
            if (HQ.form.checkRequirePass(App.frmDetail)) {
                //  var crrCustID = App.cboAddCustID.getValue();
                App.frmDetail.submit({
                    waitMsg: HQ.common.getLang('SavingData'),
                    method: 'POST',
                    url: 'OM23800/SaveCustomer',
                    timeout: 1800000,
                    params: {
                        objHeader: Ext.encode(App.stoAR_Customer.getRecordsValues()),
                        CustId: App.cboAddCustID.getValue()
                    },
                    success: function (msg, data) {
                        HQ.isDetailChange = false;
                        App.frmDetail.unmask();
                        // HQ.message.process(msg, data, true);
                        HQ.message.show(201405071);
                        // App.cboAddCustID.getStore().reload({
                        //callback: function () {                            
                        //App.cboAddCustID.forceSelection = false;
                        //  Popup.setPage4Cust(data.result.CustId);
                        //App.cboAddCustID.setValue(data.result.CustId);
                        App.stoAR_Customer.reload();
                        data.result.CustId = '';
                        //   }
                        // });

                    },
                    failure: function (msg, data) {
                        HQ.message.process(msg, data, true);
                        // App.stoAR_Customer.reload();
                        App.frmDetail.unmask();
                    }
                });
            }
        } else {
            HQ.message.show(4);
        }
    },
    deleteData: function (item) {
        if (item == 'yes') {
            App.frmDetail.getForm().updateRecord();
            App.frmDetail.submit({
                waitMsg: HQ.common.getLang("DeletingData"),
                url: 'OM23800/DeleteCustomer',
                timeout: 7200,
                success: function (msg, data) {
                    HQ.isDetailChange = false;
                    //App.cboAddCustID.getStore().reload();
                    HQ.message.process(msg, data, true);
                    App.stoAR_Customer.reload();                                        
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                }
            });           
        }

    },

    refresh: function (item) {
        if (item == 'yes') {
            HQ.isDetailChange = false;
            App.stoAR_Customer.reload();

        }
    },

    filterComboSate: function (sender, e) {
        if (sender.hasFocus) {
            App.cboAddState.setValue('');           
            App.cboAddDistrict.setValue('');
        }

        var code = App.cboAddTerritory.getValue();
        App.cboAddState.store.clearFilter();
        App.cboAddState.store.filter("Territory", code);
    },

    filterComboCityDistrict : function (sender, e) {
        if (sender.hasFocus) {
            App.cboAddDistrict.setValue('');
        }
        var code = App.cboAddState.getValue();
        App.cboAddDistrict.store.clearFilter();
        App.cboAddDistrict.store.filter("CountryState", code);
    }

    , setPage4Cust: function (custID) {
        var objPageCust = Popup.findRecordCombo(custID);
        if (objPageCust) {
            var positionCust = Popup.calcPage(objPageCust.index);
            App.cboAddCustID.loadPage(positionCust);
        }
    }

    , findRecordCombo : function (value) {
        var data = null;
        var store = App.cboAddCustID.store;
        var allRecords = store.snapshot || store.allData || store.data;
        allRecords.each(function (record) {
            if (record.data.CustID == value) {
                data = record;
                return false;
            }
        });
        return data;
    }
    , calcPage : function (value) {
        var tmpValue = (Number(value) + 1) / 20;
        if (Number.isInteger(tmpValue))
            return Number(tmpValue);
        else
            return Math.floor(Number(tmpValue)) + 1;
    }

     , btnClearProfileImageImage_click: function (sender, e) {
         App.fupBusinessPicImages.reset();
         App.imgProfilePicImages.setImageUrl("");
         App.hdnProfilePicImages.setValue("");
     }

    , fupProfilePicImages_change: function (fup, newValue, oldValue, eOpts) {
        if (fup.value) {
            var ext = fup.value.split(".").pop().toLowerCase();
            if (ext == "jpg" || ext == "png" || ext == "gif") {
                App.hdnProfilePicImages.setValue(fup.value);
                Popup.readImage(fup, App.imgProfilePicImages);
            }
            else {
                HQ.message.show(148, '', '');
            }
        }
    }

    , btnClearBusinessPicImage_click: function (sender, e) {
        App.fupBusinessPicImages.reset();
        App.imgBusinessPic.setImageUrl("");
        App.hdnBusinessPicImages.setValue("");
    }

    , fupBusinessPicImages_change: function (fup, newValue, oldValue, eOpts) {
        if (fup.value) {
            var ext = fup.value.split(".").pop().toLowerCase();
            if (ext == "jpg" || ext == "png" || ext == "gif") {
                App.hdnBusinessPicImages.setValue(fup.value);
               Popup.readImage(fup, App.imgBusinessPic);
            }
            else {
                HQ.message.show(148, '', '');
            }
        }
    }
    ,  readImage: function (fup, imgControl) {
        var files = fup.fileInputEl.dom.files;
        if (files && files[0]) {
            var FR = new FileReader();
            FR.onload = function (e) {
                imgControl.setImageUrl(e.target.result);
           
            };
            FR.readAsDataURL(files[0]);
        }
    }


    , displayImage : function (imgControl, fileName) {
        Ext.Ajax.request({
            url: 'OM23800/ImageToBin',
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            params: JSON.stringify({
                fileName: fileName
            }),
            success: function (result) {
                var jsonData = Ext.decode(result.responseText);
                if (jsonData.imgSrc) {
                    imgControl.setImageUrl(jsonData.imgSrc);
               
                }
                else {
                    imgControl.setImageUrl("");
                }
            },
            failure: function (errorMsg, data) {
                HQ.message.process(errorMsg, data, true);
            }
        });
    }
};

var btnFindCust_Click = function (item, e) {
    if (Ext.isEmpty(App.cboAddBranchID.getValue())) {
        HQ.message.show(1000, [App.cboAddBranchID.fieldLabel], '', true);
        return false;
    }
    App.grdCustID.store.addListener('load', function () {
        HQ.common.showBusy(false, HQ.waitMsg, App.winCustID);
    });
    App.grdCustID.store.reload();

    App.winCustID.show();
}
var btnSearch_Click = function (item, e) {
    HQ.common.showBusy(true, HQ.waitMsg, App.winCustID);
    App.grdCustID.store.reload();
}
var grdCustID_CellDblClick = function (grid, row, col, rec) {
    HQ.objCustSelect = rec.data;
    App.cboAddCustID.setValue(rec.data.CustID);
    App.frmDetail.unmask();
    App.frmMain.unmask();
    App.winCustID.hide();
}

var showLargeImage = function (imgControl) {
    App.winImage.setHeight(App.frmMain.getHeight() - 20);
    App.winImage.setWidth(App.frmMain.getHeight() - 10);    
    App.imgView.setImageUrl(imgControl.imageUrl);
    App.winImage.show();
}

// Tree search
var filterTree = function (el, e) {
    var tree = App.treeAVC,
        text = App.txtTreeSearch.rawValue.toLowerCase().unsign().trim();
    tree.clearFilter();
    if (Ext.isEmpty(text, false)) {
        return;
    }
    filterTreeByByValue(tree, text);
};

var filterTree_TriggerClick = function () {
    var tree = App.treeAVC,
        text = App.txtTreeSearch.rawValue.toLowerCase().unsign();
    tree.clearFilter();
    if (Ext.isEmpty(text, false)) {
        tree.expandAll();
        return;
    } else {
        filterTreeByByValue(tree, text);
    }
};

var getNodeValue = function (nodeText) {
    var index = nodeText.indexOf('>');
    if (index > -1) {
        nodeText = nodeText.substring(index);
    }
    index = nodeText.indexOf('<');
    if (index > -1) {
        return nodeText.substring(1, index).toLowerCase().unsign();
    } else {
        return nodeText.toLowerCase().unsign();
    }
}

var filterTreeByByValue = function (tree, text) {
    Ext.suspendLayouts();
    var lstParent = [];
    tree.filterBy(function (node) {
        var nodeText = getNodeValue(node.data.text);
        var fieldData = nodeText.indexOf(text);
        if (fieldData > -1) {
            lstParent.push(node.id);
            return node;
        } else {
            if (lstParent.indexOf(node.parentNode.id) > -1) {
                lstParent.push(node.id);
                return node;
            }
        }
    });

    Ext.getCmp('pnlTreeAVC').body.scrollTo('top', 0);
    Ext.resumeLayouts();
}

var clearFilter = function () {
    var field = App.txtTreeSearch,
        tree = App.treeAVC;

    field.setValue("");
    tree.clearFilter(true);
    tree.getView().focus();
};

var tree_CheckChange = function (item, checked, options) {
    App.frmMain.mask();
    App.frmMain.suspendLayouts();
    checkNode(checked, item);
    if (checked == true) {
        checkParentNode(item);
    } else {
        unCheckParentNode(item);
    }
    if (checked == true) {
        var nodeBranch = null;
        if (item.data.Type == 'S') {
            item.set('checked', checked);
        } else if (item.data.Type == 'M') {
            item.set('checked', checked);
        } else if (item.data.Type == 'B') {
            nodeBranch = item;
            nodeBranch.set('checked', checked);
        }
        else if (item.data.Type == 'T') {
            item.set('checked', checked);
        } else if (item.data.Type == 'N') {
            item.set('checked', checked);
        }
        else if (item.data.Type == 'R') {
            item.set('checked', checked);
        }
    }
    else {
        if (item.data.Type == 'M' || item.data.Type == 'S' || item.data.Type == 'T') {
            _selectedSales = '';
        }
    }
    getSelected();


    App.frmMain.resumeLayouts();
    App.frmMain.doLayout();
    App.frmMain.unmask();
}

var checkNode = function (checked, node) {
    if (node.childNodes.length > 0) {
        for (var i = 0; i < node.childNodes.length; i++) {
            node.set('checked', checked)
            checkNode(checked, node.childNodes[i]);
        }
    }
    var index = dsr.indexOf(node.data.Data);
   
    if (checked)
        if (index == -1) {
            dsr.push(node.data.Data);
        }
    if (!checked)
        if (index != -1) {
            dsr.splice(index, 1);
        }
    
    node.set('checked', checked);
}


var checkParentNode = function (node) {
    if (node.parentNode != null) {
        //if (node.data.Type == 'S' || node.data.Type == 'M' || node.data.Type == 'T') {
        node.parentNode.set('checked', true);
        checkParentNode(node.parentNode);
        //}
    }
}

var clearCheckNode = function () {
    App.treeAVC.clearChecked();
    dsr = [];
    dsrColor = [];
}

var unCheckParentNode = function (node) {
    if (node.parentNode != null) { // && (node.parentNode.data.Type == 'M' || node.parentNode.data.Type == 'S'||  node.parentNode.data.Type == 'T')) {
        var flat = false;
        for (var i = 0; i < node.parentNode.childNodes.length; i++) {
            if (node.parentNode.childNodes[i].data.checked) {
                flat = true;
                break;
            }
        }
        if (!flat) {
            node.parentNode.set('checked', false);
            unCheckParentNode(node.parentNode);
        }
    }
}

var getSelected = function () {
    //if (dsr == undefined) {
        dsr = [];
    //}
    var allNodes = getDeepAllLeafNodes(App.treeAVC.getRootNode(), true);
    allNodes.forEach(function (node) {
        if (node.data.checked) {
            if (node.data.Type != 'B') {
                var index = dsr.indexOf(node.data.Data);
                if (index == -1) {
                    dsr.push(node.data.Data);
                }
            }
            if (node.childNodes.length > 0) {
                getChildSelected(node);
            }
        }
    });
    setValueAllCurrentSales();
}

var getChildSelected = function (node) {
    for (var i = 0; i < node.childNodes.length; i++) {
        if (node.childNodes[i].data.checked) {
            if (node.childNodes[i].data.Type != 'B') {
                var index = dsr.indexOf(node.childNodes[i].data.Data);
                if (index == -1) {
                    dsr.push(node.childNodes[i].data.Data);
                }
            }
            if (node.childNodes[i].childNodes.length) {
                getChildSelected(node.childNodes[i]);
            }
        }

        
    }
}
var getDeepAllLeafNodes = function (node, onlyLeaf) {
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
         }
        );
    }
    return allNodes;
};

var expandAll = function () {
    if (App.treeAVC.getRootNode() != undefined) {
        App.frmMain.mask();
        Ext.suspendLayouts();
        App.treeAVC.getRootNode().expand();
        var length = App.treeAVC.getRootNode().childNodes.length;
        if (length > 0) {
            for (var i = 0; i < length; i++) {
                if (App.treeAVC.getRootNode().childNodes[i].childNodes.length > 0) {
                    expandNode(App.treeAVC.getRootNode().childNodes[i]);
                }
            }
        }
        Ext.resumeLayouts(true);
        App.frmMain.unmask();
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

var setValueAllCurrentSales = function () {
    _selectedSales = "";
    for (var i = 0; i < dsr.length; i++) {
        _selectedSales += dsr[i] + ",";
    }
};

var firstLoadTree = function() {
    getSelected();
}