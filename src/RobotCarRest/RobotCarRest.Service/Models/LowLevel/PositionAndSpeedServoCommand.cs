using System;

namespace Paregov.RobotCar.Rest.Service.Models.LowLevel
{
    public class PositionAndSpeedServoCommand
    {
        public PositionAndSpeedServoCommand(
            Int16 position,
            byte speed)
        {
            Position = position;
            Speed = speed;
        }

        // 2 bytes for position (Int16)
        public Int16 Position { get; set; } = Int16.MinValue; // Position in degrees (-180 to 180)

        // 1 byte for speed (0-100%)
        public byte Speed { get; set; } = 0; // Speed in percentage (0-100)
    }
}
