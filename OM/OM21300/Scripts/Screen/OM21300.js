var map;
var markerCluster;
var cluster = false;
var level = 0;
var locationMarkers = [];
var custTopMarkers = [];
var markers = [];
var cityBorder = [];
var districtBorder = [];
var dsr = [];
var dsmColor = [];
var dsrColor = [];
var shapes = [];
var districtData = [];
var cityCust = [];
if (typeof variable === 'undefined') {
    cityMap = [];
}


var frmMain_BoxReady = function () {
    HQ.common.showBusy(true, HQ.waitMsg);

    HQ.numSource = 0;
    HQ.maxSource = 9;

    App.txtFromDate.setValue(HQ.businessDate);
    App.txtFromAmt.setValue(0);
    App.txtToAmt.setValue(0);
    //App.btnCluster.hide();
    //App.btnDetail.show();
    App.direct.OM21300_GetTreeData({
        success: function (result) {
            checkSource();
        },
        failure: function (errorMsg) {
        },
        timeout: 360000
    });

    App.cboDay.store.addListener('load', stoData_Load);
    App.cboBrand.store.addListener('load', stoData_Load);
    App.cboVisitType.store.addListener('load', stoData_Load);
    App.stoDistrict.addListener('load', stoData_Load);
    App.stoFillType.addListener('load', stoData_Load);
    App.stoCustClass.addListener('load', stoData_Load);
    App.stoCityShape.addListener('load', stoData_Load);
    App.stoLocationType.addListener('load', stoData_Load);

    App.cboLocationType.addListener('change', cboLocationType_Change);

    App.cboLocationType.store.load();
    App.stoVisitType.load();
    App.stoCustClass.load();
    App.stoFillType.load();
    App.cboDay.store.load();
    App.cboBrand.store.load();
    App.stoDistrict.load();
    App.stoCityShape.load();


    App.gpgCust.items.getByKey("refresh").hide();

    google.maps.Polygon.prototype.getBounds = function () {
        var bounds = new google.maps.LatLngBounds();
        this.getPath().forEach(function (element, index) { bounds.extend(element); });
        return bounds;
    };
    App.txtFromAmt.addListener('blur', function () {
        if (!App.txtFromAmt.getValue()) {
            App.txtFromAmt.setValue(0);
        }
        if (App.txtToAmt.getValue() < App.txtFromAmt.getValue()) {
            App.txtToAmt.setValue(App.txtFromAmt.getValue());
        }
    });
    App.txtToAmt.addListener('blur', function () {
        if (!App.txtToAmt.getValue()) {
            App.txtToAmt.setValue(0);
        }
        if (App.txtToAmt.getValue() < App.txtFromAmt.getValue()) {
            App.txtToAmt.setValue(App.txtFromAmt.getValue());
        }
    });
    App.txtFromAmt.selectOnFocus = false;
    App.txtToAmt.selectOnFocus = false;

    if (window.mobilecheck()) {
        App.winFilter.add(App.pnlFilter);
        App.pnlFilter.header.hide();

        App.btnPopup.show();
        App.cboBrand.setEditable(false);
        App.cboFillType.setEditable(false);
        App.cboVisitType.setEditable(false);
        App.btnClose.show();
        App.cboDay.setEditable(false);


        overridePickerClick(App.cboBrand);

        overridePickerClick(App.cboDay);
        overridePickerClick(App.cboCustClass);


        App.pnlFilter.addBodyCls('filter-form');
        App.tnlMap.closeTab(App.pnlCust);
        App.btnExport.hide();
        App.winCust.setHeight(300);
        App.pnlCustInfo.setHeight(265);
        App.pnlCustChart.setHeight(265);
        App.tnlCust.add(App.pnlCustInfo);
        App.tnlCust.add(App.pnlCustChart);
        App.chartCust.clearListeners();
        App.tnlCust.show();

    }
    App.winLegend.hide();

    App.btnPopup.addListener('click', function () {
        App.winCust.hide();
        App.winFilter.show();
        App.winFilter.setWidth(320);
        App.winFilter.setPosition(document.documentElement.clientWidth - 320, 0);
    });


}
var overridePickerClick = function (combo) {
    var picker = combo.getPicker();
    picker.onItemClick = function (record, item, index, e, eOpts) {

        picker.events["itemclick"].listeners.length = 0;
        var tmp = [];
        if (item.getAttribute('class').indexOf('hq-mcombo-item-checked') == -1) {
            item.setAttribute('class', 'x-boundlist-item hq-mcombo-item-unchecked hq-mcombo-item-checked');
            combo.displayTplData.push(record.data);
        } else {
            item.setAttribute('class', 'x-boundlist-item hq-mcombo-item-unchecked');
            combo.displayTplData.splice(combo.displayTplData.indexOf(record.data), 1);

        }
        combo.setRawValue(combo.getDisplayValue());
    };
    picker.addListener('hide', function () {
        var tmp = [];
        for (i = 0; i < combo.displayTplData.length; i++) {
            tmp.push(combo.displayTplData[i][combo.valueField]);
        }
        combo.setValue(tmp);
    });
}
var stoData_Load = function () {
    checkSource();
}
var checkSource = function () {
    HQ.numSource++;
    if (HQ.numSource == HQ.maxSource) {
        if (HQ.role == 'DSR') {
            App.lblTree.hide();
            App.pnlTree.hide();
        }

        App.cboFillType.setValue(App.stoFillType.data.items[0].data.Code);

        var dataDay = [];
        App.cboDay.store.data.each(function (item) {
            dataDay.push(item.data.Code);
        });
        App.cboDay.setValue(dataDay);


        App.cboVisitType.setValue('ALL');

        var dataCustClass = [];

        App.cboCustClass.store.data.each(function (item) {
            dataCustClass.push(item.data.ClassId);
        });

        App.cboCustClass.setValue(dataCustClass);

        var brand = [];

        App.cboBrand.store.data.each(function (item) {
            brand.push(item.data.Code);
        });

        App.cboBrand.setValue(brand);


        var rootNode = App.treeDSR.getRootNode();
        for (var i = 0; i < rootNode.childNodes.length; i++) {
            var firstCpny = rootNode.childNodes[i];

            checkNode(true, firstCpny);
        }
        HQ.common.showBusy(false);
    }
}
var pnlMap_AfterRender = function () {
    map = new google.maps.Map(document.getElementById('wrapMap'), {
        center: { lat: 16.204529937414772, lng: 107.86158541384881 },
        zoom: 6,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    });

    var contextMenu = google.maps.event.addListener(
        map,
        "rightclick",
        function (event) {
            if (window.mobilecheck()) {
                App.menuMap.showAt(event.pixel.x, event.pixel.y);
            } else {
                App.menuMap.showAt(event.pixel.x + App.pnlFilter.getWidth(), event.pixel.y);
            }

        }
    );

    var contextMenu2 = google.maps.event.addListener(
        map,
        "click",
        function (event) {
            App.winCust.hide();
        }
    );

    google.maps.event.addListener(
            map,
            "longpress",
            function (event) {
                if (window.mobilecheck()) {
                    App.menuMap.showAt(event.pixel.x, event.pixel.y);
                } else {
                    App.menuMap.showAt(event.pixel.x + App.pnlFilter.getWidth(), event.pixel.y);
                }
            }
        );

    new LongPress(map, 1000, 'map');
}

var pnlFilter_Collapse = function () {
    google.maps.event.trigger(map, "resize");
}

var printMaps = function (type) {
    var bounds = new google.maps.LatLngBounds();
    if (HQ.role != 'DSM' && HQ.role != 'DSR') {
        for (var i = 0; i < cityBorder.length; i++) {
            cityBorder[i].getPath().forEach(function (element, index) { bounds.extend(element); });
        }

    } else {
        for (var i = 0; i < districtBorder.length; i++) {
            if (districtBorder[i].data.City == cityB[0].data.City) {
                districtBorder[i].getPath().forEach(function (element, index) { bounds.extend(element); });
            }
        }

    }

    map.fitBounds(bounds);

    var recentZoom = map.getZoom();
    var recentCenter = map.getCenter();
    google.maps.event.trigger(map, "resize");
    var html = $("html");
    var body = $('body'),
        mapContainer = $('#wrapMap'),
        mapContainerParent = mapContainer.parent(),
        printContainer = $('<div>');

    body.prepend(printContainer);

    if (type == 'A0') {
        mapContainer.css({ "height": "33.1in", "width": "46.8in" });
        html.css({ "height": "32.6in", "width": "46.8in" });
    } else if (type == 'A1') {
        mapContainer.css({ "height": "23.4in", "width": "33.1in" });
        html.css({ "height": "23.35in", "width": "33.1in" });
    } else if (type == 'A2') {
        mapContainer.css({ "height": "16.5in", "width": "23.4in" });
        html.css({ "height": "16.45in", "width": "23.4in" });
    } else if (type == 'A3') {
        mapContainer.css({ "height": "11.7in", "width": "16.5in" });
        html.css({ "height": "11.65in", "width": "16.5in" });
    } else if (type == 'A4') {
        mapContainer.css({ "height": "8.3in", "width": "11.7in" });
        html.css({ "height": "8.25in", "width": "11.7in" });
    }

    printContainer
        .addClass('print-container')
        .css('position', 'relative')
        .height(mapContainer.height())
        .append(mapContainer)
        .append('<style type="text/css" media="print">@page {' +
                    'size: ' + type + ' landscape;' +
                    'margin: 0mm 0mm 0mm 0mm;' +
                    'counter-increment: page;' +
                    '@bottom-right {' +
                    'content: "Page " counter(page) " of " counter(pages);' +
                  '}' +
        '}</style>');

    var content = body.children()
        .not('script')
        .not(printContainer)
        .detach();

    $('<div id="mask" style = "position:fixed;width:100%;height:100%;background: rgba(45, 45, 45, 0.27);top:0;left:0;line-height:100px;text-align:center;"><span style="font-size:24px;">Đang xử lý ...</span></div>').appendTo('body');
    google.maps.event.trigger(map, "resize");

    google.maps.event.addListenerOnce(map, 'idle', function () {
        google.maps.event.addListenerOnce(map, 'idle', function () {
            google.maps.event.addListenerOnce(map, 'tilesloaded', function () {
                setTimeout(function () {
                    $('#mask').remove();
                    window.print();
                    html.css({ "height": "", "width": "" });
                    mapContainer.css({ "height": "100%", "width": "100%" });

                    body.prepend(content);
                    mapContainerParent.prepend(mapContainer);
                    printContainer.remove();
                    google.maps.event.trigger(map, "resize");
                    map.setCenter(recentCenter);
                    map.setZoom(recentZoom);
                }, 1000);
            });
            map.setZoom(recentZoom);
            map.fitBounds(bounds);
        });        
        map.setCenter(recentCenter);
        map.setZoom(recentZoom - 2);
    });
};
var tree_CheckChange = function (item, checked, options) {
    checkNode(checked, item);
    if (checked == true) {
        checkParentNode(item);
    } else {
        unCheckParentNode(item);
    }
    if (checked == true) {
        //var nodeBranch = null;
        //var nodeCpny = null;
        //if (item.data.Type == 'C') {
        //    nodeBranch = item.parentNode.parentNode;
        //} else if (item.data.Type == 'S') {
        //    nodeBranch = item.parentNode.parentNode;
        //} else if (item.data.Type == 'M') {
        //    nodeBranch = item.parentNode;
        //} else if (item.data.Type == 'B') {
        //    nodeBranch = item;
        //}
        //var nodeCpny = nodeBranch.parentNode;
        //if (HQ.role == 'DSM') {
        //    for (var i = 0; i < nodeCpny.childNodes.length; i++) {
        //        if (nodeCpny.childNodes[i].data.Data != nodeBranch.data.Data) {
        //            checkNode(false, nodeCpny.childNodes[i]);
        //        }
        //    }
        //}
    }

    //HQ.common.showBusy(true, HQ.waitMsg);

}


