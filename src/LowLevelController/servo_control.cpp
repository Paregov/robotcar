// Copyright © Svetoslav Paregov. All rights reserved.

#include <stdio.h>
#include "pico/stdlib.h"
#include "hardware/irq.h"
#include "hardware/timer.h"
#include "pico_native_pwm.hpp"
#include "servo_control.hpp"

#define TIMER_INTERVAL_US 10000
#define TIMER_INTERVAL_MS (TIMER_INTERVAL_US / 1000)

#define SERVO_SPEED_TABLE_SIZE 10

typedef struct {
    uint8_t min_percentage; // Minimum speed percentage.
    uint8_t max_percentage; // Maximum speed percentage.
    uint16_t min_time_ms;   // Minimum time in milliseconds before change degrees.
} servo_speed_settings_t;

const servo_info_t servo_180 = {
    .degrees = 180,
    .bottom_degrees_limit = 0,
    .top_degrees_limit = 180,
    .left_us = 500.0f,
    .center_us = 1500.0f,
    .right_us = 2500.0f,
    .degree_to_us = ((2500.0f - 500.0f) / 180.0f),
    .pwm_number = 0,    // This will be set later in init_servos()
    .is_inverted = false
};

const servo_info_t servo_270 = {
    .degrees = 270,
    .bottom_degrees_limit = 0,
    .top_degrees_limit = 270,
    .left_us = 500.0f,
    .center_us = 1500.0f,
    .right_us = 2500.0f,
    .degree_to_us = ((2500.0f - 500.0f) / 270.0f),
    .pwm_number = 0,    // This will be set later in init_servos()
    .is_inverted = false
};

