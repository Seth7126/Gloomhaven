using System;
using UnityEngine;

public static class Ease
{
	public static readonly Easer Linear = (float t) => t;

	public static readonly Easer QuadIn = (float t) => t * t;

	public static readonly Easer QuadOut = (float t) => 1f - QuadIn(1f - t);

	public static readonly Easer QuadInOut = (float t) => (!(t <= 0.5f)) ? (QuadOut(t * 2f - 1f) / 2f + 0.5f) : (QuadIn(t * 2f) / 2f);

	public static readonly Easer CubeIn = (float t) => t * t * t;

	public static readonly Easer CubeOut = (float t) => 1f - CubeIn(1f - t);

	public static readonly Easer CubeInOut = (float t) => (!(t <= 0.5f)) ? (CubeOut(t * 2f - 1f) / 2f + 0.5f) : (CubeIn(t * 2f) / 2f);

	public static readonly Easer BackIn = (float t) => t * t * (2.70158f * t - 1.70158f);

	public static readonly Easer BackOut = (float t) => 1f - BackIn(1f - t);

	public static readonly Easer BackInOut = (float t) => (!(t <= 0.5f)) ? (BackOut(t * 2f - 1f) / 2f + 0.5f) : (BackIn(t * 2f) / 2f);

	public static readonly Easer ExpoIn = (float t) => Mathf.Pow(2f, 10f * (t - 1f));

	public static readonly Easer ExpoOut = (float t) => 1f - ExpoIn(t);

	public static readonly Easer ExpoInOut = (float t) => (!(t < 0.5f)) ? (ExpoOut(t * 2f) / 2f) : (ExpoIn(t * 2f) / 2f);

	public static readonly Easer SineIn = (float t) => 0f - Mathf.Cos(MathF.PI / 2f * t) + 1f;

	public static readonly Easer SineOut = (float t) => Mathf.Sin(MathF.PI / 2f * t);

	public static readonly Easer SineInOut = (float t) => (0f - Mathf.Cos(MathF.PI * t)) / 2f + 0.5f;

	public static readonly Easer ElasticIn = (float t) => 1f - ElasticOut(1f - t);

	public static readonly Easer ElasticOut = (float t) => Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - 0.075f) * (MathF.PI * 2f) / 0.3f) + 1f;

	public static readonly Easer ElasticInOut = (float t) => (!(t <= 0.5f)) ? (ElasticOut(t * 2f - 1f) / 2f + 0.5f) : (ElasticIn(t * 2f) / 2f);

	public static Easer FromType(EaseType type)
	{
		return type switch
		{
			EaseType.Linear => Linear, 
			EaseType.QuadIn => QuadIn, 
			EaseType.QuadOut => QuadOut, 
			EaseType.QuadInOut => QuadInOut, 
			EaseType.CubeIn => CubeIn, 
			EaseType.CubeOut => CubeOut, 
			EaseType.CubeInOut => CubeInOut, 
			EaseType.BackIn => BackIn, 
			EaseType.BackOut => BackOut, 
			EaseType.BackInOut => BackInOut, 
			EaseType.ExpoIn => ExpoIn, 
			EaseType.ExpoOut => ExpoOut, 
			EaseType.ExpoInOut => ExpoInOut, 
			EaseType.SineIn => SineIn, 
			EaseType.SineOut => SineOut, 
			EaseType.SineInOut => SineInOut, 
			EaseType.ElasticIn => ElasticIn, 
			EaseType.ElasticOut => ElasticOut, 
			EaseType.ElasticInOut => ElasticInOut, 
			_ => Linear, 
		};
	}
}