var checkNode = function (checked, node) {
    if (node.childNodes.length > 0) {
        for (var i = 0; i < node.childNodes.length; i++) {
            checkNode(checked, node.childNodes[i]);
        }
    }

    if (node.data.Type == 'S') {
        var index = dsr.indexOf(node.data.Data);
        if (checked) {
            if (index == -1) {
                dsr.push(node.data.Data);
                dsrColor.push(node.data.Data + '@' + node.data.Color);
            }
        } else {
            if (index != -1) {
                dsr.splice(index, 1);
                dsrColor.splice(index, 1);
            }
        }
    }

    if (node.data.Type == 'M') {
        var index = dsmColor.indexOf(node.data.Data + '@' + node.data.Color);
        if (checked) {
            if (index == -1)
                dsmColor.push(node.data.Data + '@' + node.data.Color);
        } else {
            if (index != -1)
                dsmColor.splice(index, 1);
        }
    }
    node.set('checked', checked);
}
var checkParentNode = function (node) {
    if (node.parentNode != null) {
        if (node.data.Type == 'S' || node.data.Type == 'M' || node.parentNode.data.Type == 'B') {
            node.parentNode.set('checked', true);
            if (node.parentNode.data.Type == 'M') {
                var index = dsmColor.indexOf(node.parentNode.data.Data + '@' + node.parentNode.data.Color);
                if (index == -1) {
                    dsmColor.push(node.parentNode.data.Data + '@' + node.parentNode.data.Color);
                }
            }
            checkParentNode(node.parentNode);
        }
    }
}
var unCheckParentNode = function (node) {
    if (node.parentNode != null && (node.parentNode.data.Type == 'M' || node.parentNode.data.Type == 'B' || node.parentNode.data.Type == 'C')) {
        var flat = false;
        for (var i = 0; i < node.parentNode.childNodes.length; i++) {
            if (node.parentNode.childNodes[i].data.checked) {
                flat = true;
                break;
            }
        }
        if (!flat) {
            node.parentNode.set('checked', false);
            if (node.parentNode.data.Type == 'M') {
                var index = dsmColor.indexOf(node.parentNode.data.Data + '@' + node.parentNode.data.Color);
                if (index != -1) {
                    dsmColor.splice(index, 1);
                }
            }
            unCheckParentNode(node.parentNode);
        }
    }
}

