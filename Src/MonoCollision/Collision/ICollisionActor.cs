namespace ConsoleApp26
{
    public interface ICollisionActor
    {
        Collider GetCollider();
        void HandleCollision(Collision collision);
    }
}