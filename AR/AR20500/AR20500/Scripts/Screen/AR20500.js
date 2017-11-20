var _Change = false;
var keys = ['ID'];
var _firstLoad = true;
var loadSourceCombo = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    if (HQ.isShowCustHT) HQ.grid.show(App.grdCust, ['CustHT'])
    if (HQ.IsShowERPCust) HQ.grid.show(App.grdCust, ['ERPCustID'])

    App.stoAR20500_pdWeekofVisitAll.load(function () {
        App.cboTerritory.getStore().load(function () {
            App.cboCpnyID.getStore().load(function () {
                App.cboSlsperId.getStore().load(function () {
                    App.cboStatus.getStore().load(function () {
                        App.cboHandle.getStore().load(function () {
                            HQ.common.showBusy(false, HQ.common.getLang("loadingData"));
                            if (_firstLoad) {
                                App.cboStatus.setValue("H");
                                App.cboHandle.setValue("N");
                                _firstLoad = false;
                                App.FromDate.setValue(HQ.bussinessDate);
                                App.ToDate.setValue(HQ.bussinessDate);
                            }
                        })
                    })
                })
            })
        })
    });
};

var cboTerritory_Change = function (sender, e) {
    if (_Change == true) {
        HQ.message.show(20150303, '', 'refresh');
    } else {
        if (sender.valueModels != null && (!App.cboCpnyID.store.loading)) {
            App.cboCpnyID.store.reload();
        }
    }
};

var cboCpnyID_Change = function (sender, e) {
    if (_Change == true) {
        HQ.message.show(20150303, '', 'refresh');
    } else {
        if (sender.valueModels != null && (!App.cboSlsperId.store.loading || !App.cboColSalesRouteID.store.loading)) {
            App.grdCust.store.removeAll();
            App.cboSlsperId.store.reload();
            App.cboColSalesRouteID.store.reload();
        }
        //App.grdCust.removeAll();
    }
};

var cboCpnyID_Select = function (sender, e) {
    if (_Change == true) {
        HQ.message.show(20150303, '', 'refresh');
    } else {
        if (sender.valueModels != null && (!App.cboSlsperId.store.loading || !App.cboColSalesRouteID.store.loading)) {
            App.grdCust.store.removeAll();
            App.cboSlsperId.store.reload();
            App.cboColSalesRouteID.store.reload();
        }
        //App.grdCust.removeAll();
    }
};

var cboStatus_Change = function (value) {
    if (_Change == true) {
        HQ.message.show(20150303, '', 'refresh');
    } else {
        App.grdCust.store.removeAll();
        App.cboHandle.store.reload();
        //App.grdCust.removeAll();
    }
};


var btnLoad_Click = function () {
    if (App.frmMain.isValid()) {
        App.stoCust.reload();
    }
};

var ColCheck_Header_Change = function (value) {
    if (value) {
        App.stoCust.suspendEvents();
        App.stoCust.each(function (item) {
            if (item.data.Status == App.cboStatus.getValue())
                item.set("ColCheck", value.checked);
        });
        App.stoCust.resumeEvents();
        App.grdCust.view.refresh();
    }
};

