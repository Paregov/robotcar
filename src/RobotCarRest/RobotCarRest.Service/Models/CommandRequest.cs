// Copyright © Svetoslav Paregov. All rights reserved.

using NetworkController.Models.Enums;

namespace Paregov.RobotCar.Rest.Service.Models
{
    public class CommandRequest
    {
        public int Version { get; set; } = 1;

        public Command Command { get; set; }

        public SubCommand SubCommand { get; set; }

        public float Value { get; set; }

        public float Speed { get; set; }
    }
}
