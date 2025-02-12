using System.ComponentModel.DataAnnotations;

namespace NewsProject.Models.VM
{
    public class EmployeeEditVM
    {
        public string Id { get; set; }
        public string FullName { get; set; }       
        public string EmailAddress { get; set; }
        public string UserRole { get; set; }
        public List<string> Roles { get; set; }
    }
}
