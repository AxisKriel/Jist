Jist - Javascript Interpreted Scripting for TShock
====

Jist is a plugin for Tshock (https://github.com/NyxStudios/TShock) and TSAPI (https://github.com/Deathmax/TerrariaAPI-Server) which allows you to run server-sided javascripts on your TShock server that help you automate all kinds of tasks that would otherwise have to be done with an overly simple plugin. 

Jist is based on the Jint 2.0 javascript interpreter (https://github.com/sebastienros/jint) which completely interprets javascript without **any external dependencies**, or without compiling the code into .net assemblies of any form, which can be **unloaded and loaded at any time.** Scripts **share the same environment**.  Scripts are *not* contained to one file and functions, variables or resources can be shared and referenced from one script to another.  The engine does not confine usage in any scenario.

Jist comes with a comprehensive API which support all kinds of handy functionality to interact with TShock.  Jist also ships with TShock related functions to manipulate all kinds of aspects of players, and your server.

Check out the API reference, and the guides for more information on making scripts, and handy recipes.  If you're interested in exposing your own functions to the Jist javascript engine, check out the Developer guide for more information.

## Why Javascript?

The primary reason for Javascript is the Jint runtime, and the dynamic features it supports.  The second reason is that Javascript is an incredibly popular scripting language that runs all the dynamic content you see in your browser today.  It's incredibly easy to program for C# developers as the C-like syntax is very familiar.  If you have ever developed for the web you will surely at least be familiar with the Javascript syntax.

## Features

* Fully interpreted scripting support
* Load scripts on the fly without having to restart your server
* Comprehensive scripting API
* Developer API, export your own functions in your plugin to scripts

## Commands

```
/jist reload            Reloads the JIST runtime
/jist ev  "<snippet>"   Executes the Javascript snippet and prints the result to the console
/jist dumpenv           Dumps a list of all javascript functions registered in the JS engine
/jist dumptasks         Dumps all Jist recurring tasks, with their ID and the time until they run again

```

## Permissions

```
jist.cmd                Has permission to execute the /jist command
```
