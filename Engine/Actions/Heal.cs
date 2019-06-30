using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Models;

namespace Engine.Actions
{
    class Heal : BaseAction, IActions
    {        
        private readonly int _pointsToHeal;       

        public Heal( GameItem itemInUse, int pointsToHeal):base(itemInUse)
        {
            if(itemInUse.Category != GameItem.ItemCategory.Consumable)
            {
                throw new ArgumentException($"{itemInUse.Name} is not consumable!");
            }           
            _pointsToHeal = pointsToHeal;

        }
        public void Execute(LivingEntity actor, LivingEntity target)
        {
            string actorName = (actor is Player) ? "You" : $"The {actor.Name.ToLower()}";
            string targentName = (target is Player) ? "yourself" : $"the {target.Name.ToLower()}";
            ReportResult($"{actorName} heal {targentName} for {_pointsToHeal} point{(_pointsToHeal > 1 ? "s" : "")}");
            target.Heal(_pointsToHeal);
        }        
    }
}