const servo_speed_settings_t servos_speed_table[SERVOS_COUNT][SERVO_SPEED_TABLE_SIZE] = {
    {   // Base servo speed settings
        { .min_percentage = 10, .max_percentage = 20, .min_time_ms = 110 },
        { .min_percentage = 20, .max_percentage = 30, .min_time_ms = 100 }, 
        { .min_percentage = 30, .max_percentage = 40, .min_time_ms = 90 }, 
        { .min_percentage = 40, .max_percentage = 50, .min_time_ms = 80 },
        { .min_percentage = 50, .max_percentage = 60, .min_time_ms = 70 },
        { .min_percentage = 60, .max_percentage = 70, .min_time_ms = 60 },
        { .min_percentage = 70, .max_percentage = 80, .min_time_ms = 50 },
        { .min_percentage = 80, .max_percentage = 90, .min_time_ms = 40 },
        { .min_percentage = 90, .max_percentage = 100, .min_time_ms = 30 },
        { .min_percentage = 100, .max_percentage = 110, .min_time_ms = 20 }
    },
    {   // Shoulder servo speed settings
        { .min_percentage = 10, .max_percentage = 20, .min_time_ms = 110 },
        { .min_percentage = 20, .max_percentage = 30, .min_time_ms = 100 }, 
        { .min_percentage = 30, .max_percentage = 40, .min_time_ms = 90 }, 
        { .min_percentage = 40, .max_percentage = 50, .min_time_ms = 80 },
        { .min_percentage = 50, .max_percentage = 60, .min_time_ms = 70 },
        { .min_percentage = 60, .max_percentage = 70, .min_time_ms = 60 },
        { .min_percentage = 70, .max_percentage = 80, .min_time_ms = 50 },
        { .min_percentage = 80, .max_percentage = 90, .min_time_ms = 40 },
        { .min_percentage = 90, .max_percentage = 100, .min_time_ms = 30 },
        { .min_percentage = 100, .max_percentage = 110, .min_time_ms = 20 }
    },
    {   // Elbow servo speed settings
        { .min_percentage = 10, .max_percentage = 20, .min_time_ms = 100 },
        { .min_percentage = 20, .max_percentage = 30, .min_time_ms = 90 }, 
        { .min_percentage = 30, .max_percentage = 40, .min_time_ms = 80 }, 
        { .min_percentage = 40, .max_percentage = 50, .min_time_ms = 70 },
        { .min_percentage = 50, .max_percentage = 60, .min_time_ms = 60 },
        { .min_percentage = 60, .max_percentage = 70, .min_time_ms = 50 },
        { .min_percentage = 70, .max_percentage = 80, .min_time_ms = 40 },
        { .min_percentage = 80, .max_percentage = 90, .min_time_ms = 30 },
        { .min_percentage = 90, .max_percentage = 100, .min_time_ms = 20 },
        { .min_percentage = 100, .max_percentage = 110, .min_time_ms = 10 }
    },
    {   // Arm used right now
        { .min_percentage = 10, .max_percentage = 20, .min_time_ms = 100 },
        { .min_percentage = 20, .max_percentage = 30, .min_time_ms = 90 }, 
        { .min_percentage = 30, .max_percentage = 40, .min_time_ms = 80 }, 
        { .min_percentage = 40, .max_percentage = 50, .min_time_ms = 70 },
        { .min_percentage = 50, .max_percentage = 60, .min_time_ms = 60 },
        { .min_percentage = 60, .max_percentage = 70, .min_time_ms = 50 },
        { .min_percentage = 70, .max_percentage = 80, .min_time_ms = 40 },
        { .min_percentage = 80, .max_percentage = 90, .min_time_ms = 30 },
        { .min_percentage = 90, .max_percentage = 100, .min_time_ms = 20 },
        { .min_percentage = 100, .max_percentage = 110, .min_time_ms = 10 }
    },
    {   // Wrist servo speed settings
        { .min_percentage = 10, .max_percentage = 20, .min_time_ms = 100 },
        { .min_percentage = 20, .max_percentage = 30, .min_time_ms = 90 }, 
        { .min_percentage = 30, .max_percentage = 40, .min_time_ms = 80 }, 
        { .min_percentage = 40, .max_percentage = 50, .min_time_ms = 70 },
        { .min_percentage = 50, .max_percentage = 60, .min_time_ms = 60 },
        { .min_percentage = 60, .max_percentage = 70, .min_time_ms = 50 },
        { .min_percentage = 70, .max_percentage = 80, .min_time_ms = 40 },
        { .min_percentage = 80, .max_percentage = 90, .min_time_ms = 30 },
        { .min_percentage = 90, .max_percentage = 100, .min_time_ms = 20 },
        { .min_percentage = 100, .max_percentage = 110, .min_time_ms = 10 }
    },
    {   // Not used
        { .min_percentage = 10, .max_percentage = 20, .min_time_ms = 100 },
        { .min_percentage = 20, .max_percentage = 30, .min_time_ms = 90 }, 
        { .min_percentage = 30, .max_percentage = 40, .min_time_ms = 80 }, 
        { .min_percentage = 40, .max_percentage = 50, .min_time_ms = 70 },
        { .min_percentage = 50, .max_percentage = 60, .min_time_ms = 60 },
        { .min_percentage = 60, .max_percentage = 70, .min_time_ms = 50 },
        { .min_percentage = 70, .max_percentage = 80, .min_time_ms = 40 },
        { .min_percentage = 80, .max_percentage = 90, .min_time_ms = 30 },
        { .min_percentage = 90, .max_percentage = 100, .min_time_ms = 20 },
        { .min_percentage = 100, .max_percentage = 110, .min_time_ms = 10 }
    },
    {   // Gripper servo speed settings
        { .min_percentage = 10, .max_percentage = 20, .min_time_ms = 100 },
        { .min_percentage = 20, .max_percentage = 30, .min_time_ms = 90 }, 
        { .min_percentage = 30, .max_percentage = 40, .min_time_ms = 80 }, 
        { .min_percentage = 40, .max_percentage = 50, .min_time_ms = 70 },
        { .min_percentage = 50, .max_percentage = 60, .min_time_ms = 60 },
        { .min_percentage = 60, .max_percentage = 70, .min_time_ms = 50 },
        { .min_percentage = 70, .max_percentage = 80, .min_time_ms = 40 },
        { .min_percentage = 80, .max_percentage = 90, .min_time_ms = 30 },
        { .min_percentage = 90, .max_percentage = 100, .min_time_ms = 20 },
        { .min_percentage = 100, .max_percentage = 110, .min_time_ms = 10 }
    },
    {   // Not used
        { .min_percentage = 10, .max_percentage = 20, .min_time_ms = 100 },
        { .min_percentage = 20, .max_percentage = 30, .min_time_ms = 90 }, 
        { .min_percentage = 30, .max_percentage = 40, .min_time_ms = 80 }, 
        { .min_percentage = 40, .max_percentage = 50, .min_time_ms = 70 },
        { .min_percentage = 50, .max_percentage = 60, .min_time_ms = 60 },
        { .min_percentage = 60, .max_percentage = 70, .min_time_ms = 50 },
        { .min_percentage = 70, .max_percentage = 80, .min_time_ms = 40 },
        { .min_percentage = 80, .max_percentage = 90, .min_time_ms = 30 },
        { .min_percentage = 90, .max_percentage = 100, .min_time_ms = 20 },
        { .min_percentage = 100, .max_percentage = 110, .min_time_ms = 10 }
    }
};

