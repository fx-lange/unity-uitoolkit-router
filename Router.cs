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
        private readonly RouterView _routerView;
        
        private class NestedRoute
        {
            public Route Route;
            public List<NestedRoute> Hierarchy;
        }
        private readonly Dictionary<string, NestedRoute> _routesDict = new();

        private NestedRoute _currentRoute;
        private IRouteComponent _currComponent; //TODO redundant 
        private NavTarget _currTarget = null;

        public delegate Task<NavTarget> BeforeEachDel(NavTarget to, NavTarget from);
        private BeforeEachDel _beforeEach = (to, _) => Task.FromResult(to);

        private readonly Stack<NavTarget> _history = new();
        
        public Router(VisualElement view, List<Route> routes)
        {
            _routerView = view.Q<RouterView>();
            if (_routerView == null)
            {
                Debug.LogWarning("No RouterView found");
                return;
            }

            foreach (var route in routes)
            {
                SetupRoute(route, null);
            }

            void SetupRoute(Route route, NestedRoute parent)
            {
                var nestedRoute = new NestedRoute()
                {
                    Route = route
                };

                if (parent != null )
                {
                    nestedRoute.Hierarchy = new List<NestedRoute>(parent.Hierarchy);
                }
                else
                {
                    nestedRoute.Hierarchy = new List<NestedRoute>();
                }
                nestedRoute.Hierarchy.Add( nestedRoute );
                
                //TODO check for duplicates? different collection type?
                _routesDict[route.Name] = nestedRoute;
                
                foreach (var child in route.Children)
                {
                    SetupRoute(child, nestedRoute);
                }
                
                //TODO precache route.comp.view?
            }
        }

        public void BeforeEachAsync(BeforeEachDel beforeEach)
        {
            _beforeEach = beforeEach;
        }
        
        public void BeforeEach(Func<NavTarget, NavTarget, NavTarget> beforeEach)
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

            _currTarget = resultState;

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

        
        private async Task<NavTarget> DoRoute(string name, Params @params)
        {
            if (!_routesDict.ContainsKey(name))
            {
                return null;
            }
            
            var route = _routesDict[name];

            var target = new NavTarget()
            {
                Name = name, Params = @params
            };

            var guardResult = await CheckGuards(target, _currTarget);
            if (guardResult == null) return guardResult;

            await Show(route.Route, @params);
            return guardResult;
            
            async Task<NavTarget> CheckGuards(NavTarget to, NavTarget from)
            {
                //leave guards
                if (_currentRoute != null)
                {
                    foreach (var routeNode in _currentRoute.Hierarchy)
                    {
                        var leaveGuard = await routeNode.Route.Component.BeforeRouteLeave(to, from);
                        if (leaveGuard == null) return null;
                        if (leaveGuard != to)
                        {
                            return await DoRoute(leaveGuard.Name, leaveGuard.Params);
                        }
                    }
                }
                
                //global
                var globalGuard = await _beforeEach(to, from);
                if (globalGuard == null) return null;
                if (globalGuard != to)
                {
                    return await DoRoute(globalGuard.Name, globalGuard.Params);
                }
                
                //update
                if (to == _currTarget)//TODO not just if same target but rather same components 
                {
                    //reused -> beforeUpdate guard
                }

                for (var i = 0; i < _currentRoute.Hierarchy.Count; i++)
                {
                    var currRouteNode = _currentRoute.Hierarchy[i].Route;
                    if (route.Hierarchy.Count <= i)
                    {
                        break;
                    }

                    var targetRouteNode = route.Hierarchy[i].Route;
                    if (currRouteNode.Component == targetRouteNode.Component)
                    {
                        var updateGuard = await currRouteNode.Component.BeforeRouteUpdate(to, from);
                        if (updateGuard == null) return null;
                        if (updateGuard != to)
                        {
                            return await DoRoute(updateGuard.Name, updateGuard.Params);
                        }
                    }
                }

                return to;
            }
        }

        private async Task Show(Route route, Params @params)
        {
            //TODO nesting
            var comp = route.Component;
            _currComponent?.Hide();
            _routerView.Clear(); //TODO? await hide before clear?
            _routerView.Add(comp.View);
            comp.Show(@params); //TODO? await show before return
            _currComponent = comp;
        }
    }
}