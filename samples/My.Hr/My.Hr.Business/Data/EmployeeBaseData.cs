using My.Hr.Business.Entities;

namespace My.Hr.Business.Data
{
    public partial class EmployeeBaseData
    {
        public partial class EfMapperProfile
        {
            partial void EfMapperProfileCtor(AutoMapper.IMappingExpression<EmployeeBase, EfModel.Employee> s2d, AutoMapper.IMappingExpression<EfModel.Employee, EmployeeBase> d2s)
            {
                // Reshape the Termination-related columns into a TermaintionDetail type.
                d2s.ForPath(s => s.Termination!.Date, o => o.MapFrom(d => d.TerminationDate));
                d2s.ForPath(s => s.Termination!.ReasonSid, o => o.MapFrom(d => d.TerminationReasonCode));
            }
        }
    }
}