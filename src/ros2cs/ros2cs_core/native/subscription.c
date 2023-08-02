#include <stdlib.h>
#include <stdint.h>
#include <rcl/subscription.h>
#include <rcl/node.h>
#include <rcl/types.h>
#include <rosidl_runtime_c/message_type_support_struct.h>
#include <rosidl_runtime_c/visibility_control.h>

ROSIDL_GENERATOR_C_EXPORT
uint8_t ros2cs_native_subscription_valid(const rcl_subscription_t *subscription)
{
    // since bool has different sizes in C and C++
    if (rcl_subscription_is_valid(subscription))
    {
        return 1;
    }
    return 0;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t ros2cs_native_init_subscription_options(const rmw_qos_profile_t *qos, rcl_subscription_options_t **output)
{
    rcl_subscription_options_t *options = malloc(sizeof(rcl_subscription_options_t));
    if (options == NULL)
    {
        return RCL_RET_BAD_ALLOC;
    }
    *options = rcl_subscription_get_default_options();
    options->qos = *qos;
    *output = options;
    return RCL_RET_OK;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t ros2cs_native_dispose_subscription_options(rcl_subscription_options_t *options)
{
    rcl_ret_t ret = rcl_subscription_options_fini(options);
    if (ret == RCL_RET_OK)
    {
        free(options);
    }
    return ret;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t ros2cs_native_init_subscription(const rcl_node_t *node, const rosidl_message_type_support_t *type_support, const char *topic, const rcl_subscription_options_t *options, rcl_subscription_t **output)
{
    rcl_subscription_t *subscription = malloc(sizeof(rcl_subscription_t));
    if (subscription == NULL)
    {
        return RCL_RET_BAD_ALLOC;
    }
    *subscription = rcl_get_zero_initialized_subscription();

    rcl_ret_t ret = rcl_subscription_init(subscription, node, type_support, topic, options);
    if (ret == RCL_RET_OK)
    {
        *output = subscription;
    }
    else
    {
        free(subscription);
    }
    return ret;
}

ROSIDL_GENERATOR_C_EXPORT
void ros2cs_native_free_subscription(rcl_subscription_t *subscription)
{
    free(subscription);
}