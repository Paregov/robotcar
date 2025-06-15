// Copyright © Svetoslav Paregov. All rights reserved.

using System.Collections.Generic;

namespace NetworkController.Models
{
    public class CommandServosVelocity
    {
        public int Version { get; set; } = 1;

        public List<CommandServoVelocity> Servos { get; set; } = new();
    }

    public class CommandServoVelocity
    {
        public int Velocity { get; set; }

        public int Acceleration { get; set; }

        public int TimeOutMilliseconds { get; set; } = 1000;
    }
}
