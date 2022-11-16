using System.Collections.Generic;

namespace Plugins.Router
{
    public class Route
    {
        public string Name;
        public IRouteComponent Component;
        public Dictionary<string, IRouteComponent> Components;
        public List<Route> Children;
    }
}