// Copyright © Svetoslav Paregov. All rights reserved.

using Microsoft.Extensions.Logging;
using Paregov.RobotCar.Rest.Service.Hardware;

namespace Paregov.RobotCar.Rest.Service.Servos
{
    public class Servos : IServos
    {
        private readonly ILogger<Servos> _logger;
        private readonly IHardwareControl _hardwareControl;

        public Servos(
            ILogger<Servos> logger,
            IHardwareControl hardwareControl)
        {
            _logger = logger;
            _hardwareControl = hardwareControl;
        }

        public bool SetAllServosToCenter()
        {
            return true;
        }

        public bool SetServoPosition(int servoId, float position, float acceleration, float velocity)
        {
            return true;
        }

        public bool SetServoPosition(int servoId, float position)
        {
            return true;
        }

        public float GetServoPosition(int servoId)
        {
            return 0.0f;
        }
    }
}
