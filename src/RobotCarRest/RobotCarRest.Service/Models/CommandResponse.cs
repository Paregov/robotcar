// Copyright © Svetoslav Paregov. All rights reserved.

namespace Paregov.RobotCar.Rest.Service.Models
{
    public class CommandResponse
    {
        public int Version { get; set; } = 1;

        public bool IsSuccess { get; set; } = true;
    }
}
