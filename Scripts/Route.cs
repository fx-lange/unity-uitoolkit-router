using System.Collections.Generic;

namespace UITK.Router
{
    public class Route
    {
        public string Name;
        public IRoutable Component;
        // public Dictionary<string, IRoutable> Components; named views are not supported yet
        public List<Route> Children;
    }
}