namespace TfsTimeSheet.Validations
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;

    public class TimeSheetTimeValidation : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                var dateTime = Convert.ToDateTime(value);
                if (dateTime.Minute == 0 ||
                    dateTime.Minute == 15 ||
                    dateTime.Minute == 30 ||
                    dateTime.Minute == 45)
                {
                    return ValidationResult.ValidResult;
                }

                return new ValidationResult(false, "Invalid time. Only 15 minute blocks are allowed.");
            }
            catch (Exception)
            {
                return new ValidationResult(false, "Invalid time.");
            }
        }
    }
}
