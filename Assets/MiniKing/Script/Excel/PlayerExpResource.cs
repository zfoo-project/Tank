using Spring.Storage.Model.Anno;

namespace MiniKing.Script.Excel
{
    [Resource]
    public class PlayerExpResource
    {
        [Id]
        public int playerLevel;

        public int exp;

        public int gem;

        public int gold;
    }
}