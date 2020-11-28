namespace Lab5_6.Models
{
    public class User
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string PhoneNumberEncrypted { get; set; }
        public string PhoneNumberNonce { get; set; }
        public string CreditCardEncrypted { get; set; }
        public string CreditCardNonce { get; set; }
    }
}
