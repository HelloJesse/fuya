﻿
var lanFlag = getLanguageFolder();
var _Mlen_1 = "入库通知单编辑";
var _Mlen_2 = "请选择要作废的数据";
var _Mlen_3 = "确定作废选中记录？";
var _Mlen_4 = "提示";
var _Mlen_5 = "操作成功";
var _Mlen_6 = "操作失败:";
var _Mlen_7 = "请选择要打开的数据";
var _Mlen_8 = "请选择要操作的数据";
var _Mlen_9 = "请先保存入库通知单主档";
var _Mlen_10 = "确定要执行此操作？";
var _Mlen_11 = "收货单编辑";
var _Mlen_12 = "检查是否可以下推异常:";
var _Mlen_13 = "状态不是已输入,不可保存";
var _Mlen_14 = "保存明细成功";
var _Mlen_15 = "保存明细失败:";
var _Mlen_16 = "保存明细异常:";
var _Mlen_17 = "保存成功";
var _Mlen_18 = "保存失败:";
var _Mlen_19 = "没有找到操作的数据";
var _Mlen_20 = "入库单未确认,不可下推";
var _Mlen_21 = "下推数量不能大于入库数量";
var _Mlen_22 = "未找到有效的数据";
var _Mlen_23 = "登录超时,请重新登录.";
var _Mlen_24 = "正在加载....";
var _Mlen_25 = "正在处理....";
var _Mlen_26 = "请输入...";
var _Mlen_27 = "打印";
var _Mlen_28 = "确定要设置默认值？";
var _Mlen_29 = "确定？";
var _Mlen_30 = "请设置服务器日期格式为";
var _Mlen_31 = "编辑状态下不能设置默认值";
var _Mlen_32 = "帮助";
var _Mlen_33 = "页面 PageID 无效,请检查!";
var _Mlen_34 = "自定义列";
var _Mlen_35 = "无效.";
var _Mlen_36 = "提示信息";
var _Mlen_37 = "请先保存主档";
var _Mlen_38 = "单据未确认,不可下推";
var _Mlen_39 = "导入成功";
var _Mlen_40 = "导入失败";
var _Mlen_41 = "请输入商品条码或批次号";
var _Mlen_42 = "确定重新复核？";
var _Mlen_43 = "请先扫描承运单号或出库单号后进行复核";
var _Mlen_44 = "件数必须大于零";
var _Mlen_45 = "以下商品中复核数量与已拣数量不一致,是否继续完成操作";
var _Mlen_46 = "请输入或扫描运单号(出库单号)";
var _Mlen_47 = "该出库单或运单号已复核,是否再次复核";
var _Mlen_48 = "出库单中不包含该商品条码或批次号";
var _Mlen_49 = "分配收货区";
var _Mlen_50 = "以下商品中复核数量与已拣数量不一致,不能完成复核";
var _Mlen_51 = "复核数量必须大于零";
var _Mlen_52 = "下线数量不能大于等于上线数量";
var _Mlen_53 = "该出库单或运单号已称重";
var _Mlen_54 = "请输入耗材条码";
var _Mlen_55 = "重量必须为数字";
var _Mlen_56 = "请选择一条耗材数据";
var _Mlen_57 = "需求数量与已拣数量不一致,不能复核";
var _Mlen_58 = "请输入端口号";
var _Mlen_59 = "该商品不属于耗材";
var _Mlen_61 = "下线数量必须大于等于0";
var _Mlen_62 = "上线数量必须大于等于0";
var _Mlen_63 = "请先设置属性列信息";
var _Mlen_64 = "业务单状态不是已输入,不可引入库存";
var _Mlen_65 = "商品不能为空";
var _Mlen_66 = "获取主档ID失败";
var _Mlen_67 = "获取明细ID失败";
var _Mlen_68 = "批次属性保存成功";
var _Mlen_69 = "确定要执行删除操作";
var _Mlen_70 = "请选择要接受的任务";
var _Mlen_71 = "安排任务只能选择一条任务单";
var _Mlen_72 = "请输入拒绝接受任务的原因";
var _Mlen_73 = "请选择基站信息";
var _Mlen_74 = "任务单只有是已接受状态,才可安排任务";
var _Mlen_75 = "用户类型为基站时,必须选择用户所属基站";
var _Mlen_76 = "用户类型为客户时,必须选择用户所属客户";
var _Mlen_77 = "请先规划巡航任务";
var _Mlen_78 = "请先选择要显示的飞机航线";
var _Mlen_79 = "盘旋圈数必须为数字";
var _Mlen_80 = "请先选择已安排任务的飞机";
var _Mlen_81 = "没有检测到需要监控的任务!";
var _Mlen_82 = "海图尚未加载完成,请稍后点击!";
var _Mlen_83 = "任务或飞机为空,不允许操作航线";
var _Mlen_84 = "请输入航点高度";
var _Mlen_85 = "请输入正确格式的航点高度";
var _Mlen_86 = "确定从新上传航线!";
var _Mlen_87 = "没有需要上传的航线信息!";
var _Mlen_88 = "已安排的任务未最终确认,操作已返回!";
var _Mlen_89 = "请输入正确的航线航点信息!";
var _Mlen_90 = "请输入手机号码!";
var _Mlen_91 = "请输入串口号!";
var _Mlen_92 = "移动信息不能为空";
var _Mlen_93 = "速度不能为空";
var _Mlen_94 = "云台控制尚未登陆成功,请检查";
var _Mlen_95 = "已经返航,不允许在添加新跟踪目标";
var _Mlen_96 = "任务状态不是已完成，不允许下载航线";
var _Mlen_97 = "定点盘旋，经度和纬度不能为空,请检查!";
var _Mlen_98 = "必须是巡航任务，才允许切入指定的航线航点!";

