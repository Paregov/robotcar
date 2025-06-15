// Copyright © Svetoslav Paregov. All rights reserved.

#include <stdio.h>
#include "pico/stdlib.h"
#include "hardware/irq.h"
#include "hardware/timer.h"
#include "pico_native_pwm.hpp"
#include "servo_control.hpp"

#define TIMER_INTERVAL_US 10000

#define BASE_MOTOR_INDEX 0
#define SHOULDER_MOTOR_INDEX 1
#define ELBOW_MOTOR_INDEX 2
#define ARM_MOTOR_INDEX 3
#define WRIST_MOTOR_INDEX 4
#define GRIPPER_MOTOR_INDEX 6

#define ELAPSED_TIME_MS_10_PERCENT 100
#define ELAPSED_TIME_MS_20_PERCENT 90
#define ELAPSED_TIME_MS_30_PERCENT 80
#define ELAPSED_TIME_MS_40_PERCENT 70
#define ELAPSED_TIME_MS_50_PERCENT 60
#define ELAPSED_TIME_MS_60_PERCENT 50
#define ELAPSED_TIME_MS_70_PERCENT 40
#define ELAPSED_TIME_MS_80_PERCENT 30
#define ELAPSED_TIME_MS_90_PERCENT 20
#define ELAPSED_TIME_MS_100_PERCENT 10

const servo_info_t servo_180 = {
    .degrees = 180,
    .left_us = 500.0f,
    .center_us = 1500.0f,
    .right_us = 2500.0f,
    .degree_to_us = (2500.0f - 500.0f) / 180.0f
};

const servo_info_t servo_270 = {
    .degrees = 270,
    .left_us = 500.0f,
    .center_us = 1500.0f,
    .right_us = 2500.0f,
    .degree_to_us = (2500.0f - 500.0f) / 270.0f
};

servo_info_t servos_info_array[SERVOS_COUNT];

motor_speed_t servo_motor_speeds_array[SERVOS_COUNT] = {
    { .dirction = 0, .speed = 0, .elapsed_time = 0, .timeout = 0 },
    { .dirction = 0, .speed = 0, .elapsed_time = 0, .timeout = 0 },
    { .dirction = 0, .speed = 0, .elapsed_time = 0, .timeout = 0 },
    { .dirction = 0, .speed = 0, .elapsed_time = 0, .timeout = 0 },
    { .dirction = 0, .speed = 0, .elapsed_time = 0, .timeout = 0 },
    { .dirction = 0, .speed = 0, .elapsed_time = 0, .timeout = 0 },
    { .dirction = 0, .speed = 0, .elapsed_time = 0, .timeout = 0 },
    { .dirction = 0, .speed = 0, .elapsed_time = 0, .timeout = 0 }
};

struct repeating_timer servo_control_timer;

void process_servo_motor_speed(motor_speed_t *motor, uint8_t motor_index)
{
    // Convert microseconds to milliseconds
    uint32_t time_in_ms = (TIMER_INTERVAL_US / 1000);
    
    motor->timeout -= time_in_ms;
    if (motor->timeout <= 0)
    {
        // Stop the motor if timeout has reached
        motor->speed = 0;
        motor->dirction = 0;
        motor->timeout = 0; // Reset timeout

        return;
    }

    motor->elapsed_time += time_in_ms;

    if (
        (motor->speed >= 10 && motor->speed < 20
        && motor->elapsed_time >= ELAPSED_TIME_MS_10_PERCENT) ||
        (motor->speed >= 20 && motor->speed < 30
        && motor->elapsed_time >= ELAPSED_TIME_MS_20_PERCENT) ||
        (motor->speed >= 30 && motor->speed < 40
        && motor->elapsed_time >= ELAPSED_TIME_MS_30_PERCENT) ||
        (motor->speed >= 40 && motor->speed < 50
        && motor->elapsed_time >= ELAPSED_TIME_MS_40_PERCENT) ||
        (motor->speed >= 50 && motor->speed < 60
        && motor->elapsed_time >= ELAPSED_TIME_MS_50_PERCENT) ||
        (motor->speed >= 60 && motor->speed < 70
        && motor->elapsed_time >= ELAPSED_TIME_MS_60_PERCENT) ||
        (motor->speed >= 70 && motor->speed < 80
        && motor->elapsed_time >= ELAPSED_TIME_MS_70_PERCENT) ||
        (motor->speed >= 80 && motor->speed < 90
        && motor->elapsed_time >= ELAPSED_TIME_MS_80_PERCENT) ||
        (motor->speed >= 90 && motor->speed < 100
        && motor->elapsed_time >= ELAPSED_TIME_MS_90_PERCENT) ||
        (motor->speed >= 100 && motor->elapsed_time >= ELAPSED_TIME_MS_100_PERCENT)
    )
    {
        uint16_t new_degrees = servos_info_array[motor_index].current_degrees;
        uint8_t pwm_number = servos_info_array[motor_index].pwm_number;

        if (motor->dirction > 0)
        {
            new_degrees += 1;
        }
        else if (motor->dirction < 0)
        {
            new_degrees -= 1;
        }

        motor->elapsed_time = 0; // Reset elapsed time after processing

        // Set the PWM duty cycle based on the speed percentage
        set_servo_position_in_degrees(pwm_number, new_degrees);
    }
}


