// -----------------------------------------------------------------------
// <copyright file="Protocols.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class Protocols
    {
        private static readonly Dictionary<byte, Type> protoToTypes;
        private static readonly Dictionary<Type, byte> typeToProtos;

        static Protocols()
        {
            var typeAndIds = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract)
                .Where(t => typeof(IProtocol).IsAssignableFrom(t))
                .Select(t => new Tuple<Type, ProtoAttribute>(t, t.GetCustomAttribute<ProtoAttribute>()))
                .Where(tp => tp.Item2 != null)
                .Select(tp => new Tuple<Type, byte>(tp.Item1, tp.Item2.id));

            protoToTypes = new Dictionary<byte, Type>();
            typeToProtos = new Dictionary<Type, byte>();

            foreach (var tp in typeAndIds)
            {
                typeToProtos.Add(tp.Item1, tp.Item2);
                if (!protoToTypes.TryGetValue(tp.Item2, out var _))
                {
                    protoToTypes.Add(tp.Item2, tp.Item1);
                }
                else
                {
                    UnityEngine.Debug.LogError($"Duplicate proto id: {tp.Item2}");
                }
            }
        }

        public static byte GetId<T>()
            where T : IProtocol
        {
            return GetId(typeof(T));
        }

        public static byte GetId(Type type)
        {
            if (typeToProtos.TryGetValue(type, out var id))
            {
                return id;
            }

            throw new ELException(type.FullName);
        }

        public static Type GetType(byte id)
        {
            if (protoToTypes.TryGetValue(id, out Type type))
            {
                return type;
            }

            throw new ELException($"proto: {id}");
        }
    }

    [Serializable]
    [Proto(3)]
    public struct InstanceId : IProtocol
    {
        public byte value;

        public void Deserialize(ByteBuffer readFrom)
        {
            this.value = readFrom.ConsumeByte();
        }

        public void Serialize(ByteBuffer writeTo)
        {
            writeTo.Append(this.value);
        }
    }

    [Serializable]
    [Proto(4)]
    public struct SyncNetGameState : IProtocol
    {
        public NetGameState state;

        public void Deserialize(ByteBuffer readFrom)
        {
            this.state = (NetGameState)readFrom.ConsumeByte();
        }

        public void Serialize(ByteBuffer writeTo)
        {
            writeTo.Append((byte)this.state);
        }
    }

    [Serializable]
    [Proto(5)]
    public struct CustomBytes : IProtocol
    {
        public byte[] bytes;

        public void Deserialize(ByteBuffer readFrom)
        {
            var count = readFrom.ConsumeInt();
            if (count > 0)
            {
                this.bytes = readFrom.Consume(count);
            }
        }

        public void Serialize(ByteBuffer writeTo)
        {
            if (this.bytes != null)
            {
                writeTo.Append(this.bytes.Length);
                writeTo.Append(this.bytes);
            }
            else
            {
                writeTo.Append(0);
            }
        }
    }
}