var chkShowBranchID_Change = function (item, value) {
    changeLocation();
}
var cboLocationType_Change = function () {
    changeLocation();
}
var cboCustClass_Change = function () {

}
var cboBrand_Change = function () {

}
var cboFillType_Change = function () {
    var fillType = HQ.store.findInStore(App.cboFillType.store, ['Code'], [App.cboFillType.getValue()]);
    if (fillType) {
        var pinImageRed = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + 'EC2828',
                   new google.maps.Size(15, 30),
                   new google.maps.Point(0, 0),
                   new google.maps.Point(0, 0),
       new google.maps.Size(15, 30));

        var pinImageGreen = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + '28EC4B',
                          new google.maps.Size(15, 30),
                          new google.maps.Point(0, 0),
                          new google.maps.Point(0, 0), new google.maps.Size(15, 30));
        var code = fillType.Code;
        var min = App.txtFromAmt.getValue();
        var max = App.txtToAmt.getValue();
        for (s = 0; s < cityCust.length; s++) {
            var cityMarkers = cityCust[s].markers;
            for (var i = 0; i < cityMarkers.length; i++) {
                var item = cityMarkers[i];
                if (code == 0) {
                    item.setOptions({ icon: ((min == 0 && item.data.Amt > min) || (min > 0 && item.data.Amt >= min)) && ((item.data.Amt <= max && max > 0) || max == 0) ? pinImageGreen : pinImageRed });
                } else if (code == 1) {
                    for (var j = 0; j < dsmColor.length; j++) {
                        var data = dsmColor[j].split('@');
                        if (item.data.BranchID + '#' + item.data.DSM == data[0]) {
                            var pinImage = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + data[1],
                                        new google.maps.Size(15, 28),
                                        new google.maps.Point(0, 0),
                                        new google.maps.Point(0, 0),
                                        new google.maps.Size(15, 28));
                            item.setOptions({ icon: pinImage });
                            break;
                        }
                    }
                } else if (code == 2) {
                    for (var j = 0; j < dsrColor.length; j++) {
                        var data = dsrColor[j].split('@');
                        var data2 = data[0].split('#');

                        if (item.data.BranchID + '#' + item.data.SlsperId == data2[0] + '#' + data2[1]) {
                            var pinImage = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + data[1],
                                        new google.maps.Size(15, 28),
                                        new google.maps.Point(0, 0),
                                        new google.maps.Point(0, 0),
                                        new google.maps.Size(15, 28));
                            item.setOptions({ icon: pinImage });
                            break;
                        }
                    }
                }
                    //else if (code == 3) {
                    //    App.stoChannel.each(function (channel) {
                    //        if (item.data.Channel == channel.data.Code) {
                    //            var pinImage = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + channel.data.Color.replace("#", ""),
                    //                        new google.maps.Size(15, 28),
                    //                        new google.maps.Point(0, 0),
                    //                        new google.maps.Point(0, 0),
                    //                        new google.maps.Size(15, 28));
                    //            item.setOptions({ icon: pinImage });
                    //            return false;
                    //        }
                    //    });
                    //} else if (code == 4) {
                    //    App.stoSegment.each(function (segment) {
                    //        if (item.data.Channel == segment.data.ClassID && item.data.SegmentID == segment.data.SegmentID) {
                    //            var pinImage = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + segment.data.Color.replace("#", ""),
                    //                        new google.maps.Size(15, 28),
                    //                        new google.maps.Point(0, 0),
                    //                        new google.maps.Point(0, 0),
                    //                        new google.maps.Size(15, 28));
                    //            item.setOptions({ icon: pinImage });
                    //            return false;
                    //        }
                    //    });
                    //}
                else if (code == 5) {
                    App.stoShop.each(function (segment) {
                        if (item.data.Shop == segment.data.Shop) {
                            var pinImage = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + segment.data.Color.replace("#", ""),
                                        new google.maps.Size(15, 28),
                                        new google.maps.Point(0, 0),
                                        new google.maps.Point(0, 0),
                                        new google.maps.Size(15, 28));
                            item.setOptions({ icon: pinImage });
                            return false;
                        }
                    });
                }
                else if (code == 6) {
                    App.stoActualsituation.each(function (segment) {
                        if (item.data.Actualsituation == segment.data.Actualsituation) {
                            var pinImage = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + segment.data.Color.replace("#", ""),
                                        new google.maps.Size(15, 28),
                                        new google.maps.Point(0, 0),
                                        new google.maps.Point(0, 0),
                                        new google.maps.Size(15, 28));
                            item.setOptions({ icon: pinImage });
                            return false;
                        }
                    });
                }
                else if (code == 7) {
                    App.stoRevenue.each(function (segment) {
                        if (item.data.Revenue == segment.data.Revenue) {
                            var pinImage = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + segment.data.Color.replace("#", ""),
                                        new google.maps.Size(15, 28),
                                        new google.maps.Point(0, 0),
                                        new google.maps.Point(0, 0),
                                        new google.maps.Size(15, 28));
                            item.setOptions({ icon: pinImage });
                            return false;
                        }
                    });
                }
            }
        }

        generateLegend();
    }
}
var btnFilter_Click = function () {
    App.winCust.hide();

    clearMarkers();
    districtData = [];
    cityCust = [];
    shapes = [];
    if (App.stoCust) {
        App.stoCust.clearFilter();
        App.stoCust.clearData();
    }

    App.stoCustTop.clearFilter();
    App.stoCustTop.clearData();

    clearCustTopMarker();
    clearLocationMarker();
    clearCityBorder();
    clearDistrictBorder();

    level = 0;
    HQ.common.showBusy(true, HQ.waitMsg);
    HQ.numCov = 0;
    HQ.maxCov = 3;
    if (HQ.role != 'DSM' && HQ.role != 'DSR') {
        App.stoCity.load({
            params: {
                fromDate: App.txtFromDate.getValue(),
                dsr: getDSRPost(dsr).join(),
                dayOfWeek: App.cboDay.getValue().join(),
                weekEO: App.cboVisitType.getValue(),
                brand: App.cboBrand.getValue().join(),
                classID: App.cboCustClass.getValue().join(),
                fromAmt: App.txtFromAmt.getValue(),
                toAmt: App.txtToAmt.getValue()
            },
            callback: function () {
                checkSourceCov();
            }
        });
    } else {
        App.stoDistrictDSM.load({
            params: {
                dsr: getDSRPost(dsr).join()
            }, callback: function () {
                checkSourceCov();
            }
        });

    }
    App.stoLocation.load({
        params: {
            fromDate: App.txtFromDate.getValue(),
            dsr: getDSRPost(dsr).join(),
            dayOfWeek: App.cboDay.getValue().join(),
            weekEO: App.cboVisitType.getValue(),
            brand: App.cboBrand.getValue().join(),
            classID: App.cboCustClass.getValue().join(),
            fromAmt: App.txtFromAmt.getValue(),
            toAmt: App.txtToAmt.getValue()
        }, callback: function () {
            checkSourceCov();
        }
    });
    App.stoCust1000.load({
        params: {
            fromDate: App.txtFromDate.getValue(),
            dsr: getDSRPost(dsr).join(),
            dayOfWeek: App.cboDay.getValue().join(),
            weekEO: App.cboVisitType.getValue(),
            brand: App.cboBrand.getValue().join(),
            classID: App.cboCustClass.getValue().join(),
            fromAmt: App.txtFromAmt.getValue(),
            toAmt: App.txtToAmt.getValue()
        }, callback: function () {
            checkSourceCov();
        }
    });
}
var getSourceBorder = function () {
    if (shapes.length > 0) {
        if (HQ.role != 'DSM' && HQ.role != 'DSR') {
            var flat = false;

            for (j = 0; j < cityMap.length; j++) {
                if (cityMap[j].shapeID == shapes[0].ShapeID) {
                    flat = true;
                    break;
                }
            }

            if (!flat) {
                App.stoCityBorder.load({
                    params: {
                        shapeID: shapes[0].ShapeID
                    },
                    callback: function () {
                        var tmp = { shapeID: shapes[0].ShapeID, points: [], name: shapes[0].Name, city: shapes[0].City };
                        var records = App.stoCityBorder.getRecordsValues();

                        for (i = 0; i < records.length; i++) {
                            tmp.points.push(new google.maps.LatLng(records[i].Y, records[i].X));
                        }
                        cityMap.push(tmp);
                        shapes.splice(0, 1);
                        getSourceBorder();
                    }
                });
            } else {
                shapes.splice(0, 1);
                getSourceBorder();
            }


        } else {
            App.stoDistrictBorder.load({
                params: {
                    cityID: shapes[0].CityID,
                    districtID: shapes[0].DistrictID,
                    shapeID: shapes[0].ShapeID
                },
                callback: function () {
                    districtData.push({ ShapeID: shapes[0].ShapeID, Data: App.stoDistrictBorder.getRecordsValues(), Name: shapes[0].Name, DistrictID: shapes[0].DistrictID, City: shapes[0].City, District: shapes[0].District, CityID: shapes[0].CityID });
                    shapes.splice(0, 1);
                    getSourceBorder();
                }
            });
        }
    } else {
        if (HQ.role != 'DSM' && HQ.role != 'DSR') {
            draw();
        } else {
            App.stoCoverage.load({
                params: {
                    fromDate: App.txtFromDate.getValue(),
                    dsr: getDSRPost(dsr).join(),
                    dayOfWeek: App.cboDay.getValue().join(),
                    weekEO: App.cboVisitType.getValue(),
                    brand: App.cboBrand.getValue().join(),
                    classID: App.cboCustClass.getValue().join(),
                    fromAmt: App.txtFromAmt.getValue(),
                    toAmt: App.txtToAmt.getValue()

                }, callback: function () {
                    addStoCust();
                    if (App.stoDistrictDSM.data.items.length > 0) {
                        var cityCustObj = { City: App.stoDistrictDSM.data.items[0].data.City, Custs: App.stoCoverage.getRecordsValues() };
                        cityCust.push(cityCustObj);
                        drawCust(cityCustObj);
                    }

                    draw();
                }
            });
        }

    }
}
var checkSourceCov = function () {
    HQ.numCov++;
    if (HQ.numCov == HQ.maxCov) {
        if (HQ.role != 'DSM' && HQ.role != 'DSR') {
            if (HQ.role == 'ASM') {
                App.stoCity.data.each(function (item) {
                    App.stoCityShape.data.each(function (itemShape) {
                        if (item.data.CityID == itemShape.data.CityID) {
                            shapes.push(itemShape.data);
                        }
                    })
                });
                getSourceBorder();
            } else {
                draw();
            }

        } else {
            App.stoDistrictDSM.data.each(function (item) {
                for (var i = 0; i < App.stoDistrict.data.items.length; i++) {
                    if (App.stoDistrict.data.items[i].data.City == item.data.City && App.stoDistrict.data.items[i].data.District == item.data.District) {
                        shapes.push(App.stoDistrict.data.items[i].data);
                    }
                }
            });
            getSourceBorder();
        }
    }
}
var addStoCust = function () {
    if (!App.stoCust) return;
    App.stoCust.clearFilter();
    App.stoCust.suspendEvents();
    //var countAmt = 0;
    App.stoCoverage.data.each(function (item) {
        var cust = Ext.create('App.mdlCust');
        cust.data.CpnyID = item.data.CpnyID;
        cust.data.BranchID = item.data.BranchID;
        cust.data.DSM = item.data.DSM;
        cust.data.SlsperId = item.data.SlsperId;
        cust.data.CustId = item.data.CustId;
        cust.data.CustName = item.data.CustName;
        cust.data.SegmentID = item.data.SegmentID;
        cust.data.Channel = item.data.Channel;
        cust.data.Amt = item.data.Amt;
        cust.data.Addr1 = item.data.Addr1;
        cust.data.Addr2 = item.data.Addr2;
        App.stoCust.insert(App.stoCust.getCount() - 1, cust);
        //if (cust.data.Amt > 0) countAmt++;
    });
    App.stoCust.commitChanges();
    App.stoCust.resumeEvents();
    App.stoCust.loadPage(1);
    App.grdCust.view.refresh();
}
var addStoCustTop = function () {
    App.stoCustTop.clearFilter();
    App.stoCustTop.suspendEvents();

    App.stoCust1000.data.each(function (item) {
        var cust = Ext.create('App.mdlCustTop');
        cust.data.CpnyID = item.data.CpnyID;
        cust.data.BranchID = item.data.BranchID;
        cust.data.DSM = item.data.DSM;
        cust.data.SlsperId = item.data.SlsperId;
        cust.data.CustId = item.data.CustId;
        cust.data.CustName = item.data.CustName;
        cust.data.SegmentID = item.data.SegmentID;
        cust.data.Channel = item.data.Channel;
        cust.data.Amt = item.data.Amt;
        cust.data.Addr1 = item.data.Addr1;
        cust.data.Addr2 = item.data.Addr2;
        App.stoCustTop.insert(App.stoCustTop.getCount() - 1, cust);
        //if (cust.data.Amt > 0) countAmt++;
    });
    App.stoCustTop.commitChanges();
    App.stoCustTop.resumeEvents();
    App.stoCustTop.loadPage(1);
    App.grdCustTop.view.refresh();
}
var draw = function () {
    if (window.mobilecheck()) {
        App.winFilter.hide();
    }


    clearDistrictBorder();
    clearCityBorder();

    if (HQ.role != 'DSM' && HQ.role != 'DSR') {
        drawCityBorder();
    } else {
        drawDistrictBorderDSM();
    }

    //drawCust();
    drawCust1000();
    drawLocation();

    addStoCustTop();
    //App.lblCountAmt.setText(Ext.util.Format.number(countAmt, '0,000'));
    //App.lblCountNoAmt.setText(Ext.util.Format.number(App.stoCust.data.items.length - countAmt, '0,000'));
    //App.lblPercentAmt.setText(Ext.util.Format.number(countAmt * 100 / App.stoCust.data.items.length, '0,000') + ' %');

    generateLegend();

    HQ.common.showBusy(false);
}
var clearDistrictBorder = function () {
    for (var i = 0; i < districtBorder.length; i++) {
        districtBorder[i].setMap(null);
        if (districtBorder[i].custBorder) {
            districtBorder[i].showBorder = false;
            districtBorder[i].custBorder.setMap(null);
        }
    }
    districtBorder = [];
}
var clearCityBorder = function () {
    for (var i = 0; i < cityBorder.length; i++) {
        cityBorder[i].setMap(null);
        if (cityBorder[i].custBorder) {
            cityBorder[i].showBorder = false;
            cityBorder[i].custBorder.setMap(null);
        }
    }
    cityBorder = [];
}
var drawCityBorder = function () {
    cityBorder = [];
    if (HQ.role == 'GTM') {
        for (var i = 0; i < cityMap.length; i++) {
            var cityData = HQ.store.findInStore(App.stoCity, ['City'], [cityMap[i].city]);

            var pos = cityMap[i].points;
            var countOrder = cityData ? cityData.CustOrd : 0;
            var totalCust = cityData ? cityData.CustOrd + cityData.CustNoOrd : 0;

            var percent = cityData ? cityData.Coverage : 0;// totalCust == 0 ? 0 : (countOrder / totalCust * 100);
            var color = cityData ? cityData.Color : 'red';// percent > 90 ? 'green' : (percent > 80 ? 'yellow' : 'red');

            var border = new google.maps.Polygon({
                paths: pos,
                strokeColor: color,
                strokeOpacity: 0.8,
                strokeWeight: 2,
                fillColor: color,
                fillOpacity: 0.35,
                color: color,
                level: 0,
                data: {
                    Percent: percent,
                    CountOrder: countOrder,
                    TotalCust: totalCust,
                    City: cityMap[i].city,
                    Name: cityMap[i].name,
                    BranchID: cityData ? cityData.BranchID : ''
                }
            });

            cityBorderListener(border, cityData);

            border.setMap(map);
            cityBorder.push(border);
        }
    } else {
        App.stoCity.data.each(function (cityRecord) {
            for (var i = 0; i < cityMap.length; i++) {
                if (cityMap[i].city == cityRecord.data.City) {
                    var pos = cityMap[i].points;
                    var countOrder = cityRecord.data.CustOrd;
                    var totalCust = cityRecord.data.CustOrd + cityRecord.data.CustNoOrd;

                    var percent = cityRecord ? cityRecord.data.Coverage : 0;// totalCust == 0 ? 0 : (countOrder / totalCust * 100);
                    var color = cityRecord.data.Color;//
                    //var color = percent > 90 ? 'green' : (percent > 80 ? 'yellow' : 'red');

                    var border = new google.maps.Polygon({
                        paths: pos,
                        strokeColor: color,
                        strokeOpacity: 0.8,
                        strokeWeight: 2,
                        fillColor: color,
                        fillOpacity: 0.35,
                        color: color,
                        level: 0,
                        data: {
                            Percent: percent,
                            CountOrder: countOrder,
                            TotalCust: totalCust,
                            City: cityMap[i].city,
                            Name: cityMap[i].name,
                            BranchID: cityRecord.data.BranchID
                        }
                    });

                    cityBorderListener(border, cityRecord.data);
                    border.setMap(map);
                    cityBorder.push(border);
                }
            }
        });
    }
    if (cityBorder.length > 0) centerBorder();
}
var cityBorderListener = function (border, city) {
    new LongPress(border, 1000, 'cust');

    google.maps.event.addListener(border, "longpress", function (event) {
        google.maps.event.trigger(border, 'rightclick', event);
    });

    google.maps.event.addListener(border, "mouseover", function () {
        if (border.level == 0) {
            App.lblInfo.setText('Chi Nhánh: ' + this.data.BranchID + '  -  Thành Phố/Tỉnh: ' + this.data.Name);
            App.lblCountAmt.setText(Ext.util.Format.number(this.data.CountOrder, '0,000'));
            App.lblCountNoAmt.setText(Ext.util.Format.number(this.data.TotalCust - this.data.CountOrder, '0,000'));
            App.lblPercentAmt.setText(Ext.util.Format.number(this.data.Percent, '0,000') + ' %');
        }
    });

    google.maps.event.addListener(border, "mouseout", function () {
        if (border.level == 0) {
            App.lblInfo.setText('');
            App.lblCountAmt.setText('0');
            App.lblCountNoAmt.setText('0');
            App.lblPercentAmt.setText('0 %');
        }
    });

    border.addListener('rightclick', function (e) {
        if (border.level == 0) {
            setupMenuBorder(this);
            var keys = Object.keys(e);
            for (var i = 0; i < keys.length; i++) {
                if (e[keys[i]].type == "mousedown" || e[keys[i]].type == "contextmenu") {
                    e[keys[i]].preventDefault();
                    if (Ext.isIE) {
                        App.menuBorder.showAt(e[keys[i]].x + App.pnlFilter.width, e[keys[i]].y);
                    } else {
                        App.menuBorder.showAt(e[keys[i]].x, e[keys[i]].y);
                    }

                    break;
                }
            }
        }
    });

    border.addListener('dblclick', function (e) {
        if (border.level == 0) {
            if (!getCityCustObj(border.data.City)) {
                loadCust(border.data.City, function () {
                    cityBorderClick(border, city);
                });
            } else {
                cityBorderClick(border, city);
            }
        }
    });
}

