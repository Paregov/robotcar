
import machine
import time
import ssd1306 # Import the SSD1306 driver

# --- Configuration ---
# I2C1 pins (Check your Pico W pinout diagram)
# Default I2C1 pins are:
# GP3 for SCL1
# GP2 for SDA1
I2C1_SCL_PIN = 3
I2C1_SDA_PIN = 2
I2C_BUS_ID = 1 # Using I2C1

# OLED Display dimensions
OLED_WIDTH = 128
OLED_HEIGHT = 32
OLED_I2C_ADDR = 0x3C # Default I2C address for many SSD1306 displays. Check your module!


class Display:
    def __init__(self):
        self.oled = self.init_display()

    # --- Initialization ---
    def init_display(self):
        """Initializes the I2C communication and the SSD1306 display."""
        print(f"Initializing I2C on bus {I2C_BUS_ID} with SCL={I2C1_SCL_PIN}, SDA={I2C1_SDA_PIN}")
        try:
            # Initialize I2C1.
            # For Pico W, machine.I2C takes id, scl, sda as arguments.
            i2c = machine.I2C(I2C_BUS_ID, scl=machine.Pin(I2C1_SCL_PIN), sda=machine.Pin(I2C1_SDA_PIN), freq=400000)
            print("I2C initialized.")

            # Scan for I2C devices to confirm the display is detected
            devices = i2c.scan()
            if not devices:
                print("No I2C devices found. Check wiring and I2C address.")
                return None
            else:
                print("I2C devices found:", [hex(device) for device in devices])
                if OLED_I2C_ADDR not in devices:
                    print(
                        f"OLED display not found at address {hex(OLED_I2C_ADDR)}. Found: {[hex(d) for d in devices]}")
                    print("Please check the OLED_I2C_ADDR variable in the script.")
                    return None

            # Initialize the SSD1306 OLED display
            oled = ssd1306.SSD1306_I2C(OLED_WIDTH, OLED_HEIGHT, i2c, addr=OLED_I2C_ADDR)
            oled.rotate(0)
            print("SSD1306 OLED initialized.")
            return oled
        except Exception as e:
            print(f"Error during display initialization: {e}")
            return None

    # --- Display Functions ---
    def display_text(self, text_lines, clear_display=True):
        """
        Displays multiple lines of text on the OLED.
        Each line in text_lines list will be displayed on a new row.
        Assumes standard 8x8 pixel characters.
        """
        if self.oled is None:
            print("OLED display not initialized. Cannot display text.")
            return

        if clear_display:
            self.oled.fill(0)  # Clear the display (0 = black, 1 = white)

        char_height = 8  # Height of a character in pixels
        for i, line in enumerate(text_lines):
            if i * char_height >= OLED_HEIGHT:
                print(f"Warning: Text line '{line}' exceeds display height.")
                break
            # oled.text(text, x, y, color=1)
            # color=1 means white, color=0 means black (to erase)
            self.oled.text(line, 0, i * char_height, 1)

        self.oled.show()  # Update the display with the new content
