using System.Collections.Generic;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Spring.Logger;
using Spring.Util.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Test.Editor
{
    public class OssPolicyVO
    {
        /**
         * oss生成的协议
         */
        public string policy;

        /**
         * 访问码accessKey
         */
        public string accessKeyId;

        /**
         * 签名
         */
        public string signature;

        /**
         * 文件夹，路径
         */
        public string dir;

        /**
         * 地址
         */
        public string host;


        /**
         * 过期时间
         */
        public string expire;
    }

    public class WebTest
    {
        [Ignore("empty")]
        [Test]
        public void WebTestSimplePasses()
        {
            var ossPolicyStr ="{\"policy\":\"aaa\",\"accessKeyId\":\"bbb\",\"signature\":\"tE/ccc=\",\"dir\":\"test/img/20201029/9_uUFfXqkHFUU68QZjv8bkljem0ascMfXh_\",\"host\":\"https://report.zfoo.com\",\"expire\":\"1603984869\"}\n";

            var ossPolicy = JsonUtils.string2Object<OssPolicyVO>(ossPolicyStr);

            var asyncRequest = ossUpload(ossPolicy, Encoding.UTF8.GetBytes(ossPolicyStr));
            while (true)
            {
                if (asyncRequest.isDone)
                {
                    //异常处理，很多博文用了error!=null这是错误的，请看下文其他属性部分
                    if (asyncRequest.webRequest.isHttpError || asyncRequest.webRequest.isNetworkError)
                    {
                        Log.Info(asyncRequest.webRequest.error);
                        Log.Info(JsonUtils.object2String(asyncRequest.webRequest.GetResponseHeaders()));
                        Log.Info(asyncRequest.webRequest.ToString());
                    }
                    else
                    {
                        Log.Info(asyncRequest.webRequest.downloadHandler.text);
                    }

                    break;
                }
            }
        }


        public static UnityWebRequestAsyncOperation ossUpload(OssPolicyVO ossPolicy, byte[] bytes)
        {
            var formData = new WWWForm();
            formData.AddField("key", ossPolicy.dir);
            formData.AddField("policy", ossPolicy.policy);
            formData.AddField("OSSAccessKeyId", ossPolicy.accessKeyId);
            formData.AddField("success_action_status", 200);
            formData.AddField("callback", string.Empty);
            formData.AddField("signature", ossPolicy.signature);
            formData.AddBinaryData("file", bytes);

            var webRequest = UnityWebRequest.Post(ossPolicy.host, formData);
            
            webRequest.SetRequestHeader("Content-Type", "multipart/form-data");
            return webRequest.SendWebRequest();
        }
        
        /**
         * 指定bucket和object上传的方式
         */
        public static UnityWebRequestAsyncOperation ossUpload(string host, byte[] bytes)
        {
            UnityWebRequest request = UnityWebRequest.Put(host, bytes);
            request.method = "PUT";
            request.timeout = 150000;
            request.SetRequestHeader("Content-Type", "text/plain");
            return request.SendWebRequest();
        }
    }
}