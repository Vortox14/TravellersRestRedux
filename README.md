# TravellersRest-RestfulTweaks

A Traveller's Rest mod with a lot of small tweaks. Each option can be configured separately in the config file.

* Change the stack size from the default of 99
* Change the amount of items that can be places in a keg/tap (either to a fixed value or matching stack size)
* adjust the player speed and run speed
* Soil stays wet once watered
* Fireplace does not consume fuel
* All recipes require no fuel
* All recipes purchased in the cave cost only one fragment.
* Quick crafting 
* Crops compelete a full stage of growth each night (including tree crops - affects newly plated crops only)
* Reusable crops (grapes etc) can be harvested every day
* Dump a list of items, crops or recipes to the console on startup.
* Several modifiers for new staff: refresh list of staff on open, no negative perks, always 3 perks (does not always work), starting level.
* Increase customer capacity, number of rented rooms, placable floor tiles, dining/crafting zone size and number of crafting zones.
* Wilson sells items for 1 coin, 
* Fish value multiplier 
* Bad bird is funny; gain rep when customers laugh at your bird calling them ugly.
* Walk through crops
* aging barrel stack size adjustment now working, 
* more price adjustment options
* endless water buckets (NOTE: do not use during tutorial, and when enabled you can not fill empty buckets at the well so fill buckets before activating)
* hotkeys for: make all birds talk, make all crops grow, make all trees grow
* Crops grow in all seasons
* Increased loot from slaughtering animals

1.5.0 goals:
* [NOT STARTED] change: grow trees to max instead of one age level
* [NOT STARTED] change: grow crops to max instead of one age level, update graphic right away (Cropsetter.UpdateCropVisual())
* [NOT STARTED] re-harvestable crops all become ready to harvest
* [NOT STARTED] More milk from animals
* [NOT STARTED] Rented rooms not messy ( RentedRoom.MessUpRoom() prefix to block function  )
* [NOT STARTED] Tables never messy ( Table.AddFirtiness() prefix to block function )
* [NOT STARTED] Customer do not mess up floor (Customer.Awake() Postfix -> Customer.customerInfo.floorDirtProbability change from  55 to 0;)
* [NOT STARTED] Customer fast eating  (Customer.Awake() Postfix -> Customer.Awake() Postfix -> Customer.customerInfo.timeEatingMin, timeEatingMax)
* [NOT STARTED] Customer never angry (Customer.Awake() Postfix -> Customer.Awake() Postfix -> Customer.customerInfo.testRateRowdyCustomers, rowdyCustomersProbability , )
* [NOT STARTED] Customer always calm down (Customer.Awake() Postfix -> Customer.Awake() Postfix -> Customer.customerInfo.calmRowdyCustomersProbability =100
* [NOT STARTED] Customer more patient (Customer.Awake() Postfix -> Customer.Awake() Postfix -> Customer.customerInfo.requestOrderPatience, requestRoomPatience
* [NOT STARTED] Customer more likely to re-order (Customer.Awake() Postfix -> Customer.Awake() Postfix -> Customer.customerInfo.requestAgainProbability

## Downloading the mod

Mods are available on [Nexus Mods](https://www.nexusmods.com/travellersrest) or you can download the mod from [compiled-releases](https://github.com/DrStalker/TravellersRest-ReastfulTweaks/tree/main/compiled-releases)


## How to install mods:

* Install [Bepinex](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.2)﻿ (Stable version 5.4 for Windows 64; the filename will look like `BepInEx_win_x64_5.4.23.2.zip`)
* Start the game, quit the game after it finishes loading
* This will create a Bepinex config file and a plugins folder that you can put additional mods in
* (optional) Enable the Bepinex Console (see the detailed guide or the Bepinex documentation for steps)
* Copy the mod .dll to the plugins directory.


## How to change mod settings:

* Install the mod and start the game.
* Bepinex will create a file in the \BepInEx\config\ with default settings for the mod.
* Exit the game, edit the config file, restart the game.


## Is this mod save to add/remove mid play-through?

Yes, but be warned reducing stack size can cause item loss; seperate your items into stacks of 99 or fewer to avoid this.


## Traveller's Rest Modding Guide

﻿[Here are my notes on modding Traveler's Rest.](https://docs.google.com/document/d/e/2PACX-1vSciLNh4KgUxE4L2h_K0KAxi2hE6Z1rhroX0DJVhZIqNEgz2RvYESqffRl8GFONKKF1MjYIIGI5OKHE/pub)

