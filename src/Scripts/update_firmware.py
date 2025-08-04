import RPi.GPIO as GPIO
import time
import subprocess

# Pin definitions
BOOT_ENABLE_PIN = 23
RUN_CONTROL_PIN = 24

# Setup GPIO using BCM numbering
GPIO.setmode(GPIO.BCM)
GPIO.setup(BOOT_ENABLE_PIN, GPIO.OUT)
GPIO.setup(RUN_CONTROL_PIN, GPIO.OUT)

print("Starting firmware update process...")

try:
    # Bring RUN_CONTROL and BOOT_ENABLE down
    print("Setting RUN_CONTROL and BOOT_ENABLE to LOW.")
    GPIO.output(BOOT_ENABLE_PIN, GPIO.LOW)
    GPIO.output(RUN_CONTROL_PIN, GPIO.LOW)

    # Wait for one second
    time.sleep(1)

    print("Setting RUN_CONTROL to HIGH.")
    GPIO.output(RUN_CONTROL_PIN, GPIO.HIGH)

    # Wait a moment for the Pico to enter bootloader mode
    time.sleep(0.1)

    # Execute the picoboot3 command
    command = [
        "/home/pi/.local/bin/picoboot3",
        "-f", "/home/pi/robotcar/restserver/1.0.0.10/firmware/LowLevelController.bin",
        "-p", "/dev/serial0",
        "-a"
    ]
    print(f"Executing command: {' '.join(command)}")
    
    result = subprocess.run(command, capture_output=True, text=True)

    if result.returncode == 0:
        print("Firmware update successful!")
        print("stdout:", result.stdout)
    else:
        print("Firmware update failed.")
        print("stdout:", result.stdout)
        print("stderr:", result.stderr)

    print("Setting BOOT_ENABLE to HIGH.")
    GPIO.output(BOOT_ENABLE_PIN, GPIO.HIGH)

finally:
    # Clean up GPIO
    print("Cleaning up GPIO.")
    GPIO.cleanup()

