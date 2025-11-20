namespace UdpKit.Protocol;

internal abstract class Query : Message
{
	public Result Result;

	public bool HasResult => Result != null;

	public bool Failed => Result == null;

	public virtual bool IsUnique => false;

	public virtual bool Resend => false;

	public virtual uint BaseTimeout => 500u;
}
internal abstract class Query<TResult> : Query where TResult : Result
{
	public new TResult Result => (TResult)base.Result;
}
