using FluentValidation;

namespace Beef.Demo.Business.Validation
{
    public class RobotValidator : FluentValidation.AbstractValidator<Robot>
    {
        public RobotValidator()
        {
            RuleFor(x => x.ModelNo).NotEmpty().MaximumLength(50);
            RuleFor(x => x.SerialNo).NotEmpty().MaximumLength(50);
            RuleFor(x => x.EyeColor).IsValid();
            RuleFor(x => x.PowerSource).IsValid();
        }
    }
}