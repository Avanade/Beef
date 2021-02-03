using Beef;
using Beef.Data.Database;
using Microsoft.EntityFrameworkCore;
using My.Hr.Common.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace My.Hr.Business.Data
{
    public partial class EmployeeData
    {
        partial void EmployeeDataCtor()
        {
            // Implement the GetByArgs OnQuery search/filtering logic.
            _getByArgsOnQuery = (q, args, _) =>
            {
                _ef.WithWildcard(args?.FirstName, (w) => q = q.Where(x => EF.Functions.Like(x.FirstName, w)));
                _ef.WithWildcard(args?.LastName, (w) => q = q.Where(x => EF.Functions.Like(x.LastName, w)));
                _ef.With(args?.Genders, () => q = q.Where(x => args!.Genders!.ToCodeList().Contains(x.GenderCode)));
                _ef.With(args?.StartFrom, () => q = q.Where(x => x.StartDate >= args!.StartFrom));
                _ef.With(args?.StartTo, () => q = q.Where(x => x.StartDate <= args!.StartTo));

                if (args?.IsIncludeTerminated == null || !args.IsIncludeTerminated.Value)
                    q = q.Where(x => x.TerminationDate == null);

                return q.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ThenBy(x => x.StartDate);
            };
        }

        /// <summary>
        /// Executes the 'Get' stored procedure passing the identifier and returns the result.
        /// </summary>
        private Task<Employee?> GetOnImplementationAsync(Guid id) =>
            ExecuteStatement(_db.StoredProcedure("[Hr].[spEmployeeGet]").Param(DbMapper.Default.GetParamName(nameof(Employee.Id)), id));

        /// <summary>
        /// Executes the 'Create' stored procedure and returns the result.
        /// </summary>
        private Task<Employee> CreateOnImplementationAsync(Employee value) =>
            ExecuteStatement("[Hr].[spEmployeeCreate]", value, Beef.Mapper.OperationTypes.Create);

        /// <summary>
        /// Executes the 'Update' stored procedure and returns the result.
        /// </summary>
        private Task<Employee> UpdateOnImplementationAsync(Employee value) =>
            ExecuteStatement("[Hr].[spEmployeeUpdate]", value, Beef.Mapper.OperationTypes.Update);

        /// <summary>
        /// Executes the stored procedure, passing Employee parameters, and the EmergencyContacts as a table-valued parameter (TVP), the operation type to aid mapping, 
        /// and requests for the result to be reselected.
        /// </summary>
        private Task<Employee> ExecuteStatement(string storedProcedureName, Employee value, Beef.Mapper.OperationTypes operationType)
        {
            var sp = _db.StoredProcedure(storedProcedureName)
                        .Params(p => DbMapper.Default.MapToDb(value, p, operationType))
                        .TableValuedParam("@EmergencyContactList", EmergencyContactData.DbMapper.Default.CreateTableValuedParameter(value.EmergencyContacts!))
                        .ReselectRecordParam();

            return ExecuteStatement(sp)!;
        }

        /// <summary>
        /// Executes the underlying stored procedure and processes the result (used by Get, Create and Update).
        /// </summary>
        private async Task<Employee?> ExecuteStatement(DatabaseCommand db)
        {
            Employee? employee = null;

            // Execute the generated stored procedure, selecting (querying) two sets of data:
            // 1. The selected Employee (single row), the row is not mandatory, and stop (do not goto second set) where null. Use the underlying DbMapper to map between columns and .NET Type.
            // 2. Zero or more EmergencyContact rows. Use EmergencyContactData.DbMapper to map between columns and .NET Type. Update the Employee with result.
            await db.SelectQueryMultiSetAsync(
                new MultiSetSingleArgs<Employee>(DbMapper.Default, r => employee = r, isMandatory: false, stopOnNull: true),
                new MultiSetCollArgs<EmergencyContactCollection, EmergencyContact>(EmergencyContactData.DbMapper.Default, r => employee!.EmergencyContacts = r)).ConfigureAwait(false);

            return employee;
        }

        /// <summary>
        /// Terminates an existing employee by updating their termination columns.
        /// </summary>
        private async Task<Employee> TerminateOnImplementationAsync(TerminationDetail value, Guid id)
        {
            // Need to pre-query the data to, 1) check they exist, 2) check they are still employed, and 3) update.
            var curr = await GetOnImplementationAsync(id).ConfigureAwait(false);
            if (curr == null)
                throw new NotFoundException();

            if (curr.Termination != null)
                throw new ValidationException("An Employee can not be terminated more than once.");

            if (value.Date < curr.StartDate)
                throw new ValidationException("An Employee can not be terminated prior to their start date.");

            curr.Termination = value;
            return await UpdateOnImplementationAsync(curr).ConfigureAwait(false);
        }
    }
}