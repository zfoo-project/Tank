using System;
using System.Collections.Generic;
using System.Reflection;
using Spring.Storage.Model.Anno;
using Spring.Util;

namespace Spring.Storage.Model.Vo
{
    public class IndexDef
    {
        public string key;
        public bool unique;
        public FieldInfo field;


        public IndexDef(FieldInfo field)
        {
            this.field = field;
            var index = field.GetCustomAttribute<Index>();
            this.key = index.key;
            this.unique = index.unique;
        }

        public static Dictionary<string, IndexDef> CreateResourceIndexes(Type clazz)
        {
            var fields = AssemblyUtils.GetFieldsByAnnoInPOJOClass(clazz, typeof(Index));
            var indexs = new List<IndexDef>(fields.Length);


            foreach (var field in fields)
            {
                IndexDef indexDef = new IndexDef(field);
                indexs.Add(indexDef);
            }

            var result = new Dictionary<string, IndexDef>();
            foreach (var index in indexs)
            {
                result[index.key] = index;
            }

            return result;
        }
    }
}