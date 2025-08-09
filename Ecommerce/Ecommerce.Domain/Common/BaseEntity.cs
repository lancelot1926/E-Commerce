﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }

    // Auditing fields (we’ll fill these automatically in Infrastructure later)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}