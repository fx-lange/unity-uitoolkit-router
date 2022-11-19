using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Plugins.Router
{
    public class Params : Dictionary<string, string> { }

    public class Router
    {
        private readonly Stack<Target> _history = new();
        private readonly RouterView _routerView;
        private IRouteComponent _currComponent;
        private Target? _curr = null;
        
        private readonly Dictionary<string, Route> _routesDict = new();

        public delegate Task<Target> BeforeEachDel(Target from, Target to);
        private BeforeEachDel _beforeEach = (_, to) => Task.FromResult(to);

        public Router(VisualElement view, List<Route> routes)
        {
            _routerView = view.Q<RouterView>();
            if (_routerView == null)
            {
                Debug.LogWarning("No RouterView found");
                return;
            }

            SetupRoutes(routes);
        }
        
        public void BeforeEachAsync(BeforeEachDel beforeEach)
        {
            _beforeEach = beforeEach;
        }
        
        public void BeforeEach(Func<Target, Target, Target> beforeEach)
        {
            _beforeEach = (from, to) => Task.FromResult(beforeEach(from, to));
        }

        public async Task<bool> Push(string name, Params @params = null)
        {
            var resultState = await DoRoute(name, @params);
            if (resultState == null)
            {
                return false;
            }

            _history.Push(resultState);
            return true;
        }

        public async Task<bool> Back()
        {
            if (_history.Count <= 1)
            {
                return false;
            }
            
            _history.Pop();

            var target = _history.Peek();
            var result = await DoRoute(target.Name, target.Params);
            return result != null;
        }

        private void SetupRoutes(List<Route> routes)
        {
            foreach (var route in routes)
            {
                _routesDict[route.Name] = route;
            }
        }
        
        private async Task<Target> DoRoute(string name, Params @params)
        {
            if (!_routesDict.ContainsKey(name))
            {
                return null;
            }
            
            var to = new Target()
            {
                Name = name, Params = @params
            };

            if (_currComponent != null)
            {
                var leaveGuard = await _currComponent.BeforeRouteLeave(to, _curr);
                if (leaveGuard == null)
                {
                    return null;
                }

                if (leaveGuard != to)
                {
                    //TODO potential endless loop
                    return await DoRoute(leaveGuard.Name, leaveGuard.Params);
                }
            }

            var globalGuard = await _beforeEach(_curr, to);
            if (globalGuard == null)
            {
                return null;
            }

            if (globalGuard != to)
            {
                return await DoRoute(globalGuard.Name, globalGuard.Params);
            }

            if (to == _curr)
            {
                //reused -> beforeUpdate guard
            }
            
            var route = _routesDict[name];
            Show(route, @params);
            return to;
        }

        private async Task Show(Route route, Params @params)
        {
            var comp = route.Component;
            _currComponent?.Hide();
            _routerView.Clear(); //TODO? await hide before clear?
            _routerView.Add(comp.View);
            comp.Show(@params); //TODO? await show before return
            _currComponent = comp;
        }
    }
}