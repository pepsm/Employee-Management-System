﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeesManagementSystem.Models
{
    public class Employee : User
    {
        public Employee() { }
        public Employee(int id, string fullName, string email, string phoneNumber, string password, double wage)
            : base(id, fullName, email, phoneNumber, password, Role.Employee, wage)
        {

        }
    }
}