servo_info_t servos_info_array[SERVOS_COUNT];

motor_speed_t servo_motor_speeds_array[SERVOS_COUNT] = {
    { .direction = 0, .speed = 0, .elapsed_time = 0, .timeout = 0 },
    { .direction = 0, .speed = 0, .elapsed_time = 0, .timeout = 0 },
    { .direction = 0, .speed = 0, .elapsed_time = 0, .timeout = 0 },
    { .direction = 0, .speed = 0, .elapsed_time = 0, .timeout = 0 },
    { .direction = 0, .speed = 0, .elapsed_time = 0, .timeout = 0 },
    { .direction = 0, .speed = 0, .elapsed_time = 0, .timeout = 0 },
    { .direction = 0, .speed = 0, .elapsed_time = 0, .timeout = 0 },
    { .direction = 0, .speed = 0, .elapsed_time = 0, .timeout = 0 }
};

struct repeating_timer servo_control_timer;

bool should_servo_move(
    servo_speed_settings_t *settings,
    uint8_t speed_percentage,
    uint16_t time_elapsed)
{
    for (int i = 0; i < SERVO_SPEED_TABLE_SIZE; i++)
    {
        if (speed_percentage >= settings[i].min_percentage &&
            speed_percentage < settings[i].max_percentage &&
            time_elapsed >= settings[i].min_time_ms)
        {
            // The servo should move
            return true;
        }
    }

    // The servo should not move
    return false;
}

