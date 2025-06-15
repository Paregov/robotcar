// Copyright © Svetoslav Paregov. All rights reserved.

#ifndef COMMANDS_PROTOCOL_HPP
#define COMMANDS_PROTOCOL_HPP


// Represents a single joystick object from the JSON array
typedef struct {
    int d; // Direction.
    int s; // Speed in percentage (0-100).
    int t; // Timeout in milliseconds. When to stop the motor if no new command is received.
} Joystick;

// Represents the top-level parsed data, containing an array of joysticks
typedef struct {
    Joystick* joysticks; // Pointer to the buffer holding joystick data
    int count;           // Number of joysticks parsed
} JoystickData;

void init_commands_protocol();
void process_commands_protocol();

#endif // COMMANDS_PROTOCOL_HPP
