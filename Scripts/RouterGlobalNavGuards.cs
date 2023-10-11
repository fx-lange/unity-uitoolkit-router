using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace UITK.Router
{
    public partial class Router
    {
        private GlobalNavGuards _beforeEach = new();
        private GlobalNavGuards _beforeResolve = new();

        public delegate NavTarget NavGuardDelegate(NavTarget to, NavTarget from);
        public delegate Task<NavTarget> NavGuardDelegateAsync(NavTarget to, NavTarget from);

        public event NavGuardDelegateAsync BeforeEachAsync
        {
            add => _beforeEach.BeforeAsync += value;
            remove => _beforeEach.BeforeAsync -= value;
        }

        public event NavGuardDelegate BeforeEach
        {
            add => _beforeEach.Before += value;
            remove => _beforeEach.Before -= value;
        }

        public event NavGuardDelegateAsync BeforeResolveAsync
        {
            add => _beforeResolve.BeforeAsync += value;
            remove => _beforeResolve.BeforeAsync -= value;
        }

        public event NavGuardDelegate BeforeResolve
        {
            add => _beforeResolve.Before += value;
            remove => _beforeResolve.Before -= value;
        }

        private void ClearGlobalGuards()
        {
            _beforeEach = new GlobalNavGuards();
            _beforeResolve = new GlobalNavGuards();
        }

        private class GlobalNavGuards
        {
            private List<NavGuardDelegateAsync> _list = new();

            private Dictionary<NavGuardDelegate, NavGuardDelegateAsync>
                _wrappedDelegates = new();

            public IEnumerable<NavGuardDelegateAsync> Guards
            {
                get => new ReadOnlyCollection<NavGuardDelegateAsync>(_list);
            }

            public event NavGuardDelegateAsync BeforeAsync
            {
                add => _list.Add(value);
                remove => _list.Remove(value);
            }

            public event NavGuardDelegate Before
            {
                add
                {
                    NavGuardDelegateAsync wrapped = (from, to) => Task.FromResult(value(from, to));
                    _list.Add(wrapped);
                    _wrappedDelegates[value] = wrapped;
                }

                remove
                {
                    if (_wrappedDelegates.TryGetValue(value, out var wrapped))
                    {
                        _list.Remove(wrapped);
                        _wrappedDelegates.Remove(value);
                    }
                }
            }
        }
    }
}