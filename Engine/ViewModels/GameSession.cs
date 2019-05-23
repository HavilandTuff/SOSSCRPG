using System;
//using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using Engine.Models;
using Engine.Factories;
using Engine.EventArgs;

namespace Engine.ViewModels
{
    public class GameSession : BaseNotificationClass
    {
        public event EventHandler<GameMessageEventArgs> OnMessageRaised;

        #region Properties
        private Location _currentLocation;
        private Monster _currentMonster;
        private Trader _currentTrader;
        public bool HasMonster => CurrentMonster != null;
        public bool HasTrader => CurrentTrader != null;
        public Player CurrentPlayer { get; set; }
        public World CurrentWorld { get; set; }
        

        public Location CurrentLocation
        {
            get { return _currentLocation; }
            set
            {
                _currentLocation = value;
                OnPropertyChanged(nameof(CurrentLocation));
                OnPropertyChanged(nameof(HasLocationToNorth));
                OnPropertyChanged(nameof(HasLocationToSouth));
                OnPropertyChanged(nameof(HasLocationToWest));
                OnPropertyChanged(nameof(HasLocationToEast));

                GivePlayerQuestAtLocation();
                GetMonsterAtLocation();
                CurrentTrader = CurrentLocation.TraderHere;
                
            }
        }

        public Monster CurrentMonster
        {
            get { return _currentMonster; }
            set
            {
                _currentMonster = value;
                OnPropertyChanged(nameof(CurrentMonster));
                OnPropertyChanged(nameof(HasMonster));

                if(CurrentMonster != null)
                {
                    RaiseMessage(" ");
                    RaiseMessage($"You see a {CurrentMonster.Name} here!");
                }
            }
        }

        public Trader CurrentTrader
        {
            get { return _currentTrader; }
            set
            {
                _currentTrader = value;
                OnPropertyChanged(nameof(CurrentTrader));
                OnPropertyChanged(nameof(HasTrader));
            }
        }
        public Weapon CurrentWeapon { get; set; }
        #endregion



        public GameSession()
        {
            CurrentPlayer = new Player
            {
                Name = "Scott",
                CharacterClass = "Fighter",
                HitPoints = 10,
                Gold = 1000000,
                ExperiencePoints = 0,
                Level = 1
            };
            if(!CurrentPlayer.Weapons.Any())
            {
                CurrentPlayer.AddItemToInventory(ItemFactory.CreateGameItem(1001));
            }

            CurrentWorld = WorldFactory.CreateWorld();

            CurrentLocation = CurrentWorld.LocationAt(0, 0);
        }
        public void MoveNorth()
        {
            if (HasLocationToNorth)
            {
                CurrentLocation = CurrentWorld.LocationAt(CurrentLocation.XCoordinate, CurrentLocation.YCoordinate + 1);
            }
        }
        public void MoveSouth()
        {
            if (HasLocationToSouth)
            {
                CurrentLocation = CurrentWorld.LocationAt(CurrentLocation.XCoordinate, CurrentLocation.YCoordinate - 1);
            }
        }
        public void MoveWest()
        {
            if (HasLocationToWest)
            {
                CurrentLocation = CurrentWorld.LocationAt(CurrentLocation.XCoordinate - 1, CurrentLocation.YCoordinate);
            }
        }
        public void MoveEast()
        {
            if (HasLocationToEast)
            {
                CurrentLocation = CurrentWorld.LocationAt(CurrentLocation.XCoordinate + 1, CurrentLocation.YCoordinate);
            }
        }
        public bool HasLocationToNorth => CurrentWorld.LocationAt(CurrentLocation.XCoordinate, CurrentLocation.YCoordinate + 1) != null;
        public bool HasLocationToSouth => CurrentWorld.LocationAt(CurrentLocation.XCoordinate, CurrentLocation.YCoordinate - 1) != null;
        public bool HasLocationToWest => CurrentWorld.LocationAt(CurrentLocation.XCoordinate - 1, CurrentLocation.YCoordinate) != null;
        public bool HasLocationToEast => CurrentWorld.LocationAt(CurrentLocation.XCoordinate + 1, CurrentLocation.YCoordinate) != null;
        private void CompleteQuestAtLocation()
        {
            foreach(Quest quest in CurrentLocation.QuestsAvailableHere)
            {
                QuestStatus questToComplete = 
                    CurrentPlayer.Quests.FirstOrDefault(q => q.PlayerQuest.ID == quest.ID &&
                                                             !q.IsCompleted);
                if (questToComplete != null)
                {
                    if(CurrentPlayer.HasAllTheseItems(quest.ItemsToComplete))
                    {
                        foreach(ItemQuantity itemQuantity in quest.ItemsToComplete)
                        {
                            for(int i =0; i<itemQuantity.Quantity; i++)
                            {
                                CurrentPlayer.RemoveItemFromInventory(CurrentPlayer.Inventory.First(item => item.ItemTypeID == itemQuantity.ItemID));
                            }
                        }
                        RaiseMessage("");
                        RaiseMessage($"You completed the {quest.Name} quest.");

                        CurrentPlayer.ExperiencePoints += quest.RewardExperiencePonts;
                        RaiseMessage($"You received {quest.RewardExperiencePonts} experience points.");

                        CurrentPlayer.Gold += quest.RewardGold;
                        RaiseMessage($"You received {quest.RewardGold} gold.");

                        foreach(ItemQuantity itemQuantity in quest.RewardItems)
                        {
                            GameItem rewardItem = ItemFactory.CreateGameItem(itemQuantity.ItemID);
                            CurrentPlayer.AddItemToInventory(rewardItem);
                            RaiseMessage($"You received {rewardItem.Name}");
                        }
                        questToComplete.IsCompleted = true;
                    }
                }
            }
        }
        private void GivePlayerQuestAtLocation()
        {
            foreach(Quest quest in CurrentLocation.QuestsAvailableHere)
            {
                if(!CurrentPlayer.Quests.Any(q=>q.PlayerQuest.ID == quest.ID))
                {
                    CurrentPlayer.Quests.Add(new QuestStatus(quest));
                    RaiseMessage(" ");
                    RaiseMessage($"You received '{quest.Name}' quest.");
                    RaiseMessage(quest.Description);
                    RaiseMessage("Return with:");
                    foreach(ItemQuantity itemQuantity in quest.ItemsToComplete)
                    {
                        RaiseMessage($"   {itemQuantity.Quantity} {ItemFactory.CreateGameItem(itemQuantity.ItemID).Name} ");

                    }
                    RaiseMessage("And you will recieve: ");
                    RaiseMessage($"   {quest.RewardExperiencePonts} experience points,");
                    RaiseMessage($"   {quest.RewardGold} gold, ");
                    foreach(ItemQuantity itemQuantity in quest.RewardItems)
                    {
                        RaiseMessage($"   {itemQuantity.Quantity} {ItemFactory.CreateGameItem(itemQuantity.ItemID).Name}");
                    }
                }
            }

        }

