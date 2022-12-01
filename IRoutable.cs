using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace Plugins.Router
{
    public interface IRoutable
    {
        public VisualElement View { get; }
        public void Show(Params @params);
        public void Hide();

        //created()? Init(params?) Init() instead of show, what is hide supposed to do actually
        
        // public async Task<Target> BeforeRouteEnter(Target to, Target from) 
        // {
        //     return to; 
        // }
        // Before would be before Show, before Initialized ->
        // in vue: without access to this
        
        public async Task<NavTarget> BeforeRouteUpdate(NavTarget to, NavTarget from) 
        {
            return to; 
        }
        
        public async Task<NavTarget> BeforeRouteLeave(NavTarget to, NavTarget from) 
        {
            return to; 
        }
    }
}