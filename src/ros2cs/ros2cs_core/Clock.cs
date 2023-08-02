// Copyright 2019-2021 Robotec.ai
// Copyright 2019 Dyno Robotics (by Samuel Lindgren samuel@dynorobotics.se)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Runtime.InteropServices;
using ROS2.Native;

namespace ROS2
{
    /// <summary> A simple structure to hold seconds and nanoseconds </summary>
    /// <description> This is meant to be an intermediate data object before time is packed into
    /// a rosgraph clock message or into a different format native to application layer </description>
    public readonly struct RosTime
    {
        public readonly long Seconds;

        public readonly uint Nanoseconds;

        public double TotalSeconds
        {
            get => this.Seconds + this.Nanoseconds / 1e9;
        }

        public RosTime(long sec, uint nanosec)
        {
            this.Seconds = sec;
            this.Nanoseconds = nanosec;
        }
    }

    /// <summary> A clock class which queries an internal rcl clock and exposes RosTime </summary>
    public class Clock : IExtendedDisposable
    {
        private const uint NANOS_PER_SECOND = 1_000_000_000;

        public bool IsDisposed
        {
            get
            {
                bool ok = ros2cs_native_clock_valid(this.Handle);
                GC.KeepAlive(this);
                return !ok;
            }
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(
            "ros2cs_native",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        private static extern bool ros2cs_native_clock_valid(IntPtr clock);

        /// <summary> Query current time </summary>
        /// <returns> Time in full seconds and nanoseconds </returns>
        public RosTime Now
        {
            get
            {
                long nanoseconds;
                try
                {
                    rcl_clock_get_now(Handle, out nanoseconds).Throw();
                    GC.KeepAlive(this);
                }
                catch (RclError e) when (e.Code == RclReturnCode.RCL_RET_INVALID_ARGUMENT)
                {
                    throw new ObjectDisposedException("rcl clock has been disposed", e);
                }
                long seconds = nanoseconds / NANOS_PER_SECOND;
                uint nanos = (uint)(nanoseconds - seconds * NANOS_PER_SECOND);
                return new RosTime(seconds, nanos);
            }
        }

        [return: MarshalAs(UnmanagedType.I4)]
        [DllImport(
            "rcl",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        private static extern RclReturnCode rcl_clock_get_now(IntPtr clock, out long nanoseconds);

        internal IntPtr Handle { get; private set; } = IntPtr.Zero;

        public Clock()
        {
            ros2cs_native_clock_init(out IntPtr handle).Throw();
            this.Handle = handle;
        }

        [return: MarshalAs(UnmanagedType.I4)]
        [DllImport(
            "ros2cs_native",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        private static extern RclReturnCode ros2cs_native_clock_init(out IntPtr clock);

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.Handle == IntPtr.Zero)
            {
                return;
            }
            rcl_clock_fini(this.Handle).Throw();
            ros2cs_native_free_clock(this.Handle);
            this.Handle = IntPtr.Zero;
        }

        [return: MarshalAs(UnmanagedType.I4)]
        [DllImport(
            "rcl",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        private static extern RclReturnCode rcl_clock_fini(IntPtr clock);

        [DllImport(
            "ros2cs_native",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void ros2cs_native_free_clock(IntPtr clock);

        ~Clock()
        {
            this.Dispose(false);
        }
    }
}
