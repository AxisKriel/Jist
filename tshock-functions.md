###`tshock_change_group(player, group)`
Changes the online player specified by `player`'s group to the TShock group specified by `group`.

* **Parameters**'
  * `player`: A player object, takes the same format as `tshock_get_player()`
  * `group`: The new TShock group to move to, remember to `tshock_group_exists(group)` first

* **Returns**
  * `true` if the group change operation succeeded, `false` otherwise.

**Example:**
```js
var groupExists = tshock_group_exists("tshock.group.group1");
var player = tshock_get_player("PlayerName");
var group;
if (groupExists && player !== undefined) {
    group = tshock_group("tshock.group.group1");
    
    if (tshock_change_group(player, group) == true) {
        alert("PlayerName changed their group!");
    }
}
```

###`tshock_exec(player, command)`
Forces an online player provided by `player` to execute the TShock command provided by `command` as though they typed it themselves.

**NOTE: `tshock_exec` bypasses permissions, and will force the player to execute the command regardless of if they have permission to or not if they had typed it themselves.  Be careful!**

* **Parameters**
  * `player` is the same as `tshock_get_player` use **`tshock_server()`** in place of a player to execute a command in the server console
  * `command`: a `string` containing the command to execute.

* **Returns**
  * `true` if the operation succeeded `false` if it failed.

**Example:**
```js
tshock_exec(tshock_server(), "/bc Hello world!");
```

###`tshock_get_player(player)`
Retrieves a TShock player object, or `undefined` if they can't be found or are not online.

* **Parameters**
  * `player`: can be a `string` with the user's name, or a `player` object already
  * `permission`: a `string` containing the desired permission to check against.

* **Returns**
  * `TShockAPI.TSPlayer` if the player was found, `undefined` if they aren't online or an error was 

**Example:**
```js
var player = tshock_get_player("PlayerName");
if (player === undefined) {
    alert("Player by the name of PlayerName cannot be found.");
} else {
    alert("Player is found!");
}
```

###`tshock_group(groupName)`
Retrieves a TShock group object from the TShock database for use in a script.

* **Parameters**
  * `groupName`: A string containing the group to retrieve

* **Returns**
  * a `TShockAPI.Group` object if the group was found, or `undefined` if it wasn't, or an error occurred.

**Example:**
```js
var groupExists = tshock_group_exists("tshock.group.group1");
var group;
if (groupExists) {
    group = tshock_group("tshock.group.group1");
    //do something with "group" here
}
```
###`tshock_group_exists(groupName)`
Determines if the TShock group provided by `groupName` exists in the TShock database

* **Parameters**
  * `groupName`: A string containing the group to check against

* **Returns**
  * `true` if the TShock group exists in the TShock database, `false` otherwise.

**Example:**
```js
var groupExists = tshock_group_exists("tshock.group.group1");
if (groupExists) {
    alert("The group tshock.group.group1 exists!");
}
```

###`tshock_has_permission(player, permission)`
Determines whether the TShock player provided by `player` has a `permission`.

* **Parameters**
  * `player` is the same as `tshock_get_player`
  * `permission` must be a `string` containing the desired permission to check against.

* **Returns**
  * `true` if the `player` has `permission`, `false` if they do not.

**Example:**
```js
var player = tshock_get_player("PlayerName");

if (player === undefined) {
    alert("Player by the name of PlayerName cannot be found.");
} else {
    if (tshock_has_permission(player, "tshock.cmd.sudo") == true) {
        alert("PlayerName has the SUDO permission!");
    } else {
        alert("Permission denied!");
    }
}
```

###`tshock_msg(player, message)`
Sends the online player provided by `player` a message provided by `message`.

* **Parameters**'
  * `player`: A player object, takes the same format as `tshock_get_player()`
  * `message`: A `string` containing the message to send to the `player`.

* **Returns**
  * N/A

**Example:**
```js
tshock_msg("OnlinePerson", "Hello OnlinePerson!");
var player = tshock_get_player("PlayerName");
if (player !== undefined) {
    tshock_msg(player, "Hello world!");
}
```

###`tshock_msg_colour(colour, player, message)`
Same as `tshock_msg()` but with any colour.

* **Parameters**'
  * `colour`: A colour for the message to `player`.  Can be R,G,B or HTML format.
  * `player`: A player object, takes the same format as `tshock_get_player()`
  * `message`: A `string` containing the message to send to the `player`.

* **Returns**
  * N/A

**Example:**
```js
//Prints a black message to OnlinePerson
tshock_msg_colour("0,0,0", "OnlinePerson", "Hello OnlinePerson!");
var player = tshock_get_player("PlayerName");
if (player !== undefined) {
    //Prints a grey message to PlayerName
    tshock_msg_colour("#333", player, "Hello world!");
}
```

###`tshock_broadcast(message)`
Sends all players a broadcast `message`, as if the console had typed `/bc <message>`.

* **Parameters**'
  * `message`: A `string` containing the message to broadcast.

* **Returns**
  * N/A

**Example:**
```js
tshock_broadcast("Hello World!");
```

###`tshock_broadcast_colour(colour, message)`
Same as `tshock_broadcast()` but with any colour.

* **Parameters**'
  * `colour`: A colour for the message to `player`.  Can be R,G,B or HTML format.
  * `message`: A `string` containing the message to broadcast.

* **Returns**
  * N/A

**Example:**
```js
tshock_broadcast_colour("#ff0000", "This is a red broadcast message");
tshock_broadcast_colour("#00ff00", "This is a green broadcast message");
```
