namespace MarInTime.Application.Repositories
{
    public interface ILandChecker
    {
        public Task<bool> IsOnLand(double lng, double lat);
    }
}