var cityBorderClick = function (border, city) {
    if (HQ.IsShowDisctrictBorder) {
        border.level = 1;
        if (border.custBorder) {
            border.custBorder.setMap(null);
            border.showDetail = false;
        }
        HQ.common.showBusy(true, HQ.waitMsg);
        App.stoDistrictBorder.load({
            params: {
                cityID: city.CityID,
                districtID: '',
                shapeID: ''
            },
            callback: function () {
                var cityB = [];
                for (var i = 0; i < cityBorder.length; i++) {
                    if (cityBorder[i].data.City == city.City) {
                        cityB.push(cityBorder[i]);
                        if (cityBorder[i].custBorder) {
                            cityBorder[i].showBorder = false;
                            cityBorder[i].custBorder.setMap(null);
                        }
                    }
                }
                drawDistrictBorder(city, cityB);
                HQ.common.showBusy(false, HQ.waitMsg);
            }
        });
    }
}

var calculateConvexHull = function () {
    points.sort(sortPointY);
    points.sort(sortPointX);
    drawHull(points);
}
var sortPointX = function (a, b) { return a.lng() - b.lng(); }
var sortPointY = function (a, b) { return a.lat() - b.lat(); }

var drawHull = function (points) {
    if (points.length == 0) {
        return;
    }
    var hullPoints = [];

    chainHull_2D(points, points.length, hullPoints);

    hullPoints.sort(sortPointY);
    hullPoints.sort(sortPointX);
    var hullPoints2 = [];

    chainHull_2D(hullPoints, hullPoints.length, hullPoints2);
    polyline = new google.maps.Polygon({
        map: map,
        paths: hullPoints2,
        fillColor: "#0077CC",
        strokeWidth: 1,
        fillOpacity: 0.3,
        strokeColor: "#0077CC",
        strokeOpacity: 0.7
    });

    new LongPress(polyline, 1000, 'cust');
    google.maps.event.addListener(polyline, "longpress", function (event) {
        google.maps.event.trigger(polyline, 'rightclick', event);
    });
    polyline.addListener('rightclick', function (e) {
        var keys = Object.keys(e);
        for (var i = 0; i < keys.length; i++) {
            if (e[keys[i]].type == "mousedown" || e[keys[i]].type == "contextmenu") {
                e[keys[i]].preventDefault();
                if (Ext.isIE) {
                    App.menuCustBorder.showAt(e[keys[i]].x + App.pnlFilter.width, e[keys[i]].y);
                } else {
                    App.menuCustBorder.showAt(e[keys[i]].x, e[keys[i]].y);
                }

                break;
            }
        }
    });

    App.menuCustBorder.border = polyline;
    App.menuBorder.border.custBorder = polyline;
    polyline.parentBorder = App.menuBorder.border;
}
var centerBorder = function () {
    var bounds = new google.maps.LatLngBounds();

    for (var i = 0; i < cityBorder.length; i++) {
        cityBorder[i].getPath().forEach(function (element, index) { bounds.extend(element); });
    }

    map.fitBounds(bounds);
    map.setCenter(bounds.getCenter());
}
var drawDistrictBorder = function (city, cityB) {
    //clearDistrictBorder();

    for (var i = 0; i < cityB.length; i++) {
        cityB[i].setMap(null);
    }

    App.stoDistrict.data.each(function (district) {
        if (district.data.CityID == city.CityID) {
            var pos = [];
            for (var j = 0; j < App.stoDistrictBorder.data.items.length; j++) {
                var data = App.stoDistrictBorder.data.items[j].data;
                if (district.data.ShapeID == data.ShapeID) {
                    pos.push(new google.maps.LatLng(data.Y, data.X));
                }
            }

            //var countOrder = 0;
            //var totalCust = 0;
            //var cityCustObj = getCityCustObj(city.City);

            //for (var i = 0; i < cityCustObj.Custs.length; i++) {
            //    var item = cityCustObj.Custs[i];
            //    if (item.District == district.data.District) {
            //        totalCust++;
            //        if (item.Amt > 0) {
            //            countOrder++;
            //        }
            //    }
            //}

            //var percent = totalCust == 0 ? 0 : countOrder / totalCust * 100;
            var color = district.data ? district.data.Color : 'red';//percent > 90 ? 'green' : (percent > 80 ? 'yellow' : 'red');
           
            var border = new google.maps.Polygon({
                paths: pos,
                data: district.data,
                color: color,
                cityBorder: cityB,
                TotalCust: district.data.CustOrd + district.data.CustNoOrd,// totalCust,
                Percent: district.data.Coverage,//percent,
                CountOrder:district.data.CustOrd, //countOrder,
                strokeColor: color,
                strokeOpacity: 0.8,
                strokeWeight: 2,
                fillColor: color,
                showDetail: false,
                showBorder: false,
                fillOpacity: 0.35
            });

            google.maps.event.addListener(border, "mouseover", function () {
                App.lblInfo.setText('Chi Nhánh: ' + this.cityBorder[0].data.BranchID + '  -  Thành Phố/Tỉnh: ' + this.cityBorder[0].data.Name + '  -  Quận/Huyện: ' + this.data.Name);
                App.lblCountAmt.setText(Ext.util.Format.number(district.data.CustOrd, '0,000'));
                App.lblCountNoAmt.setText(Ext.util.Format.number(district.data.CustNoOrd, '0,000'));
                App.lblPercentAmt.setText(Ext.util.Format.number(district.data.Coverage, '0,000') + ' %');
            });

            google.maps.event.addListener(border, "mouseout", function () {
                App.lblInfo.setText('');
                App.lblCountAmt.setText('0');
                App.lblCountNoAmt.setText('0');
                App.lblPercentAmt.setText('0 %');
            });

            border.addListener('rightclick', function (e) {
                setupMenuBorder(this);
                var keys = Object.keys(e);
                for (var i = 0; i < keys.length; i++) {
                    if (e[keys[i]].type == "mousedown" || e[keys[i]].type == "contextmenu") {
                        e[keys[i]].preventDefault();
                        if (Ext.isIE) {
                            App.menuBorder.showAt(e[keys[i]].x + App.pnlFilter.width, e[keys[i]].y);
                        } else {
                            App.menuBorder.showAt(e[keys[i]].x, e[keys[i]].y);
                        }
                        break;
                    }
                }
            });

            new LongPress(border, 1000, 'district');

            google.maps.event.addListener(border, "longpress", function (event) {
                google.maps.event.trigger(border, 'rightclick', event);
            });

            border.setMap(map);
            districtBorder.push(border);

            var bounds = new google.maps.LatLngBounds();
            for (var i = 0; i < districtBorder.length; i++) {
                if (districtBorder[i].data.City == cityB[0].data.City) {
                    if (districtBorder[i].getPath())
                        districtBorder[i].getPath().forEach(function (element, index) { bounds.extend(element); });
                }
            }
            map.setZoom(13);
            map.fitBounds(bounds);
            map.setCenter(bounds.getCenter());
        }
    });
    //map.setCenter(cityBorder[0].getBounds().getCenter());
    //map.setZoom(11);
}
var drawDistrictBorderDSM = function () {
    clearDistrictBorder();

    App.stoDistrictDSM.data.each(function (district) {
        for (var i = 0; i < App.stoDistrict.data.items.length; i++) {
            if (App.stoDistrict.data.items[i].data.City == district.data.City && App.stoDistrict.data.items[i].data.District == district.data.District) {
                var pos = [];
                for (var j = 0; j < districtData.length; j++) {
                    if (App.stoDistrict.data.items[i].data.ShapeID == districtData[j].ShapeID) {
                        for (var k = 0; k < districtData[j].Data.length; k++) {
                            pos.push(new google.maps.LatLng(districtData[j].Data[k].Y, districtData[j].Data[k].X));
                        }
                    }
                }

                //var countOrder = 0;
                //var totalCust = 0;
                //var branchID = '';
                //App.stoCoverage.data.each(function (item) {
                //    if (item.data.District == district.data.District && item.data.City == district.data.City) {
                //        totalCust++;
                //        branchID = item.data.BranchID;
                //        if (item.data.Amt > 0) {
                //            countOrder++;
                //        }
                //    }
                //});

                //var percent = totalCust == 0 ? 0 : countOrder / totalCust * 100;
                var color = district.data ? district.data.Color : 'red';// percent > 90 ? 'green' : (percent > 80 ? 'yellow' : 'red');
                var border = new google.maps.Polygon({
                    paths: pos,
                    data: App.stoDistrict.data.items[i].data,
                    color: color,
                    TotalCust: district.data.CustOrd + district.data.CustNoOrd,// totalCust,
                    Percent: district.data.Coverage,//percent,
                    CountOrder: district.data.CustOrd, //countOrder,
                    strokeColor: color,
                    strokeOpacity: 0.8,
                    strokeWeight: 2,
                    fillColor: color,
                    showDetail: false,
                    showBorder: false,
                    fillOpacity: 0.35
                });

                new LongPress(border, 1000, 'border');

                google.maps.event.addListener(border, "mouseover", function () {
                    App.lblInfo.setText('Chi Nhánh: ' + branchID + '  -  Thành Phố/Tỉnh: ' + this.data.CityName + '  -  Quận/Huyện: ' + this.data.Name);
                    App.lblCountAmt.setText(Ext.util.Format.number(district.data.CustOrd, '0,000'));
                    App.lblCountNoAmt.setText(Ext.util.Format.number(district.data.CustNoOrd, '0,000'));
                    App.lblPercentAmt.setText(Ext.util.Format.number(district.data.Coverage, '0,000') + ' %');
                });

                google.maps.event.addListener(border, "mouseout", function () {
                    App.lblInfo.setText('');
                    App.lblCountAmt.setText('0');
                    App.lblCountNoAmt.setText('0');
                    App.lblPercentAmt.setText('0 %');
                });

                google.maps.event.addListener(border, "longpress", function (event) {
                    google.maps.event.trigger(border, 'rightclick', event);
                });

                border.addListener('rightclick', function (e) {
                    setupMenuBorder(this);
                    var keys = Object.keys(e);
                    for (var i = 0; i < keys.length; i++) {
                        if (e[keys[i]].type == "mousedown" || e[keys[i]].type == "contextmenu") {
                            e[keys[i]].preventDefault();
                            if (Ext.isIE) {
                                App.menuBorder.showAt(e[keys[i]].x + App.pnlFilter.width, e[keys[i]].y);
                            } else {
                                App.menuBorder.showAt(e[keys[i]].x, e[keys[i]].y);
                            }

                            break;
                        }
                    }
                });
                border.setMap(map);
                districtBorder.push(border);
            }
        }
    });
    if (districtBorder.length > 0) {
        var bounds = new google.maps.LatLngBounds();
        for (var i = 0; i < districtBorder.length; i++) {
            districtBorder[i].getPath().forEach(function (element, index) { bounds.extend(element); });
        }
        map.setZoom(13);
        map.fitBounds(bounds);
        map.setCenter(bounds.getCenter());
    }
}
var setupMenuBorder = function (border) {
    App.menuBorder.border = border;
    if (border.showDetail) {
        App.btnShowDetail.hide();
        App.btnHideDetail.show();
    } else {
        App.btnShowDetail.show();
        App.btnHideDetail.hide();
    }

    //if (border.showBorder) {
    //    App.btnShowBorder.hide();
    //    App.btnHideBorder.show();
    //} else {
    //    App.btnShowBorder.show();
    //    App.btnHideBorder.hide();


    //}
    App.btnShowBorder.hide();
    App.btnHideBorder.hide();
    if (HQ.role != 'DSM' && HQ.role != 'DSR') {
        if (border.level == 0) {
            App.btnCity.hide();
        } else {
            App.btnCity.show();
        }
    } else {
        App.btnCity.hide();
    }
}
var btnHideCustBorder_Click = function () {
    App.menuCustBorder.border.setMap(null);
    App.menuCustBorder.border.parentBorder.showBorder = false;
}
var btnShowDetail_Click = function () {
    App.menuBorder.border.showDetail = true;
    if (App.menuBorder.border.level != undefined) {
        showDetail(App.menuBorder.border.data.City, 0);
    } else {
        showDetail(App.menuBorder.border.data.District, 1);
    }
}
var btnHideDetail_Click = function () {
    App.menuBorder.border.showDetail = false;
    if (App.menuBorder.border.level != undefined) {
        hideDetail(App.menuBorder.border.data.City, 0);
    } else {
        hideDetail(App.menuBorder.border.data.District, 1);
    }
}
var btnShowBorder_Click = function () {
    App.menuBorder.border.showBorder = true;
    var level = 0;
    var code = '';
    var city = '';

    var cityCustObj = null;
    if (HQ.role != 'DSM' && HQ.role != 'DSR') {
        if (App.menuBorder.border.level != undefined) {
            code = App.menuBorder.border.data.City;
            level = 0;
            city = App.menuBorder.border.data.City;
        } else {
            code = App.menuBorder.border.data.District;
            level = 1;
            city = App.menuBorder.border.cityBorder[0].data.City;
        }
        cityCustObj = getCityCustObj(city);
        if (!cityCustObj) {
            loadCust(city, function () {
                showBorder(getCityCustObj(city), code, level);
            });
        } else {
            showBorder(cityCustObj, code, level);
        }
    } else {
        level = 1;
        city = App.menuBorder.border.data.City;
        code = App.menuBorder.border.data.District;
        cityCustObj = getCityCustObj(city);
        showBorder(cityCustObj, code, level);
    }

}
var btnHideBorder_Click = function () {
    App.menuBorder.border.showBorder = false;
    if (App.menuBorder.border.level != undefined) {
        hideBorder(App.menuBorder.border.data.City, 0);
    } else {
        hideBorder(App.menuBorder.border.data.District, 1);
    }
}
var btnClose_Click = function () {
    App.winFilter.hide();
}
var btnCity_Click = function () {
    for (var i = 0; i < districtBorder.length; i++) {
        if (districtBorder[i].data.City == App.menuBorder.border.data.City) {
            districtBorder[i].setMap(null);
            if (districtBorder[i].custBorder) {
                districtBorder[i].custBorder.setMap(null);
                districtBorder[i].showBorder = false;
            }
        }
    }
    var bounds = new google.maps.LatLngBounds();
    for (var i = 0; i < App.menuBorder.border.cityBorder.length; i++) {
        App.menuBorder.border.cityBorder[i].setMap(map);
        App.menuBorder.border.cityBorder[i].level = 0;

        App.menuBorder.border.cityBorder[i].getPath().forEach(function (element, index) { bounds.extend(element); });
    }
    map.fitBounds(bounds);
    map.setCenter(bounds.getCenter());
}
var clearCustTopMarker = function () {
    for (var i = 0; i < custTopMarkers.length; i++) {
        custTopMarkers[i].setMap(null);
    }
    custTopMarkers = [];
}
var clearLocationMarker = function () {
    for (var i = 0; i < locationMarkers.length; i++) {
        locationMarkers[i].setMap(null);
    }
    locationMarkers = [];
}
var showBorder = function (cityCustObj, code, level) {
    var markers = cityCustObj.markers;
    var points = [];
    for (var i = 0; i < markers.length; i++) {
        var data = markers[i].data;
        if ((level == 0 && data.City == code) || (level == 1 && data.District == code)) {
            points.push(new google.maps.LatLng(data.Lat, data.Lng));
        }
    }
    points.sort(sortPointY);
    points.sort(sortPointX);
    drawHull(points);
}
var hideBorder = function (code, level) {
    App.menuBorder.border.custBorder.setMap(null);
}
var hideDetail = function (code, level) {
    var border = level == 0 ? App.menuBorder.border : App.menuBorder.border.cityBorder;
    for (var i = 0; i < markers.length; i++) {
        markers[i].setMap(null);
    }
    
    //if (level == 0) {
    //    var cityCustObj = null;
    //    for (var i = 0; i < cityCust.length; i++) {
    //        if (cityCust[i].City == border.data.City) {
    //            cityCustObj = cityCust[i];
    //            break;
    //        }
    //    }
    //    for (var i = 0; i < cityCustObj.markers.length; i++) {
    //        cityCustObj.markers[i].setMap(null);
    //    }
    //} else {
    //    for (var i = 0; i < markers.length; i++) {
    //        var data = markers[i].data;
    //        if ((level == 0 && data.City == code) || (level == 1 && data.District == code)) {
    //            markers[i].setMap(null);
    //        }
    //    }
    //}
}
var showDetail = function (code, level) {

    var border = null;
    if (HQ.role != 'DSM' && HQ.role != 'DSR') {
        border = level == 0 ? App.menuBorder.border : App.menuBorder.border.cityBorder[0];
        var cityCustObj = null;
        if (!getCityCustObj(border.data.City)) {
            var branchIDs = border.data.BranchID.split(',');
            loadCust(border.data.City, function () {
                cityCustObj = getCityCustObj(border.data.City);

                if (level == 0) {
                    for (var i = 0; i < cityCustObj.markers.length; i++) {
                        cityCustObj.markers[i].setMap(map);
                    }
                } else if (level == 1) {
                    for (var i = 0; i < cityCustObj.markers.length; i++) {
                        if (cityCustObj.markers[i].data.District == App.menuBorder.border.data.District) {
                            cityCustObj.markers[i].setMap(map);
                        }
                    }
                }
                HQ.common.showBusy(false);
            });
        } else {
            cityCustObj = getCityCustObj(border.data.City);

            if (level == 0) {
                for (var i = 0; i < cityCustObj.markers.length; i++) {
                    cityCustObj.markers[i].setMap(map);
                }
            } else if (level == 1) {
                for (var i = 0; i < cityCustObj.markers.length; i++) {
                    if (cityCustObj.markers[i].data.District == App.menuBorder.border.data.District) {
                        cityCustObj.markers[i].setMap(map);
                    }
                }
            }
        }

    } else {
        border = App.menuBorder.border;

        cityCustObj = getCityCustObj(border.data.City);

        if (level == 0) {
            for (var i = 0; i < cityCustObj.markers.length; i++) {
                cityCustObj.markers[i].setMap(map);
            }
        } else if (level == 1) {
            for (var i = 0; i < cityCustObj.markers.length; i++) {
                if (cityCustObj.markers[i].data.District == App.menuBorder.border.data.District) {
                    cityCustObj.markers[i].setMap(map);
                }
            }
        }
    }



}
var drawCust = function (cityCustObj) {
    var pinImageRed = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + 'EC2828',
                    new google.maps.Size(15, 30),
                    new google.maps.Point(0, 0),
                    new google.maps.Point(0, 0),
        new google.maps.Size(15, 30));

    var pinImageGreen = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + '28EC4B',
                      new google.maps.Size(15, 30),
                      new google.maps.Point(0, 0),
                      new google.maps.Point(0, 0), new google.maps.Size(15, 30));

    var pinStar = new google.maps.MarkerImage("Content/Images/OM21300/star_marker.png",
                   new google.maps.Size(30, 45),
                   new google.maps.Point(0, 0),
                   new google.maps.Point(0, 0), new google.maps.Size(30, 45));
    markers = [];

    var type = App.cboFillType.getValue();
    var min = App.txtFromAmt.getValue();
    var max = App.txtToAmt.getValue();
    for (var i = 0; i < cityCustObj.Custs.length; i++) {
        var item = cityCustObj.Custs[i];
        var latLng = new google.maps.LatLng(item.Lat, item.Lng);
        var marker = null;

        if (item.Type == 'S') {
            marker = new google.maps.Marker({ position: latLng, icon: pinStar });
        } else if (type == '0') {
            marker = new google.maps.Marker({ position: latLng, icon: ((min == 0 && item.Amt > min) || (min > 0 && item.Amt >= min)) && ((item.Amt <= max && max > 0) || max == 0) ? pinImageGreen : pinImageRed });
        } else if (type == '1') {
            for (var j = 0; j < dsmColor.length; j++) {
                var data = dsmColor[j].split('@');
                if (item.BranchID + '#' + item.DSM == data[0]) {
                    var pinImage = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + data[1],
                                new google.maps.Size(15, 28),
                                new google.maps.Point(0, 0),
                                new google.maps.Point(0, 0),
                                new google.maps.Size(15, 28));
                    marker = new google.maps.Marker({ position: latLng, icon: pinImage });
                    break;
                }
            }
        } else if (type == '2') {
            for (var j = 0; j < dsrColor.length; j++) {
                var data = dsrColor[j].split('@');
                var data2 = data[0].split('#');
                if (item.BranchID + '#' + item.SlsperId == data2[0] + '#' + data2[1]) {
                    var pinImage = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + data[1],
                                new google.maps.Size(15, 28),
                                new google.maps.Point(0, 0),
                                new google.maps.Point(0, 0),
                                new google.maps.Size(15, 28));
                    marker = new google.maps.Marker({ position: latLng, icon: pinImage });
                    break;
                }
            }
        }
        else if (type == '3') {
            App.stoBrand.each(function (brand) {
                if (item.Brand == brand.data.Code) {
                    var pinImage = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + brand.data.Color.replace("#", ""),
                                new google.maps.Size(15, 28),
                                new google.maps.Point(0, 0),
                                new google.maps.Point(0, 0),
                                new google.maps.Size(15, 28));
                    marker = new google.maps.Marker({ position: latLng, icon: pinImage });
                    return false;
                }
            });
        }
        //else if (type == '4') {
        //    App.stoSegment.each(function (segment) {
        //        if (item.Channel == segment.data.ClassID && item.SegmentID == segment.data.SegmentID) {
        //            var pinImage = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + segment.data.Color.replace("#", ""),
        //                        new google.maps.Size(15, 28),
        //                        new google.maps.Point(0, 0),
        //                        new google.maps.Point(0, 0),
        //                        new google.maps.Size(15, 28));
        //            marker = new google.maps.Marker({ position: latLng, icon: pinImage });
        //            return false;
        //        }
        //    });
        //}
        if (marker != null) {
            marker.setZIndex(100);
            marker.CustId = item.CustId;
            marker.BranchID = item.BranchID;
            marker.CpnyID = item.CpnyID;
            marker.data = item;
            marker.addListener('click', function (e) {
                var $this = this;
                $this.setAnimation(google.maps.Animation.BOUNCE);
                setTimeout(function () { $this.setAnimation(null); }, 1000);
                showPopup(e, this.data);
            });
            markers.push(marker);
        }
    }
    cityCustObj.markers = markers;
}
var drawCust1000 = function () {
    custTopMarkers = [];
    var pinStar = new google.maps.MarkerImage("Content/Images/OM21300/star_marker.png",
                 new google.maps.Size(35, 35),
                 new google.maps.Point(0, 0),
                 new google.maps.Point(0, 0), new google.maps.Size(35, 35));

    for (var i = 0; i < App.stoCust1000.data.items.length; i++) {
        var locData = App.stoCust1000.data.items[i].data;
        var latLng = new google.maps.LatLng(locData.Lat, locData.Lng);
        var marker = new google.maps.Marker({ position: latLng, icon: pinStar, map: map });

        marker.data = locData;
        marker.setZIndex(102);
        marker.addListener('click', function (e) {
            var $this = this;
            $this.setAnimation(google.maps.Animation.BOUNCE);
            setTimeout(function () { $this.setAnimation(null); }, 1000);
            showPopup(e, this.data);
        });
        custTopMarkers.push(marker);
    }
}
var drawLocation = function () {
    locationMarkers = [];

    for (var i = 0; i < App.stoLocation.data.items.length; i++) {
        var locData = App.stoLocation.data.items[i].data;
        var latLng = new google.maps.LatLng(locData.Lat, locData.Lng);
        var marker = null;
        if (locData.Type == 'B') {
            var pinImage = new google.maps.MarkerImage("Content/Images/OM21300/branch.png",
                                new google.maps.Size(40, 40),
                                new google.maps.Point(0, 0),
                                new google.maps.Point(0, 0),
                                new google.maps.Size(40, 40));
            marker = new google.maps.Marker({ position: latLng, icon: pinImage });
        } else if (locData.Type == 'H') {
            var pinImage = new google.maps.MarkerImage("Content/Images/OM21300/hq.png",
                                new google.maps.Size(40, 40),
                                new google.maps.Point(0, 0),
                                new google.maps.Point(0, 0),
                                new google.maps.Size(40, 40));
            marker = new google.maps.Marker({ position: latLng, icon: pinImage });
        } else if (locData.Type == 'S') {
            var pinImage = new google.maps.MarkerImage("Content/Images/OM21300/sun.png",
                                new google.maps.Size(40, 40),
                                new google.maps.Point(0, 0),
                                new google.maps.Point(0, 0),
                                new google.maps.Size(40, 40));
            marker = new google.maps.Marker({ position: latLng, icon: pinImage });
        } else if (locData.Type == 'D') {
            var pinImage = new google.maps.MarkerImage("Content/Images/OM21300/sub.png",
                                new google.maps.Size(40, 40),
                                new google.maps.Point(0, 0),
                                new google.maps.Point(0, 0),
                                new google.maps.Size(40, 40));
            marker = new google.maps.Marker({ position: latLng, icon: pinImage });
        }
        //else if (locData.Type == 'T') {
        //    var pinImage = new google.maps.MarkerImage("Content/Images/OM21300/topstore.png",
        //                        new google.maps.Size(40, 40),
        //                        new google.maps.Point(0, 0),
        //                        new google.maps.Point(0, 0),
        //                        new google.maps.Size(40, 40));
        //    marker = new google.maps.Marker({ position: latLng, icon: pinImage });
        //}
        if (marker != null) {
            marker.data = locData;
            marker.setZIndex(101);
            locationMarkers.push(marker);
        }
    }

    changeLocation();
}
var changeLocation = function () {
    var locationType = App.cboLocationType.getValue();

    for (i = 0; i < custTopMarkers.length; i++) {
        custTopMarkers[i].setMap(locationType.indexOf('T') != -1 ? map : null);
    }

    for (var i = 0; i < locationMarkers.length; i++) {
        locationMarkers[i].setMap(locationType.indexOf(locationMarkers[i].data.Type) != -1 ? map : null);
    }
}
var drawMap = function () {
    var pinImageRed = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + 'EC2828',
                    new google.maps.Size(15, 30),
                    new google.maps.Point(0, 0),
                    new google.maps.Point(0, 0),
        new google.maps.Size(15, 30));

    var pinImageGreen = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + '28EC4B',
                      new google.maps.Size(15, 30),
                      new google.maps.Point(0, 0),
                      new google.maps.Point(0, 0), new google.maps.Size(15, 30));
    markers = [];
    var type = App.cboFillType.getValue();
    var min = App.txtFromAmt.getValue();
    var max = App.txtToAmt.getValue();
    if (cluster) {
        App.stoCoverage.data.each(function (item) {
            var latLng = new google.maps.LatLng(item.data.Lat, item.data.Lng);

            var marker = null;
            if (type == '0') {
                marker = new google.maps.Marker({ position: latLng, icon: ((min == 0 && item.data.Amt > min) || (min > 0 && item.data.Amt >= min)) && ((item.data.Amt <= max && max > 0) || max == 0) ? pinImageGreen : pinImageRed });
            } else if (type == '1') {
                for (var i = 0; i < dsmColor.length; i++) {
                    var data = dsmColor[i].split('@');
                    if (item.data.BranchID + '#' + item.data.DSM == data[0]) {
                        var pinImage = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + data[1],
                                    new google.maps.Size(15, 28),
                                    new google.maps.Point(0, 0),
                                    new google.maps.Point(0, 0),
                                    new google.maps.Size(15, 28));
                        marker = new google.maps.Marker({ position: latLng, icon: pinImage });
                        break;
                    }
                }
            } else if (type == '2') {
                for (var i = 0; i < dsrColor.length; i++) {
                    var data = dsrColor[i].split('@');
                    if (item.data.BranchID + '#' + item.data.SlsperId == data[0]) {
                        var pinImage = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + data[1],
                                    new google.maps.Size(15, 28),
                                    new google.maps.Point(0, 0),
                                    new google.maps.Point(0, 0),
                                    new google.maps.Size(15, 28));
                        marker = new google.maps.Marker({ position: latLng, icon: pinImage });
                        break;
                    }
                }
            }
            //else if (type == '3') {
            //    App.stoChannel.each(function (channel) {
            //        if (item.data.Channel == channel.data.Code) {
            //            var pinImage = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + channel.data.Color.replace("#", ""),
            //                        new google.maps.Size(15, 28),
            //                        new google.maps.Point(0, 0),
            //                        new google.maps.Point(0, 0),
            //                        new google.maps.Size(15, 28));
            //            marker = new google.maps.Marker({ position: latLng, icon: pinImage });
            //            return false;
            //        }
            //    });
            //} else if (type == '4') {
            //    App.stoSegment.each(function (segment) {
            //        if (item.data.Channel == segment.data.ClassID && item.data.SegmentID == segment.data.SegmentID) {
            //            var pinImage = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + segment.data.Color.replace("#", ""),
            //                        new google.maps.Size(15, 28),
            //                        new google.maps.Point(0, 0),
            //                        new google.maps.Point(0, 0),
            //                        new google.maps.Size(15, 28));
            //            marker = new google.maps.Marker({ position: latLng, icon: pinImage });
            //            return false;
            //        }
            //    });
            //}
            if (marker != null) {
                marker.CustId = item.data.CustId;
                marker.BranchID = item.data.BranchID;
                marker.CpnyID = item.data.CpnyID;
                marker.data = item.data;
                marker.addListener('click', function (e) {
                    var $this = this;
                    $this.setAnimation(google.maps.Animation.BOUNCE);
                    setTimeout(function () { $this.setAnimation(null); }, 1000);
                    showPopup(e, item.data);
                });
                markers.push(marker);
            }
        });
        var mcOptions = { gridSize: 50, maxZoom: 15 };
        markerCluster = new MarkerClusterer(map, markers, mcOptions);
    } else {
        App.stoCoverage.data.each(function (item) {
            var latLng = new google.maps.LatLng(item.data.Lat, item.data.Lng);
            var marker = null;
            if (type == '0') {
                marker = new google.maps.Marker({ position: latLng, icon: ((min == 0 && item.data.Amt > min) || (min > 0 && item.data.Amt >= min)) && ((item.data.Amt <= max && max > 0) || max == 0) ? pinImageGreen : pinImageRed, map: map });
            } else if (type == '1') {
                for (var i = 0; i < dsmColor.length; i++) {
                    var data = dsmColor[i].split('@');
                    if (item.data.BranchID + '#' + item.data.DSM == data[0]) {
                        var pinImage = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + data[1],
                                    new google.maps.Size(15, 28),
                                    new google.maps.Point(0, 0),
                                    new google.maps.Point(0, 0),
                                    new google.maps.Size(15, 28));
                        marker = new google.maps.Marker({ position: latLng, icon: pinImage, map: map });
                        break;
                    }
                }
            } else if (type == '2') {
                for (var i = 0; i < dsrColor.length; i++) {
                    var data = dsrColor[i].split('@');
                    if (item.data.BranchID + '#' + item.data.SlsperId == data[0]) {
                        var pinImage = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + data[1],
                                    new google.maps.Size(15, 28),
                                    new google.maps.Point(0, 0),
                                    new google.maps.Point(0, 0),
                                    new google.maps.Size(15, 28));
                        marker = new google.maps.Marker({ position: latLng, icon: pinImage, map: map });
                        break;
                    }
                }
            }
            //else if (type == '3') {
            //    App.stoChannel.each(function (channel) {
            //        if (item.data.Channel == channel.data.Code) {
            //            var pinImage = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + channel.data.Color.replace("#", ""),
            //                        new google.maps.Size(15, 28),
            //                        new google.maps.Point(0, 0),
            //                        new google.maps.Point(0, 0),
            //                        new google.maps.Size(15, 28));
            //            marker = new google.maps.Marker({ position: latLng, icon: pinImage, map: map });
            //            return false;
            //        }
            //    });
            //} else if (type == '4') {
            //    App.stoSegment.each(function (segment) {
            //        if (item.data.Channel == segment.data.ClassID && item.data.SegmentID == segment.data.SegmentID) {
            //            var pinImage = new google.maps.MarkerImage("http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|" + segment.data.Color.replace("#", ""),
            //                        new google.maps.Size(15, 28),
            //                        new google.maps.Point(0, 0),
            //                        new google.maps.Point(0, 0),
            //                        new google.maps.Size(15, 28));
            //            marker = new google.maps.Marker({ position: latLng, icon: pinImage, map: map });
            //            return false;
            //        }
            //    });
            //}
            if (marker != null) {
                marker.CustId = item.data.CustId;
                marker.BranchID = item.data.BranchID;
                marker.CpnyID = item.data.CpnyID;
                marker.data = item.data;
                marker.addListener('click', function (e) {
                    var $this = this;
                    $this.setAnimation(google.maps.Animation.BOUNCE);
                    setTimeout(function () { $this.setAnimation(null); }, 1000);
                    showPopup(e, item.data);
                });
                markers.push(marker);
            }
        });
    }
}

