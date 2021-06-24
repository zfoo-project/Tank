namespace Summer.Resource.Model.UpdatableVersion
{
    /// <summary>
    /// 版本资源列表更新失败回调函数。
    /// </summary>
    /// <param name="downloadUri">版本资源列表更新地址。</param>
    /// <param name="errorMessage">错误信息。</param>
    public delegate void UpdateVersionListFailureCallback(string downloadUri, string errorMessage);
}
