// Copyright © Svetoslav Paregov. All rights reserved.

using System;

namespace NetworkController.Hardware
{
    public interface ISpiCommunicator : IDisposable
    {
        public bool SendMessage(string message);

        public bool SendBytesMessage(byte[] message);
    }
}
