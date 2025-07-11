// Copyright © Svetoslav Paregov. All rights reserved.


using System.Text.Json.Serialization;

namespace Paregov.RobotCar.Rest.Service.Models.LowLevel
{
    public class LowLevelCommand
    {
        [JsonPropertyName("lw")]
        public SingleMotorCommand LeftWheel { get; set; } = new();

        [JsonPropertyName("rw")]
        public SingleMotorCommand RightWheel { get; set; } = new();

        [JsonPropertyName("smb")]
        public SingleMotorCommand Base { get; set; } = new();

        [JsonPropertyName("sms")]
        public SingleMotorCommand Shoulder { get; set; } = new();

        [JsonPropertyName("sme")]
        public SingleMotorCommand Elbow { get; set; } = new();

        [JsonPropertyName("sma")]
        public SingleMotorCommand Arm { get; set; } = new();

        [JsonPropertyName("smw")]
        public SingleMotorCommand Wrist { get; set; } = new();

        [JsonPropertyName("smg")]
        public SingleMotorCommand Gripper { get; set; } = new();
    }

    public class SingleMotorCommand
    {
        public static SingleMotorCommand FromReading(int reading)
        {
            var command = new SingleMotorCommand();
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
