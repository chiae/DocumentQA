namespace DocumentQA.Models
{
    /// <summary>
    /// Represents an authenticated user within the system, including their
    /// unique identifier, email address, and securely stored password
    /// credentials.
    ///
    /// The password is never stored in plain text. Instead, a salted hash
    /// is generated during registration and persisted for authentication
    /// purposes. This entity is used by the authentication pipeline to
    /// validate login attempts and associate uploaded documents with the
    /// correct user.
    /// </summary>
    public class UserEntity
    {
        /// <summary>
        /// Unique identifier for the user.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The user's email address, used as their login credential.
        /// </summary>
        public string Email { get; set; } = default!;

        /// <summary>
        /// The hashed password generated using a secure hashing algorithm.
        /// </summary>
        public byte[] PasswordHash { get; set; } = default!;

        /// <summary>
        /// The cryptographic salt used when hashing the user's password.
        /// </summary>
        public byte[] PasswordSalt { get; set; } = default!;
    }
}