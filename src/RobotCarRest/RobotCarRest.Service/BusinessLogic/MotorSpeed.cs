using System;
using Paregov.RobotCar.Rest.Service.Models.LowLevel;

namespace Paregov.RobotCar.Rest.Service.BusinessLogic;

public class MotorSpeed : IMotorSpeed
{
    public (SingleMotorCommand left, SingleMotorCommand right) GetDcMotorSpeeds(int xValue, int yValue)
    {
        // Convert the 0-255 joystick readings to a -127 to 128 range.
        int xMapped = xValue - 128;
        int yMapped = yValue - 128;

        // This creates a small area around the center where the motors won't move.
        if (Math.Abs(xMapped) < 10) xMapped = 0;
        if (Math.Abs(yMapped) < 10) yMapped = 0;

        // Motor Speed Calculation.
        // Forward and backward movement.
        int forwardSpeed = xMapped;
        int turnSpeed = yMapped;

        // Calculate the speed for each motor.
        int leftSpeed = forwardSpeed + turnSpeed;
        int rightSpeed = forwardSpeed - turnSpeed;

        // Constrain the speeds to the -127 to 127 range.
        if (leftSpeed > 127) leftSpeed = 127;
        if (leftSpeed < -127) leftSpeed = -127;
        if (rightSpeed > 127) rightSpeed = 127;
        if (rightSpeed < -127) rightSpeed = -127;

        // Remap to 0-100 for motor output.
        int leftMotorSpeed = (leftSpeed * 100) / 127;
        int rightMotorSpeed = (rightSpeed * 100) / 127;

        var left = new SingleMotorCommand
        {
            Direction = leftMotorSpeed < 0 ? -1 : (leftMotorSpeed > 0 ? 1 : 0),
            Speed = Math.Abs(leftMotorSpeed)
        };

        var right = new SingleMotorCommand
        {
            Direction = rightMotorSpeed < 0 ? -1 : (rightMotorSpeed > 0 ? 1 : 0),
            Speed = Math.Abs(rightMotorSpeed)
        };

        return (left, right);
    }
}
