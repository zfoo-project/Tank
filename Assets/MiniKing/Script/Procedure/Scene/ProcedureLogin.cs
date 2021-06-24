using System;
using System.IO;
using System.Text;
using CsProtocol;
using CsProtocol.Buffer;
using MiniKing.Script.Constant;
using MiniKing.Script.Module.Login.Service;
using MiniKing.Script.UI.Common;
using MiniKing.Script.UI.Login;
using Spring.Core;
using Spring.Logger;
using Spring.Util;
using Spring.Util.Json;
using Summer.Procedure;
using Summer.Setting;

namespace MiniKing.Script.Procedure.Scene
{
    [Bean]
    public class ProcedureLogin : FsmState<IProcedureFsmManager>
    {
        [Autowired]
        private ISettingManager settingManager;

        [Autowired]
        private ILoginService loginService;

        public override void OnEnter(IFsm<IProcedureFsmManager> fsm)
        {
            base.OnEnter(fsm);

            if (settingManager.HasSetting(GameConstant.SETTING_LOGIN_TOKEN))
            {
                CommonController.GetInstance().loadingRotate.Show();
                loginService.ConnectToGateway();
            }
            else
            {
                LoginController.GetInstance().buttonLogin.Show();
            }
        }
    }
}