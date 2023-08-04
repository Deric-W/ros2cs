@#######################################################################
@# Included from rosidl_generator_cs/resource/idl.cs.em
@#
@# Context:
@#  - package_name (string)
@#  - interface_path (Path relative to the directory named after the package)
@#  - message (IdlMessage, with structure containing names, types and members)
@#  - get_dotnet_type, escape_string, get_field_name - helper functions for cs translation of types
@#######################################################################
@{
from rosidl_parser.definition import AbstractNestableType
from rosidl_parser.definition import BasicType
from rosidl_parser.definition import NamespacedType
from rosidl_parser.definition import NamedType
from rosidl_parser.definition import AbstractGenericString
from rosidl_parser.definition import AbstractString
from rosidl_parser.definition import AbstractWString
from rosidl_parser.definition import AbstractNestedType
from rosidl_parser.definition import Array
from rosidl_parser.definition import AbstractSequence
from rosidl_generator_c import idl_type_to_c
from rosidl_cmake import convert_camel_case_to_lower_case_underscore
}@
@#>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
@# Handle namespaces
@#<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

@{
message_class = message.structure.namespaced_type.name
message_class_lower = convert_camel_case_to_lower_case_underscore(message_class)
c_full_name = idl_type_to_c(message.structure.namespaced_type)
internals_interface = "MessageInternals"
parent_interface = "Message"
header_type = "std_msgs.msg.Header"
for member in message.structure.members:
  if get_dotnet_type(member.type) == header_type:
    parent_interface = "MessageWithHeader"
}

@[for ns in message.structure.namespaced_type.namespaces]@
namespace @(ns)
{
@[end for]@
// message class
public class @(message_class) : @(internals_interface), @(parent_interface)
{
  private IntPtr _handle;

  public bool IsDisposed { get { return disposed; } }
  private bool disposed;

  // constant declarations
@[for constant in message.constants]@
  public const @(get_dotnet_type(constant.type)) @(constant.name) = @(constant_value_to_dotnet(constant.type, constant.value));
@[end for]@

  private const string NATIVE_LIBRARY = "@(package_name)_@(message_class_lower)__rosidl_typesupport_c_native";

  // members
@[for member in message.structure.members]@
@[  if isinstance(member.type, AbstractNestableType)]@
  public @(get_dotnet_type(member.type)) @(get_field_name(member.type, member.name, message_class)) { get; set; }
@[    if get_dotnet_type(member.type) == header_type]@

  // Generic interface for all messages with headers
  public void SetHeaderFrame(string frameID)
  {
    @(get_field_name(member.type, member.name, message_class)).Frame_id = frameID;
  }

  public string GetHeaderFrame()
  {
    return @(get_field_name(member.type, member.name, message_class)).Frame_id;
  }

  public void UpdateHeaderTime(int sec, uint nanosec)
  {
    @(get_field_name(member.type, member.name, message_class)).Stamp.Sec = sec;
    @(get_field_name(member.type, member.name, message_class)).Stamp.Nanosec = nanosec;
  }
@[    end if]@
@[  elif isinstance(member.type, AbstractSequence) and isinstance(member.type.value_type, AbstractNestableType)]@
  public @(get_dotnet_type(member.type)) @(get_field_name(member.type, member.name, message_class)) { get; set; }
@[  elif isinstance(member.type, Array) and isinstance(member.type.value_type, AbstractNestableType)]@
  public @(get_dotnet_type(member.type)) @(get_field_name(member.type, member.name, message_class)) { get; private set; }
@[  end if]@
@[end for]@

  [DllImport(
    NATIVE_LIBRARY,
    ExactSpelling = true,
    CallingConvention = CallingConvention.Cdecl,
    EntryPoint = "@(c_full_name)_native_get_type_support")]
  private static extern IntPtr native_get_typesupport();

  [DllImport(
    NATIVE_LIBRARY,
    ExactSpelling = true,
    CallingConvention = CallingConvention.Cdecl,
    EntryPoint = "@(c_full_name)_native_create_native_message")]
  private static extern IntPtr native_create_native_message();

