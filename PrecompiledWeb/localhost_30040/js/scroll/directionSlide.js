/*	window.onload = function() {
	var oActionBlock = document.getElementById('action-block');
	var oActionBar = document.getElementById('action-bar');
	var oScrollBar = document.getElementById('scroll-bar');
	var oShowAmount = document.getElementById('showAmount').getElementsByTagName('input')[0];
	var length = 550;

	clickSlide(oActionBlock, oActionBar, oScrollBar, 300, length, oShowAmount);
	drag(oActionBlock, oActionBar, 300, length, oShowAmount);
	addScale(60, 300, length, oScrollBar);
	inputBlur(oActionBlock, oActionBar, length, oShowAmount);
}		*/

//oActionBlock  actionBlock对应滑块的id --指针
//oActionBar  对应滑动多少的显示条的id --已渲染区域
//oSlideBar 

var _showAreaControl = null;//哪个控件值

function SlideBar(data){
	var _this = this;
	var oActionBlock = document.getElementById(data.actionBlock);
	var oActionBar = document.getElementById(data.actionBar);
	var oSlideBar = document.getElementById(data.slideBar);
	var barLength = data.barLength;
	var interval = data.interval;
	var maxNumber = data.maxNumber;
	var flag = data.flag;
	var oShowArea = null;
	if(data.showArea){
		oShowArea = document.getElementById(data.showArea);	
	}

	if(oShowArea){
		_this.addScale(oSlideBar, interval, maxNumber, barLength, flag);
		_this.inputBlur(oActionBlock, oActionBar, maxNumber, barLength, oShowArea);
		_this.clickSlide(oActionBlock, oActionBar, oSlideBar, maxNumber, barLength, oShowArea, flag);
		_this.drag(oActionBlock, oActionBar, maxNumber, barLength, oShowArea, flag);
	}
	else{
	    _this.addScale(oSlideBar, interval, maxNumber, barLength, flag);
	    _this.clickSlide(oActionBlock, oActionBar, oSlideBar, maxNumber, barLength, flag);
	    _this.drag(oActionBlock, oActionBar, maxNumber, barLength, flag);
	}
	
}

