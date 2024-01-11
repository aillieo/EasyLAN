using System;
using System.Threading;
using System.Threading.Tasks;

namespace AillieoUtils.EasyLAN
{
    internal static class NetConnectionExtensions
    {
        internal async static Task<T> ReceiveProto<T>(this NetConnection connection, CancellationToken cancellationToken)
            where T : IProtocol
        {
            TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T>();

            void onData(ByteBuffer buffer)
            {
                var bytes = buffer.ToArray();
                buffer.Clear();
                if (SerializeUtils.Deserialize(bytes, out IProtocol o))
                {
                    if (o is T proto)
                    {
                        connection.onData -= onData;
                        taskCompletionSource.SetResult(proto);
                    }
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

        internal async static Task SendProto<T>(this NetConnection connection, T obj, CancellationToken cancellationToken)
            where T : IProtocol
        {
            ByteBuffer buffer = new ByteBuffer(128);
            byte[] bytes = SerializeUtils.Serialize(obj);
            buffer.Append(bytes);
            await connection.SendAsync(buffer, cancellationToken);
        }

        internal async static Task<R> RequestProto<T, R>(this NetConnection connection, T request, CancellationToken cancellationToken)
            where T : IProtocol
            where R : IProtocol
        {
            await connection.SendProto(request, cancellationToken);
            return await connection.ReceiveProto<R>(cancellationToken);
        }
    }
}