  [DllImport(
    NATIVE_LIBRARY,
    ExactSpelling = true,
    CallingConvention = CallingConvention.Cdecl,
    EntryPoint = "@(c_full_name)_native_destroy_native_message")]
  private static extern void native_destroy_native_message(IntPtr messageHandle);

@[for member in message.structure.members]@
@[  if isinstance(member.type, BasicType)]@
  [DllImport(
    NATIVE_LIBRARY,
    ExactSpelling = true,
    CallingConvention = CallingConvention.Cdecl,
    EntryPoint = "@(c_full_name)_native_read_field_@(member.name)")]
  private static extern @(get_dotnet_type(member.type)) native_read_field_@(member.name)(IntPtr messageHandle);

  [DllImport(
    NATIVE_LIBRARY,
    ExactSpelling = true,
    CallingConvention = CallingConvention.Cdecl,
    EntryPoint = "@(c_full_name)_native_write_field_@(member.name)")]
  private static extern void native_write_field_@(member.name)(IntPtr messageHandle, @(get_dotnet_type(member.type)) value);

@[  elif isinstance(member.type, AbstractGenericString)]@
  [DllImport(
    NATIVE_LIBRARY,
    ExactSpelling = true,
    CallingConvention = CallingConvention.Cdecl,
    EntryPoint = "@(c_full_name)_native_read_field_@(member.name)")]
  private static extern IntPtr native_read_field_@(member.name)(IntPtr messageHandle);

  [DllImport(
    NATIVE_LIBRARY,
    ExactSpelling = true,
    CallingConvention = CallingConvention.Cdecl,
    EntryPoint = "@(c_full_name)_native_write_field_@(member.name)")]
  private static extern void native_write_field_@(member.name)(IntPtr messageHandle, [MarshalAs(UnmanagedType.LPStr)] string value); 

@[  elif isinstance(member.type, AbstractNestedType)]@
@[    if isinstance(member.type.value_type, BasicType)]@
  [DllImport(
    NATIVE_LIBRARY,
    ExactSpelling = true,
    CallingConvention = CallingConvention.Cdecl,
    EntryPoint = "@(c_full_name)_native_read_field_@(member.name)")]
  private static extern IntPtr native_read_field_@(member.name)(out int array_size, IntPtr messageHandle);

  [DllImport(
    NATIVE_LIBRARY,
    ExactSpelling = true,
    CallingConvention = CallingConvention.Cdecl,
    EntryPoint = "@(c_full_name)_native_write_field_@(member.name)")]
  private static extern bool native_write_field_@(member.name)(
    [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.@(get_marshal_type(member.type.value_type)), SizeParamIndex = 1)]
    @(get_dotnet_type(member.type)) values,
    int array_size,
    IntPtr messageHandle);

@[    elif isinstance(member.type.value_type, AbstractString)]@
  [DllImport(
    NATIVE_LIBRARY,
    ExactSpelling = true,
    CallingConvention = CallingConvention.Cdecl,
    EntryPoint = "@(c_full_name)_native_read_field_@(member.name)")]
  private static extern IntPtr native_read_field_@(member.name)(int index, IntPtr messageHandle);

  [DllImport(
    NATIVE_LIBRARY,
    ExactSpelling = true,
    CallingConvention = CallingConvention.Cdecl,
    EntryPoint = "@(c_full_name)_native_write_field_@(member.name)")]
  private static extern bool native_write_field_@(member.name)(
    [MarshalAs(UnmanagedType.LPStr)] string value,
    int index,
    IntPtr messageHandle);

@[    end if]@
@[  end if]@

@[  if isinstance(member.type, (NamedType, NamespacedType))]@
  [DllImport(
    NATIVE_LIBRARY,
    ExactSpelling = true,
    CallingConvention = CallingConvention.Cdecl,
    EntryPoint = "@(c_full_name)_native_get_nested_message_handle_@(member.name)")]
  private static extern IntPtr native_get_nested_message_handle_@(member.name)(IntPtr messageHandle);

@[  end if]@
@
@[  if isinstance(member.type, AbstractNestedType) and \
         isinstance(member.type.value_type, (NamedType, NamespacedType, AbstractString))]@
@[    if isinstance(member.type.value_type, (NamedType, NamespacedType))]@
  [DllImport(
    NATIVE_LIBRARY,
    ExactSpelling = true,
    CallingConvention = CallingConvention.Cdecl,
    EntryPoint = "@(c_full_name)_native_get_nested_message_handle_@(member.name)")]
  private static extern IntPtr native_get_nested_message_handle_@(member.name)(IntPtr messageHandle, int index);
@[    end if]@

