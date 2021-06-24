using NUnit.Framework;
using Spring.Collection.Tree;
using Spring.Logger;
using Spring.Util.Json;

namespace Test.Editor.Spring.Collection
{
    public class TreeDataTest
    {
        [Test]
        public void treeDataTest()
        {
            var treeData = new MultiwayTree();
            treeData.SetData("a", "hi");
            treeData.SetData("a.b", "hello");
            treeData.SetData("a.b.c", "world1");
            treeData.SetData("a.b.d", "world2");
            treeData.SetData("a.b.e", "world3");
            Log.Info(treeData.GetData("a.b"));
            Log.Info(JsonUtils.object2String(treeData.GetNode("a.b").GetAllChild()));
        }
    }
}