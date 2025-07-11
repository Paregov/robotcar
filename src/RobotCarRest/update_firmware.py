# Copyright © Svetoslav Paregov. All rights reserved.

# Control the pins to:
# 1. Make the low level controller start in bootloader.
# 2. Reboot the low level controller.

import RPi.GPIO as GPIO
import time
import subprocess


def update_uart():
    # -f /home/paregov/firmware/LowLevelController.bin -p /dev/serial0 -a
    try:
        command = [
            "picoboot3",
            "-f", "/home/paregov/robotcar/firmware/LowLevelController.bin",
            "-p", "/dev/serial0",
            "-a"
        ]

        result = subprocess.run(command, capture_output=True, text=True, check=True)
        print("Stdout:", result.stdout)
        print("Stderr:", result.stderr)
        print("picoboot3 command executed successfully.")
    except subprocess.CalledProcessError as e:
        print(f"Error calling picoboot3: {e}")
        print(f"Stdout (on error): {e.stdout}")
        print(f"Stderr (on error): {e.stderr}")
    except FileNotFoundError:
        print("Error: 'picoboot3' command not found. Make sure it's in your PATH.")


def update_spi():
    # -f /home/paregov/firmware/LowLevelController.bin -p /dev/serial0 -a
    try:
        command = [
            "picoboot3",
            "-i", "spi",
            "-f", "/home/paregov/robotcar/firmware/LowLevelController.bin",
            "-bus", "0",
            "--device", "0",
            "--baud", "10000000",
            "-a"
        ]

        result = subprocess.run(command, capture_output=True, text=True, check=True)
        print("Stdout:", result.stdout)
        print("Stderr:", result.stderr)
        print("picoboot3 command executed successfully.")
    except subprocess.CalledProcessError as e:
        print(f"Error calling picoboot3: {e}")
        print(f"Stdout (on error): {e.stdout}")
        print(f"Stderr (on error): {e.stderr}")
    except FileNotFoundError:
        print("Error: 'picoboot3' command not found. Make sure it's in your PATH.")


print("Starting update_firmware.py ...")
# Pin Definitions
BOOT_ENABLE_PIN = 23
RUN_CTRL_PIN = 24

GPIO.setmode(GPIO.BCM)

GPIO.setup(BOOT_ENABLE_PIN, GPIO.OUT)
GPIO.setup(RUN_CTRL_PIN, GPIO.OUT)

print("Set the boot enable pin.")
GPIO.output(BOOT_ENABLE_PIN, GPIO.LOW)

print("Reset low level controller.")
GPIO.output(RUN_CTRL_PIN, GPIO.LOW)
print("Wait for 1 second.")
time.sleep(1)
print("Start the controller again.")
GPIO.output(RUN_CTRL_PIN, GPIO.HIGH)

print("Start upload of the new firmware.")
update_uart()

GPIO.output(BOOT_ENABLE_PIN, GPIO.HIGH)

print("Reset pins to inputs.")
# Set back to input
GPIO.setup(BOOT_ENABLE_PIN, GPIO.IN)
GPIO.setup(RUN_CTRL_PIN, GPIO.IN)
