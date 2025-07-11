// Copyright © Svetoslav Paregov. All rights reserved.

#include "stdlib.h"
#include "pico/stdlib.h"
#include "pico/sync.h"
#include "hardware/pwm.h"
#include "dc_motors_control.hpp"
#include "pico_native_pwm.hpp"

#define TIMER_INTERVAL_US 10000
#define LEFT_MOTOR_INDEX 0
#define RIGHT_MOTOR_INDEX 1

// Define the GPIO pins for the motors
const uint LEFT_MOTOR_FORWARD_PIN = 27;     // IN2
const uint LEFT_MOTOR_BACKWARD_PIN = 26;    // IN1
const uint RIGHT_MOTOR_FORWARD_PIN = 14;    // IN4
const uint RIGHT_MOTOR_BACKWARD_PIN = 15;   // IN3

motor_speed_t dc_motors_speeds[2] = {
    { .direction = 0, .speed = 0, .timeout = 0 }, // Left motor
    { .direction = 0, .speed = 0, .timeout = 0 }  // Right motor
};

struct repeating_timer dc_motors_control_timer;

void process_dc_motor_speed(
    motor_speed_t *motor,
    uint gpio_forward,
    uint gpio_backward,
    int pwm_index)
{
    // Convert microseconds to milliseconds.
    motor->timeout -= (TIMER_INTERVAL_US / 1000);
    if (motor->timeout <= 0)
    {
        // Stop the motor if timeout has reached
        motor->speed = 0;
        motor->direction = 0;
        motor->timeout = 0; // Reset timeout

        set_pwm_duty_cycle_in_percent(pwm_index, 0);
        gpio_put(gpio_forward, 0);
        gpio_put(gpio_backward, 0);

        return;
    }

    if (motor->direction > 0)
    {
        // Forward direction
        gpio_put(gpio_forward, 1);
        gpio_put(gpio_backward, 0);
    }
    else if (motor->direction < 0)
    {
        // Backward direction
        gpio_put(gpio_forward, 0);
        gpio_put(gpio_backward, 1);
    }
    else
    {
        // Stop the motor
        gpio_put(gpio_forward, 0);
        gpio_put(gpio_backward, 0);
    }
    
    set_pwm_duty_cycle_in_percent(pwm_index, motor->speed);
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
bool dc_motors_timer_callback(struct repeating_timer *t)
{
    process_dc_motor_speed(
        &dc_motors_speeds[LEFT_MOTOR_INDEX],
        LEFT_MOTOR_FORWARD_PIN,
        LEFT_MOTOR_BACKWARD_PIN,
        PWM_NUMBER_DC_MOTOR_LEFT);
    process_dc_motor_speed(
        &dc_motors_speeds[RIGHT_MOTOR_INDEX],
        RIGHT_MOTOR_FORWARD_PIN,
        RIGHT_MOTOR_BACKWARD_PIN,
        PWM_NUMBER_DC_MOTOR_RIGHT);

    return true; // Keep the timer repeating
}

void init_dc_motors()
{
    // Initialize GPIO pins for motor control
    gpio_init(LEFT_MOTOR_FORWARD_PIN);
    gpio_set_dir(LEFT_MOTOR_FORWARD_PIN, GPIO_OUT);
    gpio_init(LEFT_MOTOR_BACKWARD_PIN);
    gpio_set_dir(LEFT_MOTOR_BACKWARD_PIN, GPIO_OUT);
    gpio_init(RIGHT_MOTOR_FORWARD_PIN);
    gpio_set_dir(RIGHT_MOTOR_FORWARD_PIN, GPIO_OUT);
    gpio_init(RIGHT_MOTOR_BACKWARD_PIN);
    gpio_set_dir(RIGHT_MOTOR_BACKWARD_PIN, GPIO_OUT);

    // Set initial state to stop motors
    gpio_put(LEFT_MOTOR_FORWARD_PIN, 0);
    gpio_put(LEFT_MOTOR_BACKWARD_PIN, 0);
    gpio_put(RIGHT_MOTOR_FORWARD_PIN, 0);
    gpio_put(RIGHT_MOTOR_BACKWARD_PIN, 0);

    set_dc_motors_speed(
        (motor_speed_t){ .direction = 0, .speed = 0, .timeout = 0 }, // Left motor
        (motor_speed_t){ .direction = 0, .speed = 0, .timeout = 0 }  // Right motor
    );
    
    // Create a repeating timer that calls dc_motors_timer_callback.
    // The first argument is the interval in microseconds.
    // A negative value means the timer will be fired relative to the previous scheduled fire time,
    // which is better for periodic tasks to avoid drift.
    // The last argument is a pointer where the SDK will store timer information.
    add_repeating_timer_us(-TIMER_INTERVAL_US, dc_motors_timer_callback, NULL, &dc_motors_control_timer);
}

void set_dc_motors_speed(motor_speed_t left, motor_speed_t right)
{
    uint32_t saved = save_and_disable_interrupts();
    
    dc_motors_speeds[LEFT_MOTOR_INDEX] = left;
    dc_motors_speeds[RIGHT_MOTOR_INDEX] = right;

    restore_interrupts(saved);
}

void set_left_dc_motor_speed(motor_speed_t speed)
{
    dc_motors_speeds[LEFT_MOTOR_INDEX] = speed;
}

void set_right_dc_motor_speed(motor_speed_t speed)
{
    dc_motors_speeds[RIGHT_MOTOR_INDEX] = speed;
}
