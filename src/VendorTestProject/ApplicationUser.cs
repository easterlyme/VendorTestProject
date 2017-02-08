﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace VendorTestProject
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string MyCustomField { get; set; }
    }
}