var tnlMap_Change = function (sender, item) {
    if (item.id == 'pnlMap') google.maps.event.trigger(map, "resize");

    App.winCust.hide();
}
var showPopup = function (e, data) {
    App.winCust.hide();
    HQ.common.showBusy(true, HQ.waitMsg);
    App.lblCust_BranchID.setText(data.BranchID);
    App.lblCust_CustID.setText(data.CustId);
    App.lblCust_CustName.setText(data.CustName);
    App.lblCust_Addr.setText(data.Addr1);
    App.lblCust_Addr2.setText(data.Addr2);
    App.lblCust_DSM.setText(data.DSM);
    App.lblCust_DSR.setText(data.SlsperId);
    App.lblCust_Amt.setText(Ext.util.Format.number(data.Amt, '0,000'));
    App.lblCust_AmtP3M.setText(Ext.util.Format.number(data.AmtP3M, '0,000'));
    App.lblCust_AmtPre.setText(Ext.util.Format.number(data.AmtPre, '0,000'));

    if (Ext.isEmpty(data.Image)) {
        App.imgCust.setImageUrl('Content/Images/no_image.png');
    } else {
        App.imgCust.setImageUrl(data.Image);
    }

    App.stoCustChartPie.load({
        params: {
            fromDate: App.txtFromDate.getValue(),
            dsr: data.BranchID + '#' + data.SlsperId,
            dayOfWeek: App.cboDay.getValue().join(),
            weekEO: App.cboVisitType.getValue(),
            brand: App.cboBrand.getValue().join(),
            classID: App.cboCustClass.getValue().join(),
            fromAmt: App.txtFromAmt.getValue(),
            toAmt: App.txtToAmt.getValue(),
            custID: data.CustId
        }
        , callback: function () {
            HQ.common.showBusy(false);
            App.winCust.showAt(App.frmMain.getWidth() - 400, 25);
        }
    });

}
var generateLegend = function () {
    if ((HQ.role != 'DSM' && HQ.role != 'DSR') || window.mobilecheck()) return;
    App.pnlLegend.removeAll();
    var num = 0;
    var fillType = App.cboFillType.getValue();
    if (fillType == '0') {
        num = 1;
        var container = new Ext.form.FieldContainer({
            layout: {
                type: 'hbox'
            },
            items: [
                new Ext.form.Label({ flex: 1, html: '<div><span style=" width:15px;height:15px;background-color:#28EC4B;display:inline-block;vertical-align:middle;margin-right:5px;"></span><span style="vertical-align: middle;">Có Đơn Hàng</span></div>' }),
                new Ext.form.Label({ flex: 1, html: '<div><span style=" width:15px;height:15px;background-color:#EC2828;display:inline-block;vertical-align:middle;margin-right:5px;"></span><span style="vertical-align: middle;">Không Có Đơn Hàng</span></div>' })
            ]
        });
        App.pnlLegend.add(container);
    } else if (fillType == 1) {
        num = (dsmColor.length + 1) / 2;
        var container = null;
        for (i = 0; i < dsmColor.length; i++) {
            var data = dsmColor[i].split('@');
            if (i % 2 == 0) {
                container = new Ext.form.FieldContainer({
                    layout: {
                        type: 'hbox'
                    }
                });

                var label = new Ext.form.Label({ flex: 1, html: '<div><span style="width:15px;height:15px;background-color:#' + data[1] + ';display:inline-block;vertical-align:middle;margin-right:5px;"></span><span style="vertical-align: middle;">' + data[0].split('#')[1] + '</span></div>' });

                container.add(label);

                if (i + 1 < dsmColor.length) {
                    data = dsmColor[i + 1].split('@');
                    label = new Ext.form.Label({ flex: 1, html: '<div><span style="width:15px;height:15px;background-color:#' + data[1] + ';display:inline-block;vertical-align:middle;margin-right:5px;"></span><span style="vertical-align: middle;">' + data[0].split('#')[1] + '</span></div>' });
                    container.add(label);
                }
                App.pnlLegend.add(container);
            }
        }
    } else if (fillType == 2) {
        num = (dsrColor.length + 1) / 2;
        var container = null;
        for (i = 0; i < dsrColor.length; i++) {
            var data = dsrColor[i].split('@');
            if (i % 2 == 0) {
                container = new Ext.form.FieldContainer({
                    layout: {
                        type: 'hbox'
                    }
                });

                var label = new Ext.form.Label({ flex: 1, html: '<div><span style="width:15px;height:15px;background-color:#' + data[1] + ';display:inline-block;vertical-align:middle;margin-right:5px;"></span><span style="vertical-align: middle;">' + data[0].split('#')[1] + '</span></div>' });

                container.add(label);

                if (i + 1 < dsrColor.length) {
                    data = dsrColor[i + 1].split('@');
                    label = new Ext.form.Label({ flex: 1, html: '<div><span style="width:15px;height:15px;background-color:#' + data[1] + ';display:inline-block;vertical-align:middle;margin-right:5px;"></span><span style="vertical-align: middle;">' + data[0].split('#')[1] + '</span></div>' });
                    container.add(label);
                }
                App.pnlLegend.add(container);
            }
        }
    }
    else if (fillType == 3) {
        num = (App.stoCustClass.data.items.length + 1) / 2;
        var container = null;
        for (i = 0; i < App.stoCustClass.data.items.length; i++) {
            var data = App.stoCustClass.data.items[i].data;
            if (i % 2 == 0) {
                container = new Ext.form.FieldContainer({
                    layout: {
                        type: 'hbox'
                    }
                });

                var label = new Ext.form.Label({ flex: 1, html: '<div><span style="width:15px;height:15px;background-color:' + data.Color + ';display:inline-block;vertical-align:middle;margin-right:5px;"></span><span style="vertical-align: middle;">' + data.ClassId + '</span></div>' });

                container.add(label);

                if (i + 1 < App.stoCustClass.data.items.length) {
                    data = App.stoCustClass.data.items[i + 1].data;
                    label = new Ext.form.Label({ flex: 1, html: '<div><span style="width:15px;height:15px;background-color:' + data.Color + ';display:inline-block;vertical-align:middle;margin-right:5px;"></span><span style="vertical-align: middle;">' + data.ClassId + '</span></div>' });
                    container.add(label);
                }
                App.pnlLegend.add(container);
            }
        }
    }
    else if (fillType == 4) {
        num = (App.stoBrand.data.items.length + 1) / 2;

        var container = null;
        for (i = 0; i < App.stoBrand.data.items.length; i++) {
            var data = App.stoBrand.data.items[i].data;
            if (i % 2 == 0) {
                container = new Ext.form.FieldContainer({
                    layout: {
                        type: 'hbox'
                    }
                });

                var label = new Ext.form.Label({ flex: 1, html: '<div><span style="width:15px;height:15px;background-color:' + data.Color + ';display:inline-block;vertical-align:middle;margin-right:5px;"></span><span style="vertical-align: middle;">' + data.Code + '</span></div>' });

                container.add(label);

                if (i + 1 < App.stoBrand.data.items.length) {
                    data = App.stoBrand.data.items[i + 1].data;
                    label = new Ext.form.Label({ flex: 1, html: '<div><span style="width:15px;height:15px;background-color:' + data.Color + ';display:inline-block;vertical-align:middle;margin-right:5px;"></span><span style="vertical-align: middle;">' + data.Code + '</span></div>' });
                    container.add(label);
                }
                App.pnlLegend.add(container);
            }
        }
    }
    if (num == 0) {
        App.winLegend.hide();
        return;
    }
    var requiredHeight = num * 21 + 40;
    requiredHeight = requiredHeight > 200 ? 200 : requiredHeight;
    App.winLegend.setHeight(requiredHeight);
    App.winLegend.showAt(document.documentElement.clientWidth - 251, document.documentElement.clientHeight - requiredHeight - 24);
}
var clearMarkers = function () {
    for (var i = 0; i < cityCust.length; i++) {
        for (var j = 0; j < cityCust[i].markers.length; j++) {
            cityCust[i].markers[j].setMap(null);
        }
        cityCust[i].markers = [];
    }
}
var btnDetail_Click = function () {
    App.winCust.hide();
    clearMarkers();

    cluster = false;
    App.btnDetail.hide();
    App.btnCluster.show();

    HQ.common.showBusy(true, HQ.waitMsg);
    setTimeout(function () {
        drawMap(false);
        HQ.common.showBusy(false, HQ.waitMsg);
    }, 1000);
}
var btnCluster_Click = function () {
    App.winCust.hide();
    clearMarkers();

    HQ.common.showBusy(true, HQ.waitMsg);
    App.btnDetail.show();
    App.btnCluster.hide();

    cluster = true;

    setTimeout(function () {
        drawMap(false);
        HQ.common.showBusy(false, HQ.waitMsg);
    }, 1000);
}
var btnDashboard_Click = function () {
    App.winDashBoard.show();
    App.stoChartPie.load({
        params: {
            fromDate: App.txtFromDate.getValue(),
            dsr: getDSRPost(dsr).join(),
            dayOfWeek: App.cboDay.getValue().join(),
            weekEO: App.cboVisitType.getValue(),
            brand: App.cboBrand.getValue().join(),
            classID: App.cboCustClass.getValue().join(),
            fromAmt: App.txtFromAmt.getValue(),
            toAmt: App.txtToAmt.getValue()
        }, callback: function () {
            App.stoChartColumn.load({
                params: {
                    fromDate: App.txtFromDate.getValue(),
                    dsr: getDSRPost(dsr).join(),
                    dayOfWeek: App.cboDay.getValue().join(),
                    weekEO: App.cboVisitType.getValue(),
                    brand: App.cboBrand.getValue().join(),
                    classID: App.cboCustClass.getValue().join(),
                    fromAmt: App.txtFromAmt.getValue(),
                    toAmt: App.txtToAmt.getValue()
                }, callback: function () {
                    HQ.common.showBusy(false);
                }
            });
        }
    });
}
var tree_BeforeItemClick = function () {
}
var labelCustRenderer = function (lbl, field, e, y) {
    var total = 0;

    App.stoCustChartPie.each(function (rec) {
        total += rec.get('Amount');
    });

    return lbl + ' ' + (+(e.data.Amount / total * 100).toFixed(2)) + '%';
}
var tipRenderer = function (storeItem, item) {
    //calculate percentage.
    var total = 0;

    App.stoChartPie.each(function (rec) {
        total += rec.get('Amount');
    });

    this.setTitle(storeItem.get('Name') + ': ' + (+(storeItem.get('Amount') / total * 100).toFixed(2)) + '%');
}
var tipCustRenderer = function (storeItem, item) {
    //calculate percentage.
    var total = 0;

    App.stoCustChartPie.each(function (rec) {
        total += rec.get('Amount');
    });

    this.setTitle(storeItem.get('Name') + ': ' + (+(storeItem.get('Amount') / total * 100).toFixed(2)) + '%');
}

