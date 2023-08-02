// Copyright 2019-2021 Robotec.ai
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
    /// <summary>
    /// Predefined QOS configurations.
    /// </summary>
    /// <remarks>
    /// This is mapped to rmw presets, for example SENSOR_DATA is rmw_qos_profile_sensor_data
    /// </remarks>
    public enum QosPresetProfile : int
    {
        SENSOR_DATA,
        PARAMETERS,
        DEFAULT,
        SERVICES_DEFAULT,
        PARAMETER_EVENTS,
        SYSTEM_DEFAULT
    }

    /// <summary>
    /// Settings for message retention.
    /// </summary>
    public enum HistoryPolicy : int
    {
        QOS_POLICY_HISTORY_SYSTEM_DEFAULT,
        QOS_POLICY_HISTORY_KEEP_LAST,
        QOS_POLICY_HISTORY_KEEP_ALL,
        QOS_POLICY_HISTORY_UNKNOWN
    }

    /// <summary>
    /// Settings for message reliability.
    /// </summary>
    public enum ReliabilityPolicy : int
    {
        QOS_POLICY_RELIABILITY_SYSTEM_DEFAULT,
        QOS_POLICY_RELIABILITY_RELIABLE,
        QOS_POLICY_RELIABILITY_BEST_EFFORT,
        OS_POLICY_RELIABILITY_UNKNOWN
    }

    /// <summary>
    /// Settings for message durability.
    /// </summary>
    public enum DurabilityPolicy : int
    {
        QOS_POLICY_DURABILITY_SYSTEM_DEFAULT,
        QOS_POLICY_DURABILITY_TRANSIENT_LOCAL,
        QOS_POLICY_DURABILITY_VOLATILE,
        QOS_POLICY_DURABILITY_UNKNOWN
    }

    /// <summary>
    /// Settings for liveliness.
    /// </summary>
    public enum LivelinessPolicy : int
    {
        QOS_POLICY_LIVELINESS_SYSTEM_DEFAULT,
        QOS_POLICY_LIVELINESS_AUTOMATIC,
        // original has deprecated entries
        [Obsolete("Use QOS_POLICY_LIVELINESS_MANUAL_BY_TOPIC if manually asserted liveliness is needed.")]
        QOS_POLICY_LIVELINESS_MANUAL_BY_NODE,
        QOS_POLICY_LIVELINESS_MANUAL_BY_TOPIC,
        QOS_POLICY_LIVELINESS_UNKNOWN
    }

    /// <summary>
    /// Quality of Service settings for publishers, subscriptions, services and clients.
    /// </summary>
    public sealed class QualityOfServiceProfile
    {
        /// <summary>
        /// Current history setting.
        /// </summary>
        public HistoryPolicy History
        {
            get => this.GetData().History;
            set
            {
                ros2cs_native_set_qos_history(this.Handle, value).Throw();
                GC.KeepAlive(this);
            }
        }

        /// <summary>
        /// Current history depth.
        /// </summary>
        /// <exception cref="OverflowException"> The depth cant be converted to size_t. </exception>
        public ulong Depth
        {
            get => this.GetData().Depth;
            set
            {
                ros2cs_native_set_qos_depth(this.Handle, new UIntPtr(value)).Throw();
                GC.KeepAlive(this);
            }
        }

        /// <summary>
        /// Current reliability setting.
        /// </summary>
        public ReliabilityPolicy Reliability
        {
            get => this.GetData().Reliability;
            set
            {
                ros2cs_native_set_qos_reliability(this.Handle, value).Throw();
                GC.KeepAlive(this);
            }
        }

        /// <summary>
        /// Current durability setting.
        /// </summary>
        public DurabilityPolicy Durability
        {
            get => this.GetData().Durability;
            set
            {
                ros2cs_native_set_qos_durability(this.Handle, value).Throw();
                GC.KeepAlive(this);
            }
        }

        internal IntPtr Handle { get; private set; } = IntPtr.Zero;

        /// <summary> Construct using a preset </summary>
        public QualityOfServiceProfile(QosPresetProfile preset_profile = QosPresetProfile.DEFAULT)
        {
            ros2cs_native_init_qos(preset_profile, out IntPtr handle).Throw();
            this.Handle = handle;
        }

        [return: MarshalAs(UnmanagedType.I4)]
        [DllImport(
            "ros2cs_native",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        private static extern RclReturnCode ros2cs_native_init_qos([MarshalAs(UnmanagedType.I4)] QosPresetProfile profile, out IntPtr qos);

        [return: MarshalAs(UnmanagedType.I4)]
        [DllImport(
            "ros2cs_native",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        private static extern RclReturnCode ros2cs_native_set_qos_history(IntPtr qos, [MarshalAs(UnmanagedType.I4)] HistoryPolicy history);

        [return: MarshalAs(UnmanagedType.I4)]
        [DllImport(
            "ros2cs_native",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        private static extern RclReturnCode ros2cs_native_set_qos_depth(IntPtr qos, UIntPtr depth);

        [return: MarshalAs(UnmanagedType.I4)]
        [DllImport(
            "ros2cs_native",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        private static extern RclReturnCode ros2cs_native_set_qos_reliability(IntPtr qos, [MarshalAs(UnmanagedType.I4)] ReliabilityPolicy reliability);

        [return: MarshalAs(UnmanagedType.I4)]
        [DllImport(
            "ros2cs_native",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        private static extern RclReturnCode ros2cs_native_set_qos_durability(IntPtr qos, [MarshalAs(UnmanagedType.I4)] DurabilityPolicy durability);

        private (HistoryPolicy History, ulong Depth, ReliabilityPolicy Reliability, DurabilityPolicy Durability) GetData()
        {
            ros2cs_native_get_qos(
                this.Handle,
                out HistoryPolicy history,
                out UIntPtr depth,
                out ReliabilityPolicy reliability,
                out DurabilityPolicy durability
            ).Throw();
            GC.KeepAlive(this);
            return (history, depth.ToUInt64(), reliability, durability);
        }

        [return: MarshalAs(UnmanagedType.I4)]
        [DllImport(
            "ros2cs_native",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        private static extern RclReturnCode ros2cs_native_get_qos(
            IntPtr qos,
            [MarshalAs(UnmanagedType.I4)] out HistoryPolicy history,
            out UIntPtr depth,
            [MarshalAs(UnmanagedType.I4)] out ReliabilityPolicy reliability,
            [MarshalAs(UnmanagedType.I4)] out DurabilityPolicy durability
        );

        ~QualityOfServiceProfile()
        {
            ros2cs_native_dispose_qos(this.Handle);
        }

        [return: MarshalAs(UnmanagedType.I4)]
        [DllImport(
            "ros2cs_native",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void ros2cs_native_dispose_qos(IntPtr qos);
    }
}
