# RobotCar Development Environment Setup

This guide explains how to set up your development environment for the RobotCar project, including ROS 2 Jazzy and Raspberry Pi Pico 2 (RP2350) development tools.

## Prerequisites

- Ubuntu 22.04 LTS or compatible Linux distribution
- Internet connection for downloading packages
- Sudo privileges for system package installation

## Method 1: Automated Setup with Ansible (Recommended)

### Installing Ansible

Ansible is an automation tool that will handle the entire setup process for you.

1. **Update your package list:**

   ```bash
   sudo apt update
   ```

2. **Install Ansible:**

   ```bash
   sudo apt install ansible -y
   ```

3. **Verify the installation:**

   ```bash
   ansible --version
   ```

### Running the Setup Playbook

Once Ansible is installed, you can use the provided playbook to automatically set up your development environment:

1. **Navigate to the setup directory:**

   ```bash
   cd setup/ansible
   ```

2. **Run the Ansible playbook:**

   ```bash
   ansible-playbook setup_ros2_pico2_dev.yml
   ```

   This command will:
   - Install ROS 2 Jazzy Desktop Full
   - Install Raspberry Pi Pico SDK with RP2350 support
   - Install micro-ROS build system
   - Configure all necessary environment variables
   - Install development tools and dependencies

3. **Reload your shell environment:**

   ```bash
   source ~/.bashrc
   ```

### What the Playbook Installs

The automated setup includes:

- **System Prerequisites:**
  - Build tools (gcc, cmake, git, etc.)
  - Python 3 development environment
  - Package management tools

- **ROS 2 Jazzy:**
  - Desktop Full installation
  - Development tools
  - Automatic environment sourcing

- **Raspberry Pi Pico SDK:**
  - ARM cross-compilation toolchain
  - Pico SDK with RP2350 (Pico 2) support
  - Environment variables configuration

- **micro-ROS:**
  - Build system for embedded ROS applications
  - Firmware workspace for RP2350 target
  - Integration with ROS 2 Jazzy

## Method 2: Manual Setup

If you prefer to install components manually or need to troubleshoot the automated setup, refer to the individual installation guides for:

- [ROS 2 Jazzy Installation](https://docs.ros.org/en/jazzy/Installation.html)
- [Raspberry Pi Pico SDK](https://github.com/raspberrypi/pico-sdk)
- [micro-ROS Setup](https://micro.ros.org/docs/tutorials/core/first_application_linux/)

## Verification

After the setup is complete, verify your installation:

1. **Check ROS 2:**

   ```bash
   ros2 --help
   ```

2. **Check Pico SDK:**

   ```bash
   echo $PICO_SDK_PATH
   ```

3. **Check micro-ROS workspace:**

   ```bash
   ls ~/microros_ws
   ```

## Troubleshooting

- **Permission errors:** Ensure you have sudo privileges
- **Network issues:** Check your internet connection and proxy settings
- **Environment variables:** Restart your terminal or run `source ~/.bashrc`
- **Package conflicts:** Update your system packages before running the playbook
- **ROS 2 package not found:** If you get "No package matching 'ros-jazzy-*' is available", the repository architecture might be incorrect. Run:

  ```bash
  sudo sed -i 's/x86_64/amd64/g' /etc/apt/sources.list.d/ros2.list
  sudo apt update
  ```

## Next Steps

After completing the setup:

1. Build the LowLevelController firmware for the Pico 2
2. Set up the RobotCarRest API service
3. Configure the RemoteControl interface
4. Test the hardware communication protocols

For project-specific build instructions, refer to the documentation in each component's directory.
