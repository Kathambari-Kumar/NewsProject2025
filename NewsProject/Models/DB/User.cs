using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NewsProject.Models.DB
{
    public class User : IdentityUser
    {
        [StringLength(150)]
        [DisplayName("First Name")]
        public required string FirstName { get; set; }

        [StringLength(150)]
        [DisplayName("Last Name")]
        public required string LastName { get; set; }

        [StringLength(150)]
        public DateTime DOB { get; set; }

        [DisplayName("User Role")]
        public string UserRole {  get; set; } = string.Empty;
        public bool IsNewsLetter { get; set; }
        public List<Subscription>? Subscriptions { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public List<Order> OrderList { get; set; } = new List<Order>();

    }
}