var pie_ItemClick = function (item) {

    HQ.common.showBusy(true, HQ.waitMsg);
    App.stoChartColumn.load({
        params: {
            fromDate: App.txtFromDate.getValue(),
            toDate: App.txtToDate.getValue(),
            dsr: getDSRPost(dsr).join(),
            dayOfWeek: App.cboDay.getValue().join(),
            weekEO: App.cboVisitType.getValue(),
            brand: App.cboBrand.getValue().join(),
            classID: item.storeItem.data.Code,
            fromAmt: App.txtFromAmt.getValue(),
            toAmt: App.txtToAmt.getValue()
        }, callback: function () {
            HQ.common.showBusy(false);
        }
    });
}
var renderQtyAmt = function (value) {
    return Ext.util.Format.number(value, '0,000');
}

var grdCust_CellDblClick = function (item, td, cellIndex, record, tr, rowIndex, e) {
    for (s = 0; s < cityCust.length; s++) {
        var cityMarkers = cityCust[s].markers;
        for (var i = 0; i < cityMarkers.length; i++) {
            var marker = cityMarkers[i];
            if (marker.CustId == record.data.CustId && marker.BranchID == record.data.BranchID && marker.CpnyID == record.data.CpnyID) {
                App.tnlMap.setActiveTab(0);
                map.setCenter(markers[i].position);
                map.setZoom(16);
                marker.setAnimation(google.maps.Animation.BOUNCE);
                setTimeout(function () { marker.setAnimation(null); }, 1000);
                if (marker.map == undefined) {
                    marker.setMap(map);
                }
                showPopup(null, marker.data);
                break;
            }
        }
    }

}
var grdCustTop_CellDblClick = function (item, td, cellIndex, record, tr, rowIndex, e) {
    for (var i = 0; i < custTopMarkers.length; i++) {
        if (custTopMarkers[i].data.CustId == record.data.CustId && custTopMarkers[i].data.BranchID == record.data.BranchID && custTopMarkers[i].data.CpnyID == record.data.CpnyID) {
            App.tnlMap.setActiveTab(0);
            map.setCenter(custTopMarkers[i].position);
            map.setZoom(16);
            custTopMarkers[i].setAnimation(google.maps.Animation.BOUNCE);
            setTimeout(function () { custTopMarkers[i].setAnimation(null); }, 1000);
            if (custTopMarkers[i].map == undefined) {
                custTopMarkers[i].setMap(map);
            }
            showPopup(null, custTopMarkers[i].data);
            break;
        }
    }
}
var btnExport_Click = function () {
    App.frmMain.submit({
        url: 'OM21300/Export',
        timeout: 1000000,
        clientValidation: false,
        success: function (msg, data) {
        },
        params: {
            fromDate: formatDate(App.txtFromDate.getValue()),
            dsr: getDSRPost(dsr).join(),
            dayOfWeek: App.cboDay.getValue().join(),
            weekEO: App.cboVisitType.getValue(),
            brand: App.cboBrand.getValue().join(),
            classID: App.cboCustClass.getValue().join(),
            fromAmt: App.txtFromAmt.getValue(),
            toAmt: App.txtToAmt.getValue()
        },
        failure: function (msg, data) {
            HQ.message.process(msg, data, true);
        }
    });
}

