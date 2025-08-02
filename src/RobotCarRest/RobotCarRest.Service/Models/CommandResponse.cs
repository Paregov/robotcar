// Copyright © Svetoslav Paregov. All rights reserved.

namespace Paregov.RobotCar.Rest.Service.Models
{
    public class CommandResponse
    {
        public int Version { get; set; } = 1;

        public bool IsSuccess { get; set; } = true;

        /// <summary>
        /// Error message describing what went wrong when IsSuccess is false.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Detailed error information for debugging purposes.
        /// </summary>
        public string? ErrorDetails { get; set; }
    }
}
