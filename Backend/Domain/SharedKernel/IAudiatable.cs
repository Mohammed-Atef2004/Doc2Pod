using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.SharedKernel
{
    public interface IAudiatable
    {
         DateTime CreatedAt { get; }
         DateTime? UpdatedAt { get; }
         string? CreatedBy { get; }
         string? UpdatedBy { get; }

        void SetCreated(string user);
        void SetUpdated(string UpdatedBy);
    }
}