function formatDate(date) {
    return (date.getMonth() + 1) + '/' + date.getDate() + '/' + date.getFullYear();
}

var loadCust = function (city, func) {
    var dsrDetail = [];

    for (var i = 0 ; i < dsr.length; i++) {
        var dsrData = dsr[i].split('#');
        //if (dsrData[2] == city) {
            dsrDetail.push(dsr[i]);
        //}
    }

    HQ.common.showBusy(true, HQ.waitMsg);

    App.stoCoverage.load({
        params: {
            fromDate: App.txtFromDate.getValue(),
            dsr: getDSRPost(dsrDetail).join(),
            dayOfWeek: App.cboDay.getValue().join(),
            weekEO: App.cboVisitType.getValue(),
            brand: App.cboBrand.getValue().join(),
            classID: App.cboCustClass.getValue().join(),
            fromAmt: App.txtFromAmt.getValue(),
            toAmt: App.txtToAmt.getValue()

        }, callback: function () {
            addStoCust();
            HQ.common.showBusy(false);
            var cityCustObj = { City: city, Custs: App.stoCoverage.getRecordsValues() };
            drawCust(cityCustObj);
            cityCust.push(cityCustObj);
            func();
        }
    });
}
var getCityCustObj = function (city) {
    var cityCustObj = null;
    for (var i = 0; i < cityCust.length; i++) {
        if (cityCust[i].City == city) {
            cityCustObj = cityCust[i];
            break;
        }
    }
    return cityCustObj;
}

