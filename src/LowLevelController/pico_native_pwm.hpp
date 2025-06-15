// Copyright © Svetoslav Paregov. All rights reserved.

#ifndef PICO_NATIVE_PWM_HPP
#define PICO_NATIVE_PWM_HPP

#include <stdio.h>
#include "pico/stdlib.h"

// We have 8 PWM channels for servos and 2 for DC motors.
#define PWMS_COUNT  10
#define PWM_FREQUENCY 50
#define PWM_PERIOD (1000000/PWM_FREQUENCY)
#define PWM_CLOCK_DIVIDER (float)38.19
#define PWM_WRAP (uint16_t)65465

#define PWM_NUMBER_DC_MOTOR_LEFT    8
#define PWM_NUMBER_DC_MOTOR_RIGHT   9

void init_pwms();
void set_pwm_pulse_width_us(uint8_t pwmNumber, uint16_t pulseWidthUs);
void set_pwm_duty_cycle_in_percent(uint8_t pwmNumber, float percent);

#endif // PICO_NATIVE_PWM_HPP
