using NewsProject.Models.DB;

namespace NewsProject.Models.VM
{
    public class EmployeeListVM
    {
        public string Id { get; set; } = string.Empty;
        public string Fullname { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
    }
}
