// -----------------------------------------------------------------------
// <copyright file="NetConnectionExtensions.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class NetConnectionExtensions
    {
        internal static async Task<T> ReceiveProto<T>(this NetConnection connection, CancellationToken cancellationToken)
            where T : IProtocol, new()
        {
            var taskCompletionSource = new TaskCompletionSource<T>();

            void onData(ByteBuffer buffer)
            {
                var proto = buffer.ConsumeByte();
                if (proto == Protocols.GetId<T>())
                {
                    var type = Protocols.GetType(proto);

                    var o = new T();
                    o.Deserialize(buffer);

                    connection.onData -= onData;
                    taskCompletionSource.SetResult(o);
                }
                else
                {
                    buffer.Prepend(proto);
                }
            }

            connection.onData += onData;

            var recevie = taskCompletionSource.Task;

            try
            {
                await recevie.SetTimeout(3000);
            }
            catch (TimeoutException)
            {
                UnityEngine.Debug.LogError($"timeout receive {typeof(T)}");
                throw;
            }
            finally
            {
                connection.onData -= onData;
            }

            cancellationToken.ThrowIfCancellationRequested();

            return await recevie;
        }

        internal static async Task SendProto<T>(this NetConnection connection, T obj, CancellationToken cancellationToken)
            where T : IProtocol
        {
            var buffer = new ByteBuffer(128);
            var proto = Protocols.GetId<T>();
            buffer.Append(proto);
            obj.Serialize(buffer);
            await connection.SendAsync(buffer, cancellationToken);
        }

        internal static async Task<R> RequestProto<T, R>(this NetConnection connection, T request, CancellationToken cancellationToken)
            where T : IProtocol
            where R : IProtocol, new()
        {
            await connection.SendProto(request, cancellationToken);
            return await connection.ReceiveProto<R>(cancellationToken);
        }
    }
}
