using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace UITK.Router
{
    public interface IRoutable
    {
        public VisualElement View { get; }
        public void Show(Params @params);
        public void Hide();

        //created()? Init(params?) Init() instead of show
        
        // public async Task<Target> BeforeRouteEnter(Target to, Target from) 
        // {
        //     return to; 
        // }
        // Before would be before Show, before Initialized ->
        // in vue: without access to this -> static
        
        public Task<NavTarget> BeforeRouteUpdate(NavTarget to, NavTarget from) 
        {
            return Task.FromResult(to); 
        }
        
        public Task<NavTarget> BeforeRouteLeave(NavTarget to, NavTarget from) 
        {
            return Task.FromResult(to); 
        }
    }
}