void process_servo_motor_speed(motor_speed_t *motor, uint8_t motor_index)
{
    motor->timeout -= TIMER_INTERVAL_MS;
    if (motor->timeout <= 0)
    {
        // Stop the motor if timeout has reached
        motor->speed = 0;
        motor->direction = 0;
        motor->timeout = 0; // Reset timeout

        return;
    }

    motor->elapsed_time += TIMER_INTERVAL_MS;

    // Check if the servo should move based on its speed settings
    if (should_servo_move(
        (servo_speed_settings_t *)servos_speed_table[motor_index],
        motor->speed,
        motor->elapsed_time))
    {
        uint16_t new_degrees = servos_info_array[motor_index].current_degrees;
        uint8_t pwm_number = servos_info_array[motor_index].pwm_number;
        bool is_inverted = servos_info_array[motor_index].is_inverted;

        if (motor->direction > 0 && !is_inverted)
        {
            // If the motor is moving forward and not inverted, increase degrees
            new_degrees += 1;
        }
        else if (motor->direction > 0 && is_inverted)
        {
            // If the motor is moving forward and inverted, decrease degrees
            new_degrees -= 1;
        }
        else if (motor->direction < 0 && !is_inverted)
        {
            // If the motor is moving backward and not inverted, decrease degrees
            new_degrees -= 1;
        }
        else if (motor->direction < 0 && is_inverted)
        {
            new_degrees += 1;
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
    process_servo_motor_speed(&servo_motor_speeds_array[BASE_MOTOR_INDEX], BASE_MOTOR_INDEX);
    process_servo_motor_speed(&servo_motor_speeds_array[SHOULDER_MOTOR_INDEX], SHOULDER_MOTOR_INDEX);
    process_servo_motor_speed(&servo_motor_speeds_array[ELBOW_MOTOR_INDEX], ELBOW_MOTOR_INDEX);
    process_servo_motor_speed(&servo_motor_speeds_array[ARM_MOTOR_INDEX], ARM_MOTOR_INDEX);
    process_servo_motor_speed(&servo_motor_speeds_array[WRIST_MOTOR_INDEX], WRIST_MOTOR_INDEX);
    process_servo_motor_speed(&servo_motor_speeds_array[GRIPPER_MOTOR_INDEX], GRIPPER_MOTOR_INDEX);

    return true; // Keep the timer repeating
}

void init_servos()
{
    // Base
    servos_info_array[BASE_MOTOR_INDEX] = servo_270;
    servos_info_array[BASE_MOTOR_INDEX].pwm_number = 0;

    // Shoulder
    servos_info_array[SHOULDER_MOTOR_INDEX] = servo_270;
    servos_info_array[SHOULDER_MOTOR_INDEX].pwm_number = 1;
    servos_info_array[SHOULDER_MOTOR_INDEX].is_inverted = true; // Inverted for this robot arm
    //servos_info_array[SHOULDER_MOTOR_INDEX].bottom_degrees_limit = 45;
    //servos_info_array[SHOULDER_MOTOR_INDEX].top_degrees_limit = 225;

    // Elbow
    servos_info_array[ELBOW_MOTOR_INDEX] = servo_270;
    servos_info_array[ELBOW_MOTOR_INDEX].pwm_number = 2;

    // Arm
    servos_info_array[ARM_MOTOR_INDEX] = servo_270;
    servos_info_array[ARM_MOTOR_INDEX].pwm_number = 3;

    // Wrist
    servos_info_array[WRIST_MOTOR_INDEX] = servo_270; 
    servos_info_array[WRIST_MOTOR_INDEX].pwm_number = 4;

    // Wrist2 - Not present in this Robot Arm
    servos_info_array[5] = servo_270;
    servos_info_array[5].pwm_number = 5;

    // Gripper
    servos_info_array[GRIPPER_MOTOR_INDEX] = servo_270;
    servos_info_array[GRIPPER_MOTOR_INDEX].pwm_number = 6;
    //servos_info_array[GRIPPER_MOTOR_INDEX].bottom_degrees_limit = 45;
    //servos_info_array[GRIPPER_MOTOR_INDEX].top_degrees_limit = 225;

    // Not present in this Robot Arm
    servos_info_array[7] = servo_270;
    servos_info_array[7].pwm_number = 7;
    
    set_servo_position_in_degrees(0, servos_info_array[0].degrees/2); // base
    set_servo_position_in_degrees(1, servos_info_array[1].degrees/2); // shoulder
    set_servo_position_in_degrees(2, servos_info_array[2].degrees/2); // elbow
    set_servo_position_in_degrees(3, servos_info_array[3].degrees/2); // arm
    set_servo_position_in_degrees(4, servos_info_array[4].degrees/2); // wrist
    // set_servo_position_in_degrees(5, servos[5].degrees/2); // wrist2 - Not present in this Robot Arm
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
    else if (degrees <= 0.0f)
    {
        degrees = 0.0f; // If we try to set a negative degrees, we will set 0 degrees.
    }

    // Since we are using absolute position in degrees, we are getting the left position in microseconds.
    float pulse_width = servos_info_array[servo].left_us +
                        (servos_info_array[servo].degree_to_us * degrees);
    if (pulse_width > servos_info_array[servo].right_us)
    {
        // If we try to set a bigger than allowed pulse width, we will set the maximum allowed pulse width.
        pulse_width = servos_info_array[servo].right_us;
    }

    if (pulse_width < servos_info_array[servo].left_us)
    {
        // If we try to set a smaller than allowed pulse width, we will set the minimum allowed pulse width.
        pulse_width = servos_info_array[servo].left_us;
    }

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

bool set_servo_motor_speed(uint8_t servo, motor_speed_t speed)
{
    if (servo >= SERVOS_COUNT)
    {
        printf("Error: Invalid servo index %d. Must be between 0 and %d.\n", servo, SERVOS_COUNT - 1);
        return false;
    }

    servo_motor_speeds_array[servo] = speed;

    return true;
}
