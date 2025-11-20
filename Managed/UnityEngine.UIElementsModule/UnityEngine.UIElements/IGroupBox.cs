namespace UnityEngine.UIElements;

internal interface IGroupBox
{
}
internal interface IGroupBox<T> : IGroupBox where T : IGroupManager
{
}