SlideBar.prototype = {
	//初始化(添加刻度线)
    addScale: function (slideBar, interval, total, barLength, flag) {
		// interval代表刻度之间间隔多少, total代表最大刻度
		// slideBar表示在哪个容器添加刻度
        var num = total / interval; //num为应该有多少个刻度

        //特殊滚动条 ，左右
        if (flag) {
            var middle = barLength / 2; //取中间px 值
            //右 
            for (var i = 0; i < num + 1; i++) {

                var oScale = document.createElement('div');
                oScale.style.width = '2px';
                oScale.style.height = '14px';
                oScale.style.position = 'absolute';
                oScale.style.background = '#AFAFAF';
                oScale.style.zIndex = '-10';
                oScale.style.left = (i * interval * middle) / total + middle + 'px';
                slideBar.appendChild(oScale);

                var oText = document.createElement('div');
                oText.style.position = 'absolute';
                oText.style.top = '16px';
                oText.style.height = '16px';
                oText.innerHTML = i * interval;
                slideBar.appendChild(oText);
                oText.style.left = ((i * interval * middle) / total) - (oText.offsetWidth / 2) + middle + 'px';

            }
            //左
            for (var i = 0; i < num + 1; i++) {

                var oScale = document.createElement('div');
                oScale.style.width = '2px';
                oScale.style.height = '14px';
                oScale.style.position = 'absolute';
                oScale.style.background = '#AFAFAF';
                oScale.style.zIndex = '-10';
                oScale.style.right = (i * interval * middle) / total + middle + 'px';
                slideBar.appendChild(oScale);

                var oText = document.createElement('div');
                oText.style.position = 'absolute';
                oText.style.top = '16px';
                oText.style.height = '16px';
                oText.innerHTML = i * interval;
                slideBar.appendChild(oText);
                oText.style.right = ((i * interval * middle) / total) - (oText.offsetWidth / 2) + middle + 'px';
            }
        } else {
            for (var i = 0; i < num + 1; i++) {

                var oScale = document.createElement('div');
                oScale.style.width = '2px';
                oScale.style.height = '14px';
                oScale.style.position = 'absolute';
                oScale.style.background = '#AFAFAF';
                oScale.style.zIndex = '-10';
                oScale.style.left = (i * interval * barLength) / total + 'px';
                slideBar.appendChild(oScale);

                var oText = document.createElement('div');
                oText.style.position = 'absolute';
                oText.style.top = '16px';
                oText.style.height = '16px';
                oText.innerHTML = i * interval;
                slideBar.appendChild(oText);
                oText.style.left = ((i * interval * barLength) / total) - (oText.offsetWidth / 2) + 'px';

            }
        }

	},

	// 监听输入框
	inputBlur : function(actionBlock, actionBar, maxNumber, barLength, input){
		//actionBlock指滑块,actionBar指显示条,input指显示的输入框
		var _this = this;
		input.onblur = function(){
			var inputVal = this.value;
			_this.autoSlide(actionBlock, actionBar, maxNumber, barLength, inputVal);
		}
	},

	/* 在输入框输入值后自动滑动	*/
	autoSlide : function(actionBlock, actionBar, total, barLength, inputVal){
		//inputVal表示输入框中输入的值
		var _this = this;
		var target = (inputVal / total * barLength);
		_this.checkAndMove(actionBlock, actionBar, target);
	},

	/*	检查target(确认移动方向)并滑动	*/
	checkAndMove : function(actionBlock, actionBar, target){
		if(target > actionBar.offsetWidth){
			actionBarSpeed = 8;		//actionBar的移动度和方向
		}
		else if(target == actionBar.offsetWidth){
			return;
		}
		else if(target < actionBar.offsetWidth){
			actionBarSpeed = -8;
		}
		
		var timer = setInterval(function(){
			var actionBarPace = actionBar.offsetWidth + actionBarSpeed;

			if(Math.abs(actionBarPace - target) < 10){
				actionBarPace = target;
				clearInterval(timer);
			}
			actionBar.style.width = actionBarPace + 'px';
			actionBlock.style.left = actionBar.offsetWidth - (actionBlock.offsetWidth / 2) + 'px';
		},30);
	},

	/*	鼠标点击刻度滑动块自动滑动	*/
	clickSlide: function (actionBlock, actionBar, slideBar, total, barLength, showArea, flag) {
		var _this = this;
		slideBar.onclick = function(ev){
			var ev = ev || event;
		    //方法拓展，计算div 与 浏览器左边缘距离
			var osl = getPoint(slideBar);
			//var target = ev.clientX -  slideBar.offsetLeft;
			var target = ev.clientX - osl;

			if(target < 0){
				//表示鼠标已经超出那个范围
				target = 0;
			}
			if(target > barLength){
				target = barLength;
			}

			_this.checkAndMove(actionBlock, actionBar, target);
			if(showArea){

			    var blFlag = 0;//用于记录 方向拖拽的是左还是右
			    if (flag) {
                    var middle = barLength / 2;
                    if (target == middle) {
                        showArea.value = 0;
                        blFlag = 0;
                    } else if(target < middle) {
                        //左区域
                        target = middle - target;
                        showArea.value = Math.round(target);
                        blFlag = -1;
                    } else if (target > middle) {
                        //右边区域
                        target = target - middle;
                        showArea.value = Math.round(target);
                        blFlag = 1;
                    }
			    } else {
                    showArea.value = Math.round(target / barLength * total);
			    }

			    //发送命令
			    sendDiversionOrder(showArea, blFlag);
			}
		}
	},

	/*	鼠标按着拖动滑动条	*/
	drag: function (actionBlock, actionBar, total, barLength, showArea, flag) {
		/*	参数分别是点击滑动的那个块,滑动的距离,滑动条的最大数值,显示数值的地方(输入框)	*/
		actionBlock.onmousedown = function(ev) {
			var ev = ev || event;
			var thisBlock = this;
			var disX = ev.clientX;
			var currentLeft = thisBlock.offsetLeft;

			var blFlag = 0;//用于记录 方向拖拽的是左还是右

			document.onmousemove = function(ev) {
				var ev = ev || event;
				var left = ev.clientX - disX;

				if (currentLeft + left <= (barLength - thisBlock.offsetWidth / 2 ) && currentLeft + left >= 0 - thisBlock.offsetWidth / 2) {
					thisBlock.style.left = currentLeft + left + 'px';
					actionBar.style.width = currentLeft + left + (actionBlock.offsetWidth / 2) + 'px';
					if(showArea){

					    if (flag) {
                            var middle = barLength / 2;

                            if (actionBar.offsetWidth == middle) {
                                showArea.value = 0;
                                blFlag = 0;
                            } else if (actionBar.offsetWidth < middle) {
                                //左区域
                                var target = middle - actionBar.offsetWidth;
                                blFlag = -1;
                                showArea.value = Math.round(target);
                            } else if (actionBar.offsetWidth > middle) {
                                //右边区域
                                var target = actionBar.offsetWidth - middle;
                                showArea.value = Math.round(target);
                                blFlag = 1;
                            }
					    } else {
                            showArea.value = Math.round(actionBar.offsetWidth / barLength * total);
					    }
					}
				}
				return false;
			}

			document.onmouseup = function() {
				document.onmousemove = document.onmouseup = null;

			    //发送命令
				sendDiversionOrder(showArea, blFlag);
			}

			return false;
		}
	},

	getStyle : function(obj, attr){
		return obj.currentStyle?obj.currentStyle[attr]:getComputedStyle(obj)[attr];
	}
}

