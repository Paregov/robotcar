// Copyright © Svetoslav Paregov. All rights reserved.

namespace NetworkController.Models.Enums
{
    public enum CommandType
    {
        InvalidCommand = 0,
        BaseMotorCommand = 1,
        ShoulderMotorCommand = 2,
        ElbowMotorCommand = 3,
        ArmMotorCommand = 4,
        WristMotorCommand = 5,
        Wrist2MotorCommand = 6,
        GripperMotorCommand = 7,
        LeftMotorCommand = 8,
        RightMotorCommand = 9,
        LeftRearMotorCommand = 10,
        RightRearMotorCommand = 11,
        StopAllMotorsCommand = 12,
    }
}
