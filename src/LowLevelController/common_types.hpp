#ifndef COMMON_TYPES_HPP
#define COMMON_TYPES_HPP

typedef enum {
    INVALID_COMMAND = 0,
    BASE_MOTOR_DIRECTION_COMMAND = 1,
    SHOULDER_MOTOR_DIRECTION_COMMAND = 2,
    ELBOW_MOTOR_DIRECTION_COMMAND = 3,
    ARM_MOTOR_COMMAND = 4,
    WRIST_MOTOR_COMMAND = 5,
    WRIST_2_MOTOR_COMMAND = 6,
    GRIPPER_MOTOR_COMMAND = 7,
    LEFT_MOTOR_COMMAND = 8,
    RIGHT_MOTOR_COMMAND = 9,
    LEFT_REAR_MOTOR_COMMAND = 10,
    RIGHT_REAR_MOTOR_COMMAND = 11,
    STOP_ALL_MOTORS_COMMAND = 12,
    BASE_MOTOR_POSITION_COMMAND = 13,
    SHOULDER_MOTOR_POSITION_COMMAND = 14,
    ELBOW_MOTOR_POSITION_COMMAND = 15,
    ARM_MOTOR_POSITION_COMMAND = 16,
    WRIST_MOTOR_POSITION_COMMAND = 17,
    GRIPPER_MOTOR_POSITION_COMMAND = 18,
} command_type_t;

// Represents a single command received from the transport layer.
typedef struct
{
    // Type of the command, e.g., MOTOR_COMMAND.
    command_type_t type;

    // Data associated with the command.
    // The size of the data is determined by the command type.
    uint8_t data[7];
} command_8_bytes_t;

typedef struct
{
    // Direction of the motor: 1 for forward, -1 for backward.
    int8_t direction;

    // Speed in percentage.
    uint8_t speed;

    // Elapsed time in milliseconds since the last update.
    int16_t elapsed_time;

    // Timeout in milliseconds when to stop the servo.
    int16_t timeout;
} motor_speed_t;

// 4 bytes command structure for the motor control.
typedef struct {
    int8_t d;   // Direction.
    uint8_t s;  // Speed in percentage (0-100).
    uint16_t t; // Timeout in milliseconds. When to stop the motor if no new command is received.
} direction_speed_motor_command_t;

// 4 bytes command structure for the motor control.
typedef struct {
    int16_t position;   // Position.
    uint8_t speed;  // Speed in percentage (0-100).
} position_motor_command_t;

typedef struct {
    direction_speed_motor_command_t lw;  // left wheel
    direction_speed_motor_command_t rw;  // right wheel
    direction_speed_motor_command_t smb; // servo motor base
    direction_speed_motor_command_t sms; // servo motor shoulder
    direction_speed_motor_command_t sme; // servo motor elbow
    direction_speed_motor_command_t sma; // servo motor wrist angle
    direction_speed_motor_command_t smw; // servo motor wrist rotation
    direction_speed_motor_command_t smg; // servo motor gripper
} all_motors_command_t;

#endif // COMMON_TYPES_HPP
