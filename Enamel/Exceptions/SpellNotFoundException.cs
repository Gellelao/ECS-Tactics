using System;

namespace Enamel.Exceptions;

public class SpellNotFoundException : Exception
{
    public SpellNotFoundException()
    {
    }

    public SpellNotFoundException(string message)
        : base(message)
    {
    }

    public SpellNotFoundException(string message, Exception inner)
        : base(message, inner)
    {
    }
}