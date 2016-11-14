var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdSYS_BuildLog);
            break;
        case "prev":
            HQ.grid.prev(App.grdSYS_BuildLog);
            break;
        case "next":
            HQ.grid.next(App.grdSYS_BuildLog);
            break;
        case "last":
            HQ.grid.last(App.grdSYS_BuildLog);
            break;
        case "refresh":
            App.stoSYS_BuildLog.reload();
            HQ.grid.first(App.grdSYS_BuildLog);
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