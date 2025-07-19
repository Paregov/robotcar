// Copyright © Svetoslav Paregov. All rights reserved.

namespace Paregov.RobotCar.Rest.Service.Models.Enums
{
    public enum CommandType
    {
        InvalidCommand = 0,
        BaseMotorDirectionCommand = 1,
        ShoulderMotorDirectionCommand = 2,
        ElbowMotorDirectionCommand = 3,
        ArmMotorCommand = 4,
        WristMotorCommand = 5,
        Wrist2MotorCommand = 6,
        GripperMotorCommand = 7,
        LeftMotorCommand = 8,
        RightMotorCommand = 9,
        LeftRearMotorCommand = 10,
        RightRearMotorCommand = 11,
        StopAllMotorsCommand = 12,
        BaseMotorPositionCommand = 13,
        ShoulderMotorPositionCommand = 14,
        ElbowMotorPositionCommand = 15,
        ArmMotorPositionCommand = 16,
        WristMotorPositionCommand = 17,
        GripperMotorPositionCommand = 18,
    }
}
