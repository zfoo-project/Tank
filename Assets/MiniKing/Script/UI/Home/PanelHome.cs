using System;
using MiniKing.Script.Common;
using MiniKing.Script.Excel;
using Spring.Storage;
using Spring.Util;
using TMPro;
using UnityEngine;

namespace MiniKing.Script.UI.Home
{
    public class PanelHome : MonoBehaviour
    {
        public TextMeshProUGUI level;

        public TextMeshProUGUI name;

        public TextMeshProUGUI exp;

        public GameObject userLevelBar;

        public void refresh()
        {
            level.text = PlayerData.playerInfo.level.ToString();
            if (!StringUtils.IsBlank(PlayerData.playerInfo.name))
            {
                name.text = PlayerData.playerInfo.name.Substring(0, Math.Min(6, PlayerData.playerInfo.name.Length));
            }

            var storage = StorageContext.GetStorageManager().GetStorage<int, PlayerExpResource>();
            var playerExpResource = storage.Get(PlayerData.playerInfo.level);
            exp.text = StringUtils.Format("{}/{}", PlayerData.playerInfo.exp, playerExpResource.exp.ToString());

            userLevelBar.transform.localScale = new Vector3(1.0F * PlayerData.playerInfo.exp / playerExpResource.exp, 1, 1);
        }
    }
}