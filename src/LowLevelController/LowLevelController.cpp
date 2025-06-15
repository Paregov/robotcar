// Copyright © Svetoslav Paregov. All rights reserved.

#include <stdio.h>
#include "pico/stdlib.h"

#include "commands_protocol.hpp"
#include "dc_motors_control.hpp"
#include "logger.hpp"
#include "pico_native_pwm.hpp"
#include "servo_control.hpp"
#include "spi_transport.hpp"
#include "uart_transport.hpp"

const uint LED_PIN = 25;

int main()
{
    const int led_blink_interval_ms = 1000;
    const int delay_ms = 10;
    uint8_t led = 1;
    int led_blink_interval = 500;

    stdio_init_all();


    gpio_init(LED_PIN);
    gpio_set_dir(LED_PIN, GPIO_OUT);
    gpio_put(LED_PIN, 0);

    init_logger();
    init_pwms();
    init_servos();
    init_dc_motors();
    init_commands_protocol();
    init_spi();
    //init_uart_transport();

    gpio_put(LED_PIN, led);
    
    while (1)
    {
        process_commands_protocol();

        sleep_ms(delay_ms);
        led_blink_interval  -= delay_ms;
        if (led_blink_interval <= 0)
        {
            led_blink_interval = led_blink_interval_ms;
            led = led ^ 1; // Toggle the LED state
            gpio_put(LED_PIN, led);
        }
    }
    
    return 0;
}
