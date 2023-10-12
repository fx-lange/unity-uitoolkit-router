using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace UITK.Router
{
    public class Params : Dictionary<string, object>
    {
    }

    [CreateAssetMenu]
    public partial class Router : ScriptableObject
    {
        public bool HasHistory => _history.Count > 0;
        public int Depth => _currentRoute!=null ? _currentRoute.NestingList.Count - 1 : 0;
        private RouterView _routerView;

        private class NestedRoute : Route
        {
            public List<NestedRoute> NestingList;
            public NestedRoute Parent;
            
            public NestedRoute(Route route)
            {
                Name = route.Name;
                Component = route.Component;
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

        public void Setup(VisualElement view, List<Route> routes, bool useTransitions, string transitionName = "fade")
        {
            Clear();
            _useTransition = useTransitions;
            if (useTransitions)
            {
                SetTransitionName(transitionName);
            }

            _routerView = view.Q<RouterView>();
            if (_routerView == null)
            {
                Debug.LogWarning("View without RouterView");
                return;
            }

            foreach (var route in routes)
            {
                SetupRoute(route, null, _routerView);
            }

            return;

            void SetupRoute(Route route, NestedRoute parent, RouterView routerView)
            {
                if (route.Component == null)
                {
                    Debug.LogWarning("Route without component: Skipping");
                    return;
                }
                
                var nestedRoute = new NestedRoute(route)
                {
                    Parent = parent
                };
                
                var componentView = nestedRoute.Component.View;
                if (componentView == null)
                {
                    Debug.LogError($"Component for {nestedRoute.Name} not set up properly: View is null");
                    return;
                }

                if (!routerView.Contains(componentView))
                {
                    routerView.Add(componentView);
                }
                Hide(componentView);

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
                var nestedRouterView = componentView.Q<RouterView>();
                if (nestedRouterView == null)
                {
                    Debug.LogError($"Nesting for {nestedRoute.Name} not set up properly: Missing nested RouterView");
                    return;
                }
                
                foreach (var child in nestedRoute.Children)
                {
                    SetupRoute(child, nestedRoute, nestedRouterView);
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
            var success = await ResolveRoute(nestedRoute, @params);

            if (!success)
            {
                return false;
            }

            _currTarget = toTarget;
            _history.Push(fromTarget);

            _afterEach(toTarget, fromTarget);
            return true;
        }

        public async Task<bool> Back()
        {
            if (_history.Count == 0)
            {
                return false;
            }

            var from = _currTarget;
            var target = _history.Pop();
            target = await ProcessRoute(target.Name, target.Params);
            if (target == null)
            {
                return false;
            }

            //route confirmed (uninterruptible)

            var route = _routes[target.Name];
            var success = await ResolveRoute(route, target.Params);
            if (!success)
            {
                return false;
            }

            _currTarget = target;
            _afterEach(target, from);
            return true;
        }
        
        public async Task<bool> Up()
        {
            var from = _currTarget;
            var parentRoute = _currentRoute.Parent;

            if (parentRoute == null)
            {
                Debug.LogWarning("Can't go up");
                return false;
            }

            var backTarget = _history.Peek();
            if (backTarget.Name == parentRoute.Name)
            {
                return await Back(); //reusing potential params
            }

            var target = await ProcessRoute(parentRoute.Name, new Params());
            if (target == null)
            {
                return false;
            }

            //route confirmed (uninterruptible)

            var route = _routes[target.Name];
            var success = await ResolveRoute(route, target.Params);
            if (!success)
            {
                return false;
            }

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

        private async Task<bool> ResolveRoute(NestedRoute nestedRoute, Params @params)
        {
            List<Task> leaveTasks = new();
            if (_currentRoute != null)
            {
                for (int i = 0; i < _currentRoute.NestingList.Count; ++i)
                {
                    var currRouteNodeComponent = _currentRoute.NestingList[i].Component;
                    bool stays = nestedRoute.NestingList.Count > i &&
                                 nestedRoute.NestingList[i].Component == currRouteNodeComponent;
                    if (!stays)
                    {
                        leaveTasks.Add(RunLeaveTransition(currRouteNodeComponent));
                    }
                }
            }

            await Task.WhenAll(leaveTasks);

            List<Task> enterTasks = new();
            RouterView routerView = _routerView;
            foreach (var route in nestedRoute.NestingList)
            {
                if (routerView == null)
                {
                    Debug.LogError("Missing nested RouterView");
                    break;
                }

                var routeNodeComponent = route.Component;
                if (routeNodeComponent == null)
                {
                    Debug.LogError($"{route.Name} without Component");
                    return false;
                }

                //don't animate/activate active components 
                //todo use active flag?
                if (_currentRoute == null || _currentRoute.NestingList.All(r => r.Component != routeNodeComponent))
                {
                   routeNodeComponent.Activate(@params);
                   enterTasks.Add(RunEnterTransition(routeNodeComponent));
                }

                routerView = routeNodeComponent.View.Q<RouterView>();
            }

            await Task.WhenAll(enterTasks);

            _currentRoute = nestedRoute;
            return true;
        }

        private void Clear()
        {
            _currentRoute = null;
            _currTarget = null;
            _history.Clear();
            _routes.Clear();
            ClearGlobalGuards();
            _afterEach = delegate { };
        }
    }
}