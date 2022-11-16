using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Plugins.Router
{
    public class Params : Dictionary<string, string> { }

    public class Router
    {
        public class State
        {
            public Route Route;
            public Params Params;
        }

        private readonly Stack<State> _history = new();
        private RouterView _routerView;
        private IRouteComponent _currComponent;

        private readonly Dictionary<string, Route> _routes = new();

        public void Setup(VisualElement view, List<Route> routes)
        {
            _routerView = view.Q<RouterView>();
            if (_routerView == null)
            {
                Debug.LogWarning("No RouterView found");
                return;
            }

            SetupRoutes(routes);
        }

        public async Task<bool> Push(string name, Params @params = null)
        {
            if (!_routes.ContainsKey(name))
            {
                return false;
            }

            var route = _routes[name];
            var comp = route.Component;
            
            //hide & show
            Show(comp, @params);

            _history.Push(new(){Route = route, Params = @params});
            return true;
    }

        private void Show(IRouteComponent comp, Params @params)
        {
            _currComponent?.Hide();
            _routerView.Clear(); //TODO? await hide before clear?
            _routerView.Add(comp.View);
            comp.Show(@params); //TODO? await show before return
            _currComponent = comp;
        }

        public async Task<bool> Back()
        {
            if (_history.Count <= 1)
            {
                return false;
            }
            
            _history.Pop();

            var state = _history.Peek();
            Show(state.Route.Component, state.Params);
            return true;
        }

        private void SetupRoutes(List<Route> routes)
        {
            foreach (var route in routes)
            {
                _routes[route.Name] = route;
            }
        }
    }

    //TODO how to handle nesting? -> inject into (named)Router Views
    //TODO reusing components instead of rendering again (especially when only switching the child)
    //TODO -- same for back?
    //TODO navigation guard -> BeforeRouteUpdate
    //TODO -- cancel navigation -> return false in BeforeRouteUpdate,
    //TODO -- reroute navigation -> somehow via BeforeRouteUpdate (like to,from,next maybe)
    //TODO name vs path -> start with name only, don't see a benefit for path in non web
}