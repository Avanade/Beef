// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;

namespace Beef.Entities
{
    /// <summary>
    /// Represents a Change log class.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ChangeLog : EntityBase
    {
        private DateTime? _createdDate;
        private string? _createdBy;
        private DateTime? _updatedDate;
        private string? _updatedBy;

        /// <summary>
        /// Gets or sets the Created <see cref="DateTime"/>.
        /// </summary>
        [JsonProperty("createdDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? CreatedDate
        {
            get => _createdDate; 
            set => SetValue(ref _createdDate, value, false, DateTimeTransform.DateTimeLocal, nameof(CreatedDate)); 
        }

        /// <summary>
        /// Gets or sets the Created username.
        /// </summary>
        [JsonProperty("createdBy", NullValueHandling = NullValueHandling.Ignore)]
        public string? CreatedBy
        {
            get => _createdBy;
            set => SetValue(ref _createdBy!, value, false, StringTrim.End, StringTransform.EmptyToNull, nameof(CreatedBy));
        }

        /// <summary>
        /// Gets or sets the Updated <see cref="DateTime"/>.
        /// </summary>
        [JsonProperty("updatedDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? UpdatedDate
        {
            get => _updatedDate;
            set => SetValue(ref _updatedDate, value, false, DateTimeTransform.DateTimeLocal, nameof(UpdatedDate));
        }

        /// <summary>
        /// Gets or sets the Updated username.
        /// </summary>
        [JsonProperty("updatedBy", NullValueHandling = NullValueHandling.Ignore)]
        public string? UpdatedBy
        {
            get => _updatedBy;
            set => SetValue(ref _updatedBy!, value, false, StringTrim.End, StringTransform.EmptyToNull, nameof(UpdatedBy));
        }

        #region ICopyFrom

        /// <summary>
        /// Performs a copy from another <see cref="ChangeLog"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="ChangeLog"/> to copy from.</param>
        public void CopyFrom(ChangeLog from)
        {
            var fval = ValidateCopyFromType<ChangeLog>(from);

            base.CopyFrom(fval);
            CreatedDate = fval.CreatedDate;
            CreatedBy = fval.CreatedBy;
            UpdatedDate = fval.UpdatedDate;
            UpdatedBy = fval.UpdatedBy;
        }

        #endregion

        #region ICloneable

        /// <summary>
        /// Creates a deep copy of the <see cref="ChangeLog"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="ChangeLog"/>.</returns>
        public override object Clone()
        {
            ChangeLog clone = new ChangeLog();
            clone.CopyFrom(this);
            return clone;
        }

        #endregion

        #region ICleanUp

        /// <summary>
        /// Performs a clean-up of the <see cref="ChangeLog"/> resetting property values as appropriate to ensure a basic level of data consistency.
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();
            CreatedDate = Cleaner.Clean(CreatedDate, DateTimeTransform.DateTimeLocal);
            CreatedBy = Cleaner.Clean(CreatedBy, StringTrim.End, StringTransform.EmptyToNull);
            UpdatedDate = Cleaner.Clean(UpdatedDate, DateTimeTransform.DateTimeLocal);
            UpdatedBy = Cleaner.Clean(UpdatedBy, StringTrim.End, StringTransform.EmptyToNull);
        }

        /// <summary>
        /// Indicates whether considered initial; i.e. all properties have their initial value.
        /// </summary>
        /// <returns><c>true</c> indicates is initial; otherwise, <c>false</c>.</returns>
        public override bool IsInitial
        {
            get
            {
                return Cleaner.IsInitial(CreatedDate)
                    && Cleaner.IsInitial(CreatedBy)
                    && Cleaner.IsInitial(UpdatedDate)
                    && Cleaner.IsInitial(UpdatedBy);
            }
        }

        #endregion
    }
}