/*
 * @brief Callback function for the repeating timer.
 * * This function is the Interrupt Service Routine (ISR). It will be called automatically
 * by the hardware timer every 10ms.
 * * IMPORTANT: Keep ISRs short and fast. Avoid long delays, complex calculations,
 * or calling functions that are not interrupt-safe (like many stdio functions).
 * * @param t Pointer to the repeating_timer structure.
 * @return bool Must return true to continue the timer. Returning false would stop it.
 */
bool servo_motors_timer_callback(struct repeating_timer *t)
{
    process_servo_motor_speed(&servo_motor_speeds_array[BASE_MOTOR_INDEX], 0);
    process_servo_motor_speed(&servo_motor_speeds_array[SHOULDER_MOTOR_INDEX], 1);
    process_servo_motor_speed(&servo_motor_speeds_array[ELBOW_MOTOR_INDEX], 2);
    process_servo_motor_speed(&servo_motor_speeds_array[ARM_MOTOR_INDEX], 3);
    process_servo_motor_speed(&servo_motor_speeds_array[WRIST_MOTOR_INDEX], 4);
    process_servo_motor_speed(&servo_motor_speeds_array[GRIPPER_MOTOR_INDEX], 6);

    return true; // Keep the timer repeating
}

void init_servos()
{
    servos_info_array[0] = servo_270;
    servos_info_array[0].pwm_number = 0;

    servos_info_array[1] = servo_270;
    servos_info_array[1].pwm_number = 1;

    servos_info_array[2] = servo_270;
    servos_info_array[2].pwm_number = 2;

    servos_info_array[3] = servo_270;
    servos_info_array[3].pwm_number = 3;

    servos_info_array[4] = servo_270; 
    servos_info_array[4].pwm_number = 4;

    servos_info_array[5] = servo_270;
    servos_info_array[5].pwm_number = 5;

    servos_info_array[6] = servo_270;
    servos_info_array[6].pwm_number = 6;

    servos_info_array[7] = servo_270;
    servos_info_array[7].pwm_number = 7;
    
    set_servo_position_in_degrees(0, servos_info_array[0].degrees/2); // base
    set_servo_position_in_degrees(1, servos_info_array[1].degrees/2); // shoulder
    set_servo_position_in_degrees(2, servos_info_array[2].degrees/2); // elbow
    set_servo_position_in_degrees(3, servos_info_array[3].degrees/2); // arm0
    set_servo_position_in_degrees(4, servos_info_array[4].degrees/2); // wrist2
    // set_servo_position_in_degrees(5, servos[5].degrees/2); // wrist3 - Not present in this Robot Arm
    set_servo_position_in_degrees(6, servos_info_array[6].degrees/2); // gripper
    // set_servo_position_in_degrees(7, servos[7].degrees/2); // - Not present in this Robot Arm
    
    // Create a repeating timer that calls servo_motors_timer_callback.
    // The first argument is the interval in microseconds.
    // A negative value means the timer will be fired relative to the previous scheduled fire time,
    // which is better for periodic tasks to avoid drift.
    // The last argument is a pointer where the SDK will store timer information.
    add_repeating_timer_us(-TIMER_INTERVAL_US, servo_motors_timer_callback, NULL, &servo_control_timer);
}

void set_servo_position_in_degrees(uint8_t servo, float degrees)
{
    // If we try to set a bigger than allowed degrees, we will set the maximum allowed degrees.
    if ((uint16_t)degrees > servos_info_array[servo].degrees)
    {
        degrees = (float)servos_info_array[servo].degrees;
    }
    else if (degrees < 0.0f)
    {
        degrees = 0.0f; // If we try to set a negative degrees, we will set 0 degrees.
    }

    float pulse_width = servos_info_array[servo].center_us + (servos_info_array[servo].degree_to_us * degrees);
    servos_info_array[servo].current_degrees = (uint16_t)degrees;

    set_pwm_pulse_width_us(servos_info_array[servo].pwm_number, (uint16_t)pulse_width);
}

void set_base_servo_speed(motor_speed_t speed)
{
    servo_motor_speeds_array[BASE_MOTOR_INDEX] = speed;
}

void set_shoulder_servo_speed(motor_speed_t speed)
{
    servo_motor_speeds_array[SHOULDER_MOTOR_INDEX] = speed;
}

void set_elbow_servo_speed(motor_speed_t speed)
{
    servo_motor_speeds_array[ELBOW_MOTOR_INDEX] = speed;
}

void set_arm_servo_speed(motor_speed_t speed)
{
    servo_motor_speeds_array[ARM_MOTOR_INDEX] = speed;
}

void set_wrist_servo_speed(motor_speed_t speed)
{
    servo_motor_speeds_array[WRIST_MOTOR_INDEX] = speed;
}

void set_gripper_servo_speed(motor_speed_t speed)
{
    servo_motor_speeds_array[GRIPPER_MOTOR_INDEX] = speed;
}

