#include <stdlib.h>
#include <stdint.h>
#include <rcl/wait.h>
#include <rcl/context.h>
#include <rcl/subscription.h>
#include <rcl/client.h>
#include <rcl/service.h>
#include <rcl/guard_condition.h>
#include <rcl/allocator.h>
#include <rcl/types.h>
#include <rosidl_runtime_c/visibility_control.h>

ROSIDL_GENERATOR_C_EXPORT
uint8_t ros2cs_native_wait_set_valid(const rcl_wait_set_t *wait_set)
{
    // since bool has different sizes in C and C++
    if (rcl_wait_set_is_valid(wait_set))
    {
        return 1;
    }
    return 0;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t ros2cs_native_init_wait_set(rcl_context_t *context, rcl_wait_set_t **output)
{
    rcl_wait_set_t *wait_set = malloc(sizeof(rcl_wait_set_t));
    if (wait_set == NULL)
    {
        return RCL_RET_BAD_ALLOC;
    }
    *wait_set = rcl_get_zero_initialized_wait_set();
    rcl_ret_t ret = rcl_wait_set_init(wait_set, 0, 0, 0, 0, 0, 0, context, rcl_get_default_allocator());
    if (ret == RCL_RET_OK)
    {
        *output = wait_set;
    }
    else
    {
        free(wait_set);
    }
    return ret;
}

ROSIDL_GENERATOR_C_EXPORT
void ros2cs_native_free_wait_set(rcl_wait_set_t *wait_set)
{
    free(wait_set);
}

ROSIDL_GENERATOR_C_EXPORT
uint8_t ros2cs_native_wait_set_get_subscription(const rcl_wait_set_t *wait_set, size_t index, const rcl_subscription_t **subscription)
{
    if (index < wait_set->size_of_subscriptions)
    {
        *subscription = wait_set->subscriptions[index];
        return 1;
    }
    return 0;
}

ROSIDL_GENERATOR_C_EXPORT
uint8_t ros2cs_native_wait_set_get_client(const rcl_wait_set_t *wait_set, size_t index, const rcl_client_t **client)
{
    if (index < wait_set->size_of_clients)
    {
        *client = wait_set->clients[index];
        return 1;
    }
    return 0;
}

ROSIDL_GENERATOR_C_EXPORT
uint8_t ros2cs_native_wait_set_get_service(const rcl_wait_set_t *wait_set, size_t index, const rcl_service_t **service)
{
    if (index < wait_set->size_of_services)
    {
        *service = wait_set->services[index];
        return 1;
    }
    return 0;
}

ROSIDL_GENERATOR_C_EXPORT
uint8_t ros2cs_native_wait_set_get_guard_condition(const rcl_wait_set_t *wait_set, size_t index, const rcl_guard_condition_t **guard_condition)
{
    if (index < wait_set->size_of_guard_conditions)
    {
        *guard_condition = wait_set->guard_conditions[index];
        return 1;
    }
    return 0;
}