// Copyright © Svetoslav Paregov. All rights reserved.

using Paregov.RobotCar.Rest.Service.BusinessLogic;

namespace RobotCarRest.UnitTests;

[TestClass]
public sealed class MotorSpeedTests
{
    private readonly MotorSpeed _motorSpeed = new();

    [TestMethod]
    [DataRow(128, 128, 0, 0, 0, 0, "Center")]
    [DataRow(137, 128, 0, 0, 0, 0, "Center - Deadzone X high")]
    [DataRow(119, 128, 0, 0, 0, 0, "Center - Deadzone X low")]
    [DataRow(128, 137, 0, 0, 0, 0, "Center - Deadzone Y high")]
    [DataRow(128, 119, 0, 0, 0, 0, "Center - Deadzone Y low")]
    [DataRow(255, 128, 1, 100, 1, 100, "Forward")]
    [DataRow(0, 128, -1, 100, -1, 100, "Backward")]
    [DataRow(128, 0, -1, 100, 1, 100, "Left")]
    [DataRow(128, 255, 1, 100, -1, 100, "Right")]
    [DataRow(255, 255, 1, 100, 0, 0, "Forward-Right")]
    [DataRow(255, 0, 0, 0, 1, 100, "Forward-Left")]
    [DataRow(0, 255, 0, 0, -1, 100, "Backward-Right")]
    [DataRow(0, 0, -1, 100, 0, 0, "Backward-Left")]
    [DataRow(141, 128, 1, 10, 1, 10, "Forward 10%")]
    [DataRow(192, 128, 1, 50, 1, 50, "Forward 50%")]
    [DataRow(64, 128, -1, 50, -1, 50, "Backward 50%")]
    [DataRow(128, 192, 1, 50, -1, 50, "Right 50%")]
    [DataRow(128, 64, -1, 50, 1, 50, "Left 50%")]
    public void GetCorrectDcMotorSpeeds(
        int xValue,
        int yValue,
        int expectedLeftDirection,
        int expectedLeftSpeed,
        int expectedRightDirection,
        int expectedRightSpeed,
        string caseName)
    {
        // Act
        var (left, right) = _motorSpeed.GetDcMotorSpeeds(xValue, yValue);

        // Assert
        Assert.AreEqual(expectedLeftDirection, left.Direction, $"Left Direction failed for case: {caseName}");
        Assert.AreEqual(expectedLeftSpeed, left.Speed, $"Left Speed failed for case: {caseName}");
        Assert.AreEqual(expectedRightDirection, right.Direction, $"Right Direction failed for case: {caseName}");
        Assert.AreEqual(expectedRightSpeed, right.Speed, $"Right Speed failed for case: {caseName}");
    }
}
