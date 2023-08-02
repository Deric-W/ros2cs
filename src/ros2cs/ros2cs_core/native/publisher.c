#include <stdlib.h>
#include <stdint.h>
#include <rcl/publisher.h>
#include <rcl/node.h>
#include <rcl/types.h>
#include <rosidl_runtime_c/message_type_support_struct.h>
#include <rosidl_runtime_c/visibility_control.h>

ROSIDL_GENERATOR_C_EXPORT
uint8_t ros2cs_native_publisher_valid(const rcl_publisher_t *publisher)
{
    // since bool has different sizes in C and C++
    if (rcl_publisher_is_valid(publisher))
    {
        return 1;
    }
    return 0;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t ros2cs_native_init_publisher_options(const rmw_qos_profile_t *qos, rcl_publisher_options_t **output)
{
    rcl_publisher_options_t *options = malloc(sizeof(rcl_publisher_options_t));
    if (options == NULL)
    {
        return RCL_RET_BAD_ALLOC;
    }
    *options = rcl_publisher_get_default_options();
    options->qos = *qos;
    *output = options;
    return RCL_RET_OK;
}

ROSIDL_GENERATOR_C_EXPORT
void ros2cs_native_dispose_publisher_options(rcl_publisher_options_t *options)
{
    free(options);
}

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t ros2cs_native_init_publisher(const rcl_node_t *node, const rosidl_message_type_support_t *type_support, const char *topic, const rcl_publisher_options_t *options, rcl_publisher_t **output)
{
    rcl_publisher_t *publisher = malloc(sizeof(rcl_publisher_t));
    if (publisher == NULL)
    {
        return RCL_RET_BAD_ALLOC;
    }
    *publisher = rcl_get_zero_initialized_publisher();

    rcl_ret_t ret = rcl_publisher_init(publisher, node, type_support, topic, options);
    if (ret == RCL_RET_OK)
    {
        *output = publisher;
    }
    else
    {
        free(publisher);
    }
    return ret;
}

ROSIDL_GENERATOR_C_EXPORT
void ros2cs_native_free_publisher(rcl_publisher_t *publisher)
{
    free(publisher);
}