var btnProcess_Click = function () {
    var count = 0;
    for (var i = 0; i < App.grdCust.store.getCount() ; i++) {
        var data = App.grdCust.store.data.items[i].data;
        if (data.ColCheck) {
            count++;
        }
    }
    if (App.cboHandle.getValue() && count > 0) {
        if (App.cboHandle.getValue() == 'A') {
            var rowerror = '';
            var isnullclass = '';
            var isnullpriceclass = '';
            if (HQ.IsRequireRefCustID) {
                for (var i = 0; i < App.grdCust.store.getCount() ; i++) {
                    var data = App.grdCust.store.data.items[i].data;
                    if (data.ColCheck) {
                        if (Ext.isEmpty(data.ERPCustID)) {
                            HQ.message.show(1000, HQ.common.getLang('ERPCustID'), '');
                            rowerror += i + 1 + ',';
                            break;
                        }
                    }
                }
            }
            if (rowerror != '') {
                return;
            }
            var minDate = HQ.bussinessDate;
            for (var i = 0; i < App.grdCust.store.getCount() ; i++) {
                var data = App.grdCust.store.data.items[i].data;
                if (data.ColCheck) {
                    if (data.MinMCPDate < minDate) {
                        minDate = data.MinMCPDate;
                    }
                    //if (!isValidSel(data)) rowerror += i + 1 + ',';
                    //if(data.ClassId==''||data.ClassId==null)
                    //    isnullclass += i + 1 + ',';
                    //if (data.PriceClass == '' || data.PriceClass == null)
                    //    isnullpriceclass += i + 1 + ',';
                }
            }
            //if (isnullclass != '') {
            //    HQ.message.show(1000, HQ.common.getLang('ClassId'), '');
            //    return;
            //}
            //if (isnullpriceclass != '') {
            //    HQ.message.show(1000, HQ.common.getLang('PriceClass'), '');
            //    return;
            //}
            //if (rowerror != '') {
            //    HQ.message.show(201302071, rowerror, '');
            //    return;
            //}
          
            App.dteFromDate.setValue(minDate);
            App.dteToDate.setValue(HQ.EndDateYear);
            App.dteFromDate.setMinValue(minDate);
            App.dteFromDate.validate();
            App.dteToDate.validate();
            App.winProcess.show();
        }
        else {
            var d = Ext.Date.parse("01/01/1990", "m/d/Y");
            if (App.FromDate.getValue() < d || App.ToDate.getValue() < d) return;
            var flat = false;
            App.stoCust.data.each(function (item) {
                if (item.data.ColCheck) {
                    flat = true;
                    return false;
                }
            });
            if (flat && !Ext.isEmpty(App.cboHandle.getValue()) && App.cboHandle.getValue() != 'N') {
                App.frmMain.submit({
                    clientValidation: false,
                    waitMsg: HQ.common.getLang("Handle"),
                    method: 'POST',
                    url: 'AR20500/Process',
                    timeout: 180000,
                    params: {
                        lstCust: Ext.encode(App.grdCust.store.getRecordsValues()),
                        fromDate: HQ.bussinessDate,
                        toDate: HQ.bussinessDate,
                        askApprove: 0
                    },
                    success: function (msg, data) {
                        HQ.message.show(201405071);
                        App.stoCust.reload();
                    },
                    failure: function (msg, data) {
                        HQ.message.process(msg, data, true);
                    }
                });
            }
        }
    }
};

var btnOKMCP_Click = function () {
    if (App.dteFromDate.isValid() && App.dteToDate.isValid()) {
        //if (App.dteFromDate.lastValue.getFullYear() != App.dteToDate.lastValue.getFullYear()) {
        //    HQ.message.show(201506111);
        //    return;
        //}
        App.winProcess.hide();
        var d = Ext.Date.parse("01/01/1990", "m/d/Y");
        if (App.FromDate.getValue() < d || App.ToDate.getValue() < d) return;
        var flat = false;
        App.stoCust.data.each(function (item) {
            if (item.data.ColCheck) {
                flat = true;
                return false;
            }
        });
        if (flat && !Ext.isEmpty(App.cboHandle.getValue()) && App.cboHandle.getValue() != 'N') {
            App.frmMain.submit({
                clientValidation: false,
                waitMsg: HQ.common.getLang("Handle"),
                method: 'POST',
                url: 'AR20500/Process',
                timeout: 180000,
                params: {
                    lstCust: Ext.encode(App.grdCust.store.getRecordsValues()),
                    fromDate: App.dteFromDate.getValue(),
                    toDate: App.dteToDate.getValue(),
                    askApprove: 0

                },
                success: function (msg, data) {
                    HQ.message.show(201405071);
                    App.stoCust.reload();
                },
                failure: function (msg, data) {
                    HQ.message.process(msg, data, true);
                    //App.stoCust.reload();
                }
            });
        }
    }
};

var btnExit_Click = function () {
    App.winProcess.hide();
};

