namespace MyEf.Hr.Business.Data
{
    public partial class EmployeeData
    {
        partial void EmployeeDataCtor()
        {
            // Implement the GetByArgs OnQuery search/filtering logic.
            _getByArgsOnQuery = (q, args) =>
            {
                _ef.WithWildcard(args?.FirstName, (w) => q = q.Where(x => EF.Functions.Like(x.FirstName!, w)));
                _ef.WithWildcard(args?.LastName, (w) => q = q.Where(x => EF.Functions.Like(x.LastName!, w)));
                _ef.With(args?.Genders, () => q = q.Where(x => args!.Genders!.ToCodeList().Contains(x.GenderCode)));
                _ef.With(args?.StartFrom, () => q = q.Where(x => x.StartDate >= args!.StartFrom));
                _ef.With(args?.StartTo, () => q = q.Where(x => x.StartDate <= args!.StartTo));

                if (args?.IsIncludeTerminated == null || !args.IsIncludeTerminated.Value)
                    q = q.Where(x => x.TerminationDate == null);

                return q.IgnoreAutoIncludes().OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ThenBy(x => x.StartDate);
            };
        }

        /// <summary>
        /// Terminates an existing employee by updating their termination columns.
        /// </summary>
        private Task<Result<Employee>> TerminateOnImplementationAsync(TerminationDetail value, Guid id)
        {
            // Need to pre-query the data to, 1) check they exist, 2) check they are still employed, and 3) update.
            return Result.GoAsync(GetAsync(id)).ThenAsAsync(async curr =>
            {
                if (curr == null)
                    return Result.NotFoundError();

                if (curr.Termination != null)
                    return Result.ValidationError("An Employee can not be terminated more than once.");

                if (value.Date < curr.StartDate)
                    return Result.ValidationError("An Employee can not be terminated prior to their start date.");

                curr.Termination = value;
                return await UpdateAsync(curr).ConfigureAwait(false);
            });
        }
    }
}