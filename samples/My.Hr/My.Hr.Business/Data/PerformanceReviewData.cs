using System.Linq;

namespace My.Hr.Business.Data
{
    public partial class PerformanceReviewData
    {
        partial void PerformanceReviewDataCtor()
        {
            _getByEmployeeIdOnQuery = (q_, employeeId, _) => q_.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.Date);
        }
    }
}