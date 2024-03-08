using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chomp.Util {
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyAdditionalInfoAttribute : Attribute {
        public readonly string text;
        public AssemblyAdditionalInfoAttribute(string text) => this.text = text;
    }
}
