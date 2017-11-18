mini.parse();
var _Grid = mini.get("#grdList");
var _Form = new mini.Form("#form1");
var _PageId;

//标准方法接口定义
function SetData(data) {
    if (data) {
        //跨页面传递的数据对象，克隆后才可以安全使用
        data = mini.clone(data);
        _PageId = data.pageId;
        _Form.loading();      //加载遮罩
        $.ajax({
            url:_Datapath +"Base/UserColumnsCode.aspx?method=GetUserColumns",
            type: "post",
            data: { PageId: _PageId },
            success: function (text) {
                data = mini.decode(text);
                _Grid.setData(data.DataList);
                setSelecteds();
                _Form.unmask(); //取消遮罩
            },
            error: function (jqXHR, textStatus, errorThrown) {
                notify("加载失败:" + jqXHR.responseText);
                _Form.unmask(); //取消遮罩
            }
        });
    }
}

//选择用户已选择的列
function setSelecteds() {
    var rows = _Grid.findRows(function (row) {
        if (row.Checked == 1) return true;
        else return false
    });
    _Grid.selects(rows);
}

//默认选中用户已勾选的列
function selectDataRow(data) {
    if (data.length <= 0 ) {
        return;
    }
    for (var i = 0; i < data.length; i++) {
        _Grid.select(data[i]);
    }
}

function CloseWindow(action) {
    if (window.CloseOwnerWindow) return window.CloseOwnerWindow(action);
    else window.close();
}

//取消
function onCancel(e) {
    CloseWindow("cancel");
}

//保存用户修改
function onSaveUserColumns(){
    _Grid.validate();
    if (_Grid.isValid() == false) {
        var error = _Grid.getCellErrors()[0];
        _Grid.beginEditCell(error.record, error.column);
        return;
    }
    var dataDetail = _Grid.getSelecteds();
    var jsonDetail = mini.encode(dataDetail);
    _Form.loading();      //加载遮罩
    $.ajax({
        url: _Datapath + "Base/UserColumnsCode.aspx?method=UpdateUserColumnsData",
        data: { data: jsonDetail, PageId : _PageId },
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                showTips("操作成功");
                CloseWindow("save");
            } else {
                if (isLoginOut(data) == false) {
                    notify("操作失败:" + data.msg);
                }
            }
            _Form.unmask(); //取消遮罩
        },
        error: function (jqXHR, textStatus, errorThrown) {
            notify("操作失败:" + jqXHR.responseText);
            onCancel();
            _Form.unmask(); //取消遮罩
        }
    });
}

//调整顺序
function applySort() {
    _Grid.sortBy("ColOrder", "asc");
    //_Grid.accept();
}
_Grid.on(
	"cellendedit", function (e) {
	applySort();
	}
);
//排序
applySort();