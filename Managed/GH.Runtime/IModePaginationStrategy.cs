public interface IModePaginationStrategy
{
	int Paginate(int totalActiveButtons, int offset, int lastModeIndex);
}
