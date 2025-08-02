// Copyright © Svetoslav Paregov. All rights reserved.

#include <stdio.h>
#include "pico/stdlib.h"
#include "common_types.hpp"

#define SERVOS_COUNT 8

#define BASE_MOTOR_INDEX 0
#define SHOULDER_MOTOR_INDEX 1
#define ELBOW_MOTOR_INDEX 2
#define ARM_MOTOR_INDEX 3
#define WRIST_MOTOR_INDEX 4
#define GRIPPER_MOTOR_INDEX 6

// TODO: Check if I need to use integer instead of unsigned interger.
typedef struct
{
    // Maximum degrees of movement for the servo.
    // This is what the servo supports.
    // For example, 180 degrees or 270 degrees.
    int16_t degrees;

    // Current position in degrees for the servo.
    int16_t current_degrees;

    // Bottom limit in degrees for the servo.
    // Software limit to prevent the servo from going below this value.
    int16_t bottom_degrees_limit;

    // Top limit in degrees for the servo.
    // Software limit to prevent the servo from going above this value.
    int16_t top_degrees_limit;

    // Left position in micro seconds for the servo (us).
    float left_us;
    
    // Center position in micro seconds for the servo (us).
    float center_us;
    
    // Right position in micro seconds for the servo (us).
    float right_us;

    // How many us is one degree of movement.
    float degree_to_us;

    // PWM channel number for the servo
    uint8_t pwm_number;

    // If true, the servo is inverted and will move in the opposite direction.
    bool is_inverted;
} servo_info_t;

void init_servos();
void set_servo_position_in_degrees(uint8_t servo, int16_t degrees);
void set_base_servo_speed(motor_direction_speed_t speed);
void set_shoulder_servo_speed(motor_direction_speed_t speed);
void set_elbow_servo_speed(motor_direction_speed_t speed);
void set_arm_servo_speed(motor_direction_speed_t speed);
void set_wrist_servo_speed(motor_direction_speed_t speed);
void set_gripper_servo_speed(motor_direction_speed_t speed);

bool set_servo_motor_direction_speed(uint8_t servo, motor_direction_speed_t speed);
