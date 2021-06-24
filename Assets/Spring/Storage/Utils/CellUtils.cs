using NPOI.SS.UserModel;
using Spring.Util;

namespace Spring.Storage.Utils
{
    public class CellUtils
    {
        /**
        * 获取单元格值
        *
        * @return 值，类型可能为：Date、Double、Boolean、String
        */
        public static object GetCellValue(ICell cell)
        {
            return GetCellValue(cell, cell.CellType);
        }

        public static string GetCellStringValue(ICell cell)
        {
            return GetCellValue(cell).ToString().Trim();
        }


        /**
        * 获取单元格值<br>
        * 如果单元格值为数字格式，则判断其格式中是否有小数部分，无则返回Long类型，否则返回Double类型
        */
        public static object GetCellValue(ICell cell, CellType cellType)
        {
            object value;
            switch (cellType)
            {
                case CellType.Numeric:
                    value = GetNumericValue(cell);
                    break;
                case CellType.Boolean:
                    value = cell.BooleanCellValue;
                    break;
                case CellType.Formula:
                    // 遇到公式时查找公式结果类型
                    value = GetCellValue(cell, cell.CachedFormulaResultType);
                    break;
                case CellType.Blank:
                    value = StringUtils.EMPTY;
                    break;
                case CellType.Error:
                    var error = FormulaError.ForInt(cell.ErrorCellValue);
                    value = (null == error) ? StringUtils.EMPTY : error.String;
                    break;
                default:
                    value = cell.StringCellValue;
                    break;
            }

            return value;
        }


        // -------------------------------------------------------------------------------------------------------------- Private method start

        /**
        * 获取数字类型的单元格值
        *
        * @return 单元格值，可能为Long、Double、Date
        */
        private static object GetNumericValue(ICell cell)
        {
            var value = cell.NumericCellValue;

            var style = cell.CellStyle;
            if (null == style)
            {
                return value;
            }

            // 判断是否为日期
            if (DateUtil.IsCellDateFormatted(cell))
            {
                return cell.DateCellValue;
            }

            var format = style.GetDataFormatString();
            // 普通数字
            if (null != format && format.IndexOf(StringUtils.PERIOD) < 0)
            {
                var longPart = (long) value;
                if (longPart == value)
                {
                    // 对于无小数部分的数字类型，转为Long
                    return longPart;
                }
            }

            return value;
        }
    }
}