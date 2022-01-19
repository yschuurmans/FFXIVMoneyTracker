using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVMoneyTracker
{
    public unsafe class InventoryHelper
    {
        public int GetGil()
        {
            return InventoryManager.Instance()->GetItemCountInContainer(1, InventoryType.Currency);
        }
    }
}
