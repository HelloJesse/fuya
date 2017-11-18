mini.parse();
function GetParams_Loc(url, c) {
    if (!url) url = location.href;
    if (!c) c = "?";
    url = url.split(c)[1];
    var params = {};
    if (url) {
        var us = url.split("&");
        for (var i = 0, l = us.length; i < l; i++) {
            var ps = us[i].split("=");
            //if (ps.length == 2) {
                params[ps[0]] = decodeURIComponent(ps[1]);
            //}
            //else {
            //    var strparm = "";
            //    for (var j = 1; j < ps.length; j++)
            //    {
            //        strparm = strparm + ps[j];
            //    }
            //    params[ps[0]] = decodeURIComponent(strparm);
            //}
        }
    }
    return params;
}


var _GrdList = mini.get("grdList");
var _MainID = 0;
var _PKName = '';
var _PKShowText = '';
var _PkValue = '';
var _KeyString = '';
var _KeyValue_String = '';
var _DBConnection = '';
var _CREATE_BY_Name = '';

var params = GetParams_Loc(); //获取传入的参数
if (params.MainID) {
    _MainID = params.MainID;
    _PKName = params.PKName;
    _PKShowText = params.PKShowText;
    _PkValue = params.PkValue;
    _KeyString = params.KeyString;
    _KeyValue_String = params.KeyValue_String;
    _DBConnection = params.DBConnection;
    _CREATE_BY_Name = params.CREATE_BY_Name;
    //document.getElementById("FileUpload").removeAttribute("uploadurl");
    //document.getElementById("FileUpload").setAttribute("uploadurl", "FileManagerAction.aspx?method=UploadData&MainID=" + _MainID + "&DBConnection=" + _DBConnection + "&CREATE_BY_Name=" + _CREATE_BY_Name);
    var _FileUpload = mini.get("FileUpload");
    _FileUpload.uploadUrl = "FileManagerAction.aspx?method=UploadData&MainID=" + _MainID + "&DBConnection=" + _DBConnection + "&CREATE_BY_Name=" + _CREATE_BY_Name;
    AddFormInfo();
    _GrdList.reload();
    //处理上传的url参数
    

}
else {
    alert("未获取到主键值,请联系管理员.");
}

_GrdList.set({
    onbeforeload: function (e) {
        e.data.MainID = _MainID;
        e.data.DBConnection = _DBConnection;
    }
});


