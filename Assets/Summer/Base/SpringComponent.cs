using System.Linq;
using Spring.Core;
using Spring.Util;
using UnityEngine;

namespace Summer.Base
{
    /// <summary>
    /// 游戏框架组件抽象类。
    /// </summary>
    public abstract class SpringComponent : MonoBehaviour
    {
        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected virtual void Awake()
        {
            SpringContext.RegisterBean(this);

            // 如果所有集成了SpringComponent的组件都初始化完毕，则启动Spring
            var startSpringFlag = true;
            var subTypeList = AssemblyUtils.GetAllSubClassType(typeof(SpringComponent));
            foreach (var subType in subTypeList)
            {
                if (SpringContext.GetAllBeans().Any(it => it.GetType() == subType))
                {
                    continue;
                }

                startSpringFlag = false;
                break;
            }

            if (startSpringFlag)
            {
                SpringContext.GetBean<BaseComponent>().StartSpring();
            }
        }
    }
}