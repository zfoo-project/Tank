using MiniKing.Script.Constant;
using MiniKing.Script.Excel;
using MiniKing.Script.UI.Common;
using MiniKing.Script.Util;
using Spring.Core;
using Spring.Event;
using Spring.Logger;
using Spring.Storage.Model.Anno;
using Spring.Storage.Model.Vo;
using Summer.Base;
using Summer.I18n;
using Summer.Procedure;
using Summer.Resource.Model.Constant;
using Summer.Scene;
using Summer.Scene.Model;
using Summer.Sound;
using UnityEngine;

namespace MiniKing.Script.Procedure.Scene
{
    [Bean]
    public class ProcedureChangeScene : FsmState<IProcedureFsmManager>
    {

        private static readonly int LOAD_SCENE_PROGRESS_MAX_VALUE = 1_0000;
        
        [Autowired]
        private ISceneManager sceneManager;

        [Autowired]
        private II18nManager i18NManager;

        [Autowired]
        private ISoundManager soundManager;

        [Autowired]
        private BaseComponent baseComponent;

        [Autowired]
        private SceneComponent sceneComponent;


        [ResInjection]
        private Storage<int, SceneResource> sceneResources;

        private bool changeSceneComplete;

        private SceneEnum sceneEnum;
        
        
        public override void OnEnter(IFsm<IProcedureFsmManager> fsm)
        {
            base.OnEnter(fsm);

            changeSceneComplete = false;

            // 停止所有声音
            soundManager.StopAllLoadingSounds();
            soundManager.StopAllLoadedSounds();

            // 卸载所有场景
            sceneManager.UnloadAllScenes();

            // 还原游戏速度
            baseComponent.ResetNormalGameSpeed();

            sceneEnum = fsm.GetData<SceneEnum>(SceneConstant.NEXT_SCENE_ENUM);

            var sceneResource = sceneResources.Get((int) sceneEnum);
            sceneManager.LoadScene(AssetPathUtils.GetSceneAsset(sceneResource.sceneAsset), ResourceConstant.SceneAsset, this);
         
            CommonController.GetInstance().panelChangeScene.Show(LOAD_SCENE_PROGRESS_MAX_VALUE);
            var message = i18NManager.GetString(I18nEnum.load_scene_tip.ToString(), 0, 100);
            CommonController.GetInstance().progressBar.SetBar(0, message);
        }

        public override void OnUpdate(IFsm<IProcedureFsmManager> fsm, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(fsm, elapseSeconds, realElapseSeconds);

            if (!changeSceneComplete)
            {
                return;
            }

            fsm.ChangeState(SceneConstant.SCENE_MAP[sceneEnum]);
        }

        public void ChangeScene(SceneEnum sceneEnum)
        {
            var fsm = SpringContext.GetBean<IProcedureFsmManager>().ProcedureFsm;
            fsm.SetData(SceneConstant.NEXT_SCENE_ENUM, sceneEnum);
            fsm.ChangeState<ProcedureChangeScene>();
        }
        
        [EventReceiver]
        private void OnLoadSceneUpdateEvent(LoadSceneUpdateEvent eve)
        {
            if (eve.userData != this)
            {
                return;
            }

            var progress = eve.progress;

            Log.Info("Load scene '{}' update, progress '{}'.", eve.sceneAssetName, eve.progress.ToString("P2"));
            var message = i18NManager.GetString(I18nEnum.load_scene_tip.ToString(), Mathf.FloorToInt(progress * 100), 100);
            CommonController.GetInstance().progressBar.SetBar(Mathf.FloorToInt(progress * LOAD_SCENE_PROGRESS_MAX_VALUE), message);
        }
        

        [EventReceiver]
        private void OnLoadSceneSuccessEvent(LoadSceneSuccessEvent eve)
        {
            if (eve.userData != this)
            {
                return;
            }

            Log.Info("Load scene '{}' OK.", eve.sceneAssetName);

            var sceneResource = sceneResources.Get(sceneEnum.GetHashCode());

            if (!string.IsNullOrEmpty(sceneResource.backgroundMusicAsset))
            {
                MusicUtils.PlayMusic(sceneResource.backgroundMusicAsset);
            }

            changeSceneComplete = true;
            CommonController.GetInstance().panelChangeScene.Hide();
        }

        [EventReceiver]
        private void OnLoadSceneFailureEvent(LoadSceneFailureEvent eve)
        {
            if (eve.userData != this)
            {
                return;
            }

            Log.Error("Load scene '{}' failure", eve.sceneAssetName);
        }


        [EventReceiver]
        private void OnLoadSceneDependencyAssetEvent(LoadSceneDependencyAssetEvent eve)
        {
            if (eve.userData != this)
            {
                return;
            }

            Log.Info("Load scene '{}' dependency asset '{}', count '{}/{}'.", eve.sceneAssetName, eve.dependencyAssetName, eve.loadedCount.ToString(), eve.totalCount.ToString());
        }
    }
}