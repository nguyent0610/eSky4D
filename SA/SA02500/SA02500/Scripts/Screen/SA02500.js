
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
                    Check();
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


function Check() {
    //var decimal = /^(?=.*\d)((?=.*[a-z])|(?=.*[A-Z]))(?=.*[^a-zA-Z0-9])(?!.*\s).{6,}$/;
    if (App.txtOldPassword.value == "") {
        HQ.message.show(1500, '', null);
        App.txtOldPassword.focus();
        return;
    }
    else if (App.txtReNewPassword.value == "") {
        HQ.message.show(1501, '', null);
        App.txtReNewPassword.focus();
        return;
    }
    else if (HQ.GroupAdmin=="1" && HQ.TextValAdmin != '0') {
        var decimal = new RegExp("^(?=.*\\d)((?=.*[a-z])|(?=.*[A-Z]))(?=.*[^a-zA-Z0-9])(?!.*\\s).{" + HQ.TextValAdmin + ",}$", "");

        if (!App.txtNewPassword.value.match(decimal)) {
            HQ.message.show(20180111, [HQ.TextValAdmin],null,true);
            App.txtNewPassword.focus();
            return;
        }
    }
    else if (HQ.GroupAdmin == "0" && HQ.TextVal != '0') {
        //var decimal = /^(?=.*\d)((?=.*[a-z])|(?=.*[A-Z]))(?=.*[^a-zA-Z0-9])(?!.*\s).{6,}$/;
        var decimal = new RegExp("^(?=.*\\d)((?=.*[a-z])|(?=.*[A-Z]))(?=.*[^a-zA-Z0-9])(?!.*\\s).{" + HQ.TextVal + ",}$", "");

        if (!App.txtNewPassword.value.match(decimal)) {
            HQ.message.show(998, [HQ.TextVal], null, true);
            App.txtNewPassword.focus();
            return;
        }
    }
    //if (HQ.TextVal == '1') {
    //    if (!App.txtNewPassword.value.match(decimal)) {
    //        HQ.message.show(998, '', null);
    //        App.txtNewPassword.focus();
    //        return;
    //    }
    //}
    else if (App.txtReNewPassword.value != App.txtNewPassword.value) {
        HQ.message.show(1503, '', null);
        App.txtReNewPassword.focus();
        return;
    }
    save();
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

};

var txtReNewPassword_Change = function (sender, e) {


};

var txtOldPassword_Change = function (sender, e) {

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