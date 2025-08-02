// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using System.Device.I2c;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Paregov.RobotCar.Rest.Service.Hardware.Communication.Config;

namespace Paregov.RobotCar.Rest.Service.Hardware.Display
{
    /// <summary>
    /// SSD1306 OLED Display driver for 128x32 displays over I2C.
    /// Provides basic text and pixel drawing capabilities using optimized character patterns.
    /// Uses .NET I2cDevice directly for communication.
    /// </summary>
    public class Ssd1306Display : IDisposable
    {
        private readonly ILogger<Ssd1306Display> _logger;
        private readonly Ssd1306Options _options;
        private readonly object _lock = new();
        private I2cDevice? _i2cDevice;

        // SSD1306 Commands
        private const byte SSD1306_SETCONTRAST = 0x81;
        private const byte SSD1306_DISPLAYALLON_RESUME = 0xA4;
        private const byte SSD1306_DISPLAYALLON = 0xA5;
        private const byte SSD1306_NORMALDISPLAY = 0xA6;
        private const byte SSD1306_INVERTDISPLAY = 0xA7;
        private const byte SSD1306_DISPLAYOFF = 0xAE;
        private const byte SSD1306_DISPLAYON = 0xAF;
        private const byte SSD1306_SETDISPLAYOFFSET = 0xD3;
        private const byte SSD1306_SETCOMPINS = 0xDA;
        private const byte SSD1306_SETVCOMDETECT = 0xDB;
        private const byte SSD1306_SETDISPLAYCLOCKDIV = 0xD5;
        private const byte SSD1306_SETPRECHARGE = 0xD9;
        private const byte SSD1306_SETMULTIPLEX = 0xA8;
        private const byte SSD1306_SETLOWCOLUMN = 0x00;
        private const byte SSD1306_SETHIGHCOLUMN = 0x10;
        private const byte SSD1306_SETSTARTLINE = 0x40;
        private const byte SSD1306_MEMORYMODE = 0x20;
        private const byte SSD1306_COLUMNADDR = 0x21;
        private const byte SSD1306_PAGEADDR = 0x22;
        private const byte SSD1306_COMSCANINC = 0xC0;
        private const byte SSD1306_COMSCANDEC = 0xC8;
        private const byte SSD1306_SEGREMAP = 0xA0;
        private const byte SSD1306_CHARGEPUMP = 0x8D;

        // Character generation commands
        private const byte SSD1306_SET_PAGE_START = 0xB0;
        private const byte SSD1306_SET_COLUMN_LOW = 0x00;
        private const byte SSD1306_SET_COLUMN_HIGH = 0x10;

        // Display dimensions for 128x32
        public const int Width = 128;
        public const int Height = 32;
        public const int Pages = Height / 8;

        // Character dimensions for built-in font
        private const int CharWidth = 6;  // 5 pixels + 1 spacing
        private const int CharHeight = 8; // 8 pixels height
        private const int CharsPerLine = Width / CharWidth;
        private const int LinesPerDisplay = Height / CharHeight;

        // Display buffer
        private readonly byte[] _buffer = new byte[Width * Pages];
        private bool _isInitialized;

