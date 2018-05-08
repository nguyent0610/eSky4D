var Restore_Click = function () {
    App.frmMain.submit({
        waitMsg: 'Recovering Password...',
        url: 'SA02600/ResetPassword',
        params: {
            username: App.UserName.getValue()
        },
        success: function (result, data) {
            HQ.message.show(1508, '', '');
            App.UserName.setValue('');
        }
            , failure: function (errorMsg, data) {
                if (data.result.error == '')
                    HQ.message.show(1507, '', null);
                else HQ.message.show(data.result.code, data.result.error, null);
            }
    });
};

var LoginPage_Click = function () {
    if (parent != undefined)
        parent.location = 'Login';
    else window.location = 'Login';
};