//////////////////////////////////////////////////////////////////////////////////////////
document.write('<script src="' + bootPATH + 'multisort.js" type="text/javascript"></sc' + 'ript>');
mini.parse();
var grid = mini.get("#grdList");
var ids = mini.get("#IDS");
var form = new mini.Form("#form1");
grid.on("beforeload", function (e) {
    e.data.IDS = ids.getValue();
})
function GetTabsddNew(name,title,url,args) {
    //var tab = { name: "岗位信息", title: "岗位信息", url: 'wms/Main/Sys_D_User_Edit.html', showCloseButton: true };
    if (args) setCacheArgs(args);

    var tab = { name: name, title: title, url: url ,showCloseButton:true};
    if (top["win"]) {
        top["win"].addTab_Child(tab);
    } else {
        window.open(url.split("/")[url.split("/").length - 1]);
    }

}

//启用、停用
function UpdateActived(urls,tablename, falg) {
    //'act':启用 'disact':'停用'
    var row = grid.getSelecteds();
    if (row.length == 0) {
        mini.alert("请选择要处理的数据.");
        return;
    }
    var ids = "";
    for (var i = 0; i < row.length;i++){
        ids += row[i].ID + ',';
    }
    ids = ids.substring(0, ids.length - 1);
    $.ajax({
        url: urls, //"data/EditManager.aspx?method=CheckApplyFeeBill",
        data: {
            tablename: tablename, tag: falg, ids: ids
        },
        type: 'post',
        dataType: 'json',
        success: function (data) {
            if (data.iswrong == "0") {
                mini.alert("操作成功。");
                grid.reload();
            } else if (isLoginOut(result) == false) {
                mini.alert(data.errmassage);
            }
        }
    });
}
//设置排序
var setSortFlag = false;
grid.set({
    onbeforeload: function (e) {
        var grd = e.sender;
        if (e.data.pageIndex == grd.pageIndex) {
            e.data.SortFlag = 1;
        } else {
            e.data.SortFlag = 0;
        }
    },
    onload: function (e) {
        var grd = e.sender;
        var result = grd.getResultObject();
        if (isLoginOut(result) == false) {
            if (result.errMessage > '') {
                notify(result.errMessage);
            }
            ids.setValue(result["Ids"]);
        }
        if (setSortFlag == false) {
            setSortFlag = true;
            var sorter = new MultiSort(grd);
        }
    }

});

//通用查询方法
function SystemSearch() {
    if (form == null) {
        form = new mini.Form("#form1");
    }
    ids.setValue("");
    var data = form.getData(true);
    grid.load(data)
}

function onDateRenderer(e) {
    var value = e.value;
    if (value) return mini.formatDate(value, 'yyyy-MM-dd');
    return "";
}
function onDateTimeRenderer(e) {
    var value = e.value;
    if (value) return mini.formatDate(value, 'yyyy-MM-dd HH:mm:ss');
    return "";
}
function onBoolRenderer(e) {
    if (e.value == "1" || e.value=="true") return "是";
    else return "否";
}

function GetEditFormModify(gridid, caption, url) {
    var grid = mini.get(gridid);
    var row = grid.getSelecteds();
    if (row.length == 0) {
        mini.alert("请选择要打开的数据.");
        return;
    }
    //if (row.length > 1) {
    //    mini.alert("只能选择一条数据进行修改。");
    //    return;
    //}
    var id = row[0].ID;
    if (!id) {
        id = row[0].BILLID;
    }
    var name = row[0].name;
    if (name == null)
    {
        name = row[0].NAME;
    }
    if (name == null) {
        name = row[0].Name;
    }
    if (name == null) {
        name = "";
    }
    //GetTabsddNew(caption + id, caption +":"+name, url+'?id=' + id)
    GetTabsddNew(caption + id, caption + ":" + name, url, { id: id });
}