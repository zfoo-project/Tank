using System;

namespace Spring.Storage.Model.Anno
{
    public class Index : Attribute
    {
        public string key;

        public bool unique;


        public Index(string key, bool unique)
        {
            this.key = key;
            this.unique = unique;
        }
    }
}