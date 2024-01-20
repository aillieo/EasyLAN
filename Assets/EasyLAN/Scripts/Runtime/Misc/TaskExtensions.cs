// -----------------------------------------------------------------------
// <copyright file="TaskExtensions.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class TaskExtensions
    {
        public static async void Await(this Task task)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var stackTrace = new StackTrace();
            try
            {
                await task;
            }
            catch (Exception e)
            {
                var msg = $"{e}\n{stackTrace}";
                UnityEngine.Debug.LogError(msg);
                throw;
            }
#else
            await task;
#endif
        }

        public static async Task SetTimeout(this Task task, int millisecondsDelay)
        {
            using (var tcs = new CancellationTokenSource())
            {
                var timeout = Task.Delay(millisecondsDelay, tcs.Token);
                var complete = await Task.WhenAny(timeout, task);
                if (complete == timeout)
                {
                    tcs.Cancel();
                    throw new TimeoutException();
                }
            }
        }

        public static async Task<T> SetTimeout<T>(this Task<T> task, int millisecondsDelay)
        {
            using (var tcs = new CancellationTokenSource())
            {
                var timeout = Task.Delay(millisecondsDelay, tcs.Token);
                var complete = await Task.WhenAny(timeout, task);
                if (complete == timeout)
                {
                    tcs.Cancel();
                    throw new TimeoutException();
                }

                return await task;
            }
        }
    }
}
