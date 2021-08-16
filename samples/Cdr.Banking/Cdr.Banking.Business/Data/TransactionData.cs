using Beef;
using Beef.Data.Cosmos;
using Cdr.Banking.Common.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Cdr.Banking.Business.Data
{
    public partial class TransactionData
    {
        /// <summary>
        /// Wires up the extensions.
        /// </summary>
        partial void TransactionDataCtor()
        {
            _getTransactionsOnQuery = GetTransactionsOnQuery;
        }

        /// <summary>
        /// Perform the query filering for the GetTransactions.
        /// </summary>
        private IQueryable<Model.Transaction> GetTransactionsOnQuery(IQueryable<Model.Transaction> query, string? _, TransactionArgs? args, CosmosDbArgs? __)
        {
            if (args == null || args.IsInitial)
                return query.OrderByDescending(x => x.TransactionDateTime);

            var q = query.WhereWith(args.FromDate, x => x.TransactionDateTime >= args.FromDate);
            q = q.WhereWith(args.ToDate, x => x.TransactionDateTime <= args.ToDate);
            q = q.WhereWith(args.MinAmount, x => x.Amount >= args.MinAmount);
            q = q.WhereWith(args.MaxAmount, x => x.Amount <= args.MaxAmount);

            // The text filtering will perform a case-insensitive (based on uppercase) comparison on Description and Reference properties. 
            q = q.WhereWith(args.Text, x => x.Description!.ToUpper().Contains(args.Text!.ToUpper()) || x.Reference!.ToUpper().Contains(args.Text!.ToUpper()));

            // Order by TransactionDateTime in descending order.
            return q.OrderByDescending(x => x.TransactionDateTime);
        }
    }
}