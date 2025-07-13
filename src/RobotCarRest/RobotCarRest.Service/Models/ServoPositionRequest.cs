// Copyright © Svetoslav Paregov. All rights reserved.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Paregov.RobotCar.Rest.Service.Models
{
    /// <summary>
    /// Request model for setting a servo position with control parameters.
    /// </summary>
    public class SetServoPositionRequest
    {
        /// <summary>
        /// The servo ID to control (0-based index).
        /// </summary>
        [Required]
        [Range(0, 255, ErrorMessage = "Servo ID must be between 0 and 255")]
        public int ServoId { get; set; }

        /// <summary>
        /// The target position in degrees.
        /// </summary>
        [Required]
        [Range(-180, 180, ErrorMessage = "Position must be between -180 and 180 degrees")]
        public float PositionDegrees { get; set; }

        /// <summary>
        /// The acceleration for the movement (degrees per second squared).
        /// </summary>
        [Range(0, 10000, ErrorMessage = "Acceleration must be between 0 and 10000 degrees/s²")]
        public float AccelerationDegreesPerSecSquared { get; set; } = 100.0f;

        /// <summary>
        /// The maximum velocity for the movement (degrees per second).
        /// </summary>
        [Range(0, 1000, ErrorMessage = "Velocity must be between 0 and 1000 degrees/s")]
        public float VelocityDegreesPerSecond { get; set; } = 50.0f;

        /// <summary>
        /// The timeout for the movement in milliseconds.
        /// </summary>
        [Range(100, 30000, ErrorMessage = "Timeout must be between 100 and 30000 milliseconds")]
        public int TimeoutMs { get; set; } = 5000;
    }

    /// <summary>
    /// Request model for setting multiple servo positions simultaneously.
    /// </summary>
    public class SetMultipleServosRequest
    {
        /// <summary>
        /// List of servo position commands to execute.
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one servo command is required")]
        public List<SetServoPositionRequest> ServoCommands { get; set; } = new List<SetServoPositionRequest>();

        /// <summary>
        /// Whether to execute all commands simultaneously or sequentially.
        /// </summary>
        public bool ExecuteSimultaneously { get; set; } = true;
    }

    /// <summary>
    /// Response model for servo position operations.
    /// </summary>
    public class ServoPositionResponse : CommandResponse
    {
        /// <summary>
        /// The servo ID that was controlled.
        /// </summary>
        public int? ServoId { get; set; }

        /// <summary>
        /// The current position of the servo in degrees (if available).
        /// </summary>
        public float? CurrentPositionDegrees { get; set; }

        /// <summary>
        /// Additional details about the operation.
        /// </summary>
        public string? Details { get; set; }
    }
}
