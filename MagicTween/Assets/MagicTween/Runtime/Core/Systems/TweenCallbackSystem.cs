using System;
using System.Runtime.CompilerServices;
using Unity.Entities;
using MagicTween.Core.Components;
using MagicTween.Diagnostics;
using System.Collections.Generic;

namespace MagicTween.Core
{
    [UpdateInGroup(typeof(MagicTweenCallbackSystemGroup))]
    public sealed partial class TweenCallbackSystem : SystemBase
    {
        public bool IsExecuting => _isExecuting;

        bool _isExecuting;
        EntityQuery query1;
        EntityQuery query2;

        protected override void OnCreate()
        {
            query1 = SystemAPI.QueryBuilder()
                .WithAspect<TweenAspect>()
                .WithAll<TweenCallbackActions>()
                .Build();
            query2 = SystemAPI.QueryBuilder()
                .WithAspect<TweenAspect>()
                .WithAll<TweenCallbackActionsNoAlloc>()
                .Build();
        }

        protected override void OnUpdate()
        {
            _isExecuting = true;
            try
            {
                CompleteDependency();
                var job1 = new SystemJob1();
                job1.Run(query1);
                var job2 = new SystemJob2();
                job2.Run(query2);
            }
            finally
            {
                _isExecuting = false;
            }
        }

        partial struct SystemJob1 : IJobEntity
        {
            public void Execute(TweenCallbackActions actions, in TweenCallbackFlags callbackFlags)
            {
                if ((callbackFlags.flags & CallbackFlags.OnStart) == CallbackFlags.OnStart) TryInvoke(actions.onStart);
                if ((callbackFlags.flags & CallbackFlags.OnPlay) == CallbackFlags.OnPlay) TryInvoke(actions.onPlay);
                if ((callbackFlags.flags & CallbackFlags.OnPause) == CallbackFlags.OnPause) TryInvoke(actions.onPause);
                if ((callbackFlags.flags & CallbackFlags.OnUpdate) == CallbackFlags.OnUpdate) TryInvoke(actions.onUpdate);
                if ((callbackFlags.flags & CallbackFlags.OnRewind) == CallbackFlags.OnRewind) TryInvoke(actions.onRewind);
                if ((callbackFlags.flags & CallbackFlags.OnStepComplete) == CallbackFlags.OnStepComplete) TryInvoke(actions.onStepComplete);
                if ((callbackFlags.flags & CallbackFlags.OnComplete) == CallbackFlags.OnComplete) TryInvoke(actions.onComplete);
                if ((callbackFlags.flags & CallbackFlags.OnKill) == CallbackFlags.OnKill) TryInvoke(actions.onKill);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void TryInvoke(Action action)
            {
                try { action?.Invoke(); }
                catch (Exception ex) { Debugger.LogExceptionInsideTween(ex); }
            }
        }

        partial struct SystemJob2 : IJobEntity
        {
            public void Execute(TweenCallbackActionsNoAlloc actions, in TweenCallbackFlags callbackFlags)
            {
                if ((callbackFlags.flags & CallbackFlags.OnStart) == CallbackFlags.OnStart) TryInvoke(actions.onStart);
                if ((callbackFlags.flags & CallbackFlags.OnPlay) == CallbackFlags.OnPlay) TryInvoke(actions.onPlay);
                if ((callbackFlags.flags & CallbackFlags.OnPause) == CallbackFlags.OnPause) TryInvoke(actions.onPause);
                if ((callbackFlags.flags & CallbackFlags.OnUpdate) == CallbackFlags.OnUpdate) TryInvoke(actions.onUpdate);
                if ((callbackFlags.flags & CallbackFlags.OnRewind) == CallbackFlags.OnRewind) TryInvoke(actions.onRewind);
                if ((callbackFlags.flags & CallbackFlags.OnStepComplete) == CallbackFlags.OnStepComplete) TryInvoke(actions.onStepComplete);
                if ((callbackFlags.flags & CallbackFlags.OnComplete) == CallbackFlags.OnComplete) TryInvoke(actions.onComplete);
                if ((callbackFlags.flags & CallbackFlags.OnKill) == CallbackFlags.OnKill) TryInvoke(actions.onKill);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void TryInvoke(List<(object, Action<object>)> actions)
            {
                try
                {
                    for (int i = 0; i < actions.Count; i++)
                    {
                        var action = actions[i];
                        action.Item2.Invoke(action.Item1);
                    }
                }
                catch (Exception ex) { Debugger.LogExceptionInsideTween(ex); }
            }
        }
    }
}