// Copyright © Svetoslav Paregov. All rights reserved.

using System.Collections.Generic;

namespace Paregov.RobotCar.Rest.Service.Models
{
    public class JoysticksRequest
    {
        public List<int> Joysticks { get; set; } = new List<int>();
    }
}
