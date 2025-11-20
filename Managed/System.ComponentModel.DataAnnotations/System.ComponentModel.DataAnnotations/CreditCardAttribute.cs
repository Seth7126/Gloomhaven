using System.Linq;

namespace System.ComponentModel.DataAnnotations;

/// <summary>Specifies that a data field value is a credit card number.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class CreditCardAttribute : DataTypeAttribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.CreditCardAttribute" /> class.</summary>
	public CreditCardAttribute()
		: base(DataType.CreditCard)
	{
		base.DefaultErrorMessage = "The {0} field is not a valid credit card number.";
	}

	/// <summary>Determines whether the specified credit card number is valid. </summary>
	/// <returns>true if the credit card number is valid; otherwise, false.</returns>
	/// <param name="value">The value to validate.</param>
	public override bool IsValid(object value)
	{
		if (value == null)
		{
			return true;
		}
		if (!(value is string text))
		{
			return false;
		}
		string text2 = text.Replace("-", "");
		text2 = text2.Replace(" ", "");
		int num = 0;
		bool flag = false;
		foreach (char item in text2.Reverse())
		{
			if (item < '0' || item > '9')
			{
				return false;
			}
			int num2 = (item - 48) * ((!flag) ? 1 : 2);
			flag = !flag;
			while (num2 > 0)
			{
				num += num2 % 10;
				num2 /= 10;
			}
		}
		return num % 10 == 0;
	}
}