  [DllImport(
    NATIVE_LIBRARY,
    ExactSpelling = true,
    CallingConvention = CallingConvention.Cdecl,
    EntryPoint = "@(c_full_name)_native_get_array_size_@(member.name)")]
  private static extern int native_get_array_size_@(member.name)(IntPtr messageHandle);

  [DllImport(
    NATIVE_LIBRARY,
    ExactSpelling = true,
    CallingConvention = CallingConvention.Cdecl,
    EntryPoint = "@(c_full_name)_native_init_sequence_@(member.name)")]
  private static extern bool native_init_sequence_@(member.name)(IntPtr messageHandle, int size);

@[  end if]@
@[end for]@

  public IntPtr TypeSupportHandle
  {
    get
    {
      return native_get_typesupport();
    }
  }

  // Handle. Create on first use. Can be set for nested classes. TODO -- access...
  public IntPtr Handle
  {
    get
    {
      if (_handle == IntPtr.Zero)
        _handle = native_create_native_message();
      return _handle;
    }
  }

  public @(message_class)()
  {
@[for member in message.structure.members]@
@[  if isinstance(member.type, (NamedType, NamespacedType))]@
    @(get_field_name(member.type, member.name, message_class)) = new @(get_dotnet_type(member.type))();
@[  elif isinstance(member.type, AbstractString)]@
    @(get_field_name(member.type, member.name, message_class)) = "";
@[  elif isinstance(member.type, AbstractSequence)]@
    @(get_field_name(member.type, member.name, message_class)) = new @(get_dotnet_type(member.type.value_type))[0];
@[  elif isinstance(member.type, Array)]@
    @(get_field_name(member.type, member.name, message_class)) = new @(get_dotnet_type(member.type.value_type))[@(member.type.size)];
@[  end if]@
@[end for]@
  }

  public void ReadNativeMessage()
  {
    ReadNativeMessage(Handle);
  }

  public void ReadNativeMessage(IntPtr handle)
  {
    if (handle == IntPtr.Zero)
      throw new System.InvalidOperationException("Invalid handle for reading");
@[for member in message.structure.members]@
@[  if isinstance(member.type, BasicType)]@
    @(get_field_name(member.type, member.name, message_class)) = native_read_field_@(member.name)(handle);
@[  elif isinstance(member.type, (NamedType, NamespacedType))]@
    @(get_field_name(member.type, member.name, message_class)).ReadNativeMessage(native_get_nested_message_handle_@(member.name)(handle));
@[  elif isinstance(member.type, AbstractString)]@
    {
      IntPtr pStr = native_read_field_@(member.name)(handle);
      @(get_field_name(member.type, member.name, message_class)) = Marshal.PtrToStringAnsi(pStr);
    }
@[  elif isinstance(member.type, AbstractNestedType) and isinstance(member.type.value_type, BasicType)]@
    { //TODO - (adam) this is a bit clunky. Is there a better way to marshal unsigned and bool types?
      int arraySize = 0;
      IntPtr pArr = native_read_field_@(member.name)(out arraySize, handle);
      @(get_field_name(member.type, member.name, message_class)) = new @(get_dotnet_type(member.type.value_type))[arraySize];
      @(get_marshal_array_type(member.type))[] __@(get_field_name(member.type, member.name, message_class)) = new @(get_marshal_array_type(member.type))[arraySize];

      if (arraySize != 0)
      {
        int start = 0;
        Marshal.Copy(pArr, __@(get_field_name(member.type, member.name, message_class)), start, arraySize);
      }
      for (int i = 0; i < arraySize; ++i)
      {
@[    if get_dotnet_type(member.type.value_type) == 'bool']@
        bool _boolean_value = __@(get_field_name(member.type, member.name, message_class))[i] != 0;
        @(get_field_name(member.type, member.name, message_class))[i] = _boolean_value;
@[    else]@
        @(get_field_name(member.type, member.name, message_class))[i] = (@(get_dotnet_type(member.type.value_type)))(__@(get_field_name(member.type, member.name, message_class))[i]);
@[    end if]@
      }
    }
@[  elif isinstance(member.type, AbstractNestedType) and \
         isinstance(member.type.value_type, (NamedType, NamespacedType, AbstractString))]@
    {
      int __native_array_size = native_get_array_size_@(member.name)(handle);
      @(get_field_name(member.type, member.name, message_class)) = new @(get_dotnet_type(member.type.value_type))[__native_array_size];
      for (int i = 0; i < __native_array_size; ++i)
      {
@[    if isinstance(member.type.value_type, (NamedType, NamespacedType))]@
        @(get_field_name(member.type, member.name, message_class))[i] = new @(get_dotnet_type(member.type.value_type))();
        @(get_field_name(member.type, member.name, message_class))[i].ReadNativeMessage(native_get_nested_message_handle_@(member.name)(handle, i));
@[    elif isinstance(member.type.value_type, AbstractString)]@
        @(get_field_name(member.type, member.name, message_class))[i] = Marshal.PtrToStringAnsi(native_read_field_@(member.name)(i, handle));
@[    end if]@
      }
    }
@[  end if]@
@[end for]@
  }

