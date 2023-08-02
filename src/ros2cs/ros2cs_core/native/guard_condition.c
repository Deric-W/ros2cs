#include <stdlib.h>
#include <stdint.h>
#include <rcl/guard_condition.h>
#include <rcl/context.h>
#include <rcl/types.h>
#include <rosidl_runtime_c/visibility_control.h>

ROSIDL_GENERATOR_C_EXPORT
uint8_t ros2cs_native_guard_condition_valid(const rcl_guard_condition_t *guard_condition)
{
    // since there is no rcl_guard_condition_is_valid
    if (rcl_guard_condition_get_options(guard_condition) != NULL)
    {
        return 1;
    }
    return 0;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t ros2cs_native_init_guard_condition(rcl_context_t *context, rcl_guard_condition_t **output)
{
    rcl_guard_condition_t *guard_condition = malloc(sizeof(rcl_guard_condition_t));
    if (guard_condition == NULL)
    {
        return RCL_RET_BAD_ALLOC;
    }
    *guard_condition = rcl_get_zero_initialized_guard_condition();

    rcl_ret_t ret = rcl_guard_condition_init(guard_condition, context, rcl_guard_condition_get_default_options());
    if (ret == RCL_RET_OK)
    {
        *output = guard_condition;
    }
    else
    {
        free(guard_condition);
    }
    return ret;
}

ROSIDL_GENERATOR_C_EXPORT
void ros2cs_native_free_guard_condition(rcl_guard_condition_t *guard_condition)
{
    free(guard_condition);
}