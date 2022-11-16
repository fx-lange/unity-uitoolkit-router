using UnityEngine.UIElements;

namespace Plugins.Router
{
    public interface IRouteComponent
    {
        public VisualElement View { get; }
        public void Show(Params @params);
        public void Hide();

        public bool BeforeRouteUpdate(Router.State to, Router.State from) 
        {
            return true;
        }
        
        public void WatchParamsChange(Params to, Params from) { }
    }
}