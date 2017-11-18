function saveTable(formid, pageid) {
    var form = new mini.Form(formid);
    var data = form.getData();      //获取表单多个控件的数据
    var json = mini.encode(data);   //序列化成JSON
    $.ajax({
        url: "http://" + location.host + "/Action.ashx?method=S&pageid=" & pageid,
        type: "post",
        data: { submitData: json },
        success: function (text) {
            alert("提交成功，返回结果:" + text);
        }
    });
}