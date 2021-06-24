using System.Collections.Generic;
using UnityEngine;

namespace Summer.Util
{
    public abstract class UnityUtils
    {
        /// <summary>
        /// 获取或增加组件。
        /// </summary>
        /// <typeparam name="T">要获取或增加的组件。</typeparam>
        /// <param name="gameObject">目标对象。</param>
        /// <returns>获取或增加的组件。</returns>
        public static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }

            return component;
        }


        private static readonly List<Transform> cachedTransforms = new List<Transform>();

        /// <summary>
        /// 递归设置游戏对象的层次。
        /// </summary>
        /// <param name="gameObject"><see cref="GameObject" /> 对象。</param>
        /// <param name="layer">目标层次的编号。</param>
        public static void SetLayerRecursively(GameObject gameObject, int layer)
        {
            gameObject.GetComponentsInChildren(true, cachedTransforms);
            for (int i = 0; i < cachedTransforms.Count; i++)
            {
                cachedTransforms[i].gameObject.layer = layer;
            }

            cachedTransforms.Clear();
        }


        /// <summary>
        /// 设置相对位置的 x 坐标。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="newValue">x 坐标值。</param>
        public static void SetLocalPositionX(Transform transform, float newValue)
        {
            Vector3 v = transform.localPosition;
            v.x = newValue;
            transform.localPosition = v;
        }

        /// <summary>
        /// 设置相对位置的 y 坐标。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="newValue">y 坐标值。</param>
        public static void SetLocalPositionY(Transform transform, float newValue)
        {
            Vector3 v = transform.localPosition;
            v.y = newValue;
            transform.localPosition = v;
        }

        /// <summary>
        /// 设置相对位置的 z 坐标。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="newValue">z 坐标值。</param>
        public static void SetLocalPositionZ(Transform transform, float newValue)
        {
            Vector3 v = transform.localPosition;
            v.z = newValue;
            transform.localPosition = v;
        }

        /// <summary>
        /// 增加相对位置的 x 坐标。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="deltaValue">x 坐标值。</param>
        public static void AddLocalPositionX(Transform transform, float deltaValue)
        {
            Vector3 v = transform.localPosition;
            v.x += deltaValue;
            transform.localPosition = v;
        }

        /// <summary>
        /// 增加相对位置的 y 坐标。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="deltaValue">y 坐标值。</param>
        public static void AddLocalPositionY(Transform transform, float deltaValue)
        {
            Vector3 v = transform.localPosition;
            v.y += deltaValue;
            transform.localPosition = v;
        }

        /// <summary>
        /// 增加相对位置的 z 坐标。
        /// </summary>
        /// <param name="transform"><see cref="Transform" /> 对象。</param>
        /// <param name="deltaValue">z 坐标值。</param>
        public static void AddLocalPositionZ(Transform transform, float deltaValue)
        {
            Vector3 v = transform.localPosition;
            v.z += deltaValue;
            transform.localPosition = v;
        }


        /// <summary>
        /// 取 <see cref="Vector3" /> 的 (x, y, z) 转换为 <see cref="Vector2" /> 的 (x, z)。
        /// </summary>
        /// <param name="vector3">要转换的 Vector3。</param>
        /// <returns>转换后的 Vector2。</returns>
        public static Vector2 ToVector2(Vector3 vector3)
        {
            return new Vector2(vector3.x, vector3.z);
        }

        /// <summary>
        /// 取 <see cref="Vector2" /> 的 (x, y) 转换为 <see cref="Vector3" /> 的 (x, 0, y)。
        /// </summary>
        /// <param name="vector2">要转换的 Vector2。</param>
        /// <returns>转换后的 Vector3。</returns>
        public static Vector3 ToVector3(Vector2 vector2)
        {
            return new Vector3(vector2.x, 0f, vector2.y);
        }
    }
}