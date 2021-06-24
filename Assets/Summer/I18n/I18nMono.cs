using System;
using System.Text.RegularExpressions;
using Spring.Core;
using Spring.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Summer.I18n
{
    public class I18nMono : MonoBehaviour
    {
        private void Start()
        {
            if (!SpringContext.GetScanFlag())
            {
                return;
            }

            RefreshText();
            RefreshTextMeshProUGUI();
        }

        private void onStart()
        {
            if (!SpringContext.GetScanFlag())
            {
                return;
            }

            RefreshText();
            RefreshTextMeshProUGUI();
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

        [ThreadStatic]
        private static Regex regex;


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