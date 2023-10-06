using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace UITK.Router
{
    public interface IRoutable
    {
        public VisualElement View { get; }
        
        public Task<NavTarget> BeforeRouteEnter(NavTarget to, NavTarget from) 
        {
            return Task.FromResult(to); 
        }
        // in vue.js: without access to this -> static?
        
        public Task<NavTarget> BeforeRouteUpdate(NavTarget to, NavTarget from) 
        {
            return Task.FromResult(to); 
        }
        
        public void Show(Params @params);

        public Task<NavTarget> BeforeRouteLeave(NavTarget to, NavTarget from) 
        {
            return Task.FromResult(to); 
        }
        
        public void Hide();
    }
}