//处理控件信息
function AddFormInfo()
{

    var tr1 = document.createElement("tr");
    var td = document.createElement("td");
    td.style.fontSize = "large";
    td.innerHTML = _PKShowText + ':' + _PkValue;
    tr1.appendChild(td);
    var tr2;
    var tr3;

    var keyname = _KeyString.split('|');
    var keyvalue = _KeyValue_String.split('|');
    var keys = "";
    var keysv = "";
    for (var i = 0; i < keyname.length; i++) {
            keys = keyname[i].split(':')[0];
            keysv = keyname[i].split(':')[1];
            if (keys.length > 0) {
                if (i < 2) {
                    for (var j = 0; j < keyvalue.length; j++) {
                        if (keys == keyvalue[j].split(':')[0]) {
                            td = document.createElement("td");
                            td.style.fontSize = "large";
                            td.innerHTML = keysv + ':' + keyvalue[j].split(':')[1];
                            tr1.appendChild(td);
                        }
                    }
                }
                else {
                    if (i == 2) {
                        tr2 = document.createElement("tr");
                        for (var j = 0; j < keyvalue.length; j++) {
                            if (keys == keyvalue[j].split(':')[0]) {
                                td = document.createElement("td");
                                td.style.fontSize = "large";
                                td.innerHTML = keysv + ':' + keyvalue[j].split(':')[1];
                                tr2.appendChild(td);
                            }
                        }
                    }
                    else {
                        if (i < 5) {
                            for (var j = 0; j < keyvalue.length; j++) {
                                if (keys == keyvalue[j].split(':')[0]) {
                                    td = document.createElement("td");
                                    td.style.fontSize = "large";
                                    td.innerHTML = keysv + ':' + keyvalue[j].split(':')[1];
                                    tr2.appendChild(td);
                                }
                            }
                        }
                        else {
                            if (i == 5) {
                                tr3 = document.createElement("tr");
                                for (var j = 0; j < keyvalue.length; j++) {
                                    if (keys == keyvalue[j].split(':')[0]) {
                                        td = document.createElement("td");
                                        td.style.fontSize = "large";
                                        td.innerHTML = keysv + ':' + keyvalue[j].split(':')[1];
                                        tr3.appendChild(td);
                                    }
                                }
                            }
                            else {
                                for (var j = 0; j < keyvalue.length; j++) {
                                    if (keys == keyvalue[j].split(':')[0]) {
                                        td = document.createElement("td");
                                        td.style.fontSize = "large";
                                        td.innerHTML = keysv + ':' + keyvalue[j].split(':')[1];
                                        tr3.appendChild(td);
                                    }
                                }
                            }
                        }
                    }

                }

            }
    }
    document.getElementById("FormTable").appendChild(tr1);
    if (tr2)
    {
        document.getElementById("FormTable").appendChild(tr2);
    }
    if (tr3) {
        document.getElementById("FormTable").appendChild(tr3);
    }
    //var tastring = " <tr> <td style='font-size:large'>" + _PKShowText + ':' + _PkValue + "</td>";
    //var keystring = "<tr>";
    //var keyname = _KeyString.split('|');
    //var keyvalue = _KeyValue_String.split('|');
    //var keys = "";
    //var keysv = "";
    //for (var i = 0; i < keyname.length; i++)
    //{
    //    keys = keyname[i].split(':')[0];
    //    keysv = keyname[i].split(':')[1];
    //    if (keys.length > 0) {
    //        if (i <2) {
    //            for (var j = 0; j < keyvalue.length; j++) {
    //                if (keys == keyvalue[j].split(':')[0]) {
    //                    tastring = tastring + "<td style='font-size:large'>" + keysv + ':' + keyvalue[j].split(':')[1] + "</td>";
    //                }
    //            }
    //        }
    //        else {
    //            if (i == 2) {
    //                tastring = tastring + "</tr><tr>"
    //                for (var j = 0; j < keyvalue.length; j++) {
    //                    if (keys == keyvalue[j].split(':')[0]) {
    //                        tastring = tastring + "<td style='font-size:large'>" + keysv + ':' + keyvalue[j].split(':')[1] + "</td>";
    //                    }
    //                }
    //            }
    //            else {
    //                if (i < 5) {
    //                    for (var j = 0; j < keyvalue.length; j++) {
    //                        if (keys == keyvalue[j].split(':')[0]) {
    //                            tastring = tastring + "<td style='font-size:large'>" + keysv + ':' + keyvalue[j].split(':')[1] + "</td>";
    //                        }
    //                    }
    //                }
    //                else 
    //                {
    //                    if (i == 5) {
    //                        tastring = tastring + "</tr><tr>"
    //                        for (var j = 0; j < keyvalue.length; j++) {
    //                            if (keys == keyvalue[j].split(':')[0]) {
    //                                tastring = tastring + "<td style='font-size:large'>" + keysv + ':' + keyvalue[j].split(':')[1] + "</td>";
    //                            }
    //                        }
    //                    }
    //                    else {
    //                        for (var j = 0; j < keyvalue.length; j++) {
    //                            if (keys == keyvalue[j].split(':')[0]) {
    //                                tastring = tastring + "<td style='font-size:large'>" + keysv + ':' + keyvalue[j].split(':')[1] + "</td>";
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}
    //tastring = tastring + "</tr>";

    //document.getElementById("FormTable").innerHTML = tastring;
    
}

//关闭界面
function fclosetab()
{
    top["win"].closeTab();
}

//刷新方法
function gridreload()
{
    _GrdList.reload();
}

//上传方法
function uplaoddata()
{
    mini.get("FileUpload").setText("");
    var win = mini.get("winImport");
    win.show();
}
//上传成功
function onUploadSuccess(e) {
    gridreload();//刷新列表
    var win = mini.get("winImport");
    win.hide();
   // alert("上传成功");
}
//上传失败
function onUploadError(e) {
    alert("上传失败.");
}

///确认
function importOk() {
    var fileupload = mini.get("FileUpload");
    fileupload.startUpload();
}
//取消
function importCancel() {
    var win = mini.get("winImport");
    win.hide();
}
 
//删除
function grddelted() {
    var rows = _GrdList.getSelecteds();
    if (rows.length == 0) {
        notify("请选择要删除的数据.");
        return;
    }
    mini.confirm("确定要执行删除操作？", "提示",
      function (action) {
          if (action == 'ok') {
              var bids = [];
              for (var i = 0; i < rows.length; i++) {

                  bids.push(rows[i].FileID);
              }

              var billids = bids.join(',');
              var urls = "FileManagerAction.aspx?method=DeleteInfo";
              $.ajax({
                  url: urls,
                  data: { FileIDs: billids, CREATE_BY_Name: _CREATE_BY_Name },
                  type: "post",
                  success: function (text) {
                      var data = mini.decode(text);

                  },
                  error: function (jqXHR, textStatus, errorThrown) {
                      alert("删除失败:" + jqXHR.responseText);
                  }
              });

          }
      });

}