        private void GetMonsterAtLocation()
        {
            CurrentMonster = CurrentLocation.GetMonster();
        }

        public void AttackCurrentMonster()
        {
            if(CurrentWeapon == null)
            {
                RaiseMessage("You must select a weapon to attack!");
                return;
            }
            int damageToMonster = RandomNumberGenerator.NumberBetween(CurrentWeapon.MinimumDamage, CurrentWeapon.MinimumDamage);
            if(damageToMonster == 0)
            {
                RaiseMessage($"You missed the {CurrentMonster.Name}!");
            }
            else
            {
                CurrentMonster.HitPoints -= damageToMonster;
                RaiseMessage($"You hit the {CurrentMonster.Name} for {damageToMonster} points.");
            }

            if(CurrentMonster.HitPoints <= 0)
            {
                RaiseMessage(" ");
                RaiseMessage($"You defeated {CurrentMonster.Name}!");
                CurrentPlayer.ExperiencePoints += CurrentMonster.RewardExperiencePoints;
                RaiseMessage($"You receive {CurrentMonster.RewardExperiencePoints} experience points.");
                CurrentPlayer.Gold += CurrentMonster.RewardGold;
                RaiseMessage($"You received {CurrentMonster.RewardGold} gold.");
                foreach(ItemQuantity itemQuantity in CurrentMonster.Inventory)
                    {
                    GameItem item = ItemFactory.CreateGameItem(itemQuantity.ItemID);
                    CurrentPlayer.AddItemToInventory(item);
                    RaiseMessage($"You recieved {itemQuantity.Quantity} {item.Name}.");
                }
                GetMonsterAtLocation();
            }
            else
            {
                int damageToPlayer = RandomNumberGenerator.NumberBetween(CurrentMonster.MinimumDamage, CurrentMonster.MaximumDamage);
                if(damageToPlayer == 0)
                {
                    RaiseMessage($"The monster attacks but misses you!");
                }
                else
                {
                    CurrentPlayer.HitPoints -= damageToPlayer;
                    RaiseMessage($"The {CurrentMonster.Name} hit you for {damageToPlayer} points!");
                }

                if(CurrentPlayer.HitPoints <= 0)
                {
                    RaiseMessage(" ");
                    RaiseMessage($"The {CurrentMonster.Name} killed you!");
                    CurrentLocation = CurrentWorld.LocationAt(0, -1);
                    CurrentPlayer.HitPoints = CurrentPlayer.Level * 10;
                }
            }
        }

        private void RaiseMessage(string message)
        {
            OnMessageRaised?.Invoke(this, new GameMessageEventArgs(message));
        }
    }
}
