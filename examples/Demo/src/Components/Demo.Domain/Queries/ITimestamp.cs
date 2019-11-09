using System;

namespace Demo.Domain.Queries
{
    public interface ITimestamp
    {
        DateTime CurrentDate { get; set; }
    }
}