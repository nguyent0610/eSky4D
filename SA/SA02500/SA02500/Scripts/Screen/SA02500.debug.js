
// Command of the topbar on screen
var menuClick = function (command) {
    switch (command) {
        case "first":
            break;
        case "next":
            break;
        case "prev":
            break;
        case "last":
            break;
        case "save":
            if (HQ.isUpdate || HQ.isInsert || HQ.isDelete) {
                if (HQ.form.checkRequirePass(App.frmMain))//check require truoc khi save
                    save();
            }
            break;
        case "close":
            HQ.common.close(this);
            break;
        case "new":
            break;
        case "delete":
            break;
        case "refresh":
            if (HQ.isChange) {
                HQ.message.show(20150303, '', 'refresh');
            }
            else {
                HQ.isChange = false;
                App.txtNewPassword.setValue("");
                App.txtOldPassword.setValue("");
                App.txtReNewPassword.setValue("");
                frmMain_afterRender();
            }
            break;
        default:
    }
};

var frmMain_afterRender = function () {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            url: 'SA02500/SA02500Render',
            success: function (result, data) {
                App.txtPassRule.setValue(data.result.totalpassRule);
                App.txtPassRule.setReadOnly(true);
            },
            failure: function (errorMsg, data) {
            }
        });
    }
};

var LoginPage_Click = function () {
    if (parent != undefined)
        parent.location = 'Login';
    else window.location = 'Login';
};

function save() {
    if (App.frmMain.isValid()) {
        App.frmMain.submit({
            waitMsg: 'Submiting...',
            url: 'SA02500/SA02500Save',
            params: {
            },
            success: function (result, data) {
                HQ.message.show(1504, '', null);
                menuClick("refresh");

            }
            , failure: function (errorMsg, data) {

                if (data.result.code) {
                    HQ.message.show(data.result.code, '', '');
                    menuClick("refresh");
                }
                else {
                    processMessage(errorMsg, data);
                }
            }
        });
    }
};

var txtNewPassword_Change = function (sender, e) {

    if (App.txtNewPassword.value == "") {
        HQ.message.show(1501, '', null);
        App.txtNewPassword.focus();
        return;
    }
    var decimal = /^(?=.*\d)((?=.*[a-z])|(?=.*[A-Z]))(?=.*[^a-zA-Z0-9])(?!.*\s).{6,}$/;
    if (App.txtNewPassword.value.match(decimal)) {
        return true;
    } else {
        HQ.message.show(998, '', null);
        App.txtNewPassword.focus();
        return false;
    }
};

var txtReNewPassword_Change = function (sender, e) {

    if (App.txtReNewPassword.value == "") {
        HQ.message.show(1501, '', null);
        App.txtReNewPassword.focus();
        return;
    }
    if (App.txtReNewPassword.value != App.txtNewPassword.value) {
        HQ.message.show(1503, '', null);
        App.txtReNewPassword.focus();
        return;
    }
};

var txtOldPassword_Change = function (sender, e) {
    if (App.txtOldPassword.value == "") {
        HQ.message.show(1500, '', null);
        App.txtOldPassword.focus();
        return;
    }
};


/////////////////////////////////////////////////////////////////////////
//// Other Functions ////////////////////////////////////////////////////
function refresh(item) {
    if (item == 'yes') {
        App.txtNewPassword.setValue("");
        App.txtOldPassword.setValue("");
        App.txtReNewPassword.setValue("");
        frmMain_afterRender();
    }
};
///////////////////////////////////