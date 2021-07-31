using System;
using CsProtocol;
using MiniKing.Script.Common;
using MiniKing.Script.Constant;
using MiniKing.Script.Procedure.Scene;
using MiniKing.Script.UI.Common;
using MiniKing.Script.UI.Home;
using Spring.Core;
using Spring.Logger;
using Spring.Util;
using Spring.Util.Json;
using Summer.I18n;
using Summer.Net.Dispatcher;
using Summer.Setting;

namespace MiniKing.Script.Module.System.Controller
{
    [Bean]
    public class SystemFacade
    {
        [Autowired]
        private II18nManager i18nManager;

        [Autowired]
        private ISettingManager settingManager;

        [Autowired]
        private ProcedureChangeScene procedureChangeScene;

        [PacketReceiver]
        public void AtError(Error error)
        {
            var errorMessage = error.errorMessage;

            if (StringUtils.IsBlank(errorMessage))
            {
                return;
            }

            var i18nMessage = i18nManager.GetString(errorMessage);
            CommonController.GetInstance().snackbar.ServerError(i18nMessage);

            var i18nEnum = I18nEnum.default_enum;
            Enum.TryParse<I18nEnum>(errorMessage, out i18nEnum);
            if (i18nEnum == I18nEnum.default_enum)
            {
                return;
            }

            // 如果账号不存在，则直接退出到登录/注册界面
            if (i18nEnum == I18nEnum.error_account_not_exist)
            {
                settingManager.RemoveSetting(GameConstant.SETTING_LOGIN_TOKEN);
                procedureChangeScene.ChangeScene(SceneEnum.Login);
                return;
            }
            else if(i18nEnum == I18nEnum.error_account_sensitive_word)
            {
                PlayerData.loginError = true;
                return;
            }
        }

        [PacketReceiver]
        public void AtCurrencyUpdateNotice(CurrencyUpdateNotice notice)
        {
            PlayerData.currencyVo = notice.currencyVo;
            HomeController.refreshStatusBar();
        }

        [PacketReceiver]
        public void AtPlayerExpNotice(PlayerExpNotice notice)
        {
            PlayerData.playerInfo.exp = notice.exp;
            PlayerData.playerInfo.level = notice.level;
            HomeController.refreshPanelHome();
        }

        [PacketReceiver]
        public void AtBattleResultResponse(BattleResultResponse notice)
        {
            // do something
        }
        
        [PacketReceiver]
        public void AtScoreRankResponse(ScoreRankResponse notice)
        {
            Log.Info(JsonUtils.object2String(notice));
            PlayerData.ranks = notice.ranks;
            HomeController.GetInstance().panelRanking.Show();
        }
    }
}