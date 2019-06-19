using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Models;

namespace Engine.Factories
{
    public static class ItemFactory
    {
        private static readonly List<GameItem> _standardGameItems = new List<GameItem>();
        static ItemFactory()
        {
            BuildWaepon(1001, "Pointy stick", 1, 1, 2);
            BuildWaepon(1002, "Rusty sword", 5, 1, 3);
            BuildMiscellaneousItem(9001, "Snake fang", 1);
            BuildMiscellaneousItem(9002, "Snakeskin", 2);
            BuildMiscellaneousItem(9003, "Rat tail", 1);
            BuildMiscellaneousItem(9004, "Rat fur", 2);
            BuildMiscellaneousItem(9005, "Spider fang", 1);
            BuildMiscellaneousItem(9006, "Spider silk", 2);
        }

        public static GameItem CreateGameItem( int itemTypeID)
        {
            return _standardGameItems.FirstOrDefault(item => item.ItemTypeID == itemTypeID)?.Clone();
        }

        private static void BuildMiscellaneousItem(int id, string name,int price)
        {
            _standardGameItems.Add(new GameItem(GameItem.ItemCategory.Miscellenus, id, name, price));
        }

        private static void BuildWaepon(int id, string name, int price, int minimumDamage, int maximumDamage)
        {
            _standardGameItems.Add(new GameItem(GameItem.ItemCategory.Weapon, id, name, price, true, minimumDamage, maximumDamage));
        }
    }
}
