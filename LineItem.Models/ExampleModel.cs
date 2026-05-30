using System;
using System.Collections.Generic;

namespace LineItem.Models;

public class ExampleModel
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime CreatedDate { get; set; }
}
