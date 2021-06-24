using System;

namespace Spring.Storage.Conversion
{
    public interface IConversionService
    {
        object Convert(string value, Type targetType);
    }
}