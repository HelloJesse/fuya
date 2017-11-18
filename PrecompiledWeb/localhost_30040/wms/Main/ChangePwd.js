mini.parse();

var form = new mini.Form("#form1");

//更新密码
function UpdatePwd() {
    
    form.validate();
    //验证数据
    if (form.isValid() == false) {
        var errorTexts = form.getErrorTexts();
        notify(errorTexts);
        return;
    }

  
    var oldpwd = mini.get("#OLDPWD").value;
    var newpwd = mini.get("#NEWPWD").value;
    var newpwdagain = mini.get("#NEWPWD_AGAIN").value;
    if (newpwd != newpwdagain) {
        notify("两次输入密码不一致");
        return;
    }

    $.ajax({
        url: _Datapath + "Main/ChangePwd.aspx?method=UpdatePwd",
        data: {
            oldPwd: oldpwd, newPwd: newpwd
        },
        type: 'post',
        success: function (text) {
            var data = mini.decode(text);
            if (data.msg == "true") {
                
                //mini.alert("密码更新成功");
                CloseWindow("OK");
            }
            else {
                if (isLoginOut(data) == false) {
                    notify("密码更新失败：" + data.msg);
                }
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            notify("操作失败:" + jqXHR.responseText);
        }
    });
}

function onCancel(e) {
    CloseWindow("cancel");
}

//关闭
function CloseWindow(action) {
    if (window.CloseOwnerWindow) return window.CloseOwnerWindow(action);
    else window.close();
}