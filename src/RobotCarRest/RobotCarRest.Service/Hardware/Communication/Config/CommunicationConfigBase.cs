// Copyright © Svetoslav Paregov. All rights reserved.

namespace Paregov.RobotCar.Rest.Service.Hardware.Communication.Config
{
    /// <summary>
    /// Base class for hardware communication configurations.
    /// Provides common properties and validation for all communication protocols.
    /// </summary>
    public abstract class CommunicationConfigBase
    {
        /// <summary>
        /// Gets or sets the timeout for communication operations in milliseconds.
        /// </summary>
        public int TimeoutMs { get; set; } = 5000;

        /// <summary>
        /// Gets or sets whether the communication channel should be automatically retried on failure.
        /// </summary>
        public bool AutoRetry { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum number of retry attempts.
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Gets or sets the delay between retry attempts in milliseconds.
        /// </summary>
        public int RetryDelayMs { get; set; } = 100;

        /// <summary>
        /// Gets or sets whether debug logging is enabled for this communication channel.
        /// </summary>
        public bool EnableDebugLogging { get; set; } = false;

        /// <summary>
        /// Validates the configuration parameters.
        /// Derived classes should override this method to add protocol-specific validation.
        /// </summary>
        /// <returns>True if the configuration is valid; otherwise, false.</returns>
        public virtual bool IsValid()
        {
            return TimeoutMs > 0 && 
                   MaxRetryAttempts >= 0 && 
                   RetryDelayMs >= 0;
        }

        /// <summary>
        /// Gets a string representation of the configuration for logging purposes.
        /// </summary>
        /// <returns>A formatted string containing the configuration details.</returns>
        public virtual string GetConfigurationSummary()
        {
            return $"Timeout: {TimeoutMs}ms, AutoRetry: {AutoRetry}, MaxRetries: {MaxRetryAttempts}, RetryDelay: {RetryDelayMs}ms, Debug: {EnableDebugLogging}";
        }
    }
}
