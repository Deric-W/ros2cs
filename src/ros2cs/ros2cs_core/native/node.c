#include <stdlib.h>
#include <stdint.h>
#include <rcl/node.h>
#include <rcl/node_options.h>
#include <rcl/context.h>
#include <rcl/types.h>
#include <rosidl_runtime_c/visibility_control.h>

ROSIDL_GENERATOR_C_EXPORT
uint8_t ros2cs_native_node_valid(const rcl_node_t *node)
{
    // since bool has different sizes in C and C++
    if (rcl_node_is_valid(node))
    {
        return 1;
    }
    return 0;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t ros2cs_native_init_node_options(rcl_node_options_t **output)
{
    rcl_node_options_t *options = malloc(sizeof(rcl_node_options_t));
    if (options == NULL)
    {
        return RCL_RET_BAD_ALLOC;
    }
    *options = rcl_node_get_default_options();
    *output = options;
    return RCL_RET_OK;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t ros2cs_native_dispose_node_options(rcl_node_options_t *options)
{
    rcl_ret_t ret = rcl_node_options_fini(options);
    if (ret == RCL_RET_OK)
    {
        free(options);
    }
    return ret;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t ros2cs_native_init_node(const char *name, const char *namespace, rcl_context_t *context, const rcl_node_options_t *options, rcl_node_t **output)
{
    rcl_node_t *node = malloc(sizeof(rcl_node_t));
    if (node == NULL)
    {
        return RCL_RET_BAD_ALLOC;
    }
    *node = rcl_get_zero_initialized_node();

    rcl_ret_t ret = rcl_node_init(node, name, namespace, context, options);
    if (ret == RCL_RET_OK)
    {
        *output = node;
    }
    else
    {
        free(node);
    }
    return ret;
}

ROSIDL_GENERATOR_C_EXPORT
void ros2cs_native_free_node(rcl_node_t *node)
{
    free(node);
}