using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace TikSync
{
    public class NativeCoreTikSync : IDisposable
    {
        private IntPtr _ptr;
        private static bool _loaded;

        static NativeCoreTikSync()
        {
            try
            {
                var test = tiksync_init(IntPtr.Zero, 0);
                if (test != IntPtr.Zero) tiksync_free(test);
                _loaded = true;
            }
            catch
            {
                _loaded = false;
            }
        }

        public static bool IsAvailable => _loaded;

        public NativeCoreTikSync(string apiKey)
        {
            if (!_loaded) return;
            var bytes = Encoding.UTF8.GetBytes(apiKey);
            var pinned = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                _ptr = tiksync_init(pinned.AddrOfPinnedObject(), bytes.Length);
            }
            finally
            {
                pinned.Free();
            }
        }

        public Dictionary<string, string> GetConnectHeaders()
        {
            if (_ptr == IntPtr.Zero) return new Dictionary<string, string>();

            var jsonPtr = tiksync_get_headers_json(_ptr);
            if (jsonPtr == IntPtr.Zero) return new Dictionary<string, string>();

            var json = Marshal.PtrToStringUTF8(jsonPtr);
            tiksync_free_string(jsonPtr);

            if (string.IsNullOrEmpty(json)) return new Dictionary<string, string>();

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                    ?? new Dictionary<string, string>();
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        public void Dispose()
        {
            if (_ptr != IntPtr.Zero)
            {
                tiksync_free(_ptr);
                _ptr = IntPtr.Zero;
            }
            GC.SuppressFinalize(this);
        }

        ~NativeCoreTikSync() { Dispose(); }

        [DllImport("tiksync_core", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tiksync_init(IntPtr apiKeyPtr, int apiKeyLen);

        [DllImport("tiksync_core", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tiksync_free(IntPtr ptr);

        [DllImport("tiksync_core", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tiksync_get_headers_json(IntPtr ptr);

        [DllImport("tiksync_core", CallingConvention = CallingConvention.Cdecl)]
        private static extern void tiksync_free_string(IntPtr ptr);
    }
}
