#include <stdlib.h>
#include <stdint.h>
#include <rcl/client.h>
#include <rcl/node.h>
#include <rcl/types.h>
#include <rosidl_runtime_c/service_type_support_struct.h>
#include <rosidl_runtime_c/visibility_control.h>

ROSIDL_GENERATOR_C_EXPORT
uint8_t ros2cs_native_client_valid(const rcl_client_t *client)
{
    // since bool has different sizes in C and C++
    if (rcl_client_is_valid(client))
    {
        return 1;
    }
    return 0;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t ros2cs_native_init_client_options(const rmw_qos_profile_t *qos, rcl_client_options_t **output)
{
    rcl_client_options_t *options = malloc(sizeof(rcl_client_options_t));
    if (options == NULL)
    {
        return RCL_RET_BAD_ALLOC;
    }
    *options = rcl_client_get_default_options();
    options->qos = *qos;
    *output = options;
    return RCL_RET_OK;
}

ROSIDL_GENERATOR_C_EXPORT
void ros2cs_native_dispose_client_options(rcl_client_options_t *options)
{
    free(options);
}

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t ros2cs_native_init_client(const rcl_node_t *node, const rosidl_service_type_support_t * type_support, const char *name, const rcl_client_options_t *options, rcl_client_t **output)
{
    rcl_client_t *client = malloc(sizeof(rcl_client_t));
    if (client == NULL)
    {
        return RCL_RET_BAD_ALLOC;
    }
    *client = rcl_get_zero_initialized_client();

    rcl_ret_t ret = rcl_client_init(client, node, type_support, name, options);
    if (ret == RCL_RET_OK)
    {
        *output = client;
    }
    else
    {
        free(client);
    }
    return ret;
}

ROSIDL_GENERATOR_C_EXPORT
void ros2cs_native_free_client(rcl_client_t *client)
{
    free(client);
}