# robotcar

The project is aiming to quickly bring to life a 6 DOF (including the gripper) robotic arm. The initial plan was to use ROS2, but when started digging into that and wrote part of the code, desided to switch to implementation without ROS2. I will go back to the original plan, but at this point need this ASAP. My toddler is interested in the robotic arm and I want to surprise him.

## Requirements

- Control over REST API
  I need to be able to control it from the phone if needed with a small app. 

- Individual servo control

- Support for trajectories is optional

## Design

A Raspberry Pi Pico 2 for the low level hardware controller. The onboard PWM channels will be used to control the servo positions. This will give me a precise positioning. 

For REST API server Raspberry Pi Zero 2W will be used. Both boards will communicate over UART on 115,200 baud rate.

### Low level controller

A picoboot3 will be used, so update of the firmware can be done over the air (OTA).

[GitHub - IndoorCorgi/picoboot3: Custom bootloader that allows firmware updates to Raspberry Pi Pico via UART/I2C/SPI.](https://github.com/IndoorCorgi/picoboot3)

The code here will be in C++.

To build: 

Open the Pico - Developer PowerShell.

cd to the project

```powershell
cd build
cmake ..
ninja
```

### REST API server

The Raspberry Pi Zero 2W should be powerful enough to handle simple REST API server. The implementation will be in C# (.NET8). 

It will perform also the OTA update of the low level controller firmware.

These links are to test the installation. Once it is done will leave the working one.

[Install .NET 8 on Raspberry pi · GitHub](https://gist.github.com/ramonsmits/b15d97965bcfacc19920be2e84b49c4e)

This is more promissing

[Install and use Microsoft Dot NET 8 with the Raspberry Pi - Pete Codes](https://www.petecodes.co.uk/install-and-use-microsoft-dot-net-8-with-the-raspberry-pi/)

To install .NET 8 use this command in Raspberry Pi Zero 2W

```bash
wget -O - https://raw.githubusercontent.com/pjgpetecodes/dotnet8pi/main/install.sh | sudo bash
```

Call:

```bash
sudo raspi-config
```

Go to "Interface Options"

Disable console logging over serial.

Enable hardware serial.

Reboot.

Enable the SPI and I2C interfaces. 

```bash
sudo raspi-config
```

Go to "Interfaces Options"

Go to "SPI"

"Would you like the SPI interface to be enabled?" - Select "Yes" 

Do the same for the I2C.

All files for the project will be in the **~/robotcar**

Install picoboot3 updater.

```bash
pip3 install picoboot3 --break-system-packages
```

Step 1: Create Your Python Script
First, create your Python script. For this example, let's make a simple script that blinks an LED connected to GPIO 17 and writes the current time to a log file every 10 seconds.

Save this script in your home directory, for example, at /home/pi/myscript.py. You can find the code for myscript.py in the code block below this guide.

Important: Make your script executable using the chmod command in the terminal:

```bash
chmod +x /home/pi/myscript.py
```

Step 2: Copy the robotcar.system file

```bash
sudo cp robotcar.service /etc/systemd/system/robotcar.service
```

Step 3: Enable and Start the Service
Now you need to tell systemd to recognize and enable your new service.

Reload the systemd daemon to make it aware of the new service file:

```bash
sudo systemctl daemon-reload
```

Enable your service so it starts automatically on every boot:

```bash
sudo systemctl enable robotcar.service
```

Start the service immediately to test it without rebooting:

```bash
sudo systemctl start robotcar.service
```

Step 4: Check the Status of Your Service
You can check if your service is running correctly at any time.

```bash
sudo systemctl status robotcar.service
```

To see the output from your script (anything it prints), you can use journalctl:

```bash
journalctl -u myservice.service -f
```

The -f flag will "follow" the log, showing new output in real-time.

## Environment setup

I am working on Windows and the setup is for this envoronment.

1. Install Raspberry Pi Pico SDK - [Raspberry Pi Pico Windows Installer - Raspberry Pi](https://www.raspberrypi.com/news/raspberry-pi-pico-windows-installer/)

2. Check this article on the VS Code pico extension and how to get started - [Get started with Raspberry Pi Pico-series and VS Code - Raspberry Pi](https://www.raspberrypi.com/news/get-started-with-raspberry-pi-pico-series-and-vs-code/)

3. Install Visual Studio 2022 Community Edition
