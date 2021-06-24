using Spring.Storage.Model.Anno;

namespace MiniKing.Script.Excel
{
    [Resource]
    public class SceneResource
    {
        [Id]
        public int id;

        public string sceneAsset;

        public string backgroundMusicAsset;
    }
}