using Vintagestory.API.Common;

namespace ElectricityUnofficial.Interface;

public interface IEnergyStorageItem
{
    int receiveEnergy(ItemStack itemstack, int maxReceive);
}