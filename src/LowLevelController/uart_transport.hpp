// Copyright © Svetoslav Paregov. All rights reserved.

#ifndef UART_TRANSPORT_HPP
#define UART_TRANSPORT_HPP

void init_uart_transport();

// If command is received, it will return the length of the command.
uint32_t uart_get_received_message(uint8_t* buffer, uint32_t max_length);

#endif // UART_TRANSPORT_HPP
