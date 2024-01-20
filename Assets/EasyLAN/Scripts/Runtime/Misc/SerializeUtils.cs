// -----------------------------------------------------------------------
// <copyright file="SerializeUtils.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System;
    using System.Text;
    using UnityEngine;

    internal static class SerializeUtils
    {
        public static byte[] Serialize<T>(T obj)
            where T : IProtocol
        {
            try
            {
                var head = obj.GetType().FullName;
                var headBytes = Encoding.UTF8.GetBytes(head);
                var json = JsonUtility.ToJson(obj);
                var jsonBytes = Encoding.UTF8.GetBytes(json);
                var headLength = headBytes.Length;
                var length = sizeof(int) + headLength + json.Length;
                var bytes = new byte[sizeof(int) + length];
                var lengthBytes = BitConverter.GetBytes(length);
                var headLengthBytes = BitConverter.GetBytes(headLength);
                var index = 0;

                //UnityEngine.Debug.Log("[S] " + head);
                //UnityEngine.Debug.Log("[S] " + headLength);
                UnityEngine.Debug.Log("[S] " + json);
                //UnityEngine.Debug.Log("[S] " + length);

                Array.Copy(lengthBytes, 0, bytes, index, lengthBytes.Length);
                index += lengthBytes.Length;
                Array.Copy(headLengthBytes, 0, bytes, index, headLengthBytes.Length);
                index += headLengthBytes.Length;
                Array.Copy(headBytes, 0, bytes, index, headBytes.Length);
                index += headBytes.Length;
                Array.Copy(jsonBytes, 0, bytes, index, jsonBytes.Length);

                //UnityEngine.Debug.Log("[S] " + string.Join(",", bytes.Select(b=> b.ToString())));

                return bytes;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                return null;
            }
        }

        public static bool Deserialize(byte[] bytes, out IProtocol obj)
        {
            try
            {
                var index = 4;
                var headLength = BitConverter.ToInt32(bytes, index);
                index += sizeof(int);
                var head = Encoding.UTF8.GetString(bytes, index, headLength);
                //UnityEngine.Debug.Log("[D] " + head);
                index += headLength;
                var json = Encoding.UTF8.GetString(bytes, index, bytes.Length - index);
                UnityEngine.Debug.Log("[D] " + json);
                var type = Type.GetType(head);
                obj = JsonUtility.FromJson(json, type) as IProtocol;
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
