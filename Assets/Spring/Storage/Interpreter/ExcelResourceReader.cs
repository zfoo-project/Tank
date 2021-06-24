using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NPOI.SS.UserModel;
using NPOI.Util;
using Spring.Core;
using Spring.Storage.Model.Anno;
using Spring.Storage.Model.Vo;
using Spring.Storage.Utils;
using Spring.Util;

namespace Spring.Storage.Interpreter
{
    [Bean]
    public class ExcelResourceReader : IResourceReader
    {
        public List<object> Read(Type resourceType, Stream inputStream)
        {
            
            var wb = WorkbookFactory.Create(inputStream);
            var result = new List<object>();

            // 默认取到第一个sheet页
            var sheet = wb.GetSheetAt(0);
            var fieldInfos = GetFieldInfos(sheet, resourceType);

            var iterator = sheet.GetRowEnumerator();
            // 行数定位到有效数据行，默认是第四行为有效数据行
            iterator.MoveNext();
            iterator.MoveNext();
            iterator.MoveNext();

            // 从ROW_SERVER这行开始读取数据
            while (iterator.MoveNext())
            {
                var row = iterator.Current as IRow;
                var instance = Activator.CreateInstance(resourceType);
                var idCell = row.GetCell(0);

                if (idCell == null || string.IsNullOrEmpty(CellUtils.GetCellStringValue(idCell)))
                {
                    continue;
                }

                foreach (var fieldInfo in fieldInfos)
                {
                    var cell = row.GetCell(fieldInfo.index);
                    if (cell != null)
                    {
                        var content = CellUtils.GetCellStringValue(cell);

                        if (!string.IsNullOrEmpty(content))
                        {
                            Inject(instance, fieldInfo.field, content);
                        }
                    }

                    // 如果读的是id列的单元格，则判断当前id是否为空
                    if (fieldInfo.field.IsDefined(typeof(Id)))
                    {
                        if (cell == null || string.IsNullOrEmpty(CellUtils.GetCellStringValue(cell)))
                        {
                            throw new RuntimeException(StringUtils.Format("静态资源[resource:{}]存在id未配置的项", resourceType.Name));
                        }
                    }
                }

                result.Add(instance);
            }

            return result;
        }

        private void Inject(Object instance, FieldInfo field, string content)
        {
            try
            {
                var value = StorageContext.GetConversionService().Convert(content, field.FieldType);
                AssemblyUtils.SetField(field, instance, value);
            }
            catch (Exception e)
            {
                throw new RuntimeException(StringUtils.Format("无法将Excel资源[class:{}]中的[content:{}]转换为属性[field:{}]"
                    , instance.GetType().Name, content, field.FieldType.Name), e);
            }
        }


        // 只读取代码里写的字段
        private List<ExcelFieldInfo> GetFieldInfos(ISheet sheet, Type clazz)
        {
            var fieldRow = sheet.GetRow(0);
            if (fieldRow == null)
            {
                throw new RuntimeException(StringUtils.Format("无法获取资源[class:{}]的Excel文件的属性控制列", clazz.Name));
            }

            var cellFieldMap = new Dictionary<string, int>();
            for (var i = 0; i < fieldRow.LastCellNum; i++)
            {
                var cell = fieldRow.GetCell(i);
                if (cell == null)
                {
                    continue;
                }

                var name = CellUtils.GetCellStringValue(cell);
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                cellFieldMap[name] = i;
            }

            var fieldList = AssemblyUtils.GetFieldTypes(clazz)
                .Where(it => !it.IsDefined(typeof(Transient), false) && !it.IsStatic && !it.IsInitOnly)
                .ToList();

            foreach (var field in fieldList)
            {
                if (!cellFieldMap.ContainsKey(field.Name))
                {
                    throw new RuntimeException(StringUtils.Format("资源类[class:{}]的声明属性[filed:{}]无法获取，请检查配置表的格式", clazz, field.Name));
                }
            }

            return fieldList.Select(it => new ExcelFieldInfo(cellFieldMap[it.Name], it)).ToList();
        }


        private class ExcelFieldInfo
        {
            public int index;
            public FieldInfo field;

            public ExcelFieldInfo(int index, FieldInfo field)
            {
                this.index = index;
                this.field = field;
            }
        }
    }
}