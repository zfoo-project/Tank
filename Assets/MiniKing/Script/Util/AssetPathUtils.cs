using Spring.Util;

namespace MiniKing.Script.Util
{
    public abstract class AssetPathUtils
    {
        public static readonly string MAIN_FONT_ASSET_PATH = "Source_Han_Sans_SDF";

        public static string GetSceneAsset(string assetName)
        {
            return StringUtils.Format("Assets/Tank/Scene/{}.unity", assetName);
        }

        public static string GetMusicAsset(string assetName)
        {
            return StringUtils.Format("Assets/MiniKing/Sound/{}", assetName);
        }

        public static string GetEntityAsset(string assetName)
        {
            return StringUtils.Format("Assets/MiniKing/Entitie/{}.prefab", assetName);
        }


        public static string GetUISoundAsset(string assetName)
        {
            return StringUtils.Format("Assets/MiniKing/UI/UISound/{}.wav", assetName);
        }
    }
}