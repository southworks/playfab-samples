using System;
using System.Runtime.InteropServices;
using System.Text;
using TicTacToe.Models;
using TicTacToe.Models.Helpers;
using UnityEngine;

namespace TicTacToe.Helpers
{
    public class PartyNetworkMessageHelper
    {
        public static void BufferData<T>(T data, out byte[] buffer, out IntPtr unmanagedPointer)
        {
            var json = JsonUtility.ToJson(data);
            buffer = Encoding.ASCII.GetBytes(json);
            var size = Marshal.SizeOf(buffer[0]) * buffer.Length;
            unmanagedPointer = Marshal.AllocHGlobal(size);
            Marshal.Copy(buffer, 0, unmanagedPointer, buffer.Length);
        }

        public static string GetStringFromBuffer(IntPtr buffer, uint bufferSize)
        {
            var managedArray = new byte[bufferSize];

            Marshal.Copy(buffer, managedArray, 0, (int)bufferSize);

            return Encoding.ASCII.GetString(managedArray);
        }

        public static T GetParsedDataFromBuffer<T>(IntPtr buffer, uint bufferSize)
        {
            try
            {
                var json = GetStringFromBuffer(buffer, bufferSize);
                return JsonUtility.FromJson<T>(json);
            }
            catch
            {
                return default;
            }
        }

        public static T GetDataFromMessageWrapper<T>(IntPtr buffer, uint bufferSize)
        {
            return GetParsedDataFromBuffer<PartyNetworkMessageWrapper<T>>(buffer, bufferSize).MessageData;
        }

        public static PartyNetworkMessageEnum GetTypeFromMessageWrapper(IntPtr buffer, uint bufferSize)
        {
            return GetParsedDataFromBuffer<PartyNetworkMessageWrapper<object>>(buffer, bufferSize).MessageType;
        }
    }
}
