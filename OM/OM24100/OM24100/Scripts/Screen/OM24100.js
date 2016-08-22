var Declare = {

};

var Event = {
    Store: {
        stoMCP_load: function (store, records, successful, eOpts) {
            if (successful) {
                var markers = [];
                records.forEach(function (record) {
                    var marker = {
                        "id": record.data.CustId,
                        "title": record.data.CustId + ": " + record.data.CustName,
                        "lat": record.data.Lat,
                        "lng": record.data.Lng,
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
                Gmap.Process.drawMCP(markers, App.chkNumber.value);
            }
            App.frmMain.getEl().unmask();
        }
    },

    Form: {
        btnHideTrigger_click: function (sender) {
            sender.clearValue();
        },

        cboAreaPlan_change: function (cbo, newValue, oldValue, eOpts) {
            //App.cboProvincePlan.store.reload();
            App.cboDistributorPlan.store.reload();
        },

        cboDistributorPlan_change: function (cbo, newValue, oldValue, eOpts) {
            App.cboSalesManPlan.store.reload();
        },

        btnLoadDataPlan_click: function (btn, e, eOpts) {
            if (App.pnlMCP.isValid()) {
                App.grdMCP.store.reload();
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

        btnResetGeo_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (Process.isCustSelected(App.grdMCP.store)) {
                    HQ.message.show(2015052201, '', 'Process.resetGeo');
                }
                else {
                    HQ.message.show(20412, HQ.common.getLang('Customer'), '');
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnExportToExcel_click: function (btn, e, eOpts) {
            if (App.pnlMCP.isValid()) {
                Ext.net.DirectMethod.request({
                    url: "OM24100/ExportMcpExcel",
                    isUpload: true,
                    formProxyArg: "pnlMCP",
                    cleanRequest: true,
                    timeout: 1000000,
                    params: {
                        channel: "",
                        territory: Process.passNullValue(App.cboAreaPlan),
                        province: Process.passNullValue(App.cboProvincePlan),
                        distributor: Process.passNullValue(App.cboDistributorPlan),
                        shopType: "",
                        slsperId: Process.passNullValue(App.cboSalesManPlan),
                        daysOfWeek: Process.passNullValue(App.cboDayOfWeek),
                        weekOfVisit: Process.passNullValue(App.cboWeekOfVisit),

                        territoryHeader: Process.passNullRawValue(App.cboAreaPlan),
                        distributorHeader: Process.passNullRawValue(App.cboDistributorPlan),
                        slsperHeader: Process.passNullRawValue(App.cboSalesManPlan),
                        daysOfWeekHeader: Process.passNullRawValue(App.cboDayOfWeek),
                        weekOfVisitHeader: Process.passNullRawValue(App.cboWeekOfVisit)
                    },
                    failure: function (msg, data) {
                        HQ.message.process(msg, data, true);
                    }
                });
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

        btnSuggest_click: function (btn, e, eOpts) {
            if (App.slmMCP.getSelection().length) {
                var selected = App.slmMCP.getSelection()[0];
                Gmap.Process.getGeobyAddress(selected);
            }
            else {
                HQ.message.show(718, '', '');
            }
        },

        btnUpdate_Suggest_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                Process.updateNewPostion();
            }
            else {
                HQ.message.show(4, '', '');
            }
        },

        btnSuggestAddr_click: function (btn, e, eOpts) {
            if (App.grdMCP.store.getCount()) {
                HQ.message.show(2015102601, Math.round((App.grdMCP.store.getCount() * 2) / 60), 'Process.suggestAddress');
            }
        },

        btnUpdateAddr_click: function (btn, e, eOpts) {
            if (HQ.isUpdate) {
                if (Process.isCustSelected(App.grdMCP.store)) {
                    HQ.message.show(50, '', 'Process.updateAddress');
                }
                else {
                    HQ.message.show(20412, HQ.common.getLang('Customer'), '');
                }
            }
            else {
                HQ.message.show(4, '', '');
            }
        }
    },

    Grid: {
        chkMcpAll_change: function (chk, newValue, oldValue, eOpts) {
            var record;
            var length= App.grdMCP.store.getCount();
            for (var i = 0; i < length; i++) {
                record = App.grdMCP.store.data.items[i];
                record.data.Selected = chk.value;
            }
            App.grdMCP.store.commitChanges();
            App.grdMCP.view.refresh();
            //App.grdMCP.store.commitChanges();
            //App.grdMCP.store.each(function (record) {
            //    record.set("Selected", chk.value);
            //});
        },

        slmMCP_Select: function (rowModel, record, index, eOpts) {
            if (record && record.data.Lat && record.data.Lng){
                Gmap.Process.navMapCenterByLocation(record.data.Lat, record.data.Lng, record.data.kCustId);
            }
        },

        grdMCP_viewGetRowClass: function (record, rowIndex, rowParams, store) {
            if (!record.data.Lat || !record.data.Lng) {
                return "row-FF0000";
            }
        },

        grdMCP_edit: function (editor, e) {
            var selectedCount = 0;
            if (e.field == "Selected") {
                e.grid.store.each(function (record) {
                    if (record.data.Selected) {
                        selectedCount++;
                    }
                });
            }

            if (selectedCount == e.grid.store.getCount()) {
                App.chkMcpAll.suspendCheckChange = true;
                App.chkMcpAll.setValue(true);
                App.chkMcpAll.suspendCheckChange = false;
            }
            else {
                App.chkMcpAll.suspendCheckChange = true;
                App.chkMcpAll.setValue(false);
                App.chkMcpAll.suspendCheckChange = false;
            }
        }
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
            };

            Gmap.Declare.map = new google.maps.Map(Gmap.Declare.map_canvas, myOptions);
            Gmap.Declare.directionsService = new google.maps.DirectionsService();
            Gmap.Declare.directionsDisplay = new google.maps.DirectionsRenderer();
            Gmap.Declare.infoWindow = new google.maps.InfoWindow();
            Gmap.Declare.geocoder = new google.maps.Geocoder();

            Gmap.Declare.stopMarkers = [];
        },

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

        find_marker_id: function (id) {
            for (i = 0; i < Gmap.Declare.stopMarkers.length; i++) {
                if (Gmap.Declare.stopMarkers[i].id == id) {
                    return Gmap.Declare.stopMarkers[i];
                }
            }
            return null;
        },

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

        clearMap: function (stopMarkers) {
            if (Gmap.Declare.suggestMarker) {
                Gmap.Declare.suggestMarker.setMap(null);
                Gmap.Declare.suggestMarker = null;
            }
            for (i = 0; i < stopMarkers.length; i++) {
                Gmap.Declare.stopMarkers[i].setMap(null);

                if (i == stopMarkers.length - 1) {
                    Gmap.Declare.stopMarkers = [];
                }
            }
            Gmap.Declare.directionsDisplay.setMap(Gmap.Declare.map);
        },

        drawMCP: function (markers, numbered) {
            Gmap.Process.prepairMap();
            Gmap.Process.clearMap(Gmap.Declare.stopMarkers);

            if (markers.length > 0) {
                Gmap.Declare.stopMarkers = [];
                // For each marker in list
                for (i = 0; i < markers.length; i++) {
                    var data = markers[i];
                    Gmap.Process.makeMarker(data, i, numbered);
                }

                Gmap.Declare.directionsDisplay.setMap(Gmap.Declare.map);
                //directionsDisplay.setOptions({ suppressMarkers: true });
            }
            Gmap.Process.showContextMenu(Gmap.Declare.map);
        },

        makeMarker: function (markerData, index, numbered) {
            if (markerData.lat && markerData.lng) {
                var myLatlng = new google.maps.LatLng(markerData.lat, markerData.lng);

                // pin color
                var pinColor = "FE6256";//"BDBDBD";//FE6256

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
                var marker = new google.maps.Marker({
                    id: markerData.id,
                    position: myLatlng,
                    map: Gmap.Declare.map,
                    title: markerData.title,
                    draggable:true,
                    icon: numbered? 
                        Ext.String.format('http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld={0}|{1}|000000', index + 1, pinColor)
                        : 'Images/OM24100/circle.png',
                });

                // Set info display of the marker
                (function (marker, markerData) {
                    google.maps.event.addListener(marker, "click", function (e) {
                        Gmap.Declare.infoWindow.setContent(markerData.description);
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
                        var record = HQ.store.findRecord(App.grdMCP.store, ["CustId"], [markerData.id]);
                        if (record) {
                            Process.showSuggestPopup(record, newLat, newLng);
                        }
                        marker.setPosition(myLatlng);
                    });

                    google.maps.event.addListener(Gmap.Declare.map, "click", function (event) {
                        Gmap.Declare.infoWindow.close();
                    });
                })(marker, markerData);

                Gmap.Declare.stopMarkers.push(marker);
            }
        },

        getGeobyAddress: function (record) {
            Gmap.Declare.geocoder = new google.maps.Geocoder();
            if (Gmap.Declare.suggestMarker) {
                Gmap.Declare.suggestMarker.setMap(null);
                Gmap.Declare.suggestMarker = null;
            }
            Gmap.Declare.geocoder.geocode({ 'address': record.data.Addr }, function (results, status) {
                if (status == google.maps.GeocoderStatus.OK) {
                    var location = results[0].geometry.location;
                    if (location.lat() != record.data.Lat || location.lng() != record.data.Lng) {
                        Gmap.Declare.map.setCenter(location);
                        Gmap.Declare.suggestMarker = new google.maps.Marker({
                            map: Gmap.Declare.map,
                            position: location,
                            icon: 'Images/OM24100/flaggreen.png',
                            animation: google.maps.Animation.DROP,
                            record: record
                        });

                        google.maps.event.addListener(Gmap.Declare.suggestMarker, "click", function (e) {
                            // Set animation of marker
                            if (Gmap.Declare.suggestMarker.getAnimation() != null) {
                                Gmap.Declare.suggestMarker.setAnimation(null);
                            } else {
                                Gmap.Declare.suggestMarker.setAnimation(google.maps.Animation.BOUNCE);
                                setTimeout(function () {
                                    Gmap.Declare.suggestMarker.setAnimation(null);
                                    Process.showSuggestPopup(record, location.lat(), location.lng(), Gmap.Declare.suggestMarker);
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
        },

        getAddressByGeo: function (store, idx) {
            var isContinued = false;
            var record = store.getAt(idx);
            if (record) {
                if (record.data.Lat && record.data.Lng
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
                                        //App.grdMCP.selModel.select(record);
                                        record.set("SuggestAddr", results[0].formatted_address);
                                    } else {
                                        console.log('No results found. CustID: ' + record.data.CustId);
                                    }
                                } else {
                                    console.log('Geocoder failed due to: ' + status + '. CustID: ' + record.data.CustId);
                                }

                                idx = idx + 1;
                                Gmap.Process.getAddressByGeo(store, idx);
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

                if (isContinued) {
                    idx = idx + 1;
                    Gmap.Process.getAddressByGeo(store, idx);
                }
            }
            else {
                App.frmMain.getEl().unmask();
            }
        },

        showContextMenu: function (objMap) {
            //	create the ContextMenuOptions object
            var contextMenuOptions = {};
            contextMenuOptions.classNames = { menu: 'context_menu', menuSeparator: 'context_menu_separator' };

            //	create an array of ContextMenuItem objects
            var menuItems = [];
            menuItems.push({ className: 'context_menu_item', eventName: 'set_location', label: HQ.common.getLang('AdjustLocation') });
            menuItems.push({ className: 'context_menu_item', eventName: 'set_coordinates', label: HQ.common.getLang('ResetLocation') });
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
            google.maps.event.addListener(objMap, 'rightclick', function (mouseEvent) {
                //if (polygon.containsLatLng(mouseEvent.latLng)) {
                contextMenu.show(mouseEvent.latLng);
                //}
            });

            //	listen for the ContextMenu 'menu_item_selected' event
            google.maps.event.addListener(contextMenu, 'menu_item_selected', function (latLng, eventName) {
                //	latLng is the position of the ContextMenu
                //	eventName is the eventName defined for the clicked ContextMenuItem in the ContextMenuOptions
                switch (eventName) {
                    case 'set_location':
                        var newLat = latLng.lat();
                        var newLng = latLng.lng();

                        if (App.slmMCP.getSelection().length) {
                            var selected = App.slmMCP.getSelection()[0];
                            Process.showSuggestPopup(selected, newLat, newLng);
                        }
                        else {
                            HQ.message.show(718, '', '');
                        }

                        contextMenu.hide();
                        break;
                    case 'set_coordinates':
                        Event.Form.btnResetGeo_click();
                        contextMenu.hide();
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
        }
    }
};

var Process = {
    resetGeo: function (item) {
        if (item == "yes") {
            if (App.pnlMCP.isValid()) {
                App.pnlMCP.submit({
                    waitMsg: 'Submiting...',
                    url: 'OM24100/ResetGeo',
                    timeout: 1800000,
                    params: {
                        lstSelCust: (function () {
                            values = []
                            App.grdMCP.store.each(function (record) {
                                if (record.data.Selected) {
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
                            App.grdMCP.store.each(function (record) {
                                listCustDone.forEach(function (custDone) {
                                    if (record.data.Selected && record.data.CustId == custDone.CustID) {
                                        record.data.Lat = 0;
                                        record.data.Lng = 0;
                                        record.data.Selected = false;
                                        record.data.tstamp = custDone.tstamp;

                                        var markerId = record.data.CustId;
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
                            App.grdMCP.store.commitChanges();
                            App.grdMCP.view.refresh();
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
            return combo.value +" - "+combo.rawValue;
        }
        else {
            return "";
        }
    },

    showSuggestPopup: function (record, newLat, newLng, suggestMarker) {
        //suggestMarker.position.A or F
        App.frmMain_Suggest.loadRecord(record);
        App.frmMain_Suggest.suggestMarker = suggestMarker;
        App.frmMain_Suggest.newLat = newLat;
        App.frmMain_Suggest.newLng = newLng;

        if (record.data.Lat && record.data.Lng) {
            App.imgMarkerOld_Suggest.setImageUrl(
                "https://maps.googleapis.com/maps/api/staticmap?zoom=16&size=300x200&maptype=roadmap&markers=color:red%7Clabel:B%7C"
                + record.data.Lat + ","
                + record.data.Lng);
        }
        else {
            App.imgMarkerOld_Suggest.setImageUrl("Images/OM24100/maps.jpg");
        }

        App.imgMarkerNew_Suggest.setImageUrl(
            "https://maps.googleapis.com/maps/api/staticmap?zoom=16&size=300x200&maptype=roadmap&markers=color:green%7Clabel:A%7C"
            + newLat + "," + newLng);
        App.winSuggest.show();
    },

    updateNewPostion: function () {
        var record = App.frmMain_Suggest.getRecord();
        var newLat = App.frmMain_Suggest.newLat;
        var newLng = App.frmMain_Suggest.newLng;
        var suggestMarker = App.frmMain_Suggest.suggestMarker;
        App.frmMain_Suggest.submit({
            waitMsg: 'Submiting...',
            url: 'OM24100/UpdateNewPosition',
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
                var rec = HQ.store.findRecord(App.grdMCP.store, ["CustId"], [record.data.CustId]);
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
                    var markerId = rec.data.CustId;
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
                            "id": rec.data.CustId,
                            "title": rec.data.CustId + ": " + rec.data.CustName,
                            "lat": rec.data.Lat,
                            "lng": rec.data.Lng,
                            "description":
                                '<div id="content">' +
                                    '<div id="siteNotice">' +
                                    '</div>' +
                                    '<h1 id="firstHeading" class="firstHeading">' +
                                        rec.data.CustName +
                                    '</h1>' +
                                    '<div id="bodyContent">' +
                                        '<p>' +
                                            rec.data.Addr +
                                        '</p>' +
                                    '</div>' +
                                '</div>'
                        }
                        Gmap.Process.makeMarker(markerData, rec.index);
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

    suggestAddress: function (item) {
        if (item == "yes") {
            App.frmMain.getEl().mask('FindingAddress', 'x-mask-loading');
            var idx = 0;
            Gmap.Process.getAddressByGeo(App.grdMCP.store, idx);
        }
    },

    updateAddress: function (item) {
        if (item == "yes") {
            if (App.pnlMCP.isValid()) {
                App.pnlMCP.submit({
                    waitMsg: 'Submiting...',
                    url: 'OM24100/UpdateAddress',
                    timeout: 1800000,
                    params: {
                        lstSelCust: (function () {
                            values = []
                            App.grdMCP.store.each(function (record) {
                                if (record.data.Selected) {
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
                            App.grdMCP.store.each(function (record) {
                                listCustDone.forEach(function (custDone) {
                                    if (record.data.Selected && record.data.CustId == custDone.CustId) {
                                        record.set('Addr', custDone.Addr1);
                                        record.set('SuggestAddr', "");
                                        record.set('Selected', false);
                                        record.set('ctstamp', custDone.tstamp);
                                        return false;
                                    }
                                });
                            });
                            //App.grdMCP.store.commitChanges();
                            //App.grdMCP.view.refresh();
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
    }
};