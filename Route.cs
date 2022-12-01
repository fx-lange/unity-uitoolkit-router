using System.Collections.Generic;

namespace Plugins.Router
{
    public class Route
    {
        public string Name;
        public IRoutable Component;
        public Dictionary<string, IRoutable> Components;
        public List<Route> Children;
    }
}