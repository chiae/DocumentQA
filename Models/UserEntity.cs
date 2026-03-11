namespace DocumentQA.Models
{
    public class UserEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Email { get; set; } = default!;
        public byte[] PasswordHash { get; set; } = default!;
        public byte[] PasswordSalt { get; set; } = default!;
    }
}
