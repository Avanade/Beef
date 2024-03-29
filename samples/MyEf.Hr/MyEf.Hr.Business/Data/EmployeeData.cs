﻿namespace MyEf.Hr.Business.Data;

public partial class EmployeeData
{
    partial void EmployeeDataCtor()
    {
        // Implement the GetByArgs OnQuery search/filtering logic.
        _getByArgsOnQuery = (q, args) =>
        {
            _ef.WithWildcard(args?.FirstName, w => q = q.Where(x => EF.Functions.Like(x.FirstName!, w)));
            _ef.WithWildcard(args?.LastName, w => q = q.Where(x => EF.Functions.Like(x.LastName!, w)));
            _ef.With(args?.Genders, g => q = q.Where(x => g.ToCodeList().Contains(x.GenderCode)));
            _ef.With(args?.StartFrom, f => q = q.Where(x => x.StartDate >= f));
            _ef.With(args?.StartTo, t => q = q.Where(x => x.StartDate <= t));

            if (args?.IsIncludeTerminated is null || !args.IsIncludeTerminated.Value)
                q = q.Where(x => x.TerminationDate == null);

            return q.IgnoreAutoIncludes().OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ThenBy(x => x.StartDate);
        };
    }

    /// <summary>
    /// Terminates an existing employee by updating the termination-related columns.
    /// </summary>
    /// <remarks>Need to pre-query the data to, 1) check they exist, 2) check they are still employed, 3) not prior to starting, and, then 4) update.</remarks>
    private Task<Result<Employee>> TerminateOnImplementationAsync(TerminationDetail value, Guid id)
        => Result.GoAsync(GetAsync(id))
            .When(e => e is null, _ => Result.NotFoundError())
            .When(e => e.Termination is not null, _ => Result.ValidationError("An Employee can not be terminated more than once."))
            .When(e => value.Date < e.StartDate, _ => Result.ValidationError("An Employee can not be terminated prior to their start date."))
            .Then(e => e.Termination = value)
            .ThenAsAsync(e => UpdateAsync(e));
}