

***

###`alert(string)`
This function prints the parameter provided by `string` to the console window,

**Example:**
```js
alert("Hello World!");
//> Hello World!
```

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

###D

***

###`dump(object)`
This function describes an object to the console, used for debugging purposes.

**Example:**
```js
var object = {
    "test": "test"
};

dump(object);
```

###J

***

###`jist_for_each_player(callBackForEachPlayer)`
This function executes `callBackForEachPlayer` for each player that
is currently online in your server.  It acts as a fast iterator, much like
`foreach` in other programming languages such as C#.

* `callBackForEachPlayer` must be a `function(player)`.
* `player` is of type `TShockAPI.TSPlayer`.

**Example:**
```js

jist_for_each_player(function(player) {
    alert(player.Name + " is online!");
});

//Wolfje is online!
//Panther is Online!
```

###`jist_player_count()`
returns the amount of players online in your server.

**Example:**
```js
var onlinePlayers = jist_player_count();

alert("There are " + onlinePlayers + " players online in my server.");

//> There are 16 players online in my server.
```

###`jist_random(from, to)`
This function generates a random number between `from` and `to`.  
Both parameters must be integers.

**Example:**
```js
alert(jist_random(1, 10));
alert(jist_random(1, 10));
alert(jist_random(1, 10));
//2
//7
//4
```

###`jist_repeat(numberOfTimes, functionToRepeat)`
Simple fast iterator, that repeats `functionToRepeat` `numberOfTimes` times.  Can be used in place of a regular loop if need be.

* functionToRepeat must be a `function(index)`
    * index is the zero-based index of the number of times the function has repeated

**Example:**
```js
alert("Counting to 4!");
jist_repeat(5, function(i) {
    alert(i);
});

//Counting to 4!
//0
//1
//2
//3
//4
```

###`jist_run_after(milliseconds, functionToRun, args)`
Queues a function provided by `functionToRun` to run after the specified amount of time provided by `milliseconds`.  `args` are any parameters you would like to pass to `functionToRun`.

This function is extremely similar to `setTimeout` in browser javascript.  It's asynchronous and is extremely
useful for executing pieces of script after a certain period.

* `functionToRun` must be a `function(args)`
* `args` are **optional**

**Example:**
```js
alert("This line runs immediately");
jist_run_after(5000, function(args) {
    alert("This line runs after 5 seconds");
});

//Output:
//This line runs immediately
//(5 second pause)
//This line runs after 5 seconds
```

###`jist_task_queue(hours, minutes, seconds, functionToRun)`
Adds a recurring scheduled task that will run `functionToRun` every `hours`:`minutes`:`seconds`,
and keep running it every `hours`:`minutes`:`seconds` until Jist is reloaded, or the task is stopped.

* `functionToRun` must be a `function()`.
* returns a **task ID** that is used to identify the scheduled task, so it can be later stopped if required.

**Example:**
```js
var recurringTaskID = jist_task_queue(0, 5, 0, function() {
    alert("This function is executing every five minutes!");
});
```

