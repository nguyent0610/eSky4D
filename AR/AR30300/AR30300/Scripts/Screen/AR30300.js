//// Declare //////////////////////////////////////////////////////////
var _Source = 0;
var _maxSource = 3;
var _isLoadMaster = false;
var typeAlbum = '';
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    App.cboBranchID.setValue('ECO');
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        HQ.common.showBusy(false);
        tree_AfterRender("treeCust");
        App.btnUpLoadImage.disable();
        App.btnDelete.disable();
    }
};

var firstLoad = function () {
    HQ.util.checkAccessRight();
    App.frmMain.isValid();   
    App.cboTerritory.getStore().addListener('load', checkLoad);
    App.cboBranchID.getStore().addListener('load', checkLoad);
    App.cboContractType.getStore().addListener('load', checkLoad);
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    HQ.isNew = true;
    App.dtpFromDate.setValue(HQ.bussinessDate);
    App.dtpToDate.setValue(HQ.bussinessDate);
    App.stoAlbumDefault.reload();
    App.stoAlbumOther.reload();
   
};
////////////////////////////////////////////////////////////////////////
//// Event /////////////////////////////////////////////////////////////

var menuClick = function (command) {
    switch (command) {
        case "first":
            break;
        case "prev":
            break;
        case "next":
            break;
        case "last":
            break;
        case "refresh":    
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

var checkGrid = function (store, field) {
    var count = 0;
    var allRecords = store.snapshot || store.allData || store.data;
    allRecords.each(function (record) {
        if (record.data[field]) {
            count++;
            return false;
        }
    });
    if (count > 0)
        return true;
    else
        return false;
};

var slmAR_Customer_Change = function () {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
};

var dtpFromDate_Change = function (dtp, newValue, oldValue, eOpts) {
    App.dtpToDate.setMinValue(App.dtpFromDate.getValue());
    if (App.dtpToDate.getValue() < App.dtpFromDate.getValue()) {
        App.dtpToDate.setValue(App.dtpFromDate.getValue());
    }
   
};

var cboTerritory_Select = function (sender, value) {
    if (sender.valueModels != null){
        if (!App.cboSalesState.store.loading) {
            App.cboSalesState.store.reload();
        }
        if (!App.cboState.store.loading) {
            App.cboState.store.reload();
        }

    }
};

var cboTerritory_Change = function (sender, value) {
    if (sender.valueModels != null) {
        if (!App.cboSalesState.store.loading) {
            App.cboSalesState.store.reload();
        }
        if (!App.cboState.store.loading) {
            App.cboState.store.reload();
        }
    }
};

var cboBranchID_Change = function (sender, value) {
    if (sender.valueModels != null){
        if (!App.cboSlsperId.store.loading) {
            App.cboSlsperId.store.reload();
        }
        //if (!App.cboCustID.store.loading) {
        //    App.cboCustID.store.reload();
        //}
    }
};

var cboBranchID_Select = function (sender, value) {
    if (sender.valueModels != null) {
        if (!App.cboSlsperId.store.loading) {
            App.cboSlsperId.store.reload();
        }
        //if (!App.cboCustID.store.loading) {
        //    App.cboCustID.store.reload();
        //}
    }
};

//var cboSlsperId_Select = function (sender, value) {
//    if (sender.valueModels != null && !App.cboCustID.store.loading) {
//        App.cboCustID.store.reload();
//    }
//};

//var cboSlsperId_Change = function (sender, value) {
//    if (sender.valueModels != null && !App.cboCustID.store.loading) {
//        App.cboCustID.store.reload();
//    }
//};

var frmChange = function () {
    //HQ.isChange = HQ.store.isChange(App.stoAR_Customer);
    HQ.common.changeData(HQ.isChange, 'AR30300');
    //HQ.common.lockItem(App.frmMain, HQ.isChange);
};

var stoAlbumListDefault_Load = function (sto) {
    sto.filterBy(function (record) {
        if (record.data.isSystem == 1) {
            return record;
        }
    });
}
var stoAlbumListOther_Load = function (sto) {
    sto.filterBy(function (record) {
        if (record.data.isSystem == 0) {
            return record;
        }
    });

}
////Function menuClick
var save = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: HQ.common.getLang("WaitMsg"),
            timeout: 1000000,
            url: 'AR30300/Save',
            params: {
                lstAR_Customer: HQ.store.getAllData(App.stoAR_Customer, ['colCheck'], [true], true)
            },
            success: function (msg, data) {
                HQ.message.show(201405071);
                refresh('yes');
            },
            failure: function (msg, data) {
                HQ.message.process(msg, data, true);
            }
        });
    }
};

