using System;
using System.Collections.Generic;

namespace pract8_trpo.Models;

public partial class RefreshToken
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string TokenHash { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public virtual User User { get; set; } = null!;
}
