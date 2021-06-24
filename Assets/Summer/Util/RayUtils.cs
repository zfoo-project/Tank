using Spring.Util;
using UnityEngine;

namespace Summer.Util
{
    public class RayUtils
    {
        // 射线碰撞获取碰撞点的坐标
        public static Vector3 GetClickedPoint(Vector2 screenPoint, string layerName)
        {
            Vector3 temp = Vector3.zero;

            Ray ray = Camera.main.ScreenPointToRay(screenPoint);
            RaycastHit hit;
            if (!StringUtils.IsBlank(layerName))
            {
                if (Physics.Raycast(ray, out hit, 999.99f, 1 << LayerMask.NameToLayer(layerName)))
                {
                    temp = hit.point;
                }
            }
            else
            {
                if (Physics.Raycast(ray, out hit))
                {
                    temp = hit.point;
                }
            }

            return temp;
        }

        public static Vector3 GetClickedPoint(Vector2 screenPoint, LayerMask layerMask)
        {
            Vector3 temp = Vector3.zero;

            int layerMaskValue = GetLayerFromLayerMask(layerMask.value);

            Ray ray = Camera.main.ScreenPointToRay(screenPoint);
            RaycastHit hit;
            if (layerMaskValue == -1)
            {
                if (Physics.Raycast(ray, out hit))
                {
                    temp = hit.point;
                }
            }
            else
            {
                if (Physics.Raycast(ray, out hit, 9999.99f, layerMaskValue))
                {
                    temp = hit.point;
                }
            }

            return temp;
        }

        //射线碰撞获取指定层的游戏物体
        public static GameObject GetClickedObject(Vector2 screenPoint, LayerMask layerMask)
        {
            GameObject temp = null;

            int layerMaskValue = GetLayerFromLayerMask(layerMask.value);

            Ray ray = Camera.main.ScreenPointToRay(screenPoint);
            RaycastHit hit;
            if (layerMaskValue == -1)
            {
                if (Physics.Raycast(ray, out hit))
                {
                    temp = hit.collider.gameObject;
                }
            }
            else
            {
                if (Physics.Raycast(ray, out hit, 999.99f, layerMaskValue))
                {
                    temp = hit.collider.gameObject;
                }
            }

            return temp;
        }

        public static GameObject GetClickedObject(Vector2 screenPoint, string layerName)
        {
            GameObject temp = null;

            Ray ray = Camera.main.ScreenPointToRay(screenPoint);
            RaycastHit hit;
            if (!StringUtils.IsBlank(layerName))
            {
                if (Physics.Raycast(ray, out hit, 999.99f, 1 << LayerMask.NameToLayer(layerName)))
                {
                    temp = hit.collider.gameObject;
                }
            }
            else
            {
                if (Physics.Raycast(ray, out hit))
                {
                    temp = hit.collider.gameObject;
                }
            }

            return temp;
        }

        public static int GetLayerFromLayerMask(LayerMask layerMask)
        {
            int layerNumber = 0;
            int layer = layerMask.value;
            while (layer > 0)
            {
                layer = layer >> 1;
                layerNumber++;
            }

            return layerNumber - 1;
        }
    }
}