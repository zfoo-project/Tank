using Summer.Resource.Model.Callback;
using Spring.Collection.Reference;

namespace Summer.Resource.Model.Vo
{
    public sealed class LoadBinaryInfo : IReference
    {
        private string binaryAssetName;
        private ResourceInfo resourceInfo;
        private LoadBinaryCallbacks loadBinaryCallbacks;
        private object userData;


        public string BinaryAssetName
        {
            get { return binaryAssetName; }
        }

        public ResourceInfo ResourceInfo
        {
            get { return resourceInfo; }
        }

        public LoadBinaryCallbacks LoadBinaryCallbacks
        {
            get { return loadBinaryCallbacks; }
        }

        public object UserData
        {
            get { return userData; }
        }

        public static LoadBinaryInfo Create(string binaryAssetName, ResourceInfo resourceInfo,
            LoadBinaryCallbacks loadBinaryCallbacks, object userData)
        {
            LoadBinaryInfo loadBinaryInfo = ReferenceCache.Acquire<LoadBinaryInfo>();
            loadBinaryInfo.binaryAssetName = binaryAssetName;
            loadBinaryInfo.resourceInfo = resourceInfo;
            loadBinaryInfo.loadBinaryCallbacks = loadBinaryCallbacks;
            loadBinaryInfo.userData = userData;
            return loadBinaryInfo;
        }

        public void Clear()
        {
            binaryAssetName = null;
            resourceInfo = null;
            loadBinaryCallbacks = null;
            userData = null;
        }
    }
}