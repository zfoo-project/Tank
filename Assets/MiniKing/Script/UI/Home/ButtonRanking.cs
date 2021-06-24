using CsProtocol;
using Spring.Core;
using Spring.Logger;
using Summer.Net;
using UnityEngine;

namespace MiniKing.Script.UI.Home
{
    public class ButtonRanking : MonoBehaviour
    {

        public void GetScoreRank()
        {
            SpringContext.GetBean<INetManager>().Send(ScoreRankRequest.ValueOf());
        }
        
    }
}