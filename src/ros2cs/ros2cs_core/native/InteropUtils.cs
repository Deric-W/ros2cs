using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ROS2.Native
{
    /// <summary>
    /// Utilities for native interop.
    /// </summary>
    internal static class InteropUtils
    {
        /// <summary>
        /// Encoding used for converting strings.
        /// </summary>
        private static readonly Encoding Encoding = Encoding.UTF8;

        /// <summary>
        /// Convert a string to the format used by the rcl.
        /// </summary>
        /// <param name="str"> String to convert. </param>
        /// <returns> Null terminated buffer. </returns>
        internal static byte[] ToRcl(this string str)
        {
            byte[] buffer = new byte[Encoding.GetByteCount(str) + 1];
            Encoding.GetBytes(str, 0, str.Length, buffer, buffer.GetLowerBound(0));
            buffer[buffer.GetUpperBound(0)] = 0;
            return buffer;
        }

        internal delegate RclReturnCode CreateOptions(out IntPtr options);

        internal delegate RclReturnCode CreateHandle(IntPtr options, out IntPtr handle);

        /// <summary>
        /// Create a handle with its options while handling errors.
        /// </summary>
        /// <returns> Pointer to options and rcl object. </returns>
        internal static (IntPtr Options, IntPtr Handle) CreateHandleWithOptions(CreateOptions createOptions, CreateHandle createHandle, Func<IntPtr, RclReturnCode> freeOptions)
        {
            createOptions(out IntPtr options).Throw();
            try
            {
                createHandle(options, out IntPtr handle).Throw();
                return (options, handle);
            }
            catch (Exception)
            {
                freeOptions(options).Throw();
                throw;
            }
        }

        /// <summary>
        /// Throw an exception when the return code signals an error.
        /// </summary>
        /// <param name="ret"> Return code to check. </param>
        internal static void Throw(this RclReturnCode ret)
        {
            if (ret == RclReturnCode.RCL_RET_OK)
            {
                return;
            }
            string msg = TakeLastError();
            RclError exception = new RclError(ret, msg);
            switch (ret)
            {
                case RclReturnCode.RCL_RET_OK:
                    break;
                case RclReturnCode.RCL_RET_NODE_INVALID_NAME:
                    throw new InvalidNodeNameException(msg, exception);
                case RclReturnCode.RCL_RET_NODE_INVALID_NAMESPACE:
                    throw new InvalidNamespaceException(msg, exception);
                case RclReturnCode.RCL_RET_WAIT_SET_EMPTY:
                    throw new WaitSetEmptyException(msg, exception);
                case RclReturnCode.RCL_RET_NOT_INIT:
                case RclReturnCode.RCL_RET_NODE_INVALID:
                case RclReturnCode.RCL_RET_PUBLISHER_INVALID:
                case RclReturnCode.RCL_RET_SUBSCRIPTION_INVALID:
                case RclReturnCode.RCL_RET_CLIENT_INVALID:
                case RclReturnCode.RCL_RET_SERVICE_INVALID:
                case RclReturnCode.RCL_RET_WAIT_SET_INVALID:
                    throw new ObjectDisposedException(msg, exception);
                default:
                    throw exception;
            }
        }

        private static string TakeLastError()
        {
            string msg = Marshal.PtrToStringAnsi(ros2cs_native_get_last_error());
            rcutils_reset_error();
            return msg;
        }

        [DllImport(
            "ros2cs_native",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr ros2cs_native_get_last_error();

        [DllImport(
            "rcutils",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void rcutils_reset_error();
    }
}