﻿using System.Globalization;
using System.Windows.Controls;

namespace SlideShow;

public class ValueRangeRule : ValidationRule
{
    public double Min { get; set; }
    public double Max { get; set; }

    public ValueRangeRule()
    {
    }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is string text)
        {
            var style = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;

            if (!double.TryParse(text, style, CultureInfo.CurrentCulture, out double num))
            {
                return new ValidationResult(false, "Illegal characters");
            }

            if ((num < Min) || (num > Max) || string.IsNullOrEmpty(text))
            {
                return new ValidationResult(false,
                    $"Valid range: {Min} to {Max}");
            }
        }
        else
        {
            return new ValidationResult(false, "Invalid input type");
        }
        return ValidationResult.ValidResult;
    }
}