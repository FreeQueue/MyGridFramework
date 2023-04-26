using Framework;
using OpenRA.Activities;
using UnityEngine;

namespace OpenRA.Traits
{
	[RequireExplicitImpl]
	public interface INotifyCreated
	{
		void Created(Actor self);
	}

	[RequireExplicitImpl]
	public interface INotifyActorDisposing
	{
		void Disposing(Actor self);
	}

	[RequireExplicitImpl]
	public interface INotifyBecomingIdle
	{
		void OnBecomingIdle(Actor self);
	}

	[RequireExplicitImpl]
	public interface INotifyIdle
	{
		void TickIdle(Actor self);
	}

	[RequireExplicitImpl]
	public interface ICreationActivity
	{
		Activity GetCreationActivity();
	}

	public interface IMoveInfo : ITraitInfoInterface
	{
		Color GetTargetLineColor();
	}
}