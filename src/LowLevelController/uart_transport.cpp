// Copyright © Svetoslav Paregov. All rights reserved.

#include <stdio.h>
#include "pico/stdlib.h"
#include "hardware/uart.h"
#include "hardware/irq.h"
#include "uart_transport.hpp"

// Configuration
#define UART_ID         uart0
#define BAUD_RATE       115200
#define UART_TX_PIN     0
#define UART_RX_PIN     1

// Define the start and end of message markers
const uint8_t START_BYTES[] = {0xAA, 0xBB, 0xCC};
const uint8_t END_BYTES[] = {0xDD, 0xEE, 0xFF};

#define MARKER_LEN sizeof(START_BYTES)

// Define the maximum size for the data payload (between start and end markers)
#define DATA_BUFFER_SIZE 512

// Buffer to store the incoming data payload
volatile uint8_t data_buffer[DATA_BUFFER_SIZE];
// Stores the final length of the received message payload
volatile uint32_t received_message_len = 0;
// Flag to indicate that a complete message has been received and is ready for processing
volatile bool message_ready = false;

// State machine for parsing the UART stream
typedef enum {
    STATE_WAITING_FOR_START,
    STATE_RECEIVING_DATA
} receive_state_t;

volatile receive_state_t current_state = STATE_WAITING_FOR_START;

// Index for matching start/end byte sequences
volatile uint32_t marker_match_idx = 0;
// Index for writing data into the main buffer
volatile uint32_t buffer_idx = 0;

// --- UART RX Interrupt Service Routine ---
// This function is called every time the UART receives data.
void on_uart_rx()
{
    while (uart_is_readable(UART_ID))
    {
        uint8_t ch = uart_getc(UART_ID);

        switch (current_state)
        {
            // State 1: Looking for the 3-byte start sequence
            case STATE_WAITING_FOR_START:
                if (ch == START_BYTES[marker_match_idx])
                {
                    marker_match_idx++;
                    // If all 3 start bytes have been matched in order
                    if (marker_match_idx >= MARKER_LEN)
                    {
                        current_state = STATE_RECEIVING_DATA; // Change state
                        marker_match_idx = 0; // Reset marker index for the end sequence
                        buffer_idx = 0; // Reset data buffer index
                    }
                }
                else
                {
                    // If the byte does not match, reset the sequence matching
                    marker_match_idx = 0;
                }
                break;

            // State 2: We have found the start sequence, now capture data until the end sequence
            case STATE_RECEIVING_DATA:
                // First, store the character in the buffer
                if (buffer_idx < DATA_BUFFER_SIZE)
                {
                    data_buffer[buffer_idx++] = ch;
                }
                else
                {
                    // Buffer overflow! Discard message and go back to waiting for a new one.
                    current_state = STATE_WAITING_FOR_START;
                    marker_match_idx = 0;
                    buffer_idx = 0;
                    break; // Exit the switch
                }
                
                // Now, check if this byte is part of the end sequence
                if (ch == END_BYTES[marker_match_idx])
                {
                    marker_match_idx++;
                    // If all 3 end bytes have been matched
                    if (marker_match_idx >= MARKER_LEN)
                    {
                        // A complete message is received!
                        message_ready = true;
                        // The final message length is the buffer index minus the end bytes
                        received_message_len = buffer_idx - MARKER_LEN;
                        // Reset state machine to look for the next message
                        current_state = STATE_WAITING_FOR_START;
                        marker_match_idx = 0;
                        buffer_idx = 0;
                    }
                }
                else
                {
                    // If byte doesn't match the end sequence, reset the end-marker matching
                    marker_match_idx = 0;
                }
                break;
        }
    }
}


void init_uart_transport()
{
    // Initialize the UART with the specified baud rate
    uart_init(UART_ID, BAUD_RATE);

    // Set the TX and RX pins for the UART
    gpio_set_function(UART_TX_PIN, GPIO_FUNC_UART);
    gpio_set_function(UART_RX_PIN, GPIO_FUNC_UART);

    // Set the data format: 8 data bits, 1 stop bit, no parity
    uart_set_format(UART_ID, 8, 1, UART_PARITY_NONE);

    // Disable flow control
    uart_set_hw_flow(UART_ID, false, false);

    // Disable FIFO
    uart_set_fifo_enabled(UART_ID, false);

    // Set up the RX interrupt handler
    int UART_IRQ = (UART_ID == uart0) ? UART0_IRQ : UART1_IRQ;
    irq_set_exclusive_handler(UART_IRQ, on_uart_rx);
    irq_set_enabled(UART_IRQ, true);
    
    // Enable RX interrupts
    uart_set_irq_enables(UART_ID, true, false);
}

uint32_t uart_get_received_message(uint8_t* buffer, uint32_t max_length)
{
    if (message_ready && received_message_len > 0)
    {
        if (max_length < received_message_len)
        {
            // If the provided buffer is smaller than the received message return the length
            // of the received message, but do not copy the data.
            // This will indicate that the buffer was too small.
            // The caller can handle this case as needed.
            return received_message_len;
        }
        
        // Copy the data to the provided buffer
        for (uint32_t i = 0; i < received_message_len; i++)
        {
            buffer[i] = data_buffer[i];
        }
        
        // Reset the message ready flag
        message_ready = false;
        
        // Return the actual length of the copied message
        return received_message_len;
    }
    
    return 0; // No message ready
}