        /// <summary>
        /// Initializes a new instance of the Ssd1306Display class.
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="options">SSD1306 configuration options</param>
        public Ssd1306Display(ILogger<Ssd1306Display> logger, IOptions<Ssd1306Options> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        /// <summary>
        /// Initializes the SSD1306 display with default settings for 128x32 resolution.
        /// </summary>
        /// <returns>True if initialization was successful; otherwise, false</returns>
        public bool Initialize()
        {
            try
            {
                lock (_lock)
                {
                    _logger.LogInformation("Initializing SSD1306 128x32 display with I2C bus {BusId}, address 0x{DeviceAddress:X2}...", 
                        _options.BusId, _options.DeviceAddress);

                    // Create I2C connection settings
                    var connectionSettings = new I2cConnectionSettings(_options.BusId, _options.DeviceAddress);
                    _i2cDevice = I2cDevice.Create(connectionSettings);

                    // Initialization sequence for 128x32 SSD1306
                    var initCommands = new byte[]
                    {
                        SSD1306_DISPLAYOFF,                    // 0xAE
                        SSD1306_SETDISPLAYCLOCKDIV, 0x80,      // 0xD5, 0x80
                        SSD1306_SETMULTIPLEX, 0x1F,           // 0xA8, 0x1F (31 for 32 pixels height)
                        SSD1306_SETDISPLAYOFFSET, 0x00,       // 0xD3, 0x00
                        SSD1306_SETSTARTLINE | 0x00,          // 0x40
                        SSD1306_CHARGEPUMP, 0x14,             // 0x8D, 0x14
                        SSD1306_MEMORYMODE, 0x00,             // 0x20, 0x00 - Horizontal addressing mode
                        SSD1306_SEGREMAP | 0x01,              // 0xA1
                        SSD1306_COMSCANDEC,                   // 0xC8
                        SSD1306_SETCOMPINS, 0x02,             // 0xDA, 0x02 (for 32 pixel height)
                        SSD1306_SETCONTRAST, 0x8F,            // 0x81, 0x8F (different contrast for 32px)
                        SSD1306_SETPRECHARGE, 0xF1,           // 0xD9, 0xF1
                        SSD1306_SETVCOMDETECT, 0x40,          // 0xDB, 0x40
                        SSD1306_DISPLAYALLON_RESUME,          // 0xA4
                        SSD1306_NORMALDISPLAY,                // 0xA6
                        SSD1306_DISPLAYON                     // 0xAF
                    };

                    foreach (var command in initCommands)
                    {
                        if (!SendCommand(command))
                        {
                            _logger.LogError("Failed to send initialization command: 0x{Command:X2}", command);
                            return false;
                        }
                    }

                    _isInitialized = true;
                    _logger.LogInformation("SSD1306 128x32 display initialized successfully");

                    // Clear the display
                    Clear();
                    Update();

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize SSD1306 display");
                return false;
            }
        }

        /// <summary>
        /// Clears the display buffer.
        /// </summary>
        public void Clear()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
        }

        /// <summary>
        /// Updates the display with the current buffer content.
        /// </summary>
        /// <returns>True if update was successful; otherwise, false</returns>
        public bool Update()
        {
            if (!_isInitialized || _i2cDevice == null)
            {
                _logger.LogWarning("Display not initialized. Call Initialize() first.");
                return false;
            }

            try
            {
                lock (_lock)
                {
                    // Set column address range
                    SendCommand(SSD1306_COLUMNADDR);
                    SendCommand(0);         // Column start address
                    SendCommand(Width - 1); // Column end address

                    // Set page address range
                    SendCommand(SSD1306_PAGEADDR);
                    SendCommand(0);         // Page start address
                    SendCommand(Pages - 1); // Page end address

                    // Send display data
                    return SendData(_buffer);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update display");
                return false;
            }
        }

        /// <summary>
        /// Draws text at the specified position using a simple character rendering approach.
        /// Since SSD1306 doesn't have a built-in character ROM like some other displays,
        /// we use a minimal bitmap font approach optimized for the display.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="text">Text to draw</param>
        public void DrawText(int x, int y, string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            int currentX = x;
            foreach (char c in text)
            {
                if (currentX >= Width) break;
                DrawChar(currentX, y, c);
                currentX += CharWidth;
            }
        }

        /// <summary>
        /// Draws a single character at the specified position using a minimal bitmap approach.
        /// This method uses a simplified character representation optimized for the SSD1306.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="c">Character to draw</param>
        public void DrawChar(int x, int y, char c)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;

            // Get simple character pattern from display's perspective
            var pattern = GetCharacterPattern(c);

            for (int col = 0; col < 5; col++)
            {
                if (x + col >= Width) break;

                byte columnData = pattern[col];
                for (int row = 0; row < 8; row++)
                {
                    if (y + row >= Height)
                    {
                        break;
                    }

                    if ((columnData & (1 << row)) != 0)
                    {
                        SetPixel(x + col, y + row, true);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the character pattern for display rendering.
        /// Uses a minimal set optimized for the SSD1306 display characteristics.
        /// </summary>
        /// <param name="c">Character to get pattern for</param>
        /// <returns>5-byte array representing the character pattern</returns>
        private byte[] GetCharacterPattern(char c)
        {
            // Simple patterns for common characters - optimized for SSD1306
            return c switch
            {
                ' ' => new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 },
                '!' => new byte[] { 0x00, 0x00, 0x5F, 0x00, 0x00 },
                '"' => new byte[] { 0x00, 0x07, 0x00, 0x07, 0x00 },
                '#' => new byte[] { 0x14, 0x7F, 0x14, 0x7F, 0x14 },
                '$' => new byte[] { 0x24, 0x2A, 0x7F, 0x2A, 0x12 },
                '%' => new byte[] { 0x23, 0x13, 0x08, 0x64, 0x62 },
                '&' => new byte[] { 0x36, 0x49, 0x55, 0x22, 0x50 },
                '\'' => new byte[] { 0x00, 0x05, 0x03, 0x00, 0x00 },
                '(' => new byte[] { 0x00, 0x1C, 0x22, 0x41, 0x00 },
                ')' => new byte[] { 0x00, 0x41, 0x22, 0x1C, 0x00 },
                '*' => new byte[] { 0x08, 0x2A, 0x1C, 0x2A, 0x08 },
                '+' => new byte[] { 0x08, 0x08, 0x3E, 0x08, 0x08 },
                ',' => new byte[] { 0x00, 0x50, 0x30, 0x00, 0x00 },
                '-' => new byte[] { 0x08, 0x08, 0x08, 0x08, 0x08 },
                '.' => new byte[] { 0x00, 0x60, 0x60, 0x00, 0x00 },
                '/' => new byte[] { 0x20, 0x10, 0x08, 0x04, 0x02 },
                '0' => new byte[] { 0x3E, 0x51, 0x49, 0x45, 0x3E },
                '1' => new byte[] { 0x00, 0x42, 0x7F, 0x40, 0x00 },
                '2' => new byte[] { 0x42, 0x61, 0x51, 0x49, 0x46 },
                '3' => new byte[] { 0x21, 0x41, 0x45, 0x4B, 0x31 },
                '4' => new byte[] { 0x18, 0x14, 0x12, 0x7F, 0x10 },
                '5' => new byte[] { 0x27, 0x45, 0x45, 0x45, 0x39 },
                '6' => new byte[] { 0x3C, 0x4A, 0x49, 0x49, 0x30 },
                '7' => new byte[] { 0x01, 0x71, 0x09, 0x05, 0x03 },
                '8' => new byte[] { 0x36, 0x49, 0x49, 0x49, 0x36 },
                '9' => new byte[] { 0x06, 0x49, 0x49, 0x29, 0x1E },
                ':' => new byte[] { 0x00, 0x36, 0x36, 0x00, 0x00 },
                ';' => new byte[] { 0x00, 0x56, 0x36, 0x00, 0x00 },
                '<' => new byte[] { 0x00, 0x08, 0x14, 0x22, 0x41 },
                '=' => new byte[] { 0x14, 0x14, 0x14, 0x14, 0x14 },
                '>' => new byte[] { 0x41, 0x22, 0x14, 0x08, 0x00 },
                '?' => new byte[] { 0x02, 0x01, 0x51, 0x09, 0x06 },
                '@' => new byte[] { 0x32, 0x49, 0x79, 0x41, 0x3E },
                'A' => new byte[] { 0x7E, 0x11, 0x11, 0x11, 0x7E },
                'B' => new byte[] { 0x7F, 0x49, 0x49, 0x49, 0x36 },
                'C' => new byte[] { 0x3E, 0x41, 0x41, 0x41, 0x22 },
                'D' => new byte[] { 0x7F, 0x41, 0x41, 0x22, 0x1C },
                'E' => new byte[] { 0x7F, 0x49, 0x49, 0x49, 0x41 },
                'F' => new byte[] { 0x7F, 0x09, 0x09, 0x01, 0x01 },
                'G' => new byte[] { 0x3E, 0x41, 0x41, 0x51, 0x32 },
                'H' => new byte[] { 0x7F, 0x08, 0x08, 0x08, 0x7F },
                'I' => new byte[] { 0x00, 0x41, 0x7F, 0x41, 0x00 },
                'J' => new byte[] { 0x20, 0x40, 0x41, 0x3F, 0x01 },
                'K' => new byte[] { 0x7F, 0x08, 0x14, 0x22, 0x41 },
                'L' => new byte[] { 0x7F, 0x40, 0x40, 0x40, 0x40 },
                'M' => new byte[] { 0x7F, 0x02, 0x04, 0x02, 0x7F },
                'N' => new byte[] { 0x7F, 0x04, 0x08, 0x10, 0x7F },
                'O' => new byte[] { 0x3E, 0x41, 0x41, 0x41, 0x3E },
                'P' => new byte[] { 0x7F, 0x09, 0x09, 0x09, 0x06 },
                'Q' => new byte[] { 0x3E, 0x41, 0x51, 0x21, 0x5E },
                'R' => new byte[] { 0x7F, 0x09, 0x19, 0x29, 0x46 },
                'S' => new byte[] { 0x46, 0x49, 0x49, 0x49, 0x31 },
                'T' => new byte[] { 0x01, 0x01, 0x7F, 0x01, 0x01 },
                'U' => new byte[] { 0x3F, 0x40, 0x40, 0x40, 0x3F },
                'V' => new byte[] { 0x1F, 0x20, 0x40, 0x20, 0x1F },
                'W' => new byte[] { 0x7F, 0x20, 0x18, 0x20, 0x7F },
                'X' => new byte[] { 0x63, 0x14, 0x08, 0x14, 0x63 },
                'Y' => new byte[] { 0x03, 0x04, 0x78, 0x04, 0x03 },
                'Z' => new byte[] { 0x61, 0x51, 0x49, 0x45, 0x43 },
                '[' => new byte[] { 0x00, 0x00, 0x7F, 0x41, 0x41 },
                '\\' => new byte[] { 0x02, 0x04, 0x08, 0x10, 0x20 },
                ']' => new byte[] { 0x41, 0x41, 0x7F, 0x00, 0x00 },
                '^' => new byte[] { 0x04, 0x02, 0x01, 0x02, 0x04 },
                '_' => new byte[] { 0x40, 0x40, 0x40, 0x40, 0x40 },
                '`' => new byte[] { 0x00, 0x01, 0x02, 0x04, 0x00 },
                'a' => new byte[] { 0x20, 0x54, 0x54, 0x54, 0x78 },
                'b' => new byte[] { 0x7F, 0x48, 0x44, 0x44, 0x38 },
                'c' => new byte[] { 0x38, 0x44, 0x44, 0x44, 0x20 },
                'd' => new byte[] { 0x38, 0x44, 0x44, 0x48, 0x7F },
                'e' => new byte[] { 0x38, 0x54, 0x54, 0x54, 0x18 },
                'f' => new byte[] { 0x08, 0x7E, 0x09, 0x01, 0x02 },
                'g' => new byte[] { 0x08, 0x14, 0x54, 0x54, 0x3C },
                'h' => new byte[] { 0x7F, 0x08, 0x04, 0x04, 0x78 },
                'i' => new byte[] { 0x00, 0x44, 0x7D, 0x40, 0x00 },
                'j' => new byte[] { 0x20, 0x40, 0x44, 0x3D, 0x00 },
                'k' => new byte[] { 0x00, 0x7F, 0x10, 0x28, 0x44 },
                'l' => new byte[] { 0x00, 0x41, 0x7F, 0x40, 0x00 },
                'm' => new byte[] { 0x7C, 0x04, 0x18, 0x04, 0x78 },
                'n' => new byte[] { 0x7C, 0x08, 0x04, 0x04, 0x78 },
                'o' => new byte[] { 0x38, 0x44, 0x44, 0x44, 0x38 },
                'p' => new byte[] { 0x7C, 0x14, 0x14, 0x14, 0x08 },
                'q' => new byte[] { 0x08, 0x14, 0x14, 0x18, 0x7C },
                'r' => new byte[] { 0x7C, 0x08, 0x04, 0x04, 0x08 },
                's' => new byte[] { 0x48, 0x54, 0x54, 0x54, 0x20 },
                't' => new byte[] { 0x04, 0x3F, 0x44, 0x40, 0x20 },
                'u' => new byte[] { 0x3C, 0x40, 0x40, 0x20, 0x7C },
                'v' => new byte[] { 0x1C, 0x20, 0x40, 0x20, 0x1C },
                'w' => new byte[] { 0x3C, 0x40, 0x30, 0x40, 0x3C },
                'x' => new byte[] { 0x44, 0x28, 0x10, 0x28, 0x44 },
                'y' => new byte[] { 0x0C, 0x50, 0x50, 0x50, 0x3C },
                'z' => new byte[] { 0x44, 0x64, 0x54, 0x4C, 0x44 },
                '{' => new byte[] { 0x00, 0x08, 0x36, 0x41, 0x00 },
                '|' => new byte[] { 0x00, 0x00, 0x7F, 0x00, 0x00 },
                '}' => new byte[] { 0x00, 0x41, 0x36, 0x08, 0x00 },
                '~' => new byte[] { 0x08, 0x04, 0x08, 0x10, 0x08 },
                _ => new byte[] { 0x7F, 0x41, 0x41, 0x41, 0x7F } // Unknown character - box
            };
        }

        /// <summary>
        /// Sets a pixel at the specified coordinates.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="on">True to turn pixel on, false to turn off</param>
        public void SetPixel(int x, int y, bool on)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;

            int index = x + (y / 8) * Width;
            byte bit = (byte)(1 << (y % 8));

            if (on)
                _buffer[index] |= bit;
            else
                _buffer[index] &= (byte)~bit;
        }

        /// <summary>
        /// Draws a horizontal line.
        /// </summary>
        /// <param name="x">Starting X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="length">Length of the line</param>
        public void DrawHorizontalLine(int x, int y, int length)
        {
            for (int i = 0; i < length; i++)
            {
                SetPixel(x + i, y, true);
            }
        }

        /// <summary>
        /// Draws a vertical line.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Starting Y coordinate</param>
        /// <param name="length">Length of the line</param>
        public void DrawVerticalLine(int x, int y, int length)
        {
            for (int i = 0; i < length; i++)
            {
                SetPixel(x, y + i, true);
            }
        }

        /// <summary>
        /// Draws a rectangle outline.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public void DrawRectangle(int x, int y, int width, int height)
        {
            DrawHorizontalLine(x, y, width);
            DrawHorizontalLine(x, y + height - 1, width);
            DrawVerticalLine(x, y, height);
            DrawVerticalLine(x + width - 1, y, height);
        }

        /// <summary>
        /// Sends a command to the SSD1306.
        /// </summary>
        /// <param name="command">Command byte</param>
        /// <returns>True if successful; otherwise, false</returns>
        private bool SendCommand(byte command)
        {
            if (_i2cDevice == null)
            {
                _logger.LogError("I2C device not initialized");
                return false;
            }

            try
            {
                var data = new byte[] { 0x00, command }; // 0x00 = command mode
                _i2cDevice.Write(data);

                if (_options.EnableDebugLogging)
                {
                    _logger.LogDebug("Sent I2C command: 0x{Command:X2}", command);
                }

                // Add delay if configured
                if (_options.OperationDelayMs > 0)
                {
                    Thread.Sleep(_options.OperationDelayMs);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send command: 0x{Command:X2}", command);
                return false;
            }
        }

        /// <summary>
        /// Sends data to the SSD1306.
        /// </summary>
        /// <param name="data">Data bytes</param>
        /// <returns>True if successful; otherwise, false</returns>
        private bool SendData(byte[] data)
        {
            if (_i2cDevice == null)
            {
                _logger.LogError("I2C device not initialized");
                return false;
            }

            try
            {
                // Send data in chunks to avoid I2C buffer limits
                const int chunkSize = 32;
                for (int i = 0; i < data.Length; i += chunkSize)
                {
                    int remaining = Math.Min(chunkSize, data.Length - i);
                    var chunk = new byte[remaining + 1];
                    chunk[0] = 0x40; // 0x40 = data mode
                    Array.Copy(data, i, chunk, 1, remaining);

                    _i2cDevice.Write(chunk);

                    if (_options.EnableDebugLogging && i % 128 == 0) // Log every few chunks
                    {
                        _logger.LogDebug("Sent I2C data chunk at offset {Offset}, size {Size}", i, remaining);
                    }

                    // Small delay between chunks to avoid overwhelming the display
                    Thread.Sleep(1);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send data");
                return false;
            }
        }

        /// <summary>
        /// Gets whether the display is initialized.
        /// </summary>
        public bool IsInitialized => _isInitialized && _i2cDevice != null;

        /// <summary>
        /// Disposes the display resources.
        /// </summary>
        public void Dispose()
        {
            if (_isInitialized && _i2cDevice != null)
            {
                try
                {
                    Clear();
                    Update();
                    SendCommand(SSD1306_DISPLAYOFF);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error during display cleanup");
                }
                finally
                {
                    _i2cDevice?.Dispose();
                    _i2cDevice = null;
                    _isInitialized = false;
                }
            }
            GC.SuppressFinalize(this);
        }
    }
}
