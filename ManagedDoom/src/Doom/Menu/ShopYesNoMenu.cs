//
// Copyright (C) 1993-1996 Id Software, Inc.
// Copyright (C) 2019-2020 Nobuaki Tanaka
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//



using System;
using System.Collections.Generic;

namespace ManagedDoom
{
    public sealed class ShopYesNoMenu : MenuDef
    {
        private string[] text;
        private Action action;
        private Player player;
        private int cost;

        public ShopYesNoMenu(DoomMenu menu) : base(menu)
        {
            //this.text = text.Split('\n');
            //this.action = action;
        }

        public void SetUpShop(string text, Player player, Action action, int cost)   
        {
            this.text = text.Split('\n');
            this.player = player;
			this.action = action;
            this.cost = cost;
		}

        public override bool DoEvent(DoomEvent e)
        {
            if (e.Type != EventType.KeyDown)
            {
                return true;
            }

            if (e.Key == DoomKey.Y ||
                e.Key == DoomKey.Enter ||
                e.Key == DoomKey.Space)
            {
                if (player.Currency >= cost)
                {
                    action();
                    player.RemoveCurrency(cost);
                    Menu.Close();
                } 
                else
                {
                    player.SendMessage("You Don't Have The Money Idiot");
                }
                Menu.StartSound(Sfx.PISTOL);
            }

            if (e.Key == DoomKey.N ||
                e.Key == DoomKey.Escape)
            {
                Menu.Close();
                Menu.StartSound(Sfx.SWTCHX);
            }

            return true;
        }

        public IReadOnlyList<string> Text => text;
    }
}
