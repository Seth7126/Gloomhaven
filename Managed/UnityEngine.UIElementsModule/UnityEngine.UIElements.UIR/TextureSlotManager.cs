using System.Runtime.CompilerServices;

namespace UnityEngine.UIElements.UIR;

internal class TextureSlotManager
{
	private static readonly int k_SlotCount;

	internal static readonly int[] slotIds;

	internal static readonly int textureTableId;

	private TextureId[] m_Textures;

	private int[] m_Tickets;

	private int m_CurrentTicket;

	private int m_FirstUsedTicket;

	private Vector4[] m_GpuTextures;

	internal TextureRegistry textureRegistry = TextureRegistry.instance;

	public int FreeSlots { get; private set; } = k_SlotCount;

	static TextureSlotManager()
	{
		textureTableId = Shader.PropertyToID("_TextureInfo");
		k_SlotCount = (UIRenderDevice.shaderModelIs35 ? 8 : 4);
		slotIds = new int[k_SlotCount];
		for (int i = 0; i < k_SlotCount; i++)
		{
			slotIds[i] = Shader.PropertyToID($"_Texture{i}");
		}
	}

	public TextureSlotManager()
	{
		m_Textures = new TextureId[k_SlotCount];
		m_Tickets = new int[k_SlotCount];
		m_GpuTextures = new Vector4[k_SlotCount];
		Reset();
	}

	public void Reset()
	{
		m_CurrentTicket = 0;
		m_FirstUsedTicket = 0;
		for (int i = 0; i < k_SlotCount; i++)
		{
			m_Textures[i] = TextureId.invalid;
			m_Tickets[i] = -1;
			m_GpuTextures[i] = new Vector4(-1f, 1f, 1f, 0f);
		}
	}

	public void StartNewBatch()
	{
		m_FirstUsedTicket = ++m_CurrentTicket;
		FreeSlots = k_SlotCount;
	}

	public int IndexOf(TextureId id)
	{
		for (int i = 0; i < k_SlotCount; i++)
		{
			if (m_Textures[i].index == id.index)
			{
				return i;
			}
		}
		return -1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void MarkUsed(int slotIndex)
	{
		int num = m_Tickets[slotIndex];
		if (num < m_FirstUsedTicket)
		{
			int freeSlots = FreeSlots - 1;
			FreeSlots = freeSlots;
		}
		m_Tickets[slotIndex] = ++m_CurrentTicket;
	}

	public int FindOldestSlot()
	{
		int num = m_Tickets[0];
		int result = 0;
		for (int i = 1; i < k_SlotCount; i++)
		{
			if (m_Tickets[i] < num)
			{
				num = m_Tickets[i];
				result = i;
			}
		}
		return result;
	}

	public void Bind(TextureId id, int slot, MaterialPropertyBlock mat)
	{
		Texture texture = textureRegistry.GetTexture(id);
		if (texture == null)
		{
			texture = Texture2D.whiteTexture;
		}
		m_Textures[slot] = id;
		MarkUsed(slot);
		m_GpuTextures[slot] = new Vector4(id.ConvertToGpu(), 1f / (float)texture.width, 1f / (float)texture.height, 0f);
		mat.SetTexture(slotIds[slot], texture);
		mat.SetVectorArray(textureTableId, m_GpuTextures);
	}
}