var btnReadData_Click = function () {
    tree_AfterRender("treeCust");
    App.stoImage.reload();
    var tree = App.treePanelCustomer,
    text = App.cboSlsperId.getRawValue();
   // text = App.txtCustID.getValue();
    tree.clearFilter();
    if (Ext.isEmpty(text, false)) {
         expandNode(App.treePanelCustomer);
          //  expandAll(App.treePanelCustomer);     
           
        return;
    } else {
        filterTreeByByValue(tree, text);
    }
    
    
}
var convertImgToBase64URL = function (url, filename, callback) {
    var img = new Image();
    img.crossOrigin = 'Anonymous';
    img.onload = function () {
        var canvas = document.createElement('CANVAS'),
        ctx = canvas.getContext('2d'), dataURL;
        canvas.height = this.height;
        canvas.width = this.width;
        ctx.drawImage(this, 0, 0);
        dataURL = canvas.toDataURL();

        callback(dataURL, filename, url);
        canvas = null;
    };
    img.src = url;
};
//---------------Button menu---------------------
var btnUnselect_Click = function () {
    var elements = Ext.DomQuery.select('#chkDownload');
    Ext.each(elements, function (el) {
        if (el.checked)
            el.checked = false;
    });
};
var btnSaveImage_Click = function () {
    if (App.dtvAlbumDefault.selModel.selected.length > 0) {
        if (App.dtvAlbumDefault.selModel.selected.items[0]) {
            var flag = false;
            var links = [];
            var linksName = [];
            var elements = Ext.DomQuery.select('#chkDownload');
            Ext.each(elements, function (el) {
                if (el.checked) {
                    links.push(el.name);
                    linksName.push(el.alt);
                }
            });
            if (links.length > 0)
                downloadAll(links, linksName);
            else
                HQ.message.show('2016111511');
        } else {
            HQ.message.show('2016111610');
        }
    }
    if (App.dtvAlbumOther.selModel.selected.length > 0) {
        if (App.dtvAlbumOther.selModel.selected.items[0]) {
            var flag = false;
            var links = [];
            var linksName = [];
            var elements = Ext.DomQuery.select('#chkDownload');
            Ext.each(elements, function (el) {
                if (el.checked) {
                    links.push(el.name);
                    linksName.push(el.alt);
                }
            });
            if (links.length > 0)
                downloadAll(links, linksName);
            else
                HQ.message.show('2016111511');
        } else {
            HQ.message.show('2016111610');
        }
    }
}
var downloadAll = function (urls, linksName) {
    var link = document.createElement('a');
    link.style.display = 'none';
    document.body.appendChild(link);
    for (var i = 0; i < urls.length; i++) {
        link.setAttribute('href', urls[i]);
        link.setAttribute('download', linksName[i]);
        link.click();
    }
    document.body.removeChild(link);
};

var btnUpLoadImage_Click = function (fup) {
    if (fup.value) {
        _fup1 = fup;
        var ext = fup.value.split(".").pop().toLowerCase();
        if (ext == "jpg" || ext == "png") {
            HQ.common.showBusy(true, HQ.common.getLang('Uploading...'), App.frmMain);
            readImage(fup, App.imgImages2, App.hdnPPCStorePicReq2);
        }
        else {
            HQ.message.show(148, '', '');
        }
    }
};
var readImage = function (fup, imgControl, ctr) {
    var files = fup.fileInputEl.dom.files;
    if (files && files[0]) {
        if (files[0].size / 1024 / 1024 > 20) {
            HQ.message.show(20150403, ['20'], '', true);
            HQ.common.showBusy(false);
        }
        else {
            ctr.setValue(fup.value);
            var FR = new FileReader();
            FR.onload = function (e) {
                imgControl.setImageUrl(e.target.result);
                HQ.common.showBusy(false);
                App.frmMain.submit({
                    clientValidation: false,
                    waitMsg: HQ.common.getLang("Loadingdata"),
                    method: 'POST',
                    url: 'AR30300/UploadFiles',
                    timeout: 180000,
                    params: {
                        fileName: fup.value,
                        branchID:"ECO",
                        slsperID: App.treePanelCustomer.selModel.selected.items[0].data.ParentID,
                        albumID: App.dtvAlbumOther.selModel.selected.items[0].data.AlbumID,
                        custID: App.treePanelCustomer.selModel.selected.items[0].data.RecID,
                    },
                    success: function (msg, data) {
                        HQ.message.show(2019011468);                      
                        App.stoImage.reload();                      
                    },
                    failure: function (msg, data) {
                        HQ.message.process(msg, data, true);
                        App.stoImage.reload();
                    }
                });
            };
            FR.readAsDataURL(files[0]);
        }
    }
};



