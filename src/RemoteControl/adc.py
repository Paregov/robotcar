# Complete code for Raspberry Pi Pico W to read ADS7830 and send data to a REST API
# Requires the urequests library to be installed on the Pico W.
# You can install it using mip:


import machine
import time

# --- Configuration ---
# ADS7830 I2C Address
ADS7830_ADDRESS = 0x48  # Default address, check datasheet if different

# I2C Pins
I2C_SDA_PIN = machine.Pin(4)  # GP4
I2C_SCL_PIN = machine.Pin(5)  # GP5

# --- ADS7830 Register Commands ---
# Single-ended input selection (bits 6-4)
# 000: CH0, 001: CH1, 010: CH2, 011: CH3, 100: CH4, 101: CH5, 110: CH6, 111: CH7
# Power-down mode (bits 3-1) - 000: Normal operation
# Start bit (bit 7) - Always 1

base_command = 0b10000111
address_ch0 = 0b00000000
address_ch1 = 0b01000000
address_ch2 = 0b00010000
address_ch3 = 0b01010000
address_ch4 = 0b00100000
address_ch5 = 0b01100000
address_ch6 = 0b00110000
address_ch7 = 0b01110000

ADS7830_COMMANDS = [
    base_command | address_ch0,  # CH0, Normal, Start
    base_command | address_ch1,  # CH1, Normal, Start
    base_command | address_ch2,  # CH2, Normal, Start
    base_command | address_ch3,  # CH3, Normal, Start
    base_command | address_ch4,  # CH4, Normal, Start
    base_command | address_ch5,  # CH5, Normal, Start
    base_command | address_ch6,  # CH6, Normal, Start
    base_command | address_ch7,  # CH7, Normal, Start
]

class ADC:
    def __init__(self):
        # --- Initialize I2C ---
        self.i2c = machine.I2C(0, sda=I2C_SDA_PIN, scl=I2C_SCL_PIN, freq=100000)

    def read_ads7830(self, channel):
        """Reads a single channel from the ADS7830."""
        if 0 <= channel <= 7:
            command = ADS7830_COMMANDS[channel]
            self.i2c.writeto(ADS7830_ADDRESS, bytes([command]))
            time.sleep_us(100)  # Allow conversion time
            data = self.i2c.readfrom(ADS7830_ADDRESS, 2)
            # The first byte contains status bits (ignored here), the second byte
            # contains the 8-bit ADC value.
            return data[1]
        else:
            print("Invalid ADS7830 channel")
            return None

    def read_all_channels_as_dictionary(self):
        """Reads all channels of the ADS7830."""
        results = {}
        for i in range(8):
            value = self.read_ads7830(i)
            if value is not None:
                results[f"CH{i}"] = value

        return results

    def read_all_channels(self):
        """Reads all channels of the ADS7830."""
        results = []
        for i in range(8):
            value = self.read_ads7830(i)
            if value is not None:
                results.append(value)
            else:
                results.append(-1)

        return results
