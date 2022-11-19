using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace Plugins.Router
{
    public interface IRouteComponent
    {
        public VisualElement View { get; }
        public void Show(Params @params);
        public void Hide();

        //created()?
        // public async Task<Target> BeforeRouteEnter(Target to, Target from) 
        // {
        //     return to; 
        // }
        // Before would be before Show, before Initialized ->
        // in vue: without access to this
        
        // public async Task<Target> BeforeRouteUpdate(Target to, Target from) 
        // {
        //     return to; 
        // }
        
        public async Task<Target> BeforeRouteLeave(Target to, Target from) 
        {
            return to; 
        }
        
        // public void WatchParamsChange(Params to, Params from) { }
        //TODO could also be an router event instead of always called event method
    }
}