var btnPrintImage_Click = function () {

}
var btnDeleteImage_Click = function (obj, command, record, index, fn) {
    if (App.dtvImage.selModel.selected.length > 0) {
        HQ.message.show(2016090601, '', 'ClearImage');
    }
}
function ClearImage(item) {
    if (item == 'yes') {
        App.frmMain.submit({
            timeout: 1800000,
            clientValidation: false,
            type: 'POST',
            url: 'AR30300/AR30300DeleteImage',
            params: ({
                branchID: "ECO",
                albumID: App.dtvAlbumOther.selModel.selected.items[0].data.AlbumID,
                slsperID: App.treePanelCustomer.selModel.selected.items[0].data.RecID,
                imageName: App.dtvImage.selModel.selected.items[0].data.linkreal,
            }),
            success: function (msg, data) {
                App.stoImage.reload();
            },
            failure: function (errorMsg, data) {
                HQ.message.process(errorMsg, data, true);
            }
        });
    }
};
var btnDownloadAlbum_Click = function () {
    if (App.dtvAlbumDefault.selModel.selected.items[0]) {
        var zipFilename = App.dtvAlbumDefault.selModel.selected.items[0].data.TypeAlbum + '_' + App.dtvAlbumDefault.selModel.selected.items[0].data.CustID + '.zip';
        var zip = new JSZip();
        var count = 0;
        var urls = [];
        var urlsName = [];
        var store = App.stoImage;
        var allRecords = store.snapshot || store.allData || store.data;

        
        store.suspendEvents();
        allRecords.each(function (record) {
            if (!Ext.isEmpty(record.data.Pic)) {
                urls.push(record.data.Pic);
                urlsName.push(record.data.ImageName);
            }
        });
        store.resumeEvents();

        if (urls.length > 0) {
            for (var i = 0; i < urls.length; i++) {
                zip.file(urlsName[i], urls[i].split(',')[1], { base64: true });
            }
            zip.generateAsync({ type: 'blob' }).then(function (content) {
                saveAs(content, zipFilename);
                HQ.common.showBusy(false);
            });

        }
        else {
            HQ.message.show('2016111611');
            HQ.common.showBusy(false);
        }
    }
    else {
        HQ.message.show('2016111610');
    }
};

var prepareDataAlbum = function (data) {
    data.shortName = Ext.util.Format.ellipsis(data.AlbumName, 15);
    return data;
};
var prepareDataAlbumOther = function (data) {
    data.shortName = Ext.util.Format.ellipsis(data.AlbumName, 15);
    return data;
};