  public void WriteNativeMessage()
  {
    if (_handle == IntPtr.Zero)
    { // message object reused for subsequent publishing.
      // This could be problematic if sequences sizes changed, but me handle that by checking for it in the c implementation
      _handle = native_create_native_message();
    }

    WriteNativeMessage(Handle);
  }

  // Write from CS to native handle
  public void WriteNativeMessage(IntPtr handle)
  {
    if (handle == IntPtr.Zero)
      throw new System.InvalidOperationException("Invalid handle for writing");
@[for member in message.structure.members]@
@[  if isinstance(member.type, (BasicType, AbstractGenericString))]@
    native_write_field_@(member.name)(handle, @(get_field_name(member.type, member.name, message_class)));
@[  elif isinstance(member.type, (NamedType, NamespacedType))]@
    @(get_field_name(member.type, member.name, message_class)).WriteNativeMessage(native_get_nested_message_handle_@(member.name)(handle));
@[  elif isinstance(member.type, AbstractNestedType) and isinstance(member.type.value_type, BasicType)]@
    {
      @[    if message_class == "PointCloud2"]@
      uint point_cloud_size = Height * Row_step;
      if (point_cloud_size > Data.Length)
        throw new System.InvalidOperationException("PointCloud2 data invalid: smaller than indicated by width and row step");
      // special optimization for PointCloud2, where you can pass larger data[] array to avoid realocations, making use of message information about its size
      bool success = native_write_field_@(member.name)(@(get_field_name(member.type, member.name, message_class)), (int)point_cloud_size, handle);
      @[    else]@
      bool success = native_write_field_@(member.name)(@(get_field_name(member.type, member.name, message_class)), @(get_field_name(member.type, member.name, message_class)).Length, handle);
      @[    end if]
      if (!success)
        throw new System.InvalidOperationException("Error writing field for @(member.name)");
    }
@[  elif isinstance(member.type, AbstractNestedType) and \
         isinstance(member.type.value_type, (NamedType, NamespacedType, AbstractString))]@
    {
      bool success = native_init_sequence_@(member.name)(handle, @(get_field_name(member.type, member.name, message_class)).Length);
      if (!success)
        throw new System.InvalidOperationException("Error initializing sequence for @(member.name)");
      for (int i = 0; i < @(get_field_name(member.type, member.name, message_class)).Length; ++i)
      {
@[    if isinstance(member.type.value_type, (NamedType, NamespacedType))]@
        IntPtr innerHandle = native_get_nested_message_handle_@(member.name)(handle, i);
        @(get_field_name(member.type, member.name, message_class))[i].WriteNativeMessage(innerHandle);
@[    elif isinstance(member.type.value_type, AbstractString)]@
        native_write_field_@(member.name)(@(get_field_name(member.type, member.name, message_class))[i], i, handle);
@[    end if]@
      }
    }
@[  end if]@
@[end for]@
  }

  public void Dispose()
  {
    if (!disposed)
    {
      if (_handle != IntPtr.Zero)
      {
        native_destroy_native_message(_handle);
        _handle = IntPtr.Zero;
        disposed = true;
      }
    }
  }

  ~@(message_class)()
  {
    Dispose();
  }

};  // class @(message_class)
@#>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
@# close namespaces
@#<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
@[for ns in reversed(message.structure.namespaced_type.namespaces)]@
}  // namespace @(ns)
@[end for]@
