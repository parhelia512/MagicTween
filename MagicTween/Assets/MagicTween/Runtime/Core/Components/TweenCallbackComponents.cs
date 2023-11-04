using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace MagicTween.Core.Components
{
    public sealed class TweenDelegates<T> : IComponentData, IDisposable
    {
        public TweenGetter<T> getter;
        public TweenSetter<T> setter;

        public TweenDelegates() { }

        // Implement pooling using Dispose of Managed Component.
        // This is not the expected use of Dispose, but it works.
        public void Dispose()
        {
            TweenDelegatesPool<T>.Return(this);
        }
    }

    // Since it is difficult to use generics due to ECS restrictions,
    // use UnsafeUtility.As to force assignment of delegates when creating components.
    // So the type of the target and the type of the TweenGetter/TweenSetter argument must match absolutely,
    // otherwise undefined behavior will result.
    public sealed class TweenDelegatesNoAlloc<T> : IComponentData, IDisposable
    {
        [HideInInspector] public object target;
        [HideInInspector] public TweenGetter<object, T> getter;
        [HideInInspector] public TweenSetter<object, T> setter;

        public TweenDelegatesNoAlloc() { }

        public void Dispose()
        {
            TweenDelegatesNoAllocPool<T>.Return(this);
        }
    }

    public sealed class TweenCallbackActions : IComponentData, IDisposable
    {
        public Action onStart;
        public Action onPlay;
        public Action onPause;
        public Action onUpdate;
        public Action onStepComplete;
        public Action onComplete;
        public Action onKill;

        // internal callback
        public Action onRewind;

        public void Dispose()
        {
            TweenCallbackActionsPool.Return(this);
        }

        internal bool HasAction()
        {
            if (onStart != null) return true;
            if (onPlay != null) return true;
            if (onPause != null) return true;
            if (onUpdate != null) return true;
            if (onStepComplete != null) return true;
            if (onComplete != null) return true;
            if (onKill != null) return true;
            if (onRewind != null) return true;
            return false;
        }
    }

    public sealed class TweenCallbackActionsNoAlloc : IComponentData, IDisposable
    {
        public class FastAction
        {
            struct Item
            {
                public object target;
                public Action<object> action;
            }

            Item[] _items = new Item[4];
            int _count;

            public int Count => _count;

            public void Add(object target, Action<object> action)
            {
                if (_items.Length == _count)
                {
                    Array.Resize(ref _items, _count * 2);
                }

                _items[_count] = new Item()
                {
                    target = target,
                    action = action
                };
                _count++;
            }

            public void Invoke()
            {
                for (int i = 0; i < _items.Length; i++)
                {
                    if (i == _count) return;
                    var item = _items[i];
                    item.action?.Invoke(item.target);
                }
            }

            public void Clear()
            {
                for (int i = 0; i < _items.Length; i++)
                {
                    _items[i] = default;
                }
                _count = 0;
            }
        }

        public readonly FastAction onStart = new();

        public readonly FastAction onPlay = new();
        public readonly FastAction onPause = new();
        public readonly FastAction onUpdate = new();
        public readonly FastAction onStepComplete = new();
        public readonly FastAction onComplete = new();
        public readonly FastAction onKill = new();

        // internal callback
        public readonly FastAction onRewind = new();

        public void Dispose()
        {
            TweenCallbackActionsNoAllocPool.Return(this);
        }

        internal bool HasAction()
        {
            if (onStart.Count > 0) return true;
            if (onPlay.Count > 0) return true;
            if (onPause.Count > 0) return true;
            if (onUpdate.Count > 0) return true;
            if (onStepComplete.Count > 0) return true;
            if (onComplete.Count > 0) return true;
            if (onKill.Count > 0) return true;
            if (onRewind.Count > 0) return true;
            return false;
        }
    }
}