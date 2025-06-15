// Copyright © Svetoslav Paregov. All rights reserved.

#ifndef DC_MOTORS_CONTROL_HPP
#define DC_MOTORS_CONTROL_HPP

#include "common_types.hpp"

void init_dc_motors();
void set_dc_motors_speed(motor_speed_t left, motor_speed_t right);
void set_left_dc_motor_speed(motor_speed_t speed);
void set_right_dc_motor_speed(motor_speed_t speed);

#endif // DC_MOTORS_CONTROL_HPP
