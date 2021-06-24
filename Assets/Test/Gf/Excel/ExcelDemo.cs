using System;
using System.IO;
using Summer.Base;
using Summer.Base.Model;
using Summer.Procedure;
using Summer.Resource;
using Summer.Resource.Model.Callback;
using NPOI.SS.UserModel;
using Spring.Core;
using Spring.Logger;
using Spring.Util;
using UnityEngine;

public class ExcelDemo : FsmState<IProcedureFsmManager>
{
    public override void OnEnter(IFsm<IProcedureFsmManager> fsm)
    {
        base.OnEnter(fsm);

        Log.Info("excel test");

        var resourceManager = SpringContext.GetBean<ResourceManager>();
        var fileUri = PathUtils.GetRemotePath(Path.Combine(resourceManager.readOnlyPath, "StudentResource.xls"));

        Log.Info("ResourceIniter资源路径[{}]", fileUri);

        resourceManager.simpleLoadResource.LoadBytes(fileUri, new LoadBytesCallbacks(OnLoadPackageVersionListSuccess, OnLoadPackageVersionListFailure), null);
    }


    private void OnLoadPackageVersionListSuccess(string fileUri, byte[] bytes, float duration, object userData)
    {
        MemoryStream memoryStream = null;
        try
        {
            memoryStream = new MemoryStream(bytes, false);
            
            //excel 2007之后版本
            var Mybook = WorkbookFactory.Create(memoryStream);
            //获取表名为  “男性” 的表
            var sheet = Mybook.GetSheetAt(0);
            //获取行数
            int RowLength = sheet.LastRowNum;

            Debug.Log(RowLength);
            //遍历所有行
            for (int i = 0; i <= sheet.LastRowNum; i++)
            {
                //获取行
                IRow sheet_row = sheet.GetRow(i);
                //当前行为null时，跳过此行
                if (sheet_row == null) continue;
                //遍历行的所有列
                for (int b = 0; b < sheet_row.LastCellNum; b++)
                {
                    Log.Info(sheet_row.GetCell(b));
                }
            }
        }
        catch (Exception exception)
        {
            throw new GameFrameworkException(StringUtils.Format("Parse package version list exception '{}'.", exception.ToString()), exception);
        }
        finally
        {
            if (memoryStream != null)
            {
                memoryStream.Dispose();
            }
        }
    }
    
    private void OnLoadPackageVersionListFailure(string fileUri, string errorMessage, object userData)
    {
        throw new GameFrameworkException(StringUtils.Format(
            "Package version list '{}' is invalid, error message is '{}'.", fileUri,
            string.IsNullOrEmpty(errorMessage) ? "<Empty>" : errorMessage));
    }
}