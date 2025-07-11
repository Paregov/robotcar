// Copyright © Svetoslav Paregov. All rights reserved.

namespace Paregov.RobotCar.Rest.Service.Models
{
    public class ServoPositionCmd
    {
        public float Position { get; set; }

        public float Acceleration { get; set; }

        public float Velocity { get; set; }
    }
}
