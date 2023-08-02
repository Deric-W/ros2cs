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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ROS2.Internal;
using ROS2.Native;


namespace ROS2
{
    /// <summary>
    /// Client with a topic and types for messages wrapping a rcl client.
    /// </summary>
    /// <remarks>
    /// This is the implementation produced by <see cref="Node.CreateClient"/>,
    /// use this method to create new instances.
    /// </remarks>
    /// <seealso cref="ROS2.Node"/>
    /// <inheritdoc cref="IClient{I, O}"/>
    public sealed class Client<I, O> : IClient<I, O>, IRawClient
    where I : Message, new()
    where O : Message, new()
    {
        /// <inheritdoc/>
        public string Topic { get; private set; }

        /// <remarks>
        /// This dictionary is thread safe.
        /// </remarks>
        /// <inheritdoc/>
        public IReadOnlyDictionary<long, Task<O>> PendingRequests { get; private set; }

        /// <remarks>
        /// This dictionary is thread safe.
        /// </remarks>
        /// <inheritdoc/>
        IReadOnlyDictionary<long, Task> IClientBase.PendingRequests { get { return this.UntypedPendingRequests; } }

        /// <summary>
        /// Wrapper for <see cref="IClientBase.PendingRequests"/>.
        /// </summary>
        private readonly IReadOnlyDictionary<long, Task> UntypedPendingRequests;

        /// <inheritdoc/>
        public bool IsDisposed
        {
            get
            {
                bool ok = NativeClientMethods.ros2cs_native_client_valid(this.Handle);
                GC.KeepAlive(this);
                return !ok;
            }
        }

        /// <summary>
        /// Handle to the rcl client.
        /// </summary>
        public IntPtr Handle { get; private set; } = IntPtr.Zero;

        /// <summary>
        /// Handle to the rcl client options.
        /// </summary>
        private IntPtr Options = IntPtr.Zero;

        /// <summary>
        /// Node associated with this instance.
        /// </summary>
        private readonly Node Node;

        /// <summary>
        /// Mapping from request id without Response to <see cref="TaskCompletionSource"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="TaskCompletionSource.Task"/> is stored separately to allow
        /// <see cref="Cancel"/> to work even if the source returns multiple tasks.
        /// Furthermore, this object is used for locking.
        /// </remarks>
        private readonly Dictionary<long, (TaskCompletionSource<O>, Task<O>)> Requests = new Dictionary<long, (TaskCompletionSource<O>, Task<O>)>();

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <remarks>
        /// The caller is responsible for adding the instance to <paramref name="node"/>.
        /// This action is not thread safe.
        /// </remarks>
        /// <param name="topic"> Topic to subscribe to. </param>
        /// <param name="node"> Node to associate with. </param>
        /// <param name="qos"> QOS setting for this subscription. </param>
        /// <exception cref="ObjectDisposedException"> If <paramref name="node"/> was disposed. </exception>
        internal Client(string topic, Node node, QualityOfServiceProfile qos = null)
        {
            this.Topic = topic;
            this.Node = node;

            var lockedRequests = new LockedDictionary<long, (TaskCompletionSource<O>, Task<O>)>(this.Requests);
            this.PendingRequests = new MappedValueDictionary<long, (TaskCompletionSource<O>, Task<O>), Task<O>>(
                lockedRequests,
                tuple => tuple.Item2
            );
            this.UntypedPendingRequests = new MappedValueDictionary<long, (TaskCompletionSource<O>, Task<O>), Task>(
                lockedRequests,
                tuple => tuple.Item2
            );

            QualityOfServiceProfile qualityOfServiceProfile = qos ?? new QualityOfServiceProfile(QosPresetProfile.SERVICES_DEFAULT);
            IntPtr typeSupportHandle = MessageTypeSupportHelper.GetTypeSupportHandle<I>();

            (this.Options, this.Handle) = InteropUtils.CreateHandleWithOptions(
                (out IntPtr options) => NativeClientMethods.ros2cs_native_init_client_options(
                    qualityOfServiceProfile.handle,
                    out options
                ),
                (IntPtr options, out IntPtr handle) => NativeClientMethods.ros2cs_native_init_client(
                    this.Node.Handle,
                    typeSupportHandle,
                    this.Topic.ToRcl(),
                    options,
                    out handle
                ),
                options => { NativeClientMethods.ros2cs_native_dispose_client_options(options); return RclReturnCode.RCL_RET_OK; }
            );
            GC.KeepAlive(qualityOfServiceProfile);
        }

        /// <remarks>
        /// This method is not thread safe.
        /// </remarks>
        /// <exception cref="ObjectDisposedException"> If the instance was disposed. </exception>
        /// <inheritdoc/>
        public bool IsServiceAvailable()
        {
            bool available = false;
            NativeClientMethods.rcl_service_server_is_available(
                this.Node.Handle,
                this.Handle,
                out available
            ).Throw();
            GC.KeepAlive(this);
            return available;
        }

        /// <remarks>
        /// This method is thread safe.
        /// </remarks>
        /// <inheritdoc/>
        public bool TryProcess()
        {
            rmw_request_id_t header = default(rmw_request_id_t);
            O message = new O();
            (TaskCompletionSource<O>, Task<O>) source;
            bool exists = false;

            lock (this.Requests)
            {
                // prevent taking responses before RegisterSource was called
                RclReturnCode ret = NativeClientMethods.rcl_take_response(
                    this.Handle,
                    ref header,
                  (message as MessageInternals).Handle
                );
                GC.KeepAlive(this);

                switch (ret)
                {
                    case RclReturnCode.RCL_RET_CLIENT_TAKE_FAILED:
                    case RclReturnCode.RCL_RET_CLIENT_INVALID:
                        return false;
                    default:
                        ret.Throw();
                        break;
                }

                if (this.Requests.TryGetValue(header.sequence_number, out source))
                {
                    exists = true;
                    this.Requests.Remove(header.sequence_number);
                }
            }
            if (exists)
            {
                (message as MessageInternals).ReadNativeMessage();
                source.Item1.SetResult(message);
            }
            else
            {
                Debug.Print("received request which was not pending, maybe canceled");
            }
            return true;
        }

        /// <remarks>
        /// The provided message can be modified or disposed after this call.
        /// Furthermore, this method is thread safe.
        /// </remarks>
        /// <exception cref="ObjectDisposedException"> If the instance was disposed. </exception>
        /// <inheritdoc/>
        public O Call(I msg)
        {
            var task = CallAsync(msg);
            task.Wait();
            return task.Result;
        }

        /// <remarks>
        /// This method is thread safe.
        /// </remarks>
        /// <exception cref="ObjectDisposedException"> If the instance was disposed. </exception>
        /// <inheritdoc/>
        public Task<O> CallAsync(I msg)
        {
            return CallAsync(msg, TaskCreationOptions.None);
        }

        /// <remarks>
        /// This method is thread safe.
        /// </remarks>
        /// <exception cref="ObjectDisposedException"> If the instance was disposed. </exception>
        /// <inheritdoc/>
        public Task<O> CallAsync(I msg, TaskCreationOptions options)
        {
            var source = new TaskCompletionSource<O>(options);
            lock (this.Requests)
            {
                // prevents TryProcess from receiving Responses before we called RegisterSource
                long sequence_number = SendRequest(msg);
                return RegisterSource(source, sequence_number);
            }
        }

        /// <summary>
        /// Send a Request to the Service
        /// </summary>
        /// <param name="msg">Message to be send</param>
        /// <returns>sequence number of the Request</returns>
        private long SendRequest(I msg)
        {
            long sequence_number = default(long);
            MessageInternals msgInternals = msg as MessageInternals;
            msgInternals.WriteNativeMessage();
            NativeClientMethods.rcl_send_request(
                this.Handle,
                msgInternals.Handle,
                out sequence_number
            ).Throw();
            GC.KeepAlive(this);
            return sequence_number;
        }

        /// <summary>
        /// Associate a task with a sequence number
        /// </summary>
        /// <param name="source">source used to controll the <see cref="Task"/></param>
        /// <param name="sequence_number">sequence number received when sending the Request</param>
        /// <returns>The associated task.</returns>
        private Task<O> RegisterSource(TaskCompletionSource<O> source, long sequence_number)
        {
            // handle Task not being a singleton
            Task<O> task = source.Task;
            Requests.Add(sequence_number, (source, task));
            return task;
        }

        /// <remarks>
        /// Tasks are automatically removed on completion and have to be removed only when canceled.
        /// Furthermore, this method is thread safe.
        /// </remarks>
        /// <inheritdoc/>
        public bool Cancel(Task task)
        {
            var pair = default(KeyValuePair<long, (TaskCompletionSource<O>, Task<O>)>);
            lock (this.Requests)
            {
                try
                {
                    pair = this.Requests.First(entry => entry.Value.Item2 == task);
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                // has to be true
                bool success = this.Requests.Remove(pair.Key);
                Debug.Assert(success, "failed to remove matching request");
            }
            pair.Value.Item1.SetCanceled();
            return true;
        }

        /// <remarks>
        /// This method is not thread safe and may not be called from
        /// multiple threads simultaneously or while the client is in use.
        /// Disposal is automatically performed on finalization by the GC.
        /// Any pending tasks are removed and set to have faulted with
        /// <see cref="ObjectDisposedException"/>.
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

            // only do if Node.CurrentClients and this.Requests have not been finalized
            // save since if we are being finalized we are not in a wait set anymore
            if (disposing)
            {
                bool success = this.Node.RemoveClient(this);
                Debug.Assert(success, "failed to remove client");
                this.DisposeAllTasks();
            }

            NativeClientMethods.rcl_client_fini(this.Handle, this.Node.Handle).Throw();
            this.FreeHandles();
        }

        /// <inheritdoc/>
        void IRawClient.DisposeFromNode()
        {
            if (this.Handle == IntPtr.Zero)
            {
                return;
            }

            this.DisposeAllTasks();
            NativeClientMethods.rcl_client_fini(this.Handle, this.Node.Handle).Throw();
            this.FreeHandles();
        }

        /// <summary>
        /// Dispose all tasks currently pending.
        /// </summary>
        private void DisposeAllTasks()
        {
            lock (this.Requests)
            {
                foreach (var source in this.Requests.Values)
                {
                    source.Item1.TrySetException(new ObjectDisposedException($"client for topic '{this.Topic}'"));
                }
                this.Requests.Clear();
            }
        }

        /// <summary>
        /// Free the rcl handles and replace them with null pointers.
        /// </summary>
        /// <remarks>
        /// The handles are not finalised by this method.
        /// </remarks>
        private void FreeHandles()
        {
            NativeClientMethods.ros2cs_native_free_client(this.Handle);
            this.Handle = IntPtr.Zero;
            NativeClientMethods.ros2cs_native_dispose_client_options(this.Options);
            this.Options = IntPtr.Zero;
        }

        ~Client()
        {
            this.Dispose(false);
        }
    }

