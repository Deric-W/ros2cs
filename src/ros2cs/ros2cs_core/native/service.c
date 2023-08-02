#include <stdlib.h>
#include <stdint.h>
#include <rcl/service.h>
#include <rcl/node.h>
#include <rcl/types.h>
#include <rosidl_runtime_c/service_type_support_struct.h>
#include <rosidl_runtime_c/visibility_control.h>

ROSIDL_GENERATOR_C_EXPORT
uint8_t ros2cs_native_service_valid(const rcl_service_t *service)
{
    // since bool has different sizes in C and C++
    if (rcl_service_is_valid(service))
    {
        return 1;
    }
    return 0;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t ros2cs_native_init_service_options(const rmw_qos_profile_t *qos, rcl_service_options_t **output)
{
    rcl_service_options_t *options = malloc(sizeof(rcl_service_options_t));
    if (options == NULL)
    {
        return RCL_RET_BAD_ALLOC;
    }
    *options = rcl_service_get_default_options();
    options->qos = *qos;
    *output = options;
    return RCL_RET_OK;
}

ROSIDL_GENERATOR_C_EXPORT
void ros2cs_native_dispose_service_options(rcl_service_options_t *options)
{
    free(options);
}

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t ros2cs_native_init_service(const rcl_node_t *node, const rosidl_service_type_support_t *type_support, const char *name, const rcl_service_options_t *options, rcl_service_t **output)
{
    rcl_service_t *service = malloc(sizeof(rcl_service_t));
    if (service == NULL)
    {
        return RCL_RET_BAD_ALLOC;
    }
    *service = rcl_get_zero_initialized_service();

    rcl_ret_t ret = rcl_service_init(service, node, type_support, name, options);
    if (ret == RCL_RET_OK)
    {
        *output = service;
    }
    else
    {
        free(service);
    }
    return ret;
}

ROSIDL_GENERATOR_C_EXPORT
void ros2cs_native_free_service(rcl_service_t *service)
{
    free(service);
}