var menuClick = function (command) {
    switch (command) {
        case "first":
            HQ.grid.first(App.grdIN_Transactions);
            break;
        case "prev":
            HQ.grid.prev(App.grdIN_Transactions);
            break;
        case "next":
            HQ.grid.next(App.grdIN_Transactions);
            break;
        case "last":
            HQ.grid.last(App.grdIN_Transactions);
            break;
        case "refresh":

            App.stoIN_Transactions.reload();
            App.stoIN30100_GetStockBegEndBal.load(function () {
                if (App.stoIN30100_GetStockBegEndBal.data.getCount() > 0) {
                    App.lblBeginStock.setValue(App.stoIN30100_GetStockBegEndBal.data.items[0].data.BeginQty);
                    App.lblEndStock.setValue(App.stoIN30100_GetStockBegEndBal.data.items[0].data.EndQty);
                }
                else {
                    App.lblBeginStock.setValue(0);
                    App.lblEndStock.setValue(0);
                }
            });

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
var stoLoadgrdIN_Transactions = function (sto) {

};

/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        App.stoIN_Transactions.reload();
    }
};

