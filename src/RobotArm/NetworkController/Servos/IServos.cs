﻿// Copyright © Svetoslav Paregov. All rights reserved.

namespace NetworkController.Servos
{
    public interface IServos
    {
        public bool SetAllServosToCenter();

        public bool SetServoPosition(
            int servoId,
            float position,
            float acceleration,
            float velocity);

        public float GetServoPosition(int servoId);
    }
}
