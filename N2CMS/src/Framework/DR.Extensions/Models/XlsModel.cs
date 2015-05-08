using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;

namespace N2.Models
{
    public abstract class XlsRecord
    {
        public virtual object GetValue(string name)
        {
            PropertyInfo info = GetType().GetProperty(name);
            if (info != null)
                return info.GetValue(this, new object[0]);

            return null;
        }
    }

    public class XlsModel
    {
        private string SheetName { get; set; }
        private readonly List<string> _columns = new List<string>();
        private readonly List<XlsRecord> _data = new List<XlsRecord>();
        private readonly ISheet _sheet;
        private readonly HSSFWorkbook _workbook;
        private readonly ICellStyle _dateStyle;
        private readonly ICellStyle _nullStyle;
        private readonly ICellStyle _headStyle;

        public XlsModel(string sheetName)
        {
            SheetName = sheetName;

            // Create a new workbook and a sheet named "User Accounts"
            _workbook = new HSSFWorkbook();
            _sheet = _workbook.CreateSheet(SheetName);

            var boldFont = _workbook.CreateFont();
            boldFont.FontHeightInPoints = 10;
            boldFont.FontName = "Arial";
            boldFont.Boldweight = (short) FontBoldWeight.Bold;

            _dateStyle = _workbook.CreateCellStyle();
            _dateStyle.DataFormat = _workbook.CreateDataFormat().GetFormat("dd/mm/yyyy");

            _nullStyle = _workbook.CreateCellStyle();
            _nullStyle.FillForegroundColor = HSSFColor.Grey40Percent.Index;
            _nullStyle.FillPattern = FillPattern.SolidForeground;

            _headStyle = _workbook.CreateCellStyle();
            _headStyle.SetFont(boldFont);
        }

        public void Add(XlsRecord record)
        {
            _data.Add(record);
        }

        public void AddColumn(string name)
        {
            if (!_columns.Contains(name))
                _columns.Add(name);
        }

        public void Export(Stream stream)
        {
            // Add header labels
            var rowIndex = 0;
            var colIndex = 0;
            var row = _sheet.CreateRow(rowIndex);
            foreach (var column in _columns)
            {
                var cell = row.CreateCell(colIndex++);
                cell.SetCellValue(column);
                cell.CellStyle = _headStyle;
            }
            rowIndex++;

            // Add data rows
            // Add data rows
            foreach (var record in _data)
            {
                row = _sheet.CreateRow(rowIndex);
                var c = 0;
                foreach (var column in _columns)
                {
                    object val = record.GetValue(column);
                    string str = val as string;
                    if (str != null)
                    {                        
                        if (str.Length > 0 && str.Length >= 32000)
                            str = "[BLOB] " + str.Substring(0, Math.Min(str.Length, 32000)); // TODO Attachments
                        row.CreateCell(c++, CellType.String).SetCellValue(str);
                    }
                    else if (val is double)
                        row.CreateCell(c++, CellType.Numeric).SetCellValue((double) val);
                    else if (val is int)
                        row.CreateCell(c++, CellType.Numeric).SetCellValue((int) val);
                    else if (val is DateTime)
                    {
                        var cell = row.CreateCell(c++, CellType.Numeric);
                        cell.SetCellValue((DateTime) val); // TODO Format
                        cell.CellStyle = _dateStyle;
                    }
                    else if (val == null)
                    {
                        var cell = row.CreateCell(c++, CellType.Blank);
                        cell.CellStyle = _nullStyle;
                    }
                    else
                        row.CreateCell(c++, CellType.String).SetCellValue(val.ToString());
                }
                rowIndex++;
            }

            _sheet.AutoSizeColumn(_columns.IndexOf("Discriminator"));
            _sheet.AutoSizeColumn(_columns.IndexOf("Published"));
            _sheet.AutoSizeColumn(_columns.IndexOf("Updated"));

            _sheet.SetAutoFilter(new CellRangeAddress(0,rowIndex,0,_columns.Count));
            _workbook.Write(stream);
        }   
    }
}