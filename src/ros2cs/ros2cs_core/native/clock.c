#include <stdlib.h>
#include <stdint.h>
#include <rcl/time.h>
#include <rcl/types.h>
#include <rcl/allocator.h>
#include <rosidl_runtime_c/visibility_control.h>

ROSIDL_GENERATOR_C_EXPORT
uint8_t ros2cs_native_clock_valid(rcl_clock_t *clock)
{
    // since bool has different sizes in C and C++
    if (rcl_clock_valid(clock))
    {
        return 1;
    }
    return 0;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t ros2cs_native_clock_init(rcl_clock_t **output)
{
    rcl_clock_t *clock = malloc(sizeof(rcl_clock_t));
    if (clock == NULL)
    {
        return RCL_RET_BAD_ALLOC;
    }
    rcl_allocator_t allocator = rcl_get_default_allocator();
    rcl_ret_t ret = rcl_ros_clock_init(clock, &allocator);
    if (ret == RCL_RET_OK)
    {
        *output = clock;
    }
    else
    {
        free(clock);
    }
    return ret;
}

ROSIDL_GENERATOR_C_EXPORT
void ros2cs_native_free_clock(rcl_clock_t *clock)
{
    free(clock);
}
