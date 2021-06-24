using System;
using MiniKing.Script.Common;
using Spring.Logger;
using Spring.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniKing.Script.UI.Home
{
    public class PanelRanking : MonoBehaviour
    {
        public void Show()
        {
            gameObject.SetActive(true);

            var group = GetComponentInChildren<VerticalLayoutGroup>();
            var count = 0;
            var ranks = PlayerData.ranks;
            foreach (Transform transform in group.transform)
            {
                if (ranks.Count > count)
                {
                    transform.gameObject.SetActive(true);
                    var rank = ranks[count++];
                    var nameTransform = transform.Find("Text_Name");
                    var scoreTransform = transform.Find("Text_Score");

                    var name = nameTransform.GetComponentInChildren<TextMeshProUGUI>();
                    if (!StringUtils.IsBlank(rank.playerInfo.name))
                    {
                        name.text = rank.playerInfo.name.Substring(0, Math.Min(6, rank.playerInfo.name.Length));
                    }
                    
                    var score = scoreTransform.GetComponentInChildren<TextMeshProUGUI>();
                    score.text = rank.score.ToString();
                }
                else
                {
                    transform.gameObject.SetActive(false);
                }
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void refresh()
        {
        }
    }
}