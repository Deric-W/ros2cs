#include <stdlib.h>
#include <stdint.h>
#include <rcl/context.h>
#include <rcl/init.h>
#include <rcl/init_options.h>
#include <rcl/types.h>
#include <rcl/allocator.h>
#include <rosidl_runtime_c/visibility_control.h>
#include <rmw/rmw.h>

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t ros2cs_native_init_context(rcl_context_t **output)
{
    rcl_ret_t ret;
    rcl_context_t *context = malloc(sizeof(rcl_context_t));
    if (context == NULL)
    {
        return RCL_RET_BAD_ALLOC;
    }
    *context = rcl_get_zero_initialized_context();

    rcl_init_options_t options = rcl_get_zero_initialized_init_options();
    ret = rcl_init_options_init(&options, rcl_get_default_allocator());
    if (ret != RCL_RET_OK)
    {
        goto free_context;
    }

    ret = rcl_init(0, NULL, &options, context);
    rcl_init_options_fini(&options);
    if (ret == RCL_RET_OK)
    {
        *output = context;
        return ret;
    }

free_context:
    free(context);
    return ret;
}

ROSIDL_GENERATOR_C_EXPORT
uint8_t ros2cs_native_context_valid(const rcl_context_t *context)
{
    // since bool has different sizes in C and C++
    if (rcl_context_is_valid(context))
    {
        return 1;
    }
    return 0;
}

ROSIDL_GENERATOR_C_EXPORT
void ros2cs_native_free_context(rcl_context_t *context)
{
    free(context);
}

ROSIDL_GENERATOR_C_EXPORT
const char* ros2cs_native_rmw_implementation_identifier()
{
    return rmw_get_implementation_identifier();
}