using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WellEquipment.Models
{
    public class Role
    {
        public string Name { get; set; }
        public List<Account> Accounts { get; set; }
        public Role()
        {
            Accounts = new List<Account>();
        }
    }
}
