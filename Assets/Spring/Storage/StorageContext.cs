using Spring.Core;
using Spring.Storage.Conversion;
using Spring.Storage.Helper;
using Spring.Storage.Interpreter;
using Spring.Storage.Manager;

namespace Spring.Storage
{
    [Bean]
    public class StorageContext
    {
        private static StorageContext instance;

        [Autowired]
        private IResourceReader resourceReader;

        [Autowired]
        private IConversionService conversionService;

        [Autowired]
        private IStorageManager storageManager;

        [Autowired]
        private IStorageHelper storageHelper;

        public StorageContext()
        {
            instance = this;
        }


        public static IConversionService GetConversionService()
        {
            return instance.conversionService;
        }

        public static IResourceReader GetResourceReader()
        {
            return instance.resourceReader;
        }

        public static IStorageManager GetStorageManager()
        {
            return instance.storageManager;
        }
        
        public static IStorageHelper GetStorageHelper()
        {
            return instance.storageHelper;
        }
    }
}