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
        #region PropertyNames

        /// <summary>
        /// Represents the <see cref="CreatedDate"/> property name.
        /// </summary>
        public const string Property_CreatedDate = "CreatedDate";

        /// <summary>
        /// Represents the <see cref="CreatedBy"/> property name.
        /// </summary>
        public const string Property_CreatedBy = "CreatedBy";

        /// <summary>
        /// Represents the <see cref="UpdatedDate"/> property name.
        /// </summary>
        public const string Property_UpdatedDate = "UpdatedDate";

        /// <summary>
        /// Represents the <see cref="UpdatedBy"/> property name.
        /// </summary>
        public const string Property_UpdatedBy = "UpdatedBy";

        #endregion

        #region Privates

        private DateTime? _createdDate;
        private string _createdBy;
        private DateTime? _updatedDate;
        private string _updatedBy;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeLog"/> class.
        /// </summary>
        public ChangeLog()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Created <see cref="DateTime"/>.
        /// </summary>
        [JsonProperty("createdDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? CreatedDate
        {
            get { return this._createdDate; }
            set { SetValue(ref this._createdDate, value, false, DateTimeTransform.DateTimeLocal, Property_CreatedDate); }
        }

        /// <summary>
        /// Gets or sets the Created username.
        /// </summary>
        [JsonProperty("createdBy", NullValueHandling = NullValueHandling.Ignore)]
        public string CreatedBy
        {
            get { return this._createdBy; }
            set { SetValue(ref this._createdBy, value, false, StringTrim.End, StringTransform.EmptyToNull, Property_CreatedBy); }
        }

        /// <summary>
        /// Gets or sets the Updated <see cref="DateTime"/>.
        /// </summary>
        [JsonProperty("updatedDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? UpdatedDate
        {
            get { return this._updatedDate; }
            set { SetValue(ref this._updatedDate, value, false, DateTimeTransform.DateTimeLocal, Property_UpdatedDate); }
        }

        /// <summary>
        /// Gets or sets the Updated username.
        /// </summary>
        [JsonProperty("updatedBy", NullValueHandling = NullValueHandling.Ignore)]
        public string UpdatedBy
        {
            get { return this._updatedBy; }
            set { SetValue(ref this._updatedBy, value, false, StringTrim.End, StringTransform.EmptyToNull, Property_UpdatedBy); }
        }

        #endregion

        #region ICopyFrom

        /// <summary>
        /// Performs a copy from another <see cref="ChangeLog"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="ChangeLog"/> to copy from.</param>
        public void CopyFrom(ChangeLog from)
        {
            var fval = ValidateCopyFromType<ChangeLog>(from);

            base.CopyFrom(fval);
            this.CreatedDate = fval.CreatedDate;
            this.CreatedBy = fval.CreatedBy;
            this.UpdatedDate = fval.UpdatedDate;
            this.UpdatedBy = fval.UpdatedBy;
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
            this.CreatedDate = Cleaner.Clean(this.CreatedDate, DateTimeTransform.DateTimeLocal);
            this.CreatedBy = Cleaner.Clean(this.CreatedBy, StringTrim.End, StringTransform.EmptyToNull);
            this.UpdatedDate = Cleaner.Clean(this.UpdatedDate, DateTimeTransform.DateTimeLocal);
            this.UpdatedBy = Cleaner.Clean(this.UpdatedBy, StringTrim.End, StringTransform.EmptyToNull);
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
