// Copyright © Svetoslav Paregov. All rights reserved.

#include <stdio.h>
#include "pico/stdlib.h"
#include "pico/time.h"
#include "hardware/pwm.h"
#include "pico_native_pwm.hpp"

// GP numbers
uint16_t pwmNumberToGpio[PWMS_COUNT] = { 2, 3, 6, 7, 8, 9, 10, 11, 21, 20 };

void init_pwms()
{
    // Set LED pins to use PWM
    for (uint8_t i = 0; i < PWMS_COUNT; i++)
    {
        gpio_set_function(pwmNumberToGpio[i], GPIO_FUNC_PWM);
    }

    // Configure PWM settings
    pwm_config config = pwm_get_default_config();
    // 125Mhz main clock
    // Set clock divider (resolution)
    pwm_config_set_clkdiv(&config, PWM_CLOCK_DIVIDER);
    pwm_config_set_wrap(&config, PWM_WRAP);

    // Initialize PWM for each servo
    uint slice_num;
    for (uint8_t i = 0; i < PWMS_COUNT; i++)
    {
        slice_num = pwm_gpio_to_slice_num(pwmNumberToGpio[i]);
        pwm_init(slice_num, &config, true);
    }
}

void set_pwm_pulse_width_us(uint8_t pwmNumber, uint16_t pulseWidthUs)
{
    if (0 == pulseWidthUs)
    {
        pwm_set_gpio_level(pwmNumberToGpio[pwmNumber], 0);
    }
    else if (PWM_PERIOD <= pulseWidthUs)
    {
        pwm_set_gpio_level(pwmNumberToGpio[pwmNumber], PWM_WRAP);
    }
    else
    {
        float pwmValue = ((float)PWM_WRAP * (float)pulseWidthUs) / 20000.0f;
        pwm_set_gpio_level(pwmNumberToGpio[pwmNumber], (uint16_t)pwmValue);
    }
}

void set_pwm_duty_cycle_in_percent(uint8_t pwmNumber, float percent)
{
    if (percent < 0.0f)
    {
        percent = 0.0f;
    }
    else if (percent > 100.0f)
    {
        percent = 100.0f;
    }

    uint16_t level = (uint16_t)((percent / 100.0f) * PWM_WRAP);
    set_pwm_pulse_width_us(pwmNumber, level);
}
