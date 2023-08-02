#include <stdlib.h>
#include <rcl/rcl.h>
#include <rcl/types.h>
#include <rcl/error_handling.h>
#include <rmw/rmw.h>
#include <rmw/types.h>
#include <rmw/qos_profiles.h>
#include <rosidl_runtime_c/visibility_control.h>

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t ros2cs_native_init_qos(int profile, rmw_qos_profile_t **output)
{
    enum
    {
        SENSOR_DATA,
        PARAMETERS,
        DEFAULT,
        SERVICES_DEFAULT,
        PARAMETER_EVENTS,
        SYSTEM_DEFAULT
    };

    rmw_qos_profile_t *qos = malloc(sizeof(rmw_qos_profile_t));
    if (qos == NULL)
    {
        return RCL_RET_BAD_ALLOC;
    }

    switch (profile)
    {
        case SENSOR_DATA:
            *qos = rmw_qos_profile_sensor_data;
            break;
        case PARAMETERS:
            *qos = rmw_qos_profile_parameters;
            break;
        case DEFAULT:
            *qos = rmw_qos_profile_default;
            break;
        case SERVICES_DEFAULT:
            *qos = rmw_qos_profile_services_default;
            break;
        case PARAMETER_EVENTS:
            *qos = rmw_qos_profile_parameter_events;
            break;
        case SYSTEM_DEFAULT:
            *qos = rmw_qos_profile_system_default;
            break;
        default:
            *qos = rmw_qos_profile_unknown;
            break;
    }
    *output = qos;
    return RCL_RET_OK;
}

ROSIDL_GENERATOR_C_EXPORT
void ros2cs_native_dispose_qos(rmw_qos_profile_t *qos)
{
    free(qos);
}

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t ros2cs_native_set_qos_history(rmw_qos_profile_t *qos, int history)
{
    RCL_CHECK_ARGUMENT_FOR_NULL(qos, RCL_RET_INVALID_ARGUMENT);
    qos->history = history;
    return RCL_RET_OK;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t ros2cs_native_set_qos_depth(rmw_qos_profile_t *qos, size_t depth)
{
    RCL_CHECK_ARGUMENT_FOR_NULL(qos, RCL_RET_INVALID_ARGUMENT);
    qos->depth = depth;
    return RCL_RET_OK;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t ros2cs_native_set_qos_reliability(rmw_qos_profile_t *qos, int reliability)
{
    RCL_CHECK_ARGUMENT_FOR_NULL(qos, RCL_RET_INVALID_ARGUMENT);
    qos->reliability = reliability;
    return RCL_RET_OK;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t ros2cs_native_set_qos_durability(rmw_qos_profile_t *qos, int durability)
{
    RCL_CHECK_ARGUMENT_FOR_NULL(qos, RCL_RET_INVALID_ARGUMENT);
    qos->durability = durability;
    return RCL_RET_OK;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t ros2cs_native_get_qos(rmw_qos_profile_t *qos, int *history, size_t *depth, int *reliability, int *durability)
{
    RCL_CHECK_ARGUMENT_FOR_NULL(qos, RCL_RET_INVALID_ARGUMENT);
    *history = qos->history;
    *depth = qos->depth;
    *reliability = qos->reliability;
    *durability = qos->durability;
    return RCL_RET_OK;
}