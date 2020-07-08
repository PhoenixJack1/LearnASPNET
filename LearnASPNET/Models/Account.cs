using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WellEquipment.Models
{
    public class Account
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public Role Role { get; set; }
        public string Name { get; set; }
        public Filters AccountFilters;
        public Account(string login, string password, string role, string name)
        {
            Login = login; Password = password; Name = name;
            Role userrole = SaveEquip.Roles.FirstOrDefault(r => r.Name == role);
            if (userrole != null)
                Role = userrole;
            AccountFilters = new Filters();
        }
    }
}
