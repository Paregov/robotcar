// Copyright © Svetoslav Paregov. All rights reserved.

#include <stdio.h>
#include "pico/stdlib.h"
#include "common_types.hpp"

#define SERVOS_COUNT 8

typedef struct servo_info_t
{
    // Maximum degrees of movement for the servo.
    uint16_t degrees;

    // Current position in degrees for the servo.
    uint16_t current_degrees;
    
    // Left position in micro seconds for the servo (us).
    float left_us;
    
    // Center position in micro seconds for the servo (us).
    float center_us;
    
    // Right position in micro seconds for the servo (us).
    float right_us;

    // How many us is one degree of movement.
    float degree_to_us;

    uint8_t pwm_number; // PWM channel number for the servo
} servo_info_t;

void init_servos();
void set_servo_position_in_degrees(uint8_t servo, float degrees);
void set_base_servo_speed(motor_speed_t speed);
void set_shoulder_servo_speed(motor_speed_t speed);
void set_elbow_servo_speed(motor_speed_t speed);
void set_arm_servo_speed(motor_speed_t speed);
void set_wrist_servo_speed(motor_speed_t speed);
void set_gripper_servo_speed(motor_speed_t speed);
