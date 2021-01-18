using Beef.Demo.Common.Entities;
using Beef.Validation;

namespace Beef.Demo.Business.Validation
{
    /// <summary>
    /// Represents a <see cref="Robot"/> validator.
    /// </summary>
    public class RobotValidator : Validator<Robot>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RobotValidator"/>.
        /// </summary>
        public RobotValidator()
        {
            Property(x => x.ModelNo).Mandatory().Common(CommonValidators.Text);
            Property(x => x.SerialNo).Mandatory().Common(CommonValidators.Text);
            Property(x => x.EyeColor).IsValid();
            Property(x => x.PowerSource).IsValid();
        }
    }
}
