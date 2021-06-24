using Summer.Resource.Model.Group;

namespace Summer.Resource.Model.UpdatableVersion
{
    /// <summary>
    /// 使用可更新模式并更新指定资源组完成时的回调函数。
    /// </summary>
    /// <param name="resourceGroup">更新的资源组。</param>
    /// <param name="result">更新资源结果，全部成功为 true，否则为 false。</param>
    public delegate void UpdateResourcesCompleteCallback(IResourceGroup resourceGroup, bool result);
}
