mini.parse();

loadDropDownData([["FlyCheckFlag", "FlyCheckFlag"]]);

//回显已勾选的检查项目
function SetData(data) {
    if (data) {
        data = mini.clone(data);
        mini.get("FlyCheckFlag").setValue(data.CheckFlagStr);
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
    var obj = mini.get("FlyCheckFlag").getValue();
    var data = {obj: obj};
    data = mini.encode(data);   //序列化成JSON
    CloseWindow(data);
}

//全选 or 反选
//1 :全选 0 反选
function onChecked(flag) {
    var obj = mini.get("FlyCheckFlag");
    if (flag == 1) {
        obj.selectAll();
    } else {
        var data = obj.data;
        for (var i = 0; i < data.length; i++) {
            if (obj.isSelected(data[i])) {//选中的勾掉
                obj.deselect(data[i]);
            } else {
                obj.select(data[i]);
            }
        }
    }
}








