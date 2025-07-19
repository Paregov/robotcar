using Paregov.RobotCar.Rest.Service.Models.LowLevel;

namespace Paregov.RobotCar.Rest.Service.BusinessLogic.Interfaces;

public interface IMotorSpeed
{
    (DirectionAndSpeedMotorCommand left, DirectionAndSpeedMotorCommand right) GetDcMotorSpeeds(int xValue, int yValue);
}
