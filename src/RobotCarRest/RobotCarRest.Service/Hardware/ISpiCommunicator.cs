// Copyright © Svetoslav Paregov. All rights reserved.

using System;

namespace Paregov.RobotCar.Rest.Service.Hardware
{
    public interface ISpiCommunicator : IDisposable
    {
        public bool SendMessage(string message);

        public bool SendBytesMessage(byte[] message);
    }
}