function getPoint(obj) { //获取某元素以浏览器左上角为原点的坐标  
    //var obj = document.getElementById("scroll-bar");
    var t = obj.offsetTop; //获取该元素对应父容器的上边距  
    var l = obj.offsetLeft; //对应父容器的上边距  
    //判断是否有父容器，如果存在则累加其边距  
    while (obj = obj.offsetParent) {//等效 obj = obj.offsetParent;while (obj != undefined)  
        t += obj.offsetTop; //叠加父容器的上边距  
        l += obj.offsetLeft; //叠加父容器的左边距  
    }
    //alert("top: " + t + " left: " + l);
    return l;
}

////每 1 秒钟，监控一次用户是否拖拽了滚动条控件
//function setSlideControl() {
//    //检查当前是否有命令 _TaskID _UavSetID 来自 D_Uav_ImageDemo.js
//    if (_TaskID == null || _UavSetID == null) {
//        return;
//    }

//    window.setInterval(function () {
//        sendDiversionOrder();
//    }, 1000);//每_SetTimeFly秒调用一次,系统代码设置 3s
//}

//发送指定命令
//showArea 方向角度值
//blFlag 主要用于 左右转向判断 0居中 -1左转 1右转

var _zoomIdNum = 0;//记录上一次变倍加减数值
function sendDiversionOrder(showArea, blFlag) {
    //检查是否有任务
    //if (_TaskID == null || _UavSetID == null) {
    //    showOPInfo(_Mlen_81); return;
    //}

    var sId = showArea.id;//根据名称判断用户具体操作了哪个滚动条
    var sVal =  parseInt(showArea.value);
    var directionName = "";//命令名称
    var resultInfo = "";//显示信息

    if (sId == "showArea") {//左右转向
        if (blFlag <=0) {
            directionName = "左调";
        } else {
            directionName = "右调";
        }

    } else if (sId == "showAreaUP") {//上下：只使用一个命令即可
        directionName = "下调";
    } else if (sId == "showAreaBB") {//变倍加减：接口缺少加减数值 
        if (sVal <= _zoomIdNum) {
            directionName = "变倍减";
        } else {
            directionName = "变倍加";
        }
        _zoomIdNum = sVal;//记录值
    }
    //请求发送命令
    $.ajax({
        url: _Datapath + "Busi/DUavControlCode.aspx?method=onCameraControl",
        data: { Type: directionName, TaskID: _TaskID, UavID: _UavSetID, Direction: sVal },
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                //在指令列表中输出信息
                //判断是否为 升高or下降
                if (directionName == "变倍加" || directionName == "变倍减") {
                    resultInfo = "云台控制" + directionName + ":命令发送成功" + "...";
                } else {
                    resultInfo = "云台控制" + directionName + ":" + sVal + "°命令发送成功" + "...";
                }

                showOPInfo(resultInfo);
            } else {
                if (isLoginOut(data) == false) {
                    if (directionName == "变倍加" || directionName == "变倍减") {
                        resultInfo = "云台控制" + directionName + ":命令发送失败" + "...";
                    } else {
                        resultInfo = "云台控制" + directionName + ":" + sVal + "°命令发送失败" + "...";
                    }
                    resultInfo = resultInfo + "\r\n" + "[ERR]: " + data.ErrMessage + "...";
                    showOPInfo(resultInfo);
                }
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            notify(_Mlen_6 + jqXHR.responseText);
        }
    });
}










