// Copyright © Svetoslav Paregov. All rights reserved.

#include <stdio.h>
#include "pico/stdlib.h"
#include "common_types.hpp"

void init_spi();

// If command is received, it will return the length of the command.
uint32_t spi_get_received_message(uint8_t* buffer, uint32_t max_length);
command_8_bytes_t spi_get_received_command();
