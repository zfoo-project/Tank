using System.Collections.Generic;
using System.Linq;

namespace Summer.Editor.ResourceAnalyzer.Model
{
    public sealed class CircularDependencyChecker
    {
        private readonly Stamp[] stamps;

        public CircularDependencyChecker(Stamp[] stamps)
        {
            this.stamps = stamps;
        }

        public string[][] Check()
        {
            HashSet<string> hosts = new HashSet<string>();
            foreach (Stamp stamp in stamps)
            {
                hosts.Add(stamp.HostAssetName);
            }

            List<string[]> results = new List<string[]>();
            foreach (string host in hosts)
            {
                LinkedList<string> route = new LinkedList<string>();
                HashSet<string> visited = new HashSet<string>();
                if (Check(host, route, visited))
                {
                    results.Add(route.ToArray());
                }
            }

            return results.ToArray();
        }

        private bool Check(string host, LinkedList<string> route, HashSet<string> visited)
        {
            visited.Add(host);
            route.AddLast(host);

            foreach (Stamp stamp in stamps)
            {
                if (host != stamp.HostAssetName)
                {
                    continue;
                }

                if (visited.Contains(stamp.DependencyAssetName))
                {
                    route.AddLast(stamp.DependencyAssetName);
                    return true;
                }

                if (Check(stamp.DependencyAssetName, route, visited))
                {
                    return true;
                }
            }

            route.RemoveLast();
            visited.Remove(host);
            return false;
        }
    }
}