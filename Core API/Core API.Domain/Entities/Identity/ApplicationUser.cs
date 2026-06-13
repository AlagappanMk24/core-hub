using Core_API.Domain.Common;
using Core_API.Domain.Entities.Companies;
using Core_API.Domain.Entities.Customers;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Domain.Entities.Identity
{
    /// <summary>
    /// Represents a user within the application, extending the base identity user with profile and business-specific details.
    /// </summary>
    public class ApplicationUser : AppIdentityUser
    {
        #region Profile Information

        /// <summary>
        /// Gets or sets the user's full legal name.
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Gets or sets the profile image URL.
        /// </summary>
        public string? ProfileImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the street address.
        /// </summary>
        public string? StreetAddress { get; set; }

        /// <summary>
        /// Gets or sets the city of residence.
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Gets or sets the state or province.
        /// </summary>
        public string? State { get; set; }

        /// <summary>
        /// Gets or sets the postal or zip code.
        /// </summary>
        public string? PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the ISO country code (e.g., "IN", "US").
        /// </summary>
        public string? CountryCode { get; set; }

        #endregion

        #region Application State & UI

        /// <summary>
        /// Gets or sets the user's current primary role. Not mapped to the database.
        /// </summary>
        [NotMapped]
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if this is the user's first time logging into the system.
        /// </summary>
        public bool IsFirstLogin { get; set; } = true;

        /// <summary>
        /// Gets or sets the timestamp of the last successful login.
        /// </summary>
        public DateTime? LastLogin { get; set; }

        #endregion

        #region Authentication & OTP Security

        /// <summary>
        /// Gets or sets the temporary identifier used during the OTP validation handshake.
        /// </summary>
        public string? OtpIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the hashed or plain-text OTP code for two-factor authentication.
        /// </summary>
        public string? TwoFactorCode { get; set; }

        /// <summary>
        /// Gets or sets the expiration time for the current TwoFactorCode.
        /// </summary>
        public DateTime? TwoFactorExpiry { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when OTP was last sent.
        /// </summary>
        public DateTime? OtpLastSentAt { get; set; }

        // ===== OTP Attempt Tracking =====

        /// <summary>
        /// Gets or sets the count of consecutive failed OTP validation attempts.
        /// </summary>
        public int FailedOtpAttempts { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total consecutive failed OTP attempts across sessions.
        /// </summary>
        public int ConsecutiveFailedAttempts { get; set; } = 0;

        /// <summary>
        /// Gets or sets the timestamp when the user will be unlocked.
        /// </summary>
        public DateTime? OtpLockoutEnd { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the last OTP validation attempt.
        /// </summary>
        public DateTime? LastOtpAttemptAt { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the last OTP attempt.
        /// </summary>
        public string? LastOtpAttemptIp { get; set; }

        /// <summary>
        /// Gets or sets whether OTP validation is currently locked.
        /// </summary>
        public bool IsOtpLocked => OtpLockoutEnd.HasValue && OtpLockoutEnd.Value > DateTime.UtcNow;

        #endregion

        #region Foreign Keys

        /// <summary>
        /// Gets or sets the foreign key for the associated Company.
        /// </summary>
        public int? CompanyId { get; set; }

        /// <summary>
        /// Gets or sets the foreign key for the associated Customer.
        /// </summary>
        public int? CustomerId { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Navigation property for the associated Company.
        /// </summary>
        [ForeignKey(nameof(CompanyId))]
        [ValidateNever]
        public virtual Company? Company { get; set; }

        /// <summary>
        /// Navigation property for the associated Customer.
        /// </summary>
        [ForeignKey(nameof(CustomerId))]
        [ValidateNever]
        public virtual Customer? Customer { get; set; }

        /// <summary>
        /// Collection of refresh tokens associated with this user session history.
        /// </summary>
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        #endregion
    }
}