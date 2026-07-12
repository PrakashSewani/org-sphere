using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1873:Pass argument expressions to logging method arguments correctly",
    Justification = "Log arguments are simple property accesses, not expensive evaluations")]
