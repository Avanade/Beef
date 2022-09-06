namespace My.Hr.Business.Data
{
    public partial class PerformanceReviewData
    {
        partial void PerformanceReviewDataCtor()
        {
            _getByEmployeeIdOnQuery = (q, id) => q.Where(x => x.EmployeeId == id).OrderByDescending(x => x.Date);
        }
    }
}