var dteFromDate_change = function (dtp, newValue, oldValue, eOpts) {
    App.dteToDate.setMinValue(newValue);
    App.dteToDate.validate();
};

var dteToDate_change = function (dtp, newValue, oldValue, eOpts) {
    App.dteFromDate.setMaxValue(newValue);
    App.dteFromDate.validate();
};

var askApprove = function (item) {
    if (item == 'yes') {
        App.frmMain.submit({
            clientValidation: false,
            waitMsg: HQ.common.getLang("Handle"),
            method: 'POST',
            url: 'AR20500/Process',
            timeout: 180000,
            params: {
                lstCust: Ext.encode(App.grdCust.store.getRecordsValues()),
                fromDate: App.dteFromDate.getValue(),
                toDate: App.dteToDate.getValue(),
                askApprove: 1
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                App.stoCust.reload();
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
                App.stoCust.reload();
            }
        });
    }
};

var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdCust);
            break;
        case "prev":
            HQ.grid.prev(App.grdCust);
            break;
        case "next":
            HQ.grid.next(App.grdCust);
            break;
        case "last":
            HQ.grid.last(App.grdCust);
            break;
        case "refresh":
            if (_Change) {
                HQ.message.show(20150303, '', 'refresh');
            } else {
                App.stoCust.reload();
            }
            break;
        case "new":
            break;
        case "delete":
            break;
        case "save":
            break;
        case "print":
            break;
        case "close":
            break;
    }

};

var grdCust_BeforeEdit = function (item, e) {
    if (!HQ.isShowCustHT && e.field == 'CustHT') return false;
    if (!HQ.IsShowERPCust && e.field == 'ERPCustID') return false;

    if (e.field != 'ColCheck' && App.cboStatus.getValue() != 'H') return false;
    if (e.field == 'WeekofVisit') {
        App.cboColWeekofVisit.getStore().reload();
    }
    if (['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'].indexOf(e.field) > -1) {
        var objCheck = HQ.store.findRecord(App.stoAR20500_pdWeekofVisitAll, ['SlsFreq'], [e.record.data.SlsFreq]);
        if (objCheck) {
            return objCheck.data.IsEdit;
        }
    }
};

var grdCust_Edit = function (item, e, oldvalue, newvalue) {
    var det = e.record.data;
    if (e.field == 'WeekofVisit') {
        var record = HQ.store.findRecord(App.cboColWeekofVisit.getStore(), ["Code"], [e.value]);
        if (record) {
            e.record.set("Mon", record.data.Mon);
            e.record.set("Tue", record.data.Tue);
            e.record.set("Wed", record.data.Wed);
            e.record.set("Thu", record.data.Thu);
            e.record.set("Fri", record.data.Fri);
            e.record.set("Sat", record.data.Sat);
            e.record.set("Sun", record.data.Sun);
        }
    }
    if (e.field == 'SlsFreq' && e.value != e.originalValue) {
        if (e.value != 'F1' && e.value != 'F2') {
            e.record.set('WeekofVisit', 'NA');
        }
        else e.record.set('WeekofVisit', '');
        e.record.set("Mon", false);
        e.record.set("Tue", false);
        e.record.set("Wed", false);
        e.record.set("Thu", false);
        e.record.set("Fri", false);
        e.record.set("Sat", false);
        e.record.set("Sun", false);
    }
    if (e.field == 'Phone') {
        if (isNumeric(e.value) == true) {
            e.record.set("Phone", oldvalue.fn.arguments[1].originalValue);
            HQ.message.show(20171118, '');
        }
    }
    //HQ.grid.checkInsertKey(App.grdCust, e, keys);
};

