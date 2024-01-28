// -----------------------------------------------------------------------
// <copyright file="SerializeUtils.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN.Sample
{
    using System;
    using System.Text;
    using UnityEngine;

    public static class SerializeUtils
    {
        public static byte[] Serialize<T>(T obj)
        {
            try
            {
                var head = obj.GetType().FullName;
                var json = JsonUtility.ToJson(obj);
                UnityEngine.Debug.Log($"[Ser] {head} {json}");

                ByteBuffer buffer = new ByteBuffer(0);
                buffer.Append(head);
                buffer.Append(json);

                var bytes = buffer.ToArray();
                buffer.Clear();

                UnityEngine.Debug.Log($"[Ser] {bytes.ToStringEx()}");

                return bytes;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                return null;
            }
        }

        public static bool Deserialize(byte[] bytes, out object obj)
        {
            try
            {
                ByteBuffer buffer = new ByteBuffer(bytes);

                UnityEngine.Debug.Log($"[Des] {bytes.ToStringEx()}");

                var head = buffer.ConsumeString();
                var json = buffer.ConsumeString();

                UnityEngine.Debug.Log($"[Des] {head} {json}");

                var type = Type.GetType(head);
                obj = JsonUtility.FromJson(json, type);
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                obj = default;
                return false;
            }
        }
    }
}
