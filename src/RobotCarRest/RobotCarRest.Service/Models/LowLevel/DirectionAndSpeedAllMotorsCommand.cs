// Copyright © Svetoslav Paregov. All rights reserved.


using System.Text.Json.Serialization;

namespace Paregov.RobotCar.Rest.Service.Models.LowLevel
{
    public class DirectionAndSpeedAllMotorsCommand
    {
        [JsonPropertyName("lw")]
        public DirectionAndSpeedMotorCommand LeftWheel { get; set; } = new();

        [JsonPropertyName("rw")]
        public DirectionAndSpeedMotorCommand RightWheel { get; set; } = new();

        [JsonPropertyName("smb")]
        public DirectionAndSpeedMotorCommand Base { get; set; } = new();

        [JsonPropertyName("sms")]
        public DirectionAndSpeedMotorCommand Shoulder { get; set; } = new();

        [JsonPropertyName("sme")]
        public DirectionAndSpeedMotorCommand Elbow { get; set; } = new();

        [JsonPropertyName("sma")]
        public DirectionAndSpeedMotorCommand Arm { get; set; } = new();

        [JsonPropertyName("smw")]
        public DirectionAndSpeedMotorCommand Wrist { get; set; } = new();

        [JsonPropertyName("smg")]
        public DirectionAndSpeedMotorCommand Gripper { get; set; } = new();
    }

    public class DirectionAndSpeedMotorCommand
    {
        public static DirectionAndSpeedMotorCommand FromReading(int reading)
        {
            var command = new DirectionAndSpeedMotorCommand();
            // Treat small readings as zero to avoid jitter.
            reading = reading is > 117 and < 138 ? 127 : reading;

            if (reading < 127)
            {
                command.Direction = -1;
                command.Speed = (128 - reading) * 100 / 128; // Convert to percentage
            }
            else if (reading > 127)
            {
                command.Direction = 1;
                command.Speed = (reading - 128) * 100 / 127; // Convert to percentage
            }
            else
            {
                command.Direction = 0; // No movement
                command.Speed = 0;
            }

            return command;
        }

        [JsonPropertyName("d")]
        public int Direction { get; set; } = 1; // 1 for forward, -1 for backward

        [JsonPropertyName("s")]
        public int Speed { get; set; } = 0; // Speed in percentage (0-100)

        [JsonPropertyName("t")]
        public int TimeOutMilliseconds { get; set; } = 300; // Timeout in milliseconds
    }
}
