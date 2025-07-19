// Copyright © Svetoslav Paregov. All rights reserved.

using System.Collections.Generic;

namespace Paregov.RobotCar.Rest.Service.BusinessLogic.Interfaces
{
    public interface IServos
    {
        public bool SetAllServosToCenter();

        public bool SetServoPosition(
            int servoId,
            float position,
            float acceleration,
            float velocity);

        /// <summary>
        /// Sets a servo position with full control parameters including timeout.
        /// </summary>
        /// <param name="servoId">The servo ID (0-based index)</param>
        /// <param name="positionDegrees">Target position in degrees</param>
        /// <param name="accelerationDegreesPerSecSquared">Acceleration in degrees per second squared</param>
        /// <param name="velocityDegreesPerSecond">Velocity in degrees per second</param>
        /// <param name="timeoutMs">Timeout in milliseconds</param>
        /// <returns>True if the command was sent successfully</returns>
        public bool SetServoPositionWithTimeout(
            int servoId,
            float positionDegrees,
            float accelerationDegreesPerSecSquared,
            float velocityDegreesPerSecond,
            int timeoutMs);

        /// <summary>
        /// Sets multiple servo positions using a List of tuples.
        /// </summary>
        /// <param name="servoCommands">List of servo position commands</param>
        /// <param name="executeSimultaneously">Whether to execute all commands at once</param>
        /// <returns>True if all commands were sent successfully</returns>
        public bool SetMultipleServoPositions(
            List<(int servoId, float positionDegrees, float acceleration, float velocity, int timeoutMs)> servoCommands,
            bool executeSimultaneously = true);

        public float GetServoPosition(int servoId);

        /// <summary>
        /// Gets the current position of a servo in degrees.
        /// </summary>
        /// <param name="servoId">The servo ID (0-based index)</param>
        /// <returns>Current position in degrees</returns>
        public float GetServoPositionDegrees(int servoId);
    }
}
