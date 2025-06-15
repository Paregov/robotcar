# Generated Cmake Pico project file

cmake_minimum_required(VERSION 3.13)

set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

# Initialise pico_sdk from installed location
# (note this can come from environment, CMake cache etc)

# == DO NOT EDIT THE FOLLOWING LINES for the Raspberry Pi Pico VS Code Extension to work ==
if(WIN32)
    set(USERHOME $ENV{USERPROFILE})
else()
    set(USERHOME $ENV{HOME})
endif()
set(sdkVersion 2.1.1)
#set(toolchainVersion RISCV_RPI_2_0_0_5)
set(picotoolVersion 2.1.1)
set(picoVscode ${USERHOME}/.pico-sdk/cmake/pico-vscode.cmake)
if (EXISTS ${picoVscode})
    include(${picoVscode})
endif()
# ====================================================================================
set(PICO_BOARD pico2 CACHE STRING "Board type")

# Pull in Raspberry Pi Pico SDK (must be before project)
include(pico_sdk_import.cmake)

project(LowLevelController C CXX ASM)

# Initialise the Raspberry Pi Pico SDK
pico_sdk_init()

# Add executable. Default name is the project name, version 0.1

add_executable(LowLevelController
    commands_protocol.cpp
    dc_motors_control.cpp
    logger.cpp
    LowLevelController.cpp
    pico_native_pwm.cpp
    servo_control.cpp
    spi_transport.cpp
    uart_transport.cpp)

pico_set_program_name(LowLevelController "LowLevelController")
pico_set_program_version(LowLevelController "0.1")
pico_set_linker_script(${CMAKE_PROJECT_NAME} ${CMAKE_SOURCE_DIR}/memmap_default_rp2350.ld)

# Modify the below lines to enable/disable output over UART/USB
pico_enable_stdio_uart(LowLevelController 1)
pico_enable_stdio_usb(LowLevelController 0)

# Add the standard library to the build
target_link_libraries(LowLevelController
        pico_stdlib
        hardware_pwm
        hardware_dma
        hardware_irq
        hardware_spi
        pico_binary_info)

# Add the standard include files to the build
target_include_directories(LowLevelController PRIVATE
        ${CMAKE_CURRENT_LIST_DIR}
)

pico_add_extra_outputs(LowLevelController)

