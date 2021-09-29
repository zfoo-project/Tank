using System;
using System.Text.RegularExpressions;
using DG.Tweening;
using MiniKing.Script.Common;
using Spring.Core;
using Spring.Event;
using Spring.Logger;
using Spring.Util;
using Summer.I18n;
using Summer.Net.Core.Model;
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
            RefreshText();
            RefreshTextMeshProUGUI();
            var sequence = DOTween.Sequence();
            sequence.AppendInterval(0.1f);
            sequence.AppendCallback(() => { refresh(); });
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void refresh()
        {
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
                    var timeTransform = transform.Find("Text_Time");

                    var name = nameTransform.GetComponentInChildren<TextMeshProUGUI>();
                    if (!StringUtils.IsBlank(rank.playerInfo.name))
                    {
                        name.text = rank.playerInfo.name.Substring(0, Math.Min(6, rank.playerInfo.name.Length));
                    }

                    var score = scoreTransform.GetComponentInChildren<TextMeshProUGUI>();
                    score.text = rank.score.ToString();

                    var time = timeTransform.GetComponentInChildren<TextMeshProUGUI>();
                    if (rank.time == 0)
                    {
                        time.text = "2021-06-01 10:10:00";
                    }
                    else
                    {
                        var timeStr = TimeUtils.ToCommonFormat(TimeUtils.TimestampToDateTime(rank.time));
                        timeStr = StringUtils.SubstringBeforeFirst(timeStr, StringUtils.PERIOD);
                        time.text = timeStr;
                    }
                }
                else
                {
                    transform.gameObject.SetActive(false);
                }
            }
        }

        private void RefreshText()
        {
            var texts = GetComponentsInChildren<Text>(true);
            var i18nManager = SpringContext.GetBean<II18nManager>();
            var mainFontAsset = i18nManager.mainFontAsset;
            foreach (var text in texts)
            {
                if (!string.IsNullOrEmpty(text.text))
                {
                    text.text = ToI18n(text.text);
                }

                if (mainFontAsset != null && mainFontAsset is Font)
                {
                    text.font = (Font) mainFontAsset;
                }
            }
        }

        private void RefreshTextMeshProUGUI()
        {
            var texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            var i18nManager = SpringContext.GetBean<II18nManager>();
            var mainFontAsset = i18nManager.mainFontAsset;
            foreach (var text in texts)
            {
                if (!string.IsNullOrEmpty(text.text))
                {
                    text.text = ToI18n(text.text);
                }

                if (mainFontAsset != null && mainFontAsset is TMP_FontAsset)
                {
                    text.font = (TMP_FontAsset) mainFontAsset;
                }
            }
        }

        [ThreadStatic] private static Regex regex;


        private string ToI18n(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var i18nManager = SpringContext.GetBean<II18nManager>();

            if (text.IndexOf('{') < 0)
            {
                return i18nManager.GetString(text);
            }

            // 可能含有富文本，进行处理
            if (regex == null)
            {
                regex = new Regex("{ *[\\w|.|_]* *}");
            }

            var builder = StringUtils.CachedStringBuilder();
            builder.Append(text);

            var matches = regex.Matches(text);
            foreach (Match match in matches)
            {
                var matchValue = match.Value;
                if (matchValue.Equals("{}"))
                {
                    continue;
                }

                var i18nKey = matchValue.Substring(1, matchValue.Length - 2).Trim();
                builder.Replace(matchValue, i18nManager.GetString(i18nKey));
            }

            return builder.ToString();
        }
    }
}