//////////////////////////////////////////////////////////////////////////////////////////
mini.parse();

var tree = mini.get("tree_menu");

//点击树节点事件
var _treeId = null;
var _treePid = null;
function onNodeSelect(e) {
    var node = e.node;
    var treePid = node.Pid;
    if (treePid == -1 || treePid == null) {
        return;
    }
    //获取菜单ID 
    var treeId = node.ID;
    _treeId = treeId;
    _treePid = treePid;
    //var win = mini.get("win1");
    //win.show();

    //需要获取显示已经保存的HelpHtml 内容
    $.ajax({
        url: _Datapath + "Main/Help_Demo_Manage.aspx?method=GetHelp_Demo_Manage",
        data: { ID: _treeId, ParentID: _treePid },
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.msg == "true") {
                //回显
                document.getElementById("HelpContent").innerHTML = data.text;
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            notify("操作失败:" + jqXHR.responseText);
        }
    });
}

