using System;
using System.Collections.Generic;
using MiniKing.Script.Procedure.Scene;

namespace MiniKing.Script.Constant
{
    public class SceneConstant
    {
        public static readonly string NEXT_SCENE_ENUM = "NextSceneEnum";

        /**
         * 场景相关
         */
        public static readonly Dictionary<SceneEnum, Type> SCENE_MAP = new Dictionary<SceneEnum, Type>()
        {
            {SceneEnum.Login, typeof(ProcedureLogin)},
            {SceneEnum.Menu, typeof(ProcedureMenu)},
            {SceneEnum.Level1, typeof(ProcedureLevel1)}
        };
    }

    public enum SceneEnum
    {
        Login = 1,
        Menu = 2,
        Level1 = 3
    }
}