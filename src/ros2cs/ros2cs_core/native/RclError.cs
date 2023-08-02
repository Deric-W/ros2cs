using System;

namespace ROS2.Native
{
    /// <summary>
    /// Exception thrown when calling a native functions fails.
    /// </summary>
    [Serializable]
    public class RclError : Exception
    {
        private const string RclTypeDocs = "https://docs.ros.org/en/humble/p/rcl/generated/file_include_rcl_types.h.html#defines";

        /// <summary>
        /// Return code of the native call.
        /// </summary>
        public RclReturnCode Code { get; private set; }

        /// <inheritdoc/>
        public override string Message => $"[Code {(int)this.Code} {base.Message}]";

        public RclError(RclReturnCode code, string message) : base(message)
        {
            this.Code = code;
            this.HelpLink = RclTypeDocs;
        }
    }
}