// Copyright © Svetoslav Paregov. All rights reserved.

#include <stdio.h>
#include <string.h> // For memset
#include "pico/stdlib.h"
#include "uart_transport.hpp"
#include "spi_transport.hpp"
#include "commands_protocol.hpp"
#include "dc_motors_control.hpp"
#include "servo_control.hpp"
#include "common_types.hpp"

void init_commands_protocol()
{

}

void process_commands_protocol()
{
    uint32_t buffer_size = 512;
    char buffer[buffer_size];

    command_8_bytes_t command = spi_get_received_command();

    if (command.type == INVALID_COMMAND)
    {
        return;
    }
    
    motor_direction_speed_t motor_direction_speed = {
        .direction = (int8_t)command.data[0], // Direction
        .speed = command.data[1], // Speed in percentage (0-100)
        .timeout = (int16_t)((uint16_t)command.data[2] << 8 | command.data[3]) // Timeout in milliseconds
    };

    if (LEFT_MOTOR_COMMAND == command.type)
    {
        set_left_dc_motor_speed(motor_direction_speed);
    }

    if (RIGHT_MOTOR_COMMAND == command.type)
    {
        set_right_dc_motor_speed(motor_direction_speed);
    }

    if (BASE_MOTOR_DIRECTION_COMMAND == command.type)
    {
        set_base_servo_speed(motor_direction_speed);
    }

    if (SHOULDER_MOTOR_DIRECTION_COMMAND == command.type)
    {
        set_shoulder_servo_speed(motor_direction_speed);
    }

    if (ELBOW_MOTOR_DIRECTION_COMMAND == command.type)
    {
        set_elbow_servo_speed(motor_direction_speed);
    }

    if (ARM_MOTOR_DIRECTION_COMMAND == command.type)
    {
        set_arm_servo_speed(motor_direction_speed);
    }

    if (WRIST_MOTOR_DIRECTION_COMMAND == command.type)
    {
        set_wrist_servo_speed(motor_direction_speed);
    }

    if (GRIPPER_MOTOR_DIRECTION_COMMAND == command.type)
    {
        set_gripper_servo_speed(motor_direction_speed);
    }
}
