using System;
using System.Collections.Generic;
using System.IO;

namespace Spring.Storage.Model.Vo
{
    public interface IStorage
    {
        void Init(Type resourceType, List<object> list);
        
        List<object> GetAll();
    }
}