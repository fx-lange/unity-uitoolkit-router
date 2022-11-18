using UnityEngine.UIElements;

namespace Plugins.Router
{
    public interface IRouteComponent
    {
        public VisualElement View { get; }
        public void Show(Params @params);
        public void Hide();

        //created()?
        public bool BeforeRouteUpdate(Router.State to, Router.State from) 
        {
            return true; //Nav Guard
        }
        
        // public void WatchParamsChange(Params to, Params from) { }
        //TODO could also be an router event instead of always called event method
    }
}