var getDSRPost = function (dsrData) {
    var dsrPost = [];
    for (var i = 0; i < dsrData.length; i++) {
        var dsrInfo = dsrData[i].split('#');
        dsrPost.push(dsrInfo[0] + '#' + dsrInfo[1]);
    }
    return dsrPost;
}

function LongPress(obj, length, type) {
    var me = this;
    me.length = length;
    me.obj = obj;
    me.timeoutId = null;
    google.maps.event.addListener(obj, 'mousedown', function (e) {
        me.onMouseDown(e);
    });

    google.maps.event.addListener(obj, 'mouseup', function (e) {
        me.onMouseUp(e);
    });
    google.maps.event.addListener(obj, 'drag', function (e) {
        me.onMapDrag(e);

        App.menuMap.hide();
        App.menuBorder.hide();
        App.menuCustBorder.hide();

    });
};

LongPress.prototype.onMouseUp = function (e) {
    clearTimeout(this.timeoutId);
};
LongPress.prototype.onMouseDown = function (e) {
    clearTimeout(this.timeoutId);
    var obj = this.obj;
    var event = e;
    this.timeoutId = setTimeout(function () {
        google.maps.event.trigger(obj, 'longpress', event);
    }, this.length);
};
LongPress.prototype.onMapDrag = function (e) {
    clearTimeout(this.timeoutId);
};

window.mobilecheck = function () {
    var check = false;
    if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
        check = true;
    }

    return check;
}