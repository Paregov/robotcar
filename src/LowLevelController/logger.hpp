// Copyright © Svetoslav Paregov. All rights reserved.

#ifndef LOGGER_HPP
#define LOGGER_HPP

#include <stdio.h>
#include "pico/stdlib.h"

#define LOGGER_ENABLED 0

void init_logger();
size_t log_message(char *message);

#endif // LOGGER_HPP