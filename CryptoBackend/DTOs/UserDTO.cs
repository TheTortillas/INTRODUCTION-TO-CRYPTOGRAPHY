using System.ComponentModel.DataAnnotations;

namespace CryptoBackend.DTOs
{
    public class UserDTO
    {
        public int ID { get; set; } = 0;
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty; 
        public string SecondLastname { get; set; } = string.Empty;     
        public string Email { get; set; } = string.Empty;  
        public string Password { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
    }
}