var prepareDataImage = function (data) {
    data.shortName = Ext.util.Format.ellipsis(data.ImageName, 15);
    return data;
};
var treePanelCustomer_SlecheckChange = function () {
    if (App.dtvAlbumDefault.selModel.selected.length > 0) {
        if (App.dtvAlbumDefault.selModel.selected.items[0].data.isSystem == 1) {
                App.btnUpLoadImage.disable();
                App.btnDelete.disable();
        }
    }
    if (App.dtvAlbumOther.selModel.selected.length > 0) {
        if (App.dtvAlbumOther.selModel.selected.items[0].data.isSystem == 0) {
            if (App.treePanelCustomer.selModel.selected.items[0].data.Type == 'RM' || App.treePanelCustomer.selModel.selected.items[0].data.Type == 'AM'
                || App.treePanelCustomer.selModel.selected.items[0].data.Type == 'SS') {
                App.btnUpLoadImage.disable();
            } else {
                App.btnUpLoadImage.enable();
            }
        }
    }
    App.stoImage.reload();
}
var dtvAlbumDefault_SelectionChange = function (selModel, selected) {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    if (selModel.selected.length > 0) {
        if (selModel.selected.items[0].data.isSystem == 1) {
            App.btnUpLoadImage.disable();
            App.btnDelete.disable();
            App.dtvAlbumOther.getSelectionModel().deselectAll();
        }
        typeAlbum = App.dtvAlbumDefault.selModel.selected.items[0] == undefined ? '' : App.dtvAlbumDefault.selModel.selected.items[0].data.AlbumID;
        App.stoImage.reload();
    }
};
var dtvAlbumOther_SelectionChange = function (selModel, selected) {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    if (selModel.selected.length > 0) {
        if (selModel.selected.items[0].data.isSystem == 0) {
            App.dtvAlbumDefault.getSelectionModel().deselectAll();
            App.btnDelete.enable();
        }
        if (App.treePanelCustomer.selModel.selected.length > 0) {
            if (App.treePanelCustomer.selModel.selected.items[0].data.Type == 'RM' || App.treePanelCustomer.selModel.selected.items[0].data.Type == 'AM'
                || App.treePanelCustomer.selModel.selected.items[0].data.Type == 'SS') {
                App.btnUpLoadImage.disable();
            } else {
                App.btnUpLoadImage.enable();
            }
        }
        typeAlbum = App.dtvAlbumOther.selModel.selected.items[0] == undefined ? '' : App.dtvAlbumOther.selModel.selected.items[0].data.AlbumID;
        App.stoImage.reload();
    }
};
var dtvImage_ItemDblClick = function (sender, selected) {
    //App.imgView.setImageUrl('');
    //if (selected) {
    //    App.winView.show();
    //    App.winView.selModel = sender.selModel;
    //    App.imgView.setImageUrl(selected.data.Pic);
    //}
};
var onerror_Image = function (eImage) {
    var divParent = eImage.parentElement.parentElement;
    divParent.hidden = true;
};

var ondblclick_Image = function () {
    App.imgView.setImageUrl('');
    if (App.dtvImage.selModel.selected) {
        App.winView.show();
        App.winView.selModel = App.dtvImage.selModel;
        App.imgView.setImageUrl(App.dtvImage.selModel.selected.items[0].data.Pic);
    }
};

var btnPrevImage_Click = function () {
    var curIndex = -1;
    if (App.winView.selModel.store.totalCount > 0) {
        if (App.winView.selModel.getSelection()[0]) {
            curIndex = App.winView.selModel.getSelection()[0].index;
        }
    }
    if ((curIndex - 1) >= 0) {
        App.winView.selModel.select(curIndex - 1);
        App.imgView.setImageUrl(App.winView.selModel.store.data.items[(curIndex - 1)].data.Pic);
    }
};

var btnNextImage_Click = function () {
    var maxIndex = -1;
    var curIndex = -1;
    if(App.winView.selModel.store.totalCount > 0){
        maxIndex = App.winView.selModel.store.totalCount;
        if(App.winView.selModel.getSelection()[0]){
            curIndex = App.winView.selModel.getSelection()[0].index;
        }
    }
    if (curIndex > -1 && (curIndex + 1) < maxIndex) {
        App.winView.selModel.select(curIndex + 1);
        App.imgView.setImageUrl(App.winView.selModel.store.data.items[(curIndex + 1)].data.Pic);
    }
};
///////////////////////////////////////// TREE CUST VIEW ////////////////////////////////////
tree_AfterRender = function (id) {
    HQ.common.showBusy(true, HQ.waitMsg);
    App.direct.AR30300GetTreeCustomer(id,App.cboTerritory.getValue(), App.cboState.getValue(), App.cboSlsperId.getValue(), App.txtCustID.getValue(), App.cboBranchID.getValue(), {
        success: function (result) {
            App.treePanelCustomer.getRootNode().expand();
            HQ.common.showBusy(false, HQ.waitMsg);
        }, failure: function (result) {
            HQ.common.showBusy(false, HQ.waitMsg);

        }
    });
}
treePanelCustomer_checkChange = function (node, checked) {
    if (node.hasChildNodes()) {
        node.eachChild(function (childNode) {
            childNode.set('checked', checked);
            treePanelCustomer_checkChange(childNode, checked);
        });
    }
}
var tree_ItemCollapse = function (a, b) {
    collapseNode(a);
}//ham nay controller goi
btnExpand_click = function (btn, e, eOpts) {
    App.treePanelCustomer.suspendLayouts();
    expandAll(App.treePanelCustomer);
    App.treePanelCustomer.resumeLayouts(true);  
}

