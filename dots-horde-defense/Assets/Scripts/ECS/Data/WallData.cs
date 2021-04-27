using Unity.Entities;

public struct WallData : IComponentData
{
	public Entity TopPart;
	public Entity RightPart;
	public Entity BotPart;
	public Entity LeftPart;
}