// Copyright © Svetoslav Paregov. All rights reserved.

namespace Paregov.RobotCar.Rest.Service.Models
{
    public class CommandResponse
    {
        public int Version { get; set; } = 1;

        public bool IsSuccess { get; set; } = true;

        /// <summary>
        /// Message describing what went wrong when IsSuccess is false.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Detailed information for debugging purposes.
        /// </summary>
        public string? Details { get; set; }
    }
}
