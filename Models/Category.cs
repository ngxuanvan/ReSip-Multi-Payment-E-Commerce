using System;
using System.Collections.Generic;

namespace ResipWeb.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Slug { get; set; }

    public string? Description { get; set; }

    public int? ParentId { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