btnCollapse_click = function (btn, e, eOpts) {
    App.treePanelCustomer.suspendLayouts();
    collapseAll(App.treePanelCustomer);
    App.treePanelCustomer.resumeLayouts(true);
}
getLeafNodes = function (node) {
    var childNodes = [];
    node.eachChild(function (child) {
        if (child.isLeaf()) {
            childNodes.push(child);
        }
        else {
            var children = getLeafNodes(child);
            if (children.length) {
                children.forEach(function (nill) {
                    childNodes.push(nill);
                });
            }
        }
    });
    return childNodes;
}
var updateTreeView = function (tree, fn) {
    var view = tree.getView();
    view.getStore().loadRecords(fn(tree.getRootNode()));
    view.refresh();
};
/// ///////// Expand /////////////////////////////////////////////////////////////
var expandAll = function (tree) {
    this.updateTreeView(tree, function (root) {
        var nodes = [];
        App.treeCust.suspendLayouts();       
        root.cascadeBy(function (node) {
            if (!node.isRoot() || tree.rootVisible) {
                node.data.expanded = true;
                nodes.push(node);
            }
        });
        return nodes;
        App.treeCust.resumeLayouts(true);
    });

   
}
var expandNode = function (node) {
    if (node != undefined) {
        node.expand();
        if (node.childNodes.length > 0) {
            for (var i = 0; i < node.childNodes.length; i++) {
                expandNode(node.childNodes[i]);
            }
        }
    }
}

/// ///////// Collapse /////////////////////////////////////////////////////////////
var collapseAll = function (tree) {
    this.updateTreeView(tree, function (root) {
        root.cascadeBy(function (node) {
            if (!node.isRoot() || tree.rootVisible) {
                node.data.expanded = false;
            }
        });
        return tree.rootVisible ? [root] : root.childNodes;
    });
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
var removeUnicode = function (str) {
    str = str.toLowerCase();
    str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/gi, 'a');
    str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/gi, 'e');
    str = str.replace(/ì|í|ị|ỉ|ĩ/gi, 'i');
    str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/gi, 'o');
    str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/gi, 'u');
    str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/gi, 'y');
    str = str.replace(/đ/gi, 'd');
    // str = str.replace(/\W+/g, ' ');
    // str = str.replace(/\s/g, '-');
    return str;
}
var filterTreeByByValue = function (tree,text) {
    App.frmMain.mask();
   Ext.suspendLayouts();
    var lstParent = [];
    var re = new RegExp(".*" + removeUnicode(text) + ".*", "i");
   // var re = new RegExp(".*" + text + ".*", "i");
    tree.filterBy(function (node) {
        if (re.test(removeUnicode(node.data.text))) {
         // if (re.test(node.data.text)) {      
            lstParent.push(node.id);
            return node;
        } else {
            if (lstParent.indexOf(node.parentNode.id) > -1) {
                lstParent.push(node.id);
                return node;
            }
        }
    });
    // Ext.getCmp('treeCust').body.scrollTo('top', 0);
    Ext.resumeLayouts(true);
    //App.treePanelCustomer.resumeLayouts(true);
    App.frmMain.unmask();
}

//------------------------CustSearch------------------
var btnFindCustID_click = function () {
    App.winCustID.show();
};
var stoAR_pgCust_load = function (sto) {
    HQ.isFirstLoad = false;
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
    }
};
var btnSearch_Click = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
    App.stoAR_pgCust.reload();
}
var grdCustID_CellDblClick = function (cellIndex, record) {
    _custID = App.grdCustID.selModel.selected.items[0].data.CustID;
    App.winCustID.hide();
    App.txtCustID.setValue(_custID);
}