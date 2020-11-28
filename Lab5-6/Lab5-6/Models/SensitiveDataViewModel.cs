using System.ComponentModel.DataAnnotations;

namespace Lab5_6.Models
{
    public class SensitiveDataViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "PhoneNumber")]
        public string PhoneNumber { get; set; }

        [DataType(DataType.CreditCard)]
        [Display(Name = "CreditCard")]
        public string CreditCard { get; set; }
    }
}
