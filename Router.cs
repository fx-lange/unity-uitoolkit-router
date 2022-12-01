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
        private NavTarget _currTarget = null;

        public delegate Task<NavTarget> BeforeEachDel(NavTarget to, NavTarget from);
        private BeforeEachDel _beforeEach = (to, _) => Task.FromResult(to);

        private readonly Stack<NavTarget> _history = new();
        
        public Router(VisualElement view, List<Route> routes)
        {
            _routerView = view.Q<RouterView>();
            if (_routerView == null)
            {
                Debug.LogWarning("View without RouterView");
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
                //TODO precache route.comp.view?

                if (route.Children == null) return;
                foreach (var child in route.Children)
                {
                    SetupRoute(child, nestedRoute);
                }
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
            var target = await DoRoute(name, @params);
            if (target == null || target == _currTarget)
            {
                return false;
            }

            var nestedRoute = _routesDict[target.Name];
            await Show(nestedRoute, @params);
            
            _currTarget = target;
            _history.Push(target);
            return true;
        }

        public async Task<bool> Back()
        {
            if (_history.Count <= 0)
            {
                return false;
            }

            _history.Pop();

            var target = _history.Peek();
            var result = await DoRoute(target.Name, target.Params);
            if (result == null) return false;

            var route = _routesDict[target.Name];
            await Show(route, target.Params);
            return true;
        }

        
        private async Task<NavTarget> DoRoute(string name, Params @params)
        {
            if (!_routesDict.ContainsKey(name))
            {
                Debug.LogWarning($"Route {name} not found");
                return null;
            }
            
            var targetRoute = _routesDict[name];
            var target = new NavTarget()
            {
                Name = name, 
                Params = @params,
            };

            var guardResult = await CheckGuards(target, _currTarget);
            if (guardResult == null) return null;
            
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
                
                //global beforeEach guard
                var globalGuard = await _beforeEach(to, from);
                if (globalGuard == null) return null;
                if (globalGuard != to)
                {
                    return await DoRoute(globalGuard.Name, globalGuard.Params);
                }
                
                //per component beforeUpdate guard
                if (_currentRoute == null)
                {
                    return to;
                }
                for (var i = 0; i < _currentRoute.Hierarchy.Count; i++)
                {
                    var currRouteNode = _currentRoute.Hierarchy[i].Route;
                    if (targetRoute.Hierarchy.Count <= i)
                    {
                        break;
                    }

                    var targetRouteNode = targetRoute.Hierarchy[i].Route;
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

        private async Task<bool> Show(NestedRoute nestedRoute, Params @params)
        {
            if (_currentRoute != null)
            {
                for (int i = 0; i < _currentRoute.Hierarchy.Count; ++i)
                {
                    var currRouteNodeComponent = _currentRoute.Hierarchy[i].Route.Component;
                    // bool stays = route.Hierarchy.Count > i && route.Hierarchy[i].Route.Component == currRouteNodeComponent;
                    // if (!stays)
                    // {
                    //      hide & clear
                    //      //TODO reverse order better?
                    // }
                    currRouteNodeComponent.Hide(); //TODO await? OR await ALL
                    //TODO clear which routerView?
                }
            }

            RouterView routerView = _routerView;
            for (int i = 0; i < nestedRoute.Hierarchy.Count; ++i)
            {
                if (routerView == null)
                {
                    Debug.LogWarning("Missing nested RouterView");
                    break;
                }

                var route = nestedRoute.Hierarchy[i].Route;
                var routeNodeComponent = route.Component;
                if (routeNodeComponent == null)
                {
                    Debug.LogWarning($"{route.Name} without Component");
                    return false;
                }
                routerView.Clear();
                routeNodeComponent.Show(@params);//TODO? await show before return OR await ALL
                var nestedView = routeNodeComponent.View;
                routerView.Add(nestedView);
                
                routerView = nestedView.Q<RouterView>();
            }

            _currentRoute = nestedRoute;
            return true;
        }
    }
}