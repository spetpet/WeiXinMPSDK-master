using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Senparc.Weixin.MP.Sample.CommonService.QyMessageHandler;
using Senparc.Weixin.QY.Entities;
using Senparc.Weixin.QY.MessageHandlers;
using System.Data;
using System.Data.OleDb;
using System.Collections;
using System.Drawing;
using Senparc.Weixin.MP;

namespace Senparc.Weixin.MP.Sample.CommonService.QyMessageHandlers
{
    public class QyCustomMessageHandler : QyMessageHandler<QyCustomMessageContext>
    {
        public static string oraconnectstr = "Provider=OraOLEDB.Oracle;Data Source=wmrdc;Persist Security Info=True;User ID=wmrdc;Password=wmrdc;Unicode=True";
        public static string sqlconnectstr = "Provider=sqloledb;Data Source=173.2.28.1;Persist Security Info=True;Password=Sunray!%(#%&;User ID=sa;Initial Catalog=secc";
        public static string sqlconnectstr153 = "Provider=sqloledb;Data Source=173.5.28.153;Persist Security Info=True;Password=sa123456;User ID=sa;Initial Catalog=MA";
        public static string sqlconnectstrZGZY = "Provider=sqloledb;Data Source=173.5.28.153;Persist Security Info=True;Password=sa123456;User ID=sa;Initial Catalog=ZGZY";
        public OleDbConnection oraconn = new OleDbConnection(oraconnectstr);
        public OleDbConnection sqlconn = new OleDbConnection(sqlconnectstr);
        public OleDbConnection sqlconn153 = new OleDbConnection(sqlconnectstr153);
        public OleDbConnection sqlconnZGZY = new OleDbConnection(sqlconnectstrZGZY);
        
        
        
        
        public QyCustomMessageHandler(Stream inputStream, PostModel postModel, int maxRecordCount = 0)
            : base(inputStream, postModel, maxRecordCount)
        {
        }

       
        public override IResponseMessageBase OnTextRequest(RequestMessageText requestMessage)
        {
            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            var responsenews = base.CreateResponseMessage<ResponseMessageNews>();

            if (CurrentMessageContext.StorageData == null)
            {
                CurrentMessageContext.StorageData = "nothing";
            }
           //responseMessage.Content = "您发送了消息：" + requestMessage.Content;

            switch (CurrentMessageContext.StorageData.ToString())
            {
            #region 删码
		     case "gsp_del":
                    string sql_gsp = "update c_gsp_nbr_trkg c set c.stat_code=90 where c.create_date_time>sysdate-2 and c.cntr_nbr='"+requestMessage.Content.ToString()+"'";
                    OleDbCommand oracommand_gsp_del = new OleDbCommand(sql_gsp, oraconn);
                    oraconn.Open();
                    int del_rows = 9999;
                    del_rows = oracommand_gsp_del.ExecuteNonQuery();
                    //oracommand_gsp_del.Transaction.Commit();
                    oraconn.Close();
                    if (del_rows == 0)
                        responseMessage.Content = "没有该货箱号或货箱记录中不存在有效的药监码";
                    else
                        if (del_rows == 9999)
                            responseMessage.Content = "删除程序出错";
                        else
                            responseMessage.Content = "共删除药监码"+del_rows.ToString()+"条";
                    //responseMessage.Content = CurrentMessageContext.StorageData.ToString();
                    (CurrentMessageContext.StorageData) = "nothing";
                    break; 
	              #endregion

            #region 签到
            case "sign_in":
                    //responseMessage.Content = requestMessage.FromUserName.ToString() + requestMessage.Content.ToString() + DateTime.Now.ToString();
                    string sql_sign_insert = "INSERT into tbCheckin(user_id,sign_date_time,memo) VALUES('" + requestMessage.FromUserName.ToString() + "','" + DateTime.Now.ToString() + "','" + requestMessage.Content.ToString() + "')";
                    //string sql_sign_insert = "INSERT into tbCheckin(user_id,sign_date_time,memo) VALUES(" + requestMessage.FromUserName.ToString() + ",'2015/3/10 11:06:05'," + requestMessage.Content.ToString() + ")";
                    OleDbCommand sqlcommand_sign_insert = new OleDbCommand(sql_sign_insert,sqlconnZGZY);
                    sqlconnZGZY.Open();
                    int success_rows = 9999;
                    success_rows = sqlcommand_sign_insert.ExecuteNonQuery();
                    sqlconnZGZY.Close();
                    if (success_rows == 0)
                        responseMessage.Content = "很抱歉，签到由于网络延时或系统原因并未成功，请稍候再试。";
                    else
                        if (success_rows == 9999)
                            responseMessage.Content = "签到程序出错!";
                        else
                            responseMessage.Content = "签到已成功，谢谢。";

                    CurrentMessageContext.StorageData = "nothing";
                    break; 
            #endregion

            #region 百特提交步骤
            case "btadd1":
                    responseMessage.Content = "请输入该日期的库存托数数量，如：11000";
                    (CurrentMessageContext.StorageData) = "btadd2";
                    break;
            case "btadd2":
                    responseMessage.Content = "请输入该日期的入库托数数量，如：670";
                    (CurrentMessageContext.StorageData) = "btadd3";
                    break;
            case "btadd3":
                    responseMessage.Content = "请输入该日期的出库托数数量，如：680";
                    (CurrentMessageContext.StorageData) = "btadd4";
                    break;
            case "btadd4":
                    responseMessage.Content = "数据日期：" + (CurrentMessageContext.RequestMessages[CurrentMessageContext.RequestMessages.Count - 4] as RequestMessageText).Content + "\r\n库存托数：" + (CurrentMessageContext.RequestMessages[CurrentMessageContext.RequestMessages.Count - 3] as RequestMessageText).Content + "\r\n入库托数：" + (CurrentMessageContext.RequestMessages[CurrentMessageContext.RequestMessages.Count - 2] as RequestMessageText).Content + "\r\n出库托数：" + requestMessage.Content + "\r\n确定请输入1，退出请输入0";
                    (CurrentMessageContext.StorageData) = "btadd5";
                    break;
            case "btadd5":
                    if (requestMessage.Content == "1")
                    {
                        string tempbtdate = (CurrentMessageContext.RequestMessages[CurrentMessageContext.RequestMessages.Count - 5] as RequestMessageText).Content;
                        string tempbtinv = (CurrentMessageContext.RequestMessages[CurrentMessageContext.RequestMessages.Count - 4] as RequestMessageText).Content;
                        string tempbtin = (CurrentMessageContext.RequestMessages[CurrentMessageContext.RequestMessages.Count - 3] as RequestMessageText).Content;
                        string tempbtout = (CurrentMessageContext.RequestMessages[CurrentMessageContext.RequestMessages.Count - 2] as RequestMessageText).Content;
                        string tempsqldate = tempbtdate.Substring(0, 4) + "-" + tempbtdate.Substring(4, 2) + "-" + tempbtdate.Substring(6, 2) + " 00:00:00";
                        //responseMessage.Content = tempsqldate + tempbtdate + tempbtinv + tempbtin + tempbtout;

                        string sql1 = "select COUNT(*) from baxter_inv where CONVERT(varchar(10),recdate , 112)='" + tempbtdate + "'";
                        OleDbCommand oracommand1 = new OleDbCommand(sql1, sqlconn153);
                        sqlconn153.Open();
                        string rowcount = oracommand1.ExecuteScalar().ToString();
                        oracommand1.Dispose();
                        sqlconn153.Close();
                        //responseMessage.Content = rowcount;
                        if (rowcount == "0")
                        {
                            string sql11 = "insert into baxter_inv(recdate,broadnum,innum,outnum) values('" + tempsqldate + "'," + tempbtinv + "," + tempbtin + "," + tempbtout + ")";
                            //string sql11 = "insert into baxter_inv(recdate,broadnum,innum,outnum) values('2014-05-05 00:00:00',1,1,1)";
                            OleDbCommand oracommand11 = new OleDbCommand(sql11, sqlconn153);
                            sqlconn153.Open();
                            int rows = oracommand11.ExecuteNonQuery();
                            if (rows != 0)
                            {
                                responseMessage.Content = "提交成功，流程结束";
                            }
                            else { responseMessage.Content = "所提交的数据提交失败，请注意输入格式后，输入“提交”重新进行！"; }
                            oracommand11.Dispose();
                            sqlconn153.Close();


                        }
                        else
                        {

                            responseMessage.Content = "所提交的数据日期已存在，请查正后再输入“提交”重新进行！";
                        }
                    }
                    else
                    {
                        responseMessage.Content = "提交流程已退出";

                    }


                    CurrentMessageContext.StorageData = "nothing";
                    break;
            #endregion


                default:
                
                     #region 文字查询
                        switch (requestMessage.Content)
                        {
                       
                            #region QQ群
                            case "群":


                                responseMessage.Content = "请<a href=\"http://shang.qq.com/wpa/qunwpa?idkey=430baa22ac2d663687f8003c5583086975f6886055a88cedc5e8f4303465ba5a/\">点击进入QQ群</a>";

                                break;
                            #endregion

                            #region 百特库存
                            case "百特":

                    


                                    //responseMessage.Content = "请点击<a herf=\"http://211.0.97.4/myweixin/baxter.aspx\">百特详细库存查询</a>";
                                    responsenews.Articles.Add(new Article()
                                    {
                                        Title = "查询百特历史数据请点击这里进入",
                                        Description = "百特库存统计查询",
                                        PicUrl = "http://211.97.0.4/myweixin/imgs/b.jpg",
                                        Url = "http://211.97.0.4/myweixin/baxter.aspx"

                                    });
                                    return responsenews;
                   
                                break;
                            #endregion

                            #region 库存
                            case "库存":
                                    string sql_kc = "select im.season,sum(ceil(si.QTY_ON_HAND/case im.std_case_qty when 0 then 99999 else nvl(im.std_case_qty,999999) end)) from sku_invn si left join item_master im on im.sku_id =si.sku_id where si.WHSE='S00' group by im.season order by im.season";
                                    OleDbCommand oracommand_kc = new OleDbCommand(sql_kc, oraconn);
                                    oraconn.Open();
                                    OleDbDataReader orareader = oracommand_kc.ExecuteReader();
                                    string resu_kc = "";
                                    int total_kc = 0;
                                    while (orareader.Read())
                                    {
                                        total_kc += Convert.ToInt32(orareader[1].ToString());
                                        resu_kc += orareader[0].ToString() + "货主：" + orareader[1].ToString() + "托\r\n";
                                    }
                                    orareader.Close();
                                    oraconn.Close();
                                    resu_kc += "全部库存共：" + total_kc.ToString() + "托";
                                    responseMessage.Content = resu_kc;
                   

                                CurrentMessageContext.StorageData = "noting";
                                break;
                            #endregion

                            #region 订单
                            case "订单":

                   
                                    //responseMessage.Content = "test";
                                    string resu_order = "";

                                    string sqlout = "select im.season,sum(ceil(pd.pkt_qty/im.std_pack_qty)) from pkt_hdr ph inner join pkt_dtl pd on pd.pkt_ctrl_nbr=ph.pkt_ctrl_nbr left join item_master im on im.sku_id=pd.sku_id where to_char(ph.create_date_time,'yymmdd')=to_char(sysdate,'yymmdd') and ph.whse='S00' and ph.pkt_sfx is not null group by im.season";
                                    OleDbCommand oracommandout = new OleDbCommand(sqlout, oraconn);
                                    oraconn.Open();
                                    OleDbDataReader orareader_out = oracommandout.ExecuteReader();
                                    resu_order += "出库：\r\n";
                                    int total_out = 0;
                                    while (orareader_out.Read())
                                    {
                                        total_out += Convert.ToInt32(orareader_out[1].ToString());
                                        resu_order += orareader_out[0].ToString() + "货主：" + orareader_out[1].ToString() + "件\r\n";
                                    }
                                    orareader_out.Close();
                                    oraconn.Close();
                                    resu_order += "全部出库共：" + total_out.ToString() + "件\r\n";

                                    string sqlin = "select ad.batch_nbr,sum(ceil(ad.units_rcvd/im.std_pack_qty)) from asn_dtl ad left join asn_hdr ah on ah.shpmt_nbr=ad.shpmt_nbr left join item_master im on im.sku_id=ad.sku_id where to_char(ah.first_rcpt_date_time,'yymmdd')=to_char(sysdate,'yymmdd') and ah.to_whse='S00' group by ad.batch_nbr";
                                    OleDbCommand oracommandin = new OleDbCommand(sqlin, oraconn);
                                    oraconn.Open();
                                    OleDbDataReader orareaderin = oracommandin.ExecuteReader();
                                    int incount = 0;
                                    int intotal = 0;

                                    resu_order += "入库:\r\n";
                                    while (orareaderin.Read())
                                    {
                                        intotal += Convert.ToInt32(orareaderin[1].ToString());
                                        incount += 1;
                                    }
                                    orareaderin.Close();
                                    resu_order += "全部入库" + incount.ToString() + "个批次，共：" + intotal.ToString() + "件\r\n";
                                    oraconn.Close();
                                    responseMessage.Content = resu_order;
                    

                                CurrentMessageContext.StorageData = "noting";
                                break;
                            #endregion

                            #region 温度
                            case "温度":

                   
                                    //responseMessage.Content = "test";
                                    string sql = "select area,convert(decimal(15,2),AVG(temperature_value)) 平均温度,convert(decimal(15,2),AVG(humidity_value)) 平均湿度 from (select case left(node_barcode,(len(node_barcode)-3)) when '1F' then '1F控温仓库' when '2F' then '2F控温仓库' when '3F' then '3F常温仓库' when '3Ffreez' then '3F冷冻库' when '3Fcold' then '3F冷藏库' end area,temperature_value,humidity_value from secc.dbo.warehouse_temperature_data where id in (SELECT MAX(id) id FROM [secc].[dbo].[warehouse_temperature_data] where created_date>=dateadd(day,-1,GETDATE())group by node_barcode)) t where area is not null group by area order by area";

                                    OleDbCommand sqlcommand = new OleDbCommand(sql, sqlconn);
                                    sqlconn.Open();
                                    OleDbDataReader sqlreader = sqlcommand.ExecuteReader();
                                    string resu = "";
                                    //int total = 0;
                                    while (sqlreader.Read())
                                    {
                                        //total += Convert.ToInt32(orareader[1].ToString());
                                        // Convert.ToInt16(sqlreader[1].ToString()
                                        resu += sqlreader[0].ToString() + "\r\n温度：" + sqlreader[1].ToString() + "度\r\n湿度：" + sqlreader[2].ToString() + "%\r\n状态：正常\r\n";
                                    }
                                    sqlreader.Close();
                                    sqlconn.Close();
                                    //resu += "全部库存共：" + total.ToString() + "托";
                                    responseMessage.Content = resu;
                   

                                CurrentMessageContext.StorageData = "noting";
                                break;
                            #endregion

                            #region 卡单
                            case "卡单":

                   
                                    //responseMessage.Content = "test";
                                    string resu_kd = "";

                                    string sql_kd = "select ipd.pkt_ctrl_nbr,mg.msg from inpt_pkt_dtl ipd left join msg_log mg on mg.ref_value_1=ipd.error_seq_nbr left join inpt_pkt_hdr iph on iph.pkt_ctrl_nbr=ipd.pkt_ctrl_nbr where iph.whse='S00'";
                                    OleDbCommand oracommand_kd = new OleDbCommand(sql_kd, oraconn);
                                    oraconn.Open();
                                    OleDbDataReader orareader_kd = oracommand_kd.ExecuteReader();

                                    int total_kd = 0;
                                    while (orareader_kd.Read())
                                    {
                                        total_kd += 1;
                                        resu_kd += "单号：" + orareader_kd[0].ToString() + "错误：" + orareader_kd[1].ToString() + "\r\n";
                                    }
                                    orareader_kd.Close();
                                    oraconn.Close();
                                    resu_kd = "共有" + total_kd.ToString() + "单卡单！！\r\n" + resu_kd;
                                    responseMessage.Content = resu_kd;
                   

                                CurrentMessageContext.StorageData = "noting";
                                break;
                            #endregion

                            #region 百特提交
                            case "提交":

                                CurrentMessageContext.StorageData = "btadd1";

                                responseMessage.Content = "请输入数据日期（YYYYMMDD）如：20140505";

                                break;
                            #endregion

               

                

                            default:
                    
                                    string sql3 = "select ph.pkt_nbr,im.sku_desc,pd.orig_pkt_qty,pd.stat_code from pkt_hdr ph inner join pkt_dtl pd on pd.pkt_ctrl_nbr=ph.pkt_ctrl_nbr left join item_master im on im.sku_id=pd.sku_id where ph.whse='S00' and ph.pkt_sfx is not null and (ph.pkt_ctrl_nbr='" + requestMessage.Content + "' or ph.pkt_nbr='" + requestMessage.Content + "') ";
                                    OleDbCommand oracommand3 = new OleDbCommand(sql3, oraconn);
                                    oraconn.Open();
                                    OleDbDataReader orareader3 = oracommand3.ExecuteReader();
                                    string resu3 = "";
                                    //int total3 = 0;
                                    while (orareader3.Read())
                                    {
                                        //total3 += Convert.ToInt32(orareader3[2].ToString());
                                        string tempstat = "无";
                                        switch (orareader3[3].ToString())
                                        {
                                            case "40":
                                                tempstat = "包装完成";
                                                break;
                                            case "10":
                                                tempstat = "未选择";
                                                break;
                                            case "20":
                                                tempstat = "已打印";
                                                break;
                                            case "90":
                                                tempstat = "已关车";
                                                break;
                                            case "35":
                                                tempstat = "包装中";
                                                break;
                                            default:
                                                tempstat = "无";
                                                break;


                                        }
                                        resu3 += "品种：" + orareader3[1].ToString() + "\r\n数量：" + orareader3[2].ToString() + "\r\n状态：" + tempstat + "\r\n";
                                        //resu3 += "test";
                                    }
                                    orareader3.Close();
                                    oraconn.Close();
                                    //resu3 += "全部出库共：" + total3.ToString() + "件";
                                    responseMessage.Content = resu3;
                    

                                    var result = new StringBuilder();
                                    result.AppendFormat("您刚才发送了文字信息：   ‘{0}’未能识别\r\n查询请输入\r\n“库存”查库存情况\r\n“温度”查仓库温湿度情况\r\n“订单”查订单完成情况\r\n直接输入拣货单号或订单号查具体订单情况，谢谢", requestMessage.Content);

                                    responseMessage.Content = result.ToString();
                    

                                CurrentMessageContext.StorageData = "noting";

                                break;
                        }
                        #endregion
                       
                        break;
            }
            return responseMessage;
        }


