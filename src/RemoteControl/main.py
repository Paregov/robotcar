
import machine
import time
import urequests
import network
import socket
import json

from display import Display
from adc import ADC

#import mip
#mip.install("urequests")

# REST API Endpoint
API_ENDPOINT = "http://192.168.200.54:5000/api/v1/remotecontrol/joysticks"  # Replace with your API endpoint

# Wi-Fi Credentials
WIFI_SSID = ""      # Replace with your Wi-Fi SSID
WIFI_PASSWORD = ""  # Replace with your Wi-Fi password


def connect_wifi():
    """Connects to the Wi-Fi network."""
    wlan = network.WLAN(network.STA_IF)
    wlan.active(True)
    if not wlan.isconnected():
        print('Connecting to Wi-Fi...')
        wlan.connect(WIFI_SSID, WIFI_PASSWORD)
        while not wlan.isconnected():
            time.sleep(1)
            print('.')
    print('Wi-Fi connected!')
    ip = wlan.ifconfig()[0]
    print(f'IP Address: {ip}')
    return wlan


def send_data_to_api(readings: list[int]):
    """Sends the ADC readings to the REST API."""
    try:
        headers = {'Content-Type': 'application/json'}
        data_object = player_data = {
            "Joysticks": readings
        }
        json_data = json.dumps(data_object)
        response = urequests.post(API_ENDPOINT, headers=headers, data=json_data)
        if response.status_code == 200:
            print("Data sent successfully!")
        else:
            print(f"Failed to send data. Status code: {response.status_code}")
            try:
                print(f"Response content: {response.text}")
            except Exception as e:
                print(f"Could not decode response content: {e}.")
        response.close()
    except OSError as e:
        print(f"Network error: {e}")
    except Exception as e:
        print(f"Error sending data: {e}")


if __name__ == "__main__":
    # Connect to Wi-Fi
    wlan = connect_wifi()
    display = Display()
    adc = ADC()

    while True:
        # Read all ADS7830 channels
        adc_values = adc.read_all_channels()
        print("ADC Readings:", adc_values)

        line1 = f"0: {adc_values[0]:03}    1:{adc_values[1]:03}"
        line2 = f"2: {adc_values[2]:03}    3:{adc_values[3]:03}"
        line3 = f"4: {adc_values[4]:03}    5:{adc_values[5]:03}"
        line4 = f"6: {adc_values[6]:03}    7:{adc_values[7]:03}"

        display.display_text(
            [
                line1,
                line2,
                line3,
                line4
            ],
            True
        )

        # Let's send commands only if we are outside the idle state of the joysticks.
        need_to_send = False

        for reading in adc_values:
            if reading < 118 or reading > 137:
                need_to_send = True
                break

        if need_to_send:
            # Send data to the REST API
            if wlan.isconnected():
                send_data_to_api(adc_values)
            else:
                print("Not connected to Wi-Fi. Skipping data sending.")

        time.sleep_ms(50)
