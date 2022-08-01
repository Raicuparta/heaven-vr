using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace BoneworksLIV.AvatarTrackers
{
    [StructLayout(LayoutKind.Sequential)]
    public struct test
    {
        public float value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AngStruct1
    {
        public float Val1;
        public int Val2;
    }

    public struct NJBuffer
    {
        public IntPtr addr;
        public int length;
        public int type;
        public static int _type = 39;
    }

    // Contains an unmanaged pointer to a nojson object (copy of existing, or new if not found)
    public struct NJPointer
    {
        public IntPtr addr;
        public static int _type = (int)PathfinderType.Pointer;
    }

    public struct PFPointer
    {
        public IntPtr addr;
        public static int _type = (int)PathfinderType.Pointer;
    }

    public class PObject : IDisposable
    {
        private PFPointer _pathfinderPointer;

        public PObject()
        {

        }

        public PObject(string key)
        {
            // get the pointer from the bridge
            SDKBridgePathfinder.GetValue(key, out _pathfinderPointer, (int)PathfinderType.Pointer);
        }

        ~PObject()
        {
            Dispose();
        }

        public bool IsValid => _pathfinderPointer.addr != IntPtr.Zero;

        public bool LoadKey(string key)
        {
            // get the pointer from the bridge
            SDKBridgePathfinder.GetValue(key, out _pathfinderPointer, (int)PathfinderType.Pointer);

            // return if we got a pointer
            return _pathfinderPointer.addr != IntPtr.Zero;
        }

        public PObject this[string key]
        {
            get
            {
                return new PObject(key);
            }
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(_pathfinderPointer.addr);
        }

        //public T Value<T>()
        //{
        //    return Value<T>(string.Empty);
        //}

        //public T Value<T>(string key)
        //{
        //    switch (Type.GetTypeCode(typeof(T)))
        //    {
        //        case TypeCode.String:
        //            var result = "";
        //            SDKBridge.GetString(key, ref result);
        //            return (T)Convert.ChangeType(result, typeof(T));
        //        case TypeCode.Boolean:
        //            T boolVal;
        //            SDKBridge.GetValue(key, out boolVal, (int)PathfinderType.Boolean);
        //            return boolVal;
        //        case TypeCode.Int32:
        //            T intVal;
        //            SDKBridge.GetValue<T>(key, out intVal, (int)PathfinderType.Int);
        //            return intVal;
        //        case TypeCode.Single:
        //            T singleVal;
        //            SDKBridge.GetValue<T>(key, out singleVal, (int)PathfinderType.Float);
        //            return singleVal;
        //        case TypeCode.Object:
        //            if (typeof(T) == typeof(PObject))
        //            {
        //                //var pObject = new PObject(key);
        //                //return (T)pObject;
        //            }
        //            break;
        //        default:
        //            T value;
        //            SDKBridge.GetValue<T>(key, out value, (int)Type.GetTypeCode(typeof(T)));
        //            return value;
        //    }
        //}
    }

    public enum PathfinderType
    {
        Container = 0,
        Pointer = 40,
        Buffer = 39,
        String = 36,
        Int = 9,
        Float = 13,
        Texture = 27,
        Quaternion = 8,
        Matrix4x4 = 6,
        Vector3 = 30,
        RigidTransform = 38,
        Boolean = 42,
        CopyPath = 251,
        Struct = 252
    }

    public class SDKBridgePathfinder
    {

        // Types: 
        // 00 = Nojson container
        // 36 = String (UTF8 encoding implied)
        // 09 = Int32 (Likely to be renumbered)
        // 13 = Float32 

        [DllImport("LIV_Bridge", EntryPoint = "LivSDKSet")]
        [SuppressUnmanagedCodeSecurity]
        private static extern int SetValue(string path, IntPtr value, int valuelength, int valuetype);

        //[DllImport("LIV_Bridge", EntryPoint = "LivSDKSet")]
        //[SuppressUnmanagedCodeSecurity]
        //private static extern int SetVect3(string path, [In,Out] vect3 value, int valuelength, int valuetype);

        [DllImport("LIV_Bridge", EntryPoint = "LivSDKGet")]
        [SuppressUnmanagedCodeSecurity]
        private static extern int GetValue(string path, IntPtr value, int valuelength, int valuetype);

        //[DllImport("LIV_Bridge", EntryPoint = "LivSDKGet")]
        //[SuppressUnmanagedCodeSecurity]
        //private static extern int GetVect3(string path, [In,Out] vect3 value, int valuelength, int valuetype);

        [DllImport("LIV_Bridge", EntryPoint = "NJSet")]
        [SuppressUnmanagedCodeSecurity]
        public static extern int njset(IntPtr nj, string path, IntPtr value, int valuelength, int valuetype);

        [DllImport("LIV_Bridge", EntryPoint = "NJGet")]
        [SuppressUnmanagedCodeSecurity]
        public static extern int njget(IntPtr nj, string path, IntPtr value, int valuelength, int valuetype);

        [DllImport("LIV_Bridge", CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr NJGetSmall(IntPtr nj, string path, int type);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern int memcmp(IntPtr intPtr1, IntPtr intPtr2, UIntPtr count);

        public static bool CompareNJPointer(NJPointer nj1, NJPointer nj2)
        {
            // check cases where pointer is zero for both or either
            if (nj1.addr == IntPtr.Zero && nj2.addr == IntPtr.Zero) return true;
            if (nj1.addr == IntPtr.Zero && nj2.addr != IntPtr.Zero) return false;
            if (nj1.addr != IntPtr.Zero && nj2.addr == IntPtr.Zero) return false;

            // read the first 2 bytes to get the length of each object, as per pathfinder, length starts at the pointer
            var len1 = Marshal.ReadInt16(nj1.addr);
            var len2 = Marshal.ReadInt16(nj2.addr);

            // if lengths are different, then the two objects are different
            if (len1 != len2)
            {
                return false;
            }

            // perform memory compare of objects
            var retVal = memcmp(nj1.addr, nj2.addr, (UIntPtr)len1);
            return retVal == 0;
        }

        public static NJPointer CreatePathfinderObject(int size = 65536)
        {
            var addr = Marshal.AllocHGlobal(size);
            Marshal.WriteInt32(addr, 262148);
            var pfPointer = new NJPointer();
            pfPointer.addr = addr;
            return pfPointer;
        }

        public static void InitializeRootNode(string rootNode, string pathCheck)
        {
            var pfCheck = SDKBridgePathfinder.AsPFPointer(pathCheck);
            if (pfCheck.addr == IntPtr.Zero)
            {
                InitializeRootNode(rootNode);
            }
        }

        public static void InitializeRootNode(string rootNode)
        {
            var pf = SDKBridgePathfinder.CreatePathfinderObject();
            SDKBridgePathfinder.SetPFObject(rootNode, pf);
            SDKBridgePathfinder.FreePFPointer(pf);
        }

        public static string GetNJPointerKey(NJPointer np)
        {
            if (np.addr == IntPtr.Zero) return null;

            if (Marshal.ReadByte(np.addr + 2) == 4)
            {
                return string.Empty;
            }

            var bytes = new List<byte>();
            var loc = np.addr + 4;

            while (true)
            {
                var charBuffer = Marshal.ReadByte(loc);
                if (charBuffer == 1) break;
                bytes.Add(charBuffer);
                loc += 1;
            }

            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        public static object GetNJPointerValue(NJPointer np)
        {
            if (np.addr == IntPtr.Zero) return null;

            var type = Marshal.ReadByte(np.addr + 3);

            var dataOffset = Marshal.ReadByte(np.addr + 2) & ((1 << 6) - 1);
            var dataStart = np.addr + dataOffset;

            var obj = new object();

            if (type == 36)
            {
                var data = Marshal.PtrToStringAnsi(dataStart);
                obj = data;
            }
            else if (type == 0)
            {
                var njPointer = new NJPointer { addr = dataStart };
                obj = njPointer;
            }
            else
            {
                if (type == 38) type = 14;

                var dataType = Type.GetType("System." + Enum.GetName(typeof(TypeCode), type));
                obj = Marshal.PtrToStructure(dataStart, dataType);
            }

            return obj;
        }

        public static bool GetValue<T>(string key, out T mystruct, int structtype)
        {
            // C# managed code thunking - kind of annoying - can probably be made better - but this will do for initial implementation

            // Allocate T on stack (unmanaged)
            // pin it - mainly so we can get its address
            // pass it to getvalue (which will initialize its contents)
            // copy the value to mystruct target

            T value = default(T);

            GCHandle gc2 = GCHandle.Alloc(value, GCHandleType.Pinned);

            var result = GetValue(key, gc2.AddrOfPinnedObject(), Marshal.SizeOf(value), structtype);

            mystruct = (T)gc2.Target;

            gc2.Free();

            return result != 0;
        }

        //public static bool SetVect3(string key, vect3 v)
        //{
        //    var result = SetVect3(key, v, 12, 30);
        //    return result != 0;
        //}

        //public static bool GetVect3(string key, out vect3 v)
        //{
        //    v = default;
        //    var result = GetVect3(key, v, 12, 30);
        //    return result != 0;
        //}

        public static bool SetValue<T>(string key, ref T mystruct, int structtype)
        {
            GCHandle gc1 = GCHandle.Alloc(mystruct, GCHandleType.Pinned);
            var result = SetValue(key, gc1.AddrOfPinnedObject(), Marshal.SizeOf(mystruct), structtype);
            gc1.Free();
            return result != 0;
        }

        public static bool SetValue<T>(NJPointer njPointer, string key, ref T structValue, PathfinderType structType)
        {
            var resultHandle = GCHandle.Alloc(structValue, GCHandleType.Pinned);
            var result = njset(njPointer.addr, key, resultHandle.AddrOfPinnedObject(), Marshal.SizeOf(structValue), (int)structType);

            resultHandle.Free();

            return result != 0;
        }

        public static bool SetPFObject(string key, NJPointer njPointer)
        {
            var result = SetValue(key, njPointer.addr, Marshal.ReadInt16(njPointer.addr), (int)PathfinderType.Container);

            return result != 0;
        }

        public static bool SetPFObject(NJPointer njOwner, string key, NJPointer njPointer)
        {
            var result = njset(njOwner.addr, key, njPointer.addr, Marshal.ReadInt16(njPointer.addr), (int)PathfinderType.Container);

            return result != 0;
        }

        public static bool CopyPath(string key, string path)
        {
            var utf8 = Encoding.UTF8;

            byte[] utfstring = utf8.GetBytes(path + char.MinValue);

            GCHandle gc1 = GCHandle.Alloc(utfstring, GCHandleType.Pinned);
            var result = SetValue(key, gc1.AddrOfPinnedObject(), utfstring.Length, (int)PathfinderType.CopyPath);
            gc1.Free();
            return result != 0;
        }

        public static bool SetString(string key, string mystring)
        {
            var utf8 = Encoding.UTF8;

            byte[] utfstring = utf8.GetBytes(mystring + char.MinValue);

            GCHandle gc1 = GCHandle.Alloc(utfstring, GCHandleType.Pinned);
            var result = SetValue(key, gc1.AddrOfPinnedObject(), utfstring.Length, (int)PathfinderType.String);
            gc1.Free();
            return result != 0;
        }

        public static bool GetString(string key, ref string mystring)
        {

            NJBuffer buf = default;
            GCHandle gc1 = GCHandle.Alloc(buf, GCHandleType.Pinned);
            var result = GetValue(key, gc1.AddrOfPinnedObject(), Marshal.SizeOf(buf), NJBuffer._type);
            if (result != 0)
            {
                mystring = Marshal.PtrToStringAnsi(((NJBuffer)gc1.Target).addr);
            }
            gc1.Free();

            return result != 0;
        }

        public static bool GetString(NJPointer njPointer, string key, ref string mystring)
        {
            NJBuffer buf = default;
            GCHandle gc1 = GCHandle.Alloc(buf, GCHandleType.Pinned);
            var result = njget(njPointer.addr, key, gc1.AddrOfPinnedObject(), Marshal.SizeOf(buf), NJBuffer._type);
            if (result != 0)
            {
                mystring = Marshal.PtrToStringAnsi(((NJBuffer)gc1.Target).addr);
            }

            gc1.Free();

            return result != 0;
        }

        public static bool SetString(NJPointer njPointer, string key, string mystring)
        {
            var utf8 = Encoding.UTF8;

            byte[] utfstring = utf8.GetBytes(mystring + char.MinValue);

            GCHandle gc1 = GCHandle.Alloc(utfstring, GCHandleType.Pinned);
            var result = njset(njPointer.addr, key, gc1.AddrOfPinnedObject(), utfstring.Length, (int)PathfinderType.String);
            gc1.Free();
            return result != 0;

        }

        public static bool GetValue<T>(NJPointer njPointer, string key, out T mystruct, int structtype)
        {
            T value = default(T);

            GCHandle gc2 = GCHandle.Alloc(value, GCHandleType.Pinned);

            var result = njget(njPointer.addr, key, gc2.AddrOfPinnedObject(), Marshal.SizeOf(value), structtype);

            mystruct = (T)gc2.Target;

            gc2.Free();

            return result != 0;
        }
        public static string AsString(string key)
        {
            string result = "";
            GetString(key, ref result);
            return result;
        }


        public static string AsString(NJPointer np, string key)
        {
            string result = "";
            GetString(np, key, ref result);
            return result;
        }

        public static int AsInt(string key)
        {
            int value;
            GetValue(key, out value, (int)PathfinderType.Int);
            return value;
        }

        public static int AsInt(NJPointer nj, string key)
        {
            int value;
            GetValue(nj, key, out value, (int)PathfinderType.Int);
            return value;
        }

        public static int AsIntQuick(string key)
        {
            var val = NJGetSmall(IntPtr.Zero, key, (int)PathfinderType.Int);
            var j = (long)val & 4294967295;
            return (int)j;
        }

        public static int AsIntQuick(NJPointer nj, string key)
        {
            var val = NJGetSmall(nj.addr, key, (int)PathfinderType.Int);
            var j = (long)val & 4294967295;
            return (int)j;
        }

        public static float AsFloat(string key)
        {
            float value;
            GetValue(key, out value, (int)PathfinderType.Float);
            return value;
        }

        public static float AsFloat(NJPointer nj, string key)
        {
            float value;
            GetValue(nj, key, out value, (int)PathfinderType.Float);
            return value;
        }

        public static bool AsBool(string key)
        {
            int value;
            GetValue(key, out value, (int)PathfinderType.Boolean);
            return value != 0;
        }

        public static bool AsBool(NJPointer nj, string key)
        {
            int value;
            GetValue(nj, key, out value, (int)PathfinderType.Boolean);
            return value != 0;
        }

        public static NJPointer AsPFPointer(string key)
        {
            var ptr = NJGetSmall(IntPtr.Zero, key, 40);
            var value = new NJPointer { addr = ptr };
            return value;
        }

        public static NJPointer AsPFPointer(NJPointer pf, string key)
        {
            var ptr = NJGetSmall(pf.addr, key, 40);
            var value = new NJPointer {addr = ptr};
            return value;

            //GetValue(pf, key, out value, (int)PathfinderType.Pointer);
            //return value;
        }

        public static void FreePFPointer(NJPointer njPointer)
        {
            Marshal.FreeHGlobal(njPointer.addr);
        }
    }

    public class NJCursor
    {
        //var utf8 = Encoding.UTF8;
        //byte[] utfkey = utf8.GetBytes(key + char.MinValue);
        //GCHandle gch = GCHandle.Alloc(utfkey, GCHandleType.Pinned);

        //byte[] utfstring = utf8.GetBytes(mystring);

        private byte[] utfKey;

        public string Key
        {
            get => Encoding.UTF8.GetString(utfKey);
            set => utfKey = Encoding.UTF8.GetBytes(value + char.MinValue);
        }

        public T Value<T>()
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.String:
                    var result = "";
                    SDKBridgePathfinder.GetString(Key, ref result);
                    return (T)Convert.ChangeType(result, typeof(T));
                case TypeCode.Boolean:
                    T boolVal;
                    SDKBridgePathfinder.GetValue<T>(Key, out boolVal, (int)PathfinderType.Boolean);
                    return boolVal;
                default:
                    T value;
                    SDKBridgePathfinder.GetValue<T>(Key, out value, (int)Type.GetTypeCode(typeof(T)));
                    return value;
            }
        }

        public string AsString() => Value<string>();
        public int AsInt() => Value<int>();
        public bool AsBool() => Value<bool>();
        public float AsFloat() => Value<float>();

        public bool Exists()
        {
            var v = string.Empty;
            return SDKBridgePathfinder.GetString(Key, ref v);
        }

        public void Set<T>(T value)
        {

        }

        public object this[string key]
        {
            get => string.Empty;
            set
            {
                var v = value;
            }
        }
    }

    public class LIVSDK
    {
        //public string this[string index]
        //{
        //    get { return SDKBridge.AsString(index); }
        //    set { SDKBridge.SetString(index, value); }
        //}
        //public int this[string index, int _default]
        //{
        //    get { return SDKBridge.AsInt(index); }
        //    set { SDKBridge.SetValue(index, ref value, 9); }
        //}

        public NJCursor this[string index]
        {
            get
            {
                var cursor = new NJCursor
                {
                    Key = index
                };

                return cursor;
            }
        }

        public void EnableFeature(string feature)
        {
            int flagValue = 1;
            SDKBridgePathfinder.SetValue($"LIV.features.{feature}", ref flagValue, 9);
        }

        public void DisableFeature(string feature)
        {
            int flagValue = 0;
            SDKBridgePathfinder.SetValue($"LIV.features.{feature}", ref flagValue, 9);
        }
    }

    public static class SDKFeature
    {
        public const string ScanForDevices = "scanfordevices";
        public const string DebugFrames = "debugframes";
        public const string QuestService = "questservice";
        public const string Watermark = "watermark";
    }

    public class NoJSON
    {

    }
}
