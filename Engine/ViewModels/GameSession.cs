﻿using System;
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
        private Player _currentPlayer;
        public bool HasMonster => CurrentMonster != null;
        public bool HasTrader => CurrentTrader != null;
        public Player CurrentPlayer
        {
            get { return _currentPlayer; }
            set
            {
                if(_currentPlayer != null)
                {
                    _currentPlayer.OnActionPerformed -= OnCurrentPlayerPerformedAction;
                    _currentPlayer.OnLeveledUp -= OnCurrentPlayerLeveledUp;
                    _currentPlayer.OnKilled -= OnCurrentPlayerKilled;
                }
                _currentPlayer = value;
                if(_currentPlayer!=null)
                {
                    _currentPlayer.OnActionPerformed += OnCurrentPlayerPerformedAction;
                    _currentPlayer.OnLeveledUp += OnCurrentPlayerLeveledUp;
                    _currentPlayer.OnKilled += OnCurrentPlayerKilled;
                }
            }
        }
        public World CurrentWorld { get;  }
        

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

                CompleteQuestAtLocation();
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
                if(_currentMonster!=null)
                {
                    _currentMonster.OnKilled -= OnCurrnetMonsterKilled;
                    _currentMonster.OnActionPerformed -= OnCurrentMonsterPerformedAction;
                }
                _currentMonster = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasMonster));

                if(CurrentMonster != null)
                {
                    _currentMonster.OnKilled += OnCurrnetMonsterKilled;
                    _currentMonster.OnActionPerformed += OnCurrentMonsterPerformedAction;
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
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasTrader));
            }
        }
       
        #endregion



        public GameSession()
        {
            CurrentPlayer = new Player("Scott", "Fighter", 0, 10, 10, 100);
            
            if(!CurrentPlayer.Weapons.Any())
            {
                CurrentPlayer.AddItemToInventory(ItemFactory.CreateGameItem(1001));
            }
            CurrentPlayer.AddItemToInventory(ItemFactory.CreateGameItem(2001));
            CurrentPlayer.LearnRecipe(RecipeFactory.RecipeById(1));
            CurrentPlayer.AddItemToInventory(ItemFactory.CreateGameItem(3001));
            CurrentPlayer.AddItemToInventory(ItemFactory.CreateGameItem(3002));
            CurrentPlayer.AddItemToInventory(ItemFactory.CreateGameItem(3003));


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
                        CurrentPlayer.RemoveItemsFromInventory(quest.ItemsToComplete);
                        RaiseMessage("");
                        RaiseMessage($"You completed the {quest.Name} quest.");
                        RaiseMessage($"You received {quest.RewardExperiencePonts} experience points.");
                        CurrentPlayer.AddExperiencePoints( quest.RewardExperiencePonts);
                        RaiseMessage($"You received {quest.RewardGold} gold.");
                        CurrentPlayer.ReceiveGold(quest.RewardGold);
                        
                        foreach(ItemQuantity itemQuantity in quest.RewardItems)
                        {
                            GameItem rewardItem = ItemFactory.CreateGameItem(itemQuantity.ItemID);
                            RaiseMessage($"You received {rewardItem.Name}");
                            CurrentPlayer.AddItemToInventory(rewardItem);
                            
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
            if(CurrentMonster == null)
            {
                return;
            }
            if(CurrentPlayer.CurrentWeapon == null)
            {
                RaiseMessage("You must select a weapon to attack!");
                return;
            }
            CurrentPlayer.UseCurrentWeaponOn(CurrentMonster);

            if (CurrentMonster.IsDead)
            {
                GetMonsterAtLocation();
            }
            else
            {
                CurrentMonster.UseCurrentWeaponOn(CurrentPlayer);

            }
        }

        public void UseCurrentConsumable()
        {
            if (CurrentPlayer.CurrentConsumable != null)
            {
                CurrentPlayer.UseConsumable();
            }
        }
        public void CraftItemUsing(Recipe recipe)
        {
            if(CurrentPlayer.HasAllTheseItems(recipe.Ingredients))
            {
                CurrentPlayer.RemoveItemsFromInventory(recipe.Ingredients);
                foreach(ItemQuantity itemQuantity in recipe.OutputItems)
                {
                    for(int i=0; i<itemQuantity.Quantity; i++)
                    {
                        GameItem outputItem = ItemFactory.CreateGameItem(itemQuantity.ItemID);
                        CurrentPlayer.AddItemToInventory(outputItem);
                        RaiseMessage($"You crafted 1 {outputItem.Name}.");
                    }
                }
            }
            else
            {
                RaiseMessage("You do not have required ingredients:");
                foreach(ItemQuantity itemQuantity in recipe.Ingredients)
                {
                    RaiseMessage($" {itemQuantity.Quantity} {ItemFactory.ItemName(itemQuantity.ItemID)}");
                }
            }
        }
        private void OnCurrentPlayerKilled(object sender, System.EventArgs eventArgs)
        {
            RaiseMessage(" ");
            RaiseMessage($"You have been killed");
            CurrentLocation = CurrentWorld.LocationAt(0, -1);
            CurrentPlayer.CompletelyHeal();
        }
        private void OnCurrentPlayerPerformedAction(object sender, string result)
        {
            RaiseMessage(result);
        }
        private void OnCurrentMonsterPerformedAction(object sender, string result)
        {
            RaiseMessage(result);
        }
        private void OnCurrnetMonsterKilled( object sender, System.EventArgs eventArgs)
        {
            RaiseMessage(" ");
            RaiseMessage($"You defeated {CurrentMonster.Name}!");
            RaiseMessage($"You received {CurrentMonster.RewardExperiencePoints} experience points.");
            CurrentPlayer.AddExperiencePoints( CurrentMonster.RewardExperiencePoints);
            RaiseMessage($"You received {CurrentMonster.RewardGold} gold.");
            CurrentPlayer.ReceiveGold(CurrentMonster.Gold);
            foreach(GameItem item in CurrentMonster.Inventory)
            {
                RaiseMessage($"You received one {item.Name}.");
                CurrentPlayer.AddItemToInventory(item);
            }
        }
        private void OnCurrentPlayerLeveledUp(object sender, System.EventArgs eventArgs)
        {
            RaiseMessage($"You are now on level {CurrentPlayer.Level}");
        }
        private void RaiseMessage(string message)
        {
            OnMessageRaised?.Invoke(this, new GameMessageEventArgs(message));
        }
    }
}
