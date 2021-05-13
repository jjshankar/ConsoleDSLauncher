using System;
using System.Collections.Generic;
using System.Text;

namespace ECAR.DocuSign.Models
{
    /// <summary>
    /// Data model for reminder notifications for documents sent via ECAR.DocuSign.
    /// </summary>
    public class ReminderModel
    {
        /// <summary>
        /// Indicates whether a reminder is enabled.
        /// </summary>
        public bool ReminderEnabled { get; set; }

        /// <summary>
        /// First reminder will be sent after this many days.
        /// </summary>
        public int ReminderDelayDays { get; set; }

        /// <summary>
        /// Frequency of reminder emails after the first (in days).
        /// </summary>
        public int ReminderFrequencyDays { get; set; }
    }
}
