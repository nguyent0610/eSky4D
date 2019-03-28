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
                App.btnLoginPage.setVisible(data.result.isShowLogin);
                App.pnlRule.setVisible(data.result.isShowRule);
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

function chkShowPassword_Change(item, newValue, oldValue) {
    var type = 'password';
    if (newValue) {
        type = 'text';
    }
    App.txtOldPassword.inputEl.dom.type = type;
    App.txtNewPassword.inputEl.dom.type = type;
    App.txtReNewPassword.inputEl.dom.type = type;
}

function Check() {
    var checkRule = App.pnlRule.isVisible();
    //var decimal = /^(?=.*\d)((?=.*[a-z])|(?=.*[A-Z]))(?=.*[^a-zA-Z0-9])(?!.*\s).{6,}$/;
    if (App.txtOldPassword.value == "") {
        HQ.message.show(2019031101, '', null);
        App.txtOldPassword.focus();
        return;
    }
    else if (App.txtReNewPassword.value == "") {
        HQ.message.show(2019031102, '', null);
        App.txtReNewPassword.focus();
        return;
    }
    else if (HQ.GroupAdmin == "1" && HQ.TextValAdmin != '0' && checkRule) {
        var decimal = new RegExp("^(?=.*\\d)((?=.*[a-z])|(?=.*[A-Z]))(?=.*[^a-zA-Z0-9])(?!.*\\s).{" + HQ.TextValAdmin + ",}$", "");

        if (!App.txtNewPassword.value.match(decimal)) {
            HQ.message.show(20180111, [HQ.TextValAdmin], null, true);
            App.txtNewPassword.focus();
            return;
        }
    }
    else if (HQ.GroupAdmin == "0" && HQ.TextVal != '0' && checkRule) {
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
    else if (App.txtReNewPassword.value.length < 6) {
        HQ.message.show(2019030701, [App.txtReNewPassword.fieldLabel], null, true);
        App.txtReNewPassword.focus();
        return;
    }
    else if (App.txtReNewPassword.value.length > 100) {
        HQ.message.show(2019031103, [App.txtReNewPassword.fieldLabel], null, true);
        App.txtReNewPassword.focus();
        return;
    }
    else if (App.txtNewPassword.value.length < 6) {
        HQ.message.show(2019030701, [App.txtNewPassword.fieldLabel], null, true);
        App.txtNewPassword.focus();
        return;
    }
    else if (App.txtNewPassword.value.length > 100) {
        HQ.message.show(2019031103, [App.txtNewPassword.fieldLabel], null, true);
        App.txtNewPassword.focus();
        return;
    }
    save();
};

function save() {
    if (App.frmMain.isValid()) {
        var checkRule = App.pnlRule.isVisible();
        App.frmMain.submit({
            waitMsg: 'Submiting...',
            url: 'SA02500/SA02500Save',
            params: {
                checkRule: checkRule
            },
            success: function (result, data) {
                HQ.message.show(1504, '', null);
                if (App.btnLoginPage.isVisible()) {
                    menuClick("refresh");
                }
                else {
                    LoginPage_Click();
                }

            }
            , failure: function (errorMsg, data) {
                debugger
                if (data.result.code) {
                    HQ.message.show(data.result.code, '', '');
                    menuClick("refresh");
                }
                else {
                    //processMessage(errorMsg, data);
                    HQ.message.process(errorMsg, data, true);
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

var txt_Change = function (item, newValue, oldValue) {
    //var newValue = item.value;
    debugger
    if (Ext.isEmpty(newValue)) {
        return;
    }
    if (newValue.length > 50) {
        HQ.message.show(1234, [item.fieldLabel, 50], "", true);
        item.setValue(oldValue);
        return;
    }
}

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