/*
 * This file is automatically generated; any changes will be lost. 
 */

namespace Cdr.Banking.Business.Data;

/// <summary>
/// Provides the <see cref="CreditCardAccount"/> data access.
/// </summary>
public partial class CreditCardAccountData
{

    /// <summary>
    /// Provides the <see cref="CreditCardAccount"/> to Entity Framework <see cref="Model.CreditCardAccount"/> mapping.
    /// </summary>
    public partial class EntityToModelCosmosMapper : Mapper<CreditCardAccount, Model.CreditCardAccount>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityToModelCosmosMapper"/> class.
        /// </summary>
        public EntityToModelCosmosMapper()
        {
            Map((s, d) => d.MinPaymentAmount = s.MinPaymentAmount, OperationTypes.Any, s => s.MinPaymentAmount == default, d => d.MinPaymentAmount = default);
            Map((s, d) => d.PaymentDueAmount = s.PaymentDueAmount, OperationTypes.Any, s => s.PaymentDueAmount == default, d => d.PaymentDueAmount = default);
            Map((s, d) => d.PaymentCurrency = s.PaymentCurrency, OperationTypes.Any, s => s.PaymentCurrency == default, d => d.PaymentCurrency = default);
            Map((s, d) => d.PaymentDueDate = s.PaymentDueDate, OperationTypes.Any, s => s.PaymentDueDate == default, d => d.PaymentDueDate = default);
            EntityToModelCosmosMapperCtor();
        }

        partial void EntityToModelCosmosMapperCtor(); // Enables the constructor to be extended.
    }

    /// <summary>
    /// Provides the Entity Framework <see cref="Model.CreditCardAccount"/> to <see cref="CreditCardAccount"/> mapping.
    /// </summary>
    public partial class ModelToEntityCosmosMapper : Mapper<Model.CreditCardAccount, CreditCardAccount>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelToEntityCosmosMapper"/> class.
        /// </summary>
        public ModelToEntityCosmosMapper()
        {
            Map((s, d) => d.MinPaymentAmount = (decimal)s.MinPaymentAmount!, OperationTypes.Any, s => s.MinPaymentAmount == default, d => d.MinPaymentAmount = default);
            Map((s, d) => d.PaymentDueAmount = (decimal)s.PaymentDueAmount!, OperationTypes.Any, s => s.PaymentDueAmount == default, d => d.PaymentDueAmount = default);
            Map((s, d) => d.PaymentCurrency = (string?)s.PaymentCurrency!, OperationTypes.Any, s => s.PaymentCurrency == default, d => d.PaymentCurrency = default);
            Map((s, d) => d.PaymentDueDate = (DateTime)s.PaymentDueDate!, OperationTypes.Any, s => s.PaymentDueDate == default, d => d.PaymentDueDate = default);
            ModelToEntityCosmosMapperCtor();
        }

        partial void ModelToEntityCosmosMapperCtor(); // Enables the constructor to be extended.
    }
}