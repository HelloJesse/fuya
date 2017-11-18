using NPOI.HSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using NPOI.HSSF.Util;


namespace VLP.BS
{
    public class ExportExcel
    {
        /// <summary>
        /// DataTable返回excel对应的内存流
        /// </summary>
        /// <param name="t">数据table</param>
        /// <param name="tableName">表名称</param>
        /// <param name="file">返回的流</param>
        /// <returns></returns>
        public static MemoryStream DataExportExcelStream(DataTable t, string tableName)
        {
            if (t == null)
            {
                throw new Exception("传入的数据表为null");
            }
            HSSFWorkbook hssfworkbook = new HSSFWorkbook();
            HSSFSheet sheet1 = hssfworkbook.CreateSheet(tableName);//"Sheet1"
            System.Web.UI.HtmlControls.HtmlTable extab = new System.Web.UI.HtmlControls.HtmlTable();
            extab.Border = 1;
            extab.BorderColor = "Black";
            extab.CellPadding = 1;
            extab.CellSpacing = 1;

            HSSFRow headrow = sheet1.CreateRow(0);
            int colindx = 0;
            //int rowindx = 0;
            HSSFCellStyle style1 = hssfworkbook.CreateCellStyle();
            HSSFFont font = hssfworkbook.CreateFont();
            font.Boldweight = HSSFFont.BOLDWEIGHT_BOLD;
            style1.SetFont(font);
            foreach (DataColumn col in t.Columns)
            {
                HSSFCell cell = headrow.CreateCell(colindx);
                cell.SetCellValue(col.ColumnName);
                //cell.CellStyle.SetFont(font);
                cell.CellStyle = style1;
                colindx++;
            }
            colindx = 0;
            MemoryStream file = new MemoryStream();
            hssfworkbook.Write(file);
            return file;


        }

        /// <summary>
        /// DataTable返回对应的byte[]
        /// </summary>
        /// <param name="t">数据table</param>
        /// <param name="tableName">表名称</param>
        /// <returns>byte[]</returns>
        public static byte[] DataExportExcelByte(DataTable t, string tableName)
        {
            MemoryStream file = DataExportExcelStream(t, tableName);
            file.Position = 0;
            byte[] bytes = new byte[file.Length];
            file.Read(bytes, 0, bytes.Length);
            // 设置当前流的位置为流的开始
            file.Seek(0, SeekOrigin.Begin);
            return bytes;
        }

        /// <summary>
        /// 写入excel文件 dir
        /// </summary>
        /// <param name="t">数据table</param>
        /// <param name="tableName">表名称</param>
        /// <param name="dir">返回的流</param>
        /// <returns></returns>
        public static bool DataModuleTableToExcel(DataTable t, string tableName,string dir)
        {
            byte[] buttfer = DataExportExcelByte(t, tableName);

            using (FileStream fs = new FileStream(dir, FileMode.Create))
            {
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(buttfer);
                bw.Close();
                fs.Close();
            }

            return true;
        }
 
 

    }
}
