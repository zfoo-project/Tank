using System;
using System.Collections.Generic;
using System.IO;

namespace Spring.Storage.Interpreter
{
    public interface IResourceReader
    {
        List<object> Read(Type resourceType, Stream inputStream);
    }
}