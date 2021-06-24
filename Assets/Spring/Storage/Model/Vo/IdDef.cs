using System;
using System.Reflection;
using Spring.Storage.Model.Anno;
using Spring.Util;

namespace Spring.Storage.Model.Vo
{
    public class IdDef
    {
        public FieldInfo field;

        public static IdDef ValueOf(Type type)
        {
            var fields = AssemblyUtils.GetFieldsByAnnoInPOJOClass(type, typeof(Id));
            if (fields.Length <= 0)
            {
                throw new Exception(StringUtils.Format("类[{}]没有主键Id注解", type.Name));
            }

            if (fields.Length > 1)
            {
                throw new Exception(StringUtils.Format("类[{}]的主键Id注解重复", type.Name));
            }

            if (fields[0] == null)
            {
                throw new Exception(StringUtils.Format("不合法的Id资源映射对象[{}]", type.Name));
            }

            IdDef idDef = new IdDef();
            idDef.field = fields[0];
            return idDef;
        }
    }
}