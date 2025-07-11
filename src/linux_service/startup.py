# Copyright © Svetoslav Paregov. All rights reserved.

#!/usr/bin/env python3
import subprocess
import os
# install with "sudo apt install python3-packaging"
from packaging.version import Version

def start_rest_server(path: str):
    try:
        command = [
            "dotnet", path
        ]

        print(f"Starting server from path {path} ...")
        result = subprocess.run(command, capture_output=True, text=True, check=True)
        print("Stdout:", result.stdout)
        print("Stderr:", result.stderr)

    except subprocess.CalledProcessError as e:
        print(f"Error calling dotnet: {e}")
        print(f"Stdout (on error): {e.stdout}")
        print(f"Stderr (on error): {e.stderr}")
    except FileNotFoundError:
        print("Error: 'dotnet' command not found. Make sure it's in your PATH.")


def find_newest_version_dll(base_folder):
    """
    Searches for subfolders (one level deep) representing version numbers,
    finds the newest version, and appends 'NetworkController.dll' to its path.

    Args:
        base_folder (str): The path to the folder to search within.

    Returns:
        str or None: The full path to NetworkController.dll in the newest
                     version folder, or None if no versioned subfolders are found.
    """
    newest_version = None
    newest_version_path = None

    if not os.path.isdir(base_folder):
        print(f"Error: The provided path '{base_folder}' is not a valid directory.")
        return None

    for item in os.listdir(base_folder):
        item_path = os.path.join(base_folder, item)

        if os.path.isdir(item_path):
            try:
                # Attempt to parse the folder name as a version number
                current_version = Version(item)

                if newest_version is None or current_version > newest_version:
                    newest_version = current_version
                    newest_version_path = item_path
            except ValueError:
                # If the folder name is not a valid version, just skip it
                print(f"Skipping '{item}' as it does not appear to be a version number.")
                pass

    if newest_version_path:
        dll_path = os.path.join(newest_version_path, "Paregov.RobotCar.Rest.Service.dll")
        return dll_path
    else:
        return None

if __name__ == "__main__":
    # Define the base folder where you want to search
    base_folder_to_search = "/home/pi/robotcar/restserver"
    returned_path = find_newest_version_dll(base_folder_to_search)

    if returned_path:
        print(f"The newest version's Rest service path is: {returned_path}")
        start_rest_server(returned_path)
    else:
        print("No versioned subfolders found or an error occurred.")
