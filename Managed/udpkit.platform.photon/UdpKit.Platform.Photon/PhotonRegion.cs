using System;
using System.Collections.Generic;

namespace UdpKit.Platform.Photon;

public class PhotonRegion
{
	public enum Regions
	{
		BEST_REGION,
		ASIA,
		AU,
		CAE,
		CN,
		EU,
		IN,
		JP,
		RU,
		RUE,
		ZA,
		SA,
		KR,
		TR,
		US,
		USW
	}

	public static Dictionary<Regions, PhotonRegion> regions;

	public string Code { get; }

	public string Name { get; }

	public string City { get; }

	public Regions Region { get; }

	public static PhotonRegion GetRegion(string regionCode)
	{
		foreach (PhotonRegion value in regions.Values)
		{
			if (value.Code == regionCode.ToLower())
			{
				return value;
			}
		}
		throw new Exception("Region not found. Please select a valid Region");
	}

	public static PhotonRegion GetRegion(Regions targetRegion)
	{
		return regions[targetRegion];
	}

	public override string ToString()
	{
		return string.IsNullOrEmpty(City) ? $"[{Code}] {Name}" : $"[{Code}] {Name} :: {City}";
	}

	static PhotonRegion()
	{
		regions = new Dictionary<Regions, PhotonRegion>
		{
			{
				Regions.BEST_REGION,
				new PhotonRegion("best", "Best Region", null, Regions.BEST_REGION)
			},
			{
				Regions.ASIA,
				new PhotonRegion("asia", "Asia", "Singapore", Regions.ASIA)
			},
			{
				Regions.AU,
				new PhotonRegion("au", "Australia", "Melbourne", Regions.AU)
			},
			{
				Regions.CAE,
				new PhotonRegion("cae", "Canada, East", "Montreal", Regions.CAE)
			},
			{
				Regions.CN,
				new PhotonRegion("cn", "Chinese Mainland", "Guangdong", Regions.CN)
			},
			{
				Regions.EU,
				new PhotonRegion("eu", "Europe", "Amsterdam", Regions.EU)
			},
			{
				Regions.IN,
				new PhotonRegion("in", "India", "Chennai", Regions.IN)
			},
			{
				Regions.JP,
				new PhotonRegion("jp", "Japan", "Tokyo", Regions.JP)
			},
			{
				Regions.RU,
				new PhotonRegion("ru", "Russia", "Moscow", Regions.RU)
			},
			{
				Regions.RUE,
				new PhotonRegion("rue", "Russia, East", "Khabarovsk", Regions.RUE)
			},
			{
				Regions.ZA,
				new PhotonRegion("za", "South Africa", "Johannesburg", Regions.ZA)
			},
			{
				Regions.SA,
				new PhotonRegion("sa", "South America", "Sao Paulo", Regions.SA)
			},
			{
				Regions.KR,
				new PhotonRegion("kr", "South Korea", "Seoul", Regions.KR)
			},
			{
				Regions.TR,
				new PhotonRegion("tr", "Turkey", "Istanbul", Regions.TR)
			},
			{
				Regions.US,
				new PhotonRegion("us", "USA, East", "Washington DC", Regions.US)
			},
			{
				Regions.USW,
				new PhotonRegion("usw", "USA, West", "San José", Regions.USW)
			}
		};
	}

	private PhotonRegion(string code, string name, string city, Regions region)
	{
		Code = code.ToLower();
		Name = name;
		City = city;
		Region = region;
	}
}
