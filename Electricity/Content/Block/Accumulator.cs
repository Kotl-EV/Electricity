using System;
using System.Text;
using ElectricityUnofficial.Interface;
using ElectricityUnofficial.Utils;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace Electricity.Content.Block {
    public class Accumulator : Vintagestory.API.Common.Block,IEnergyStorageItem {
        public int maxcapacity;
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            maxcapacity = AttributeGetter.GetAttributeInt(this, "maxstorage", 16000);
            Durability = 100;
        }
    
        public int receiveEnergy(ItemStack itemstack, int maxReceive)
        {
            int energy = itemstack.Attributes.GetInt("electricity:energy", 0);
            int received = Math.Min(maxcapacity - energy, maxReceive);
            itemstack.Attributes.SetInt("electricity:energy", energy + received);
            int durab = (energy + received) / (maxcapacity / GetDurability(itemstack));
            itemstack.Attributes.SetInt("durability", durab);
            return received;
        }
        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode) {
            return world.BlockAccessor
                       .GetBlock(blockSel.Position.AddCopy(BlockFacing.DOWN))
                       .SideSolid[BlockFacing.indexUP] &&
                   base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
        }

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos) {
            base.OnNeighbourBlockChange(world, pos, neibpos);

            if (
                !world.BlockAccessor
                    .GetBlock(pos.AddCopy(BlockFacing.DOWN))
                    .SideSolid[BlockFacing.indexUP]
            ) {
                world.BlockAccessor.BreakBlock(pos, null);
            }
        }
        
        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
            dsc.AppendLine(Lang.Get("Storage") + inSlot.Itemstack.Attributes.GetInt("electricity:energy", 0) + "/" + maxcapacity + "⚡   ");
        }
    
        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            Entity.Accumulator? be = world.BlockAccessor.GetBlockEntity(pos) as Entity.Accumulator;
            ItemStack item = new ItemStack(world.BlockAccessor.GetBlock(pos));
            if (be != null) item.Attributes.SetInt("electricity:energy", be.GetBehavior<Entity.Behavior.Accumulator>().GetCapacity());
            if (be != null) item.Attributes.SetInt("durability", (100 * be.GetBehavior<Entity.Behavior.Accumulator>().GetCapacity() / maxcapacity));
            return new ItemStack[] {item};
        }
    
        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            Entity.Accumulator? be = world.BlockAccessor.GetBlockEntity(pos) as Entity.Accumulator;
            ItemStack item = new ItemStack(world.BlockAccessor.GetBlock(pos));
            if (be != null) item.Attributes.SetInt("electricity:energy", be.GetBehavior<Entity.Behavior.Accumulator>().GetCapacity());
            if (be != null) item.Attributes.SetInt("durability", (100 * be.GetBehavior<Entity.Behavior.Accumulator>().GetCapacity() / maxcapacity));
            return item;
        }

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(world, blockPos, byItemStack);
            if (byItemStack != null)
            {
                Entity.Accumulator? be = world.BlockAccessor.GetBlockEntity(blockPos) as Entity.Accumulator;
                be.GetBehavior<Entity.Behavior.Accumulator>().Store(byItemStack.Attributes.GetInt("electricity:energy", 0));
            }
        }
    }
}
