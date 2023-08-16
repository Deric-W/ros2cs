#!/bin/bash
source ./install/local_setup.sh
colcon test --merge-install --packages-select ros2cs_tests; colcon test-result --verbose
