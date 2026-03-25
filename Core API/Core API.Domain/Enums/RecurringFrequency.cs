namespace Core_API.Domain.Enums
{
    /// <summary>
    /// Represents the recurrence frequency for a recurring invoice template.
    /// </summary>
    public enum RecurringFrequency
    {
        /// <summary>Invoice is generated every day.</summary>
        Daily = 1,

        /// <summary>Invoice is generated every week.</summary>
        Weekly = 2,

        /// <summary>Invoice is generated every two weeks.</summary>
        BiWeekly = 3,

        /// <summary>Invoice is generated every month.</summary>
        Monthly = 4,

        /// <summary>Invoice is generated every two months.</summary>
        BiMonthly = 5,

        /// <summary>Invoice is generated every three months.</summary>
        Quarterly = 6,

        /// <summary>Invoice is generated every six months.</summary>
        SemiAnnually = 7,

        /// <summary>Invoice is generated once per year.</summary>
        Annually = 8,

        /// <summary>Invoice is generated on a custom interval defined by <c>FrequencyInterval</c>.</summary>
        Custom = 9
    }
}