using System;
using System.Runtime.InteropServices;

namespace Sony.NP;

public class Commerce
{
	public struct CategoryLabel
	{
		public const int CATEGORY_LABEL_MAX_LEN = 16;

		public const int SDK4_0_CATEGORY_LABEL_MAX_LEN = 55;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 56)]
		internal string internalValue;

		public string Value
		{
			get
			{
				return internalValue;
			}
			set
			{
				int num = 16;
				if (Main.initResult.sceSDKVersion < 72351744)
				{
					num = 55;
				}
				if (value.Length > num)
				{
					throw new NpToolkitException("The size of the label is more than " + num + " characters.");
				}
				internalValue = value;
			}
		}

		internal void Read(MemoryBuffer buffer)
		{
			buffer.ReadString(ref internalValue);
		}
	}

	public struct ServiceEntitlementLabel
	{
		public const int SERVICE_ENTITLEMENT_LABEL_MAX_LEN = 6;

		public const int SDK4_0_SERVICE_ENTITLEMENT_LABEL_MAX_LEN = 31;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		internal string internalValue;

		public string Value
		{
			get
			{
				return internalValue;
			}
			set
			{
				int num = 6;
				if (Main.initResult.sceSDKVersion < 72351744)
				{
					num = 31;
				}
				if (value.Length > num)
				{
					throw new NpToolkitException("The size of the label is more than " + num + " characters.");
				}
				internalValue = value;
			}
		}

		internal void Read(MemoryBuffer buffer)
		{
			buffer.ReadString(ref internalValue);
		}
	}

	public struct ProductLabel
	{
		public const int PRODUCT_LABEL_MAX_LEN = 16;

		public const int SDK4_0_PRODUCT_LABEL_MAX_LEN = 47;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 48)]
		internal string internalValue;

		public string Value
		{
			get
			{
				return internalValue;
			}
			set
			{
				int num = 16;
				if (Main.initResult.sceSDKVersion < 72351744)
				{
					num = 47;
				}
				if (value.Length > num)
				{
					throw new NpToolkitException("The size of the label is more than " + num + " characters.");
				}
				internalValue = value;
			}
		}

		internal void Read(MemoryBuffer buffer)
		{
			buffer.ReadString(ref internalValue);
		}
	}

	public struct SkuLabel
	{
		public const int SKU_LABEL_MAX_LEN = 4;

		public const int SDK4_0_SKU_LABEL_MAX_LEN = 55;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 56)]
		internal string internalValue;

		public string Value
		{
			get
			{
				return internalValue;
			}
			set
			{
				int num = 4;
				if (Main.initResult.sceSDKVersion < 72351744)
				{
					num = 55;
				}
				if (value.Length > num)
				{
					throw new NpToolkitException("The size of the label is more than " + num + " characters.");
				}
				internalValue = value;
			}
		}

		internal void Read(MemoryBuffer buffer)
		{
			buffer.ReadString(ref internalValue);
		}
	}

	public struct DownloadListTarget
	{
		internal ProductLabel productLabel;

		internal SkuLabel skuLabel;

		public ProductLabel ProductLabel
		{
			get
			{
				return productLabel;
			}
			set
			{
				productLabel = value;
			}
		}

		public SkuLabel SkuLabel
		{
			get
			{
				return skuLabel;
			}
			set
			{
				skuLabel = value;
			}
		}
	}

	public struct CheckoutTarget
	{
		public const uint NP_INVALID_SERVICE_LABEL = uint.MaxValue;

		internal ProductLabel productLabel;

		internal SkuLabel skuLabel;

		internal uint serviceLabel;

		public ProductLabel ProductLabel
		{
			get
			{
				return productLabel;
			}
			set
			{
				productLabel = value;
			}
		}

		public SkuLabel SkuLabel
		{
			get
			{
				return skuLabel;
			}
			set
			{
				skuLabel = value;
			}
		}

		public uint ServiceLabel
		{
			get
			{
				return serviceLabel;
			}
			set
			{
				serviceLabel = value;
			}
		}
	}

	public class SubCategory
	{
		internal string categoryName;

		internal string categoryDescription;

		internal string imageUrl;

		internal CategoryLabel categoryLabel;

		public string CategoryName => categoryName;

		public string CategoryDescription => categoryDescription;

		public string ImageUrl => imageUrl;

		public CategoryLabel CategoryLabel => categoryLabel;

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.SubCategoryBegin);
			buffer.ReadString(ref categoryName);
			buffer.ReadString(ref categoryDescription);
			buffer.ReadString(ref imageUrl);
			categoryLabel.Read(buffer);
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.SubCategoryEnd);
		}
	}

	public class Category
	{
		internal SubCategory[] subCategories;

		internal ulong countOfProducts;

		internal string categoryName;

		internal string categoryDescription;

		internal string imageUrl;

		internal CategoryLabel categoryLabel;

		public SubCategory[] SubCategories => subCategories;

		public ulong CountOfProducts => countOfProducts;

		public string CategoryName => categoryName;

		public string CategoryDescription => categoryDescription;

		public string ImageUrl => imageUrl;

		public CategoryLabel CategoryLabel => categoryLabel;

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.CategoryBegin);
			ulong num = buffer.ReadUInt64();
			if (num != 0)
			{
				subCategories = new SubCategory[num];
				for (ulong num2 = 0uL; num2 < num; num2++)
				{
					subCategories[num2] = new SubCategory();
					subCategories[num2].Read(buffer);
				}
			}
			countOfProducts = buffer.ReadUInt64();
			buffer.ReadString(ref categoryName);
			buffer.ReadString(ref categoryDescription);
			buffer.ReadString(ref imageUrl);
			categoryLabel.Read(buffer);
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.CategoryEnd);
		}
	}

	public class Product
	{
		internal ProductLabel productLabel;

		internal string productName;

		internal string imageUrl;

		internal bool hasDetails;

		internal ProductDetails details;

		public ProductLabel ProductLabel => productLabel;

		public string ProductName => productName;

		public string ImageUrl => imageUrl;

		public bool HasDetails => hasDetails;

		public ProductDetails Details => details;

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.ProductBegin);
			productLabel.Read(buffer);
			buffer.ReadString(ref productName);
			buffer.ReadString(ref imageUrl);
			hasDetails = buffer.ReadBool();
			if (hasDetails)
			{
				details = new ProductDetails();
				details.Read(buffer);
			}
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.ProductEnd);
		}
	}

	public enum PurchasabilityStatus
	{
		NotPurchased,
		PurchasedCanPurchaseAgain,
		PurchasedCannotPurchaseAgain
	}

	public class RatingDescriptor
	{
		internal string name;

		internal string imageUrl;

		public string Name => name;

		public string ImageUrl => imageUrl;

		internal void Read(MemoryBuffer buffer)
		{
			buffer.ReadString(ref name);
			buffer.ReadString(ref imageUrl);
		}
	}

	public class ProductDetails
	{
		internal DateTime releaseDate;

		internal string longDescription;

		internal string spName;

		internal string ratingSystemId;

		internal string ratingImageUrl;

		internal RatingDescriptor[] ratingDescriptors;

		internal SkuInfo[] skuinfo;

		internal PurchasabilityStatus purchasabilityStatus;

		internal uint starRatingsTotal;

		internal double starRatingScore;

		public DateTime ReleaseDate => releaseDate;

		public string LongDescription => longDescription;

		public string SpName => spName;

		public string RatingSystemId => ratingSystemId;

		public string RatingImageUrl => ratingImageUrl;

		public RatingDescriptor[] RatingDescriptors => ratingDescriptors;

		public SkuInfo[] Skuinfo => skuinfo;

		[Obsolete("The PurchasabilityStatus property has been removed from the ProductDetails class, because it was never set. Instead, use the PurchasabilityStatus property of the SkuInfo class")]
		public PurchasabilityStatus PurchasabilityStatus => purchasabilityStatus;

		public uint StarRatingsTotal => starRatingsTotal;

		public double StarRatingScore => starRatingScore;

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.ProductDetailsBegin);
			releaseDate = Core.ReadRtcTick(buffer);
			buffer.ReadString(ref longDescription);
			buffer.ReadString(ref spName);
			buffer.ReadString(ref ratingSystemId);
			buffer.ReadString(ref ratingImageUrl);
			ulong num = buffer.ReadUInt64();
			if (num != 0)
			{
				ratingDescriptors = new RatingDescriptor[num];
				for (int i = 0; i < (int)num; i++)
				{
					ratingDescriptors[i] = new RatingDescriptor();
					ratingDescriptors[i].Read(buffer);
				}
			}
			else
			{
				ratingDescriptors = null;
			}
			ulong num2 = buffer.ReadUInt64();
			if (num2 != 0)
			{
				skuinfo = new SkuInfo[num2];
				for (int i = 0; i < (int)num2; i++)
				{
					skuinfo[i] = new SkuInfo();
					skuinfo[i].Read(buffer);
				}
			}
			else
			{
				skuinfo = null;
			}
			purchasabilityStatus = (PurchasabilityStatus)buffer.ReadUInt32();
			starRatingsTotal = buffer.ReadUInt32();
			starRatingScore = buffer.ReadDouble();
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.ProductDetailsEnd);
		}
	}

	public enum SkuType
	{
		Invalid,
		Standard,
		Preorder
	}

	public class SkuInfo
	{
		internal SkuType type;

		internal PurchasabilityStatus purchasabilityStatus;

		internal SkuLabel label;

		internal string name;

		internal string price;

		internal ulong intPrice;

		internal uint consumableUseCount;

		public SkuType Type => type;

		public PurchasabilityStatus PurchasabilityStatus => purchasabilityStatus;

		public SkuLabel Label => label;

		public string Name => name;

		public string Price => price;

		public ulong IntPrice => intPrice;

		public uint ConsumableUseCount => consumableUseCount;

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.SkuInfoBegin);
			type = (SkuType)buffer.ReadUInt32();
			purchasabilityStatus = (PurchasabilityStatus)buffer.ReadUInt32();
			label.Read(buffer);
			buffer.ReadString(ref name);
			buffer.ReadString(ref price);
			intPrice = buffer.ReadUInt64();
			consumableUseCount = buffer.ReadUInt32();
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.SkuInfoEnd);
		}
	}

	public enum EntitlementType
	{
		Invalid,
		Service,
		ServiceConsumable,
		Unified
	}

	public class ServiceEntitlement
	{
		internal ServiceEntitlementLabel entitlementLabel;

		internal DateTime createdDate;

		internal DateTime expireDate;

		internal long remainingCount;

		internal uint consumedCount;

		internal EntitlementType type;

		public ServiceEntitlementLabel EntitlementLabel => entitlementLabel;

		public DateTime CreatedDate => createdDate;

		public DateTime ExpireDate => expireDate;

		public long RemainingCount => remainingCount;

		public uint ConsumedCount => consumedCount;

		public EntitlementType Type => type;

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.ServiceEntitlementBegin);
			entitlementLabel.Read(buffer);
			createdDate = Core.ReadRtcTick(buffer);
			expireDate = Core.ReadRtcTick(buffer);
			remainingCount = buffer.ReadInt64();
			consumedCount = buffer.ReadUInt32();
			type = (EntitlementType)buffer.ReadUInt32();
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.ServiceEntitlementEnd);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetCategoriesRequest : RequestBase
	{
		public const int MAX_CATEGORIES = 8;

		internal ulong numCategories;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		internal CategoryLabel[] categoryLabels = new CategoryLabel[8];

		public CategoryLabel[] CategoryLabels
		{
			get
			{
				if (numCategories == 0)
				{
					return null;
				}
				CategoryLabel[] array = new CategoryLabel[numCategories];
				Array.Copy(categoryLabels, array, (int)numCategories);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 8)
					{
						throw new NpToolkitException("The size of the array is more than " + 8);
					}
					value.CopyTo(categoryLabels, 0);
					numCategories = (uint)value.Length;
				}
				else
				{
					numCategories = 0uL;
				}
			}
		}

		public GetCategoriesRequest()
			: base(ServiceTypes.Commerce, FunctionTypes.CommerceGetCategories)
		{
		}
	}

	public enum ProductSortOrders
	{
		DefaultOrder,
		Name,
		Price,
		ReleaseDate
	}

	public enum ProductSortDirections
	{
		Ascending,
		Descending
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetProductsRequest : RequestBase
	{
		public const int MAX_PRODUCTS = 32;

		public const int MAX_CATEGORIES = 8;

		public const int DEFAULT_PAGE_SIZE = 32;

		internal ulong numProducts;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		internal ProductLabel[] productLabels = new ProductLabel[32];

		internal ulong numCategories;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		internal CategoryLabel[] categoryLabels = new CategoryLabel[8];

		internal uint offset;

		internal uint pageSize;

		internal ProductSortOrders sortOrder;

		internal ProductSortDirections sortDirection;

		[MarshalAs(UnmanagedType.I1)]
		internal bool keepHtmlTags;

		[MarshalAs(UnmanagedType.I1)]
		internal bool useCurrencySymbol;

		public ProductLabel[] ProductLabels
		{
			get
			{
				if (numProducts == 0)
				{
					return null;
				}
				ProductLabel[] array = new ProductLabel[numProducts];
				Array.Copy(productLabels, array, (int)numProducts);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 32)
					{
						throw new NpToolkitException("The size of the array is more than " + 32);
					}
					value.CopyTo(productLabels, 0);
					numProducts = (uint)value.Length;
				}
				else
				{
					numProducts = 0uL;
				}
			}
		}

		public CategoryLabel[] CategoryLabels
		{
			get
			{
				if (numCategories == 0)
				{
					return null;
				}
				CategoryLabel[] array = new CategoryLabel[numCategories];
				Array.Copy(categoryLabels, array, (int)numCategories);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 8)
					{
						throw new NpToolkitException("The size of the array is more than " + 8);
					}
					value.CopyTo(categoryLabels, 0);
					numCategories = (uint)value.Length;
				}
				else
				{
					numCategories = 0uL;
				}
			}
		}

		public uint Offset
		{
			get
			{
				return offset;
			}
			set
			{
				offset = value;
			}
		}

		public uint PageSize
		{
			get
			{
				return pageSize;
			}
			set
			{
				pageSize = value;
			}
		}

		public ProductSortOrders SortOrder
		{
			get
			{
				return sortOrder;
			}
			set
			{
				sortOrder = value;
			}
		}

		public ProductSortDirections SortDirection
		{
			get
			{
				return sortDirection;
			}
			set
			{
				sortDirection = value;
			}
		}

		public bool KeepHtmlTags
		{
			get
			{
				return keepHtmlTags;
			}
			set
			{
				keepHtmlTags = value;
			}
		}

		public bool UseCurrencySymbol
		{
			get
			{
				return useCurrencySymbol;
			}
			set
			{
				useCurrencySymbol = value;
			}
		}

		public GetProductsRequest()
			: base(ServiceTypes.Commerce, FunctionTypes.CommerceGetProducts)
		{
			pageSize = 32u;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetServiceEntitlementsRequest : RequestBase
	{
		public const int DEFAULT_PAGE_SIZE = 64;

		internal uint offset;

		internal uint pageSize;

		public uint Offset
		{
			get
			{
				return offset;
			}
			set
			{
				offset = value;
			}
		}

		public uint PageSize
		{
			get
			{
				return pageSize;
			}
			set
			{
				pageSize = value;
			}
		}

		public GetServiceEntitlementsRequest()
			: base(ServiceTypes.Commerce, FunctionTypes.CommerceGetServiceEntitlements)
		{
			pageSize = 64u;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class ConsumeServiceEntitlementRequest : RequestBase
	{
		internal ServiceEntitlementLabel entitlementLabel;

		internal uint consumedCount;

		public ServiceEntitlementLabel EntitlementLabel
		{
			get
			{
				return entitlementLabel;
			}
			set
			{
				entitlementLabel = value;
			}
		}

		public uint ConsumedCount
		{
			get
			{
				return consumedCount;
			}
			set
			{
				consumedCount = value;
			}
		}

		public ConsumeServiceEntitlementRequest()
			: base(ServiceTypes.Commerce, FunctionTypes.CommerceConsumeServiceEntitlement)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class DisplayCategoryBrowseDialogRequest : RequestBase
	{
		internal CategoryLabel categoryLabel;

		public CategoryLabel CategoryLabel
		{
			get
			{
				return categoryLabel;
			}
			set
			{
				categoryLabel = value;
			}
		}

		public DisplayCategoryBrowseDialogRequest()
			: base(ServiceTypes.Commerce, FunctionTypes.CommerceDisplayCategoryBrowseDialog)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class DisplayProductBrowseDialogRequest : RequestBase
	{
		internal ProductLabel productLabel;

		public ProductLabel ProductLabel
		{
			get
			{
				return productLabel;
			}
			set
			{
				productLabel = value;
			}
		}

		public DisplayProductBrowseDialogRequest()
			: base(ServiceTypes.Commerce, FunctionTypes.CommerceDisplayProductBrowseDialog)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class DisplayVoucherCodeInputDialogRequest : RequestBase
	{
		public const int VOUCHER_CODE_LEN = 63;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		internal string voucherCode;

		public string VoucherCode
		{
			get
			{
				return voucherCode;
			}
			set
			{
				if (value.Length > 63)
				{
					throw new NpToolkitException("The size of the voucher code is more than " + 63 + " characters.");
				}
				voucherCode = value;
			}
		}

		public DisplayVoucherCodeInputDialogRequest()
			: base(ServiceTypes.Commerce, FunctionTypes.CommerceDisplayVoucherCodeInputDialog)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class DisplayCheckoutDialogRequest : RequestBase
	{
		public const int MAX_TARGETS = 10;

		internal ulong numTargets;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
		internal CheckoutTarget[] targets = new CheckoutTarget[10];

		public CheckoutTarget[] Targets
		{
			get
			{
				if (numTargets == 0)
				{
					return null;
				}
				CheckoutTarget[] array = new CheckoutTarget[numTargets];
				Array.Copy(targets, array, (int)numTargets);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 10)
					{
						throw new NpToolkitException("The size of the array is more than " + 10);
					}
					value.CopyTo(targets, 0);
					numTargets = (ulong)value.Length;
				}
				else
				{
					numTargets = 0uL;
				}
			}
		}

		public DisplayCheckoutDialogRequest()
			: base(ServiceTypes.Commerce, FunctionTypes.CommerceDisplayCheckoutDialog)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class DisplayDownloadListDialogRequest : RequestBase
	{
		public const int MAX_TARGETS = 10;

		internal ulong numTargets;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
		internal DownloadListTarget[] targets = new DownloadListTarget[10];

		public DownloadListTarget[] Targets
		{
			get
			{
				if (numTargets == 0)
				{
					return null;
				}
				DownloadListTarget[] array = new DownloadListTarget[numTargets];
				Array.Copy(targets, array, (int)numTargets);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 10)
					{
						throw new NpToolkitException("The size of the array is more than " + 10);
					}
					value.CopyTo(targets, 0);
					numTargets = (ulong)value.Length;
				}
				else
				{
					numTargets = 0uL;
				}
			}
		}

		public DisplayDownloadListDialogRequest()
			: base(ServiceTypes.Commerce, FunctionTypes.CommerceDisplayDownloadListDialog)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class DisplayJoinPlusDialogRequest : RequestBase
	{
		internal ulong features;

		public DisplayJoinPlusDialogRequest()
			: base(ServiceTypes.Commerce, FunctionTypes.CommerceDisplayJoinPlusDialog)
		{
			features = 1uL;
		}
	}

	public enum PsStoreIconPos
	{
		Center,
		Left,
		Right
	}

	[StructLayout(LayoutKind.Sequential)]
	public class SetPsStoreIconDisplayStateRequest : RequestBase
	{
		internal PsStoreIconPos iconPosition;

		[MarshalAs(UnmanagedType.I1)]
		internal bool showIcon;

		public PsStoreIconPos IconPosition
		{
			get
			{
				return iconPosition;
			}
			set
			{
				iconPosition = value;
			}
		}

		public bool ShowIcon
		{
			get
			{
				return showIcon;
			}
			set
			{
				showIcon = value;
			}
		}

		public SetPsStoreIconDisplayStateRequest()
			: base(ServiceTypes.Commerce, FunctionTypes.CommerceSetPsStoreIconDisplayState)
		{
		}
	}

	public class CategoriesResponse : ResponseBase
	{
		internal Category[] categories;

		public Category[] Categories => categories;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.CategoriesBegin);
			ulong num = memoryBuffer.ReadUInt64();
			if (num != 0)
			{
				categories = new Category[num];
				for (ulong num2 = 0uL; num2 < num; num2++)
				{
					categories[num2] = new Category();
					categories[num2].Read(memoryBuffer);
				}
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.CategoriesEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class ProductsResponse : ResponseBase
	{
		internal Product[] products;

		public Product[] Products => products;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.ProductsBegin);
			ulong num = memoryBuffer.ReadUInt64();
			if (num != 0)
			{
				products = new Product[num];
				for (ulong num2 = 0uL; num2 < num; num2++)
				{
					products[num2] = new Product();
					products[num2].Read(memoryBuffer);
				}
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.ProductsEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class ServiceEntitlementsResponse : ResponseBase
	{
		internal ServiceEntitlement[] entitlements;

		internal ulong totalEntitlementsAvailable;

		public ServiceEntitlement[] Entitlements => entitlements;

		public ulong TotalEntitlementsAvailable => totalEntitlementsAvailable;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.ServiceEntitlementsBegin);
			totalEntitlementsAvailable = memoryBuffer.ReadUInt64();
			ulong num = memoryBuffer.ReadUInt64();
			if (num != 0)
			{
				entitlements = new ServiceEntitlement[num];
				for (int i = 0; i < (int)num; i++)
				{
					entitlements[i] = new ServiceEntitlement();
					entitlements[i].Read(memoryBuffer);
				}
			}
			else
			{
				entitlements = null;
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.ServiceEntitlementsEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetCategories(GetCategoriesRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetProducts(GetProductsRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetServiceEntitlements(GetServiceEntitlementsRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxConsumeServiceEntitlement(ConsumeServiceEntitlementRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxDisplayCategoryBrowseDialog(DisplayCategoryBrowseDialogRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxDisplayProductBrowseDialog(DisplayProductBrowseDialogRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxDisplayVoucherCodeInputDialog(DisplayVoucherCodeInputDialogRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxDisplayCheckoutDialog(DisplayCheckoutDialogRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxDisplayDownloadListDialog(DisplayDownloadListDialogRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxDisplayJoinPlusDialog(DisplayJoinPlusDialogRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxSetPsStoreIconDisplayState(SetPsStoreIconDisplayStateRequest request, out APIResult result);

	public static int GetCategories(GetCategoriesRequest request, CategoriesResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetCategories(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetProducts(GetProductsRequest request, ProductsResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetProducts(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetServiceEntitlements(GetServiceEntitlementsRequest request, ServiceEntitlementsResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetServiceEntitlements(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int ConsumeServiceEntitlement(ConsumeServiceEntitlementRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxConsumeServiceEntitlement(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int DisplayCategoryBrowseDialog(DisplayCategoryBrowseDialogRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxDisplayCategoryBrowseDialog(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int DisplayProductBrowseDialog(DisplayProductBrowseDialogRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxDisplayProductBrowseDialog(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int DisplayVoucherCodeInputDialog(DisplayVoucherCodeInputDialogRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxDisplayVoucherCodeInputDialog(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int DisplayCheckoutDialog(DisplayCheckoutDialogRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxDisplayCheckoutDialog(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int DisplayDownloadListDialog(DisplayDownloadListDialogRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxDisplayDownloadListDialog(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int DisplayJoinPlusDialog(DisplayJoinPlusDialogRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxDisplayJoinPlusDialog(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int SetPsStoreIconDisplayState(SetPsStoreIconDisplayStateRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxSetPsStoreIconDisplayState(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}
}
