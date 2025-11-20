using System;

namespace UnityEngine.UI.Tweens;

internal class TweenEasingHandler
{
	public static float Apply(TweenEasing e, float t, float b, float c, float d)
	{
		switch (e)
		{
		case TweenEasing.Swing:
			return (0f - c) * (t /= d) * (t - 2f) + b;
		case TweenEasing.InQuad:
			return c * (t /= d) * t + b;
		case TweenEasing.OutQuad:
			return (0f - c) * (t /= d) * (t - 2f) + b;
		case TweenEasing.InOutQuad:
			if ((t /= d / 2f) < 1f)
			{
				return c / 2f * t * t + b;
			}
			return (0f - c) / 2f * ((t -= 1f) * (t - 2f) - 1f) + b;
		case TweenEasing.InCubic:
			return c * (t /= d) * t * t + b;
		case TweenEasing.OutCubic:
			return c * ((t = t / d - 1f) * t * t + 1f) + b;
		case TweenEasing.InOutCubic:
			if ((t /= d / 2f) < 1f)
			{
				return c / 2f * t * t * t + b;
			}
			return c / 2f * ((t -= 2f) * t * t + 2f) + b;
		case TweenEasing.InQuart:
			return c * (t /= d) * t * t * t + b;
		case TweenEasing.OutQuart:
			return (0f - c) * ((t = t / d - 1f) * t * t * t - 1f) + b;
		case TweenEasing.InOutQuart:
			if ((t /= d / 2f) < 1f)
			{
				return c / 2f * t * t * t * t + b;
			}
			return (0f - c) / 2f * ((t -= 2f) * t * t * t - 2f) + b;
		case TweenEasing.InQuint:
			return c * (t /= d) * t * t * t * t + b;
		case TweenEasing.OutQuint:
			return c * ((t = t / d - 1f) * t * t * t * t + 1f) + b;
		case TweenEasing.InOutQuint:
			if ((t /= d / 2f) < 1f)
			{
				return c / 2f * t * t * t * t * t + b;
			}
			return c / 2f * ((t -= 2f) * t * t * t * t + 2f) + b;
		case TweenEasing.InSine:
			return (0f - c) * Mathf.Cos(t / d * (MathF.PI / 2f)) + c + b;
		case TweenEasing.OutSine:
			return c * Mathf.Sin(t / d * (MathF.PI / 2f)) + b;
		case TweenEasing.InOutSine:
			return (0f - c) / 2f * (Mathf.Cos(MathF.PI * t / d) - 1f) + b;
		case TweenEasing.InExpo:
			if (t != 0f)
			{
				return c * Mathf.Pow(2f, 10f * (t / d - 1f)) + b;
			}
			return b;
		case TweenEasing.OutExpo:
			if (t != d)
			{
				return c * (0f - Mathf.Pow(2f, -10f * t / d) + 1f) + b;
			}
			return b + c;
		case TweenEasing.InOutExpo:
			if (t == 0f)
			{
				return b;
			}
			if (t == d)
			{
				return b + c;
			}
			if ((t /= d / 2f) < 1f)
			{
				return c / 2f * Mathf.Pow(2f, 10f * (t - 1f)) + b;
			}
			return c / 2f * (0f - Mathf.Pow(2f, -10f * (t -= 1f)) + 2f) + b;
		case TweenEasing.InCirc:
			return (0f - c) * (Mathf.Sqrt(1f - (t /= d) * t) - 1f) + b;
		case TweenEasing.OutCirc:
			return c * Mathf.Sqrt(1f - (t = t / d - 1f) * t) + b;
		case TweenEasing.InOutCirc:
			if ((t /= d / 2f) < 1f)
			{
				return (0f - c) / 2f * (Mathf.Sqrt(1f - t * t) - 1f) + b;
			}
			return c / 2f * (Mathf.Sqrt(1f - (t -= 2f) * t) + 1f) + b;
		case TweenEasing.InBack:
		{
			float num9 = 1.70158f;
			return c * (t /= d) * t * ((num9 + 1f) * t - num9) + b;
		}
		case TweenEasing.OutBack:
		{
			float num8 = 1.70158f;
			return c * ((t = t / d - 1f) * t * ((num8 + 1f) * t + num8) + 1f) + b;
		}
		case TweenEasing.InOutBack:
		{
			float num7 = 1.70158f;
			if ((t /= d / 2f) < 1f)
			{
				return c / 2f * (t * t * (((num7 *= 1.525f) + 1f) * t - num7)) + b;
			}
			return c / 2f * ((t -= 2f) * t * (((num7 *= 1.525f) + 1f) * t + num7) + 2f) + b;
		}
		case TweenEasing.InBounce:
			return c - Apply(TweenEasing.OutBounce, d - t, 0f, c, d) + b;
		case TweenEasing.OutBounce:
			if ((t /= d) < 0.36363637f)
			{
				return c * (7.5625f * t * t) + b;
			}
			if (t < 0.72727275f)
			{
				return c * (7.5625f * (t -= 0.54545456f) * t + 0.75f) + b;
			}
			if (t < 0.90909094f)
			{
				return c * (7.5625f * (t -= 0.8181818f) * t + 0.9375f) + b;
			}
			return c * (7.5625f * (t -= 21f / 22f) * t + 63f / 64f) + b;
		case TweenEasing.InOutBounce:
			if (t < d / 2f)
			{
				return Apply(TweenEasing.InBounce, t * 2f, 0f, c, d) * 0.5f + b;
			}
			return Apply(TweenEasing.OutBounce, t * 2f - d, 0f, c, d) * 0.5f + c * 0.5f + b;
		case TweenEasing.InElastic:
		{
			float num10 = 1.70158f;
			float num11 = 0f;
			float num12 = c;
			if (t == 0f)
			{
				return b;
			}
			if ((t /= d) == 1f)
			{
				return b + c;
			}
			if (num11 == 0f)
			{
				num11 = d * 0.3f;
			}
			if (num12 < Mathf.Abs(c))
			{
				num12 = c;
				num10 = num11 / 4f;
			}
			else
			{
				num10 = num11 / (MathF.PI * 2f) * Mathf.Asin(c / num12);
			}
			if (float.IsNaN(num10))
			{
				num10 = 0f;
			}
			return 0f - num12 * Mathf.Pow(2f, 10f * (t -= 1f)) * Mathf.Sin((t * d - num10) * (MathF.PI * 2f) / num11) + b;
		}
		case TweenEasing.OutElastic:
		{
			float num4 = 1.70158f;
			float num5 = 0f;
			float num6 = c;
			if (t == 0f)
			{
				return b;
			}
			if ((t /= d) == 1f)
			{
				return b + c;
			}
			if (num5 == 0f)
			{
				num5 = d * 0.3f;
			}
			if (num6 < Mathf.Abs(c))
			{
				num6 = c;
				num4 = num5 / 4f;
			}
			else
			{
				num4 = num5 / (MathF.PI * 2f) * Mathf.Asin(c / num6);
			}
			if (float.IsNaN(num4))
			{
				num4 = 0f;
			}
			return num6 * Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * d - num4) * (MathF.PI * 2f) / num5) + c + b;
		}
		case TweenEasing.InOutElastic:
		{
			float num = 1.70158f;
			float num2 = 0f;
			float num3 = c;
			if (t == 0f)
			{
				return b;
			}
			if ((t /= d / 2f) == 2f)
			{
				return b + c;
			}
			if (num2 == 0f)
			{
				num2 = d * 0.45000002f;
			}
			if (num3 < Mathf.Abs(c))
			{
				num3 = c;
				num = num2 / 4f;
			}
			else
			{
				num = num2 / (MathF.PI * 2f) * Mathf.Asin(c / num3);
			}
			if (float.IsNaN(num))
			{
				num = 0f;
			}
			if (t < 1f)
			{
				return -0.5f * (num3 * Mathf.Pow(2f, 10f * (t -= 1f)) * Mathf.Sin((t * d - num) * (MathF.PI * 2f) / num2)) + b;
			}
			return num3 * Mathf.Pow(2f, -10f * (t -= 1f)) * Mathf.Sin((t * d - num) * (MathF.PI * 2f) / num2) * 0.5f + c + b;
		}
		default:
			return c * t / d + b;
		}
	}
}
