// Copyright 2019-2023 Robotec.ai
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
using System.Diagnostics;
using System.Runtime.InteropServices;
using ROS2.Internal;
using ROS2.Native;

namespace ROS2
{
    /// <summary>
    /// Subscription of a topic with a given type wrapping a rcl subscription.
    /// </summary>
    /// <remarks>
    /// This is the implementation produced by <see cref="Node.CreateSubscription"/>,
    /// use this method to create new instances.
    /// </remarks>
    /// <seealso cref="ROS2.Node"/>
    /// <inheritdoc cref="ISubscription{T}"/>
    public sealed class Subscription<T> : ISubscription<T>, IRawSubscription where T : Message, new()
    {
        /// <inheritdoc/>
        public string Topic { get; private set; }

        /// <inheritdoc/>
        public bool IsDisposed
        {
            get
            {
                bool ok = NativeSubscriptionMethods.ros2cs_native_subscription_valid(this.Handle);
                GC.KeepAlive(this);
                return !ok;
            }
        }

        /// <summary>
        /// Handle to the rcl subscription
        /// </summary>
        public IntPtr Handle { get; private set; } = IntPtr.Zero;

        /// <summary>
        /// Handle to the rcl subscription options
        /// </summary>
        private IntPtr Options = IntPtr.Zero;

        /// <summary>
        /// Node associated with this instance.
        /// </summary>
        private readonly Node Node;

        /// <summary>
        /// Callback invoked when a message is received.
        /// </summary>
        private readonly Action<T> Callback;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <remarks>
        /// The caller is responsible for adding the instance to <paramref name="node"/>.
        /// This action is not thread safe.
        /// </remarks>
        /// <param name="topic"> Topic to subscribe to. </param>
        /// <param name="node"> Node to associate with. </param>
        /// <param name="callback"> Callback invoked when a message is received. </param>
        /// <param name="qos"> QOS setting for this subscription. </param>
        /// <exception cref="ObjectDisposedException"> If <paramref name="node"/> was disposed. </exception>
        internal Subscription(string topic, Node node, Action<T> callback, QualityOfServiceProfile qos = null)
        {
            this.Topic = topic;
            this.Node = node;
            this.Callback = callback;

            QualityOfServiceProfile qualityOfServiceProfile = qos ?? new QualityOfServiceProfile();
            IntPtr typeSupportHandle = MessageTypeSupportHelper.GetTypeSupportHandle<T>();

            (this.Options, this.Handle) = InteropUtils.CreateHandleWithOptions(
                (out IntPtr options) => NativeSubscriptionMethods.ros2cs_native_init_subscription_options(
                    qualityOfServiceProfile.Handle,
                    out options
                ),
                (IntPtr options, out IntPtr handle) => NativeSubscriptionMethods.ros2cs_native_init_subscription(
                    this.Node.Handle,
                    typeSupportHandle,
                    this.Topic.ToRcl(),
                    options,
                    out handle
                ),
                NativeSubscriptionMethods.ros2cs_native_dispose_subscription_options
            );
            GC.KeepAlive(qualityOfServiceProfile);
        }

        /// <remarks>
        /// This method is not thread safe.
        /// </remarks>
        /// <inheritdoc/>
        public bool TryProcess()
        {
            T message = new T();
            RclReturnCode ret = NativeSubscriptionMethods.rcl_take(
                this.Handle,
                (message as MessageInternals).Handle,
                IntPtr.Zero,
                IntPtr.Zero
            );
            GC.KeepAlive(this);

            switch (ret)
            {
                case RclReturnCode.RCL_RET_SUBSCRIPTION_TAKE_FAILED:
                case RclReturnCode.RCL_RET_SUBSCRIPTION_INVALID:
                    return false;
                default:
                    ret.Throw();
                    break;
            }

            (message as MessageInternals).ReadNativeMessage();
            this.Callback(message);
            return true;
        }

        /// <remarks>
        /// This method is not thread safe and may not be called from
        /// multiple threads simultaneously or while the subscription is in use.
        /// Disposal is automatically performed on finalization by the GC.
        /// </remarks>
        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            // finalizer not needed when we disposed successfully
            GC.SuppressFinalize(this);
        }

        /// <summary>Disposal logic.</summary>
        /// <param name="disposing">If this method is not called in a finalizer.</param>
        private void Dispose(bool disposing)
        {
            if (this.Handle == IntPtr.Zero)
            {
                return;
            }

            // only do if Node.CurrentSubscriptions has not been finalized
            // save since if we are being finalized we are not in a wait set anymore
            if (disposing)
            {
                bool success = this.Node.RemoveSubscription(this);
                Debug.Assert(success, "failed to remove subscription");
            }

            (this as IRawSubscription).DisposeFromNode();
        }

        /// <inheritdoc/>
        void IRawSubscription.DisposeFromNode()
        {
            if (this.Handle == IntPtr.Zero)
            {
                return;
            }

            NativeSubscriptionMethods.rcl_subscription_fini(this.Handle, this.Node.Handle).Throw();
            this.FreeHandles();
        }

        /// <summary>
        /// Free the rcl handles and replace them with null pointers.
        /// </summary>
        /// <remarks>
        /// The handles are not finalised by this method.
        /// </remarks>
        private void FreeHandles()
        {
            NativeSubscriptionMethods.ros2cs_native_free_subscription(this.Handle);
            this.Handle = IntPtr.Zero;
            NativeSubscriptionMethods.ros2cs_native_dispose_subscription_options(this.Options).Throw();
            this.Options = IntPtr.Zero;
        }

        ~Subscription()
        {
            this.Dispose(false);
        }
    }

    internal static class NativeSubscriptionMethods
    {
        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(
            "ros2cs_native",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool ros2cs_native_subscription_valid(IntPtr subscription);

        [return: MarshalAs(UnmanagedType.I4)]
        [DllImport(
            "ros2cs_native",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern RclReturnCode ros2cs_native_init_subscription_options(IntPtr qos, out IntPtr options);

        [return: MarshalAs(UnmanagedType.I4)]
        [DllImport(
            "ros2cs_native",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern RclReturnCode ros2cs_native_dispose_subscription_options(IntPtr options);

        [return: MarshalAs(UnmanagedType.I4)]
        [DllImport(
            "ros2cs_native",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern RclReturnCode ros2cs_native_init_subscription(IntPtr node, IntPtr typeSupport, [In] byte[] topic, IntPtr options, out IntPtr subscription);

        [DllImport(
            "ros2cs_native",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ros2cs_native_free_subscription(IntPtr subscription);

        [return: MarshalAs(UnmanagedType.I4)]
        [DllImport(
            "rcl",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern RclReturnCode rcl_take(IntPtr subscription, IntPtr msg, IntPtr msgInfo, IntPtr allocation);

        [return: MarshalAs(UnmanagedType.I4)]
        [DllImport(
            "rcl",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern RclReturnCode rcl_subscription_fini(IntPtr subscription, IntPtr node);
    }
}
