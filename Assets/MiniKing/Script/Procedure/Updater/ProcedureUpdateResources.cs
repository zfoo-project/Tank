using System;
using System.Collections.Generic;
using MiniKing.Script.Constant;
using MiniKing.Script.UI.Common;
using MiniKing.Script.UI.Login;
using Spring.Core;
using Spring.Event;
using Spring.Logger;
using Spring.Util;
using Spring.Util.Math;
using Spring.Util.Property;
using Summer.Download;
using Summer.I18n;
using Summer.Procedure;
using Summer.Resource;
using Summer.Resource.Model.Eve;
using Summer.Resource.Model.Group;

namespace MiniKing.Script.Procedure.Updater
{
    [Bean]
    public class ProcedureUpdateResources : FsmState<IProcedureFsmManager>
    {
        [Autowired]
        private IDownloadManager downloadManager;

        private int updateResourceCount;
        private long updateResourceTotalZipLength;

        private bool updateResourcesComplete;
        private int updateSuccessCount;

        /**
         * pair的key为资源名称，value为下载的长度大小
         */
        private List<Pair<string, int>> updateLengthData = new List<Pair<string, int>>();


        public override void OnEnter(IFsm<IProcedureFsmManager> fsm)
        {
            base.OnEnter(fsm);

            var updateResourceInfo = fsm.GetData<Pair<int, long>>(GameConstant.UPDATE_RESOURCE_INFO);
            updateResourceCount = updateResourceInfo.key;
            updateResourceTotalZipLength = updateResourceInfo.value;
            fsm.CleadData();

            Log.Info("Start update resources...");

            SpringContext.GetBean<IResourceManager>().UpdateResources(OnUpdateResourcesComplete);

            CommonController.GetInstance().progressBar.Show(NumberUtils.LongToInt(updateResourceTotalZipLength));
        }

        public override void OnLeave(IFsm<IProcedureFsmManager> fsm, bool isShutdown)
        {
            base.OnLeave(fsm, isShutdown);
            CommonController.GetInstance().progressBar.Hide();
        }

        public override void OnUpdate(IFsm<IProcedureFsmManager> fsm, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(fsm, elapseSeconds, realElapseSeconds);

            if (!updateResourcesComplete)
            {
                return;
            }

            fsm.ChangeState<ProcedurePreLoad>();
        }

        private void RefreshProgress()
        {
            long currentTotalUpdateLength = 0L;
            for (int i = 0; i < updateLengthData.Count; i++)
            {
                currentTotalUpdateLength += updateLengthData[i].value;
            }

            float progressTotal = (float) currentTotalUpdateLength / updateResourceTotalZipLength;

            string descriptionText = SpringContext.GetBean<II18nManager>().GetString(I18nEnum.downloading_progress_tip.ToString()
                , updateSuccessCount.ToString(), updateResourceCount.ToString(),
                GetByteLengthString(currentTotalUpdateLength)
                , GetByteLengthString(updateResourceTotalZipLength), NumberUtils.round(progressTotal, 2) * new decimal(100),
                GetByteLengthString((int) downloadManager.CurrentSpeed));

            CommonController.GetInstance().progressBar.SetBar(NumberUtils.LongToInt(currentTotalUpdateLength), descriptionText);
        }

        private string GetByteLengthString(long byteLength)
        {
            if (byteLength < IOUtils.BYTES_PER_KB) // 2 ^ 10
            {
                return StringUtils.Format("{} Bytes", byteLength.ToString());
            }

            if (byteLength < IOUtils.BYTES_PER_MB) // 2 ^ 20
            {
                return StringUtils.Format("{} KB", NumberUtils.round(byteLength / (float) IOUtils.BYTES_PER_KB, 2));
            }

            if (byteLength < IOUtils.BYTES_PER_GB) // 2 ^ 30
            {
                return StringUtils.Format("{} MB", NumberUtils.round(byteLength / (float) IOUtils.BYTES_PER_MB, 2));
            }

            if (byteLength < IOUtils.BYTES_PER_TB) // 2 ^ 40
            {
                return StringUtils.Format("{} GB", NumberUtils.round(byteLength / (float) IOUtils.BYTES_PER_GB, 2));
            }

            throw new Exception(StringUtils.Format("长度[byteLength:{}]太大无法下载", byteLength));
        }

        private void OnUpdateResourcesComplete(IResourceGroup resourceGroup, bool result)
        {
            if (result)
            {
                updateResourcesComplete = true;
                Log.Info("Update resources complete with no errors.");
            }
            else
            {
                Log.Error("Update resources complete with errors.");
            }
        }

        [EventReceiver]
        private void OnResourceUpdateStartEvent(ResourceUpdateStartEvent eve)
        {
            for (int i = 0; i < updateLengthData.Count; i++)
            {
                if (updateLengthData[i].key == eve.name)
                {
                    Log.Warning("Update resource '{}' is invalid.", eve.name);
                    updateLengthData[i].value = 0;
                    RefreshProgress();
                    return;
                }
            }

            updateLengthData.Add(new Pair<string, int>(eve.name, 0));
        }

        [EventReceiver]
        private void OnResourceUpdateChangedEvent(ResourceUpdateChangedEvent eve)
        {
            for (int i = 0; i < updateLengthData.Count; i++)
            {
                if (updateLengthData[i].key == eve.name)
                {
                    updateLengthData[i].value = eve.currentLength;
                    RefreshProgress();
                    return;
                }
            }

            Log.Warning("Update resource '{}' is invalid.", eve.name);
        }

        [EventReceiver]
        private void OnResourceUpdateSuccessEvent(ResourceUpdateSuccessEvent eve)
        {
            Log.Info("Update resource '{}' success.", eve.name);

            for (int i = 0; i < updateLengthData.Count; i++)
            {
                if (updateLengthData[i].key == eve.name)
                {
                    updateLengthData[i].value = eve.zipLength;
                    updateSuccessCount++;
                    RefreshProgress();
                    return;
                }
            }

            Log.Warning("Update resource '{}' is invalid.", eve.name);
        }

        [EventReceiver]
        private void OnResourceUpdateFailureEvent(ResourceUpdateFailureEvent e)
        {
            if (e.retryCount >= e.totalRetryCount)
            {
                Log.Error("Update resource '{}' failure from '{}' with error message '{}', retry count '{}'.",
                    e.name, e.downloadUri, e.errorMessage, e.retryCount.ToString());
                return;
            }
            else
            {
                Log.Info("Update resource '{}' failure from '{}' with error message '{}', retry count '{}'.",
                    e.name, e.downloadUri, e.errorMessage, e.retryCount.ToString());
            }

            for (int i = 0; i < updateLengthData.Count; i++)
            {
                if (updateLengthData[i].key == e.name)
                {
                    updateLengthData.Remove(updateLengthData[i]);
                    RefreshProgress();
                    return;
                }
            }


            Log.Warning("Update resource '{}' is invalid.", e.name);
        }
    }
}