using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Settings
{
    public sealed class ApiSettings
    {
        public const string SectionName = "ApiSettings";
        public string BaseUrl { get; init; } = default!;
        public string FrontendUrl { get; set; } = string.Empty;
    }
}
