//// Declare //////////////////////////////////////////////////////////
var _Source = 0;
var _maxSource = 2;
var _isLoadMaster = false;
///////////////////////////////////////////////////////////////////////
//// Store /////////////////////////////////////////////////////////////
var checkLoad = function (sto) {
    _Source += 1;
    if (_Source == _maxSource) {
        _isLoadMaster = true;
        _Source = 0;
        App.dtpFromDate.setValue(HQ.bussinessDate);
        HQ.common.showBusy(false);
    }
};

var firstLoad = function () {
    HQ.util.checkAccessRight();
    App.frmMain.isValid();
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.cboZone.getStore().addListener('load', checkLoad);
    App.cboClassID.getStore().addListener('load', checkLoad);
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
    App.stoAlbum.reload();
};

var dtpFromDate_Change = function (dtp, newValue, oldValue, eOpts) {
    App.dtpToDate.setMinValue(App.dtpFromDate.getValue());
    if (App.dtpToDate.getValue() < App.dtpFromDate.getValue()) {
        App.dtpToDate.setValue(App.dtpFromDate.getValue());
    }
};

var cboZone_Change = function (sender, value) {
    if (sender.valueModels != null && !App.cboTerritory.store.loading) {
        App.cboTerritory.store.reload();
    }
};

var cboZone_Select = function (sender, value) {
    if (sender.valueModels != null && !App.cboTerritory.store.loading) {
        App.cboTerritory.store.reload();
    }
};

var cboTerritory_Select = function (sender, value) {
    if (sender.valueModels != null){
        if (!App.cboCustID.store.loading) {
            App.cboCustID.store.reload();
        }
        if (!App.cboBranchID.store.loading) {
            App.cboBranchID.store.reload();
        }
    }
};

var cboTerritory_Change = function (sender, value) {
    if (sender.valueModels != null) {
        if (!App.cboCustID.store.loading) {
            App.cboCustID.store.reload();
        }
        if (!App.cboBranchID.store.loading) {
            App.cboBranchID.store.reload();
        }
    }
};

var cboClassID_Select = function (sender, value) {
    if (sender.valueModels != null){
        if (!App.cboCustID.store.loading) {
            App.cboCustID.store.reload();
        }
    }
};

var cboClassID_Change = function (sender, value) {
    if (sender.valueModels != null) {
        if (!App.cboCustID.store.loading) {
            App.cboCustID.store.reload();
        }
    }
};

var cboBranchID_Change = function (sender, value) {
    if (sender.valueModels != null){
        if (!App.cboSlsperId.store.loading) {
            App.cboSlsperId.store.reload();
        }
        if (!App.cboCustID.store.loading) {
            App.cboCustID.store.reload();
        }
    }
};

var cboBranchID_Select = function (sender, value) {
    if (sender.valueModels != null) {
        if (!App.cboSlsperId.store.loading) {
            App.cboSlsperId.store.reload();
        }
        if (!App.cboCustID.store.loading) {
            App.cboCustID.store.reload();
        }
    }
};

var cboSlsperId_Select = function (sender, value) {
    if (sender.valueModels != null && !App.cboCustID.store.loading) {
        App.cboCustID.store.reload();
    }
};

var cboSlsperId_Change = function (sender, value) {
    if (sender.valueModels != null && !App.cboCustID.store.loading) {
        App.cboCustID.store.reload();
    }
};

var frmChange = function () {
    HQ.isChange = HQ.store.isChange(App.stoAR_Customer);
    HQ.common.changeData(HQ.isChange, 'AR30300');
    //HQ.common.lockItem(App.frmMain, HQ.isChange);
};

var stoBeforeLoad = function (sto) {
    HQ.common.showBusy(true, HQ.common.getLang('loadingData'));
};

var stoAR_Customer_Load = function (sto) {
    if (_isLoadMaster) {
        HQ.common.showBusy(false);
        if (sto.data.length > 0)
            App.slmAR_Customer.select(0);
    }
};

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

var btnSearch_Click = function () {
    if (HQ.form.checkRequirePass(App.frmMain)) {
        HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
        App.stoAR_Customer.reload();
    }
};

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

var btnDownloadAlbum_Click = function () {
    if (App.dtvAlbum.selModel.selected.items[0]) {
        var zipFilename = App.dtvAlbum.selModel.selected.items[0].data.TypeAlbum + '_' + App.dtvAlbum.selModel.selected.items[0].data.CustID + '.zip';
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

var btnDownloadImageSelect_Click = function () {
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
};


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

var btnUnselect_Click = function () {
    var elements = Ext.DomQuery.select('#chkDownload');
    Ext.each(elements, function (el) {
        if (el.checked)
            el.checked = false;
    });
};

var prepareDataAlbum = function (data) {
    data.shortName = Ext.util.Format.ellipsis(data.AlbumName, 15);
    return data;
};

var prepareDataImage = function (data) {
    data.shortName = Ext.util.Format.ellipsis(data.ImageName, 15);
    return data;
};

var dtvAlbum_SelectionChange = function (selModel, selected) {
    HQ.common.showBusy(true, HQ.common.getLang("loadingData"));
    App.stoImage.reload();
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
