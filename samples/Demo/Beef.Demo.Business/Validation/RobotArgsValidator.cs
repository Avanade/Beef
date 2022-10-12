﻿using CoreEx.Wildcards;

namespace Beef.Demo.Business.Validation
{
    /// <summary>
    /// Represents a <see cref="RobotArgs"/> validator.
    /// </summary>
    public class RobotArgsValidator : Validator<RobotArgs>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonArgsValidator"/>.
        /// </summary>
        public RobotArgsValidator()
        {
            Property(x => x.ModelNo).Common(CommonValidators.Text).Wildcard(Wildcard.MultiBasic);
            Property(x => x.SerialNo).Common(CommonValidators.Text).Wildcard(Wildcard.MultiBasic);
            Property(x => x.PowerSources).AreValid();
        }
    }
}