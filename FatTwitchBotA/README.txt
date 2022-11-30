2022.11.30 working :) 
2022.11.27 latest worked on it :) im doing stuff. the stuff of things. to get the poll to work.
2022.11.24 latest worked on it :)



===== READ ME ===========================

Do this:

Open up explorer where you have the binaies
Create a file "appsettings.json" and write:
the #######... part is replaced with your twitch oauth key!

{
  "TwitchConfigFile": {
    "TwitchUsername": "swol_69",
    "OAUTH": "oauth:###############################", 
    "Timer1Seconds": 30,
	"Timer2Seconds": 5,
	"Timer3Seconds": 5
  }
}


Setup the channel to!

Now you are ready to start it up.
When you startup the program, it will attempt to join Twitch irc server.
If it hangs? restart.

To quit the bot correctly, make sure you change Fayt64 to your username and type !quit in twitch chat



=== DEV README ===

This is a wacky bot. 
Uses microsoft json and microsoft binder nuget packages.





===== TODO ========================  
  
  
 🌑 merge the photoshop code
  
  




/// ------ ASYNC STUFF ---------------------

// Async
// Task
// Await

// *** You can choose *when* to await that task. You can start other tasks concurrently. ***

// https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/

// await tillåter sekventiell kod
// await förväntar sig en Task i metodanrop (Task<Bacon> cookBacon())


/* exempel med frukost:
pourCoffee();
console

Task<Egg> eggTask  = fryEggsAsync(3); // async!
Task<Bacon baconTask = fryBaconAsync(2); 
Task<Bread> breadTask = toastBreadAsync(3);


KLASSTYPEN

Bread breadObj = await breadTask; // bread popped!
spreadButter
spreadJam
console


Egg eggObj = await eggTask; // sequential
console egg done
*/ 
  

  
  
  
  
// ------- JAVA BOT --------------

  //Jaif
    /*
      * 
      * JOEN TODO:
    Socket sock = new Socket(host, port);
    IrcSender sender = new IrcSender(sock);
    new IrcReciever(sock, sender);
    new UserInput(sender);

    TriggerService.getInstance().setSenderInstance(sender);
    */



    //Registering the triggers
    //new Slap();
    /*new Flap();
    new Fap();
    new TimeTrigger();
    new LatestKernel();
    new Webcomics();
    new SearchWikipedia();
    new SearchGoogle();
    new SearchGoogleImages();
    new SearchUncyclopedia();
    */ 




	show triggers:
	// JOEN TODO sender.sendNotice(who, "!" + kvp.Key.ToString() + " - " + triggerDict[kvp.Key].getDescription());

      // Java:
      //Object key;
      //for(Enumeration e = triggerDict.keys(); e.hasMoreElements();){
      //								key = e.nextElement();
      //sender.sendNotice(who,"!" + key.toString() + " - " + triggerDict.get(key).getDescription());
      //}

