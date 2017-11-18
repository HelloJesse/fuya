mini.parse();
var _Form = new mini.Form("#form1");
var _Languagefolder = getLanguageFolder()

loadDropDownData([["FlyWay", "FlyWay"], ["Status", "UavStatus"], ["TaskSource", "TaskSource"]]);

mini.get("#FlyWay").setValue(0);//飞行方式 0 船舶跟踪 

//显示船信息
function SetData(data) {
    if (data) {
        data = mini.clone(data);
        mini.get("#TaskSource").setValue(data.taskSource);//任务来源 
        mini.get("#FlyWay").setValue(data.flyWay);//任务来源 
    }
}

//取消
function onCancel(e) {
    CloseWindow("cancel");
}

function CloseWindow(action) {
    if (window.CloseOwnerWindow) return window.CloseOwnerWindow(action);
    else window.close();
}

//保存任务
function btnOK() {
    _Form.validate();
    //验证数据
    if (_Form.isValid() == false) {
        var errorTexts = _Form.getErrorTexts();
        notify(errorTexts);
        return;
    }

    var data = _Form.getData(true);
    data = mini.encode(data);   //序列化成JSON
    CloseWindow(data);
}