function isNumeric(n) {
    var regex = /^([0-9()-.;#/ ])*$/;
    return !HQ.util.passNull(n.toString()).match(regex);
}

var grdCust_Reject = function (record) {
    HQ.grid.checkReject(record, App.grdCust);
    stoChanged(App.stoCust);
};

var stoChanged = function (sto) {
    _Change = HQ.store.isChange(sto);
    HQ.common.changeData(_Change, 'AR20500');
    App.cboStatus.setReadOnly(_Change);
    App.btnLoad.setDisabled(_Change);
    App.cboCpnyID.setReadOnly(_Change);
    App.cboSlsperId.setReadOnly(_Change);
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true);
};

var isValidSel = function (data) {
    if (Ext.isEmpty(data.SalesRouteID) || Ext.isEmpty(data.SlsFreq) || Ext.isEmpty(data.WeekofVisit))
        return false;
    var iVisit = 0;
    if (data.Mon) {
        iVisit = iVisit + 1;
    }
    if (data.Tue) {
        iVisit = iVisit + 1;
    }
    if (data.Wed) {
        iVisit = iVisit + 1;
    }
    if (data.Thu) {
        iVisit = iVisit + 1;
    }
    if (data.Fri) {
        iVisit = iVisit + 1;
    }
    if (data.Sat) {
        iVisit = iVisit + 1;
    }
    if (data.Sun) {
        iVisit = iVisit + 1;
    }
    if (data.SlsFreq) {
        switch (data.SlsFreq) {
            case "F1":
            case "F2":
            case "F4":
            case "F4A":
                if (iVisit != 1) {
                    return false;
                }
                break;
            case "F8":
            case "F8A":
                if (iVisit != 2) {
                    return false;
                }
                break;
            case "F12":
                if (iVisit != 3) {
                    return false;
                }
                break;
            case "F16":
                if (iVisit != 4) {
                    return false;
                }
                break;
            case "F20":
                if (iVisit != 5) {
                    return false;
                }
                break;
            case "F24":
                if (iVisit != 6) {
                    return false;
                }
                break;
            case "A":
                if (iVisit == 0) {
                    return false;
                }
                break;
        }
    }
    return true;
};

//load lai trang, kiem tra neu la load lan dau thi them dong moi vao
var stoLoad = function (sto, records, successful, eOpts) {
    _Change = HQ.store.isChange(sto);
    HQ.common.changeData(HQ.isChange, 'AR20500');

    HQ.common.showBusy(false);
    stoChanged(App.stoCust);

    var markers = [];
    records.forEach(function (record) {
        var marker = {
            "index": record.index,
            "id": record.index + 1,
            "title": record.data.CustID + ": " + record.data.OutletName,
            "lat": record.data.Lat,
            "lng": record.data.Lng,
            "ImageFileName": record.data.ImageFileName,
            "description":
                '<div id="content">' +
                    '<div id="siteNotice">' +
                    '</div>' +
                    '<h1 id="firstHeading" class="firstHeading">' +
                        record.data.OutletName +
                    '</h1>' +
                    '<div id="bodyContent">' +
                        '<p>' +
                            record.data.Phone +
                        '</p>' +
                    '</div>' +
                '</div>'
        }
        markers.push(marker);
    });
    Gmap.Process.drawMCP(markers, false);
};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        _Change = false;
        App.stoCust.reload();
    }
};

var renderWeekofVisit = function (value) {
    var obj = App.stoAR20500_pdWeekofVisitAll.findRecord("Code", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};

var renderSalesRouteID = function (value) {
    var obj = App.cboColSalesRouteID.store.findRecord("Code", value);
    if (obj) {
        return obj.data.Descr;
    }
    return value;
};

var slmCust_Select = function (rowModel, record, index, eOpts) {
    if (record[0]) {
        if (record[0].data.Lat && record[0].data.Lng) {
            Gmap.Process.navMapCenterByLocation(record[0].data.Lat, record[0].data.Lng, record.index + 1);
            displayImage(App.imgImages, record[0].data.ImageFileName);// get image theo binary
        }
    }
};

var displayImage = function (imgControl, fileName) {
    Ext.Ajax.request({
        url: 'AR20500/ImageToBin',
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
};

var pnlGridMCL_viewGetRowClass = function (record) {
    if (record.data.Color != '0')
        return 'hightlight-row'
};

///////////////////////////////////////////////////////////////////////

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
                                App.slmCust.select(data.index);
                                //displayImage(App.imgImages, data.ImageFileName);
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