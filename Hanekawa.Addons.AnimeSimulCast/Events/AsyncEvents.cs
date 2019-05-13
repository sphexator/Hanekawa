﻿using System.Linq;
using System.Threading.Tasks;

namespace Hanekawa.Addons.AnimeSimulCast.Events
{
    public delegate Task AsyncEvent<in T1>(T1 arg1);

    public delegate Task AsyncEvent<in T1, in T2>(T1 arg1, T2 arg2);

    public delegate Task AsyncEvent<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);

    public delegate Task AsyncEvent<in T1, in T2, in T3, in T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    public static class AsyncEvents
    {
        public static Task InvokeAsync<T1>(this AsyncEvent<T1> asyncEvent, T1 arg1)
        {
            if (asyncEvent == null)
                return Task.CompletedTask;

            var events = asyncEvent.GetInvocationList().Cast<AsyncEvent<T1>>();
            var eventTasks = events.Select(it => it.Invoke(arg1));

            return Task.WhenAll(eventTasks);
        }

        public static Task InvokeAsync<T1, T2>(this AsyncEvent<T1, T2> asyncEvent, T1 arg1, T2 arg2)
        {
            if (asyncEvent == null)
                return Task.CompletedTask;

            var events = asyncEvent.GetInvocationList().Cast<AsyncEvent<T1, T2>>();
            var eventTasks = events.Select(it => it.Invoke(arg1, arg2));

            return Task.WhenAll(eventTasks);
        }

        public static Task InvokeAsync<T1, T2, T3>(this AsyncEvent<T1, T2, T3> asyncEvent, T1 arg1, T2 arg2, T3 arg3)
        {
            if (asyncEvent == null)
                return Task.CompletedTask;

            var events = asyncEvent.GetInvocationList().Cast<AsyncEvent<T1, T2, T3>>();
            var eventTasks = events.Select(it => it.Invoke(arg1, arg2, arg3));

            return Task.WhenAll(eventTasks);
        }

        public static Task InvokeAsync<T1, T2, T3, T4>(this AsyncEvent<T1, T2, T3, T4> asyncEvent, T1 arg1, T2 arg2,
            T3 arg3, T4 arg4)
        {
            if (asyncEvent == null)
                return Task.CompletedTask;

            var events = asyncEvent.GetInvocationList().Cast<AsyncEvent<T1, T2, T3, T4>>();
            var eventTasks = events.Select(it => it.Invoke(arg1, arg2, arg3, arg4));

            return Task.WhenAll(eventTasks);
        }
    }
}