    internal static class NativeClientMethods
    {
        [return: MarshalAs(unmanagedType: UnmanagedType.U1)]
        [DllImport(
            "ros2cs_native",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool ros2cs_native_client_valid(IntPtr client);

        [return: MarshalAs(unmanagedType: UnmanagedType.I4)]
        [DllImport(
            "ros2cs_native",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern RclReturnCode ros2cs_native_init_client_options(IntPtr qos, out IntPtr options);

        [DllImport(
            "ros2cs_native",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ros2cs_native_dispose_client_options(IntPtr options);

        [return: MarshalAs(unmanagedType: UnmanagedType.I4)]
        [DllImport(
            "ros2cs_native",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern RclReturnCode ros2cs_native_init_client(IntPtr node, IntPtr typeSupport, [In] byte[] topic, IntPtr options, out IntPtr client);

        [DllImport(
            "ros2cs_native",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ros2cs_native_free_client(IntPtr client);

        [return: MarshalAs(unmanagedType: UnmanagedType.I4)]
        [DllImport(
            "rcl",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern RclReturnCode rcl_service_server_is_available(IntPtr node, IntPtr client, out bool available);

        [return: MarshalAs(unmanagedType: UnmanagedType.I4)]
        [DllImport(
            "rcl",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern RclReturnCode rcl_send_request(IntPtr client, IntPtr request, out long sequenceNumber);

        [return: MarshalAs(unmanagedType: UnmanagedType.I4)]
        [DllImport(
            "rcl",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern RclReturnCode rcl_take_response(IntPtr client, [In, Out] ref rmw_request_id_t header, IntPtr response);

        [return: MarshalAs(unmanagedType: UnmanagedType.I4)]
        [DllImport(
            "rcl",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern RclReturnCode rcl_client_fini(IntPtr client, IntPtr node);
    }
}
