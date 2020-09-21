namespace My.Hr.Business.Data
{
    public partial class EmployeeBaseData
    {
        public partial class EfMapper
        {
            partial void EfMapperCtor()
            {
                // Adds a mapper between TerminationDetail and EfModel.Employee; this is a special case as it is not a direct property to property mapping within same type.
                SrceProperty(s => s.Termination).SetMapper(TerminationDetailData.EfMapper.Default);
            }
        }
    }
}