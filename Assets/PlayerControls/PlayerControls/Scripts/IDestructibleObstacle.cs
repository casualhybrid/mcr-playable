public interface IDestructibleObstacle
{
    bool isDestroyableDuringDash { get; }
    bool isDestroyingDuringBoost { get; }
    bool isDestroyingDuringLaser { get; }
    bool isDestroyDuringShockwave { get; }
    bool isDestroying { get; }

    HitDirection DirectionThePlayerHitFrom { get; set; }

    void HandleGotHitByShockWave();

    void HandleGotHitByLaser();
}