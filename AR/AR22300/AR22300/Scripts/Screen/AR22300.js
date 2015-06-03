var Declare = {

};

var Event = {
    Store: {
        stoMCP_load: function (store, records, successful, eOpts) {
            if (successful) {
                var markers = [];
                records.forEach(function (record) {
                    var marker = {
                        "id": record.index + 1,
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
                Gmap.Process.drawMCP(markers, false);
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
            if (App.grdMCP.selModel.getSelection().length) {
                HQ.message.show(2015052201, '', 'Process.resetGeo');
            }
            else {
                HQ.message.show(20412, HQ.common.getLang('Customer'), '');
            }
        }
    },

    Grid: {
        chkMcpAll_change: function (chk, newValue, oldValue, eOpts) {
            for (var i = 0; i < App.grdMCP.store.getCount() ; i++) {
                App.grdMCP.store.getAt(i).set("Selected", chk.value);
            }
            //App.grdMCP.store.each(function (record) {
            //    record.set("Selected", chk.value);
            //});
        },

        slmMCP_Select: function (rowModel, record, index, eOpts) {
            if (record && record.data.Lat && record.data.Lng){
                Gmap.Process.navMapCenterByLocation(record.data.Lat, record.data.Lng, record.index + 1);
            }
        },

        grdMCP_viewGetRowClass: function (record, rowIndex, rowParams, store) {
            if (!record.data.Lat || !record.data.Lng) {
                return "row-FF0000";
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
        drawingManager: {}
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
            for (i = 0; i < stopMarkers.length; i++) {
                Gmap.Declare.stopMarkers[i].setMap(null);

                if (i == stopMarkers.length - 1) {
                    Gmap.Declare.stopMarkers = [];
                }
            }
            Gmap.Declare.directionsDisplay.setMap(Gmap.Declare.map);
        },

        drawMCP: function (markers) {
            Gmap.Process.prepairMap();

            if (markers.length > 0) {
                Gmap.Declare.stopMarkers = [];
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
                            Gmap.Declare.map = new google.maps.Map(Gmap.Declare.map_canvas, myOptions);
                        }

                        // Make the marker at each location
                        var markerLabel = data.visitSort;
                        var marker = new google.maps.Marker({
                            id: data.id,
                            position: myLatlng,
                            map: Gmap.Declare.map,
                            title: data.title,
                            icon: Ext.String.format('http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld={0}|{1}|000000', i + 1, pinColor)
                        });

                        // Set info display of the marker
                        (function (marker, data) {
                            google.maps.event.addListener(marker, "click", function (e) {
                                infoWindow.setContent(data.description);
                                infoWindow.open(Gmap.Declare.map, marker);

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

                        Gmap.Declare.stopMarkers.push(marker);
                    }
                }

                Gmap.Declare.directionsDisplay.setMap(Gmap.Declare.map);
                //directionsDisplay.setOptions({ suppressMarkers: true });
            }
            else {
                Gmap.Process.clearMap(Gmap.Declare.stopMarkers);
            }
        },
    }
};

var Process = {
    resetGeo: function (item) {
        if (item == "yes") {
            if (App.pnlMCP.isValid()) {
                App.pnlMCP.submit({
                    waitMsg: 'Submiting...',
                    url: 'AR22300/ResetGeo',
                    params: {
                        lstSelCust: (function () {
                            values = []
                            App.grdMCP.selModel.getSelection().forEach(function (record) {
                                values.push(record.data);
                            });
                            return Ext.encode(values);
                        })()
                    },
                    success: function (action, data) {
                        if (data.result.msgcode) {
                            HQ.message.show(data.result.msgCode, (data.result.msgParam ? data.result.msgParam : ''), '');
                        }
                        App.grdMCP.selModel.getSelection().forEach(function (record) {
                            record.set("Lat", 0);
                            record.set("Lng", 0);

                            var markerId = record.index + 1;
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
                        });
                        App.grdMCP.store.commitChanges();
                        App.grdMCP.selModel.deselectAll();
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