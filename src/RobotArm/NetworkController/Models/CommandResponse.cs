// Copyright © Svetoslav Paregov. All rights reserved.

namespace NetworkController.Models
{
    public class CommandResponse
    {
        public int Version { get; set; } = 1;

        public bool IsSuccess { get; set; } = true;
    }
}