if(lanFlag == "en/")
{
    //英文
    _Mlen_1 = "IN WAREHOUSE EDIT";
    _Mlen_2 = "PLEASE SELECT DATA TO EDIT";
    _Mlen_3 = "Be sure to perform this operation？";
    _Mlen_4 = "PROMPT";
    _Mlen_5 = "UPDATE SUCCESSFULLY";
    _Mlen_6 = "UPDATE FAILED";
    _Mlen_7 = "PLEASE SELECT DATA TO EDIT";
    _Mlen_8 = "PLEASE SELECT DATA TO EDIT";
    _Mlen_9 = "Please save the single file storage notice";
    _Mlen_10 = "Be sure to perform this operation？";
    _Mlen_11 = "RECEIVER EDIT";
    _Mlen_12 = "Abnormal:";
    _Mlen_13 = "Status is not already entered, can not be saved";
    _Mlen_14 = "SAVE DETAILS SUCCESSFULLY";
    _Mlen_15 = "SAVE DETAIL FAILED";
    _Mlen_16 = "SAVE DETAIL EXCEPTION:";
    _Mlen_17 = "SAVE SUCCESSFULLY";
    _Mlen_18 = "SAVE FAILED";
    _Mlen_19 = "No data found";
    _Mlen_20 = "Status Is Not confirmed, can not be pushed down";
    _Mlen_21 = "Push down the number can not be greater than the number of storage";
    _Mlen_22 = "No valid data found";
    _mlen_23 = "Login timeout, please sign in again.";
    _Mlen_24 = "Loading....";
    _Mlen_25 = "Processing....";
    _Mlen_26 = "Please enter...";
    _Mlen_27 = "Print";
    _Mlen_28 = "To set the default value？";
    _Mlen_29 = "Confrm？";
    _Mlen_30 = "Please set the server date format";
    _Mlen_31 = "Edit state can not set the default value";
    _Mlen_32 = "Help";
    _Mlen_33 = "PageID invalid. Please check!";
    _Mlen_34 = "Customize columns";
    _Mlen_35 = "Invalid.";
    _Mlen_36 = "Prompt Info";
    _Mlen_37 = "Please save Master";
    _Mlen_38 = "Unconfirmed documents, can not be pushed down";
    _Mlen_39 = "IMPORT SUCCESSFUL";
    _Mlen_40 = "IMPORT FAILED";
    _Mlen_41 = "Please input Goods Code";
    _Mlen_42 = "Be sure to refresh check？";
    _Mlen_43 = "Please scan Stock_Out_No first";
    _Mlen_44 = "The Number can't be zero or empty";
    _Mlen_45 = "The follow Goods are not same between PickQty and CheckPicQty";
    _Mlen_46 = "Please input or scan Stock_Out_No";
    _Mlen_47 = "This Bill has been Checked,if Check again";
    _Mlen_48 = "Not found GoodCode in this Bill";
    _Mlen_49 = "Distribution receiving areas";
    _Mlen_50 = "The follow Goods are not same between PickQty and CheckPicQty";
    _Mlen_51 = "The CheckPicQty can't be empty or zero";
    _Mlen_52 = "";
    _Mlen_53 = "This Bill has been weighted";
    _Mlen_54 = "Please enter the Consumptive barcode";
    _Mlen_55 = "The weight must be a number";
    _Mlen_56 = "Please select a Consumptive row";
    _Mlen_57 = "The Needs Number not equal picked Number,Can't continue check";
    _Mlen_58 = "Please enter the Port number";
    _Mlen_59 = "This barcode is not a Consumptive barcode";
    _Mlen_61 = "LLimit must be larger than 0";
    _Mlen_62 = "ULimit must be larger than 0";
    _Mlen_63 = "Please set the propertylist info first";
    _Mlen_64 = "Status is not already entered, can not be import Inverty";
    _Mlen_65 = "the goods can not be empty";
    _Mlen_66 = "Failed to get the MainID";
    _Mlen_67 = "Failed to get the DTLID";
    _Mlen_68 = "Successed to save the property info";
    _Mlen_69 = "Sure to delete this Detail?";
    _Mlen_70 = "";
    _Mlen_71 = "";
    _Mlen_72 = "";
    _Mlen_73 = "";
    _Mlen_74 = "";
    _Mlen_75 = "";
    _Mlen_76 = "";
    _Mlen_77 = "";
    _Mlen_78 = "";
    _Mlen_79 = "";
    _Mlen_80 = "";
    _Mlen_81 = "";
    _Mlen_82 = "";
    _Mlen_83 = "";
    _Mlen_84 = "";
    _Mlen_85 = "";
    _Mlen_86 = "";
    _Mlen_87 = "";
    _Mlen_88 = "";
    _Mlen_89 = "";
    _Mlen_90 = "";
    _Mlen_91 = "";
    _Mlen_92 = "";
    _Mlen_93 = "";
    _Mlen_94 = "";
    _Mlen_95 = "";
    _Mlen_96 = "";
    _Mlen_97 = "";
}