###`acmd_alias_create(aliasName, cost, cooldownSeconds, permissionNeeded, functionToRun)`
_Requires SEconomy, AliasCmd & Scripting_

Creates an AliasCmd that everyone with `permissionNeeded` permission can run, if they have the money to.  `functionToRun` gets executed with the player that executed it.  Commands do not have to have permissions to run if everyone is supposed to be able to use your alias, nor does it have to have a `cost` _(ie, the command can be free)_

* **Parameters**
  * `aliasName`: The name of the slash-command to add.
  * `cost`: A `string` with the amount of money the command costs a player to run.  Use `"0"` for free.-
  * `cooldownSeconds`: The time in seconds in between a player being allowed to re-execute the alias.  Use `"0"` for no cooldown.
  * `permissionNeeded`: A `string` with the permission users need to run the command. Use `""` for no permission required.
  * `functionToRun`: A `function(player, args)` that is executed when a user executes your alias.
    * `player` points to the player object that executed it
    * `args` refers to the CommandArgs that come with a command.

* **Returns**
  * N/A

**Example:**
```js
//Create a free alias called test, with no permissions and no cooldown.
acmd_alias_create("test", "0", 0, "", function(player, args) {
    tshock_msg(player, "You typed /test!!!");
});

// /test
// You typed /test!!!
```
