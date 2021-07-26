using System;
using System.Collections.Generic;
using System.Text;

namespace ECAR.DocuSign.Models
{
    /// <summary>
    /// Available types of DocuSign fields (tabs) for prefilling. Other field types are currently not supported.
    /// </summary>
    public enum Presets
    {
        /// <summary>
        /// A date field in the DocuSign document. Pass a formatted value (##/##/####).
        /// </summary>
        Date,

        /// <summary>
        /// A text field in the DocuSign document.
        /// </summary>
        Text,

        /// <summary>
        /// An SSN field in the DocuSign document.  Pass a formatted value (###-##-####).
        /// </summary>
        Ssn,

        /// <summary>
        /// A checkbox field in the DocuSign document.  Pass "true"/"false" as value.
        /// </summary>
        Checkbox
    }

    /// <summary>
    /// Data model for prefilling fields in a DocuSign document.
    /// </summary>
    public class DocPreset
    {
        /// <summary>
        /// The name of the field to set in the DocuSign document.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The value to set in the DocuSign document.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The type of field in the DocuSign document.
        /// </summary>
        public Presets Type { get; set; }

        /// <summary>
        /// Specifies whether the pre-filled field should be locked for user entry. Default: false (not locked).
        /// </summary>
        public bool Locked { get; set; } = false;
    }
}
