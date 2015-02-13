var pointsArray = [];
// JS Code for Index View
var Index = {
    menuClick: function (cmd) {
        switch (cmd) {
            case "first":
                {
                    App.SelectionModelPOS_ActualVisit.select(0);
                }
                break;

            case "prev":
                {
                    App.SelectionModelPOS_ActualVisit.selectPrevious();
                }
                break;

            case "next":
                {
                    App.SelectionModelPOS_ActualVisit.selectNext();
                }
                break;

            case "last":
                {
                    App.SelectionModelPOS_ActualVisit.select(App.grdActualVisit.store.getCount() - 1);
                }
                break;

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
                    else if(!App.pnlActualVisit.hidden){
                        if (App.pnlActualVisit.isValid()) {
                            if (App.radSalesmanAll.value) {
                                App.grdAllCurrentSalesman.store.reload();
                            }
                            else {
                                App.grdVisitCustomerActual.store.reload();
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
                    if (parent.App.tabOM30400) {
                        parent.App.tabOM30400.close();
                    }
                    else {
                        parent.close();
                    }
                }
                break;
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
        Index.menuClick("refresh");
    },

    stoVisitCustomerPlan_load: function (store, records, successful, eOpts) {
        if (successful) {
            var markers = [];
            records.forEach(function (record) {
                var marker = {
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
            PosGmap.drawRoutes(markers, false);
        }
    },

    selectionModelVisitCustomerPlan_Select: function (rowModel, record, index, eOpts) {
        if (record) {
            PosGmap.navMapCenterByLocation(record.data.Lat, record.data.Lng);
        }
    },

    // tab MCL

    cboAreaMCL_change: function (cbo, newValue, oldValue, eOpts) {
        App.cboProvinceMCL.store.reload();
        App.cboDistributorMCL.store.reload();
    },

    cboDistributorMCL_change: function (cbo, newValue, oldValue, eOpts) {
        //App.cboSalesManPlan.store.reload();
    },

    btnLoadDataMCL_click: function (btn, e, eOpts) {
        Index.menuClick("refresh");
    },

    onComboBoxSelect: function (cbo) {
        App.grdMCL.store.pageSize = parseInt(cbo.getValue(), 10);
        App.grdMCL.store.reload();
    },

    stoMCL_load: function (store, records, successful, eOpts) {
        if (successful) {
            var markers = [];

            store.each(function (record) {
                var marker = {
                    "title": record.data.CustId + ": " + record.data.CustName,
                    "lat": record.data.Lat,
                    "lng": record.data.Lng,
                    "stop": true,
                    "description":
                        '<div id="content">' +
                            '<div id="siteNotice">' +
                            '</div>' +
                            '<h1 id="firstHeading" class="firstHeading">' +
                                record.data.CustName +
                            '</h1>' +
                            '<div id="bodyContent">' +
                                '<p>' +
                                    record.data.Addr1 +
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

            App.grdVisitCustomerActual.hide();
            App.grdAllCurrentSalesman.show();
        }
        else {
            App.cboSalesManActual.enable();
            App.btnGetCurrentLocation.enable();

            App.grdVisitCustomerActual.show();
            App.grdAllCurrentSalesman.hide();
        }
    },

    btnLoadDataActual_click: function (btn, e, eOpts) {
        Index.menuClick("refresh");
    },

    grdVisitCustomerActual_viewGetRowClass: function (record) {
        if (record.data.Type == "IO") {
            return "ci-row"
        }
        else if (record.data.Type == "OO") {
            return "co-row";
        }
    },

    btnGetCurrentLocation_click: function (btn, e, eOpts) {
        var store = App.grdVisitCustomerActual.store;
        if (store.getCount() > 0) {
            var lastRecord = store.getAt(store.getCount() - 1);
            PosGmap.navMapCenterByLocation(lastRecord.data.Lat, lastRecord.data.Lng);

            App.SelectionModelVisitCustomerActual.select(store.getCount() - 1);
        }
    },

    stoVisitCustomerActual_load: function (store, records, successful, eOpts) {
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
    },

    stoAllCurrentSalesman_load: function (store, records, successful, eOpts) {
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
        Index.menuClick("refresh");
    },

    stoCustHistory_load: function (store, records, successful, eOpts) {
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
        if (clickCol)
        {
            if (clickCol.dataIndex === "CheckOut") {
                PosGmap.navMapCenterByLocation(record.data.CoLat, record.data.CoLng);
            }
            else if(clickCol.dataIndex === "CheckIn"){
                PosGmap.navMapCenterByLocation(record.data.CiLat, record.data.CiLng);
            }
        }
    }
};

// JS Code for POS Gmap
var PosGmap = {
    map_canvas: {},
    map: {},
    directionsService: {},
    directionsDisplay: {},
    directionsDisplays:[],
    infoWindow: {},
    stopMarkers: [],

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
                var pinColor = "FE6256";
                if (data.type) {
                    if (data.type == "IO") { // Check in
                        pinColor = "CCFF33";
                    }
                    else if(data.type == "OO"){ // Check out
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
                        icon: 'Images/OM30400/circle_green.png'
                        //animation: google.maps.Animation.DROP
                    });
                }
                else {
                    var marker = new google.maps.Marker({
                        position: myLatlng,
                        map: map,
                        title: data.title,
                        icon: Ext.String.format('http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld={0}|{1}|000000', i + 1, pinColor)
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
        else
        {
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
                    setTimeout(function(){
                        PosGmap.requestForWaysRoute(lat_lngCols, idx);
                    },300);
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

