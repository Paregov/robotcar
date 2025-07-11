using Paregov.RobotCar.Rest.Service.Models.LowLevel;

namespace Paregov.RobotCar.Rest.Service.BusinessLogic;

public interface IMotorSpeed
{
    (SingleMotorCommand left, SingleMotorCommand right) GetDcMotorSpeeds(int xValue, int yValue);
}
