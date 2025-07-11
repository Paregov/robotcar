#ifndef COMMON_TYPES_HPP
#define COMMON_TYPES_HPP

typedef enum {
    INVALID_COMMAND = 0,
    BASE_MOTOR_COMMAND = 1,
    SHOULDER_MOTOR_COMMAND = 2,
    ELBOW_MOTOR_COMMAND = 3,
    ARM_MOTOR_COMMAND = 4,
    WRIST_MOTOR_COMMAND = 5,
    WRIST_2_MOTOR_COMMAND = 6,
    GRIPPER_MOTOR_COMMAND = 7,
    LEFT_MOTOR_COMMAND = 8,
    RIGHT_MOTOR_COMMAND = 9,
    LEFT_REAR_MOTOR_COMMAND = 10,
    RIGHT_REAR_MOTOR_COMMAND = 11,
    STOP_ALL_MOTORS_COMMAND = 12
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
} motor_command_t;

typedef struct {
    motor_command_t lw;  // left wheel
    motor_command_t rw;  // right wheel
    motor_command_t smb; // servo motor base
    motor_command_t sms; // servo motor shoulder
    motor_command_t sme; // servo motor elbow
    motor_command_t sma; // servo motor wrist angle
    motor_command_t smw; // servo motor wrist rotation
    motor_command_t smg; // servo motor gripper
} all_motors_command_t;

#endif // COMMON_TYPES_HPP
