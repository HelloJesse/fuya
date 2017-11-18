//////////////////////////////////////////////////////////////////////////////////////////
mini.parse();
var editorId = "ke";
var tree = mini.get("tree_menu");

//富文本框编辑器
var editor = null;
KindEditor.ready(function (K) {
    var wheight = window.innerHeight-50;
    
     editor = K.create('#' + editorId, {
        //上传管理
        uploadJson: '../../kindeditor/asp.net/upload_json.ashx',
        //文件管理
        fileManagerJson: '../../kindeditor/asp.net/file_manager_json.ashx',
        allowFileManager: true,
        //设置编辑器创建后执行的回调函数
        afterCreate: function () {
            var self = this;
            K.ctrl(document, 13, function () {
                self.sync();
                K('form[name=example]')[0].submit();
            });
            K.ctrl(self.edit.doc, 13, function () {
                self.sync();
                K('form[name=example]')[0].submit();
            });
        },
        //上传文件后执行的回调函数,获取上传图片的路径
        afterUpload: function (url) {
            //alert(url);
            showTips("上传成功");
        },
        //编辑器宽度
        width: '100%',
        //编辑器高度
        height: wheight+'px;',
        //配置编辑器的工具栏
        items: [
        'source', '|', 'undo', 'redo', '|', 'preview', 'print', 'template', 'code', 'cut', 'copy', 'paste',
        'plainpaste', 'wordpaste', '|', 'justifyleft', 'justifycenter', 'justifyright',
        'justifyfull', 'insertorderedlist', 'insertunorderedlist', 'indent', 'outdent', 'subscript',
        'superscript', 'clearhtml', 'quickformat', 'selectall', '|', 'fullscreen', '/',
        'formatblock', 'fontname', 'fontsize', '|', 'forecolor', 'hilitecolor', 'bold',
        'italic', 'underline', 'strikethrough', 'lineheight', 'removeformat', '|', 'image', 'multiimage',
        'flash', 'media', 'insertfile', 'table', 'hr', 'emoticons', 'baidumap', 'pagebreak',
        'anchor', 'link', 'unlink', '|', 'about'
        ]
    });
    //prettyPrint();
});


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
        data: { ID: _treeId, ParentID: _treePid},
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.msg == "true") {
                //回显
                editor.html(data.text);
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            notify("操作失败:" + jqXHR.responseText);
        }
    });
}
//清空
function setText() {
    editor.html("");
}
//保存编辑的内容
function getText() {
    if (_treePid == -1 || _treePid == null) {
        notify("请先选择子菜单!");
        return;
    }
    var text = editor.html();
    if (text == "") {
        notify("请先录入文档内容!");
        return;
    }
    text = encodeURI(text);
    $.ajax({
        url: _Datapath + "Main/Help_Demo_Manage.aspx?method=SaveHelp_Demo_Manage",
        data: { ID: _treeId, ParentID: _treePid, Text: text },
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.msg == "true") {
                showTips("保存成功");
            } else {
                if (isLoginOut(data) == false) {
                    notify("操作失败:" + data.msg);
                }
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            notify("操作失败:" + jqXHR.responseText);
        }
    });
}

