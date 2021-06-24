using MiniKing.Script.Common;
using Spring.Util;
using TMPro;
using UnityEngine;

namespace MiniKing.Script.UI.Home
{
    public class StatusBar : MonoBehaviour
    {
        public TextMeshProUGUI energy;

        public TextMeshProUGUI gem;

        public TextMeshProUGUI gold;

        public void refresh()
        {
            energy.text = StringUtils.Format("{}/60", PlayerData.currencyVo.energy);
            gem.text = PlayerData.currencyVo.gem.ToString();
            gold.text = PlayerData.currencyVo.gold.ToString();
        }
    }
}