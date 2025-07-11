
$uri = "http://192.168.200.85:5000/api/v1/remotecontrol/joysticks"


for ($i = 0; $i -lt 100; $i++)
{
    Write-Host "Sending request $i"
    
    # Create a random joystick state
    $joystickState = @{
        Joysticks = @( 128, 128, 128, 128, 128, 128, 128, 128 )
    }

    # Convert the state to JSON
    $jsonBody = $joystickState | ConvertTo-Json

    # Send the request
    try
    {
        $response = Invoke-RestMethod -Uri $uri -Method "POST" -Body $jsonBody -ContentType "application/json" -ErrorAction Stop
        Write-Host "Response: $response"
    }
    catch
    {
        Write-Error "Failed to send request: $_"
    }
}

