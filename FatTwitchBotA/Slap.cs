
using SimpleTwitchBot;

public class Slap : Trigger 
{
	// JOEN TODO: private IrcSender sender;

	public Slap(){
	//	this.sender = TriggerService.getInstance().getSenderInstance();
		TriggerService.getInstance().registerTrigger(this,"slap");
	}

	public void use(String who, String hostmask, String channel, String[] trigger) {
	//	String victim = trigger.length > 1 ? trigger[1] : who;
	//	sender.sendAction(channel,"slaps " + victim);

	}

	public String getDescription() {
		return "Slaps a user";
	}
}
