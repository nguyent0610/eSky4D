var HQ_InvtID = '';
var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdHeader);
            break;
        case "prev":
            HQ.grid.prev(App.grdHeader);
            break;
        case "next":
            HQ.grid.next(App.grdHeader);
            break;
        case "last":
            HQ.grid.last(App.grdHeader);
            break;
        case "refresh":
            App.stoHeader.reload();
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
            HQ.common.close(this);
            break;
    }
};

var Ctrl_Expand = function (a, item) {
    HQ_InvtID = item.data.InvtID;
    App.stoDetail.reload();
};