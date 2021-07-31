using System;
using System.Collections.Generic;
using CsProtocol;
using Spring.Util.Property;

namespace MiniKing.Script.Common
{
    /**
     * 玩家自己的数据
     */
    public class PlayerData
    {
        /**
         * 临时变量
         */
        public static Pair<string, string> tempPairSs;

        public static bool loginFlag;
        public static bool loginError;
        
        public static PlayerInfo playerInfo;

        public static CurrencyVo currencyVo;
        
        public static List<RankInfo> ranks;

    }
}