        public override IResponseMessageBase OnEvent_ViewRequest(RequestMessageEvent_View requestMessage)
        {
            //return base.OnEvent_ViewRequest(requestMessage);
            return null;
        }


        public override IResponseMessageBase OnEvent_ClickRequest(RequestMessageEvent_Click requestMessage)
        {
            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();

            #region 菜单反馈
            switch (requestMessage.EventKey)
            {

                #region QQ群
                case "群":


                    responseMessage.Content = "请<a href=\"http://shang.qq.com/wpa/qunwpa?idkey=430baa22ac2d663687f8003c5583086975f6886055a88cedc5e8f4303465ba5a/\">点击进入QQ群</a>";

                    break;
                #endregion

                #region 库存
                case "库存":
                    string sql_kc = "select im.season,sum(ceil(si.QTY_ON_HAND/case im.std_case_qty when 0 then 99999 else nvl(im.std_case_qty,999999) end)) from sku_invn si left join item_master im on im.sku_id =si.sku_id where si.WHSE='S00' group by im.season order by im.season";
                    OleDbCommand oracommand_kc = new OleDbCommand(sql_kc, oraconn);
                    oraconn.Open();
                    OleDbDataReader orareader = oracommand_kc.ExecuteReader();
                    string resu_kc = "";
                    int total_kc = 0;
                    while (orareader.Read())
                    {
                        total_kc += Convert.ToInt32(orareader[1].ToString());
                        resu_kc += orareader[0].ToString() + "货主：" + orareader[1].ToString() + "托\r\n";
                    }
                    orareader.Close();
                    oraconn.Close();
                    resu_kc += "全部库存共：" + total_kc.ToString() + "托";
                    responseMessage.Content = resu_kc;


                    CurrentMessageContext.StorageData = "noting";
                    break;
                #endregion

                #region 订单
                case "订单":


                    //responseMessage.Content = "test";
                    string resu_order = "";

                    string sqlout = "select im.season,sum(ceil(pd.pkt_qty/im.std_pack_qty)) from pkt_hdr ph inner join pkt_dtl pd on pd.pkt_ctrl_nbr=ph.pkt_ctrl_nbr left join item_master im on im.sku_id=pd.sku_id where to_char(ph.create_date_time,'yymmdd')=to_char(sysdate,'yymmdd') and ph.whse='S00' and ph.pkt_sfx is not null group by im.season";
                    OleDbCommand oracommandout = new OleDbCommand(sqlout, oraconn);
                    oraconn.Open();
                    OleDbDataReader orareader_out = oracommandout.ExecuteReader();
                    resu_order += "出库：\r\n";
                    int total_out = 0;
                    while (orareader_out.Read())
                    {
                        total_out += Convert.ToInt32(orareader_out[1].ToString());
                        resu_order += orareader_out[0].ToString() + "货主：" + orareader_out[1].ToString() + "件\r\n";
                    }
                    orareader_out.Close();
                    oraconn.Close();
                    resu_order += "全部出库共：" + total_out.ToString() + "件\r\n";

                    string sqlin = "select ad.batch_nbr,sum(ceil(ad.units_rcvd/im.std_pack_qty)) from asn_dtl ad left join asn_hdr ah on ah.shpmt_nbr=ad.shpmt_nbr left join item_master im on im.sku_id=ad.sku_id where to_char(ah.first_rcpt_date_time,'yymmdd')=to_char(sysdate,'yymmdd') and ah.to_whse='S00' group by ad.batch_nbr";
                    OleDbCommand oracommandin = new OleDbCommand(sqlin, oraconn);
                    oraconn.Open();
                    OleDbDataReader orareaderin = oracommandin.ExecuteReader();
                    int incount = 0;
                    int intotal = 0;

                    resu_order += "入库:\r\n";
                    while (orareaderin.Read())
                    {
                        intotal += Convert.ToInt32(orareaderin[1].ToString());
                        incount += 1;
                    }
                    orareaderin.Close();
                    resu_order += "全部入库" + incount.ToString() + "个批次，共：" + intotal.ToString() + "件\r\n";
                    oraconn.Close();
                    responseMessage.Content = resu_order;


                    CurrentMessageContext.StorageData = "noting";
                    break;
                #endregion

                #region 温度
                case "温度":


                    //responseMessage.Content = "test";
                    string sql = "select area,convert(decimal(15,2),AVG(temperature_value)) 平均温度,convert(decimal(15,2),AVG(humidity_value)) 平均湿度 from (select case left(node_barcode,(len(node_barcode)-3)) when '1F' then '1F控温仓库' when '2F' then '2F控温仓库' when '3F' then '3F常温仓库' when '3Ffreez' then '3F冷冻库' when '3Fcold' then '3F冷藏库' end area,temperature_value,humidity_value from secc.dbo.warehouse_temperature_data where id in (SELECT MAX(id) id FROM [secc].[dbo].[warehouse_temperature_data] where created_date>=dateadd(day,-1,GETDATE())group by node_barcode)) t where area is not null group by area order by area";

                    OleDbCommand sqlcommand = new OleDbCommand(sql, sqlconn);
                    sqlconn.Open();
                    OleDbDataReader sqlreader = sqlcommand.ExecuteReader();
                    string resu = "";
                    //int total = 0;
                    while (sqlreader.Read())
                    {
                        //total += Convert.ToInt32(orareader[1].ToString());
                        // Convert.ToInt16(sqlreader[1].ToString()
                        resu += sqlreader[0].ToString() + "\r\n温度：" + sqlreader[1].ToString() + "度\r\n湿度：" + sqlreader[2].ToString() + "%\r\n状态：正常\r\n";
                    }
                    sqlreader.Close();
                    sqlconn.Close();
                    //resu += "全部库存共：" + total.ToString() + "托";
                    responseMessage.Content = resu;


                    CurrentMessageContext.StorageData = "noting";
                    break;
                #endregion

                #region 卡单
                case "卡单":


                    //responseMessage.Content = "test";
                    string resu_kd = "";

                    string sql_kd = "select ipd.pkt_ctrl_nbr,mg.msg from inpt_pkt_dtl ipd left join msg_log mg on mg.ref_value_1=ipd.error_seq_nbr left join inpt_pkt_hdr iph on iph.pkt_ctrl_nbr=ipd.pkt_ctrl_nbr where iph.whse='S00'";
                    OleDbCommand oracommand_kd = new OleDbCommand(sql_kd, oraconn);
                    oraconn.Open();
                    OleDbDataReader orareader_kd = oracommand_kd.ExecuteReader();

                    int total_kd = 0;
                    while (orareader_kd.Read())
                    {
                        total_kd += 1;
                        resu_kd += "单号：" + orareader_kd[0].ToString() + "错误：" + orareader_kd[1].ToString() + "\r\n";
                    }
                    orareader_kd.Close();
                    oraconn.Close();
                    resu_kd = "共有" + total_kd.ToString() + "单卡单！！\r\n" + resu_kd;
                    responseMessage.Content = resu_kd;


                    CurrentMessageContext.StorageData = "noting";
                    break;
                #endregion

                #region 删码
                case "删码":
                    responseMessage.Content = "请输入您要删除药监码的货箱号";
                    CurrentMessageContext.StorageData = "gsp_del";
                    break; 
                #endregion

                #region 冷库查询签到
                case "签到":


                    //responseMessage.Content = "test";
                    string sql_sin = "SELECT right(wz.node_barcode,2) node_name,wz.temperature_value tem,wz.humidity_value hum from warehouse_zigbee wz where wz.node_barcode LIKE '3Fcold%' ORDER BY wz.node_barcode";

                    OleDbCommand sqlcommand_sin = new OleDbCommand(sql_sin, sqlconn);
                    sqlconn.Open();
                    OleDbDataReader sqlreader_sin = sqlcommand_sin.ExecuteReader();
                    string resu_sin = "冷库各点温湿度：\r\n";
                    //int total = 0;
                    while (sqlreader_sin.Read())
                    {
                        //total += Convert.ToInt32(orareader[1].ToString());
                        // Convert.ToInt16(sqlreader[1].ToString()
                        resu_sin += "编号："+sqlreader_sin[0].ToString() + "  温度：" + sqlreader_sin[1].ToString() + "度   湿度：" + sqlreader_sin[2].ToString() + "%\r\n";
                    }
                    sqlreader_sin.Close();
                    sqlconn.Close();
                    //resu += "全部库存共：" + total.ToString() + "托";
                    resu_sin += "请回复检查结果以便完成签到，如回复“正常”等，谢谢。";
                    responseMessage.Content = resu_sin;


                    CurrentMessageContext.StorageData = "sign_in";
                    break;
                #endregion

               





                default:

                      CurrentMessageContext.StorageData = "noting";

                    break;
            }
            #endregion

            return responseMessage;

        }

        public override IResponseMessageBase OnEvent_ScancodeWaitmsgRequest(RequestMessageEvent_ScancodeWaitmsg requestMessage)
        {
            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "sacn_wait";
            return responseMessage;
        }

        public override IResponseMessageBase OnEvent_ScancodePushRequest(RequestMessageEvent_ScancodePush requestMessage)
        {
            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "sacn_push";
            return responseMessage;
        }

       

        public override QY.Entities.IResponseMessageBase DefaultResponseMessage(QY.Entities.IRequestMessageBase requestMessage)
        {
            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "抱歉，未能识别你的输入";
            return responseMessage;
        }
    }
}
