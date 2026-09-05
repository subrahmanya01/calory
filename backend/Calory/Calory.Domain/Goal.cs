using System;
using System.Collections.Generic;
using System.Text;

namespace Calory.Domain
{

    // Not finalized, but this is a simple representation of a goal entity in the domain layer.
    internal class Goal
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Calories { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
