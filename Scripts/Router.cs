using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace UITK.Router
{
    public class Params : Dictionary<string, string>
    {
    }

    [CreateAssetMenu]
    public partial class Router : ScriptableObject
    {
        public bool HasHistory => _history.Count > 1;

        private RouterView _routerView;

        private class NestedRoute : Route
        {
            public List<NestedRoute> NestingList;

            public NestedRoute(Route route)
            {
                Name = route.Name;
                Component = route.Component;
                Components = route.Components;
                Children = route.Children;
            }
        }

        private readonly Dictionary<string, NestedRoute> _routes = new();

        private NestedRoute _currentRoute;
        private NavTarget _currTarget = null;

        private readonly Stack<NavTarget> _history = new();

        private Action<NavTarget, NavTarget> _afterEach = delegate { };

        public event Action<NavTarget, NavTarget> AfterEach
        {
            add => _afterEach += value;
            remove => _afterEach -= value;
        }

        public void Setup(VisualElement view, List<Route> routes)
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

            return;

            void SetupRoute(Route route, NestedRoute parent)
            {
                var nestedRoute = new NestedRoute(route);
                
                if (parent != null)
                {
                    nestedRoute.NestingList = new List<NestedRoute>(parent.NestingList);
                }
                else
                {
                    nestedRoute.NestingList = new List<NestedRoute>();
                }

                nestedRoute.NestingList.Add(nestedRoute);

                //TODO check for duplicates? different collection type?
                _routes[nestedRoute.Name] = nestedRoute;

                if (nestedRoute.Children == null) return;
                foreach (var child in nestedRoute.Children)
                {
                    SetupRoute(child, nestedRoute);
                }
            }
        }

        public async Task<bool> Push(string name, Params @params = null)
        {
            var fromTarget = _currTarget;
            var toTarget = await ProcessRoute(name, @params);
            if (toTarget == null || toTarget == _currTarget)
            {
                return false;
            }

            //route confirmed (uninterruptible)

            var nestedRoute = _routes[toTarget.Name];
            ResolveRoute(nestedRoute, @params);

            _currTarget = toTarget;
            _history.Push(toTarget);

            _afterEach(toTarget, fromTarget);
            return true;
        }

        public async Task<bool> Back()
        {
            if (_history.Count <= 0)
            {
                return false;
            }

            _history.Pop();

            var from = _currTarget;
            var target = _history.Peek();
            target = await ProcessRoute(target.Name, target.Params);
            if (target == null)
            {
                return false;
            }

            //route confirmed (uninterruptible)

            var route = _routes[target.Name];
            ResolveRoute(route, target.Params);
            _currTarget = target;
            _afterEach(target, from);
            return true;
        }


        private async Task<NavTarget> ProcessRoute(string name, Params @params)
        {
            if (!_routes.ContainsKey(name))
            {
                Debug.LogWarning($"Route {name} not found");
                return null;
            }

            var targetRoute = _routes[name];
            var target = new NavTarget()
            {
                Name = name,
                Params = @params,
            };

            var guardResult = await CheckGuards(target, _currTarget);
            return guardResult;

            async Task<NavTarget> CheckGuards(NavTarget to, NavTarget from)
            {
                //leave guards
                if (_currentRoute != null)
                {
                    foreach (var routeNode in _currentRoute.NestingList)
                    {
                        var leaveGuard = await routeNode.Component.BeforeRouteLeave(to, from);
                        if (leaveGuard == null) return null;
                        if (leaveGuard != to)
                        {
                            return await ProcessRoute(leaveGuard.Name, leaveGuard.Params);
                        }
                    }
                }

                //global beforeEach guards
                foreach (var beforeEach in _beforeEach.Guards)
                {
                    var globalGuardTarget = await beforeEach(to, from);
                    if (globalGuardTarget == null) return null;
                    if (globalGuardTarget != to)
                    {
                        return await ProcessRoute(globalGuardTarget.Name, globalGuardTarget.Params);
                    }
                }

                //per (reused) component beforeUpdate guards
                if (_currentRoute == null)
                {
                    return to;
                }

                List<Route> toBeActivated = new();
                foreach (var targetRouteNode in targetRoute.NestingList)
                {
                    var reuse = _currentRoute.NestingList.Any(currentRouteStep =>
                        currentRouteStep.Component == targetRouteNode.Component);

                    if (!reuse)
                    {
                        toBeActivated.Add(targetRouteNode);
                        continue;
                    }

                    var updateGuard = await targetRouteNode.Component.BeforeRouteUpdate(to, from);
                    if (updateGuard == null) return null;
                    if (updateGuard != to)
                    {
                        return await ProcessRoute(updateGuard.Name, updateGuard.Params);
                    }
                }

                //per (activated) component beforeEnter guards
                foreach (var routeNode in toBeActivated)
                {
                    var enterGuard = await routeNode.Component.BeforeRouteEnter(to, from);
                    if (enterGuard == null) return null;
                    if (enterGuard != to)
                    {
                        return await ProcessRoute(enterGuard.Name, enterGuard.Params);
                    }
                }

                //global beforeResolve guards
                foreach (var beforeResolve in _beforeResolve.Guards)
                {
                    var globalGuardTarget = await beforeResolve(to, from);
                    if (globalGuardTarget == null) return null;
                    if (globalGuardTarget != to)
                    {
                        return await ProcessRoute(globalGuardTarget.Name, globalGuardTarget.Params);
                    }
                }

                return to;
            }
        }

        private bool ResolveRoute(NestedRoute nestedRoute, Params @params)
        {
            if (_currentRoute != null)
            {
                for (int i = 0; i < _currentRoute.NestingList.Count; ++i)
                {
                    var currRouteNodeComponent = _currentRoute.NestingList[i].Component;
                    bool stays = nestedRoute.NestingList.Count > i && nestedRoute.NestingList[i].Component == currRouteNodeComponent;
                    if (!stays)
                    {
                        currRouteNodeComponent.Deactivate();
                        // currRouteNodeComponent.View?
                        // //TODO await? OR await ALL
                        //TODO clear which routerView? -> no but only needed if we want to clear/remove 
                    }
                }
            }

            RouterView routerView = _routerView;
            foreach (var route in nestedRoute.NestingList)
            {
                if (routerView == null)
                {
                    Debug.LogWarning("Missing nested RouterView");
                    break;
                }

                var routeNodeComponent = route.Component;
                if (routeNodeComponent == null)
                {
                    Debug.LogWarning($"{route.Name} without Component");
                    return false;
                }

                // routerView.Clear(); 

                routeNodeComponent.Activate(@params); //TODO? await show before return OR await ALL
                var nestedView = routeNodeComponent.View;
                routerView.Add(nestedView);

                routerView = nestedView.Q<RouterView>();
            }

            _currentRoute = nestedRoute;
            return true;
        }
    }
}

//next & ideas
/*
 * cancel nav until after beforeResolve
 * RouteNames : SO nested inside Router
 * RouteNames : TypedEnum (dependency)
 * TypedEnum.ToString() as route name (combine)
 * RouteAction abstraction uxml query+route (params how?)
 * route guards handling modals
 * clear() -> push() or push(clear:true) or clearpush() or up()? or replace()
 * header via nesting or via guards?
 * uitk performance: hide/show vs intantiate vs add/remove
 * multitasking vs multithreading with async/await -> remove/hide on transition end
 */