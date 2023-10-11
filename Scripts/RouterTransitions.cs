using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace UITK.Router
{
    public partial class Router
    {
        private readonly int _scheduleDelay = 10;

        private readonly string _suffixEnterActive = "-enter-active";
        private readonly string _suffixEnterFrom = "-enter";
        private readonly string _suffixEnterTo = "-enter-to";

        private readonly string _suffixLeaveActive = "-leave-active";
        private readonly string _suffixLeaveFrom = "-leave";
        private readonly string _suffixLeaveTo = "-leave-to";

        private bool _useTransition;

        private string _enterActiveClass;
        private string _enterFromClass;
        private string _enterToClass;
        private string _leaveActiveClass;
        private string _leaveFromClass;
        private string _leaveToClass;

        private void SetTransitionName(string transitionName)
        {
            _enterActiveClass = transitionName + _suffixEnterActive;
            _enterFromClass = transitionName + _suffixEnterFrom;
            _enterToClass = transitionName + _suffixEnterTo;
            _leaveActiveClass = transitionName + _suffixLeaveActive;
            _leaveFromClass = transitionName + _suffixLeaveFrom;
            _leaveToClass = transitionName + _suffixLeaveTo;
        }

        private async Task RunLeaveTransition(IRoutable component)
        {
            var ve = component.View;
            if (!_useTransition)
            {
                Hide(ve);
                component.Deactivate();
                return;
            }
            
            var finished = await ExecuteTransitionSequence(ve, _leaveFromClass, _leaveActiveClass, _leaveToClass);
            if (finished)
            {
                Hide(ve);
                component.Deactivate();
            }
        }

        private async Task RunEnterTransition(IRoutable component)
        {
            var ve = component.View;
            UnHide(ve);

            if (!_useTransition)
            {
                return;
            }
            
            var finished = await ExecuteTransitionSequence(ve, _enterFromClass, _enterActiveClass, _enterToClass);
            if (!finished)
            {
                // ve.AddToClassList("hide");
            }
        }

        private void Hide(VisualElement ve)
        {
            ve.style.display = DisplayStyle.None;
        }

        private void UnHide(VisualElement ve)
        {
            ve.style.display = StyleKeyword.Undefined;
        }

        private async Task<bool> ExecuteTransitionSequence(VisualElement ve, string fromClass, string activeClass,
            string toClass)
        {
            TaskCompletionSource<bool> tcs = new();

            // Debug.Log($"EXECUTE {fromClass}");
            var started = false;
            EventCallback<TransitionRunEvent> transitionStarted = null;
            EventCallback<TransitionEndEvent> transitionEnd = null;
            EventCallback<TransitionCancelEvent> transitionCancel = null;

            //(1 frame=0) set from 
            ve.AddToClassList(fromClass);
            ve.schedule.Execute(_ =>
            {
                // Debug.Log($"frame=1");
                //(2 frame=1) active transition
                ve.AddToClassList(activeClass);
                ve.schedule.Execute(_ =>
                {
                    // Debug.Log($"frame=2");
                    //(3 frame=2) set to and remove from
                    ve.RemoveFromClassList(fromClass);
                    ve.AddToClassList(toClass);

                    ve.schedule.Execute(_ =>
                    {
                        // foreach (var @class in ve.GetClasses())
                        // {
                        //     Debug.Log(@class);
                        // }

                        if (started) return;

                        Debug.LogWarning("ABORT, no transition started");
                        ve.UnregisterCallback(transitionStarted);
                        ve.RemoveFromClassList(activeClass);
                        ve.RemoveFromClassList(toClass);

                        tcs.SetResult(true);
                    }).StartingIn(10);
                }).StartingIn(_scheduleDelay);
            }).StartingIn(_scheduleDelay);


            transitionEnd = _ =>
            {
                //(4 after transition) remove all
                Debug.Log($"END {fromClass}");
                ve.RemoveFromClassList(activeClass);
                ve.RemoveFromClassList(toClass);
                ve.UnregisterCallback(transitionEnd);
                ve.UnregisterCallback(transitionCancel);

                tcs.SetResult(true);
            };

            transitionCancel = _ =>
            {
                //(4b cancel transition) 
                Debug.Log($"CANCEL {fromClass}");
                ve.RemoveFromClassList(activeClass);
                ve.RemoveFromClassList(toClass);
                ve.UnregisterCallback(transitionEnd);
                ve.UnregisterCallback(transitionCancel);
                tcs.SetResult(false);
            };

            transitionStarted = _ =>
            {
                Debug.Log($"STARTED {fromClass}");
                started = true;
                ve.RegisterCallback(transitionEnd);
                ve.RegisterCallback(transitionCancel);
                ve.UnregisterCallback(transitionStarted);
            };

            ve.RegisterCallback(transitionStarted);

            return await tcs.Task;
        }
    }
}