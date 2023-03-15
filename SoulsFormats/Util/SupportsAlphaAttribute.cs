using System;

namespace SoulsFormats.Util {
    /// <summary>
    /// Indicates whether the alpha component of a Color is used.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class SupportsAlphaAttribute : Attribute {
        /// <summary>
        /// If true, alpha is used; if false, alpha is ignored.
        /// </summary>
        public bool Supports { get; }

        /// <summary>
        /// Creates an attribute with the given value.
        /// </summary>
        public SupportsAlphaAttribute(bool supports) => this.Supports = supports;
    }
}
