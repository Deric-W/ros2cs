#include <rcl/error_handling.h>
#include <rosidl_runtime_c/visibility_control.h>

ROSIDL_GENERATOR_C_EXPORT
char * ros2cs_native_get_last_error() {
  return rcl_get_error_string().str;
}